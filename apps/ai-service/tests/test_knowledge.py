import app  # Initialize the repository module path for native test execution.
import io
import math
import os
import time
import unittest
from unittest.mock import patch, MagicMock
from zipfile import ZipFile

os.environ["AI_SERVICE_KEY"] = "isolated-unit-service-key-32-characters-only"
os.environ["LLM_API_KEY"] = "isolated-unit-test-key"
from fastapi.testclient import TestClient
from PIL import Image, ImageDraw, ImageFont
from app.main import app
from modules.sv5.knowledge.ai import settings, routes
from modules.sv5.knowledge.ai.chunking import chunk
from modules.sv5.knowledge.ai.extraction import extract, ocr_available
from modules.sv5.knowledge.ai.embeddings import embed
from modules.sv5.knowledge.ai.errors import PipelineError


class KnowledgeTests(unittest.TestCase):
    def test_unicode_chunk_coverage_overlap_and_locators(self):
        text = "Chuẩn đầu ra 🏫 日本語 " * 220
        chunks = chunk([{"text": text, "locator": "Trang 3"}])
        coverage = set()
        for index, item in enumerate(chunks):
            self.assertEqual(item["ordinal"], index + 1)
            self.assertEqual(item["locator"], "Trang 3")
            self.assertLessEqual(len(item["text"]), 1200)
            self.assertEqual(item["text"], text[item["startOffset"]:item["startOffset"] + len(item["text"])])
            coverage.update(range(item["startOffset"], item["startOffset"] + len(item["text"])))
        self.assertEqual(coverage, set(range(len(text))))
        self.assertGreater(len(chunks), 1)

    def test_docx_paragraph_citations(self):
        buffer = io.BytesIO()
        with ZipFile(buffer, "w") as archive:
            archive.writestr("word/document.xml", '<w:document xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main"><w:body><w:p><w:r><w:t>PLO 1</w:t></w:r></w:p><w:p><w:r><w:t>CLO 2</w:t></w:r></w:p></w:body></w:document>')
        sections, warning = extract("test.docx", buffer.getvalue(), time.monotonic() + 30)
        self.assertEqual(sections, [{"text": "PLO 1", "locator": "Đoạn 1"}, {"text": "CLO 2", "locator": "Đoạn 2"}])
        self.assertIsNone(warning)

    def test_invalid_blank_and_bad_utf8(self):
        for name, data in [("x.txt", b" "), ("x.txt", b"\xff"), ("x.pdf", b"not pdf"), ("x.docx", b"not docx"), ("x.exe", b"abc")]:
            with self.subTest(name=name, data=data), self.assertRaises(PipelineError):
                extract(name, data, time.monotonic() + 30)

    def test_utf8_bom(self):
        sections, _ = extract("x.md", "\ufeff# Xin chào".encode(), time.monotonic() + 30)
        self.assertEqual(sections[0]["text"], "# Xin chào")

    def test_embedding_reorders_provider_rows(self):
        response = MagicMock(status_code=200)
        response.json.return_value = {"data": [{"index": 1, "embedding": [0.0, 1.0]}, {"index": 0, "embedding": [1.0, 0.0]}]}
        with patch("modules.sv5.knowledge.ai.embeddings.httpx.Client") as factory:
            factory.return_value.__enter__.return_value.post.return_value = response
            chunks = [{"text": "a"}, {"text": "b"}]
            embed(chunks, settings.MODEL, time.monotonic() + 30)
            self.assertEqual(chunks[0]["embedding"], [1.0, 0.0])

    def test_embedding_rejects_nan_and_wrong_indices(self):
        for rows in [[{"index": 0, "embedding": [math.nan]}], [{"index": 5, "embedding": [1.0]}]]:
            response = MagicMock(status_code=200)
            response.json.return_value = {"data": rows}
            with patch("modules.sv5.knowledge.ai.embeddings.httpx.Client") as factory:
                factory.return_value.__enter__.return_value.post.return_value = response
                with self.assertRaises(PipelineError):
                    embed([{"text": "a"}], settings.MODEL, time.monotonic() + 30)

    def test_embedding_does_not_leak_upstream_error(self):
        with patch("modules.sv5.knowledge.ai.embeddings.httpx.Client") as factory:
            factory.return_value.__enter__.return_value.post.return_value = MagicMock(status_code=401, text="secret provider details")
            with self.assertRaises(PipelineError) as error:
                embed([{"text": "a"}], settings.MODEL, time.monotonic() + 30)
            self.assertNotIn("secret", str(error.exception))

    def test_internal_auth_and_missing_key(self):
        client = TestClient(app)
        self.assertEqual(client.get("/internal/knowledge/configuration").status_code, 401)
        self.assertEqual(client.get("/internal/knowledge/configuration", headers={"X-Service-Key": "wrong"}).status_code, 401)
        with patch.object(settings, "API_KEY", ""):
            response = client.get("/internal/knowledge/configuration", headers={"X-Service-Key": settings.SERVICE_KEY})
            self.assertFalse(response.json()["embeddingConfigured"])

    def test_upload_limit_before_multipart_parser(self):
        response = TestClient(app).post("/internal/knowledge/process", content=b"x",
            headers={"X-Service-Key": settings.SERVICE_KEY, "Content-Length": str(22 * 1024 * 1024)})
        self.assertEqual(response.status_code, 413)

    def test_pipeline_api_processes_text(self):
        def fake_embed(chunks, model, deadline):
            for item in chunks:
                item["embedding"] = [1.0, 0.0, 0.0]
        with patch.object(routes, "embed", fake_embed):
            response = TestClient(app).post("/internal/knowledge/process", headers={"X-Service-Key": settings.SERVICE_KEY},
                data={"model": settings.MODEL}, files={"file": ("x.txt", "Chuẩn đầu ra chương trình".encode())})
        self.assertEqual(response.status_code, 200, response.text)
        self.assertEqual(response.json()["chunks"][0]["embedding"], [1.0, 0.0, 0.0])
        self.assertEqual(response.json()["pipelineVersion"], "python-ocr-v1/1200-150")

    @unittest.skipUnless(ocr_available(), "Tesseract vie+eng not installed on this host")
    def test_real_scan_ocr(self):
        image = Image.new("RGB", (1200, 500), "white")
        draw = ImageDraw.Draw(image)
        fonts = ["/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf", "C:/Windows/Fonts/arial.ttf"]
        font = next((ImageFont.truetype(path, 42) for path in fonts if os.path.exists(path)), ImageFont.load_default(size=42))
        draw.text((60, 100), "Knowledge Base\nLearning outcomes and assessment", fill="black", font=font)
        buffer = io.BytesIO()
        image.save(buffer, format="PDF")
        sections, warning = extract("scan.pdf", buffer.getvalue(), time.monotonic() + 90)
        self.assertIn("Knowledge", sections[0]["text"])
        self.assertEqual(sections[0]["locator"], "Trang 1")
        self.assertIn("OCR 1", warning)


if __name__ == "__main__":
    unittest.main()
