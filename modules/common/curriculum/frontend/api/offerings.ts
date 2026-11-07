import { apiJson } from "../../../../../shared/frontend/auth-client";
import type { PagedResult, ProgramCourse } from "../types";
import type { AcademicPeriodBatch } from "../types/academics";
import type { AcademicClass, CourseOffering, OfferingLecturer } from "../types/offerings";
const root = (v: number) => `/api/curriculum/program-versions/${v}`;
const post = (body: unknown) => ({ method: "POST", body: JSON.stringify(body) });
export const offeringApi = {
  periods: (v: number) => apiJson<AcademicPeriodBatch[]>(`${root(v)}/academic-periods`),
  classes: (v: number, period: string, page: number, search: string) => apiJson<PagedResult<AcademicClass>>(`${root(v)}/classes?${new URLSearchParams({ period, page: String(page), search })}`),
  class: (v: number, id: number) => apiJson<AcademicClass>(`${root(v)}/classes/${id}`),
  createClass: (v: number, academicPeriod: string, code: string, name: string) => apiJson<AcademicClass>(`${root(v)}/classes`, post({ academicPeriod, code, name })),
  offerings: (v: number, c: number) => apiJson<CourseOffering[]>(`${root(v)}/classes/${c}/offerings`),
  subjectOfferings: (v: number, c: number, page: number, search: string) => apiJson<PagedResult<CourseOffering>>(`${root(v)}/courses/${c}/offerings?${new URLSearchParams({ page: String(page), search })}`),
  offering: (v: number, id: number) => apiJson<CourseOffering>(`${root(v)}/offerings/${id}`),
  open: (v: number, c: number, programCourseId: number, lecturerId: number | null) => apiJson<CourseOffering>(`${root(v)}/classes/${c}/offerings`, post({ programCourseId, lecturerId })),
  subjects: (v: number, c: number, page: number, search: string) => apiJson<PagedResult<ProgramCourse>>(`${root(v)}/classes/${c}/available-courses?${new URLSearchParams({ page: String(page), search })}`),
  lecturers: (v: number, search: string) => apiJson<OfferingLecturer[]>(`${root(v)}/offering-lecturers?${new URLSearchParams({ search })}`),
  assign: (v: number, id: number, lecturerId: number | null) => apiJson<CourseOffering>(`${root(v)}/offerings/${id}/lecturer`, { method: "PUT", body: JSON.stringify({ lecturerId }) }),
};
