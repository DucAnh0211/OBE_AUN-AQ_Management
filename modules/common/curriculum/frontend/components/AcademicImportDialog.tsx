import { useState } from "react";
import { academicApi } from "../api/academics";
import { directoryApi } from "../api/directory";
import type { ImportPreview, ImportRow } from "../types/academics";
import { AcademicDialog } from "./AcademicDialog";

/** Nơi nhận dữ liệu import: sổ điểm một lớp học phần, hoặc danh sách một lớp sinh hoạt (chỉ sinh viên). */
export type ImportTarget = {
  title: string;
  modes: ("students" | "grades")[];
  read: (form: FormData) => Promise<{ rows: ImportRow[]; schemeRevision: number; sheets: string[] }>;
  sheets: (form: FormData) => Promise<{ sheets: string[] }>;
  preview: (mode: string, rows: ImportRow[], schemeRevision: number, overwrite: boolean) => Promise<ImportPreview>;
  apply: (mode: string, rows: ImportRow[], schemeRevision: number, overwrite: boolean) => Promise<ImportPreview>;
  template?: (mode: string) => Promise<void>;
};
export const offeringImportTarget = (versionId: number, offeringId: number, period: string): ImportTarget => {
  const request = (mode: string, rows: ImportRow[], schemeRevision: number, overwriteExisting: boolean) => ({ mode, academicPeriod: period, schemeRevision, rows, overwriteExisting, offeringId });
  return {
    title: `Import sinh viên hoặc điểm · ${period}`, modes: ["students", "grades"],
    read: form => academicApi.read(versionId, offeringId, form), sheets: form => academicApi.sheets(versionId, offeringId, form),
    preview: (m, r, s, o) => academicApi.preview(versionId, offeringId, request(m, r, s, o)), apply: (m, r, s, o) => academicApi.apply(versionId, offeringId, request(m, r, s, o)),
    template: mode => academicApi.template(versionId, offeringId, mode),
  };
};
export const homeClassImportTarget = (versionId: number, homeClassId: number, code: string): ImportTarget => ({
  title: `Import sinh viên vào lớp ${code}`, modes: ["students"],
  read: form => directoryApi.readHomeClassImport(versionId, homeClassId, form), sheets: form => directoryApi.readHomeClassImport(versionId, homeClassId, form),
  preview: (_m, rows) => directoryApi.previewHomeClassImport(versionId, homeClassId, rows), apply: (_m, rows) => directoryApi.applyHomeClassImport(versionId, homeClassId, rows),
});

