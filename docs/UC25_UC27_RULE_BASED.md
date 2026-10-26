# UC25–UC27: báo cáo dữ liệu cấu trúc

Cập nhật 26/10/2026. Phạm vi điều chỉnh đã được người dùng xác nhận giảng viên đồng ý: báo cáo thiết kế CTĐT, không tính điểm hoặc tỷ lệ sinh viên đạt chuẩn. Ba UC chạy bằng quy tắc xác định từ Curriculum, không gọi AI hoặc phụ thuộc hệ thống nhóm SV2+SV3.

## Chức năng đã triển khai

| UC | Đầu vào | Đầu ra |
|---|---|---|
| UC25 | Chương trình, phiên bản, một học phần | Thông tin học phần, CLO/PLO, ma trận, mapping, các trường hợp thiếu liên kết |
| UC26 | Phiên bản; tùy chọn một học phần | Danh sách CLO, ma trận CLO–PLO, CLO chưa mapping và độ bao phủ trong phạm vi |
| UC27 | Toàn bộ một phiên bản | Tổng hợp PLO, số học phần/CLO hỗ trợ, ma trận học phần–PLO, điểm phân bố tín chỉ × trọng số |

Cả ba UC có giao diện `/reporting`, xem trước, lịch sử phân trang, mở lại bằng URL `?report=<uuid>`, tải DOCX/PDF tiếng Việt và thu hồi. Báo cáo luôn ghi **chưa phê duyệt**. Quy trình sửa nội dung/gửi duyệt thuộc UC28, chưa triển khai trong đợt này.

Các chỉ số dùng CLO/học phần phân biệt theo ID, không gộp các CLO cùng mã ở hai học phần. CLO có nhiều PLO không bị đếm trùng trong tỷ lệ CLO có mapping. PLO được CLO hỗ trợ chỉ tính khi có đường liên kết CLO → học phần–PLO hợp lệ; chỉ tồn tại học phần–PLO chưa đủ. Mẫu số bằng 0 hiển thị “Chưa có dữ liệu”. Học phần/CLO đã lưu trữ không nằm trong lần tạo mới.

Giữ nguyên X/Y và I/R/E của source. X=2, Y=1 ở cấp học phần–PLO; điểm phân bố = tổng tín chỉ × trọng số X/Y, không nhân I/R/E. CLO–PLO chỉ thể hiện có/không liên kết. Những chỉ số này không đánh giá chất lượng diễn đạt CLO, mức phù hợp học thuật hay kết quả kiểm định.

## Chạy trên máy phát triển

Thực hiện tại thư mục gốc repository. Cần môi trường .NET 10, Node/npm và PostgreSQL theo hướng dẫn gốc.

1. Áp dụng migration lên database đang dùng (thay tham số kết nối bằng cấu hình của bạn):

   ```powershell
   $env:ConnectionStrings__Postgres = "Host=localhost;Port=5432;Database=obe_management;Username=postgres;Password=YOUR_PASSWORD"
   dotnet run --project tools/database -- baseline-existing
   dotnet run --project tools/database -- migrate
   ```

   Database trống chỉ cần `migrate`. Database cũ được kiểm tra cấu trúc trước khi ghi baseline; schema Reporting chưa có sẽ được tạo bằng EF. Không đổi thang mapping hay xóa dữ liệu Curriculum. Xem [hướng dẫn EF Core](EF_CORE_ARCHITECTURE.md). Docker dùng service `database-init` chạy EF migrations và seed C#; không xóa volume để nâng cấp.

2. Cấp mã truy cập cho một người và các **ID chương trình** được phép:

   ```powershell
   powershell -NoProfile -ExecutionPolicy Bypass -File tools/New-ReportingAccess.ps1 -UserId sv5 -ProgramIds 1
   powershell -NoProfile -ExecutionPolicy Bypass -File tools/New-ReportingAccess.ps1 -UserId reviewer -ProgramIds 1 -ReadOnly
   ```

   Lưu `AccessToken` được trả về để nhập trên giao diện. Script sinh khóa ngẫu nhiên 256 bit, chỉ lưu SHA-256 trong `apps/api/reporting-access.local.json` (đã được gitignore), không lưu khóa rõ. Không dùng khóa mẫu trong test làm khóa thật. Script từ chối ghi đè người dùng trùng ID. Xóa entry cũ trước khi cấp lại khóa sẽ vô hiệu hóa khóa cũ.

   Với nhiều chương trình, chạy trong PowerShell đã cho phép script ở scope Process: `& .\tools\New-ReportingAccess.ps1 -UserId sv5 -ProgramIds 1,2`. Tránh mở transcript khi cấp khóa. Không đưa khóa vào URL, mã nguồn hoặc ảnh chụp demo.

