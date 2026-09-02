# OBE & AUN-QA Management tích hợp AI

Scaffold ban đầu của hệ thống quản lý Outcome-Based Education (OBE), hồ sơ
kiểm định AUN-QA và khai thác tri thức có căn cứ bằng AI.

## Kiến trúc

Dự án dùng vertical-module monorepo và được ghép thành đúng ba đơn vị triển
khai:

- Web App dùng chung.
- Backend API dạng modular monolith.
- AI Service dùng chung cho ingestion, retrieval và generation.

PostgreSQL + pgvector là nguồn dữ liệu nghiệp vụ và vector; MinIO/S3-compatible
lưu file. Các dependency và business code sẽ chỉ được thêm sau khi scaffold
được duyệt.

## Phân công module

| Module | Owner | Phạm vi |
| --- | --- | --- |
| curriculum | SV1 | CTĐT, Course, PLO/CLO và Mapping Advisor |
| accreditation | SV4 | AUN, Evidence, Ingestion, Retrieval và Gap Analysis |
| reporting | SV5 | Chat, Citation, Report và Export |

Code dùng chung nằm trong shared. Các thư mục apps chỉ là composition root,
không chứa business logic.

## Quy tắc phụ thuộc

- Module chỉ dùng shared/contracts và public exports của module khác.
- Không truy cập repository hoặc bảng nội bộ xuyên module.
- AI chỉ đề xuất; mọi quyết định nghiệp vụ cần người dùng xác nhận và được audit.
- Không lưu file binary lớn trong database.

## Trạng thái

Repository đang ở giai đoạn scaffold-only: chưa cài dependency, chưa có entity,
API, UI hay cấu hình triển khai chạy thật.

## Bước tiếp theo

Chốt framework backend, phiên bản AUN-QA, quy tắc đủ minh chứng và template báo
cáo; sau đó mới khởi tạo stack cụ thể cho từng composition root.
