# Kế hoạch hoàn thành phần chung OBE cho nhóm SV4 và SV5

Ngày lập: 14/11/2026. Trạng thái: đã triển khai phần chung và kiểm chứng trên môi trường thử riêng; tài liệu này giữ đặc tả để đối chiếu.

Cập nhật triển khai 14/11/2026: xem [biên bản hoàn thành và kết quả kiểm thử](SHARED_IMPLEMENTATION_STATUS.md) cùng [hợp đồng đầu ra](SHARED_OUTPUT_CONTRACTS.md). Nội dung bên dưới được giữ làm đặc tả đối chiếu.

## 1. Mục tiêu và cách sử dụng

Hoàn thiện phần dùng chung để nhóm hai người tự vận hành hệ thống độc lập, cung cấp dữ liệu đúng và có thể truy xuất nguồn cho module Minh chứng/kiểm định của SV4 và Báo cáo/AI/chatbot của SV5.

Tài liệu này có thể giao trực tiếp cho AI có quyền truy cập repository. AI phải đọc mã nguồn hiện tại, thực hiện lần lượt các gói công việc, kiểm thử và cập nhật tiến độ. Không coi tên endpoint, giao diện có dữ liệu mẫu hoặc build thành công là bằng chứng nghiệp vụ đã hoàn thành.

Nguồn yêu cầu: `C:\Users\Admin\Downloads\sv_kiểm định.docx`. Nếu máy triển khai không có file này, dùng ma trận phạm vi trong mục 3; không bịa thêm yêu cầu từ tài liệu gốc. Yêu cầu trực tiếp của người dùng về nhóm độc lập và loại bỏ AI của SV1–3 có ưu tiên cao hơn cách phân công năm người trong tài liệu gốc.

Các công thức, ngưỡng và quyền chi tiết bên dưới là **mặc định thiết kế đề xuất cho đồ án**, không phải tuyên bố về chuẩn AUN-QA hoặc quy định của một trường. AI được triển khai theo mặc định này khi nhận lệnh thực hiện kế hoạch; ghi chúng vào tài liệu nghiệp vụ và cấu hình sản phẩm. Không cần liên tục hỏi lại các lựa chọn đã được chốt trong kế hoạch.

## 2. Ranh giới trách nhiệm

### Thuộc phần chung do SV4 và SV5 cùng sở hữu

- Nền tảng đăng nhập, phân quyền, danh mục, database, API, layout, lưu file và audit.
- Toàn bộ nghiệp vụ SV1: CTĐT, học phần, phiên bản, PLO, CLO, mapping và ma trận.
- Toàn bộ nghiệp vụ SV2: assessment, assessment–CLO, lớp học phần, nhập/chốt điểm, điểm học phần và GPA học kỳ/tích lũy.
- Toàn bộ nghiệp vụ SV3: engine CLO/PLO, dashboard, thống kê, so sánh học kỳ/khóa/ngành.
- Interface/API dữ liệu đọc phục vụ SV4 và SV5, cùng snapshot kết quả có nguồn gốc.
- Hạ tầng gọi AI dùng chung: kiểm tra và tái sử dụng client hiện có nếu cần cho SV4/SV5; kiểm tra contract bằng fake client, không yêu cầu trả phí để nghiệm thu phần chung.

### Thuộc phần riêng

- SV4: nghiệp vụ minh chứng, liên kết tiêu chí AUN, hồ sơ kiểm định, phân loại/gợi ý minh chứng bằng AI.
- SV5: bố cục và xuất báo cáo học vụ/kiểm định đầy đủ, sinh nội dung AI, chatbot và câu trả lời theo minh chứng.
- AI Mapping Advisor, Assessment Advisor, Dashboard Insight của SV1–3 không thuộc phạm vi.

Phần chung bàn giao dữ liệu và một luồng chứng minh consumer đọc được dữ liệu. Không chuyển engine tính toán vào Reporting hoặc chatbot. Không mở rộng kế hoạch này thành việc hoàn thành luôn UC19–UC30. Các tính năng AI đã tồn tại không cần bị xóa.

## 3. Ma trận yêu cầu và nghiệm thu

Hiện trạng là kết quả rà soát trước khi lập kế hoạch; AI phải xác minh lại trên working tree khi triển khai.

