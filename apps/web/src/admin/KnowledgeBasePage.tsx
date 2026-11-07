import { useCallback, useEffect, useRef, useState, type FormEvent } from "react";
import { apiJson, authenticatedFetch } from "../../../../shared/frontend/auth-client";
import { apiBaseUrl } from "../../../../shared/frontend/api-config";
import { DialogFrame, useConfirmDialog } from "../components/Dialogs";
import "./knowledge-base.css";

type DocumentInfo = { id: string; title: string; fileName: string; size: number; sha256: string; createdAt: string; disabledAt: string | null };
type Item = { document: DocumentInfo; status: string; chunkCount: number; dimensions: number; model: string | null; error: string | null; warning: string | null };
type Chunk = { ordinal: number; text: string; locator: string; startOffset: number };
type Configuration = { embeddingConfigured: boolean; embeddingModel: string; maxFileMb: number; ocrAvailable: boolean; error: string | null };
const labels: Record<string, string> = { uploaded: "Đã lưu bản gốc", queued: "Chờ xử lý", processing: "Đang xử lý", ready: "Hoàn tất", failed: "Xử lý lỗi", disabled: "Đã vô hiệu hóa" };
const base = "/api/knowledge";
const message = (error: unknown) => error instanceof Error ? error.message : "Không thực hiện được thao tác.";
async function problem(response: Response) {
  const body = await response.json().catch(() => ({})) as { detail?: string };
  return new Error(body.detail ?? `Yêu cầu thất bại (HTTP ${response.status}).`);
}

