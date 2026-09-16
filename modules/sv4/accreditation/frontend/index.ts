import type { FrontendModuleDefinition } from "../../../../shared/frontend/module";

export const accreditationFrontend: FrontendModuleDefinition = {
  id: "accreditation",
  label: "Kiểm định AUN-QA",
  shortLabel: "AUN",
  owner: "SV4 - Đức Anh",
  route: "/accreditation",
  summary: "Quản lý bộ tiêu chuẩn, tiêu chí, minh chứng và phân tích khoảng trống.",
  features: ["Tiêu chí AUN-QA", "Minh chứng", "Nạp tài liệu", "Gap Analysis"],
};
