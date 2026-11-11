using Microsoft.EntityFrameworkCore;
using ObeAunQa.Modules.Curriculum.Domain;
using ObeAunQa.Modules.Curriculum.Domain.Entities;
using ObeAunQa.Modules.Curriculum.Infrastructure;

namespace ObeAunQa.Modules.Curriculum.Tests;

public sealed class EfModelTests
{
    private static CurriculumDbContext Context() => new(new DbContextOptionsBuilder<CurriculumDbContext>()
        .UseNpgsql("Host=localhost;Database=model_only;Username=unused").Options);

    [Fact]
    public void ProgramHasManyVersionsDespitePartialUniqueCurrentIndex()
    {
        using var db = Context();
        var entity = db.Model.FindEntityType(typeof(ProgramVersion))!;
        var relation = entity.GetForeignKeys().Single(f => f.Properties.Count == 1 && f.Properties[0].Name == nameof(ProgramVersion.ProgramId));
        Assert.False(relation.IsUnique);
        Assert.Equal(false, entity.FindProperty(nameof(ProgramVersion.IsCurrent))!.GetDefaultValue());
        Assert.Contains(entity.GetIndexes(), index => index.IsUnique && index.GetFilter()?.Contains("is_current") == true);
    }

    [Fact]
    public void MappingForeignKeysKeepVersionAndCourseScope()
    {
        using var db = Context();
        var coursePlo = db.Model.FindEntityType(typeof(CoursePlo))!;
        Assert.Contains(coursePlo.GetForeignKeys(), fk => fk.Properties.Select(p => p.Name).SequenceEqual(new[] { "PloId", "ProgramVersionId" }));
        Assert.Contains(coursePlo.GetForeignKeys(), fk => fk.Properties.Select(p => p.Name).SequenceEqual(new[] { "ProgramCourseId", "ProgramVersionId" }));
        var cloPlo = db.Model.FindEntityType(typeof(CloPlo))!;
        Assert.Equal(new[] { "CloId", "CoursePloId" }, cloPlo.FindPrimaryKey()!.Properties.Select(p => p.Name));
        Assert.All(cloPlo.GetForeignKeys(), fk => Assert.Contains(fk.Properties, p => p.Name == "ProgramCourseId"));
        Assert.DoesNotContain(db.Model.GetEntityTypes().SelectMany(e => e.GetProperties()), p => p.IsShadowProperty());
    }

    [Fact]
    public void ProgramAndCourseCodesAreUniqueInsideOneUniversityOnly()
    {
        using var db = Context();
        var program = db.Model.FindEntityType(typeof(Program))!;
        Assert.Contains(program.GetIndexes(), index => index.IsUnique && index.Properties.Select(p => p.Name).SequenceEqual(new[] { "UniversityId", "Code" }));
        Assert.DoesNotContain(program.GetIndexes(), index => index.IsUnique && index.Properties.Count == 1);
        var course = db.Model.FindEntityType(typeof(Course))!;
        Assert.Contains(course.GetIndexes(), index => index.IsUnique && index.Properties.Select(p => p.Name).SequenceEqual(new[] { "UniversityId", "InstitutionalCode" }));
        Assert.All(new[] { program, course }, entity => Assert.Contains(entity.GetForeignKeys(), fk => fk.PrincipalEntityType.ClrType == typeof(University) && fk.DeleteBehavior == DeleteBehavior.Restrict));
    }

    [Fact]
    public void GradebooksBelongToOfferingAndOfferingReusesCatalogMappings()
    {
        using var db = Context();
        var enrollment = db.Model.FindEntityType(typeof(CourseEnrollment))!;
        Assert.Contains(enrollment.GetIndexes(), i => i.IsUnique && i.Properties.Select(p => p.Name).SequenceEqual(new[] { "OfferingId", "StudentId" }));
        Assert.Contains(enrollment.GetForeignKeys(), fk => fk.PrincipalEntityType.ClrType == typeof(CourseOffering) && fk.Properties.Select(p => p.Name).SequenceEqual(new[] { "OfferingId", "ProgramCourseId", "AcademicPeriod" }));
        var offering = db.Model.FindEntityType(typeof(CourseOffering))!;
        Assert.Contains(offering.GetForeignKeys(), fk => fk.PrincipalEntityType.ClrType == typeof(ProgramCourse) && fk.Properties.Select(p => p.Name).SequenceEqual(new[] { "ProgramCourseId", "ProgramVersionId" }));
        // Lớp học phần = học phần × học kỳ × nhóm lớp, không phụ thuộc lớp sinh hoạt.
        Assert.Contains(offering.GetIndexes(), i => i.IsUnique && i.Properties.Select(p => p.Name).SequenceEqual(new[] { "ProgramCourseId", "AcademicPeriod", "GroupName" }));
        Assert.Contains(offering.GetForeignKeys(), fk => fk.PrincipalEntityType.ClrType == typeof(HomeClass) && !fk.IsRequired);
        Assert.Contains(offering.GetIndexes(), i => i.IsUnique && i.Properties.Select(p => p.Name).SequenceEqual(new[] { "Code" }));
        Assert.Contains("nextval", offering.FindProperty("Code")!.GetDefaultValueSql());
        var lecturers = db.Model.FindEntityType(typeof(OfferingLecturer))!;
        Assert.Contains(lecturers.GetIndexes(), i => i.IsUnique && i.GetFilter() == "\"Role\" = 'main'");
    }

    [Fact]
    public void AssessmentSchemeBelongsToCourseSyllabusAndMapsClos()
    {
        using var db = Context();
        var component = db.Model.FindEntityType(typeof(AssessmentComponent))!;
        Assert.Contains(component.GetIndexes(), i => i.IsUnique && i.Properties.Select(p => p.Name).SequenceEqual(new[] { "ProgramCourseId", "Code" }));
        Assert.Null(component.FindProperty("ProgramVersionId"));
        var map = db.Model.FindEntityType(typeof(AssessmentComponentClo))!;
        Assert.Equal(new[] { "ComponentId", "CloId" }, map.FindPrimaryKey()!.Properties.Select(p => p.Name));
        Assert.Contains(map.GetForeignKeys(), fk => fk.PrincipalEntityType.ClrType == typeof(Clo));
        Assert.Null(db.Model.FindEntityType(typeof(ProgramVersion))!.FindProperty("GradeSchemeRevision"));
    }

    [Fact]
    public void ModelMatchesCommittedMigrations()
    {
        using var db = Context();
        Assert.False(db.Database.HasPendingModelChanges());
    }

    [Fact]
    public void BusinessRulesHandleBoundaryValuesWithoutDatabase()
    {
        Assert.Equal(3, CurriculumAnalysisRules.RequiredPlos(3));
        Assert.Equal(5, CurriculumAnalysisRules.RequiredPlos(14));
        Assert.Null(CurriculumAnalysisRules.Deviation(0, 0));
        Assert.False(CurriculumAnalysisRules.ExceedsLimit(0.20m));
        Assert.True(CurriculumAnalysisRules.ExceedsLimit(-0.20001m));
        Assert.False(VersionRules.CanPublish(0));
        Assert.True(VersionRules.CanPublish(1));
        Assert.False(VersionRules.CanEdit("draft", DateTime.UtcNow));
    }
}
