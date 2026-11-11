"""Disposable stack only (never real data). Default API :5298 + container obe-academic-test-db;
override with SMOKE_BASE and SMOKE_DATABASE_URL (psycopg) when running inside the compose network.
Covers the academic records model aligned with Thông tư 08/2021/TT-BGDĐT: khoa/bộ môn, hồ sơ giảng viên,
lớp sinh hoạt, lớp học phần + nhiều giảng viên, cơ cấu điểm theo đề cương + CLO, quy chế/thang điểm,
nộp → công bố → phúc khảo, học lại và bảng điểm/GPA."""
import io, json, os, subprocess, uuid, zipfile
from pathlib import Path
import httpx

base=os.environ.get('SMOKE_BASE','http://localhost:5298'); password='IsolatedAcademic123!'; changed='IsolatedAcademic456!'
tag=uuid.uuid4().hex[:6].upper()
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
    url=os.environ.get('SMOKE_DATABASE_URL')
    if url:
        import psycopg
        with psycopg.connect(url) as conn: return str(conn.execute(query).fetchone()[0])
    r=subprocess.run(['docker','exec','-i','obe-academic-test-db','psql','-v','ON_ERROR_STOP=1','-U','academic_test','-d','academic_legacy_test','-At'],input=query,text=True,capture_output=True)
    assert r.returncode==0,r.stderr
    return r.stdout.strip()
def check(r,status=200): assert r.status_code==status,(r.request.method,str(r.request.url),r.status_code,r.text); return r.json() if r.content else None

admin=login('academic-admin@example.test')
student_email=f'academic-student-{tag.lower()}@example.test'   # mỗi lần chạy một tài khoản sinh viên mới
for email,name,role in (('academic-lecturer@example.test','Academic lecturer','lecturer'),('academic-assistant@example.test','Academic assistant','lecturer'),(student_email,'Academic student','student')):
    r=admin.post('/api/admin/users',json={'email':email,'fullName':name,'role':role,'temporaryPassword':password})
    assert r.status_code in (201,409),r.text
lecturer=login('academic-lecturer@example.test'); assistant=login('academic-assistant@example.test'); student=login(student_email)
uid={u['email']:u['id'] for e in ('academic-lecturer','academic-assistant',student_email) for u in check(admin.get('/api/admin/users',params={'q':e,'pageSize':100}))['items']}; lecturer_id=uid['academic-lecturer@example.test']; assistant_id=uid['academic-assistant@example.test']; student_account=uid[student_email]
programs=check(admin.get('/api/curriculum/programs'))['items']; program=programs[0]; university=program['universityId']
versions=check(admin.get(f"/api/curriculum/programs/{program['id']}/versions")); version=next(v for v in versions if v['status']=='finalized')['id']
prefix=f'/api/curriculum/program-versions/{version}'; uni=f'/api/curriculum/universities/{university}'
courses=check(admin.get(prefix+'/courses?pageSize=100'))['items']
course=next(c for c in courses if c['credits']>=2 and check(admin.get(f"{prefix}/courses/{c['id']}/clos"))     # học phần chưa có lịch sử điểm
             and not check(admin.get(f"{prefix}/courses/{c['id']}/assessment-scheme"))['locked'])
other=next(c for c in courses if c['id']!=course['id'])
clos=[c['code'] for c in check(admin.get(f"{prefix}/courses/{course['id']}/clos"))]
course_id=course['id']

