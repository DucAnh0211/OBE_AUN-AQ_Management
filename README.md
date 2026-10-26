# OBE & AUN-QA Management

Hệ thống quản lý Outcome-Based Education (OBE), chương trình đào tạo, minh chứng
kiểm định AUN-QA và báo cáo có căn cứ.

## Trạng thái hiện tại

Repository đã được khởi tạo thành ứng dụng chạy được:

- Backend: ASP.NET Core 10 modular monolith.
- Frontend: React 19, TypeScript và Vite.
- Database: PostgreSQL qua EF Core; migrations nằm trong từng module.
- Các module: Curriculum, Accreditation và Reporting.

Backend đã kết nối PostgreSQL và cung cấp CRUD cho CTĐT, phiên bản CTĐT, học
phần, PLO, CLO và các mapping học phần–PLO/CLO–PLO. API kiểm tra quy tắc số PLO
theo tín chỉ và độ cân bằng PLO cũng đã sẵn sàng. Reporting đã có UC25–UC27
rule-based: xem trước, lịch sử, snapshot, quyền theo chương trình và xuất DOCX/PDF.
Xem [hướng dẫn chạy UC25–UC27](docs/UC25_UC27_RULE_BASED.md) để áp dụng migration
và cấp mã truy cập. Accreditation, pipeline minh chứng và UC28–UC30 tiếp tục
được phát triển ở các bước sau.

## Yêu cầu môi trường

- .NET SDK 10.0.401 trở lên trong dòng 10.0.
- Node.js 24 trở lên và npm.
- PostgreSQL 17 cho phần dữ liệu đã seed.

## Chạy backend

Từ thư mục gốc repository:

Chuẩn bị database theo [hướng dẫn EF Core](docs/EF_CORE_ARCHITECTURE.md):
database cũ cần baseline trước khi migrate; database trống chỉ cần migrate.

```powershell
dotnet build ObeAunQa.slnx
$env:ConnectionStrings__Postgres = "Host=localhost;Port=5432;Database=obe_management;Username=postgres;Password=YOUR_PASSWORD"
dotnet run --project apps/api/ObeAunQa.Api.csproj --launch-profile http
```

API chạy tại `http://localhost:5099`. Các endpoint kiểm tra:

- `GET /health`
- `GET /api/modules`
- `GET /api/curriculum/health`
- `GET /api/accreditation/health`
- `GET /api/reporting/health`

Swagger UI: `http://localhost:5099/swagger`. OpenAPI JSON:
`http://localhost:5099/openapi/v1.json`. Các request mẫu nằm trong
`apps/api/ObeAunQa.Api.http`.

## Chạy frontend

Mở một terminal khác:

```powershell
Set-Location apps/web
npm install
npm run dev
```

Web chạy tại `http://localhost:5173` và mặc định gọi API tại
`http://localhost:5099`. Sao chép `apps/web/.env.example` thành
`apps/web/.env.local` nếu cần thay địa chỉ API.

Mở `http://localhost:5173/curriculum` để quản lý CTĐT, phiên bản và học phần
bằng giao diện tiếng Việt.

## Chạy toàn bộ bằng Docker

```powershell
Copy-Item .env.example .env
docker compose up --build
```

Docker Compose chạy PostgreSQL, áp dụng EF migrations và seed V1/V2 bằng C#, khởi động API và phục vụ frontend
qua Nginx tại `http://localhost:8080`. Xem hướng dẫn đầy đủ tại
[`docs/DOCKER.md`](docs/DOCKER.md).

## Kiến trúc

```text
apps/
  api/                         ASP.NET Core composition root
  web/                         React composition root
modules/
  common/curriculum/           CTĐT, học phần, PLO, CLO, mapping
  sv4/accreditation/           AUN-QA, file/quyền/phiên bản minh chứng, gap analysis
  sv5/reporting/               Ingestion/retrieval dùng chung, chat, báo cáo, export
shared/
  backend/                     Contract kỹ thuật dùng chung
  frontend/                    Type dùng chung
tools/database/                EF migration runner và kiểm tra baseline database cũ
```

Các thư mục trong `apps` chỉ ghép module và cấu hình host. Business logic nằm
trong module tương ứng; module khác chỉ truy cập public contract của nhau.

## Build kiểm tra

```powershell
dotnet build ObeAunQa.slnx
dotnet test ObeAunQa.slnx

Set-Location apps/web
npm run build
```
