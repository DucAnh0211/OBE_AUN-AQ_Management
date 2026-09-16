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

## Kiểm tra production build

```powershell
npm run build
npm run preview
```

Biến `VITE_API_URL` xác định địa chỉ backend. Giá trị mặc định là
`http://localhost:5099`; xem `.env.example`.
