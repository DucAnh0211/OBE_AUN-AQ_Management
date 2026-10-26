using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ObeAunQa.Modules.Reporting.Domain.Entities;

namespace ObeAunQa.Modules.Reporting.Infrastructure;

public partial class ReportingDbContext : DbContext
{
    public ReportingDbContext(DbContextOptions<ReportingDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AuditEvent> AuditEvents { get; set; }

    public virtual DbSet<Report> Reports { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReportingDbContext).Assembly,
            type => type.Namespace == "ObeAunQa.Modules.Reporting.Infrastructure");
    }

}
