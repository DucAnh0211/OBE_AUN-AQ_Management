import { useCallback, useEffect, useMemo, useState, type FormEvent } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import {
  AccreditationApiError,
  accreditationApi,
  type CriterionTree,
  type FrameworkStatus,
  type FrameworkSummary,
  type FrameworkTree,
  type RequirementTree,
  type TranslationStatus,
} from "../../../../modules/sv4/accreditation/frontend";
import { useAuth } from "../auth/AuthContext";
import { DialogFrame, useConfirmDialog } from "../components/Dialogs";

const statusLabels: Record<FrameworkStatus, string> = {
  draft: "Bản nháp",
  published: "Đã công bố",
  retired: "Đã ngừng sử dụng",
};

const translationLabels: Record<string, string> = {
  official: "Bản chính thức",
  reviewed: "Đã rà soát",
  draft: "Bản tham khảo",
  machine_translated: "Dịch máy",
  fallback_vi: "Dùng bản tiếng Việt",
  fallback_en: "Dùng bản tiếng Anh",
  missing: "Thiếu nội dung",
};

function messageOf(error: unknown, fallback: string) {
  return error instanceof Error ? error.message : fallback;
}

function validTranslationStatus(value: string): TranslationStatus {
  return ["official", "reviewed", "draft", "machine_translated"].includes(value)
    ? value as TranslationStatus
    : "draft";
}

function FrameworkMetadataDialog({ framework, onClose, onSaved }: {
  framework: FrameworkSummary;
  onClose: () => void;
  onSaved: () => void;
}) {
  const [defaultLanguage, setDefaultLanguage] = useState(framework.defaultLanguage);
  const [sourceTitle, setSourceTitle] = useState(framework.sourceTitle);
  const [sourceUrl, setSourceUrl] = useState(framework.sourceUrl ?? "");
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");
  async function submit(event: FormEvent) {
    event.preventDefault(); setBusy(true); setError("");
    try {
      await accreditationApi.updateFramework(framework.id, {
        defaultLanguage,
        sourceTitle: sourceTitle.trim(),
        sourceUrl: sourceUrl.trim() || null,
      });
      onSaved();
    } catch (caught) { setError(messageOf(caught, "Không thể cập nhật thông tin phiên bản.")); }
    finally { setBusy(false); }
  }
  return <DialogFrame title="Cập nhật thông tin phiên bản" description="Chỉ bản nháp mới có thể thay đổi ngôn ngữ mặc định và tài liệu nguồn." onClose={onClose}>
    <form className="admin-form" onSubmit={submit}>
      {error && <div className="alert alert--error" role="alert">{error}</div>}
      <label><span>Ngôn ngữ mặc định</span><select autoFocus value={defaultLanguage} onChange={(event) => setDefaultLanguage(event.target.value)}><option value="vi">Tiếng Việt</option><option value="en">English</option></select></label>
      <label><span>Tên tài liệu nguồn</span><input required value={sourceTitle} onChange={(event) => setSourceTitle(event.target.value)} /></label>
      <label><span>Liên kết tài liệu nguồn</span><input type="url" value={sourceUrl} onChange={(event) => setSourceUrl(event.target.value)} placeholder="https://…" /></label>
      <div className="dialog__actions"><button className="button button--ghost" type="button" onClick={onClose} disabled={busy}>Hủy</button><button className="button button--primary" disabled={busy}>{busy ? "Đang lưu…" : "Lưu thay đổi"}</button></div>
    </form>
  </DialogFrame>;
}

type CriterionDraft = {
  code: string;
  displayOrder: string;
  title: string;
  translationStatus: TranslationStatus;
};

