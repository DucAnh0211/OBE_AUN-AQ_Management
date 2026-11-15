"""Disposable API :5298/web :5198. Run outcome_smoke.py first.
Use PYTHONPATH=tmp/shared-test-python if Playwright was installed there.
"""
import json,os
from pathlib import Path
from playwright.sync_api import sync_playwright,expect

fixture=json.loads(Path('tmp/outcome-test/fixture.json').read_text(encoding='utf-8'))
web=os.environ.get('SMOKE_WEB','http://127.0.0.1:5198')
v=fixture['versionId'];o=fixture['offeringId']
print('UI: starting browser',flush=True)
with sync_playwright() as p:
    browser=p.chromium.launch(channel=os.environ.get('SMOKE_CHANNEL','msedge'),headless=True)
    page=browser.new_page(viewport={'width':1440,'height':1000});errors=[]
    page.on('pageerror',lambda e:errors.append(str(e)))
    print('UI: navigating',flush=True)
    page.goto(f'{web}/curriculum/versions/{v}/outcome-dashboard',wait_until='domcontentloaded',timeout=30000)
    print('UI: logging in',flush=True)
    page.get_by_label('Email',exact=True).fill('academic-admin@example.test')
    page.get_by_label('Mật khẩu',exact=True).fill('IsolatedAcademic456!')
    page.get_by_role('button',name='Đăng nhập',exact=True).click()
    expect(page.get_by_role('heading',name='Kết quả chuẩn đầu ra',exact=True)).to_be_visible(timeout=20000)
    expect(page.get_by_label('Lớp học phần',exact=True).locator('option')).not_to_have_count(1)
    expect(page.get_by_role('combobox',name='Học kỳ',exact=True)).to_be_visible()
    expect(page.get_by_role('combobox',name='Khóa',exact=True)).to_be_visible()
    page.get_by_label('Học kỳ',exact=True).select_option('2026-2027-HK1')
    page.get_by_label('Khóa',exact=True).select_option('K1')
    with page.expect_response(lambda r:'/outcome-summary?' in r.url and 'cohort=K1' in r.url and 'period=2026-2027-HK1' in r.url) as filtered:
        page.get_by_role('button',name='Tải kết quả',exact=True).click()
    assert filtered.value.status==200
    expect(page.get_by_role('heading',name='CLO',exact=True)).to_be_visible()
    page.get_by_label('Khóa',exact=True).select_option('')
    page.get_by_label('Lớp học phần',exact=True).select_option(str(o))
    page.get_by_role('button',name='Tải kết quả',exact=True).click()
    expect(page.get_by_role('heading',name='CLO',exact=True)).to_be_visible()
    expect(page.get_by_role('heading',name='PLO',exact=True)).to_be_visible()
    expect(page.get_by_text('Chính thức ·',exact=False)).to_be_visible()
    page.get_by_label('Học kỳ đối chiếu',exact=True).select_option('2026-2027-HK2')
    page.get_by_label('Học kỳ',exact=True).select_option('2026-2027-HK2')
    expect(page.get_by_label('Học kỳ đối chiếu',exact=True)).to_have_value('')
    page.get_by_label('Học kỳ',exact=True).select_option('2026-2027-HK1')
    page.get_by_role('button',name='Tạo snapshot theo phạm vi đang hiển thị',exact=True).click()
    expect(page.get_by_role('button',name='Tải JSON bàn giao',exact=True)).to_be_visible()
    with page.expect_download() as download:
        page.get_by_role('button',name='Tải JSON bàn giao',exact=True).click()
    assert download.value.suggested_filename.endswith('.json')
    Path('tmp/outcome-test').mkdir(exist_ok=True)
    page.screenshot(path='tmp/outcome-test/dashboard-desktop.png',full_page=True)
    page.set_viewport_size({'width':390,'height':844})
    page.screenshot(path='tmp/outcome-test/dashboard-mobile.png',full_page=True)
    assert page.evaluate('document.documentElement.scrollWidth <= window.innerWidth+1'),'body overflows on mobile'
    page.set_viewport_size({'width':1440,'height':1000})
    page.goto(f"{web}/curriculum/versions/{v}/offerings/{fixture['draftOfferingId']}/grade-grid")
    expect(page.get_by_role('heading',name='Bảng nhập điểm lớp học phần')).to_be_visible()
    field=page.get_by_role('spinbutton',name=fixture['tag']+'S1 A',exact=True)
    expect(field).to_be_enabled();field.fill('8')
    page.get_by_role('spinbutton',name=fixture['tag']+'S1 B',exact=True).fill('7')
    page.get_by_role('button',name='Lưu các dòng đã sửa',exact=True).click()
    expect(page.get_by_text('Đã lưu 1 sinh viên.',exact=True)).to_be_visible()
    page.get_by_role('button',name='Tải lại',exact=True).click();expect(field).to_have_value('8')
    page.screenshot(path='tmp/outcome-test/grade-grid.png',full_page=True)
    assert not errors,errors
    browser.close()
print('PASS outcome UI: real login/API, filters, CLO/PLO dashboard, snapshot JSON download, mobile overflow, grade grid persistence, no JS errors')
