import { useCallback, useEffect, useRef, useState, type FormEvent, type ReactNode } from "react";
import { Link, useParams } from "react-router-dom";
import { curriculumApi } from "../api";
import { academicApi } from "../api/academics";
import { directoryApi } from "../api/directory";
import { offeringApi } from "../api/offerings";
import { AcademicDialog } from "../components/AcademicDialog";
import { AcademicImportDialog, offeringImportTarget } from "../components/AcademicImportDialog";
import type { PagedResult, ProgramCourse } from "../types";
import { componentKinds, specialMarks, type AssessmentComponent, type AssessmentCoverage, type AssessmentScheme, type ComponentKind, type Enrollment, type GradeBand, type Gradebook, type GradingPolicy } from "../types/academics";
import type { HomeClass, OrgUnit } from "../types/directory";
import { gradeStatusLabels, type CourseOffering } from "../types/offerings";
import { DomainEyebrow, domainPage, type DomainKey } from "../components/Domain";
import { GradeStatusStepper } from "../components/GradeStatusStepper";
import { AttemptChip, Chip, LetterChip, ResultChip } from "../components/StatusChip";
import { Capacity, SummaryCard } from "../components/SummaryCard";
import { CourseNav, VersionNav } from "../components/VersionNav";
import { LecturerList } from "./OfferingPages";
import { ErrorMessage, Notice, Pager, message, score, useVersionContext } from "./academicUi";
import "./academics.css";

const kindLabel = (kind: ComponentKind) => componentKinds.find(k => k.value === kind)?.label ?? kind;
const round = (value: number, digits: number) => Math.round(value * 10 ** digits + Number.EPSILON) / 10 ** digits;
const band = (value: number, bands: GradeBand[]) => [...bands].sort((a, b) => b.minScore - a.minScore).find(b => value >= b.minScore);

function CourseHeading({ course, versionId, domain, detail, crumbs, children }: { course: ProgramCourse | null; versionId: number; domain: DomainKey; detail?: string; crumbs?: ReactNode; children?: ReactNode }) {
  return <><div className="breadcrumbs"><Link to={`/curriculum/versions/${versionId}`}>Danh sách học phần</Link><span>/</span>{crumbs ?? <span>{course?.institutionalCode ?? "…"}</span>}</div>
    <div className="admin-heading"><div><DomainEyebrow domain={domain} detail={detail ?? (course ? `${course.institutionalCode} · ${course.credits} tín chỉ` : undefined)} /><h1>{course?.name ?? "Học phần"}</h1></div>
      {children && <div className="admin-heading__actions">{children}</div>}</div></>;
}

/** Tổng quan cơ cấu điểm của mọi học phần trong phiên bản CTĐT. */
export function AssessmentOverviewPage() {
  const versionId = Number(useParams().versionId);
  const { version, program } = useVersionContext(versionId);
  const [rows, setRows] = useState<AssessmentCoverage[] | null>(null); const [error, setError] = useState("");
  useEffect(() => { let active = true; void academicApi.coverage(versionId).then(r => { if (active) setRows(r); }).catch(e => { if (active) setError(message(e)); }); return () => { active = false; }; }, [versionId]);
  const done = rows?.filter(r => r.componentCount > 0 && !r.unmappedComponents.length && !r.unassessedClos.length).length ?? 0;
  return <section className={domainPage("assessment", "academic-page")}><div className="breadcrumbs"><Link to={`/curriculum/versions/${versionId}`}>Danh sách học phần</Link><span>/</span><span>Cơ cấu điểm</span></div>
    <div className="admin-heading"><div><DomainEyebrow domain="assessment" detail={version?.versionCode} /><h1>Cơ cấu điểm theo đề cương học phần</h1><p>Mỗi học phần có cơ cấu đánh giá riêng trong đề cương; mỗi đầu điểm cần chỉ rõ CLO được đánh giá để tính mức đạt chuẩn đầu ra.</p></div>
      {program && <div className="admin-heading__actions"><Link className="button button--secondary" to={`/curriculum/universities/${program.universityId}/grading-policy`}>Quy chế & thang điểm</Link></div>}</div>
    <VersionNav versionId={versionId} staff />
    <ErrorMessage value={error} />
    {rows && <SummaryCard label="Tình trạng đề cương" items={[["Học phần", rows.length], ["Đã đủ cơ cấu và liên kết CLO", done], ["Chưa có cơ cấu điểm", rows.filter(r => !r.componentCount).length],
      ["Còn CLO chưa được đánh giá", rows.filter(r => r.unassessedClos.length).length]]} />}
    {!rows ? !error && <p role="status">Đang tải…</p> : <div className="data-table-wrap"><table className="data-table"><thead><tr><th>Học phần</th><th>Tình trạng</th><th>Đầu điểm</th><th>Đầu điểm chưa gắn CLO</th><th>CLO chưa được đánh giá</th><th><span className="sr-only">Thao tác</span></th></tr></thead>
      <tbody>{rows.map(r => <tr key={r.courseId}><td><Link className="table-link" to={`/curriculum/versions/${versionId}/courses/${r.courseId}/assessment`}><strong>{r.courseCode}</strong> · {r.courseName}</Link></td>
        <td>{!r.componentCount ? <Chip tone="grade-draft">Chưa có</Chip> : r.unmappedComponents.length || r.unassessedClos.length ? <Chip tone="grade-submitted">Thiếu liên kết CLO</Chip> : <Chip tone="pass">Đầy đủ</Chip>}</td>
        <td>{r.componentCount || "—"}</td><td>{r.unmappedComponents.join(", ") || "—"}</td><td>{r.unassessedClos.join(", ") || "—"}</td>
        <td className="align-right"><Link className="button button--small button--secondary" to={`/curriculum/versions/${versionId}/courses/${r.courseId}/assessment`}>Đề cương đánh giá</Link></td></tr>)}</tbody></table></div>}
  </section>;
}

