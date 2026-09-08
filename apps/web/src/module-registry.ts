/** Web composition metadata; concrete imports are added with the frontend stack. */
export const webModules = [
  {
    id: "curriculum",
    entryPoint: "modules/common/curriculum/frontend",
    responsibility: "common",
    owners: ["SV4", "SV5"],
  },
  {
    id: "accreditation",
    entryPoint: "modules/sv4/accreditation/frontend",
    responsibility: "individual",
    owners: ["SV4"],
  },
  {
    id: "reporting",
    entryPoint: "modules/sv5/reporting/frontend",
    responsibility: "individual",
    owners: ["SV5"],
  },
] as const;

export const webModuleIds = webModules.map(({ id }) => id);
