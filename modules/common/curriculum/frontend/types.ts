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

export type ProblemDetails = {
  title?: string;
  detail?: string;
  status?: number;
  code?: string;
  errors?: Record<string, string[]>;
};
