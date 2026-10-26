export type FrameworkStatus = "draft" | "published" | "retired";
export type TranslationStatus = "official" | "reviewed" | "draft" | "machine_translated";

export type FrameworkSummary = {
  id: number;
  code: string;
  version: string;
  assessmentLevel: string;
  defaultLanguage: string;
  status: FrameworkStatus;
  sourceTitle: string;
  sourceUrl: string | null;
  criterionCount: number;
  requirementCount: number;
  publishedAt: string | null;
  retiredAt: string | null;
};

export type RequirementTree = {
  id: number;
  code: string;
  displayOrder: number;
  sourcePage: number | null;
  statement: string;
  language: string;
  translationStatus: string;
};

export type CriterionTree = {
  id: number;
  code: string;
  displayOrder: number;
  title: string;
  language: string;
  translationStatus: string;
  requirements: RequirementTree[];
};

export type FrameworkTree = {
  id: number;
  code: string;
  version: string;
  status: FrameworkStatus;
  requestedLanguage: string;
  defaultLanguage: string;
  criterionCount: number;
  requirementCount: number;
  criteria: CriterionTree[];
};

export type TranslationText = {
  title?: string | null;
  statement?: string | null;
  description?: string | null;
  guidance?: string | null;
  status?: TranslationStatus | null;
  source?: string | null;
};

export type CreateFrameworkInput = {
  code: string;
  version: string;
  assessmentLevel: "programme";
  defaultLanguage: string;
  sourceTitle: string;
  sourceUrl: string | null;
};

export type UpdateFrameworkInput = {
  defaultLanguage: string;
  sourceTitle: string;
  sourceUrl: string | null;
};

export type CriterionInput = {
  code: string;
  displayOrder: number;
  translations?: Record<string, TranslationText>;
};

export type RequirementInput = {
  code: string;
  displayOrder: number;
  sourcePage: number | null;
  translations?: Record<string, TranslationText>;
};

export type ImportCriterion = CriterionInput & { requirements: RequirementInput[] };
export type FrameworkImportInput = CreateFrameworkInput & { criteria: ImportCriterion[] };

export type ImportValidationResult = {
  valid: boolean;
  code: string | null;
  version: string | null;
  criterionCount: number;
  requirementCount: number;
  errors: Record<string, string[]>;
};

export type AccreditationProblem = {
  title?: string;
  detail?: string;
  status?: number;
  code?: string;
  errors?: Record<string, string[]>;
};
