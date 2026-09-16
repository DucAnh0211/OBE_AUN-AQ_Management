# OBE & AUN-QA Management

Hệ thống quản lý Outcome-Based Education (OBE), chương trình đào tạo, minh chứng
kiểm định AUN-QA và báo cáo có căn cứ.

## Trạng thái hiện tại

Repository đã được khởi tạo thành ứng dụng chạy được:

- Backend: ASP.NET Core 10 modular monolith.
- Frontend: React 19, TypeScript và Vite.
- Database: PostgreSQL; schema và seed CTĐT V1 nằm trong `infra/postgres`.
- Các module: Curriculum, Accreditation và Reporting.

Backend hiện cung cấp health check, module registry và endpoint khung của từng
module. Kết nối database và CRUD nghiệp vụ sẽ được triển khai ở các bước tiếp theo.

## Yêu cầu môi trường

- .NET SDK 10.0.401 trở lên trong dòng 10.0.
- Node.js 24 trở lên và npm.
- PostgreSQL 17 cho phần dữ liệu đã seed.

## Chạy backend

Từ thư mục gốc repository:

```powershell
dotnet build ObeAunQa.slnx
dotnet run --project apps/api/ObeAunQa.Api.csproj --launch-profile http
```

API chạy tại `http://localhost:5099`. Các endpoint kiểm tra:

- `GET /health`
- `GET /api/modules`
- `GET /api/curriculum/health`
- `GET /api/accreditation/health`
- `GET /api/reporting/health`

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

## Kiến trúc

```text
apps/
  api/                         ASP.NET Core composition root
  web/                         React composition root
modules/
  common/curriculum/           CTĐT, học phần, PLO, CLO, mapping
  sv4/accreditation/           AUN-QA, minh chứng, ingestion, gap analysis
  sv5/reporting/               Chat, trích dẫn, báo cáo, export
shared/
  backend/                     Contract kỹ thuật dùng chung
  frontend/                    Type dùng chung
infra/postgres/                Schema, import template và kiểm tra dữ liệu
```

Các thư mục trong `apps` chỉ ghép module và cấu hình host. Business logic nằm
trong module tương ứng; module khác chỉ truy cập public contract của nhau.

## Build kiểm tra

```powershell
dotnet build ObeAunQa.slnx

Set-Location apps/web
npm run build
```
