import app  # Initialize the repository module path for native test execution.
"""Integration fixture only: real extraction/OCR, deterministic embeddings without paid calls.

Run uvicorn tests.smoke_app:app from apps/ai-service with a disposable .NET/DB/MinIO stack.
Never import this module from the production entrypoint.
"""
import os
import time
os.environ["AI_SERVICE_KEY"] = "isolated-kb-service-key-32-characters-only"
os.environ["LLM_API_KEY"] = "isolated-no-provider-calls"

from app.main import app
from modules.sv5.knowledge.ai import routes, retrieval, agent
from modules.sv5.knowledge.ai.errors import PipelineError


def embed(chunks, model, deadline):
    if any("FAIL_EMBEDDING" in item["text"] for item in chunks):
        raise PipelineError("Simulated embedding failure for integration testing.")
    if any("SLOW_EMBEDDING" in item["text"] for item in chunks):
        time.sleep(6)
    for item in chunks:
        item["embedding"] = [1.0, 0.0, 0.0]


routes.embed = embed
retrieval.embed = embed


def structured(model, schema, system, payload):
    if schema is agent.AgentPlan:
        question = payload["question"]
        if "quy chế" in question.lower() or "tín chỉ" in question.lower():
            return agent.AgentPlan(intent="knowledge", question=question, queries=["quy chế"], reply="")
        remembered = any("Minh" in m["content"] for m in payload.get("history", []))
        return agent.AgentPlan(intent="conversation", question=question, queries=[],
            reply="Mình nhớ bạn tên là Minh 😊" if remembered else "Chào bạn! 👋 Mình có thể giúp bạn tìm hiểu tài liệu, quy định và chuẩn đầu ra. Bạn muốn bắt đầu từ đâu?")
    if schema is agent.FriendlyReply:
        return agent.FriendlyReply(reply="Mình chưa tìm được nguồn phù hợp. Bạn nói rõ hơn chủ đề cần tìm nhé 🙂")
    if schema is retrieval.DocumentPlan:
        return retrieval.DocumentPlan(question=payload["question"], queries=["quy chế"] if "missing" not in payload["question"] else ["nonexistent-title"])
    if schema is retrieval.RerankResult:
        return retrieval.RerankResult(results=[retrieval.Relevance(key=c["key"], score=4 if "150" in c["text"] else 0,
            quote="150" if "150" in c["text"] else "") for c in payload["candidates"]])
    evidence = payload["evidence"]
    return retrieval.GroundedAnswer(parts=[retrieval.AnswerPart(text="Điều kiện tốt nghiệp là hoàn thành 150 tín chỉ.",
        citations=[retrieval.Reference(key=c["key"], quote="150") for c in evidence])])


retrieval.structured = structured
