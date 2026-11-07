"""Only disposable obe-academic-test-db / academic_legacy_test and API :5298."""
import io, json, subprocess, time, uuid, zipfile
from pathlib import Path
import httpx

base='http://localhost:5298'; password='IsolatedAcademic123!'; changed='IsolatedAcademic456!'
def login(email):
    c=httpx.Client(base_url=base,timeout=30)
    r=c.post('/api/auth/login',json={'email':email,'password':password})
    if r.status_code==401: r=c.post('/api/auth/login',json={'email':email,'password':changed})
    assert r.status_code==200,r.text
    data=r.json(); c.headers['Authorization']='Bearer '+data['accessToken']
    if data['user']['mustChangePassword']:
        r=c.post('/api/auth/change-password',json={'currentPassword':password,'newPassword':changed}); assert r.status_code==200,r.text
        c.headers['Authorization']='Bearer '+r.json()['accessToken']
    return c
def sql(query):
    r=subprocess.run(['docker','exec','-i','obe-academic-test-db','psql','-v','ON_ERROR_STOP=1','-U','academic_test','-d','academic_legacy_test','-At'],input=query,text=True,capture_output=True)
    assert r.returncode==0,r.stderr
    return r.stdout.strip()
def check(r,status=200): assert r.status_code==status,(r.status_code,r.text); return r.json() if r.content else None

admin=login('academic-admin@example.test')
for role in ('lecturer','student'):
    r=admin.post('/api/admin/users',json={'email':f'academic-{role}@example.test','fullName':'Academic '+role,'role':role,'temporaryPassword':password})
    assert r.status_code in (201,409),r.text
lecturer=login('academic-lecturer@example.test'); student=login('academic-student@example.test')
users=check(admin.get('/api/admin/users'))['items']; lecturer_id=next(u['id'] for u in users if u['role']=='lecturer')
programs=check(admin.get('/api/curriculum/programs'))['items']; program=programs[0]
versions=check(admin.get(f"/api/curriculum/programs/{program['id']}/versions")); version=next(v for v in versions if v['status']=='finalized')['id']
prefix=f'/api/curriculum/program-versions/{version}'
courses=check(admin.get(prefix+'/courses?pageSize=100'))['items']; course=courses[0]['id']; other=courses[1]['id']
assert all(len(c['institutionalCode'])==6 and c['institutionalCode'].isascii() and c['institutionalCode'].isdigit() for c in courses)
assert sql("SELECT count(*) FROM curriculum.courses WHERE institutional_code IS NULL OR institutional_code !~ '^[0-9]{6}$'")=='0'
assert sql("SELECT count(*) FROM curriculum.program_courses WHERE course_code_snapshot IS NULL OR course_code_snapshot !~ '^[0-9]{6}$'")=='0'
assert int(sql('SELECT count(*) FROM curriculum.source_course_rows'))>0
period='2026-2027-HK1'
class_a=check(admin.post(prefix+'/classes',json={'academicPeriod':period,'code':'A1','name':'Lớp A1'}))
class_b=check(admin.post(prefix+'/classes',json={'academicPeriod':period,'code':'B1','name':'Lớp B1'}))
class_c=check(admin.post(prefix+'/classes',json={'academicPeriod':'2026-2027-HK2','code':'A1','name':'Lớp A1 kỳ 2'}))
mapping_before=sql('SELECT count(*) FROM curriculum.clo_plos')
offering=check(admin.post(f"{prefix}/classes/{class_a['id']}/offerings",json={'programCourseId':course,'lecturerId':lecturer_id}))
second=check(admin.post(f"{prefix}/classes/{class_b['id']}/offerings",json={'programCourseId':course}))
third=check(admin.post(f"{prefix}/classes/{class_c['id']}/offerings",json={'programCourseId':course}))
other_offering=check(admin.post(f"{prefix}/classes/{class_a['id']}/offerings",json={'programCourseId':other}))
assert offering['code']!=second['code'] and offering['programCourseId']==second['programCourseId']
assert all(len(o['code'])==6 and o['code'].isascii() and o['code'].isdigit() for o in [offering,second,third,other_offering])
assert sql('SELECT count(*) FROM curriculum.clo_plos')==mapping_before
subject_path=f'{prefix}/courses/{course}/offerings'
assert check(admin.get(subject_path))['totalItems']==3
assert {o['id'] for o in check(admin.get(subject_path))['items']}=={offering['id'],second['id'],third['id']}
assert [o['id'] for o in check(lecturer.get(subject_path))['items']]==[offering['id']]
assert check(admin.get(subject_path,params={'search':second['code']}))['totalItems']==1
assert check(admin.get(subject_path,params={'search':'B1'}))['items'][0]['id']==second['id']
assert check(admin.get(subject_path,params={'page':2}))['items']==[]
assert student.get(subject_path).status_code==403
assert httpx.get(base+subject_path).status_code==401
assert admin.get(f'{prefix}/courses/9999999/offerings').status_code==404
check(admin.post(f"{prefix}/classes/{class_a['id']}/offerings",json={'programCourseId':course}),409)
assert lecturer.post(prefix+'/classes',json={'academicPeriod':period,'code':'DENIED','name':'Denied'}).status_code==403
assert lecturer.get(f"{prefix}/offerings/{second['id']}").status_code==403
assert lecturer.get(f"{prefix}/classes/{class_b['id']}").status_code==404
assert check(lecturer.get(prefix+'/classes',params={'period':period}))['totalItems']==1
assert len(check(lecturer.get(f"{prefix}/classes/{class_a['id']}/offerings")))==1
assert lecturer.get(f"{prefix}/courses/{course}").status_code==200
# Teaching assignment grants mapping reads, not catalog outcome editing or another class's grades.
check(admin.put(f'/api/admin/users/{lecturer_id}/course-assignments',json={'ids':[]}),204)
assert lecturer.get(f"{prefix}/courses/{course}").status_code==200
path=f"{prefix}/offerings/{offering['id']}"

