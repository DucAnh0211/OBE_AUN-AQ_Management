import { apiBaseUrl } from "./api-config";

export type UserRole = "admin" | "lecturer" | "student" | "school_admin";
export type AuthUser = {
  id: number;
  universityId: number | null;
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
    const problem = await response.json() as { detail?: string; title?: string; errors?: Record<string, string[]> };
    const validation = Object.values(problem.errors ?? {}).flat().filter(value => typeof value === "string").slice(0, 4).join(" ");
    return new Error(validation || problem.detail || problem.title || `HTTP ${response.status}`);
  } catch {
    return new Error(`Yêu cầu thất bại (HTTP ${response.status}).`);
  }
}

export function subscribeSession(listener: SessionListener) {
  listeners.add(listener);
  return () => { listeners.delete(listener); };
}

export function getSelectedSchool(): number | null {
  if (currentUser?.role !== "admin") return currentUser?.universityId ?? null;
  const id = Number(localStorage.getItem(`obe-school-${currentUser.id}`)); return id > 0 ? id : null;
}
export function setSelectedSchool(id: number | null) {
  if(currentUser?.role !== "admin") return;
  if(id) localStorage.setItem(`obe-school-${currentUser.id}`,String(id)); else localStorage.removeItem(`obe-school-${currentUser.id}`);
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
  const school = getSelectedSchool();
  if (currentUser?.role === "admin" && school) headers.set("X-University-Id", String(school));
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
  if (init.body && !(init.body instanceof FormData)) headers.set("Content-Type", "application/json");
  const response = await authenticatedFetch(`${apiBaseUrl}${path}`, { ...init, headers });
  if (!response.ok) throw await parseError(response);
  if (response.status === 204) return undefined as T;
  return await response.json() as T;
}