type Draft = { code: string; name: string; kind: ComponentKind; weight: string; maxScore: string; clos: Record<string, string> };
const draftOf = (c: AssessmentComponent): Draft => ({ code: c.code, name: c.name, kind: c.kind, weight: String(c.weight), maxScore: String(c.maxScore),
  clos: Object.fromEntries(Object.entries(c.clos ?? {}).map(([k, v]) => [k, String(v)])) });

/** Đề cương đánh giá của một học phần: đầu điểm, trọng số, loại đánh giá và liên kết CLO. */
export function CourseAssessmentPage({ isAdmin }: { isAdmin: boolean }) {
  const { versionId: v, courseId: c } = useParams(); const versionId = Number(v), courseId = Number(c);
  const { universityId } = useVersionContext(versionId);
  const [course, setCourse] = useState<ProgramCourse | null>(null); const [scheme, setScheme] = useState<AssessmentScheme | null>(null); const [policy, setPolicy] = useState<GradingPolicy | null>(null);
  const [rows, setRows] = useState<Draft[]>([]); const [departments, setDepartments] = useState<OrgUnit[]>([]);
  const [department, setDepartment] = useState(""); const [countsTowardGpa, setCountsTowardGpa] = useState(true);
  const [error, setError] = useState(""); const [notice, setNotice] = useState(""); const [busy, setBusy] = useState(false);
  const load = useCallback(async () => {
    setError("");
    try {
      const [course, scheme, policy] = await Promise.all([curriculumApi.getCourse(versionId, courseId), academicApi.scheme(versionId, courseId), academicApi.policy(versionId)]);
      setCourse(course); setScheme(scheme); setPolicy(policy); setRows(scheme.components.map(draftOf));
      setDepartment(course.departmentId ? String(course.departmentId) : ""); setCountsTowardGpa(course.countsTowardGpa ?? true);
    } catch (e) { setError(message(e)); }
  }, [versionId, courseId]);
  useEffect(() => { void load(); }, [load]);
  useEffect(() => { if (!universityId) return; let active = true; void directoryApi.orgUnits(universityId).then(r => { if (active) setDepartments(r.filter(u => u.kind === "department")); }).catch(() => {}); return () => { active = false; }; }, [universityId]);
  const cloCodes = scheme?.cloCodes ?? [];
  const editable = isAdmin && !!scheme && !course?.archivedAt;
  const structureLocked = !!scheme?.locked;
  const totalUnits = rows.reduce((s, r) => s + Math.round((Number(r.weight) || 0) * 100), 0);
  const finalWeight = rows.filter(r => r.kind === "final").reduce((s, r) => s + (Number(r.weight) || 0), 0);
  const minComponents = course && course.credits < 2 ? 1 : policy?.minComponents ?? 1;
  const hints = [
    totalUnits !== 10000 && rows.length ? `Tổng trọng số là ${(totalUnits / 100).toFixed(2)}%, cần đúng 100%.` : "",
    rows.length && rows.length < minComponents ? `Quy chế yêu cầu tối thiểu ${minComponents} đầu điểm.` : "",
    policy?.minFinalWeight && rows.length && finalWeight < policy.minFinalWeight ? `Đánh giá cuối kỳ cần tối thiểu ${policy.minFinalWeight}% (hiện ${finalWeight}%).` : "",
    ...cloCodes.filter(code => !rows.some(r => Number(r.clos[code]) > 0)).map(code => `${code} chưa được đầu điểm nào đánh giá.`),
  ].filter(Boolean);
  function update(i: number, value: Partial<Draft>) { setRows(list => list.map((r, j) => j === i ? { ...r, ...value } : r)); setNotice(""); setError(""); }
  async function save(e: FormEvent) {
    e.preventDefault(); if (!scheme) return; setError(""); setNotice("");
    if (rows.some(r => !r.weight.trim() || !r.maxScore.trim() || !Number.isFinite(Number(r.weight)) || !Number.isFinite(Number(r.maxScore)))) { setError("Hãy nhập đầy đủ trọng số và điểm tối đa cho tất cả đầu điểm."); return; }
    if (totalUnits !== 10000) { setError(`Tổng trọng số hiện tại là ${(totalUnits / 100).toFixed(2)}%. Tổng trọng số các đầu điểm phải bằng 100% để lưu cơ cấu điểm.`); return; }
    setBusy(true);
    try {
      const components = rows.map(r => ({ code: r.code, name: r.name, kind: r.kind, weight: Number(r.weight), maxScore: Number(r.maxScore),
        clos: Object.fromEntries(Object.entries(r.clos).filter(([, v]) => v.trim() && Number(v) > 0).map(([k, v]) => [k, Number(v)])) }));
      const saved = await academicApi.saveScheme(versionId, courseId, { ...scheme, components });
      setScheme(saved); setRows(saved.components.map(draftOf)); setNotice("Đã lưu cơ cấu điểm và liên kết CLO của học phần.");
    } catch (e) { setError(message(e)); } finally { setBusy(false); }
  }
  async function saveCatalog() {
    setBusy(true); setError(""); setNotice("");
    try { await directoryApi.saveCourseCatalog(versionId, courseId, department ? Number(department) : null, countsTowardGpa); setNotice("Đã lưu bộ môn quản lý và cách tính GPA."); }
    catch (e) { setError(message(e)); } finally { setBusy(false); }
  }
  async function template() {
    setBusy(true); setError(""); setNotice("");
    try { const r = await academicApi.applyTemplate(versionId, courseId); setNotice(`Đã áp dụng cơ cấu này cho ${r.updatedCourses} học phần chưa có cơ cấu; ${r.skippedCourses} học phần đã có cơ cấu được giữ nguyên.`); }
    catch (e) { setError(message(e)); } finally { setBusy(false); }
  }
  return <section className={domainPage("assessment", "academic-page")}><CourseHeading course={course} versionId={versionId} domain="assessment" />
    <CourseNav versionId={versionId} courseId={courseId} staff />
    <ErrorMessage value={error} /><Notice value={notice} />
    <fieldset className="academic-fieldset"><legend>Thông tin quản lý học phần</legend><div className="academic-form-row">
      <label>Bộ môn quản lý<select value={department} disabled={!isAdmin || busy} onChange={e => setDepartment(e.target.value)}><option value="">Chưa gán bộ môn</option>{departments.map(d => <option key={d.id} value={d.id}>{d.code} · {d.name}</option>)}</select></label>
      <label className="check-control"><input type="checkbox" checked={countsTowardGpa} disabled={!isAdmin || busy} onChange={e => setCountsTowardGpa(e.target.checked)} /> Tính vào điểm trung bình (bỏ chọn với GDTC, GDQP-AN)</label>
      {isAdmin && <button type="button" className="button button--secondary" disabled={busy} onClick={() => void saveCatalog()}>Lưu thông tin</button>}</div></fieldset>
    <h2>Cơ cấu điểm trong đề cương</h2>
    {structureLocked && <p className="academic-notice">Đã có lịch sử nhập điểm: chỉ sửa được tên đầu điểm và liên kết CLO. Thay đổi trọng số, thang điểm hoặc loại đánh giá cần phiên bản CTĐT mới.</p>}
    {policy && <p>Quy chế của trường: tối thiểu {minComponents} đầu điểm{policy.minFinalWeight ? `, cuối kỳ ≥ ${policy.minFinalWeight}%` : ""}; điểm học phần làm tròn {policy.courseScoreDecimals} chữ số thập phân rồi quy đổi điểm chữ.</p>}
    <form className="admin-form" onSubmit={e => void save(e)}>
      <div className="data-table-wrap"><table className="data-table"><thead><tr><th>Mã</th><th>Tên đầu điểm</th><th>Loại</th><th>Trọng số (%)</th><th>Điểm tối đa</th>{cloCodes.map(code => <th key={code} title="% đóng góp vào CLO">{code}</th>)}{editable && !structureLocked && <th></th>}</tr></thead>
        <tbody>{rows.map((r, i) => <tr key={i}>
          <td><input aria-label={`Mã đầu điểm ${i + 1}`} required pattern="[A-Z][A-Z0-9_]{0,19}" maxLength={20} placeholder="QT" value={r.code} disabled={!editable || structureLocked || busy} onChange={e => update(i, { code: e.target.value.toUpperCase() })} /></td>
          <td><input aria-label={`Tên đầu điểm ${i + 1}`} required maxLength={100} value={r.name} disabled={!editable || busy} onChange={e => update(i, { name: e.target.value })} /></td>
          <td><select aria-label={`Loại đầu điểm ${i + 1}`} value={r.kind} disabled={!editable || structureLocked || busy} onChange={e => update(i, { kind: e.target.value as ComponentKind })}>{componentKinds.map(k => <option key={k.value} value={k.value}>{k.label}</option>)}</select></td>
          <td><input aria-label={`Trọng số ${i + 1}`} type="number" min="0.01" max="100" step="0.01" required value={r.weight} disabled={!editable || structureLocked || busy} onChange={e => update(i, { weight: e.target.value })} /></td>
          <td><input aria-label={`Điểm tối đa ${i + 1}`} type="number" min="0.01" max="1000" step="0.01" required value={r.maxScore} disabled={!editable || structureLocked || busy} onChange={e => update(i, { maxScore: e.target.value })} /></td>
          {cloCodes.map(code => <td key={code} className="clo-cell" style={Number(r.clos[code]) > 0 ? { background: `color-mix(in srgb, var(--domain-assessment) ${Math.round(8 + Math.min(100, Number(r.clos[code])) * 0.22)}%, transparent)` } : undefined}><input aria-label={`${r.code || `Đầu điểm ${i + 1}`} đánh giá ${code} (%)`} type="number" min="0" max="100" step="0.01" placeholder="—" value={r.clos[code] ?? ""} disabled={!editable || busy} onChange={e => update(i, { clos: { ...r.clos, [code]: e.target.value } })} /></td>)}
          {editable && !structureLocked && <td><button type="button" className="button button--danger button--small" disabled={busy} onClick={() => setRows(list => list.filter((_, j) => j !== i))}>Bỏ</button></td>}
        </tr>)}</tbody></table></div>
      {!rows.length && <p className="empty-state">Học phần chưa có cơ cấu điểm. {isAdmin ? "Thêm đầu điểm theo đề cương đã được phê duyệt." : ""}</p>}
      {!cloCodes.length && <p className="academic-notice">Học phần chưa có CLO; khai báo CLO ở trang CLO/PLO trước khi liên kết đầu điểm.</p>}
      {rows.length > 0 && <div className="weight-total" aria-hidden="true"><span>Tổng trọng số: <strong>{(totalUnits / 100).toFixed(2)}%</strong> / 100%</span>
        <span className="weight-total__bar"><span style={{ width: `${Math.min(100, totalUnits / 100)}%` }} className={totalUnits === 10000 ? "is-ok" : totalUnits > 10000 ? "is-over" : ""} /></span></div>}
      <div aria-live="polite">{hints.length > 0 ? <ul className="academic-hints">{hints.map(h => <li key={h}>{h}</li>)}</ul>
        : rows.length > 0 && <ul className="academic-hints academic-hints--ok"><li>✓ Cơ cấu điểm hợp lệ theo quy chế; mọi CLO đều được đánh giá.</li></ul>}</div>
      {editable && <div className="dialog__actions">
        {!structureLocked && <button type="button" className="button button--secondary" disabled={busy || rows.length >= 20} onClick={() => setRows(list => [...list, { code: "", name: "", kind: "process", weight: "", maxScore: "10", clos: {} }])}>Thêm đầu điểm</button>}
        {!!scheme?.components.length && <button type="button" className="button button--ghost" disabled={busy} onClick={() => void template()}>Dùng làm mẫu cho học phần chưa có cơ cấu</button>}
        <button className="button button--primary" disabled={busy || !rows.length}>{busy ? "Đang lưu…" : "Lưu cơ cấu điểm"}</button></div>}
    </form>
  </section>;
}

