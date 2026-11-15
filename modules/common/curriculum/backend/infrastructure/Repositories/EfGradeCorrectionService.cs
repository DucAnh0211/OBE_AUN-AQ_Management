using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ObeAunQa.SharedKernel;
using ObeAunQa.Modules.Curriculum.Application;
using ObeAunQa.Modules.Curriculum.Domain;
using ObeAunQa.Modules.Curriculum.Domain.Entities;
namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal sealed class EfGradeCorrectionService(CurriculumDbContext db, CurriculumWriteSession writes,
    EfAcademicRepository grades, AcademicService academic, ICurrentUser user) : IGradeCorrectionService
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    public async Task<IReadOnlyList<GradeCorrectionInfo>> ListAsync(int page,string? status,CancellationToken ct) {
        if (!SystemRoles.IsAdmin(user.Role) && user.Role!=SystemRoles.Lecturer) throw new AcademicAccessException();
        if(status!=null && status is not ("pending" or "approved" or "rejected")) AcademicValidator.Invalid("status","Trạng thái không hợp lệ.");
        var rows=await db.Set<GradeCorrectionRequest>().AsNoTracking()
            .Where(r=>(SystemRoles.IsAdmin(user.Role) || r.RequestedBy==user.UserId) && (status==null || r.Status==status))
            .OrderByDescending(r=>r.RequestedAt).ThenBy(r=>r.Id).Skip((Math.Clamp(page,1,10000)-1)*50).Take(50).ToArrayAsync(ct);
        var result=new List<GradeCorrectionInfo>(); foreach(var row in rows) result.Add(await Info(row,ct)); return result;
    }
    public async Task<GradeCorrectionInfo> RequestAsync(long versionId,long offeringId,long enrollmentId,SaveStudentGrades request,CancellationToken ct) {
        if(user.Role!=SystemRoles.Lecturer) throw new AcademicAccessException();
        var offering=await academic.CourseAccessAsync(versionId,offeringId,true,ct);
        return await writes.RunAsync(async ()=> {
            await writes.LockVersionAsync(versionId,ct);
            var state=await db.Set<CourseOffering>().AsNoTracking().SingleAsync(o=>o.Id==offeringId,ct);
            if(state.GradeStatus!=AcademicCodes.Published) throw new CurriculumConflictException("not_published","Chỉ gửi yêu cầu sửa điểm đã công bố.");
            var book=await grades.GradesAsync(offering,enrollmentId,ct);
            if(request.Revision!=book.Enrollment.Revision || request.SchemeRevision!=book.Scheme.Revision)
                throw new CurriculumConflictException("revision_conflict","Điểm hoặc cơ cấu điểm đã thay đổi. Hãy tải lại trước khi gửi yêu cầu.");
            AcademicValidator.Scores(request.Scores,book.Scheme,allowEmpty:false);
            var proposal=request with { Reason=AcademicValidator.Reason(request.Reason,true), SpecialMark=AcademicValidator.SpecialMark(request.SpecialMark) };
            if(proposal.SpecialMark==book.Enrollment.SpecialMark && proposal.Scores.All(p=>book.Scores.GetValueOrDefault(p.Key)==p.Value))
                throw new CurriculumConflictException("no_changes","Chưa có điểm hoặc điểm đặc biệt nào thay đổi.");
            if(await db.Set<GradeCorrectionRequest>().AnyAsync(r=>r.EnrollmentId==enrollmentId && r.Status=="pending",ct))
                throw new CurriculumConflictException("request_pending","Sinh viên này đang có yêu cầu chờ duyệt.");
            var before=new SaveStudentGrades(book.Enrollment.Revision,book.Scheme.Revision,book.Scores,book.Enrollment.SpecialMark);
            var row=new GradeCorrectionRequest { Id=Guid.NewGuid(),OfferingId=offeringId,EnrollmentId=enrollmentId,RequestedBy=user.UserId,
                RequestedAt=DateTime.UtcNow,BeforeJson=JsonSerializer.Serialize(before,Json),ProposalJson=JsonSerializer.Serialize(proposal,Json) };
            db.Add(row); await db.SaveChangesAsync(ct); return await Info(row,ct);
        },ct);
    }
    public async Task<GradeCorrectionInfo> DecideAsync(Guid id,GradeCorrectionDecision decision,CancellationToken ct) {
        if(!SystemRoles.IsAdmin(user.Role)) throw new AcademicAccessException();
        return await writes.RunAsync(async ()=> {
            var row=(await db.Set<GradeCorrectionRequest>().FromSqlInterpolated($"SELECT * FROM curriculum.grade_correction_requests WHERE \"Id\"={id} FOR UPDATE").ToListAsync(ct)).SingleOrDefault()
                ?? throw new CurriculumConflictException("request_not_found","Không tìm thấy yêu cầu sửa điểm trong trường.");
            if(row.Status!="pending") throw new CurriculumConflictException("request_reviewed","Yêu cầu đã được xử lý.");
            if(row.RequestedBy==user.UserId) throw new AcademicAccessException();
            var reason=AcademicValidator.Reason(decision.Reason,!decision.Approve);
            var offering=await db.Set<CourseOffering>().AsNoTracking().SingleAsync(o=>o.Id==row.OfferingId,ct);
            await writes.LockVersionAsync(offering.ProgramVersionId,ct);
            if(decision.Approve) {
                var info=await academic.CourseAccessAsync(offering.ProgramVersionId,offering.Id,true,ct);
                if(info.GradeStatus!=AcademicCodes.Published) throw new CurriculumConflictException("not_published","Trạng thái công bố đã thay đổi.");
                await grades.SaveApprovedGradesAsync(info,row.EnrollmentId,JsonSerializer.Deserialize<SaveStudentGrades>(row.ProposalJson,Json)!,user.UserId,ct);
            }
            row.Status=decision.Approve?"approved":"rejected"; row.ReviewedBy=user.UserId;row.ReviewedAt=DateTime.UtcNow;row.DecisionReason=reason;
            await db.SaveChangesAsync(ct);return await Info(row,ct);
        },ct);
    }
    private async Task<GradeCorrectionInfo> Info(GradeCorrectionRequest r,CancellationToken ct) {
        var offering=await db.Set<CourseOffering>().AsNoTracking().SingleAsync(o=>o.Id==r.OfferingId,ct);
        var student=await db.Set<CourseEnrollment>().AsNoTracking().Where(e=>e.Id==r.EnrollmentId).Select(e=>new {e.Student.StudentCode,e.Student.FullName}).SingleAsync(ct);
        var before=JsonSerializer.Deserialize<SaveStudentGrades>(r.BeforeJson,Json)!;
        return new(r.Id,offering.ProgramVersionId,r.OfferingId,offering.Code,r.EnrollmentId,student.StudentCode,student.FullName,r.RequestedBy,r.RequestedAt,r.Status,
            before.Scores,before.SpecialMark,JsonSerializer.Deserialize<SaveStudentGrades>(r.ProposalJson,Json)!,r.ReviewedBy,r.ReviewedAt,r.DecisionReason);
    }
}
