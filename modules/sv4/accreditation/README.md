# Accreditation

- Owner: SV4.
- Phạm vi: AUN criteria, Evidence, Ingestion, Retrieval và Gap Analysis.
- Frontend: trang, thành phần, API client và kiểu dữ liệu của Accreditation.
- Backend: domain, application, infrastructure và presentation của Accreditation.
- AI: ingestion, retrieval, classification, gap analysis, prompt và evaluation.

Public entry points là `frontend/index.ts`, `backend/index.ts` và
`ai/__init__.py`. Module chỉ dùng public contract của Curriculum, không truy cập
implementation nội bộ.

UC19 backend quản lý phiên bản khung AUN-QA, criteria và requirements. Module
có API xem cây, import/preview JSON, quản lý draft, publish và retire. Guide
AUN-QA v4.0 chính thức là nguồn cấu trúc chuẩn; SAR Khoa học máy tính chỉ là
nguồn tham khảo tiếng Việt và hồ sơ chương trình.

Seed AUN-QA v4.0 gồm đủ 8 criteria và 53 statement tiếng Anh chính thức, lấy từ
Appendix A (trang in 63–67) của *Guide to AUN-QA Assessment at Programme Level
Version 4.0*, tháng 10/2020. Bản dịch tiếng Việt được quản lý độc lập và không
được đánh dấu `official` nếu chưa qua xác minh.

Seed tiếng Việt gồm đủ 53 statement tham khảo từ SAR Khoa học máy tính do đơn
vị cung cấp. Các statement này có trạng thái `draft`; riêng 5.8 được lấy từ phần
thân SAR vì mục lục của tài liệu bỏ sót dòng này.
