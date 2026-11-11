import { apiJson } from "../../../../../shared/frontend/auth-client";
import type { PagedResult, ProgramCourse } from "../types";
import type { AcademicPeriodBatch } from "../types/academics";
import type { BulkEnrollResult, CourseOffering, LecturerRole, OfferingInput, OfferingLecturer } from "../types/offerings";
const root = (v: number) => `/api/curriculum/program-versions/${v}`;
const json = (method: string, body: unknown) => ({ method, body: JSON.stringify(body) });
export const offeringApi = {
  periods: (v: number) => apiJson<AcademicPeriodBatch[]>(`${root(v)}/academic-periods`),
  offerings: (v: number, period: string, page: number, search: string) => apiJson<PagedResult<CourseOffering>>(`${root(v)}/offerings?${new URLSearchParams({ period, page: String(page), search })}`),
  subjectOfferings: (v: number, c: number, page: number, search: string) => apiJson<PagedResult<CourseOffering>>(`${root(v)}/courses/${c}/offerings?${new URLSearchParams({ page: String(page), search })}`),
  offering: (v: number, id: number) => apiJson<CourseOffering>(`${root(v)}/offerings/${id}`),
  open: (v: number, input: OfferingInput) => apiJson<CourseOffering>(`${root(v)}/offerings`, json("POST", input)),
  update: (v: number, id: number, groupName: string, capacity: number | null, targetHomeClassId: number | null) =>
    apiJson<CourseOffering>(`${root(v)}/offerings/${id}`, json("PUT", { groupName, capacity, targetHomeClassId })),
  saveLecturers: (v: number, id: number, lecturers: { userId: number; role: LecturerRole }[]) => apiJson<CourseOffering>(`${root(v)}/offerings/${id}/lecturers`, json("PUT", { lecturers })),
  enrollHomeClass: (v: number, id: number, homeClassId: number) => apiJson<BulkEnrollResult>(`${root(v)}/offerings/${id}/enroll-home-class`, json("POST", { homeClassId })),
  subjects: (v: number, page: number, search: string) => apiJson<PagedResult<ProgramCourse>>(`${root(v)}/available-courses?${new URLSearchParams({ page: String(page), search })}`),
  lecturers: (v: number, search: string) => apiJson<OfferingLecturer[]>(`${root(v)}/offering-lecturers?${new URLSearchParams({ search })}`),
};
