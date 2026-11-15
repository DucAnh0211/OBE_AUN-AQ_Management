using Microsoft.EntityFrameworkCore;
using ObeAunQa.Modules.Knowledge.Domain;
namespace ObeAunQa.Modules.Knowledge.Infrastructure;
public sealed class KnowledgeDbContext(DbContextOptions<KnowledgeDbContext> options, ObeAunQa.SharedKernel.ICurrentUser? currentUser = null) : DbContext(options)
{
    public DbSet<IngestionJob> Jobs => Set<IngestionJob>();
    public DbSet<KnowledgeChunk> Chunks => Set<KnowledgeChunk>();
    public long? ScopeSchoolId => currentUser?.UniversityId;
    public bool ScopeBypass => currentUser == null || currentUser.IsSystemContext || currentUser.Role == "admin" && currentUser.UniversityId == null;
    public override async Task<int> SaveChangesAsync(CancellationToken ct = default) {
        await ObeAunQa.SharedKernel.SchoolData.GuardAsync(this, currentUser, ct);
        return await base.SaveChangesAsync(ct);
    }
    protected override void OnModelCreating(ModelBuilder model) {
        model.ApplyConfigurationsFromAssembly(typeof(KnowledgeDbContext).Assembly);
        ObeAunQa.SharedKernel.SchoolData.Configure(model, this);
        model.Entity<ChatThread>().HasQueryFilter("OwnerRole", t => currentUser == null || currentUser.IsSystemContext || t.OwnerRole == currentUser.Role);
    }
}
