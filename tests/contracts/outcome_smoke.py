"""Real HTTP/DB OBE acceptance. Run only on a disposable stack initialized with demo.
SMOKE_BASE defaults to :5298. Uses unique fixtures; does not touch existing programs.
The dedicated stack must have academic-admin@example.test / IsolatedAcademic123!.
"""
import json, os, uuid
from pathlib import Path
import httpx

base=os.environ.get('SMOKE_BASE','http://127.0.0.1:5298')
tag=uuid.uuid4().hex[:8].upper(); password='IsolatedAcademic123!'; changed='IsolatedAcademic456!'
def check(r,status=200):
    assert r.status_code==status,(r.request.method,str(r.request.url),r.status_code,r.text[:1800])
    return r.json() if r.content else None
def login(email):
    c=httpx.Client(base_url=base,timeout=60)
    r=c.post('/api/auth/login',json={'email':email,'password':password})
    if r.status_code==401:r=c.post('/api/auth/login',json={'email':email,'password':changed})
    session=check(r);c.headers['Authorization']='Bearer '+session['accessToken']
    if session['user']['mustChangePassword']:
        session=check(c.post('/api/auth/change-password',json={'currentPassword':password,'newPassword':changed}))
        c.headers['Authorization']='Bearer '+session['accessToken']
    return c
admin=login('academic-admin@example.test')
def account(role):
    email=f'obe-{role}-{tag.lower()}-{uuid.uuid4().hex[:4]}@example.test'
    u=check(admin.post('/api/admin/users',json={'email':email,'fullName':role+' OBE','role':role,'temporaryPassword':password}),201)
    return u,login(email)
teacher,lecturer=account('lecturer'); outsider,unassigned=account('lecturer')
fm,faculty=account('faculty_manager');dm,department=account('department_manager'); stranger,wrong_faculty=account('faculty_manager')
su,student=account('student')
u=check(admin.post('/api/curriculum/universities',json={'code':'OBE'+tag,'name':'OBE test '+tag}),201)
uni=f"/api/curriculum/universities/{u['id']}"
f=check(admin.post(uni+'/org-units',json={'kind':'faculty','code':'F1','name':'Faculty one','parentId':None}))
f2=check(admin.post(uni+'/org-units',json={'kind':'faculty','code':'F2','name':'Faculty two','parentId':None}))
d=check(admin.post(uni+'/org-units',json={'kind':'department','code':'D1','name':'Department one','parentId':f['id']}))
for person,unit in [(fm,f),(dm,d),(stranger,f2)]:check(admin.put(f"/api/admin/users/{person['id']}/org-unit-assignments",json=[unit['id']]),204)
check(admin.put(f"/api/admin/users/{dm['id']}/org-unit-assignments",json=[f['id']]),400)
p=check(admin.post('/api/curriculum/programs',json={'universityId':u['id'],'code':'OBE','name':'OBE fixture'}),201)
check(admin.put(f"/api/curriculum/programs/{p['id']}/faculty",json={'orgUnitId':f['id']}),204)
v=check(admin.post(f"/api/curriculum/programs/{p['id']}/versions",json={'versionCode':'OBE1','sourceVersionId':None}),201)
root=f"/api/curriculum/program-versions/{v['id']}"
course=check(admin.post(root+'/courses',json={'institutionalCode':'900101','name':'Outcome fixture','credits':3,'semester':'1','displayOrder':1}),201)
cid=course['id'];cp=f'{root}/courses/{cid}'
check(admin.put(cp+'/catalog',json={'departmentId':d['id'],'countsTowardGpa':True}),204)
plo=check(faculty.post(root+'/plos',json={'code':'PLO1','statement':'Apply engineering knowledge effectively','levelCode':None}),201)
clos=[check(admin.post(cp+'/clos',json={'code':f'CLO{i}','statement':f'Apply learning objective number {i} effectively','levelCode':None}),201) for i in (1,2)]
mapping=check(department.post(cp+'/plo-mappings',json={'ploId':plo['id'],'weightCode':'X','progressionCode':'I','fit':None,'fitReason':None}),201)
for clo in clos:check(department.post(cp+f"/clos/{clo['id']}/plo-mappings",json={'coursePloId':mapping['id']}),201)
scheme={'revision':0,'components':[{'code':'A','name':'Process','kind':'process','weight':40,'maxScore':10,'clos':{'CLO1':100}},
    {'code':'B','name':'Final exam','kind':'final','weight':60,'maxScore':10,'clos':{'CLO1':50,'CLO2':50}}]}
