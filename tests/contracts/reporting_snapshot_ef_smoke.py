"""Isolated seeded database only. Creates a temporary V2 course/CLO, edits the CLO,
then deletes that CLO and archives the temporary course in finally.
Checks report persistence remains independent of live EF-tracked curriculum entities.
Requires REPORT_TEST_URL, REPORT_TEST_AUTHOR (admin JWT, since this test edits Curriculum).
"""
import json
import os
import urllib.request
import urllib.error

base=os.environ['REPORT_TEST_URL'].rstrip('/')
headers={'Authorization':'Bearer '+os.environ['REPORT_TEST_AUTHOR'],'Content-Type':'application/json'}
def call(path,method='GET',body=None):
    req=urllib.request.Request(base+path,method=method,headers=headers,data=None if body is None else json.dumps(body).encode())
    with urllib.request.urlopen(req,timeout=30) as r:
        raw=r.read()
        return json.loads(raw) if raw else None

r='/api/reporting/reports'
old=call(r,'POST',{'kind':'clo','versionId':2})
# API writes mark provenance/status manual/draft, so use an already manual test CLO
# in a newly created course instead of editing a source-backed seed row.
course=call('/api/curriculum/program-versions/2/courses','POST',{'name':'Snapshot EF temporary course','credits':1,'semester':'TEST'})
coursepath=f'/api/curriculum/program-versions/2/courses/{course["id"]}'
try:
    temporary=call(coursepath+'/clos','POST',{'code':'CLO1','statement':'Nội dung trước khi thay đổi','levelCode':'C3'})
    before=call(r,'POST',{'kind':'course','versionId':2,'programCourseId':course['id']})
    call(coursepath+'/clos/'+str(temporary['id']),'PUT',{'code':'CLO1','statement':'Nội dung sau khi thay đổi','levelCode':'C3'})
    assert call(r+'/'+before['id'])==before
    after=call(r,'POST',{'kind':'course','versionId':2,'programCourseId':course['id']})
    assert before['snapshotHash'] != after['snapshotHash']
    assert call(r+'/'+old['id'])==old
finally:
    if 'temporary' in locals(): call(coursepath+'/clos/'+str(temporary['id']),'DELETE')
    call(coursepath,'DELETE')
print('PASS EF snapshots: source changes change new report hash; stored reports remain unchanged; fixture archived')
