import { useCallback, useEffect, useRef, useState, type FormEvent } from "react";
import { Link, useParams } from "react-router-dom";
import { curriculumApi } from "../api";
import { offeringApi } from "../api/offerings";
import type { CourseOffering } from "../types/offerings";
import { academicApi } from "../api/academics";
import type { ProgramCourse, ProgramVersion, PagedResult } from "../types";
import type { AssessmentScheme, Enrollment, Gradebook, AssessmentComponent } from "../types/academics";
import { AcademicDialog } from "../components/AcademicDialog";
import { AcademicImportDialog } from "../components/AcademicImportDialog";
import "./academics.css";

const message = (e: unknown) => e instanceof Error ? e.message : "Không xử lý được yêu cầu.";
const display = (n: number | null | undefined) => n == null ? "Chưa đủ điểm" : n.toLocaleString("vi-VN", { maximumFractionDigits: 2 });
function ErrorMessage({ value }: { value: string }) { return value ? <p className="alert alert--error" role="alert">{value}</p> : null; }
function CourseHeading({ course, versionId, children }: { course: ProgramCourse | null; versionId: number; children?: React.ReactNode }) {
  return <><div className="breadcrumbs"><Link to={`/curriculum/versions/${versionId}`}>Danh sách bộ môn</Link><span>/</span><span>{course?.institutionalCode ?? "…"}</span></div><div className="admin-heading"><div><span className="eyebrow">{course?.institutionalCode}</span><h1>{course?.name ?? "Học phần"}</h1></div><div className="admin-heading__actions">{children}</div></div></>;
}

type AssessmentDraft = Omit<AssessmentComponent, "weight" | "maxScore"> & { weight: string; maxScore: string };
const assessmentDraft = (components: AssessmentComponent[]): AssessmentDraft[] => components.map(c => ({ ...c, weight: String(c.weight), maxScore: String(c.maxScore) }));

