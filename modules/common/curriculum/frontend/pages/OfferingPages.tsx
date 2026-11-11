import { useCallback, useEffect, useState, type FormEvent } from "react";
import { Link, useParams, useSearchParams } from "react-router-dom";
import { directoryApi } from "../api/directory";
import { offeringApi } from "../api/offerings";
import { AcademicDialog } from "../components/AcademicDialog";
import type { PagedResult, ProgramCourse } from "../types";
import type { AcademicPeriodBatch } from "../types/academics";
import type { HomeClass } from "../types/directory";
import { lecturerRoleLabels, type CourseOffering, type LecturerRole, type OfferingLecturer } from "../types/offerings";
import { DomainEyebrow, domainPage } from "../components/Domain";
import { RowMenu } from "../components/RowMenu";
import { GradeStatusChip, RoleChip } from "../components/StatusChip";
import { Capacity } from "../components/SummaryCard";
import { VersionNav } from "../components/VersionNav";
import { ErrorMessage, Notice, Pager, PeriodPicker, message, useVersionContext } from "./academicUi";
import "./academics.css";

type Assignment = { userId: number; name: string; role: LecturerRole };
const fromOffering = (o: CourseOffering): Assignment[] => o.lecturers.map(l => ({ userId: l.userId, name: l.fullName ?? `ID ${l.userId}`, role: l.role }));

/** Phân công nhiều giảng viên; đúng một giảng viên chính khi đã phân công. */
function LecturerEditor({ versionId, value, onChange, disabled }: { versionId: number; value: Assignment[]; onChange: (v: Assignment[]) => void; disabled: boolean }) {
  const [query, setQuery] = useState(""); const [options, setOptions] = useState<OfferingLecturer[]>([]); const [pick, setPick] = useState("");
  useEffect(() => { let active = true; const t = setTimeout(() => { void offeringApi.lecturers(versionId, query).then(r => { if (active) setOptions(r); }).catch(() => {}); }, 200); return () => { active = false; clearTimeout(t); }; }, [query, versionId]);
  function add() {
    const person = options.find(o => String(o.id) === pick); if (!person || value.some(v => v.userId === person.id)) return;
    onChange([...value, { userId: person.id, name: person.fullName, role: value.some(v => v.role === "main") ? "assistant" : "main" }]); setPick("");
  }
  const mains = value.filter(v => v.role === "main").length;
  return <fieldset className="academic-fieldset"><legend>Giảng viên giảng dạy</legend>
    {value.length > 0 && <table className="data-table"><thead><tr><th>Giảng viên</th><th>Vai trò</th><th></th></tr></thead><tbody>{value.map((v, i) => <tr key={v.userId}><td>{v.name}</td>
      <td><select aria-label={`Vai trò của ${v.name}`} value={v.role} disabled={disabled} onChange={e => onChange(value.map((x, j) => j === i ? { ...x, role: e.target.value as LecturerRole } : x))}>{(Object.keys(lecturerRoleLabels) as LecturerRole[]).map(r => <option key={r} value={r}>{lecturerRoleLabels[r]}</option>)}</select></td>
      <td><button type="button" className="button button--ghost button--small" disabled={disabled} onClick={() => onChange(value.filter((_, j) => j !== i))}>Bỏ</button></td></tr>)}</tbody></table>}
    {value.length > 0 && mains !== 1 && <p className="alert alert--error">Cần đúng một giảng viên chính.</p>}
    <div className="toolbar"><label>Tìm giảng viên<input type="search" placeholder="Họ tên hoặc email" value={query} disabled={disabled} onChange={e => setQuery(e.target.value)} /></label>
      <label>Chọn<select aria-label="Chọn giảng viên" value={pick} disabled={disabled} onChange={e => setPick(e.target.value)}><option value="">—</option>{options.map(o => <option key={o.id} value={o.id} disabled={value.some(v => v.userId === o.id)}>{o.fullName} · {o.email}</option>)}</select></label>
      <button type="button" className="button button--secondary" disabled={disabled || !pick} onClick={add}>Thêm giảng viên</button></div>
    <p>Giảng viên được phân công xem và nhập điểm lớp học phần; chỉ giảng viên chính được nộp điểm.</p>
  </fieldset>;
}

