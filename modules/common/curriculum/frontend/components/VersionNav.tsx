import { useEffect, useRef, type ReactNode } from "react";
import { NavLink, useLocation } from "react-router-dom";
import { DomainIcon, domains, type DomainKey } from "./Domain";

type Item = [label: string, path: string, end?: boolean];
/** Thanh tab cuộn ngang trên màn hình hẹp: luôn đưa tab đang chọn vào khung nhìn. */
function ScrollingNav({ label, className = "section-nav", children }: { label: string; className?: string; children: ReactNode }) {
  const ref = useRef<HTMLElement>(null); const { pathname } = useLocation();
  useEffect(() => {
    const nav = ref.current; const active = nav?.querySelector<HTMLElement>("a.active");
    if (nav && active && nav.scrollWidth > nav.clientWidth) nav.scrollLeft = Math.max(0, active.offsetLeft - (nav.clientWidth - active.offsetWidth) / 2);
  }, [pathname]);
  return <nav ref={ref} className={className} aria-label={label}>{children}</nav>;
}
/** Điều hướng của một phiên bản CTĐT, chia theo nhóm chức năng (sinh viên chỉ thấy nhóm CTĐT). */
export function VersionNav({ versionId, staff }: { versionId: number; staff: boolean }) {
  const v = `/curriculum/versions/${versionId}`;
  const groups: [DomainKey, Item[]][] = [
    ["curriculum", [["Học phần", v, true], ["Chuẩn đầu ra", `${v}/plos`], ["HP – PLO", `${v}/course-plo-matrix`], ["CLO – PLO", `${v}/clo-plo-matrix`], ["Kiểm tra dữ liệu", `${v}/validation`]]],
    ...(staff ? [["teaching", [["Lớp học phần", `${v}/offerings`], ["Lớp sinh hoạt", `${v}/home-classes`]]], ["assessment", [["Cơ cấu điểm", `${v}/assessment-scheme`]]]] as [DomainKey, Item[]][] : []),
  ];
  return <ScrollingNav label="Chức năng của phiên bản CTĐT">
    {groups.map(([domain, items]) => <div key={domain} className={`section-nav__group domain--${domain}`} role="group" aria-label={domains[domain].label}>
      <span className="section-nav__label" title={domains[domain].label}><DomainIcon domain={domain} size={14} /><span>{domain === "curriculum" ? "CTĐT" : domain === "teaching" ? "Đào tạo" : "Đánh giá"}</span></span>
      {items.map(([label, to, end]) => <NavLink key={to} to={to} end={end}>{label}</NavLink>)}
    </div>)}
  </ScrollingNav>;
}

/** Điều hướng trong một trường: ngành, tổ chức, sinh viên, quy chế. */
export function UniversityNav({ universityId, isAdmin }: { universityId: number; isAdmin: boolean }) {
  const u = `/curriculum/universities/${universityId}`;
  const groups: [DomainKey, Item[]][] = [
    ["curriculum", [["Ngành & CTĐT", u, true]]],
    ["teaching", [["Khoa / Bộ môn", `${u}/org-units`], ...(isAdmin ? [["Sinh viên", `${u}/students`]] as Item[] : [])]],
    ["assessment", [["Quy chế & thang điểm", `${u}/grading-policy`]]],
  ];
  return <ScrollingNav label="Chức năng của trường">
    {groups.map(([domain, items]) => <div key={domain} className={`section-nav__group domain--${domain}`} role="group" aria-label={domains[domain].label}>
      <span className="section-nav__label"><DomainIcon domain={domain} size={14} /></span>
      {items.map(([label, to, end]) => <NavLink key={to} to={to} end={end}>{label}</NavLink>)}
    </div>)}
  </ScrollingNav>;
}

/** Điều hướng trong một học phần: chuẩn đầu ra, đề cương đánh giá, các lớp học phần đã mở. */
export function CourseNav({ versionId, courseId, staff }: { versionId: number; courseId: number; staff: boolean }) {
  const c = `/curriculum/versions/${versionId}/courses/${courseId}`;
  const groups: [DomainKey, Item[]][] = [
    ["curriculum", [["CLO/PLO", `${c}/outcomes`]]],
    ...(staff ? [["assessment", [["Đề cương đánh giá", `${c}/assessment`]]], ["teaching", [["Lớp học phần", `${c}/offerings`]]]] as [DomainKey, Item[]][] : []),
  ];
  return <ScrollingNav label="Chức năng của học phần" className="section-nav section-nav--compact">
    {groups.map(([domain, items]) => <div key={domain} className={`section-nav__group domain--${domain}`}>
      <span className="section-nav__label"><DomainIcon domain={domain} size={14} /></span>
      {items.map(([label, to]) => <NavLink key={to} to={to}>{label}</NavLink>)}
    </div>)}
  </ScrollingNav>;
}
