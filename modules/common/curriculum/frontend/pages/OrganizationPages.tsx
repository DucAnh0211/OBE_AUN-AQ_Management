import { useCallback, useEffect, useState, type FormEvent } from "react";
import { Link, useParams } from "react-router-dom";
import { curriculumApi } from "../api";
import { directoryApi } from "../api/directory";
import { AcademicDialog } from "../components/AcademicDialog";
import type { CurriculumProgram, University } from "../types";
import type { GradeBand, GradingPolicy } from "../types/academics";
import type { LecturerProfile, OrgUnit } from "../types/directory";
import { DomainEyebrow, domainPage } from "../components/Domain";
import { UniversityNav } from "../components/VersionNav";
import { ErrorMessage, Notice, message } from "./academicUi";
import "./academics.css";

function useUniversity(universityId: number) {
  const [university, setUniversity] = useState<University | null>(null);
  useEffect(() => { let active = true; void curriculumApi.getUniversity(universityId).then(u => { if (active) setUniversity(u); }).catch(() => {}); return () => { active = false; }; }, [universityId]);
  return university;
}
function UniversityCrumbs({ universityId, university, label }: { universityId: number; university: University | null; label: string }) {
  return <div className="breadcrumbs"><Link to="/curriculum">Trường đại học</Link><span>/</span><Link to={`/curriculum/universities/${universityId}`}>{university?.name ?? "…"}</Link><span>/</span><span>{label}</span></div>;
}

function OrgUnitDialog({ universityId, value, faculties, onClose, onSaved }: { universityId: number; value: OrgUnit | { kind: "faculty" | "department"; parentId: number | null }; faculties: OrgUnit[]; onClose: () => void; onSaved: () => void }) {
  const existing = "id" in value ? value : null;
  const [kind, setKind] = useState(value.kind); const [code, setCode] = useState(existing?.code ?? ""); const [name, setName] = useState(existing?.name ?? "");
  const [parent, setParent] = useState(value.parentId ? String(value.parentId) : ""); const [busy, setBusy] = useState(false); const [error, setError] = useState("");
  async function save(e: FormEvent) {
    e.preventDefault(); setBusy(true); setError("");
    try { await directoryApi.saveOrgUnit(universityId, existing?.id ?? null, { kind, code, name, parentId: kind === "department" && parent ? Number(parent) : null }); onSaved(); }
    catch (e) { setError(message(e)); } finally { setBusy(false); }
  }
  return <AcademicDialog title={existing ? `Cập nhật ${existing.code}` : kind === "faculty" ? "Thêm khoa" : "Thêm bộ môn"} onClose={busy ? () => {} : onClose}><form className="admin-form" onSubmit={e => void save(e)}><ErrorMessage value={error} />
    <label>Loại đơn vị<select value={kind} disabled={busy || !!existing} onChange={e => setKind(e.target.value as "faculty" | "department")}><option value="faculty">Khoa / Viện</option><option value="department">Bộ môn</option></select></label>
    {kind === "department" && <label>Thuộc khoa<select required value={parent} disabled={busy} onChange={e => setParent(e.target.value)}><option value="">Chọn khoa</option>{faculties.map(f => <option key={f.id} value={f.id}>{f.code} · {f.name}</option>)}</select></label>}
    <div className="academic-form-row"><label>Mã<input autoFocus required maxLength={30} value={code} disabled={busy} onChange={e => setCode(e.target.value.toUpperCase())} /></label>
      <label>Tên<input required minLength={2} maxLength={200} value={name} disabled={busy} onChange={e => setName(e.target.value)} /></label></div>
    <div className="dialog__actions"><button className="button button--primary" disabled={busy}>{busy ? "Đang lưu…" : "Lưu"}</button></div></form></AcademicDialog>;
}

