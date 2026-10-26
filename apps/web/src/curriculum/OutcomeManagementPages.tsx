import { useCallback, useEffect, useMemo, useState, type FormEvent, type ReactNode } from "react";
import { Link, NavLink, useParams } from "react-router-dom";
import { curriculumApi } from "../../../../modules/common/curriculum/frontend/api";
import type {
  Clo,
  CloPloMapping,
  CoursePloMapping,
  Plo,
  PloBalance,
  PloCreditCheck,
  ProgramCourse,
  ProgramVersion,
} from "../../../../modules/common/curriculum/frontend/types";
import { useAuth } from "../auth/AuthContext";
import { DialogFrame, useConfirmDialog } from "../components/Dialogs";

type MatrixData = {
  version: ProgramVersion;
  courses: ProgramCourse[];
  plos: Plo[];
  courseMappings: Record<number, CoursePloMapping[]>;
};

function messageOf(error: unknown, fallback: string) {
  return error instanceof Error ? error.message : fallback;
}

async function loadAllCourses(versionId: number) {
  const first = await curriculumApi.listCourses(versionId, 1, 100, "", "", false);
  const pages = await Promise.all(
    Array.from({ length: Math.max(0, first.totalPages - 1) }, (_, index) =>
      curriculumApi.listCourses(versionId, index + 2, 100, "", "", false),
    ),
  );
  return [first, ...pages].flatMap((page) => page.items);
}

async function loadMatrixData(versionId: number): Promise<MatrixData> {
  const [version, courses, plos] = await Promise.all([
    curriculumApi.getVersion(versionId),
    loadAllCourses(versionId),
    curriculumApi.listPlos(versionId),
  ]);
  const mappings = await Promise.all(
    courses.map(async (course) => [course.id, await curriculumApi.listCoursePloMappings(versionId, course.id)] as const),
  );
  return { version, courses, plos, courseMappings: Object.fromEntries(mappings) };
}

export function VersionOutcomeNav({ versionId }: { versionId: number }) {
  const items = [
    ["PLO", `/curriculum/versions/${versionId}/plos`],
    ["Học phần – PLO", `/curriculum/versions/${versionId}/course-plo-matrix`],
    ["CLO – PLO", `/curriculum/versions/${versionId}/clo-plo-matrix`],
    ["Kiểm tra dữ liệu", `/curriculum/versions/${versionId}/validation`],
  ] as const;
  return (
    <nav className="outcome-nav" aria-label="Quản lý chuẩn đầu ra">
      {items.map(([label, to]) => <NavLink key={to} to={to}>{label}</NavLink>)}
    </nav>
  );
}

function OutcomeShell({ versionId, version, title, description, children, actions }: {
  versionId: number;
  version: ProgramVersion | null;
  title: string;
  description: string;
  children: ReactNode;
  actions?: ReactNode;
}) {
  return <section className="admin-page">
    <div className="breadcrumbs"><Link to={`/curriculum/versions/${versionId}`}>Học phần</Link><span>/</span><span>{version?.versionCode ?? "…"}</span><span>/</span><span>{title}</span></div>
    <div className="admin-heading"><div><span className="eyebrow">CHUẨN ĐẦU RA · {version?.versionCode ?? "…"}</span><h1>{title}</h1><p>{description}</p></div>{actions && <div className="admin-heading__actions">{actions}</div>}</div>
    <VersionOutcomeNav versionId={versionId} />
    {children}
  </section>;
}

function ScreenState({ loading, error, empty, children, retry }: {
  loading: boolean;
  error: string;
  empty?: boolean;
  children: ReactNode;
  retry?: () => void;
}) {
  if (loading) return <div className="empty-state" aria-live="polite">Đang tải dữ liệu chuẩn đầu ra…</div>;
  if (error) return <div className="alert alert--error" role="alert"><span>{error}</span>{retry && <button className="button button--small button--ghost" onClick={retry}>Thử lại</button>}</div>;
  if (empty) return <div className="empty-state">Chưa có dữ liệu để hiển thị.</div>;
  return <>{children}</>;
}

type PloDraft = { code: string; statement: string; levelCode: string };

