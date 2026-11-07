using Microsoft.EntityFrameworkCore;
using Npgsql;
using ObeAunQa.Modules.Accreditation.Application;
using ObeAunQa.Modules.Accreditation.Domain.Entities;

namespace ObeAunQa.Modules.Accreditation.Infrastructure;

internal sealed class EfProgramAccreditationRepository(AccreditationDbContext db) : IProgramAccreditationRepository
{
    public async Task<IReadOnlyList<ProgramAccreditation>> ListAsync(long[] programIds, CancellationToken ct) =>
        await db.ProgramAccreditations.AsNoTracking().Include(a => a.Framework).Where(a => programIds.Contains(a.ProgramId)).ToListAsync(ct);

    public Task<ProgramAccreditation?> FindAsync(long id, CancellationToken ct) =>
        db.ProgramAccreditations.Include(a => a.Framework).SingleOrDefaultAsync(a => a.Id == id, ct);

    public Task<AunFramework?> FindFrameworkAsync(long id, CancellationToken ct) =>
        db.AunFrameworks.AsNoTracking().SingleOrDefaultAsync(f => f.Id == id, ct);

    public void Add(ProgramAccreditation accreditation) => db.ProgramAccreditations.Add(accreditation);

    public void Remove(ProgramAccreditation accreditation) => db.ProgramAccreditations.Remove(accreditation);

    public async Task SaveAsync(CancellationToken ct)
    {
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new AccreditationConflictException("accreditation_round_exists", "Đợt kiểm định này đã được ghi nhận cho CTĐT.");
        }
    }
}