# ---- Khoa / bộ môn, hồ sơ giảng viên ----
faculty=check(admin.post(uni+'/org-units',json={'kind':'faculty','code':'K'+tag,'name':'Khoa kiểm thử','parentId':None}))
dept=check(admin.post(uni+'/org-units',json={'kind':'department','code':'BM'+tag,'name':'Bộ môn kiểm thử','parentId':faculty['id']}))
check(admin.post(uni+'/org-units',json={'kind':'department','code':'X'+tag,'name':'Không có khoa','parentId':None}),400)
check(admin.post(uni+'/org-units',json={'kind':'faculty','code':'K'+tag,'name':'Trùng mã','parentId':None}),409)
check(lecturer.post(uni+'/org-units',json={'kind':'faculty','code':'L'+tag,'name':'Không được phép','parentId':None}),403)
assert {u['id'] for u in check(lecturer.get(uni+'/org-units'))}>={faculty['id'],dept['id']}
check(admin.put(f"/api/curriculum/programs/{program['id']}/faculty",json={'orgUnitId':faculty['id']}),204)
assert check(admin.get(f"/api/curriculum/programs/{program['id']}"))['facultyId']==faculty['id']
check(admin.put(f"{prefix}/courses/{course_id}/catalog",json={'departmentId':dept['id'],'countsTowardGpa':True}),204)
assert check(admin.get(f"{prefix}/courses/{course_id}"))['departmentId']==dept['id']
check(admin.put(f'/api/curriculum/lecturers/{lecturer_id}',json={'lecturerCode':'GV'+tag,'departmentId':dept['id'],'academicTitle':'PGS','degree':'TS'}))
check(admin.put(f'/api/curriculum/lecturers/{assistant_id}',json={'lecturerCode':'GV'+tag,'departmentId':None,'academicTitle':None,'degree':'ThS'}),409)
check(admin.put(f'/api/curriculum/lecturers/{student_account}',json={'lecturerCode':'SV'+tag,'departmentId':None,'academicTitle':None,'degree':None}),404)
profile=next(p for p in check(admin.get('/api/curriculum/lecturers',params={'search':'Academic lecturer'})) if p['userId']==lecturer_id)
assert profile['departmentName']=='Bộ môn kiểm thử' and profile['academicTitle']=='PGS'
check(lecturer.get('/api/curriculum/lecturers'),403)

# ---- Quy chế & thang điểm (mặc định TT08) ----
policy=check(admin.get(uni+'/grading-policy'))
assert [b['letter'] for b in policy['bands']]==['A','B','C','D','F'] and policy['courseScoreDecimals']==1 and policy['minComponents']==2
check(admin.put(uni+'/grading-policy',json={**policy,'bands':[{'letter':'A','minScore':8.5,'point4':4,'isPassing':True}]}),400)
policy=check(admin.put(uni+'/grading-policy',json={**policy,'minFinalWeight':50}))
assert policy['minFinalWeight']==50

# ---- Lớp sinh hoạt + hồ sơ sinh viên ----
hc=check(admin.post(prefix+'/home-classes',json={'code':'CNTT'+tag,'name':'CNTT kiểm thử','cohortCode':'K66','advisorUserId':lecturer_id}))
assert hc['advisorName']=='Academic lecturer'
check(admin.post(prefix+'/home-classes',json={'code':'CNTT'+tag,'name':'Trùng','cohortCode':None,'advisorUserId':None}),409)
codes=[f'{tag}1',f'{tag}2',f'{tag}3',f'{tag}4']
hpath=f"{prefix}/home-classes/{hc['id']}/students"
parsed=check(admin.post(hpath+'/import/read',data={'mode':'students','text':f'MSSV\tHoTen\n{codes[0]}\tNguyễn An\n{codes[1]}\tTrần Bình'}))
preview=check(admin.post(hpath+'/import/preview',json={'rows':parsed['rows']})); assert preview['canApply'] and all(r['action']=='add' for r in preview['rows'])
assert check(admin.post(hpath+'/import/apply',json={'rows':parsed['rows']}))['appliedRows']==2
check(admin.post(hpath,json={'studentCode':codes[2],'fullName':'Lê Chi'}))
check(admin.post(hpath,json={'studentCode':codes[2],'fullName':'Sai tên'}),409)
roster=check(lecturer.get(uni+'/students',params={'homeClassId':hc['id'],'versionId':version}))['items']   # cố vấn học tập
assert len(roster)==3 and all(s['homeClassCode']=='CNTT'+tag and s['cohortCode']=='K66' for s in roster)
check(assistant.get(uni+'/students',params={'homeClassId':hc['id'],'versionId':version}),403)
check(lecturer.get(uni+'/students'),403)
sid={s['studentCode']:s['id'] for s in roster}
linked=check(admin.put(f"{uni}/students/{sid[codes[0]]}",json={'fullName':'Nguyễn An','cohortCode':'K66','status':'studying','homeClassId':hc['id'],'userId':student_account}))
assert linked['userId']==student_account
check(admin.put(f"{uni}/students/{sid[codes[1]]}",json={'fullName':'Trần Bình','cohortCode':'K66','status':'studying','homeClassId':hc['id'],'userId':student_account}),409)
check(admin.put(f"{uni}/students/{sid[codes[1]]}",json={'fullName':'Trần Bình','cohortCode':'K66','status':'studying','homeClassId':hc['id'],'userId':lecturer_id}),400)