export function AcademicImportDialog({ target, onClose, onSaved }: { target: ImportTarget; onClose: () => void; onSaved: (message: string) => void }) {
  const [mode, setMode] = useState<string>(target.modes[0]); const [file, setFile] = useState<File | null>(null); const [text, setText] = useState("");
  const [sheets, setSheets] = useState<string[]>([]); const [sheet, setSheet] = useState(""); const [overwrite, setOverwrite] = useState(false);
  const [preview, setPreview] = useState<ImportPreview | null>(null); const [request, setRequest] = useState<{ rows: ImportRow[]; schemeRevision: number } | null>(null);
  const [busy, setBusy] = useState(false); const [error, setError] = useState("");
  const invalidate = () => { setPreview(null); setRequest(null); setError(""); };
  async function pickFile(value: File | null) {
    invalidate(); setFile(value); setSheets([]); setSheet("");
    if (!value) return;
    if (value.size > 5 * 1024 * 1024) { setError("Tệp vượt quá 5 MB."); setFile(null); return; }
    setBusy(true);
    try { const form = new FormData(); form.append("file", value); form.append("listSheets", "true"); const result = await target.sheets(form); setSheets(result.sheets); setSheet(result.sheets[0] ?? ""); }
    catch (e) { setError(message(e)); setFile(null); } finally { setBusy(false); }
  }
  async function inspect() {
    setBusy(true); setError(""); setPreview(null); setRequest(null);
    try {
      const form = new FormData(); if (file) form.append("file", file); else form.append("text", text);
      form.append("mode", mode); if (sheet && file) form.append("sheet", sheet);
      const parsed = await target.read(form);
      const result = await target.preview(mode, parsed.rows, parsed.schemeRevision, overwrite);
      setPreview(result); setRequest({ rows: result.rows.map(r => r.row).filter(r => r.rowNumber > 0), schemeRevision: parsed.schemeRevision });
    } catch (e) { setError(message(e)); } finally { setBusy(false); }
  }
  async function apply() {
    if (!request || !preview?.canApply) return; setBusy(true); setError("");
    try {
      const result = await target.apply(mode, request.rows, request.schemeRevision, overwrite);
      if (!result.canApply) { setPreview(result); setRequest(null); setError("Dữ liệu đã thay đổi hoặc có dòng lỗi. Hãy kiểm tra rồi xem trước lại."); }
      else onSaved(`Đã nhập ${result.appliedRows} dòng; ${result.rows.filter(r => r.action === "skip").length} dòng có sẵn được bỏ qua.`);
    }
    catch (e) { setError(message(e)); setRequest(null); } finally { setBusy(false); }
  }
  return <AcademicDialog title={target.title} onClose={busy ? () => {} : onClose}>
    <div className="admin-form"><p>Chọn tệp .xlsx/CSV UTF-8 hoặc dán vùng ô đã sao chép từ Google Sheets/Excel, gồm cả hàng tiêu đề. Tối đa 500 dòng, 5 MB. Định dạng cột MSSV là văn bản để giữ số 0 đầu.</p>
      {target.modes.length > 1 && <label>Loại import<select value={mode} disabled={busy} onChange={e => { setMode(e.target.value); invalidate(); }}><option value="students">Danh sách sinh viên</option><option value="grades">Điểm sinh viên đã có trong lớp học phần</option></select></label>}
      {target.template && <button className="button button--secondary" disabled={busy} onClick={() => void target.template!(mode).catch(e => setError(message(e)))}>Tải mẫu Excel</button>}
      <label>Tệp Excel/CSV<input type="file" accept=".xlsx,.csv,.tsv,.txt" disabled={busy} onChange={e => void pickFile(e.target.files?.[0] ?? null)} /></label>
      {file && <label>Sheet<select value={sheet} disabled={busy} onChange={e => { setSheet(e.target.value); invalidate(); }}>{sheets.map(s => <option key={s}>{s}</option>)}</select></label>}
      <label>Hoặc dán bảng từ trang tính<textarea rows={5} placeholder={mode === "students" ? "MSSV\tHoTen\nSV001\tNguyễn Văn An" : "MSSV\tMÃ_ĐẦU_ĐIỂM\nSV001\t8.5"} value={text} disabled={busy || !!file} onChange={e => { setText(e.target.value); invalidate(); }} /></label>
      {mode === "grades" && <><p>Ô điểm trống được bỏ qua, không xóa điểm đã có. Dùng mã đầu điểm trong mẫu. Điểm thập phân dùng dấu chấm hoặc dấu phẩy; ô công thức phải được chuyển thành giá trị.</p><label className="check-control"><input type="checkbox" checked={overwrite} disabled={busy} onChange={e => { setOverwrite(e.target.checked); invalidate(); }} /> Cho phép cập nhật điểm đã có (có lưu lịch sử)</label></>}
      {error && <p role="alert" className="alert alert--error">{error}</p>}
      <button className="button button--secondary" disabled={busy || (!file && !text.trim())} onClick={() => void inspect()}>{busy ? "Đang xử lý…" : "Xem trước và kiểm tra"}</button>
      {preview && <div><p role="status">{preview.rows.length} dòng · {preview.rows.filter(r => r.errors.length > 0).length} dòng lỗi. {preview.canApply ? "Có thể nhập dữ liệu." : "Chưa ghi dữ liệu; cần sửa hết lỗi trước khi nhập."}</p><div className="academic-preview"><table className="data-table"><thead><tr><th>Dòng</th><th>MSSV</th><th>Họ tên</th>{mode === "grades" && <th>Điểm</th>}<th>Kết quả kiểm tra</th></tr></thead><tbody>{preview.rows.map((r, i) => <tr key={i}><td>{r.row.rowNumber || "—"}</td><td>{r.row.studentCode}</td><td>{r.row.fullName}</td>{mode === "grades" && <td>{Object.entries(r.row.scores).map(([k, v]) => `${k}: ${v ?? "—"}`).join("; ")}</td>}<td>{r.errors.length ? r.errors.join(" ") : ({ add: "Thêm mới", skip: "Đã có, bỏ qua", update: "Cập nhật điểm" } as Record<string, string>)[r.action]}</td></tr>)}</tbody></table></div></div>}
      <div className="dialog__actions"><button className="button button--ghost" disabled={busy} onClick={onClose}>Đóng</button><button className="button button--primary" disabled={busy || !preview?.canApply || !request} onClick={() => void apply()}>Xác nhận import</button></div>
    </div>
  </AcademicDialog>;
}
function message(e: unknown) { return e instanceof Error ? e.message : "Không xử lý được bảng dữ liệu."; }
