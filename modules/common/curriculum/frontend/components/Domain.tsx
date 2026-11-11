/** Nhóm chức năng: mỗi nhóm có một màu nhận diện (token --domain-* trong apps/web/src/styles.css). */
export type DomainKey = "curriculum" | "teaching" | "assessment" | "accreditation";
export const domains: Record<DomainKey, { label: string }> = {
  curriculum: { label: "CTĐT & chuẩn đầu ra" },
  teaching: { label: "Tổ chức đào tạo" },
  assessment: { label: "Đánh giá & điểm" },
  accreditation: { label: "Kiểm định" },
};
/** className cho <section>: dải tiêu đề, tab và thẻ tóm tắt lấy màu của nhóm. */
export const domainPage = (key: DomainKey, extra = "") => `admin-page domain-page domain--${key}${extra ? ` ${extra}` : ""}`;

const paths: Record<DomainKey, string> = {
  curriculum: "M4 5.5A2.5 2.5 0 0 1 6.5 3H20v15H6.5A2.5 2.5 0 0 0 4 20.5zM4 20.5A2.5 2.5 0 0 0 6.5 23H20v-5",
  teaching: "M9 11a4 4 0 1 0 0-8 4 4 0 0 0 0 8zm-7 10v-1a6 6 0 0 1 12 0v1M17 11a3 3 0 1 0 0-6m3 16v-1a5 5 0 0 0-3-4.6",
  assessment: "M9 3h6v3H9zM6 5H4v17h16V5h-2M8 12l3 3 5-6",
  accreditation: "M12 2l3 3h4v4l3 3-3 3v4h-4l-3 3-3-3H5v-4l-3-3 3-3V5h4zM8.5 12l2.5 2.5 5-5",
};
export function DomainIcon({ domain, size = 16 }: { domain: DomainKey; size?: number }) {
  return <svg className="domain-icon" width={size} height={size} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true"><path d={paths[domain]} /></svg>;
}
/** Nhãn nhóm trên dải tiêu đề trang. */
export function DomainEyebrow({ domain, detail }: { domain: DomainKey; detail?: string }) {
  return <span className="eyebrow domain-eyebrow"><DomainIcon domain={domain} size={14} />{domains[domain].label}{detail ? <span className="domain-eyebrow__detail"> · {detail}</span> : null}</span>;
}
