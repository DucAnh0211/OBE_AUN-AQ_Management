# Học vụ: sinh viên, giảng viên, học phần, lớp học phần và điểm

Toàn bộ nghiệp vụ nằm ở `modules/common/curriculum` (entities, DTO/interface, service, EF repository, import reader, UI trong `frontend/pages`). `apps/web` chỉ đăng ký route và nối nút điều hướng. SV4/SV5 không chứa nghiệp vụ điểm; không dùng AI để xử lý điểm. Sơ đồ dữ liệu: [docs/erd/academic-records.md](erd/academic-records.md).

Mô hình bám theo cách các trường đại học Việt Nam vận hành và **Thông tư 08/2021/TT-BGDĐT** (quy chế đào tạo trình độ đại học). Các con số quy chế là **mặc định có thể chỉnh** theo quy chế riêng của từng trường.

## Thuật ngữ

| Giao diện | Bảng | Ý nghĩa |
| --- | --- | --- |
| Khoa / Bộ môn | `org_units` | Đơn vị tổ chức. Khoa quản lý ngành (CTĐT); bộ môn thuộc khoa, quản lý học phần và giảng viên. |
| Học phần | `program_courses` (+ `courses`) | Học phần trong phiên bản CTĐT: tín chỉ, CLO, ánh xạ PLO, **đề cương đánh giá**. |
| Lớp học phần | `course_offerings` | Học phần mở trong một học kỳ: mã 6 số, nhóm lớp (N01, N02…), sĩ số, giảng viên, sổ điểm riêng. |
| Lớp sinh hoạt | `home_classes` | Lớp hành chính theo khóa (ví dụ KHMT01-K66), cố định suốt khóa, có cố vấn học tập. |
| Sinh viên | `academic_students` | MSSV duy nhất trong trường, khóa, lớp sinh hoạt, tình trạng học tập, tài khoản đăng nhập (tùy chọn). |
| Giảng viên | `lecturer_profiles` + tài khoản `lecturer` | Mã giảng viên, bộ môn, học hàm (GS/PGS), học vị (CN/KS/ThS/TS/TSKH). |

## Luồng sử dụng

