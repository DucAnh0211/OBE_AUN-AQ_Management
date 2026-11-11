import { useEffect, useId, useRef, useState } from "react";
import { Link } from "react-router-dom";

export type RowMenuItem = { label: string; to?: string; onSelect?: () => void; disabled?: boolean };
/** Thao tác phụ của một dòng gom vào menu "⋯": mở bằng chuột/bàn phím, đóng bằng Esc hoặc bấm ra ngoài. */
export function RowMenu({ label, items }: { label: string; items: RowMenuItem[] }) {
  const [open, setOpen] = useState(false); const id = useId(); const root = useRef<HTMLDivElement>(null); const button = useRef<HTMLButtonElement>(null);
  useEffect(() => {
    if (!open) return;
    const outside = (e: MouseEvent) => { if (!root.current?.contains(e.target as Node)) setOpen(false); };
    const key = (e: KeyboardEvent) => { if (e.key === "Escape") { setOpen(false); button.current?.focus(); } };
    document.addEventListener("mousedown", outside); document.addEventListener("keydown", key);
    root.current?.querySelector<HTMLElement>("[role=menuitem]:not([aria-disabled=true])")?.focus();
    return () => { document.removeEventListener("mousedown", outside); document.removeEventListener("keydown", key); };
  }, [open]);
  function move(e: React.KeyboardEvent) {
    if (e.key !== "ArrowDown" && e.key !== "ArrowUp") return; e.preventDefault();
    const list = [...(root.current?.querySelectorAll<HTMLElement>("[role=menuitem]") ?? [])];
    const index = list.indexOf(document.activeElement as HTMLElement);
    list[(index + (e.key === "ArrowDown" ? 1 : list.length - 1)) % list.length]?.focus();
  }
  if (!items.length) return null;
  return <div className="row-menu" ref={root}>
    <button ref={button} type="button" className="button button--ghost button--small row-menu__trigger" aria-haspopup="menu" aria-expanded={open} aria-controls={id} aria-label={label} title={label} onClick={() => setOpen(o => !o)}>⋯</button>
    {open && <div className="row-menu__panel" id={id} role="menu" onKeyDown={move}>
      {items.map(item => item.to && !item.disabled
        ? <Link key={item.label} role="menuitem" className="row-menu__item" to={item.to} onClick={() => setOpen(false)}>{item.label}</Link>
        : <button key={item.label} type="button" role="menuitem" className="row-menu__item" aria-disabled={item.disabled} disabled={item.disabled} onClick={() => { setOpen(false); item.onSelect?.(); }}>{item.label}</button>)}
    </div>}
  </div>;
}
