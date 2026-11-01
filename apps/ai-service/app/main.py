"""Deployment entrypoint for the SV5 Python AI module."""

from datetime import datetime, timezone

from fastapi import FastAPI

from app.module_registry import MODULE_IDS
from modules.sv5.knowledge.ai.routes import router as knowledge_router
from modules.sv5.knowledge.ai.retrieval import router as retrieval_router
from modules.sv5.knowledge.ai.agent import router as agent_router
from modules.sv5.knowledge.ai.request_guard import KnowledgeRequestGuard


app = FastAPI(
    title="OBE & AUN-QA AI Service",
    version="0.1.0",
    description="SV5 knowledge ingestion, conversational agent and authorized curriculum tools.",
)


app.add_middleware(KnowledgeRequestGuard)
app.include_router(knowledge_router)
app.include_router(retrieval_router)
app.include_router(agent_router)


@app.get("/health")
def health() -> dict[str, str]:
    return {
        "status": "healthy",
        "checkedAtUtc": datetime.now(timezone.utc).isoformat(),
    }


@app.get("/capabilities")
def capabilities() -> dict[str, object]:
    return {
        "status": "knowledge-chat-ready",
        "modules": list(MODULE_IDS),
        "implemented": ["knowledge-extraction", "pdf-ocr", "chunking", "openai-embeddings", "semantic-ranking", "llm-reranking", "grounded-chat", "langgraph-agent", "persistent-threads", "ef-checkpoints", "curriculum-read-tools"],
        "planned": ["evidence-classification", "gap-analysis"],
        "capabilities": {
            "accreditation": [
                "evidence-classification",
                "gap-analysis",
            ],
            "reporting": [
                "document-ingestion",
                "evidence-retrieval",
                "grounded-chat",
                "citation",
                "report-generation",
            ],
        },
    }
