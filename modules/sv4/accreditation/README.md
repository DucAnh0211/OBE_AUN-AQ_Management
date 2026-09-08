# Accreditation

- Owner: SV4.
- Phạm vi: AUN criteria, Evidence, Ingestion, Retrieval và Gap Analysis.
- Frontend: trang, thành phần, API client và kiểu dữ liệu của Accreditation.
- Backend: domain, application, infrastructure và presentation của Accreditation.
- AI: ingestion, retrieval, classification, gap analysis, prompt và evaluation.

Public entry points là `frontend/index.ts`, `backend/index.ts` và
`ai/__init__.py`. Module chỉ dùng public contract của Curriculum, không truy cập
implementation nội bộ.

Trạng thái hiện tại: scaffold-only, chưa có business code hoặc dependency.
