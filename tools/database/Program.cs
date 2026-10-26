using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using ObeAunQa.Modules.Curriculum.Infrastructure;
using ObeAunQa.Modules.Reporting.Infrastructure;
using ObeAunQa.Modules.Identity.Infrastructure;
using ObeAunQa.Modules.Accreditation.Infrastructure;

if (args.Length != 1 || args[0] is not ("migrate" or "baseline-existing" or "fingerprint" or "seed-demo" or "verify-demo" or "initialize"))
{
    Console.Error.WriteLine("Usage: dotnet run --project tools/database -- migrate|baseline-existing|fingerprint|seed-demo|verify-demo|initialize\nSet ConnectionStrings__Postgres first. initialize baselines verified legacy schemas and migrates; Database__SeedDemo=true opts into demo seeding on an empty curriculum only.");
    return 2;
}
var connection = Environment.GetEnvironmentVariable("ConnectionStrings__Postgres")
    ?? throw new InvalidOperationException("Set ConnectionStrings__Postgres. No default database is selected.");
await using var curriculum = new CurriculumDbContext(new DbContextOptionsBuilder<CurriculumDbContext>().UseNpgsql(connection,
    p => p.MigrationsHistoryTable("__EFMigrationsHistory", "curriculum")).Options);
await using var reporting = new ReportingDbContext(new DbContextOptionsBuilder<ReportingDbContext>().UseNpgsql(connection,
    p => p.MigrationsHistoryTable("__EFMigrationsHistory", "reporting")).Options);
await using var accreditation = new AccreditationDbContext(new DbContextOptionsBuilder<AccreditationDbContext>().UseNpgsql(connection,
    p => p.MigrationsHistoryTable("__EFMigrationsHistory", "accreditation")).Options);
await using var identity = new IdentityDbContext(new DbContextOptionsBuilder<IdentityDbContext>().UseNpgsql(connection,
    p => p.MigrationsHistoryTable("__EFMigrationsHistory", "security")).Options);
// Identity assignments reference Curriculum; always create Curriculum first.
var contexts = new (DbContext Context, string Schema)[] { (curriculum, "curriculum"), (reporting, "reporting"), (accreditation, "accreditation"), (identity, "security") };
var expected = JsonSerializer.Deserialize<Dictionary<string, string>>(Resource("LegacySchemaFingerprints.json"))!;
var fingerprintSql = LegacySchemaFingerprint.Query;
var baselineSchemas = new HashSet<string>();

if (args[0] == "seed-demo")
{
    Console.WriteLine(await new CurriculumDemoSeeder(curriculum).SeedIfEmptyAsync() ? "Demo V1/V2 seeded and verified using EF Core." : "Existing curriculum detected; demo seed skipped, data unchanged.");
    return 0;
}
if (args[0] == "verify-demo")
{
    await new CurriculumDemoVerifier(curriculum).VerifyAsync();
    Console.WriteLine("Demo V1/V2 verification passed.");
    return 0;
}

// Validate every module before recording either baseline. Existing application tables are never rebuilt.
foreach (var (db, schema) in contexts)
{
    var applied = (await db.Database.GetAppliedMigrationsAsync()).ToArray();
    if (args[0] == "fingerprint")
    {
        Console.WriteLine($"{schema}: {await Fingerprint(db)}");
        continue;
    }
    if (applied.Length > 0) continue;
    var hasTables = await db.Database.SqlQueryRaw<int>(
        "SELECT COUNT(*)::integer AS \"Value\" FROM information_schema.tables WHERE table_schema={0} AND table_type='BASE TABLE' AND table_name<>'__EFMigrationsHistory'", schema).SingleAsync() > 0;
    if (args[0] == "migrate" && hasTables)
        throw new InvalidOperationException($"{schema}: existing tables have no EF history. Back up and run baseline-existing first.");
    if (args[0] is "baseline-existing" or "initialize" && hasTables)
    {
        if (!expected.TryGetValue(schema, out var hash) || hash != await Fingerprint(db))
            throw new InvalidOperationException($"{schema}: schema does not match the verified legacy baseline. No baseline written; inspect schema differences first.");
        baselineSchemas.Add(schema);
    }

    async Task<string> Fingerprint(DbContext context)
    {
        var json = await context.Database.SqlQueryRaw<string>(fingerprintSql, schema).SingleAsync();
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(json))).ToLowerInvariant();
    }
}
if (args[0] == "fingerprint") return 0;
foreach (var (db, schema) in contexts)
{
    if (baselineSchemas.Contains(schema))
    {
        await using var transaction = await db.Database.BeginTransactionAsync();
        var history = db.GetService<IHistoryRepository>();
        await db.Database.ExecuteSqlRawAsync(history.GetCreateIfNotExistsScript());
        var baseline = db.Database.GetMigrations().First();
        await db.Database.ExecuteSqlRawAsync(history.GetInsertScript(new HistoryRow(baseline, "10.0.14")));
        await transaction.CommitAsync();
        Console.WriteLine($"{schema}: recorded {baseline}; existing data unchanged.");
    }
    if (args[0] is "migrate" or "initialize")
    {
        await db.Database.MigrateAsync();
        Console.WriteLine($"{schema}: migrations applied.");
    }
}
if (args[0] is "migrate" or "initialize") await new AccreditationSeeder(accreditation).SeedAsync();
if (args[0] == "initialize" && bool.TryParse(Environment.GetEnvironmentVariable("Database__SeedDemo"), out var seedDemo) && seedDemo)
    Console.WriteLine(await new CurriculumDemoSeeder(curriculum).SeedIfEmptyAsync() ? "Demo V1/V2 seeded and verified using EF Core." : "Existing curriculum detected; demo seed skipped, data unchanged.");
return 0;

static string Resource(string name)
{
    var assembly = Assembly.GetExecutingAssembly();
    using var stream = assembly.GetManifestResourceStream(assembly.GetManifestResourceNames().Single(n => n.EndsWith(name, StringComparison.Ordinal)))!;
    using var reader = new StreamReader(stream);
    return reader.ReadToEnd();
}
