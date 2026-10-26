# AI Service composition root

AI Service duy nhất của hệ thống. Thư mục này chỉ mount capability AI của
Accreditation (SV4) và Reporting (SV5) vào FastAPI/worker dùng chung; không có
capability AI của Curriculum và không tự phê duyệt hoặc sửa dữ liệu nghiệp vụ.

Phân công: SV4 làm AI phân loại, tóm tắt và gợi ý minh chứng; SV5 sở hữu pipeline
parse/chunk/embedding/index và retrieval dùng chung, AI báo cáo và chatbot.
SV4 sử dụng retrieval qua public contract của SV5; file gốc, quyền và phiên bản
vẫn do backend Accreditation quản lý. Danh sách capabilities là phạm vi dự kiến,
không phải bằng chứng chức năng đã hoạt động.

Trạng thái hiện tại: FastAPI health stub chạy qua Docker profile `ai`; chưa có
nghiệp vụ AI hoặc kết nối mô hình.

```powershell
docker compose --profile ai up --build ai
```

Endpoint hiện có:

- `GET /health`
- `GET /capabilities`
