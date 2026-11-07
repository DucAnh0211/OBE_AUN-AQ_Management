namespace ObeAunQa.Modules.Curriculum.Application;

public sealed class UniversityService(IUniversityRepository repository)
{
    public Task<PagedResult<UniversityResponse>> GetUniversitiesAsync(
        int page,
        int pageSize,
        string? search,
        bool includeArchived,
        long[]? programIds,
        CancellationToken cancellationToken)
    {
        var query = CurriculumValidator.ListQuery(page, pageSize, search);
        return repository.GetUniversitiesAsync(query.Page, query.PageSize, query.Search, includeArchived, programIds, cancellationToken);
    }

    public async Task<UniversityResponse> GetUniversityAsync(long id, CancellationToken cancellationToken) =>
        await repository.GetUniversityAsync(id, cancellationToken)
            ?? throw new CurriculumNotFoundException("trường đại học", id);

    public Task<UniversityResponse> CreateUniversityAsync(CreateUniversityRequest request, CancellationToken cancellationToken)
    {
        var university = CurriculumValidator.University(request);
        return repository.CreateUniversityAsync(university.Code, university.Name, cancellationToken);
    }

    public async Task<UniversityResponse> UpdateUniversityAsync(long id, UpdateUniversityRequest request, CancellationToken cancellationToken) =>
        await repository.UpdateUniversityAsync(id, CurriculumValidator.UniversityName(request), cancellationToken)
            ?? throw new CurriculumNotFoundException("trường đại học", id);

    public async Task ArchiveUniversityAsync(long id, CancellationToken cancellationToken)
    {
        if (!await repository.ArchiveUniversityAsync(id, cancellationToken))
            throw new CurriculumNotFoundException("trường đại học", id);
    }

    public Task<IReadOnlyDictionary<long, long[]>> GetProgramIdsAsync(long[] universityIds, CancellationToken cancellationToken) =>
        repository.GetProgramIdsAsync(universityIds, cancellationToken);
}
