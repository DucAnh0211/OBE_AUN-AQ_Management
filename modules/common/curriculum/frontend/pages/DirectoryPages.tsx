import { useCallback, useEffect, useState, type FormEvent } from "react";
import { Link, useParams } from "react-router-dom";
import { directoryApi } from "../api/directory";
import { offeringApi } from "../api/offerings";
import { AcademicDialog } from "../components/AcademicDialog";
import { AcademicImportDialog, homeClassImportTarget } from "../components/AcademicImportDialog";
import type { PagedResult } from "../types";
import { studentStatusLabels, type Account, type HomeClass, type Student, type StudentStatus, type Transcript } from "../types/directory";
import type { OfferingLecturer } from "../types/offerings";
import { DomainEyebrow, domainPage } from "../components/Domain";
import { RowMenu } from "../components/RowMenu";
import { AttemptChip, Chip, LetterChip, ResultChip, StudentStatusChip } from "../components/StatusChip";
import { SummaryCard } from "../components/SummaryCard";
import { UniversityNav, VersionNav } from "../components/VersionNav";
import { ErrorMessage, Notice, Pager, message, score, useVersionContext } from "./academicUi";
import "./academics.css";

function HomeClassDialog({ versionId, value, onClose, onSaved }: { versionId: number; value: HomeClass | null; onClose: () => void; onSaved: (c: HomeClass) => void }) {
  const [code, setCode] = useState(value?.code ?? ""); const [name, setName] = useState(value?.name ?? ""); const [cohort, setCohort] = useState(value?.cohortCode ?? "");
  const [advisor, setAdvisor] = useState(value?.advisorUserId ? String(value.advisorUserId) : ""); const [query, setQuery] = useState(""); const [options, setOptions] = useState<OfferingLecturer[]>([]);
  const [busy, setBusy] = useState(false); const [error, setError] = useState("");
  useEffect(() => { let active = true; const t = setTimeout(() => { void offeringApi.lecturers(versionId, query).then(r => { if (active) setOptions(r); }).catch(() => {}); }, 200); return () => { active = false; clearTimeout(t); }; }, [versionId, query]);
  async function save(e: FormEvent) {
    e.preventDefault(); setBusy(true); setError("");
    try { onSaved(await directoryApi.saveHomeClass(versionId, value?.id ?? null, { code, name, cohortCode: cohort.trim() || null, advisorUserId: advisor ? Number(advisor) : null })); }
    catch (e) { setError(message(e)); } finally { setBusy(false); }
  }
  return <AcademicDialog title={value ? `Cập nhật lớp ${value.code}` : "Thêm lớp sinh hoạt"} onClose={busy ? () => {} : onClose}><form className="admin-form" onSubmit={e => void save(e)}><ErrorMessage value={error} />
    <div className="academic-form-row"><label>Mã lớp<input autoFocus required maxLength={30} placeholder="CNTT01-K66" value={code} disabled={busy} onChange={e => setCode(e.target.value.toUpperCase())} /></label>
      <label>Khóa<input maxLength={20} placeholder="K66" value={cohort} disabled={busy} onChange={e => setCohort(e.target.value.toUpperCase())} /></label></div>
    <label>Tên lớp<input required maxLength={200} value={name} disabled={busy} onChange={e => setName(e.target.value)} /></label>
    <div className="academic-form-row"><label>Tìm cố vấn học tập<input type="search" placeholder="Họ tên hoặc email giảng viên" value={query} disabled={busy} onChange={e => setQuery(e.target.value)} /></label>
      <label>Cố vấn học tập<select value={advisor} disabled={busy} onChange={e => setAdvisor(e.target.value)}><option value="">Chưa phân công</option>{advisor && !options.some(o => String(o.id) === advisor) && <option value={advisor}>{value?.advisorName ?? `ID ${advisor}`}</option>}{options.map(o => <option key={o.id} value={o.id}>{o.fullName} · {o.email}</option>)}</select></label></div>
    <p>Lớp sinh hoạt gắn với khóa tuyển sinh và giữ nguyên qua các học kỳ. Cố vấn học tập xem được danh sách và bảng điểm sinh viên của lớp.</p>
    <div className="dialog__actions"><button className="button button--primary" disabled={busy}>{busy ? "Đang lưu…" : "Lưu lớp"}</button></div></form></AcademicDialog>;
}

