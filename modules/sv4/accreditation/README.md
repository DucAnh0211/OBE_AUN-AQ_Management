# Accreditation

- Owner: SV4.
- Phạm vi: AUN criteria, quản lý file minh chứng, metadata, quyền, phiên bản,
  tìm kiếm danh mục và Gap Analysis.
- Frontend: trang, thành phần, API client và kiểu dữ liệu của Accreditation.
- Backend: domain, application, infrastructure và presentation của Accreditation.
- AI: classification, summarization, gap analysis, prompt và evaluation.

SV4 phát sự kiện sau khi lưu hoặc thay đổi tài liệu, hiển thị trạng thái indexing
và dùng public contract retrieval của SV5. Pipeline parse/chunk/embedding/index
và retrieval nội dung dùng chung thuộc SV5; SV4 không triển khai pipeline riêng.

Public entry points là `frontend/index.ts`, `backend/index.ts` và
`ai/__init__.py`. Module dùng public contract của Curriculum và retrieval của SV5, không truy cập
implementation nội bộ.

Trạng thái hiện tại: scaffold-only, chưa có business code hoặc dependency.