function OfferingSummary({ offering, versionId }: { offering: CourseOffering; versionId: number }) {
  return <SummaryCard label="Thông tin lớp học phần" items={[
    ["Mã lớp học phần", <strong key="c">{offering.code}</strong>],
    ["Học kỳ · Nhóm", `${offering.academicPeriod} · ${offering.groupName}`],
    ["Lớp sinh hoạt", offering.targetHomeClassCode ?? "Nhiều lớp / không gắn"],
    ["Giảng viên", <LecturerList key="l" offering={offering} />],
    ["Sĩ số", <Capacity key="s" count={offering.studentCount} capacity={offering.capacity} />],
  ]} actions={<Link className="button button--secondary button--small" to={`/curriculum/versions/${versionId}/courses/${offering.programCourseId}/assessment`}>Đề cương đánh giá</Link>} />;
}

function GradebookWorkflow({ offering, isAdmin, userId, busy, onAction }: { offering: CourseOffering; isAdmin: boolean; userId: number; busy: boolean; onAction: (action: "submit" | "publish" | "reopen") => void }) {
  const isMain = offering.lecturers.some(l => l.userId === userId && l.role === "main");
  const s = offering.gradeStatus;
  const detail = <p>{s === "draft" ? "Giảng viên nhập điểm; khi đủ điểm, giảng viên chính bấm Nộp điểm." : s === "submitted" ? "Điểm đã nộp, chờ phòng đào tạo công bố. Giảng viên không sửa được nữa."
    : `Điểm đã công bố${offering.publishedAt ? ` lúc ${new Date(offering.publishedAt).toLocaleString("vi-VN")}` : ""}. Sửa điểm (phúc khảo) cần phòng đào tạo và phải ghi lý do.`}
    {offering.statusNote && s === "draft" ? ` Lý do trả lại gần nhất: ${offering.statusNote}` : ""}</p>;
  return <GradeStatusStepper status={s} detail={detail} actions={<>
    {s !== "draft" && isAdmin && <button className="button button--secondary" disabled={busy} onClick={() => onAction("reopen")}>Trả lại để sửa</button>}
    {s === "draft" && (isAdmin || isMain) && <button className="button button--primary" disabled={busy || offering.readOnly} onClick={() => onAction("submit")}>Nộp điểm</button>}
    {s === "submitted" && isAdmin && <button className="button button--primary" disabled={busy} onClick={() => onAction("publish")}>Công bố điểm</button>}
  </>} />;
}

