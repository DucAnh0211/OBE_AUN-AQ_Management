using Microsoft.EntityFrameworkCore;
using ObeAunQa.Modules.Curriculum.Application;
using ObeAunQa.Modules.Curriculum.Domain;
using ObeAunQa.Modules.Curriculum.Domain.Entities;

namespace ObeAunQa.Modules.Curriculum.Infrastructure;

internal sealed class EfProgramVersionRepository(CurriculumDbContext db, CurriculumWriteSession writes) : IProgramVersionRepository
{
    public async Task<IReadOnlyList<ProgramVersionResponse>> GetProgramVersionsAsync(long programId, bool includeArchived, CancellationToken ct) =>
        await db.ProgramVersions.Where(v => v.ProgramId == programId && (includeArchived || v.ArchivedAt == null)).OrderByDescending(v => v.CreatedAt).ThenByDescending(v => v.Id).VersionResponses().ToListAsync(ct);
    public Task<ProgramVersionResponse?> GetProgramVersionAsync(long id, CancellationToken ct) => db.ProgramVersions.Where(v => v.Id == id).VersionResponses().SingleOrDefaultAsync(ct);
    public async Task<ProgramVersionResponse> CreateProgramVersionAsync(long programId, string code, long? sourceId, CancellationToken ct)
    {
        var id = await writes.RunAsync(async () => {
            if (!await db.Programs.AnyAsync(p => p.Id == programId && p.ArchivedAt == null, ct)) throw new CurriculumNotFoundException("chương trình đào tạo", programId);
            if (sourceId != null)
            {
                var source = await db.ProgramVersions.SingleOrDefaultAsync(v => v.Id == sourceId && v.ProgramId == programId && v.ArchivedAt == null, ct)
                    ?? throw new CurriculumNotFoundException("phiên bản chương trình đào tạo nguồn", sourceId.Value);
                if (source.DatasetStatus != CurriculumStatuses.Finalized) throw new CurriculumConflictException("source_version_not_finalized", "Chỉ có thể sao chép học phần từ phiên bản đã finalized.");
            }
            var version = new ProgramVersion { ProgramId = programId, VersionCode = code, DatasetStatus = CurriculumStatuses.Draft, SourceVersionId = sourceId };
            db.ProgramVersions.Add(version); await db.SaveChangesAsync(ct);
            if (sourceId != null)
            {
                var courses = await db.ProgramCourses.AsNoTracking().Where(c => c.ProgramVersionId == sourceId && c.ArchivedAt == null).ToListAsync(ct);
                db.ProgramCourses.AddRange(courses.Select(c => new ProgramCourse { ProgramVersionId = version.Id, CourseId = c.CourseId,
                    SourceRow = c.SourceRow, CourseCodeSourceValue = c.CourseCodeSourceValue, Credits = c.Credits, SemesterSourceValue = c.SemesterSourceValue,
                    DatasetStatus = CurriculumStatuses.Draft, QualityFlags = c.QualityFlags, CourseCodeSnapshot = c.CourseCodeSnapshot,
                    CourseNameSnapshot = c.CourseNameSnapshot, DisplayOrder = c.DisplayOrder }));
            }
            return version.Id;
        }, ct);
        return (await GetProgramVersionAsync(id, ct))!;
    }
    public async Task<ProgramVersionResponse?> UpdateProgramVersionAsync(long id, string code, CancellationToken ct)
    {
        await writes.RunAsync(async () => { var v = await writes.DraftAsync(id, ct); v.VersionCode = code; v.UpdatedAt = DateTime.UtcNow; return true; }, ct);
        return await GetProgramVersionAsync(id, ct);
    }
    public async Task<ProgramVersionResponse> PublishProgramVersionAsync(long id, CancellationToken ct)
    {
        await writes.RunAsync(async () => {
            var version = await writes.DraftAsync(id, ct);
            if (!VersionRules.CanPublish(await db.ProgramCourses.CountAsync(c => c.ProgramVersionId == id && c.ArchivedAt == null, ct)))
                throw new CurriculumConflictException("version_has_no_courses", "Phiên bản phải có ít nhất một học phần trước khi công bố.");
            await db.ProgramVersions.Where(v => v.ProgramId == version.ProgramId && v.IsCurrent).ExecuteUpdateAsync(s => s.SetProperty(v => v.IsCurrent, false).SetProperty(v => v.UpdatedAt, DateTime.UtcNow), ct);
            version.DatasetStatus = CurriculumStatuses.Finalized; version.IsCurrent = true; version.PublishedAt = version.UpdatedAt = DateTime.UtcNow;
            await db.ProgramCourses.Where(c => c.ProgramVersionId == id && c.ArchivedAt == null).ExecuteUpdateAsync(s => s.SetProperty(c => c.DatasetStatus, CurriculumStatuses.Finalized).SetProperty(c => c.UpdatedAt, DateTime.UtcNow), ct);
            return true;
        }, ct);
        return (await GetProgramVersionAsync(id, ct))!;
    }
    public Task<bool> ArchiveProgramVersionAsync(long id, CancellationToken ct) => writes.RunAsync(async () => {
        if (!await db.ProgramVersions.AnyAsync(v => v.Id == id && v.ArchivedAt == null, ct)) return false;
        var version = await writes.LockVersionAsync(id, ct);
        var wasCurrent = version.IsCurrent;
        version.DatasetStatus = CurriculumStatuses.Archived; version.IsCurrent = false; version.ArchivedAt = version.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        if (wasCurrent)
        {
            var replacement = await db.ProgramVersions.Where(v => v.ProgramId == version.ProgramId && v.Id != id && v.DatasetStatus == CurriculumStatuses.Finalized && v.ArchivedAt == null)
                .OrderByDescending(v => v.PublishedAt.HasValue).ThenByDescending(v => v.PublishedAt).ThenByDescending(v => v.Id).FirstOrDefaultAsync(ct);
            if (replacement != null) { replacement.IsCurrent = true; replacement.UpdatedAt = DateTime.UtcNow; }
        }
        return true;
    }, ct);
}