1. **Trường → Khoa / Bộ môn**: khai báo khoa, bộ môn trực thuộc; gán khoa quản lý cho từng CTĐT.
2. **Trường → Quy chế & thang điểm**: làm tròn điểm học phần (mặc định 1 chữ số), GPA (2 chữ số), số đầu điểm tối thiểu (2; học phần dưới 2 tín chỉ cho phép 1), trọng số cuối kỳ tối thiểu (để trống nếu trường không quy định), cách lấy điểm khi học lại/cải thiện (cao nhất hoặc gần nhất), thang điểm chữ ↔ hệ 4 (mặc định A 8,5 · B 7,0 · C 5,5 · D 4,0 · F; có mẫu chi tiết A+ … F).
3. **Quản trị → Hồ sơ giảng viên**: bổ sung mã GV, bộ môn, học hàm, học vị cho tài khoản giảng viên.
4. **Học phần → Đề cương đánh giá**: khai báo đầu điểm (mã, tên, loại: chuyên cần/quá trình/giữa kỳ/thực hành/đồ án/cuối kỳ, trọng số, điểm tối đa) và **% đóng góp của từng đầu điểm vào từng CLO**. Hệ thống kiểm tra tổng 100%, số đầu điểm tối thiểu, trọng số cuối kỳ theo quy chế; cảnh báo CLO chưa được đánh giá và đầu điểm chưa gắn CLO (cũng hiện ở trang *Kiểm tra dữ liệu*). Cùng trang: bộ môn quản lý học phần và cờ *Tính vào GPA* (bỏ chọn với GDTC, GDQP-AN). Nút *Dùng làm mẫu* sao chép cơ cấu sang các học phần chưa có.
5. **CTĐT → Lớp sinh hoạt**: tạo lớp theo khóa, chọn cố vấn học tập, thêm/import sinh viên (`MSSV`, `HoTen`). Admin sửa hồ sơ sinh viên ở *Trường → Sinh viên* (tình trạng, khóa, lớp, liên kết tài khoản).
6. **CTĐT → Lớp học phần**: chọn năm học → học kỳ → *Mở lớp học phần*: chọn học phần, nhóm lớp (tự cấp N01, N02… nếu để trống), sĩ số tối đa, lớp sinh hoạt (nếu mở riêng), giảng viên (đúng một giảng viên chính, có thể thêm trợ giảng/GV thực hành).
7. **Lớp học phần → Sinh viên / điểm**: thêm sinh viên, import danh sách/điểm, *Thêm cả lớp sinh hoạt*. Nhập điểm từng đầu điểm hoặc điểm đặc biệt (I chưa hoàn thiện, R miễn/bảo lưu, P đạt). Màn hình hiện điểm học phần, điểm chữ, hệ 4 dự kiến.
8. **Nộp → công bố → phúc khảo**: giảng viên chính bấm *Nộp điểm* khi mọi sinh viên đủ điểm hoặc có điểm đặc biệt; sau khi nộp giảng viên không sửa được. Phòng đào tạo (admin) *Công bố điểm* — hệ thống lưu kết quả công bố (điểm, điểm chữ, hệ 4, đạt/không) và không thay đổi kết quả này khi thang điểm đổi sau. Sửa điểm đã công bố chỉ admin, bắt buộc ghi lý do, lịch sử ghi nguồn *Phúc khảo / sửa sau công bố*. *Trả lại để sửa* (kèm lý do) đưa sổ điểm về trạng thái đang nhập và thu hồi kết quả khỏi bảng điểm cho tới khi công bố lại.
9. **Bảng điểm**: theo học kỳ (GPA học kỳ, tín chỉ đạt) và tích lũy (GPA hệ 4, tín chỉ tích lũy, xếp loại học lực: Xuất sắc ≥ 3,6 · Giỏi ≥ 3,2 · Khá ≥ 2,5 · Trung bình ≥ 2,0 · Yếu ≥ 1,0 · Kém). Chỉ gồm điểm đã công bố. Admin xem mọi sinh viên, cố vấn học tập xem sinh viên lớp mình, sinh viên xem *Bảng điểm của tôi* khi hồ sơ đã liên kết tài khoản.

## Quy tắc nghiệp vụ

- **Điểm học phần** = `Σ(điểm/điểm tối đa × trọng số)/10`, làm tròn theo quy chế (mặc định 1 chữ số), rồi mới quy đổi điểm chữ/hệ 4. Chỉ tính khi đủ mọi đầu điểm; ô trống khác điểm 0.
- **Ghi danh**: chỉ sinh viên *đang học*; một sinh viên không học hai lớp học phần của cùng học phần trong cùng học kỳ; không vượt sĩ số; chỉ thêm/import khi sổ điểm còn *đang nhập*.
- **Học lại / cải thiện**: khi ghi danh, nếu sinh viên đã học cùng mã học phần (kể cả ở phiên bản CTĐT khác) ở học kỳ trước → *Học lại* (lần trước chưa đạt) hoặc *Cải thiện* (đã đạt). GPA học kỳ tính mọi lần học; GPA tích lũy chỉ lấy một lần theo quy chế.
- **Quyền**: admin toàn quyền. Giảng viên được phân công lớp học phần (mọi vai trò) xem và nhập điểm khi sổ đang nhập; chỉ giảng viên chính được nộp. Phân công học phần (CLO) không cho quyền vào sổ điểm. Cố vấn học tập xem danh sách và bảng điểm sinh viên lớp mình. Sinh viên chỉ xem bảng điểm của mình.
- **Cơ cấu điểm bị khóa** khi học phần đã có lịch sử nhập điểm: chỉ sửa tên đầu điểm và liên kết CLO; thay trọng số/thang điểm/loại cần phiên bản CTĐT mới. Sao chép phiên bản sao chép cơ cấu điểm theo học phần (chưa có liên kết CLO vì CLO không được sao chép), không sao chép lớp, sinh viên, điểm.
- **Import** (sổ điểm và lớp sinh hoạt): `.xlsx`/CSV UTF-8 hoặc dán từ Sheets, tối đa 500 dòng/5 MB, xem trước bắt buộc, nguyên lô (một dòng lỗi thì không ghi gì), kiểm tra revision để chặn ghi đè; ô công thức bị từ chối. Tiêu đề: `MSSV`, `HoTen` và mã đầu điểm.
- Mỗi lần ghi điểm lưu điểm cũ/mới, người sửa, thời điểm UTC, nguồn (nhập tay/import/phúc khảo) và lý do.

