# Tiến độ phần chung

> Cập nhật 15/11/2026: phân quyền và quy trình sửa điểm hiện dùng [mô hình bốn role và phạm vi trường](FOUR_ROLES_AND_SCHOOL_SCOPE.md). Nội dung Khoa/Bộ môn và sửa trực tiếp điểm công bố bên dưới mô tả bản trước.

Ngày hoàn thành kiểm chứng 14/11/2026. Đã triển khai G0–G7 trong phạm vi phần chung của kế hoạch, với các giới hạn nghiệp vụ ghi rõ bên dưới. Mã nguồn đã kiểm thử trên stack riêng và đã cập nhật migration/API/web lên Docker đang dùng theo yêu cầu tiếp theo của người dùng. Xem [biên bản cập nhật Docker](DOCKER_SHARED_DEPLOYMENT_2026-11-14.md).

| Gói | Trạng thái | Bằng chứng |
| --- | --- | --- |
| G0 baseline | Đạt | 106 test nền; build frontend; DB/API/MinIO dùng riêng |
| G1 quyền/nền tảng | Đã triển khai, kiểm thử đạt | Role Khoa/Bộ môn + scope, UI gán đơn vị, audit transaction, HTTP MinIO thật; client AI chung và fake HTTP tests |
| G2 đầu vào/readiness | Đã triển khai, kiểm thử đạt | Readiness, clone PLO/CLO/mapping/assessment liên kết, grid nhập điểm, giữ quy tắc học vụ cũ |
| G3 CLO | Đã triển khai, kiểm thử đạt | Golden fixture, null/0, threshold, coverage, nguồn assessment và snapshot |
| G4 PLO | Đã triển khai, kiểm thử đạt | Tín chỉ × X/Y, chọn lần học, khóa/kỳ/version, so sánh, GPA phạm vi |
| G5 dashboard | Đã triển khai, kiểm thử đạt | UI trình duyệt thật desktop/mobile, JSON download, grid lưu/tải lại, không lỗi JS |
| G6 contract | Đã triển khai, kiểm thử đạt | IOutcomeReadService, summary/detail phân trang, Reporting adapter, JSON snapshot/hiệu lực/quyền |
| G7 nghiệm thu | Đạt | HTTP học vụ/OBE/MinIO; UI cũ và mới; migration 32 bảng checksum không đổi; build/test/architecture đạt |

Baseline: `dotnet test ObeAunQa.slnx --no-restore --verbosity minimal`, `npm --prefix apps/web run build` thành công. Frontend có cảnh báo bundle lớn đã tồn tại. Docker Desktop chưa chạy ở thời điểm baseline; đã khởi động để chuẩn bị stack thử riêng. Không dùng database người dùng cho kiểm thử mutation.

## Các kiểm chứng đã thực hiện

- Build solution: 0 warning, 0 error.
- Test backend lần cuối: **115/115 đạt** (73 Curriculum, 5 Identity, 8 Accreditation, 13 Reporting, 16 Knowledge), 0 skipped. Có regression chọn cùng lần học định lượng cho aggregate CLO và PLO khi lần mới mang điểm I.
- Frontend `npm --prefix apps/web run build`: đạt, còn cảnh báo bundle lớn có sẵn.
- `python tests/contracts/architecture_check.py`: đạt, 331 file theo ranh giới kiến trúc.
- `academic_smoke.py`: đạt đầy đủ nhập/import → nộp → công bố → phúc khảo → reopen, học lại/GPA, quyền và clone cơ cấu điểm.
- `outcome_smoke.py`: đạt công thức, nguồn đã công bố, GPA, revision conflict, default retake theo quy chế trường, clone outcome, snapshot/hash/stale/latest/export, đọc lịch sử sau archive, Reporting consumer, summary không kèm điểm nguồn, chi tiết phân trang, audit rollback, quyền Khoa/Bộ môn/GV/SV và thu hồi quyền.
- `outcome_ui_smoke.py`: đạt login/API thật, bộ lọc, tổng hợp, snapshot/download, mobile không tràn ngang, grid lưu và tải lại, không lỗi JavaScript.
- `academic_ui_smoke.py`: đạt giao diện mở lớp, thêm sinh viên, nhập/import, điểm chữ dự kiến, khóa điểm đã công bố, cơ cấu điểm, GPA/bảng điểm của admin và sinh viên, hồ sơ GV và responsive. Đã sửa tính độc lập fixture của bộ test (MSSV/họ tên duy nhất mỗi lần chạy) và selector thông báo success để không nhầm trạng thái loading; không bỏ assertion nghiệp vụ.
- `shared_storage_smoke.py`: đạt MinIO thật trong container riêng, upload/read checksum, quyền, vô hiệu hóa và định dạng bị từ chối; không gọi AI thật.
- `outcome_migration_smoke.py`: đạt nâng từ schema trước OBE, 32 bảng Curriculum/Identity giữ nguyên count + checksum từng dòng, bao gồm điểm/lịch sử/phân công. Database kiểm chứng `obe_shared_upgrade_8f3337a0`.
- EF `migrations has-pending-model-changes` cho Curriculum và Identity: không có model change chưa có migration.

