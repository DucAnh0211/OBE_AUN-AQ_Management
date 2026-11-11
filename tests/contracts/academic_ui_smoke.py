"""UI on a disposable stack only; run academic_smoke.py first (it writes tmp/academic-test/fixture.json).
SMOKE_WEB = web origin (default Vite :5173), SMOKE_CHANNEL = browser channel (default msedge; empty = bundled Chromium)."""
import json, os
from pathlib import Path
from playwright.sync_api import sync_playwright, expect

web=os.environ.get('SMOKE_WEB','http://localhost:5173').rstrip('/')
folder=Path('tmp/academic-test'); f=json.loads((folder/'fixture.json').read_text())
v=f['versionId']; u=f['universityId']; year=f['period'].rsplit('-HK',1)[0]
def login(page, email):
    page.get_by_label('Email',exact=True).fill(email)
    page.get_by_label('Mật khẩu',exact=True).fill('IsolatedAcademic456!')
    page.get_by_role('button',name='Đăng nhập',exact=True).click()
with sync_playwright() as p:
    browser=p.chromium.launch(channel=os.environ.get('SMOKE_CHANNEL','msedge') or None,headless=True)
    page=browser.new_page(viewport={'width':1440,'height':1000}); errors=[]
    page.on('pageerror',lambda e:errors.append(str(e)))
    page.goto(f'{web}/curriculum/versions/{v}')
    login(page,'academic-admin@example.test')
    expect(page.get_by_role('heading',name='Danh sách học phần',exact=True)).to_be_visible(timeout=20000)
    assert page.locator('tbody tr').first.locator('td').nth(1).inner_text().isdigit()
    expect(page.get_by_role('link',name='Lớp sinh hoạt',exact=True)).to_be_visible()

    # Lớp học phần theo học kỳ → mở lớp học phần mới với giảng viên và sĩ số
    page.get_by_role('link',name='Lớp học phần',exact=True).first.click()
    page.get_by_role('combobox',name='Năm học',exact=True).select_option(year)
    page.get_by_role('combobox',name='Học kỳ',exact=True).select_option(f['period'])
    page.get_by_role('button',name='Xem lớp học phần',exact=True).click()
    expect(page.get_by_role('heading',name=f"Lớp học phần · {f['period']}",exact=True)).to_be_visible()
    expect(page.locator('tbody tr').first).to_be_visible()   # bảng đã tải (academic_smoke.py mở sẵn lớp học phần ở học kỳ này)
    before=page.locator('tbody tr').count()
    page.get_by_role('button',name='Mở lớp học phần',exact=True).click()
    dialog=page.get_by_role('dialog')
    dialog.get_by_label('Tìm học phần',exact=True).fill(f['courseCode'])
    select=dialog.get_by_role('combobox',name='Học phần trong CTĐT',exact=True)
    expect(select.locator('option')).to_have_count(2)
    select.select_option(str(f['courseId']))
    dialog.get_by_label('Sĩ số tối đa',exact=True).fill('30')
    dialog.get_by_label('Tìm giảng viên',exact=True).fill('Academic lecturer')
    picker=dialog.get_by_role('combobox',name='Chọn giảng viên',exact=True); expect(picker.locator('option')).to_have_count(2)
    picker.select_option(index=1); dialog.get_by_role('button',name='Thêm giảng viên',exact=True).click()
    expect(dialog.get_by_role('combobox',name='Vai trò của Academic lecturer',exact=True)).to_have_value('main')
    dialog.get_by_role('button',name='Mở lớp học phần',exact=True).click()
    expect(page.get_by_role('status')).to_contain_text('Đã mở lớp học phần')
    expect(page.locator('tbody tr')).to_have_count(before+1)
    assert page.locator('tbody tr').first.locator('td').first.inner_text().isdigit()
    page.screenshot(path=str(folder/'offerings-desktop.png'),full_page=True)

    # Sổ điểm lớp học phần đang nhập: thêm sinh viên, nhập điểm, điểm chữ dự kiến, import, thêm cả lớp sinh hoạt
    page.goto(f"{web}/curriculum/versions/{v}/offerings/{f['draftOfferingId']}/students")
    expect(page.get_by_text('Sổ điểm: Đang nhập điểm')).to_be_visible()
    page.get_by_role('button',name='Thêm sinh viên',exact=True).click()
    dialog=page.get_by_role('dialog')
    dialog.get_by_label('Mã sinh viên',exact=True).fill('UI'+f['tag'])
    dialog.get_by_label('Họ và tên',exact=True).fill('Nguyễn Thị Giao Diện')
    dialog.get_by_role('button',name='Lưu sinh viên',exact=True).click()
    expect(page.get_by_role('link',name='Nguyễn Thị Giao Diện',exact=True)).to_be_visible()
    page.get_by_role('link',name='Nguyễn Thị Giao Diện',exact=True).click()
    page.get_by_label('Điểm QT',exact=True).fill('8.5'); page.get_by_label('Điểm THI',exact=True).fill('75')
    expect(page.get_by_text('Điểm học phần dự kiến')).to_contain_text('7,9')
    expect(page.get_by_text('Điểm học phần dự kiến')).to_contain_text('B')
    page.get_by_role('button',name='Lưu điểm',exact=True).click()
    expect(page.get_by_role('status')).to_contain_text('Đã lưu điểm')
    page.reload(); expect(page.get_by_label('Điểm QT',exact=True)).to_have_value('8.5')
    page.screenshot(path=str(folder/'grade-entry-desktop.png'),full_page=True)
    page.get_by_role('link',name='Danh sách sinh viên',exact=True).click()
    row=page.locator('tbody tr',has_text='Nguyễn Thị Giao Diện'); expect(row).to_contain_text('B'); expect(row).to_contain_text('dự kiến')
    page.get_by_role('button',name='Import sinh viên / điểm',exact=True).click()
    dialog=page.get_by_role('dialog'); dialog.get_by_label('Hoặc dán bảng từ trang tính',exact=True).fill('MSSV\tHoTen\nUJ'+f['tag']+'\tTrần Thị Trang Tính')
    dialog.get_by_role('button',name='Xem trước và kiểm tra',exact=True).click()
    expect(dialog.get_by_role('status')).to_contain_text('Có thể nhập dữ liệu')
    dialog.get_by_role('button',name='Xác nhận import',exact=True).click()
    expect(page.get_by_role('link',name='Trần Thị Trang Tính',exact=True)).to_be_visible()
    page.get_by_role('button',name='Thêm cả lớp sinh hoạt',exact=True).click()
    dialog=page.get_by_role('dialog'); dialog.get_by_role('combobox',name='Lớp sinh hoạt',exact=True).select_option(str(f['homeClassId']))
    dialog.get_by_role('button',name='Thêm sinh viên',exact=True).click()
    expect(page.get_by_role('status')).to_contain_text('Đã thêm')
    page.set_viewport_size({'width':390,'height':844})
    page.screenshot(path=str(folder/'students-mobile.png'),full_page=True)
    assert page.evaluate('document.documentElement.scrollWidth <= innerWidth'), 'Horizontal page overflow'
    page.set_viewport_size({'width':1440,'height':1000})

    # Sổ điểm đã công bố: chỉ admin sửa và phải ghi lý do
    page.goto(f"{web}/curriculum/versions/{v}/offerings/{f['offeringId']}/students")
    expect(page.get_by_text('Sổ điểm: Đã công bố')).to_be_visible()
    expect(page.get_by_role('button',name='Trả lại để sửa',exact=True)).to_be_visible()
    expect(page.get_by_role('button',name='Thêm sinh viên',exact=True)).to_be_disabled()
    page.locator('tbody tr').first.get_by_role('link',name='Xem / nhập điểm').click()
    expect(page.get_by_label('Lý do sửa điểm (bắt buộc)',exact=True)).to_be_visible()
    expect(page.get_by_role('button',name='Lưu điểm',exact=True)).to_be_disabled()

    # Đề cương đánh giá (đã khóa cấu trúc), quy chế, lớp sinh hoạt, bảng điểm, khoa/bộ môn, giảng viên
    page.goto(f"{web}/curriculum/versions/{v}/courses/{f['courseId']}/assessment")
    expect(page.get_by_role('heading',name='Cơ cấu điểm trong đề cương',exact=True)).to_be_visible()
    expect(page.get_by_text('Đã có lịch sử nhập điểm')).to_be_visible()
    expect(page.get_by_label('Trọng số 1',exact=True)).to_be_disabled(); expect(page.get_by_label('Tên đầu điểm 1',exact=True)).to_be_enabled()
    page.screenshot(path=str(folder/'course-assessment-desktop.png'),full_page=True)
    page.goto(f'{web}/curriculum/versions/{v}/assessment-scheme')
    expect(page.get_by_role('heading',name='Cơ cấu điểm theo đề cương học phần',exact=True)).to_be_visible()
    page.goto(f'{web}/curriculum/universities/{u}/grading-policy')
    expect(page.get_by_role('heading',name='Thang điểm chữ và hệ 4',exact=True)).to_be_visible()
    expect(page.get_by_label('Điểm chữ 1',exact=True)).to_have_value('A')
    page.goto(f'{web}/curriculum/versions/{v}/home-classes')
    page.get_by_role('link',name='Danh sách sinh viên').first.click()
    expect(page.get_by_text('Cố vấn học tập',exact=True)).to_be_visible()
    page.goto(f"{web}/curriculum/universities/{u}/students/{f['studentIds'][f['tag']+'1']}/transcript")
    expect(page.get_by_role('heading',name='Bảng điểm sinh viên',exact=True)).to_be_visible()
    expect(page.get_by_text('GPA tích lũy (hệ 4)')).to_be_visible()
    page.screenshot(path=str(folder/'transcript-desktop.png'),full_page=True)
    page.goto(f'{web}/curriculum/universities/{u}/org-units')
    expect(page.get_by_role('heading',name='Khoa và bộ môn',exact=True)).to_be_visible()
    page.goto(f'{web}/admin/lecturers')
    expect(page.get_by_role('heading',name='Giảng viên',exact=True)).to_be_visible()
    expect(page.locator('tbody tr',has_text='Academic lecturer')).to_contain_text('PGS. TS')
    assert not errors,errors

    # Sinh viên tự xem bảng điểm đã công bố
    student=browser.new_page(viewport={'width':390,'height':844})
    student.on('pageerror',lambda e:errors.append(str(e)))
    student.goto(web+'/'); login(student,f['studentEmail'])
    student.get_by_role('link',name='Bảng điểm của tôi').click()
    expect(student.get_by_role('heading',name='Bảng điểm của tôi',exact=True)).to_be_visible(timeout=20000)
    expect(student.get_by_text('Xếp loại')).to_be_visible()
    assert student.evaluate('document.documentElement.scrollWidth <= innerWidth'), 'Transcript overflows on mobile'
    student.screenshot(path=str(folder/'my-transcript-mobile.png'),full_page=True)
    assert not errors,errors
    browser.close()
print('PASS academic UI: offerings by semester with lecturer/capacity, gradebook status, grade entry with letter preview, imports, home-class bulk enrol, published-grade lock, '
      'syllabus assessment lock, grading policy, home classes, transcripts (admin and student), org units, lecturer profiles, responsive and no JavaScript errors')
