# Identity

Module dùng chung cho đăng nhập JWT, refresh token, đổi/reset mật khẩu, quản lý
admin/lecturer/student, phân công học phần/CTĐT và audit.

`IdentityDbContext` quản lý schema `security`. Entity, DTO, interface và cấu hình
mapping ở file riêng. `IdentityService` dùng `IIdentityRepository`,
`IIdentityTokenService` và `IIdentityPasswordHasher`; phần EF và bảo mật framework
nằm trong Infrastructure. `AccessControlService` kiểm tra quyền bằng LINQ.

Các read model từ Curriculum/Accreditation chỉ phục vụ kiểm tra quyền và phân công;
không thuộc migration Identity. Khi clone phiên bản, `IAssignmentCloneService`
sao chép phân công giảng viên sang các học phần tương ứng.

Migration chạy trước API bằng `tools/database`. Bootstrap admin lấy cấu hình môi
trường và yêu cầu đổi mật khẩu lần đầu. Không có tài khoản/mật khẩu seed cố định.
Xem [hướng dẫn EF Core](../../../docs/EF_CORE_ARCHITECTURE.md).
