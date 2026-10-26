using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace ObeAunQa.Modules.Accreditation.Infrastructure;
public sealed class AccreditationDbContextFactory : IDesignTimeDbContextFactory<AccreditationDbContext>
{
    public AccreditationDbContext CreateDbContext(string[] args) => new(new DbContextOptionsBuilder<AccreditationDbContext>()
        .UseNpgsql(Environment.GetEnvironmentVariable("ConnectionStrings__Postgres") ?? throw new InvalidOperationException("Set ConnectionStrings__Postgres"), p => p.MigrationsHistoryTable("__EFMigrationsHistory", "accreditation")).Options);
}
