import type { ReactNode } from "react";
import { attemptLabels, type AttemptKind } from "../types/academics";
import { studentStatusLabels, type StudentStatus } from "../types/directory";
import { gradeStatusLabels, lecturerRoleLabels, type GradeStatus, type LecturerRole } from "../types/offerings";

/** Chip trạng thái dùng chung: màu theo ý nghĩa, có chấm màu để không chỉ dựa vào màu chữ. */
export function Chip({ tone, children, dot = true, title }: { tone: string; children: ReactNode; dot?: boolean; title?: string }) {
  return <span className={`status status--${tone}${dot ? " status--dot" : ""}`} title={title}>{children}</span>;
}
export const GradeStatusChip = ({ status }: { status: GradeStatus }) => <Chip tone={`grade-${status}`}>{gradeStatusLabels[status]}</Chip>;
export const AttemptChip = ({ kind }: { kind: AttemptKind }) => kind === "first" ? <span className="muted-inline">{attemptLabels.first}</span> : <Chip tone={`attempt-${kind}`}>{attemptLabels[kind]}</Chip>;
export const StudentStatusChip = ({ status }: { status: StudentStatus }) => <Chip tone={`student-${status}`}>{studentStatusLabels[status]}</Chip>;
export const RoleChip = ({ role }: { role: LecturerRole }) => <span className={`role-chip role-chip--${role}`}>{role === "main" ? "Chính" : lecturerRoleLabels[role]}</span>;
/** Điểm chữ: A xanh lục, B xanh dương, C vàng, D cam, F đỏ; I/R/P trung tính. */
export function LetterChip({ letter }: { letter: string | null | undefined }) {
  if (!letter) return <span className="muted-inline">—</span>;
  const tone = /^[A-F]/.test(letter) ? letter[0] : "X";
  return <span className={`grade-letter grade-letter--${tone}`}>{letter}</span>;
}
export function ResultChip({ passed, letter, provisional }: { passed: boolean | null; letter?: string | null; provisional?: boolean }) {
  if (letter === "I") return <Chip tone="grade-draft">Chưa hoàn thiện</Chip>;
  if (passed == null) return <span className="muted-inline">—</span>;
  return <span className={`status status--dot status--${passed ? "pass" : "fail"}${provisional ? " status--provisional" : ""}`} title={provisional ? "Kết quả dự kiến, chưa công bố" : undefined}>
    {passed ? "Đạt" : "Không đạt"}{provisional ? " · dự kiến" : ""}</span>;
}
