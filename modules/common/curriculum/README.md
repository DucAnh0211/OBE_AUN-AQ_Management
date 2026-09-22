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
