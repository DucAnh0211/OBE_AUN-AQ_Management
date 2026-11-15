# Bốn vai trò và phạm vi trường

Yêu cầu phân quyền ngày 15/11/2026 thay thế mô hình quản lý khoa/bộ môn trong kế hoạch cũ.

| Vai trò | Tài khoản và tài nguyên | Điểm | Hỏi đáp |
|---|---|---|---|
| Admin tổng (`admin`) | Tạo trường, tạo admin trường, quản trị toàn hệ thống; chọn trường khi tạo tài nguyên | Quản trị và xét duyệt yêu cầu | Theo phạm vi trường đang chọn; có quyền giám sát toàn hệ thống |
| Admin trường (`school_admin`) | Quản lý tài khoản giảng viên/sinh viên và toàn bộ tài nguyên của trường; không tạo admin hoặc trường khác | Phân công, công bố điểm, duyệt hoặc từ chối đề nghị sửa | Tài nguyên của trường |
| Giảng viên (`lecturer`) | Đọc tài nguyên trường; admin gán hồ sơ vào bộ môn và phân công lớp học phần | Nhập điểm lớp được phân công, thuộc đúng bộ môn; nộp điểm; gửi đề nghị sau công bố | Tài nguyên trường, gồm tài liệu nội bộ |
| Sinh viên (`student`) | Chỉ đọc dữ liệu được cấp, CTĐT được gán; không quản lý tài khoản hoặc sửa học vụ | Chỉ bảng điểm và kết quả CLO/PLO cá nhân đã công bố | Tài liệu dùng chung cho trường và tài liệu riêng của chính mình; công cụ bảng điểm chỉ lấy người đang đăng nhập |

## Thao tác sử dụng

1. Đăng nhập tài khoản admin tổng hiện có. Tạo trường hoặc sử dụng trường đã có.
2. Chọn **Trường đang quản lý** ở đầu trang, mở **Quản lý tài khoản**, tạo tài khoản **Admin trường**. Tài khoản mới bắt buộc đổi mật khẩu tạm.
3. Admin trường tạo khoa/bộ môn, CTĐT, phiên bản, học phần, PLO/CLO và cơ cấu điểm trong trường mình.
4. Admin trường tạo tài khoản giảng viên/sinh viên. Trong **Hồ sơ giảng viên**, gán giảng viên vào bộ môn; trong cấu hình học phần gán bộ môn phụ trách; khi mở lớp học phần chọn giảng viên thuộc bộ môn đó.
5. Thêm hồ sơ sinh viên vào lớp và liên kết tài khoản sinh viên; gán CTĐT để sinh viên tra cứu CTĐT và kết quả chuẩn đầu ra.
6. Giảng viên nhập điểm theo đầu điểm, nộp điểm; admin trường công bố.
7. Sau công bố, giảng viên mở điểm chi tiết của sinh viên, nhập điểm đề nghị và lý do, bấm **Gửi yêu cầu sửa điểm**. Điểm chính thức giữ nguyên khi đang chờ.
8. Admin trường vào **Yêu cầu sửa điểm**, xem bảng điểm cũ/đề nghị, phê duyệt hoặc từ chối. Từ chối bắt buộc có lý do.

Khi phê duyệt, hệ thống kiểm tra revision điểm/cơ cấu điểm, cập nhật điểm và bảng điểm, ghi lịch sử và trạng thái yêu cầu trong cùng giao dịch. Một yêu cầu chỉ xử lý một lần, kể cả khi hai người duyệt đồng thời. Nếu dữ liệu nguồn đã thay đổi, trả xung đột để tải lại; không ghi đè âm thầm. Snapshot CLO/PLO cũ được giữ nguyên và đánh dấu stale khi nguồn thay đổi.

Điểm đã công bố không được sửa trực tiếp, kể cả bằng endpoint của admin. Chức năng trả lại sổ điểm chỉ áp dụng cho sổ đã nộp và chưa công bố.

## Tài liệu và chatbot

Khi tải tài liệu lên Knowledge Base, chọn phạm vi:

- **Toàn trường**: thành viên trường được tra cứu.
- **Nội bộ giảng viên và admin trường**: sinh viên không được truy xuất.
- **Riêng một sinh viên và giảng viên**: chọn tài khoản sinh viên trong cùng trường; sinh viên khác không được truy xuất.

