# Sinh viên và điểm học phần — nghiệp vụ chung

Toàn bộ nghiệp vụ ở `modules/common/curriculum`: entities, DTO/interface, service, EF repository, import reader và UI trong `frontend/pages/AcademicPages.tsx`. `apps/web` chỉ đăng ký route và nối nút từ màn hình học phần. Không có nghiệp vụ điểm trong SV4/SV5, không sử dụng AI để xử lý điểm.

## Sử dụng

Màn hình phiên bản là **Danh sách bộ môn**. Tên bộ môn và nút **Danh sách học phần** mở các học phần đã được mở từ đúng bộ môn đó, có tìm theo mã học phần/lớp/học kỳ. Mỗi dòng bộ môn chỉ có hai đường dẫn **Danh sách học phần** và **CLO/PLO**. Nút **Lớp theo học kỳ** để truy cập luồng tổ chức lớp chỉ đặt ở đầu màn hình phiên bản. Danh sách học phần của bộ môn hiển thị lớp dưới dạng văn bản; giảng viên chỉ thấy các học phần mình được phân công.

Theo cách gọi nghiệp vụ của dự án, “bộ môn” ở màn hình này chính là bản ghi môn gốc `ProgramCourse` hiện có, không tạo thêm một cấp khoa/bộ môn tổ chức mới. Các học phần mở vẫn là `CourseOffering`; ID, mapping, mã và dữ liệu điểm được giữ nguyên. API tra cứu mới: `GET /api/curriculum/program-versions/{versionId}/courses/{courseId}/offerings?page=1&search=...`.

1. Mở Trường đại học → CTĐT → phiên bản → **Cơ cấu điểm**. Admin nhập mã, tên, trọng số (%) và điểm tối đa cho từng đầu điểm theo quy định thực tế. Mỗi phiên bản dùng một cơ cấu chung cho mọi học phần. Chưa có tỷ lệ mặc định, điểm chữ hoặc quy tắc đạt/trượt được giả định.
2. Mở **Lớp theo học kỳ**, chọn **Đợt học** (năm học) → **Học kỳ** → **Mở danh sách lớp**. Admin **Thêm lớp** bằng mã/tên lớp. Cùng mã lớp được dùng lại ở học kỳ khác, không trùng trong cùng phiên bản và học kỳ.
3. Vào lớp → **Thêm học phần vào lớp**. Tìm theo mã/tên môn gốc trong CTĐT rồi chọn bằng dropdown, chọn giảng viên phụ trách hoặc để chưa phân công. Hệ thống cấp mã học phần mở gồm 6 chữ số. Một môn gốc chỉ mở một lần trong cùng lớp; mở ở lớp khác sẽ có mã khác. Giảng viên được phân công riêng cho từng học phần mở, không suy ra từ phân công môn gốc.
4. Bấm **Sinh viên / điểm** trong lớp học phần để thêm/import sinh viên và nhập điểm. Danh sách, điểm, revision và lịch sử thuộc đúng học phần mở; cùng MSSV có thể xuất hiện ở hai lớp của cùng môn với hai bộ điểm độc lập. Học kỳ lấy từ lớp, không nhập lại ở sổ điểm.
5. **Thêm sinh viên** bằng MSSV, họ tên; bấm sinh viên để nhập điểm. Hồ sơ này không tự tạo tài khoản đăng nhập và chưa liên kết với tài khoản student. Admin và giảng viên được phân công mới xem/nhập được; tài khoản student không xem danh sách/điểm của người khác.
6. **Import sinh viên / điểm**: chọn loại → tải mẫu Excel → điền → chọn `.xlsx`/CSV UTF-8 hoặc dán bảng từ Sheets → chọn sheet → xem trước → sửa hết lỗi → xác nhận. Import điểm cần sinh viên đã có trong đúng lớp học phần. Mỗi lần tối đa 500 dòng/5 MB; `.xls` cần lưu lại thành `.xlsx`. Không cần kết nối tài khoản Google.