/** Khoa → bộ môn của trường; khoa quản lý ngành (CTĐT), bộ môn quản lý học phần và giảng viên. */
export function OrgUnitsPage({ isAdmin }: { isAdmin: boolean }) {
  const universityId = Number(useParams().universityId); const university = useUniversity(universityId);
  const [units, setUnits] = useState<OrgUnit[] | null>(null); const [programs, setPrograms] = useState<(CurriculumProgram & { facultyId: number | null })[]>([]);
  const [error, setError] = useState(""); const [notice, setNotice] = useState(""); const [dialog, setDialog] = useState<OrgUnit | { kind: "faculty" | "department"; parentId: number | null } | null>(null);
  const load = useCallback(async () => {
    setError("");
    try { const [u, p] = await Promise.all([directoryApi.orgUnits(universityId), directoryApi.programs(universityId)]); setUnits(u); setPrograms(p.items); }
    catch (e) { setError(message(e)); }
  }, [universityId]);
  useEffect(() => { void load(); }, [load]);
  const faculties = (units ?? []).filter(u => u.kind === "faculty");
  async function archive(u: OrgUnit) { setError(""); try { await directoryApi.archiveOrgUnit(universityId, u.id); setNotice(`Đã lưu trữ ${u.code}.`); await load(); } catch (e) { setError(message(e)); } }
  async function assign(programId: number, facultyId: string) { setError(""); try { await directoryApi.assignFaculty(programId, facultyId ? Number(facultyId) : null); setNotice("Đã gán khoa quản lý ngành."); await load(); } catch (e) { setError(message(e)); } }
  return <section className={domainPage("teaching", "academic-page")}><UniversityCrumbs universityId={universityId} university={university} label="Khoa / Bộ môn" />
    <div className="admin-heading"><div><DomainEyebrow domain="teaching" detail={university?.name} /><h1>Khoa và bộ môn</h1><p>Khoa quản lý ngành đào tạo; bộ môn thuộc khoa, quản lý học phần (đề cương) và giảng viên.</p></div>
      {isAdmin && <div className="admin-heading__actions"><button className="button button--primary" onClick={() => setDialog({ kind: "faculty", parentId: null })}>Thêm khoa</button></div>}</div>
    <UniversityNav universityId={universityId} isAdmin={isAdmin} />
    <ErrorMessage value={error} /><Notice value={notice} />
    {!units ? !error && <p role="status">Đang tải…</p> : !faculties.length ? <p className="empty-state">Chưa khai báo khoa nào.</p> : faculties.map(f => <div key={f.id} className="org-unit-block">
      <div className="toolbar"><h2>{f.code} · {f.name}</h2>{isAdmin && <div className="table-actions"><button className="button button--small button--secondary" onClick={() => setDialog({ kind: "department", parentId: f.id })}>Thêm bộ môn</button>
        <button className="button button--small button--ghost" onClick={() => setDialog(f)}>Sửa</button><button className="button button--small button--ghost" onClick={() => void archive(f)}>Lưu trữ</button></div>}</div>
      <ul className="org-unit-list">{units.filter(d => d.parentId === f.id).map(d => <li key={d.id}><span><strong>{d.code}</strong> · {d.name}</span>{isAdmin && <span className="table-actions"><button className="button button--small button--ghost" onClick={() => setDialog(d)}>Sửa</button><button className="button button--small button--ghost" onClick={() => void archive(d)}>Lưu trữ</button></span>}</li>)}
        {!units.some(d => d.parentId === f.id) && <li className="empty-state">Chưa có bộ môn.</li>}</ul></div>)}
    <h2>Ngành đào tạo theo khoa</h2>
    <div className="data-table-wrap"><table className="data-table"><thead><tr><th>Mã CTĐT</th><th>Tên ngành / CTĐT</th><th>Khoa quản lý</th></tr></thead><tbody>{programs.map(p => <tr key={p.id}><td>{p.code}</td><td>{p.name}</td>
      <td>{isAdmin ? <select aria-label={`Khoa quản lý ${p.code}`} value={p.facultyId ?? ""} onChange={e => void assign(p.id, e.target.value)}><option value="">Chưa gán</option>{faculties.map(f => <option key={f.id} value={f.id}>{f.code} · {f.name}</option>)}</select> : faculties.find(f => f.id === p.facultyId)?.name ?? "—"}</td></tr>)}</tbody></table></div>
    {dialog && <OrgUnitDialog universityId={universityId} value={dialog} faculties={faculties} onClose={() => setDialog(null)} onSaved={() => { setDialog(null); setNotice("Đã lưu đơn vị."); void load(); }} />}
  </section>;
}

