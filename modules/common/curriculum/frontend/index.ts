import type { FrontendModuleDefinition } from "../../../../shared/frontend/module";

export const curriculumFrontend: FrontendModuleDefinition = {
  id: "curriculum",
  label: "Chương trình đào tạo",
  shortLabel: "CTĐT",
  owner: "SV4 + SV5",
  route: "/curriculum",
  summary: "Quản lý phiên bản CTĐT, học phần, PLO, CLO và bảng đối sánh.",
  features: ["Phiên bản CTĐT", "Học phần", "PLO & CLO", "Curriculum Map"],
};
