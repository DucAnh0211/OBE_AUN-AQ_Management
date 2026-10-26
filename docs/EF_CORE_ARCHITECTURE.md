# EF Core và tổ chức backend

Cập nhật 26/10/2026 sau khi kết hợp `main` vào `reporting`. Curriculum, Reporting,
Identity và Accreditation đều dùng EF Core 10 với Npgsql; không còn Dapper hoặc
repository thực thi CRUD bằng Npgsql trực tiếp. Chức năng đăng nhập, phân quyền,
phân công học phần và quản lý khung AUN-QA từ `main` được giữ lại.

## Cấu trúc mỗi module

```text
backend/
  domain/Entities/                 Mỗi entity một file, không phụ thuộc EF
  domain/Rules/                    Quy tắc nghiệp vụ độc lập database
  application/DTOs/<nghiệp vụ>/    Mỗi request/response một file
  application/Interfaces/         Mỗi interface một file
  application/Exceptions/         Mỗi exception một file
  application/                    Service và validation
  infrastructure/Repositories/    Repository EF theo nghiệp vụ
  infrastructure/Persistence/     DbContext
    Configurations/               Mỗi entity một IEntityTypeConfiguration
    Migrations/                   EF migration và model snapshot
  presentation/                   Endpoint và đăng ký DI
```

Các interface được chia theo chương trình, phiên bản, học phần, PLO, CLO,
hai loại mapping, phân tích và nguồn báo cáo. Reporting đọc Curriculum qua
`ICurriculumReportSource`, không đọc repository nội bộ của Curriculum.
Application không tham chiếu EF Core hoặc Infrastructure. Cấu hình `UseNpgsql`
được truyền qua DI; không đặt mật khẩu hoặc chuỗi kết nối trong entity/OnConfiguring.

Mỗi module vẫn là **một project .csproj**, chia tầng bằng thư mục/namespace.
Đây chưa phải bốn assembly cưỡng chế chiều phụ thuộc bằng compiler.
`tests/contracts/architecture_check.py` kiểm tra quy tắc phụ thuộc và tách file.

| Module | DbContext | Schema |
|---|---|---|
| Curriculum | CurriculumDbContext | curriculum |
| Reporting | ReportingDbContext | reporting |
| Accreditation | AccreditationDbContext | accreditation |
| Identity | IdentityDbContext | security |

Identity có các read model hẹp để kiểm tra phân công/phiên bản/khung kiểm định;
chúng map sang bảng do module khác sở hữu và được `ExcludeFromMigrations`.
Migration Identity vẫn tạo FK phân công tới Curriculum nhưng không tạo/sửa lại
bảng Curriculum. Service nghiệp vụ dùng interface; DbContext và LINQ nằm ở Infrastructure.

## Database đang có dữ liệu

Sao lưu trước khi nâng cấp. API không tự chạy migration khi khởi động.
Trong PowerShell tại thư mục gốc:

```powershell
$env:ConnectionStrings__Postgres = "Host=localhost;Port=5432;Database=obe_management;Username=postgres;Password=YOUR_PASSWORD"
dotnet run --project tools/database -- baseline-existing
dotnet run --project tools/database -- migrate
```

`baseline-existing` chỉ ghi lịch sử migration nếu schema trùng cấu trúc legacy
đã kiểm chứng trước khi chuyển sang EF Core.
Công cụ đối chiếu cột, default, constraint, index và view trước khi ghi lịch sử.
Schema chưa tồn tại được bỏ qua; `migrate` sẽ tạo schema đó. Schema khác mẫu
bị từ chối, cần kiểm tra chênh lệch; không tự sửa dữ liệu hoặc xóa bảng.
Fingerprint có thể khác khi thay phiên bản PostgreSQL hoặc thay đổi DDL thủ công.

`migrate` từ chối database có bảng nghiệp vụ nhưng chưa có lịch sử EF.
Không chạy thẳng `dotnet ef database update` để bỏ qua kiểm tra này trên database cũ.
Baseline xác nhận điểm xuất phát tương thích; không có nghĩa công cụ tự hòa giải
mọi khác biệt schema cho các migration tương lai.

## Database mới, không cần dữ liệu demo

Tạo database PostgreSQL trống, đặt biến kết nối như trên rồi chạy:

```powershell
dotnet run --project tools/database -- migrate
```

EF tạo bảng, khóa, index và view; seed danh mục X/Y và I/R/E. Không tự nạp
59 học phần demo trừ khi bạn chọn bước seed dưới đây.

`migrate`/`initialize` cũng nạp khung AUN-QA Programme 4.0 (8 criteria, 53 requirements,
tiếng Anh và tiếng Việt) bằng `AccreditationSeeder`. Nội dung và trạng thái bản dịch
được giữ nguyên từ `main`; bản tiếng Việt tham khảo không trở thành bản chính thức.
Khung đã tồn tại không bị ghi đè. Tài khoản admin đầu tiên được API tạo từ
`BootstrapAdmin__*` sau migration và bắt buộc đổi mật khẩu.

## Dữ liệu mẫu V1/V2 bằng EF Core

```powershell
dotnet run --project tools/database -- seed-demo
dotnet run --project tools/database -- verify-demo
```

`CurriculumDemoSeeder` đọc JSON tại
`modules/common/curriculum/backend/infrastructure/Persistence/Seeding/Data`,
tạo entity và gọi `SaveChangesAsync` trong một transaction Serializable.
`CurriculumDemoVerifier` dùng LINQ kiểm tra số lượng, liên kết, quy tắc tín chỉ,
CLO hỗ trợ nhiều PLO và độ cân bằng trước khi commit. Kiểm tra thất bại sẽ rollback.

