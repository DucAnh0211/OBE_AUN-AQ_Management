import app  # Initialize the repository module path for native test execution.
import json
import os
import unittest
from unittest.mock import patch

# Set test credentials before importing the shared settings module (test discovery order).
os.environ["AI_SERVICE_KEY"] = "isolated-unit-service-key-32-characters-only"
os.environ["LLM_API_KEY"] = "isolated-unit-test-key"
from modules.sv5.knowledge.ai import agent, retrieval
from modules.sv5.knowledge.ai.agent.checkpoints import EfCheckpointSaver
from modules.sv5.knowledge.ai.errors import PipelineError


class MemoryBoundary:
    """Test-only stand-in for the EF callback API; reused across new saver instances."""
    def __init__(self):
        self.checkpoints = {}; self.writes = {}; self.retrievals = 0; self.fail_once = False

    def __call__(self, method, path, **kwargs):
        if path == "/checkpoint" and method == "GET":
            key = kwargs.get("params", {}).get("checkpointId") or max(self.checkpoints, default=None)
            return None if key not in self.checkpoints else {"checkpoint": self.checkpoints[key], "writes": list(self.writes.get(key, {}).values())}
        if path == "/checkpoint":
            value = kwargs["json"]; self.checkpoints[value["checkpointId"]] = value
        elif path == "/writes":
            value = kwargs["json"]; target = self.writes.setdefault(value["checkpointId"], {})
            for row in value["writes"]:
                key = (row["taskId"], row["index"])
                if key not in target or row["index"] < 0: target[key] = row
        elif path == "/retrieve":
            self.retrievals += 1
            if self.fail_once:
                self.fail_once = False
                raise PipelineError("Simulated interruption")
            return {"parts": [{"text": "150 tín chỉ", "citations": [{"key": "source", "quote": "150"}]}], "notice": None, "documentCount": 1, "chunkCount": 1, "rerankModel": "test", "chatModel": "test"}


class AgentTests(unittest.TestCase):
    def test_greeting_is_conversation_without_search_and_is_checkpointed(self):
        boundary = MemoryBoundary(); saver = EfCheckpointSaver(boundary, "thread-1")
        graph = agent.build_agent(saver, boundary)
        plan = agent.AgentPlan(intent="conversation", question="", queries=[], reply="Chào bạn! 👋 Mình có thể giúp gì cho bạn hôm nay?")
        with patch.object(retrieval, "structured", return_value=plan):
            result = graph.invoke({"question": "Chào bạn", "history": []}, {"configurable": {"thread_id": "thread-1"}}, durability="sync")
        self.assertIn("👋", result["answer"]["notice"])
        self.assertEqual(boundary.retrievals, 0)
        self.assertGreaterEqual(len(boundary.checkpoints), 4)
        restored = agent.build_agent(EfCheckpointSaver(boundary, "thread-1"), boundary).get_state({"configurable": {"thread_id": "thread-1"}})
        self.assertEqual(restored.values["answer"], result["answer"])
        self.assertFalse(restored.next)

    def test_failure_resumes_from_saved_node_without_rerouting(self):
        boundary = MemoryBoundary(); boundary.fail_once = True
        config = {"configurable": {"thread_id": "thread-2"}}
        plan = agent.AgentPlan(intent="knowledge", question="Điều kiện tốt nghiệp?", queries=["quy chế"], reply="")
        with patch.object(retrieval, "structured", return_value=plan) as provider:
            graph = agent.build_agent(EfCheckpointSaver(boundary, "thread-2"), boundary)
            with self.assertRaises(PipelineError): graph.invoke({"question": "Điều kiện tốt nghiệp?", "history": []}, config, durability="sync")
            resumed = agent.build_agent(EfCheckpointSaver(boundary, "thread-2"), boundary)
            result = resumed.invoke(None, config, durability="sync")
            self.assertEqual(provider.call_count, 1)
            self.assertEqual(result["answer"]["parts"][0]["text"], "150 tín chỉ")
            self.assertEqual(boundary.retrievals, 2)

    def test_invalid_branch_fields_are_rejected_before_retrieval(self):
        plans = [
            agent.AgentPlan(intent="conversation", question="", queries=[], reply=" "),
            agent.AgentPlan(intent="knowledge", question=" ", queries=["quy chế"], reply=""),
            agent.AgentPlan(intent="knowledge", question="Điều kiện tốt nghiệp?", queries=[], reply=""),
            agent.AgentPlan(intent="knowledge", question="Điều kiện tốt nghiệp?", queries=[" "], reply=""),
        ]
        for plan in plans:
            with self.subTest(plan=plan):
                boundary = MemoryBoundary()
                graph = agent.build_agent(EfCheckpointSaver(boundary, "invalid-plan"), boundary)
                with patch.object(retrieval, "structured", return_value=plan), self.assertRaises(PipelineError):
                    graph.invoke({"question": "Kiểm thử", "history": []}, {"configurable": {"thread_id": "invalid-plan"}})
                self.assertEqual(boundary.retrievals, 0)

    def test_failed_greeting_can_resume_after_validation_fix(self):
        boundary = MemoryBoundary()
        config = {"configurable": {"thread_id": "retry-greeting"}}
        graph = agent.build_agent(EfCheckpointSaver(boundary, "retry-greeting"), boundary)
        with patch.object(retrieval, "structured", side_effect=PipelineError("Previous route failure")), self.assertRaises(PipelineError):
            graph.invoke({"question": "Chào bạn", "history": []}, config, durability="sync")
        plan = agent.AgentPlan(intent="conversation", question="", queries=[], reply="Chào bạn! 👋")
        restored = agent.build_agent(EfCheckpointSaver(boundary, "retry-greeting"), boundary)
        with patch.object(retrieval, "structured", return_value=plan):
            result = restored.invoke(None, config, durability="sync")
        self.assertEqual(result["answer"]["notice"], plan.reply)
        self.assertEqual(boundary.retrievals, 0)

    def test_checkpointer_rejects_pickle_payload(self):
        saver = EfCheckpointSaver(MemoryBoundary(), "thread-3")
        with self.assertRaises(PipelineError): saver.unpack({"type": "pickle", "data": ""})

    def test_pending_writes_wait_for_checkpoint_commit(self):
        from concurrent.futures import ThreadPoolExecutor
        from threading import Event
        boundary = MemoryBoundary(); saver = EfCheckpointSaver(boundary, "race-test")
        started = Event()
        def writes():
            started.set()
            saver.put_writes(saver.config("checkpoint-1"), [("answer", "hello")], "task")
        with ThreadPoolExecutor() as pool:
            future = pool.submit(writes)
            self.assertTrue(started.wait(2))
            self.assertFalse(boundary.writes)
            saver.put({"configurable": {}}, {"id": "checkpoint-1"}, {}, {})
            future.result(timeout=2)
        self.assertIn("checkpoint-1", boundary.writes)


if __name__ == "__main__": unittest.main()
