"""Disposable EF/Postgres stack at :5298 / obe-curriculum-tool-test-db only."""
import json
import subprocess
import uuid
from pathlib import Path
import httpx

base = 'http://localhost:5298'
key = 'isolated-kb-service-key-32-characters-only'
client = httpx.Client(base_url=base, timeout=30)
r = client.post('/api/auth/login', json={'email':'agent-admin@example.test','password':'IsolatedAgent123!'})
assert r.status_code == 200, r.text
token = r.json(); client.headers['Authorization'] = 'Bearer ' + token['accessToken']
if token['user']['mustChangePassword']:
    token = client.post('/api/auth/change-password', json={'currentPassword':'IsolatedAgent123!','newPassword':'IsolatedAgent456!'}).json()
    client.headers['Authorization'] = 'Bearer ' + token['accessToken']
admin_id = token['user']['id']
r = client.post('/api/admin/users', json={'email':'tool-lecturer@example.test','fullName':'Tool Lecturer','role':'lecturer','temporaryPassword':'IsolatedAgent123!'})
assert r.status_code == 201, r.text
lecturer = httpx.Client(base_url=base)
token = lecturer.post('/api/auth/login', json={'email':'tool-lecturer@example.test','password':'IsolatedAgent123!'}).json()
lecturer.headers['Authorization'] = 'Bearer ' + token['accessToken']
token = lecturer.post('/api/auth/change-password', json={'currentPassword':'IsolatedAgent123!','newPassword':'IsolatedAgent456!'}).json()
lecturer_id = token['user']['id']

def sql(query):
    r = subprocess.run(['docker','exec','-i','obe-curriculum-tool-test-db','psql','-v','ON_ERROR_STOP=1','-U','agent_test','-d','agent_test','-At'], input=query, capture_output=True, text=True)
    assert r.returncode == 0, r.stderr

def run_for(owner):
    thread, run, lease = [str(uuid.uuid4()) for _ in range(3)]
    sql(f'''INSERT INTO knowledge.chat_threads ("Id","OwnerId","Title","CreatedAt","UpdatedAt") VALUES ('{thread}',{owner},'Tool test',now(),now());
    INSERT INTO knowledge.chat_exchanges ("Id","ThreadId","Question","Status","Phase","Lease","LeaseUntil","Attempts","CreatedAt")
    VALUES ('{run}','{thread}','Synthetic curriculum test','processing','route','{lease}',now()+interval '30 minutes',1,now());''')
    return {'threadId':thread,'runId':run,'lease':lease}

admin_run, lecturer_run = run_for(admin_id), run_for(lecturer_id)
def call(run, tool, status=200, **kwargs):
    path=f"/api/knowledge/internal/agent/{run['threadId']}/{run['runId']}/curriculum"
    r=httpx.post(base+path, headers={'X-Service-Key':key,'X-Run-Lease':run['lease']}, json={'tool':tool,**kwargs}, timeout=30)
    assert r.status_code == status, (r.status_code,r.text)
    return r.json() if status==200 else None

programs=call(admin_run,'programs')['data']['items']; assert programs
assert not call(lecturer_run,'programs')['data']['items']
program=programs[0]['id']
versions=call(admin_run,'versions',programId=program)['data']['items']
version=next((v['id'] for v in versions if v['isCurrent']),versions[0]['id'])
courses=call(admin_run,'courses',versionId=version)['data']
assert courses['totalItems']>25 and courses['hasMore']
second=call(admin_run,'courses',versionId=version,page=2)['data']
assert not {c['id'] for c in courses['items']} & {c['id'] for c in second['items']}
course=courses['items'][0]['id']; other=courses['items'][1]['id']
r=client.put(f'/api/admin/users/{lecturer_id}/course-assignments',json={'ids':[course]}); assert r.status_code==204,r.text
allowed=call(lecturer_run,'courses',versionId=version)['data']
assert allowed['totalItems']==1 and allowed['items'][0]['id']==course
assert call(lecturer_run,'plos',versionId=version)['data']['items']
clos=call(lecturer_run,'clos',versionId=version,courseId=course)['data']['items']; assert clos
mapping=call(lecturer_run,'course_plo',versionId=version,courseId=course)['data']['items']
assert mapping and all(m['weightCode'] in ('X','Y') and m['progressionCode'] in ('I','R','E') for m in mapping)
call(lecturer_run,'clo_plo',versionId=version,courseId=course,cloId=clos[0]['id'])
call(lecturer_run,'clos',status=403,versionId=version,courseId=other)
call(admin_run,'sql',status=400,versionId=version)
call(admin_run,'plos',status=400,versionId=version,page=-1)
call(lecturer_run,'plos',status=403,versionId=999999)
client.put(f'/api/admin/users/{lecturer_id}/course-assignments',json={'ids':[]}).raise_for_status()
call(lecturer_run,'plos',status=403,versionId=version)
path=f"/api/knowledge/internal/agent/{admin_run['threadId']}/{admin_run['runId']}/curriculum"
assert httpx.post(base+path,json={'tool':'programs'}).status_code==401
assert httpx.post(base+path,headers={'X-Service-Key':key,'X-Run-Lease':str(uuid.uuid4())},json={'tool':'programs'}).status_code==410
folder=Path('tmp/curriculum-agent'); folder.mkdir(parents=True,exist_ok=True)
(folder/'fixture.json').write_text(json.dumps({'admin':admin_run,'versionId':version,'courseId':course}),encoding='utf-8')
print('PASS: all seven read tools, EF data/pagination, exact X/Y and I/R/E, lecturer course scope, revoked assignment, forbidden SQL and internal lease authentication')