function CriterionEditor({ initial, language, structureEditable, suggestedOrder, busy, onClose, onSave }: {
  initial?: CriterionTree;
  language: string;
  structureEditable: boolean;
  suggestedOrder: number;
  busy: boolean;
  onClose: () => void;
  onSave: (draft: CriterionDraft) => void;
}) {
  const initialStatus = initial?.language === language ? validTranslationStatus(initial.translationStatus) : "draft";
  const [draft, setDraft] = useState<CriterionDraft>({
    code: initial?.code ?? String(suggestedOrder),
    displayOrder: String(initial?.displayOrder ?? suggestedOrder),
    title: initial?.title ?? "",
    translationStatus: initialStatus,
  });
  const creating = !initial;
  return <DialogFrame
    title={creating ? "Thêm tiêu chuẩn" : structureEditable ? `Cập nhật tiêu chuẩn ${initial.code}` : `Cập nhật bản dịch tiêu chuẩn ${initial.code}`}
    description={structureEditable ? "Mã và thứ tự quyết định vị trí trong cây bộ tiêu chí." : "Cấu trúc đã công bố chỉ cho phép cập nhật bản dịch tiếng Việt."}
    onClose={onClose}
  >
    <form className="admin-form admin-form--grid" onSubmit={(event) => { event.preventDefault(); onSave(draft); }}>
      <label><span>Mã tiêu chuẩn</span><input required autoFocus value={draft.code} disabled={!structureEditable} onChange={(event) => setDraft({ ...draft, code: event.target.value })} /></label>
      <label><span>Thứ tự hiển thị</span><input required min={1} type="number" value={draft.displayOrder} disabled={!structureEditable} onChange={(event) => setDraft({ ...draft, displayOrder: event.target.value })} /></label>
      <label className="form-wide"><span>Tiêu đề · {language === "vi" ? "Tiếng Việt" : "English"}</span><input required value={draft.title} onChange={(event) => setDraft({ ...draft, title: event.target.value })} /></label>
      <label className="form-wide"><span>Trạng thái bản dịch</span><select value={draft.translationStatus} onChange={(event) => setDraft({ ...draft, translationStatus: event.target.value as TranslationStatus })}>{language === "en" && <option value="official">Bản chính thức</option>}<option value="reviewed">Đã rà soát</option><option value="draft">Bản tham khảo</option><option value="machine_translated">Dịch máy</option></select></label>
      <div className="dialog__actions form-wide"><button className="button button--ghost" type="button" onClick={onClose} disabled={busy}>Hủy</button><button className="button button--primary" disabled={busy}>{busy ? "Đang lưu…" : creating ? "Thêm tiêu chuẩn" : "Lưu thay đổi"}</button></div>
    </form>
  </DialogFrame>;
}

type RequirementDraft = {
  code: string;
  displayOrder: string;
  sourcePage: string;
  statement: string;
  translationStatus: TranslationStatus;
};

function RequirementEditor({ criterion, initial, language, structureEditable, suggestedOrder, busy, onClose, onSave }: {
  criterion: CriterionTree;
  initial?: RequirementTree;
  language: string;
  structureEditable: boolean;
  suggestedOrder: number;
  busy: boolean;
  onClose: () => void;
  onSave: (draft: RequirementDraft) => void;
}) {
  const initialStatus = initial?.language === language ? validTranslationStatus(initial.translationStatus) : "draft";
  const [draft, setDraft] = useState<RequirementDraft>({
    code: initial?.code ?? `${criterion.code}.${suggestedOrder}`,
    displayOrder: String(initial?.displayOrder ?? suggestedOrder),
    sourcePage: initial?.sourcePage ? String(initial.sourcePage) : "",
    statement: initial?.statement ?? "",
    translationStatus: initialStatus,
  });
  const creating = !initial;
  return <DialogFrame
    title={creating ? `Thêm yêu cầu cho tiêu chuẩn ${criterion.code}` : structureEditable ? `Cập nhật yêu cầu ${initial.code}` : `Cập nhật bản dịch ${initial.code}`}
    description={structureEditable ? `Mã yêu cầu phải bắt đầu bằng “${criterion.code}.”.` : "Cấu trúc đã công bố chỉ cho phép cập nhật bản dịch tiếng Việt."}
    onClose={onClose}
    className="requirement-dialog"
  >
    <form className="admin-form admin-form--grid" onSubmit={(event) => { event.preventDefault(); onSave(draft); }}>
      <label><span>Mã yêu cầu</span><input required autoFocus pattern={`${criterion.code.replace(".", "\\.")}\\..+`} value={draft.code} disabled={!structureEditable} onChange={(event) => setDraft({ ...draft, code: event.target.value })} /></label>
      <label><span>Thứ tự hiển thị</span><input required min={1} type="number" value={draft.displayOrder} disabled={!structureEditable} onChange={(event) => setDraft({ ...draft, displayOrder: event.target.value })} /></label>
      <label><span>Trang tài liệu nguồn</span><input min={1} type="number" value={draft.sourcePage} disabled={!structureEditable} onChange={(event) => setDraft({ ...draft, sourcePage: event.target.value })} placeholder="Không bắt buộc" /></label>
      <label><span>Trạng thái bản dịch</span><select value={draft.translationStatus} onChange={(event) => setDraft({ ...draft, translationStatus: event.target.value as TranslationStatus })}>{language === "en" && <option value="official">Bản chính thức</option>}<option value="reviewed">Đã rà soát</option><option value="draft">Bản tham khảo</option><option value="machine_translated">Dịch máy</option></select></label>
      <label className="form-wide"><span>Nội dung yêu cầu · {language === "vi" ? "Tiếng Việt" : "English"}</span><textarea required value={draft.statement} onChange={(event) => setDraft({ ...draft, statement: event.target.value })} /></label>
      <div className="dialog__actions form-wide"><button className="button button--ghost" type="button" onClick={onClose} disabled={busy}>Hủy</button><button className="button button--primary" disabled={busy}>{busy ? "Đang lưu…" : creating ? "Thêm yêu cầu" : "Lưu thay đổi"}</button></div>
    </form>
  </DialogFrame>;
}

