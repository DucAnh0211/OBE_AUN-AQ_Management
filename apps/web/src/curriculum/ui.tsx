import type { ReactNode } from "react";
import { ApiError } from "../../../../modules/common/curriculum/frontend/api";
import { AccreditationApiError } from "../../../../modules/sv4/accreditation/frontend";
import { DialogFrame } from "../components/Dialogs";

export function formatDate(value: string | null): string {
  return value
    ? new Intl.DateTimeFormat("vi-VN", { dateStyle: "short", timeStyle: "short" }).format(new Date(value))
    : "—";
}

export function statusLabel(status: string): string {
  return ({ draft: "Bản nháp", finalized: "Đã công bố", archived: "Đã lưu trữ", active: "Đang sử dụng" })[status] ?? status;
}

export function errorInfo(error: unknown): { message: string; fields: Record<string, string[]> } {
  if (error instanceof ApiError || error instanceof AccreditationApiError) return { message: error.message, fields: error.fieldErrors };
  return { message: "Đã xảy ra lỗi không xác định.", fields: {} };
}

export function PageHeader({ eyebrow, title, description, actions, after }: {
  eyebrow?: ReactNode;
  title: string;
  description: string;
  actions?: ReactNode;
  /** Rendered right after the heading band (e.g. section navigation). */
  after?: ReactNode;
}) {
  return (
    <>
    <div className="admin-heading">
      <div>
        {eyebrow && (typeof eyebrow === "string" ? <span className="eyebrow">{eyebrow}</span> : eyebrow)}
        <h1>{title}</h1>
        <p>{description}</p>
      </div>
      {actions && <div className="admin-heading__actions">{actions}</div>}
    </div>
    {after}
    </>
  );
}

export function Alert({ message, onRetry }: { message: string; onRetry?: () => void }) {
  return (
    <div className="alert alert--error" role="alert">
      <span>{message}</span>
      {onRetry && <button className="button button--small button--ghost" onClick={onRetry}>Thử lại</button>}
    </div>
  );
}

export function EmptyState({ children }: { children: ReactNode }) {
  return <div className="empty-state">{children}</div>;
}

export function LoadingState() {
  return <div className="empty-state" aria-live="polite">Đang tải dữ liệu…</div>;
}

export function Dialog({ title, description, children, onClose }: {
  title: string;
  description?: string;
  children: ReactNode;
  onClose: () => void;
}) {
  return (
    <DialogFrame title={title} description={description} onClose={onClose}>
      {children}
    </DialogFrame>
  );
}

export function FieldError({ errors }: { errors?: string[] }) {
  return errors?.length ? <span className="field-error">{errors[0]}</span> : null;
}

export function Pagination({ page, totalPages, totalItems, onChange }: {
  page: number;
  totalPages: number;
  totalItems: number;
  onChange: (page: number) => void;
}) {
  return (
    <div className="pagination">
      <span>Tổng cộng {totalItems} mục</span>
      <div>
        <button className="button button--small button--ghost" disabled={page <= 1} onClick={() => onChange(page - 1)}>Trước</button>
        <span>Trang {page}/{Math.max(totalPages, 1)}</span>
        <button className="button button--small button--ghost" disabled={page >= totalPages} onClick={() => onChange(page + 1)}>Sau</button>
      </div>
    </div>
  );
}

// Calendar dates arrive as yyyy-MM-dd and must not shift across time zones.
export function formatDay(value: string | null): string {
  if (!value) return "—";
  const [year, month, day] = value.split("-");
  return `${day}/${month}/${year}`;
}
