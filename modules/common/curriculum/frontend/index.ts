import type { FrontendModuleDefinition } from "../../../../shared/frontend/module";

export const curriculumFrontend: FrontendModuleDefinition = {
  id: "curriculum",
  label: "Trường đại học",
  shortLabel: "CTĐT",
  owner: "SV4 + SV5",
  route: "/curriculum",
  summary: "Chọn trường và ngành đã kiểm định để xem CTĐT, học phần, PLO, CLO và bảng đối sánh.",
  features: ["Phiên bản CTĐT", "Học phần", "PLO & CLO", "Curriculum Map"],
};
