# Knowledge Base cho admin

## Luồng và phân công

Frontend `/admin/knowledge` → API .NET `/api/knowledge` → Python `/internal/knowledge/process`.

- **SV4 / Accreditation (.NET):** metadata, file gốc bất biến trên MinIO, checksum SHA-256, người tải lên, thời điểm/người vô hiệu hóa. Mỗi lần upload có ID/object key riêng; chưa có thao tác thay thế nội dung hay nhóm nhiều revision của một tài liệu.
- **SV5 / Knowledge (.NET):** kiểm tra quyền admin, hàng đợi bền vững bằng EF, gọi Python, kiểm tra kết quả, lưu chunks/embedding và trạng thái trong schema `knowledge`.
- **SV5 / Python:** trích xuất PDF/DOCX/TXT/Markdown, OCR PDF scan bằng Tesseract, chunking, gọi OpenAI Embeddings API. Toàn bộ thuật toán xử lý tài liệu/AI nằm ở `modules/sv5/knowledge/ai`.
- **Frontend:** tải file và theo dõi tiến độ qua .NET. Không nhận API key OpenAI, khóa MinIO hoặc khóa nội bộ Python.

Python không tự sửa database và không quyết định quyền tài khoản. Endpoint nội bộ yêu cầu `X-Service-Key`. Quản lý Knowledge Base chỉ dành cho admin; chatbot và nguồn trích dẫn cho admin/giảng viên, chặn sinh viên và token Reporting riêng. Các policy yêu cầu đã đổi mật khẩu bắt buộc. Xem [Knowledge Chat](KNOWLEDGE_CHAT.md).

## Các chức năng đã có

1. Tải một file tối đa 20 MiB lên bucket MinIO. Bucket tạo mới mặc định private; bucket có sẵn phải do quản trị viên cấu hình private.
2. Chọn xử lý ngay sau upload hoặc bấm **Xử lý** sau. Nếu Python/API key chưa sẵn sàng, bản gốc vẫn lưu thành công.
3. Theo dõi `uploaded → queued → processing → ready/failed`. UI tự cập nhật mỗi 4 giây, có **Thử lại** với job lỗi.
4. Xem chunks và vị trí nguồn, model, số chiều embedding; tải bản gốc qua API có phân quyền.
5. **Vô hiệu hóa:** nguồn không được tải/đọc/xử lý tiếp; xóa chunks, giữ metadata và bản gốc để lưu vết. Không phải xóa vật lý khỏi MinIO.

Mỗi job có lease 15 phút, timeout xử lý 10 phút; worker cũ không ghi đè kết quả của worker mới. Sau crash, job được nhận lại khi lease hết hạn, tối đa 3 lần rồi cần admin thử lại. Một worker xử lý một tài liệu mỗi lần. Python giới hạn một job đồng thời trong mỗi process.

Kết quả chỉ được công bố khi toàn bộ chunks và vectors hợp lệ, lưu trong một transaction EF. Nguồn được kiểm tra lại trước khi lưu và trước khi trả chunks/bản gốc. Nếu việc dọn chunks lỗi sau vô hiệu hóa, kiểm tra nguồn vẫn chặn truy cập.

## Chạy local: MinIO Docker, .NET/Python/React bằng lệnh

Chạy từ thư mục gốc repository. Các lệnh dưới đây dùng Windows PowerShell.

### 1. `.env`

Giữ các giá trị riêng đang dùng. Bổ sung/kiểm tra:

```dotenv
# Trỏ đúng database bạn muốn nâng cấp; cổng 5432 local hoặc 5433 Docker.
ConnectionStrings__Postgres='Host=localhost;Port=5433;Database=obe_management;Username=postgres;Password=YOUR_DATABASE_PASSWORD'
OBJECT_STORAGE_ENDPOINT=http://localhost:9000
OBJECT_STORAGE_BUCKET=obe-knowledge
OBJECT_STORAGE_ACCESS_KEY=YOUR_MINIO_USER
OBJECT_STORAGE_SECRET_KEY=YOUR_MINIO_PASSWORD
AI_SERVICE_URL=http://localhost:8000
AI_SERVICE_KEY=YOUR_RANDOM_SHARED_KEY_AT_LEAST_32_CHARACTERS
LLM_API_KEY=YOUR_OPENAI_API_KEY
EMBEDDING_MODEL=text-embedding-3-small
OCR_TESSERACT_PATH='C:/Program Files/Tesseract-OCR/tesseract.exe'
OCR_LANGUAGES=vie+eng
```

`AI_SERVICE_KEY` giống nhau ở .NET và Python; `.env` được Git bỏ qua. Python đọc `.env` gốc bằng python-dotenv. `tools/dev.ps1` đọc `.env` như dữ liệu, ánh xạ JWT/admin sang biến .NET; không thực thi nội dung `.env`. Script chỉ hỗ trợ dòng `KEY=value` đơn, có hoặc không có dấu nháy, không hỗ trợ multiline/interpolation/inline comment.

### 2. MinIO

```powershell
docker compose up -d minio
```

API `http://localhost:9000`, console `http://localhost:9001`, đăng nhập bằng `OBJECT_STORAGE_ACCESS_KEY` / `OBJECT_STORAGE_SECRET_KEY`. Volume `minio_data` lưu file qua các lần khởi động.