type BandDraft = { letter: string; minScore: string; point4: string; isPassing: boolean };
const toDraft = (b: GradeBand): BandDraft => ({ letter: b.letter, minScore: String(b.minScore), point4: String(b.point4), isPassing: b.isPassing });
const circular08: GradeBand[] = [{ letter: "A", minScore: 8.5, point4: 4, isPassing: true }, { letter: "B", minScore: 7, point4: 3, isPassing: true }, { letter: "C", minScore: 5.5, point4: 2, isPassing: true }, { letter: "D", minScore: 4, point4: 1, isPassing: true }, { letter: "F", minScore: 0, point4: 0, isPassing: false }];
const detailed: GradeBand[] = [{ letter: "A+", minScore: 9, point4: 4, isPassing: true }, { letter: "A", minScore: 8.5, point4: 3.7, isPassing: true }, { letter: "B+", minScore: 8, point4: 3.5, isPassing: true }, { letter: "B", minScore: 7, point4: 3, isPassing: true },
  { letter: "C+", minScore: 6.5, point4: 2.5, isPassing: true }, { letter: "C", minScore: 5.5, point4: 2, isPassing: true }, { letter: "D+", minScore: 5, point4: 1.5, isPassing: true }, { letter: "D", minScore: 4, point4: 1, isPassing: true }, { letter: "F", minScore: 0, point4: 0, isPassing: false }];

