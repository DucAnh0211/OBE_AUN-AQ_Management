# AI service Python

Thư mục này chỉ là host triển khai Python (FastAPI, Docker, dependencies). Mã nghiệp vụ AI thuộc **SV5**, đặt tại [modules/sv5/knowledge/ai](../../modules/sv5/knowledge/ai). SV4 quản lý file gốc, quyền và metadata qua .NET. Xem [cấu trúc agent và tools](../../modules/sv5/knowledge/ai/README.md).

Knowledge Base đã triển khai: PDF/DOCX/TXT/Markdown → OCR nếu cần → chunking → OpenAI embedding. .NET gọi API nội bộ và lưu kết quả bằng EF; frontend chỉ gọi .NET có JWT.

Xem [hướng dẫn Knowledge Base](../../docs/KNOWLEDGE_BASE.md) để cấu hình, chạy local và kiểm thử.

Endpoint:

- POST /internal/knowledge/agent: LangGraph phân luồng xã giao/tra cứu, tiếp tục từ checkpoint lưu qua EF Core ở .NET. Cần `DOTNET_API_URL` trỏ tới backend; Docker đã cấu hình. Threads/lịch sử và quyền sở hữu do .NET quản lý, frontend không gửi lịch sử tự khai báo.

- GET /health: kiểm tra process.
- GET /capabilities: mô tả phạm vi hiện có và dự kiến.
- GET /internal/knowledge/configuration: cấu hình model/OCR, yêu cầu X-Service-Key.
- POST /internal/knowledge/process: multipart file, model, force_ocr; trả chunks, vị trí nguồn, embedding và phiên bản pipeline.

- POST /internal/knowledge/document-keywords: hiểu câu hỏi/hội thoại, trả từ khóa tiêu đề/tên file để .NET lọc nguồn trước.
- POST /internal/knowledge/embed-query: embedding câu hỏi.
- POST /internal/knowledge/semantic-rank: cosine trên các vector đã được .NET giới hạn trong nguồn hợp lệ.
- POST /internal/knowledge/answer: LLM reranking top 24, chọn tối đa 6 đoạn, trả lời có trích dẫn đã kiểm tra.

Chatbot dành cho admin và giảng viên; sinh viên không được truy cập. Người dùng chỉ chat, hệ thống tự lọc tài liệu. Xem [luồng tìm kiếm và trích dẫn](../../docs/KNOWLEDGE_CHAT.md).
Phân loại và gap analysis chưa được triển khai. Curriculum không có AI trong phạm vi chung.
