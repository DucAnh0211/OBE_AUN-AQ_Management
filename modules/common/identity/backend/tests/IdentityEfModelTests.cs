using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using ObeAunQa.Modules.Identity.Infrastructure;

namespace ObeAunQa.Modules.Identity.Tests;
public sealed class IdentityEfModelTests
{
    [Fact]
    public void Identity_migrations_own_only_security_and_preserve_assignment_foreign_keys()
    {
        using var db=new IdentityDbContext(new DbContextOptionsBuilder<IdentityDbContext>().UseNpgsql("Host=localhost;Database=model_only").Options);
        var model=db.GetService<IDesignTimeModel>().Model;
        Assert.All(model.GetEntityTypes().Where(e=>e.GetSchema()!="security"),e=>Assert.True(e.IsTableExcludedFromMigrations()));
        var assignments=model.GetEntityTypes().Single(e=>e.GetTableName()=="lecturer_course_assignments");
        Assert.Contains(assignments.GetForeignKeys(),f=>f.PrincipalEntityType.GetTableName()=="program_courses" && f.DeleteBehavior==DeleteBehavior.Cascade);
        Assert.False(db.Database.HasPendingModelChanges());
    }
}
