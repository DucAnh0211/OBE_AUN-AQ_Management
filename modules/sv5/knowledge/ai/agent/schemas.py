from typing import Literal, TypedDict
from uuid import UUID
from pydantic import Field
from .. import retrieval

class AgentRequest(retrieval.StrictModel):
    threadId: UUID
    runId: UUID
    lease: UUID
    question: str = Field(min_length=1, max_length=2000)
    history: list[retrieval.ChatTurn] = Field(default_factory=list, max_length=8)


class AgentPlan(retrieval.StrictModel):
    intent: Literal["conversation", "knowledge", "curriculum"]
    question: str
    queries: list[str]
    reply: str


class FriendlyReply(retrieval.StrictModel):
    reply: str


class AgentState(TypedDict, total=False):
    question: str
    history: list[dict]
    plan: dict
    answer: dict
    tool_results: list[dict]
    tool_count: int


class CurriculumToolCall(retrieval.StrictModel):
    tool: Literal["programs", "versions", "courses", "plos", "clos", "course_plo", "clo_plo"]
    programId: int | None
    versionId: int | None
    courseId: int | None
    cloId: int | None
    search: str | None
    page: int


class CurriculumDecision(retrieval.StrictModel):
    action: Literal["tool", "answer"]
    call: CurriculumToolCall | None
    answer: str


