namespace ObeAunQa.Modules.Curriculum.Application;

public interface IUniversityRepository
{
    // programIds == null means no scope limit; otherwise only universities owning one of those programs.
    Task<PagedResult<UniversityResponse>> GetUniversitiesAsync(
        int page,
        int pageSize,
        string? search,
        bool includeArchived,
        long[]? programIds,
        CancellationToken cancellationToken);

    Task<UniversityResponse?> GetUniversityAsync(long id, CancellationToken cancellationToken);

    Task<UniversityResponse> CreateUniversityAsync(string code, string name, CancellationToken cancellationToken);

    Task<UniversityResponse?> UpdateUniversityAsync(long id, string name, CancellationToken cancellationToken);

    Task<bool> ArchiveUniversityAsync(long id, CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<long, long[]>> GetProgramIdsAsync(long[] universityIds, CancellationToken cancellationToken);
}
