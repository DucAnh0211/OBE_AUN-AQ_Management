import type {
  CourseInput,
  Clo,
  CloPloMapping,
  CoursePloMapping,
  PloBalance,
  PloCreditCheck,
  CurriculumProgram,
  PagedResult,
  ProblemDetails,
  ProgramCourse,
  ProgramVersion,
  Plo,
} from "./types";
import { apiBaseUrl } from "../../../../shared/frontend/api-config";
import { authenticatedFetch } from "../../../../shared/frontend/auth-client";

export { apiBaseUrl } from "../../../../shared/frontend/api-config";
export const swaggerUrl = `${apiBaseUrl}/swagger`;

export class ApiError extends Error {
  constructor(
    message: string,
    public readonly status: number,
    public readonly fieldErrors: Record<string, string[]> = {},
    public readonly code?: string,
  ) {
    super(message);
    this.name = "ApiError";
  }
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  let response: Response;
  try {
    response = await authenticatedFetch(`${apiBaseUrl}${path}`, {
      ...init,
      headers: {
        ...(init?.body ? { "Content-Type": "application/json" } : {}),
        ...init?.headers,
      },
    });
  } catch {
    throw new ApiError("Không thể kết nối đến backend. Hãy kiểm tra API đang chạy.", 0);
  }

  if (!response.ok) {
    let problem: ProblemDetails = {};
    try {
      problem = (await response.json()) as ProblemDetails;
    } catch {
      // Response không có JSON hợp lệ.
    }
    throw new ApiError(
      problem.detail ?? problem.title ?? `Yêu cầu thất bại (HTTP ${response.status}).`,
      response.status,
      problem.errors,
      problem.code,
    );
  }

  if (response.status === 204) {
    return undefined as T;
  }
  return (await response.json()) as T;
}

function queryString(values: Record<string, string | number | boolean | undefined>): string {
  const params = new URLSearchParams();
  Object.entries(values).forEach(([key, value]) => {
    if (value !== undefined && value !== "") params.set(key, String(value));
  });
  return params.toString();
}

const curriculumPath = "/api/curriculum";

