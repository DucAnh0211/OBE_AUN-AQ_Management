import { useCallback, useEffect, useMemo, useState, type FormEvent } from "react";
import { Link, useNavigate } from "react-router-dom";
import {
  accreditationApi,
  type CreateFrameworkInput,
  type FrameworkImportInput,
  type FrameworkStatus,
  type FrameworkSummary,
  type ImportValidationResult,
} from "../../../../modules/sv4/accreditation/frontend";
import { useAuth } from "../auth/AuthContext";
import { DialogFrame, useConfirmDialog } from "../components/Dialogs";

const statusLabels: Record<FrameworkStatus, string> = {
  draft: "Bản nháp",
  published: "Đã công bố",
  retired: "Đã ngừng sử dụng",
};

function formatDate(value: string | null) {
  return value
    ? new Intl.DateTimeFormat("vi-VN", { dateStyle: "short" }).format(new Date(value))
    : "—";
}

function messageOf(error: unknown, fallback: string) {
  return error instanceof Error ? error.message : fallback;
}

function CreateFrameworkDialog({ onClose, onCreated }: {
  onClose: () => void;
  onCreated: (framework: FrameworkSummary) => void;
}) {
  const [code, setCode] = useState("AUN-QA-PROGRAMME");
  const [version, setVersion] = useState("");
  const [defaultLanguage, setDefaultLanguage] = useState("vi");
  const [sourceTitle, setSourceTitle] = useState("");
  const [sourceUrl, setSourceUrl] = useState("");
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");

  async function submit(event: FormEvent) {
    event.preventDefault();
    setBusy(true); setError("");
    const input: CreateFrameworkInput = {
      code: code.trim(),
      version: version.trim(),
      assessmentLevel: "programme",
      defaultLanguage,
      sourceTitle: sourceTitle.trim(),
      sourceUrl: sourceUrl.trim() || null,
    };
    try { onCreated(await accreditationApi.createFramework(input)); }
    catch (caught) { setError(messageOf(caught, "Không thể tạo phiên bản bộ tiêu chí.")); }
    finally { setBusy(false); }
  }

  return <DialogFrame
    title="Tạo phiên bản bộ tiêu chí"
    description="Phiên bản mới được tạo ở trạng thái bản nháp và chưa có cấu trúc tiêu chí."
    onClose={onClose}
  >
    <form className="admin-form admin-form--grid" onSubmit={submit}>
      {error && <div className="alert alert--error form-wide" role="alert">{error}</div>}
      <label><span>Mã bộ tiêu chí</span><input required autoFocus value={code} onChange={(event) => setCode(event.target.value)} /></label>
      <label><span>Phiên bản</span><input required value={version} onChange={(event) => setVersion(event.target.value)} placeholder="Ví dụ: 5.0" /></label>
      <label><span>Ngôn ngữ mặc định</span><select value={defaultLanguage} onChange={(event) => setDefaultLanguage(event.target.value)}><option value="vi">Tiếng Việt</option><option value="en">English</option></select></label>
      <label><span>Cấp đánh giá</span><input value="Cấp chương trình" disabled /></label>
      <label className="form-wide"><span>Tên tài liệu nguồn</span><input required value={sourceTitle} onChange={(event) => setSourceTitle(event.target.value)} placeholder="Tên hướng dẫn hoặc tài liệu ban hành" /></label>
      <label className="form-wide"><span>Liên kết tài liệu nguồn</span><input type="url" value={sourceUrl} onChange={(event) => setSourceUrl(event.target.value)} placeholder="https://…" /></label>
      <div className="dialog__actions form-wide"><button className="button button--ghost" type="button" onClick={onClose} disabled={busy}>Hủy</button><button className="button button--primary" disabled={busy}>{busy ? "Đang tạo…" : "Tạo phiên bản"}</button></div>
    </form>
  </DialogFrame>;
}

