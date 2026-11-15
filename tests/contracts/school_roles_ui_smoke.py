"""Real UI against the isolated fixture from school_roles_smoke.py."""
import json
from pathlib import Path
import httpx
from playwright.sync_api import sync_playwright,expect
f=json.loads(Path("tmp/school-test/fixture.json").read_text(encoding="utf-8"))
web="http://127.0.0.1:5198"
api="http://127.0.0.1:5298"
def api_login(email):
    c=httpx.Client(base_url=api);r=c.post("/api/auth/login",json={"email":email,"password":"IsolatedRoles456!"});r.raise_for_status()
    c.headers["Authorization"]="Bearer "+r.json()["accessToken"];return c
pre=api_login(f["schoolAdminEmail"])
for row in pre.get("/api/curriculum/grade-correction-requests?status=pending").json():
    if row["enrollmentId"]==f["enrollmentId"]:
        pre.post(f"/api/curriculum/grade-correction-requests/{row['id']}/decision",json={"approve":False,"reason":"Retry UI test preparation"}).raise_for_status()
existing=pre.get(f"/api/curriculum/program-versions/{f['versionId']}/offerings/{f['offeringId']}/students/{f['enrollmentId']}/grades").json()
new_score="7" if existing["scores"]["A"]!=7 else "6"
def signin(page,email,path):
    page.goto(web+path,wait_until="domcontentloaded")
    page.get_by_label("Email",exact=True).fill(email)
    page.get_by_label("Mật khẩu",exact=True).fill("IsolatedRoles456!")
    page.get_by_role("button",name="Đăng nhập",exact=True).click()
    expect(page.get_by_text("Đang khôi phục phiên đăng nhập…")).to_have_count(0)
with sync_playwright() as p:
    browser=p.chromium.launch(channel="msedge",headless=True)
    errors=[]
    teacher=browser.new_page(viewport={"width":1440,"height":1000});teacher.on("pageerror",lambda e:errors.append(str(e)))
    path=f"/curriculum/versions/{f['versionId']}/offerings/{f['offeringId']}/students/{f['enrollmentId']}/grades"
    signin(teacher,f["teacherEmail"],path)
    expect(teacher.get_by_role("heading",name="Kết quả dự kiến")).to_be_visible(timeout=20000)
    teacher.get_by_label("Điểm A",exact=True).fill(new_score)
    teacher.get_by_label("Điểm B",exact=True).fill(new_score)
    teacher.get_by_label("Lý do sửa điểm (bắt buộc)").fill("UI verified correction request")
    teacher.get_by_role("button",name="Gửi yêu cầu sửa điểm",exact=True).click()
    expect(teacher.get_by_text("Đã gửi yêu cầu sửa điểm.",exact=False)).to_be_visible()
    teacher.goto(web+"/grade-correction-requests")
    expect(teacher.get_by_text("UI verified correction request",exact=False)).to_be_visible()
    expect(teacher.get_by_role("button",name="Phê duyệt",exact=True)).to_have_count(0)
    admin=browser.new_page(viewport={"width":1440,"height":1000});admin.on("pageerror",lambda e:errors.append(str(e)))
    signin(admin,f["schoolAdminEmail"],"/grade-correction-requests")
    expect(admin.get_by_text("UI verified correction request",exact=False)).to_be_visible(timeout=20000)
    admin.get_by_role("button",name="Phê duyệt",exact=True).click()
    expect(admin.get_by_text("Đã phê duyệt và cập nhật điểm cùng lịch sử thay đổi.",exact=True)).to_be_visible()
    teacher.goto(web+path)
    expect(teacher.get_by_label("Điểm A",exact=True)).to_have_value(new_score)
    teacher.screenshot(path="tmp/school-test/lecturer-correction.png",full_page=True)
    admin.goto(web+"/admin/users")
    admin.get_by_role("button",name="Tạo tài khoản",exact=True).click()
    expect(admin.get_by_label("Vai trò",exact=True).locator("option")).to_have_count(2)
    admin.goto(web+f"/curriculum/versions/{f['versionId']}/outcome-dashboard")
    admin.locator("summary").filter(has_text="Quy tắc tính và ngưỡng đánh giá").click()
    expect(admin.get_by_label("Ngưỡng điểm CLO",exact=True)).to_be_enabled()
    expect(admin.get_by_role("button",name="Lưu revision mới",exact=True)).to_be_visible()
    admin.goto(web+"/reporting")
    expect(admin.get_by_text("Mã truy cập",exact=True)).to_have_count(0)
    admin.goto(web+"/admin/knowledge")
    admin.get_by_label("Ai được sử dụng tài liệu",exact=True).select_option("personal")
    expect(admin.get_by_label("Sinh viên sở hữu",exact=True)).to_be_visible()
    expect(admin.get_by_label("Sinh viên sở hữu",exact=True)).to_contain_text(f["studentEmail"])
    student=browser.new_page(viewport={"width":390,"height":844});student.on("pageerror",lambda e:errors.append(str(e)))
    signin(student,f["studentEmail"],"/me/transcript")
    expect(student.get_by_text(f["tag"]+"S1",exact=False).first).to_be_visible(timeout=20000)
    expect(student.get_by_text(f["tag"]+"S2",exact=False)).to_have_count(0)
    student.goto(web+"/knowledge/chat")
    expect(student.get_by_role("link",name="Cuộc trò chuyện mới",exact=True)).to_be_visible(timeout=15000)
    student.screenshot(path="tmp/school-test/student-chat-mobile.png",full_page=True)
    global_admin=browser.new_page(viewport={"width":1440,"height":1000});global_admin.on("pageerror",lambda e:errors.append(str(e)))
    signin(global_admin,"roles-admin@example.test","/admin/users")
    global_admin.get_by_label("Trường đang quản lý",exact=True).select_option(str(f["schoolA"]["id"]))
    expect(global_admin.get_by_label("Trường đang quản lý",exact=True)).to_have_value(str(f["schoolA"]["id"]))
    global_admin.goto(web+"/admin/users")
    global_admin.get_by_role("button",name="Tạo tài khoản",exact=True).click()
    expect(global_admin.get_by_label("Vai trò",exact=True).locator("option")).to_have_count(4)
    global_admin.set_viewport_size({"width":390,"height":844})
    global_admin.screenshot(path="tmp/school-test/global-school-mobile.png",full_page=True)
    for page in [student,global_admin]:
        assert page.evaluate("document.documentElement.scrollWidth<=innerWidth+1"),"horizontal page overflow"
    assert not errors,errors
    browser.close()
print("PASS four-role UI: school selector, role options, lecturer proposal, administrator approval, own transcript, student chat, document audience, OBE policy, mobile layout; no JavaScript errors")