type CriterionEditorState = { criterion?: CriterionTree };
type RequirementEditorState = { criterion: CriterionTree; requirement?: RequirementTree };

export function AccreditationFrameworkDetailPage() {
  const { frameworkId: rawId } = useParams();
  const frameworkId = Number(rawId);
  const { user } = useAuth();
  const navigate = useNavigate();
  const isAdmin = user?.role === "admin";
  const [framework, setFramework] = useState<FrameworkSummary | null>(null);
  const [tree, setTree] = useState<FrameworkTree | null>(null);
  const [language, setLanguage] = useState("vi");
  const [query, setQuery] = useState("");
  const [expanded, setExpanded] = useState<Set<number>>(new Set());
  const [loading, setLoading] = useState(true);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [validationErrors, setValidationErrors] = useState<Record<string, string[]>>({});
  const [showMetadata, setShowMetadata] = useState(false);
  const [criterionEditor, setCriterionEditor] = useState<CriterionEditorState | null>(null);
  const [requirementEditor, setRequirementEditor] = useState<RequirementEditorState | null>(null);
  const { confirm, confirmationDialog } = useConfirmDialog();

  const load = useCallback(async () => {
    if (!Number.isInteger(frameworkId) || frameworkId < 1) {
      setError("Mã phiên bản bộ tiêu chí không hợp lệ."); setLoading(false); return;
    }
    setLoading(true); setError("");
    try {
      const [summary, nextTree] = await Promise.all([
        accreditationApi.getFramework(frameworkId),
        accreditationApi.getTree(frameworkId, language),
      ]);
      setFramework(summary); setTree(nextTree);
      setExpanded((current) => current.size || !nextTree.criteria.length ? current : new Set([nextTree.criteria[0].id]));
    } catch (caught) { setError(messageOf(caught, "Không thể tải cấu trúc bộ tiêu chí.")); }
    finally { setLoading(false); }
  }, [frameworkId, language]);

  useEffect(() => { void load(); }, [load]);

  const structureEditable = isAdmin && framework?.status === "draft";
  const translationEditable = isAdmin && (framework?.status === "draft" || language !== "en");
  const normalizedQuery = query.trim().toLocaleLowerCase(language === "vi" ? "vi" : "en");
  const visibleCriteria = useMemo(() => {
    if (!tree || !normalizedQuery) return tree?.criteria ?? [];
    return tree.criteria.flatMap((criterion) => {
      const criterionMatches = `${criterion.code} ${criterion.title}`.toLocaleLowerCase(language === "vi" ? "vi" : "en").includes(normalizedQuery);
      const requirements = criterionMatches
        ? criterion.requirements
        : criterion.requirements.filter((item) => `${item.code} ${item.statement}`.toLocaleLowerCase(language === "vi" ? "vi" : "en").includes(normalizedQuery));
      return criterionMatches || requirements.length ? [{ ...criterion, requirements }] : [];
    });
  }, [language, normalizedQuery, tree]);

  function setFailure(caught: unknown, fallback: string) {
    setError(messageOf(caught, fallback));
    setValidationErrors(caught instanceof AccreditationApiError ? caught.fieldErrors : {});
  }

  async function saveCriterion(draft: CriterionDraft) {
    if (!tree) return;
    const initial = criterionEditor?.criterion;
    setBusy(true); setError(""); setValidationErrors({});
    try {
      let criterionId = initial?.id;
      if (initial) {
        if (structureEditable) {
          await accreditationApi.updateCriterion(frameworkId, initial.id, {
            code: draft.code.trim(),
            displayOrder: Number(draft.displayOrder),
          });
        }
      } else {
        const created = await accreditationApi.addCriterion(frameworkId, {
          code: draft.code.trim(),
          displayOrder: Number(draft.displayOrder),
        });
        criterionId = created.id;
      }
      if (!criterionId) return;
      await accreditationApi.upsertCriterionTranslation(frameworkId, criterionId, language, {
        title: draft.title.trim(),
        status: draft.translationStatus,
        source: "Cập nhật thủ công trong hệ thống",
      });
      setCriterionEditor(null); setSuccess(initial ? "Đã cập nhật tiêu chuẩn." : "Đã thêm tiêu chuẩn."); await load();
    } catch (caught) { setFailure(caught, "Không thể lưu tiêu chuẩn."); }
    finally { setBusy(false); }
  }

  async function saveRequirement(draft: RequirementDraft) {
    const state = requirementEditor;
    if (!state) return;
    const initial = state.requirement;
    setBusy(true); setError(""); setValidationErrors({});
    try {
      let requirementId = initial?.id;
      const structure = {
        code: draft.code.trim(),
        displayOrder: Number(draft.displayOrder),
        sourcePage: draft.sourcePage ? Number(draft.sourcePage) : null,
      };
      if (initial) {
        if (structureEditable) await accreditationApi.updateRequirement(frameworkId, initial.id, structure);
      } else {
        const created = await accreditationApi.addRequirement(frameworkId, state.criterion.id, structure);
        requirementId = created.id;
      }
      if (!requirementId) return;
      await accreditationApi.upsertRequirementTranslation(frameworkId, requirementId, language, {
        statement: draft.statement.trim(),
        status: draft.translationStatus,
        source: "Cập nhật thủ công trong hệ thống",
      });
      setRequirementEditor(null); setSuccess(initial ? "Đã cập nhật yêu cầu." : "Đã thêm yêu cầu."); await load();
    } catch (caught) { setFailure(caught, "Không thể lưu yêu cầu."); }
    finally { setBusy(false); }
  }

  async function deleteCriterion(criterion: CriterionTree) {
    const accepted = await confirm({
      title: `Xóa tiêu chuẩn ${criterion.code}?`,
      description: `${criterion.requirements.length} yêu cầu thuộc tiêu chuẩn này cũng sẽ bị xóa. Thao tác không thể hoàn tác.`,
      confirmLabel: "Xóa tiêu chuẩn",
      tone: "danger",
    });
    if (!accepted) return;
    setBusy(true);
    try { await accreditationApi.deleteCriterion(frameworkId, criterion.id); setSuccess("Đã xóa tiêu chuẩn."); await load(); }
    catch (caught) { setFailure(caught, "Không thể xóa tiêu chuẩn."); }
    finally { setBusy(false); }
  }

  async function deleteRequirement(requirement: RequirementTree) {
    const accepted = await confirm({
      title: `Xóa yêu cầu ${requirement.code}?`,
      description: "Yêu cầu và các bản dịch liên quan sẽ bị xóa. Thao tác không thể hoàn tác.",
      confirmLabel: "Xóa yêu cầu",
      tone: "danger",
    });
    if (!accepted) return;
    setBusy(true);
    try { await accreditationApi.deleteRequirement(frameworkId, requirement.id); setSuccess("Đã xóa yêu cầu."); await load(); }
    catch (caught) { setFailure(caught, "Không thể xóa yêu cầu."); }
    finally { setBusy(false); }
  }

  async function publish() {
    if (!framework) return;
    const accepted = await confirm({
      title: `Công bố ${framework.code} ${framework.version}?`,
      description: "Hệ thống sẽ kiểm tra đủ 8 tiêu chuẩn và 53 yêu cầu. Sau khi công bố, cấu trúc và nội dung tiếng Anh chuyển sang chỉ đọc.",
      confirmLabel: "Công bố phiên bản",
    });
    if (!accepted) return;
    setBusy(true); setError(""); setValidationErrors({});
    try { await accreditationApi.publishFramework(framework.id); setSuccess("Đã công bố phiên bản bộ tiêu chí."); await load(); }
    catch (caught) { setFailure(caught, "Không thể công bố phiên bản."); }
    finally { setBusy(false); }
  }

  async function retire() {
    if (!framework) return;
    const accepted = await confirm({
      title: `Ngừng sử dụng phiên bản ${framework.version}?`,
      description: "Giảng viên sẽ không còn truy cập phiên bản này trong danh sách bộ tiêu chí đang sử dụng.",
      confirmLabel: "Ngừng sử dụng",
      tone: "danger",
    });
    if (!accepted) return;
    setBusy(true);
    try { await accreditationApi.retireFramework(framework.id); setSuccess("Đã ngừng sử dụng phiên bản."); await load(); }
    catch (caught) { setFailure(caught, "Không thể ngừng sử dụng phiên bản."); }
    finally { setBusy(false); }
  }

  function toggleCriterion(id: number) {
    setExpanded((current) => {
      const next = new Set(current);
      if (next.has(id)) next.delete(id); else next.add(id);
      return next;
    });
  }

  function toggleAll() {
    if (!tree) return;
    const allExpanded = visibleCriteria.every((item) => expanded.has(item.id));
    setExpanded(allExpanded ? new Set() : new Set(visibleCriteria.map((item) => item.id)));
  }

  if (loading) return <section className="admin-page"><div className="compact-loading">Đang tải cấu trúc bộ tiêu chí AUN-QA…</div></section>;
  if (!framework || !tree) return <section className="admin-page"><div className="alert alert--error" role="alert"><span>{error || "Không tìm thấy phiên bản bộ tiêu chí."}</span><button className="button button--small button--ghost" type="button" onClick={() => navigate("/accreditation")}>Về danh sách</button></div></section>;

  return <section className="admin-page">
    <div className="breadcrumbs"><Link to="/accreditation">Bộ tiêu chí AUN-QA</Link><span>/</span><span>{framework.code} · {framework.version}</span></div>
    <header className="workspace-heading">
      <div><span className="eyebrow">CẤU TRÚC BỘ TIÊU CHÍ</span><h1>{framework.code} · {framework.version}</h1><p>{framework.sourceTitle}</p></div>
      {isAdmin && <div className="admin-heading__actions">{structureEditable && <button className="button button--ghost" type="button" onClick={() => setShowMetadata(true)}>Thông tin phiên bản</button>}{structureEditable && <button className="button button--primary" type="button" disabled={busy} onClick={() => void publish()}>Công bố</button>}{framework.status === "published" && <button className="button button--danger" type="button" disabled={busy} onClick={() => void retire()}>Ngừng sử dụng</button>}</div>}
    </header>

    {error && <div className="alert alert--error" role="alert"><span>{error}</span><button className="button button--small button--ghost" type="button" onClick={() => void load()}>Tải lại</button></div>}
    {success && <div className="alert alert--success" role="status">{success}</div>}
    {!!Object.keys(validationErrors).length && <div className="framework-validation" role="alert"><strong>Phiên bản chưa đủ điều kiện công bố</strong>{Object.entries(validationErrors).map(([field, messages]) => <div key={field}><code>{field}</code><span>{messages.join(" ")}</span></div>)}</div>}

    <div className="framework-summary">
      <div><span>Trạng thái</span><strong><span className={`status status--framework-${framework.status}`}>{statusLabels[framework.status]}</span></strong></div>
      <div><span>Tiêu chuẩn</span><strong>{tree.criterionCount}</strong></div>
      <div><span>Yêu cầu</span><strong>{tree.requirementCount}</strong></div>
      <div><span>Ngôn ngữ mặc định</span><strong>{framework.defaultLanguage === "vi" ? "Tiếng Việt" : "English"}</strong></div>
    </div>

    {(!structureEditable || !isAdmin) && <div className="read-only-notice"><strong>Chế độ chỉ đọc cấu trúc</strong><span>{isAdmin && language === "vi" ? "Bạn vẫn có thể cập nhật bản dịch tiếng Việt của phiên bản này." : "Cấu trúc tiêu chuẩn và yêu cầu không thể chỉnh sửa ở trạng thái hiện tại."}</span></div>}

    <div className="framework-source"><div><span>Nguồn tham chiếu</span><strong>{framework.sourceTitle}</strong></div>{framework.sourceUrl && <a className="button button--small button--ghost" href={framework.sourceUrl} target="_blank" rel="noreferrer">Mở tài liệu nguồn ↗</a>}</div>

    <div className="criteria-toolbar">
      <label className="criteria-search"><span className="sr-only">Tìm trong bộ tiêu chí</span><input value={query} onChange={(event) => setQuery(event.target.value)} placeholder="Tìm mã, tiêu chuẩn hoặc nội dung yêu cầu" /></label>
      <label className="filter-control"><span>Ngôn ngữ</span><select value={language} onChange={(event) => { setLanguage(event.target.value); setSuccess(""); }}><option value="vi">Tiếng Việt</option><option value="en">English</option></select></label>
      <button className="button button--ghost" type="button" disabled={!!normalizedQuery} onClick={toggleAll}>{visibleCriteria.every((item) => expanded.has(item.id)) ? "Thu gọn tất cả" : "Mở rộng tất cả"}</button>
      {structureEditable && <button className="button button--primary" type="button" disabled={busy} onClick={() => setCriterionEditor({})}>Thêm tiêu chuẩn</button>}
    </div>

    {!visibleCriteria.length ? <div className="empty-state"><strong>Không tìm thấy nội dung phù hợp</strong><span>Thử tìm bằng mã như 2.4 hoặc một cụm từ trong yêu cầu.</span></div> : <div className="criteria-tree">
      {visibleCriteria.map((criterion) => {
        const open = !!normalizedQuery || expanded.has(criterion.id);
        return <article className="criterion-section" key={criterion.id}>
          <div className="criterion-section__heading">
            <button className="criterion-toggle" type="button" onClick={() => toggleCriterion(criterion.id)} aria-expanded={open} aria-controls={`criterion-${criterion.id}`}>
              <span className="criterion-code">{criterion.code}</span>
              <span className="criterion-title"><strong>{criterion.title}</strong><small>{criterion.requirements.length} yêu cầu · {translationLabels[criterion.translationStatus] ?? criterion.translationStatus}</small></span>
              <span className="criterion-chevron" aria-hidden="true">{open ? "−" : "+"}</span>
            </button>
            {translationEditable && <div className="criterion-actions"><button className="button button--small button--ghost" type="button" disabled={busy} onClick={() => setCriterionEditor({ criterion })}>{structureEditable ? "Cập nhật" : "Sửa bản dịch"}</button>{structureEditable && <><button className="button button--small button--secondary" type="button" disabled={busy} onClick={() => setRequirementEditor({ criterion })}>Thêm yêu cầu</button><button className="button button--small button--danger" type="button" disabled={busy} onClick={() => void deleteCriterion(criterion)}>Xóa</button></>}</div>}
          </div>
          {open && <div id={`criterion-${criterion.id}`} className="requirements-wrap"><table className="requirements-table">
            <thead><tr><th>Mã</th><th>Nội dung yêu cầu</th><th>Nguồn</th><th>Bản dịch</th>{translationEditable && <th className="align-right">Thao tác</th>}</tr></thead>
            <tbody>{criterion.requirements.map((requirement) => <tr key={requirement.id}>
              <td><strong>{requirement.code}</strong></td>
              <td><p>{requirement.statement}</p></td>
              <td>{requirement.sourcePage ? `Trang ${requirement.sourcePage}` : "—"}</td>
              <td><span className="translation-meta">{requirement.language.toUpperCase()} · {translationLabels[requirement.translationStatus] ?? requirement.translationStatus}</span></td>
              {translationEditable && <td><div className="table-actions"><button className="button button--small button--ghost" type="button" disabled={busy} onClick={() => setRequirementEditor({ criterion, requirement })}>{structureEditable ? "Cập nhật" : "Sửa bản dịch"}</button>{structureEditable && <button className="button button--small button--danger" type="button" disabled={busy} onClick={() => void deleteRequirement(requirement)}>Xóa</button>}</div></td>}
            </tr>)}{!criterion.requirements.length && <tr><td colSpan={translationEditable ? 5 : 4}><div className="table-empty">Tiêu chuẩn này chưa có yêu cầu đánh giá.</div></td></tr>}</tbody>
          </table></div>}
        </article>;
      })}
    </div>}

    {showMetadata && <FrameworkMetadataDialog framework={framework} onClose={() => setShowMetadata(false)} onSaved={() => { setShowMetadata(false); setSuccess("Đã cập nhật thông tin phiên bản."); void load(); }} />}
    {criterionEditor && <CriterionEditor initial={criterionEditor.criterion} language={language} structureEditable={structureEditable} suggestedOrder={(tree.criteria.at(-1)?.displayOrder ?? 0) + 1} busy={busy} onClose={() => setCriterionEditor(null)} onSave={(draft) => void saveCriterion(draft)} />}
    {requirementEditor && <RequirementEditor criterion={requirementEditor.criterion} initial={requirementEditor.requirement} language={language} structureEditable={structureEditable} suggestedOrder={(requirementEditor.criterion.requirements.at(-1)?.displayOrder ?? 0) + 1} busy={busy} onClose={() => setRequirementEditor(null)} onSave={(draft) => void saveRequirement(draft)} />}
    {confirmationDialog}
  </section>;
}
