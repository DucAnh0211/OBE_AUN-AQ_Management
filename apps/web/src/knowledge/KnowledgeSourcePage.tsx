import { lazy, Suspense, useEffect, useRef, useState } from "react";
import { Link, useParams, useSearchParams } from "react-router-dom";
import { apiJson, authenticatedFetch } from "../../../../shared/frontend/auth-client";
import { apiBaseUrl } from "../../../../shared/frontend/api-config";
import "./knowledge-chat.css";
const PdfSourceViewer = lazy(() => import("./PdfSourceViewer"));

type Source = { documentId: string; ordinal: number; title: string; fileName: string; contentType: string; sourceHash: string; chunkHash: string; locator: string; startOffset: number; text: string; sectionText: string; page: number | null };

export function KnowledgeSourcePage() {
  const { documentId, ordinal } = useParams(); const [search] = useSearchParams();
  const sourceHash = search.get("sourceHash") ?? ""; const chunkHash = search.get("chunkHash") ?? "";
  const [source, setSource] = useState<Source | null>(null); const [blobUrl, setBlobUrl] = useState("");
  const [error, setError] = useState(""); const [loading, setLoading] = useState(true);
  const highlight = useRef<HTMLElement>(null);
  useEffect(() => {
    const abort = new AbortController(); let objectUrl = ""; let timer: ReturnType<typeof setTimeout>;
    setSource(null); setBlobUrl(""); setError(""); setLoading(true);
    const path = `/api/knowledge/chat/sources/${documentId}/${ordinal}?sourceHash=${encodeURIComponent(sourceHash)}&chunkHash=${encodeURIComponent(chunkHash)}`;
    async function refresh(first: boolean) {
      try {
        const data = await apiJson<Source>(path, { signal: abort.signal });
        if (first) {
          const response = await authenticatedFetch(`${apiBaseUrl}/api/knowledge/chat/sources/${documentId}/original?sourceHash=${encodeURIComponent(sourceHash)}`, { signal: abort.signal });
          if (!response.ok) throw new Error("Không mở được bản gốc hoặc nguồn đã bị vô hiệu hóa.");
          const bytes = await response.arrayBuffer();
          if (abort.signal.aborted) return;
          objectUrl = URL.createObjectURL(new Blob([bytes], { type: data.contentType })); setBlobUrl(objectUrl);
        }
        if (!abort.signal.aborted) { setSource(data); setLoading(false); timer = setTimeout(() => void refresh(false), 20000); }
      } catch (e) {
        if (!abort.signal.aborted) { setSource(null); setBlobUrl(""); if (objectUrl) URL.revokeObjectURL(objectUrl); setLoading(false); setError(e instanceof Error ? e.message : "Nguồn không khả dụng."); }
      }
    }
    void refresh(true);
    return () => { abort.abort(); clearTimeout(timer); if (objectUrl) URL.revokeObjectURL(objectUrl); };
  }, [documentId, ordinal, sourceHash, chunkHash]);
  useEffect(() => { highlight.current?.scrollIntoView({ block: "center" }); }, [source?.chunkHash]);
  async function download() {
    if (!source) return;
    try {
      const response = await authenticatedFetch(`${apiBaseUrl}/api/knowledge/chat/sources/${documentId}/original?sourceHash=${encodeURIComponent(sourceHash)}`);
      if (!response.ok) throw new Error("Không tải được bản gốc. Nguồn có thể đã bị vô hiệu hóa.");
      const url = URL.createObjectURL(new Blob([await response.arrayBuffer()], { type: "application/octet-stream" }));
      const link = document.createElement("a"); link.href = url; link.download = source.fileName;
      document.body.append(link); link.click(); link.remove(); setTimeout(() => URL.revokeObjectURL(url), 1000);
    } catch (e) { setError(e instanceof Error ? e.message : "Không tải được bản gốc."); }
  }
  const chars = Array.from(source?.sectionText ?? ""); const chunkLength = Array.from(source?.text ?? "").length;
  const requestedStart = Number(search.get("quoteStart")); const requestedLength = Number(search.get("quoteLength"));
  const validQuote = Number.isInteger(requestedStart) && Number.isInteger(requestedLength) && requestedStart >= 0 && requestedLength > 0 && requestedStart + requestedLength <= chunkLength;
  const start = (source?.startOffset ?? 0) + (validQuote ? requestedStart : 0);
  const length = validQuote ? requestedLength : chunkLength;
  return <section className="admin-page knowledge-source">
    <Link to="/knowledge/chat">← Về chatbot</Link>
    {loading && <p role="status">Đang mở nguồn trích dẫn…</p>}
    {error && <p role="alert" className="error-message">{error}</p>}
    {source && <>
      <header className="workspace-heading"><div><h1>{source.title}</h1><p>{source.fileName} · {source.locator} · Đoạn trích #{source.ordinal}</p></div>
        <button className="button" type="button" onClick={() => void download()}>Tải bản gốc</button></header>
      <p>Phần tô sáng là đoạn nguồn được sử dụng. Bản gốc được đối chiếu checksum với nguồn trích dẫn.</p>
      {source.page && <Suspense fallback={<p role="status">Đang mở PDF…</p>}><PdfSourceViewer url={blobUrl} initialPage={source.page} /></Suspense>}
      <h2>{source.locator}</h2>
      {!source.page && source.contentType.includes("wordprocessingml") && <p>Vị trí theo đoạn văn trong DOCX; số trang phụ thuộc phần mềm và cách dàn trang.</p>}
      <pre className="source-section">{chars.slice(0, start).join("")}<mark ref={highlight}>{chars.slice(start, start + length).join("")}</mark>{chars.slice(start + length).join("")}</pre>
    </>}
  </section>;
}
