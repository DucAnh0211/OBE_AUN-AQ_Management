# Reporting

- Owner: SV5.
- Phạm vi: grounded chat, citation, report generation, review và export.
- Frontend: trang, thành phần, API client và kiểu dữ liệu của Reporting.
- Backend: domain, application, infrastructure và presentation của Reporting.
- AI: query router, grounded chat, report generator, prompt và evaluation.

Public entry points là `frontend/index.ts`, `backend/index.ts` và
`ai/__init__.py`. Module dùng Curriculum và Accreditation qua public contracts,
không import repository hoặc implementation nội bộ.

Trạng thái hiện tại: scaffold-only, chưa có business code hoặc dependency.
