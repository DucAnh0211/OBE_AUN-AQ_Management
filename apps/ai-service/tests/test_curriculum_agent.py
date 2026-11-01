import app
import os
os.environ['AI_SERVICE_KEY'] = 'isolated-unit-service-key-32-characters-only'
os.environ['LLM_API_KEY'] = 'isolated-unit-test-key'
import unittest
from unittest.mock import patch
from pydantic import ValidationError
from langgraph.checkpoint.memory import InMemorySaver
from modules.sv5.knowledge.ai import agent, retrieval
from modules.sv5.knowledge.ai.agent.schemas import CurriculumDecision, CurriculumToolCall
from modules.sv5.knowledge.ai.errors import PipelineError


class CurriculumAgentTests(unittest.TestCase):
    def test_database_route_uses_tool_and_server_sources_without_document_search(self):
        calls = []
        query = CurriculumToolCall(tool='course_plo', versionId=1, courseId=2, programId=None, cloId=None, search=None, page=1)
        source = {'query': query.model_dump(), 'hash': 'snapshot', 'title': 'Mapping', 'url': '/curriculum/versions/1/courses/2/outcomes'}
        def callback(method, path, **kwargs):
            if path == '/retrieve': self.fail('Database questions must not use keyword document search')
            if path == '/curriculum':
                calls.append(kwargs['json'])
                return {'source': source, 'data': {'items': [{'weightCode': 'X', 'progressionCode': 'I'}], 'totalItems': 1, 'hasMore': False}}
        plan = agent.AgentPlan(intent='curriculum', question='Mapping của học phần?', queries=[], reply='')
        steps = [plan, CurriculumDecision(action='tool', call=query, answer=''), CurriculumDecision(action='answer', call=None, answer='Mapping có weightCode X và progressionCode I.')]
        with patch.object(retrieval, 'structured', side_effect=steps):
            result = agent.build_agent(InMemorySaver(), callback).invoke({'question': plan.question, 'history': []}, {'configurable': {'thread_id': 'db-test'}})
        self.assertEqual(result['answer']['databaseSources'], [source])
        self.assertEqual(len(calls), 2)  # Fetch and refresh before synthesis.
        self.assertIn('X', result['answer']['notice'])

    def test_no_database_answer_without_tool_evidence(self):
        steps = [agent.AgentPlan(intent='curriculum', question='PLO?', queries=[], reply=''), CurriculumDecision(action='answer', call=None, answer='Invented result')]
        with patch.object(retrieval, 'structured', side_effect=steps), self.assertRaises(PipelineError):
            agent.build_agent(InMemorySaver(), lambda *a, **k: None).invoke({'question': 'PLO?', 'history': []}, {'configurable': {'thread_id': 'no-source'}})

    def test_sql_and_write_tools_are_not_in_schema(self):
        for tool in ['sql', 'delete_course', 'update_plo']:
            with self.subTest(tool=tool), self.assertRaises(ValidationError):
                CurriculumToolCall(tool=tool, versionId=1, courseId=None, programId=None, cloId=None, search=None, page=1)

    def test_revoked_checkpoint_data_is_rejected_before_model_call(self):
        from modules.sv5.knowledge.ai.agent.tools.curriculum import curriculum_step
        with patch.object(retrieval, 'structured') as model, self.assertRaises(PipelineError):
            def callback(method, path, **kwargs):
                if path == '/curriculum': raise PipelineError('Revoked')
            curriculum_step({'plan': {'question': 'PLO?'}, 'tool_results': [{'source': {'query': {'tool': 'plos'}}}]}, callback)
        model.assert_not_called()
