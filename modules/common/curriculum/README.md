# Curriculum

- Phạm vi trách nhiệm: dùng chung.
- Đồng sở hữu: SV4 và SV5.
- Nghiệp vụ: CTĐT, phiên bản chương trình, học phần, PLO/CLO, mức Bloom, mapping
  CLO–PLO, so sánh phiên bản và xuất ma trận.
- Frontend: trang, thành phần, API client và kiểu dữ liệu của Curriculum.
- Backend: domain, application, infrastructure và presentation của Curriculum.
- AI: không thuộc phạm vi module này theo kế hoạch hiện tại.

Public entry points là `frontend/index.ts` và `backend/index.ts`. Thay đổi public
contract cần được cả SV4 và SV5 review; module khác không import implementation
nội bộ.

Trạng thái hiện tại: đã có API cho CTĐT, phiên bản, học phần, PLO, CLO, mapping
học phần–PLO, mapping CLO–PLO, kiểm tra số PLO theo tín chỉ và điểm cân bằng PLO.

Các endpoint PLO/CLO chính:

- `GET|POST /api/curriculum/program-versions/{versionId}/plos`
- `GET|PUT|DELETE /api/curriculum/program-versions/{versionId}/plos/{ploId}`
- `GET|POST /api/curriculum/program-versions/{versionId}/courses/{programCourseId}/clos`
- `GET|PUT|DELETE /api/curriculum/program-versions/{versionId}/courses/{programCourseId}/clos/{cloId}`
- `GET|POST /api/curriculum/program-versions/{versionId}/courses/{programCourseId}/plo-mappings`
- `PUT|DELETE /api/curriculum/program-versions/{versionId}/courses/{programCourseId}/plo-mappings/{mappingId}`
- `GET|POST /api/curriculum/program-versions/{versionId}/courses/{programCourseId}/clos/{cloId}/plo-mappings`
- `DELETE /api/curriculum/program-versions/{versionId}/courses/{programCourseId}/clos/{cloId}/plo-mappings/{coursePloId}`
- `GET /api/curriculum/program-versions/{versionId}/plo-credit-check`
- `GET /api/curriculum/program-versions/{versionId}/plo-balance`

Các thao tác tạo, sửa và xóa PLO/CLO/mapping chỉ áp dụng cho phiên bản `draft`.

Persistence dùng EF Core với entity/configuration riêng từng file, DTO chia theo
nghiệp vụ và repository/interface riêng cho từng nhóm thao tác.
Xem [kiến trúc và migration EF Core](../../../docs/EF_CORE_ARCHITECTURE.md).

## Nhiều trường đại học

`curriculum.universities` là danh mục trường. Mỗi CTĐT và học phần thuộc đúng một trường,
nên mã CTĐT (`programs.code`) và mã học phần (`courses.institutional_code`) chỉ duy nhất
trong một trường: hai trường có thể cùng có CTĐT `CS` hoặc học phần `CS101` mà không dùng
chung dữ liệu. Tạo CTĐT bắt buộc gửi `universityId`.

- `GET|POST /api/curriculum/universities`
- `GET|PUT|DELETE /api/curriculum/universities/{universityId}`
- `GET /api/curriculum/programs?universityId=`

Admin xem mọi trường. Giảng viên và sinh viên chỉ thấy trường có CTĐT trong phạm vi được
phân công; quyền đọc CTĐT không đổi. Chỉ lưu trữ được trường khi không còn CTĐT đang hoạt
động. Migration `AddUniversities` gán dữ liệu cũ vào trường `TRUONG-01` ("Trường chưa đặt
tên"); admin đổi tên trên giao diện. Database trống không tạo trường này từ migration.
