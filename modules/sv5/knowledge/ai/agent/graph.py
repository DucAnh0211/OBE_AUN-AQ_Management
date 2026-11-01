from langgraph.graph import StateGraph, START, END
from .. import settings, retrieval
from ..errors import PipelineError
from .schemas import AgentPlan, FriendlyReply, AgentState
from .prompts import SYSTEM_PROMPT, CLARIFY_PROMPT
from .tools.documents import search_documents
from .tools.curriculum import curriculum_step

def build_agent(saver, callback):
    def route(state):
        callback("POST", "/phase/route")
        plan = retrieval.structured(settings.CHAT_MODEL, AgentPlan,
            SYSTEM_PROMPT,
            {"question": state["question"], "history": state.get("history", [])})
        if len(plan.question) > 2000 or len(plan.queries) > 4 or len(plan.reply) > 6000:
            raise PipelineError("Mình chưa hiểu rõ câu hỏi. Bạn thử diễn đạt lại nhé.")
        if plan.intent in ("knowledge", "curriculum"):
            if not plan.question.strip():
                raise PipelineError("Chưa xác định được câu hỏi cần tra cứu.")
            if plan.intent == "knowledge" and (not plan.queries or any(not q.strip() or len(q) > 200 or len(q.split()) > 12 for q in plan.queries)):
                raise PipelineError("Chưa xác định được từ khóa tài liệu phù hợp.")
        elif not plan.reply.strip():
            raise PipelineError("Trợ lý chưa trả được nội dung.")
        return {"plan": plan.model_dump()}

    def conversation(state):
        callback("POST", "/phase/conversation")
        return {"answer": {"parts": [], "notice": state["plan"]["reply"], "documentCount": 0, "chunkCount": 0,
            "rerankModel": None, "chatModel": settings.CHAT_MODEL}}

    def search(state):
        callback("POST", "/phase/retrieval")
        result = search_documents(callback, state["plan"])
        return {"answer": result}

    def finish(state):
        result = state["answer"]
        if state["plan"]["intent"] == "knowledge" and not result["parts"]:
            reply = retrieval.structured(settings.CHAT_MODEL, FriendlyReply,
                CLARIFY_PROMPT,
                {"question": state["question"], "reason": result.get("notice")})
            if not reply.reply.strip() or len(reply.reply) > 6000:
                raise PipelineError("Phản hồi không hợp lệ.")
            result = {**result, "notice": reply.reply}
        callback("POST", "/phase/complete")
        return {"answer": result}

    graph = StateGraph(AgentState)
    graph.add_node("route", route); graph.add_node("conversation", conversation)
    graph.add_node("retrieve", search); graph.add_node("finish", finish)
    graph.add_node("curriculum", lambda state: curriculum_step(state, callback))
    graph.add_edge(START, "route")
    graph.add_conditional_edges("route", lambda state: state["plan"]["intent"], {"conversation": "conversation", "knowledge": "retrieve", "curriculum": "curriculum"})
    graph.add_conditional_edges("curriculum", lambda state: "finish" if state.get("answer") else "curriculum")
    graph.add_edge("conversation", "finish"); graph.add_edge("retrieve", "finish"); graph.add_edge("finish", END)
    return graph.compile(checkpointer=saver)