/** Lớp sinh hoạt (lớp hành chính) của phiên bản CTĐT áp dụng cho khóa. */
export function HomeClassesPage({ isAdmin }: { isAdmin: boolean }) {
  const versionId = Number(useParams().versionId); const { version } = useVersionContext(versionId);
  const [rows, setRows] = useState<PagedResult<HomeClass> | null>(null); const [page, setPage] = useState(1); const [search, setSearch] = useState("");
  const [error, setError] = useState(""); const [notice, setNotice] = useState(""); const [dialog, setDialog] = useState<HomeClass | "new" | null>(null);
  const load = useCallback(async () => { setError(""); try { setRows(await directoryApi.homeClasses(versionId, page, search)); } catch (e) { setError(message(e)); } }, [versionId, page, search]);
  useEffect(() => { void load(); }, [load]);
  return <section className={domainPage("teaching", "academic-page")}><div className="breadcrumbs"><Link to={`/curriculum/versions/${versionId}`}>Học phần trong CTĐT</Link><span>/</span><span>Lớp sinh hoạt</span></div>
    <div className="admin-heading"><div><DomainEyebrow domain="teaching" detail={version?.versionCode} /><h1>Lớp sinh hoạt</h1><p>Lớp hành chính theo khóa (ví dụ CNTT01-K66). Sinh viên của một lớp có thể được thêm nhanh vào lớp học phần.</p></div>
      {isAdmin && <div className="admin-heading__actions"><button className="button button--primary" onClick={() => setDialog("new")}>Thêm lớp sinh hoạt</button></div>}</div>
    <VersionNav versionId={versionId} staff />
    <ErrorMessage value={error} /><Notice value={notice} />
    <div className="toolbar"><input aria-label="Tìm lớp sinh hoạt" placeholder="Tìm mã lớp, tên lớp hoặc khóa" value={search} onChange={e => { setSearch(e.target.value); setPage(1); }} /></div>
    {!rows ? !error && <p role="status">Đang tải…</p> : !rows.items.length ? <p className="empty-state">Chưa có lớp sinh hoạt.</p> : <><div className="data-table-wrap"><table className="data-table"><thead><tr><th>Mã lớp</th><th>Tên lớp</th><th>Khóa</th><th>Cố vấn học tập</th><th>Sinh viên</th><th>Thao tác</th></tr></thead>
      <tbody>{rows.items.map(c => <tr key={c.id}><td><strong>{c.code}</strong></td><td><Link to={`/curriculum/versions/${versionId}/home-classes/${c.id}`}>{c.name}</Link></td><td>{c.cohortCode ?? "—"}</td><td>{c.advisorName ?? "—"}</td><td>{c.studentCount}</td>
        <td><div className="table-actions"><Link className="button button--small button--secondary" to={`/curriculum/versions/${versionId}/home-classes/${c.id}`}>Danh sách sinh viên</Link>{isAdmin && <button className="button button--small button--ghost" onClick={() => setDialog(c)}>Sửa</button>}</div></td></tr>)}</tbody></table></div>
      <Pager page={page} totalPages={rows.totalPages} label={`${rows.totalItems} lớp`} onPage={setPage} /></>}
    {dialog && <HomeClassDialog versionId={versionId} value={dialog === "new" ? null : dialog} onClose={() => setDialog(null)} onSaved={c => { setDialog(null); setNotice(`Đã lưu lớp ${c.code}.`); void load(); }} />}
  </section>;
}

