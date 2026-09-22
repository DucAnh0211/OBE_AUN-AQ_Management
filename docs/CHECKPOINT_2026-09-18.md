# Checkpoint tiến độ đồ án - 18/09/2026

## 1. Tóm tắt

| Phạm vi | Tiến độ ước tính | Trạng thái |
|---|---:|---|
| Toàn bộ đồ án | **35%** | Đã có nền tảng chạy được và một phần Curriculum |
| Phần dùng chung Curriculum | **70%** | CTĐT, phiên bản và học phần đã xong; API/UI PLO-CLO còn thiếu |
| Module SV4 - Accreditation | **5%** | Mới có scaffold, health và capability endpoints |
| Module SV5 - Reporting | **5%** | Mới có scaffold, health và capability endpoints |
| AI service | **5%** | Mới có khung ứng dụng, chưa có nghiệp vụ AI |

Phần trăm trên đo theo **chức năng đã có trong code**, không tính riêng thời gian nghiên cứu tài liệu. Đây là ước lượng quản lý tiến độ, không phải số liệu được Git tự động tính.

## 2. Cách tính tiến độ toàn đồ án

| Nhóm công việc | Trọng số | Đã đạt | Giải thích |
|---|---:|---:|---|
| Nền tảng dự án | 10% | 10% | ASP.NET Core, React, PostgreSQL, module registry, Swagger và CORS đã có |
| Curriculum dùng chung | 30% | 21% | Hoàn thành khoảng 70% phạm vi Curriculum |
| Accreditation - SV4 | 35% | 2% | Có module scaffold nhưng chưa có UC19-UC24 |
| Reporting - SV5 | 15% | 1% | Có module scaffold nhưng chưa có nghiệp vụ báo cáo |
| AI và tích hợp cuối | 10% | 1% | Có app AI khởi tạo, chưa có pipeline xử lý |
| **Tổng** | **100%** | **35%** | Làm tròn từ tiến độ các nhóm |

## 3. Những phần đã hoàn thành

### 3.1. Nền tảng dự án

- [x] Khởi tạo solution ASP.NET Core 10.
- [x] Khởi tạo React 19, TypeScript và Vite.
- [x] Chia module `curriculum`, `accreditation`, `reporting`.
- [x] Kết nối PostgreSQL bằng Npgsql và Dapper.
- [x] Có OpenAPI và Swagger UI.
- [x] Có CORS cho frontend `http://localhost:5173`.
- [x] Có cấu hình chạy backend tại `http://localhost:5099`.
- [x] Có module registry cho backend và frontend.

### 3.2. Curriculum - CTĐT, phiên bản và học phần

- [x] CRUD chương trình đào tạo.
- [x] Tìm kiếm và phân trang CTĐT.
- [x] Xóa mềm CTĐT.
- [x] CRUD phiên bản CTĐT.
- [x] Trạng thái `draft`, `finalized`, `archived`.
- [x] Sao chép học phần khi tạo phiên bản mới.
- [x] Công bố và khóa phiên bản.
- [x] CRUD học phần trong phiên bản nháp.
- [x] Tìm kiếm, lọc học kỳ và phân trang học phần.
- [x] Kiểm tra mã học phần trùng trong một phiên bản.
- [x] API trả lỗi theo Problem Details.
- [x] Giao diện tiếng Việt cho CTĐT, phiên bản và học phần.
- [x] API client TypeScript kết nối backend.

### 3.3. Dữ liệu PLO-CLO

- [x] Có schema cho PLO, CLO, `course_plos` và `clo_plos`.
- [x] Có dữ liệu V1 làm nguồn.
- [x] Có bộ sinh dữ liệu V2.
- [x] Chỉ sử dụng 5 PLO chính thức.
- [x] Đảm bảo số PLO tối thiểu theo tín chỉ.
- [x] Một PLO có thể có nhiều CLO.
- [x] Một CLO có thể liên kết nhiều PLO.
- [x] Không còn CLO không có PLO.
- [x] Không còn PLO của môn chưa được CLO hỗ trợ.
- [x] Phân bố PLO nằm trong giới hạn lệch 20%.
- [x] Có JSON, SQL seed và báo cáo kiểm tra.
- [x] Có tài liệu hướng dẫn sinh lại database.

Số liệu V2 hiện tại:

| Dữ liệu | Số lượng |
|---|---:|
| PLO | 5 |
| Học phần | 59 |
| Mapping học phần-PLO | 151 |
| CLO | 155 |
| Mapping CLO-PLO | 210 |
| CLO liên kết nhiều PLO | 55 |

