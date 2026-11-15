import { useCallback, useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { apiJson } from "../../../../shared/frontend/auth-client";
type Request = { id:string;versionId:number;offeringId:number;offeringCode:string;enrollmentId:number;studentCode:string;studentName:string;
  requestedBy:number;requestedAt:string;status:string;before:Record<string,number|null>;previousSpecialMark:string|null;
  proposal:{scores:Record<string,number|null>;specialMark:string|null;reason:string};reviewedBy:number|null;reviewedAt:string|null;decisionReason:string|null };
const labels:Record<string,string>={pending:"Chờ duyệt",approved:"Đã phê duyệt",rejected:"Đã từ chối"};
export function GradeCorrectionsPage({isAdmin}:{isAdmin:boolean}) {
  const [rows,setRows]=useState<Request[]>([]),[status,setStatus]=useState("pending"),[page,setPage]=useState(1);
  const [error,setError]=useState(""),[notice,setNotice]=useState(""),[busy,setBusy]=useState(false),[reasons,setReasons]=useState<Record<string,string>>({});
  const load=useCallback(async()=>{setBusy(true);setError("");try{setRows(await apiJson<Request[]>(`/api/curriculum/grade-correction-requests?page=${page}${status?`&status=${status}`:""}`));}catch(e){setError(String(e));}finally{setBusy(false);}},[page,status]);
  useEffect(()=>{void load();},[load]);
  async function decide(id:string,approve:boolean) {
    setBusy(true);setError("");setNotice("");
    try{await apiJson(`/api/curriculum/grade-correction-requests/${id}/decision`,{method:"POST",body:JSON.stringify({approve,reason:reasons[id]?.trim()||null})});
      setNotice(approve?"Đã phê duyệt và cập nhật điểm cùng lịch sử thay đổi.":"Đã từ chối; điểm giữ nguyên.");await load();
    }catch(e){setError(String(e));}finally{setBusy(false);}
  }
  return <section className="admin-page"><h1>Yêu cầu sửa điểm đã công bố</h1>
    <p>{isAdmin?"Kiểm tra điểm cũ, điểm đề nghị và lý do trước khi quyết định. Phê duyệt sẽ tính lại kết quả của sinh viên.":"Theo dõi các yêu cầu của bạn. Gửi yêu cầu tại màn hình điểm chi tiết của từng sinh viên."}</p>
    <label>Trạng thái <select value={status} onChange={e=>{setStatus(e.target.value);setPage(1);}}><option value="">Tất cả</option>{Object.entries(labels).map(([key,label])=><option key={key} value={key}>{label}</option>)}</select></label>
    {error&&<p role="alert">{error}</p>}{notice&&<p role="status">{notice}</p>}
    {rows.map(r=><article key={r.id} className="form-panel"><h2>{r.studentCode} · {r.studentName}</h2>
      <p>Lớp học phần {r.offeringCode} · {labels[r.status]} · người gửi #{r.requestedBy} · {new Date(r.requestedAt).toLocaleString("vi-VN")}</p>
      <div className="data-table-wrap"><table className="data-table"><thead><tr><th>Đầu điểm</th><th>Hiện tại khi gửi</th><th>Đề nghị</th></tr></thead><tbody>
        {Object.entries(r.proposal.scores).map(([code,value])=><tr key={code}><th>{code}</th><td>{r.before[code]??"Chưa có"}</td><td>{value??"Chưa có"}</td></tr>)}
        <tr><th>Điểm đặc biệt</th><td>{r.previousSpecialMark??"—"}</td><td>{r.proposal.specialMark??"—"}</td></tr></tbody></table></div>
      <p><strong>Lý do:</strong> {r.proposal.reason}</p>
      {isAdmin&&r.status==="pending"&&<div className="admin-form"><label>Ý kiến xét duyệt (bắt buộc khi từ chối)<input aria-label={`Ý kiến ${r.studentCode}`} minLength={5} maxLength={500} value={reasons[r.id]??""} onChange={e=>setReasons(s=>({...s,[r.id]:e.target.value}))}/></label>
        <div className="table-actions"><button className="button button--primary" disabled={busy} onClick={()=>void decide(r.id,true)}>Phê duyệt</button><button className="button button--danger" disabled={busy||(reasons[r.id]?.trim().length??0)<5} onClick={()=>void decide(r.id,false)}>Từ chối</button></div></div>}
      {r.reviewedAt&&<p>Người xử lý #{r.reviewedBy} · {new Date(r.reviewedAt).toLocaleString("vi-VN")} · {r.decisionReason??"Đã phê duyệt"}</p>}
      <Link to={`/curriculum/versions/${r.versionId}/offerings/${r.offeringId}/students/${r.enrollmentId}/grades`}>Xem điểm và lịch sử</Link>
    </article>)}
    {!rows.length&&!busy&&<p>Không có yêu cầu trong phạm vi này.</p>}
    <button className="button button--secondary" disabled={busy||page===1} onClick={()=>setPage(p=>p-1)}>Trước</button> Trang {page} <button className="button button--secondary" disabled={busy||rows.length<50} onClick={()=>setPage(p=>p+1)}>Sau</button>
  </section>;
}
