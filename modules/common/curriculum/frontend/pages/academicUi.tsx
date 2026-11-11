import { useEffect, useState } from "react";
import { curriculumApi } from "../api";
import type { CurriculumProgram, ProgramVersion } from "../types";
import type { AcademicPeriodBatch } from "../types/academics";

export const message = (e: unknown) => e instanceof Error ? e.message : "Không xử lý được yêu cầu.";
export const score = (n: number | null | undefined, digits = 2) => n == null ? "—" : n.toLocaleString("vi-VN", { maximumFractionDigits: digits });
export function ErrorMessage({ value }: { value: string }) { return value ? <p className="alert alert--error" role="alert">{value}</p> : null; }
export function Notice({ value }: { value: string }) { return value ? <p className="academic-notice" role="status">{value}</p> : null; }
export function Pager({ page, totalPages, label, onPage }: { page: number; totalPages: number; label: string; onPage: (page: number) => void }) {
  return <div className="pagination"><span>{label} · Trang {page}/{Math.max(1, totalPages)}</span>
    <button type="button" className="button button--ghost" disabled={page <= 1} onClick={() => onPage(page - 1)}>Trước</button>
    <button type="button" className="button button--ghost" disabled={page >= totalPages} onClick={() => onPage(page + 1)}>Sau</button></div>;
}
/** Chọn năm học → học kỳ; giữ nguyên mã đợt đã lưu (ví dụ 2026-2027-HK1). */
export function PeriodPicker({ batches, period, onChange, submitLabel }: { batches: AcademicPeriodBatch[]; period: string; onChange: (period: string) => void; submitLabel: string }) {
  const [batch, setBatch] = useState(""); const [semester, setSemester] = useState("");
  useEffect(() => { const b = batches.find(b => b.semesters.some(s => s.period === period)); if (b) { setBatch(b.name); setSemester(period); } }, [batches, period]);
  const semesters = batches.find(b => b.name === batch)?.semesters ?? [];
  return <form className="toolbar academic-period-selector" onSubmit={e => { e.preventDefault(); if (semesters.some(s => s.period === semester)) onChange(semester); }}>
    <label>Năm học<select required value={batch} onChange={e => { setBatch(e.target.value); setSemester(""); }}><option value="">Chọn năm học</option>{batches.map(b => <option key={b.name}>{b.name}</option>)}</select></label>
    <label>Học kỳ<select required value={semester} disabled={!batch} onChange={e => setSemester(e.target.value)}><option value="">Chọn học kỳ</option>{semesters.map(s => <option key={s.period} value={s.period}>{s.name}</option>)}</select></label>
    <button className="button button--secondary" disabled={!semester}>{submitLabel}</button>
  </form>;
}
/** Phiên bản CTĐT kèm CTĐT và trường (cần cho các API theo trường như danh bạ sinh viên). */
export function useVersionContext(versionId: number) {
  const [state, setState] = useState<{ version: ProgramVersion | null; program: CurriculumProgram | null; error: string }>({ version: null, program: null, error: "" });
  useEffect(() => {
    let active = true;
    void (async () => {
      try { const version = await curriculumApi.getVersion(versionId); const program = await curriculumApi.getProgram(version.programId); if (active) setState({ version, program, error: "" }); }
      catch (e) { if (active) setState({ version: null, program: null, error: message(e) }); }
    })();
    return () => { active = false; };
  }, [versionId]);
  return { ...state, universityId: state.program?.universityId ?? null };
}
