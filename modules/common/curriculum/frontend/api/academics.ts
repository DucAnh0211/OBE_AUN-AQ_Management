import { apiBaseUrl } from "../../../../../shared/frontend/api-config";
import { apiJson, authenticatedFetch } from "../../../../../shared/frontend/auth-client";
import type { PagedResult } from "../types";
import type { AcademicPeriodBatch, AssessmentCoverage, AssessmentScheme, Enrollment, Gradebook, GradingPolicy, ImportPreview, ImportRequest, ImportRow } from "../types/academics";
import type { CourseOffering } from "../types/offerings";

const root = (version: number) => `/api/curriculum/program-versions/${version}`;
const course = (version: number, id: number) => `${root(version)}/offerings/${id}`;
const json = (method: string, body: unknown) => ({ method, body: JSON.stringify(body) });
export async function downloadFile(path: string, name: string) {
  const response = await authenticatedFetch(`${apiBaseUrl}${path}`);
  if (!response.ok) throw new Error("Không tải được mẫu import.");
  const url = URL.createObjectURL(await response.blob()); const link = document.createElement("a"); link.href = url; link.download = name; link.click(); setTimeout(() => URL.revokeObjectURL(url), 1000);
}
export const academicApi = {
  periods: (v: number, c: number) => apiJson<AcademicPeriodBatch[]>(`${course(v, c)}/academic-periods`),
  /** Cơ cấu điểm thuộc đề cương của từng học phần. */
  scheme: (v: number, courseId: number) => apiJson<AssessmentScheme>(`${root(v)}/courses/${courseId}/assessment-scheme`),
  saveScheme: (v: number, courseId: number, scheme: AssessmentScheme) => apiJson<AssessmentScheme>(`${root(v)}/courses/${courseId}/assessment-scheme`, json("PUT", { revision: scheme.revision, components: scheme.components })),
  offeringScheme: (v: number, c: number) => apiJson<AssessmentScheme>(`${course(v, c)}/assessment-scheme`),
  applyTemplate: (v: number, sourceCourseId: number) => apiJson<{ updatedCourses: number; skippedCourses: number }>(`${root(v)}/assessment-scheme/apply-template`, json("POST", { sourceCourseId })),
  coverage: (v: number) => apiJson<AssessmentCoverage[]>(`${root(v)}/assessment-coverage`),
  policy: (v: number) => apiJson<GradingPolicy>(`${root(v)}/grading-policy`),
  students: (v: number, c: number, page: number, search: string) => apiJson<PagedResult<Enrollment>>(`${course(v, c)}/students?${new URLSearchParams({ page: String(page), search })}`),
  enroll: (v: number, c: number, studentCode: string, fullName: string, academicPeriod: string) => apiJson<Enrollment>(`${course(v, c)}/students`, json("POST", { studentCode, fullName, academicPeriod })),
  grades: (v: number, c: number, e: number) => apiJson<Gradebook>(`${course(v, c)}/students/${e}/grades`),
  saveGrades: (v: number, c: number, e: number, book: Gradebook, scores: Record<string, number | null>, specialMark: string | null, reason: string | null) =>
    apiJson<Gradebook>(`${course(v, c)}/students/${e}/grades`, json("PUT", { revision: book.enrollment.revision, schemeRevision: book.scheme.revision, scores, specialMark, reason })),
  /** Nộp điểm (giảng viên chính), công bố hoặc trả lại (phòng đào tạo). */
  gradeStatus: (v: number, c: number, action: "submit" | "publish" | "reopen", reason: string | null) => apiJson<CourseOffering>(`${course(v, c)}/grades/${action}`, json("POST", { reason })),
  read: (v: number, c: number, form: FormData) => apiJson<{ rows: ImportRow[]; schemeRevision: number; sheets: string[] }>(`${course(v, c)}/import/read`, { method: "POST", body: form }),
  sheets: (v: number, c: number, form: FormData) => apiJson<{ sheets: string[] }>(`${course(v, c)}/import/read`, { method: "POST", body: form }),
  preview: (v: number, c: number, request: ImportRequest) => apiJson<ImportPreview>(`${course(v, c)}/import/preview`, json("POST", request)),
  apply: (v: number, c: number, request: ImportRequest) => apiJson<ImportPreview>(`${course(v, c)}/import/apply`, json("POST", request)),
  template: (v: number, c: number, mode: string) => downloadFile(`${course(v, c)}/import/template?mode=${mode}`, `${mode}-template.xlsx`),
};
