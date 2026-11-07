import { useState } from "react";
import { Link } from "react-router-dom";
import Markdown from "react-markdown";
import remarkGfm from "remark-gfm";
import { Copy, Check, BookOpen, ArrowSquareOut } from "@phosphor-icons/react";

type Citation = { key: string; title: string; locator: string; quote: string; url: string };
export type ChatAnswer = { parts: { text: string; citations: Citation[] }[]; notice: string | null; documentCount: number; chunkCount: number;
  databaseSources?: { title: string; url: string; hash: string }[] | null };
function RichText({ children }: { children: string }) {
  return <Markdown remarkPlugins={[remarkGfm]} skipHtml components={{ a: ({ children }) => <span>{children}</span> }}>{children}</Markdown>;
}
export function ChatMessageView({ answer }: { answer: ChatAnswer }) {
  const [copied, setCopied] = useState(false);
  const text = answer.notice ?? answer.parts.map(p => p.text).join("\n\n");
  async function copy() { try { await navigator.clipboard.writeText(text); setCopied(true); setTimeout(() => setCopied(false), 1800); } catch { setCopied(false); } }
  return <div className="assistant-message">
    <div className="assistant-message__body">
      {answer.notice && <div className="chat-markdown"><RichText>{answer.notice}</RichText></div>}
      {!!answer.databaseSources?.length && <details className="answer-sources"><summary><BookOpen size={14} />Nguồn dữ liệu chương trình</summary><div>
        {answer.databaseSources.map((source, i) => <div className="source-reference" key={`${source.hash}-${i}`}><Link to={source.url} target="_blank" rel="noopener noreferrer"><span>{source.title}</span><ArrowSquareOut size={15} /></Link></div>)}
        <small>Dữ liệu tại thời điểm trả lời. Mở nguồn để xem trạng thái hiện tại.</small>
      </div></details>}
      {answer.parts.map((part, index) => <div key={index} className="answer-section"><div className="chat-markdown"><RichText>{part.text}</RichText></div>
        <details className="answer-sources"><summary><BookOpen size={14} />{part.citations.length} nguồn tham khảo</summary><div>
          {part.citations.map((c, i) => <div className="source-reference" key={`${c.key}-${i}`}><Link to={c.url} target="_blank" rel="noopener noreferrer"><span>{c.title} <small>· {c.locator}</small></span><ArrowSquareOut size={15} /></Link><blockquote>{c.quote}</blockquote></div>)}
        </div></details>
      </div>)}
      <div className="answer-actions"><button className="chat-icon" aria-label={copied ? "Đã sao chép" : "Sao chép câu trả lời"} title={copied ? "Đã sao chép" : "Sao chép"} onClick={() => void copy()}>{copied ? <Check size={16} /> : <Copy size={16} />}</button></div>
    </div>
  </div>;
}