/** Quy chế đánh giá của trường: làm tròn, số đầu điểm tối thiểu, trọng số cuối kỳ, học lại và thang điểm chữ/hệ 4. */
export function GradingPolicyPage({ isAdmin }: { isAdmin: boolean }) {
  const universityId = Number(useParams().universityId); const university = useUniversity(universityId);
  const [policy, setPolicy] = useState<GradingPolicy | null>(null); const [bands, setBands] = useState<BandDraft[]>([]);
  const [decimals, setDecimals] = useState("1"); const [minComponents, setMinComponents] = useState("2"); const [minFinal, setMinFinal] = useState(""); const [retake, setRetake] = useState("highest"); const [gpaDecimals, setGpaDecimals] = useState("2");
  const [error, setError] = useState(""); const [notice, setNotice] = useState(""); const [busy, setBusy] = useState(false);
  const accept = (p: GradingPolicy) => { setPolicy(p); setBands(p.bands.map(toDraft)); setDecimals(String(p.courseScoreDecimals)); setMinComponents(String(p.minComponents)); setMinFinal(p.minFinalWeight ? String(p.minFinalWeight) : ""); setRetake(p.retakePolicy); setGpaDecimals(String(p.gpaDecimals)); };
  useEffect(() => { let active = true; void directoryApi.policy(universityId).then(p => { if (active) accept(p); }).catch(e => { if (active) setError(message(e)); }); return () => { active = false; }; }, [universityId]);
  async function save(e: FormEvent) {
    e.preventDefault(); setBusy(true); setError(""); setNotice("");
    try { accept(await directoryApi.savePolicy(universityId, { courseScoreDecimals: Number(decimals), minComponents: Number(minComponents), minFinalWeight: minFinal ? Number(minFinal) : null, retakePolicy: retake, gpaDecimals: Number(gpaDecimals),
      bands: bands.map(b => ({ letter: b.letter, minScore: Number(b.minScore), point4: Number(b.point4), isPassing: b.isPassing })) })); setNotice("Đã lưu quy chế. Điểm đã công bố giữ nguyên kết quả cũ; quy chế mới áp dụng cho lần công bố sau."); }
    catch (e) { setError(message(e)); } finally { setBusy(false); }
  }
  const set = (i: number, v: Partial<BandDraft>) => setBands(list => list.map((b, j) => j === i ? { ...b, ...v } : b));
  const off = !isAdmin || busy;
  return <section className={domainPage("assessment", "academic-page")}><UniversityCrumbs universityId={universityId} university={university} label="Quy chế & thang điểm" />
    <div className="admin-heading"><div><DomainEyebrow domain="assessment" detail={university?.name} /><h1>Quy chế đánh giá và thang điểm</h1><p>Giá trị mặc định theo Thông tư 08/2021/TT-BGDĐT. Trường có thể chi tiết hóa thang điểm chữ (A+, B+…) và quy định riêng về trọng số cuối kỳ, cách lấy điểm khi học lại.</p></div></div>
    <UniversityNav universityId={universityId} isAdmin={isAdmin} />
    <ErrorMessage value={error} /><Notice value={notice} />
    {policy && <form className="admin-form" onSubmit={e => void save(e)}>
      <div className="academic-form-row"><label>Làm tròn điểm học phần (chữ số thập phân)<select value={decimals} disabled={off} onChange={e => setDecimals(e.target.value)}>{[0, 1, 2].map(n => <option key={n}>{n}</option>)}</select></label>
        <label>Làm tròn GPA<select value={gpaDecimals} disabled={off} onChange={e => setGpaDecimals(e.target.value)}>{[1, 2, 3].map(n => <option key={n}>{n}</option>)}</select></label></div>
      <div className="academic-form-row"><label>Số đầu điểm tối thiểu (học phần ≥ 2 TC)<input type="number" min={1} max={10} value={minComponents} disabled={off} onChange={e => setMinComponents(e.target.value)} /></label>
        <label>Trọng số cuối kỳ tối thiểu (%)<input type="number" min={0.01} max={100} step={0.01} placeholder="Không quy định" value={minFinal} disabled={off} onChange={e => setMinFinal(e.target.value)} /></label>
        <label>Điểm tích lũy khi học lại / cải thiện<select value={retake} disabled={off} onChange={e => setRetake(e.target.value)}><option value="highest">Lấy lần có điểm cao nhất</option><option value="latest">Lấy lần học gần nhất</option></select></label></div>
      <h2>Thang điểm chữ và hệ 4</h2>
      <div className="data-table-wrap"><table className="data-table"><thead><tr><th>Điểm chữ</th><th>Từ điểm (thang 10)</th><th>Điểm hệ 4</th><th>Đạt</th>{isAdmin && <th></th>}</tr></thead><tbody>{bands.map((b, i) => <tr key={i}>
        <td><input aria-label={`Điểm chữ ${i + 1}`} required maxLength={5} value={b.letter} disabled={off} onChange={e => set(i, { letter: e.target.value.toUpperCase() })} /></td>
        <td><input aria-label={`Ngưỡng ${i + 1}`} type="number" min={0} max={10} step={0.01} required value={b.minScore} disabled={off} onChange={e => set(i, { minScore: e.target.value })} /></td>
        <td><input aria-label={`Hệ 4 của ${b.letter || i + 1}`} type="number" min={0} max={4} step={0.01} required value={b.point4} disabled={off} onChange={e => set(i, { point4: e.target.value })} /></td>
        <td><input aria-label={`${b.letter || i + 1} là đạt`} type="checkbox" checked={b.isPassing} disabled={off} onChange={e => set(i, { isPassing: e.target.checked })} /></td>
        {isAdmin && <td><button type="button" className="button button--ghost button--small" disabled={busy} onClick={() => setBands(list => list.filter((_, j) => j !== i))}>Bỏ</button></td>}</tr>)}</tbody></table></div>
      {isAdmin && <div className="dialog__actions"><button type="button" className="button button--secondary" disabled={busy} onClick={() => setBands([...bands, { letter: "", minScore: "", point4: "", isPassing: true }])}>Thêm mức</button>
        <button type="button" className="button button--ghost" disabled={busy} onClick={() => setBands(circular08.map(toDraft))}>Mặc định TT08 (A–F)</button>
        <button type="button" className="button button--ghost" disabled={busy} onClick={() => setBands(detailed.map(toDraft))}>Mẫu chi tiết (A+ … F)</button>
        <button className="button button--primary" disabled={busy}>{busy ? "Đang lưu…" : "Lưu quy chế"}</button></div>}
      <p>Xếp loại học lực theo GPA tích lũy hệ 4: Xuất sắc ≥ 3,6 · Giỏi ≥ 3,2 · Khá ≥ 2,5 · Trung bình ≥ 2,0 · Yếu ≥ 1,0 · Kém &lt; 1,0.</p>
    </form>}
  </section>;
}

