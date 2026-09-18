# Nạp dữ liệu PLO–CLO 59 môn vào PostgreSQL

Xem hướng dẫn đầy đủ để dựng lại database, seed V1 và sinh dữ liệu PLO–CLO V2 tại [HUONG_DAN_SINH_DU_LIEU_DATABASE.md](HUONG_DAN_SINH_DU_LIEU_DATABASE.md).

Các lệnh dưới đây chạy trong PowerShell tại thư mục gốc của repository. Cần một
PostgreSQL server đang chạy và `psql` có trong `PATH`. Khi dùng `-U postgres`,
`psql` sẽ hỏi mật khẩu; không ghi mật khẩu vào lệnh hoặc tệp SQL.

## 1. Tạo database một lần

```powershell
psql --version
psql -h localhost -U postgres -d postgres -c "CREATE DATABASE obe_management;"
```

Nếu database `obe_management` đã tồn tại, bỏ qua lệnh `CREATE DATABASE`.

## 2. Tạo schema Curriculum

```powershell
psql -h localhost -U postgres -d obe_management -v ON_ERROR_STOP=1 -f infra/postgres/001_curriculum.sql
```

Schema gồm CTĐT, phiên bản, môn, PLO, CLO, hai bảng nối, tài liệu nguồn và lần
nhập. `course_plos` và `clo_plos` có khóa ngoại ghép để không nối chéo môn hoặc
chéo phiên bản CTĐT.

## 3. Kiểm tra JSON và sinh seed SQL

```powershell
python scripts/generate_plo_clo_seed.py
```

Script kiểm tra 5 PLO, 59 môn, quy tắc tín chỉ cho 34 môn mới và mọi PLO có CLO.
Script tạo `outputs/plo_clo_full_20260915/seed_plo_clo_59_mon.sql`; PostgreSQL
không cần quyền đọc tệp JSON trên máy của bạn. Seed SQL chứa nội dung JSON UTF-8
và SHA-256 của JSON, Excel, DOCX để truy vết lần nhập.

## 4. Nạp seed trong một transaction

```powershell
psql -h localhost -U postgres -d obe_management -v ON_ERROR_STOP=1 -f outputs/plo_clo_full_20260915/seed_plo_clo_59_mon.sql
```

Lệnh tạo/ cập nhật dữ liệu theo khóa duy nhất, rồi kiểm tra số bản ghi trước
`COMMIT`: 5 PLO, 59 môn, 59 dòng gốc, 146 liên kết môn–PLO, 146 CLO và 146 liên
kết CLO–PLO. Nếu kiểm tra thất bại, transaction bị hủy. Chạy lại cùng seed không
tạo bản ghi trùng.

## 5. Bổ sung schema phục vụ CRUD API

```powershell
psql -h localhost -U postgres -d obe_management -v ON_ERROR_STOP=1 -f infra/postgres/004_curriculum_crud.sql
```

Migration bổ sung trạng thái phiên bản, xóa mềm, timestamps và snapshot thông tin
học phần. Migration không sửa bảng hoặc dữ liệu PLO, CLO và mapping; có thể chạy
lại an toàn.

## 6. Xem kết quả

```powershell
psql -h localhost -U postgres -d obe_management -v ON_ERROR_STOP=1 -f infra/postgres/003_verify_curriculum.sql
```

Truy vấn đếm phải đúng `expected`; hai truy vấn lỗi liên kết và lỗi quy tắc tín
chỉ của môn mới phải trả về 0 dòng. Điểm cân bằng hiện tại là PLO1=203,
PLO2=157, PLO3=100, PLO4=181, PLO5=117; PLO1, PLO3, PLO5 vượt ngưỡng 20%.
Truy vấn cuối giữ đúng bốn môn nguồn dưới ngưỡng tín chỉ ở dòng 6, 7, 8, 13.

Tệp Excel/DOCX không được lưu binary trong PostgreSQL. Bảng `source_documents`
chỉ giữ tên và hash; khi có MinIO thì cập nhật `object_key` sau khi tải tệp lên.

Schema, seed và migration CRUD đã được kiểm tra trên PostgreSQL 17 với dữ liệu V1.

## 7. Sinh lại PLO/CLO cho V2

Sinh JSON, báo cáo kiểm tra và seed SQL từ dữ liệu V1:

```powershell
python .\scripts\regenerate_plo_clo_v2.py
```

Nạp V2 vào database; lệnh sẽ yêu cầu nhập mật khẩu PostgreSQL:

```powershell
& "F:\Postgre17\bin\psql.exe" `
  -h localhost `
  -p 5432 `
  -U postgres `
  -d obe_management `
  -W `
  -v ON_ERROR_STOP=1 `
  -f .\outputs\plo_clo_v2_20260917\seed_plo_clo_v2.sql
```

Seed tạo V2 ở trạng thái `draft`, sao chép 59 học phần từ V1 và thay thế toàn
bộ PLO/CLO/mapping của riêng V2 trong một transaction. V1 không bị thay đổi.

Kiểm tra dữ liệu sau khi seed:

```powershell
& "F:\Postgre17\bin\psql.exe" `
  -h localhost -p 5432 -U postgres -d obe_management -W `
  -v ON_ERROR_STOP=1 `
  -f .\infra\postgres\006_verify_plo_clo_v2.sql
```
