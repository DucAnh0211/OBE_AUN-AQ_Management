import app  # Initialize the repository module path for native test execution.
import unittest
from unittest.mock import patch
from fastapi.testclient import TestClient
from app.main import app
from modules.sv5.knowledge.ai import retrieval as r
from modules.sv5.knowledge.ai import settings
from modules.sv5.knowledge.ai.errors import PipelineError


class RetrievalTests(unittest.TestCase):
    def request(self):
        return r.AnswerRequest(question="Điều kiện tốt nghiệp?", candidates=[
            r.Evidence(key="a", title="Quy chế", locator="Trang 2", text="Sinh viên cần hoàn thành 150 tín chỉ."),
            r.Evidence(key="b", title="Quy chế", locator="Trang 1", text="Trường có thư viện.")])

    def test_cosine_orders_by_meaning_not_input_order(self):
        result = r.semantic_rank(r.RankRequest(query=[1, 0], chunks=[
            r.VectorChunk(key="wrong", embedding=[0, 1]), r.VectorChunk(key="right", embedding=[2, 0])]))
        self.assertEqual(result[0], {"key": "right", "score": 1})

    def test_dimension_mismatch_and_zero_vectors_rejected(self):
        for query, vector in [([1, 0], [1]), ([0, 0], [1, 0]), ([1, 0], [0, 0])]:
            with self.assertRaises(PipelineError):
                r.semantic_rank(r.RankRequest(query=query, chunks=[r.VectorChunk(key="x", embedding=vector)]))

    def test_reranking_filters_irrelevant_and_preserves_exact_quote(self):
        ranking = r.RerankResult(results=[r.Relevance(key="b", score=0, quote=""),
            r.Relevance(key="a", score=4, quote="150 tín chỉ")])
        answer = r.GroundedAnswer(parts=[r.AnswerPart(text="Cần hoàn thành 150 tín chỉ.",
            citations=[r.Reference(key="a", quote="150 tín chỉ")])])
        with patch.object(r, "structured", side_effect=[ranking, answer]) as call:
            result = r.answer(self.request())
            self.assertEqual(len(call.call_args.args[3]["evidence"]), 1)
            self.assertEqual(result["parts"][0]["citations"][0]["key"], "a")

    def test_no_relevant_evidence_never_generates_answer(self):
        ranking = r.RerankResult(results=[r.Relevance(key=k, score=1, quote="") for k in ["a", "b"]])
        with patch.object(r, "structured", return_value=ranking) as call:
            self.assertEqual(r.answer(self.request())["parts"], [])
            self.assertEqual(call.call_count, 1)

    def test_invented_rerank_key_or_quote_rejected(self):
        for key, quote in [("invented", "150 tín chỉ"), ("a", "999 tín chỉ")]:
            ranking = r.RerankResult(results=[r.Relevance(key=key, score=4, quote=quote), r.Relevance(key="b", score=0, quote="")])
            with patch.object(r, "structured", return_value=ranking), self.assertRaises(PipelineError):
                r.answer(self.request())

    def test_answer_cannot_cite_excluded_or_fabricated_sources(self):
        ranking = r.RerankResult(results=[r.Relevance(key="a", score=4, quote="150 tín chỉ"), r.Relevance(key="b", score=0, quote="")])
        for key, quote in [("b", "thư viện"), ("a", "999 tín chỉ")]:
            answer = r.GroundedAnswer(parts=[r.AnswerPart(text="Claim", citations=[r.Reference(key=key, quote=quote)])])
            with patch.object(r, "structured", side_effect=[ranking, answer]), self.assertRaises(PipelineError):
                r.answer(self.request())

    def test_keyword_planning_accepts_followup_history(self):
        expected = r.DocumentPlan(question="Điều kiện tốt nghiệp theo Quy chế 2025?", queries=["quy chế 2025", "quy chế"])
        with patch.object(r, "structured", return_value=expected) as call:
            result = r.document_keywords(r.PlanRequest(question="Còn tốt nghiệp thì sao?", history=[r.ChatTurn(role="user", content="Quy chế 2025")]))
            self.assertEqual(result.queries, expected.queries)
            self.assertTrue(call.call_args.args[3]["history"])

    def test_internal_endpoints_require_service_key(self):
        with patch.object(settings, "SERVICE_KEY", "x" * 32), TestClient(app) as client:
            for path in ["document-keywords", "embed-query", "semantic-rank", "answer"]:
                self.assertEqual(client.post("/internal/knowledge/" + path, json={}).status_code, 401)


if __name__ == "__main__":
    unittest.main()
