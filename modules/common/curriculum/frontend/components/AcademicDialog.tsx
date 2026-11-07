import { useEffect, useRef, type ReactNode } from "react";

export function AcademicDialog({ title, children, onClose }: { title: string; children: ReactNode; onClose: () => void }) {
  const ref = useRef<HTMLDialogElement>(null);
  useEffect(() => { const previous = document.activeElement as HTMLElement | null; ref.current?.showModal(); return () => { ref.current?.close(); previous?.focus(); }; }, []);
  return <dialog className="academic-dialog" ref={ref} aria-label={title} onCancel={e => { e.preventDefault(); onClose(); }}>
    <header><h2>{title}</h2><button type="button" className="button button--ghost" onClick={onClose} aria-label="Đóng">×</button></header>{children}
  </dialog>;
}
