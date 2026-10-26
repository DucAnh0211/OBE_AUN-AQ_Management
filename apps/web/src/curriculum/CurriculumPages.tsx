import { type FormEvent, type ReactNode, useCallback, useEffect, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { ApiError, curriculumApi, swaggerUrl } from "../../../../modules/common/curriculum/frontend/api";
import type { CurriculumProgram, PagedResult, ProgramCourse, ProgramVersion } from "../../../../modules/common/curriculum/frontend/types";
import { useAuth } from "../auth/AuthContext";
import { DialogFrame, useConfirmDialog } from "../components/Dialogs";

const pageSize = 10;

function formatDate(value: string | null): string {
  return value
    ? new Intl.DateTimeFormat("vi-VN", { dateStyle: "short", timeStyle: "short" }).format(new Date(value))
    : "—";
}

function statusLabel(status: string): string {
  return ({ draft: "Bản nháp", finalized: "Đã công bố", archived: "Đã lưu trữ", active: "Đang sử dụng" })[status] ?? status;
}

function errorInfo(error: unknown): { message: string; fields: Record<string, string[]> } {
  if (error instanceof ApiError) return { message: error.message, fields: error.fieldErrors };
  return { message: "Đã xảy ra lỗi không xác định.", fields: {} };
}

function PageHeader({ eyebrow, title, description, actions }: {
  eyebrow: string;
  title: string;
  description: string;
  actions?: ReactNode;
}) {
  return (
    <div className="admin-heading">
      <div>
        <span className="eyebrow">{eyebrow}</span>
        <h1>{title}</h1>
        <p>{description}</p>
      </div>
      {actions && <div className="admin-heading__actions">{actions}</div>}
    </div>
  );
}

function Alert({ message, onRetry }: { message: string; onRetry?: () => void }) {
  return (
    <div className="alert alert--error" role="alert">
      <span>{message}</span>
      {onRetry && <button className="button button--small button--ghost" onClick={onRetry}>Thử lại</button>}
    </div>
  );
}

function EmptyState({ children }: { children: ReactNode }) {
  return <div className="empty-state">{children}</div>;
}

function LoadingState() {
  return <div className="empty-state" aria-live="polite">Đang tải dữ liệu…</div>;
}

function Dialog({ title, description, children, onClose }: {
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

function FieldError({ errors }: { errors?: string[] }) {
  return errors?.length ? <span className="field-error">{errors[0]}</span> : null;
}

function Pagination({ page, totalPages, totalItems, onChange }: {
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

type ProgramDialogState = { mode: "create" } | { mode: "edit"; program: CurriculumProgram };

function ProgramForm({ state, onClose, onSaved }: {
  state: ProgramDialogState;
  onClose: () => void;
  onSaved: () => void;
}) {
  const editing = state.mode === "edit";
  const [code, setCode] = useState(editing ? state.program.code : "");
  const [name, setName] = useState(editing ? state.program.name : "");
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");
  const [fields, setFields] = useState<Record<string, string[]>>({});

  async function submit(event: FormEvent) {
    event.preventDefault();
    setSaving(true);
    setError("");
    setFields({});
    try {
      if (editing) await curriculumApi.updateProgram(state.program.id, name);
      else await curriculumApi.createProgram(code, name);
      onSaved();
    } catch (caught) {
      const info = errorInfo(caught);
      setError(info.message);
      setFields(info.fields);
    } finally {
      setSaving(false);
    }
  }

  return (
    <Dialog title={editing ? "Cập nhật chương trình đào tạo" : "Tạo chương trình đào tạo"} onClose={onClose}>
      <form className="admin-form" onSubmit={submit}>
        {error && <Alert message={error} />}
        <label>
          <span>Mã CTĐT</span>
          <input value={code} onChange={(event) => setCode(event.target.value)} disabled={editing || saving} placeholder="Ví dụ: CS" autoFocus={!editing} />
          <FieldError errors={fields.code} />
        </label>
        <label>
          <span>Tên chương trình đào tạo</span>
          <input value={name} onChange={(event) => setName(event.target.value)} disabled={saving} placeholder="Ví dụ: Khoa học máy tính" autoFocus={editing} />
          <FieldError errors={fields.name} />
        </label>
        <div className="dialog__actions">
          <button className="button button--ghost" type="button" onClick={onClose} disabled={saving}>Hủy</button>
          <button className="button button--primary" disabled={saving}>{saving ? "Đang lưu…" : "Lưu"}</button>
        </div>
      </form>
    </Dialog>
  );
}

export function CurriculumProgramsPage() {
  const { user } = useAuth();
  const isAdmin = user?.role === "admin";
  const { confirm, confirmationDialog } = useConfirmDialog();
  const [data, setData] = useState<PagedResult<CurriculumProgram> | null>(null);
  const [page, setPage] = useState(1);
  const [searchInput, setSearchInput] = useState("");
  const [search, setSearch] = useState("");
  const [includeArchived, setIncludeArchived] = useState(false);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [dialog, setDialog] = useState<ProgramDialogState | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError("");
    try {
      setData(await curriculumApi.listPrograms(page, pageSize, search, includeArchived));
    } catch (caught) {
      setError(errorInfo(caught).message);
    } finally {
      setLoading(false);
    }
  }, [includeArchived, page, search]);

  useEffect(() => { void load(); }, [load]);

  async function archive(program: CurriculumProgram) {
    const accepted = await confirm({
      title: "Lưu trữ chương trình đào tạo?",
      description: `CTĐT “${program.name}” sẽ không còn xuất hiện trong danh sách đang hoạt động. Dữ liệu liên quan vẫn được giữ lại.`,
      confirmLabel: "Lưu trữ",
      tone: "danger",
    });
    if (!accepted) return;
    try {
      await curriculumApi.archiveProgram(program.id);
      await load();
    } catch (caught) {
      setError(errorInfo(caught).message);
    }
  }

  return (
    <section className="admin-page">
      <PageHeader
        eyebrow="Quản trị dữ liệu đào tạo"
        title="Chương trình đào tạo"
        description="Quản lý CTĐT và đi sâu vào từng phiên bản, học phần."
        actions={isAdmin ? <><a className="button button--ghost" href={swaggerUrl} target="_blank" rel="noreferrer">Mở Swagger</a><button className="button button--primary" onClick={() => setDialog({ mode: "create" })}>+ Tạo CTĐT</button></> : undefined}
      />

      <div className="toolbar">
        <form className="search-box" onSubmit={(event) => { event.preventDefault(); setPage(1); setSearch(searchInput.trim()); }}>
          <input value={searchInput} onChange={(event) => setSearchInput(event.target.value)} placeholder="Tìm theo mã hoặc tên CTĐT" aria-label="Tìm chương trình đào tạo" />
          <button className="button button--secondary">Tìm kiếm</button>
        </form>
        <label className="check-control"><input type="checkbox" checked={includeArchived} onChange={(event) => { setPage(1); setIncludeArchived(event.target.checked); }} /> Hiện dữ liệu lưu trữ</label>
      </div>

      {error && <Alert message={error} onRetry={() => void load()} />}
      {loading ? <LoadingState /> : !data?.items.length ? <EmptyState>Chưa có chương trình đào tạo phù hợp.</EmptyState> : (
        <>
          <div className="data-table-wrap">
            <table className="data-table">
              <thead><tr><th>Mã</th><th>Chương trình đào tạo</th><th>Phiên bản hiện hành</th><th>Số phiên bản</th><th>Cập nhật</th><th className="align-right">Thao tác</th></tr></thead>
              <tbody>{data.items.map((program) => (
                <tr key={program.id} className={program.isArchived ? "row--archived" : ""}>
                  <td><strong>{program.code}</strong></td>
                  <td><Link className="table-link" to={`/curriculum/programs/${program.id}`}>{program.name}</Link>{program.isArchived && <span className="status status--archived">Đã lưu trữ</span>}</td>
                  <td>{program.currentVersionCode ?? "Chưa có"}</td>
                  <td>{program.versionCount}</td>
                  <td>{formatDate(program.updatedAt)}</td>
                  <td><div className="table-actions"><Link className="button button--small button--secondary" to={`/curriculum/programs/${program.id}`}>Mở</Link>{isAdmin && !program.isArchived && <><button className="button button--small button--ghost" onClick={() => setDialog({ mode: "edit", program })}>Sửa</button><button className="button button--small button--danger" onClick={() => void archive(program)}>Lưu trữ</button></>}</div></td>
                </tr>
              ))}</tbody>
            </table>
          </div>
          <Pagination page={data.page} totalPages={data.totalPages} totalItems={data.totalItems} onChange={setPage} />
        </>
      )}
      {confirmationDialog}
      {dialog && <ProgramForm state={dialog} onClose={() => setDialog(null)} onSaved={() => { setDialog(null); void load(); }} />}
    </section>
  );
}

type VersionDialogState = { mode: "create" } | { mode: "edit"; version: ProgramVersion };

function VersionForm({ state, versions, programId, onClose, onSaved }: {
  state: VersionDialogState;
  versions: ProgramVersion[];
  programId: number;
  onClose: () => void;
  onSaved: () => void;
}) {
  const editing = state.mode === "edit";
  const [code, setCode] = useState(editing ? state.version.versionCode : "");
  const [sourceId, setSourceId] = useState("");
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");
  const [fields, setFields] = useState<Record<string, string[]>>({});
  const sources = versions.filter((version) => version.status === "finalized" && !version.archivedAt);

  async function submit(event: FormEvent) {
    event.preventDefault();
    setSaving(true);
    setError("");
    setFields({});
    try {
      if (editing) await curriculumApi.updateVersion(state.version.id, code);
      else await curriculumApi.createVersion(programId, code, sourceId ? Number(sourceId) : null);
      onSaved();
    } catch (caught) {
      const info = errorInfo(caught);
      setError(info.message);
      setFields(info.fields);
    } finally {
      setSaving(false);
    }
  }

  return (
    <Dialog title={editing ? "Đổi mã phiên bản" : "Tạo phiên bản CTĐT"} description={!editing ? "Có thể tạo trống hoặc sao chép danh sách học phần từ phiên bản đã công bố." : undefined} onClose={onClose}>
      <form className="admin-form" onSubmit={submit}>
        {error && <Alert message={error} />}
        <label><span>Mã phiên bản</span><input value={code} onChange={(event) => setCode(event.target.value)} disabled={saving} placeholder="Ví dụ: V2" autoFocus /><FieldError errors={fields.versionCode} /></label>
        {!editing && <label><span>Sao chép học phần từ</span><select value={sourceId} onChange={(event) => setSourceId(event.target.value)} disabled={saving}><option value="">Không sao chép — tạo phiên bản trống</option>{sources.map((version) => <option key={version.id} value={version.id}>{version.versionCode} · {version.courseCount} học phần</option>)}</select><FieldError errors={fields.sourceVersionId} /></label>}
        <div className="dialog__actions"><button className="button button--ghost" type="button" onClick={onClose} disabled={saving}>Hủy</button><button className="button button--primary" disabled={saving}>{saving ? "Đang lưu…" : "Lưu"}</button></div>
      </form>
    </Dialog>
  );
}

export function ProgramVersionsPage() {
  const { user } = useAuth();
  const isAdmin = user?.role === "admin";
  const { confirm, confirmationDialog } = useConfirmDialog();
  const { programId: rawId } = useParams();
  const programId = Number(rawId);
  const navigate = useNavigate();
  const [program, setProgram] = useState<CurriculumProgram | null>(null);
  const [versions, setVersions] = useState<ProgramVersion[]>([]);
  const [includeArchived, setIncludeArchived] = useState(false);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [dialog, setDialog] = useState<VersionDialogState | null>(null);
  const [busyId, setBusyId] = useState<number | null>(null);

  const load = useCallback(async () => {
    if (!Number.isInteger(programId) || programId < 1) return;
    setLoading(true);
    setError("");
    try {
      const [programResult, versionResult] = await Promise.all([
        curriculumApi.getProgram(programId),
        curriculumApi.listVersions(programId, includeArchived),
      ]);
      setProgram(programResult);
      setVersions(versionResult);
    } catch (caught) {
      setError(errorInfo(caught).message);
    } finally {
      setLoading(false);
    }
  }, [includeArchived, programId]);

  useEffect(() => { void load(); }, [load]);

  async function publish(version: ProgramVersion) {
    const accepted = await confirm({
      title: `Công bố phiên bản ${version.versionCode}?`,
      description: "Sau khi công bố, học phần và chuẩn đầu ra của phiên bản này chuyển sang chế độ chỉ đọc.",
      confirmLabel: "Công bố phiên bản",
    });
    if (!accepted) return;
    setBusyId(version.id);
    try { await curriculumApi.publishVersion(version.id); await load(); }
    catch (caught) { setError(errorInfo(caught).message); }
    finally { setBusyId(null); }
  }

  async function archive(version: ProgramVersion) {
    const accepted = await confirm({
      title: `Lưu trữ phiên bản ${version.versionCode}?`,
      description: "Phiên bản sẽ được chuyển khỏi danh sách đang sử dụng nhưng dữ liệu vẫn được giữ lại.",
      confirmLabel: "Lưu trữ",
      tone: "danger",
    });
    if (!accepted) return;
    setBusyId(version.id);
    try { await curriculumApi.archiveVersion(version.id); await load(); }
    catch (caught) { setError(errorInfo(caught).message); }
    finally { setBusyId(null); }
  }

  if (!Number.isInteger(programId) || programId < 1) return <section className="admin-page"><Alert message="Mã chương trình đào tạo không hợp lệ." /></section>;

  return (
    <section className="admin-page">
      <div className="breadcrumbs"><Link to="/curriculum">Chương trình đào tạo</Link><span>/</span><span>{program?.code ?? "…"}</span></div>
      <PageHeader eyebrow={program?.code ?? "CTĐT"} title={program?.name ?? "Phiên bản chương trình đào tạo"} description="Quản lý các phiên bản và danh sách học phần thuộc từng phiên bản." actions={isAdmin ? <button className="button button--primary" disabled={!program || program.isArchived} onClick={() => setDialog({ mode: "create" })}>+ Tạo phiên bản</button> : undefined} />
      <div className="toolbar toolbar--end"><label className="check-control"><input type="checkbox" checked={includeArchived} onChange={(event) => setIncludeArchived(event.target.checked)} /> Hiện phiên bản lưu trữ</label></div>
      {error && <Alert message={error} onRetry={() => void load()} />}
      {loading ? <LoadingState /> : !versions.length ? <EmptyState>Chưa có phiên bản CTĐT.</EmptyState> : (
        <div className="version-grid">
          {versions.map((version) => {
            const source = versions.find((item) => item.id === version.sourceVersionId);
            return <article className={`version-card ${version.status === "archived" ? "version-card--archived" : ""}`} key={version.id}>
              <div className="version-card__top"><div><span className={`status status--${version.status}`}>{statusLabel(version.status)}</span>{version.isCurrent && <span className="status status--current">Hiện hành</span>}</div><strong>{version.versionCode}</strong></div>
              <dl><div><dt>Học phần</dt><dd>{version.courseCount}</dd></div><div><dt>Nguồn sao chép</dt><dd>{source?.versionCode ?? (version.sourceVersionId ? `#${version.sourceVersionId}` : "Tạo mới")}</dd></div><div><dt>Ngày công bố</dt><dd>{formatDate(version.publishedAt)}</dd></div></dl>
              <div className="version-card__actions"><button className="button button--secondary" onClick={() => navigate(`/curriculum/versions/${version.id}`)}>Xem học phần</button><Link className="button button--ghost" to={`/curriculum/versions/${version.id}/plos`}>Chuẩn đầu ra</Link>{isAdmin && version.status === "draft" && <><button className="button button--ghost" onClick={() => setDialog({ mode: "edit", version })}>Đổi mã</button><button className="button button--primary" disabled={busyId === version.id} onClick={() => void publish(version)}>Công bố</button></>}{isAdmin && version.status !== "archived" && <button className="button button--danger" disabled={busyId === version.id} onClick={() => void archive(version)}>Lưu trữ</button>}</div>
            </article>;
          })}
        </div>
      )}
      {confirmationDialog}
      {dialog && <VersionForm state={dialog} versions={versions} programId={programId} onClose={() => setDialog(null)} onSaved={() => { setDialog(null); void load(); }} />}
    </section>
  );
}

type CourseDialogState = { mode: "create" } | { mode: "edit"; course: ProgramCourse };

function CourseForm({ state, versionId, onClose, onSaved }: {
  state: CourseDialogState;
  versionId: number;
  onClose: () => void;
  onSaved: () => void;
}) {
  const editing = state.mode === "edit";
  const course = editing ? state.course : null;
  const [institutionalCode, setInstitutionalCode] = useState(course?.institutionalCode ?? "");
  const [name, setName] = useState(course?.name ?? "");
  const [credits, setCredits] = useState(String(course?.credits ?? 3));
  const [semester, setSemester] = useState(course?.semester ?? "");
  const [displayOrder, setDisplayOrder] = useState(course ? String(course.displayOrder) : "");
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");
  const [fields, setFields] = useState<Record<string, string[]>>({});

  async function submit(event: FormEvent) {
    event.preventDefault();
    setSaving(true);
    setError("");
    setFields({});
    const input = {
      institutionalCode: institutionalCode.trim() || null,
      name,
      credits: Number(credits),
      semester,
      displayOrder: displayOrder ? Number(displayOrder) : null,
    };
    try {
      if (editing) await curriculumApi.updateCourse(versionId, state.course.id, input);
      else await curriculumApi.createCourse(versionId, input);
      onSaved();
    } catch (caught) {
      const info = errorInfo(caught);
      setError(info.message);
      setFields(info.fields);
    } finally {
      setSaving(false);
    }
  }

  return (
    <Dialog title={editing ? "Cập nhật học phần" : "Thêm học phần"} onClose={onClose}>
      <form className="admin-form admin-form--grid" onSubmit={submit}>
        {error && <div className="form-wide"><Alert message={error} /></div>}
        <label><span>Mã học phần <small>(không bắt buộc)</small></span><input value={institutionalCode} onChange={(event) => setInstitutionalCode(event.target.value)} disabled={saving} placeholder="Ví dụ: CS101" autoFocus /><FieldError errors={fields.institutionalCode} /></label>
        <label><span>Tên học phần</span><input value={name} onChange={(event) => setName(event.target.value)} disabled={saving} placeholder="Nhập tên học phần" /><FieldError errors={fields.name} /></label>
        <label><span>Số tín chỉ</span><input type="number" min="1" max="30" value={credits} onChange={(event) => setCredits(event.target.value)} disabled={saving} /><FieldError errors={fields.credits} /></label>
        <label><span>Học kỳ</span><input value={semester} onChange={(event) => setSemester(event.target.value)} disabled={saving} placeholder="Ví dụ: Kỳ 1" /><FieldError errors={fields.semester} /></label>
        <label><span>Thứ tự hiển thị {editing ? "" : <small>(không bắt buộc)</small>}</span><input type="number" min="1" value={displayOrder} onChange={(event) => setDisplayOrder(event.target.value)} disabled={saving} required={editing} /><FieldError errors={fields.displayOrder} /></label>
        <div className="dialog__actions form-wide"><button className="button button--ghost" type="button" onClick={onClose} disabled={saving}>Hủy</button><button className="button button--primary" disabled={saving}>{saving ? "Đang lưu…" : "Lưu"}</button></div>
      </form>
    </Dialog>
  );
}

export function ProgramCoursesPage() {
  const { user } = useAuth();
  const { confirm, confirmationDialog } = useConfirmDialog();
  const { versionId: rawId } = useParams();
  const versionId = Number(rawId);
  const [version, setVersion] = useState<ProgramVersion | null>(null);
  const [program, setProgram] = useState<CurriculumProgram | null>(null);
  const [data, setData] = useState<PagedResult<ProgramCourse> | null>(null);
  const [page, setPage] = useState(1);
  const [searchInput, setSearchInput] = useState("");
  const [semesterInput, setSemesterInput] = useState("");
  const [search, setSearch] = useState("");
  const [semester, setSemester] = useState("");
  const [includeArchived, setIncludeArchived] = useState(false);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [dialog, setDialog] = useState<CourseDialogState | null>(null);

  const load = useCallback(async () => {
    if (!Number.isInteger(versionId) || versionId < 1) return;
    setLoading(true);
    setError("");
    try {
      const versionResult = await curriculumApi.getVersion(versionId);
      const [programResult, courseResult] = await Promise.all([
        curriculumApi.getProgram(versionResult.programId),
        curriculumApi.listCourses(versionId, page, pageSize, search, semester, includeArchived),
      ]);
      setVersion(versionResult);
      setProgram(programResult);
      setData(courseResult);
    } catch (caught) {
      setError(errorInfo(caught).message);
    } finally {
      setLoading(false);
    }
  }, [includeArchived, page, search, semester, versionId]);

  useEffect(() => { void load(); }, [load]);
  const editable = user?.role === "admin" && version?.status === "draft";

  async function archive(course: ProgramCourse) {
    const accepted = await confirm({
      title: "Lưu trữ học phần?",
      description: `Học phần “${course.name}” sẽ không còn xuất hiện trong danh sách đang hoạt động.`,
      confirmLabel: "Lưu trữ",
      tone: "danger",
    });
    if (!accepted) return;
    try { await curriculumApi.archiveCourse(versionId, course.id); await load(); }
    catch (caught) { setError(errorInfo(caught).message); }
  }

  if (!Number.isInteger(versionId) || versionId < 1) return <section className="admin-page"><Alert message="Mã phiên bản không hợp lệ." /></section>;

  return (
    <section className="admin-page">
      <div className="breadcrumbs"><Link to="/curriculum">Chương trình đào tạo</Link><span>/</span>{program && <Link to={`/curriculum/programs/${program.id}`}>{program.code}</Link>}<span>/</span><span>{version?.versionCode ?? "…"}</span></div>
      <PageHeader eyebrow={`${program?.code ?? "CTĐT"} · ${version?.versionCode ?? "Phiên bản"}`} title="Danh sách học phần" description={editable ? "Phiên bản đang ở trạng thái bản nháp và có thể chỉnh sửa." : "Phiên bản chỉ đọc; các thao tác thay đổi học phần đã được khóa."} actions={<><Link className="button button--secondary" to={`/curriculum/versions/${versionId}/plos`}>Chuẩn đầu ra</Link>{user?.role === "admin" && <button className="button button--primary" disabled={!editable} onClick={() => setDialog({ mode: "create" })}>+ Thêm học phần</button>}</>} />
      {version && <div className="summary-strip"><div><span>Trạng thái</span><strong className={`status status--${version.status}`}>{statusLabel(version.status)}</strong></div><div><span>Tổng học phần</span><strong>{data?.totalItems ?? version.courseCount}</strong></div><div><span>Công bố</span><strong>{formatDate(version.publishedAt)}</strong></div></div>}
      <div className="toolbar">
        <form className="search-box search-box--courses" onSubmit={(event) => { event.preventDefault(); setPage(1); setSearch(searchInput.trim()); setSemester(semesterInput.trim()); }}>
          <input value={searchInput} onChange={(event) => setSearchInput(event.target.value)} placeholder="Tìm mã hoặc tên học phần" aria-label="Tìm học phần" />
          <input value={semesterInput} onChange={(event) => setSemesterInput(event.target.value)} placeholder="Học kỳ, ví dụ Kỳ 1" aria-label="Lọc học kỳ" />
          <button className="button button--secondary">Lọc</button>
        </form>
        <label className="check-control"><input type="checkbox" checked={includeArchived} onChange={(event) => { setPage(1); setIncludeArchived(event.target.checked); }} /> Hiện học phần lưu trữ</label>
      </div>
      {error && <Alert message={error} onRetry={() => void load()} />}
      {loading ? <LoadingState /> : !data?.items.length ? <EmptyState>Không có học phần phù hợp.</EmptyState> : <>
        <div className="data-table-wrap"><table className="data-table"><thead><tr><th>STT</th><th>Mã học phần</th><th>Tên học phần</th><th>Tín chỉ</th><th>Học kỳ</th><th>Trạng thái</th><th className="align-right">Thao tác</th></tr></thead><tbody>{data.items.map((course) => {
          const archived = Boolean(course.archivedAt) || course.status === "archived";
          return <tr key={course.id} className={archived ? "row--archived" : ""}><td>{course.displayOrder}</td><td>{course.institutionalCode ?? "—"}</td><td><strong>{course.name}</strong></td><td>{course.credits}</td><td>{course.semester ?? "—"}</td><td><span className={`status status--${archived ? "archived" : "active"}`}>{archived ? "Đã lưu trữ" : "Đang sử dụng"}</span></td><td><div className="table-actions"><Link className="button button--small button--secondary" to={`/curriculum/versions/${versionId}/courses/${course.id}/outcomes`}>CLO/PLO</Link>{editable && !archived ? <><button className="button button--small button--ghost" onClick={() => setDialog({ mode: "edit", course })}>Sửa</button><button className="button button--small button--danger" onClick={() => void archive(course)}>Lưu trữ</button></> : <span className="muted-text">Chỉ đọc</span>}</div></td></tr>;
        })}</tbody></table></div>
        <Pagination page={data.page} totalPages={data.totalPages} totalItems={data.totalItems} onChange={setPage} />
      </>}
      {confirmationDialog}
      {dialog && <CourseForm state={dialog} versionId={versionId} onClose={() => setDialog(null)} onSaved={() => { setDialog(null); void load(); }} />}
    </section>
  );
}
