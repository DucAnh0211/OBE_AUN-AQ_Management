"""Four-role acceptance against a disposable API (:5298), PostgreSQL and MinIO.
Creates unique fixtures; never run against the production stack.
"""
import hashlib
import json
import os
import subprocess
import uuid
from pathlib import Path
import httpx

BASE=os.getenv("SMOKE_BASE","http://127.0.0.1:5298")
assert ":5298" in BASE, "Use the isolated test API"
tag=uuid.uuid4().hex[:8].upper()
checks=0
def check(r,status=200):
    global checks
    statuses=status if isinstance(status,tuple) else (status,)
    assert r.status_code in statuses,(r.request.method,str(r.request.url),r.status_code,r.text[:1500])
    checks+=1
    return r.json() if r.content and "json" in r.headers.get("content-type","") else None
def login(email,password="IsolatedRoles123!"):
    c=httpx.Client(base_url=BASE,timeout=40)
    session=check(c.post("/api/auth/login",json={"email":email,"password":password}))
    c.headers["Authorization"]="Bearer "+session["accessToken"]
    if session["user"]["mustChangePassword"]:
        session=check(c.post("/api/auth/change-password",json={"currentPassword":password,"newPassword":"IsolatedRoles456!"}))
        c.headers["Authorization"]="Bearer "+session["accessToken"]
    return c
admin=login("roles-admin@example.test","IsolatedRoles456!")
def create(actor,role,suffix,school=None):
    email=f"{tag}-{suffix}@example.test"
    u=check(actor.post("/api/admin/users",json={"email":email,"fullName":suffix,"role":role,"temporaryPassword":"IsolatedRoles123!","universityId":school}),201)
    return u,login(email)
check(admin.post("/api/admin/users",json={"email":f"{tag}-bad@example.test","fullName":"No school","role":"school_admin","temporaryPassword":"IsolatedRoles123!"}),400)
schools=[check(admin.post("/api/curriculum/universities",json={"code":f"R{tag}{i}","name":f"Role school {tag} {i}"}),201) for i in (1,2)]
a,sa=create(admin,"school_admin","school-a",schools[0]["id"])
b,sb=create(admin,"school_admin","school-b",schools[1]["id"])
teacher,lecturer=create(sa,"lecturer","teacher")
other_teacher,unassigned=create(sa,"lecturer","unassigned-teacher")
student1,s1=create(sa,"student","student-1")
student2,s2=create(sa,"student","student-2")
foreign_student,fs=create(sb,"student","foreign-student")
check(sa.post("/api/admin/users",json={"email":f"{tag}-elevated@example.test","fullName":"Escalation","role":"admin","temporaryPassword":"IsolatedRoles123!"}),403)
check(sa.post("/api/admin/users",json={"email":f"{tag}-oldrole@example.test","fullName":"Old role","role":"faculty_manager","temporaryPassword":"IsolatedRoles123!"}),400)
check(sa.post("/api/curriculum/universities",json={"code":f"ILLEGAL{tag}","name":"Illegal school"}),403)
check(lecturer.get("/api/admin/users"),403)
check(s1.post("/api/admin/users",json={}),403)
check(sa.post(f"/api/admin/users/{b['id']}/reset-password",json={"temporaryPassword":"UnwantedPass123!"}),403)
users=check(sa.get("/api/admin/users?pageSize=100"))["items"]
assert all(u["universityId"]==schools[0]["id"] for u in users)
sa.headers["X-University-Id"]=str(schools[1]["id"])
assert all(u["universityId"]==schools[0]["id"] for u in check(sa.get("/api/admin/users?pageSize=100"))["items"])
sa.headers.pop("X-University-Id")

def program(actor,school):
    p=check(actor.post("/api/curriculum/programs",json={"universityId":school,"code":"IT","name":"Software engineering"}),201)
    v=check(actor.post(f"/api/curriculum/programs/{p['id']}/versions",json={"versionCode":"2026","sourceVersionId":None}),201)
    root=f"/api/curriculum/program-versions/{v['id']}"
    c=check(actor.post(root+"/courses",json={"institutionalCode":"900111","name":"Testing course","credits":3,"semester":"1","displayOrder":1}),201)
    return p,v,c,root
