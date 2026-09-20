type AppImportMeta = ImportMeta & {
  readonly env: {
    readonly DEV?: boolean;
    readonly VITE_API_URL?: string;
  };
};

const appEnv = (import.meta as AppImportMeta).env;
const configuredApiUrl = appEnv.VITE_API_URL;

export const apiBaseUrl = (
  configuredApiUrl !== undefined
    ? configuredApiUrl
    : appEnv.DEV
      ? "http://localhost:5099"
      : ""
).replace(/\/$/, "");
