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

Trạng thái hiện tại: scaffold-only, chưa có business code hoặc dependency.
