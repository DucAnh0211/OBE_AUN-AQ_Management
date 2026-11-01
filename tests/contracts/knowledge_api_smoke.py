"""Disposable integration stack only; KB_TEST_URL, KB_TEST_EMAIL, KB_TEST_PASSWORD required.
Start Python with apps/ai-service/tests/smoke_app.py so this never calls a paid provider.
"""
import hashlib
import io
import os
import time
import uuid
import httpx
from PIL import Image, ImageDraw, ImageFont

base = os.environ["KB_TEST_URL"].rstrip("/")
email = os.environ["KB_TEST_EMAIL"]
password = os.environ["KB_TEST_PASSWORD"]
client = httpx.Client(base_url=base, timeout=60)
session = client.post("/api/auth/login", json={"email": email, "password": password})
assert session.status_code == 200, session.text
data = session.json()
client.headers["Authorization"] = "Bearer " + data["accessToken"]
if data["user"]["mustChangePassword"]:
    response = client.post("/api/auth/change-password", json={"currentPassword": password, "newPassword": "IsolatedKnowledge456!"})
    assert response.status_code == 200, response.text
    client.headers["Authorization"] = "Bearer " + response.json()["accessToken"]

prefix = "/api/knowledge"
assert httpx.get(base + prefix + "/documents").status_code == 401
configuration = client.get(prefix + "/configuration")
assert configuration.status_code == 200, configuration.text
assert configuration.json()["embeddingConfigured"], configuration.text


def upload(name, content):
    response = client.post(prefix + "/documents", data={"title": name}, files={"file": (name, content)})
    assert response.status_code == 201, response.text
    return response.json()


def get_item(id):
    response = client.get(prefix + "/documents")
    assert response.status_code == 200, response.text
    return next(x for x in response.json() if x["document"]["id"] == id)


def wait_status(id, status):
    for _ in range(45):
        item = get_item(id)
        if item["status"] == status:
            return item
        time.sleep(1)
    raise AssertionError(item)


original = ("Chuẩn đầu ra kiến thức và kỹ năng. " * 150).encode()
doc = upload("knowledge.txt", original)
id = doc["id"]
assert doc["sha256"] == hashlib.sha256(original).hexdigest()
assert client.get(f"{prefix}/documents/{id}/original").content == original
assert client.post(f"{prefix}/documents/{id}/process").status_code == 202
assert client.post(f"{prefix}/documents/{id}/process").status_code == 409
item = wait_status(id, "ready")
assert item["dimensions"] == 3 and item["chunkCount"] > 1, item
chunks = client.get(f"{prefix}/documents/{id}/chunks").json()
assert chunks[0]["locator"] == "Văn bản"
assert "embedding" not in chunks[0]
assert client.post(f"{prefix}/documents/{id}/disable").status_code == 204
for path in ("original", "chunks"):
    assert client.get(f"{prefix}/documents/{id}/{path}").status_code == 410
assert client.post(f"{prefix}/documents/{id}/process").status_code == 410

if configuration.json()["ocrAvailable"]:
    image = Image.new("RGB", (1200, 450), "white")
    draw = ImageDraw.Draw(image)
    font = ImageFont.truetype("C:/Windows/Fonts/arial.ttf", 42) if os.path.exists("C:/Windows/Fonts/arial.ttf") else ImageFont.load_default(size=42)
    draw.text((60, 90), "Knowledge Base\nLearning outcomes and assessment", fill="black", font=font)
    buffer = io.BytesIO()
    image.save(buffer, format="PDF")
    scan = upload("scanned-knowledge.pdf", buffer.getvalue())
    assert client.post(f"{prefix}/documents/{scan['id']}/process?forceOcr=true").status_code == 202
    processed_scan = wait_status(scan["id"], "ready")
    assert "OCR 1" in processed_scan["warning"], processed_scan
    scan_chunks = client.get(f"{prefix}/documents/{scan['id']}/chunks").json()
    assert "Knowledge" in scan_chunks[0]["text"]
    assert scan_chunks[0]["locator"] == "Trang 1"

bad = upload("invalid.pdf", b"not a PDF")
assert client.post(f"{prefix}/documents/{bad['id']}/process").status_code == 202
wait_status(bad["id"], "failed")
assert client.post(f"{prefix}/documents/{bad['id']}/process").status_code == 202
wait_status(bad["id"], "failed")

failed = upload("provider-fail.txt", b"FAIL_EMBEDDING")
assert client.post(f"{prefix}/documents/{failed['id']}/process").status_code == 202
wait_status(failed["id"], "failed")
assert client.get(f"{prefix}/documents/{failed['id']}/chunks").json() == []

slow = upload("cancel-source.txt", b"SLOW_EMBEDDING")
assert client.post(f"{prefix}/documents/{slow['id']}/process").status_code == 202
wait_status(slow["id"], "processing")
assert client.post(f"{prefix}/documents/{slow['id']}/disable").status_code == 204
time.sleep(7)
assert get_item(slow["id"])["status"] == "disabled"
assert client.get(f"{prefix}/documents/{slow['id']}/chunks").status_code == 410

assert client.post(prefix + "/documents", data={"title": "bad"}, files={"file": ("bad.exe", b"no")}).status_code == 400
assert client.post(prefix + "/documents", data={"title": "empty"}, files={"file": ("empty.txt", b"")}).status_code == 400

for role in ("lecturer", "student"):
    user_email = f"kb-{role}-{uuid.uuid4().hex[:8]}@example.test"
    response = client.post("/api/admin/users", json={"email": user_email, "fullName": "Knowledge permission test", "temporaryPassword": "IsolatedUser123!", "role": role})
    assert response.status_code == 201, response.text
    user_client = httpx.Client(base_url=base)
    session = user_client.post("/api/auth/login", json={"email": user_email, "password": "IsolatedUser123!"}).json()
    user_client.headers["Authorization"] = "Bearer " + session["accessToken"]
    if session["user"]["mustChangePassword"]:
        changed = user_client.post("/api/auth/change-password", json={"currentPassword": "IsolatedUser123!", "newPassword": "IsolatedUser456!"})
        assert changed.status_code == 200, changed.text
        user_client.headers["Authorization"] = "Bearer " + changed.json()["accessToken"]
    for path in ("/documents", "/configuration", f"/documents/{bad['id']}/original", f"/documents/{bad['id']}/chunks"):
        assert user_client.get(prefix + path).status_code == 403
    assert user_client.post(f"{prefix}/documents/{bad['id']}/process").status_code == 403
    assert user_client.post(f"{prefix}/documents/{bad['id']}/disable").status_code == 403
    assert user_client.post(prefix + "/documents", data={"title": "forbidden"}, files={"file": ("x.txt", b"x")}).status_code == 403
    user_client.close()

print("PASS: originals on MinIO, checksums, Python pipeline, chunks, duplicate queue, failures/retry, invalidation while running, admin-only authorization")
