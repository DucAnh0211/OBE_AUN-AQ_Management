# Accreditation

- Owner: SV4.
- Phạm vi: AUN criteria, quản lý file minh chứng, metadata, quyền, phiên bản,
  tìm kiếm danh mục và Gap Analysis.
- Frontend: trang, thành phần, API client và kiểu dữ liệu của Accreditation.
- Backend: domain, application, infrastructure và presentation của Accreditation.
- AI: classification, summarization, gap analysis, prompt và evaluation.

SV4 phát sự kiện sau khi lưu hoặc thay đổi tài liệu, hiển thị trạng thái indexing
và dùng public contract retrieval của SV5. Pipeline parse/chunk/embedding/index
và retrieval nội dung dùng chung thuộc SV5; SV4 không triển khai pipeline riêng.

Public entry points là `frontend/index.ts`, `backend/index.ts` và
`ai/__init__.py`. Module dùng public contract của Curriculum và retrieval của SV5, không truy cập
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

Persistence dùng `AccreditationDbContext`, cấu hình từng entity, EF migrations và
`EfAccreditationRepository`. `AccreditationSeeder` nạp JSON bằng EF khi khung 4.0
chưa tồn tại, giữ nguyên dữ liệu/bản dịch đã chỉnh sửa. Không còn SQL bootstrap.
Xem [hướng dẫn EF Core](../../../docs/EF_CORE_ARCHITECTURE.md).

## Kiểm định theo ngành

`accreditation.program_accreditations` lưu từng đợt kiểm định của một CTĐT: bộ tiêu chí,
kết quả (`self_assessed`, `accredited`, `not_met`), ngày đánh giá, ngày hết hiệu lực, mức
kết luận 1–7 theo Guide AUN-QA v4.0 và ghi chú nguồn. Trạng thái `expired` không lưu mà
tính từ `valid_until`. Một kiểm định còn hiệu lực được ưu tiên hơn đợt tự đánh giá mới hơn.

- `GET /api/accreditation/directory/universities`: danh sách trường kèm số ngành đã kiểm định.
- `GET /api/accreditation/directory/universities/{id}/programs?status=accredited|all`:
  mặc định chỉ trả ngành đang còn hiệu lực kiểm định.
- `GET /api/accreditation/directory/programs/{programId}/accreditations`: lịch sử theo CTĐT.
- `POST /api/accreditation/programs/{programId}/accreditations`, `PUT|DELETE
  /api/accreditation/program-accreditations/{id}`: chỉ admin.

Nhóm `directory` mở cho mọi vai trò đã đổi mật khẩu nhưng vẫn giới hạn theo CTĐT được phân
công. Kết quả kiểm định do admin nhập theo căn cứ; hệ thống không tự suy ra từ SAR.
Kiểm thử luồng này: `tests/contracts/university_directory_smoke.py`.