Tiêu đề sinh viên: `MSSV`, `HoTen` (cũng nhận “Mã sinh viên”, “Họ và tên”). Tiêu đề điểm: `MSSV`, `HoTen` tùy chọn và mã đầu điểm đã cấu hình, ví dụ `QT`, `THI`. Đây chỉ là ví dụ mã; không được coi là cơ cấu chuẩn của trường. Giữ MSSV dạng văn bản để bảo toàn số 0 đầu. Excel hỗ trợ chuỗi hoặc số nguyên có định dạng `000000…`; số quá dài đã bị Excel làm tròn không thể khôi phục. Ô công thức phải chuyển thành giá trị trước khi nhập; không thực thi công thức hay link ngoài.

Import sinh viên trùng đợt: bỏ qua nếu tên khớp, báo lỗi nếu tên khác. MSSV là duy nhất trong từng trường. Import điểm: ô trống bỏ qua, 0 vẫn là điểm 0; thiếu đầu điểm không tính tổng. Mặc định chặn điểm đã có giá trị khác; chỉ cập nhật khi chọn rõ và xem trước lại. Cả lô nhập trong một transaction: bất kỳ dòng lỗi nào cũng không ghi dữ liệu. Lần xác nhận kiểm tra lại revision và dữ liệu hiện tại để chặn thay đổi sau xem trước.

## Quy tắc điểm và lưu trữ

Dropdown đợt học lấy các năm từ năm hiện tại trừ 4 đến cộng 2, cộng mọi đợt đã có lớp trong phiên bản. Năm học mới có Học kỳ 1, Học kỳ 2 và Học kỳ hè (khóa `HKHE`). Mã đợt cũ vẫn được giữ nguyên. Giảng viên chỉ thấy lớp/học phần mở được phân công; admin thấy toàn bộ. Phân công môn gốc chỉ giữ quyền nghiệp vụ CTĐT cũ, không cho đọc hoặc nhập điểm mọi lớp mở môn đó.

## Môn gốc và học phần mở

`ProgramCourse` là môn gốc trong phiên bản CTĐT, giữ CLO, liên kết PLO và các mapping X/Y, I/R/E hiện có. `AcademicClass` thuộc phiên bản + học kỳ. `CourseOffering` tham chiếu `ProgramCourse` và lớp; không sao chép CLO/PLO hay sinh lại mapping. Bổ sung hoặc sửa mapping vẫn tuân thủ quy trình phiên bản nháp/phát hành của CTĐT.

`CourseEnrollment.OfferingId` xác định sổ điểm cụ thể; FK ghép bắt buộc OfferingId, ProgramCourseId và AcademicPeriod khớp nhau. Mã học phần mở cấp bằng PostgreSQL sequence do EF Core quản lý, từ `100000` đến `999999`, có unique index và kiểm tra đúng sáu chữ số. Mã có thể có khoảng trống khi giao dịch không thành công; không dùng số lượng bản ghi để sinh mã. Mã môn gốc được giữ riêng và hiển thị cùng tên môn.

Migration `AddSemesterClassesAndCourseOfferings` chuyển mỗi nhóm dữ liệu cũ theo phiên bản/học kỳ vào lớp `LEGACY` / **Dữ liệu cũ** và tạo học phần mở tương ứng. Chỉ thêm OfferingId vào enrollment: giữ nguyên enrollment ID, điểm, lịch sử, cơ cấu điểm và mapping. Chưa có thông tin lớp/giảng viên thực tế nên không đoán phân công: admin cần phân công cho các học phần trong lớp Dữ liệu cũ. Migration không tự chuyển sinh viên sang lớp mới do admin tạo sau đó.