| Yêu cầu | Hiện trạng đã thấy | Công việc và tiêu chí nghiệm thu |
| --- | --- | --- |
| UC01 CTĐT | Có CRUD | Tạo/sửa/lưu trữ đúng quyền, quan hệ trường và đơn vị đúng |
| UC02 Học phần | Có CRUD và phiên bản | Học phần trong CTĐT khác lớp mở thực tế; khóa ngoại đúng |
| UC03 PLO | Có CRUD | PLO thuộc phiên bản, không sửa trái phép dữ liệu đã khóa |
| UC04 CLO | Có CRUD | CLO thuộc học phần trong phiên bản, có kiểm soát trạng thái |
| UC05 CLO–PLO | Có liên kết | Không liên kết chéo phiên bản/học phần; công thức sử dụng liên kết rõ ràng |
| UC06 Ma trận | Có UI | Hiển thị đúng dữ liệu thật, quyền sửa thống nhất với API |
| Phiên bản PLO/CLO | Có cấu trúc phiên bản | Bảo toàn lịch sử; bản sao có đủ outcome/mapping hoặc chỉ rõ còn thiếu |
| UC07 Assessment | Có | Tổng trọng số 100%, điểm tối đa hợp lệ, đúng học phần |
| UC08 Assessment–CLO | Có tỷ lệ | Chuẩn hóa ý nghĩa tỷ lệ theo mục 5; phát hiện mapping thiếu |
| UC09 Lớp học phần | Có | Kỳ học, GV, danh sách và phạm vi truy cập chính xác |
| UC10 Nhập điểm | Có nhập/import | Kiểm tra UI nhập nhiều sinh viên dạng bảng; bổ sung nếu hiện chỉ sửa từng người |
| UC11 Điểm học phần | Có | Giữ quy tắc hiện có, điểm 0 khác chưa nhập, kết quả công bố ổn định |
| UC12 GPA/GPAS | Có GPA kỳ/tích lũy | Kiểm chứng học lại, miễn, môn không tính GPA; UI ghi tên rõ ràng |
| UC13 CLO cá nhân | Chưa thấy engine | Điểm, trạng thái và chi tiết nguồn tính khớp fixture |
| UC14 CLO lớp | Chưa thấy | Mẫu số, số đạt, dữ liệu thiếu và tỷ lệ đúng |
| UC15 PLO | Chưa thấy | Tính theo sinh viên rồi tổng hợp; không trùng lần học |
| UC16 Dashboard CLO | Chưa thấy | Bộ lọc, bảng, biểu đồ và truy xuống dữ liệu có phân quyền |
| UC17 Dashboard PLO | Chưa thấy | Kết quả, coverage, nguồn đóng góp và trạng thái đầy đủ |
| UC18 Thống kê | Có trường dữ liệu đầu vào | Thống kê theo kỳ/khóa/ngành, so sánh có kiểm tra tính tương thích |
| Auth và quyền | Admin/lecturer/student | Bổ sung phạm vi Khoa/Bộ môn; kiểm tra cả API và UI |
| Audit | Có ở một số nghiệp vụ | Bao phủ thay đổi outcome/mapping/policy và vòng đời kết quả |
| Storage/client AI | Có nền tảng | Tái sử dụng qua contract, phân quyền và xử lý lỗi; không tạo dịch vụ trùng |

GPAS chưa được tài liệu gốc định nghĩa riêng. Trong đồ án này dùng tên rõ ràng `GPA học kỳ` và `GPA tích lũy`, giữ cách tính hiện có; ghi cách hiểu này vào tài liệu bàn giao.

## 4. Kiến trúc và những chỗ cần đọc

- Backend: ASP.NET Core modular monolith; persistence EF Core/PostgreSQL.
- Frontend: React/TypeScript; `apps/web` ghép route, các module chứa nghiệp vụ.
- `modules/common/curriculum/backend`: domain, application, infrastructure, presentation, tests.
- `modules/common/curriculum/frontend`: API/types/pages/components của học vụ.
- `apps/web/src/curriculum`: các trang CTĐT/outcome hiện có; giữ cấu trúc khi sửa.
- `modules/common/identity/backend` và `shared/backend/Security.cs`: người dùng, quyền và public contract.
- `modules/sv4/accreditation/backend`: có storage/catalog; đọc interface trước khi dùng lại.
- `modules/sv5/reporting/backend`: consumer báo cáo hiện tại.
- `modules/sv5/knowledge`, `apps/ai-service`: kiểm tra hạ tầng AI hiện có, không xây lại.
- `tools/database`: migration runner và demo seeding.
- `tests/contracts`: smoke tests API/UI; một số test yêu cầu stack dùng một lần.

Đọc tối thiểu các file này trước khi thay đổi:

1. `docs/SHARED_ACADEMIC_GRADEBOOK.md`, `docs/EF_CORE_ARCHITECTURE.md`.
2. `AcademicService.cs`, `AcademicValidator.cs` trong application Curriculum.
3. `domain/Rules/GradeRules.cs`, `GpaRules.cs`, `CurriculumAnalysisRules.cs`.
4. `EfAcademicRepository.cs`, `EfStudentDirectoryRepository.cs`, `EfProgramVersionRepository.cs`.
5. `application/DTOs/Reporting/CurriculumReportSnapshot.cs`, `EfCurriculumReportSource.cs`.
6. `modules/sv5/reporting/backend/application/ReportBuilder.cs`.
7. Các endpoint, trang UI, tests và caller của contract sẽ sửa.

Engine OBE mới đặt trong Curriculum, dùng hàm domain thuần cho phép tính. Không cần microservice, message bus, data warehouse hoặc framework workflow mới. Tách interface đọc cho consumer; không để module khác tự query bảng Curriculum. Không thay cấu trúc báo cáo cũ một cách phá vỡ tương thích.

## 5. Quy tắc tính mặc định cho lần triển khai đầu

### 5.1 Phạm vi và trạng thái

