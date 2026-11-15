using Microsoft.EntityFrameworkCore;
using ObeAunQa.Modules.Curriculum.Application;
using ObeAunQa.Modules.Curriculum.Domain.Entities;
using ObeAunQa.SharedKernel;
namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal sealed class EfOutcomeCloneService(CurriculumDbContext db, CurriculumWriteSession writes, ICurrentUser user) : IOutcomeCloneService
{
    public async Task CopyAsync(long targetVersionId, long sourceVersionId, CancellationToken ct)
    {
        if (!SystemRoles.IsAdmin(user.Role)) throw new AcademicAccessException();
        if (targetVersionId == sourceVersionId) AcademicValidator.Invalid("sourceVersionId", "Chọn phiên bản nguồn khác bản đích.");
        await writes.RunAsync(async () => {
            foreach (var id in new[] {targetVersionId, sourceVersionId}.Order()) await writes.LockVersionAsync(id,ct);
            var target = await writes.DraftAsync(targetVersionId, ct);
            var source = await db.ProgramVersions.SingleAsync(v => v.Id == sourceVersionId,ct);
            if (source.ProgramId != target.ProgramId || source.DatasetStatus != "finalized")
                throw new CurriculumConflictException("invalid_source", "Nguồn phải là phiên bản đã công bố cùng CTĐT.");
            if (await db.Plos.AnyAsync(p => p.ProgramVersionId == targetVersionId,ct)
                || await db.Clos.AnyAsync(c => c.ProgramCourse.ProgramVersionId == targetVersionId,ct)
                || await db.CoursePlos.AnyAsync(c => c.ProgramVersionId == targetVersionId,ct))
                throw new CurriculumConflictException("target_not_empty", "Đích đã có outcome/mapping; không ghi đè.");
            var sourceCourses = await db.ProgramCourses.AsNoTracking().Where(c => c.ProgramVersionId == sourceVersionId && c.ArchivedAt == null).ToArrayAsync(ct);
            var targets = await db.ProgramCourses.Where(c => c.ProgramVersionId == targetVersionId && c.ArchivedAt == null).ToArrayAsync(ct);
            if (sourceCourses.Any(c => targets.All(t => t.CourseId != c.CourseId)))
                throw new CurriculumConflictException("courses_mismatch", "Đích thiếu học phần của nguồn. Hãy tạo bản sao học phần trước.");
            var courseMap = sourceCourses.ToDictionary(c=>c.Id,c=>targets.Single(t=>t.CourseId==c.CourseId));
            var plos = await db.Plos.AsNoTracking().Where(p=>p.ProgramVersionId==sourceVersionId).OrderBy(p=>p.Id).ToArrayAsync(ct);
            var ploMap=plos.ToDictionary(p=>p.Id,p=>new Plo {ProgramVersionId=targetVersionId,Code=p.Code,Statement=p.Statement,LevelCode=p.LevelCode,Provenance=p.Provenance,SourceCell=p.SourceCell});
            db.AddRange(ploMap.Values);await db.SaveChangesAsync(ct);
            var sourceIds=sourceCourses.Select(c=>c.Id).ToArray();
            var clos=await db.Clos.AsNoTracking().Where(c=>sourceIds.Contains(c.ProgramCourseId)&&c.DatasetStatus!="archived").ToArrayAsync(ct);
            var cloMap=clos.ToDictionary(c=>c.Id,c=>new Clo {ProgramCourseId=courseMap[c.ProgramCourseId].Id,Code=c.Code,Statement=c.Statement,LevelCode=c.LevelCode,
                Provenance=c.Provenance,DatasetStatus="draft",SourceReference=c.SourceReference,AssumptionNote=c.AssumptionNote});
            db.AddRange(cloMap.Values);await db.SaveChangesAsync(ct);
            var links=await db.CoursePlos.AsNoTracking().Where(l=>l.ProgramVersionId==sourceVersionId&&sourceIds.Contains(l.ProgramCourseId)).ToArrayAsync(ct);
            var linkMap=links.ToDictionary(l=>l.Id,l=>new CoursePlo {ProgramVersionId=targetVersionId,ProgramCourseId=courseMap[l.ProgramCourseId].Id,PloId=ploMap[l.PloId].Id,
                WeightCode=l.WeightCode,ProgressionCode=l.ProgressionCode,Provenance=l.Provenance,Fit=l.Fit,FitReason=l.FitReason});
            db.AddRange(linkMap.Values);await db.SaveChangesAsync(ct);
            var linkIds=links.Select(l=>l.Id).ToArray();
            var cloLinks=await db.CloPlos.AsNoTracking().Where(l=>linkIds.Contains(l.CoursePloId)).ToArrayAsync(ct);
            db.AddRange(cloLinks.Where(l=>cloMap.ContainsKey(l.CloId)).Select(l=>new CloPlo {CloId=cloMap[l.CloId].Id,CoursePloId=linkMap[l.CoursePloId].Id,ProgramCourseId=cloMap[l.CloId].ProgramCourseId}));
            var assessments=await db.Set<AssessmentComponent>().AsNoTracking().Where(a=>sourceIds.Contains(a.ProgramCourseId)).ToArrayAsync(ct);
            var componentMap=new Dictionary<long,AssessmentComponent>();
            foreach(var a in assessments) {
                var course=courseMap[a.ProgramCourseId];
                var component=await db.Set<AssessmentComponent>().SingleOrDefaultAsync(x=>x.ProgramCourseId==course.Id&&x.Code==a.Code,ct);
                if(component==null) {component=new(){ProgramCourseId=course.Id,Code=a.Code,Name=a.Name,Kind=a.Kind,Weight=a.Weight,MaxScore=a.MaxScore,DisplayOrder=a.DisplayOrder};db.Add(component);}
                else if(component.Weight!=a.Weight||component.MaxScore!=a.MaxScore||component.Kind!=a.Kind)
                    throw new CurriculumConflictException("assessment_mismatch","Cơ cấu điểm đích khác nguồn.");
                componentMap[a.Id]=component;course.GradeSchemeRevision++;
            }
            await db.SaveChangesAsync(ct);
            var componentIds=assessments.Select(a=>a.Id).ToArray();
            var maps=await db.Set<AssessmentComponentClo>().AsNoTracking().Where(m=>componentIds.Contains(m.ComponentId)).ToArrayAsync(ct);
            db.AddRange(maps.Where(m=>cloMap.ContainsKey(m.CloId)).Select(m=>new AssessmentComponentClo {ComponentId=componentMap[m.ComponentId].Id,CloId=cloMap[m.CloId].Id,Weight=m.Weight}));
            return true;
        },ct);
    }
}
