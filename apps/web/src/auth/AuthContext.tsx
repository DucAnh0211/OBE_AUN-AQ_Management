import { createContext, useContext, useEffect, useMemo, useState, type ReactNode } from "react";
import {
  changePassword as changePasswordRequest,
  getCurrentUser,
  login as loginRequest,
  logout as logoutRequest,
  refreshSession,
  subscribeSession,
  type AuthUser,
} from "../../../../shared/frontend/auth-client";

type AuthContextValue = {
  loading: boolean;
  user: AuthUser | null;
  login(email: string, password: string): Promise<void>;
  logout(): Promise<void>;
  changePassword(currentPassword: string, newPassword: string): Promise<void>;
};

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [loading, setLoading] = useState(true);
  const [user, setUser] = useState<AuthUser | null>(getCurrentUser());

  useEffect(() => {
    const unsubscribe = subscribeSession(setUser);
    void refreshSession().finally(() => setLoading(false));
    return unsubscribe;
  }, []);

  const value = useMemo<AuthContextValue>(() => ({
    loading,
    user,
    async login(email, password) { await loginRequest(email, password); },
    async logout() { await logoutRequest(); },
    async changePassword(currentPassword, newPassword) {
      await changePasswordRequest(currentPassword, newPassword);
    },
  }), [loading, user]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const value = useContext(AuthContext);
  if (!value) throw new Error("useAuth must be used inside AuthProvider");
  return value;
}
