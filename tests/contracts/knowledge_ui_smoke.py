"""Browser smoke test for a disposable stack. Requires Playwright and KB_TEST_* env."""
import os
from pathlib import Path
from playwright.sync_api import sync_playwright, expect

web = os.environ.get("KB_TEST_WEB", "http://localhost:5173")
artifacts = Path("tmp/knowledge-ui")
artifacts.mkdir(parents=True, exist_ok=True)
with sync_playwright() as p:
    browser = p.chromium.launch(channel="msedge", headless=True)
    page = browser.new_page(viewport={"width": 1440, "height": 1000})
    errors = []
    page.on("pageerror", lambda e: errors.append(str(e)))
    page.goto(web + "/admin/knowledge")
    page.get_by_label("Email", exact=True).fill(os.environ["KB_TEST_EMAIL"])
    page.get_by_label("Mật khẩu", exact=True).fill(os.environ["KB_TEST_PASSWORD"])
    page.get_by_role("button", name="Đăng nhập", exact=True).click()
    expect(page.get_by_role("heading", name="Knowledge Base", exact=True)).to_be_visible()
    page.get_by_label("Tên tài liệu", exact=True).fill("UI Knowledge Test")
    page.locator('input[type="file"]').set_input_files({"name": "ui-knowledge.txt", "mimeType": "text/plain", "buffer": ("Chuẩn đầu ra chương trình và học phần. " * 60).encode()})
    page.get_by_role("button", name="Tải tài liệu lên", exact=True).click()
    expect(page.get_by_text("Đã lưu bản gốc và xếp hàng xử lý. Bạn có thể rời trang; hệ thống tiếp tục chạy nền.", exact=True)).to_be_visible(timeout=30000)
    row = page.get_by_role("row").filter(has_text="UI Knowledge Test").first
    expect(row.get_by_text("Hoàn tất", exact=True)).to_be_visible(timeout=45000)
    row.get_by_role("button", name="Xem chunks", exact=True).click()
    dialog = page.get_by_role("dialog")
    expect(dialog.get_by_text("Chunk 1 · Văn bản", exact=True)).to_be_visible()
    page.screenshot(path=str(artifacts / "chunks.png"), full_page=True)
    page.keyboard.press("Escape")
    with page.expect_download() as download:
        row.get_by_role("button", name="Tải bản gốc", exact=True).click()
    assert download.value.suggested_filename == "ui-knowledge.txt"
    page.screenshot(path=str(artifacts / "desktop.png"), full_page=True)
    page.set_viewport_size({"width": 390, "height": 844})
    page.screenshot(path=str(artifacts / "mobile.png"), full_page=True)
    assert page.evaluate("document.documentElement.scrollWidth <= window.innerWidth"), "Page overflow"
    page.set_viewport_size({"width": 1440, "height": 1000})
    row.get_by_role("button", name="Vô hiệu hóa", exact=True).click()
    page.get_by_role("dialog").get_by_role("button", name="Vô hiệu hóa", exact=True).click()
    expect(row.get_by_text("Đã vô hiệu hóa", exact=True)).to_be_visible()
    assert not errors, errors
    browser.close()
print("PASS UI: upload, auto-processing, chunk preview, original download, disable, mobile layout, no JS errors")