3. Chạy ứng dụng:

   ```powershell
   dotnet restore ObeAunQa.slnx
   dotnet run --project apps/api/ObeAunQa.Api.csproj
   ```

   Ở terminal thứ hai:

   ```powershell
   cd apps/web
   npm.cmd ci
   npm.cmd run dev
   ```

   Cấu hình PostgreSQL theo `ConnectionStrings:Postgres` hiện có. Frontend dùng `VITE_API_URL` (mặc định dev `http://localhost:5099`); API cho phép CORS `http://localhost:5173` mặc định. Mở `http://localhost:5173/reporting`, nhập khóa, chọn chương trình/phiên bản/học phần và tạo báo cáo.

## Quyền, snapshot và truy vết

Repository chưa có IAM dùng chung. Adapter khóa truy cập này chỉ bảo vệ **API báo cáo**; nó không thay thế đăng nhập, vai trò và bảo vệ các API Curriculum hiện có. Cần tích hợp IAM và bảo vệ toàn bộ API trước khi triển khai hệ thống cho người dùng thực tế. Khi dùng ngoài localhost phải cấu hình HTTPS.

Cấu hình tương đương cho môi trường triển khai:

```json
{
  "Reporting": {
    "Access": {
      "Users": [
        { "Id": "sv5", "TokenSha256": "<64-hex-SHA256-of-random-key>", "ProgramIds": [1], "CanGenerate": true }
      ]
    }
  }
}
```

API đọc lại quyền trong mỗi request. Không có cấu hình hợp lệ thì từ chối truy cập. `CanGenerate=false` chỉ cho xem/tải trong các chương trình được cấp; `true` thêm quyền tạo và thu hồi mọi báo cáo trong phạm vi đó. Người khác không được xem snapshot bằng cách đoán UUID. Khóa trên trình duyệt chỉ ở bộ nhớ; tải lại trang cần kết nối lại. File cấu hình local hỗ trợ reload; biến môi trường `Reporting__Access__Users__0__...` có ưu tiên cao hơn và cần khởi động lại khi thay đổi. Với Docker, mount file local vào content root API hoặc dùng biến môi trường, không bake khóa vào image.

Nguồn Curriculum được đọc trong transaction Repeatable Read. Lưu toàn bộ snapshot, phiên bản quy tắc và SHA-256 cùng kết quả tính; xem lại/tải lại sử dụng snapshot đã lưu, không tính lại theo dữ liệu hiện tại. Snapshot lịch sử không tự đổi khi Curriculum được sửa/lưu trữ. Tạo báo cáo mới để phản ánh dữ liệu mới; dùng thu hồi để chặn truy cập bản cũ nếu không còn được phép phát hành. Hash phục vụ đối chiếu dữ liệu, không phải chữ ký số hoặc cơ chế chống sửa database bởi quản trị viên.

`reporting.audit_events` ghi tạo/xem/tải DOCX/tải PDF/thu hồi kèm người thao tác. Báo cáo thu hồi không còn trong danh sách và trả 404 khi mở/tải. Quyền hiện tại tiếp tục áp dụng cho các snapshot cũ. File đã tải về thiết bị người nhận không thể thu hồi từ xa. Endpoint tạo/tải giới hạn 20 request/phút/người; phản hồi báo cáo có `Cache-Control: no-store`.

Nguồn hiện tại chỉ là bản ghi Curriculum có ID/provenance. **Chưa tích hợp file minh chứng SV4, citation theo trang, pipeline RAG hoặc sự kiện vô hiệu hóa minh chứng.** Báo cáo ghi rõ “chưa tích hợp”, không suy ra thiếu minh chứng và không cung cấp kết luận AUN-QA. Các điều kiện P0 của pipeline minh chứng vẫn phải làm khi kết nối SV4.