scheme_path=prefix+'/assessment-scheme'
assert httpx.get(base+scheme_path).status_code==401
assert student.get(scheme_path).status_code==403
assert lecturer.get(scheme_path).status_code==200
check(admin.put(f'/api/admin/users/{lecturer_id}/course-assignments',json={'ids':[course]}),204)
check(lecturer.get(scheme_path))
assert lecturer.get(f"{prefix}/offerings/{other_offering['id']}/students?period=2026-HK1").status_code==403
scheme={'revision':0,'components':[{'code':'QT','name':'Quá trình','weight':40,'maxScore':10},{'code':'THI','name':'Thi kết thúc','weight':60,'maxScore':100}]}
check(admin.put(scheme_path,json={**scheme,'components':[{**scheme['components'][0],'weight':99}]}),400)
check(lecturer.put(scheme_path,json=scheme),403)
saved=check(admin.put(scheme_path,json=scheme)); assert saved['revision']==1
check(admin.put(scheme_path,json=scheme),409)
enroll=check(lecturer.post(path+'/students',json={'studentCode':'001234','fullName':'Nguyễn Văn An','academicPeriod':'2026-HK1'}))
enrollment=enroll['id']; grade_path=f'{path}/students/{enrollment}/grades'
check(admin.post(path+'/students',json={'studentCode':'001234','fullName':'Nguyễn Văn An','academicPeriod':'2026-HK1'}),409)
check(admin.post(path+'/students',json={'studentCode':'001234','fullName':'Sai tên','academicPeriod':'2026-HK2'}),409)
check(admin.post(f"{prefix}/offerings/{third['id']}/students",json={'studentCode':'001234','fullName':'Nguyễn Văn An','academicPeriod':'2026-2027-HK2'}))
assert check(lecturer.get(path+'/students?period=2026-HK1'))['totalItems']==1
options=check(lecturer.get(prefix+'/academic-periods'))
assert any(b['name']=='2026-2027' and any(s['period']=='2026-2027-HK1' for s in b['semesters']) for b in options)
assert lecturer.get(f"{prefix}/offerings/{other_offering['id']}/academic-periods").status_code==403
assert student.get(path+'/academic-periods').status_code==403
assert student.get(path+'/students?period=2026-HK1').status_code==403
assert admin.get(f"{prefix}/offerings/{other_offering['id']}/students/{enrollment}/grades").status_code==404
request={'revision':0,'schemeRevision':1,'scores':{'QT':8,'THI':80}}
check(lecturer.put(grade_path,json={**request,'scores':{'QT':11}}),400)
book=check(lecturer.put(grade_path,json=request)); assert book['enrollment']['totalScore']==8 and len(book['history'])==2
check(lecturer.put(grade_path,json=request),409)
assert check(admin.get(scheme_path))['locked']
check(admin.put(scheme_path,json={**scheme,'revision':1}),409)
book=check(admin.put(grade_path,json={**request,'revision':book['enrollment']['revision'],'scores':{'QT':0,'THI':None}}))
assert book['enrollment']['totalScore'] is None and book['scores']['QT']==0 and book['scores']['THI'] is None
assert book['history'][0]['previousScore'] is not None