function HomeClassSelect({ versionId, value, onChange, disabled, label = "Lớp sinh hoạt (nếu mở riêng cho một lớp)" }: { versionId: number; value: string; onChange: (v: string) => void; disabled: boolean; label?: string }) {
  const [classes, setClasses] = useState<HomeClass[]>([]);
  useEffect(() => { let active = true; void directoryApi.homeClasses(versionId, 1, "").then(r => { if (active) setClasses(r.items); }).catch(() => {}); return () => { active = false; }; }, [versionId]);
  return <label>{label}<select value={value} disabled={disabled} onChange={e => onChange(e.target.value)}><option value="">Không gắn lớp sinh hoạt</option>{classes.map(c => <option key={c.id} value={c.id}>{c.code} · {c.name}{c.cohortCode ? ` (${c.cohortCode})` : ""}</option>)}</select></label>;
}

function OpenOfferingDialog({ versionId, period, onClose, onSaved }: { versionId: number; period: string; onClose: () => void; onSaved: (o: CourseOffering) => void }) {
  const [query, setQuery] = useState(""); const [page, setPage] = useState(1); const [subjects, setSubjects] = useState<PagedResult<ProgramCourse> | null>(null); const [subject, setSubject] = useState("");
  const [group, setGroup] = useState(""); const [capacity, setCapacity] = useState(""); const [target, setTarget] = useState(""); const [lecturers, setLecturers] = useState<Assignment[]>([]);
  const [busy, setBusy] = useState(false); const [error, setError] = useState("");
  useEffect(() => { let active = true; setSubjects(null); const t = setTimeout(() => { void offeringApi.subjects(versionId, page, query).then(r => { if (active) setSubjects(r); }).catch(e => { if (active) setError(message(e)); }); }, 250); return () => { active = false; clearTimeout(t); }; }, [versionId, page, query]);
  async function save(e: FormEvent) {
    e.preventDefault(); setBusy(true); setError("");
    try { onSaved(await offeringApi.open(versionId, { programCourseId: Number(subject), academicPeriod: period, groupName: group.trim() || null, capacity: capacity ? Number(capacity) : null,
      targetHomeClassId: target ? Number(target) : null, lecturers: lecturers.map(l => ({ userId: l.userId, role: l.role })) })); }
    catch (e) { setError(message(e)); } finally { setBusy(false); }
  }
  return <AcademicDialog title={`Mở lớp học phần · ${period}`} onClose={busy ? () => {} : onClose}><form className="admin-form" onSubmit={e => void save(e)}><ErrorMessage value={error} />
    <label>Tìm học phần<input autoFocus type="search" placeholder="Mã hoặc tên học phần trong CTĐT" value={query} disabled={busy} onChange={e => { setQuery(e.target.value); setSubject(""); setPage(1); }} /></label>
    <label>Học phần trong CTĐT<select required value={subject} disabled={busy || !subjects} onChange={e => setSubject(e.target.value)}><option value="">{subjects ? "Chọn học phần" : "Đang tìm…"}</option>{subjects?.items.map(s => <option key={s.id} value={s.id}>{s.institutionalCode} · {s.name} · {s.credits} TC</option>)}</select></label>
    {subjects && <Pager page={page} totalPages={subjects.totalPages} label={`${subjects.totalItems} học phần`} onPage={p => { setPage(p); setSubject(""); }} />}
    <div className="academic-form-row"><label>Nhóm lớp<input placeholder="Tự đặt N01, N02…" maxLength={20} value={group} disabled={busy} onChange={e => setGroup(e.target.value.toUpperCase())} /></label>
      <label>Sĩ số tối đa<input type="number" min={1} max={1000} placeholder="Không giới hạn" value={capacity} disabled={busy} onChange={e => setCapacity(e.target.value)} /></label></div>
    <HomeClassSelect versionId={versionId} value={target} onChange={setTarget} disabled={busy} />
    <LecturerEditor versionId={versionId} value={lecturers} onChange={setLecturers} disabled={busy} />
    <p>Mã lớp học phần 6 chữ số được hệ thống cấp khi lưu. CLO/PLO lấy từ học phần trong CTĐT, không phải ánh xạ lại.</p>
    <div className="dialog__actions"><button className="button button--primary" disabled={busy || !subject}>{busy ? "Đang lưu…" : "Mở lớp học phần"}</button></div>
  </form></AcademicDialog>;
}