function PloEditor({ initial, busy, onCancel, onSave }: {
  initial?: Plo;
  busy: boolean;
  onCancel: () => void;
  onSave: (draft: PloDraft) => Promise<void>;
}) {
  const [draft, setDraft] = useState<PloDraft>({ code: initial?.code ?? "", statement: initial?.statement ?? "", levelCode: initial?.levelCode ?? "" });
  return <DialogFrame
    title={initial ? `Cập nhật ${initial.code}` : "Thêm chuẩn đầu ra PLO"}
    description="Mã PLO là duy nhất trong phiên bản chương trình đào tạo."
    onClose={onCancel}
  >
      <form className="admin-form" onSubmit={(event) => { event.preventDefault(); void onSave(draft); }}>
        <label><span>Mã PLO</span><input required autoFocus value={draft.code} onChange={(event) => setDraft({ ...draft, code: event.target.value })} placeholder="PLO1" /></label>
        <label><span>Mức năng lực</span><input value={draft.levelCode} onChange={(event) => setDraft({ ...draft, levelCode: event.target.value })} placeholder="Ví dụ: C3" /></label>
        <label><span>Nội dung chuẩn đầu ra</span><textarea required value={draft.statement} onChange={(event) => setDraft({ ...draft, statement: event.target.value })} /></label>
        <div className="dialog__actions"><button className="button button--ghost" type="button" onClick={onCancel}>Hủy</button><button className="button button--primary" disabled={busy}>{busy ? "Đang lưu…" : "Lưu PLO"}</button></div>
      </form>
  </DialogFrame>;
}

export function PloListPage() {
  const { versionId: rawId } = useParams();
  const versionId = Number(rawId);
  const { user } = useAuth();
  const { confirm, confirmationDialog } = useConfirmDialog();
  const [version, setVersion] = useState<ProgramVersion | null>(null);
  const [plos, setPlos] = useState<Plo[]>([]);
  const [editing, setEditing] = useState<Plo | "new" | null>(null);
  const [busy, setBusy] = useState(false);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const load = useCallback(async () => {
    if (!Number.isInteger(versionId) || versionId < 1) return;
    setLoading(true); setError("");
    try { const [v, items] = await Promise.all([curriculumApi.getVersion(versionId), curriculumApi.listPlos(versionId)]); setVersion(v); setPlos(items); }
    catch (caught) { setError(messageOf(caught, "Không tải được danh sách PLO.")); }
    finally { setLoading(false); }
  }, [versionId]);
  useEffect(() => { void load(); }, [load]);
  const editable = user?.role === "admin" && version?.status === "draft";
  async function save(draft: PloDraft) {
    setBusy(true); setError("");
    try {
      const input = { code: draft.code, statement: draft.statement, levelCode: draft.levelCode.trim() || null };
      if (editing === "new") await curriculumApi.createPlo(versionId, input);
      else if (editing) await curriculumApi.updatePlo(versionId, editing.id, input);
      setEditing(null); await load();
    } catch (caught) { setError(messageOf(caught, "Không lưu được PLO.")); }
    finally { setBusy(false); }
  }
  async function remove(plo: Plo) {
    const accepted = await confirm({
      title: `Xóa ${plo.code}?`,
      description: "Thao tác này không thể hoàn tác. PLO đang được ánh xạ sẽ không thể xóa.",
      confirmLabel: "Xóa PLO",
      tone: "danger",
    });
    if (!accepted) return;
    setBusy(true); setError("");
    try { await curriculumApi.deletePlo(versionId, plo.id); await load(); }
    catch (caught) { setError(messageOf(caught, "Không xóa được PLO.")); }
    finally { setBusy(false); }
  }
  return <OutcomeShell versionId={versionId} version={version} title="Danh sách PLO" description={editable ? "Quản lý chuẩn đầu ra của phiên bản CTĐT đang ở bản nháp." : "Danh sách chuẩn đầu ra ở chế độ chỉ đọc."} actions={editable && <button className="button button--primary" onClick={() => setEditing("new")}>+ Thêm PLO</button>}>
    <ScreenState loading={loading} error={error} empty={!plos.length} retry={() => void load()}>
      <div className="outcome-grid">{plos.map((plo, index) => <article className="outcome-panel" key={plo.id}>
        <div className="outcome-panel__index">{String(index + 1).padStart(2, "0")}</div>
        <div className="outcome-panel__body"><div className="outcome-panel__title"><strong>{plo.code}</strong>{plo.levelCode && <span className="status status--current">{plo.levelCode}</span>}</div><p>{plo.statement}</p><small>Nguồn: {plo.provenance}</small></div>
        {editable && <div className="table-actions"><button className="button button--small button--ghost" onClick={() => setEditing(plo)}>Sửa</button><button className="button button--small button--danger" disabled={busy} onClick={() => void remove(plo)}>Xóa</button></div>}
      </article>)}</div>
    </ScreenState>
    {confirmationDialog}
    {editing && <PloEditor initial={editing === "new" ? undefined : editing} busy={busy} onCancel={() => setEditing(null)} onSave={save} />}
  </OutcomeShell>;
}