function StudentDialog({ universityId, student, versionId, onClose, onSaved }: { universityId: number; student: Student; versionId?: number; onClose: () => void; onSaved: (s: Student) => void }) {
  const [fullName, setFullName] = useState(student.fullName); const [cohort, setCohort] = useState(student.cohortCode ?? ""); const [status, setStatus] = useState<StudentStatus>(student.status);
  const [homeClass, setHomeClass] = useState(student.homeClassId ? String(student.homeClassId) : ""); const [classes, setClasses] = useState<HomeClass[]>([]);
  const [account, setAccount] = useState(student.userId ? String(student.userId) : ""); const [query, setQuery] = useState(""); const [accounts, setAccounts] = useState<Account[]>([]);
  const [busy, setBusy] = useState(false); const [error, setError] = useState("");
  const classVersion = student.programVersionId ?? versionId;
  useEffect(() => { if (!classVersion) return; let active = true; void directoryApi.homeClasses(classVersion, 1, "").then(r => { if (active) setClasses(r.items); }).catch(() => {}); return () => { active = false; }; }, [classVersion]);
  useEffect(() => { let active = true; const t = setTimeout(() => { void directoryApi.studentAccounts(query).then(r => { if (active) setAccounts(r); }).catch(() => {}); }, 200); return () => { active = false; clearTimeout(t); }; }, [query]);
  async function save(e: FormEvent) {
    e.preventDefault(); setBusy(true); setError("");
    try { onSaved(await directoryApi.updateStudent(universityId, student.id, { fullName, cohortCode: cohort.trim() || null, status, homeClassId: homeClass ? Number(homeClass) : null, userId: account ? Number(account) : null })); }
    catch (e) { setError(message(e)); } finally { setBusy(false); }
  }
  return <AcademicDialog title={`Hồ sơ sinh viên ${student.studentCode}`} onClose={busy ? () => {} : onClose}><form className="admin-form" onSubmit={e => void save(e)}><ErrorMessage value={error} />
    <label>Họ và tên<input required minLength={2} maxLength={200} value={fullName} disabled={busy} onChange={e => setFullName(e.target.value)} /></label>
    <div className="academic-form-row"><label>Khóa<input maxLength={20} value={cohort} disabled={busy} onChange={e => setCohort(e.target.value.toUpperCase())} /></label>
      <label>Tình trạng học tập<select value={status} disabled={busy} onChange={e => setStatus(e.target.value as StudentStatus)}>{(Object.keys(studentStatusLabels) as StudentStatus[]).map(s => <option key={s} value={s}>{studentStatusLabels[s]}</option>)}</select></label></div>
    <label>Lớp sinh hoạt<select value={homeClass} disabled={busy} onChange={e => setHomeClass(e.target.value)}><option value="">Chưa có lớp</option>{student.homeClassId && !classes.some(c => c.id === student.homeClassId) && <option value={student.homeClassId}>{student.homeClassCode}</option>}{classes.map(c => <option key={c.id} value={c.id}>{c.code} · {c.name}</option>)}</select></label>
    <div className="academic-form-row"><label>Tìm tài khoản sinh viên<input type="search" placeholder="Họ tên hoặc email" value={query} disabled={busy} onChange={e => setQuery(e.target.value)} /></label>
      <label>Tài khoản đăng nhập<select value={account} disabled={busy} onChange={e => setAccount(e.target.value)}><option value="">Chưa liên kết</option>{account && !accounts.some(a => String(a.id) === account) && <option value={account}>ID {account}</option>}{accounts.map(a => <option key={a.id} value={a.id}>{a.fullName} · {a.email}</option>)}</select></label></div>
    <p>Sinh viên không ở trạng thái “Đang học” sẽ không được thêm vào lớp học phần mới. Liên kết tài khoản để sinh viên tự xem bảng điểm đã công bố.</p>
    <div className="dialog__actions"><button className="button button--primary" disabled={busy}>{busy ? "Đang lưu…" : "Lưu hồ sơ"}</button></div></form></AcademicDialog>;
}

