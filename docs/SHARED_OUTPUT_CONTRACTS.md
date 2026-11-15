# Dữ liệu dùng chung cho SV4 và SV5

> Phân quyền hiện hành: [bốn role, tài nguyên theo trường và yêu cầu sửa điểm](FOUR_ROLES_AND_SCHOOL_SCOPE.md), thay thế role quản lý Khoa/Bộ môn của bản trước.

Phần tính toán thuộc `modules/common/curriculum`. Consumer lấy dữ liệu qua interface hoặc HTTP, không truy vấn trực tiếp bảng Curriculum và không tự tính lại công thức trong prompt AI.

## Đường dùng trên giao diện

- Mở một phiên bản CTĐT → tab **Kết quả CLO/PLO**.
- Chọn lớp học phần/học phần, học kỳ và khóa. Giảng viên phải chọn lớp được phân công; Bộ môn chọn phạm vi học phần thuộc bộ môn.
- Học kỳ và Khóa là dropdown từ dữ liệu trong phạm vi được xem, có lựa chọn Tất cả; học kỳ đối chiếu dùng cùng danh sách và bỏ kỳ đang chọn. `outcome-options` trả thêm `cohorts`, chỉ gồm khóa của sinh viên ghi danh trong các lớp được phép xem; sinh viên chỉ thấy khóa của chính mình. Chưa có dữ liệu khóa thì chỉ có lựa chọn Tất cả khóa.
- Bấm **Tải kết quả**. Cấu hình chưa đủ sẽ được liệt kê với đối tượng cần sửa.
- Xem CLO/PLO, tỷ lệ, coverage và chi tiết nguồn assessment/ghi danh.
- Admin/Khoa/Bộ môn có quyền tạo snapshot chính thức trong phạm vi. Giảng viên tạo bản xem trước.
- Tải JSON để bàn giao; mở lại snapshot gần nhất hoặc đọc ID lịch sử. Snapshot cũ được giữ nguyên, trả `stale=true` nếu dữ liệu nguồn/policy đã thay đổi.
- Danh sách sinh viên của lớp → **Bảng nhập điểm** để nhập nhiều dòng. Điểm đã công bố/phúc khảo dùng màn hình chi tiết có lý do.
- Admin → Người dùng → chọn vai trò Khoa/Bộ môn → Phân công để gán đơn vị. Khoa quản lý qua `Program.FacultyId`, Bộ môn qua `Course.DepartmentId`.
- Bản draft được sao chép từ nguồn có nút **Sao chép PLO/CLO và ánh xạ từ nguồn** trên thẻ phiên bản. Đích có outcome sẽ được từ chối ghi đè.

## Interface trong backend

`ObeAunQa.Modules.Curriculum.Application.IOutcomeReadService`:

```csharp
Task<OutcomeResult> ReadAsync(OutcomeScope scope, CancellationToken ct);
Task<OutcomeSnapshotResponse> SnapshotAsync(long versionId, Guid id, CancellationToken ct);
```

`IOutcomeAnalyticsService` mở rộng với policy, options, compare, create/latest snapshot và audit. `IOutcomeCloneService.CopyAsync(targetVersionId, sourceVersionId, ct)` sao chép outcome/mapping trong transaction.

Tất cả các hàm đọc giữ quyền của `ICurrentUser`; không thay bằng tài khoản quản trị để phục vụ chatbot. Sinh viên chỉ nhận dữ liệu cá nhân đã công bố, không đọc/export snapshot lớp. Giảng viên chỉ đọc lớp thực tế được phân công, không có quyền tổng hợp toàn ngành.

Reporting có `OutcomeReportSource` làm consumer của interface trên và endpoint bàn giao `/api/reporting/outcome-source/{versionId}/{snapshotId}`. Endpoint này kiểm tra thêm quyền chương trình của Reporting. Chức năng xuất DOCX/PDF cũ giữ nguyên nội dung thiết kế CTĐT; phần sinh báo cáo đánh giá đầy đủ thuộc SV5.

SV4 có thể dùng cùng `IOutcomeReadService.SnapshotAsync` để tạo tham chiếu minh chứng đến snapshot ID và hash. Việc gắn tiêu chí, lập hồ sơ và phân loại minh chứng thuộc module SV4.

Hạ tầng file tiếp tục dùng public contract `IDocumentLibrary` của Accreditation (upload/find/list/read/disable), với kiểm tra quyền tại endpoint nghiệp vụ. Không tạo kho file thứ hai cho phần chung. HTTP/MinIO round-trip, checksum và vô hiệu hóa được kiểm thử trong `shared_storage_smoke.py`.