scheme=check(department.put(cp+'/assessment-scheme',json=scheme))
academic_policy=check(admin.get(uni+'/grading-policy'))
check(admin.put(uni+'/grading-policy',json={**academic_policy,'retakePolicy':'latest'}))
assert check(admin.get(root+'/outcome-policy'))['retakePolicy']=='latest'
policy=check(faculty.put(root+'/outcome-policy',json={'revision':0,'cloThreshold':5,'ploThreshold':5,'groupTargetRate':70,'retakePolicy':'highest','formulaVersion':'obe/1'}))
check(faculty.put(root+'/outcome-policy',json={**policy,'revision':0}),409)
check(wrong_faculty.get(root+'/outcome-policy'),403)
check(faculty.post(root+'/publish'),403)
check(admin.post(root+'/publish'))
o=check(admin.post(root+'/offerings',json={'programCourseId':cid,'academicPeriod':'2026-2027-HK1','groupName':'N01','lecturers':[{'userId':teacher['id'],'role':'main'}]}))
op=f"{root}/offerings/{o['id']}"
enrollments=[]
for i,(a,b) in enumerate([(8,6),(4,2),(10,8)],1):
    e=check(admin.post(op+'/students',json={'studentCode':f'{tag}S{i}','fullName':f'Student {i}','academicPeriod':'2026-2027-HK1'}));enrollments.append(e)
    check(lecturer.put(op+f"/students/{e['id']}/grades",json={'revision':e['revision'],'schemeRevision':scheme['revision'],'scores':{'A':a,'B':b},'specialMark':None,'reason':None}))