# ---- Cơ cấu điểm theo đề cương + liên kết CLO ----
spath=f'{prefix}/courses/{course_id}/assessment-scheme'
scheme=check(admin.get(spath)); assert scheme['cloCodes']==sorted(clos) and scheme['courseId']==course_id
rev=scheme['revision']
qt={'code':'QT','name':'Quá trình','weight':40,'maxScore':10,'kind':'process','clos':{clos[0]:100}}
thi={'code':'THI','name':'Thi cuối kỳ','weight':60,'maxScore':100,'kind':'final','clos':{c:50 for c in clos}}
check(admin.put(spath,json={'revision':rev,'components':[{**thi,'weight':100}]}),400)                 # < 2 đầu điểm
check(admin.put(spath,json={'revision':rev,'components':[{**qt,'weight':60},{**thi,'weight':40}]}),400)  # cuối kỳ < 50%
check(admin.put(spath,json={'revision':rev,'components':[qt,{**thi,'clos':{'CLO-KHONG-CO':10}}]}),400)
check(lecturer.put(spath,json={'revision':rev,'components':[qt,thi]}),403)
saved=check(admin.put(spath,json={'revision':rev,'components':[qt,thi]})); assert saved['components'][1]['kind']=='final' and saved['components'][0]['clos']=={clos[0]:100}
check(admin.put(spath,json={'revision':rev,'components':[qt,thi]}),409)
check(lecturer.get(f"{prefix}/courses/{other['id']}/assessment-scheme"),403)   # chưa được phân công học phần này
coverage={c['courseId']:c for c in check(admin.get(prefix+'/assessment-coverage'))}
assert coverage[course_id]['componentCount']==2 and coverage[course_id]['unassessedClos']==[] and coverage[course_id]['unmappedComponents']==[]
check(student.get(prefix+'/assessment-coverage'),403)
template=check(admin.post(prefix+'/assessment-scheme/apply-template',json={'sourceCourseId':course_id}))
assert template['updatedCourses']+template['skippedCourses']==len(courses)-1