### 3.4. Kiểm thử hiện tại

- [x] `dotnet test ObeAunQa.slnx`: **11/11 test đạt**.
- [x] `npm run build`: **thành công**.
- [x] TypeScript và Vite build không lỗi.
- [x] Báo cáo sinh dữ liệu V2 không có vi phạm ràng buộc.
- [ ] Chưa có integration test chạy API với PostgreSQL thật trong pipeline tự động.
- [ ] Chưa có kiểm thử end-to-end frontend-backend.

## 4. Curriculum còn thiếu gì?

Phần Curriculum mới hoàn thành API và giao diện cho CTĐT, phiên bản và học phần. Các phần sau vẫn cần làm:

- [x] API xem danh sách PLO theo phiên bản CTĐT.
- [x] API CRUD PLO trong phiên bản nháp.
- [x] API xem CLO theo học phần.
- [x] API CRUD CLO trong phiên bản nháp.
- [x] API quản lý mapping học phần-PLO.
- [x] API quản lý mapping CLO-PLO.
- [x] API kiểm tra số PLO theo tín chỉ.
- [x] API xem điểm cân bằng PLO.
- [ ] Màn hình danh sách PLO.
- [ ] Màn hình CLO theo học phần.
- [ ] Màn hình ma trận học phần-PLO.
- [ ] Màn hình ma trận CLO-PLO.
- [ ] Giao diện cảnh báo dữ liệu không hợp lệ.

Điều kiện để Curriculum được coi là hoàn thành: SV4 và SV5 có thể đọc CTĐT, học phần, PLO, CLO và mapping thông qua public API mà không truy cập trực tiếp bảng nội bộ.

## 5. Checkpoint riêng của SV4 - Accreditation

### 5.1. Tiến độ theo yêu cầu của giảng viên

| Hạng mục | Trọng số trong SV4 | Tiến độ hiện tại |
|---|---:|---:|
| Khung module, health và capabilities | 5% | 5% |
| UC19 - Quản lý bộ tiêu chí AUN-QA | 15% | 0% |
| UC20 - Quản lý minh chứng | 25% | 0% |
| UC21 - Gắn minh chứng với tiêu chí | 15% | 0% |
| UC22 - Gắn minh chứng với học phần/CTĐT | 10% | 0% |
| UC23 - Tìm kiếm minh chứng | 10% | 0% |
| UC24 - Theo dõi tình trạng hồ sơ | 10% | 0% |
| AI Evidence Assistant | 10% | 0% |
| **Tổng phần SV4** | **100%** | **5%** |

### 5.2. Những gì SV4 đã có

- [x] Thư mục module `modules/sv4/accreditation`.
- [x] Project backend Accreditation.
- [x] Endpoint `/api/accreditation/health`.
- [x] Endpoint `/api/accreditation/capabilities`.
- [x] Module được đăng ký vào backend và frontend.
- [x] Đã xác định phạm vi UC19-UC24.
- [x] Đã có một báo cáo tự đánh giá AUN-QA 111 trang để làm dữ liệu thử nghiệm.
- [x] Đã xác định báo cáo này là chỉ mục/dẫn chiếu minh chứng, không phải bộ minh chứng đầy đủ.

### 5.3. Những gì SV4 chưa có

- [ ] Migration cho dữ liệu Accreditation.
- [ ] Bảng bộ tiêu chuẩn, tiêu chuẩn và tiêu chí AUN-QA.
- [ ] Seed 8 tiêu chuẩn và 52 tiêu chí.
- [ ] Kho file minh chứng.
- [ ] Upload, download, sửa và xóa mềm minh chứng.
- [ ] Mapping tiêu chí-minh chứng.
- [ ] Mapping minh chứng-CTĐT/học phần.
- [ ] Tìm kiếm và bộ lọc minh chứng.
- [ ] Trạng thái hồ sơ kiểm định.
- [ ] Danh sách minh chứng còn thiếu.
- [ ] Trích xuất nội dung PDF, Word và Excel.
- [ ] AI phân loại minh chứng.
- [ ] AI tóm tắt minh chứng.
- [ ] AI gợi ý tiêu chí phù hợp.
- [ ] AI gợi ý tài liệu còn thiếu.
- [ ] Giao diện cho UC19-UC24.
- [ ] Test backend, frontend và AI của Accreditation.

