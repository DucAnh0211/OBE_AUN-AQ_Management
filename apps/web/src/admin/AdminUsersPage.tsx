import { useCallback, useEffect, useMemo, useState, type FormEvent } from "react";
import { curriculumApi } from "../../../../modules/common/curriculum/frontend/api";
import { apiJson, type AuthUser, type UserRole } from "../../../../shared/frontend/auth-client";
import { DialogFrame, useConfirmDialog, useInputDialog } from "../components/Dialogs";

type UserPage = { items: AuthUser[]; page: number; pageSize: number; totalItems: number; totalPages: number };
type AssignmentOption = { id: number; label: string; meta: string };
type AssignmentState = {
  user: AuthUser;
  options: AssignmentOption[];
  selected: number[];
  query: string;
  loading: boolean;
  saving: boolean;
  error: string;
};

const roleLabels: Record<UserRole, string> = { admin: "Quản trị viên", lecturer: "Giảng viên", student: "Sinh viên" };

async function loadAssignmentOptions(user: AuthUser): Promise<{ current: number[]; options: AssignmentOption[] }> {
  const kind = user.role === "lecturer" ? "course" : "program";
  const [current, programPage] = await Promise.all([
    apiJson<number[]>(`/api/admin/users/${user.id}/${kind}-assignments`),
    curriculumApi.listPrograms(1, 100, "", false),
  ]);
  if (user.role === "student") {
    return { current, options: programPage.items.map((program) => ({ id: program.id, label: `${program.code} · ${program.name}`, meta: "Chương trình đào tạo" })) };
  }
  const versionGroups = await Promise.all(programPage.items.map(async (program) => ({
    program,
    versions: await curriculumApi.listVersions(program.id, false),
  })));
  const courseGroups = await Promise.all(versionGroups.flatMap(({ program, versions }) =>
    versions.map(async (version) => ({
      program,
      version,
      courses: (await curriculumApi.listCourses(version.id, 1, 100, "", "", false)).items,
    }))));
  return {
    current,
    options: courseGroups.flatMap(({ program, version, courses }) => courses.map((course) => ({
      id: course.id,
      label: `${course.institutionalCode ?? "—"} · ${course.name}`,
      meta: `${program.code} / ${version.versionCode}`,
    }))),
  };
}

