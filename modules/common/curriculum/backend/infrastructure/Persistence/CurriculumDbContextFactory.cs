using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ObeAunQa.Modules.Curriculum.Infrastructure;

public sealed class CurriculumDbContextFactory : IDesignTimeDbContextFactory<CurriculumDbContext>
{
    public CurriculumDbContext CreateDbContext(string[] args)
    {
        var connection = Environment.GetEnvironmentVariable("ConnectionStrings__Postgres")
            ?? throw new InvalidOperationException("Set ConnectionStrings__Postgres for EF tooling.");
        return new(new DbContextOptionsBuilder<CurriculumDbContext>().UseNpgsql(connection,
            postgres => postgres.MigrationsHistoryTable("__EFMigrationsHistory", "curriculum")).Options);
    }
}
