using ObeAunQa.Modules.Curriculum.Application;

namespace ObeAunQa.Modules.Curriculum.Tests;

public sealed class CurriculumValidatorTests
{
    [Fact]
    public void Program_NormalizesCodeAndName()
    {
        var result = CurriculumValidator.Program(
            new CreateProgramRequest(" cs ", "  Khoa học máy tính  "));

        Assert.Equal("CS", result.Code);
        Assert.Equal("Khoa học máy tính", result.Name);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public void ListQuery_RejectsInvalidPagination(int page, int pageSize)
    {
        var exception = Assert.Throws<CurriculumValidationException>(
            () => CurriculumValidator.ListQuery(page, pageSize, null));

        Assert.NotEmpty(exception.Errors);
    }

    [Fact]
    public void Course_AllowsMissingInstitutionalCode()
    {
        var result = CurriculumValidator.Course(
            null,
            "Nhập môn lập trình",
            3,
            "Kỳ 1",
            null,
            requireDisplayOrder: false);

        Assert.Null(result.InstitutionalCode);
        Assert.Equal("Nhập môn lập trình", result.Name);
        Assert.Null(result.DisplayOrder);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(31)]
    public void Course_RejectsInvalidCredits(int credits)
    {
        var exception = Assert.Throws<CurriculumValidationException>(() =>
            CurriculumValidator.Course(
                "CS101",
                "Nhập môn lập trình",
                credits,
                "Kỳ 1",
                1,
                requireDisplayOrder: true));

        Assert.Contains("credits", exception.Errors.Keys);
    }

    [Fact]
    public void PagedResult_CalculatesTotalPages()
    {
        var result = PagedResult<int>.Create([1, 2], 2, 20, 41);

        Assert.Equal(3, result.TotalPages);
    }
}
