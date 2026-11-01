import httpx
from langsmith import tracing_context
from .. import settings
from ..errors import PipelineError
from .checkpoints import EfCheckpointSaver
from .schemas import AgentRequest
from .graph import build_agent

def run_agent(request: AgentRequest):
    url = f"{settings.DOTNET_API_URL}/api/knowledge/internal/agent/{request.threadId}/{request.runId}"
    with httpx.Client(timeout=240, headers={"X-Service-Key": settings.SERVICE_KEY, "X-Run-Lease": str(request.lease)}) as client:
        def callback(method, path, **kwargs):
            try:
                response = client.request(method, url + path, **kwargs)
                if not response.is_success:
                    if response.status_code in (401, 403, 404, 410):
                        raise PipelineError("Cuộc trò chuyện hoặc tiến trình không còn hiệu lực.")
                    raise PipelineError("Chưa lưu hoặc tra cứu được dữ liệu hội thoại. Bạn thử lại nhé.")
                return response.json() if response.content else None
            except httpx.HTTPError as error:
                raise PipelineError("Kết nối với dịch vụ hội thoại bị gián đoạn. Checkpoint đã lưu sẽ được dùng khi thử lại.") from error
        saver = EfCheckpointSaver(callback, str(request.threadId))
        graph = build_agent(saver, callback)
        config = {"configurable": {"thread_id": str(request.threadId)}, "recursion_limit": 24}
        # Run-specific branches isolate retries from newer messages; EF owns the conversation history.
        with tracing_context(enabled=False):
            snapshot = graph.get_state(config)
            if snapshot.values and not snapshot.next:
                return snapshot.values["answer"]
            inputs = None if snapshot.values else {"question": request.question, "history": [m.model_dump() for m in request.history]}
            result = graph.invoke(inputs, config, durability="sync")
        return result["answer"]

