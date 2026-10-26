"""Run only against an isolated test API. Creates its own program, exercises all writes,
and archives it on success. Requires CURRICULUM_TEST_URL. Standard library only.
"""
import json
import os
import urllib.request
import urllib.error
import uuid

base = os.environ['CURRICULUM_TEST_URL'].rstrip('/') + '/api/curriculum'
def call(path, method='GET', body=None, status=200):
    req=urllib.request.Request(base+path, method=method, headers={'Content-Type':'application/json'}, data=None if body is None else json.dumps(body).encode())
    try: r=urllib.request.urlopen(req,timeout=30)
    except urllib.error.HTTPError as e: r=e
    raw=r.read()
    assert r.status == status,(method,path,r.status,raw[:1000])
    return json.loads(raw) if raw else None

code='EF'+uuid.uuid4().hex[:12].upper()
program=call('/programs','POST',{'code':code,'name':'Chương trình thử nghiệm EF'},201)
pid=program['id']
call('/programs','POST',{'code':code,'name':'Chương trình trùng'},409)
assert call('/programs?q='+code)['totalItems']==1
call(f'/programs/{pid}','PUT',{'name':'Chương trình kiểm thử cập nhật'})
version=call(f'/programs/{pid}/versions','POST',{'versionCode':'V1'},201)
v='/program-versions/'+str(version['id'])
call(v+'/publish','POST',{},409)
call(v,'PUT',{'versionCode':'V1A'})
call(f'/programs/{pid}/versions','POST',{'versionCode':'COPY','sourceVersionId':version['id']},409)
course_input={'institutionalCode':code+'C','name':'Học phần thử nghiệm','credits':3,'semester':'Kỳ 1'}
course=call(v+'/courses','POST',course_input,201)
c=v+'/courses/'+str(course['id'])
call(v+'/courses','POST',course_input,409)
course_input['displayOrder']=2
call(c,'PUT',course_input)
assert call(v+'/courses?q='+code)['totalItems']==1
plo=call(v+'/plos','POST',{'code':'PLO1','statement':'Phân tích hệ thống dữ liệu','levelCode':'C4'},201)
p=v+'/plos/'+str(plo['id'])
call(v+'/plos','POST',{'code':'PLO1','statement':'Phân tích hệ thống dữ liệu'},409)
call(p,'PUT',{'code':'PLO1','statement':'Thiết kế hệ thống dữ liệu','levelCode':'C6'})
clo=call(c+'/clos','POST',{'code':'CLO1','statement':'Phân tích dữ liệu học phần','levelCode':'C4'},201)
cl=c+'/clos/'+str(clo['id'])
call(c+'/clos','POST',{'code':'CLO1','statement':'Phân tích dữ liệu học phần'},409)
call(cl,'PUT',{'code':'CLO1','statement':'Thiết kế dữ liệu học phần','levelCode':'C6'})
mapping=call(c+'/plo-mappings','POST',{'ploId':plo['id'],'weightCode':'X','progressionCode':'I'},201)
m=c+'/plo-mappings/'+str(mapping['id'])
call(c+'/plo-mappings','POST',{'ploId':plo['id'],'weightCode':'X','progressionCode':'I'},409)
call(m,'PUT',{'weightCode':'Y','progressionCode':'E'})
link=cl+'/plo-mappings'
call(link,'POST',{'coursePloId':mapping['id']},201)
call(link,'POST',{'coursePloId':mapping['id']},409)
assert len(call(link))==1
assert call(p,'DELETE',status=409)['code']=='plo_has_course_mappings'
assert call(m,'DELETE',status=409)['code']=='course_plo_has_clo_mappings'
check=call(v+'/plo-credit-check'); assert check['violationCount']==1
balance=call(v+'/plo-balance'); assert balance['items'][0]['score']==3
call(link+'/'+str(mapping['id']),'DELETE',status=204)
call(link,'POST',{'coursePloId':mapping['id']},201)
call(cl,'DELETE',status=204) # deleting CLO must remove dependent links atomically
call(m,'DELETE',status=204)
call(p,'DELETE',status=204)
assert call(v+'/plos')==[]
call(v+'/publish','POST',{})
call(c,'PUT',course_input,409)
copy=call(f'/programs/{pid}/versions','POST',{'versionCode':'V2','sourceVersionId':version['id']},201)
v2='/program-versions/'+str(copy['id'])
assert copy['courseCount']==1 and copy['status']=='draft'
assert call(v2+'/plos')==[] # Existing behavior copies courses, not outcomes.
copied=call(v2+'/courses')['items'][0]
assert copied['courseId']==course['courseId'] and copied['name']==course['name']
call(v2+'/publish','POST',{})
assert call(f'/programs/{pid}')['currentVersionId']==copy['id']
call(v2,'DELETE',status=204)
assert call(f'/programs/{pid}')['currentVersionId']==version['id']
v3=call(f'/programs/{pid}/versions','POST',{'versionCode':'V3','sourceVersionId':version['id']},201)
v3path='/program-versions/'+str(v3['id'])
c3=call(v3path+'/courses')['items'][0]
call(v3path+'/courses/'+str(c3['id']),'DELETE',status=204)
assert call(v3path+'/courses')['totalItems']==0
assert call(v3path+'/courses?includeArchived=true')['totalItems']==1
assert call(v3path+'/plo-balance')['isBalanced'] is False
call(f'/programs/{pid}','DELETE',status=204)
assert call('/programs?q='+code)['totalItems']==0
print('PASS Curriculum EF: all CRUD, duplicate conflicts, mapping dependencies, statistics, publish, clone, current replacement, archive')
