"""Requires knowledge_chat_smoke.py fixtures and isolated Vite/API; never production."""
import json
from pathlib import Path
import httpx
from playwright.sync_api import sync_playwright, expect

folder = Path("tmp/chat-ui"); fixture = json.loads((folder / "fixture.json").read_text())
with sync_playwright() as p:
    browser = p.chromium.launch(channel="msedge", headless=True)
    context = browser.new_context(viewport={"width": 1440, "height": 1000})
    page = context.new_page(); errors = []
    page.on("pageerror", lambda e: errors.append(str(e)))
    page.goto("http://localhost:5173/knowledge/chat")
    page.get_by_label("Email", exact=True).fill("chat-lecturer@example.test")
    page.get_by_label("Mật khẩu", exact=True).fill("IsolatedChat456!")
    page.get_by_role("button", name="Đăng nhập", exact=True).click()
    expect(page.get_by_role("heading", name="Chatbot", exact=True)).to_be_visible()
    assert page.locator("select").count() == 0
    page.get_by_label("Câu hỏi của bạn").fill("Quy chế tốt nghiệp yêu cầu bao nhiêu tín chỉ?")
    page.get_by_role("button", name="Gửi câu hỏi").click()
    expect(page.locator(".chat-answer")).to_contain_text("150 tín chỉ", timeout=30000)
    page.screenshot(path=str(folder / "chat-desktop.png"), full_page=True)
    with page.expect_popup() as popup:
        page.get_by_role("link").filter(has_text="Quy chế tốt nghiệp PDF").click()
    source = popup.value
    source.on("pageerror", lambda e: errors.append(str(e)))
    expect(source.get_by_role("heading", name="Quy chế tốt nghiệp PDF", exact=True)).to_be_visible(timeout=15000)
    expect(source.get_by_role("img", name="Bản gốc trang 2")).to_have_attribute("data-rendered", "true", timeout=20000)
    expect(source.locator("mark")).to_contain_text("150")
    source.screenshot(path=str(folder / "source-before-download.png"), full_page=True)
    with source.expect_response(lambda r: "/original?" in r.url) as original:
        with source.expect_download(timeout=10000) as download:
            source.get_by_role("button", name="Tải bản gốc").click()
    assert download.value.suggested_filename == "graduation.pdf"
    source.screenshot(path=str(folder / "source-pdf.png"), full_page=True)
    page.set_viewport_size({"width": 390, "height": 844})
    page.screenshot(path=str(folder / "chat-mobile.png"), full_page=True)
    assert page.evaluate("document.documentElement.scrollWidth <= innerWidth")
    # Source invalidation must remove an already-open viewer at its next recheck.
    admin = httpx.Client(base_url="http://localhost:5299")
    login = admin.post("/api/auth/login", json={"email": "chat-admin@example.test", "password": "IsolatedChat456!"})
    assert login.status_code == 200
    admin.headers["Authorization"] = "Bearer " + login.json()["accessToken"]
    citation = next(c for c in fixture["citations"] if c["fileName"] == "graduation.pdf")
    assert admin.post(f"/api/knowledge/documents/{citation['documentId']}/disable").status_code == 204
    expect(source.get_by_role("alert")).to_contain_text("vô hiệu hóa", timeout=25000)
    assert source.locator("canvas").count() == 0
    url = citation["url"].replace("/knowledge/sources", "/api/knowledge/chat/sources")
    assert admin.get(url).status_code == 410
    assert admin.get(f"/api/knowledge/chat/sources/{citation['documentId']}/original", params={"sourceHash": citation["sourceHash"]}).status_code == 410
    assert not errors, errors
    browser.close()
print("PASS UI: natural chat only, citation popup, PDF page 2, exact excerpt, download, mobile layout, source revocation, no JS errors")
