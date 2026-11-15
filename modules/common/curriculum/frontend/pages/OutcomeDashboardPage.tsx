import "./outcomes.css";
import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { apiJson } from "../../../../../shared/frontend/auth-client";
import { VersionNav } from "../components/VersionNav";
import { downloadFile } from "../api/academics";
import { useAuth } from "../../../../../apps/web/src/auth/AuthContext";

type Policy = { revision: number; cloThreshold: number; ploThreshold: number; groupTargetRate: number; retakePolicy: string; formulaVersion: string };
type Aggregate = { outcomeId: number; code: string; eligibleCount: number; evaluatedCount: number; achievedCount: number; missingCount: number; excludedCount: number; achievementRate: number | null; dataCoverageRate: number | null; meanScore: number | null; status: string };
type Student = { studentId: number; studentCode: string; studentName: string; code: string; offeringId: number | null; score: number | null; status: string; reason: string | null; contributions: {assessmentId: number; score: number | null; maxScore: number; effectiveWeight: number}[]; enrollmentIds: number[]; expectedCourses: number; evaluatedCourses: number };
type Scope = {versionId: number; offeringId: number | null; programCourseId: number | null; period: string | null; cohort: string | null; preview: boolean};
type Result = { status: string; sourceHash: string; issues: {code: string; entityId: number; message: string}[]; clos: Aggregate[]; plos: Aggregate[]; scope: Scope; policy: Policy; courses: {id: number; code: string; name: string}[] };
type Snapshot = {id: string; stale: boolean; result: Result};
const labels: Record<string,string> = {official:"Chính thức",preview:"Xem trước",partial:"Chưa đầy đủ",achieved:"Đạt",not_achieved:"Chưa đạt",insufficient_data:"Thiếu dữ liệu",excluded:"Không tính",no_data:"Chưa có dữ liệu"};
const fmt = (n: number | null, suffix = "") => n == null ? "—" : `${n.toLocaleString("vi-VN",{maximumFractionDigits:2})}${suffix}`;

function Summary({title,rows}: {title: string; rows: Aggregate[]}) {
  return <section><h2>{title}</h2>{rows.length === 0 ? <p>Chưa có chuẩn đầu ra trong phạm vi này.</p> : <div className="matrix-wrap"><table className="data-table"><thead><tr><th>Chuẩn</th><th>Đạt / được tính</th><th>Tỷ lệ đạt</th><th>Bao phủ dữ liệu</th><th>Điểm TB</th><th>Thiếu / loại</th><th>Kết luận nhóm</th></tr></thead><tbody>{rows.map(r=><tr key={r.outcomeId}><th>{r.code}</th><td>{r.achievedCount}/{r.evaluatedCount}</td><td><meter min={0} max={100} value={r.achievementRate ?? 0} aria-label={`${r.code} tỷ lệ đạt`} /> {fmt(r.achievementRate,"%")}</td><td>{fmt(r.dataCoverageRate,"%")} ({r.evaluatedCount}/{r.eligibleCount})</td><td>{fmt(r.meanScore)}</td><td>{r.missingCount} / {r.excludedCount}</td><td>{labels[r.status]}</td></tr>)}</tbody></table></div>}</section>;
}