export function AssessmentSchemePage({ isAdmin }: { isAdmin: boolean }) {
  const versionId = Number(useParams().versionId);
  const [scheme, setScheme] = useState<AssessmentScheme | null>(null); const [version, setVersion] = useState<ProgramVersion | null>(null);
  const [components, setComponents] = useState<AssessmentDraft[]>([]); const [error, setError] = useState(""); const [notice, setNotice] = useState(""); const [busy, setBusy] = useState(false);
  const load = useCallback(async () => { setError(""); try { const [s, v] = await Promise.all([academicApi.scheme(versionId), curriculumApi.getVersion(versionId)]); setScheme(s); setComponents(assessmentDraft(s.components)); setVersion(v); } catch (e) { setError(message(e)); } }, [versionId]);
  useEffect(() => { void load(); }, [load]);
  const editable = isAdmin && !!scheme && !scheme.locked;
  const totalWeightUnits = components.reduce((sum, c) => sum + Math.round((Number(c.weight) || 0) * 100), 0);
  const totalWeight = (totalWeightUnits / 100).toFixed(2);
  function update(index: number, value: Partial<AssessmentDraft>) { setComponents(list => list.map((c, i) => i === index ? { ...c, ...value } : c)); setNotice(""); setError(""); }
  async function save(e: FormEvent) {
    e.preventDefault(); if (!scheme) return; setError(""); setNotice("");
    if (components.some(c => !c.weight.trim() || !c.maxScore.trim() || !Number.isFinite(Number(c.weight)) || !Number.isFinite(Number(c.maxScore)))) { setError("Hãy nhập đầy đủ trọng số và điểm tối đa cho tất cả đầu điểm."); return; }
    if (totalWeightUnits !== 10000) { setError(`Tổng trọng số hiện tại là ${totalWeight}%. Tổng trọng số các đầu điểm phải bằng 100% để lưu cơ cấu điểm.`); return; }
    setBusy(true);
    try {
      const values = components.map(c => ({ ...c, weight: Number(c.weight), maxScore: Number(c.maxScore) }));
      const s = await academicApi.saveScheme(versionId, { ...scheme, components: values });
      setScheme(s); setComponents(assessmentDraft(s.components)); setNotice("Đã lưu cơ cấu điểm dùng chung cho tất cả học phần trong phiên bản này.");
    } catch (e) { setError(message(e)); } finally { setBusy(false); }
  }
  return <section className="admin-page academic-page"><div className="breadcrumbs"><Link to={`/curriculum/versions/${versionId}`}>Danh sách bộ môn</Link><span>/</span><span>Cơ cấu điểm</span></div>
    <div className="admin-heading"><div><span className="eyebrow">CTĐT · {version?.versionCode}</span><h1>Cơ cấu điểm chung</h1><p>Thiết lập đầu điểm theo quy định CTĐT. Tổng trọng số phải bằng 100%; không tự gán tỷ lệ mặc định.</p></div></div>
    <ErrorMessage value={error} />{notice && <p role="status" className="academic-notice">{notice}</p>}
    {scheme?.locked && <p className="academic-notice">Cơ cấu điểm đã khóa vì có lịch sử nhập điểm hoặc phiên bản đã lưu trữ. Tạo phiên bản CTĐT mới khi cần thay đổi cơ cấu.</p>}
    <form onSubmit={e => void save(e)} className="admin-form">
      <div className="data-table-wrap"><table className="data-table"><thead><tr><th>Mã đầu điểm</th><th>Tên đầu điểm</th><th>Trọng số (%)</th><th>Điểm tối đa</th>{editable && <th>Thao tác</th>}</tr></thead><tbody>{components.map((c, i) => <tr key={i}><td><input aria-label={`Mã đầu điểm ${i + 1}`} value={c.code} disabled={!editable || busy} required pattern="[A-Z][A-Z0-9_]{0,19}" maxLength={20} placeholder="Ví dụ: QT" onChange={e => update(i, { code: e.target.value.toUpperCase() })} /></td><td><input aria-label={`Tên đầu điểm ${i + 1}`} value={c.name} disabled={!editable || busy} required maxLength={100} onChange={e => update(i, { name: e.target.value })} /></td><td><input aria-label={`Trọng số ${i + 1}`} aria-describedby="assessment-weight-total" type="number" placeholder="Nhập trọng số" value={c.weight} disabled={!editable || busy} min="0.01" max="100" step="0.01" required onChange={e => update(i, { weight: e.target.value })} /></td><td><input aria-label={`Điểm tối đa ${i + 1}`} type="number" value={c.maxScore} disabled={!editable || busy} min="0.01" max="1000" step="0.01" required onChange={e => update(i, { maxScore: e.target.value })} /></td>{editable && <td><button className="button button--danger" type="button" disabled={busy} onClick={() => setComponents(rows => rows.filter((_, j) => i !== j))}>Bỏ đầu điểm</button></td>}</tr>)}</tbody></table></div>
      {!components.length && <p className="empty-state">Chưa cấu hình đầu điểm. Admin cần nhập cơ cấu chính thức trước khi nhập điểm sinh viên.</p>}
      <div id="assessment-weight-total" aria-live="polite"><p>Tổng trọng số: <strong>{totalWeight}%</strong>.</p>{editable && components.length > 0 && <p>{components.some(c => !c.weight.trim()) ? "Hãy nhập trọng số cho từng đầu điểm. Tổng trọng số phải bằng 100%." : totalWeightUnits === 10000 ? "Tổng trọng số đã đủ 100%." : totalWeightUnits < 10000 ? `Cần bổ sung ${((10000 - totalWeightUnits) / 100).toFixed(2)}% để đủ 100%.` : `Tổng trọng số vượt ${((totalWeightUnits - 10000) / 100).toFixed(2)}%. Hãy điều chỉnh về 100%.`}</p>}</div>
      <p>Điểm tổng kết quy về thang 10, chỉ tính khi đủ mọi đầu điểm; chưa tự quy đổi điểm chữ hoặc xếp loại.</p>
      {editable && <div className="dialog__actions"><button type="button" className="button button--secondary" disabled={busy || components.length >= 20} onClick={() => { setComponents(list => [...list, { code: "", name: "", weight: "", maxScore: "10" }]); setError(""); setNotice(""); }}>Thêm đầu điểm</button><button className="button button--primary" disabled={busy || !components.length}>{busy ? "Đang lưu…" : "Lưu cơ cấu điểm"}</button></div>}
    </form>
  </section>;
}

