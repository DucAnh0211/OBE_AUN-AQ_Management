import { useEffect, useRef, useState } from "react";
import { Link, NavLink } from "react-router-dom";
import { Books, CaretDown, GraduationCap, House, SignOut, Sparkle, UsersThree } from "@phosphor-icons/react";
import type { FrontendModuleDefinition } from "../../../../shared/frontend/module";
import { moduleIcon } from "./module-icons";

const tabLabels: Record<string, string> = { curriculum: "Trường", accreditation: "Kiểm định", reporting: "Báo cáo" };

function AccountMenu({ fullName, email, roleLabel, isAdmin, onLogout }: {
  fullName: string;
  email: string;
  roleLabel: string;
  isAdmin: boolean;
  onLogout: () => void;
}) {
  const [open, setOpen] = useState(false);
  const container = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!open) return;
    function closeOutside(event: MouseEvent) { if (!container.current?.contains(event.target as Node)) setOpen(false); }
    function closeOnEscape(event: KeyboardEvent) { if (event.key === "Escape") setOpen(false); }
    document.addEventListener("mousedown", closeOutside);
    document.addEventListener("keydown", closeOnEscape);
    return () => { document.removeEventListener("mousedown", closeOutside); document.removeEventListener("keydown", closeOnEscape); };
  }, [open]);

  return <div className="account-menu" ref={container}>
    <button className="account-menu__trigger" type="button" aria-haspopup="menu" aria-expanded={open} aria-label={`Tài khoản ${fullName}`} title={fullName} onClick={() => setOpen(value => !value)}>
      <span className="user-avatar" aria-hidden="true">{fullName.trim().charAt(0).toUpperCase()}</span>
      <CaretDown size={13} aria-hidden="true" />
    </button>
    {open && <div className="account-menu__panel" role="menu" aria-label="Tài khoản">
      <div className="account-menu__identity"><strong>{fullName}</strong><small>{roleLabel}</small><small title={email}>{email}</small></div>
      {isAdmin && <>
        <Link className="account-menu__item" role="menuitem" to="/admin/knowledge" onClick={() => setOpen(false)}><Books size={17} aria-hidden="true" />Knowledge Base</Link>
        <Link className="account-menu__item" role="menuitem" to="/admin/users" onClick={() => setOpen(false)}><UsersThree size={17} aria-hidden="true" />Quản lý tài khoản</Link>
      </>}
      <button className="account-menu__item" role="menuitem" type="button" onClick={onLogout}><SignOut size={17} aria-hidden="true" />Đăng xuất</button>
    </div>}
  </div>;
}

export function AppHeader({ modules, canChat, isAdmin, fullName, email, roleLabel, onLogout }: {
  modules: readonly FrontendModuleDefinition[];
  canChat: boolean;
  isAdmin: boolean;
  fullName: string;
  email: string;
  roleLabel: string;
  onLogout: () => void;
}) {
  return <header className="app-header">
    <div className="app-header__inner">
      <Link className="brand" to="/" aria-label="OBE · AUN-QA, về trang tổng quan"><GraduationCap size={22} weight="fill" aria-hidden="true" /><strong>OBE · AUN-QA</strong></Link>
      <nav className="main-nav" aria-label="Điều hướng chính">
        <NavLink to="/" end>Tổng quan</NavLink>
        {modules.map(module => <NavLink key={module.id} to={module.route}>{module.label}</NavLink>)}
      </nav>
      <div className="app-header__actions">
        {canChat && <NavLink className="assistant-link" to="/knowledge/chat" title="Trợ lý tài liệu" aria-label="Trợ lý tài liệu"><Sparkle size={16} weight="fill" aria-hidden="true" /><span>Trợ lý</span></NavLink>}
        <AccountMenu fullName={fullName} email={email} roleLabel={roleLabel} isAdmin={isAdmin} onLogout={onLogout} />
      </div>
    </div>
  </header>;
}

/** Phone and tablet navigation: every destination stays one tap away, nothing is hidden in a menu. */
export function AppTabs({ modules }: { modules: readonly FrontendModuleDefinition[] }) {
  return <nav className="app-tabs" aria-label="Điều hướng chính trên thiết bị di động">
    <NavLink to="/" end><House size={21} aria-hidden="true" /><span>Tổng quan</span></NavLink>
    {modules.map(module => {
      const TabIcon = moduleIcon(module.id);
      return <NavLink key={module.id} to={module.route}><TabIcon size={21} aria-hidden="true" /><span>{tabLabels[module.id] ?? module.shortLabel}</span></NavLink>;
    })}
  </nav>;
}
