"""UI verification against the disposable agent stack; requires knowledge_agent_smoke.py."""
import json
from pathlib import Path
from playwright.sync_api import sync_playwright, expect

folder = Path("tmp/agent-ui"); fixture = json.loads((folder / "fixture.json").read_text())
with sync_playwright() as p:
    browser = p.chromium.launch(channel="msedge", headless=True)
    context = browser.new_context(viewport={"width": 1600, "height": 1000})
    page = context.new_page(); errors = []
    page.on("pageerror", lambda e: errors.append(str(e)))
    page.goto("http://localhost:5173/knowledge/chat?thread=" + fixture["threadId"])
    page.get_by_label("Email", exact=True).fill("agent-lecturer@example.test")
    page.get_by_label("Mật khẩu", exact=True).fill("IsolatedAgent456!")
    page.get_by_role("button", name="Đăng nhập", exact=True).click()
    expect(page.locator(".assistant-message").first).to_contain_text("Chào bạn!", timeout=20000)
    expect(page.locator(".conversation-content")).to_contain_text("Minh")
    page.get_by_role("button", name="Thu gọn thanh điều hướng", exact=True).click()
    expect(page.locator("#main-navigation-sidebar")).to_be_hidden()
    page.reload()
    expect(page.locator("#main-navigation-sidebar")).to_be_hidden()
    expect(page.locator(".conversation-content")).to_contain_text("Minh")
    page.get_by_role("button", name="Mở thanh điều hướng", exact=True).click()
    expect(page.locator("#main-navigation-sidebar")).to_be_visible()
    page.screenshot(path=str(folder / "desktop-expanded.png"), full_page=True)
    page.get_by_role("button", name="Thu gọn thanh điều hướng", exact=True).click()
    page.screenshot(path=str(folder / "desktop-collapsed.png"), full_page=True)
    page.locator(".answer-sources summary").first.click()
    with page.expect_popup() as popup:
        page.locator(".source-reference a").first.click()
    source = popup.value
    expect(source.get_by_role("heading", name="Quy chế tốt nghiệp", exact=True)).to_be_visible(timeout=20000)
    expect(source.locator("mark")).to_contain_text("150")
    source.close()
    page.get_by_role("button", name="Cuộc trò chuyện mới", exact=True).first.click()
    expect(page.locator(".chat-welcome")).to_be_visible()
    page.screenshot(path=str(folder / "welcome-desktop.png"), full_page=True)
    field = page.get_by_label("Nhắn tin cho Trợ lý OBE")
    field.fill("Chào bạn")
    field.press("Enter")
    expect(page.locator(".assistant-message")).to_contain_text("👋", timeout=20000)
    assert page.locator(".answer-sources").count() == 0
    page.reload(); expect(page.locator(".assistant-message")).to_contain_text("👋", timeout=20000)
    page.set_viewport_size({"width": 390, "height": 844})
    page.screenshot(path=str(folder / "mobile-conversation.png"), full_page=True)
    assert page.evaluate("document.documentElement.scrollWidth <= innerWidth"), "Horizontal overflow"
    assert page.get_by_role("button", name="Mở thanh điều hướng").is_visible()
    page.get_by_role("button", name="Mở lịch sử trò chuyện").click()
    expect(page.locator(".conversation-sidebar")).to_be_visible()
    page.locator(".conversation-row.is-current .conversation-delete").click()
    page.get_by_role("dialog").get_by_role("button", name="Xóa cuộc trò chuyện", exact=True).click()
    expect(page.locator(".chat-welcome")).to_be_visible()
    page.get_by_role("button", name="Đóng lịch sử", exact=True).last.click()
    page.get_by_role("button", name="Mở thanh điều hướng", exact=True).click()
    expect(page.locator("#main-navigation-sidebar")).to_be_visible()
    page.get_by_role("button", name="Thu gọn thanh điều hướng", exact=True).click()
    page.goto("http://localhost:5173/")
    expect(page.get_by_role("button", name="Mở thanh điều hướng", exact=True)).to_be_visible()
    assert not errors, errors
    browser.close()
print("PASS UI: natural greeting, stored threads/reload, create/delete, source popup, global sidebar collapse/reopen/persistence, mobile drawer and composer, no JS errors")
