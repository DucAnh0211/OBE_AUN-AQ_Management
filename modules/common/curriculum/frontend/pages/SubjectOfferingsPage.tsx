import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { curriculumApi } from "../api";
import { offeringApi } from "../api/offerings";
import type { PagedResult, ProgramCourse } from "../types";
import type { CourseOffering } from "../types/offerings";
import { ErrorMessage, Pager, message } from "./academicUi";
import { OfferingRows } from "./OfferingPages";
import { DomainEyebrow, domainPage } from "../components/Domain";
import { CourseNav } from "../components/VersionNav";
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
      .catch(e => { if (active) setError(message(e)); });
    return () => { active = false; };
  }, [version, subject, page, search]);
  return <section className={domainPage("teaching", "academic-page")}>
    <div className="breadcrumbs"><Link to={`/curriculum/versions/${version}`}>Danh sách học phần</Link><span>/</span><span>{course?.name ?? "Học phần"}</span></div>
    <div className="admin-heading"><div><DomainEyebrow domain="teaching" detail={course ? `Học phần ${course.institutionalCode}` : undefined} /><h1>{course?.name ?? "Học phần"}</h1><p>Các lớp học phần đã mở cho học phần này qua các học kỳ. Mỗi lớp học phần có mã riêng; CLO/PLO và cơ cấu điểm dùng chung từ đề cương học phần.</p></div></div>
    <CourseNav versionId={version} courseId={subject} staff />
    <div className="toolbar academic-roster-toolbar"><h2>Danh sách lớp học phần</h2><input aria-label="Tìm lớp học phần của học phần" type="search" placeholder="Tìm mã lớp học phần, nhóm hoặc học kỳ" value={search} onChange={e => { setSearch(e.target.value); setPage(1); }} /></div>
    <ErrorMessage value={error} />
    {!rows ? !error && <p role="status">Đang tải…</p> : !rows.items.length ? <p className="empty-state">Chưa có lớp học phần phù hợp của học phần này hoặc bạn chưa được phân công.</p> : <>
      <OfferingRows versionId={version} rows={rows.items} isAdmin={false} showPeriod />
      <Pager page={page} totalPages={rows.totalPages} label={`${rows.totalItems} lớp học phần`} onPage={setPage} />
    </>}
  </section>;
}
