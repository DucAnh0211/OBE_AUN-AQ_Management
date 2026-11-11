export type ComponentKind = "attendance" | "process" | "midterm" | "practice" | "project" | "final";
export const componentKinds: { value: ComponentKind; label: string }[] = [
  { value: "attendance", label: "Chuyên cần" }, { value: "process", label: "Quá trình" }, { value: "midterm", label: "Giữa kỳ" },
  { value: "practice", label: "Thực hành" }, { value: "project", label: "Đồ án / tiểu luận" }, { value: "final", label: "Cuối kỳ" },
];
/** clos: mã CLO → % đóng góp của đầu điểm vào CLO đó. */
export type AssessmentComponent = { code: string; name: string; weight: number; maxScore: number; kind: ComponentKind; clos: Record<string, number> | null };
export type AcademicPeriodBatch = { name: string; semesters: { name: string; period: string }[] };
export type AssessmentScheme = { courseId: number; revision: number; locked: boolean; components: AssessmentComponent[]; cloCodes: string[] | null };
export type AssessmentCoverage = { courseId: number; courseCode: string; courseName: string; componentCount: number; unmappedComponents: string[]; unassessedClos: string[] };
export type GradeBand = { letter: string; minScore: number; point4: number; isPassing: boolean };
export type GradingPolicy = { universityId: number; courseScoreDecimals: number; minComponents: number; minFinalWeight: number | null; retakePolicy: "highest" | "latest"; gpaDecimals: number; bands: GradeBand[] };
export type AttemptKind = "first" | "retake" | "improve";
export const attemptLabels: Record<AttemptKind, string> = { first: "Lần đầu", retake: "Học lại", improve: "Cải thiện" };
export const specialMarks: { value: string; label: string }[] = [
  { value: "", label: "Không" }, { value: "I", label: "I – Chưa hoàn thiện" }, { value: "R", label: "R – Miễn / bảo lưu" }, { value: "P", label: "P – Đạt (học phần điều kiện)" },
];
export type Enrollment = { id: number; studentCode: string; fullName: string; academicPeriod: string; revision: number; gradedComponents: number; totalScore: number | null;
  letterGrade: string | null; gradePoint: number | null; passed: boolean | null; specialMark: string | null; attemptKind: AttemptKind; homeClassCode: string | null; published: boolean };
export type GradeChange = { componentCode: string; previousScore: number | null; newScore: number | null; actorId: number; changedAt: string; source: string; reason: string | null };
export type Gradebook = { enrollment: Enrollment; scheme: AssessmentScheme; scores: Record<string, number | null>; history: GradeChange[] };
export type ImportRow = { rowNumber: number; studentCode: string; fullName: string; scores: Record<string, number | null>; expectedRevision?: number | null };
export type ImportRequest = { mode: string; academicPeriod: string; schemeRevision: number; rows: ImportRow[]; overwriteExisting: boolean; offeringId: number };
export type ImportPreview = { canApply: boolean; appliedRows: number; rows: { row: ImportRow; action: string; errors: string[] }[] };
