import type { FrontendModuleDefinition } from "../../../../shared/frontend/module";

export const reportingFrontend: FrontendModuleDefinition = {
  id: "reporting",
  label: "Báo cáo và trợ lý",
  shortLabel: "BC",
  owner: "SV5 - Chí Hoàng",
  route: "/reporting",
  summary: "Hỏi đáp có căn cứ, soạn báo cáo SAR, trích dẫn và xuất tài liệu.",
  features: ["Chat", "Trích dẫn", "Báo cáo SAR", "Xuất tài liệu"],
};