function LecturerDialog({ value, departments, onClose, onSaved }: { value: LecturerProfile; departments: (OrgUnit & { universityName: string })[]; onClose: () => void; onSaved: () => void }) {
  const [code, setCode] = useState(value.lecturerCode ?? ""); const [department, setDepartment] = useState(value.departmentId ? String(value.departmentId) : "");
  const [title, setTitle] = useState(value.academicTitle ?? ""); const [degree, setDegree] = useState(value.degree ?? ""); const [busy, setBusy] = useState(false); const [error, setError] = useState("");
  async function save(e: FormEvent) {
    e.preventDefault(); setBusy(true); setError("");
    try { await directoryApi.saveLecturer(value.userId, { lecturerCode: code, departmentId: department ? Number(department) : null, academicTitle: title || null, degree: degree || null }); onSaved(); }
    catch (e) { setError(message(e)); } finally { setBusy(false); }
  }
  return <AcademicDialog title={`Hồ sơ giảng viên · ${value.fullName}`} onClose={busy ? () => {} : onClose}><form className="admin-form" onSubmit={e => void save(e)}><ErrorMessage value={error} />
    <label>Mã giảng viên<input autoFocus required maxLength={30} value={code} disabled={busy} onChange={e => setCode(e.target.value.toUpperCase())} /></label>
    <label>Bộ môn<select value={department} disabled={busy} onChange={e => setDepartment(e.target.value)}><option value="">Chưa gán</option>{departments.map(d => <option key={d.id} value={d.id}>{d.universityName} · {d.code} · {d.name}</option>)}</select></label>
    <div className="academic-form-row"><label>Học hàm<select value={title} disabled={busy} onChange={e => setTitle(e.target.value)}><option value="">Không</option><option>GS</option><option>PGS</option></select></label>
      <label>Học vị<select value={degree} disabled={busy} onChange={e => setDegree(e.target.value)}><option value="">Chưa cập nhật</option>{["CN", "KS", "ThS", "TS", "TSKH"].map(d => <option key={d}>{d}</option>)}</select></label></div>
    <div className="dialog__actions"><button className="button button--primary" disabled={busy}>{busy ? "Đang lưu…" : "Lưu hồ sơ"}</button></div></form></AcademicDialog>;
}

/** Hồ sơ giảng viên (mã GV, bộ môn, học hàm, học vị) gắn với tài khoản lecturer. */
export function LecturersPage() {
  const [rows, setRows] = useState<LecturerProfile[] | null>(null); const [search, setSearch] = useState(""); const [departments, setDepartments] = useState<(OrgUnit & { universityName: string })[]>([]);
  const [error, setError] = useState(""); const [notice, setNotice] = useState(""); const [editing, setEditing] = useState<LecturerProfile | null>(null);
  const load = useCallback(async () => { setError(""); try { setRows(await directoryApi.lecturers(search)); } catch (e) { setError(message(e)); } }, [search]);
  useEffect(() => { const t = setTimeout(() => void load(), 200); return () => clearTimeout(t); }, [load]);
  useEffect(() => { let active = true; void (async () => {
    const universities = await directoryApi.universities();
    const lists = await Promise.all(universities.items.map(async u => (await directoryApi.orgUnits(u.id)).filter(d => d.kind === "department").map(d => ({ ...d, universityName: u.code }))));
    if (active) setDepartments(lists.flat());
  })().catch(() => {}); return () => { active = false; }; }, []);
  return <section className={domainPage("teaching", "academic-page")}><div className="admin-heading"><div><DomainEyebrow domain="teaching" detail="Hồ sơ giảng viên" /><h1>Giảng viên</h1><p>Tài khoản giảng viên lấy từ Quản lý tài khoản; bổ sung mã giảng viên, bộ môn, học hàm và học vị.</p></div></div>
    <ErrorMessage value={error} /><Notice value={notice} />
    <div className="toolbar"><input aria-label="Tìm giảng viên" placeholder="Họ tên hoặc email" value={search} onChange={e => setSearch(e.target.value)} /></div>
    {!rows ? !error && <p role="status">Đang tải…</p> : !rows.length ? <p className="empty-state">Không có tài khoản giảng viên phù hợp.</p> : <div className="data-table-wrap"><table className="data-table"><thead><tr><th>Giảng viên</th><th>Email</th><th>Mã GV</th><th>Bộ môn</th><th>Học hàm · Học vị</th><th>Thao tác</th></tr></thead>
      <tbody>{rows.map(r => <tr key={r.userId}><td>{r.fullName}</td><td>{r.email}</td><td>{r.lecturerCode ?? "—"}</td><td>{r.departmentName ?? "—"}</td><td>{[r.academicTitle, r.degree].filter(Boolean).join(". ") || "—"}</td>
        <td><button className="button button--small button--secondary" onClick={() => setEditing(r)}>Cập nhật hồ sơ</button></td></tr>)}</tbody></table></div>}
    {editing && <LecturerDialog value={editing} departments={departments} onClose={() => setEditing(null)} onSaved={() => { setEditing(null); setNotice("Đã lưu hồ sơ giảng viên."); void load(); }} />}
  </section>;
}