type MappingEditorState = { course: ProgramCourse; plo: Plo; mapping?: CoursePloMapping };

function CourseMappingEditor({ state, busy, onCancel, onSave, onDelete }: {
  state: MappingEditorState;
  busy: boolean;
  onCancel: () => void;
  onSave: (input: { weightCode: string; progressionCode: string; fit: string | null; fitReason: string | null }) => Promise<void>;
  onDelete: () => Promise<void>;
}) {
  const [weightCode, setWeightCode] = useState(state.mapping?.weightCode ?? "X");
  const [progressionCode, setProgressionCode] = useState(state.mapping?.progressionCode ?? "I");
  const [fit, setFit] = useState(state.mapping?.fit ?? "");
  const [fitReason, setFitReason] = useState(state.mapping?.fitReason ?? "");
  return <DialogFrame
    title={`${state.course.institutionalCode ?? state.course.name} → ${state.plo.code}`}
    description={state.mapping ? "Cập nhật thông tin ánh xạ" : "Thiết lập ánh xạ học phần với PLO"}
    onClose={onCancel}
    className="matrix-dialog"
  >
    <form className="admin-form admin-form--grid" onSubmit={(event: FormEvent) => { event.preventDefault(); void onSave({ weightCode, progressionCode, fit: fit.trim() || null, fitReason: fitReason.trim() || null }); }}>
      <label><span>Trọng số</span><select value={weightCode} onChange={(event) => setWeightCode(event.target.value)}><option value="X">X · Chính</option><option value="Y">Y · Hỗ trợ</option></select></label>
      <label><span>Mức tiến triển</span><select value={progressionCode} onChange={(event) => setProgressionCode(event.target.value)}><option value="I">I · Giới thiệu</option><option value="R">R · Củng cố</option><option value="E">E · Thành thạo</option></select></label>
      <label className="form-wide"><span>Mức phù hợp</span><input value={fit} onChange={(event) => setFit(event.target.value)} placeholder="Không bắt buộc" /></label>
      <label className="form-wide"><span>Giải thích</span><textarea value={fitReason} onChange={(event) => setFitReason(event.target.value)} placeholder="Lý do học phần đóng góp cho PLO này" /></label>
      <div className="dialog__actions form-wide">{state.mapping && <button className="button button--danger" type="button" disabled={busy} onClick={() => void onDelete()}>Xóa ánh xạ</button>}<span className="dialog__spacer" /><button className="button button--ghost" type="button" onClick={onCancel}>Hủy</button><button className="button button--primary" disabled={busy}>{busy ? "Đang lưu…" : "Lưu ánh xạ"}</button></div>
    </form>
  </DialogFrame>;
}