# Read/preview/apply CSV or Sheets: all-or-nothing and explicit overwrite.
parsed=check(admin.post(path+'/import/read',data={'mode':'students','text':'MSSV\tHoTen\n000002\tTrần Bình\n000003\tLê Chi'}))
payload={'mode':'students','academicPeriod':period,'offeringId':offering['id'],'schemeRevision':1,'rows':parsed['rows'],'overwriteExisting':False}
preview=check(lecturer.post(path+'/import/preview',json=payload)); assert preview['canApply']
assert check(admin.get(path+'/students?period=2026-HK1'))['totalItems']==1
payload['rows']=[r['row'] for r in preview['rows']]
result=check(lecturer.post(path+'/import/apply',json=payload)); assert result['appliedRows']==2
again=check(lecturer.post(path+'/import/preview',json=payload)); assert all(r['action']=='skip' for r in again['rows'])
bad={**payload,'rows':[{'rowNumber':2,'studentCode':'004444','fullName':'Test Valid','scores':{}},{'rowNumber':3,'studentCode':'001234','fullName':'Sai tên','scores':{}}]}
result=check(admin.post(path+'/import/apply',json=bad)); assert not result['canApply']
assert check(admin.get(path+'/students?period=2026-HK1&search=004444'))['totalItems']==0
parsed=check(admin.post(path+'/import/read',files={'file':('grades.csv','MSSV,QT,THI\n000002,9,90\n000003,0,0'.encode('utf-8'),'text/csv')},data={'mode':'grades'}))
payload={**payload,'mode':'grades','rows':parsed['rows']}
preview=check(admin.post(path+'/import/preview',json=payload)); assert preview['canApply']; payload['rows']=[r['row'] for r in preview['rows']]
check(admin.post(path+'/import/apply',json=payload))
rows=check(admin.get(path+'/students?period=2026-HK1'))['items']; totals={r['studentCode']:r['totalScore'] for r in rows}; assert totals['000002']==9 and totals['000003']==0
payload['rows'][0]['scores']['QT']=8
assert not check(admin.post(path+'/import/preview',json=payload))['canApply']
payload['overwriteExisting']=True
preview=check(admin.post(path+'/import/preview',json=payload)); assert preview['canApply']; payload['rows']=[r['row'] for r in preview['rows']]
target=next(r['id'] for r in rows if r['studentCode']=='000002'); gpath=f'{path}/students/{target}/grades'
current=check(admin.get(gpath)); check(admin.put(gpath,json={'revision':current['enrollment']['revision'],'schemeRevision':1,'scores':{'QT':7}}))
assert not check(admin.post(path+'/import/apply',json=payload))['canApply']
preview=check(admin.post(path+'/import/preview',json=payload)); payload['rows']=[r['row'] for r in preview['rows']]
check(admin.post(path+'/import/apply',json=payload)); assert check(admin.get(gpath))['scores']['QT']==8
assert check(admin.get(gpath))['history'][0]['source']=='import'
check(admin.post(path+'/import/read',files={'file':('bad.csv',b'MSSV,BAD\n000002,4','text/csv')},data={'mode':'grades'}),400)
template=admin.get(path+'/import/template?mode=grades'); assert template.status_code==200
with zipfile.ZipFile(io.BytesIO(template.content)) as z: assert b'QT' in z.read('xl/worksheets/sheet1.xml')

