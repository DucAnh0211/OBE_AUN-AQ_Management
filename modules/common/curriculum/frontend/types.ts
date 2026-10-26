export type PagedResult<T> = {
  items: T[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
};

export type CurriculumProgram = {
  id: number;
  code: string;
  name: string;
  isArchived: boolean;
  versionCount: number;
  currentVersionId: number | null;
  currentVersionCode: string | null;
  createdAt: string;
  updatedAt: string;
};

export type ProgramVersion = {
  id: number;
  programId: number;
  versionCode: string;
  status: "draft" | "finalized" | "archived";
  isCurrent: boolean;
  sourceVersionId: number | null;
  courseCount: number;
  publishedAt: string | null;
  archivedAt: string | null;
  createdAt: string;
  updatedAt: string;
};

export type ProgramCourse = {
  id: number;
  programVersionId: number;
  courseId: number;
  institutionalCode: string | null;
  name: string;
  credits: number;
  semester: string | null;
  displayOrder: number;
  status: string;
  sourceRow: number | null;
  archivedAt: string | null;
  createdAt: string;
  updatedAt: string;
};

export type CourseInput = {
  institutionalCode: string | null;
  name: string;
  credits: number;
  semester: string;
  displayOrder: number | null;
};

export type Plo = { id:number; programVersionId:number; code:string; statement:string; levelCode:string|null; provenance:string };
export type Clo = { id:number; programCourseId:number; code:string; statement:string; levelCode:string|null; provenance:string; status:string };
export type CoursePloMapping = { id:number; programVersionId:number; programCourseId:number; ploId:number; ploCode:string; weightCode:string; progressionCode:string; provenance:string; fit:string|null; fitReason:string|null };
export type CloPloMapping = { cloId:number; cloCode:string; coursePloId:number; ploId:number; ploCode:string };

export type CoursePloCreditCheck = {
  programCourseId: number;
  institutionalCode: string | null;
  courseName: string;
  credits: number;
  requiredPloCount: number;
  actualPloCount: number;
  isValid: boolean;
};

export type PloCreditCheck = {
  programVersionId: number;
  maximumRequiredPloCount: number;
  isValid: boolean;
  violationCount: number;
  courses: CoursePloCreditCheck[];
};

export type PloBalanceItem = {
  ploCode: string;
  score: number;
  meanScore: number;
  deviation: number | null;
  exceedsTwentyPercent: boolean;
};

export type PloBalance = {
  programVersionId: number;
  allowedDeviation: number;
  isBalanced: boolean;
  items: PloBalanceItem[];
};

export type ProblemDetails = {
  title?: string;
  detail?: string;
  status?: number;
  code?: string;
  errors?: Record<string, string[]>;
};
