import { useState, type FormEvent, type ReactNode } from "react";
import { GraduationCap } from "@phosphor-icons/react";
import { useAuth } from "./AuthContext";

function AuthLayout({ title, description, children }: {
  title: string;
  description: string;
  children: ReactNode;
}) {
  return <main className="auth-shell">
    <div className="auth-column">
      <div className="auth-brand"><GraduationCap size={26} weight="fill" aria-hidden="true" /><strong>OBE · AUN-QA</strong></div>
      <header className="auth-heading"><h1>{title}</h1><p>{description}</p></header>
      <section className="auth-card">{children}</section>
      <small className="auth-foot">Truy cập được ghi nhận nhằm bảo vệ dữ liệu học thuật.</small>
    </div>
  </main>;
}

export function LoginPage() {
  const { login } = useAuth();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [busy, setBusy] = useState(false);

  async function submit(event: FormEvent) {
    event.preventDefault(); setBusy(true); setError("");
    try { await login(email, password); }
    catch (caught) { setError(caught instanceof Error ? caught.message : "Không thể đăng nhập. Thử lại sau."); }
    finally { setBusy(false); }
  }

  return <AuthLayout title="Dữ liệu đào tạo minh bạch và nhất quán" description="Sử dụng tài khoản do quản trị viên cấp.">
    <form className="admin-form auth-form" onSubmit={submit}>
      {error && <div className="alert alert--error" role="alert">{error}</div>}
      <label><span>Email</span><input type="email" autoComplete="username" required autoFocus value={email} onChange={(event) => setEmail(event.target.value)} placeholder="ten@truong.edu.vn" /></label>
      <label><span>Mật khẩu</span><input type="password" autoComplete="current-password" required value={password} onChange={(event) => setPassword(event.target.value)} /></label>
      <button className="button button--primary button--block" disabled={busy}>{busy ? "Đang đăng nhập…" : "Đăng nhập"}</button>
    </form>
  </AuthLayout>;
}

export function ChangePasswordPage() {
  const { changePassword, logout } = useAuth();
  const [currentPassword, setCurrentPassword] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [confirm, setConfirm] = useState("");
  const [error, setError] = useState("");
  const [busy, setBusy] = useState(false);

  async function submit(event: FormEvent) {
    event.preventDefault();
    if (newPassword !== confirm) { setError("Mật khẩu xác nhận không khớp."); return; }
    setBusy(true); setError("");
    try { await changePassword(currentPassword, newPassword); }
    catch (caught) { setError(caught instanceof Error ? caught.message : "Không thể đổi mật khẩu. Thử lại sau."); }
    finally { setBusy(false); }
  }

  return <AuthLayout title="Đổi mật khẩu tạm" description="Hoàn tất bước này trước khi sử dụng các phân hệ nghiệp vụ.">
    <div className="password-requirements" aria-label="Yêu cầu mật khẩu"><span>Tối thiểu 10 ký tự</span><span>Có chữ hoa và chữ thường</span><span>Có ít nhất một chữ số</span></div>
    <form className="admin-form auth-form" onSubmit={submit}>
      {error && <div className="alert alert--error" role="alert">{error}</div>}
      <label><span>Mật khẩu hiện tại</span><input type="password" autoComplete="current-password" required value={currentPassword} onChange={(event) => setCurrentPassword(event.target.value)} /></label>
      <label><span>Mật khẩu mới</span><input type="password" autoComplete="new-password" minLength={10} required value={newPassword} onChange={(event) => setNewPassword(event.target.value)} /></label>
      <label><span>Xác nhận mật khẩu mới</span><input type="password" autoComplete="new-password" minLength={10} required value={confirm} onChange={(event) => setConfirm(event.target.value)} /></label>
      <div className="auth-form__actions"><button className="button button--primary" disabled={busy}>{busy ? "Đang lưu…" : "Cập nhật mật khẩu"}</button><button className="button button--ghost" type="button" onClick={() => void logout()}>Đăng xuất</button></div>
    </form>
  </AuthLayout>;
}
