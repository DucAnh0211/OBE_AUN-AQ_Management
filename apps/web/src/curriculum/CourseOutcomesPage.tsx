import { useCallback, useEffect, useState, type FormEvent } from "react";
import { Link, useParams } from "react-router-dom";
import { curriculumApi } from "../../../../modules/common/curriculum/frontend/api";
import type { Clo, CloPloMapping, CoursePloMapping, Plo, ProgramCourse, ProgramVersion } from "../../../../modules/common/curriculum/frontend/types";
import { useAuth } from "../auth/AuthContext";
import { DialogFrame, useConfirmDialog } from "../components/Dialogs";
import { VersionOutcomeNav } from "./OutcomeManagementPages";

type CloDraft = { code: string; statement: string; levelCode: string };

function CloEditor({ initial, busy, onClose, onSave }: {
  initial?: Clo;
  busy: boolean;
  onClose: () => void;
  onSave: (draft: CloDraft) => Promise<void>;
}) {
  const [draft, setDraft] = useState<CloDraft>({ code: initial?.code ?? "", statement: initial?.statement ?? "", levelCode: initial?.levelCode ?? "" });
  function submit(event: FormEvent) { event.preventDefault(); void onSave(draft); }
  return <DialogFrame title={initial ? `Cập nhật ${initial.code}` : "Thêm chuẩn đầu ra học phần"} description="CLO mô tả kết quả người học cần đạt sau khi hoàn thành học phần." onClose={onClose}>
    <form className="admin-form admin-form--grid" onSubmit={submit}>
      <label><span>Mã CLO</span><input required autoFocus value={draft.code} onChange={(event) => setDraft({ ...draft, code: event.target.value })} placeholder="CLO1" /></label>
      <label><span>Mức Bloom</span><input value={draft.levelCode} onChange={(event) => setDraft({ ...draft, levelCode: event.target.value })} placeholder="Ví dụ: L3" /></label>
      <label className="form-wide"><span>Nội dung chuẩn đầu ra</span><textarea required value={draft.statement} onChange={(event) => setDraft({ ...draft, statement: event.target.value })} /></label>
      <div className="dialog__actions form-wide"><button className="button button--ghost" type="button" onClick={onClose}>Hủy</button><button className="button button--primary" disabled={busy}>{busy ? "Đang lưu…" : "Lưu CLO"}</button></div>
    </form>
  </DialogFrame>;
}

