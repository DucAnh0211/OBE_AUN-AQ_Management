# Reporting

- Owner: SV5.
- Phạm vi: pipeline minh chứng và retrieval dùng chung, grounded chat, citation,
  report generation, review và export.
- Frontend: trang, thành phần, API client và kiểu dữ liệu của Reporting.
- Backend: domain, application, infrastructure và presentation của Reporting.
- AI: parsing, chunking, embedding, indexing, retrieval, query router,
  grounded chat, report generator, prompt và evaluation.

SV4 sở hữu file gốc, metadata, quyền và phiên bản; SV5 nhận sự kiện thay đổi để
xử lý job indexing, cung cấp trạng thái/retry và public contract retrieval cho
cả hai module. Quyền, citation, snapshot và vô hiệu hóa nguồn là điều kiện P0.
UC25–UC27 theo phạm vi điều chỉnh đã được giảng viên đồng ý theo xác nhận của
người dùng: báo cáo thiết kế học phần, mapping và độ bao phủ CLO/PLO.

Public entry points là `frontend/index.ts`, `backend/index.ts` và
`ai/__init__.py`. Module dùng Curriculum và Accreditation qua public contracts,
không import repository hoặc implementation nội bộ.

UC25–UC27 đã có nghiệp vụ rule-based: giao diện chọn phạm vi, xem trước, snapshot
lưu PostgreSQL, lịch sử, quyền theo chương trình, thu hồi, audit và DOCX/PDF tiếng Việt.
Hướng dẫn chạy, migration, cấp quyền và giới hạn: [UC25–UC27](../../../docs/UC25_UC27_RULE_BASED.md).
UC28–UC30 và pipeline minh chứng/AI chưa triển khai; chưa tích hợp minh chứng SV4.

Persistence dùng `ReportingDbContext` và `EfReportRepository`; report/audit là
entity riêng, snapshot lưu JSONB. Interface repository thuộc Application.
Xem [kiến trúc và migration EF Core](../../../docs/EF_CORE_ARCHITECTURE.md).
