import { apiJson } from "../../../../../shared/frontend/auth-client";
import type { CurriculumProgram, PagedResult, University } from "../types";
import type { GradeBand, GradingPolicy, ImportPreview, ImportRow } from "../types/academics";
import type { Account, HomeClass, LecturerProfile, OrgUnit, Student, StudentStatus, Transcript } from "../types/directory";
const api = "/api/curriculum";
const json = (method: string, body: unknown) => ({ method, body: JSON.stringify(body) });
const homeClasses = (v: number) => `${api}/program-versions/${v}/home-classes`;
export const directoryApi = {
  universities: () => apiJson<PagedResult<University>>(`${api}/universities?page=1&pageSize=100`),
  orgUnits: (u: number, includeArchived = false) => apiJson<OrgUnit[]>(`${api}/universities/${u}/org-units?includeArchived=${includeArchived}`),
  saveOrgUnit: (u: number, id: number | null, input: { kind: string; code: string; name: string; parentId: number | null }) =>
    apiJson<OrgUnit>(id ? `${api}/universities/${u}/org-units/${id}` : `${api}/universities/${u}/org-units`, json(id ? "PUT" : "POST", input)),
  archiveOrgUnit: (u: number, id: number) => apiJson<void>(`${api}/universities/${u}/org-units/${id}`, { method: "DELETE" }),
  programs: (u: number) => apiJson<PagedResult<CurriculumProgram & { facultyId: number | null }>>(`${api}/programs?${new URLSearchParams({ universityId: String(u), page: "1", pageSize: "100" })}`),
  assignFaculty: (programId: number, orgUnitId: number | null) => apiJson<void>(`${api}/programs/${programId}/faculty`, json("PUT", { orgUnitId })),
  saveCourseCatalog: (v: number, courseId: number, departmentId: number | null, countsTowardGpa: boolean) =>
    apiJson<void>(`${api}/program-versions/${v}/courses/${courseId}/catalog`, json("PUT", { departmentId, countsTowardGpa })),
  policy: (u: number) => apiJson<GradingPolicy>(`${api}/universities/${u}/grading-policy`),
  savePolicy: (u: number, input: { courseScoreDecimals: number; minComponents: number; minFinalWeight: number | null; retakePolicy: string; gpaDecimals: number; bands: GradeBand[] }) =>
    apiJson<GradingPolicy>(`${api}/universities/${u}/grading-policy`, json("PUT", input)),
  lecturers: (search: string) => apiJson<LecturerProfile[]>(`${api}/lecturers?${new URLSearchParams({ search })}`),
  saveLecturer: (userId: number, input: { lecturerCode: string; departmentId: number | null; academicTitle: string | null; degree: string | null }) =>
    apiJson<LecturerProfile>(`${api}/lecturers/${userId}`, json("PUT", input)),
  homeClasses: (v: number, page: number, search: string) => apiJson<PagedResult<HomeClass>>(`${homeClasses(v)}?${new URLSearchParams({ page: String(page), search })}`),
  homeClass: (v: number, id: number) => apiJson<HomeClass>(`${homeClasses(v)}/${id}`),
  saveHomeClass: (v: number, id: number | null, input: { code: string; name: string; cohortCode: string | null; advisorUserId: number | null }) =>
    apiJson<HomeClass>(id ? `${homeClasses(v)}/${id}` : homeClasses(v), json(id ? "PUT" : "POST", input)),
  addToHomeClass: (v: number, id: number, studentCode: string, fullName: string) => apiJson<Student>(`${homeClasses(v)}/${id}/students`, json("POST", { studentCode, fullName })),
  readHomeClassImport: (v: number, id: number, form: FormData) => apiJson<{ rows: ImportRow[]; schemeRevision: number; sheets: string[] }>(`${homeClasses(v)}/${id}/students/import/read`, { method: "POST", body: form }),
  previewHomeClassImport: (v: number, id: number, rows: ImportRow[]) => apiJson<ImportPreview>(`${homeClasses(v)}/${id}/students/import/preview`, json("POST", { rows })),
  applyHomeClassImport: (v: number, id: number, rows: ImportRow[]) => apiJson<ImportPreview>(`${homeClasses(v)}/${id}/students/import/apply`, json("POST", { rows })),
  students: (u: number, page: number, search: string, homeClassId?: number, versionId?: number) => apiJson<PagedResult<Student>>(`${api}/universities/${u}/students?${new URLSearchParams({
    page: String(page), search, ...(homeClassId ? { homeClassId: String(homeClassId) } : {}), ...(versionId ? { versionId: String(versionId) } : {}) })}`),
  updateStudent: (u: number, id: number, input: { fullName: string; cohortCode: string | null; status: StudentStatus; homeClassId: number | null; userId: number | null }) =>
    apiJson<Student>(`${api}/universities/${u}/students/${id}`, json("PUT", input)),
  studentAccounts: (search: string) => apiJson<Account[]>(`${api}/student-accounts?${new URLSearchParams({ search })}`),
  transcript: (u: number, id: number) => apiJson<Transcript>(`${api}/universities/${u}/students/${id}/transcript`),
  myTranscript: () => apiJson<Transcript>(`${api}/me/transcript`),
};
