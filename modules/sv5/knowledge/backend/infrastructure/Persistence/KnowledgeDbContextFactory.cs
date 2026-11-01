using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace ObeAunQa.Modules.Knowledge.Infrastructure;
public sealed class KnowledgeDbContextFactory : IDesignTimeDbContextFactory<KnowledgeDbContext>
{
    public KnowledgeDbContext CreateDbContext(string[] args) => new(new DbContextOptionsBuilder<KnowledgeDbContext>()
        .UseNpgsql(Environment.GetEnvironmentVariable("ConnectionStrings__Postgres") ?? throw new InvalidOperationException("Set ConnectionStrings__Postgres"),
            p => p.MigrationsHistoryTable("__EFMigrationsHistory", "knowledge")).Options);
}
