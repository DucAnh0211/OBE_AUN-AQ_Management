import os
from pathlib import Path
from dotenv import load_dotenv

# Local .env is optional; process environment wins in deployments.
for parent in Path(__file__).resolve().parents:
    if (parent / "compose.yaml").exists():
        load_dotenv(parent / ".env", override=False)
        break
SERVICE_KEY = os.getenv("AI_SERVICE_KEY", "")
API_KEY = os.getenv("LLM_API_KEY", "")
MODEL = os.getenv("EMBEDDING_MODEL", "").strip() or "text-embedding-3-small"
CHAT_MODEL = os.getenv("CHAT_MODEL", "").strip() or "gpt-6-luna"
RERANK_MODEL = os.getenv("RERANK_MODEL", "").strip() or CHAT_MODEL
DOTNET_API_URL = os.getenv("DOTNET_API_URL", "http://localhost:5099").rstrip("/")
OCR_LANGUAGES = os.getenv("OCR_LANGUAGES", "vie+eng")
TESSERACT = os.getenv("OCR_TESSERACT_PATH", "tesseract")