Hàm JSON gọi AI dùng chung ở `ObeAunQa.SharedKernel.AiHttp.PostAsync<T>(http, baseUrl, sharedKey, relativePath, body, ct)`. Module sở hữu prompt/DTO và cấu hình timeout HttpClient; secret lấy từ cấu hình môi trường, không đưa vào payload hoặc audit. Transport gắn X-Service-Key, xử lý lỗi kết nối/JSON/timeout và truyền cancellation. `PythonKnowledgeChatPipeline` đã dùng hàm này; SV4 có thể gọi cùng hàm cho endpoint nghiệp vụ của mình. Fake HTTP tests kiểm tra URL, secret header, body, lỗi upstream, cấu hình thiếu và cancellation; không gọi LLM thật.

## HTTP

Prefix: `/api/curriculum/program-versions/{versionId}`. Cần Bearer token hợp lệ đã đổi mật khẩu.

| Method và path | Chức năng |
| --- | --- |
| GET `/outcome-options` | Danh sách học phần/lớp/kỳ trong quyền để tạo bộ lọc |
| GET/PUT `/outcome-policy` | Đọc hoặc tạo revision policy; PUT phải gửi revision hiện tại |
| GET `/outcome-readiness` | Dữ liệu tính kèm issues cấu hình |
| GET `/clo-results`, `/plo-results` | Kết quả thống nhất từ cùng engine |
| GET `/outcome-summary` | Tổng hợp cho dashboard, không kèm toàn bộ điểm nguồn/sinh viên |
| GET `/outcome-students` | Chi tiết phân trang phía server, `page`, `search`, 50 dòng/trang |
| GET `/offerings/{offeringId}/clo-results` | Kết quả trong một lớp |
| GET `/outcome-comparisons` | So sánh hai kỳ theo policy hiện hành; cần `period`, `otherPeriod` |
| POST `/outcome-snapshots` | Lưu dữ liệu nguồn và kết quả bất biến |
| GET `/outcome-snapshots/{id}` | Đọc lịch sử và trạng thái stale theo quyền hiện tại |
| GET `/outcome-snapshots/{id}/export` | Tải JSON snapshot |
| GET `/outcome-snapshots/latest` | Snapshot gần nhất khớp phạm vi, trả 409 nếu stale, 404 nếu chưa có |
| POST `/copy-outcomes/{sourceVersionId}` | Sao chép outcome/mapping vào bản draft trống cùng CTĐT |

GET kết quả/readiness/latest nhận `offeringId`, `programCourseId`, `period`, `cohort`, `preview`. Có thể bỏ bộ lọc để đọc toàn version trong quyền. Phạm vi vượt quyền bị từ chối, không âm thầm loại dữ liệu rồi ghi là tổng toàn bộ.

Ví dụ tạo snapshot:

```json
{
  "versionId": 1,
  "offeringId": 12,
  "programCourseId": null,
  "period": null,
  "cohort": null,
  "preview": false
}
```

Các ID trong ví dụ chỉ minh họa, phải thay bằng ID hiện có. Body versionId phải khớp path. Kỳ học dùng `YYYY-YYYY-HK1`, `HK2`, `HKHE`.

Admin đọc audit ở `GET /api/curriculum/audit-events?page=1`. Mỗi trang tối đa 50 bản ghi, gồm actor, entity, thao tác, thời điểm và thay đổi; ghi cùng transaction nghiệp vụ. Identity vẫn có audit riêng.

## Cấu trúc kết quả

- `schemaVersion`: `obe/1`.
- `input`: scope, policy, trường/chương trình, danh sách outcome/assessment/mapping, ghi danh và điểm được phép đọc. Đây cũng là input để tái tính snapshot.
- `sourceHash`: SHA-256 của input chuẩn hóa, có thứ tự xác định.
- `status`: `preview`, `official`, `partial`, `no_data`. Official nói về nguồn điểm đã công bố; không đồng nghĩa mọi SV đạt hoặc CTĐT đã được phê duyệt học thuật.
- `issues`: lỗi cấu hình có code/entityId/message. Ví dụ `assessment_mapping`, `unassessed_clo`, `unmapped_clo`, `unsupported_plo`, `invalid_period`, `duplicate_enrollment`.
- `cloStudents` / `ploStudents`: điểm chưa làm tròn, ngưỡng, trạng thái, reason, enrollmentIds; CLO có assessment/điểm tối đa/trọng số hiệu lực để giải thích; PLO có số môn dự kiến/đã đánh giá.
- `clos` / `plos`: số được xét, số có kết quả, số đạt, số thiếu/loại, tỷ lệ đạt, coverage, điểm trung bình và kết luận nhóm.
- `gpas`: GPA học kỳ và tích lũy **trong phạm vi đang chọn**, dùng thang điểm/kết quả công bố đã lưu. GPA đầy đủ của sinh viên lấy từ API bảng điểm hiện có; không đổi tên GPA một môn thành GPA toàn khóa.
- `OutcomeSnapshotResponse`: id, createdAt UTC, createdBy, stale và result gốc. Stale là thuộc tính tính lúc đọc, không sửa result đã lưu.

