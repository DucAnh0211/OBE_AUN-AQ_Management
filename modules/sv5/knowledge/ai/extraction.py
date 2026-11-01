from io import BytesIO
from pathlib import Path
import time
from zipfile import ZipFile

from defusedxml import ElementTree
import pdfplumber
import pypdfium2 as pdfium
import pytesseract

from . import settings
from .errors import PipelineError

MAX_CHARACTERS = 1_500_000
pytesseract.pytesseract.tesseract_cmd = settings.TESSERACT


def check_deadline(deadline: float) -> None:
    if time.monotonic() >= deadline:
        raise PipelineError("Xử lý vượt thời gian cho phép. Hãy chia nhỏ tài liệu và thử lại.")


def ocr_available() -> bool:
    try:
        languages = pytesseract.get_languages(config="")
        return all(language in languages for language in settings.OCR_LANGUAGES.split("+"))
    except Exception:
        return False


def extract(file_name: str, content: bytes, deadline: float, force_ocr: bool = False) -> tuple[list[dict], str | None]:
    sections: list[dict] = []
    total = 0
    ocr_pages = 0
    blank = 0

    def add(text: str, locator: str) -> None:
        nonlocal total
        check_deadline(deadline)
        text = text.replace("\r\n", "\n").replace("\r", "\n").replace("\x00", "")
        total += len(text)
        if total > MAX_CHARACTERS:
            raise PipelineError("Văn bản vượt 1,5 triệu ký tự. Hãy chia nhỏ tài liệu.")
        if text.strip():
            sections.append({"text": text, "locator": locator})

    try:
        extension = Path(file_name).suffix.lower()
        if extension == ".pdf":
            with pdfplumber.open(BytesIO(content)) as pdf, pdfium.PdfDocument(content) as rendered:
                if len(pdf.pages) > 500:
                    raise PipelineError("PDF vượt giới hạn 500 trang. Hãy chia nhỏ tài liệu.")
                for index, page in enumerate(pdf.pages):
                    check_deadline(deadline)
                    text = page.extract_text() or ""
                    if force_ocr or sum(c.isalnum() for c in text) < 30:
                        if not ocr_available():
                            raise PipelineError("Python service chưa có Tesseract hoặc bộ ngôn ngữ vie+eng. Cài OCR rồi thử lại.")
                        rendered_page = rendered[index]
                        try:
                            width, height = rendered_page.get_size()
                            bitmap = rendered_page.render(scale=min(3, 3000 / max(width, height)))
                            try:
                                image = bitmap.to_pil()
                                try:
                                    text = pytesseract.image_to_string(image, lang=settings.OCR_LANGUAGES,
                                        config="--psm 3", timeout=max(1, min(90, int(deadline - time.monotonic()))))
                                finally:
                                    image.close()
                            finally:
                                bitmap.close()
                        finally:
                            rendered_page.close()
                        ocr_pages += 1
                    if not text.strip():
                        blank += 1
                    add(text, f"Trang {index + 1}")
                    page.close()
        elif extension == ".docx":
            with ZipFile(BytesIO(content)) as archive:
                entry = archive.getinfo("word/document.xml")
                if entry.file_size > 16 * 1024 * 1024:
                    raise PipelineError("Nội dung DOCX giải nén vượt 16 MB.")
                xml = ElementTree.fromstring(archive.read(entry))
                ns = "{http://schemas.openxmlformats.org/wordprocessingml/2006/main}"
                for index, paragraph in enumerate(xml.iter(ns + "p"), 1):
                    text = "".join((node.text or "") if node.tag == ns + "t" else "\t" if node.tag == ns + "tab" else "\n" if node.tag == ns + "br" else "" for node in paragraph.iter())
                    add(text, f"Đoạn {index}")
        elif extension in (".txt", ".md"):
            add(content.decode("utf-8-sig"), "Văn bản")
        else:
            raise PipelineError("Chỉ hỗ trợ PDF, DOCX, TXT và Markdown.")
    except PipelineError:
        raise
    except Exception as error:
        raise PipelineError("Không đọc được tài liệu. Kiểm tra file hỏng, PDF có mật khẩu, mã hóa UTF-8 hoặc cài đặt OCR.") from error
    if not sections:
        raise PipelineError("Không nhận diện được văn bản. Hãy dùng tài liệu hoặc bản scan rõ hơn.")
    warning = f"Đã OCR {ocr_pages} trang; {blank} trang không nhận diện được chữ. Kiểm tra chunks trước khi sử dụng." if ocr_pages else None
    return sections, warning
