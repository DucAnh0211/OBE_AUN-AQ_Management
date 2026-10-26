using ObeAunQa.Modules.Accreditation.Application;

namespace ObeAunQa.Modules.Accreditation.Tests;

public sealed class FrameworkValidatorTests
{
    [Fact]
    public void Validate_AcceptsCompleteVersionFourStructure()
    {
        var result = FrameworkValidator.Validate(CreateVersionFour());
        Assert.True(result.Valid);
        Assert.Equal(8, result.CriterionCount);
        Assert.Equal(53, result.RequirementCount);
    }

    [Fact]
    public void Validate_RejectsSarStructureMissingRequirementFiveEight()
    {
        var request = CreateVersionFour();
        var criteria = request.Criteria!.Select(c => c.Code == "5"
            ? c with { Requirements = c.Requirements!.Where(r => r.Code != "5.8").ToList() }
            : c).ToList();
        var result = FrameworkValidator.Validate(request with { Criteria = criteria });
        Assert.False(result.Valid);
        Assert.Equal(52, result.RequirementCount);
        Assert.Contains(result.Errors, x => x.Value.Any(message => message.Contains("8 requirements")));
    }

    [Fact]
    public void Validate_RejectsRequirementUnderWrongCriterion()
    {
        var request = CreateVersionFour();
        var first = request.Criteria![0];
        var requirements = first.Requirements!.ToList();
        requirements[0] = requirements[0] with { Code = "2.1" };
        var result = FrameworkValidator.Validate(request with
        { Criteria = [first with { Requirements = requirements }, .. request.Criteria.Skip(1)] });
        Assert.False(result.Valid);
        Assert.Contains(result.Errors.Keys, key => key.EndsWith("requirements[0].code"));
    }

    private static FrameworkImportRequest CreateVersionFour()
    {
        int[] counts = [5,7,6,7,8,6,9,5];
        var criteria = counts.Select((count,index) =>
        {
            var code=(index+1).ToString();
            return new ImportCriterion(code,index+1,null,
                Enumerable.Range(1,count).Select(i => new ImportRequirement($"{code}.{i}",i,null,null)).ToList());
        }).ToList();
        return new FrameworkImportRequest("AUN-QA-PROGRAMME","4.0","programme","vi",
            "Guide to AUN-QA Assessment at Programme Level Version 4.0",null,criteria);
    }
}
