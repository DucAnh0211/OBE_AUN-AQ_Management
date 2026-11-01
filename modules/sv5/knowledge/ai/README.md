# AI của SV5

Mã nghiệp vụ Python thuộc module Knowledge của SV5. `apps/ai-service` chỉ khởi động FastAPI và đóng gói Docker; không phải nơi chứa nghiệp vụ AI chung của SV1.

`agent/prompts.py` định nghĩa `SCOPE_POLICY`: chỉ hỗ trợ nghiệp vụ OBE/AUN-QA, vẫn đáp lời chào/cảm ơn và từ chối lịch sự chủ đề ngoài phạm vi. Phân loại theo mục đích, không chặn từ khóa: CLO học phần Giáo dục thể chất hoặc PLO chương trình Du lịch vẫn hợp lệ; dự đoán bóng đá/lên lịch du lịch cá nhân bị từ chối và không gọi tool. Yêu cầu hỗn hợp chỉ giữ phần nghiệp vụ. Kiểm tra model thật (có phí API): `apps/ai-service/.venv/Scripts/python.exe tests/contracts/agent_scope_live.py --live` từ repo root.

```text
ai/
  agent/
    prompts.py           # System prompt, hướng dẫn chọn tool và trả lời
    schemas.py           # Hợp đồng kế hoạch, trạng thái và tool call
    graph.py             # LangGraph: route → conversation/retrieve/curriculum → finish
    runtime.py           # Chạy/khôi phục thread qua API nội bộ .NET
    checkpoints.py       # Checkpoint và pending writes qua EF Core
    routes.py            # Endpoint agent
    tools/
      curriculum.py      # Tool loop đọc PLO/CLO/học phần/mapping
      documents.py       # Tool tìm tài liệu đã lọc tiêu đề/tên file
  extraction.py          # Parse và OCR
  chunking.py
  embeddings.py
  retrieval.py           # Semantic, rerank và trả lời theo tài liệu
  settings.py
```

Agent có 7 tool database chỉ đọc: `programs`, `versions`, `courses`, `plos`, `clos`, `course_plo`, `clo_plo`. AI chọn tool/tham số từ câu hỏi và kết quả các bước trước, tối đa 8 lượt tool; mỗi trang 25 bản ghi, luôn có tổng số và dấu phân trang. Khi có nhiều CTĐT/phiên bản chưa rõ, agent hỏi lại, không tự gán ID. Câu hỏi cần kết hợp cả quy định tài liệu và database hiện được phân luồng theo nhu cầu chính; chưa có nhánh tổng hợp hai nguồn trong cùng lượt.

Python không kết nối trực tiếp PostgreSQL, không sinh/thực thi SQL. Callback `.NET /api/knowledge/internal/agent/{threadId}/{runId}/curriculum` kiểm tra service key, lease còn hiệu lực và lấy chủ sở hữu từ thread. Adapter SV5 tái sử dụng CurriculumService + AccessControlService; admin đọc CTĐT, giảng viên chỉ đọc chương trình/phiên bản và học phần được phân công. SV1/common/curriculum chỉ cung cấp nghiệp vụ CRUD/EF hiện có, không có mã AI.

Nguồn database được server tạo URL tới màn hình nghiệp vụ và hash snapshot. Graph làm mới dữ liệu trước khi tổng hợp; worker kiểm tra lại quyền/hash trước khi lưu trả lời, kể cả khi phục hồi checkpoint. Link nguồn mở dữ liệu hiện tại, không phải kho lưu lịch sử phiên bản dữ liệu draft. Dữ liệu đã trả lời là snapshot tại thời điểm đó. Giữ nguyên X/Y, I/R/E và không suy diễn thang đóng góp riêng cho CLO–PLO.

Chạy từ repo root: `powershell -File tools/dev.ps1 -Target ai`. Docker: `docker compose build ai` rồi `docker compose --profile ai up -d ai`. Lần thêm tool database cần build lại API và web cùng AI. Không có migration mới cho tool (metadata nguồn lưu trong JSON câu trả lời hiện có).

Kiểm thử Python: từ `apps/ai-service`, chạy `.venv/Scripts/python.exe -m unittest discover -s tests -p 'test_*.py'`. Test được đặt ở host để kiểm tra cả FastAPI lẫn module SV5. Các bài unit dùng provider giả lập; cần kiểm tra câu hỏi thực tế để đánh giá chất lượng lựa chọn tool và diễn giải.
