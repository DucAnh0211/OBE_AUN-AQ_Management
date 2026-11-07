import { KnowledgeBasePage } from "./admin/KnowledgeBasePage";
import { KnowledgeChatPage } from "./knowledge/KnowledgeChatPage";
import { KnowledgeSourcePage } from "./knowledge/KnowledgeSourcePage";
import { Navigate, Route, Routes, useLocation } from "react-router-dom";
import { GraduationCap } from "@phosphor-icons/react";
import type { FrontendModuleDefinition } from "../../../shared/frontend/module";
import { AccreditationFrameworkDetailPage } from "./accreditation/AccreditationFrameworkDetailPage";
import { AccreditationFrameworksPage } from "./accreditation/AccreditationFrameworksPage";
import { AdminUsersPage } from "./admin/AdminUsersPage";
import { ChangePasswordPage, LoginPage } from "./auth/AuthPages";
import { useAuth } from "./auth/AuthContext";
import { CourseOutcomesPage } from "./curriculum/CourseOutcomesPage";
import { ProgramCoursesPage, ProgramVersionsPage } from "./curriculum/CurriculumPages";
import { SubjectOfferingsPage } from "../../../modules/common/curriculum/frontend/pages/SubjectOfferingsPage";
import { AcademicClassesPage, AcademicClassPage } from "../../../modules/common/curriculum/frontend/pages/AcademicClassPages";
import { AssessmentSchemePage, CourseStudentsPage, StudentGradesPage } from "../../../modules/common/curriculum/frontend/pages/AcademicPages";
import { UniversitiesPage, UniversityProgramsPage } from "./curriculum/UniversityPages";
import {
  CloPloMatrixPage,
  CoursePloMatrixPage,
  DataValidationPage,
  PloListPage,
} from "./curriculum/OutcomeManagementPages";
import { webModules } from "./module-registry";
import { ReportingPage } from "../../../modules/sv5/reporting/frontend";
import { AppHeader, AppTabs } from "./shell/AppHeader";
import { Home } from "./shell/Home";

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

function ModulePage({ module }: { module: FrontendModuleDefinition }) {
  return <section className="admin-page">
    <header className="workspace-heading">
      <div><h1>{module.label}</h1><p>{module.summary}</p></div>
    </header>
    <div className="notice-panel">
      <span className="module-code">{module.shortLabel}</span>
      <div><h2>Phân hệ chưa có màn hình nghiệp vụ</h2><p>Các chức năng bên dưới chưa được triển khai trong phiên bản đang chạy.</p></div>
    </div>
    <div className="capability-list" aria-label="Phạm vi phân hệ">
      {module.features.map((feature, index) => <div key={feature}><span>{String(index + 1).padStart(2, "0")}</span><strong>{feature}</strong><small>Chưa triển khai</small></div>)}
    </div>
  </section>;
}

export function App() {
  const { loading, user, logout } = useAuth();
  const location = useLocation();

  if (loading) return <main className="auth-shell"><div className="session-loading"><GraduationCap size={30} weight="fill" aria-hidden="true" /><p>Đang khôi phục phiên đăng nhập…</p></div></main>;
  if (!user) return <LoginPage />;
  if (user.mustChangePassword) return <ChangePasswordPage />;

  const canChat = user.role !== "student";
  const isAdmin = user.role === "admin";
  const allowedModules = webModules.filter((module) =>
    module.id === "curriculum" ||
    (module.id === "accreditation" && user.role !== "student") ||
    (module.id === "reporting" && isAdmin));

  return <div className={`app-shell ${location.pathname === "/knowledge/chat" ? "app-shell--chat" : ""}`}>
    <AppHeader modules={allowedModules} canChat={canChat} isAdmin={isAdmin} fullName={user.fullName} email={user.email} roleLabel={roleLabels[user.role]} onLogout={() => void logout()} />
    <main className="workspace-content">
      <Routes>
        <Route path="/" element={<Home canAccredit={user.role !== "student"} canChat={canChat} isAdmin={isAdmin} roleDescription={roleDescriptions[user.role]} />} />
        {canChat && <Route path="/knowledge/chat" element={<KnowledgeChatPage />} />}
        {canChat && <Route path="/knowledge/sources/:documentId/:ordinal" element={<KnowledgeSourcePage />} />}
        <Route path="/curriculum/universities/:universityId" element={<UniversityProgramsPage />} />
        <Route path="/curriculum/programs/:programId" element={<ProgramVersionsPage />} />
        <Route path="/curriculum/versions/:versionId" element={<ProgramCoursesPage />} />
        {canChat && <Route path="/curriculum/versions/:versionId/assessment-scheme" element={<AssessmentSchemePage isAdmin={isAdmin} />} />}
        {canChat && <Route path="/curriculum/versions/:versionId/courses/:courseId/offerings" element={<SubjectOfferingsPage />} />}
        {canChat && <Route path="/curriculum/versions/:versionId/classes" element={<AcademicClassesPage isAdmin={isAdmin} />} />}
        {canChat && <Route path="/curriculum/versions/:versionId/classes/:classId" element={<AcademicClassPage isAdmin={isAdmin} />} />}
        {canChat && <Route path="/curriculum/versions/:versionId/offerings/:offeringId/students" element={<CourseStudentsPage />} />}
        {canChat && <Route path="/curriculum/versions/:versionId/offerings/:offeringId/students/:enrollmentId/grades" element={<StudentGradesPage />} />}
        <Route path="/curriculum/versions/:versionId/courses/:courseId/outcomes" element={<CourseOutcomesPage />} />
        <Route path="/curriculum/versions/:versionId/plos" element={<PloListPage />} />
        <Route path="/curriculum/versions/:versionId/course-plo-matrix" element={<CoursePloMatrixPage />} />
        <Route path="/curriculum/versions/:versionId/clo-plo-matrix" element={<CloPloMatrixPage />} />
        <Route path="/curriculum/versions/:versionId/validation" element={<DataValidationPage />} />
        {user.role !== "student" && <Route path="/accreditation/frameworks/:frameworkId" element={<AccreditationFrameworkDetailPage />} />}
        {isAdmin && <Route path="/admin/knowledge" element={<KnowledgeBasePage />} />}
        {isAdmin && <Route path="/admin/users" element={<AdminUsersPage />} />}
        {allowedModules.map((module) => <Route key={module.id} path={module.route} element={
          module.id === "curriculum"
            ? <UniversitiesPage />
            : module.id === "accreditation"
              ? <AccreditationFrameworksPage />
              : module.id === "reporting" ? <ReportingPage /> : <ModulePage module={module} />
        } />)}
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </main>
    <AppTabs modules={allowedModules} />
  </div>;
}
