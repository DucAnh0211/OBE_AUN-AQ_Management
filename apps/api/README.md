# Backend API

ASP.NET Core API là composition root duy nhất của backend modular monolith.

## Chạy

Từ thư mục gốc repository:

```powershell
dotnet run --project apps/api/ObeAunQa.Api.csproj --launch-profile http
```

Địa chỉ mặc định: `http://localhost:5099`.

`Program.cs` chỉ đăng ký CORS, health check và public entry point của ba module.
Entity, use case, repository và endpoint nghiệp vụ phải nằm trong thư mục module.

Connection string PostgreSQL được đọc từ `ConnectionStrings:Postgres`. Khi cần
ghi đè bằng biến môi trường trong PowerShell:

```powershell
$env:ConnectionStrings__Postgres = "Host=localhost;Port=5432;Database=obe_aunqa;Username=postgres;Password=YOUR_PASSWORD"
```
