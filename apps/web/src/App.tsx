import { useEffect, useState } from "react";
import { Link, Navigate, NavLink, Route, Routes } from "react-router-dom";
import { apiBaseUrl } from "../../../shared/frontend/api-config";
import type { FrontendModuleDefinition } from "../../../shared/frontend/module";
import { AccreditationFrameworkDetailPage } from "./accreditation/AccreditationFrameworkDetailPage";
import { AccreditationFrameworksPage } from "./accreditation/AccreditationFrameworksPage";
import { AdminUsersPage } from "./admin/AdminUsersPage";
import { ChangePasswordPage, LoginPage } from "./auth/AuthPages";
import { useAuth } from "./auth/AuthContext";
import { CourseOutcomesPage } from "./curriculum/CourseOutcomesPage";
import {
  CurriculumProgramsPage,
  ProgramCoursesPage,
  ProgramVersionsPage,
} from "./curriculum/CurriculumPages";
import {
  CloPloMatrixPage,
  CoursePloMatrixPage,
  DataValidationPage,
  PloListPage,
} from "./curriculum/OutcomeManagementPages";
import { webModules } from "./module-registry";

type ApiState = "checking" | "online" | "offline";

const roleLabels = {
  admin: "Quản trị viên",
  lecturer: "Giảng viên",
  student: "Sinh viên",
} as const;

const roleDescriptions = {
  admin: "Quản lý tài khoản, CTĐT và dữ liệu kiểm định.",
  lecturer: "Theo dõi CTĐT và cập nhật CLO của học phần được phân công.",
  student: "Tra cứu CTĐT và chuẩn đầu ra đã công bố.",
} as const;

function Dashboard({ modules }: { modules: FrontendModuleDefinition[] }) {
  const { user } = useAuth();
  const [apiState, setApiState] = useState<ApiState>("checking");

  useEffect(() => {
    const controller = new AbortController();
    fetch(`${apiBaseUrl}/health`, { signal: controller.signal })
      .then((response) => {
        if (!response.ok) throw new Error("API unavailable");
        setApiState("online");
      })
      .catch((error: unknown) => {
        if (error instanceof DOMException && error.name === "AbortError") return;
        setApiState("offline");
      });
    return () => controller.abort();
  }, []);

  return <section className="dashboard">
    <header className="workspace-heading">
      <div>
        <span className="eyebrow">TỔNG QUAN</span>
        <h1>Không gian làm việc</h1>
        <p>Truy cập dữ liệu đào tạo và kiểm định theo phạm vi tài khoản của bạn.</p>
      </div>
      <div className={`service-state service-state--${apiState}`} role="status">
        <span className="status-dot" />
        <span>{apiState === "checking" ? "Đang kiểm tra kết nối" : apiState === "online" ? "Hệ thống hoạt động" : "Mất kết nối API"}</span>
      </div>
    </header>

    <div className="dashboard-layout">
      <div>
        <div className="section-heading"><div><h2>Phân hệ được phép truy cập</h2><p>Chọn phân hệ để tiếp tục công việc.</p></div><span>{modules.length} phân hệ</span></div>
        <div className="workspace-list">
          {modules.map((module) => <Link className="workspace-row" key={module.id} to={module.route}>
            <span className={`module-code module-code--${module.id}`}>{module.shortLabel}</span>
            <span className="workspace-row__content"><strong>{module.label}</strong><small>{module.summary}</small></span>
            <span className="workspace-row__meta">{module.features.slice(0, 2).join(" · ")}</span>
            <span className="workspace-row__arrow" aria-hidden="true">→</span>
          </Link>)}
        </div>
      </div>

      {user && <aside className="access-panel">
        <span className="eyebrow">PHẠM VI HIỆN TẠI</span>
        <h2>{roleLabels[user.role]}</h2>
        <p>{roleDescriptions[user.role]}</p>
        <dl>
          <div><dt>Tài khoản</dt><dd>{user.email}</dd></div>
          <div><dt>Trạng thái</dt><dd><span className="status status--active">Hoạt động</span></dd></div>
        </dl>
      </aside>}
    </div>
  </section>;
}

