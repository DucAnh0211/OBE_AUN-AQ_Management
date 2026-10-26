using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace ObeAunQa.Modules.Identity.Infrastructure;
public sealed class IdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args) => new(new DbContextOptionsBuilder<IdentityDbContext>()
        .UseNpgsql(Environment.GetEnvironmentVariable("ConnectionStrings__Postgres") ?? throw new InvalidOperationException("Set ConnectionStrings__Postgres"), p => p.MigrationsHistoryTable("__EFMigrationsHistory", "security")).Options);
}
