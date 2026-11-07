import { type FormEvent, useCallback, useEffect, useState } from "react";
import { accreditationApi } from "../../../../modules/sv4/accreditation/frontend";
import type { AccreditationStatus, EffectiveAccreditationStatus, FrameworkSummary, ProgramAccreditation } from "../../../../modules/sv4/accreditation/frontend/types";
import { useConfirmDialog } from "../components/Dialogs";
import { Alert, Dialog, errorInfo, FieldError, formatDay } from "./ui";

const statusLabels: Record<EffectiveAccreditationStatus, string> = {
  accredited: "Đã kiểm định",
  expired: "Hết hiệu lực",
  self_assessed: "Đã tự đánh giá",
  not_met: "Chưa đạt",
};

const recordedStatuses: AccreditationStatus[] = ["accredited", "self_assessed", "not_met"];

// The seven verdict terms exactly as named by the AUN-QA Guide v4.0, printed page 57.
const verdicts = [
  "Absolutely Inadequate",
  "Inadequate and Improvement is Necessary",
  "Inadequate but Minor Improvement Will Make It Adequate",
  "Adequate as Expected",
  "Better Than Adequate",
  "Example of Best Practices",
  "Excellent (Example of World-class or Leading Practices)",
];

export function AccreditationStatusChip({ status }: { status: EffectiveAccreditationStatus | null }) {
  return status
    ? <span className={`status status--acc-${status}`}>{statusLabels[status]}</span>
    : <span className="status status--acc-none">Chưa kiểm định</span>;
}

type FormState = { mode: "create" } | { mode: "edit"; record: ProgramAccreditation };

