import { useEffect, useState } from "react";
import { Link, Navigate, NavLink, Route, Routes } from "react-router-dom";
import type { FrontendModuleDefinition } from "../../../shared/frontend/module";
import { webModules } from "./module-registry";

type ApiState = "checking" | "online" | "offline";

const apiUrl = import.meta.env.VITE_API_URL ?? "http://localhost:5099";

function Dashboard() {
  const [apiState, setApiState] = useState<ApiState>("checking");

  useEffect(() => {
    const controller = new AbortController();

    fetch(`${apiUrl}/health`, { signal: controller.signal })
      .then((response) => {
        if (!response.ok) {
          throw new Error("API unavailable");
        }
        setApiState("online");
      })
      .catch((error: unknown) => {
        if (error instanceof DOMException && error.name === "AbortError") {
          return;
        }
        setApiState("offline");
      });

    return () => controller.abort();
  }, []);

  return (
    <>
      <section className="hero">
        <div>
          <span className="eyebrow">OBE & AUN-QA MANAGEMENT</span>
          <h1>Dữ liệu đào tạo và minh chứng kiểm định trong một hệ thống</h1>
          <p>
            Theo dõi CTĐT, PLO/CLO, tiêu chí AUN-QA và báo cáo có căn cứ từ một
            nguồn dữ liệu thống nhất.
          </p>
        </div>
        <div className={`api-status api-status--${apiState}`}>
          <span className="status-dot" />
          API {apiState === "checking" ? "đang kiểm tra" : apiState === "online" ? "đã kết nối" : "chưa chạy"}
        </div>
      </section>

      <section className="module-grid" aria-label="Các phân hệ">
        {webModules.map((module, index) => (
          <Link className="module-card" key={module.id} to={module.route}>
            <div className="module-card__topline">
              <span className="module-card__index">0{index + 1}</span>
              <span className="module-card__owner">{module.owner}</span>
            </div>
            <div className={`module-mark module-mark--${module.id}`}>
              {module.shortLabel}
            </div>
            <h2>{module.label}</h2>
            <p>{module.summary}</p>
            <span className="module-card__action">Mở phân hệ <span aria-hidden="true">→</span></span>
          </Link>
        ))}
      </section>
    </>
  );
}

function ModulePage({ module }: { module: FrontendModuleDefinition }) {
  return (
    <section className="module-page">
      <div className="module-page__heading">
        <div className={`module-mark module-mark--${module.id}`}>
          {module.shortLabel}
        </div>
        <div>
          <span className="eyebrow">{module.owner}</span>
          <h1>{module.label}</h1>
          <p>{module.summary}</p>
        </div>
      </div>

      <div className="feature-grid">
        {module.features.map((feature, index) => (
          <article className="feature-card" key={feature}>
            <span>0{index + 1}</span>
            <h2>{feature}</h2>
            <p>Màn hình nghiệp vụ sẽ được triển khai trong sprint tiếp theo.</p>
          </article>
        ))}
      </div>
    </section>
  );
}

export function App() {
  return (
    <div className="app-shell">
      <header className="topbar">
        <Link className="brand" to="/">
          <span className="brand__mark">OA</span>
          <span>
            <strong>OBE · AUN-QA</strong>
            <small>Academic Quality Platform</small>
          </span>
        </Link>
        <nav aria-label="Điều hướng chính">
          <NavLink to="/" end>Trang chủ</NavLink>
          {webModules.map((module) => (
            <NavLink key={module.id} to={module.route}>{module.shortLabel}</NavLink>
          ))}
        </nav>
      </header>

      <main>
        <Routes>
          <Route path="/" element={<Dashboard />} />
          {webModules.map((module) => (
            <Route
              key={module.id}
              path={module.route}
              element={<ModulePage module={module} />}
            />
          ))}
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </main>

      <footer>
        <span>OBE & AUN-QA Management</span>
        <span>Khởi tạo tháng 09/2026</span>
      </footer>
    </div>
  );
}
