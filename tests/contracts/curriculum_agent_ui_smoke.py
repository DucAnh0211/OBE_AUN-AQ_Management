"""Isolated stack only; requires tmp/curriculum-agent/worker-thread.json from the live probe."""
import json
from pathlib import Path
from playwright.sync_api import sync_playwright, expect

fixture=json.loads(Path('tmp/curriculum-agent/worker-thread.json').read_text(encoding='utf-8'))
with sync_playwright() as p:
    browser=p.chromium.launch(channel='msedge',headless=True)
    page=browser.new_page(viewport={'width':1440,'height':1000})
    errors=[]; page.on('pageerror',lambda e:errors.append(str(e)))
    page.goto('http://localhost:5173/knowledge/chat?thread='+fixture['threadId'])
    page.get_by_label('Email',exact=True).fill('agent-admin@example.test')
    page.get_by_label('Mật khẩu',exact=True).fill('IsolatedAgent456!')
    page.get_by_role('button',name='Đăng nhập',exact=True).click()
    expect(page.locator('.assistant-message')).to_contain_text('PLO1',timeout=20000)
    page.locator('.answer-sources summary').click()
    expect(page.locator('.source-reference a').last).to_be_visible()
    page.screenshot(path='tmp/curriculum-agent/database-sources.png',full_page=True)
    with page.expect_popup() as popup:page.locator('.source-reference a[href$="/plos"]').click()
    source=popup.value
    expect(source.get_by_text('PLO1',exact=True).first).to_be_visible(timeout=15000)
    assert not errors,errors
    browser.close()
print('PASS UI: stored database answer, source references and authorized PLO source page')
