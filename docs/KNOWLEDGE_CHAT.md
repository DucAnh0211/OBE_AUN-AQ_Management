# Chatbot: lọc tài liệu trước, semantic search, reranking và trích dẫn

Mã AI thuộc SV5 tại `modules/sv5/knowledge/ai`; `apps/ai-service` chỉ là host. Agent đã tách `agent/prompts.py`, `schemas.py`, `graph.py`, `runtime.py`, `checkpoints.py`, `routes.py` và `tools/`. Xem [cấu trúc và công cụ database](../modules/sv5/knowledge/ai/README.md).

Ngoài tài liệu, nhánh `curriculum` đọc CTĐT, phiên bản, học phần, PLO, CLO, mapping học phần–PLO và CLO–PLO qua API .NET. Công cụ chỉ đọc, giới hạn theo quyền chủ hội thoại và dữ liệu phân công hiện tại; không dùng SQL do model sinh. Câu trả lời kèm nguồn mở màn hình nghiệp vụ; hash snapshot được kiểm tra lại trước khi lưu. Common/curriculum vẫn là module nghiệp vụ không có AI; phần điều phối và AI đọc dữ liệu nằm trong SV5.

## Giao diện và quyền

Admin/giảng viên mở `/knowledge/chat` và nhập câu hỏi thông thường. Không có ô từ khóa hay yêu cầu chọn tài liệu. Giảng viên được đọc các nguồn Knowledge Base còn hiệu lực; chỉ admin được upload/process/disable. Sinh viên, anonymous và reporting API key không truy cập chat hoặc bản gốc nguồn.

Hội thoại được lưu riêng theo tài khoản trong PostgreSQL bằng EF Core. UI có danh sách hội thoại, tạo/xóa, Markdown, nút sao chép và nguồn có thể mở. Tải lại trang vẫn giữ lịch sử; admin không đọc được hội thoại riêng của giảng viên. Thanh điều hướng chính có nút thu ở góc trên bên phải và nút mở lại ở góc trên bên trái; trình duyệt ghi nhớ trạng thái.

LangGraph phân biệt trò chuyện thông thường với câu hỏi cần tài liệu. Lời chào, cảm ơn và trao đổi xã giao được trả lời tự nhiên, có thể có emoji, không chạy tìm kiếm. Câu hỏi nghiệp vụ đi qua pipeline dưới đây. Server lấy tối đa bốn lượt hỏi/đáp gần nhất làm ngữ cảnh; lịch sử không phải bằng chứng. Mỗi hội thoại tối đa 200 lượt, sau đó tạo hội thoại mới.

## Threads và checkpoint bền vững

- Migration `20261101062551_AddChatThreadsAndCheckpoints` thêm bốn bảng trong schema `knowledge`: `chat_threads`, `chat_exchanges`, `chat_checkpoints`, `chat_checkpoint_writes`. Xóa hội thoại xóa cả lượt chat, checkpoint và pending writes; không xóa tài liệu Knowledge Base.
- Python dùng LangGraph 1.2.19: `route → conversation/retrieve → finish`. Custom checkpointer gửi trạng thái và pending writes qua API nội bộ .NET để lưu bằng EF. Checkpoint được tách theo lượt xử lý trong từng thread; lịch sử giữa các lượt do server cung cấp. Không dùng bộ nhớ tiến trình làm nơi lưu bền vững, không gửi trace tới LangSmith.
- Gửi tin nhắn trả HTTP 202; worker xử lý nền, UI cập nhật trạng thái. Mỗi hội thoại chỉ có một lượt đang chạy. Request ID chống gửi trùng. Job có lease 10 phút, timeout xử lý 8 phút; tiến trình bị dừng có thể phục hồi sau khi lease hết hạn, tối đa ba lần claim trước khi báo lỗi. Lỗi xử lý hiển thị nút thử lại và tiếp tục từ checkpoint đã lưu.
- Callback nội bộ yêu cầu service key và lease đang hiệu lực. Kiểm tra lại quyền nguồn, checksum và quote trước khi lưu câu trả lời khôi phục từ checkpoint. UI chưa cung cấp time travel hay thao tác sửa checkpoint.

## Các bước thực hiện

1. Python dùng `CHAT_MODEL` rút câu hỏi độc lập và tối đa 4 truy vấn từ khóa thay thế. Không gửi toàn kho tài liệu tới model ở bước này.
2. .NET gọi boundary SV4 `IDocumentCatalog`, lọc **chỉ Title/FileName**, không tìm keyword trong body. Các từ trong một truy vấn là AND, không phân biệt hoa/thường; vẫn phân biệt dấu tiếng Việt. Thử các truy vấn theo thứ tự và dừng ở truy vấn đầu tiên có tài liệu đang `ready`, chưa vô hiệu hóa. EF parameterizes điều kiện `Contains`, không chèn SQL từ model.
3. Không có nguồn khớp: hỏi người dùng làm rõ, không fallback sang toàn kho. Hơn 20 nguồn hoặc 4.000 chunks: yêu cầu mô tả câu hỏi cụ thể hơn. Danh mục SQL giới hạn 100 kết quả; truy vấn quá rộng bị từ chối. Có thể bỏ sót nguồn nếu tên file/tiêu đề không mô tả chủ đề, đây là hệ quả của phạm vi lọc đã chọn.
4. Python embedding câu hỏi bằng cùng model của nguồn. Backend chặn nguồn khác model/số chiều. EF đọc các vector `real[]` theo batch 128 **chỉ trong các ID đã lọc**. Python tính exact cosine, lấy top 24 toàn cục. Chưa có pgvector/ANN index; giới hạn hiện tại phù hợp kho nhỏ và tập nguồn đã thu hẹp.
5. Python dùng `RERANK_MODEL` chấm lại từng cặp câu hỏi–chunk từ 0 đến 4 bằng nội dung, độc lập với cosine. Kiểm tra đủ/đúng mã và quote nguyên văn. Chọn tối đa 6 chunk có điểm >=2; loại nội dung trùng hoàn toàn. Đây là **LLM reranking**, chưa dùng cross-encoder riêng.
6. `CHAT_MODEL` trả các đoạn trả lời với mã nguồn và quote. Python và .NET đều chặn mã không thuộc tập nguồn hoặc quote không tồn tại; không có bằng chứng thì trả thông báo thiếu nguồn. Mã/URL/title/page trong citation được server dựng từ dữ liệu thật, model không được tự tạo URL.