function ImportFrameworkDialog({ onClose, onImported }: {
  onClose: () => void;
  onImported: (framework: FrameworkSummary) => void;
}) {
  const [raw, setRaw] = useState("");
  const [preview, setPreview] = useState<ImportValidationResult | null>(null);
  const [parsed, setParsed] = useState<FrameworkImportInput | null>(null);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");

  function changeRaw(value: string) {
    setRaw(value); setPreview(null); setParsed(null); setError("");
  }

  async function readFile(file?: File) {
    if (!file) return;
    try { changeRaw(await file.text()); }
    catch { setError("Không thể đọc tệp JSON đã chọn."); }
  }

  async function previewImport() {
    setError("");
    let input: FrameworkImportInput;
    try { input = JSON.parse(raw) as FrameworkImportInput; }
    catch { setError("Nội dung không phải JSON hợp lệ."); return; }
    setBusy(true);
    try {
      const result = await accreditationApi.previewImport(input);
      setParsed(input); setPreview(result);
    } catch (caught) { setError(messageOf(caught, "Không thể kiểm tra dữ liệu import.")); }
    finally { setBusy(false); }
  }

  async function importFramework() {
    if (!parsed || !preview?.valid) return;
    setBusy(true); setError("");
    try { onImported(await accreditationApi.importFramework(parsed)); }
    catch (caught) { setError(messageOf(caught, "Không thể nhập bộ tiêu chí.")); }
    finally { setBusy(false); }
  }

  const errorItems = preview ? Object.entries(preview.errors) : [];
  return <DialogFrame
    title="Nhập bộ tiêu chí từ JSON"
    description="Kiểm tra cấu trúc trước khi tạo bản nháp. Dữ liệu chỉ được nhập khi không còn lỗi."
    onClose={onClose}
    className="framework-import-dialog"
  >
    <div className="admin-form">
      {error && <div className="alert alert--error" role="alert">{error}</div>}
      <label className="file-control"><span>Chọn tệp JSON</span><input type="file" accept="application/json,.json" onChange={(event) => void readFile(event.target.files?.[0])} /></label>
      <label><span>Nội dung JSON</span><textarea className="json-editor" value={raw} onChange={(event) => changeRaw(event.target.value)} placeholder="Dán nội dung bộ tiêu chí tại đây…" spellCheck={false} /></label>
      {preview && <div className={`import-preview ${preview.valid ? "import-preview--valid" : "import-preview--invalid"}`} role="status">
        <div><span>Kết quả kiểm tra</span><strong>{preview.valid ? "Dữ liệu hợp lệ" : "Cần chỉnh sửa"}</strong></div>
        <div><span>Tiêu chuẩn</span><strong>{preview.criterionCount}</strong></div>
        <div><span>Yêu cầu</span><strong>{preview.requirementCount}</strong></div>
      </div>}
      {!!errorItems.length && <div className="import-errors">{errorItems.map(([field, messages]) => <div key={field}><code>{field}</code><span>{messages.join(" ")}</span></div>)}</div>}
      <div className="dialog__actions"><button className="button button--ghost" type="button" onClick={onClose} disabled={busy}>Hủy</button><button className="button button--secondary" type="button" onClick={() => void previewImport()} disabled={busy || !raw.trim()}>Kiểm tra dữ liệu</button><button className="button button--primary" type="button" onClick={() => void importFramework()} disabled={busy || !preview?.valid}>{busy ? "Đang xử lý…" : "Nhập bộ tiêu chí"}</button></div>
    </div>
  </DialogFrame>;
}

