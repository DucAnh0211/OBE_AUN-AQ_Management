import { useState, type FormEvent, type ReactNode } from "react";
import { useAuth } from "./AuthContext";

function AuthLayout({ eyebrow, title, description, children }: {
  eyebrow: string;
  title: string;
  description: string;
  children: ReactNode;
}) {
  return <main className="auth-shell">
    <section className="auth-layout">
      <aside className="auth-context">
        <div className="brand brand--inverse"><span className="brand__mark">OA</span><span><strong>OBE · AUN-QA</strong><small>Quản lý chất lượng đào tạo</small></span></div>
        <div><span className="auth-context__index">HỆ THỐNG NỘI BỘ</span><h2>Dữ liệu đào tạo có cấu trúc, minh bạch và nhất quán.</h2><p>Dành cho cán bộ quản lý, giảng viên và sinh viên thuộc chương trình đào tạo.</p></div>
        <small>Truy cập được ghi nhận nhằm bảo vệ dữ liệu học thuật.</small>
      </aside>
      <section className="auth-card">
        <header><span className="eyebrow">{eyebrow}</span><h1>{title}</h1><p>{description}</p></header>
        {children}
      </section>
    </section>
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

  return <AuthLayout eyebrow="ĐĂNG NHẬP" title="Truy cập hệ thống" description="Sử dụng tài khoản do quản trị viên cấp.">
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

  return <AuthLayout eyebrow="BẢO MẬT TÀI KHOẢN" title="Đổi mật khẩu tạm" description="Hoàn tất bước này trước khi sử dụng các phân hệ nghiệp vụ.">
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