export function CourseStudentsPage({ isAdmin, userId }: { isAdmin: boolean; userId: number }) {
  const { versionId: v, offeringId: c } = useParams(); const versionId = Number(v), offeringId = Number(c);
  const [offering, setOffering] = useState<CourseOffering | null>(null); const period = offering?.academicPeriod ?? "";
  const [search, setSearch] = useState(""); const [page, setPage] = useState(1); const loadId = useRef(0);
  const [course, setCourse] = useState<ProgramCourse | null>(null); const [scheme, setScheme] = useState<AssessmentScheme | null>(null);
  const [rows, setRows] = useState<PagedResult<Enrollment> | null>(null);
  const [error, setError] = useState(""); const [notice, setNotice] = useState(""); const [loading, setLoading] = useState(true);
  const [dialog, setDialog] = useState<"add" | "import" | "homeClass" | "reopen" | null>(null); const [code, setCode] = useState(""); const [name, setName] = useState(""); const [busy, setBusy] = useState(false);
  const [homeClasses, setHomeClasses] = useState<HomeClass[]>([]); const [homeClass, setHomeClass] = useState(""); const [reason, setReason] = useState("");
  const load = useCallback(async () => {
    const id = ++loadId.current; setError(""); setLoading(true); setRows(null);
    try {
      const o = await offeringApi.offering(versionId, offeringId); if (id !== loadId.current) return; setOffering(o);
      const [course, scheme, roster] = await Promise.all([curriculumApi.getCourse(versionId, o.programCourseId), academicApi.offeringScheme(versionId, offeringId), academicApi.students(versionId, offeringId, page, search)]);
      if (id !== loadId.current) return; setCourse(course); setScheme(scheme); setRows(roster);
    } catch (e) { if (id === loadId.current) setError(message(e)); } finally { if (id === loadId.current) setLoading(false); }
  }, [versionId, offeringId, page, search]);
  useEffect(() => { void load(); return () => { loadId.current++; }; }, [load]);
  useEffect(() => { setDialog(null); setNotice(""); setRows(null); setOffering(null); setCourse(null); setPage(1); setSearch(""); }, [versionId, offeringId]);
  const writable = !!offering && !offering.readOnly && !!course && offering.gradeStatus === "draft";
  async function run(action: () => Promise<string>) { setError(""); setBusy(true); try { const text = await action(); setDialog(null); setNotice(text); await load(); } catch (e) { setError(message(e)); } finally { setBusy(false); } }
  const add = (e: FormEvent) => { e.preventDefault(); void run(async () => { await academicApi.enroll(versionId, offeringId, code, name, period); setCode(""); setName(""); return "Đã thêm sinh viên vào lớp học phần."; }); };
  const enrollClass = (e: FormEvent) => { e.preventDefault(); void run(async () => { const r = await offeringApi.enrollHomeClass(versionId, offeringId, Number(homeClass)); return `Đã thêm ${r.added} sinh viên; bỏ qua ${r.skipped}.${r.messages.length ? " " + r.messages.slice(0, 5).join(" ") : ""}`; }); };
  const status = (action: "submit" | "publish" | "reopen") => {
    if (action === "reopen") { setReason(""); setDialog("reopen"); return; }
    void run(async () => { await academicApi.gradeStatus(versionId, offeringId, action, null); return action === "submit" ? "Đã nộp điểm. Phòng đào tạo sẽ kiểm tra và công bố." : "Đã công bố điểm; kết quả được đưa vào bảng điểm sinh viên."; });
  };
  function openHomeClass() { setError(""); setHomeClass(offering?.targetHomeClassId ? String(offering.targetHomeClassId) : ""); setDialog("homeClass");
    void directoryApi.homeClasses(versionId, 1, "").then(r => setHomeClasses(r.items)).catch(e => setError(message(e))); }
  const offeringsLink = `/curriculum/versions/${versionId}/offerings?${new URLSearchParams({ period })}`;
  return <section className={domainPage("assessment", "academic-page")}>
    <CourseHeading course={course} versionId={versionId} domain="assessment" detail={offering ? `Sổ điểm lớp học phần ${offering.code}` : undefined}
      crumbs={<><Link to={offeringsLink}>Lớp học phần {period}</Link><span>/</span><span>{offering?.code ?? "…"}</span></>} />
    {offering && <OfferingSummary offering={offering} versionId={versionId} />}
    {offering && <GradebookWorkflow offering={offering} isAdmin={isAdmin} userId={userId} busy={busy} onAction={status} />}
    <ErrorMessage value={error} /><Notice value={notice} />
    {period && <><div className="toolbar academic-roster-toolbar"><h2>Sinh viên</h2><input aria-label="Tìm sinh viên" placeholder="Tìm MSSV hoặc họ tên" value={search} onChange={e => { setSearch(e.target.value); setPage(1); }} />
      <button className="button button--secondary" disabled={!writable || loading} onClick={() => { setError(""); setDialog("add"); }}>Thêm sinh viên</button>
      <button className="button button--secondary" disabled={!writable || loading} onClick={() => { setError(""); setDialog("import"); }}>Import sinh viên / điểm</button>
      {isAdmin && <button className="button button--secondary" disabled={!writable || loading} onClick={openHomeClass}>Thêm cả lớp sinh hoạt</button>}</div>
      {!scheme?.components.length && <p className="academic-notice">Học phần chưa có cơ cấu điểm trong đề cương. Vẫn có thể thêm sinh viên; admin cần cấu hình trước khi nhập điểm.</p>}
      {loading ? <p role="status">Đang tải…</p> : !rows?.items.length ? <p className="empty-state">Chưa có sinh viên trong lớp học phần hoặc không có kết quả tìm kiếm.</p> : <><div className="data-table-wrap"><table className="data-table"><thead><tr><th>MSSV</th><th>Họ tên</th><th>Lớp SH</th><th>Lần học</th><th>Đầu điểm</th><th className="align-right">Điểm HP</th><th>Điểm chữ</th><th className="align-right">Hệ 4</th><th>Kết quả</th><th><span className="sr-only">Thao tác</span></th></tr></thead>
        <tbody>{rows.items.map(s => { const grades = `/curriculum/versions/${versionId}/offerings/${offeringId}/students/${s.id}/grades`; return <tr key={s.id}><td>{s.studentCode}</td><td><Link className="table-link" to={grades}>{s.fullName}</Link></td>
          <td>{s.homeClassCode ?? "—"}</td><td><AttemptChip kind={s.attemptKind} /></td><td>{s.specialMark ? <Chip tone="grade-draft" dot={false}>Điểm {s.specialMark}</Chip> : `${s.gradedComponents}/${scheme?.components.length ?? 0}`}</td>
          <td className="align-right">{s.totalScore == null ? <span className="muted-inline">{s.specialMark ? "—" : "Chưa đủ"}</span> : score(s.totalScore)}</td><td><LetterChip letter={s.letterGrade} /></td><td className="align-right">{score(s.gradePoint)}</td>
          <td><ResultChip passed={s.passed} letter={s.letterGrade} provisional={!s.published} /></td>
          <td className="align-right"><Link className="button button--small button--secondary" to={grades}>Xem / nhập điểm</Link></td></tr>; })}</tbody></table></div>
        <Pager page={page} totalPages={rows.totalPages} label={`${rows.totalItems} sinh viên`} onPage={setPage} /></>}
    </>}
    {dialog === "add" && <AcademicDialog title={`Thêm sinh viên · ${offering?.code}`} onClose={busy ? () => {} : () => setDialog(null)}><form className="admin-form" onSubmit={add}><ErrorMessage value={error} />
      <label>Mã sinh viên<input autoFocus required maxLength={30} value={code} disabled={busy} onChange={e => setCode(e.target.value)} /></label><label>Họ và tên<input required minLength={2} maxLength={200} value={name} disabled={busy} onChange={e => setName(e.target.value)} /></label>
      <p>Mã sinh viên được nhận diện trong cùng trường. Sinh viên đã học học phần này ở học kỳ trước được ghi nhận là học lại hoặc học cải thiện.</p><div className="dialog__actions"><button className="button button--primary" disabled={busy}>Lưu sinh viên</button></div></form></AcademicDialog>}
    {dialog === "homeClass" && <AcademicDialog title={`Thêm cả lớp sinh hoạt · ${offering?.code}`} onClose={busy ? () => {} : () => setDialog(null)}><form className="admin-form" onSubmit={enrollClass}><ErrorMessage value={error} />
      <label>Lớp sinh hoạt<select required value={homeClass} disabled={busy} onChange={e => setHomeClass(e.target.value)}><option value="">Chọn lớp</option>{homeClasses.map(h => <option key={h.id} value={h.id}>{h.code} · {h.name} ({h.studentCount} SV)</option>)}</select></label>
      <p>Chỉ thêm sinh viên đang học; sinh viên đã có trong lớp học phần hoặc đang học học phần này ở nhóm khác sẽ được bỏ qua.</p><div className="dialog__actions"><button className="button button--primary" disabled={busy || !homeClass}>Thêm sinh viên</button></div></form></AcademicDialog>}
    {dialog === "reopen" && <AcademicDialog title="Trả lại sổ điểm để sửa" onClose={busy ? () => {} : () => setDialog(null)}><form className="admin-form" onSubmit={e => { e.preventDefault(); void run(async () => { await academicApi.gradeStatus(versionId, offeringId, "reopen", reason); return "Đã trả lại sổ điểm; kết quả công bố trước đó bị thu hồi khỏi bảng điểm cho tới khi công bố lại."; }); }}><ErrorMessage value={error} />
      <label>Lý do<textarea required minLength={5} maxLength={500} rows={3} value={reason} disabled={busy} onChange={e => setReason(e.target.value)} /></label>
      <div className="dialog__actions"><button className="button button--primary" disabled={busy || reason.trim().length < 5}>Trả lại</button></div></form></AcademicDialog>}
    {dialog === "import" && offering && <AcademicImportDialog target={offeringImportTarget(versionId, offeringId, period)} onClose={() => setDialog(null)} onSaved={text => { setDialog(null); setNotice(text); void load(); }} />}
  </section>;
}

