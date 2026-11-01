import json
from ... import settings, retrieval
from ...errors import PipelineError
from ..schemas import CurriculumDecision
from ..prompts import CURRICULUM_PROMPT


def curriculum_step(state, callback):
    callback("POST", "/phase/retrieval")
    # Re-read prior results: resumed checkpoints must not send stale/revoked data to the model.
    results = [callback("POST", "/curriculum", json=r["source"]["query"]) for r in state.get("tool_results", [])]
    count = state.get("tool_count", 0)
    if count >= 8:
        text = "Mình đã tra cứu qua nhiều bước. Bạn nêu cụ thể chương trình, phiên bản hoặc học phần cần xem để mình thu hẹp kết quả nhé."
    else:
        decision = retrieval.structured(settings.CHAT_MODEL, CurriculumDecision, CURRICULUM_PROMPT,
            {"question": state["plan"]["question"], "results": results, "remainingCalls": 8 - count})
        if decision.action == "tool":
            if decision.call is None:
                raise PipelineError("Trợ lý chưa xác định được tham số tra cứu.")
            call = decision.call.model_dump()
            if call["page"] < 1 or call["page"] > 1000 or len(call.get("search") or "") > 100:
                raise PipelineError("Tham số công cụ vượt giới hạn.")
            result = callback("POST", "/curriculum", json=call)
            if len(json.dumps(result, ensure_ascii=False)) > 60000:
                raise PipelineError("Kết quả quá dài, bạn thu hẹp câu hỏi nhé.")
            return {"tool_results": results + [result], "tool_count": count + 1}
        text = decision.answer
        if not text.strip() or len(text) > 6000:
            raise PipelineError("Phản hồi tra cứu chương trình không hợp lệ.")
        if not results:
            raise PipelineError("Chưa có dữ liệu chương trình để làm căn cứ trả lời.")
    sources = {(r["source"]["url"], r["source"]["hash"]): r["source"] for r in results}
    return {"answer": {"parts": [], "notice": text, "documentCount": 0, "chunkCount": 0,
        "rerankModel": None, "chatModel": settings.CHAT_MODEL, "databaseSources": list(sources.values())},
        "tool_results": results}