- Dữ liệu chính thức lấy từ lớp học phần và điểm đã công bố. Có thể xem trước từ draft nhưng phải gắn `preview`, không dùng như dữ liệu chính thức.
- Dùng ID nội bộ để liên kết. Không ghép chỉ bằng tên học phần, mã CLO/PLO hay tên sinh viên.
- Một snapshot luôn ghi trường, CTĐT, phiên bản, kỳ/khóa/lớp được chọn và revision policy.
- Trạng thái kết quả cá nhân: `achieved`, `not_achieved`, `insufficient_data`, `excluded`.
- Không có dữ liệu khác kết quả không đạt. Mẫu số bằng 0 trả `null`, không trả 0% hoặc 100%.
- Điểm đặc biệt I: chưa đủ dữ liệu; R/P: loại khỏi đánh giá OBE định lượng, ghi lý do. Không suy ra đạt CLO từ miễn học/đạt học phần.
- Môn không tính GPA vẫn có thể tham gia OBE nếu có mapping và điểm hợp lệ.

### 5.2 Assessment và điểm CLO

Ký hiệu: `s_ia` là điểm SV i ở assessment a; `m_a` là điểm tối đa; `w_a` là trọng số assessment theo học phần; `q_ac` là tỷ trọng nội dung assessment a dùng đánh giá CLO c.

- `w_a > 0`, tổng w trong học phần bằng 100.
- Với một assessment có mapping: mỗi q > 0, tổng q trên các CLO của assessment bằng 100. Ví dụ một bài kiểm tra đánh giá CLO1 60%, CLO2 40%.
- Assessment chưa có mapping được phép lưu để nhập liệu từng bước, nhưng là lỗi readiness cho snapshot OBE chính thức. Không tự gán CLO hoặc tự chuẩn hóa dữ liệu cũ trong migration.
- Một CLO phải có ít nhất một assessment đóng góp. Thiếu mapping trả `insufficient_data` và lỗi readiness.
- Chuẩn hóa `x_ia = 10 × s_ia / m_a`.
- Trọng số hiệu lực `e_ac = (w_a / 100) × (q_ac / 100)`.
- `CLO_ic = Σ(x_ia × e_ac) / Σ(e_ac)`, chỉ trên assessment có liên kết CLO c.
- Nếu thiếu điểm của bất kỳ assessment cần cho CLO thì CLO đó chưa đủ dữ liệu; không bỏ đầu điểm thiếu rồi chia lại trọng số.
- Dùng decimal, không làm tròn trung gian; hiển thị 2 chữ số thập phân, AwayFromZero. So sánh ngưỡng bằng giá trị chưa làm tròn.
- Mặc định điểm CLO đạt từ 5/10. Ngưỡng cấu hình theo policy của phiên bản CTĐT, có revision, không hard-code vào UI.

Đây là mô hình ước lượng CLO từ điểm tổng của assessment. UI/tài liệu phải nêu giới hạn: chưa có điểm riêng từng câu hỏi hoặc rubric criterion. Không khẳng định độ chính xác tương đương đánh giá từng câu.

### 5.3 CLO theo lớp và học phần

- `eligibleCount`: sinh viên thuộc phạm vi sau khi loại trường hợp excluded.
- `evaluatedCount`: sinh viên có kết quả CLO hợp lệ.
- `missingCount = eligibleCount - evaluatedCount`.
- `achievedCount`: trong evaluatedCount, số đạt ngưỡng cá nhân.
- `achievementRate = achievedCount / evaluatedCount × 100`.
- `dataCoverageRate = evaluatedCount / eligibleCount × 100`.
- Mặc định lớp đạt yêu cầu khi achievementRate ≥ 70% và dataCoverageRate = 100%. Thiếu coverage chỉ hiển thị kết quả một phần, không kết luận lớp đạt chính thức.
- Khi gộp nhiều lớp cùng học phần trong một kỳ: tính từ bản ghi sinh viên, không lấy trung bình các tỷ lệ lớp. Phát hiện trùng ghi danh thay vì đếm hai lần.
- CLO của các phiên bản khác nhau không tự đồng nhất dù cùng mã. Tách nhóm, trừ khi sau này có mapping tương đương được phê duyệt.

### 5.4 PLO từ CLO

Không dùng `plo-balance` làm kết quả PLO: đó là chỉ số thiết kế CTĐT.

