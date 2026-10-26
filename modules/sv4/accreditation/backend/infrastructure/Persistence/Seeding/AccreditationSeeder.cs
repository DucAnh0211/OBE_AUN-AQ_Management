using System.Data;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ObeAunQa.Modules.Accreditation.Domain.Entities;
namespace ObeAunQa.Modules.Accreditation.Infrastructure;

public sealed class AccreditationSeeder(AccreditationDbContext db)
{
    public async Task SeedAsync(CancellationToken ct = default)
    {
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct);
        if(await db.AunFrameworks.AnyAsync(f=>f.Code=="AUN-QA-PROGRAMME" && f.Version=="4.0",ct)) return;
        var assembly=typeof(AccreditationSeeder).Assembly;
        await using var stream=assembly.GetManifestResourceStream(assembly.GetManifestResourceNames().Single(n=>n.EndsWith(".Data.AunQaProgramme4.json",StringComparison.Ordinal)))!;
        var framework=(await JsonSerializer.DeserializeAsync<AunFramework>(stream,cancellationToken:ct))!;
        if(framework.AunCriteria.Count!=8 || framework.AunCriteria.Sum(c=>c.AunRequirements.Count)!=53)
            throw new InvalidOperationException("Invalid AUN-QA v4.0 seed.");
        framework.PublishedAt=DateTime.UtcNow;
        db.AunFrameworks.Add(framework);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }
}
