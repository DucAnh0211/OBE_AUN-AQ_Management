# Shared

Code và contract dùng chung cho toàn hệ thống:

- frontend: layout, auth UI, design primitives và permission helpers.
- backend: IAM, audit, error handling, storage port và AI client interface.
- ai: LLM/embedding clients, base schemas và guardrails.
- contracts: OpenAPI, JSON Schema, DTO dùng chung và fixtures.

Shared không chứa business rule riêng của Curriculum, Accreditation hoặc
Reporting. Trạng thái hiện tại: scaffold-only.
