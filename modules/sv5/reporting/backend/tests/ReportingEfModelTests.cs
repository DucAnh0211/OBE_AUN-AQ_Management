using Microsoft.EntityFrameworkCore;
using ObeAunQa.Modules.Reporting.Domain.Entities;
using ObeAunQa.Modules.Reporting.Infrastructure;

namespace ObeAunQa.Modules.Reporting.Tests;

public sealed class ReportingEfModelTests
{
    [Fact]
    public void SnapshotStorageMatchesMigrationAndKeepsJsonb()
    {
        using var db = new ReportingDbContext(new DbContextOptionsBuilder<ReportingDbContext>()
            .UseNpgsql("Host=localhost;Database=model_only;Username=unused").Options);
        Assert.False(db.Database.HasPendingModelChanges());
        var report = db.Model.FindEntityType(typeof(Report))!;
        Assert.Equal("jsonb", report.FindProperty(nameof(Report.Document))!.GetColumnType());
        Assert.Equal("reporting", report.GetSchema());
        Assert.Empty(report.GetForeignKeys()); // snapshots survive changes to source Curriculum records
        Assert.Single(db.Model.FindEntityType(typeof(AuditEvent))!.GetForeignKeys());
    }
}
