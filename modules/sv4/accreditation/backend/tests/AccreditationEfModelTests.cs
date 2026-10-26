using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using ObeAunQa.Modules.Accreditation.Infrastructure;

namespace ObeAunQa.Modules.Accreditation.Tests;
public sealed class AccreditationEfModelTests
{
    [Fact]
    public void Framework_state_constraints_and_translation_keys_match_migration()
    {
        using var db=new AccreditationDbContext(new DbContextOptionsBuilder<AccreditationDbContext>().UseNpgsql("Host=localhost;Database=model_only").Options);
        var model=db.GetService<IDesignTimeModel>().Model;
        Assert.All(model.GetEntityTypes(),e=>Assert.Equal("accreditation",e.GetSchema()));
        Assert.Contains(model.GetEntityTypes().Single(e=>e.GetTableName()=="aun_frameworks").GetCheckConstraints(),c=>c.Name=="aun_frameworks_status_ck");
        Assert.Equal(2,model.GetEntityTypes().Single(e=>e.GetTableName()=="aun_requirement_translations").FindPrimaryKey()!.Properties.Count);
        Assert.False(db.Database.HasPendingModelChanges());
    }
}
