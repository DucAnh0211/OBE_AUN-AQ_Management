from fastapi import APIRouter, Depends, HTTPException
from ..routes import authorize
from ..errors import PipelineError
from .schemas import AgentRequest
from .runtime import run_agent

router = APIRouter(prefix="/internal/knowledge", dependencies=[Depends(authorize)])


@router.post("/agent")
def agent_endpoint(request: AgentRequest):
    try:
        return run_agent(request)
    except PipelineError as error:
        raise HTTPException(422, str(error)) from None
    except Exception:
        # Never return checkpoint payloads, provider bodies or service credentials.
        raise HTTPException(502, "Trợ lý gặp gián đoạn. Bạn có thể thử lại từ checkpoint đã lưu.") from None
