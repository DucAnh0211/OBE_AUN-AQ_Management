import { useEffect, useId, useRef, type FormEvent, type KeyboardEvent, type RefObject } from "react";
import { ArrowUp, BookOpen } from "@phosphor-icons/react";
import "./knowledge-chat.css";

const maximumQuestionLength = 2000;
const maximumHeight = 200;

export function ChatComposer({ value, placeholder, disabled = false, autoFocus = false, textareaRef, onChange, onSubmit }: {
  value: string;
  placeholder: string;
  disabled?: boolean;
  autoFocus?: boolean;
  textareaRef?: RefObject<HTMLTextAreaElement | null>;
  onChange: (value: string) => void;
  onSubmit: () => void;
}) {
  const id = useId();
  const localRef = useRef<HTMLTextAreaElement>(null);
  const textarea = textareaRef ?? localRef;

  useEffect(() => {
    const element = textarea.current;
    if (!element) return;
    element.style.height = "auto";
    element.style.height = `${Math.min(maximumHeight, element.scrollHeight)}px`;
  }, [textarea, value]);

  function handleSubmit(event: FormEvent) {
    event.preventDefault();
    onSubmit();
  }

  function handleKeyDown(event: KeyboardEvent<HTMLTextAreaElement>) {
    // isComposing keeps Vietnamese IME confirmation from sending the message.
    if (event.key === "Enter" && !event.shiftKey && !event.nativeEvent.isComposing) {
      event.preventDefault();
      onSubmit();
    }
  }

  return <form className="natural-composer" onSubmit={handleSubmit}>
    <label className="sr-only" htmlFor={id}>Nhắn tin cho Trợ lý OBE</label>
    <textarea ref={textarea} id={id} placeholder={placeholder} value={value} onChange={event => onChange(event.target.value)} onKeyDown={handleKeyDown} maxLength={maximumQuestionLength} rows={1} disabled={disabled} autoFocus={autoFocus} />
    <div className="composer-actions">
      <span><BookOpen size={15} aria-hidden="true" /> Tra cứu nguồn khi cần</span>
      <button type="submit" aria-label="Gửi tin nhắn" title="Gửi tin nhắn" disabled={disabled || !value.trim()}><ArrowUp size={18} weight="bold" /></button>
    </div>
  </form>;
}