function StudentTable({ rows, universityId, isAdmin, onEdit }: { rows: Student[]; universityId: number; isAdmin: boolean; onEdit: (s: Student) => void }) {
  return <div className="data-table-wrap"><table className="data-table"><thead><tr><th>MSSV</th><th>Họ tên</th><th>Lớp sinh hoạt</th><th>Khóa</th><th>Tình trạng</th><th>Tài khoản</th><th><span className="sr-only">Thao tác</span></th></tr></thead>
    <tbody>{rows.map(s => { const transcript = `/curriculum/universities/${universityId}/students/${s.id}/transcript`; return <tr key={s.id}><td>{s.studentCode}</td><td><Link className="table-link" to={transcript}>{s.fullName}</Link></td>
      <td>{s.homeClassCode ?? "—"}</td><td>{s.cohortCode ?? "—"}</td><td><StudentStatusChip status={s.status} /></td><td>{s.userId ? "Đã liên kết" : <span className="muted-inline">—</span>}</td>
      <td><div className="row-actions"><Link className="button button--small button--secondary" to={transcript}>Bảng điểm</Link>
        {isAdmin && <RowMenu label={`Thao tác khác cho ${s.studentCode}`} items={[{ label: "Sửa hồ sơ sinh viên", onSelect: () => onEdit(s) }]} />}</div></td></tr>; })}</tbody></table></div>;
}

export function HomeClassPage({ isAdmin }: { isAdmin: boolean }) {
  const { versionId: v, homeClassId: h } = useParams(); const versionId = Number(v), homeClassId = Number(h);
  const { universityId } = useVersionContext(versionId);
  const [cls, setCls] = useState<HomeClass | null>(null); const [rows, setRows] = useState<PagedResult<Student> | null>(null); const [page, setPage] = useState(1); const [search, setSearch] = useState("");
  const [error, setError] = useState(""); const [notice, setNotice] = useState(""); const [dialog, setDialog] = useState<"add" | "import" | Student | null>(null);
  const [code, setCode] = useState(""); const [name, setName] = useState(""); const [busy, setBusy] = useState(false);
  const load = useCallback(async () => { if (!universityId) return; setError(""); try { const [c, r] = await Promise.all([directoryApi.homeClass(versionId, homeClassId), directoryApi.students(universityId, page, search, homeClassId, versionId)]); setCls(c); setRows(r); } catch (e) { setError(message(e)); } }, [universityId, versionId, homeClassId, page, search]);
  useEffect(() => { void load(); }, [load]);
  async function add(e: FormEvent) { e.preventDefault(); setBusy(true); setError(""); try { await directoryApi.addToHomeClass(versionId, homeClassId, code, name); setCode(""); setName(""); setDialog(null); setNotice("Đã thêm sinh viên vào lớp."); await load(); } catch (e) { setError(message(e)); } finally { setBusy(false); } }
  return <section className={domainPage("teaching", "academic-page")}><div className="breadcrumbs"><Link to={`/curriculum/versions/${versionId}/home-classes`}>Lớp sinh hoạt</Link><span>/</span><span>{cls?.code ?? "…"}</span></div>
    <div className="admin-heading"><div><DomainEyebrow domain="teaching" detail="Lớp sinh hoạt" /><h1>{cls?.name ?? "Lớp sinh hoạt"}</h1></div>
      {isAdmin && <div className="admin-heading__actions"><button className="button button--secondary" onClick={() => { setError(""); setDialog("import"); }}>Import danh sách</button><button className="button button--primary" onClick={() => { setError(""); setDialog("add"); }}>Thêm sinh viên</button></div>}</div>
    {cls && <SummaryCard label="Thông tin lớp sinh hoạt" items={[["Mã lớp", <strong key="c">{cls.code}</strong>], ["Khóa", cls.cohortCode], ["Cố vấn học tập", cls.advisorName ?? "Chưa phân công"], ["Sĩ số", cls.studentCount]]} />}
    <ErrorMessage value={error} /><Notice value={notice} />
    <div className="toolbar"><input aria-label="Tìm sinh viên" placeholder="Tìm MSSV hoặc họ tên" value={search} onChange={e => { setSearch(e.target.value); setPage(1); }} /></div>
    {!rows || !universityId ? !error && <p role="status">Đang tải…</p> : !rows.items.length ? <p className="empty-state">Lớp chưa có sinh viên.</p> : <><StudentTable rows={rows.items} universityId={universityId} isAdmin={isAdmin} onEdit={s => setDialog(s)} />
      <Pager page={page} totalPages={rows.totalPages} label={`${rows.totalItems} sinh viên`} onPage={setPage} /></>}
    {dialog === "add" && <AcademicDialog title={`Thêm sinh viên vào lớp ${cls?.code}`} onClose={busy ? () => {} : () => setDialog(null)}><form className="admin-form" onSubmit={e => void add(e)}><ErrorMessage value={error} />
      <label>Mã sinh viên<input autoFocus required maxLength={30} value={code} disabled={busy} onChange={e => setCode(e.target.value)} /></label><label>Họ và tên<input required minLength={2} maxLength={200} value={name} disabled={busy} onChange={e => setName(e.target.value)} /></label>
      <div className="dialog__actions"><button className="button button--primary" disabled={busy}>Thêm</button></div></form></AcademicDialog>}
    {dialog === "import" && cls && <AcademicImportDialog target={homeClassImportTarget(versionId, homeClassId, cls.code)} onClose={() => setDialog(null)} onSaved={text => { setDialog(null); setNotice(text); void load(); }} />}
    {dialog && typeof dialog === "object" && universityId && <StudentDialog universityId={universityId} student={dialog} versionId={versionId} onClose={() => setDialog(null)} onSaved={() => { setDialog(null); setNotice("Đã lưu hồ sơ sinh viên."); void load(); }} />}
  </section>;
}

