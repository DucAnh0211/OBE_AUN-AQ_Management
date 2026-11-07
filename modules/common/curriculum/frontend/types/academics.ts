export type AssessmentComponent = { code: string; name: string; weight: number; maxScore: number };
export type AcademicPeriodBatch = { name: string; semesters: { name: string; period: string }[] };
export type AssessmentScheme = { versionId: number; revision: number; locked: boolean; components: AssessmentComponent[] };
export type Enrollment = { id: number; studentCode: string; fullName: string; academicPeriod: string; revision: number; gradedComponents: number; totalScore: number | null };
export type Gradebook = { enrollment: Enrollment; scheme: AssessmentScheme; scores: Record<string, number | null>; history: { componentCode: string; previousScore: number | null; newScore: number | null; actorId: number; changedAt: string; source: string }[] };
export type ImportRow = { rowNumber: number; studentCode: string; fullName: string; scores: Record<string, number | null>; expectedRevision?: number | null };
export type ImportRequest = { mode: string; academicPeriod: string; schemeRevision: number; rows: ImportRow[]; overwriteExisting: boolean; offeringId: number };
export type ImportPreview = { canApply: boolean; appliedRows: number; rows: { row: ImportRow; action: string; errors: string[] }[] };
