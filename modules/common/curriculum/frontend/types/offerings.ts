export type AcademicClass = { id: number; versionId: number; academicPeriod: string; code: string; name: string; offeringCount: number };
export type CourseOffering = { id: number; classId: number; versionId: number; programCourseId: number; academicPeriod: string; code: string; classCode: string; className: string; subjectCode: string; subjectName: string; credits: number; lecturerId: number | null; lecturerName: string | null; readOnly: boolean; studentCount: number };
export type OfferingLecturer = { id: number; fullName: string; email: string };