| Phiên bản | Học phần | PLO | Học phần–PLO | CLO | CLO–PLO |
|---|---:|---:|---:|---:|---:|
| V1 | 59 | 5 | 146 | 146 | 146 |
| V2 | 59 | 5 | 151 | 155 | 210 |

V1 finalized/current; V2 draft và tham chiếu V1. Giữ nguyên nội dung, thang mapping,
tài liệu nguồn và hash lịch sử nhập. Manifest phân biệt hash tài liệu nguồn gốc
với checksum file JSON đóng gói (chuẩn hóa newline LF).

Seeder chỉ nạp khi Curriculum trống. Nếu đã có chương trình, lệnh bỏ qua và
không sửa/ghi đè dữ liệu, kể cả dữ liệu demo đã được người dùng chỉnh sửa.
`verify-demo` dành cho bộ demo nguyên bản; không dùng để đánh giá CTĐT đã chỉnh sửa.

## Docker và vòng đời database

Luồng: PostgreSQL healthy → `database-init` hoàn tất → API → web.
`database-init` chạy `initialize`: kiểm tra/baseline schema legacy nếu cần,
áp dụng EF migrations rồi seed nếu `Database__SeedDemo=true` và Curriculum trống.
Trong Compose, biến `.env` `SEED_DEMO` mặc định true cho demo; đặt false để chỉ migrate.
Lỗi baseline/migration/seed khiến service trả mã lỗi và API chưa khởi động.
API không tự migrate để tránh nhiều instance cùng thay đổi schema.

Toàn bộ SQL rời và shell bootstrap trong `infra/postgres` đã xóa; Compose không
còn mount SQL hoặc gọi psql để tạo bảng/nạp dữ liệu. Các tài liệu cũ tại đó cũng
được thay bằng hướng dẫn này. Tất cả tạo schema, nâng cấp, seed và kiểm tra demo
đều đi qua EF Core và mã C#.

EF Core vẫn sinh SQL để giao tiếp PostgreSQL. Các biểu thức đặc thù PostgreSQL
(expression index/view trong migration, khóa `FOR UPDATE` trong transaction,
đọc system catalog khi tiếp nhận database legacy) vẫn được thực thi qua API EF.
Không đổi cơ chế khóa hay bỏ constraint chỉ để tránh cú pháp SQL nội bộ.
Domain/Application vẫn độc lập EF theo Clean Architecture; React và ASP.NET Core
tiếp tục đảm nhiệm giao diện và HTTP, không phải công nghệ bị EF thay thế.

## Thay đổi model sau này

```powershell
dotnet tool restore
dotnet ef migrations add TenThayDoi --project modules/common/curriculum/backend --context CurriculumDbContext --output-dir infrastructure/Persistence/Migrations --namespace ObeAunQa.Modules.Curriculum.Infrastructure.Migrations
dotnet ef migrations script --idempotent --project modules/common/curriculum/backend --context CurriculumDbContext --output tmp/curriculum-migration.sql
```

Với Reporting, Identity hoặc Accreditation, thay project và context tương ứng. Review migration sinh ra,
đặc biệt thao tác xóa/đổi cột, rồi kiểm thử trên bản sao database trước khi áp dụng.
Không sửa initial migration đã áp dụng để nâng cấp database đang sử dụng.

## Hành vi và kiểm tra

Giữ API/JSON, trạng thái phiên bản, xóa mềm, thang X/Y và I/R/E; clone phiên bản
vẫn sao chép học phần, đồng thời sao chép phân công giảng viên như chức năng trên `main`.
Snapshot báo cáo lưu riêng trong JSONB,
không thay đổi khi sửa nguồn; lưu báo cáo và audit trong cùng transaction.

```powershell
dotnet test ObeAunQa.slnx
python tests/contracts/architecture_check.py
npm.cmd --prefix apps/web run build
```

Các smoke test `curriculum_ef_smoke.py`, `reporting_api_smoke.py`,
`reporting_snapshot_ef_smoke.py`, `database_ef_smoke.py` chạy trên database/API thử nghiệm riêng;
đọc biến môi trường trong đầu file trước khi chạy. Không trỏ chúng vào dữ liệu thật.
Đã kiểm thử migration mới, baseline legacy, từ chối schema lệch, CRUD, quyền,
snapshot và xuất báo cáo trên PostgreSQL 17 riêng. Database đang sử dụng của
người dùng chưa được nâng cấp trong lần sửa source này.

Đã đối chiếu 13 nhóm dữ liệu giữa seed EF và seed legacy trên database thử nghiệm:
khớp toàn bộ nội dung và tham chiếu nghiệp vụ, bỏ qua ID tự sinh/timestamp.

Kiểm tra bản kết hợp: 42 unit/model tests; `merged_modules_ef_smoke.py` kiểm tra
đăng nhập, đổi mật khẩu, quyền admin/lecturer/student, phân công, clone, thu hồi JWT,
refresh token, CRUD/import/publish/retire/bản dịch của Accreditation và quyền Reporting.
Smoke test Curriculum và snapshot cần JWT admin trong biến môi trường được ghi đầu file.
Không chạy chúng trên database thật. Database người dùng chưa bị thay đổi.

Admin đăng nhập bằng JWT được dùng Reporting cho các chương trình; lecturer/student
không có quyền Reporting theo phạm vi hiện tại trên `main`. Mã báo cáo cũ vẫn dùng
scheme riêng chỉ có quyền trên chương trình được cấu hình; không cấp quyền vào
Curriculum/Identity/Accreditation. Giao diện admin tự dùng phiên đăng nhập chung,
không yêu cầu cấp thêm mã báo cáo.
