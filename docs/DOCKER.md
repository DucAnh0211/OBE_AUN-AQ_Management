# Chạy OBE & AUN-QA Management bằng Docker

## Yêu cầu

- Docker Desktop đang chạy.
- Docker Compose v2 trở lên.
- Các cổng mặc định `8080`, `5099`, `5432` và `8000` chưa bị ứng dụng khác sử dụng.

Nếu PostgreSQL trên máy đang dùng cổng `5432`, đổi `POSTGRES_PORT` trong `.env`, ví dụ `5433`. Các container vẫn kết nối nội bộ qua cổng `5432`.

## Khởi động lần đầu

Từ thư mục gốc repository:

```powershell
Copy-Item .env.example .env
docker compose up --build
```

Hoặc chạy ngay bằng cấu hình demo trong `.env.example` mà không cần tạo `.env`:

```powershell
docker compose --env-file .env.example up --build --detach
```

Tài khoản đăng nhập demo lấy từ `BOOTSTRAP_ADMIN_EMAIL` và
`BOOTSTRAP_ADMIN_PASSWORD`. Hệ thống yêu cầu đổi mật khẩu ngay lần đăng nhập đầu.

Đổi `POSTGRES_PASSWORD` trong `.env` trước khi chia sẻ hoặc dùng ngoài máy cá nhân.

Docker sẽ tự động:

1. Tạo database `obe_management`.
2. Tạo schema Curriculum.
3. Nạp V1 với 59 học phần.
4. Áp dụng migration CRUD.
5. Nạp V2 với dữ liệu PLO-CLO đã cân bằng.
6. Chạy các truy vấn kiểm tra.
7. Chạy service `migrate` để bảo đảm schema phân quyền tồn tại khi dùng lại volume cũ.
8. Khởi động API rồi mới khởi động web.

Các địa chỉ mặc định:

| Thành phần | Địa chỉ |
|---|---|
| Web | <http://localhost:8080> |
| API readiness | <http://localhost:5099/health/ready> |
| PostgreSQL | `localhost:5432` |

## Các lệnh thường dùng

Chạy nền:

```powershell
docker compose up --build --detach
docker compose ps
```

Xem log:

```powershell
docker compose logs --follow
docker compose logs --follow db
docker compose logs migrate
docker compose logs --follow api
```

Dừng container và giữ dữ liệu:

```powershell
docker compose down
```

Khởi động lại với dữ liệu cũ:

```powershell
docker compose up --detach
```

Xóa database và seed lại hoàn toàn:

```powershell
docker compose down --volumes
docker compose up --build
```

Lệnh `down --volumes` xóa dữ liệu PostgreSQL trong Docker. Chỉ dùng khi muốn tạo database lại từ đầu.

## AI service tùy chọn

AI service hiện cung cấp health và danh sách capability, chưa gọi mô hình AI:

```powershell
docker compose --profile ai up --build --detach
Invoke-RestMethod http://localhost:8000/health
Invoke-RestMethod http://localhost:8000/capabilities
```

Tắt cả stack, bao gồm profile AI:

```powershell
docker compose --profile ai down
```

## Kiểm tra hệ thống

```powershell
Invoke-RestMethod http://localhost:5099/health/live
Invoke-RestMethod http://localhost:5099/health/ready
Invoke-WebRequest http://localhost:8080
```

Các API nghiệp vụ yêu cầu đăng nhập. Kiểm tra nhanh qua giao diện web tại
<http://localhost:8080>.

Kiểm tra số lượng V1 và V2 trong PostgreSQL:

```powershell
docker compose exec db psql `
  --username postgres `
  --dbname obe_management `
  --command "SELECT version_code, dataset_status FROM curriculum.program_versions ORDER BY id;"
```

Nếu đã đổi `POSTGRES_USER` hoặc `POSTGRES_DB`, dùng giá trị tương ứng trong lệnh.

## Lưu trữ dữ liệu

PostgreSQL sử dụng named volume `postgres_data`. Dữ liệu vẫn còn sau `restart` hoặc `down`.

Tạo bản sao lưu:

```powershell
docker compose exec db pg_dump `
  --username postgres `
  --dbname obe_management `
  --format custom `
  --file /tmp/obe_management.dump

docker compose cp db:/tmp/obe_management.dump ./obe_management.dump
```

## Xử lý lỗi

### Cổng đã được sử dụng

Sửa cổng host trong `.env`:

```dotenv
POSTGRES_PORT=5433
API_PORT=5100
WEB_PORT=8081
```

### API chưa chạy

```powershell
docker compose ps
docker compose logs db
docker compose logs api
```

API chỉ khởi động sau khi database đã seed xong và chuyển sang `healthy`.

### Đổi mật khẩu sau khi đã tạo volume

`POSTGRES_PASSWORD` chỉ được PostgreSQL dùng để tạo mật khẩu ở lần khởi tạo
volume đầu tiên. Nếu đổi giá trị này trong `.env` khi volume đã có dữ liệu, API
sẽ báo `password authentication failed`.

Nếu đây là dữ liệu demo có thể tạo lại, seed lại volume bằng các lệnh ở mục
"Muốn chạy lại bootstrap" bên dưới. Nếu cần giữ dữ liệu, cập nhật mật khẩu của
role trong PostgreSQL cho trùng với `.env`, rồi tạo lại container API:

```powershell
docker compose exec --user postgres db psql `
  --dbname postgres `
  --command "ALTER ROLE postgres WITH PASSWORD 'MAT_KHAU_MOI';"
docker compose up --detach --force-recreate api web
```

### Muốn chạy lại bootstrap

Các script bootstrap chỉ chạy khi volume PostgreSQL còn trống:

```powershell
docker compose down --volumes
docker compose up --build
```

### Thay đổi seed hoặc migration

Trong môi trường demo, tạo lại volume để áp dụng toàn bộ bootstrap. Khi triển khai production cần dùng migration runner riêng thay vì xóa volume.
