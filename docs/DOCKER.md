# Chạy OBE & AUN-QA Management bằng Docker

## Yêu cầu

- Docker Desktop đang chạy.
- Docker Compose v2 trở lên.
- Các cổng mặc định `8080`, `5099`, `5432` và `8000` chưa bị ứng dụng khác sử dụng.

Nếu PostgreSQL trên máy đang dùng cổng `5432`, đổi `POSTGRES_PORT` trong `.env`, ví dụ `5433`. Các container vẫn kết nối nội bộ qua cổng `5432`.

## Khởi động lần đầu

Docker dùng EF Core cho toàn bộ vòng đời dữ liệu. Service `database-init` chạy
migrations và seed bằng C# trước khi API khởi động. Không còn mount SQL/init shell.
Xem [hướng dẫn EF Core](EF_CORE_ARCHITECTURE.md).

Từ thư mục gốc repository:

```powershell
Copy-Item .env.example .env
docker compose up --build
```

Đổi `POSTGRES_PASSWORD` trong `.env` trước khi chia sẻ hoặc dùng ngoài máy cá nhân.

Docker sẽ tự động:

1. PostgreSQL tạo database `obe_management` và báo healthy.
2. `database-init` kiểm tra schema cũ, ghi baseline nếu khớp và chạy EF migrations.
3. Nếu `SEED_DEMO=true` và Curriculum trống, nạp V1/V2 bằng EF rồi kiểm tra trước commit.
4. API chỉ khởi động khi `database-init` thoát thành công; web chờ API healthy.

Đặt `SEED_DEMO=false` trong `.env` nếu không muốn dữ liệu mẫu. Database đã có chương
trình sẽ được giữ nguyên; chạy lại không reset seed. Schema cũ khác mẫu baseline sẽ
bị từ chối, cần kiểm tra chênh lệch trước khi nâng cấp. Không xóa volume để sửa lỗi này.

Các địa chỉ mặc định:

| Thành phần | Địa chỉ |
|---|---|
| Web | <http://localhost:8080> |
| Swagger | <http://localhost:5099/swagger> |
| Swagger qua web proxy | <http://localhost:8080/swagger> |
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
Invoke-RestMethod http://localhost:5099/api/curriculum/programs
Invoke-WebRequest http://localhost:8080
Invoke-WebRequest http://localhost:8080/swagger
```

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

API chỉ khởi động sau khi `database-init` hoàn tất thành công. Xem log bằng `docker compose logs database-init`.

### Đổi mật khẩu sau khi đã tạo volume

`POSTGRES_PASSWORD` chỉ được PostgreSQL dùng để tạo mật khẩu ở lần khởi tạo
volume đầu tiên. Nếu đổi giá trị này trong `.env` khi volume đã có dữ liệu, API
sẽ báo `password authentication failed`.

Nếu đây là dữ liệu demo có thể tạo lại, seed lại volume bằng các lệnh ở mục
"Tạo lại demo" bên dưới. Nếu cần giữ dữ liệu, cập nhật mật khẩu của
role trong PostgreSQL cho trùng với `.env`, rồi tạo lại container API:

```powershell
docker compose exec --user postgres db psql `
  --dbname postgres `
  --command "ALTER ROLE postgres WITH PASSWORD 'MAT_KHAU_MOI';"
docker compose up --detach --force-recreate api web
```

### Tạo lại demo

Chỉ dành cho dữ liệu demo có thể bỏ: các lệnh sau xóa volume, tạo lại schema bằng EF và seed mới:

```powershell
docker compose down --volumes
docker compose up --build
```

### Thay đổi seed hoặc migration

Thêm EF migration, build lại rồi chạy `docker compose up --build --detach`. Service `database-init` áp dụng migration còn thiếu và không ghi đè dữ liệu. Để chạy lại riêng bước nâng cấp: `docker compose run --rm database-init initialize`. Seed chỉ dành cho database trống; thay đổi JSON seed không tự sửa dữ liệu đã có.