- Trong một học phần, `PLO_ip_course` bằng trung bình các điểm CLO liên kết PLO p; liên kết CLO–PLO hiện tại là nhị phân nên mặc định các CLO có trọng số bằng nhau.
- Nếu một CLO bắt buộc đóng góp chưa đủ dữ liệu thì PLO học phần đó chưa đủ dữ liệu, không bỏ CLO đó khỏi mẫu số.
- PLO cá nhân theo phạm vi = trung bình có trọng số các PLO học phần hợp lệ. Trọng số học phần = tín chỉ × hệ số X/Y, với X=2, Y=1. I/R/E chỉ là thông tin tiến triển, không nhân vào điểm.
- Mặc định ngưỡng PLO cá nhân 5/10; tỷ lệ đạt mục tiêu nhóm 70%. Lưu trong policy, tách tên với ngưỡng CLO dù mặc định giống nhau.
- Trước khi tổng hợp PLO cá nhân, chọn tối đa một lần học cho mỗi sinh viên + CourseId trong phạm vi được chọn. Mặc định dùng lần có điểm học phần công bố cao nhất; hòa điểm lấy kỳ mới hơn, rồi OfferingId lớn hơn. Nếu policy trường là latest, chọn kỳ mới nhất, rồi OfferingId; lưu lựa chọn vào snapshot.
- Chỉ chọn từ các lần có điểm công bố định lượng. Không chọn lần có CLO cao nhất riêng cho từng CLO. Sau khi chọn một lần, nếu mapping/điểm của lần đó thiếu thì trả thiếu, không âm thầm lấy CLO từ lần khác.
- Sắp xếp kỳ theo năm bắt đầu và thứ tự học kỳ đã parse/chuẩn hóa, không dựa vào tên hiển thị tùy ý. Kiểm tra dữ liệu kỳ hiện có trước khi tái sử dụng logic sắp xếp.
- Trong một CTĐT/phiên bản: PLO theo khóa/ngành được tính từ điểm PLO từng sinh viên rồi đếm người đạt; không đếm từng CLO như một sinh viên.
- Phạm vi thống kê là các học phần/lần học trong bộ lọc. Snapshot ghi rõ đây là kết quả trong phạm vi đã học, không tự kết luận sinh viên đủ chuẩn tốt nghiệp.
- Nếu gộp theo ngành có nhiều phiên bản: trả từng nhóm phiên bản; không trộn PLO cùng mã nhưng khác ý nghĩa. So sánh học kỳ trực tiếp chỉ khi cùng version, outcome definition và policy revision.
- Với mỗi PLO cần có coverage số học phần đã đánh giá / số học phần liên quan dự kiến trong phạm vi, đồng thời coverage sinh viên. Thiếu thành phần trong phạm vi làm kết quả `partial`; không kết luận đạt toàn bộ chương trình.
- Phạm vi toàn CTĐT phải chỉ rõ tập học phần áp dụng. Chưa có quy tắc chọn nhóm tự chọn thì không tự coi tất cả môn tự chọn là bắt buộc; hiển thị giới hạn và không kết luận chuẩn tốt nghiệp.

### 5.5 Snapshot và tính lại

- Cấu hình policy có revision bất biến sau khi đã được dùng trong snapshot. Sửa tạo revision mới.
- Snapshot OBE lưu input cần tái tính, kết quả, source revision/hash, policy, người tạo, thời điểm UTC và phạm vi.
- Hash tạo từ dữ liệu đã chuẩn hóa và sắp xếp ổn định; không dùng thứ tự query ngẫu nhiên.
- Dùng transaction/read snapshot nhất quán khi đọc điểm và mapping. Không lấy điểm của revision này ghép mapping revision khác.
- Phúc khảo, reopen, sửa mapping/assessment/policy: giữ snapshot cũ để truy vết và đánh dấu stale/superseded; consumer phải thấy trạng thái này.
- Với quy mô đồ án, kiểm tra revision/hash lúc đọc hoặc yêu cầu tạo snapshot mới là đủ; không bắt buộc job nền. Không sửa đè nội dung snapshot cũ.
- API latest chỉ trả kết quả còn hiệu lực hoặc báo cần tính lại. Có thể tải snapshot cũ theo ID với quyền hiện tại và nhãn lịch sử.

## 6. Quyền tối thiểu

Tái sử dụng cơ chế Identity hiện có. Tên role mới đề xuất: `faculty_manager`, `department_manager`. Một tài khoản giữ mô hình role hiện tại, gán một hoặc nhiều OrgUnit trong phạm vi trường; không xây lại toàn bộ RBAC.

| Hành động | Admin | Khoa | Bộ môn | Giảng viên | Sinh viên |
| --- | --- | --- | --- | --- | --- |
| Quản lý tài khoản/phân quyền | Toàn hệ thống | Không | Không | Không | Không |
| CTĐT/PLO draft, policy OBE | Có | CTĐT thuộc khoa | Đọc phạm vi | Đọc được giao | Đọc đã công bố theo quyền cũ |
| Học phần/CLO/assessment draft | Có | Thuộc khoa | Học phần thuộc bộ môn | CLO được phân công theo quyền cũ | Không |
| Nhập điểm | Có | Đọc phạm vi | Đọc phạm vi | Lớp được phân công | Không |
| Nộp điểm | Có | Không | Không | Giảng viên chính | Không |
| Công bố/trả lại/phúc khảo | Có | Không | Không | Không | Không |
| Đọc kết quả OBE | Tất cả | CTĐT khoa | Học phần bộ môn | Học phần/lớp được giao | Kết quả cá nhân được công bố |
| Tạo snapshot chính thức | Có | Phạm vi khoa | Phạm vi học phần bộ môn | Xem trước phạm vi | Không |

- Quyền công bố CTĐT tiếp tục chỉ Admin trong lần triển khai đầu.
- Quyền Khoa/Bộ môn tính theo OrgUnit thực tế, không lấy tất cả dữ liệu cùng trường.
- Giảng viên không được xem tổng hợp toàn ngành chỉ vì được giao một học phần.
- Kiểm tra quyền trên backend cả list/detail/filter/export/snapshot và từng ID lồng nhau. Frontend chỉ phản ánh quyền.
- Khi một yêu cầu gồm dữ liệu ngoài quyền: từ chối phạm vi đó, không âm thầm trả tổng hợp thiếu rồi ghi là tổng toàn bộ.
- Export dữ liệu sinh viên chỉ người có quyền đọc chi tiết tương ứng; không dùng credential hệ thống để vượt quyền khi chatbot gọi API sau này.