## Migration `20261110115802_AlignAcademicRecordsWithRegulations`

Sao lưu database trước khi chạy `tools/database initialize`/`migrate`. Down không được hỗ trợ (dữ liệu được gộp); khôi phục bằng bản sao lưu nếu cần quay lại. Migration chuyển dữ liệu cũ, giữ nguyên ID sinh viên, ghi danh, điểm và lịch sử:

- Cơ cấu điểm cấp phiên bản → sao chép cho **mọi** học phần của phiên bản (kể cả học phần đã lưu trữ), điểm và lịch sử được trỏ sang đầu điểm cùng mã của đúng học phần; loại đầu điểm suy ra từ mã/tên (THI/CK/“cuối”, GK/“giữa”, CC/“chuyên cần”, còn lại là quá trình) — admin nên rà soát lại.
- “Lớp theo học kỳ” cũ → lớp sinh hoạt gộp theo (phiên bản, mã lớp), tên lấy theo học kỳ gần nhất; lớp `LEGACY` (Dữ liệu cũ) không thành lớp sinh hoạt. Lớp học phần giữ mã 6 số, gắn lớp sinh hoạt tương ứng, được đánh nhóm N01, N02… theo thứ tự tạo.
- Giảng viên đang phân công → giảng viên chính. Sinh viên chỉ thuộc một lớp sinh hoạt được gán lớp đó; sinh viên xuất hiện ở nhiều lớp để trống cho admin xử lý.
- Mọi sổ điểm ở trạng thái *đang nhập* (chưa có kết quả công bố); admin cần nộp/công bố lại các học kỳ đã kết thúc để đưa vào bảng điểm.
- Mỗi trường được tạo quy chế và thang điểm mặc định TT08.

Migration identity `OfferingLecturerReadAccess` chỉ cập nhật read model phân quyền (không đổi bảng).

## Dữ liệu demo

`Database__SeedDemo=true` trên database rỗng tạo thêm (sau dữ liệu CTĐT V1/V2): khoa CNTT, bộ môn KHMT, quy chế TT08, lớp sinh hoạt KHMT01-K66 với 4 sinh viên giả lập, cơ cấu điểm có liên kết CLO cho 3 học phần, 3 lớp học phần đã công bố (gồm một ca học lại) và 1 lớp đang nhập điểm. `verify-demo` vẫn chỉ kiểm tra dữ liệu CTĐT để dùng được với database demo cũ.

## Kiểm thử

- `dotnet test modules/common/curriculum/backend/tests`: quy đổi điểm chữ/hệ 4, làm tròn, điểm đặc biệt, học lại/cải thiện, GPA học kỳ/tích lũy, xếp loại, validator quy chế/cơ cấu điểm/giảng viên, EF model khớp migration.
- `tests/contracts/academic_smoke.py` (API) và `academic_ui_smoke.py` (giao diện) — **chỉ chạy trên stack dùng một lần**. Biến môi trường: `SMOKE_BASE`, `SMOKE_DATABASE_URL` (psycopg thay cho `docker exec`), `SMOKE_WEB`, `SMOKE_CHANNEL` (rỗng = Chromium đi kèm Playwright). Chạy `academic_smoke.py` trước để tạo fixture.
- `subject_navigation_ui_smoke.py`, `academic_period_ui_smoke.py`, `assessment_weight_ui_smoke.py`: giao diện với API giả trên Vite :5173.
- `tests/contracts/academic_records_migration_smoke.py`: dữ liệu cũ → migration mới → so sánh điểm, lịch sử, lớp, giảng viên (hướng dẫn trong docstring).
