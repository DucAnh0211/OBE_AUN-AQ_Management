using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ObeAunQa.Modules.Accreditation.Domain.Entities;

namespace ObeAunQa.Modules.Accreditation.Infrastructure;

public partial class AccreditationDbContext : DbContext
{
    private readonly ObeAunQa.SharedKernel.ICurrentUser? currentUser;
    public AccreditationDbContext(DbContextOptions<AccreditationDbContext> options, ObeAunQa.SharedKernel.ICurrentUser? currentUser = null)
        : base(options)
    {
        this.currentUser = currentUser;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default) {
        await ObeAunQa.SharedKernel.SchoolData.GuardAsync(this, currentUser, ct);
        return await base.SaveChangesAsync(ct);
    }

    public virtual DbSet<AunCriterion> AunCriteria { get; set; }

    public virtual DbSet<AunCriterionTranslation> AunCriterionTranslations { get; set; }

    public virtual DbSet<AunFramework> AunFrameworks { get; set; }

    public virtual DbSet<AunRequirement> AunRequirements { get; set; }

    public virtual DbSet<AunRequirementTranslation> AunRequirementTranslations { get; set; }

    public virtual DbSet<ProgramAccreditation> ProgramAccreditations { get; set; }

    public long? ScopeSchoolId => currentUser?.UniversityId;
    public bool ScopeBypass => currentUser == null || currentUser.IsSystemContext || currentUser.Role == "admin" && currentUser.UniversityId == null;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AccreditationDbContext).Assembly);
        ObeAunQa.SharedKernel.SchoolData.Configure(modelBuilder, this, "ProgressionCode", "WeightCode", "PloBalance");
        modelBuilder.Entity<SourceDocument>().HasQueryFilter("Audience", d => currentUser == null || currentUser.IsSystemContext
            || currentUser.Role != "student" || d.AccessScope == "school" || d.AccessScope == "personal" && d.OwnerUserId == currentUser.UserId);
    }

}
