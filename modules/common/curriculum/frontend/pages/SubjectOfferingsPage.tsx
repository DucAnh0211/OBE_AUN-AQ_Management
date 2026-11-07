import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { curriculumApi } from "../api";
import { offeringApi } from "../api/offerings";
import type { PagedResult, ProgramCourse } from "../types";
import type { CourseOffering } from "../types/offerings";
import "./academics.css";

export function SubjectOfferingsPage() {
  const version = Number(useParams().versionId), subject = Number(useParams().courseId);
  const [course, setCourse] = useState<ProgramCourse | null>(null);
  const [rows, setRows] = useState<PagedResult<CourseOffering> | null>(null);
  const [search, setSearch] = useState(""); const [page, setPage] = useState(1); const [error, setError] = useState("");
  useEffect(() => {
    let active = true; setRows(null); setError("");
    void Promise.all([curriculumApi.getCourse(version, subject), offeringApi.subjectOfferings(version, subject, page, search)])
      .then(([c, r]) => { if (active) { setCourse(c); setRows(r); } })
      .catch(e => { if (active) setError(e instanceof Error ? e.message : "Không tải được danh sách học phần."); });
    return () => { active = false; };
  }, [version, subject, page, search]);
  return <section className="admin-page academic-page">
    <div className="breadcrumbs"><Link to={`/curriculum/versions/${version}`}>Danh sách bộ môn</Link><span>/</span><span>{course?.name ?? "Bộ môn"}</span></div>
    <div className="admin-heading"><div><span className="eyebrow">Bộ môn · {course?.institutionalCode}</span><h1>{course?.name ?? "Bộ môn"}</h1><p>Danh sách các học phần thuộc bộ môn. Mỗi học phần có mã riêng; CLO/PLO dùng chung từ bộ môn trong phiên bản CTĐT.</p></div><Link className="button button--secondary" to={`/curriculum/versions/${version}/courses/${subject}/outcomes`}>CLO/PLO</Link></div>
    <div className="toolbar academic-roster-toolbar"><h2>Danh sách học phần</h2><input aria-label="Tìm học phần của bộ môn" type="search" placeholder="Tìm mã học phần, mã lớp hoặc học kỳ" value={search} onChange={e => { setSearch(e.target.value); setPage(1); }} /></div>
    {error && <p role="alert" className="alert alert--error">{error}</p>}
    {!rows ? !error && <p role="status">Đang tải…</p> : !rows.items.length ? <p className="empty-state">Chưa có học phần phù hợp thuộc bộ môn này hoặc bạn chưa được phân công.</p> : <>
      <div className="data-table-wrap"><table className="data-table"><thead><tr><th>Mã học phần</th><th>Học phần</th><th>Lớp</th><th>Học kỳ / đợt học</th><th>Giảng viên</th><th>Sinh viên</th><th>Thao tác</th></tr></thead><tbody>{rows.items.map(o => <tr key={o.id}><td><strong>{o.code}</strong></td><td><Link to={`/curriculum/versions/${version}/offerings/${o.id}/students`}>{o.subjectName}</Link></td><td>{o.classCode} · {o.className}</td><td>{o.academicPeriod}</td><td>{o.lecturerName ?? (o.lecturerId ? "Tài khoản không còn hoạt động" : "Chưa phân công")}</td><td>{o.studentCount}</td><td><Link className="button button--small button--secondary" to={`/curriculum/versions/${version}/offerings/${o.id}/students`}>Xem học phần</Link></td></tr>)}</tbody></table></div>
      <div className="pagination"><span>{rows.totalItems} học phần · Trang {page}/{Math.max(1, rows.totalPages)}</span><button className="button button--ghost" disabled={page <= 1} onClick={() => setPage(n => n - 1)}>Trước</button><button className="button button--ghost" disabled={page >= rows.totalPages} onClick={() => setPage(n => n + 1)}>Sau</button></div>
    </>}
  </section>;
}
