import type { FrontendModuleDefinition } from "../../../../shared/frontend/module";
export { ReportingPage } from "./ReportingPage";

export const reportingFrontend: FrontendModuleDefinition = {
  id: "reporting",
  label: "Báo cáo và trợ lý",
  shortLabel: "BC",
  owner: "SV5 - Chí Hoàng",
  route: "/reporting",
  summary: "Báo cáo học phần, mapping CLO/PLO và độ bao phủ chương trình; xuất DOCX/PDF.",
  features: ["Báo cáo học phần", "Mapping CLO/PLO", "Lịch sử báo cáo", "Xuất DOCX/PDF"],
};
