import { useEffect, useRef, useState } from "react";
import { getDocument, GlobalWorkerOptions, type PDFDocumentProxy } from "pdfjs-dist";
import workerUrl from "pdfjs-dist/build/pdf.worker.min.mjs?url";

GlobalWorkerOptions.workerSrc = workerUrl;

export default function PdfSourceViewer({ url, initialPage }: { url: string; initialPage: number }) {
  const canvas = useRef<HTMLCanvasElement>(null);
  const [document, setDocument] = useState<PDFDocumentProxy | null>(null);
  const [page, setPage] = useState(initialPage);
  const [error, setError] = useState("");
  const [rendered, setRendered] = useState(false);
  useEffect(() => {
    let disposed = false; setError(""); setDocument(null); setPage(initialPage);
    const loading = getDocument({ url, useSystemFonts: true });
    void loading.promise.then(pdf => {
      if (!disposed) {
        if (initialPage > pdf.numPages) setError("Trang trích dẫn vượt số trang bản gốc.");
        else setDocument(pdf);
      }
    }).catch(() => { if (!disposed) setError("Không hiển thị được PDF. Bạn có thể tải bản gốc để xem."); });
    return () => { disposed = true; void loading.destroy(); };
  }, [url, initialPage]);
  useEffect(() => {
    if (!document || !canvas.current) return;
    let disposed = false; let cancel: (() => void) | undefined;
    setRendered(false);
    void document.getPage(page).then(async pdfPage => {
      if (disposed || !canvas.current) return;
      const viewport = pdfPage.getViewport({ scale: Math.min(1.5, 1600 / pdfPage.getViewport({ scale: 1 }).width) });
      const element = canvas.current; element.width = Math.ceil(viewport.width); element.height = Math.ceil(viewport.height);
      const task = pdfPage.render({ canvas: element, viewport }); cancel = () => task.cancel();
      await task.promise;
      if (!disposed) setRendered(true);
    }).catch(() => { if (!disposed) setError("Không vẽ được trang PDF. Hãy tải bản gốc để xem."); });
    return () => { disposed = true; cancel?.(); };
  }, [document, page]);
  return <div className="source-pdf" aria-label="Bản gốc PDF">
    {error && <p role="alert">{error}</p>}
    {document && <div className="pdf-controls"><button type="button" disabled={page <= 1} onClick={() => setPage(p => p - 1)}>Trang trước</button>
      <span>Trang {page} / {document.numPages}</span>
      <button type="button" disabled={page >= document.numPages} onClick={() => setPage(p => p + 1)}>Trang sau</button>
      <button type="button" onClick={() => setPage(initialPage)}>Về trang trích dẫn</button></div>}
    {!rendered && !error && <p role="status">Đang hiển thị trang PDF…</p>}
    <canvas ref={canvas} role="img" aria-label={`Bản gốc trang ${page}`} data-rendered={rendered} />
  </div>;
}