export function KnowledgeBasePage() {
  const [items, setItems] = useState<Item[]>([]);
  const [config, setConfig] = useState<Configuration | null>(null);
  const [page, setPage] = useState(1);
  const [error, setError] = useState("");
  const [notice, setNotice] = useState("");
  const [busy, setBusy] = useState(false);
  const [forceOcr, setForceOcr] = useState(false);
  const [loading, setLoading] = useState(true);
  const [selected, setSelected] = useState<Item | null>(null);
  const [chunkPage, setChunkPage] = useState(1);
  const [chunks, setChunks] = useState<Chunk[]>([]);
  const [chunksLoading, setChunksLoading] = useState(false);
  const [chunkError, setChunkError] = useState("");
  const form = useRef<HTMLFormElement>(null);
  const requestNumber = useRef(0);
  const { confirm, confirmationDialog: confirmDialog } = useConfirmDialog();
  const load = useCallback(async () => {
    const request = ++requestNumber.current;
    const [list, configuration] = await Promise.all([apiJson<Item[]>(`${base}/documents?page=${page}`), apiJson<Configuration>(`${base}/configuration`)]);
    if (request === requestNumber.current) { setItems(list); setConfig(configuration); }
  }, [page]);
  useEffect(() => {
    let disposed = false;
    let timer: ReturnType<typeof setTimeout>;
    async function poll() {
      try { if (!disposed) await load(); }
      catch (e) { if (!disposed) setError(message(e)); }
      finally { if (!disposed) { setLoading(false); timer = setTimeout(() => void poll(), 4000); } }
    }
    setLoading(true); void poll();
    return () => { disposed = true; ++requestNumber.current; clearTimeout(timer); };
  }, [load]);
  useEffect(() => {
    if (!selected) return;
    let disposed = false;
    setChunksLoading(true); setChunkError(""); setChunks([]);
    apiJson<Chunk[]>(`${base}/documents/${selected.document.id}/chunks?page=${chunkPage}`)
      .then((data) => { if (!disposed) setChunks(data); })
      .catch((e: unknown) => { if (!disposed) setChunkError(message(e)); })
      .finally(() => { if (!disposed) setChunksLoading(false); });
    return () => { disposed = true; };
  }, [selected, chunkPage]);
  async function upload(event: FormEvent<HTMLFormElement>) {
    event.preventDefault(); setError(""); setNotice("");
    const data = new FormData(event.currentTarget);
    const file = data.get("file");
    if (!(file instanceof File) || file.size === 0 || file.size > 20 * 1024 * 1024) { setError("Chọn một file có dữ liệu, tối đa 20 MB."); return; }
    setBusy(true);
    try {
      const response = await authenticatedFetch(`${apiBaseUrl}${base}/documents`, { method: "POST", body: data });
      if (!response.ok) throw await problem(response);
      const document = await response.json() as DocumentInfo;
      form.current?.reset();
      setNotice("Đã lưu bản gốc trên MinIO.");
      if (data.get("process") === "on") {
        try {
          await apiJson(`${base}/documents/${document.id}/process?forceOcr=${forceOcr}`, { method: "POST" });
          setNotice("Đã lưu bản gốc và xếp hàng xử lý. Bạn có thể rời trang; hệ thống tiếp tục chạy nền.");
        } catch (e) { setError(`Bản gốc đã lưu. ${message(e)}`); }
      }
      setPage(1); await load();
    } catch (e) { setError(message(e)); }
    finally { setBusy(false); }
  }
  async function process(item: Item) {
    setBusy(true); setError(""); setNotice("");
    try { await apiJson(`${base}/documents/${item.document.id}/process?forceOcr=${forceOcr}`, { method: "POST" }); setNotice("Đã xếp hàng chunking và embedding."); await load(); }
    catch (e) { setError(message(e)); } finally { setBusy(false); }
  }
  async function download(item: Item) {
    setBusy(true); setError("");
    try {
      const response = await authenticatedFetch(`${apiBaseUrl}${base}/documents/${item.document.id}/original`);
      if (!response.ok) throw await problem(response);
      const url = URL.createObjectURL(await response.blob());
      const link = document.createElement("a"); link.href = url; link.download = item.document.fileName;
      document.body.append(link); link.click(); link.remove(); setTimeout(() => URL.revokeObjectURL(url), 1000);
    } catch (e) { setError(message(e)); } finally { setBusy(false); }
  }
  async function disable(item: Item) {
    if (!await confirm({ title: "Vô hiệu hóa tài liệu?", description: `“${item.document.title}” sẽ không còn được đọc hoặc xử lý. Bản gốc được giữ để lưu vết; các chunks bị loại bỏ.`, confirmLabel: "Vô hiệu hóa", tone: "danger" })) return;
    setBusy(true); setError("");
    try { await apiJson(`${base}/documents/${item.document.id}/disable`, { method: "POST" }); setNotice("Đã vô hiệu hóa nguồn tài liệu."); await load(); }
    catch (e) { setError(message(e)); } finally { setBusy(false); }
  }
  return <section className="admin-page kb-page">
    <header className="workspace-heading"><div><h1>Knowledge Base</h1><p>Thêm tài liệu, xem nội dung trích xuất và theo dõi quá trình tạo embedding.</p></div></header>
    {error && <div className="kb-alert kb-alert--error" role="alert">{error}</div>}
    {notice && <div className="kb-alert" role="status">{notice}</div>}
    {config && !config.embeddingConfigured && <div className="kb-alert" role="status">{config.error ?? "Chưa cấu hình API key cho embedding trên Python service."} Bạn vẫn có thể lưu bản gốc và xử lý sau.</div>}
    {config?.embeddingConfigured && !config.ocrAvailable && <div className="kb-alert" role="status">Python chưa sẵn sàng OCR: cài Tesseract và ngôn ngữ vie+eng để xử lý PDF scan.</div>}
    <form ref={form} className="admin-form kb-upload" onSubmit={(e) => void upload(e)}>
      <h2>Thêm tài liệu</h2>
      <label><span>Tên tài liệu</span><input name="title" required maxLength={200} placeholder="Ví dụ: Hướng dẫn đánh giá AUN-QA 4.0" disabled={busy} /></label>
      <label><span>File gốc · tối đa 20 MB</span><input name="file" type="file" required accept=".pdf,.docx,.txt,.md" disabled={busy} /></label>
      <p className="muted-text">PDF (cả bản scan), DOCX, TXT và Markdown UTF-8. PDF scan được OCR tiếng Việt/Anh. Mỗi lần tải lên tạo một tài liệu độc lập.</p>
      <label className="kb-checkbox"><input name="process" type="checkbox" defaultChecked disabled={busy} /><span>Chunking và tạo embedding sau khi tải lên</span></label>
      <label className="kb-checkbox"><input type="checkbox" checked={forceOcr} disabled={busy} onChange={(e) => setForceOcr(e.target.checked)} /><span>OCR toàn bộ trang PDF (dùng khi lớp chữ có sẵn bị lỗi hoặc thiếu nội dung)</span></label>
      <p className="muted-text">Khi xử lý, nội dung văn bản được gửi đến OpenAI để tạo embedding ({config?.embeddingModel ?? "…"}). Bản gốc lưu riêng trên MinIO; chỉ admin được truy cập.</p>
      <div><button className="button button--primary" disabled={busy}>{busy ? "Đang thực hiện…" : "Tải tài liệu lên"}</button></div>
    </form>
    <div className="section-heading"><div><h2>Tài liệu đã thêm</h2><p>Trạng thái tự cập nhật mỗi 4 giây.</p></div><button className="button button--ghost" disabled={busy} onClick={() => void load().catch((e: unknown) => setError(message(e)))}>Làm mới</button></div>
    {loading ? <div className="empty-state">Đang tải tài liệu…</div> : !items.length ? <div className="empty-state">Chưa có tài liệu ở trang này.</div> : <div className="data-table-wrap"><table className="data-table kb-table"><thead><tr><th>Tài liệu</th><th>Trạng thái</th><th>Kết quả</th><th>Thao tác</th></tr></thead><tbody>
      {items.map((item) => <tr key={item.document.id}>
        <td><strong>{item.document.title}</strong><div className="muted-text">{item.document.fileName} · {(item.document.size / 1024).toFixed(1)} KB</div><small>{new Date(item.document.createdAt).toLocaleString("vi-VN")}</small></td>
        <td><span className={`status status--${item.status === "ready" ? "active" : item.status === "failed" || item.status === "disabled" ? "disabled" : "draft"}`}>{labels[item.status] ?? item.status}</span>{item.error && <p className="kb-error">{item.error}</p>}{item.warning && <p className="muted-text">{item.warning}</p>}</td>
        <td>{item.chunkCount} chunks<div className="muted-text">{item.dimensions > 0 ? `${item.dimensions} chiều · ${item.model}` : "Chưa có embedding"}</div></td>
        <td><div className="table-actions">{item.status !== "disabled" && <>
          {(item.status === "uploaded" || item.status === "failed") && <button className="button button--small button--primary" disabled={busy || !config?.embeddingConfigured} onClick={() => void process(item)}>{item.status === "failed" ? "Thử lại" : "Xử lý"}</button>}
          {item.status === "ready" && <button className="button button--small button--secondary" onClick={() => { setChunkPage(1); setSelected(item); }}>Xem chunks</button>}
          <button className="button button--small button--ghost" disabled={busy} onClick={() => void download(item)}>Tải bản gốc</button>
          <button className="button button--small button--danger" disabled={busy} onClick={() => void disable(item)}>Vô hiệu hóa</button>
        </>}</div></td>
      </tr>)}
    </tbody></table></div>}
    <div className="kb-pagination"><button className="button button--ghost" disabled={page === 1 || loading} onClick={() => setPage(page - 1)}>Trang trước</button><span>Trang {page}</span><button className="button button--ghost" disabled={items.length < 20 || loading} onClick={() => setPage(page + 1)}>Trang sau</button></div>
    {selected && <DialogFrame title={selected.document.title} description={`${selected.chunkCount} chunks · ${selected.model} · ${selected.dimensions} chiều`} onClose={() => setSelected(null)}>
      <div className="kb-chunks">{chunksLoading ? <p>Đang tải chunks…</p> : chunkError ? <p role="alert">{chunkError}</p> : chunks.map((chunk) => <article key={chunk.ordinal}><strong>Chunk {chunk.ordinal} · {chunk.locator}</strong><small>Vị trí ký tự: {chunk.startOffset}</small><p>{chunk.text}</p></article>)}</div>
      <div className="kb-pagination"><button className="button button--ghost" disabled={chunkPage === 1 || chunksLoading} onClick={() => setChunkPage(chunkPage - 1)}>Trước</button><span>Trang {chunkPage}</span><button className="button button--ghost" disabled={chunkPage * 20 >= selected.chunkCount || chunksLoading} onClick={() => setChunkPage(chunkPage + 1)}>Sau</button></div>
    </DialogFrame>}
    {confirmDialog}
  </section>;
}
