import { useEffect, useRef, useState } from "react";
import { createPortal } from "react-dom";
import { Link, useNavigate, useSearchParams } from "react-router-dom";
import { ClockCounterClockwise, Trash } from "@phosphor-icons/react";
import { useConfirmDialog } from "../components/Dialogs";
import { useChatThreads } from "./ChatThreads";

export function ChatHistoryMenu() {
  const { threads, hasMore, error, loadMore, remove } = useChatThreads();
  const { confirm, confirmationDialog } = useConfirmDialog();
  const [params] = useSearchParams();
  const navigate = useNavigate();
  const [open, setOpen] = useState(false);
  const [actionError, setActionError] = useState("");
  const container = useRef<HTMLDivElement>(null);
  const selected = params.get("thread");

  useEffect(() => {
    if (!open) return;
    function closeOutside(event: MouseEvent) { if (!container.current?.contains(event.target as Node)) setOpen(false); }
    function closeOnEscape(event: KeyboardEvent) { if (event.key === "Escape") setOpen(false); }
    document.addEventListener("mousedown", closeOutside);
    document.addEventListener("keydown", closeOnEscape);
    return () => { document.removeEventListener("mousedown", closeOutside); document.removeEventListener("keydown", closeOnEscape); };
  }, [open]);

  async function removeThread(id: string, title: string) {
    setOpen(false);
    if (!await confirm({ title: "Xóa cuộc trò chuyện?", description: `“${title}” và lịch sử sẽ được xóa. Tài liệu Knowledge Base vẫn được giữ.`, confirmLabel: "Xóa cuộc trò chuyện", tone: "danger" })) return;
    setActionError("");
    try {
      await remove(id);
      if (selected === id) navigate("/knowledge/chat");
    } catch (caught) {
      setActionError(caught instanceof Error ? caught.message : "Chưa xóa được cuộc trò chuyện.");
      setOpen(true);
    }
  }

  const note = actionError || error;
  return <div className="chat-history" ref={container}>
    <button className="button button--small button--ghost" type="button" aria-expanded={open} aria-haspopup="true" onClick={() => setOpen(value => !value)}><ClockCounterClockwise size={15} aria-hidden="true" />Lịch sử</button>
    {open && <section className="chat-history__panel" aria-label="Lịch sử trò chuyện">
      {note && <p className="recents-note" role="alert">{note}</p>}
      {!note && threads.length === 0 && <p className="recents-note">Chưa có cuộc trò chuyện nào.</p>}
      {threads.map(thread => <div key={thread.id} className={`recent-row ${selected === thread.id ? "is-current" : ""}`}>
        <Link to={`/knowledge/chat?thread=${thread.id}`} title={thread.title} aria-current={selected === thread.id ? "page" : undefined} onClick={() => setOpen(false)}>{thread.title}</Link>
        <button className="recent-delete" aria-label={`Xóa ${thread.title}`} onClick={() => void removeThread(thread.id, thread.title)}><Trash size={15} /></button>
      </div>)}
      {hasMore && <button className="recents-more" onClick={() => void loadMore().catch(() => setActionError("Chưa tải thêm được lịch sử."))}>Xem thêm</button>}
    </section>}
    {confirmationDialog && createPortal(confirmationDialog, document.body)}
  </div>;
}
