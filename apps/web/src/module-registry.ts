import { curriculumFrontend } from "../../../modules/common/curriculum/frontend";
import { accreditationFrontend } from "../../../modules/sv4/accreditation/frontend";
import { reportingFrontend } from "../../../modules/sv5/reporting/frontend";

export const webModules = [
  curriculumFrontend,
  accreditationFrontend,
  reportingFrontend,
] as const;