check(unassigned.get(root+f"/plo-results?offeringId={o['id']}"),403)
draft=check(admin.get(root+f"/plo-results?offeringId={o['id']}"))
assert all(c['score'] is None for c in draft['cloStudents'])
assert all(value is None for a in draft['input']['attempts'] for value in a['scores'].values()),'unpublished grade leak'
check(lecturer.post(op+'/grades/submit',json={'reason':None}))
check(admin.post(op+'/grades/publish',json={'reason':None}))
result=check(admin.get(root+f"/plo-results?offeringId={o['id']}"))
summary=check(admin.get(root+f"/outcome-summary?offeringId={o['id']}"))
assert 'input' not in summary and 'cloStudents' not in summary and summary['sourceHash']==result['sourceHash']
page=check(admin.get(root+f"/outcome-students?offeringId={o['id']}&page=1"))
assert page['totalItems']==9 and len(page['items'])==9
assert not check(admin.get(root+f"/outcome-students?offeringId={o['id']}&page=2"))['items']
assert result['status']=='official',result['issues']
clo1=next(c for c in result['cloStudents'] if c['studentCode']==tag+'S1' and c['outcomeId']==clos[0]['id'])
assert abs(clo1['score']-50/7)<1e-10
assert result['clos'][0]['evaluatedCount']==3 and result['clos'][0]['achievedCount']==2
assert abs(result['clos'][0]['achievementRate']-200/3)<1e-10
assert len(result['gpas'])==3
scope={'versionId':v['id'],'offeringId':o['id'],'preview':False}
snap=check(faculty.post(root+'/outcome-snapshots',json=scope));sid=snap['id']
assert check(admin.get(root+'/outcome-snapshots/latest',params={'offeringId':o['id']}))['id']==sid
check(department.get(root+f'/outcome-snapshots/{sid}'))
check(wrong_faculty.get(root+f'/outcome-snapshots/{sid}'),403)
check(lecturer.get(root+'/plo-results'),403)
check(lecturer.get(root+f"/plo-results?offeringId={o['id']}"))
check(lecturer.post(root+'/outcome-snapshots',json=scope),403)
export=check(admin.get(root+f'/outcome-snapshots/{sid}/export'))
assert export['result']['sourceHash']==result['sourceHash']
consumer=check(admin.get(f"/api/reporting/outcome-source/{v['id']}/{sid}"))
assert consumer['result']['sourceHash']==result['sourceHash']
book=check(admin.get(op+f"/students/{enrollments[0]['id']}/grades"))
check(admin.put(op+f"/students/{enrollments[0]['id']}/grades",json={'revision':book['enrollment']['revision'],'schemeRevision':scheme['revision'],'scores':{'A':9,'B':8},'reason':'Regrade fixture test','specialMark':None}))
historical=check(admin.get(root+f'/outcome-snapshots/{sid}'))
assert historical['stale'] and historical['result']['sourceHash']==result['sourceHash']
check(admin.get(root+'/outcome-snapshots/latest',params={'offeringId':o['id']}),409)
clone=check(admin.post(f"/api/curriculum/programs/{p['id']}/versions",json={'versionCode':'COPY','sourceVersionId':v['id']}),201)
clone_root=f"/api/curriculum/program-versions/{clone['id']}"
check(admin.post(clone_root+f"/copy-outcomes/{v['id']}"),204)
audit_before=check(admin.get('/api/curriculum/audit-events'))['totalItems']
check(admin.post(clone_root+f"/copy-outcomes/{v['id']}"),409)
assert check(admin.get('/api/curriculum/audit-events'))['totalItems']==audit_before
assert len(check(admin.get(clone_root+'/plos')))==1
assert not check(admin.get(clone_root+'/outcome-readiness'))['issues']
# Own published result only, including nested source arrays.
check(admin.put(f"/api/admin/users/{su['id']}/program-assignments",json={'ids':[p['id']]}),204)
rows=check(admin.get(uni+'/students',params={'search':tag+'S1'}))['items'];s=next(s for s in rows if s['studentCode']==tag+'S1')
check(admin.put(uni+f"/students/{s['id']}",json={'fullName':s['fullName'],'cohortCode':'K1','status':'studying','homeClassId':None,'userId':su['id']}))
own=check(student.get(root+'/plo-results'))
assert len(own['input']['attempts'])==1 and all(x['studentCode']==tag+'S1' for x in own['cloStudents'])
check(student.get(root+'/plo-results?preview=true'),403)
check(student.get(root+f'/outcome-snapshots/{sid}/export'),403)
# Scope revocation applies to historical exports, too.
check(admin.put(f"/api/admin/users/{fm['id']}/org-unit-assignments",json=[]),204)
check(faculty.get(root+f'/outcome-snapshots/{sid}/export'),403)
clone_snapshot=check(admin.post(clone_root+'/outcome-snapshots',json={'versionId':clone['id'],'preview':True}))
check(admin.delete(clone_root),204)
assert check(admin.get(clone_root+f"/outcome-snapshots/{clone_snapshot['id']}"))['stale']
audit=check(admin.get('/api/curriculum/audit-events'))
assert audit['totalItems']>0 and any(x['entity']=='OutcomeSnapshotRecord' for x in audit['items'])
check(department.get('/api/curriculum/audit-events'),403)
draft_offering=check(admin.post(root+'/offerings',json={'programCourseId':cid,'academicPeriod':'2026-2027-HK2','groupName':'N01','lecturers':[{'userId':teacher['id'],'role':'main'}]}))
check(admin.post(root+f"/offerings/{draft_offering['id']}/students",json={'studentCode':tag+'S1','fullName':'Student 1','academicPeriod':'2026-2027-HK2'}))
second_student=next(s for s in check(admin.get(uni+'/students',params={'search':tag+'S2'}))['items'] if s['studentCode']==tag+'S2')
check(admin.put(uni+f"/students/{second_student['id']}",json={'fullName':second_student['fullName'],'cohortCode':'K2','status':'studying','homeClassId':None,'userId':None}))
options=check(admin.get(root+'/outcome-options'))
assert options['periods']==['2026-2027-HK1','2026-2027-HK2'] and options['cohorts']==['K1','K2']
assert check(lecturer.get(root+'/outcome-options'))['cohorts']==['K1','K2']
assert check(student.get(root+'/outcome-options'))['cohorts']==['K1']
check(unassigned.get(root+'/outcome-options'),403)
check(admin.put(f"/api/admin/users/{outsider['id']}/course-assignments",json={'ids':[cid]}),204)
restricted=check(unassigned.get(root+'/outcome-options'))
assert restricted['cohorts']==[] and restricted['periods']==[]
assert len(check(admin.get(root+'/plo-results',params={'offeringId':o['id'],'cohort':'K1'}))['input']['attempts'])==1
out=Path('tmp/outcome-test');out.mkdir(parents=True,exist_ok=True)
(out/'fixture.json').write_text(json.dumps({'versionId':v['id'],'offeringId':o['id'],'courseId':cid,'snapshotId':sid,'draftOfferingId':draft_offering['id'],'tag':tag,'teacherEmail':teacher['email']}),encoding='utf-8')
print('PASS outcome HTTP acceptance: golden CLO/PLO, published-only source, GPA, revision conflict, clone, snapshot immutable/stale/export, faculty/department boundaries, teacher scope, own-student access, revoked export')
