namespace ObeAunQa.Modules.Curriculum.Application;

public interface IProgramVersionRepository
{
    Task<IReadOnlyList<ProgramVersionResponse>> GetProgramVersionsAsync(
        long programId,
        bool includeArchived,
        CancellationToken cancellationToken);

    Task<ProgramVersionResponse?> GetProgramVersionAsync(
        long id,
        CancellationToken cancellationToken);

    Task<ProgramVersionResponse> CreateProgramVersionAsync(
        long programId,
        string versionCode,
        long? sourceVersionId,
        CancellationToken cancellationToken);

    Task<ProgramVersionResponse?> UpdateProgramVersionAsync(
        long id,
        string versionCode,
        CancellationToken cancellationToken);

    Task<ProgramVersionResponse> PublishProgramVersionAsync(
        long id,
        CancellationToken cancellationToken);

    Task<bool> ArchiveProgramVersionAsync(long id, CancellationToken cancellationToken);
}