export function CourseStudentsPage() {
  const { versionId: v, offeringId: c } = useParams(); const versionId = Number(v), offeringId = Number(c);
  const [offering, setOffering] = useState<CourseOffering | null>(null); const period = offering?.academicPeriod ?? "";
  const [search, setSearch] = useState(""); const [page, setPage] = useState(1); const loadId = useRef(0);
  const [course, setCourse] = useState<ProgramCourse | null>(null); const [scheme, setScheme] = useState<AssessmentScheme | null>(null);
  const [version, setVersion] = useState<ProgramVersion | null>(null); const [rows, setRows] = useState<PagedResult<Enrollment> | null>(null);
  const [error, setError] = useState(""); const [notice, setNotice] = useState(""); const [loading, setLoading] = useState(true);
  const [dialog, setDialog] = useState<"add" | "import" | null>(null); const [code, setCode] = useState(""); const [name, setName] = useState(""); const [busy, setBusy] = useState(false);
  const load = useCallback(async () => {
    const id = ++loadId.current; setError(""); setLoading(true); setRows(null);
    try {
      const o = await offeringApi.offering(versionId, offeringId); if (id !== loadId.current) return; setOffering(o);
      const [course, scheme, version, roster] = await Promise.all([curriculumApi.getCourse(versionId, o.programCourseId), academicApi.scheme(versionId), curriculumApi.getVersion(versionId), academicApi.students(versionId, offeringId, o.academicPeriod, page, search)]);
      if (id !== loadId.current) return; setCourse(course); setScheme(scheme); setVersion(version); setRows(roster);
    } catch (e) { if (id === loadId.current) setError(message(e)); } finally { if (id === loadId.current) setLoading(false); }
  }, [versionId, offeringId, page, search]);
  useEffect(() => { void load(); return () => { loadId.current++; }; }, [load]);
  useEffect(() => { setDialog(null); setNotice(""); setRows(null); setOffering(null); setCourse(null); setPage(1); setSearch(""); }, [versionId, offeringId]);
  const periodTitle = offering ? `${offering.classCode} · ${offering.academicPeriod}` : "";
  const writable = !!offering && !offering.readOnly && !!course && !version?.archivedAt;
  async function add(e: FormEvent) { e.preventDefault(); setError(""); setBusy(true); try { await academicApi.enroll(versionId, offeringId, code, name, period); setDialog(null); setCode(""); setName(""); setNotice("Đã thêm sinh viên vào học phần."); await load(); } catch (e) { setError(message(e)); } finally { setBusy(false); } }
  return <section className="admin-page academic-page"><CourseHeading course={course} versionId={versionId}><Link className="button button--secondary" to={`/curriculum/versions/${versionId}/assessment-scheme`}>Cơ cấu điểm CTĐT</Link><Link className="button button--ghost" to={`/curriculum/versions/${versionId}/courses/${offering?.programCourseId}/outcomes`}>CLO/PLO</Link></CourseHeading>
    {offering && <p><Link to={`/curriculum/versions/${versionId}/classes/${offering.classId}`}>Lớp {offering.classCode} · {offering.className}</Link> · Học phần mở: <strong>{offering.code}</strong> · {period} · Giảng viên: {offering.lecturerName ?? "Chưa phân công"}</p>}
    <ErrorMessage value={error} />{notice && <p className="academic-notice" role="status">{notice}</p>}
    {period && <><div className="toolbar academic-roster-toolbar"><h2>Sinh viên · {periodTitle}</h2><input aria-label="Tìm sinh viên" placeholder="Tìm MSSV hoặc họ tên" value={search} onChange={e => { setSearch(e.target.value); setPage(1); }} /><button className="button button--primary" disabled={!writable || loading} onClick={() => { setError(""); setDialog("add"); }}>Thêm sinh viên</button><button className="button button--secondary" disabled={!writable || loading} onClick={() => { setError(""); setDialog("import"); }}>Import sinh viên / điểm</button></div>
      {!scheme?.components.length && <p className="academic-notice">Chưa có cơ cấu đầu điểm. Vẫn có thể thêm sinh viên; admin cần cấu hình trước khi nhập điểm.</p>}
      {loading ? <p role="status">Đang tải…</p> : !rows?.items.length ? <p className="empty-state">Chưa có sinh viên trong đợt học hoặc không có kết quả tìm kiếm.</p> : <><div className="data-table-wrap"><table className="data-table"><thead><tr><th>MSSV</th><th>Họ tên</th><th>Đầu điểm đã nhập</th><th>Tổng kết /10</th><th>Thao tác</th></tr></thead><tbody>{rows.items.map(s => <tr key={s.id}><td>{s.studentCode}</td><td><Link to={`/curriculum/versions/${versionId}/offerings/${offeringId}/students/${s.id}/grades`}>{s.fullName}</Link></td><td>{s.gradedComponents}/{scheme?.components.length ?? 0}</td><td>{display(s.totalScore)}</td><td><Link className="button button--small button--secondary" to={`/curriculum/versions/${versionId}/offerings/${offeringId}/students/${s.id}/grades`}>Xem / nhập điểm</Link></td></tr>)}</tbody></table></div><div className="pagination"><span>{rows.totalItems} sinh viên · Trang {page}/{Math.max(1, rows.totalPages)}</span><button className="button button--ghost" disabled={page === 1} onClick={() => setPage(n => n - 1)}>Trước</button><button className="button button--ghost" disabled={page >= rows.totalPages} onClick={() => setPage(n => n + 1)}>Sau</button></div></>}
    </>}
    {dialog === "add" && <AcademicDialog title={`Thêm sinh viên · ${period}`} onClose={busy ? () => {} : () => setDialog(null)}><form className="admin-form" onSubmit={e => void add(e)}><ErrorMessage value={error} /><label>Mã sinh viên<input autoFocus required maxLength={30} value={code} disabled={busy} onChange={e => setCode(e.target.value)} /></label><label>Họ và tên<input required minLength={2} maxLength={200} value={name} disabled={busy} onChange={e => setName(e.target.value)} /></label><p>Mã sinh viên được nhận diện trong cùng trường. Việc thêm danh sách không tạo tài khoản đăng nhập.</p><div className="dialog__actions"><button className="button button--primary" disabled={busy}>Lưu sinh viên</button></div></form></AcademicDialog>}
    {dialog === "import" && <AcademicImportDialog versionId={versionId} offeringId={offeringId} period={period} onClose={() => setDialog(null)} onSaved={text => { setDialog(null); setNotice(text); void load(); }} />}
  </section>;
}

