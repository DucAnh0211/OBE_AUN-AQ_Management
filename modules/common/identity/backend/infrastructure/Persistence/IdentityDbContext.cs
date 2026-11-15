using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ObeAunQa.Modules.Identity.Domain.Entities;

namespace ObeAunQa.Modules.Identity.Infrastructure;

public partial class IdentityDbContext : DbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options, ObeAunQa.SharedKernel.ICurrentUser? currentUser = null)
        : base(options)
    {
        this.currentUser = currentUser;
    }

    public long? ScopeSchoolId => currentUser?.UniversityId;
    public bool ScopeBypass => currentUser == null || currentUser.IsSystemContext || !currentUser.IsAuthenticated || currentUser.Role == "admin" && currentUser.UniversityId == null;

    private readonly ObeAunQa.SharedKernel.ICurrentUser? currentUser;

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default) {
        foreach(var e in ChangeTracker.Entries().Where(e=>e.State==EntityState.Added && e.Metadata.FindProperty("SchoolId")!=null))
            e.Property("SchoolId").CurrentValue ??= currentUser?.UniversityId;
        if(currentUser?.IsAuthenticated==true) await ObeAunQa.SharedKernel.SchoolData.GuardAsync(this,currentUser,ct);
        return await base.SaveChangesAsync(ct);
    }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<LecturerCourseAssignment> LecturerCourseAssignments { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<StudentProgramAssignment> StudentProgramAssignments { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);
        ObeAunQa.SharedKernel.SchoolData.Configure(modelBuilder, this, "RefreshToken");
    }

}
