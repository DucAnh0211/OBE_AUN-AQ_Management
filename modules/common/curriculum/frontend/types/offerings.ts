export type GradeStatus = "draft" | "submitted" | "published";
export const gradeStatusLabels: Record<GradeStatus, string> = { draft: "Đang nhập điểm", submitted: "Giảng viên đã nộp", published: "Đã công bố" };
export type LecturerRole = "main" | "assistant" | "practice";
export const lecturerRoleLabels: Record<LecturerRole, string> = { main: "Giảng viên chính", assistant: "Trợ giảng", practice: "GV thực hành" };
export type OfferingLecturerAssignment = { userId: number; fullName: string | null; email: string | null; role: LecturerRole };
export type CourseOffering = { id: number; versionId: number; programCourseId: number; academicPeriod: string; code: string; groupName: string;
  subjectCode: string; subjectName: string; credits: number; lecturers: OfferingLecturerAssignment[]; readOnly: boolean; studentCount: number;
  capacity: number | null; targetHomeClassId: number | null; targetHomeClassCode: string | null; gradeStatus: GradeStatus; statusNote: string | null; publishedAt: string | null };
/** Tài khoản giảng viên đang hoạt động (danh bạ identity). */
export type OfferingLecturer = { id: number; fullName: string; email: string };
export type OfferingInput = { programCourseId: number; academicPeriod: string; groupName: string | null; capacity: number | null; targetHomeClassId: number | null;
  lecturers: { userId: number; role: LecturerRole }[] };
export type BulkEnrollResult = { added: number; skipped: number; messages: string[] };
export const mainLecturer = (o: CourseOffering) => o.lecturers.find(l => l.role === "main");
export const lecturerText = (o: CourseOffering) => o.lecturers.length
  ? o.lecturers.map(l => `${l.fullName ?? `ID ${l.userId}`}${l.role === "main" ? "" : ` (${lecturerRoleLabels[l.role]})`}`).join(", ") : "Chưa phân công";
