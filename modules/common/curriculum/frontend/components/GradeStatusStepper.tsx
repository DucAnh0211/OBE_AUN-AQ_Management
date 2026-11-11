import type { ReactNode } from "react";
import type { GradeStatus } from "../types/offerings";

const steps: { key: GradeStatus; label: string; hint: string }[] = [
  { key: "draft", label: "Đang nhập điểm", hint: "Giảng viên nhập điểm" },
  { key: "submitted", label: "Đã nộp", hint: "Chờ phòng đào tạo công bố" },
  { key: "published", label: "Đã công bố", hint: "Điểm vào bảng điểm sinh viên" },
];
/** Quy trình sổ điểm: Đang nhập → Đã nộp → Đã công bố; bước hiện tại tô màu, bước đã qua đánh dấu hoàn thành. */
export function GradeStatusStepper({ status, detail, actions }: { status: GradeStatus; detail?: ReactNode; actions?: ReactNode }) {
  const current = steps.findIndex(s => s.key === status);
  return <section className={`grade-stepper grade-stepper--${status}`} aria-label={`Sổ điểm: ${steps[current].label}`}>
    <ol>{steps.map((s, i) => <li key={s.key} className={i < current ? "is-done" : i === current ? "is-current" : ""} aria-current={i === current ? "step" : undefined}>
      <span className="grade-stepper__mark">{i < current ? "✓" : i + 1}</span><span><strong>{s.label}</strong><small>{s.hint}</small></span></li>)}</ol>
    <div className="grade-stepper__footer"><div><strong>Sổ điểm: {steps[current].label}</strong>{detail}</div>{actions && <div className="table-actions">{actions}</div>}</div>
  </section>;
}
