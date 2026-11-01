using Microsoft.EntityFrameworkCore;
using ObeAunQa.Modules.Knowledge.Domain;
namespace ObeAunQa.Modules.Knowledge.Infrastructure;
public sealed class KnowledgeDbContext(DbContextOptions<KnowledgeDbContext> options) : DbContext(options)
{
    public DbSet<IngestionJob> Jobs => Set<IngestionJob>();
    public DbSet<KnowledgeChunk> Chunks => Set<KnowledgeChunk>();
    protected override void OnModelCreating(ModelBuilder model) => model.ApplyConfigurationsFromAssembly(typeof(KnowledgeDbContext).Assembly);
}