# ---- Lớp học phần: học phần × học kỳ, nhiều giảng viên, sĩ số, lớp sinh hoạt ----
P1,P2='2025-2026-HK1','2025-2026-HK2'
open_body={'programCourseId':course_id,'academicPeriod':P1,'capacity':3,'targetHomeClassId':hc['id'],'lecturers':[{'userId':lecturer_id,'role':'main'},{'userId':assistant_id,'role':'assistant'}]}
check(admin.post(prefix+'/offerings',json={**open_body,'lecturers':[{'userId':lecturer_id,'role':'main'},{'userId':assistant_id,'role':'main'}]}),400)
check(admin.post(prefix+'/offerings',json={**open_body,'lecturers':[{'userId':student_account,'role':'main'}]}),400)
offering=check(admin.post(prefix+'/offerings',json=open_body))
second=check(admin.post(prefix+'/offerings',json={'programCourseId':course_id,'academicPeriod':P1}))
assert offering['groupName']!=second['groupName'] and len(offering['code'])==6 and offering['code'].isdigit() and offering['gradeStatus']=='draft'
assert {l['userId']:l['role'] for l in offering['lecturers']}=={lecturer_id:'main',assistant_id:'assistant'} and offering['targetHomeClassCode']=='CNTT'+tag
check(admin.post(prefix+'/offerings',json={'programCourseId':course_id,'academicPeriod':P1,'groupName':offering['groupName']}),409)
assert check(lecturer.get(spath))['revision']==saved['revision'] and check(assistant.get(spath))   # phân công lớp HP → đọc được đề cương
check(lecturer.post(prefix+'/offerings',json={'programCourseId':course_id,'academicPeriod':P1}),403)
listing=check(admin.get(prefix+'/offerings',params={'period':P1}))['items']; assert {offering['id'],second['id']}<={o['id'] for o in listing}
mine_offerings={o['id'] for o in check(assistant.get(prefix+'/offerings',params={'period':P1}))['items']}; assert offering['id'] in mine_offerings and second['id'] not in mine_offerings
assert offering['id'] in [o['id'] for o in check(lecturer.get(f'{prefix}/courses/{course_id}/offerings'))['items']]
check(lecturer.get(f"{prefix}/offerings/{second['id']}"),403); check(student.get(f"{prefix}/offerings/{offering['id']}"),403)
assert any(s['period']==P1 for b in check(lecturer.get(prefix+'/academic-periods')) for s in b['semesters'])
path=f"{prefix}/offerings/{offering['id']}"
bulk=check(admin.post(path+'/enroll-home-class',json={'homeClassId':hc['id']})); assert bulk['added']==3 and bulk['skipped']==0
assert check(admin.post(path+'/enroll-home-class',json={'homeClassId':hc['id']}))['skipped']==3
check(admin.post(path+'/students',json={'studentCode':codes[3],'fullName':'Phạm Dũng','academicPeriod':P1}),409)            # vượt sĩ số
check(admin.put(path,json={'groupName':offering['groupName'],'capacity':2,'targetHomeClassId':hc['id']}),409)
check(admin.post(f"{prefix}/offerings/{second['id']}/students",json={'studentCode':codes[0],'fullName':'Nguyễn An','academicPeriod':P1}),409)  # cùng HP, cùng kỳ
rows={r['studentCode']:r for r in check(lecturer.get(path+'/students'))['items']}
assert all(r['attemptKind']=='first' and r['homeClassCode']=='CNTT'+tag for r in rows.values())

# ---- Nhập điểm → nộp → công bố → phúc khảo ----
srev=saved['revision']
def grades(client,code,scores,mark=None,reason=None,status=200):
    g=f"{path}/students/{rows[code]['id']}/grades"; cur=check(client.get(g))
    return check(client.put(g,json={'revision':cur['enrollment']['revision'],'schemeRevision':srev,'scores':scores,'specialMark':mark,'reason':reason}),status)
b=grades(assistant,codes[0],{'QT':8,'THI':85}); assert b['enrollment']['totalScore']==8.3 and b['enrollment']['letterGrade']=='B' and b['enrollment']['gradePoint']==3 and not b['enrollment']['published']
grades(lecturer,codes[1],{'QT':9,'THI':95})
grades(lecturer,codes[2],{'QT':2},status=200)
check(lecturer.post(path+'/grades/submit',json={}),409)                       # còn sinh viên chưa đủ điểm
grades(lecturer,codes[2],{'QT':2,'THI':30})                                    # 2.6 → F
check(assistant.post(path+'/grades/submit',json={}),403)
check(lecturer.post(path+'/grades/publish',json={}),403)
assert check(lecturer.post(path+'/grades/submit',json={}))['gradeStatus']=='submitted'
grades(lecturer,codes[0],{'QT':9,'THI':85},status=409)                        # giảng viên không sửa sau khi nộp
check(admin.post(path+'/students',json={'studentCode':codes[3],'fullName':'Phạm Dũng','academicPeriod':P1}),409)
assert check(admin.post(path+'/grades/publish',json={}))['gradeStatus']=='published'
pub={r['studentCode']:r for r in check(admin.get(path+'/students'))['items']}
assert pub[codes[0]]['published'] and pub[codes[0]]['letterGrade']=='B' and pub[codes[1]]['letterGrade']=='A' and pub[codes[2]]['passed'] is False

