"""Disposable agent stack only. Mock providers; real EF/Postgres and LangGraph checkpoints."""
import json
import time
import uuid
import subprocess
from pathlib import Path
import httpx

base = "http://localhost:5298"
password = "IsolatedAgent123!"; changed = "IsolatedAgent456!"


def login(email, current=password):
    c = httpx.Client(base_url=base, timeout=30)
    r = c.post("/api/auth/login", json={"email": email, "password": current}); assert r.status_code == 200, r.text
    data = r.json(); c.headers["Authorization"] = "Bearer " + data["accessToken"]
    if data["user"]["mustChangePassword"]:
        r = c.post("/api/auth/change-password", json={"currentPassword": current, "newPassword": changed}); assert r.status_code == 200, r.text
        c.headers["Authorization"] = "Bearer " + r.json()["accessToken"]
    return c


def sql(query):
    return subprocess.check_output(["docker", "exec", "obe-agent-test-db", "psql", "-U", "agent_test", "-d", "agent_test", "-At", "-c", query], text=True).strip()


admin = login("agent-admin@example.test")
for role in ["lecturer", "student"]:
    r = admin.post("/api/admin/users", json={"email": f"agent-{role}@example.test", "fullName": "Agent " + role, "role": role, "temporaryPassword": password})
    assert r.status_code == 201, r.text
lecturer = login("agent-lecturer@example.test"); student = login("agent-student@example.test")
prefix = "/api/knowledge/chat/threads"
assert httpx.get(base + prefix).status_code == 401
assert student.get(prefix).status_code == 403
r = lecturer.post(prefix); assert r.status_code == 201, r.text
thread = r.json()["id"]
assert admin.get(prefix + "/" + thread).status_code == 404
assert admin.delete(prefix + "/" + thread).status_code == 404
assert admin.post(prefix + "/" + thread + "/messages", json={"requestId": str(uuid.uuid4()), "question": "access"}).status_code == 404


def send(question, id=None, target=thread):
    request = {"requestId": id or str(uuid.uuid4()), "question": question}
    r = lecturer.post(f"{prefix}/{target}/messages", json=request); assert r.status_code == 202, r.text
    # Replaying the same request must never add a second exchange.
    r = lecturer.post(f"{prefix}/{target}/messages", json=request); assert r.status_code == 202, r.text
    for _ in range(90):
        detail = lecturer.get(f"{prefix}/{target}").json()
        row = next(m for m in detail["messages"] if m["id"] == request["requestId"])
        assert row["status"] != "failed", row
        if row["status"] == "completed": return row
        time.sleep(.5)
    raise AssertionError(row)


greeting = send("Chào bạn")
assert "👋" in greeting["answer"]["notice"] and not greeting["answer"]["parts"]
assert greeting["checkpointId"]
assert int(sql(f'''SELECT count(*) FROM knowledge.chat_checkpoints WHERE "ExchangeId"='{greeting['id']}' ''')) >= 4
assert int(sql(f'''SELECT count(*) FROM knowledge.chat_exchanges WHERE "Id"='{greeting['id']}' ''')) == 1
send("Tôi tên là Minh")
memory = send("Bạn nhớ tên mình không?")
assert "Minh" in memory["answer"]["notice"]

# The same document pipeline remains grounded and authorized inside the graph.
r = admin.post("/api/knowledge/documents", data={"title": "Quy chế tốt nghiệp"}, files={"file": ("graduation.txt", "Hoàn thành 150 tín chỉ để tốt nghiệp.".encode())})
assert r.status_code == 201, r.text
document = r.json()["id"]
assert admin.post(f"/api/knowledge/documents/{document}/process").status_code == 202
for _ in range(30):
    status = admin.get(f"/api/knowledge/documents/{document}").json()["status"]
    if status == "ready": break
    time.sleep(1)
assert status == "ready"
answer = send("Theo quy chế tốt nghiệp, cần bao nhiêu tín chỉ?")
citations = [c for p in answer["answer"]["parts"] for c in p["citations"]]
assert citations and citations[0]["documentId"] == document
url = citations[0]["url"].replace("/knowledge/sources", "/api/knowledge/chat/sources")
assert lecturer.get(url).status_code == 200

# Another thread does not inherit this thread's personal conversational context.
other = lecturer.post(prefix).json()["id"]
isolated = send("Bạn nhớ tên mình không?", target=other)
assert "Minh" not in isolated["answer"]["notice"]
assert lecturer.delete(f"{prefix}/{other}").status_code == 204
assert lecturer.get(f"{prefix}/{other}").status_code == 404
assert int(sql(f'''SELECT count(*) FROM knowledge.chat_exchanges WHERE "ThreadId"='{other}' ''')) == 0
assert int(sql(f'''SELECT count(*) FROM knowledge.chat_checkpoints WHERE "ExchangeId"='{isolated['id']}' ''')) == 0
assert int(sql(f'''SELECT count(*) FROM knowledge.chat_checkpoint_writes WHERE "ExchangeId"='{isolated['id']}' ''')) == 0

internal = f"/api/knowledge/internal/agent/{thread}/{greeting['id']}/checkpoint"
assert lecturer.get(internal).status_code == 401
assert httpx.get(base + internal, headers={"X-Service-Key": "isolated-kb-service-key-32-characters-only", "X-Run-Lease": str(uuid.uuid4())}).status_code == 410
folder = Path("tmp/agent-ui"); folder.mkdir(parents=True, exist_ok=True)
(folder / "fixture.json").write_text(json.dumps({"threadId": thread, "documentId": document}), encoding="utf-8")
print("PASS agent API: greeting without retrieval, durable LangGraph checkpoint/writes, follow-up memory, idempotency, per-account/per-thread isolation, citations, cascade deletion and internal lease protection")
