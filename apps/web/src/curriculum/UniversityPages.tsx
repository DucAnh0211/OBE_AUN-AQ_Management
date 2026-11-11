import { type FormEvent, useCallback, useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { curriculumApi } from "../../../../modules/common/curriculum/frontend/api";
import type { CurriculumProgram, PagedResult, University } from "../../../../modules/common/curriculum/frontend/types";
import { accreditationApi } from "../../../../modules/sv4/accreditation/frontend";
import type { DirectoryProgram, DirectoryUniversity } from "../../../../modules/sv4/accreditation/frontend/types";
import { useAuth } from "../auth/AuthContext";
import { useConfirmDialog } from "../components/Dialogs";
import { AccreditationStatusChip } from "./ProgramAccreditationPanel";
import { Alert, Dialog, EmptyState, errorInfo, FieldError, formatDay, LoadingState, PageHeader, Pagination } from "./ui";
import { DomainEyebrow, domainPage } from "../../../../modules/common/curriculum/frontend/components/Domain";
import { UniversityNav } from "../../../../modules/common/curriculum/frontend/components/VersionNav";

const pageSize = 10;

type UniversityDialogState = { mode: "create" } | { mode: "edit"; university: DirectoryUniversity };

function UniversityForm({ state, onClose, onSaved }: {
  state: UniversityDialogState;
  onClose: () => void;
  onSaved: () => void;
}) {
  const editing = state.mode === "edit";
  const [code, setCode] = useState(editing ? state.university.code : "");
  const [name, setName] = useState(editing ? state.university.name : "");
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");
  const [fields, setFields] = useState<Record<string, string[]>>({});

  async function submit(event: FormEvent) {
    event.preventDefault();
    setSaving(true);
    setError("");
    setFields({});
    try {
      if (editing) await curriculumApi.updateUniversity(state.university.id, name);
      else await curriculumApi.createUniversity(code, name);
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
    <Dialog title={editing ? "Cập nhật trường" : "Thêm trường đại học"} onClose={onClose}>
      <form className="admin-form" onSubmit={submit}>
        {error && <Alert message={error} />}
        <label>
          <span>Mã trường</span>
          <input value={code} onChange={(event) => setCode(event.target.value)} disabled={editing || saving} placeholder="Ví dụ: TNU" autoFocus={!editing} />
          <FieldError errors={fields.code} />
        </label>
        <label>
          <span>Tên trường</span>
          <input value={name} onChange={(event) => setName(event.target.value)} disabled={saving} placeholder="Ví dụ: Trường Đại học Tây Nguyên" autoFocus={editing} />
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

export function UniversitiesPage() {
  const { user } = useAuth();
  const isAdmin = user?.role === "admin";
  const { confirm, confirmationDialog } = useConfirmDialog();
  const [data, setData] = useState<PagedResult<DirectoryUniversity> | null>(null);
  const [page, setPage] = useState(1);
  const [searchInput, setSearchInput] = useState("");
  const [search, setSearch] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [dialog, setDialog] = useState<UniversityDialogState | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError("");
    try {
      setData(await accreditationApi.directoryUniversities(page, pageSize, search));
    } catch (caught) {
      setError(errorInfo(caught).message);
    } finally {
      setLoading(false);
    }
  }, [page, search]);

  useEffect(() => { void load(); }, [load]);

  async function archive(university: DirectoryUniversity) {
    const accepted = await confirm({
      title: "Lưu trữ trường?",
      description: `“${university.name}” sẽ không còn xuất hiện trong danh sách. Chỉ lưu trữ được khi trường không còn CTĐT đang hoạt động.`,
      confirmLabel: "Lưu trữ",
      tone: "danger",
    });
    if (!accepted) return;
    try {
      await curriculumApi.archiveUniversity(university.id);
      await load();
    } catch (caught) {
      setError(errorInfo(caught).message);
    }
  }

  return (
    <section className={domainPage("curriculum")}>
      <PageHeader
        title="Trường đại học"
        description="Chọn trường để xem các ngành đã được kiểm định AUN-QA và chương trình đào tạo của từng ngành."
        actions={isAdmin ? <button className="button button--primary" onClick={() => setDialog({ mode: "create" })}>Thêm trường</button> : undefined}
      />
      <div className="toolbar">
        <form className="search-box" onSubmit={(event) => { event.preventDefault(); setPage(1); setSearch(searchInput.trim()); }}>
          <input value={searchInput} onChange={(event) => setSearchInput(event.target.value)} placeholder="Tìm theo mã hoặc tên trường" aria-label="Tìm trường đại học" />
          <button className="button button--secondary">Tìm kiếm</button>
        </form>
      </div>

      {error && <Alert message={error} onRetry={() => void load()} />}
      {loading ? <LoadingState /> : !data?.items.length ? <EmptyState>Chưa có trường đại học phù hợp.</EmptyState> : (
        <>
          <div className="data-table-wrap">
            <table className="data-table">
              <thead><tr><th>Trường</th><th>Mã</th><th>Ngành đã kiểm định</th><th>Tổng số CTĐT</th><th className="align-right">Thao tác</th></tr></thead>
              <tbody>{data.items.map((university) => (
                <tr key={university.id}>
                  <td><Link className="table-link" to={`/curriculum/universities/${university.id}`}>{university.name}</Link></td>
                  <td>{university.code}</td>
                  <td>{university.accreditedProgramCount}</td>
                  <td>{university.programCount}</td>
                  <td><div className="table-actions">
                    <Link className="button button--small button--secondary" to={`/curriculum/universities/${university.id}`}>Xem ngành</Link>
                    {isAdmin && <>
                      <button className="button button--small" onClick={() => setDialog({ mode: "edit", university })}>Sửa</button>
                      <button className="button button--small button--danger" onClick={() => void archive(university)}>Lưu trữ</button>
                    </>}
                  </div></td>
                </tr>
              ))}</tbody>
            </table>
          </div>
          <Pagination page={data.page} totalPages={data.totalPages} totalItems={data.totalItems} onChange={setPage} />
        </>
      )}
      {confirmationDialog}
      {dialog && <UniversityForm state={dialog} onClose={() => setDialog(null)} onSaved={() => { setDialog(null); void load(); }} />}
    </section>
  );
}

type ProgramDialogState = { mode: "create" } | { mode: "edit"; program: CurriculumProgram };

function ProgramForm({ universityId, state, onClose, onSaved }: {
  universityId: number;
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
      else await curriculumApi.createProgram(universityId, code, name);
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
    <Dialog title={editing ? "Cập nhật chương trình đào tạo" : "Thêm chương trình đào tạo"} onClose={onClose}>
      <form className="admin-form" onSubmit={submit}>
        {error && <Alert message={error} />}
        <label>
          <span>Mã CTĐT</span>
          <input value={code} onChange={(event) => setCode(event.target.value)} disabled={editing || saving} placeholder="Ví dụ: CS" autoFocus={!editing} />
          <FieldError errors={fields.code} />
        </label>
        <label>
          <span>Tên ngành / chương trình đào tạo</span>
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

export function UniversityProgramsPage() {
  const { user } = useAuth();
  const isAdmin = user?.role === "admin";
  const { confirm, confirmationDialog } = useConfirmDialog();
  const { universityId: rawId } = useParams();
  const universityId = Number(rawId);
  const [university, setUniversity] = useState<University | null>(null);
  const [data, setData] = useState<PagedResult<DirectoryProgram> | null>(null);
  const [status, setStatus] = useState<"accredited" | "all">("accredited");
  const [page, setPage] = useState(1);
  const [searchInput, setSearchInput] = useState("");
  const [search, setSearch] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [dialog, setDialog] = useState<ProgramDialogState | null>(null);

  const load = useCallback(async () => {
    if (!Number.isInteger(universityId) || universityId < 1) return;
    setLoading(true);
    setError("");
    try {
      const [universityResult, programResult] = await Promise.all([
        curriculumApi.getUniversity(universityId),
        accreditationApi.directoryPrograms(universityId, status, page, pageSize, search),
      ]);
      setUniversity(universityResult);
      setData(programResult);
    } catch (caught) {
      setError(errorInfo(caught).message);
    } finally {
      setLoading(false);
    }
  }, [page, search, status, universityId]);

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

  if (!Number.isInteger(universityId) || universityId < 1) return <section className={domainPage("curriculum")}><Alert message="Mã trường không hợp lệ." /></section>;

  return (
    <section className={domainPage("curriculum")}>
      <div className="breadcrumbs"><Link to="/curriculum">Trường đại học</Link><span>/</span><span>{university?.name ?? "…"}</span></div>
      <PageHeader
        eyebrow={<DomainEyebrow domain="curriculum" detail={university?.code} />}
        title={university?.name ?? "Ngành của trường"}
        description="Chọn ngành để xem chương trình đào tạo và kết quả kiểm định."
        actions={isAdmin ? <button className="button button--primary" onClick={() => setDialog({ mode: "create" })}>Thêm CTĐT</button> : undefined}
        after={user?.role === "student" ? undefined : <UniversityNav universityId={universityId} isAdmin={isAdmin} />}
      />
      <div className="toolbar">
        <form className="search-box" onSubmit={(event) => { event.preventDefault(); setPage(1); setSearch(searchInput.trim()); }}>
          <input value={searchInput} onChange={(event) => setSearchInput(event.target.value)} placeholder="Tìm theo mã hoặc tên ngành" aria-label="Tìm ngành" />
          <button className="button button--secondary">Tìm kiếm</button>
        </form>
        <label className="filter-control">Hiển thị
          <select value={status} onChange={(event) => { setPage(1); setStatus(event.target.value as "accredited" | "all"); }}>
            <option value="accredited">Ngành đã kiểm định</option>
            <option value="all">Tất cả ngành</option>
          </select>
        </label>
      </div>

      {error && <Alert message={error} onRetry={() => void load()} />}
      {loading ? <LoadingState /> : !data?.items.length ? (
        <EmptyState>
          {status === "accredited" ? <>Trường này chưa có ngành được ghi nhận đã kiểm định. <button className="button button--small button--secondary" onClick={() => setStatus("all")}>Xem tất cả ngành</button></> : "Chưa có chương trình đào tạo phù hợp."}
        </EmptyState>
      ) : (
        <>
          <div className="data-table-wrap">
            <table className="data-table">
              <thead><tr><th>Mã</th><th>Ngành / CTĐT</th><th>Kiểm định</th><th>Bộ tiêu chí</th><th>Hiệu lực đến</th><th>Phiên bản hiện hành</th><th className="align-right">Thao tác</th></tr></thead>
              <tbody>{data.items.map(({ program, accreditation }) => (
                <tr key={program.id}>
                  <td><strong>{program.code}</strong></td>
                  <td><Link className="table-link" to={`/curriculum/programs/${program.id}`}>{program.name}</Link></td>
                  <td><AccreditationStatusChip status={accreditation?.effectiveStatus ?? null} /></td>
                  <td>{accreditation ? `${accreditation.frameworkCode} · ${accreditation.frameworkVersion}` : "—"}</td>
                  <td>{formatDay(accreditation?.validUntil ?? null)}</td>
                  <td>{program.currentVersionCode ?? "Chưa có"}</td>
                  <td><div className="table-actions">
                    <Link className="button button--small button--secondary" to={program.currentVersionId ? `/curriculum/versions/${program.currentVersionId}` : `/curriculum/programs/${program.id}`}>Xem CTĐT</Link>
                    {isAdmin && <>
                      <button className="button button--small" onClick={() => setDialog({ mode: "edit", program })}>Sửa</button>
                      <button className="button button--small button--danger" onClick={() => void archive(program)}>Lưu trữ</button>
                    </>}
                  </div></td>
                </tr>
              ))}</tbody>
            </table>
          </div>
          <Pagination page={data.page} totalPages={data.totalPages} totalItems={data.totalItems} onChange={setPage} />
        </>
      )}
      {confirmationDialog}
      {dialog && <ProgramForm universityId={universityId} state={dialog} onClose={() => setDialog(null)} onSaved={() => { setDialog(null); if (status === "all") void load(); else { setPage(1); setStatus("all"); } }} />}
    </section>
  );
}