p,v,course,root=program(sa,schools[0]["id"])
pb,vb,cb,rb=program(sb,schools[1]["id"])
check(sa.post("/api/curriculum/programs",json={"universityId":schools[1]["id"],"code":"FORGED","name":"Forged school"}),(403,404))
check(sa.get(rb+"/courses"),(403,404))
check(lecturer.get(rb+"/plos"),(403,404))
check(sb.get(root+"/outcome-policy"),(403,404))
assert [x["id"] for x in check(sa.get("/api/curriculum/programs?pageSize=100"))["items"]]==[p["id"]]
assert [x["id"] for x in check(lecturer.get("/api/curriculum/programs?pageSize=100"))["items"]]==[p["id"]]
uni=f"/api/curriculum/universities/{schools[0]['id']}"
faculty=check(sa.post(uni+"/org-units",json={"kind":"faculty","code":"IT","name":"IT faculty","parentId":None}))
dept=check(sa.post(uni+"/org-units",json={"kind":"department","code":"SE","name":"Software department","parentId":faculty["id"]}))
check(sa.put(f"/api/curriculum/programs/{p['id']}/faculty",json={"orgUnitId":faculty["id"]}),204)
cp=root+f"/courses/{course['id']}"
check(sa.put(f"/api/admin/users/{teacher['id']}/course-assignments",json={"ids":[course["id"]]}),204)
check(lecturer.post(cp+"/clos",json={"code":"FORBIDDEN","statement":"A lecturer may only edit assigned grades","levelCode":None}),403)
check(s1.post(cp+"/clos",json={"code":"FORBIDDEN","statement":"A student has read only access","levelCode":None}),403)
check(sa.put(cp+"/catalog",json={"departmentId":dept["id"],"countsTowardGpa":True}),204)
check(sa.put(f"/api/curriculum/lecturers/{teacher['id']}",json={"lecturerCode":"L001","departmentId":dept["id"],"academicTitle":None,"degree":None}))
plo=check(sa.post(root+"/plos",json={"code":"PLO1","statement":"Apply software engineering knowledge effectively","levelCode":None}),201)
clo=check(sa.post(cp+"/clos",json={"code":"CLO1","statement":"Apply testing knowledge effectively","levelCode":None}),201)
mapping=check(sa.post(cp+"/plo-mappings",json={"ploId":plo["id"],"weightCode":"X","progressionCode":"I","fit":None,"fitReason":None}),201)
check(sa.post(cp+f"/clos/{clo['id']}/plo-mappings",json={"coursePloId":mapping["id"]}),201)
scheme=check(sa.put(cp+"/assessment-scheme",json={"revision":0,"components":[
    {"code":"A","name":"Process","kind":"process","weight":40,"maxScore":10,"clos":{"CLO1":100}},
    {"code":"B","name":"Final","kind":"final","weight":60,"maxScore":10,"clos":{"CLO1":100}}]}))
check(sa.post(root+"/publish"))
check(sa.post(root+"/offerings",json={"programCourseId":course["id"],"academicPeriod":"2026-2027-HK1","groupName":"Invalid","lecturers":[{"userId":other_teacher["id"],"role":"main"}]}),409)
offering=check(sa.post(root+"/offerings",json={"programCourseId":course["id"],"academicPeriod":"2026-2027-HK1","groupName":"N01","lecturers":[{"userId":teacher["id"],"role":"main"}]}))
op=root+f"/offerings/{offering['id']}"
enrollments=[]
check(lecturer.post(op+"/students",json={"studentCode":"FORBIDDEN","fullName":"Forbidden student","academicPeriod":"2026-2027-HK1"}),403)
for i in (1,2):
    e=check(sa.post(op+"/students",json={"studentCode":f"{tag}S{i}","fullName":f"Student {i}","academicPeriod":"2026-2027-HK1"}))
    enrollments.append(e)
    check(lecturer.put(op+f"/students/{e['id']}/grades",json={"revision":e["revision"],"schemeRevision":scheme["revision"],"scores":{"A":8,"B":6},"reason":None,"specialMark":None}))
    check(sa.put(f"/api/admin/users/{[student1,student2][i-1]['id']}/program-assignments",json={"ids":[p["id"]]}),204)
    student=next(x for x in check(sa.get(uni+"/students"))["items"] if x["studentCode"]==f"{tag}S{i}")
    check(sa.put(uni+f"/students/{student['id']}",json={"fullName":student["fullName"],"cohortCode":"K26","status":"studying","homeClassId":None,"userId":[student1,student2][i-1]["id"]}))
