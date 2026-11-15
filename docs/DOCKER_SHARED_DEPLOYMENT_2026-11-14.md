# Cập nhật Docker phần chung ngày 14 tháng 11 năm 2026

Đã build và triển khai frontend, backend và migration runner từ working tree hiện tại lên Docker Compose `obe-aun-qa`. Database đang dùng là `obe_management`, PostgreSQL host port 5433. Các volume dữ liệu được giữ nguyên.

## Truy cập

- Web: http://localhost:8080
- API: http://localhost:5099
- API readiness: http://localhost:5099/health/ready

API và web đều healthy; database-init kết thúc mã 0. PostgreSQL và AI healthy; MinIO đang chạy. API image và web image đang chạy khớp chính xác image mới build.

## Migration đã áp dụng trong lần này

Database trước triển khai còn ở mô hình học vụ ngày 07/11, nên lần này áp dụng cả hai migration tồn trước và ba migration mới:

1. Curriculum `20261110115802_AlignAcademicRecordsWithRegulations`.
2. Curriculum `20261114060956_AddOutcomeAnalytics`.
3. Curriculum `20261114061515_AddOrgUnitAssignments`.
4. Identity `20261110115833_OfferingLecturerReadAccess`.
5. Identity `20261114061528_AddManagementRoles`.

Reporting, Accreditation và Knowledge không có migration mới. Runner kiểm tra dữ liệu hiện có và bỏ qua seed demo; không reset dữ liệu.

## Sao lưu và kiểm chứng dữ liệu

Bản sao lưu PostgreSQL custom-format, đã kiểm tra bằng pg_restore list và lưu SHA-256:

`tmp/backups/docker-shared-20261114-141249/obe_management-before.dump`

Cùng thư mục có:

- `dump-sha256.txt`, `dump-contents.txt`.
- `previous-images.json`: image cũ và rollback tags cho API/web.
- `before.json`, `clone.json`, `after.json`: lịch sử migration, số lượng và checksum.
- `clone-data-differences.json`, `after-data-differences.json`: đều rỗng.
- `deployment-checks.json`: kết quả HTTP, hash bundle và xác nhận image đang chạy.

Đã thử restore backup vào database riêng `obe_upgrade_check_141249` và chạy migration trên bản sao trước khi áp dụng lên database chính. **42 kiểm tra bảo toàn dữ liệu đều đạt** trên cả bản sao và database chính: cột dữ liệu cũ không đổi, điểm/lịch sử được so theo mã đầu điểm sau khi remap ID, lớp sinh hoạt và cơ cấu đánh giá được đối chiếu theo phép chuyển đổi dự kiến.

Dữ liệu học vụ trước cập nhật gồm 1 sinh viên, 1 ghi danh, 1 lớp học phần, 3 điểm thành phần và 4 bản ghi lịch sử; các dữ liệu này được giữ lại. Không diễn giải checksum toàn bộ hàng là bất biến vì migration học vụ có thêm/bỏ cột và chuyển cơ cấu điểm từ phiên bản sang từng học phần.

Theo migration học vụ, sổ điểm cũ ở trạng thái **đang nhập**; muốn đưa kết quả vào bảng điểm mới cần thực hiện **Nộp điểm → Công bố điểm**. Không tự động phê duyệt điểm thay người dùng. Liên kết Assessment–CLO cần được khai báo/kiểm tra trước khi dùng kết quả OBE.

## Kiểm tra sau triển khai

- HTTP 200: web root, Nginx healthz, API readiness trực tiếp và qua proxy web.
- Bundle web đã phục vụ các route/tính năng `outcome-summary`, `grade-grid`, `outcome-snapshots`.
- Đã kiểm tra lịch sử EF có đủ năm migration nêu trên.
- Các kiểm thử tạo dữ liệu nghiệp vụ đã thực hiện trên stack thử riêng ở phiên triển khai mã nguồn; không chạy smoke tạo tài khoản/điểm trên database chính trong lần cập nhật Docker này.

Migrator có thông báo thiếu thư viện Kerberos `libgssapi_krb5.so.2` trong runtime image; kết nối PostgreSQL bằng mật khẩu vẫn thành công, migration kết thúc mã 0 và API readiness đạt. Xác thực Kerberos không nằm trong cấu hình đã kiểm chứng.

## Cách mở chức năng mới

Truy cập web, tải lại trang nếu đang giữ bundle cũ, rồi mở CTĐT → phiên bản → **Kết quả CLO/PLO**. Chi tiết API, quy tắc và dữ liệu bàn giao ở [hợp đồng đầu ra](SHARED_OUTPUT_CONTRACTS.md).