## 7. Các gói triển khai và đầu ra

### G0 Khảo sát và baseline

1. Đọc hướng dẫn repo/AGENTS nếu có, kiểm tra git diff và không ghi đè thay đổi của người dùng.
2. Xác minh lại ma trận mục 3; đọc caller/test trước khi sửa contract.
3. Chạy test/build phù hợp với môi trường; ghi baseline và lỗi có sẵn. Lần rà soát trước có 67 test Curriculum đạt, không coi đó là kết quả hiện tại.
4. Tạo `docs/SHARED_IMPLEMENTATION_STATUS.md`: gói việc, trạng thái, file, lệnh test và kết quả, việc còn lại.
5. Thiết lập DB/MinIO dùng một lần cho integration; không chạy smoke có mutation trên database người dùng.

Nghiệm thu: có bảng gap thực tế và baseline, xác định được đường chạy test.

### G1 Hoàn thiện quyền và nền tảng

1. Thêm role/phân công OrgUnit, migration, API quản lý và UI gán phạm vi.
2. Rà soát toàn bộ role checks đang hard-code admin/lecturer/student: shared policy, Identity, Curriculum, shell/navigation và consumer bị ảnh hưởng. Không mở rộng quyền SV4/SV5 ngoài phạm vi được xác định.
3. Bổ sung audit cho CTĐT, PLO/CLO, mapping, assessment, OBE policy, snapshot. Lưu actor, thời điểm, entity, thao tác, thay đổi cần thiết; không ghi token/mật khẩu hoặc dữ liệu dư thừa.
4. Tái sử dụng storage/catalog qua interface. Đảm bảo upload/download metadata và quyền truy cập, cấu hình dung lượng/loại file; chưa làm nghiệp vụ evidence–criterion.
5. Kiểm tra client AI hiện có: cấu hình từ secret/env, timeout/cancellation, lỗi rõ ràng, fake client test. Chỉ bổ sung điểm còn thiếu để SV4/SV5 dùng chung; không bắt buộc gọi LLM thật.

Nghiệm thu: test scope Khoa/Bộ môn, audit của mutation thành công, không có audit thành công khi transaction rollback; file/client có contract và ví dụ dùng lại.

### G2 Hoàn thiện dữ liệu đầu vào và readiness

1. Thêm policy OBE theo ProgramVersion, revision và validator theo mục 5.
2. Thêm endpoint kiểm tra sẵn sàng: lỗi/cảnh báo, entity ID, lý do, liên kết UI sửa. Kiểm tra mapping chéo version, tổng q, outcome không được đánh giá, lớp/điểm/trạng thái và kỳ học.
3. Dữ liệu cũ có q không hợp lệ được báo lỗi; không tự sửa thành 100% trong migration. Cung cấp UI chỉnh và audit.
4. Sao chép phiên bản: bổ sung tùy chọn sao chép PLO/CLO, CoursePlo, CloPlo và Assessment–CLO với ID mới trong một transaction. Không sao chép điểm, lớp mở hay snapshot. Giữ hành vi cũ qua option rõ ràng.
5. Giữ khóa outcome của finalized version; mọi đường sửa assessment mapping ở version đã dùng phải tạo revision/đánh dấu stale theo mục 5. Không cho sửa lén qua import.
6. Xác minh UC07–UC12. Nếu thiếu UI nhập bảng nhiều SV thì bổ sung bảng theo lớp, validation từng ô, lưu và import nhất quán. Giữ lịch sử/khóa điểm/quyền hiện có.

Nghiệm thu: bộ CTĐT mới tạo hoặc clone có thể đi tới trạng thái readiness hợp lệ; thiếu dữ liệu được giải thích; điểm/GPA hiện tại không bị hồi quy.

### G3 Engine CLO và API

1. Hàm domain thuần tính CLO cá nhân và aggregate lớp; không query DB trong hàm tính.
2. Application service đọc điểm, assessment, mapping, policy trong đúng scope; tạo input chuẩn hóa.
3. API read/preview kết quả cá nhân, CLO lớp và chi tiết nguồn tính; dùng phân trang cho danh sách SV.
4. Snapshot chính thức yêu cầu điểm đã công bố, cấu hình hợp lệ. Snapshot có dữ liệu thiếu ghi partial; không phát ra kết luận đạt chính thức cho tập chưa đầy đủ.
5. Hoàn thiện UI tối thiểu để người dùng xem kết quả và truy xuất đầu điểm.

Nghiệm thu: fixture mục 9 đúng; null/0/boundary và quyền đúng; snapshot có thể tái tính không phụ thuộc dữ liệu hiện tại đã thay đổi.

### G4 Engine PLO và thống kê

