import { useCallback, useEffect, useMemo, useState, type FormEvent } from "react";
import { useSearchParams } from "react-router-dom";
import type { CurriculumProgram, ProgramCourse, ProgramVersion } from "../../../common/curriculum/frontend/types";
import { reportingApi, ReportApiError, type ReportDocument, type ReportKind, type ReportPage, type ReportSession } from "./api";
import "./reporting.css";

const names: Record<ReportKind, string> = { course: "UC25 · Báo cáo học phần", clo: "UC26 · Báo cáo CLO", plo: "UC27 · Báo cáo PLO" };
const date = (value: string) => new Intl.DateTimeFormat("vi-VN", { dateStyle: "short", timeStyle: "short", timeZone: "Asia/Ho_Chi_Minh" }).format(new Date(value));
const message = (error: unknown) => error instanceof Error ? error.message : "Không thể kết nối máy chủ. Vui lòng thử lại.";

export function ReportingPage() {
  const [key, setKey] = useState("");
  const [session, setSession] = useState<{ token: string; user: ReportSession } | null>(null);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");
  async function connect(event: FormEvent) {
    event.preventDefault(); setBusy(true); setError("");
    try {
      const token = key.trim();
      const user = await reportingApi(token).session();
      setSession({ token, user }); setKey("");
    } catch (e) { setError(message(e)); }
    finally { setBusy(false); }
  }
  const disconnect = useCallback(() => { setSession(null); setKey(""); }, []);
  return <section className="admin-page reporting-page">
    <div className="admin-heading"><div><span className="eyebrow">SV5 · BÁO CÁO DỮ LIỆU</span>
      <h1>Báo cáo học phần, CLO và PLO</h1>
      <p>Tổng hợp cấu trúc chương trình và mapping theo phiên bản. Các chỉ số được tính từ dữ liệu nguồn, không dùng AI.</p>
    </div>{session && <button className="button button--ghost" onClick={disconnect}>Ngắt kết nối · {session.user.userId}</button>}</div>
    {!session ? <form className="report-access admin-form" onSubmit={connect}>
      <h2>Truy cập báo cáo</h2><p>Nhập mã truy cập do quản trị viên cấp. Báo cáo chỉ hiển thị các chương trình bạn được phép xem.</p>
      <label>Mã truy cập<input type="password" required value={key} onChange={e => setKey(e.target.value)} autoComplete="off" disabled={busy} /></label>
      <button className="button button--primary" disabled={busy}>{busy ? "Đang kết nối…" : "Kết nối"}</button>
      {error && <div className="alert alert--error" role="alert">{error}</div>}
    </form> : <ReportWorkspace token={session.token} user={session.user} disconnect={disconnect} />}
  </section>;
}

