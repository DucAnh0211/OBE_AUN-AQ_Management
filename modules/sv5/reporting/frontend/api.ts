import { apiBaseUrl } from "../../../../shared/frontend/api-config";
import { authenticatedFetch } from "../../../../shared/frontend/auth-client";
import type { CurriculumProgram, ProgramVersion, ProgramCourse } from "../../../common/curriculum/frontend/types";

export type ReportKind = "course" | "clo" | "plo";
export type ReportSession = { userId: string; programIds: number[]; canGenerate: boolean };
export type ReportSummary = {
  id: string; kind: ReportKind; title: string; programId: number; versionId: number;
  createdAt: string; createdBy: string; snapshotHash: string;
};
export type ReportDocument = ReportSummary & {
  programCourseId: number | null; programCode: string; programName: string; versionCode: string;
  versionStatus: string; ruleVersion: string;
  metrics: { label: string; numerator: number; denominator: number; percentage: number | null }[];
  findings: string[]; notes: string[];
  tables: { title: string; headers: string[]; rows: string[][] }[];
};
export type ReportPage = { items: ReportSummary[]; page: number; pageSize: number; totalItems: number };

export class ReportApiError extends Error {
  constructor(public status: number, message: string) { super(message); }
}

export function reportingApi(token: string) {
  async function response(path: string, init?: RequestInit) {
    const result = await (token ? fetch : authenticatedFetch)(`${apiBaseUrl}/api/reporting${path}`, {
      ...init, cache: "no-store", headers: { "Content-Type": "application/json", ...init?.headers, ...(token ? { Authorization: `Bearer ${token}` } : {}) },
    });
    if (!result.ok) {
      const defaults: Record<number, string> = {
        401: "Mã truy cập không hợp lệ hoặc đã bị thu hồi. Vui lòng kết nối lại.",
        403: "Bạn không có quyền thực hiện thao tác này.",
        404: "Dữ liệu không còn khả dụng hoặc ngoài phạm vi truy cập.",
        429: "Bạn đã gửi nhiều yêu cầu. Vui lòng thử lại sau một phút.",
      };
      const body = await result.json().catch(() => null) as { detail?: string; title?: string } | null;
      throw new ReportApiError(result.status, body?.detail ?? defaults[result.status] ?? body?.title ?? "Không thể xử lý yêu cầu. Vui lòng thử lại.");
    }
    return result;
  }
  async function json<T>(path: string, init?: RequestInit): Promise<T> { return (await response(path, init)).json() as Promise<T>; }
  return {
    session: () => json<ReportSession>("/session"),
    programs: (signal?: AbortSignal) => json<CurriculumProgram[]>("/programs", { signal }),
    versions: (programId: number, signal?: AbortSignal) => json<ProgramVersion[]>(`/programs/${programId}/versions`, { signal }),
    courses: (versionId: number, signal?: AbortSignal) => json<ProgramCourse[]>(`/versions/${versionId}/courses`, { signal }),
    create: (kind: ReportKind, versionId: number, programCourseId: number | null) =>
      json<ReportDocument>("/reports", { method: "POST", body: JSON.stringify({ kind, versionId, programCourseId }) }),
    list: (page: number, signal?: AbortSignal) => json<ReportPage>(`/reports?page=${page}&pageSize=10`, { signal }),
    get: (id: string, signal?: AbortSignal) => json<ReportDocument>(`/reports/${encodeURIComponent(id)}`, { signal }),
    revoke: async (id: string) => { await response(`/reports/${encodeURIComponent(id)}/revoke`, { method: "POST" }); },
    download: async (id: string, format: "docx" | "pdf") => (await response(`/reports/${encodeURIComponent(id)}/download?format=${format}`)).blob(),
  };
}
