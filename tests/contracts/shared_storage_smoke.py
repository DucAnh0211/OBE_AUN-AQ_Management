"""Disposable API configured to the dedicated MinIO instance. No AI processing needed."""
import hashlib,json,os,uuid
from pathlib import Path
import httpx
base=os.environ.get('SMOKE_BASE','http://127.0.0.1:5298')
def check(r,status=200):
    assert r.status_code==status,(r.status_code,r.text[:800])
    return r.json() if r.content else None
def client(email):
    c=httpx.Client(base_url=base,timeout=30)
    s=check(c.post('/api/auth/login',json={'email':email,'password':'IsolatedAcademic456!'}));c.headers['Authorization']='Bearer '+s['accessToken'];return c
admin=client('academic-admin@example.test');fixture=json.loads(Path('tmp/outcome-test/fixture.json').read_text())
teacher=client(fixture['teacherEmail']);payload=b'Isolated OBE source document - storage contract verification.'
doc=check(admin.post('/api/knowledge/documents',data={'title':'Shared storage '+uuid.uuid4().hex[:6]},files={'file':('obe-test.txt',payload,'text/plain')}),201)
assert doc['sha256'].lower()==hashlib.sha256(payload).hexdigest()
response=admin.get(f"/api/knowledge/documents/{doc['id']}/original");assert response.status_code==200 and response.content==payload
assert response.headers['cache-control']=='no-store'
check(teacher.get(f"/api/knowledge/documents/{doc['id']}/original"),403)
check(admin.post(f"/api/knowledge/documents/{doc['id']}/disable"),204)
check(admin.get(f"/api/knowledge/documents/{doc['id']}/original"),410)
check(admin.post('/api/knowledge/documents',data={'title':'Invalid'},files={'file':('blocked.exe',payload,'application/octet-stream')}),400)
print('PASS shared storage HTTP/MinIO: immutable upload, SHA256/round-trip, authorization, disabled originals, rejected extension; no AI call')