/** Danh bạ sinh viên toàn trường (phòng đào tạo). */
export function StudentDirectoryPage() {
  const universityId = Number(useParams().universityId);
  const [rows, setRows] = useState<PagedResult<Student> | null>(null); const [page, setPage] = useState(1); const [search, setSearch] = useState("");
  const [error, setError] = useState(""); const [notice, setNotice] = useState(""); const [editing, setEditing] = useState<Student | null>(null);
  const load = useCallback(async () => { setError(""); try { setRows(await directoryApi.students(universityId, page, search)); } catch (e) { setError(message(e)); } }, [universityId, page, search]);
  useEffect(() => { void load(); }, [load]);
  return <section className={domainPage("teaching", "academic-page")}><div className="breadcrumbs"><Link to="/curriculum">Trường đại học</Link><span>/</span><Link to={`/curriculum/universities/${universityId}`}>Trường</Link><span>/</span><span>Sinh viên</span></div>
    <div className="admin-heading"><div><DomainEyebrow domain="teaching" detail="Hồ sơ người học" /><h1>Sinh viên</h1><p>MSSV là duy nhất trong trường. Hồ sơ gồm khóa, lớp sinh hoạt, tình trạng học tập và tài khoản đăng nhập.</p></div></div>
    <UniversityNav universityId={universityId} isAdmin />
    <ErrorMessage value={error} /><Notice value={notice} />
    <div className="toolbar"><input aria-label="Tìm sinh viên" placeholder="Tìm MSSV hoặc họ tên" value={search} onChange={e => { setSearch(e.target.value); setPage(1); }} /></div>
    {!rows ? !error && <p role="status">Đang tải…</p> : !rows.items.length ? <p className="empty-state">Chưa có sinh viên.</p> : <><StudentTable rows={rows.items} universityId={universityId} isAdmin onEdit={setEditing} />
      <Pager page={page} totalPages={rows.totalPages} label={`${rows.totalItems} sinh viên`} onPage={setPage} /></>}
    {editing && <StudentDialog universityId={universityId} student={editing} onClose={() => setEditing(null)} onSaved={() => { setEditing(null); setNotice("Đã lưu hồ sơ sinh viên."); void load(); }} />}
  </section>;
}

