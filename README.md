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

| Nhóm thư mục | Module | Owner | Phạm vi |
| --- | --- | --- | --- |
| `modules/common` | curriculum | SV4 + SV5 | CTĐT, Course, PLO/CLO và Mapping thủ công; không có AI |
| `modules/sv4` | accreditation | SV4 | AUN, Evidence, Ingestion, Retrieval và Gap Analysis |
| `modules/sv5` | reporting | SV5 | Chat, Citation, Report và Export |

Curriculum đã bỏ toàn bộ capability AI; phần nghiệp vụ không dùng AI được SV4
và SV5 cùng phát triển. Code kỹ thuật dùng chung nằm trong `shared`; các thư mục
`apps` chỉ là composition root, không chứa business logic.

## Quy tắc phụ thuộc

- Module chỉ dùng `shared/contracts` và public exports của module khác.
- Public contract của `modules/common/curriculum` cần được cả SV4 và SV5 review.
- Không truy cập repository hoặc bảng nội bộ xuyên module.
- AI chỉ đề xuất; mọi quyết định nghiệp vụ cần người dùng xác nhận và được audit.
- Không lưu file binary lớn trong database.

## Trạng thái

Repository đang ở giai đoạn scaffold-only: chưa cài dependency, chưa có entity,
API, UI hay cấu hình triển khai chạy thật.

## Bước tiếp theo

Chốt framework backend, phiên bản AUN-QA, quy tắc đủ minh chứng và template báo
cáo; sau đó mới khởi tạo stack cụ thể cho từng composition root.
