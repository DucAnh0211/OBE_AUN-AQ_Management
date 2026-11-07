import { ChartBar, GraduationCap, SealCheck, SquaresFour, type Icon } from "@phosphor-icons/react";

const moduleIcons: Record<string, Icon> = {
  curriculum: GraduationCap,
  accreditation: SealCheck,
  reporting: ChartBar,
};

export function moduleIcon(id: string): Icon {
  return moduleIcons[id] ?? SquaresFour;
}
