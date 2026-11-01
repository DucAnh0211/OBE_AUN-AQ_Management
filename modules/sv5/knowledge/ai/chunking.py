from .errors import PipelineError

SIZE = 1200
OVERLAP = 150
PIPELINE_VERSION = "python-ocr-v1/1200-150"


def chunk(sections: list[dict]) -> list[dict]:
    # <= 4800 UTF-8 bytes/chunk, below the embedding token limit even for non-Latin text.
    result: list[dict] = []
    for section in sections:
        text = section["text"]
        start = 0
        while start < len(text):
            end = min(start + SIZE, len(text))
            if end < len(text):
                space = text.rfind(" ", start, end)
                if space > start + SIZE // 2:
                    end = space
            piece = text[start:end]
            if piece.strip():
                result.append({"ordinal": len(result) + 1, "text": piece, "locator": section["locator"], "startOffset": start})
            if len(result) > 2000:
                raise PipelineError("Tài liệu vượt 2.000 chunks. Hãy chia nhỏ tài liệu.")
            if end == len(text):
                break
            start = max(start + 1, end - OVERLAP)
    if not result:
        raise PipelineError("Không có văn bản để tạo embedding.")
    return result