export function CoursePloMatrixPage() {
  const { versionId: rawId } = useParams(); const versionId = Number(rawId); const { user } = useAuth();
  const [data, setData] = useState<MatrixData | null>(null); const [editor, setEditor] = useState<MappingEditorState | null>(null);
  const [loading, setLoading] = useState(true); const [busy, setBusy] = useState(false); const [error, setError] = useState("");
  const load = useCallback(async () => { setLoading(true); setError(""); try { setData(await loadMatrixData(versionId)); } catch (caught) { setError(messageOf(caught, "Không tải được ma trận học phần–PLO.")); } finally { setLoading(false); } }, [versionId]);
  useEffect(() => { void load(); }, [load]);
  const editable = user?.role === "admin" && data?.version.status === "draft";
  async function save(input: { weightCode: string; progressionCode: string; fit: string | null; fitReason: string | null }) {
    if (!editor) return; setBusy(true); setError("");
    try { if (editor.mapping) await curriculumApi.updateCoursePloMapping(versionId, editor.course.id, editor.mapping.id, input); else await curriculumApi.addCoursePloMapping(versionId, editor.course.id, { ploId: editor.plo.id, ...input }); setEditor(null); await load(); }
    catch (caught) { setError(messageOf(caught, "Không lưu được ánh xạ học phần–PLO.")); }
    finally { setBusy(false); }
  }
  async function remove() { if (!editor?.mapping) return; setBusy(true); try { await curriculumApi.deleteCoursePloMapping(versionId, editor.course.id, editor.mapping.id); setEditor(null); await load(); } catch (caught) { setError(messageOf(caught, "Không xóa được ánh xạ.")); } finally { setBusy(false); } }
  return <OutcomeShell versionId={versionId} version={data?.version ?? null} title="Ma trận học phần – PLO" description={editable ? "Chọn một ô để thiết lập trọng số và mức tiến triển." : "Ma trận thể hiện mức đóng góp của từng học phần vào chuẩn đầu ra."}>
    <ScreenState loading={loading} error={error} empty={!data?.courses.length || !data?.plos.length} retry={() => void load()}>
      {data && <><div className="matrix-legend"><span><b>X</b> Đóng góp chính</span><span><b>Y</b> Đóng góp hỗ trợ</span><span><b>I/R/E</b> Giới thiệu / Củng cố / Thành thạo</span></div><div className="matrix-wrap"><table className="matrix-table"><thead><tr><th className="matrix-table__sticky">Học phần</th><th>TC</th>{data.plos.map((plo) => <th key={plo.id} title={plo.statement}>{plo.code}</th>)}</tr></thead><tbody>{data.courses.map((course) => <tr key={course.id}><th className="matrix-table__sticky"><Link to={`/curriculum/versions/${versionId}/courses/${course.id}/outcomes`}>{course.institutionalCode ?? `#${course.id}`}</Link><small>{course.name}</small></th><td>{course.credits}</td>{data.plos.map((plo) => { const mapping = data.courseMappings[course.id]?.find((item) => item.ploId === plo.id); return <td key={plo.id}><button className={`matrix-cell ${mapping ? "matrix-cell--mapped" : ""}`} disabled={!editable} title={mapping?.fitReason ?? (editable ? "Thiết lập ánh xạ" : "Chưa ánh xạ")} onClick={() => setEditor({ course, plo, mapping })}>{mapping ? <><strong>{mapping.weightCode}</strong><span>{mapping.progressionCode}</span></> : "—"}</button></td>; })}</tr>)}</tbody></table></div></>}
    </ScreenState>
    {editor && <CourseMappingEditor state={editor} busy={busy} onCancel={() => setEditor(null)} onSave={save} onDelete={remove} />}
  </OutcomeShell>;
}

type CloMatrixData = MatrixData & { clos: Record<number, Clo[]>; cloMappings: Record<number, CloPloMapping[]> };