check(unassigned.get(op+"/students"),403)
check(sb.get(op+"/students"),(403,404))
check(s1.get(op+"/students"),403)
check(lecturer.post(op+"/grades/submit",json={"reason":None}))
check(lecturer.post(op+"/grades/publish",json={"reason":None}),403)
check(sa.post(op+"/grades/publish",json={"reason":None}))
own=check(s1.get("/api/curriculum/me/transcript"))
assert f"{tag}S2" not in json.dumps(own) and f"{tag}S1" in json.dumps(own)
other=next(x for x in check(sa.get(uni+"/students"))["items"] if x["studentCode"]==f"{tag}S2")
check(s1.get(uni+f"/students/{other['id']}/transcript"),403)
outcome=check(sa.get(root+f"/plo-results?offeringId={offering['id']}"))
assert outcome["status"]=="official" and len(outcome["cloStudents"])==2
own_outcome=check(s1.get(root+"/plo-results"))
assert len(own_outcome["cloStudents"])==1 and own_outcome["cloStudents"][0]["studentCode"]==f"{tag}S1"
snapshot=check(sa.post(root+"/outcome-snapshots",json={"versionId":v["id"],"offeringId":offering["id"],"preview":False}))
check(sb.get(root+f"/outcome-snapshots/{snapshot['id']}/export"),(403,404))
check(sa.get(f"/api/reporting/outcome-source/{v['id']}/{snapshot['id']}"))
check(sb.get(f"/api/reporting/outcome-source/{v['id']}/{snapshot['id']}"),(403,404))
ep=op+f"/students/{enrollments[0]['id']}"
book=check(lecturer.get(ep+"/grades"))
proposal={"revision":book["enrollment"]["revision"],"schemeRevision":scheme["revision"],"scores":{"A":9,"B":8},"specialMark":None,"reason":"Corrected examination marking"}
check(lecturer.put(ep+"/grades",json=proposal),409)
check(sa.put(ep+"/grades",json=proposal),409)
check(sa.post(op+"/grades/reopen",json={"reason":"Bypass published lock"}),409)
check(lecturer.post(ep+"/grade-correction-requests",json={**proposal,"reason":""}),400)
req=check(lecturer.post(ep+"/grade-correction-requests",json=proposal))
assert check(sa.get(ep+"/grades"))["scores"]==book["scores"]
check(lecturer.post(ep+"/grade-correction-requests",json=proposal),409)
decision=f"/api/curriculum/grade-correction-requests/{req['id']}/decision"
check(lecturer.post(decision,json={"approve":True}),403)
check(sb.post(decision,json={"approve":True}),(403,404,409))
check(sa.post(decision,json={"approve":False,"reason":""}),400)
assert check(sa.post(decision,json={"approve":False,"reason":"Need the signed marking record"}))["status"]=="rejected"
assert check(sa.get(ep+"/grades"))["scores"]==book["scores"]
req=check(lecturer.post(ep+"/grade-correction-requests",json=proposal))
decision=f"/api/curriculum/grade-correction-requests/{req['id']}/decision"
assert check(sa.post(decision,json={"approve":True,"reason":"Verified marking record"}))["status"]=="approved"
check(sa.post(decision,json={"approve":True}),409)
updated=check(sa.get(ep+"/grades"))
assert updated["scores"]==proposal["scores"] and updated["enrollment"]["revision"]==book["enrollment"]["revision"]+1
assert updated["history"][0]["reason"]==proposal["reason"]
assert check(sa.get(root+f"/outcome-snapshots/{snapshot['id']}"))["stale"]
check(lecturer.post(ep+"/grade-correction-requests",json=proposal),409)
assert all(r["requestedBy"]==teacher["id"] for r in check(lecturer.get("/api/curriculum/grade-correction-requests")))
assert check(sb.get("/api/curriculum/grade-correction-requests"))==[]
# Two reviewers may race; only one approval may change the grade and its revision.
from concurrent.futures import ThreadPoolExecutor
book=check(lecturer.get(ep+"/grades"))
race=check(lecturer.post(ep+"/grade-correction-requests",json={**proposal,"revision":book["enrollment"]["revision"],"scores":{"A":6,"B":6},"reason":"Concurrent reviewer fixture"}))
decision=f"/api/curriculum/grade-correction-requests/{race['id']}/decision"
with ThreadPoolExecutor(max_workers=2) as pool:
    responses=list(pool.map(lambda _:sa.post(decision,json={"approve":True}),range(2)))
assert sorted(r.status_code for r in responses)==[200,409]
book_after=check(sa.get(ep+"/grades"))
assert book_after["enrollment"]["revision"]==book["enrollment"]["revision"]+1