function ModulePage({ module }: { module: FrontendModuleDefinition }) {
  return <section className="admin-page">
    <header className="workspace-heading">
      <div><span className="eyebrow">{module.shortLabel}</span><h1>{module.label}</h1><p>{module.summary}</p></div>
    </header>
    <div className="notice-panel">
      <span className={`module-code module-code--${module.id}`}>{module.shortLabel}</span>
      <div><h2>Phân hệ chưa có màn hình nghiệp vụ</h2><p>Các chức năng bên dưới chưa được triển khai trong phiên bản đang chạy.</p></div>
    </div>
    <div className="capability-list" aria-label="Phạm vi phân hệ">
      {module.features.map((feature, index) => <div key={feature}><span>{String(index + 1).padStart(2, "0")}</span><strong>{feature}</strong><small>Chưa triển khai</small></div>)}
    </div>
  </section>;
}

export function App() {
  const { loading, user, logout } = useAuth();
  if (loading) return <main className="auth-shell"><div className="session-loading"><span className="brand__mark">OA</span><p>Đang khôi phục phiên đăng nhập…</p></div></main>;
  if (!user) return <LoginPage />;
  if (user.mustChangePassword) return <ChangePasswordPage />;

  const allowedModules = webModules.filter((module) =>
    module.id === "curriculum" ||
    (module.id === "accreditation" && user.role !== "student") ||
    (module.id === "reporting" && user.role === "admin"));

  return <div className="app-shell">
    <aside className="app-sidebar">
      <Link className="brand" to="/">
        <span className="brand__mark">OA</span>
        <span><strong>OBE · AUN-QA</strong><small>Quản lý chất lượng đào tạo</small></span>
      </Link>

      <nav className="sidebar-nav" aria-label="Điều hướng chính">
        <span className="nav-label">Không gian làm việc</span>
        <NavLink to="/" end><span className="nav-code">01</span><span>Tổng quan</span></NavLink>
        {allowedModules.map((module, index) => <NavLink key={module.id} to={module.route}>
          <span className="nav-code">{String(index + 2).padStart(2, "0")}</span><span>{module.label}</span>
        </NavLink>)}
        {user.role === "admin" && <><span className="nav-label nav-label--spaced">Quản trị</span><NavLink to="/admin/users"><span className="nav-code">QL</span><span>Tài khoản</span></NavLink></>}
      </nav>

      <div className="sidebar-user">
        <span className="user-avatar" aria-hidden="true">{user.fullName.trim().charAt(0).toUpperCase()}</span>
        <span><strong>{user.fullName}</strong><small>{roleLabels[user.role]}</small></span>
        <button type="button" onClick={() => void logout()}>Đăng xuất</button>
      </div>
    </aside>

    <div className="app-workspace">
      <header className="workspace-bar"><span>Hệ thống quản lý OBE và kiểm định AUN-QA</span><span className="workspace-bar__context">{roleLabels[user.role]}</span></header>
      <main className="workspace-content">
        <Routes>
          <Route path="/" element={<Dashboard modules={allowedModules} />} />
          <Route path="/curriculum/programs/:programId" element={<ProgramVersionsPage />} />
          <Route path="/curriculum/versions/:versionId" element={<ProgramCoursesPage />} />
          <Route path="/curriculum/versions/:versionId/courses/:courseId/outcomes" element={<CourseOutcomesPage />} />
          <Route path="/curriculum/versions/:versionId/plos" element={<PloListPage />} />
          <Route path="/curriculum/versions/:versionId/course-plo-matrix" element={<CoursePloMatrixPage />} />
          <Route path="/curriculum/versions/:versionId/clo-plo-matrix" element={<CloPloMatrixPage />} />
          <Route path="/curriculum/versions/:versionId/validation" element={<DataValidationPage />} />
          {user.role !== "student" && <Route path="/accreditation/frameworks/:frameworkId" element={<AccreditationFrameworkDetailPage />} />}
          {user.role === "admin" && <Route path="/admin/users" element={<AdminUsersPage />} />}
          {allowedModules.map((module) => <Route key={module.id} path={module.route} element={
            module.id === "curriculum"
              ? <CurriculumProgramsPage />
              : module.id === "accreditation"
                ? <AccreditationFrameworksPage />
                : <ModulePage module={module} />
          } />)}
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </main>
      <footer><span>OBE & AUN-QA Management</span><span>Dữ liệu theo phạm vi tài khoản</span></footer>
    </div>
  </div>;
}
