namespace ObeAunQa.Modules.Curriculum.Application;

public interface IProgramRepository
{
    Task<PagedResult<ProgramResponse>> GetProgramsAsync(
        int page,
        int pageSize,
        string? search,
        bool includeArchived,
        CancellationToken cancellationToken,
        long? universityId = null);

    Task<ProgramResponse?> GetProgramAsync(long id, CancellationToken cancellationToken);

    Task<ProgramResponse> CreateProgramAsync(
        long universityId,
        string code,
        string name,
        CancellationToken cancellationToken);

    Task<ProgramResponse?> UpdateProgramAsync(
        long id,
        string name,
        CancellationToken cancellationToken);

    Task<bool> ArchiveProgramAsync(long id, CancellationToken cancellationToken);
}