export function EditOfferingDialog({ versionId, offering, mode, onClose, onSaved }: { versionId: number; offering: CourseOffering; mode: "info" | "lecturers"; onClose: () => void; onSaved: (o: CourseOffering) => void }) {
  const [group, setGroup] = useState(offering.groupName); const [capacity, setCapacity] = useState(offering.capacity ? String(offering.capacity) : "");
  const [target, setTarget] = useState(offering.targetHomeClassId ? String(offering.targetHomeClassId) : ""); const [lecturers, setLecturers] = useState<Assignment[]>(fromOffering(offering));
  const [busy, setBusy] = useState(false); const [error, setError] = useState("");
  async function save(e: FormEvent) {
    e.preventDefault(); setBusy(true); setError("");
    try { onSaved(mode === "info" ? await offeringApi.update(versionId, offering.id, group, capacity ? Number(capacity) : null, target ? Number(target) : null)
      : await offeringApi.saveLecturers(versionId, offering.id, lecturers.map(l => ({ userId: l.userId, role: l.role })))); }
    catch (e) { setError(message(e)); } finally { setBusy(false); }
  }
  return <AcademicDialog title={`${mode === "info" ? "Cập nhật" : "Phân công giảng viên"} · ${offering.code}`} onClose={busy ? () => {} : onClose}><form className="admin-form" onSubmit={e => void save(e)}><ErrorMessage value={error} />
    {mode === "info" ? <><div className="academic-form-row"><label>Nhóm lớp<input required maxLength={20} value={group} disabled={busy} onChange={e => setGroup(e.target.value.toUpperCase())} /></label>
      <label>Sĩ số tối đa<input type="number" min={1} max={1000} placeholder="Không giới hạn" value={capacity} disabled={busy} onChange={e => setCapacity(e.target.value)} /></label></div>
      <HomeClassSelect versionId={versionId} value={target} onChange={setTarget} disabled={busy} /></>
      : <LecturerEditor versionId={versionId} value={lecturers} onChange={setLecturers} disabled={busy} />}
    <div className="dialog__actions"><button className="button button--primary" disabled={busy}>{busy ? "Đang lưu…" : "Lưu"}</button></div>
  </form></AcademicDialog>;
}

/** Giảng viên của lớp học phần: tên + vai trò (chính/trợ giảng/TH). */
export function LecturerList({ offering }: { offering: CourseOffering }) {
  if (!offering.lecturers.length) return <span className="muted-inline">Chưa phân công</span>;
  return <span className="lecturer-list">{offering.lecturers.map(l => <span key={l.userId}>{l.fullName ?? `ID ${l.userId}`}<RoleChip role={l.role} /></span>)}</span>;
}

/** Bảng lớp học phần: một hành động chính mỗi dòng, thao tác phụ trong menu ⋯. */
export function OfferingRows({ versionId, rows, isAdmin, onEdit, showPeriod = false }: { versionId: number; rows: CourseOffering[]; isAdmin: boolean; onEdit?: (o: CourseOffering, mode: "info" | "lecturers") => void; showPeriod?: boolean }) {
  return <div className="data-table-wrap"><table className="data-table"><thead><tr><th>Mã lớp HP</th><th>Học phần</th><th>{showPeriod ? "Học kỳ · Nhóm" : "Nhóm"}</th><th>Lớp sinh hoạt</th><th>Giảng viên</th><th>Sĩ số</th><th>Sổ điểm</th><th><span className="sr-only">Thao tác</span></th></tr></thead>
    <tbody>{rows.map(o => { const students = `/curriculum/versions/${versionId}/offerings/${o.id}/students`; return <tr key={o.id}>
      <td><Link className="table-link" to={students}><strong>{o.code}</strong></Link></td>
      <td><Link className="table-link" to={students}>{o.subjectName}</Link><br /><small className="muted-inline">{o.subjectCode} · {o.credits} TC</small></td>
      <td>{showPeriod ? <>{o.academicPeriod}<br /><small className="muted-inline">{o.groupName}</small></> : o.groupName}</td><td>{o.targetHomeClassCode ?? "—"}</td>
      <td><LecturerList offering={o} /></td><td><Capacity count={o.studentCount} capacity={o.capacity} /></td><td><GradeStatusChip status={o.gradeStatus} /></td>
      <td><div className="row-actions"><Link className="button button--secondary button--small" to={students}>Sinh viên / điểm</Link>
        <RowMenu label={`Thao tác khác cho lớp ${o.code}`} items={[
          { label: "Đề cương đánh giá của học phần", to: `/curriculum/versions/${versionId}/courses/${o.programCourseId}/assessment` },
          ...(isAdmin && onEdit ? [{ label: "Sửa nhóm, sĩ số, lớp sinh hoạt", onSelect: () => onEdit(o, "info"), disabled: o.readOnly },
            { label: "Phân công giảng viên", onSelect: () => onEdit(o, "lecturers"), disabled: o.readOnly }] : []),
        ]} /></div></td></tr>; })}</tbody></table></div>;
}

