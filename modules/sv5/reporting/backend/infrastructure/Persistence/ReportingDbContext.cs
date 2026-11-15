using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ObeAunQa.Modules.Reporting.Domain.Entities;

namespace ObeAunQa.Modules.Reporting.Infrastructure;

public partial class ReportingDbContext : DbContext
{
    private readonly ObeAunQa.SharedKernel.ICurrentUser? currentUser;
    public ReportingDbContext(DbContextOptions<ReportingDbContext> options, ObeAunQa.SharedKernel.ICurrentUser? currentUser = null)
        : base(options)
    {
        this.currentUser = currentUser;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default) {
        await ObeAunQa.SharedKernel.SchoolData.GuardAsync(this, currentUser, ct);
        return await base.SaveChangesAsync(ct);
    }

    public virtual DbSet<AuditEvent> AuditEvents { get; set; }

    public virtual DbSet<Report> Reports { get; set; }

    public long? ScopeSchoolId => currentUser?.UniversityId;
    public bool ScopeBypass => currentUser == null || currentUser.IsSystemContext || currentUser.Role == "admin" && currentUser.UniversityId == null;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReportingDbContext).Assembly,
            type => type.Namespace == "ObeAunQa.Modules.Reporting.Infrastructure");
        ObeAunQa.SharedKernel.SchoolData.Configure(modelBuilder, this, "ProgressionCode", "WeightCode", "PloBalance");
    }

}
