import { useEffect, useRef, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { ArrowClockwise, Plus, Sparkle } from "@phosphor-icons/react";
import { apiJson } from "../../../../shared/frontend/auth-client";
import { ChatComposer } from "./ChatComposer";
import { ChatHistoryMenu } from "./ChatHistoryMenu";
import { ChatMessageView, type ChatAnswer } from "./ChatMessageView";
import { chatThreadsPath as base, ChatThreadsProvider, useChatThreads, type Thread } from "./ChatThreads";
import "./knowledge-chat.css";

type Exchange = { id: string; question: string; answer: ChatAnswer | null; status: string; phase: string; error: string | null };
type Detail = { thread: Thread; messages: Exchange[] };
const message = (e: unknown) => e instanceof Error ? e.message : "Chưa kết nối được. Bạn thử lại nhé.";
const phases: Record<string, string> = { queued: "Đang chờ trợ lý", route: "Đang suy nghĩ", conversation: "Đang soạn câu trả lời", retrieval: "Đang đọc tài liệu", complete: "Đang hoàn tất" };
const suggestions = ["Trợ lý có thể hỗ trợ những gì?", "Giải thích yêu cầu về chuẩn đầu ra trong AUN-QA", "Tìm quy chế tốt nghiệp"];

function Conversation() {
  const [params, setParams] = useSearchParams(); const selected = params.get("thread");
  const { refresh: refreshThreads } = useChatThreads();
  const selectedRef = useRef(selected); selectedRef.current = selected;
  const [detail, setDetail] = useState<Detail | null>(null); const [question, setQuestion] = useState("");
  const [error, setError] = useState(""); const [loading, setLoading] = useState(false); const [sending, setSending] = useState(false);
  const [pending, setPending] = useState("");
  const textarea = useRef<HTMLTextAreaElement>(null); const scroll = useRef<HTMLDivElement>(null); const nearBottom = useRef(true);
  const keepPending = useRef(false); // A thread created while sending must keep its pending bubble.

  useEffect(() => {
    setDetail(null); setError(""); nearBottom.current = true;
    if (keepPending.current) keepPending.current = false;
    else { setPending(""); setQuestion(""); }
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
  async function send(retry?: Exchange) {
    const asked = retry?.question ?? question.trim(); if (!asked || busy) return;
    setSending(true); setError(""); setPending(asked); nearBottom.current = true;
    let id = selected;
    try {
      if (!id) { const t = await apiJson<Thread>(base, { method: "POST" }); id = t.id; selectedRef.current = id; keepPending.current = true; setParams({ thread: id }); }
      await apiJson(`${base}/${id}/messages`, { method: "POST", body: JSON.stringify({ requestId: retry?.id ?? crypto.randomUUID(), question: asked }) });
      if (selectedRef.current === id) {
        setQuestion(""); const value = await apiJson<Detail>(`${base}/${id}`);
        if (selectedRef.current === id) setDetail(value);
      }
      await refreshThreads();
    } catch (e) { if (selectedRef.current === id) { setError(message(e)); if (!retry) setQuestion(asked); } }
    finally { setSending(false); setPending(""); textarea.current?.focus(); }
  }

  const empty = !loading && !sending && messages.length === 0 && !pending;
  return <section className="chat-studio" aria-label="Trợ lý tài liệu">
    <header className="conversation-toolbar">
      <div className="conversation-toolbar__title"><Sparkle size={17} weight="fill" aria-hidden="true" /><h1>{detail?.thread.title ?? "Trợ lý tài liệu"}</h1></div>
      <div className="conversation-toolbar__actions">
        <ChatHistoryMenu />
        <Link className="button button--small button--ghost conversation-toolbar__new" to="/knowledge/chat" aria-label="Cuộc trò chuyện mới" title="Cuộc trò chuyện mới"><Plus size={14} weight="bold" aria-hidden="true" /><span>Cuộc trò chuyện mới</span></Link>
      </div>
    </header>
    <div className={`conversation-main ${empty ? "is-empty" : ""}`}>
      <div className="conversation-scroll" ref={scroll} onScroll={() => { const el = scroll.current; if (el) nearBottom.current = el.scrollHeight - el.scrollTop - el.clientHeight < 100; }}>
        <div className="conversation-content">
          {empty && <div className="chat-welcome"><h2>Tôi có thể giúp gì cho bạn?</h2><p>Tra cứu tài liệu đã nạp, quy định và chuẩn đầu ra, kèm nguồn để kiểm chứng.</p></div>}
          {loading && !pending && <p className="chat-loading" role="status">Đang mở cuộc trò chuyện…</p>}
          {messages.map(m => <div className="conversation-turn" key={m.id}>
            <div className="human-message"><p>{m.question}</p></div>
            {m.answer && <ChatMessageView answer={m.answer} />}
            {["queued", "processing"].includes(m.status) && <div className="assistant-thinking" role="status"><Sparkle size={20} weight="fill" /><span>{phases[m.phase] ?? "Đang suy nghĩ"}</span><span className="thinking-dots"><i /><i /><i /></span></div>}
            {m.status === "failed" && <div className="chat-turn-error"><p>{m.error}</p><button onClick={() => void send(m)} disabled={busy}><ArrowClockwise size={16} />Thử lại</button></div>}
          </div>)}
          {pending && !messages.some(m => m.question === pending && ["queued", "processing"].includes(m.status)) && <div className="human-message"><p>{pending}</p></div>}
        </div>
      </div>
      <div className="composer-dock">
        {error && <p className="chat-error" role="alert">{error}</p>}
        <ChatComposer textareaRef={textarea} value={question} placeholder="Bạn muốn hỏi điều gì?" disabled={busy} onChange={setQuestion} onSubmit={() => void send()} />
        {empty && <div className="chat-suggestions">{suggestions.map(text => <button key={text} onClick={() => { setQuestion(text); textarea.current?.focus(); }}>{text}</button>)}</div>}
        <div className="composer-note">AI có thể nhầm lẫn. Hãy kiểm tra nguồn khi sử dụng thông tin. <details><summary>Quyền riêng tư</summary><p>Câu hỏi, lịch sử gần đây và đoạn tài liệu liên quan được gửi tới OpenAI. Hội thoại được lưu riêng theo tài khoản; bạn có thể xóa bất cứ lúc nào.</p></details></div>
      </div>
    </div>
  </section>;
}

export function KnowledgeChatPage() {
  return <ChatThreadsProvider><Conversation /></ChatThreadsProvider>;
}
