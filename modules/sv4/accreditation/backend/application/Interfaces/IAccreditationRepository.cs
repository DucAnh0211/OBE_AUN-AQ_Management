using ObeAunQa.Modules.Accreditation.Domain.Entities;
namespace ObeAunQa.Modules.Accreditation.Application;

public interface IAccreditationRepository
{
    Task<IReadOnlyList<AunFramework>> ListAsync(string? status, string? search, CancellationToken ct);
    Task<AunFramework?> FindAsync(long id, bool tracking, CancellationToken ct);
    void Add(AunFramework framework);
    void Remove(AunFramework framework);
    Task<T> WriteAsync<T>(Func<Task<T>> action, CancellationToken ct);
}