Máy hiện tại đã có image MinIO mà Compose dùng mặc định; image này đã được kiểm thử upload/download trên container riêng. Registry của image cũ trả lỗi truy cập tại thời điểm triển khai, nên máy mới chưa có image cần chuẩn bị image tin cậy. Có thể build từ [release upstream cố định](https://github.com/minio/minio/releases/tag/RELEASE.2025-10-15T17-29-55Z):

```powershell
docker build -f infra/minio/Dockerfile -t obe-minio:local .
```

Sau đó đặt `MINIO_IMAGE=obe-minio:local` trong `.env`. Đây là phương án bổ sung cho máy chưa có image; lần build đầu tải và biên dịch nhiều dependency Go, có thể cần nhiều thời gian/RAM. Dockerfile build từ source chưa được xác nhận build hoàn tất trên máy 8 GB RAM này; kiểm thử chức năng dùng image MinIO có sẵn. Không lấy image từ mirror không rõ nguồn.

### 3. Python và OCR

Python 3.11 hoặc 3.12:

```powershell
python -m venv apps/ai-service/.venv
apps/ai-service/.venv/Scripts/python.exe -m pip install -r apps/ai-service/requirements.txt
```

Cài Tesseract trên máy chạy Python theo [hướng dẫn Tesseract cho Windows](https://tesseract-ocr.github.io/tessdoc/Installation.html), kèm dữ liệu ngôn ngữ `vie` và `eng`. Đặt `OCR_TESSERACT_PATH` đúng executable. Không cần cài Poppler; Python dùng PDFium từ pip để render trang PDF.

```powershell
& 'C:/Program Files/Tesseract-OCR/tesseract.exe' --list-langs
powershell -ExecutionPolicy Bypass -File tools/dev.ps1 ai
```

Tesseract phải liệt kê cả `vie` và `eng`. Nếu chưa cài, tài liệu có lớp văn bản vẫn xử lý được; PDF cần OCR sẽ báo lỗi có thể thử lại. Docker image AI có sẵn Tesseract và cả hai ngôn ngữ, nhưng không bắt buộc chạy Python bằng Docker.

### 4. Database, backend, frontend

Sao lưu database đang sử dụng trước khi chạy migration. Kiểm tra kỹ `ConnectionStrings__Postgres` (Windows PostgreSQL và Docker PostgreSQL là hai database khác nhau).

```powershell
powershell -ExecutionPolicy Bypass -File tools/dev.ps1 database initialize
```

Lệnh trên bổ sung `accreditation.source_documents`, schema `knowledge` và lịch sử migration tương ứng. Không sửa các initial migration cũ; database legacy vẫn đi qua kiểm tra baseline hiện có.

Mở hai terminal riêng:

```powershell
powershell -ExecutionPolicy Bypass -File tools/dev.ps1 api
```

```powershell
npm.cmd --prefix apps/web ci
powershell -ExecutionPolicy Bypass -File tools/dev.ps1 web
```

Mở `http://localhost:5173`, đăng nhập admin → **Knowledge Base**. Nếu API/web Docker đang dùng cổng 5099/8080, dừng riêng hai container đó hoặc chọn `API_PORT` khác; không cần dừng/xóa database.

Nếu muốn chạy tất cả bằng Docker: `docker compose --profile ai --profile storage up --build -d`. .NET trong Docker gọi `http://ai:8000`, MinIO tại `http://minio:9000`; đây là địa chỉ nội bộ, khác địa chỉ localhost khi chạy trực tiếp.

## Giới hạn và hành vi xử lý

- PDF tối đa 500 trang; văn bản tối đa 1,5 triệu ký tự; tối đa 2.000 chunks.
- PDF tự OCR trang có dưới 30 ký tự chữ/số. Chọn **OCR toàn bộ PDF** nếu lớp chữ sẵn có sai hoặc chỉ phủ một phần trang. OCR không đảm bảo chính xác tuyệt đối; UI hiển thị cảnh báo để kiểm tra chunks.
- DOCX lấy đoạn văn và ô bảng trong nội dung chính; chưa OCR ảnh nhúng DOCX hoặc trích footnotes/header/footer.
- TXT/Markdown dùng UTF-8. Chunk tối đa 1.200 Unicode codepoints, overlap 150 ký tự trong cùng trang/đoạn; lưu locator và offset. Chưa có chunking nhận biết bố cục bảng/phân cấp tiêu đề.
- OpenAI nhận **văn bản chunks**, không nhận file nhị phân. Model lấy từ cấu hình Python; mặc định `text-embedding-3-small`. Hợp đồng theo [OpenAI Embeddings API](https://developers.openai.com/api/reference/resources/embeddings/methods/create).
- Chunks/embedding lưu PostgreSQL bằng EF, vector kiểu `real[]`. Chatbot dùng lọc tiêu đề/tên file → cosine trong Python → LLM reranking → trả lời có trích dẫn. Chưa có pgvector index hoặc dùng KB cho báo cáo UC25–27. Trạng thái **Hoàn tất** nghĩa là đã tạo và lưu embedding.
- OCR chạy cục bộ bằng Tesseract, không gọi vision API. Chưa tự tải/cài Tesseract vào Windows của người dùng.

## Kiểm thử

```powershell
dotnet test ObeAunQa.slnx
python tests/contracts/architecture_check.py
$env:PYTHONPATH='apps/ai-service'
apps/ai-service/.venv/Scripts/python.exe -m unittest discover -s apps/ai-service/tests -v
npm.cmd --prefix apps/web run build
```

Test OCR thật tự bỏ qua nếu máy không có Tesseract vie+eng; đã chạy được trong image AI. Các test embedding mock provider để không dùng quota thật.

`tests/contracts/knowledge_api_smoke.py` và `knowledge_ui_smoke.py` chỉ dành cho stack thử riêng. Python fixture `tests/smoke_app.py` dùng extraction/OCR thật và embedding giả lập; không được dùng fixture này trong vận hành. Không có chế độ embedding giả trong production entrypoint.
