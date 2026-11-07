import { useEffect, useState, type ReactNode } from "react";
import { Link } from "react-router-dom";
import { ArrowRight, Books, CaretRight, ChartBar, GraduationCap, SealCheck, Sparkle, UsersThree, type Icon } from "@phosphor-icons/react";
import type { PagedResult } from "../../../../modules/common/curriculum/frontend/types";
import { accreditationApi } from "../../../../modules/sv4/accreditation/frontend";
import type { DirectoryUniversity, FrameworkStatus, FrameworkSummary } from "../../../../modules/sv4/accreditation/frontend/types";
import { apiBaseUrl } from "../../../../shared/frontend/api-config";
import { useAuth } from "../auth/AuthContext";

const visibleRows = 4;
const universityFetchSize = 100;
const frameworkStatusLabels: Record<FrameworkStatus, string> = { draft: "Bản nháp", published: "Đã công bố", retired: "Ngừng sử dụng" };

function greetingFor(hour: number) {
  if (hour < 11) return "Chào buổi sáng";
  if (hour < 13) return "Chào buổi trưa";
  if (hour < 18) return "Chào buổi chiều";
  return "Chào buổi tối";
}

function useOverviewData<T>(load: () => Promise<T>) {
  const [data, setData] = useState<T | null>(null);
  const [failed, setFailed] = useState(false);
  useEffect(() => {
    let disposed = false;
    load().then(value => { if (!disposed) setData(value); }).catch(() => { if (!disposed) setFailed(true); });
    return () => { disposed = true; };
  }, [load]);
  return { data, failed };
}

function OverviewPanel({ icon: PanelIcon, title, allTo, children }: { icon: Icon; title: string; allTo: string; children: ReactNode }) {
  return <section className="overview-panel" aria-label={title}>
    <header className="overview-panel__head">
      <div className="overview-panel__title"><PanelIcon size={22} aria-hidden="true" /><h2>{title}</h2></div>
      <Link className="overview-panel__more" to={allTo}>Xem tất cả <ArrowRight size={13} weight="bold" aria-hidden="true" /></Link>
    </header>
    {children}
  </section>;
}

function OverviewStats({ items }: { items: [label: string, value: number][] }) {
  return <dl className="overview-stats">{items.map(([label, value]) => <div key={label}><dt>{label}</dt><dd>{value}</dd></div>)}</dl>;
}

function PanelState({ failed, children }: { failed: boolean; children: ReactNode }) {
  return <p className="overview-panel__state" role={failed ? "alert" : "status"}>{children}</p>;
}

const loadUniversities = () => accreditationApi.directoryUniversities(1, universityFetchSize, "");

function UniversitiesPanel() {
  const { data, failed } = useOverviewData<PagedResult<DirectoryUniversity>>(loadUniversities);
  const universities = data?.items ?? [];

  return <OverviewPanel icon={GraduationCap} title="Trường đại học" allTo="/curriculum">
    {failed ? <PanelState failed>Chưa tải được danh sách trường.</PanelState>
      : !data ? <PanelState failed={false}>Đang tải danh sách trường…</PanelState>
        : <>
          <OverviewStats items={[
            ["Trường", data.totalItems],
            ["Ngành đã kiểm định", universities.reduce((total, university) => total + university.accreditedProgramCount, 0)],
            ["Tổng số CTĐT", universities.reduce((total, university) => total + university.programCount, 0)],
          ]} />
          {universities.length === 0 ? <PanelState failed={false}>Chưa có trường đại học trong phạm vi của bạn.</PanelState>
            : <div className="overview-list">{universities.slice(0, visibleRows).map(university => <Link className="overview-row" key={university.id} to={`/curriculum/universities/${university.id}`}>
              <span className="overview-row__main"><strong>{university.name}</strong><small>{university.code}</small></span>
              <span className="status status--current">{university.accreditedProgramCount} đã kiểm định</span>
              <CaretRight size={14} aria-hidden="true" />
            </Link>)}</div>}
        </>}
  </OverviewPanel>;
}

const loadFrameworks = () => accreditationApi.listFrameworks();