function AccreditationForm({ programId, state, onClose, onSaved }: {
  programId: number;
  state: FormState;
  onClose: () => void;
  onSaved: () => void;
}) {
  const record = state.mode === "edit" ? state.record : null;
  const [frameworks, setFrameworks] = useState<FrameworkSummary[]>([]);
  const [frameworkId, setFrameworkId] = useState(record ? String(record.frameworkId) : "");
  const [status, setStatus] = useState<AccreditationStatus>(record?.status ?? "accredited");
  const [assessedOn, setAssessedOn] = useState(record?.assessedOn ?? "");
  const [validUntil, setValidUntil] = useState(record?.validUntil ?? "");
  const [verdictRating, setVerdictRating] = useState(record?.verdictRating ? String(record.verdictRating) : "");
  const [note, setNote] = useState(record?.note ?? "");
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");
  const [fields, setFields] = useState<Record<string, string[]>>({});

  useEffect(() => {
    let disposed = false;
    accreditationApi.listFrameworks("published").then((items) => {
      if (disposed) return;
      setFrameworks(items);
      setFrameworkId((current) => current || (items[0] ? String(items[0].id) : ""));
    }).catch((caught: unknown) => { if (!disposed) setError(errorInfo(caught).message); });
    return () => { disposed = true; };
  }, []);

  async function submit(event: FormEvent) {
    event.preventDefault();
    setSaving(true);
    setError("");
    setFields({});
    const input = {
      frameworkId: frameworkId ? Number(frameworkId) : null,
      status,
      assessedOn: assessedOn || null,
      validUntil: validUntil || null,
      verdictRating: verdictRating ? Number(verdictRating) : null,
      note: note.trim() || null,
    };
    try {
      if (record) await accreditationApi.updateProgramAccreditation(record.id, input);
      else await accreditationApi.createProgramAccreditation(programId, input);
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
    <Dialog title={record ? "Cập nhật kết quả kiểm định" : "Ghi nhận kết quả kiểm định"} description="Chỉ ghi nhận kết quả có căn cứ, ví dụ báo cáo đánh giá hoặc giấy chứng nhận của AUN-QA." onClose={onClose}>
      <form className="admin-form" onSubmit={submit}>
        {error && <Alert message={error} />}
        <label>
          <span>Bộ tiêu chí</span>
          <select value={frameworkId} onChange={(event) => setFrameworkId(event.target.value)} disabled={saving}>
            {frameworks.length === 0 && <option value="">Chưa có bộ tiêu chí đã công bố</option>}
            {frameworks.map((framework) => <option key={framework.id} value={framework.id}>{framework.code} · {framework.version}</option>)}
            {record && !frameworks.some((framework) => framework.id === record.frameworkId) && <option value={record.frameworkId}>{record.frameworkCode} · {record.frameworkVersion}</option>}
          </select>
          <FieldError errors={fields.frameworkId} />
        </label>
        <label>
          <span>Kết quả</span>
          <select value={status} onChange={(event) => setStatus(event.target.value as AccreditationStatus)} disabled={saving}>
            {recordedStatuses.map((value) => <option key={value} value={value}>{statusLabels[value]}</option>)}
          </select>
          <FieldError errors={fields.status} />
        </label>
        <label>
          <span>Ngày đánh giá</span>
          <input type="date" value={assessedOn} onChange={(event) => setAssessedOn(event.target.value)} disabled={saving} required />
          <FieldError errors={fields.assessedOn} />
        </label>
        <label>
          <span>Hiệu lực đến (nếu có)</span>
          <input type="date" value={validUntil} onChange={(event) => setValidUntil(event.target.value)} disabled={saving} />
          <FieldError errors={fields.validUntil} />
        </label>
        <label>
          <span>Mức kết luận (nếu có)</span>
          <select value={verdictRating} onChange={(event) => setVerdictRating(event.target.value)} disabled={saving}>
            <option value="">Không ghi nhận</option>
            {verdicts.map((label, index) => <option key={label} value={index + 1}>{index + 1} · {label}</option>)}
          </select>
          <FieldError errors={fields.verdictRating} />
        </label>
        <label>
          <span>Ghi chú và nguồn</span>
          <textarea value={note} onChange={(event) => setNote(event.target.value)} disabled={saving} maxLength={1000} placeholder="Ví dụ: AUN-QA Assessment lần thứ..., số giấy chứng nhận" />
          <FieldError errors={fields.note} />
        </label>
        <div className="dialog__actions">
          <button className="button button--ghost" type="button" onClick={onClose} disabled={saving}>Hủy</button>
          <button className="button button--primary" disabled={saving}>{saving ? "Đang lưu…" : "Lưu"}</button>
        </div>
      </form>
    </Dialog>
  );
}

export function ProgramAccreditationPanel({ programId, isAdmin }: { programId: number; isAdmin: boolean }) {
  const { confirm, confirmationDialog } = useConfirmDialog();
  const [records, setRecords] = useState<ProgramAccreditation[] | null>(null);
  const [error, setError] = useState("");
  const [dialog, setDialog] = useState<FormState | null>(null);

  const load = useCallback(async () => {
    setError("");
    try {
      setRecords(await accreditationApi.programAccreditations(programId));
    } catch (caught) {
      setError(errorInfo(caught).message);
    }
  }, [programId]);

  useEffect(() => { void load(); }, [load]);

  async function remove(record: ProgramAccreditation) {
    const accepted = await confirm({
      title: "Xóa kết quả kiểm định?",
      description: `Đợt đánh giá ngày ${formatDay(record.assessedOn)} sẽ bị xóa khỏi hồ sơ của CTĐT.`,
      confirmLabel: "Xóa",
      tone: "danger",
    });
    if (!accepted) return;
    try {
      await accreditationApi.deleteProgramAccreditation(record.id);
      await load();
    } catch (caught) {
      setError(errorInfo(caught).message);
    }
  }

  return (
    <section className="accreditation-panel" aria-label="Kết quả kiểm định AUN-QA">
      <div className="list-toolbar">
        <strong>Kết quả kiểm định</strong>
        {isAdmin && <button className="button button--small button--secondary" onClick={() => setDialog({ mode: "create" })}>Ghi nhận kết quả</button>}
      </div>
      {error && <Alert message={error} onRetry={() => void load()} />}
      {records && records.length === 0 && <p className="muted-text">Chương trình này chưa có kết quả kiểm định được ghi nhận.</p>}
      {records && records.length > 0 && (
        <div className="data-table-wrap">
          <table className="data-table">
            <thead><tr><th>Kết quả</th><th>Bộ tiêu chí</th><th>Ngày đánh giá</th><th>Hiệu lực đến</th><th>Mức kết luận</th><th>Ghi chú</th>{isAdmin && <th className="align-right">Thao tác</th>}</tr></thead>
            <tbody>{records.map((record) => (
              <tr key={record.id}>
                <td><AccreditationStatusChip status={record.effectiveStatus} /></td>
                <td>{record.frameworkCode} · {record.frameworkVersion}</td>
                <td>{formatDay(record.assessedOn)}</td>
                <td>{formatDay(record.validUntil)}</td>
                <td>{record.verdictRating ? `${record.verdictRating} · ${verdicts[record.verdictRating - 1]}` : "—"}</td>
                <td>{record.note ?? "—"}</td>
                {isAdmin && <td><div className="table-actions">
                  <button className="button button--small" onClick={() => setDialog({ mode: "edit", record })}>Sửa</button>
                  <button className="button button--small button--danger" onClick={() => void remove(record)}>Xóa</button>
                </div></td>}
              </tr>
            ))}</tbody>
          </table>
        </div>
      )}
      {confirmationDialog}
      {dialog && <AccreditationForm programId={programId} state={dialog} onClose={() => setDialog(null)} onSaved={() => { setDialog(null); void load(); }} />}
    </section>
  );
}
