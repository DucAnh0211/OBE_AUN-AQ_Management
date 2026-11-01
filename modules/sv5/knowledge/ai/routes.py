import hmac
import threading
import time
from typing import Annotated
from fastapi import APIRouter, Depends, File, Form, Header, HTTPException, UploadFile
from . import settings
from .chunking import chunk, PIPELINE_VERSION
from .embeddings import embed
from .errors import PipelineError
from .extraction import extract, ocr_available

MAX_BYTES = 20 * 1024 * 1024
processing = threading.BoundedSemaphore(1)


def authorize(x_service_key: Annotated[str | None, Header()] = None) -> None:
    if len(settings.SERVICE_KEY) < 32:
        raise HTTPException(503, "Python service chưa cấu hình AI_SERVICE_KEY (ít nhất 32 ký tự).")
    if not x_service_key or not hmac.compare_digest(x_service_key, settings.SERVICE_KEY):
        raise HTTPException(401, "Service key không hợp lệ.")


router = APIRouter(prefix="/internal/knowledge", dependencies=[Depends(authorize)])


@router.get("/configuration")
def configuration() -> dict:
    return {"embeddingConfigured": bool(settings.API_KEY), "embeddingModel": settings.MODEL,
        "ocrAvailable": ocr_available(), "maxFileMb": 20,
        "error": None if settings.API_KEY else "Chưa cấu hình LLM_API_KEY trên Python service."}


@router.post("/process")
def process(file: Annotated[UploadFile, File()], model: Annotated[str, Form()], force_ocr: Annotated[bool, Form()] = False) -> dict:
    if model != settings.MODEL:
        raise HTTPException(409, "Model đã thay đổi. Hãy thử lại để dùng cấu hình mới.")
    if not settings.API_KEY:
        raise HTTPException(503, "Chưa cấu hình LLM_API_KEY trên Python service.")
    if not processing.acquire(blocking=False):
        raise HTTPException(503, "Python đang xử lý một tài liệu khác. Hãy thử lại sau.")
    try:
        content = file.file.read(MAX_BYTES + 1)
        if not 0 < len(content) <= MAX_BYTES:
            raise HTTPException(413, "File phải có dữ liệu và tối đa 20 MB.")
        deadline = time.monotonic() + 540
        sections, warning = extract(file.filename or "", content, deadline, force_ocr)
        chunks = chunk(sections)
        embed(chunks, model, deadline)
        return {"model": model, "pipelineVersion": PIPELINE_VERSION, "chunks": chunks, "warning": warning}
    except PipelineError as error:
        raise HTTPException(422, str(error)) from None
    finally:
        file.file.close()
        processing.release()
