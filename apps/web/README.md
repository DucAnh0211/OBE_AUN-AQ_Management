# Web App

React/Vite composition root dùng chung cho ba module Curriculum, Accreditation
và Reporting.

## Chạy development

```powershell
Set-Location apps/web
npm install
npm run dev
```

Mở `http://localhost:5173`.

Giao diện quản trị CTĐT sử dụng luồng phân cấp:

- `/curriculum`: chương trình đào tạo.
- `/curriculum/programs/{programId}`: phiên bản CTĐT.
- `/curriculum/versions/{versionId}`: học phần trong phiên bản.

Backend phải chạy tại địa chỉ khai báo trong `VITE_API_URL`. Các thao tác tạo,
sửa, lưu trữ và công bố gọi trực tiếp API Curriculum.

## Kiểm tra production build

```powershell
npm run build
npm run preview
```

Biến `VITE_API_URL` xác định địa chỉ backend. Giá trị mặc định là
`http://localhost:5099`; xem `.env.example`.