export function StudentGradesPage() {
  const { versionId: v, offeringId: c, enrollmentId: e } = useParams(); const versionId = Number(v), offeringId = Number(c), enrollmentId = Number(e);
  const [offering, setOffering] = useState<CourseOffering | null>(null); const loadId = useRef(0);
  const [book, setBook] = useState<Gradebook | null>(null); const [course, setCourse] = useState<ProgramCourse | null>(null); const [version, setVersion] = useState<ProgramVersion | null>(null);
  const [scores, setScores] = useState<Record<string, string>>({}); const [error, setError] = useState(""); const [notice, setNotice] = useState(""); const [busy, setBusy] = useState(false);
  const accept = (b: Gradebook) => { setBook(b); setScores(Object.fromEntries(Object.entries(b.scores).map(([k, v]) => [k, v == null ? "" : String(v)]))); };
  const load = useCallback(async () => { const id = ++loadId.current; setError(""); setBook(null); setOffering(null); try { const o = await offeringApi.offering(versionId, offeringId); if (id !== loadId.current) return; setOffering(o); const [b, c, v] = await Promise.all([academicApi.grades(versionId, offeringId, enrollmentId), curriculumApi.getCourse(versionId, o.programCourseId), curriculumApi.getVersion(versionId)]); if (id !== loadId.current) return; accept(b); setCourse(c); setVersion(v); } catch (e) { if (id === loadId.current) setError(message(e)); } }, [versionId, offeringId, enrollmentId]);
  useEffect(() => { void load(); return () => { loadId.current++; }; }, [load]);
  const complete = !!book?.scheme.components.length && book.scheme.components.every(c => scores[c.code]?.trim());
  const total = complete ? book!.scheme.components.reduce((sum, c) => sum + Number(scores[c.code]) / c.maxScore * c.weight, 0) / 10 : null;
  const writable = !!offering && !offering.readOnly && !!course && !course.archivedAt && !version?.archivedAt && !!book?.scheme.components.length;
  async function save(event: FormEvent) { event.preventDefault(); if (!book) return; setBusy(true); setError(""); setNotice(""); try { const values = Object.fromEntries(Object.entries(scores).map(([k, v]) => [k, v.trim() ? Number(v) : null])); accept(await academicApi.saveGrades(versionId, offeringId, enrollmentId, book, values)); setNotice("Đã lưu điểm và lịch sử thay đổi."); } catch (e) { setError(message(e)); } finally { setBusy(false); } }
  return <section className="admin-page academic-page"><CourseHeading course={course} versionId={versionId}>{book && <Link className="button button--secondary" to={`/curriculum/versions/${versionId}/offerings/${offeringId}/students?${new URLSearchParams({ period: book.enrollment.academicPeriod })}`}>Danh sách sinh viên</Link>}</CourseHeading><ErrorMessage value={error} />{notice && <p className="academic-notice" role="status">{notice}</p>}
    {book && <><h2>{book.enrollment.studentCode} · {book.enrollment.fullName}</h2><p>Lớp {offering?.classCode} · Học phần mở {offering?.code} · {book.enrollment.academicPeriod}. Đầu điểm dùng chung theo phiên bản CTĐT.</p>
      <form onSubmit={event => void save(event)} className="admin-form"><div className="data-table-wrap"><table className="data-table"><thead><tr><th>Đầu điểm</th><th>Trọng số</th><th>Thang điểm</th><th>Điểm sinh viên</th></tr></thead><tbody>{book.scheme.components.map(c => <tr key={c.code}><td><strong>{c.code}</strong> · {c.name}</td><td>{c.weight}%</td><td>0–{c.maxScore}</td><td><input aria-label={`Điểm ${c.code}`} type="number" min="0" max={c.maxScore} step="0.01" value={scores[c.code] ?? ""} placeholder="Chưa nhập" disabled={!writable || busy} onChange={e => setScores(s => ({ ...s, [c.code]: e.target.value }))} /></td></tr>)}</tbody></table></div>
        <p>Để trống = chưa có điểm / xóa điểm đã lưu. Nhập 0 = điểm 0. Tổng kết dự kiến: <strong>{display(total)}{total == null ? "" : " /10"}</strong>.</p>
        {!book.scheme.components.length && <p>Cần admin <Link to={`/curriculum/versions/${versionId}/assessment-scheme`}>cấu hình cơ cấu điểm</Link> trước.</p>}
        <div className="dialog__actions"><button type="button" className="button button--secondary" disabled={busy} onClick={() => void load()}>Tải lại dữ liệu</button><button className="button button--primary" disabled={!writable || busy}>{busy ? "Đang lưu…" : "Lưu điểm"}</button></div>
      </form>
      <h2>Lịch sử thay đổi gần đây</h2><div className="data-table-wrap"><table className="data-table"><thead><tr><th>Đầu điểm</th><th>Điểm cũ</th><th>Điểm mới</th><th>Người sửa (ID)</th><th>Thời điểm</th><th>Nguồn</th></tr></thead><tbody>{book.history.map((h, i) => <tr key={i}><td>{h.componentCode}</td><td>{h.previousScore ?? "—"}</td><td>{h.newScore ?? "—"}</td><td>{h.actorId}</td><td>{new Date(h.changedAt).toLocaleString("vi-VN")}</td><td>{h.source === "import" ? "Import" : "Nhập tay"}</td></tr>)}</tbody></table></div>
    </>}
  </section>;
}