def upload(actor,title,scope="school",owner=None):
    data={"title":title,"accessScope":scope}
    if owner: data["ownerUserId"]=str(owner)
    return check(actor.post("/api/knowledge/documents",data=data,files={"file":("scope.txt",b"School access fixture.","text/plain")}),201)
public=upload(sa,f"Public {tag}")
private1=upload(sa,f"Private one {tag}","personal",student1["id"])
private2=upload(sa,f"Private two {tag}","personal",student2["id"])
staff=upload(sa,f"Staff {tag}","staff")
foreign=upload(sb,f"Foreign {tag}")
check(sa.post("/api/knowledge/documents",data={"title":"Invalid owner","accessScope":"personal","ownerUserId":foreign_student["id"]},files={"file":("bad.txt",b"bad","text/plain")}),400)
for client,doc,allowed in [(s1,public,True),(s1,private1,True),(s1,private2,False),(s1,staff,False),(s1,foreign,False),(lecturer,private2,True),(sb,public,False)]:
    r=client.get(f"/api/knowledge/chat/sources/{doc['id']}/original",params={"sourceHash":doc["sha256"]})
    check(r,200 if allowed else 404)
    if allowed: assert r.content==b"School access fixture."
check(sb.get(f"/api/knowledge/documents/{public['id']}"),404)
check(sb.post(f"/api/knowledge/documents/{public['id']}/disable"),404)
thread=check(s1.post("/api/knowledge/chat/threads"),201)
check(s2.get(f"/api/knowledge/chat/threads/{thread['id']}"),404)
check(fs.get(f"/api/knowledge/chat/threads/{thread['id']}"),404)
# Forge only a queue lease in the isolated database to exercise internal tools without paying an LLM.
run=uuid.uuid4();lease=uuid.uuid4()
sql=f'''INSERT INTO knowledge.chat_exchanges ("Id","ThreadId","Question","Status","Phase","Attempts","Lease","LeaseUntil","CreatedAt",school_id)
VALUES ('{run}','{thread['id']}','Own transcript','processing','route',1,'{lease}',now()+interval '10 minutes',now(),{schools[0]['id']});'''
result=subprocess.run(["docker","exec","obe-shared-test-db","psql","-U","postgres","-d","obe_roles_test","-v","ON_ERROR_STOP=1","-c",sql],capture_output=True,text=True)
assert result.returncode==0,result.stderr
internal=f"/api/knowledge/internal/agent/{thread['id']}/{run}"
service=httpx.Client(base_url=BASE,headers={"X-Service-Key":"IsolatedSchoolAgentServiceKey123456789","X-Run-Lease":str(lease)},timeout=30)
personal=check(service.post(internal+"/curriculum",json={"tool":"my_transcript"}))
assert f"{tag}S1" in json.dumps(personal) and f"{tag}S2" not in json.dumps(personal)
hidden=check(service.post(internal+"/retrieve",json={"question":"Read another student document","queries":[f"Private two {tag}"]}))
assert hidden["documentCount"]==0
check(service.post(internal+"/curriculum",json={"tool":"plos","versionId":vb["id"]}),403)
check(s1.post(internal+"/curriculum",json={"tool":"my_transcript"}),401)
# A staff-to-student role change must not expose old staff conversations as student context.
old_staff_thread=check(unassigned.post("/api/knowledge/chat/threads"),201)
check(sa.put(f"/api/admin/users/{other_teacher['id']}",json={"fullName":"Former lecturer","role":"student","status":"active"}))
check(unassigned.get("/api/knowledge/chat/threads"),401)
changed=login(other_teacher["email"],"IsolatedRoles456!")
check(changed.get(f"/api/knowledge/chat/threads/{old_staff_thread['id']}"),404)
assert check(changed.get("/api/knowledge/chat/threads"))==[]
fixture={"tag":tag,"schoolA":schools[0],"schoolB":schools[1],"schoolAdminEmail":a["email"],"teacherEmail":teacher["email"],"studentEmail":student1["email"],"versionId":v["id"],"offeringId":offering["id"],"enrollmentId":enrollments[0]["id"],"requestId":req["id"]}
Path("tmp/school-test").mkdir(parents=True,exist_ok=True)
Path("tmp/school-test/fixture.json").write_text(json.dumps(fixture),encoding="utf-8")
print(f"PASS four roles: {checks} HTTP checks; two-school isolation, school/teaching assignments, personal grades, grade correction approval/rejection, snapshots, reporting, documents and owner-bound chatbot tools")