Phạm vi được kiểm tra tại danh mục, tìm kiếm, đọc nguồn trích dẫn và tải bản gốc. Nội dung không được lọc chỉ bằng prompt. Callback nội bộ xác thực service key, lease của lần chạy, chủ hội thoại, trường và vai trò. Công cụ `my_transcript` không nhận ID sinh viên để chọn người khác.

Hội thoại được gắn với vai trò lúc tạo. Khi đổi giảng viên thành sinh viên, hội thoại cũ không được dùng làm lịch sử hoặc ngữ cảnh dưới vai trò mới; dữ liệu hội thoại không bị xóa.

## Cách cô lập dữ liệu

Các bảng tài nguyên học vụ, kiểm định, báo cáo, tài liệu, chỉ mục tri thức và hội thoại có `school_id` cùng bộ lọc EF phía server. Khi ghi, hệ thống tự gắn trường và kiểm tra các quan hệ cha/con, gồm tham chiếu tài khoản hoặc tài liệu giữa các phân hệ. Các mã kỹ thuật cố định như X/Y, I/R/E là danh mục hằng số dùng chung.

Trường của thành viên lấy từ tài khoản và claim đã ký. `X-University-Id` chỉ chọn phạm vi cho admin tổng; thành viên trường không thể đổi trường bằng header hoặc nội dung request. API kiểm tra lại trạng thái tài khoản, token version và trường đang hoạt động. API báo cáo dùng JWT; mã truy cập báo cáo cũ không có trường không còn được dùng để đăng nhập.

## Migration và dữ liệu hiện có

- Thêm migration `SchoolTenancy` cho cả 5 phân hệ và `ChatRoleScope` cho Knowledge.
- Giữ nguyên dữ liệu nghiệp vụ và password hash. Thu hồi phiên cũ vì token trước đây chưa có trường; đăng nhập lại bằng mật khẩu hiện tại.
- Suy ra trường từ dữ liệu cha và phân công hiện có. Với hệ thống chỉ có một trường, tài nguyên cũ chưa gắn trường được đưa vào trường đó.
- Dữ liệu không xác định được trường ở hệ thống nhiều trường giữ `school_id = NULL`, chỉ admin tổng nhìn thấy, không chia sẻ cho các trường. Cần phân loại dữ liệu cũ trước khi cho thành viên sử dụng.
- Tài khoản role quản lý khoa/bộ môn cũ, nếu có, chuyển sang `school_admin` ở trạng thái disabled để admin tổng xem xét, tránh tự mở rộng quyền. Hai role đó không còn được tạo hoặc dùng trong giao diện/API.
- Đối với dữ liệu đang chạy tại thời điểm nâng cấp: có một trường và hai tài khoản (admin, giảng viên); không có role quản lý cũ cần chuyển.

## Kiểm thử

Các entrypoint cho mô hình mới:

```powershell
dotnet build ObeAunQa.slnx
dotnet test ObeAunQa.slnx --no-build --no-restore
npm --prefix apps/web run build
python tests/contracts/architecture_check.py
python tests/contracts/school_roles_smoke.py
python tests/contracts/school_roles_ui_smoke.py
python tests/contracts/school_migration_smoke.py
```

Các smoke test `school_*` chạy trên API/DB/MinIO dùng riêng; không chạy test ghi dữ liệu vào Docker đang sử dụng. Script migration chỉ đọc DB đang chạy để sao lưu, khôi phục vào DB thử nghiệm rồi nâng cấp và so sánh toàn bộ hàng dữ liệu cũ.

Các script smoke cũ dùng role `faculty_manager`/`department_manager`, quyền sửa điểm công bố trực tiếp hoặc report key thuộc mô hình trước; không dùng chúng làm tiêu chí cho mô hình phân quyền mới. Unit test công thức điểm và OBE vẫn được giữ.

Kết quả đã kiểm tra: 117 unit test .NET, 30 unit test Python AI, 139 kiểm tra HTTP phân quyền/nghiệp vụ, UI trên Edge desktop/mobile và kiểm tra nâng cấp từ bản sao 51 bảng dữ liệu đang chạy. Không gọi nhà cung cấp LLM trả phí; kiểm tra quyền truy xuất và callback thật bằng dữ liệu thử nghiệm.

Biên bản Docker: [15/11/2026](DOCKER_FOUR_ROLES_2026-11-15.md).