1. PLO học phần từ CLO; PLO cá nhân trong phạm vi từ các học phần.
2. Chọn lần học có quy tắc xác định; ghi nguồn được chọn, không nhân đôi sinh viên.
3. Tổng hợp theo lớp/học phần, học kỳ, khóa, CTĐT/phiên bản. Ngành nhiều phiên bản trả phân nhóm.
4. So sánh học kỳ với policy/outcome tương thích; khác policy thì trả lý do không thể so sánh trực tiếp.
5. Giải thích điểm/trọng số/coverage của PLO và giới hạn phạm vi.

Nghiệm thu: tổng hợp đúng theo số SV, xử lý học lại và dữ liệu thiếu, không trộn outcome chỉ vì cùng mã.

### G5 Dashboard

1. Trang CLO: bộ lọc lớp/kỳ, bảng và biểu đồ tỷ lệ, ngưỡng, số đạt/tổng được tính, coverage, dữ liệu thiếu.
2. Trang PLO: CTĐT/version/khóa/kỳ, điểm/tỷ lệ/coverage, drill-down đúng quyền.
3. Trang hoặc vùng so sánh học kỳ dùng cùng engine/API; chênh lệch tỷ lệ hiển thị theo điểm phần trăm.
4. Trạng thái loading/error/empty/partial/stale; không biến null thành 0 trên biểu đồ.
5. Dùng UI/biểu đồ đơn giản phù hợp thư viện hiện có; không thêm dependency chỉ để vẽ vài cột.

Nghiệm thu: UI phản ánh chính xác fixture backend, truy cập bằng bàn phím, labels rõ và không lộ dữ liệu ngoài quyền.

### G6 Contract bàn giao SV4 và SV5

1. Cung cấp public read interface cùng HTTP API cho assessment, học vụ/GPA và snapshot OBE; giữ contract cấu trúc CTĐT cũ tương thích.
2. Contract có schema version, scope, policy revision, trạng thái kết quả, số đếm, nguồn, source hash, thời điểm và cảnh báo.
3. Cho tải JSON của snapshot đúng quyền để tích hợp/kiểm chứng. Đây là dữ liệu bàn giao, không thay thế module báo cáo SV5.
4. Consumer thử nghiệm/test ở ranh giới module chứng minh SV4/SV5 đọc được snapshot mà không query bảng nội bộ. Không cần hoàn thành bố cục báo cáo hay AI nhận xét.
5. Viết `docs/SHARED_OUTPUT_CONTRACTS.md`: DTO, endpoint thực tế, ví dụ đủ/thiếu/stale, quyền, mã lỗi và cách tích hợp.

Nghiệm thu: consumer lấy được assessment/GPA/CLO/PLO có nguồn; snapshot cũ còn đọc được với nhãn lịch sử; tổng hợp không mâu thuẫn dashboard.

### G7 Migration, kiểm thử và bàn giao

1. Kiểm thử migration trên DB rỗng và DB cũ dùng một lần; giữ ID dữ liệu học vụ, điểm, lịch sử, phân công và báo cáo cũ.
2. Bổ sung demo chạy lặp an toàn, chỉ bật bằng cấu hình demo; không tự seed vào dữ liệu thật.
3. Kiểm thử end-to-end luồng mục 10 và các trường hợp lỗi quan trọng.
4. Chạy build/tests, cập nhật tài liệu chạy và bảng truy vết UC01–UC18 với bằng chứng.
5. Hoàn tất status với phần nào đã chạy, chưa chạy, lỗi tồn tại và giới hạn nghiệp vụ. Không đánh dấu Done nếu thiếu kiểm chứng bắt buộc.

Thứ tự phụ thuộc: G0 → G1 → G2 → G3 → G4 → G5 → G6 → G7. Có thể thiết kế contract G6 từ G2, nhưng nghiệm thu phải dùng engine thật. Nếu nhiều AI cùng làm, mỗi AI có phạm vi file riêng; một người/AI chịu trách nhiệm migration và contract dùng chung.

## 8. Contract đề xuất

Tên dưới đây là đề xuất để AI triển khai nhất quán; được đổi theo convention repo nếu cập nhật cả caller, tests và tài liệu.

- `IOutcomeReadService`: đọc kết quả/nguồn có kiểm tra scope.
- `OutcomePolicy`: VersionId, Revision, CloThreshold, PloThreshold, GroupTargetRate, FormulaVersion, RetakePolicy.
- `OutcomeScope`: UniversityId, ProgramId, VersionId, CourseId/ProgramCourseId, OfferingIds, Periods, Cohort; ghi rõ học phần được áp dụng.
- `StudentOutcomeResult`: StudentId, OutcomeId, Score, Threshold, Status, Reason, SourceReferences.
- `OutcomeAggregate`: OutcomeId, EligibleCount, EvaluatedCount, AchievedCount, MissingCount, ExcludedCount, AchievementRate, DataCoverageRate, MeanScore, Status.
- `OutcomeSnapshot`: Id, SchemaVersion, Scope, Policy, SourceRevision/Hash, CreatedAt, CreatedBy, Status, Inputs, Results, Warnings.
- `AcademicSummary`: scope, số SV, phân bố điểm, GPA học kỳ/tích lũy theo định nghĩa hiện có, cơ cấu assessment; không gắn nhãn điểm học phần hệ 4 là GPA tích lũy.

Route đề xuất dưới `/api/curriculum/program-versions/{versionId}`:

