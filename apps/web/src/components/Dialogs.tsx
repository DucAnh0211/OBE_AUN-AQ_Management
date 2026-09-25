import { useCallback, useEffect, useId, useRef, useState, type FormEvent, type ReactNode } from "react";

type DialogFrameProps = {
  title: string;
  description?: string;
  children: ReactNode;
  onClose: () => void;
  className?: string;
};

export function DialogFrame({ title, description, children, onClose, className = "" }: DialogFrameProps) {
  const panelRef = useRef<HTMLElement>(null);
  const onCloseRef = useRef(onClose);
  const titleId = useId();
  const descriptionId = useId();
  useEffect(() => { onCloseRef.current = onClose; }, [onClose]);
  useEffect(() => {
    const previous = document.activeElement as HTMLElement | null;
    panelRef.current?.querySelector<HTMLElement>("[autofocus], input:not([disabled]), select:not([disabled]), textarea:not([disabled]), button:not(.icon-button):not([disabled])")?.focus();
    function handleKeyDown(event: KeyboardEvent) {
      if (event.key === "Escape") { onCloseRef.current(); return; }
      if (event.key !== "Tab" || !panelRef.current) return;
      const focusable = Array.from(panelRef.current.querySelectorAll<HTMLElement>("button:not([disabled]), input:not([disabled]), select:not([disabled]), textarea:not([disabled]), a[href]"));
      if (!focusable.length) return;
      const first = focusable[0], last = focusable[focusable.length - 1];
      if (event.shiftKey && document.activeElement === first) { event.preventDefault(); last.focus(); }
      else if (!event.shiftKey && document.activeElement === last) { event.preventDefault(); first.focus(); }
    }
    document.addEventListener("keydown", handleKeyDown);
    return () => { document.removeEventListener("keydown", handleKeyDown); previous?.focus(); };
  }, []);
  return <div className="dialog-backdrop" role="presentation" onMouseDown={(event) => {
    if (event.target === event.currentTarget) onClose();
  }}>
    <section ref={panelRef} className={`dialog ${className}`.trim()} role="dialog" aria-modal="true" aria-labelledby={titleId} aria-describedby={description ? descriptionId : undefined}>
      <div className="dialog__heading"><div><h2 id={titleId}>{title}</h2>{description && <p id={descriptionId}>{description}</p>}</div>
        <button className="icon-button" type="button" aria-label="Đóng hộp thoại" onClick={onClose}>×</button>
      </div>
      {children}
    </section>
  </div>;
}

type ConfirmOptions = { title: string; description: string; confirmLabel?: string; tone?: "danger" | "primary" };

export function useConfirmDialog() {
  const [options, setOptions] = useState<ConfirmOptions | null>(null);
  const resolver = useRef<((confirmed: boolean) => void) | null>(null);
  const close = useCallback((confirmed: boolean) => {
    resolver.current?.(confirmed); resolver.current = null; setOptions(null);
  }, []);
  const confirm = useCallback((next: ConfirmOptions) => new Promise<boolean>((resolve) => {
    resolver.current = resolve; setOptions(next);
  }), []);
  const confirmationDialog = options ? <DialogFrame title={options.title} description={options.description} onClose={() => close(false)}>
    <div className="dialog__actions">
      <button className="button button--ghost" type="button" onClick={() => close(false)}>Hủy</button>
      <button className={`button ${options.tone === "danger" ? "button--danger-solid" : "button--primary"}`} type="button" onClick={() => close(true)}>{options.confirmLabel ?? "Xác nhận"}</button>
    </div>
  </DialogFrame> : null;
  return { confirm, confirmationDialog };
}

type InputOptions = {
  title: string; description?: string; label: string; initialValue?: string; placeholder?: string;
  inputType?: "text" | "password" | "textarea"; minimumLength?: number; submitLabel?: string;
};

export function useInputDialog() {
  const [options, setOptions] = useState<InputOptions | null>(null);
  const [value, setValue] = useState("");
  const resolver = useRef<((value: string | null) => void) | null>(null);
  const close = useCallback((result: string | null) => {
    resolver.current?.(result); resolver.current = null; setOptions(null);
  }, []);
  const requestInput = useCallback((next: InputOptions) => new Promise<string | null>((resolve) => {
    resolver.current = resolve; setValue(next.initialValue ?? ""); setOptions(next);
  }), []);
  function submit(event: FormEvent) { event.preventDefault(); close(value); }
  const inputDialog = options ? <DialogFrame title={options.title} description={options.description} onClose={() => close(null)}>
    <form className="admin-form" onSubmit={submit}><label><span>{options.label}</span>
      {options.inputType === "textarea"
        ? <textarea required autoFocus value={value} onChange={(event) => setValue(event.target.value)} placeholder={options.placeholder} />
        : <input required autoFocus type={options.inputType ?? "text"} minLength={options.minimumLength} value={value} onChange={(event) => setValue(event.target.value)} placeholder={options.placeholder} />}
    </label><div className="dialog__actions"><button className="button button--ghost" type="button" onClick={() => close(null)}>Hủy</button><button className="button button--primary">{options.submitLabel ?? "Lưu"}</button></div></form>
  </DialogFrame> : null;
  return { requestInput, inputDialog };
}