mine=check(student.get('/api/curriculum/me/transcript'))
assert mine['student']['studentCode']==codes[0] and mine['cumulative']['gpa']==3 and mine['cumulative']['earnedCredits']==course['credits'] and mine['classification']=='Khá'
check(lecturer.get('/api/curriculum/me/transcript'),403)
assert check(lecturer.get(f"{uni}/students/{sid[codes[1]]}/transcript"))['cumulative']['gpa']==4       # cố vấn học tập
check(assistant.get(f"{uni}/students/{sid[codes[1]]}/transcript"),403)
check(student.get(f"{uni}/students/{sid[codes[1]]}/transcript"),403)

grades(admin,codes[0],{'QT':9,'THI':85},status=400)                           # phúc khảo cần lý do
b=grades(admin,codes[0],{'QT':10,'THI':90},reason='Kết quả phúc khảo')        # 4 + 54 = 9.4 → A
assert b['enrollment']['letterGrade']=='A' and b['history'][0]['source']=='regrade' and b['history'][0]['reason']=='Kết quả phúc khảo'
assert check(student.get('/api/curriculum/me/transcript'))['cumulative']['gpa']==4
check(admin.post(path+'/grades/reopen',json={'reason':''}),400)
assert check(admin.post(path+'/grades/reopen',json={'reason':'Khoa yêu cầu rà soát'}))['gradeStatus']=='draft'
assert check(student.get('/api/curriculum/me/transcript'))['semesters']==[]
check(lecturer.post(path+'/grades/submit',json={})); check(admin.post(path+'/grades/publish',json={}))

# ---- Học lại / cải thiện ở học kỳ sau ----
retake=check(admin.post(prefix+'/offerings',json={'programCourseId':course_id,'academicPeriod':P2,'lecturers':[{'userId':lecturer_id,'role':'main'}]}))
rpath=f"{prefix}/offerings/{retake['id']}"
assert check(admin.post(rpath+'/students',json={'studentCode':codes[2],'fullName':'Lê Chi','academicPeriod':P2}))['attemptKind']=='retake'
assert check(admin.post(rpath+'/students',json={'studentCode':codes[0],'fullName':'Nguyễn An','academicPeriod':P2}))['attemptKind']=='improve'
rrows={r['studentCode']:r for r in check(admin.get(rpath+'/students'))['items']}
for code,scores in ((codes[2],{'QT':8,'THI':70}),(codes[0],{'QT':5,'THI':50})):
    g=f"{rpath}/students/{rrows[code]['id']}/grades"; cur=check(admin.get(g))
    check(admin.put(g,json={'revision':cur['enrollment']['revision'],'schemeRevision':srev,'scores':scores}))
check(admin.post(rpath+'/grades/submit',json={})); check(admin.post(rpath+'/grades/publish',json={}))
t=check(student.get('/api/curriculum/me/transcript'))   # cải thiện kém hơn: tích lũy giữ lần cao nhất (A)
assert len(t['semesters'])==2 and t['cumulative']['gpa']==4 and sum(r['countedInCumulative'] for s in t['semesters'] for r in s['rows'])==1
t=check(admin.get(f"{uni}/students/{sid[codes[2]]}/transcript"))   # học lại đạt: F → B
assert t['cumulative']['gpa']==3 and t['cumulative']['earnedCredits']==course['credits']

