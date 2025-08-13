from playwright.sync_api import sync_playwright
import os

URL = os.getenv("CAPTURE_URL", "https://www.tradingview.com/")
OUT = "data/input/images/capture.png"
os.makedirs("data/input/images", exist_ok=True)

with sync_playwright() as p:
    browser = p.chromium.launch(headless=True)
    page = browser.new_page(viewport={"width":1600,"height":1200})
    page.goto(URL, wait_until="networkidle")
    page.screenshot(path=OUT, full_page=True)
    browser.close()
print("[Playwright] saved:", OUT)
