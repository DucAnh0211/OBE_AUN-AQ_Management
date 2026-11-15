import { apiJson } from "../../../../../shared/frontend/auth-client";
import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { academicApi } from "../api/academics";
import type { Gradebook } from "../types/academics";

export function GradeGridPage() {
  const p=useParams(); const v=Number(p.versionId),o=Number(p.offeringId);
  const [books,setBooks]=useState<Gradebook[]>([]); const [draft,setDraft]=useState<Record<number,Record<string,number|null>>>({});
  const [status,setStatus]=useState("draft");
  const [page,setPage]=useState(1);const [more,setMore]=useState(false);const [busy,setBusy]=useState(false); const [error,setError]=useState("");const [notice,setNotice]=useState("");
  async function load() {setBusy(true);setError("");try {setStatus((await apiJson<{gradeStatus:string}>(`/api/curriculum/program-versions/${v}/offerings/${o}`)).gradeStatus);const rows=await academicApi.students(v,o,page,"");setMore(page<rows.totalPages);setBooks(await Promise.all(rows.items.map(e=>academicApi.grades(v,o,e.id))));setDraft({});}catch(e){setError(String(e));}finally{setBusy(false);}}
  useEffect(()=>{void load();},[v,o,page]);
  async function save() {
    setBusy(true);setError("");setNotice("");let saved=0;
    try {for(const b of books) {const scores=draft[b.enrollment.id];if(!scores)continue;
      const updated=await academicApi.saveGrades(v,o,b.enrollment.id,b,scores,b.enrollment.specialMark,null);
      setBooks(rows=>rows.map(r=>r.enrollment.id===updated.enrollment.id?updated:r));setDraft(d=>{const next={...d};delete next[b.enrollment.id];return next;});saved++;
    }setNotice(`Đã lưu ${saved} sinh viên.`);}catch(e){setError(`Đã lưu ${saved} dòng. Dòng tiếp theo chưa lưu: ${String(e)}. Tải lại trước khi sửa xung đột.`);}finally{setBusy(false);}
  }
  const components=books[0]?.scheme.components??[];
  return <section className="admin-page"><h1>Bảng nhập điểm lớp học phần</h1><Link to={`/curriculum/versions/${v}/offerings/${o}/students`}>Danh sách sinh viên và trạng thái sổ điểm</Link><p>Nhập điểm theo hàng và cột; ô trống là chưa có điểm. Mỗi dòng lưu riêng và có kiểm tra phiên bản để tránh ghi đè.</p>
    {error&&<p role="alert">{error}</p>}{notice&&<p role="status">{notice}</p>}{busy&&<p role="status">Đang xử lý…</p>}
    <div className="data-table-wrap"><table className="data-table"><thead><tr><th>MSSV</th><th>Họ tên</th>{components.map(c=><th key={c.code}>{c.code} / {c.maxScore}</th>)}<th>Điểm đặc biệt / chi tiết</th></tr></thead><tbody>{books.map(b=><tr key={b.enrollment.id}><th>{b.enrollment.studentCode}</th><td>{b.enrollment.fullName}</td>{components.map(c=><td key={c.code}><input type="number" min={0} max={c.maxScore} step="0.01" aria-label={`${b.enrollment.studentCode} ${c.code}`} value={(draft[b.enrollment.id]??b.scores)[c.code]??""} disabled={busy||status!=="draft"} onChange={e=>setDraft(d=>({...d,[b.enrollment.id]:{...(d[b.enrollment.id]??b.scores),[c.code]:e.target.value===""?null:Number(e.target.value)}}))}/></td>)}<td>{b.enrollment.specialMark}<Link to={`/curriculum/versions/${v}/offerings/${o}/students/${b.enrollment.id}/grades`}>Chi tiết / lịch sử</Link></td></tr>)}</tbody></table></div>
    <button className="button button--secondary" disabled={busy||!Object.keys(draft).length} onClick={()=>void save()}>Lưu các dòng đã sửa</button><button className="button button--secondary" disabled={busy} onClick={()=>void load()}>Tải lại</button><button className="button button--secondary" disabled={busy||page===1||Object.keys(draft).length>0} onClick={()=>setPage(p=>p-1)}>Trước</button> Trang {page}<button className="button button--secondary" disabled={busy||!more||Object.keys(draft).length>0} onClick={()=>setPage(p=>p+1)}>Sau</button>
  </section>;
}