# ---- Import sinh viên/điểm trong lớp học phần còn đang nhập ----
ipath=f"{prefix}/offerings/{second['id']}"
parsed=check(admin.post(ipath+'/import/read',data={'mode':'students','text':f'MSSV\tHoTen\n{codes[3]}\tPhạm Dũng'}))
payload={'mode':'students','academicPeriod':P1,'offeringId':second['id'],'schemeRevision':srev,'rows':parsed['rows'],'overwriteExisting':False}
preview=check(admin.post(ipath+'/import/preview',json=payload)); payload['rows']=[r['row'] for r in preview['rows']]
assert check(admin.post(ipath+'/import/apply',json=payload))['appliedRows']==1
parsed=check(admin.post(ipath+'/import/read',files={'file':('grades.csv',f'MSSV,QT,THI\n{codes[3]},7,70'.encode(),'text/csv')},data={'mode':'grades'}))
payload={**payload,'mode':'grades','rows':parsed['rows']}
preview=check(admin.post(ipath+'/import/preview',json=payload)); assert preview['canApply']; payload['rows']=[r['row'] for r in preview['rows']]
check(admin.post(ipath+'/import/apply',json=payload))
assert check(admin.get(ipath+'/students'))['items'][0]['totalScore']==7
check(admin.post(path+'/import/preview',json={**payload,'offeringId':offering['id']}),409)                      # sổ đã công bố
tpl=admin.get(ipath+'/import/template?mode=grades'); assert tpl.status_code==200
with zipfile.ZipFile(io.BytesIO(tpl.content)) as z: assert b'THI' in z.read('xl/worksheets/sheet1.xml')

# ---- Cơ cấu đã khóa: chỉ sửa tên và liên kết CLO ----
locked=check(admin.get(spath)); assert locked['locked']
check(admin.put(spath,json={'revision':locked['revision'],'components':[{**qt,'weight':50},{**thi,'weight':50}]}),409)
renamed=check(admin.put(spath,json={'revision':locked['revision'],'components':[{**qt,'name':'Điểm quá trình','clos':{c:100 for c in clos}},thi]}))
assert renamed['components'][0]['name']=='Điểm quá trình'
assert sql(f'SELECT count(*) FROM curriculum.assessment_component_clos m JOIN curriculum.assessment_components c ON c."Id"=m."ComponentId" WHERE c."ProgramCourseId"={course_id}')==str(len(clos)*2)
check(admin.put(f"{prefix}/courses/{course_id}/catalog",json={'departmentId':dept['id'],'countsTowardGpa':False}),409)  # đã có điểm công bố

# ---- Sao chép phiên bản CTĐT: sao chép cơ cấu điểm theo học phần, không sao chép lớp/sinh viên/điểm ----
clone=check(admin.post(f"/api/curriculum/programs/{program['id']}/versions",json={'versionCode':'ACADEMIC-'+uuid.uuid4().hex[:8],'sourceVersionId':version}),201)
cloned=next(c for c in check(admin.get(f"/api/curriculum/program-versions/{clone['id']}/courses?pageSize=100"))['items'] if c['courseId']==course['courseId'])
cs=check(admin.get(f"/api/curriculum/program-versions/{clone['id']}/courses/{cloned['id']}/assessment-scheme"))
assert [c['code'] for c in cs['components']]==['QT','THI'] and not cs['locked'] and cs['components'][1]['kind']=='final'
assert check(admin.get(f"/api/curriculum/program-versions/{clone['id']}/offerings",params={'period':P1}))['totalItems']==0
assert check(admin.get(f"/api/curriculum/program-versions/{clone['id']}/home-classes"))['totalItems']==0
assert httpx.get(base+prefix+'/offerings?period='+P1).status_code==401
assert admin.get(prefix+'/classes?period='+P1).status_code==404

folder=Path('tmp/academic-test'); folder.mkdir(parents=True,exist_ok=True)
(folder/'fixture.json').write_text(json.dumps({'versionId':version,'universityId':university,'courseId':course_id,'offeringId':offering['id'],'draftOfferingId':second['id'],
  'homeClassId':hc['id'],'homeClassCode':hc['code'],'studentIds':sid,'period':P1,'tag':tag,'studentEmail':student_email,'courseCode':course['institutionalCode']}),encoding='utf-8')
print('PASS academic records: org units, lecturer profiles, TT08 grading policy, home classes + import, syllabus schemes with CLO links and policy checks, '
      'offerings per course × semester with multiple lecturers/capacity/home-class bulk enrol, submit → publish → regrade → reopen, retake/improve with cumulative GPA, '
      'advisor/student transcript access, imports, locked schemes and version cloning')