function ReportWorkspace({ token, user, disconnect }: { token: string; user: ReportSession; disconnect: () => void }) {
  const api = useMemo(() => reportingApi(token), [token]);
  const [search, setSearch] = useSearchParams();
  const reportId = search.get("report");
  const [programs, setPrograms] = useState<CurriculumProgram[]>([]);
  const [versions, setVersions] = useState<ProgramVersion[]>([]);
  const [courses, setCourses] = useState<ProgramCourse[]>([]);
  const [programId, setProgramId] = useState("");
  const [versionId, setVersionId] = useState("");
  const [courseId, setCourseId] = useState("");
  const [kind, setKind] = useState<ReportKind>("course");
  const [history, setHistory] = useState<ReportPage | null>(null);
  const [page, setPage] = useState(1);
  const [revision, refresh] = useState(0);
  const [report, setReport] = useState<ReportDocument | null>(null);
  const [error, setError] = useState("");
  const [notice, setNotice] = useState("");
  const [busy, setBusy] = useState("");
  const [loadingVersions, setLoadingVersions] = useState(false);
  const [loadingCourses, setLoadingCourses] = useState(false);
  const [confirmRevoke, setConfirmRevoke] = useState(false);
  const fail = useCallback((e: unknown) => {
    if (e instanceof DOMException && e.name === "AbortError") return;
    if (e instanceof ReportApiError && e.status === 401) { disconnect(); return; }
    setError(message(e));
    if (e instanceof ReportApiError && (e.status === 403 || e.status === 404)) setReport(null);
  }, [disconnect]);

  useEffect(() => {
    const c = new AbortController();
    api.programs(c.signal).then(setPrograms).catch(fail);
    return () => c.abort();
  }, [api, fail, revision]);
  useEffect(() => {
    const c = new AbortController(); setVersions([]); setVersionId(""); setCourseId("");
    if (!programId) { setLoadingVersions(false); return; }
    setLoadingVersions(true);
    api.versions(Number(programId), c.signal).then(v => {
      setVersions(v); setVersionId(String((v.find(x => x.isCurrent) ?? v[0])?.id ?? ""));
    }).catch(fail).finally(() => { if (!c.signal.aborted) setLoadingVersions(false); });
    return () => c.abort();
  }, [api, fail, programId]);
  useEffect(() => {
    const c = new AbortController(); setCourses([]); setCourseId("");
    if (!versionId) { setLoadingCourses(false); return; }
    setLoadingCourses(true);
    api.courses(Number(versionId), c.signal).then(setCourses).catch(fail)
      .finally(() => { if (!c.signal.aborted) setLoadingCourses(false); });
    return () => c.abort();
  }, [api, fail, versionId]);
  useEffect(() => {
    const c = new AbortController(); setHistory(null);
    api.list(page, c.signal).then(setHistory).catch(fail);
    return () => c.abort();
  }, [api, fail, page, revision]);
  useEffect(() => {
    const c = new AbortController(); setReport(null); setConfirmRevoke(false);
    if (reportId) api.get(reportId, c.signal).then(setReport).catch(fail);
    return () => c.abort();
  }, [api, fail, reportId]);

  async function create(event: FormEvent) {
    event.preventDefault(); setBusy("create"); setError(""); setNotice("");
    try {
      const next = await api.create(kind, Number(versionId), kind === "plo" || !courseId ? null : Number(courseId));
      setSearch({ report: next.id }); setPage(1); refresh(n => n + 1);
      setNotice("Đã lưu báo cáo. Bản xem trước và file xuất sử dụng cùng dữ liệu tại thời điểm tạo.");
    } catch (e) { fail(e); } finally { setBusy(""); }
  }
  async function download(format: "docx" | "pdf") {
    if (!report) return; setBusy(format); setError("");
    try {
      const blob = await api.download(report.id, format);
      const url = URL.createObjectURL(blob);
      const a = document.createElement("a"); a.href = url; a.download = `${report.kind}-${report.id}.${format}`;
      document.body.append(a); a.click(); a.remove(); setTimeout(() => URL.revokeObjectURL(url), 1000);
    } catch (e) { fail(e); } finally { setBusy(""); }
  }
  async function revoke() {
    if (!report) return; setBusy("revoke"); setError("");
    try {
      await api.revoke(report.id); setSearch({}); setReport(null); setConfirmRevoke(false);
      setPage(1); refresh(n => n + 1); setNotice("Đã thu hồi báo cáo. Bản đã tải xuống trước đó không thể thu hồi từ thiết bị người nhận.");
    } catch (e) { fail(e); } finally { setBusy(""); }
  }
  return <>
    {error && <div className="alert alert--error" role="alert">{error} <button className="button button--small button--ghost" onClick={() => { setError(""); refresh(n => n + 1); }}>Tải lại danh sách</button></div>}
    {notice && <p className="report-notice" role="status">{notice}</p>}
    <form className="report-filters admin-form" onSubmit={create}>
      <label>Loại báo cáo<select value={kind} onChange={e => { setKind(e.target.value as ReportKind); setCourseId(""); }} disabled={!!busy}>
        {Object.entries(names).map(([k, label]) => <option key={k} value={k}>{label}</option>)}
      </select></label>
      <label>Chương trình<select required value={programId} onChange={e => { setProgramId(e.target.value); setVersionId(""); }} disabled={!!busy}>
        <option value="">Chọn chương trình</option>{programs.map(p => <option key={p.id} value={p.id}>{p.code} · {p.name}</option>)}
      </select></label>
      <label>Phiên bản<select required value={versionId} onChange={e => setVersionId(e.target.value)} disabled={!!busy || loadingVersions || !programId}>
        <option value="">{loadingVersions ? "Đang tải…" : "Chọn phiên bản"}</option>{versions.map(v => <option key={v.id} value={v.id}>{v.versionCode} · {v.status === "draft" ? "Bản nháp" : "Đã công bố"}</option>)}
      </select></label>
      {kind !== "plo" && <label>Học phần<select required={kind === "course"} value={courseId} onChange={e => setCourseId(e.target.value)} disabled={!!busy || loadingCourses || !versionId}>
        <option value="">{loadingCourses ? "Đang tải…" : kind === "clo" ? "Tất cả học phần" : "Chọn học phần"}</option>
        {courses.map(c => <option key={c.id} value={c.id}>{c.institutionalCode ?? `HP#${c.id}`} · {c.name}</option>)}
      </select></label>}
      <div className="report-create"><button className="button button--primary" disabled={!!busy || !user.canGenerate || !versionId || loadingVersions || loadingCourses || (kind === "course" && !courseId)}>
        {busy === "create" ? "Đang tạo báo cáo…" : "Tạo và xem báo cáo"}</button>
        {!user.canGenerate && <small>Tài khoản này chỉ có quyền xem và tải báo cáo.</small>}
      </div>
    </form>
    <section aria-labelledby="report-history-heading" className="report-history">
      <h2 id="report-history-heading">Báo cáo đã lưu</h2>
      {history === null ? <p role="status">Đang tải danh sách…</p> : history.items.length === 0 ? <p className="empty-state">Chưa có báo cáo trong phạm vi bạn được phép xem.</p> : <>
        <div className="report-table-scroll"><table><thead><tr><th scope="col">Báo cáo</th><th scope="col">Phiên bản ID</th><th scope="col">Người tạo</th><th scope="col">Thời gian (VN)</th><th scope="col">Thao tác</th></tr></thead>
          <tbody>{history.items.map(item => <tr key={item.id} className={item.id === reportId ? "report-row-selected" : ""}>
            <td>{item.title}</td><td>{item.versionId}</td><td>{item.createdBy}</td><td>{date(item.createdAt)}</td><td><button className="button button--small button--ghost" disabled={!!busy} onClick={() => { setError(""); setSearch({ report: item.id }); }}>Xem</button></td>
          </tr>)}</tbody></table></div>
        <div className="pagination"><span>{history.totalItems} báo cáo · Trang {page}/{Math.max(1, Math.ceil(history.totalItems / 10))}</span><div>
          <button className="button button--small button--ghost" disabled={page <= 1 || !!busy} onClick={() => setPage(p => p - 1)}>Trước</button>
          <button className="button button--small button--ghost" disabled={page * 10 >= history.totalItems || !!busy} onClick={() => setPage(p => p + 1)}>Sau</button>
        </div></div>
      </>}
    </section>
    {reportId && !report && !error && <p role="status">Đang tải báo cáo…</p>}
    {report && <article className="report-preview" aria-labelledby="report-title">
      <div className="report-preview-heading"><div><span className="eyebrow">{names[report.kind]} · CHƯA PHÊ DUYỆT</span><h2 id="report-title">{report.title}</h2></div>
        <div className="table-actions"><button className="button button--primary" disabled={!!busy} onClick={() => download("docx")}>{busy === "docx" ? "Đang xuất…" : "Tải DOCX"}</button>
          <button className="button button--secondary" disabled={!!busy} onClick={() => download("pdf")}>{busy === "pdf" ? "Đang xuất…" : "Tải PDF"}</button>
          {user.canGenerate && <button className="button button--danger" disabled={!!busy} onClick={() => setConfirmRevoke(true)}>Thu hồi</button>}</div></div>
      {confirmRevoke && <div className="report-revoke" role="alert"><p>Thu hồi báo cáo này? Người dùng sẽ không thể mở hoặc tải lại bản này.</p>
        <button className="button button--danger" disabled={!!busy} onClick={revoke}>Xác nhận thu hồi</button> <button className="button button--ghost" disabled={!!busy} onClick={() => setConfirmRevoke(false)}>Hủy</button></div>}
      <p><strong>{report.programCode} · {report.programName}</strong><br />Phiên bản {report.versionCode} · {report.versionStatus === "draft" ? "CTĐT bản nháp" : "CTĐT đã công bố"}<br />Tạo lúc {date(report.createdAt)} bởi {report.createdBy}</p>
      <p className="report-source">Báo cáo {report.id}<br />Quy tắc: {report.ruleVersion}<br />SHA-256 dữ liệu: <code>{report.snapshotHash}</code></p>
      <div className="report-metrics">{report.metrics.map(m => <div key={m.label}><span>{m.label}</span><strong>{m.numerator}/{m.denominator}</strong><small>{m.percentage === null ? "Chưa có dữ liệu" : `${m.percentage.toLocaleString("vi-VN")}%`}</small></div>)}</div>
      <section><h3>Kết quả kiểm tra</h3><ul>{report.findings.map((f, i) => <li key={i}>{f}</li>)}</ul></section>
      {report.tables.map((t, i) => <section key={i} className="report-section"><h3>{t.title}</h3>
        {t.rows.length === 0 ? <p>Chưa có dữ liệu.</p> : <div className="report-table-scroll" tabIndex={0} role="region" aria-label={t.title}>
          <table><thead><tr>{t.headers.map((h, n) => <th scope="col" key={n}>{h}</th>)}</tr></thead>
            <tbody>{t.rows.map((row, n) => <tr key={n}>{row.map((cell, j) => <td key={j}>{cell}</td>)}</tr>)}</tbody></table>
        </div>}
      </section>)}
      <section className="report-notes"><h3>Nguồn và giới hạn</h3><ul>{report.notes.map((note, i) => <li key={i}>{note}</li>)}</ul></section>
    </article>}
  </>;
}
