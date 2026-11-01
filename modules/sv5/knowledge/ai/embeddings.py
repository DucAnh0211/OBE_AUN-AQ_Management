import math
import time
import httpx
from . import settings
from .errors import PipelineError
from .extraction import check_deadline


def embed(chunks: list[dict], model: str, deadline: float) -> None:
    if not settings.API_KEY:
        raise PipelineError("Chưa cấu hình LLM_API_KEY trên Python service.")
    dimension = None
    with httpx.Client() as client:
        for offset in range(0, len(chunks), 32):
            check_deadline(deadline)
            batch = chunks[offset:offset + 32]
            try:
                response = client.post("https://api.openai.com/v1/embeddings",
                    headers={"Authorization": f"Bearer {settings.API_KEY}"},
                    json={"model": model, "input": [c["text"] for c in batch], "encoding_format": "float"},
                    timeout=max(1, min(90, deadline - time.monotonic())))
                if response.status_code != 200:
                    raise PipelineError(f"OpenAI embedding trả HTTP {response.status_code}. Kiểm tra API key, quyền model hoặc hạn mức rồi thử lại.")
                rows = sorted(response.json()["data"], key=lambda row: row["index"])
                if len(rows) != len(batch) or [row["index"] for row in rows] != list(range(len(batch))):
                    raise PipelineError("Số lượng/thứ tự embedding không khớp đầu vào.")
                for item, row in zip(batch, rows):
                    vector = row["embedding"]
                    if dimension is None:
                        dimension = len(vector)
                    if not 0 < len(vector) <= 4096 or len(vector) != dimension or not all(isinstance(v, (int, float)) and math.isfinite(v) for v in vector):
                        raise PipelineError("Embedding trả về không hợp lệ.")
                    item["embedding"] = vector
            except PipelineError:
                raise
            except Exception as error:
                raise PipelineError("Không kết nối được OpenAI hoặc phản hồi không hợp lệ. Hãy thử lại.") from error
