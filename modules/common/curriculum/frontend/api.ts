import type {
  CourseInput,
  CurriculumProgram,
  PagedResult,
  ProblemDetails,
  ProgramCourse,
  ProgramVersion,
} from "./types";
import { apiBaseUrl } from "../../../../shared/frontend/api-config";

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
    response = await fetch(`${apiBaseUrl}${path}`, {
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
};