| Method/path tương đối | Mục đích |
| --- | --- |
| GET/PUT `/outcome-policy` | Đọc/tạo revision policy |
| GET `/outcome-readiness` | Kiểm tra cấu hình/dữ liệu theo scope |
| GET `/offerings/{offeringId}/clo-results` | CLO lớp, phân trang chi tiết nếu có |
| GET `/offerings/{offeringId}/students/{studentId}/outcomes` | Chi tiết cá nhân và nguồn |
| GET `/plo-results` | PLO theo scope được phép |
| GET `/outcome-comparisons` | So sánh kỳ tương thích |
| POST `/outcome-snapshots` | Tạo snapshot chính thức hoặc preview, quyền tách biệt |
| GET `/outcome-snapshots/{snapshotId}` | Đọc snapshot và trạng thái hiệu lực |
| GET `/outcome-snapshots/{snapshotId}/export` | JSON có schema version |

Endpoint về snapshot phải kiểm tra versionId trên path khớp snapshot. Mã lỗi dùng convention hiện có: input sai 400; chưa đăng nhập 401; ngoài quyền 403 hoặc 404 nhất quán; dữ liệu không tồn tại 404; revision xung đột/readiness chưa hợp lệ 409. Không trả lỗi DB nguyên văn.

Không cần bảng cho mọi aggregate. Có thể lưu snapshot inputs/results dạng JSON theo cách Reporting hiện có và tính aggregate khi truy vấn, miễn đủ tính nhất quán, phân trang và hiệu năng dữ liệu demo. Chính sách quyền phải áp dụng trước khi đọc inputs chứa thông tin cá nhân.

## 9. Fixture tính tay bắt buộc

### CLO

Một lớp có Assessment A trọng số 40%, B trọng số 60%; cả hai thang 10. A map CLO1 100%. B map CLO1 50%, CLO2 50%.

| SV | A | B | CLO1 = (0,4A + 0,3B)/0,7 | CLO2 | Kết quả ngưỡng 5 |
| --- | --- | --- | --- | --- | --- |
| S1 | 8 | 6 | 7,142857… | 6 | Đạt cả hai |
| S2 | 4 | 2 | 3,142857… | 2 | Chưa đạt cả hai |
| S3 | 10 | 8 | 9,142857… | 8 | Đạt cả hai |
| S4 | 6 | thiếu | null | null | Chưa đủ dữ liệu |

Đối với preview có S4: mỗi CLO eligible=4, evaluated=3, achieved=2, missing=1, tỷ lệ đạt=66,666…%, coverage=75%. Không kết luận lớp đạt; snapshot chính thức không được giả công bố điểm còn thiếu. Kiểm thử chính thức bằng fixture hợp lệ đã công bố, hoặc I được công bố theo luồng hiện có để thể hiện thiếu dữ liệu.

S1 điểm học phần = 6,8; điểm CLO1 ≈ 7,14. Hai đại lượng không được dùng thay nhau. Nếu B=0 thì đó là điểm hợp lệ, không thành null.

### PLO

- HP1: 3 tín chỉ, X; một SV có CLO1=8, CLO2=6 cùng map PLO1 → PLO1 của HP1=7.
- HP2: 2 tín chỉ, Y; CLO liên kết PLO1 có điểm 4 → PLO1 của HP2=4.
- PLO1 cá nhân trong phạm vi hai môn = `(7×3×2 + 4×2×1)/(3×2 + 2×1) = 6,25`.
- Hai nhóm không trùng SV có số đạt 1/2 và 8/8: tổng 9/10=90%, không phải trung bình 50% và 100% là 75%.
- Thêm hai lần học cùng CourseId: kiểm tra chỉ một lần được dùng; hòa điểm có thứ tự chọn xác định.
- Hai version cùng tên PLO1 nhưng khác statement: tách nhóm, không merge.

### Các ca bổ sung

Kiểm tra điểm sát ngưỡng trước làm tròn, mẫu số 0, q tổng sai, tín chỉ/điểm tối đa không hợp lệ, assessment không map, CLO không map PLO, dữ liệu ngoài quyền, ID chéo version, cập nhật revision cạnh tranh, phúc khảo rồi stale, policy mới không sửa snapshot cũ.

## 10. Kiểm chứng kỹ thuật và nghiệm thu cuối

Lệnh từ repository root, theo SDK/dependency đã có:

```powershell
dotnet build ObeAunQa.slnx
dotnet test ObeAunQa.slnx
npm --prefix apps/web run build
```

Trong từng gói, chạy test module liên quan trước. Kiểm tra docstring của smoke tests trước khi chạy; chỉ dùng database/stack dùng một lần. Không mặc định các test UI có API giả thay thế test tích hợp DB thật.

Luồng nghiệm thu:

