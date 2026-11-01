import { useCallback, useEffect, useRef, useState, type FormEvent, type KeyboardEvent } from "react";
import { useSearchParams } from "react-router-dom";
import { ArrowUp, Plus, Trash, ChatCircle, BookOpen, Sparkle, SidebarSimple, X, ArrowClockwise } from "@phosphor-icons/react";
import { apiJson } from "../../../../shared/frontend/auth-client";
import { useConfirmDialog } from "../components/Dialogs";
import { ChatMessageView, type ChatAnswer } from "./ChatMessageView";
import "./knowledge-chat.css";

type Thread = { id: string; title: string; updatedAt: string };
type Exchange = { id: string; question: string; answer: ChatAnswer | null; status: string; phase: string; error: string | null };
type Detail = { thread: Thread; messages: Exchange[] };
const base = "/api/knowledge/chat/threads";
const message = (e: unknown) => e instanceof Error ? e.message : "Chưa kết nối được. Bạn thử lại nhé.";
const phases: Record<string, string> = { queued: "Đang chờ trợ lý", route: "Đang suy nghĩ", conversation: "Đang soạn câu trả lời", retrieval: "Đang đọc tài liệu", complete: "Đang hoàn tất" };

export function KnowledgeChatPage() {
  const [params, setParams] = useSearchParams(); const selected = params.get("thread");
  const selectedRef = useRef(selected); selectedRef.current = selected;
  const [threads, setThreads] = useState<Thread[]>([]); const [page, setPage] = useState(1); const [hasMore, setHasMore] = useState(false);
  const [detail, setDetail] = useState<Detail | null>(null); const [question, setQuestion] = useState("");
  const [error, setError] = useState(""); const [loading, setLoading] = useState(false); const [sending, setSending] = useState(false);
  const [pending, setPending] = useState(""); const [historyOpen, setHistoryOpen] = useState(false);
  const textarea = useRef<HTMLTextAreaElement>(null); const scroll = useRef<HTMLDivElement>(null); const nearBottom = useRef(true);
  const { confirm, confirmationDialog } = useConfirmDialog();
  const loadThreads = useCallback(async (number = 1) => {
    const items = await apiJson<Thread[]>(`${base}?page=${number}`);
    setThreads(old => number === 1 ? items : [...old, ...items.filter(x => old.every(t => t.id !== x.id))]);
    setPage(number); setHasMore(items.length === 30);
  }, []);
  useEffect(() => { void loadThreads().catch(e => setError(message(e))); }, [loadThreads]);
  useEffect(() => {
    setDetail(null); setError(""); setPending(""); nearBottom.current = true;
    if (!selected) { setLoading(false); return; }
    const abort = new AbortController(); let timer: ReturnType<typeof setTimeout>;
    setLoading(true);
    async function refresh() {
      try {
        const value = await apiJson<Detail>(`${base}/${selected}`, { signal: abort.signal });
        if (abort.signal.aborted) return;
        setDetail(value); setLoading(false);
        timer = setTimeout(() => void refresh(), document.hidden ? 8000 : 1500);
      } catch (e) { if (!abort.signal.aborted) { setLoading(false); setError(message(e)); } }
    }
    void refresh(); return () => { abort.abort(); clearTimeout(timer); };
  }, [selected]);
  const messages = detail?.messages ?? [];
  const busy = sending || messages.some(m => ["queued", "processing"].includes(m.status));
  const last = messages.at(-1);
  useEffect(() => {
    if (nearBottom.current && scroll.current) scroll.current.scrollTo({ top: scroll.current.scrollHeight, behavior: "smooth" });
  }, [messages.length, last?.status, last?.phase, pending]);
  useEffect(() => {
    if (textarea.current) { textarea.current.style.height = "auto"; textarea.current.style.height = `${Math.min(200, textarea.current.scrollHeight)}px`; }
  }, [question]);
  async function create() {
    try { const t = await apiJson<Thread>(base, { method: "POST" }); setParams({ thread: t.id }); setHistoryOpen(false); setQuestion(""); await loadThreads(); textarea.current?.focus(); }
    catch (e) { setError(message(e)); }
  }
  async function remove(thread: Thread) {
    if (!await confirm({ title: "Xóa cuộc trò chuyện?", description: `“${thread.title}” và lịch sử sẽ được xóa. Tài liệu Knowledge Base vẫn được giữ.`, confirmLabel: "Xóa cuộc trò chuyện", tone: "danger" })) return;
    try { await apiJson(`${base}/${thread.id}`, { method: "DELETE" }); if (selectedRef.current === thread.id) setParams({}); await loadThreads(); }
    catch (e) { setError(message(e)); }
  }
  async function send(event?: FormEvent, retry?: Exchange) {
    event?.preventDefault(); const asked = retry?.question ?? question.trim(); if (!asked || busy) return;
    setSending(true); setError(""); setPending(asked); nearBottom.current = true;
    let id = selected;
    try {
      if (!id) { const t = await apiJson<Thread>(base, { method: "POST" }); id = t.id; selectedRef.current = id; setParams({ thread: id }); }
      await apiJson(`${base}/${id}/messages`, { method: "POST", body: JSON.stringify({ requestId: retry?.id ?? crypto.randomUUID(), question: asked }) });
      if (selectedRef.current === id) {
        setQuestion(""); const value = await apiJson<Detail>(`${base}/${id}`);
        if (selectedRef.current === id) setDetail(value);
      }
      await loadThreads();
    } catch (e) { if (selectedRef.current === id) { setError(message(e)); if (!retry) setQuestion(asked); } }
    finally { setSending(false); setPending(""); textarea.current?.focus(); }
  }
  function keyDown(event: KeyboardEvent<HTMLTextAreaElement>) {
    if (event.key === "Enter" && !event.shiftKey && !event.nativeEvent.isComposing) { event.preventDefault(); void send(); }
  }
  const select = (id: string) => { setParams({ thread: id }); setHistoryOpen(false); setQuestion(""); };
  return <section className={`chat-studio ${historyOpen ? "chat-studio--history-open" : ""}`} aria-label="Trợ lý OBE">
    {historyOpen && <button className="chat-history-backdrop" aria-label="Đóng lịch sử" onClick={() => setHistoryOpen(false)} />}
    <aside className="conversation-sidebar" aria-label="Lịch sử trò chuyện">
      <div className="conversation-sidebar__heading"><span>Cuộc trò chuyện</span><button className="chat-icon history-close" aria-label="Đóng lịch sử" onClick={() => setHistoryOpen(false)}><X size={19} /></button></div>
      <button className="new-conversation" onClick={() => void create()}><Plus size={19} /><span>Cuộc trò chuyện mới</span></button>
      <div className="conversation-list">
        {threads.length === 0 && <p className="conversation-empty">Những cuộc trò chuyện của bạn sẽ xuất hiện ở đây.</p>}
        {threads.map(t => <div key={t.id} className={`conversation-row ${selected === t.id ? "is-current" : ""}`}>
          <button className="conversation-select" title={t.title} aria-current={selected === t.id ? "page" : undefined} onClick={() => select(t.id)}><ChatCircle size={17} /><span>{t.title}</span></button>
          <button className="chat-icon conversation-delete" aria-label={`Xóa ${t.title}`} onClick={() => void remove(t)}><Trash size={16} /></button>
        </div>)}
        {hasMore && <button className="chat-load-more" onClick={() => void loadThreads(page + 1).catch(e => setError(message(e)))}>Xem thêm</button>}
      </div>
      <div className="conversation-sidebar__foot"><BookOpen size={18} /><span>Câu trả lời có nguồn để bạn kiểm chứng.</span></div>
    </aside>
    <div className="conversation-main">
      <header className="conversation-toolbar"><div><button className="chat-icon history-toggle" aria-label="Mở lịch sử trò chuyện" aria-expanded={historyOpen} onClick={() => setHistoryOpen(true)}><SidebarSimple size={22} /></button><span className="assistant-brand"><Sparkle size={21} weight="duotone" />Trợ lý OBE</span></div><button className="chat-icon" title="Cuộc trò chuyện mới" aria-label="Cuộc trò chuyện mới" onClick={() => void create()}><Plus size={22} /></button></header>
      <div className="conversation-scroll" ref={scroll} onScroll={() => { const el = scroll.current; if (el) nearBottom.current = el.scrollHeight - el.scrollTop - el.clientHeight < 100; }}>
        <div className="conversation-content">
          {!loading && messages.length === 0 && !pending && <div className="chat-welcome"><span className="welcome-symbol"><Sparkle size={32} weight="duotone" /></span><h1>Hôm nay mình có thể<br />giúp gì cho bạn? <span>👋</span></h1><p>Cùng tìm hiểu tài liệu, quy định và chuẩn đầu ra.<br />Bạn cứ hỏi theo cách tự nhiên nhất.</p>
            <div className="chat-suggestions">{["Bạn có thể giúp mình những gì?", "Giải thích yêu cầu về chuẩn đầu ra trong AUN-QA", "Mình muốn tìm quy chế tốt nghiệp"].map(text => <button key={text} onClick={() => { setQuestion(text); textarea.current?.focus(); }}>{text}</button>)}</div>
          </div>}
          {loading && <p className="chat-loading" role="status">Đang mở cuộc trò chuyện…</p>}
          {messages.map(m => <div className="conversation-turn" key={m.id}>
            <div className="human-message"><p>{m.question}</p></div>
            {m.answer && <ChatMessageView answer={m.answer} />}
            {["queued", "processing"].includes(m.status) && <div className="assistant-thinking" role="status"><Sparkle size={20} /><span>{phases[m.phase] ?? "Đang suy nghĩ"}</span><span className="thinking-dots"><i /><i /><i /></span></div>}
            {m.status === "failed" && <div className="chat-turn-error"><p>{m.error}</p><button onClick={() => void send(undefined, m)} disabled={busy}><ArrowClockwise size={16} />Thử lại</button></div>}
          </div>)}
          {pending && !messages.some(m => m.question === pending && ["queued", "processing"].includes(m.status)) && <div className="human-message"><p>{pending}</p></div>}
        </div>
      </div>
      <div className="composer-dock">
        {error && <p className="chat-error" role="alert">{error}</p>}
        <form className="natural-composer" onSubmit={e => void send(e)}>
          <label className="sr-only" htmlFor="natural-question">Nhắn tin cho Trợ lý OBE</label>
          <textarea ref={textarea} id="natural-question" placeholder="Bạn muốn hỏi điều gì?" value={question} onChange={e => setQuestion(e.target.value)} onKeyDown={keyDown} maxLength={2000} rows={1} disabled={busy} />
          <div className="composer-actions"><span><BookOpen size={15} /> Tra cứu nguồn khi cần</span><button type="submit" aria-label="Gửi tin nhắn" title="Gửi tin nhắn" disabled={busy || !question.trim()}><ArrowUp size={21} weight="bold" /></button></div>
        </form>
        <div className="composer-note">AI có thể nhầm lẫn. Hãy kiểm tra nguồn khi sử dụng thông tin. <details><summary>Quyền riêng tư</summary><p>Câu hỏi, lịch sử gần đây và đoạn tài liệu liên quan được gửi tới OpenAI. Hội thoại được lưu riêng theo tài khoản; bạn có thể xóa bất cứ lúc nào.</p></details></div>
      </div>
    </div>
    {confirmationDialog}
  </section>;
}