Model dùng JSON schema theo [Structured Outputs của OpenAI](https://developers.openai.com/api/docs/guides/structured-outputs?api-mode=chat). Hệ thống vẫn kiểm tra mã nguồn/quote; schema đúng không chứng minh câu trả lời đúng. Kiểm tra quote chỉ chứng minh đoạn trích tồn tại, không bảo đảm mọi diễn giải của AI là đúng. Cần tập câu hỏi chuẩn của nghiệp vụ để đánh giá recall/reranking/độ chính xác trả lời.

## Trích dẫn và mở nguồn

- Link gồm DocumentId, ordinal, SHA-256 bản gốc, hash chunk (text/locator/offset), offset/độ dài quote. API kiểm tra nguồn còn hiệu lực, hash đúng và job `ready`. Hash khác trả 409; nguồn vô hiệu hóa trả 410.
- PDF dùng PDF.js render đúng trang được trích dẫn, có nút chuyển trang và trở lại trang nguồn; kèm văn bản trích xuất và highlight **đúng quote** phía dưới. Chưa tô vùng chữ trực tiếp trên mặt trang PDF. File scan có thể sai chữ OCR nên người dùng cần đối chiếu bản gốc.
- DOCX mở đoạn văn theo `Đoạn N`, tái dựng text bằng offset và bỏ overlap; TXT/Markdown mở text tương ứng và highlight. Không bịa số trang DOCX (phụ thuộc cách dàn trang). Có nút tải bản gốc cho mọi định dạng.
- API tải bản gốc qua .NET kiểm checksum, không công khai URL/credentials MinIO. UI dùng blob URL, thu hồi khi đóng/đổi nguồn. Viewer kiểm tra lại hiệu lực mỗi 20 giây; nội dung đã tải xuống không thể thu hồi.
- Nguồn được kiểm tra trước ranking, trước/sau generation và khi mở link. Không lưu response cache; câu trả lời đang có trên tab là snapshot đã gửi trước đó, link vẫn phải qua kiểm tra hiện tại.

## Cấu hình và triển khai

```dotenv
EMBEDDING_MODEL=text-embedding-3-small
CHAT_MODEL=gpt-4.1-mini
RERANK_MODEL=gpt-4.1-mini
DOTNET_API_URL=http://localhost:5099
```

Cả ba dùng `LLM_API_KEY` chỉ ở Python. Có thể chọn model hỗ trợ Chat Completions + strict JSON schema. Mặc định trên là cấu hình triển khai, không phải cam kết model chính xác nhất. Một câu hỏi đủ nguồn gọi plan, embedding, rerank và answer; có chi phí và độ trễ. Không log key hoặc nội dung upstream error. Nginx timeout 300 giây để tránh cắt request chat ở mốc mặc định 60 giây.

Docker đặt `DOTNET_API_URL=http://api:8080`; chạy native cần đặt theo cổng backend. Build lại `api`, `database-init`, `ai` và `web`, rồi chạy Compose với profile `ai` và `storage`; `database-init` áp dụng migration trước khi API khởi động. Tất cả thuật toán AI nằm ở Python; .NET chịu trách nhiệm EF, quyền, điều phối và xác minh nguồn. Tính năng này tận dụng bản gốc, chunks và jobs hiện có; không đổi thang mapping hoặc báo cáo UC25–27.

## Kiểm thử

- Python `test_agent.py`: chào hỏi không retrieval, khôi phục graph mới từ checkpoint/pending writes sau lỗi, chặn pickle.
- `knowledge_agent_smoke.py`: stack cô lập với EF/PostgreSQL thật và provider giả lập; lưu checkpoint, nhớ ngữ cảnh, idempotency, quyền sở hữu, citation và xóa cascade.
- `knowledge_agent_ui_smoke.py`: tạo/xóa, tải lại giữ lịch sử, mở nguồn, thu/mở sidebar và lưu trạng thái, bố cục mobile, không lỗi JavaScript.

- .NET `KnowledgeChatServiceTests`: lọc trước semantic, không fallback khi không có nguồn, model mismatch, nguồn vô hiệu hóa giữa generation, quote giả, hash cũ, Unicode/overlap.
- Python `test_retrieval.py`: cosine, vector lỗi, rerank nguồn không liên quan, quote/mã giả, kế hoạch từ khóa từ hội thoại, xác thực API nội bộ.
- `tests/contracts/knowledge_chat_smoke.py` và `knowledge_chat_ui_smoke.py`: chỉ chạy stack cô lập với `tests/smoke_app.py`; kiểm quyền, keyword body-only bị loại, PDF trang 2, DOCX đoạn 2, click nguồn và thu hồi nguồn. Mock provider, không đo chất lượng model thực tế.