export function CloPloMatrixPage() {
  const { versionId: rawId } = useParams(); const versionId = Number(rawId); const { user } = useAuth();
  const [data, setData] = useState<CloMatrixData | null>(null); const [loading, setLoading] = useState(true); const [busyCell, setBusyCell] = useState(""); const [error, setError] = useState("");
  const load = useCallback(async () => {
    setLoading(true); setError("");
    try {
      const base = await loadMatrixData(versionId);
      const cloPairs = await Promise.all(base.courses.map(async (course) => [course.id, await curriculumApi.listClos(versionId, course.id)] as const));
      const clos = Object.fromEntries(cloPairs) as Record<number, Clo[]>;
      const allClos = cloPairs.flatMap(([, items]) => items.map((clo) => ({ courseId: clo.programCourseId, clo })));
      const mappingPairs = await Promise.all(allClos.map(async ({ courseId, clo }) => [clo.id, await curriculumApi.listCloPloMappings(versionId, courseId, clo.id)] as const));
      setData({ ...base, clos, cloMappings: Object.fromEntries(mappingPairs) });
    } catch (caught) { setError(messageOf(caught, "Không tải được ma trận CLO–PLO.")); }
    finally { setLoading(false); }
  }, [versionId]);
  useEffect(() => { void load(); }, [load]);
  const rows = useMemo(() => data?.courses.flatMap((course) => (data.clos[course.id] ?? []).map((clo) => ({ course, clo }))) ?? [], [data]);
  const editable = data?.version.status === "draft" && (user?.role === "admin" || user?.role === "lecturer");
  async function toggle(course: ProgramCourse, clo: Clo, plo: Plo) {
    if (!data || !editable) return;
    const courseMapping = data.courseMappings[course.id]?.find((item) => item.ploId === plo.id); if (!courseMapping) return;
    const selected = data.cloMappings[clo.id]?.some((item) => item.coursePloId === courseMapping.id);
    const key = `${clo.id}-${plo.id}`; setBusyCell(key); setError("");
    try { if (selected) await curriculumApi.deleteCloPloMapping(versionId, course.id, clo.id, courseMapping.id); else await curriculumApi.addCloPloMapping(versionId, course.id, clo.id, courseMapping.id); await load(); }
    catch (caught) { setError(messageOf(caught, "Không cập nhật được ánh xạ CLO–PLO.")); }
    finally { setBusyCell(""); }
  }
  return <OutcomeShell versionId={versionId} version={data?.version ?? null} title="Ma trận CLO – PLO" description={editable ? "Đánh dấu CLO đóng góp cho PLO trong phạm vi ánh xạ của học phần." : "Ma trận chi tiết liên kết chuẩn đầu ra học phần với chuẩn đầu ra chương trình."}>
    <ScreenState loading={loading} error={error} empty={!rows.length || !data?.plos.length} retry={() => void load()}>
      {data && <><div className="matrix-legend"><span><b>●</b> Đã ánh xạ</span><span><b>○</b> Có thể ánh xạ</span><span><b>—</b> Học phần chưa liên kết PLO</span></div><div className="matrix-wrap"><table className="matrix-table matrix-table--clo"><thead><tr><th className="matrix-table__sticky">Học phần / CLO</th>{data.plos.map((plo) => <th key={plo.id} title={plo.statement}>{plo.code}</th>)}</tr></thead><tbody>{rows.map(({ course, clo }) => <tr key={clo.id}><th className="matrix-table__sticky"><span>{course.institutionalCode ?? course.name}</span><Link to={`/curriculum/versions/${versionId}/courses/${course.id}/outcomes`}>{clo.code}</Link><small>{clo.statement}</small></th>{data.plos.map((plo) => { const base = data.courseMappings[course.id]?.find((item) => item.ploId === plo.id); const selected = base && data.cloMappings[clo.id]?.some((item) => item.coursePloId === base.id); const key = `${clo.id}-${plo.id}`; return <td key={plo.id}><button className={`matrix-cell matrix-cell--toggle ${selected ? "matrix-cell--selected" : ""}`} disabled={!editable || !base || busyCell === key} onClick={() => void toggle(course, clo, plo)} aria-label={`${selected ? "Bỏ" : "Thêm"} ánh xạ ${clo.code} với ${plo.code}`}>{!base ? "—" : selected ? "●" : "○"}</button></td>; })}</tr>)}</tbody></table></div></>}
    </ScreenState>
  </OutcomeShell>;
}

type ValidationWarning = { key: string; severity: "error" | "warning"; title: string; detail: string; href?: string };

