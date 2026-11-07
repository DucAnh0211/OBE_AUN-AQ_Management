import { apiBaseUrl } from "../../../../../shared/frontend/api-config";
import { apiJson, authenticatedFetch } from "../../../../../shared/frontend/auth-client";
import type { PagedResult } from "../types";
import type { AcademicPeriodBatch, AssessmentScheme, Enrollment, Gradebook, ImportPreview, ImportRequest, ImportRow } from "../types/academics";

const root = (version: number) => `/api/curriculum/program-versions/${version}`;
const course = (version: number, id: number) => `${root(version)}/offerings/${id}`;
export const academicApi = {
  periods: (v: number, c: number) => apiJson<AcademicPeriodBatch[]>(`${course(v, c)}/academic-periods`),
  scheme: (v: number) => apiJson<AssessmentScheme>(`${root(v)}/assessment-scheme`),
  saveScheme: (v: number, scheme: AssessmentScheme) => apiJson<AssessmentScheme>(`${root(v)}/assessment-scheme`, { method: "PUT", body: JSON.stringify(scheme) }),
  students: (v: number, c: number, period: string, page: number, search: string) => apiJson<PagedResult<Enrollment>>(`${course(v, c)}/students?${new URLSearchParams({ period, page: String(page), search })}`),
  enroll: (v: number, c: number, studentCode: string, fullName: string, academicPeriod: string) => apiJson<Enrollment>(`${course(v, c)}/students`, { method: "POST", body: JSON.stringify({ studentCode, fullName, academicPeriod }) }),
  grades: (v: number, c: number, e: number) => apiJson<Gradebook>(`${course(v, c)}/students/${e}/grades`),
  saveGrades: (v: number, c: number, e: number, book: Gradebook, scores: Record<string, number | null>) => apiJson<Gradebook>(`${course(v, c)}/students/${e}/grades`, { method: "PUT", body: JSON.stringify({ revision: book.enrollment.revision, schemeRevision: book.scheme.revision, scores }) }),
  read: (v: number, c: number, form: FormData) => apiJson<{ rows: ImportRow[]; schemeRevision: number; sheets: string[] }>(`${course(v, c)}/import/read`, { method: "POST", body: form }),
  sheets: (v: number, c: number, form: FormData) => apiJson<{ sheets: string[] }>(`${course(v, c)}/import/read`, { method: "POST", body: form }),
  preview: (v: number, c: number, request: ImportRequest) => apiJson<ImportPreview>(`${course(v, c)}/import/preview`, { method: "POST", body: JSON.stringify(request) }),
  apply: (v: number, c: number, request: ImportRequest) => apiJson<ImportPreview>(`${course(v, c)}/import/apply`, { method: "POST", body: JSON.stringify(request) }),
  template: async (v: number, c: number, mode: string) => {
    const response = await authenticatedFetch(`${apiBaseUrl}${course(v, c)}/import/template?mode=${mode}`);
    if (!response.ok) throw new Error("Không tải được mẫu import.");
    const url = URL.createObjectURL(await response.blob()); const link = document.createElement("a"); link.href = url; link.download = `${mode}-template.xlsx`; link.click(); setTimeout(() => URL.revokeObjectURL(url), 1000);
  },
};
