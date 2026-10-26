"""Optional UI smoke: pip install playwright; requires installed Microsoft Edge.

Use an isolated API/database seeded with V1/V2. Same author/reader token env vars
as reporting_api_smoke.py; REPORT_TEST_WEB_URL defaults to localhost:5173.
"""
import os
from pathlib import Path
from playwright.sync_api import sync_playwright, expect

out = Path(os.environ.get('REPORT_TEST_ARTIFACTS', 'tmp/reporting-test/artifacts'))
out.mkdir(parents=True, exist_ok=True)
with sync_playwright() as p:
    browser = p.chromium.launch(channel='msedge', headless=True)
    page = browser.new_page(viewport={'width': 1440, 'height': 1000}, accept_downloads=True)
    errors = []
    page.on('pageerror', lambda error: errors.append(str(error)))
    page.goto(os.environ.get('REPORT_TEST_WEB_URL', 'http://localhost:5173').rstrip('/') + '/reporting')
    page.get_by_label('Mã truy cập').fill(os.environ['REPORT_TEST_AUTHOR'])
    page.get_by_role('button', name='Kết nối', exact=True).click()
    expect(page.get_by_label('Chương trình').locator('option')).to_have_count(2)
    page.get_by_label('Chương trình').select_option('1')
    expect(page.get_by_label('Phiên bản').locator('option')).to_have_count(3)
    page.get_by_label('Phiên bản').select_option('2')
    expect(page.get_by_role('combobox').nth(3).locator('option')).to_have_count(60)
    for kind in ('course', 'clo', 'plo'):
        page.get_by_label('Loại báo cáo').select_option(kind)
        if kind == 'course':
            page.get_by_role('combobox').nth(3).select_option(index=1)
        page.get_by_role('button', name='Tạo và xem báo cáo').click()
        expect(page.locator('.report-preview')).to_be_visible()
        expect(page.locator('.report-preview .eyebrow')).to_contain_text({'course':'UC25','clo':'UC26','plo':'UC27'}[kind])
        for fmt in ('DOCX', 'PDF'):
            with page.expect_download() as event:
                page.get_by_role('button', name='Tải '+fmt, exact=True).click()
            download = event.value
            download.save_as(out / ('ui-'+kind+'.'+fmt.lower()))
            assert (out / ('ui-'+kind+'.'+fmt.lower())).stat().st_size > 1000
        page.locator('.report-preview').screenshot(path=str(out / ('ui-'+kind+'.png')))
    page.get_by_role('button', name='Thu hồi', exact=True).click()
    page.get_by_role('button', name='Hủy', exact=True).click()
    expect(page.get_by_role('button', name='Xác nhận thu hồi')).to_have_count(0)
    if page.get_by_role('button', name='Sau', exact=True).is_enabled():
        page.get_by_role('button', name='Sau', exact=True).click()
        expect(page.locator('.pagination')).to_contain_text('Trang 2/')
        page.get_by_role('button', name='Trước', exact=True).click()
        expect(page.locator('.pagination')).to_contain_text('Trang 1/')
    page.set_viewport_size({'width':390, 'height':844})
    page.screenshot(path=str(out / 'ui-mobile.png'))
    assert page.evaluate('document.documentElement.scrollWidth <= window.innerWidth'), 'Mobile horizontal overflow'
    page.get_by_role('button', name='Ngắt kết nối', exact=False).click()
    page.get_by_label('Mã truy cập').fill(os.environ['REPORT_TEST_READER'])
    page.get_by_role('button', name='Kết nối', exact=True).click()
    expect(page.locator('.report-preview')).to_be_visible()
    expect(page.get_by_role('button', name='Tạo và xem báo cáo')).to_be_disabled()
    expect(page.get_by_role('button', name='Thu hồi', exact=True)).to_have_count(0)
    assert page.evaluate('localStorage.length') == 0
    page.get_by_role('button', name='Ngắt kết nối', exact=False).click()
    page.get_by_label('Mã truy cập').fill(os.environ['REPORT_TEST_AUTHOR'])
    page.get_by_role('button', name='Kết nối', exact=True).click()
    expect(page.locator('.report-preview')).to_be_visible()
    old_url = page.url
    page.get_by_role('button', name='Thu hồi', exact=True).click()
    page.get_by_role('button', name='Xác nhận thu hồi', exact=True).click()
    expect(page.locator('.report-preview')).to_have_count(0)
    expect(page.locator('.report-notice')).to_contain_text('Đã thu hồi')
    page.goto(old_url)
    page.get_by_label('Mã truy cập').fill(os.environ['REPORT_TEST_AUTHOR'])
    page.get_by_role('button', name='Kết nối', exact=True).click()
    expect(page.get_by_role('alert')).to_contain_text('không tồn tại')
    assert not errors, errors
    browser.close()
    print('PASS UI: three reports, six downloads, history, reader permissions, revocation, mobile, no JS errors')