const classificationTone = (label: string | null) => label === "Xuất sắc" || label === "Giỏi" ? "pass" : label === "Khá" ? "attempt-improve" : label === "Trung bình" ? "grade-submitted" : label ? "fail" : "grade-draft";
function TranscriptView({ data }: { data: Transcript }) {
  const s = data.student;
  return <>
    <SummaryCard label="Sinh viên" items={[["Sinh viên", <strong key="s">{s.studentCode} · {s.fullName}</strong>], ["Lớp · Khóa", `${s.homeClassCode ?? "—"} · ${s.cohortCode ?? "—"}`], ["Tình trạng", <StudentStatusChip key="t" status={s.status} />]]} />
    <div className="transcript-summary">
      <div className="is-key"><span>GPA tích lũy (hệ 4)</span><strong>{score(data.cumulative.gpa)}</strong></div>
      <div><span>Tín chỉ tích lũy</span><strong>{data.cumulative.earnedCredits}</strong></div>
      <div><span>Tín chỉ tính GPA</span><strong>{data.cumulative.gpaCredits}</strong></div>
      <div><span>Xếp loại</span><strong>{data.classification ? <Chip tone={classificationTone(data.classification)}>{data.classification}</Chip> : "—"}</strong></div>
    </div>
    <p className="muted-inline">Chỉ gồm điểm đã công bố. Học lại/cải thiện: điểm tích lũy lấy {data.retakePolicy === "highest" ? "lần có điểm cao nhất" : "lần học gần nhất"}; học phần điều kiện (GDTC, GDQP-AN) không tính vào GPA.</p>
    {!data.semesters.length && <p className="empty-state">Chưa có điểm học phần đã công bố.</p>}
    {data.semesters.map(sem => <section key={sem.period} className="transcript-semester" aria-label={`Học kỳ ${sem.period}`}>
      <header><h2>{sem.period}</h2><span>GPA học kỳ <strong>{score(sem.summary.gpa)}</strong> · {sem.summary.gpaCredits} TC tính GPA · {sem.summary.earnedCredits} TC đạt</span></header>
      <div className="data-table-wrap"><table className="data-table"><thead><tr><th>Mã HP</th><th>Học phần</th><th className="align-right">TC</th><th>Lớp HP</th><th>Lần học</th><th className="align-right">Điểm HP</th><th>Điểm chữ</th><th className="align-right">Hệ 4</th><th>Kết quả</th></tr></thead>
        <tbody>{sem.rows.map(r => <tr key={r.offeringCode} className={r.countedInCumulative ? "" : "transcript-row--superseded"}><td>{r.courseCode}</td>
          <td>{r.courseName}{!r.countsTowardGpa && <> <Chip tone="grade-draft" dot={false}>Không tính GPA</Chip></>}{!r.countedInCumulative && <> <Chip tone="grade-draft" dot={false} title="Lần học khác của học phần này được dùng cho điểm tích lũy">Không tính tích lũy</Chip></>}</td>
          <td className="align-right">{r.credits}</td><td>{r.offeringCode}</td><td><AttemptChip kind={r.attemptKind} /></td><td className="align-right">{score(r.finalScore)}</td><td><LetterChip letter={r.letterGrade} /></td><td className="align-right">{score(r.gradePoint)}</td>
          <td><ResultChip passed={r.passed} letter={r.letterGrade} /></td></tr>)}</tbody></table></div>
    </section>)}
  </>;
}

export function TranscriptPage() {
  const { universityId: u, studentId: s } = useParams(); const universityId = Number(u), studentId = Number(s);
  const [data, setData] = useState<Transcript | null>(null); const [error, setError] = useState("");
  useEffect(() => { let active = true; void directoryApi.transcript(universityId, studentId).then(r => { if (active) setData(r); }).catch(e => { if (active) setError(message(e)); }); return () => { active = false; }; }, [universityId, studentId]);
  return <section className={domainPage("assessment", "academic-page")}><div className="breadcrumbs"><Link to={`/curriculum/universities/${universityId}/students`}>Sinh viên</Link><span>/</span><span>Bảng điểm</span></div>
    <div className="admin-heading"><div><DomainEyebrow domain="assessment" detail="Kết quả học tập" /><h1>Bảng điểm sinh viên</h1></div></div><ErrorMessage value={error} />{data ? <TranscriptView data={data} /> : !error && <p role="status">Đang tải…</p>}</section>;
}

export function MyTranscriptPage() {
  const [data, setData] = useState<Transcript | null>(null); const [error, setError] = useState("");
  useEffect(() => { let active = true; void directoryApi.myTranscript().then(r => { if (active) setData(r); }).catch(e => { if (active) setError(message(e)); }); return () => { active = false; }; }, []);
  return <section className={domainPage("assessment", "academic-page")}><div className="admin-heading"><div><DomainEyebrow domain="assessment" detail="Kết quả học tập" /><h1>Bảng điểm của tôi</h1></div></div><ErrorMessage value={error} />{data ? <TranscriptView data={data} /> : !error && <p role="status">Đang tải…</p>}</section>;
}
