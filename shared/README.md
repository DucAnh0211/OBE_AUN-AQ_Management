# Shared

- Đồng sở hữu: SV4 và SV5.

Code và contract dùng chung cho toàn hệ thống:

- frontend: layout, auth UI, design primitives và permission helpers.
- backend: IAM, audit, error handling, storage port và AI client interface.
- ai: LLM/embedding clients, base schemas và guardrails dùng cho AI của SV4 và
  SV5; không chứa AI riêng của Curriculum.
- contracts: OpenAPI, JSON Schema, DTO dùng chung và fixtures.

Shared không chứa business rule riêng của Curriculum, Accreditation hoặc
Reporting. Nghiệp vụ Curriculum dùng chung nằm tại `modules/common/curriculum`,
không đặt trong thư mục này. Trạng thái hiện tại: scaffold-only.
