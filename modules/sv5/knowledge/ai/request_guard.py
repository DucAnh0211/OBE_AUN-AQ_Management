"""Reject unauthorized/oversized internal uploads before multipart parsing."""
import hmac
from starlette.responses import JSONResponse
from . import settings


class UploadTooLarge(Exception):
    pass


class KnowledgeRequestGuard:
    def __init__(self, app):
        self.app = app

    async def __call__(self, scope, receive, send):
        if scope["type"] != "http" or not scope["path"].startswith("/internal/knowledge"):
            return await self.app(scope, receive, send)
        headers = dict(scope["headers"])
        if len(settings.SERVICE_KEY) < 32:
            return await JSONResponse({"detail": "Python service chưa cấu hình AI_SERVICE_KEY."}, status_code=503)(scope, receive, send)
        provided = headers.get(b"x-service-key", b"")
        if not hmac.compare_digest(provided, settings.SERVICE_KEY.encode()):
            return await JSONResponse({"detail": "Service key không hợp lệ."}, status_code=401)(scope, receive, send)
        limit = 21 * 1024 * 1024
        try:
            if int(headers.get(b"content-length", b"0")) > limit:
                raise UploadTooLarge()
        except ValueError:
            return await JSONResponse({"detail": "Content-Length không hợp lệ."}, status_code=400)(scope, receive, send)
        except UploadTooLarge:
            return await JSONResponse({"detail": "File vượt giới hạn 20 MB."}, status_code=413)(scope, receive, send)
        received = 0

        async def bounded_receive():
            nonlocal received
            message = await receive()
            received += len(message.get("body", b""))
            if received > limit:
                raise UploadTooLarge()
            return message

        try:
            await self.app(scope, bounded_receive, send)
        except UploadTooLarge:
            await JSONResponse({"detail": "File vượt giới hạn 20 MB."}, status_code=413)(scope, receive, send)