/** Lớp học phần theo học kỳ: mở trực tiếp từ học phần × học kỳ, không phụ thuộc lớp sinh hoạt. */
export function OfferingsByPeriodPage({ isAdmin }: { isAdmin: boolean }) {
  const versionId = Number(useParams().versionId); const { version } = useVersionContext(versionId);
  const [params, setParams] = useSearchParams(); const period = params.get("period") ?? "";
  const [batches, setBatches] = useState<AcademicPeriodBatch[]>([]); const [rows, setRows] = useState<PagedResult<CourseOffering> | null>(null);
  const [page, setPage] = useState(1); const [search, setSearch] = useState(""); const [error, setError] = useState(""); const [notice, setNotice] = useState("");
  const [dialog, setDialog] = useState<"open" | { offering: CourseOffering; mode: "info" | "lecturers" } | null>(null);
  useEffect(() => { let active = true; void offeringApi.periods(versionId).then(r => { if (active) setBatches(r); }).catch(e => { if (active) setError(message(e)); }); return () => { active = false; }; }, [versionId]);
  const load = useCallback(async () => { if (!period) return; setRows(null); setError(""); try { setRows(await offeringApi.offerings(versionId, period, page, search)); } catch (e) { setError(message(e)); } }, [versionId, period, page, search]);
  useEffect(() => { void load(); }, [load]);
  return <section className={domainPage("teaching", "academic-page")}><div className="breadcrumbs"><Link to={`/curriculum/versions/${versionId}`}>Học phần trong CTĐT</Link><span>/</span><span>Lớp học phần</span></div>
    <div className="admin-heading"><div><DomainEyebrow domain="teaching" detail={version?.versionCode} /><h1>Lớp học phần theo học kỳ</h1><p>Mỗi lớp học phần là một học phần mở trong một học kỳ, có mã 6 số, nhóm lớp, sĩ số và giảng viên riêng. Sinh viên từ nhiều lớp sinh hoạt có thể học chung một lớp học phần.</p></div></div>
    <VersionNav versionId={versionId} staff />
    <PeriodPicker batches={batches} period={period} submitLabel="Xem lớp học phần" onChange={p => { setParams({ period: p }); setPage(1); setSearch(""); }} />
    <ErrorMessage value={error} /><Notice value={notice} />
    {period && <><div className="toolbar academic-roster-toolbar"><h2>Lớp học phần · {period}</h2><input aria-label="Tìm lớp học phần" placeholder="Mã lớp HP, nhóm, mã hoặc tên học phần" value={search} onChange={e => { setSearch(e.target.value); setPage(1); }} />
      {isAdmin && <button className="button button--primary" onClick={() => { setNotice(""); setDialog("open"); }}>Mở lớp học phần</button>}</div>
      {!rows ? <p role="status">{error ? "Không tải được danh sách." : "Đang tải…"}</p> : !rows.items.length ? <p className="empty-state">Chưa có lớp học phần trong học kỳ này. {isAdmin ? "Bấm Mở lớp học phần để bắt đầu." : "Bạn chỉ thấy lớp học phần được phân công."}</p>
        : <><OfferingRows versionId={versionId} rows={rows.items} isAdmin={isAdmin} onEdit={(offering, mode) => setDialog({ offering, mode })} /><Pager page={page} totalPages={rows.totalPages} label={`${rows.totalItems} lớp học phần`} onPage={setPage} /></>}
    </>}
    {dialog === "open" && <OpenOfferingDialog versionId={versionId} period={period} onClose={() => setDialog(null)} onSaved={o => { setDialog(null); setNotice(`Đã mở lớp học phần ${o.code} (${o.subjectCode} · ${o.groupName}).`); void load(); }} />}
    {dialog && dialog !== "open" && <EditOfferingDialog versionId={versionId} offering={dialog.offering} mode={dialog.mode} onClose={() => setDialog(null)} onSaved={() => { setDialog(null); setNotice("Đã lưu lớp học phần."); void load(); }} />}
  </section>;
}
