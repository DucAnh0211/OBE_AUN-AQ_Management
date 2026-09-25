import { apiBaseUrl } from "./api-config";

export type UserRole = "admin" | "lecturer" | "student";
export type AuthUser = {
  id: number;
  email: string;
  fullName: string;
  role: UserRole;
  status: "active" | "disabled";
  mustChangePassword: boolean;
  lastLoginAt: string | null;
  createdAt: string;
};

type SessionResponse = { accessToken: string; expiresAt: string; user: AuthUser };
type SessionListener = (user: AuthUser | null) => void;

let accessToken: string | null = null;
let currentUser: AuthUser | null = null;
let refreshPromise: Promise<AuthUser | null> | null = null;
const listeners = new Set<SessionListener>();

function setSession(session: SessionResponse | null) {
  accessToken = session?.accessToken ?? null;
  currentUser = session?.user ?? null;
  listeners.forEach((listener) => listener(currentUser));
}

async function parseError(response: Response): Promise<Error> {
  try {
    const problem = await response.json() as { detail?: string; title?: string };
    return new Error(problem.detail ?? problem.title ?? `HTTP ${response.status}`);
  } catch {
    return new Error(`Yêu cầu thất bại (HTTP ${response.status}).`);
  }
}

export function subscribeSession(listener: SessionListener) {
  listeners.add(listener);
  return () => { listeners.delete(listener); };
}

export function getCurrentUser() { return currentUser; }

export async function login(email: string, password: string): Promise<AuthUser> {
  const response = await fetch(`${apiBaseUrl}/api/auth/login`, {
    method: "POST", credentials: "include", headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email, password }),
  });
  if (!response.ok) throw await parseError(response);
  const session = await response.json() as SessionResponse;
  setSession(session);
  return session.user;
}

export async function refreshSession(): Promise<AuthUser | null> {
  if (refreshPromise) return refreshPromise;
  refreshPromise = (async () => {
    const response = await fetch(`${apiBaseUrl}/api/auth/refresh`, {
      method: "POST", credentials: "include",
    });
    if (!response.ok) { setSession(null); return null; }
    const session = await response.json() as SessionResponse;
    setSession(session);
    return session.user;
  })().finally(() => { refreshPromise = null; });
  return refreshPromise;
}

export async function logout(): Promise<void> {
  await fetch(`${apiBaseUrl}/api/auth/logout`, {
    method: "POST", credentials: "include",
    headers: accessToken ? { Authorization: `Bearer ${accessToken}` } : {},
  });
  setSession(null);
}

export async function changePassword(currentPassword: string, newPassword: string): Promise<void> {
  const response = await authenticatedFetch(`${apiBaseUrl}/api/auth/change-password`, {
    method: "POST", headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ currentPassword, newPassword }),
  }, false);
  if (!response.ok) throw await parseError(response);
  const session = await response.json() as SessionResponse;
  setSession(session);
}

export async function authenticatedFetch(
  input: RequestInfo | URL,
  init: RequestInit = {},
  retry = true,
): Promise<Response> {
  const headers = new Headers(init.headers);
  if (accessToken) headers.set("Authorization", `Bearer ${accessToken}`);
  let response = await fetch(input, { ...init, headers, credentials: "include" });
  if (response.status === 401 && retry) {
    const user = await refreshSession();
    if (user && accessToken) {
      headers.set("Authorization", `Bearer ${accessToken}`);
      response = await fetch(input, { ...init, headers, credentials: "include" });
    }
  }
  return response;
}

export async function apiJson<T>(path: string, init: RequestInit = {}): Promise<T> {
  const headers = new Headers(init.headers);
  if (init.body) headers.set("Content-Type", "application/json");
  const response = await authenticatedFetch(`${apiBaseUrl}${path}`, { ...init, headers });
  if (!response.ok) throw await parseError(response);
  if (response.status === 204) return undefined as T;
  return await response.json() as T;
}
