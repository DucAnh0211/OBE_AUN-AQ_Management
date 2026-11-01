"""Run only against the disposable chat fixture; no provider calls or real user data."""
import io
import json
import os
import time
from pathlib import Path
from zipfile import ZipFile
import httpx
from reportlab.pdfgen import canvas

base = os.environ["KB_TEST_URL"].rstrip("/")
assert base == "http://localhost:5299", "Use the isolated chat test stack"
password = "IsolatedChat123!"
changed = "IsolatedChat456!"
client = httpx.Client(base_url=base, timeout=120)


def login(email, current=password):
    session = httpx.Client(base_url=base, timeout=120)
    response = session.post("/api/auth/login", json={"email": email, "password": current})
    assert response.status_code == 200, response.text
    data = response.json()
    session.headers["Authorization"] = "Bearer " + data["accessToken"]
    if data["user"]["mustChangePassword"]:
        response = session.post("/api/auth/change-password", json={"currentPassword": current, "newPassword": changed})
        assert response.status_code == 200, response.text
        session.headers["Authorization"] = "Bearer " + response.json()["accessToken"]
    return session


client = login("chat-admin@example.test")
prefix = "/api/knowledge"
ids = []


def upload(title, name, content):
    response = client.post(prefix + "/documents", data={"title": title}, files={"file": (name, content)})
    assert response.status_code == 201, response.text
    id = response.json()["id"]; ids.append(id)
    response = client.post(f"{prefix}/documents/{id}/process")
    assert response.status_code == 202, response.text
    for _ in range(45):
        result = client.get(f"{prefix}/documents/{id}").json()
        if result["status"] == "ready":
            return id
        assert result["status"] != "failed", result
        time.sleep(1)
    raise AssertionError(result)


pdf = io.BytesIO(); c = canvas.Canvas(pdf)
c.drawString(50, 780, "This page describes the library and campus facilities in general."); c.showPage()
c.drawString(50, 780, "Graduation requires completion of 150 credits in the curriculum."); c.save()
pdf_id = upload("Quy chế tốt nghiệp PDF", "graduation.pdf", pdf.getvalue())
docx = io.BytesIO()
with ZipFile(docx, "w") as z:
    z.writestr("word/document.xml", '<w:document xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main"><w:body><w:p><w:r><w:t>Introduction</w:t></w:r></w:p><w:p><w:r><w:t>Hoàn thành 150 tín chỉ để tốt nghiệp.</w:t></w:r></w:p></w:body></w:document>')
docx_id = upload("Quy chế tốt nghiệp Word", "graduation.docx", docx.getvalue())
excluded_id = upload("Tài liệu không liên quan", "unrelated.txt", "Quy chế: hoàn thành 150 tín chỉ. SECRET_BODY_ONLY".encode())

for role in ["lecturer", "student"]:
    response = client.post("/api/admin/users", json={"email": f"chat-{role}@example.test", "fullName": "Chat " + role, "role": role, "temporaryPassword": password})
    assert response.status_code == 201, response.text
lecturer = login("chat-lecturer@example.test"); student = login("chat-student@example.test")
payload = {"question": "Quy chế tốt nghiệp yêu cầu bao nhiêu tín chỉ?"}
assert httpx.post(base + prefix + "/chat/ask", json=payload).status_code == 401
assert student.post(prefix + "/chat/ask", json=payload).status_code == 403
assert lecturer.get(prefix + "/documents").status_code == 403
assert lecturer.post(prefix + "/documents", data={"title": "forbidden"}, files={"file": ("test.txt", b"test")}).status_code == 403
response = lecturer.post(prefix + "/chat/ask", json=payload)
assert response.status_code == 200, response.text
assert response.headers["cache-control"] == "no-store"
answer = response.json(); citations = [c for p in answer["parts"] for c in p["citations"]]
assert {c["documentId"] for c in citations} == {pdf_id, docx_id}, answer
assert excluded_id not in [c["documentId"] for c in citations], "Body-only keyword match leaked into semantic search"
assert answer["documentCount"] == 2
for citation in citations:
    url = citation["url"].replace("/knowledge/sources", prefix + "/chat/sources")
    source = lecturer.get(url)
    assert source.status_code == 200, source.text
    excerpt = source.json()
    assert "150" in excerpt["text"]
    if citation["documentId"] == pdf_id:
        assert excerpt["page"] == 2
    else:
        assert excerpt["locator"] == "Đoạn 2"
    assert student.get(url).status_code == 403
    stale = url.replace(citation["chunkHash"], "0" * 64)
    assert lecturer.get(stale).status_code == 409
    original = lecturer.get(f"{prefix}/chat/sources/{citation['documentId']}/original", params={"sourceHash": citation["sourceHash"]})
    assert original.status_code == 200
missing = lecturer.post(prefix + "/chat/ask", json={"question": "missing topic"})
assert missing.status_code == 200 and not missing.json()["parts"]
followup = lecturer.post(prefix + "/chat/ask", json={"question": "Cần bao nhiêu tín chỉ?", "history": [
    {"role": "user", "content": payload["question"]}, {"role": "assistant", "content": answer["parts"][0]["text"]}]})
assert followup.status_code == 200 and followup.json()["parts"], followup.text
folder = Path("tmp/chat-ui"); folder.mkdir(parents=True, exist_ok=True)
(folder / "fixture.json").write_text(json.dumps({"ids": ids, "citations": citations}), encoding="utf-8")
print("PASS API: auto title/file filter, excluded body-only match, semantic/rerank pipeline, follow-up, exact PDF page/DOCX paragraph, stale hashes, lecturer/student/anonymous boundaries")
