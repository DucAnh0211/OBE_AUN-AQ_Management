using ObeAunQa.Modules.Accreditation.Domain.Entities;
using ObeAunQa.Modules.Accreditation.Domain.Rules;
using ObeAunQa.Modules.Curriculum.Application;

namespace ObeAunQa.Modules.Accreditation.Application;

public sealed class ProgramAccreditationService(
    IProgramAccreditationRepository repository,
    UniversityService universities,
    CurriculumService curriculum)
{
    private const int MaxNoteLength = 1000;

    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    // scope == null means every program; otherwise only those program ids are visible.
    public async Task<PagedResult<DirectoryUniversity>> GetUniversitiesAsync(int page, int pageSize, string? search, long[]? scope, CancellationToken ct)
    {
        var result = await universities.GetUniversitiesAsync(page, pageSize, search, false, scope, ct);
        var owned = await universities.GetProgramIdsAsync(result.Items.Select(u => u.Id).ToArray(), ct);
        var visible = owned.ToDictionary(pair => pair.Key, pair => scope is null ? pair.Value : pair.Value.Intersect(scope).ToArray());
        var standing = await StandingAsync(visible.Values.SelectMany(ids => ids).ToArray(), ct);
        var items = result.Items.Select(u => new DirectoryUniversity(u.Id, u.Code, u.Name, u.ProgramCount,
            visible[u.Id].Count(id => standing.TryGetValue(id, out var record) && record.EffectiveStatus == ProgramAccreditationRules.Accredited))).ToArray();
        return PagedResult<DirectoryUniversity>.Create(items, result.Page, result.PageSize, result.TotalItems);
    }

    public async Task<PagedResult<DirectoryProgram>> GetProgramsAsync(long universityId, string? status, int page, int pageSize, string? search, long[]? scope, CancellationToken ct)
    {
        var filter = status?.Trim().ToLowerInvariant() ?? ProgramAccreditationRules.Accredited;
        if (filter is not (ProgramAccreditationRules.Accredited or "all"))
            throw new AccreditationValidationException("Bộ lọc không hợp lệ.", new Dictionary<string, string[]> { ["status"] = ["Chỉ nhận accredited hoặc all."] });
        _ = await universities.GetUniversityAsync(universityId, ct);

        var programs = new List<ProgramResponse>();
        for (var fetchPage = 1; ; fetchPage++)
        {
            var batch = await curriculum.GetProgramsAsync(fetchPage, 100, search, false, ct, universityId);
            programs.AddRange(batch.Items.Where(p => scope is null || scope.Contains(p.Id)));
            if (fetchPage >= batch.TotalPages) break;
        }

        var standing = await StandingAsync(programs.Select(p => p.Id).ToArray(), ct);
        var rows = programs
            .Select(p => new DirectoryProgram(p, standing.GetValueOrDefault(p.Id)))
            .Where(row => filter == "all" || row.Accreditation?.EffectiveStatus == ProgramAccreditationRules.Accredited)
            .OrderBy(row => row.Program.Name, StringComparer.CurrentCulture)
            .ThenBy(row => row.Program.Id)
            .ToArray();
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        return PagedResult<DirectoryProgram>.Create(rows.Skip((page - 1) * pageSize).Take(pageSize).ToArray(), page, pageSize, rows.Length);
    }

    public async Task<IReadOnlyList<ProgramAccreditationResponse>> GetRecordsAsync(long programId, CancellationToken ct)
    {
        _ = await curriculum.GetProgramAsync(programId, ct);
        return (await repository.ListAsync([programId], ct)).OrderByDescending(a => a.AssessedOn).ThenByDescending(a => a.Id).Select(Response).ToArray();
    }

    public async Task<ProgramAccreditationResponse> CreateAsync(long programId, SaveProgramAccreditationRequest request, long actor, CancellationToken ct)
    {
        var program = await curriculum.GetProgramAsync(programId, ct);
        if (program.IsArchived) throw new AccreditationConflictException("program_archived", "CTĐT đã lưu trữ, không thể ghi nhận kiểm định.");
        var accreditation = new ProgramAccreditation { ProgramId = programId, CreatedBy = actor };
        await ApplyAsync(accreditation, request, ct);
        repository.Add(accreditation);
        await repository.SaveAsync(ct);
        return Response((await repository.FindAsync(accreditation.Id, ct))!);
    }

    public async Task<ProgramAccreditationResponse> UpdateAsync(long id, SaveProgramAccreditationRequest request, CancellationToken ct)
    {
        var accreditation = await repository.FindAsync(id, ct) ?? throw new AccreditationNotFoundException("Không tìm thấy kết quả kiểm định.");
        await ApplyAsync(accreditation, request, ct);
        accreditation.UpdatedAt = DateTime.UtcNow;
        await repository.SaveAsync(ct);
        return Response((await repository.FindAsync(id, ct))!);
    }

    public async Task DeleteAsync(long id, CancellationToken ct)
    {
        var accreditation = await repository.FindAsync(id, ct) ?? throw new AccreditationNotFoundException("Không tìm thấy kết quả kiểm định.");
        repository.Remove(accreditation);
        await repository.SaveAsync(ct);
    }

    // An accreditation still in force outranks a newer self-assessment; otherwise the latest round stands.
    private async Task<Dictionary<long, ProgramAccreditationResponse>> StandingAsync(long[] programIds, CancellationToken ct)
    {
        if (programIds.Length == 0) return [];
        return (await repository.ListAsync(programIds, ct)).Select(Response).GroupBy(a => a.ProgramId).ToDictionary(group => group.Key, group =>
            group.OrderByDescending(a => a.EffectiveStatus == ProgramAccreditationRules.Accredited)
                .ThenByDescending(a => a.AssessedOn).ThenByDescending(a => a.Id).First());
    }

    private async Task ApplyAsync(ProgramAccreditation target, SaveProgramAccreditationRequest request, CancellationToken ct)
    {
        var errors = new Dictionary<string, string[]>();
        var status = request.Status?.Trim().ToLowerInvariant();
        if (status is null || !ProgramAccreditationRules.Statuses.Contains(status)) errors["status"] = ["Chọn kết quả kiểm định hợp lệ."];
        if (request.AssessedOn is null) errors["assessedOn"] = ["Nhập ngày đánh giá."];
        if (request.ValidUntil is { } until && request.AssessedOn is { } assessed && until < assessed) errors["validUntil"] = ["Ngày hết hiệu lực phải sau ngày đánh giá."];
        if (request.VerdictRating is < 1 or > 7) errors["verdictRating"] = ["Mức kết luận nằm trong thang 1–7."];
        var note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();
        if (note?.Length > MaxNoteLength) errors["note"] = [$"Ghi chú không vượt quá {MaxNoteLength} ký tự."];

        AunFramework? framework = null;
        if (request.FrameworkId is null or < 1) errors["frameworkId"] = ["Chọn bộ tiêu chí."];
        else
        {
            framework = await repository.FindFrameworkAsync(request.FrameworkId.Value, ct);
            if (framework is null || framework.Status == "draft") errors["frameworkId"] = ["Bộ tiêu chí phải tồn tại và đã công bố."];
        }

        if (errors.Count > 0) throw new AccreditationValidationException("Dữ liệu kiểm định không hợp lệ.", errors);
        target.FrameworkId = framework!.Id;
        target.Status = status!;
        target.AssessedOn = request.AssessedOn!.Value;
        target.ValidUntil = request.ValidUntil;
        target.VerdictRating = request.VerdictRating;
        target.Note = note;
    }

    private static ProgramAccreditationResponse Response(ProgramAccreditation a) => new(a.Id, a.ProgramId, a.FrameworkId, a.Framework.Code, a.Framework.Version,
        a.Status, ProgramAccreditationRules.Effective(a.Status, a.ValidUntil, Today), a.AssessedOn, a.ValidUntil, a.VerdictRating, a.Note, a.CreatedAt, a.UpdatedAt);
}
