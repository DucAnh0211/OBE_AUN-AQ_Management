# AI Service composition root

AI Service duy nhất của hệ thống. Thư mục này chỉ mount capability AI của
Accreditation (SV4) và Reporting (SV5) vào FastAPI/worker dùng chung; không có
capability AI của Curriculum và không tự phê duyệt hoặc sửa dữ liệu nghiệp vụ.

Trạng thái hiện tại: FastAPI health stub chạy qua Docker profile `ai`; chưa có
nghiệp vụ AI hoặc kết nối mô hình.

```powershell
docker compose --profile ai up --build ai
```

Endpoint hiện có:

- `GET /health`
- `GET /capabilities`