export function AccreditationFrameworksPage() {
  const { user } = useAuth();
  const isAdmin = user?.role === "admin";
  const navigate = useNavigate();
  const [frameworks, setFrameworks] = useState<FrameworkSummary[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [searchInput, setSearchInput] = useState("");
  const [search, setSearch] = useState("");
  const [status, setStatus] = useState("");
  const [dialog, setDialog] = useState<"create" | "import" | null>(null);
  const [busyId, setBusyId] = useState<number | null>(null);
  const { confirm, confirmationDialog } = useConfirmDialog();

  const load = useCallback(async () => {
    setLoading(true); setError("");
    try { setFrameworks(await accreditationApi.listFrameworks(isAdmin ? status : "published", search)); }
    catch (caught) { setError(messageOf(caught, "Không thể tải danh sách bộ tiêu chí AUN-QA.")); }
    finally { setLoading(false); }
  }, [isAdmin, search, status]);

  useEffect(() => { void load(); }, [load]);

  const totals = useMemo(() => ({
    published: frameworks.filter((item) => item.status === "published").length,
    draft: frameworks.filter((item) => item.status === "draft").length,
  }), [frameworks]);

  async function remove(framework: FrameworkSummary) {
    const accepted = await confirm({
      title: `Xóa bản nháp ${framework.code} ${framework.version}?`,
      description: "Toàn bộ tiêu chuẩn và yêu cầu thuộc bản nháp này sẽ bị xóa. Thao tác không thể hoàn tác.",
      confirmLabel: "Xóa bản nháp",
      tone: "danger",
    });
    if (!accepted) return;
    setBusyId(framework.id);
    try { await accreditationApi.deleteFramework(framework.id); setSuccess("Đã xóa bản nháp."); await load(); }
    catch (caught) { setError(messageOf(caught, "Không thể xóa bản nháp.")); }
    finally { setBusyId(null); }
  }

  async function retire(framework: FrameworkSummary) {
    const accepted = await confirm({
      title: `Ngừng sử dụng phiên bản ${framework.version}?`,
      description: "Giảng viên sẽ không còn nhìn thấy phiên bản này trong danh sách đang sử dụng.",
      confirmLabel: "Ngừng sử dụng",
      tone: "danger",
    });
    if (!accepted) return;
    setBusyId(framework.id);
    try { await accreditationApi.retireFramework(framework.id); setSuccess("Đã chuyển phiên bản sang trạng thái ngừng sử dụng."); await load(); }
    catch (caught) { setError(messageOf(caught, "Không thể ngừng sử dụng phiên bản.")); }
    finally { setBusyId(null); }
  }

  return <section className="admin-page">
    <header className="workspace-heading">
      <div><span className="eyebrow">KIỂM ĐỊNH AUN-QA</span><h1>Bộ tiêu chí AUN-QA</h1><p>Quản lý phiên bản khung đánh giá, tiêu chuẩn và các yêu cầu ở cấp chương trình.</p></div>
      {isAdmin && <div className="admin-heading__actions"><button className="button button--ghost" type="button" onClick={() => setDialog("import")}>Nhập JSON</button><button className="button button--primary" type="button" onClick={() => setDialog("create")}>Tạo phiên bản</button></div>}
    </header>

    {error && <div className="alert alert--error" role="alert"><span>{error}</span><button className="button button--small button--ghost" type="button" onClick={() => void load()}>Thử lại</button></div>}
    {success && <div className="alert alert--success" role="status">{success}</div>}

    <div className="framework-overview" aria-label="Tổng quan bộ tiêu chí">
      <div><span>Phiên bản hiển thị</span><strong>{frameworks.length}</strong></div>
      <div><span>Đã công bố</span><strong>{totals.published}</strong></div>
      {isAdmin && <div><span>Bản nháp</span><strong>{totals.draft}</strong></div>}
      <div><span>Phạm vi</span><strong>Cấp chương trình</strong></div>
    </div>

    <div className="toolbar">
      <form className="search-box" onSubmit={(event) => { event.preventDefault(); setSearch(searchInput.trim()); }}>
        <input value={searchInput} onChange={(event) => setSearchInput(event.target.value)} placeholder="Tìm theo mã, phiên bản hoặc nguồn" aria-label="Tìm bộ tiêu chí" />
        <button className="button button--secondary">Tìm kiếm</button>
      </form>
      {isAdmin && <label className="filter-control"><span>Trạng thái</span><select value={status} onChange={(event) => setStatus(event.target.value)}><option value="">Tất cả</option><option value="draft">Bản nháp</option><option value="published">Đã công bố</option><option value="retired">Đã ngừng sử dụng</option></select></label>}
    </div>

    {loading ? <div className="empty-state" aria-live="polite">Đang tải bộ tiêu chí AUN-QA…</div> : !frameworks.length ? <div className="empty-state"><strong>{search || status ? "Không tìm thấy phiên bản phù hợp" : "Chưa có bộ tiêu chí AUN-QA"}</strong><span>{isAdmin ? "Tạo phiên bản mới hoặc nhập cấu trúc từ JSON." : "Hiện chưa có phiên bản đã công bố."}</span></div> : <div className="data-table-wrap"><table className="data-table framework-table">
      <thead><tr><th>Bộ tiêu chí</th><th>Trạng thái</th><th>Cấu trúc</th><th>Ngôn ngữ</th><th>Nguồn</th><th className="align-right">Thao tác</th></tr></thead>
      <tbody>{frameworks.map((framework) => <tr key={framework.id}>
        <td><Link className="table-link" to={`/accreditation/frameworks/${framework.id}`}>{framework.code} · {framework.version}</Link><div className="muted-text">Cấp chương trình · Công bố {formatDate(framework.publishedAt)}</div></td>
        <td><span className={`status status--framework-${framework.status}`}>{statusLabels[framework.status]}</span></td>
        <td><strong>{framework.criterionCount}</strong> tiêu chuẩn<div className="muted-text">{framework.requirementCount} yêu cầu</div></td>
        <td>{framework.defaultLanguage === "vi" ? "Tiếng Việt" : "English"}</td>
        <td><span className="source-title">{framework.sourceTitle}</span>{framework.sourceUrl && <a className="source-link" href={framework.sourceUrl} target="_blank" rel="noreferrer">Mở nguồn ↗</a>}</td>
        <td><div className="table-actions"><Link className="button button--small button--secondary" to={`/accreditation/frameworks/${framework.id}`}>Xem cấu trúc</Link>{isAdmin && framework.status === "draft" && <button className="button button--small button--danger" type="button" disabled={busyId === framework.id} onClick={() => void remove(framework)}>Xóa</button>}{isAdmin && framework.status === "published" && <button className="button button--small button--danger" type="button" disabled={busyId === framework.id} onClick={() => void retire(framework)}>Ngừng dùng</button>}</div></td>
      </tr>)}</tbody>
    </table></div>}

    {dialog === "create" && <CreateFrameworkDialog onClose={() => setDialog(null)} onCreated={(framework) => { setDialog(null); navigate(`/accreditation/frameworks/${framework.id}`); }} />}
    {dialog === "import" && <ImportFrameworkDialog onClose={() => setDialog(null)} onImported={(framework) => { setDialog(null); navigate(`/accreditation/frameworks/${framework.id}`); }} />}
    {confirmationDialog}
  </section>;
}