export const curriculumApi = {
  listPrograms: (page: number, pageSize: number, search: string, includeArchived: boolean) =>
    request<PagedResult<CurriculumProgram>>(
      `${curriculumPath}/programs?${queryString({ page, pageSize, q: search, includeArchived })}`,
    ),
  getProgram: (id: number) => request<CurriculumProgram>(`${curriculumPath}/programs/${id}`),
  createProgram: (code: string, name: string) =>
    request<CurriculumProgram>(`${curriculumPath}/programs`, {
      method: "POST",
      body: JSON.stringify({ code, name }),
    }),
  updateProgram: (id: number, name: string) =>
    request<CurriculumProgram>(`${curriculumPath}/programs/${id}`, {
      method: "PUT",
      body: JSON.stringify({ name }),
    }),
  archiveProgram: (id: number) =>
    request<void>(`${curriculumPath}/programs/${id}`, { method: "DELETE" }),

  listVersions: (programId: number, includeArchived: boolean) =>
    request<ProgramVersion[]>(
      `${curriculumPath}/programs/${programId}/versions?${queryString({ includeArchived })}`,
    ),
  getVersion: (id: number) => request<ProgramVersion>(`${curriculumPath}/program-versions/${id}`),
  createVersion: (programId: number, versionCode: string, sourceVersionId: number | null) =>
    request<ProgramVersion>(`${curriculumPath}/programs/${programId}/versions`, {
      method: "POST",
      body: JSON.stringify({ versionCode, sourceVersionId }),
    }),
  updateVersion: (id: number, versionCode: string) =>
    request<ProgramVersion>(`${curriculumPath}/program-versions/${id}`, {
      method: "PUT",
      body: JSON.stringify({ versionCode }),
    }),
  publishVersion: (id: number) =>
    request<ProgramVersion>(`${curriculumPath}/program-versions/${id}/publish`, { method: "POST" }),
  archiveVersion: (id: number) =>
    request<void>(`${curriculumPath}/program-versions/${id}`, { method: "DELETE" }),

  listCourses: (
    versionId: number,
    page: number,
    pageSize: number,
    search: string,
    semester: string,
    includeArchived: boolean,
  ) =>
    request<PagedResult<ProgramCourse>>(
      `${curriculumPath}/program-versions/${versionId}/courses?${queryString({
        page,
        pageSize,
        q: search,
        semester,
        includeArchived,
      })}`,
    ),
  createCourse: (versionId: number, input: CourseInput) =>
    request<ProgramCourse>(`${curriculumPath}/program-versions/${versionId}/courses`, {
      method: "POST",
      body: JSON.stringify(input),
    }),
  updateCourse: (versionId: number, courseId: number, input: CourseInput) =>
    request<ProgramCourse>(`${curriculumPath}/program-versions/${versionId}/courses/${courseId}`, {
      method: "PUT",
      body: JSON.stringify(input),
    }),
  archiveCourse: (versionId: number, courseId: number) =>
    request<void>(`${curriculumPath}/program-versions/${versionId}/courses/${courseId}`, {
      method: "DELETE",
    }),
  getCourse: (versionId:number,courseId:number) => request<ProgramCourse>(`${curriculumPath}/program-versions/${versionId}/courses/${courseId}`),
  listPlos: (versionId:number) => request<Plo[]>(`${curriculumPath}/program-versions/${versionId}/plos`),
  createPlo: (versionId:number,input:{code:string;statement:string;levelCode:string|null}) => request<Plo>(`${curriculumPath}/program-versions/${versionId}/plos`,{method:"POST",body:JSON.stringify(input)}),
  updatePlo: (versionId:number,ploId:number,input:{code:string;statement:string;levelCode:string|null}) => request<Plo>(`${curriculumPath}/program-versions/${versionId}/plos/${ploId}`,{method:"PUT",body:JSON.stringify(input)}),
  deletePlo: (versionId:number,ploId:number) => request<void>(`${curriculumPath}/program-versions/${versionId}/plos/${ploId}`,{method:"DELETE"}),
  listClos: (versionId:number,courseId:number) => request<Clo[]>(`${curriculumPath}/program-versions/${versionId}/courses/${courseId}/clos`),
  createClo: (versionId:number,courseId:number,input:{code:string;statement:string;levelCode:string|null}) => request<Clo>(`${curriculumPath}/program-versions/${versionId}/courses/${courseId}/clos`,{method:"POST",body:JSON.stringify(input)}),
  updateClo: (versionId:number,courseId:number,cloId:number,input:{code:string;statement:string;levelCode:string|null}) => request<Clo>(`${curriculumPath}/program-versions/${versionId}/courses/${courseId}/clos/${cloId}`,{method:"PUT",body:JSON.stringify(input)}),
  deleteClo: (versionId:number,courseId:number,cloId:number) => request<void>(`${curriculumPath}/program-versions/${versionId}/courses/${courseId}/clos/${cloId}`,{method:"DELETE"}),
  listCoursePloMappings: (versionId:number,courseId:number) => request<CoursePloMapping[]>(`${curriculumPath}/program-versions/${versionId}/courses/${courseId}/plo-mappings`),
  addCoursePloMapping: (versionId:number,courseId:number,input:{ploId:number;weightCode:string;progressionCode:string;fit:string|null;fitReason:string|null}) => request<CoursePloMapping>(`${curriculumPath}/program-versions/${versionId}/courses/${courseId}/plo-mappings`,{method:"POST",body:JSON.stringify(input)}),
  updateCoursePloMapping: (versionId:number,courseId:number,mappingId:number,input:{weightCode:string;progressionCode:string;fit:string|null;fitReason:string|null}) => request<CoursePloMapping>(`${curriculumPath}/program-versions/${versionId}/courses/${courseId}/plo-mappings/${mappingId}`,{method:"PUT",body:JSON.stringify(input)}),
  deleteCoursePloMapping: (versionId:number,courseId:number,mappingId:number) => request<void>(`${curriculumPath}/program-versions/${versionId}/courses/${courseId}/plo-mappings/${mappingId}`,{method:"DELETE"}),
  listCloPloMappings: (versionId:number,courseId:number,cloId:number) => request<CloPloMapping[]>(`${curriculumPath}/program-versions/${versionId}/courses/${courseId}/clos/${cloId}/plo-mappings`),
  addCloPloMapping: (versionId:number,courseId:number,cloId:number,coursePloId:number) => request<CloPloMapping>(`${curriculumPath}/program-versions/${versionId}/courses/${courseId}/clos/${cloId}/plo-mappings`,{method:"POST",body:JSON.stringify({coursePloId})}),
  deleteCloPloMapping: (versionId:number,courseId:number,cloId:number,coursePloId:number) => request<void>(`${curriculumPath}/program-versions/${versionId}/courses/${courseId}/clos/${cloId}/plo-mappings/${coursePloId}`,{method:"DELETE"}),
  getPloCreditCheck: (versionId:number) => request<PloCreditCheck>(`${curriculumPath}/program-versions/${versionId}/plo-credit-check`),
  getPloBalance: (versionId:number) => request<PloBalance>(`${curriculumPath}/program-versions/${versionId}/plo-balance`),
};