## API chính

Mọi endpoint dưới đây cần `Authorization: Bearer <access-token>`.

| Method | Đường dẫn dưới `/api/reporting` | Nội dung |
|---|---|---|
| GET | `/session`, `/programs` | Quyền hiện tại và danh sách được phép |
| GET | `/programs/{id}/versions`, `/versions/{id}/courses` | Lựa chọn phạm vi dữ liệu |
| POST | `/reports` | JSON `{ "kind": "course", "versionId": 2, "programCourseId": 123 }` |
| GET | `/reports?page=1&pageSize=10` | Lịch sử; pageSize 1–100 |
| GET | `/reports/{uuid}` | Nội dung và snapshot nguồn |
| GET | `/reports/{uuid}/download?format=docx` | Xuất DOCX; đổi thành `pdf` để xuất PDF |
| POST | `/reports/{uuid}/revoke` | Thu hồi; response 204 |

`kind` là `course`, `clo` hoặc `plo`. `course` bắt buộc `programCourseId`; `clo` cho phép null; `plo` yêu cầu null. ID học phần phải thuộc phiên bản được chọn. 400: request sai; 401: khóa thiếu/sai; 403: thiếu quyền; 404: dữ liệu/báo cáo không khả dụng hoặc ngoài phạm vi; 409: dữ liệu nguồn không hợp lệ; 429: giới hạn tần suất.

## Kiểm thử và giới hạn kiểm chứng

```powershell
dotnet test ObeAunQa.slnx
cd apps/web
npm.cmd run build
```

Đã chạy 28 test .NET (16 Curriculum + 12 Reporting) thành công, build backend không warning/error và build frontend thành công. Test Reporting kiểm tra công thức, dữ liệu trùng/mẫu số 0, lọc dữ liệu sai phạm vi, snapshot, quyền và định dạng xuất.

Smoke test thật: `tests/contracts/reporting_api_smoke.py`, chỉ dùng Python standard library; cấu hình `REPORT_TEST_URL`, `REPORT_TEST_AUTHOR`, `REPORT_TEST_READER`, `REPORT_TEST_OUTSIDER`. Chạy **chỉ trên database thử nghiệm có seed V1/V2**, vì script tạo/thu hồi báo cáo. Ba khóa tương ứng người có quyền tạo chương trình CS, người chỉ xem CS, người ngoài chương trình CS. Đã xác minh V2 có 59 học phần, 155 CLO, 210 liên kết CLO–PLO; sáu file DOCX/PDF, quyền, validation, lịch sử và thu hồi đều qua.

Smoke giao diện: `tests/contracts/reporting_ui_smoke.py`, cần Python với `playwright` và Microsoft Edge đã cài. Dùng `REPORT_TEST_AUTHOR`, `REPORT_TEST_READER` như trên và `REPORT_TEST_WEB_URL` (mặc định `http://localhost:5173`). API thử nghiệm phải cho phép CORS của địa chỉ web đó. Khóa author cần quyền trên chương trình ID 1, seed V2 ID 2. Tránh chạy đồng thời hai smoke script bằng cùng một khóa vì có giới hạn tần suất.

Đã kiểm tra trình duyệt Edge headless: tạo ba loại, sáu lượt tải file, tài khoản chỉ xem, không lưu khóa localStorage, viewport điện thoại 390px không tràn ngang và không có lỗi JavaScript. DOCX đã kiểm tra ZIP/XML và đọc bằng python-docx; Word COM trên phiên máy kiểm thử bị treo nên chưa xác nhận bố cục bằng Word. PDF được render, kiểm tra giới hạn trang/văn bản và kiểm tra trực quan. Dữ liệu thử nghiệm ở PostgreSQL riêng cổng 55432; chưa chạy migration trên database hiện hữu của người dùng.

PDF sử dụng PDFsharp/MigraDoc và nhúng Noto Sans để giữ tiếng Việt trên server. Font có giấy phép SIL OFL trong `modules/sv5/reporting/backend/assets/fonts/OFL.txt`; DOCX dùng Arial trên máy mở file. Các dependency Python/Playwright dùng để kiểm thử không phải dependency runtime của ứng dụng.