## Ranh giới và giới hạn được giữ rõ

- Công thức/ngưỡng là cấu hình đồ án theo kế hoạch; không tuyên bố là quy định AUN-QA.
- Kết quả trong phạm vi được chọn, không phải xét tốt nghiệp; version khác nhau trình bày riêng.
- Điểm CLO ước lượng từ assessment tổng, chưa có điểm từng câu/rubric criterion.
- Gọi AI thật và nghiệp vụ AI riêng SV4/SV5 không thuộc nghiệm thu phần chung. Transport được kiểm thử bằng HTTP giả và các test pipeline hiện có.
- Snapshot lưu input để tái tính; engine hiện xử lý trong bộ nhớ theo scope. Latest scan lịch sử một version; nâng cấp index khi dữ liệu tăng lớn.
- Đã migrate database đang sử dụng sau khi sao lưu và thử trên bản sao; 42 kiểm tra bảo toàn dữ liệu đạt. Hướng dẫn tích hợp/chạy ở `SHARED_OUTPUT_CONTRACTS.md`.

## Môi trường thử

- PostgreSQL: container `obe-shared-test-db`, cổng 55439, database nghiệm thu `obe_shared_final`.
- MinIO: container `obe-shared-test-minio`, cổng 59000, bucket `obe-shared-test`.
- API thử 5298, Vite thử 5198; fixture/screenshots trong `tmp/outcome-test`, `tmp/academic-test` (không đưa vào Git).
- Python Playwright được cài riêng trong `tmp/shared-test-python`, không thay dependency runtime dự án.
- Các database thử bổ sung `obe_shared_test`, `obe_shared_clean`, `obe_shared_upgrade_*` nằm trong container dùng riêng, không trùng dữ liệu ứng dụng chính.

## Đường dẫn triển khai chính

- Engine: `modules/common/curriculum/backend/domain/Rules/OutcomeRules.cs`.
- Service/read/snapshot: `infrastructure/Repositories/EfOutcomeService.cs`; clone: `EfOutcomeCloneService.cs`.
- API: `presentation/OutcomeAnalyticsEndpoints.cs`; role guard: `CurriculumManagerFilter.cs`.
- UI: `frontend/pages/OutcomeDashboardPage.tsx`, `GradeGridPage.tsx`.
- Audit: `CurriculumDbContext.SaveChangesAsync`; deletes/updates chuyển qua `CurriculumWriteSession` để audit cùng transaction.
- Client AI: `shared/backend/AiHttp.cs`, được tái sử dụng bởi `PythonKnowledgeChatPipeline`.
- Việc triển khai theo kế hoạch không còn mục bắt buộc đang chờ. Migration và build/khởi động lại Docker đã thực hiện; người dùng mở web 8080 để sử dụng. Các sổ điểm cũ cần nộp/công bố theo quy trình học vụ mới.

## Truy vết yêu cầu

| Yêu cầu | Bằng chứng triển khai và kiểm chứng |
| --- | --- |
| UC01–UC06 và phiên bản | API/service hiện có, bổ sung quyền manager, clone outcome/mapping, thao tác sửa/lưu trữ học phần trên UI; architecture/unit, outcome HTTP và academic API/UI |
| UC07–UC12 | Assessment/CLO mapping, sổ điểm, import, nộp/công bố/phúc khảo, GPA; giữ và kiểm tra lại academic smoke + UI; bổ sung grid đã test lưu/tải lại |
| UC13–UC15 | OutcomeRules, EfOutcomeService; fixture CLO/PLO tính tay, missing/null/0, ngưỡng trước làm tròn, học lại; unit và HTTP |
| UC16–UC18 | OutcomeDashboardPage, options, summary, detail phân trang, lọc lớp/học phần/kỳ/khóa trong version CTĐT, compare; UI desktop/mobile và HTTP |
| Nền tảng chung | Identity roles/scope, Curriculum audit transaction, MinIO public contract, AiHttp transport; HTTP quyền/rollback/MinIO và fake HTTP AI tests |
| Input cho SV4/SV5 | IOutcomeReadService, snapshot input/results/hash, stale/history/latest/export, Reporting consumer; HTTP integration và JSON download UI |

Ba migration mới: Curriculum `20261114060956_AddOutcomeAnalytics`, `20261114061515_AddOrgUnitAssignments`; Identity `20261114061528_AddManagementRoles`. Không thay nội dung các migration cũ.
