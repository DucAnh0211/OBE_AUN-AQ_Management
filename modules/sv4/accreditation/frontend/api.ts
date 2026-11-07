import { apiBaseUrl } from "../../../../shared/frontend/api-config";
import { authenticatedFetch } from "../../../../shared/frontend/auth-client";
import type {
  AccreditationProblem,
  CreateFrameworkInput,
  CriterionInput,
  DirectoryProgram,
  DirectoryUniversity,
  PagedResult,
  ProgramAccreditation,
  ProgramAccreditationInput,
  FrameworkImportInput,
  FrameworkSummary,
  FrameworkTree,
  ImportValidationResult,
  RequirementInput,
  TranslationText,
  UpdateFrameworkInput,
} from "./types";

export class AccreditationApiError extends Error {
  constructor(
    message: string,
    public readonly status: number,
    public readonly fieldErrors: Record<string, string[]> = {},
    public readonly code?: string,
  ) {
    super(message);
    this.name = "AccreditationApiError";
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
    throw new AccreditationApiError("Không thể kết nối đến backend. Hãy kiểm tra API đang chạy.", 0);
  }
  if (!response.ok) {
    let problem: AccreditationProblem = {};
    try { problem = await response.json() as AccreditationProblem; } catch { /* Response không có JSON hợp lệ. */ }
    throw new AccreditationApiError(
      problem.detail ?? problem.title ?? `Yêu cầu thất bại (HTTP ${response.status}).`,
      response.status,
      problem.errors,
      problem.code,
    );
  }
  if (response.status === 204) return undefined as T;
  return await response.json() as T;
}

function query(values: Record<string, string | undefined>) {
  const params = new URLSearchParams();
  Object.entries(values).forEach(([key, value]) => { if (value) params.set(key, value); });
  return params.toString();
}

const base = "/api/accreditation";

export const accreditationApi = {
  directoryUniversities: (page: number, pageSize: number, search: string) =>
    request<PagedResult<DirectoryUniversity>>(`${base}/directory/universities?${query({ page: String(page), pageSize: String(pageSize), q: search })}`),
  directoryPrograms: (universityId: number, status: "accredited" | "all", page: number, pageSize: number, search: string) =>
    request<PagedResult<DirectoryProgram>>(`${base}/directory/universities/${universityId}/programs?${query({ status, page: String(page), pageSize: String(pageSize), q: search })}`),
  programAccreditations: (programId: number) =>
    request<ProgramAccreditation[]>(`${base}/directory/programs/${programId}/accreditations`),
  createProgramAccreditation: (programId: number, input: ProgramAccreditationInput) =>
    request<ProgramAccreditation>(`${base}/programs/${programId}/accreditations`, { method: "POST", body: JSON.stringify(input) }),
  updateProgramAccreditation: (id: number, input: ProgramAccreditationInput) =>
    request<ProgramAccreditation>(`${base}/program-accreditations/${id}`, { method: "PUT", body: JSON.stringify(input) }),
  deleteProgramAccreditation: (id: number) =>
    request<void>(`${base}/program-accreditations/${id}`, { method: "DELETE" }),
  listFrameworks: (status = "", search = "") =>
    request<FrameworkSummary[]>(`${base}/frameworks?${query({ status, q: search })}`),
  getFramework: (id: number) => request<FrameworkSummary>(`${base}/frameworks/${id}`),
  getTree: (id: number, language: string) =>
    request<FrameworkTree>(`${base}/frameworks/${id}/tree?${query({ language })}`),
  createFramework: (input: CreateFrameworkInput) =>
    request<FrameworkSummary>(`${base}/frameworks`, { method: "POST", body: JSON.stringify(input) }),
  updateFramework: (id: number, input: UpdateFrameworkInput) =>
    request<FrameworkSummary>(`${base}/frameworks/${id}`, { method: "PUT", body: JSON.stringify(input) }),
  deleteFramework: (id: number) =>
    request<void>(`${base}/frameworks/${id}`, { method: "DELETE" }),
  addCriterion: (frameworkId: number, input: CriterionInput) =>
    request<{ id: number }>(`${base}/frameworks/${frameworkId}/criteria`, { method: "POST", body: JSON.stringify(input) }),
  updateCriterion: (frameworkId: number, criterionId: number, input: CriterionInput) =>
    request<void>(`${base}/frameworks/${frameworkId}/criteria/${criterionId}`, { method: "PUT", body: JSON.stringify(input) }),
  deleteCriterion: (frameworkId: number, criterionId: number) =>
    request<void>(`${base}/frameworks/${frameworkId}/criteria/${criterionId}`, { method: "DELETE" }),
  upsertCriterionTranslation: (frameworkId: number, criterionId: number, language: string, input: TranslationText) =>
    request<void>(`${base}/frameworks/${frameworkId}/criteria/${criterionId}/translations/${language}`, { method: "PUT", body: JSON.stringify(input) }),
  addRequirement: (frameworkId: number, criterionId: number, input: RequirementInput) =>
    request<{ id: number }>(`${base}/frameworks/${frameworkId}/criteria/${criterionId}/requirements`, { method: "POST", body: JSON.stringify(input) }),
  updateRequirement: (frameworkId: number, requirementId: number, input: RequirementInput) =>
    request<void>(`${base}/frameworks/${frameworkId}/requirements/${requirementId}`, { method: "PUT", body: JSON.stringify(input) }),
  deleteRequirement: (frameworkId: number, requirementId: number) =>
    request<void>(`${base}/frameworks/${frameworkId}/requirements/${requirementId}`, { method: "DELETE" }),
  upsertRequirementTranslation: (frameworkId: number, requirementId: number, language: string, input: TranslationText) =>
    request<void>(`${base}/frameworks/${frameworkId}/requirements/${requirementId}/translations/${language}`, { method: "PUT", body: JSON.stringify(input) }),
  previewImport: (input: FrameworkImportInput) =>
    request<ImportValidationResult>(`${base}/framework-imports/preview`, { method: "POST", body: JSON.stringify(input) }),
  importFramework: (input: FrameworkImportInput) =>
    request<FrameworkSummary>(`${base}/framework-imports`, { method: "POST", body: JSON.stringify(input) }),
  publishFramework: (id: number) =>
    request<FrameworkSummary>(`${base}/frameworks/${id}/publish`, { method: "POST" }),
  retireFramework: (id: number) =>
    request<FrameworkSummary>(`${base}/frameworks/${id}/retire`, { method: "POST" }),
};