API sổ điểm chuyển từ `/courses/{id}/...` sang `/offerings/{offeringId}/...`; route môn gốc tiếp tục dùng để quản lý CLO/PLO. Import preview/apply bắt buộc OfferingId và học kỳ trùng đúng học phần mở, ngoài kiểm tra revision. Khi sao chép phiên bản CTĐT không sao chép lớp/học phần mở/sinh viên/điểm; tạo lại kế hoạch giảng dạy cho phiên bản mới.

- Trọng số dương, tổng chính xác 100%; thang điểm dương, điểm tối đa và điểm nhập tối đa hai chữ số thập phân. Mỗi CTĐT phiên bản 1–20 đầu điểm. Tổng kết = `10 × Σ((điểm / điểm tối đa) × trọng số / 100)`, làm tròn hai chữ số; chỉ tính khi đủ mọi đầu điểm. Chưa tính mức đạt CLO/PLO từ điểm thành phần.
- Cơ cấu điểm có thể thiết lập lần đầu cho phiên bản đã công bố; đây là cấu hình sổ điểm bổ sung, không sửa PLO/CLO/học phần đã công bố. Sau khi có bất cứ lịch sử nhập điểm nào, cơ cấu bị khóa kể cả khi xóa hết điểm; thay đổi cơ cấu bằng phiên bản CTĐT mới. Khi sao chép phiên bản: sao chép cơ cấu điểm, không sao chép sinh viên hay điểm.
- Mỗi lần nhập/sửa/xóa điểm lưu điểm cũ/mới, ID người thực hiện, UTC, nguồn nhập tay/import. Có kiểm tra revision để chặn lưu từ màn hình cũ. Học phần/phiên bản lưu trữ chỉ xem lại dữ liệu, không nhập thêm. Lịch sử trên UI hiển thị 50 thay đổi mới nhất; lịch sử đầy đủ vẫn ở database.
- EF thêm `academic_students`, `course_enrollments`, `assessment_components`, `student_grades`, `grade_changes` trong schema `curriculum`. Không thêm bootstrap SQL hay ORM khác.

## Mã học phần và migration

Migration `20261107033658_AddAcademicGradebooksAndSixDigitCodes` giữ mã hợp lệ 6 số, cấp mã nội bộ còn trống từ `000001` cho mã thiếu/sai theo từng trường, đồng bộ các snapshot học phần giữa các phiên bản. Mã là chuỗi để giữ số 0 đầu; duy nhất trong trường. Đây là mã nội bộ được người dùng chấp thuận, chưa phải đối chiếu danh mục chính thức của trường. Mã cũ lưu ở `LegacyInstitutionalCode`, `LegacyCourseCodeSnapshot` khi có; nguồn nhập gốc và `CourseCodeSourceValue` được giữ lại. Đổi mã bổ sung này là bước nâng cấp dữ liệu được chấp thuận, áp dụng cả snapshot cũ. Có check constraint ở PostgreSQL và validator bắt buộc mã mới đúng `[0-9]{6}`.

Sao lưu database trước khi chạy `tools/database initialize` hoặc `database-init` qua Docker. Migration Down xóa bảng sinh viên/điểm và cột lưu dấu vết, vì vậy chỉ dùng rollback bằng backup khi cần giữ dữ liệu vận hành. Bản nâng cấp không tự thêm sinh viên hay đầu điểm mẫu vào database đang sử dụng.

## Kiểm thử

`dotnet test modules/common/curriculum/backend/tests` kiểm tra mã 6 số, trọng số/thang điểm, điểm thiếu/0, CSV/TSV Unicode, Excel nhiều sheet/định dạng số, công thức bị chặn, template và EF model/migrations.

`tests/contracts/academic_smoke.py` và `academic_ui_smoke.py` chỉ dành cho stack disposable: kiểm tra phân quyền, học kỳ, ghi điểm/lịch sử, concurrency, import nguyên lô, bản xem trước cũ, sao chép cơ cấu, click UI và responsive. Không chạy các fixture này vào dữ liệu thật.
