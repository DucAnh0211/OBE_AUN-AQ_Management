import type { ReactNode } from "react";

/** Thẻ tóm tắt thực thể dạng nhãn – giá trị; viền trái lấy màu nhóm của trang. */
export function SummaryCard({ items, label, actions }: { items: [string, ReactNode][]; label: string; actions?: ReactNode }) {
  return <section className="summary-card" aria-label={label}>
    <dl>{items.map(([name, value]) => <div key={name}><dt>{name}</dt><dd>{value ?? "—"}</dd></div>)}</dl>
    {actions && <div className="summary-card__actions">{actions}</div>}
  </section>;
}
/** Sĩ số hiện tại / tối đa kèm thanh tiến độ (không giới hạn thì chỉ hiện số). */
export function Capacity({ count, capacity }: { count: number; capacity: number | null }) {
  if (!capacity) return <>{count}</>;
  const ratio = Math.min(1, count / capacity);
  return <span className="capacity"><span>{count}/{capacity}</span><span className="capacity__bar" aria-hidden="true"><span style={{ width: `${ratio * 100}%` }} className={ratio >= 1 ? "is-full" : ""} /></span></span>;
}
