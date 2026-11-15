# Cập nhật Docker ngày 15/11/2026

Đã triển khai [bốn role và tài nguyên theo trường](FOUR_ROLES_AND_SCHOOL_SCOPE.md) trên stack `obe-aun-qa`, giao diện `http://localhost:8080`.

## Nội dung

- Role hợp lệ: `admin`, `school_admin`, `lecturer`, `student`.
- Migration `SchoolTenancy` của Curriculum, Identity, Accreditation, Reporting, Knowledge; migration `ChatRoleScope` của Knowledge.
- Tài nguyên học vụ, kiểm định, tài liệu, chỉ mục, báo cáo và hội thoại được lọc theo trường tại backend.
- Điểm đã công bố chỉ cập nhật bằng yêu cầu được phê duyệt, kèm revision, lịch sử và kiểm tra giao dịch.
- Sinh viên được dùng chatbot trong phạm vi tài nguyên được cấp và bảng điểm cá nhân.
- Giao diện chọn trường cho admin tổng; tạo admin trường; quản lý thành viên trường; trang xét duyệt sửa điểm và phạm vi tài liệu.

## Bảo toàn dữ liệu

Sao lưu trước khi áp dụng migration: `tmp/backups/school-live-20261115-165739/obe_management-before.dump`, kèm SHA-256, `before.json`, `after.json`.

Đã khôi phục backup vào cơ sở dữ liệu thử nghiệm, chạy migration và đối chiếu 51 bảng cũ. Trước khi cập nhật thật, API được dừng, tạo backup mới và lấy fingerprint lại. Sau migration, đối chiếu đủ 51 bảng đạt: không thay đổi hàng dữ liệu nghiệp vụ hoặc password hash. Phần được phép thay đổi là metadata trường/phạm vi hội thoại và thu hồi phiên đăng nhập.

Dữ liệu hiện có gồm một trường (`TRUONG-01`, tên “Trường chưa đặt tên”), một admin và một giảng viên. Admin hiện tại giữ role admin tổng; giảng viên và tài nguyên cũ được gắn vào trường này. Không tạo tài khoản thử nghiệm trong cơ sở dữ liệu đang dùng.

## Kiểm chứng

- 117 unit test .NET đạt.
- 30 unit test Python AI đạt, chạy trong image AI và tắt mạng.
- 139 kiểm tra HTTP trên stack thử nghiệm với hai trường độc lập đạt, gồm truy cập chéo trường, giả mạo header, vai trò cũ, quyền sinh viên, đồng thời phê duyệt, từ chối, trạng thái điểm, tài liệu, báo cáo và callback chủ hội thoại.
- UI Edge desktop/mobile đạt: bốn lựa chọn role, selector trường, đề nghị/duyệt sửa điểm, điểm cá nhân, chatbot sinh viên, phạm vi tài liệu; không có JavaScript error hoặc tràn trang ngang.
- Khởi tạo DB mới với dữ liệu demo đạt; toàn bộ 29 bảng tài nguyên Curriculum có trường hợp lệ.
- Đăng nhập qua Docker thật bằng tài khoản admin hiện có đạt; kiểm tra `/auth/me`, `/auth/school`, danh sách trường/CTĐT, yêu cầu sửa điểm, tài liệu, báo cáo đạt.
- UI tại `localhost:8080` đăng nhập, chọn trường và hiển thị đúng bốn role đạt.

Các image `api`, `web`, `ai`, `database-init` đã được dựng lại. API/web/AI và PostgreSQL healthy. MinIO tiếp tục dùng volume hiện có. Image API/web/AI trước thay đổi được giữ tag `before-school-20261115`.

Database runner vẫn có thông báo thiếu thư viện GSSAPI tùy chọn trên image Linux; kết nối bằng mật khẩu và migration hoàn thành với exit code 0. Frontend còn cảnh báo kích thước bundle có sẵn; build thành công.

## Đăng nhập và sử dụng

Đăng nhập lại bằng tài khoản `admin@huce.edu.vn` và mật khẩu hiện tại. Chọn trường ở đầu trang, vào **Quản lý tài khoản** để tạo admin trường. Admin trường tiếp tục tạo thành viên, bộ môn, học phần và tài liệu trong trường của mình. Không có tài khoản admin trường mặc định được tự tạo.

Chưa gọi nhà cung cấp LLM trả phí để đánh giá chất lượng câu trả lời. Đã kiểm tra thực tế quyền nguồn dữ liệu, công cụ cá nhân và callback; các test mô hình dùng phản hồi giả lập.