export function AdminUsersPage() {
  const [data, setData] = useState<UserPage | null>(null);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [search, setSearch] = useState("");
  const [showCreate, setShowCreate] = useState(false);
  const [email, setEmail] = useState("");
  const [fullName, setFullName] = useState("");
  const [role, setRole] = useState<UserRole>("lecturer");
  const [password, setPassword] = useState("");
  const [busy, setBusy] = useState(false);
  const [assignment, setAssignment] = useState<AssignmentState | null>(null);
  const { confirm, confirmationDialog } = useConfirmDialog();
  const { requestInput, inputDialog } = useInputDialog();

  const load = useCallback(async () => {
    try { setData(await apiJson<UserPage>("/api/admin/users?page=1&pageSize=100")); setError(""); }
    catch (caught) { setError(caught instanceof Error ? caught.message : "Không thể tải danh sách người dùng. Thử lại sau."); }
  }, []);
  useEffect(() => { void load(); }, [load]);

  const users = useMemo(() => {
    const value = search.trim().toLocaleLowerCase("vi");
    return value ? (data?.items ?? []).filter((user) => `${user.fullName} ${user.email} ${roleLabels[user.role]}`.toLocaleLowerCase("vi").includes(value)) : data?.items ?? [];
  }, [data, search]);

  async function create(event: FormEvent) {
    event.preventDefault(); setBusy(true); setError(""); setSuccess("");
    try {
      await apiJson("/api/admin/users", { method: "POST", body: JSON.stringify({ email, fullName, role, temporaryPassword: password }) });
      setEmail(""); setFullName(""); setPassword(""); setShowCreate(false);
      setSuccess(`Đã tạo tài khoản cho ${fullName}.`); await load();
    } catch (caught) { setError(caught instanceof Error ? caught.message : "Không thể tạo tài khoản."); }
    finally { setBusy(false); }
  }

  async function update(user: AuthUser, patch: Partial<Pick<AuthUser, "fullName" | "role" | "status">>) {
    setError(""); setSuccess("");
    try {
      await apiJson(`/api/admin/users/${user.id}`, { method: "PUT", body: JSON.stringify({ fullName: patch.fullName ?? user.fullName, role: patch.role ?? user.role, status: patch.status ?? user.status }) });
      setSuccess(`Đã cập nhật tài khoản ${user.email}.`); await load();
    } catch (caught) { setError(caught instanceof Error ? caught.message : "Không thể cập nhật tài khoản."); }
  }

  async function reset(user: AuthUser) {
    const temporaryPassword = await requestInput({
      title: "Đặt lại mật khẩu",
      description: `Tất cả phiên đăng nhập của ${user.email} sẽ bị thu hồi.`,
      label: "Mật khẩu tạm mới",
      inputType: "password",
      minimumLength: 10,
      placeholder: "Tối thiểu 10 ký tự",
      submitLabel: "Đặt lại mật khẩu",
    });
    if (!temporaryPassword) return;
    try {
      await apiJson(`/api/admin/users/${user.id}/reset-password`, { method: "POST", body: JSON.stringify({ temporaryPassword }) });
      setSuccess(`Đã đặt mật khẩu tạm cho ${user.email} và thu hồi các phiên cũ.`);
    } catch (caught) { setError(caught instanceof Error ? caught.message : "Không thể đặt lại mật khẩu."); }
  }

  async function changeStatus(user: AuthUser) {
    const disabling = user.status === "active";
    const accepted = await confirm({
      title: disabling ? "Khóa tài khoản" : "Mở khóa tài khoản",
      description: disabling
        ? `${user.fullName} sẽ mất quyền truy cập ngay và các phiên hiện tại bị thu hồi.`
        : `${user.fullName} có thể đăng nhập lại bằng mật khẩu hiện tại.`,
      confirmLabel: disabling ? "Khóa tài khoản" : "Mở khóa",
      tone: disabling ? "danger" : "primary",
    });
    if (accepted) await update(user, { status: disabling ? "disabled" : "active" });
  }

  async function openAssignments(user: AuthUser) {
    setAssignment({ user, options: [], selected: [], query: "", loading: true, saving: false, error: "" });
    try {
      const result = await loadAssignmentOptions(user);
      setAssignment((current) => current ? { ...current, options: result.options, selected: result.current, loading: false } : null);
    } catch (caught) {
      setAssignment((current) => current ? { ...current, loading: false, error: caught instanceof Error ? caught.message : "Không thể tải dữ liệu phân công." } : null);
    }
  }

  async function saveAssignments() {
    if (!assignment) return;
    const kind = assignment.user.role === "lecturer" ? "course" : "program";
    setAssignment({ ...assignment, saving: true, error: "" });
    try {
      await apiJson(`/api/admin/users/${assignment.user.id}/${kind}-assignments`, { method: "PUT", body: JSON.stringify({ ids: assignment.selected }) });
      setSuccess(`Đã cập nhật phân công cho ${assignment.user.fullName}.`); setAssignment(null);
    } catch (caught) {
      setAssignment({ ...assignment, saving: false, error: caught instanceof Error ? caught.message : "Không thể lưu phân công." });
    }
  }

  const filteredOptions = assignment?.options.filter((option) => `${option.label} ${option.meta}`.toLocaleLowerCase("vi").includes(assignment.query.toLocaleLowerCase("vi"))) ?? [];

  return <section className="admin-page">
    <header className="workspace-heading"><div><span className="eyebrow">QUẢN TRỊ HỆ THỐNG</span><h1>Tài khoản và phân quyền</h1><p>Quản lý trạng thái tài khoản và phạm vi dữ liệu của từng người dùng.</p></div>
      <button className="button button--primary" type="button" onClick={() => setShowCreate((value) => !value)}>{showCreate ? "Đóng biểu mẫu" : "Tạo tài khoản"}</button>
    </header>

    {error && <div className="alert alert--error" role="alert">{error}</div>}
    {success && <div className="alert alert--success" role="status">{success}</div>}

    {showCreate && <section className="form-panel"><div className="form-panel__heading"><h2>Tài khoản mới</h2><p>Người dùng phải đổi mật khẩu tạm ở lần đăng nhập đầu tiên.</p></div>
      <form className="admin-form admin-form--grid" onSubmit={create}>
        <label><span>Họ và tên</span><input required value={fullName} onChange={(event) => setFullName(event.target.value)} /></label>
        <label><span>Email</span><input type="email" required value={email} onChange={(event) => setEmail(event.target.value)} /></label>
        <label><span>Vai trò</span><select value={role} onChange={(event) => setRole(event.target.value as UserRole)}><option value="lecturer">Giảng viên</option><option value="student">Sinh viên</option><option value="admin">Quản trị viên</option></select></label>
        <label><span>Mật khẩu tạm</span><input type="password" minLength={10} required value={password} onChange={(event) => setPassword(event.target.value)} placeholder="Tối thiểu 10 ký tự" /></label>
        <div className="dialog__actions form-wide"><button className="button button--ghost" type="button" onClick={() => setShowCreate(false)}>Hủy</button><button className="button button--primary" disabled={busy}>{busy ? "Đang tạo…" : "Tạo tài khoản"}</button></div>
      </form>
    </section>}

    <div className="list-toolbar"><div><strong>Danh sách người dùng</strong><span>{data?.totalItems ?? 0} tài khoản</span></div><input value={search} onChange={(event) => setSearch(event.target.value)} placeholder="Tìm họ tên, email hoặc vai trò" aria-label="Tìm người dùng" /></div>
    <div className="data-table-wrap"><table className="data-table"><thead><tr><th>Người dùng</th><th>Vai trò</th><th>Trạng thái</th><th>Mật khẩu</th><th className="align-right">Thao tác</th></tr></thead><tbody>
      {users.map((user) => <tr key={user.id}><td><strong>{user.fullName}</strong><div className="muted-text">{user.email}</div></td><td><select aria-label={`Vai trò của ${user.fullName}`} value={user.role} onChange={(event) => void update(user, { role: event.target.value as UserRole })}><option value="admin">Quản trị viên</option><option value="lecturer">Giảng viên</option><option value="student">Sinh viên</option></select></td><td><span className={`status status--${user.status}`}>{user.status === "active" ? "Hoạt động" : "Đã khóa"}</span></td><td>{user.mustChangePassword ? <span className="status status--draft">Chờ đổi mật khẩu</span> : <span className="muted-text">Đã thiết lập</span>}</td><td><div className="table-actions">{user.role !== "admin" && <button className="button button--small button--ghost" type="button" onClick={() => void openAssignments(user)}>Phân công</button>}<button className="button button--small button--ghost" type="button" onClick={() => void reset(user)}>Đặt lại mật khẩu</button><button className={`button button--small ${user.status === "active" ? "button--danger" : "button--secondary"}`} type="button" onClick={() => void changeStatus(user)}>{user.status === "active" ? "Khóa" : "Mở khóa"}</button></div></td></tr>)}
    </tbody></table>{!users.length && <div className="table-empty">{search ? "Không tìm thấy tài khoản phù hợp." : "Chưa có tài khoản."}</div>}</div>

    {assignment && <DialogFrame title={assignment.user.role === "lecturer" ? "Phân công học phần" : "Gán chương trình đào tạo"} description={`${assignment.user.fullName} · ${assignment.user.email}`} onClose={() => setAssignment(null)}>
      {assignment.error && <div className="alert alert--error" role="alert">{assignment.error}</div>}
      {assignment.loading ? <div className="compact-loading">Đang tải dữ liệu phân công…</div> : <>
        <div className="assignment-toolbar"><input value={assignment.query} onChange={(event) => setAssignment({ ...assignment, query: event.target.value })} placeholder="Tìm theo mã hoặc tên" aria-label="Tìm dữ liệu phân công" /><span>Đã chọn {assignment.selected.length}</span></div>
        <div className="assignment-list">{filteredOptions.map((option) => <label key={option.id}><input type="checkbox" checked={assignment.selected.includes(option.id)} onChange={(event) => setAssignment({ ...assignment, selected: event.target.checked ? [...assignment.selected, option.id] : assignment.selected.filter((id) => id !== option.id) })} /><span><strong>{option.label}</strong><small>{option.meta}</small></span></label>)}{!filteredOptions.length && <div className="table-empty">Không có dữ liệu phù hợp.</div>}</div>
        <div className="dialog__actions"><button className="button button--ghost" type="button" onClick={() => setAssignment(null)}>Hủy</button><button className="button button--primary" type="button" disabled={assignment.saving} onClick={() => void saveAssignments()}>{assignment.saving ? "Đang lưu…" : "Lưu phân công"}</button></div>
      </>}
    </DialogFrame>}
    {confirmationDialog}
    {inputDialog}
  </section>;
}