# Clone scheme while keeping enrollment/grade history scoped to its original version.
clone=check(admin.post(f"/api/curriculum/programs/{program['id']}/versions",json={'versionCode':'ACADEMIC-'+uuid.uuid4().hex[:8],'sourceVersionId':version}),201)
clone_scheme=check(admin.get(f"/api/curriculum/program-versions/{clone['id']}/assessment-scheme")); assert len(clone_scheme['components'])==2 and not clone_scheme['locked']
clone_courses=check(admin.get(f"/api/curriculum/program-versions/{clone['id']}/courses?pageSize=100"))['items']; cloned_course=clone_courses[0]['id']
assert check(admin.get(f"/api/curriculum/program-versions/{clone['id']}/classes?period={period}"))['totalItems']==0
check(admin.post(f"/api/curriculum/program-versions/{clone['id']}/courses",json={'institutionalCode':'CS101','name':'Test course','credits':3,'semester':'1','displayOrder':999}),400)
numeric=check(admin.post(f"/api/curriculum/program-versions/{clone['id']}/courses",json={'institutionalCode':'012345','name':'Test course','credits':3,'semester':'1','displayOrder':999}),201); assert numeric['institutionalCode']=='012345'
check(admin.put(f'/api/admin/users/{lecturer_id}/course-assignments',json={'ids':[]}),204)
check(admin.put(path+'/lecturer',json={'lecturerId':None}))
check(lecturer.get(grade_path),403)
check(admin.put(path+'/lecturer',json={'lecturerId':lecturer_id}))
check(lecturer.get(grade_path))
# Same student in the same subject and semester has independent grades per class.
second_path=f"{prefix}/offerings/{second['id']}"
se=check(admin.post(second_path+'/students',json={'studentCode':'001234','fullName':'Nguyễn Văn An','academicPeriod':period}))
assert se['id']!=enrollment
assert check(admin.get(second_path+f"/students/{se['id']}/grades"))['scores']['QT'] is None
assert admin.get(second_path+f'/students/{enrollment}/grades').status_code==404
assert admin.put(second_path+f'/students/{enrollment}/grades',json={'revision':0,'schemeRevision':1,'scores':{'QT':1}}).status_code==404
assert lecturer.get(second_path+'/students?period='+period).status_code==403
assert lecturer.post(second_path+'/import/preview',json=payload).status_code==403
assert admin.post(second_path+'/import/preview',json=payload).status_code==400
assert lecturer.put(path+'/lecturer',json={'lecturerId':lecturer_id}).status_code==403
assert check(lecturer.get(f"{prefix}/courses/{course}/clos")) is not None
folder=Path('tmp/academic-test'); folder.mkdir(parents=True,exist_ok=True)
(folder/'fixture.json').write_text(json.dumps({'versionId':version,'courseId':course,'offeringId':offering['id'],'classId':class_a['id'],'enrollmentId':enrollment,'cloneVersionId':clone['id'],'cloneCourseId':cloned_course,'studentIds':{r['studentCode']:r['id'] for r in rows}}),encoding='utf-8')
print('PASS academics: legacy six-digit migration, shared CTDT grading, scope/role permissions, enrollments per period, missing/zero/weighted grades, audit, optimistic concurrency, atomic CSV/Sheets imports, overwrite/replay protection, Excel template and scheme clone without student grades')
