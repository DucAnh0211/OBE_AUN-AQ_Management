import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { apiJson, getSelectedSchool, setSelectedSchool, type AuthUser } from "../../../../shared/frontend/auth-client";

export function SchoolScope({ user }: { user: AuthUser }) {
  const [schools,setSchools]=useState<{id:number;name:string}[]>([]);
  const [error,setError]=useState("");
  const [schoolName,setSchoolName]=useState("");
  useEffect(()=> { if(user.role==="admin") apiJson<{items:{id:number;name:string}[]}>("/api/curriculum/universities?pageSize=100")
    .then(r=>setSchools(r.items)).catch(e=>setError(String(e))); },[user.id,user.role]);
  useEffect(()=>{if(user.role!=="admin") apiJson<{name:string}|null>("/api/auth/school")
    .then(r=>setSchoolName(r?.name??"")).catch(e=>setError(String(e)));},[user.id,user.role]);
  return <div className="list-toolbar">
    {user.role==="admin" ? <label>Trường đang quản lý <select aria-label="Trường đang quản lý" value={getSelectedSchool() ?? ""} onChange={e=>{
      setSelectedSchool(e.target.value ? Number(e.target.value) : null); window.location.assign("/");
    }}><option value="">Toàn hệ thống — chọn trường trước khi tạo tài nguyên</option>{schools.map(s=><option key={s.id} value={s.id}>{s.name}</option>)}</select></label>
      : <span>Trường: {schoolName||`#${user.universityId}`}</span>}
    {user.role!=="student" && <Link className="button button--secondary" to="/grade-correction-requests">Yêu cầu sửa điểm</Link>}
    {error && <span role="alert">{error}</span>}
  </div>;
}