export function DataValidationPage() {
  const { versionId: rawId } = useParams(); const versionId = Number(rawId);
  const [version, setVersion] = useState<ProgramVersion | null>(null); const [credit, setCredit] = useState<PloCreditCheck | null>(null); const [balance, setBalance] = useState<PloBalance | null>(null);
  const [warnings, setWarnings] = useState<ValidationWarning[]>([]); const [loading, setLoading] = useState(true); const [error, setError] = useState("");
  const load = useCallback(async () => {
    setLoading(true); setError("");
    try {
      const [v, courses, plos, creditResult, balanceResult] = await Promise.all([curriculumApi.getVersion(versionId), loadAllCourses(versionId), curriculumApi.listPlos(versionId), curriculumApi.getPloCreditCheck(versionId), curriculumApi.getPloBalance(versionId)]);
      const courseDetails = await Promise.all(courses.map(async (course) => {
        const [clos, mappings] = await Promise.all([curriculumApi.listClos(versionId, course.id), curriculumApi.listCoursePloMappings(versionId, course.id)]);
        const cloMappings = await Promise.all(clos.map((clo) => curriculumApi.listCloPloMappings(versionId, course.id, clo.id)));
        return { course, clos, mappings, cloMappings };
      }));
      const next: ValidationWarning[] = [];
      if (!plos.length) next.push({ key: "no-plos", severity: "error", title: "Phiên bản chưa có PLO", detail: "Cần khai báo ít nhất một chuẩn đầu ra chương trình.", href: `/curriculum/versions/${versionId}/plos` });
      creditResult.courses.filter((item) => !item.isValid).forEach((item) => next.push({ key: `credit-${item.programCourseId}`, severity: "error", title: `${item.institutionalCode ?? item.courseName}: số PLO chưa phù hợp`, detail: `${item.credits} tín chỉ yêu cầu tối đa ${item.requiredPloCount} PLO, hiện đang ánh xạ ${item.actualPloCount}.`, href: `/curriculum/versions/${versionId}/course-plo-matrix` }));
      balanceResult.items.filter((item) => item.exceedsTwentyPercent).forEach((item) => next.push({ key: `balance-${item.ploCode}`, severity: "warning", title: `${item.ploCode} mất cân bằng`, detail: `Điểm đóng góp ${item.score.toFixed(2)}, lệch ${item.deviation?.toFixed(2) ?? "—"} so với trung bình ${item.meanScore.toFixed(2)}.`, href: `/curriculum/versions/${versionId}/course-plo-matrix` }));
      courseDetails.forEach(({ course, clos, mappings, cloMappings }) => {
        if (!mappings.length) next.push({ key: `course-map-${course.id}`, severity: "error", title: `${course.institutionalCode ?? course.name} chưa ánh xạ PLO`, detail: "Học phần cần được liên kết với ít nhất một PLO.", href: `/curriculum/versions/${versionId}/course-plo-matrix` });
        if (!clos.length) next.push({ key: `course-clo-${course.id}`, severity: "error", title: `${course.institutionalCode ?? course.name} chưa có CLO`, detail: "Học phần cần khai báo chuẩn đầu ra học phần.", href: `/curriculum/versions/${versionId}/courses/${course.id}/outcomes` });
        clos.forEach((clo, index) => { if (!cloMappings[index]?.length) next.push({ key: `clo-${clo.id}`, severity: "warning", title: `${clo.code} chưa ánh xạ PLO`, detail: `${course.institutionalCode ?? course.name}: ${clo.statement}`, href: `/curriculum/versions/${versionId}/courses/${course.id}/outcomes` }); });
      });
      setVersion(v); setCredit(creditResult); setBalance(balanceResult); setWarnings(next);
    } catch (caught) { setError(messageOf(caught, "Không thể kiểm tra tính hợp lệ của dữ liệu.")); }
    finally { setLoading(false); }
  }, [versionId]);
  useEffect(() => { void load(); }, [load]);
  const errors = warnings.filter((item) => item.severity === "error").length; const warningCount = warnings.length - errors;
  return <OutcomeShell versionId={versionId} version={version} title="Kiểm tra dữ liệu" description="Tổng hợp các vấn đề cần xử lý trước khi công bố phiên bản chương trình đào tạo." actions={<button className="button button--secondary" onClick={() => void load()}>Kiểm tra lại</button>}>
    <ScreenState loading={loading} error={error} retry={() => void load()}>
      <div className="validation-summary"><article className={errors ? "is-invalid" : "is-valid"}><span>Lỗi cần xử lý</span><strong>{errors}</strong></article><article className={warningCount ? "is-warning" : "is-valid"}><span>Cảnh báo</span><strong>{warningCount}</strong></article><article className={credit?.isValid ? "is-valid" : "is-invalid"}><span>Giới hạn PLO/tín chỉ</span><strong>{credit?.isValid ? "Đạt" : "Chưa đạt"}</strong></article><article className={balance?.isBalanced ? "is-valid" : "is-warning"}><span>Cân bằng PLO</span><strong>{balance?.isBalanced ? "Đạt" : "Cần xem lại"}</strong></article></div>
      {!warnings.length ? <div className="validation-success"><span>✓</span><div><strong>Dữ liệu hợp lệ</strong><p>Không phát hiện lỗi hoặc cảnh báo trong phiên bản này.</p></div></div> : <div className="validation-list">{warnings.map((item) => <article className={`validation-item validation-item--${item.severity}`} key={item.key}><span className="validation-item__icon">{item.severity === "error" ? "!" : "△"}</span><div><strong>{item.title}</strong><p>{item.detail}</p></div>{item.href && <Link className="button button--small button--ghost" to={item.href}>Xử lý</Link>}</article>)}</div>}
    </ScreenState>
  </OutcomeShell>;
}
