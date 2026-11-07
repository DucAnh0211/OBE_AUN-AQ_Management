using ObeAunQa.Modules.Accreditation.Domain.Entities;

namespace ObeAunQa.Modules.Accreditation.Application;

public interface IProgramAccreditationRepository
{
    Task<IReadOnlyList<ProgramAccreditation>> ListAsync(long[] programIds, CancellationToken ct);
    Task<ProgramAccreditation?> FindAsync(long id, CancellationToken ct);
    Task<AunFramework?> FindFrameworkAsync(long id, CancellationToken ct);
    void Add(ProgramAccreditation accreditation);
    void Remove(ProgramAccreditation accreditation);
    Task SaveAsync(CancellationToken ct);
}