export function StudentGradesPage({ isAdmin }: { isAdmin: boolean }) {
  const { versionId: v, offeringId: c, enrollmentId: e } = useParams(); const versionId = Number(v), offeringId = Number(c), enrollmentId = Number(e);
  const [offering, setOffering] = useState<CourseOffering | null>(null); const loadId = useRef(0);
  const [book, setBook] = useState<Gradebook | null>(null); const [course, setCourse] = useState<ProgramCourse | null>(null); const [policy, setPolicy] = useState<GradingPolicy | null>(null);
  const [scores, setScores] = useState<Record<string, string>>({}); const [mark, setMark] = useState(""); const [reason, setReason] = useState("");
  const [error, setError] = useState(""); const [notice, setNotice] = useState(""); const [busy, setBusy] = useState(false);
  const accept = (b: Gradebook) => { setBook(b); setMark(b.enrollment.specialMark ?? ""); setReason(""); setScores(Object.fromEntries(Object.entries(b.scores).map(([k, v]) => [k, v == null ? "" : String(v)]))); };
  const load = useCallback(async () => { const id = ++loadId.current; setError(""); setBook(null); setOffering(null); try { const o = await offeringApi.offering(versionId, offeringId); if (id !== loadId.current) return; setOffering(o); const [b, c, p] = await Promise.all([academicApi.grades(versionId, offeringId, enrollmentId), curriculumApi.getCourse(versionId, o.programCourseId), academicApi.policy(versionId)]); if (id !== loadId.current) return; accept(b); setCourse(c); setPolicy(p); } catch (e) { if (id === loadId.current) setError(message(e)); } }, [versionId, offeringId, enrollmentId]);
  useEffect(() => { void load(); return () => { loadId.current++; }; }, [load]);
  const components = book?.scheme.components ?? [];
  const complete = !!components.length && components.every(c => scores[c.code]?.trim());
  const raw = complete ? components.reduce((sum, c) => sum + Number(scores[c.code]) / c.maxScore * c.weight, 0) / 10 : null;
  const total = raw == null ? null : round(raw, policy?.courseScoreDecimals ?? 1);
  const preview = total == null || !policy ? undefined : band(total, policy.bands);
  const status = offering?.gradeStatus ?? "draft";
  const writable = !!offering && !offering.readOnly && !!course && !course.archivedAt && !!components.length && (status === "draft" || isAdmin);
  async function save(event: FormEvent) {
    event.preventDefault(); if (!book) return; setBusy(true); setError(""); setNotice("");
    try { const values = Object.fromEntries(Object.entries(scores).map(([k, v]) => [k, v.trim() ? Number(v) : null]));
      accept(await academicApi.saveGrades(versionId, offeringId, enrollmentId, book, values, mark || null, reason.trim() || null)); setNotice(status === "published" ? "Đã sửa điểm đã công bố, ghi lý do vào lịch sử và cập nhật bảng điểm." : "Đã lưu điểm và lịch sử thay đổi."); }
    catch (e) { setError(message(e)); } finally { setBusy(false); }
  }
  const studentsLink = `/curriculum/versions/${versionId}/offerings/${offeringId}/students`;
  return <section className={domainPage("assessment", "academic-page")}>
    <CourseHeading course={course} versionId={versionId} domain="assessment" detail={offering ? `Sổ điểm lớp học phần ${offering.code}` : undefined}
      crumbs={<><Link to={studentsLink}>Lớp học phần {offering?.code ?? "…"}</Link><span>/</span><span>{book?.enrollment.studentCode ?? "…"}</span></>}>
      {book && <Link className="button button--secondary" to={studentsLink}>Danh sách sinh viên</Link>}</CourseHeading>
    <ErrorMessage value={error} /><Notice value={notice} />
    {book && <>
      <SummaryCard label="Sinh viên" items={[["Sinh viên", <strong key="n">{book.enrollment.studentCode} · {book.enrollment.fullName}</strong>], ["Lớp sinh hoạt", book.enrollment.homeClassCode],
        ["Lớp học phần", `${offering?.code ?? ""} · ${offering?.groupName ?? ""} · ${book.enrollment.academicPeriod}`], ["Lần học", <AttemptChip key="a" kind={book.enrollment.attemptKind} />],
        ["Sổ điểm", gradeStatusLabels[status]]]} />
      {status !== "draft" && !isAdmin && <p className="academic-notice">Sổ điểm đã nộp/công bố, giảng viên chỉ xem. Đề nghị phòng đào tạo trả lại nếu cần sửa.</p>}
      <form onSubmit={event => void save(event)} className="grade-entry">
        <div className="admin-form">
          <div className="data-table-wrap"><table className="data-table"><thead><tr><th>Đầu điểm</th><th>Loại</th><th className="align-right">Trọng số</th><th>Thang</th><th>CLO đánh giá</th><th>Điểm sinh viên</th></tr></thead>
            <tbody>{components.map(c => <tr key={c.code}><td><strong>{c.code}</strong> · {c.name}</td><td>{kindLabel(c.kind)}</td><td className="align-right">{c.weight}%</td><td>0–{c.maxScore}</td><td>{Object.keys(c.clos ?? {}).join(", ") || "—"}</td>
              <td><input aria-label={`Điểm ${c.code}`} type="number" min="0" max={c.maxScore} step="0.01" value={scores[c.code] ?? ""} placeholder="Chưa nhập" disabled={!writable || busy} onChange={e => setScores(s => ({ ...s, [c.code]: e.target.value }))} /></td></tr>)}</tbody></table></div>
          <div className="academic-form-row"><label>Điểm đặc biệt<select value={mark} disabled={!writable || busy} onChange={e => setMark(e.target.value)}>{specialMarks.map(m => <option key={m.value} value={m.value}>{m.label}</option>)}</select></label>
            {status === "published" && isAdmin && <label>Lý do sửa điểm (bắt buộc)<input required minLength={5} maxLength={500} placeholder="Ví dụ: kết quả phúc khảo ngày…" value={reason} disabled={busy} onChange={e => setReason(e.target.value)} /></label>}</div>
          <p className="muted-inline">Để trống = chưa có điểm / xóa điểm đã lưu. Nhập 0 = điểm 0.</p>
          {!components.length && <p>Cần admin <Link to={`/curriculum/versions/${versionId}/courses/${offering?.programCourseId}/assessment`}>cấu hình cơ cấu điểm trong đề cương</Link> trước.</p>}
        </div>
        <aside className="result-card" aria-live="polite">
          <h2>Kết quả dự kiến</h2>
          {mark ? <p className="result-card__line">Điểm đặc biệt <strong>{mark}</strong> không tính vào điểm trung bình.</p> : <>
            <div className="result-card__score"><strong>{total == null ? "—" : score(total)}</strong><span>/10</span></div>
            <div className="result-card__meta"><LetterChip letter={preview?.letter} /><span>Hệ 4: <strong>{preview ? preview.point4 : "—"}</strong></span>{preview && <ResultChip passed={preview.isPassing} />}</div>
            <p className="result-card__line">Điểm học phần dự kiến: {total == null ? "chưa đủ điểm" : `${score(total)} /10`}{preview ? ` → ${preview.letter} · hệ 4: ${preview.point4} · ${preview.isPassing ? "Đạt" : "Không đạt"}` : ""}.</p></>}
          {book.enrollment.published && <p className="result-card__line">Đã công bố: <LetterChip letter={book.enrollment.letterGrade} /> ({score(book.enrollment.totalScore)}).</p>}
          <button className="button button--primary" disabled={!writable || busy || (status === "published" && reason.trim().length < 5)}>{busy ? "Đang lưu…" : "Lưu điểm"}</button>
          <button type="button" className="button button--ghost" disabled={busy} onClick={() => void load()}>Tải lại dữ liệu</button>
        </aside>
      </form>
      <details className="academic-history"><summary>Lịch sử thay đổi gần đây ({book.history.length})</summary>
        <div className="data-table-wrap"><table className="data-table"><thead><tr><th>Đầu điểm</th><th>Điểm cũ</th><th>Điểm mới</th><th>Người sửa (ID)</th><th>Thời điểm</th><th>Nguồn</th><th>Lý do</th></tr></thead><tbody>{book.history.map((h, i) => <tr key={i}><td>{h.componentCode}</td><td>{h.previousScore ?? "—"}</td><td>{h.newScore ?? "—"}</td><td>{h.actorId}</td><td>{new Date(h.changedAt).toLocaleString("vi-VN")}</td><td>{h.source === "regrade" ? <Chip tone="grade-submitted">Phúc khảo</Chip> : h.source === "import" ? "Import" : "Nhập tay"}</td><td>{h.reason ?? "—"}</td></tr>)}</tbody></table></div>
      </details>
    </>}
  </section>;
}