Khi không có mẫu số, rate/meanScore là null. Sinh viên chưa có đủ điểm khác sinh viên điểm 0. Tỷ lệ nhóm là tỷ lệ người đạt trên người được tính, không phải trung bình tỷ lệ các lớp. Với cùng outcome và nhiều lần học, aggregate chọn một lần theo policy; chi tiết giữ các lần để truy vết.

## Công thức và giới hạn

- Các mặc định là lựa chọn thiết kế đồ án, không được gắn nhãn quy định AUN-QA: CLO/PLO đạt 5/10, mục tiêu nhóm 70%, chỉ kết luận nhóm đủ khi coverage 100%.
- Mỗi assessment có tổng trọng số nội dung gắn các CLO là 100%. Mapping cũ chưa đúng được báo readiness; không tự sửa dữ liệu người dùng.
- CLO = trung bình điểm assessment chuẩn hóa về 10 với trọng số hiệu lực = trọng số assessment × phần đóng góp vào CLO. Chưa có điểm từng câu/rubric criterion nên đây là ước lượng từ điểm assessment.
- PLO học phần = trung bình CLO liên kết. PLO trong phạm vi dùng tín chỉ × X=2/Y=1; I/R/E không nhân vào điểm.
- Không bỏ CLO/học phần thiếu điểm để làm tăng kết quả. Điểm R/P không chứng minh đạt CLO; I là chưa đủ dữ liệu.
- Không trộn các version hoặc PLO/CLO chỉ vì cùng mã. Toàn ngành nhiều version phải đọc từng version và trình bày tách nhóm.
- Không kết luận chuẩn tốt nghiệp từ scope học phần đã chọn, đặc biệt khi chưa mô hình hóa đầy đủ nhóm tự chọn.
- So sánh kỳ dùng cùng định nghĩa outcome và policy hiện hành; không quảng bá như so sánh lịch sử hai policy khác nhau.
- JSON đầy đủ/snapshot chứa nguồn/chi tiết trong quyền; dashboard dùng summary và endpoint chi tiết phân trang phía server. Engine hiện tính trong bộ nhớ theo phạm vi; với dữ liệu thực lớn cần tối ưu đọc/tính và thêm index scope cho lịch sử snapshot.

## Migrations và chạy thử

Migration mới chỉ thêm bảng outcome/audit/assignment và mở constraint role. Không tự cập nhật database đang dùng. Chạy `tools/database migrate` trên bản sao/DB đã sao lưu theo hướng dẫn dự án trước khi dùng tính năng.

Các kiểm tra chính:

```powershell
dotnet build ObeAunQa.slnx
dotnet test ObeAunQa.slnx
npm --prefix apps/web run build
python tests/contracts/architecture_check.py
```

Kiểm thử mutation chỉ chạy trên stack dùng một lần. Ví dụ stack thử ở API 5298, web 5198 và PostgreSQL 55439:

```powershell
$env:PYTHONUTF8 = '1'
$env:SMOKE_BASE = 'http://127.0.0.1:5298'
$env:SMOKE_DATABASE_CONTAINER = 'obe-shared-test-db'
$env:SMOKE_DATABASE_USER = 'postgres'
$env:SMOKE_DATABASE_NAME = 'obe_shared_final'
python tests/contracts/academic_smoke.py
python tests/contracts/outcome_smoke.py
python tests/contracts/outcome_migration_smoke.py
python tests/contracts/shared_storage_smoke.py
$env:PYTHONPATH = (Join-Path (Get-Location) 'tmp/shared-test-python')
python tests/contracts/outcome_ui_smoke.py
```

API fixture tạo `tmp/outcome-test/fixture.json`; UI dùng fixture đó và Playwright/Edge headless. `academic_smoke.py` phải chạy một lần trên DB sạch vì nó giữ tài khoản fixture cố định. Không chạy lặp trên DB nghiệp vụ.