export function CourseOutcomesPage() {
  const { versionId: rawVersion, courseId: rawCourse } = useParams();
  const versionId = Number(rawVersion), courseId = Number(rawCourse);
  const { user } = useAuth();
  const [version, setVersion] = useState<ProgramVersion | null>(null);
  const [course, setCourse] = useState<ProgramCourse | null>(null);
  const [clos, setClos] = useState<Clo[]>([]);
  const [courseMappings, setCourseMappings] = useState<CoursePloMapping[]>([]);
  const [plos, setPlos] = useState<Plo[]>([]);
  const [cloMappings, setCloMappings] = useState<Record<number, CloPloMapping[]>>({});
  const [editing, setEditing] = useState<Clo | "new" | null>(null);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(true);
  const [busy, setBusy] = useState(false);
  const { confirm, confirmationDialog } = useConfirmDialog();

  const load = useCallback(async () => {
    if (!Number.isInteger(versionId) || !Number.isInteger(courseId)) return;
    setLoading(true); setError("");
    try {
      const [v, c, cloItems, mappings, ploItems] = await Promise.all([
        curriculumApi.getVersion(versionId),
        curriculumApi.getCourse(versionId, courseId),
        curriculumApi.listClos(versionId, courseId),
        curriculumApi.listCoursePloMappings(versionId, courseId),
        curriculumApi.listPlos(versionId),
      ]);
      setVersion(v); setCourse(c); setClos(cloItems); setCourseMappings(mappings); setPlos(ploItems);
      const pairs = await Promise.all(cloItems.map(async (clo) => [clo.id, await curriculumApi.listCloPloMappings(versionId, courseId, clo.id)] as const));
      setCloMappings(Object.fromEntries(pairs));
    } catch (caught) { setError(caught instanceof Error ? caught.message : "Không thể tải chuẩn đầu ra học phần."); }
    finally { setLoading(false); }
  }, [versionId, courseId]);
  useEffect(() => { void load(); }, [load]);

  const editable = version?.status === "draft" && (user?.role === "admin" || user?.role === "lecturer");

  async function save(draft: CloDraft) {
    setBusy(true); setError("");
    try {
      const input = { code: draft.code, statement: draft.statement, levelCode: draft.levelCode.trim() || null };
      if (editing === "new") await curriculumApi.createClo(versionId, courseId, input);
      else if (editing) await curriculumApi.updateClo(versionId, courseId, editing.id, input);
      setEditing(null); await load();
    } catch (caught) { setError(caught instanceof Error ? caught.message : "Không thể lưu CLO."); }
    finally { setBusy(false); }
  }

  async function remove(clo: Clo) {
    const accepted = await confirm({
      title: `Xóa ${clo.code}`,
      description: "CLO sẽ bị xóa khỏi học phần. Thao tác không thể hoàn tác.",
      confirmLabel: "Xóa CLO",
      tone: "danger",
    });
    if (!accepted) return;
    try { await curriculumApi.deleteClo(versionId, courseId, clo.id); await load(); }
    catch (caught) { setError(caught instanceof Error ? caught.message : "Không thể xóa CLO."); }
  }

  async function addMapping(clo: Clo, value: string) {
    const id = Number(value); if (!id) return;
    try { await curriculumApi.addCloPloMapping(versionId, courseId, clo.id, id); await load(); }
    catch (caught) { setError(caught instanceof Error ? caught.message : "Không thể thêm ánh xạ."); }
  }

  async function removeMapping(clo: Clo, mapping: CloPloMapping) {
    try { await curriculumApi.deleteCloPloMapping(versionId, courseId, clo.id, mapping.coursePloId); await load(); }
    catch (caught) { setError(caught instanceof Error ? caught.message : "Không thể xóa ánh xạ."); }
  }

  if (loading) return <section className="admin-page"><div className="compact-loading">Đang tải chuẩn đầu ra học phần…</div></section>;
  return <section className="admin-page">
    <div className="breadcrumbs"><Link to={`/curriculum/versions/${versionId}`}>Học phần</Link><span>/</span><span>{course?.institutionalCode ?? course?.name ?? courseId}</span></div>
    <header className="workspace-heading"><div><span className="eyebrow">{course?.institutionalCode ?? "HỌC PHẦN"}</span><h1>Chuẩn đầu ra học phần</h1><p>{course?.name}</p></div>{editable && <button className="button button--primary" type="button" onClick={() => setEditing("new")}>Thêm CLO</button>}</header>
    <VersionOutcomeNav versionId={versionId} />
    {error && <div className="alert alert--error" role="alert">{error}</div>}

    {course && <div className="summary-strip"><div><span>Trạng thái phiên bản</span><strong>{version?.status === "draft" ? "Bản nháp" : "Chỉ đọc"}</strong></div><div><span>Tín chỉ</span><strong>{course.credits}</strong></div><div><span>Số CLO</span><strong>{clos.length}</strong></div></div>}

    <details className="reference-panel">
      <summary>PLO được học phần đóng góp <span>{courseMappings.length}/{plos.length}</span></summary>
      <div className="reference-list">{courseMappings.map((mapping) => {
        const plo = plos.find((item) => item.id === mapping.ploId);
        return <div key={mapping.id}><strong>{mapping.ploCode}</strong><span>{plo?.statement ?? "Không có mô tả"}</span><small>{mapping.weightCode} · {mapping.progressionCode}</small></div>;
      })}{!courseMappings.length && <div className="table-empty">Học phần chưa được ánh xạ với PLO.</div>}</div>
    </details>

    <div className="section-heading"><div><h2>Danh sách CLO</h2><p>Ánh xạ từng CLO trong phạm vi PLO của học phần.</p></div><span>{clos.length} CLO</span></div>
    <div className="clo-list">{clos.map((clo) => {
      const mappings = cloMappings[clo.id] ?? [];
      const available = courseMappings.filter((mapping) => !mappings.some((item) => item.coursePloId === mapping.id));
      return <article className="clo-row" key={clo.id}>
        <div className="clo-row__identity"><strong>{clo.code}</strong><span>{clo.levelCode ?? "Chưa đặt mức Bloom"}</span></div>
        <div className="clo-row__content"><p>{clo.statement}</p><div className="mapping-chips">{mappings.map((mapping) => <span className="mapping-chip" key={mapping.coursePloId}>{mapping.ploCode}{editable && <button type="button" aria-label={`Bỏ ánh xạ ${clo.code} với ${mapping.ploCode}`} onClick={() => void removeMapping(clo, mapping)}>×</button>}</span>)}{!mappings.length && <span className="muted-text">Chưa ánh xạ PLO</span>}</div></div>
        <div className="clo-row__actions">{editable && <><select aria-label={`Thêm ánh xạ cho ${clo.code}`} value="" disabled={!available.length} onChange={(event) => void addMapping(clo, event.target.value)}><option value="">{available.length ? "Thêm ánh xạ…" : "Đã ánh xạ hết"}</option>{available.map((mapping) => <option key={mapping.id} value={mapping.id}>{mapping.ploCode} · {mapping.weightCode}/{mapping.progressionCode}</option>)}</select><div className="table-actions"><button className="button button--small button--ghost" type="button" onClick={() => setEditing(clo)}>Cập nhật</button><button className="button button--small button--danger" type="button" onClick={() => void remove(clo)}>Xóa</button></div></>}</div>
      </article>;
    })}</div>
    {!clos.length && <div className="empty-state"><strong>Học phần chưa có CLO</strong><span>{editable ? "Thêm CLO đầu tiên để bắt đầu ánh xạ với PLO." : "Dữ liệu chưa được khai báo trong phiên bản này."}</span></div>}
    {editing && <CloEditor initial={editing === "new" ? undefined : editing} busy={busy} onClose={() => setEditing(null)} onSave={save} />}
    {confirmationDialog}
  </section>;
}