1. Admin tạo trường, khoa/bộ môn, người dùng và phân công phạm vi.
2. Tạo CTĐT/version, học phần/PLO/CLO/mapping, assessment và policy.
3. Readiness phát hiện dữ liệu thiếu; sửa xong đủ điều kiện.
4. Mở lớp hai kỳ, ghi danh, nhập/import điểm, nộp/công bố theo quyền.
5. Kiểm tra điểm học phần/GPA không hồi quy.
6. Tính CLO/PLO, đối chiếu fixture và xem dashboard, so sánh kỳ.
7. Tạo snapshot, consumer lấy JSON/contract; xác minh cùng số liệu.
8. Phúc khảo hoặc sửa cấu hình qua luồng cho phép: snapshot cũ giữ nguyên, latest báo stale; tính lại tạo ID mới.
9. Thu hồi quyền người dùng rồi kiểm tra lại list/detail/export snapshot cũ.
10. Khởi động lại ứng dụng: dữ liệu, snapshot và audit vẫn đúng; migration DB cũ giữ nguyên lịch sử.

Checklist hoàn thành:

- [ ] UC01–UC18 có đường dùng thực tế và bằng chứng tương ứng.
- [ ] Không cần kết nối nhóm sinh viên còn lại.
- [ ] Công thức và dữ liệu thiếu có giải thích trên UI/tài liệu.
- [ ] Hai ngưỡng cá nhân/nhóm được phân biệt.
- [ ] Quyền Khoa/Bộ môn và quyền cá nhân có integration tests.
- [ ] Snapshot tái lập được và không bị ghi đè khi dữ liệu thay đổi.
- [ ] SV4 lấy được cấu trúc, assessment, kết quả OBE và reference làm minh chứng.
- [ ] SV5 lấy được học vụ/GPA/CLO/PLO có scope và nguồn, không phải tính lại.
- [ ] Migration thử trên DB rỗng và DB cũ; không phá điểm/báo cáo hiện có.
- [ ] Build, unit, integration, UI và consumer contract checks có kết quả ghi lại.
- [ ] Không đánh dấu kiểm thử chưa chạy là thành công.

## 11. Chỉ dẫn thực thi cho AI

- Thực hiện công việc đã được người dùng giao, không chỉ trả lời lại bằng kế hoạch.
- Giữ thay đổi nhỏ theo từng gói, hoàn thành cả backend, frontend, migration, caller và test cần thiết.
- Không dựng dữ liệu giả để che API chưa triển khai; demo phải tách khỏi dữ liệu vận hành.
- Không đặt công thức nghiệp vụ trong prompt LLM hoặc tính lại ở frontend.
- Không tự deploy, reset/xóa DB, ghi đè dữ liệu người dùng hoặc gửi thông tin ra dịch vụ bên ngoài. Migration destructive cần phương án bảo toàn/backup và quyền phù hợp.
- Không tự hạ tiêu chí nghiệm thu khi gặp khó. Nếu môi trường chặn kiểm chứng, ghi đúng bước bị chặn và tiếp tục phần độc lập có thể làm.
- Chỉ hỏi khi thiếu thông tin không thể suy ra và ảnh hưởng quyết định quan trọng; dùng mặc định mục 5 cho lựa chọn nghiệp vụ đã được kế hoạch giải quyết.
- Mỗi gói cập nhật status và bàn giao: thay đổi, test thực chạy, hạn chế còn lại, gói tiếp theo. Một phiên làm việc hết context phải lưu checkpoint đủ để phiên sau tiếp tục.
- Chỉ đánh dấu toàn bộ phần chung hoàn tất khi checklist mục 10 thỏa mãn; việc một vài gói hoàn thành không đồng nghĩa toàn dự án đạt.

## 12. Prompt giao việc

Sao chép đoạn sau khi AI có repository và file kế hoạch:

> Hãy triển khai toàn bộ phần chung của dự án theo `docs/KE_HOACH_HOAN_THANH_PHAN_CHUNG_CHO_AI.md`. Nhóm chỉ có SV4 và SV5, làm hệ thống độc lập. Phần chung bao gồm nghiệp vụ SV1–3 không có AI, nền tảng và contract dữ liệu cho SV4/SV5. Tôi chấp nhận dùng các mặc định thiết kế trong kế hoạch làm cấu hình ban đầu của đồ án; không coi chúng là quy định bắt buộc của AUN-QA. Đọc hướng dẫn repository và mã nguồn hiện tại, xác minh baseline rồi thực hiện G0–G7. Tái sử dụng phần đã có, bảo toàn dữ liệu và thay đổi của người dùng. Hoàn thiện backend, frontend, migrations, tests và tài liệu liên quan. Không chỉ lập lại kế hoạch; hãy triển khai và kiểm chứng. Không triển khai thay phần nghiệp vụ riêng SV4/SV5 ngoài adapter/contract và kiểm thử tích hợp cần thiết. Cập nhật `docs/SHARED_IMPLEMENTATION_STATUS.md` sau mỗi gói, ghi rõ lệnh đã chạy và kết quả, không báo hoàn tất khi thiếu kiểm chứng bắt buộc. Nếu chưa thể xong trong một phiên, để checkpoint chính xác cho phiên tiếp theo.

Prompt tiếp tục khi đã có checkpoint:

> Đọc kế hoạch và `docs/SHARED_IMPLEMENTATION_STATUS.md`, kiểm tra working tree hiện tại, tiếp tục từ gói chưa hoàn thành. Giữ các quyết định đã chốt và phần đã kiểm chứng; chỉ chạy lại kiểm thử khi thay đổi/phụ thuộc mới đòi hỏi. Hoàn thành các tiêu chí còn thiếu và cập nhật checkpoint.