function AccreditationPanel() {
  const { data, failed } = useOverviewData<FrameworkSummary[]>(loadFrameworks);
  const frameworks = data ?? [];
  const active = frameworks.find(framework => framework.status === "published") ?? frameworks[0];

  return <OverviewPanel icon={SealCheck} title="Kiểm định AUN-QA" allTo="/accreditation">
    {failed ? <PanelState failed>Chưa tải được bộ tiêu chí.</PanelState>
      : !data ? <PanelState failed={false}>Đang tải bộ tiêu chí…</PanelState>
        : <>
          <OverviewStats items={[
            ["Bộ tiêu chí", frameworks.length],
            ["Tiêu chuẩn", active?.criterionCount ?? 0],
            ["Yêu cầu", active?.requirementCount ?? 0],
          ]} />
          {frameworks.length === 0 ? <PanelState failed={false}>Chưa có bộ tiêu chí AUN-QA.</PanelState>
            : <div className="overview-list">{frameworks.slice(0, visibleRows).map(framework => <Link className="overview-row" key={framework.id} to={`/accreditation/frameworks/${framework.id}`}>
              <span className="overview-row__main"><strong>{framework.code} · {framework.version}</strong><small>{framework.sourceTitle}</small></span>
              <span className={`status status--framework-${framework.status}`}>{frameworkStatusLabels[framework.status]}</span>
              <CaretRight size={14} aria-hidden="true" />
            </Link>)}</div>}
          {active && <div className="overview-panel__foot">
            <Link className="button button--small" to={`/accreditation/frameworks/${active.id}`}>Tra cứu tiêu chuẩn và yêu cầu</Link>
          </div>}
        </>}
  </OverviewPanel>;
}

export function Home({ canAccredit, canChat, isAdmin, roleDescription }: { canAccredit: boolean; canChat: boolean; isAdmin: boolean; roleDescription: string }) {
  const { user } = useAuth();
  const [apiOffline, setApiOffline] = useState(false);

  useEffect(() => {
    const controller = new AbortController();
    fetch(`${apiBaseUrl}/health`, { signal: controller.signal })
      .then((response) => { if (!response.ok) throw new Error("API unavailable"); })
      .catch((error: unknown) => {
        if (error instanceof DOMException && error.name === "AbortError") return;
        setApiOffline(true);
      });
    return () => controller.abort();
  }, []);

  const tools: { to: string; title: string; description: string; icon: Icon }[] = [
    ...(isAdmin ? [
      { to: "/reporting", title: "Báo cáo", description: "Báo cáo học phần, CLO, PLO; xuất DOCX/PDF.", icon: ChartBar },
      { to: "/admin/knowledge", title: "Knowledge Base", description: "Tài liệu nguồn phục vụ tra cứu.", icon: Books },
      { to: "/admin/users", title: "Quản lý tài khoản", description: "Người dùng và phân quyền.", icon: UsersThree },
    ] : []),
    ...(canChat ? [{ to: "/knowledge/chat", title: "Trợ lý tài liệu", description: "Hỏi đáp trên tài liệu đã nạp.", icon: Sparkle }] : []),
  ];

  return <section className="dashboard">
    <header className="workspace-heading">
      <div><h1>{greetingFor(new Date().getHours())}, {user?.fullName}</h1><p>{roleDescription}</p></div>
    </header>
    {apiOffline && <div className="alert alert--error" role="alert">Chưa kết nối được máy chủ. Một số dữ liệu có thể không tải được.</div>}
    <div className={`overview-grid ${canAccredit ? "" : "overview-grid--single"}`}>
      <UniversitiesPanel />
      {canAccredit && <AccreditationPanel />}
    </div>
    {tools.length > 0 && <>
      <div className="section-heading"><div><h2>Công cụ khác</h2></div></div>
      <nav className="tool-grid" aria-label="Công cụ khác">
        {tools.map(({ to, title, description, icon: ToolIcon }) => <Link className="tool-link" key={to} to={to}><ToolIcon size={19} aria-hidden="true" /><span><strong>{title}</strong><small>{description}</small></span></Link>)}
      </nav>
    </>}
  </section>;
}
