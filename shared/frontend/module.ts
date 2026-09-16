export type FrontendModuleDefinition = {
  id: "curriculum" | "accreditation" | "reporting";
  label: string;
  shortLabel: string;
  owner: string;
  route: string;
  summary: string;
  features: readonly string[];
};