### 5.4. Mức độ sẵn sàng về phân tích

Phần phân tích nghiệp vụ SV4 đã rõ hơn phần implementation:

- Đã chốt tên module: **Evidence & AUN Accreditation Management**.
- Đã chốt 6 use case UC19-UC24.
- Đã chốt vai trò AI Evidence Assistant.
- Đã xác định dữ liệu đầu vào từ Curriculum.
- Đã xác định tài liệu AUN-QA hiện có còn thiếu nhiều minh chứng gốc.

Có thể xem phần **phân tích và thiết kế sơ bộ của SV4 đạt khoảng 60%**, nhưng code nghiệp vụ mới đạt khoảng **5%**.

## 6. Reporting - SV5

- [x] Có module scaffold.
- [x] Có health và capability endpoints.
- [ ] Chưa có dashboard nghiệp vụ.
- [ ] Chưa có API thống kê.
- [ ] Chưa có báo cáo theo CTĐT và phiên bản.
- [ ] Chưa có báo cáo tình trạng AUN-QA.
- [ ] Chưa có xuất Excel, Word hoặc PDF.
- [ ] Chưa có grounded chat và citation.
- [ ] Chưa có AI sinh báo cáo.

Reporting đang chờ public API của Curriculum và dữ liệu đầu ra của Accreditation.

## 7. Mốc tiếp theo nên thực hiện

### Mốc A - Hoàn thiện public API Curriculum

1. Làm API đọc PLO theo phiên bản.
2. Làm API đọc CLO theo học phần.
3. Làm API đọc mapping học phần-PLO và CLO-PLO.
4. Làm API kiểm tra độ bao phủ và cân bằng.
5. Viết integration test với PostgreSQL.

**Kết quả:** Accreditation có thể lấy toàn bộ dữ liệu cần thiết qua API.

### Mốc B - Hoàn thành UC19

1. Tạo migration `accreditation`.
2. Tạo bảng `aun_frameworks`, `aun_standards`, `aun_criteria`.
3. Seed 8 tiêu chuẩn và 52 tiêu chí.
4. Làm API xem cây tiêu chí.
5. Làm màn hình Bộ tiêu chí AUN-QA.

**Kết quả:** người dùng xem được đầy đủ bộ tiêu chí trong hệ thống.

### Mốc C - Hoàn thành UC20

1. Tạo bảng `evidence_documents` và metadata file.
2. Làm upload, download và xóa mềm.
3. Kiểm tra trùng file bằng hash.
4. Làm màn hình Kho minh chứng.

**Kết quả:** người dùng quản lý được tài liệu kiểm định.

### Mốc D - Hoàn thành UC21 và UC22

1. Tạo mapping tiêu chí-minh chứng.
2. Tạo mapping minh chứng-phiên bản CTĐT.
3. Tạo mapping minh chứng-học phần.
4. Hiển thị mapping theo hai chiều.

**Kết quả:** biết minh chứng hỗ trợ tiêu chí nào và thuộc đối tượng Curriculum nào.

### Mốc E - Hoàn thành UC23, UC24 và AI

1. Tìm kiếm và lọc minh chứng.
2. Khai báo loại tài liệu cần có cho mỗi tiêu chí.
3. Tính trạng thái hồ sơ.
4. Hiển thị danh sách còn thiếu.
5. Trích xuất nội dung tài liệu.
6. AI phân loại, tóm tắt và gợi ý.

**Kết quả:** hoàn thành phạm vi riêng của SV4.

## 8. Tiêu chí cập nhật checkpoint

Sau mỗi sprint, cập nhật:

1. Ngày checkpoint.
2. Commit gần nhất.
3. Checkbox đã hoàn thành.
4. Số endpoint thực tế.
5. Số test đạt/thất bại.
6. Tình trạng build frontend/backend.
7. Dữ liệu mẫu đã seed.
8. Các rủi ro hoặc phụ thuộc đang chặn.
9. Phần trăm chỉ tăng khi chức năng đã có code và kiểm thử cơ bản.

## 9. Bằng chứng tại checkpoint này

- Commit gần nhất: `42851a8 feat: triển khai phiên bản chương trình đào tạo`.
- Backend: 11/11 test đạt ngày 18/09/2026.
- Frontend: production build thành công ngày 18/09/2026.
- V2: 59 học phần, 5 PLO, 155 CLO và 210 mapping CLO-PLO.
- Accreditation và Reporting vẫn được README xác nhận là `scaffold-only`.