export function OutcomeDashboardPage() {
  const versionId=Number(useParams().versionId); const {user}=useAuth(); const root=`/api/curriculum/program-versions/${versionId}`;
  const [period,setPeriod]=useState(""); const [cohort,setCohort]=useState(""); const [offering,setOffering]=useState(""); const [course,setCourse]=useState("");
  const [preview,setPreview]=useState(false); const [data,setData]=useState<Result|null>(null); const [policy,setPolicy]=useState<Policy|null>(null);
  const [error,setError]=useState(""); const [busy,setBusy]=useState(false); const [snapshot,setSnapshot]=useState<Snapshot|null>(null);
  const [other,setOther]=useState(""); const [comparison,setComparison]=useState<{first:Result & {input:{scope:Scope}};second:Result & {input:{scope:Scope}};differences:{code:string;percentagePoints:number|null}[]}|null>(null);
  const [options,setOptions]=useState<{courses:{id:number;label:string}[];offerings:{id:number;label:string}[];periods:string[];cohorts:string[]}>({courses:[],offerings:[],periods:[],cohorts:[]});
  const [detail,setDetail]=useState(""); const [studentPage,setStudentPage]=useState(1);
  const [studentRows,setStudentRows]=useState<{items:Student[];totalPages:number}>({items:[],totalPages:0});
  const scope:Scope={versionId,period:period||null,cohort:cohort||null,offeringId:offering?Number(offering):null,programCourseId:course?Number(course):null,preview};
  const query=()=>new URLSearchParams(Object.entries(scope).filter(([k,v])=>k!=="versionId"&&v!==null).map(([k,v])=>[k,String(v)]));
  async function run(action:()=>Promise<void>) {setBusy(true);setError("");try{await action();}catch(e){setError(e instanceof Error?e.message:"Không thực hiện được.");}finally{setBusy(false);}}
  useEffect(()=>{let cancelled=false;apiJson<Policy>(`${root}/outcome-policy`).then(p=>{if(!cancelled)setPolicy(p);}).catch(e=>{if(!cancelled)setError(String(e));});return()=>{cancelled=true;};},[root]);
  useEffect(()=>{let active=true;apiJson<typeof options>(`${root}/outcome-options`).then(o=>{if(active)setOptions({...o,cohorts:o.cohorts??[]});}).catch(e=>{if(active)setError(String(e));});return()=>{active=false;};},[root]);
  useEffect(()=>{if(!data)return;let active=true;const params=new URLSearchParams(Object.entries(data.scope).filter(([k,v])=>k!=="versionId"&&v!==null).map(([k,v])=>[k,String(v)]));params.set("page",String(studentPage));params.set("search",detail);apiJson<typeof studentRows>(`${root}/outcome-students?${params}`).then(rows=>{if(active)setStudentRows(rows);}).catch(e=>{if(active)setError(String(e));});return()=>{active=false;};},[data,studentPage,detail,root]);
  const students=studentRows.items;
  return <section className="admin-page outcome-page"><header className="workspace-heading"><div><h1>Kết quả chuẩn đầu ra</h1><p>Đánh giá CLO/PLO trong phạm vi đã chọn. Kết quả không thay thế xét chuẩn tốt nghiệp.</p></div></header>
    <VersionNav versionId={versionId} staff={user?.role!=="student"}/>
    <form className="form-grid" onSubmit={e=>{e.preventDefault();void run(async()=>{setData(await apiJson<Result>(`${root}/outcome-summary?${query()}`));setSnapshot(null);setComparison(null);setStudentPage(1);});}}>
      <label>Học kỳ<select aria-label="Học kỳ" value={period} onChange={e=>{setPeriod(e.target.value);if(other===e.target.value)setOther("");}}><option value="">Tất cả học kỳ</option>{options.periods.map(p=><option key={p} value={p}>{p}</option>)}</select></label>
      <label>Khóa<select aria-label="Khóa" value={cohort} onChange={e=>setCohort(e.target.value)}><option value="">Tất cả khóa</option>{options.cohorts.map(c=><option key={c} value={c}>{c}</option>)}</select></label>
      <label>Lớp học phần<select aria-label="Lớp học phần" value={offering} onChange={e=>setOffering(e.target.value)}><option value="">Toàn phạm vi trong quyền</option>{options.offerings.map(o=><option key={o.id} value={o.id}>{o.label}</option>)}</select></label>
      <label>Học phần<select aria-label="Học phần" value={course} onChange={e=>setCourse(e.target.value)}><option value="">Toàn phiên bản trong quyền</option>{options.courses.map(c=><option key={c.id} value={c.id}>{c.label}</option>)}</select></label>
      {user?.role!=="student"&&<label><input type="checkbox" checked={preview} onChange={e=>setPreview(e.target.checked)}/> Xem trước điểm chưa công bố</label>}
      <button className="button button--primary" disabled={busy}>Tải kết quả</button>
    </form>
    {busy&&<p role="status">Đang xử lý…</p>}{error&&<p role="alert" className="error-message">{error}</p>}
    {policy&&<details><summary>Quy tắc tính và ngưỡng đánh giá · revision {policy.revision}</summary><p>Điểm assessment chuẩn hóa về 10; CLO dùng trọng số assessment × tỷ trọng CLO. PLO dùng trung bình CLO rồi tổng hợp theo tín chỉ × X=2/Y=1. Đây là ước lượng từ điểm tổng assessment.</p><form onSubmit={e=>{e.preventDefault();void run(async()=>{setPolicy(await apiJson<Policy>(`${root}/outcome-policy`,{method:"PUT",body:JSON.stringify(policy)}));setData(null);});}}>
      <label>Ngưỡng điểm CLO<input type="number" min={0} max={10} step="0.01" disabled={user?.role!=="admin"&&user?.role!=="school_admin"} value={policy.cloThreshold} onChange={e=>setPolicy({...policy,cloThreshold:Number(e.target.value)})}/></label>
      <label>Ngưỡng điểm PLO<input type="number" min={0} max={10} step="0.01" disabled={user?.role!=="admin"&&user?.role!=="school_admin"} value={policy.ploThreshold} onChange={e=>setPolicy({...policy,ploThreshold:Number(e.target.value)})}/></label>
      <label>Mục tiêu tỷ lệ đạt nhóm (%)<input type="number" min={0} max={100} disabled={user?.role!=="admin"&&user?.role!=="school_admin"} value={policy.groupTargetRate} onChange={e=>setPolicy({...policy,groupTargetRate:Number(e.target.value)})}/></label>
      <label>Lần học dùng tổng hợp<select value={policy.retakePolicy} disabled={user?.role!=="admin"&&user?.role!=="school_admin"} onChange={e=>setPolicy({...policy,retakePolicy:e.target.value})}><option value="highest">Điểm học phần cao nhất</option><option value="latest">Lần gần nhất</option></select></label>
      {(user?.role==="admin"||user?.role==="school_admin")&&<button className="button button--secondary" disabled={busy}>Lưu revision mới</button>}</form></details>}
    {data&&<><p role="status">{labels[data.status]} · {data.courses.length} học phần · Công thức {data.policy.formulaVersion}</p>
      {data.issues.length>0&&<details open><summary>Cấu hình cần bổ sung ({data.issues.length})</summary><ul>{data.issues.map((i,n)=><li key={n}>{i.message} (ID {i.entityId})</li>)}</ul></details>}
      <Summary title="CLO" rows={data.clos}/><Summary title="PLO" rows={data.plos}/>
      <h2>Chi tiết và nguồn tính</h2><label>Lọc MSSV hoặc mã chuẩn<input value={detail} onChange={e=>{setDetail(e.target.value);setStudentPage(1);}}/></label>
      <div className="matrix-wrap"><table className="data-table"><thead><tr><th>Sinh viên</th><th>Chuẩn / lớp</th><th>Điểm</th><th>Trạng thái</th><th>Nguồn</th></tr></thead><tbody>{students.map((s,n)=><tr key={n}><td>{s.studentCode} · {s.studentName}</td><td>{s.code} / {s.offeringId??"Tổng hợp"}</td><td>{fmt(s.score)}</td><td>{labels[s.status]}<small>{s.reason}</small></td><td><details><summary>Xem cách tính</summary>{s.contributions.map(c=><p key={c.assessmentId}>Assessment #{c.assessmentId}: {fmt(c.score)}/{c.maxScore}, trọng số hiệu lực {fmt(c.effectiveWeight)}</p>)}<p>Ghi danh: {s.enrollmentIds.join(", ")}; học phần đánh giá {s.evaluatedCourses}/{s.expectedCourses}</p></details></td></tr>)}</tbody></table></div>
      <button className="button button--secondary" disabled={studentPage<=1} onClick={()=>setStudentPage(p=>p-1)}>Trước</button> Trang {studentPage} <button className="button button--secondary" disabled={studentPage>=studentRows.totalPages} onClick={()=>setStudentPage(p=>p+1)}>Sau</button>
      {user?.role!=="student"&&<section><h2>Lưu kết quả bàn giao</h2><button className="button button--secondary" disabled={busy} onClick={()=>void run(async()=>{setSnapshot(await apiJson<Snapshot>(`${root}/outcome-snapshots`,{method:"POST",body:JSON.stringify(data.scope)}));})}>Tạo snapshot theo phạm vi đang hiển thị</button>
        <button className="button button--secondary" disabled={busy} onClick={()=>void run(async()=>{const params=new URLSearchParams(Object.entries(data.scope).filter(([k,v])=>k!=="versionId"&&v!==null).map(([k,v])=>[k,String(v)]));setSnapshot(await apiJson<Snapshot>(`${root}/outcome-snapshots/latest?${params}`));})}>Mở snapshot gần nhất của phạm vi</button>
        {snapshot&&<p>Snapshot {snapshot.id} · {snapshot.stale?"Nguồn đã thay đổi":"Nguồn còn hiệu lực"} <button className="button button--secondary" onClick={()=>void run(async()=>{setSnapshot(await apiJson<Snapshot>(`${root}/outcome-snapshots/${snapshot.id}`));})}>Kiểm tra hiệu lực</button><button className="button button--secondary" onClick={()=>void run(()=>downloadFile(`${root}/outcome-snapshots/${snapshot.id}/export`,`obe-${snapshot.id}.json`))}>Tải JSON bàn giao</button></p>}
        <h2>So sánh học kỳ</h2><label>Học kỳ đối chiếu<select aria-label="Học kỳ đối chiếu" value={other} onChange={e=>setOther(e.target.value)}><option value="">Chọn học kỳ đối chiếu</option>{options.periods.filter(p=>p!==period).map(p=><option key={p} value={p}>{p}</option>)}</select></label><button className="button button--secondary" disabled={busy||!period||!other||preview} onClick={()=>void run(async()=>{setComparison(await apiJson(`${root}/outcome-comparisons?${query()}&otherPeriod=${encodeURIComponent(other)}`));})}>So sánh với kỳ đã chọn</button>
        {comparison&&<><p>{comparison.first.input.scope.period} → {comparison.second.input.scope.period} · cùng phiên bản và policy hiện hành</p><Summary title="PLO kỳ đối chiếu" rows={comparison.second.plos}/>{comparison.differences.map(d=><p key={d.code}>{d.code}: {fmt(d.percentagePoints)} điểm phần trăm</p>)}</>}
      </section>}</>}
  </section>;
}
