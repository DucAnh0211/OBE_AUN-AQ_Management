using ObeAunQa.Modules.Accreditation.Domain.Rules;

namespace ObeAunQa.Modules.Accreditation.Tests;

public sealed class ProgramAccreditationRulesTests
{
    private static readonly DateOnly Today = new(2026, 11, 5);

    [Fact]
    public void Accreditation_stays_in_force_through_its_last_valid_day()
    {
        Assert.Equal(ProgramAccreditationRules.Accredited, ProgramAccreditationRules.Effective(ProgramAccreditationRules.Accredited, Today, Today));
        Assert.Equal(ProgramAccreditationRules.Accredited, ProgramAccreditationRules.Effective(ProgramAccreditationRules.Accredited, null, Today));
    }

    [Fact]
    public void Accreditation_expires_after_its_valid_date()
    {
        Assert.Equal(ProgramAccreditationRules.Expired, ProgramAccreditationRules.Effective(ProgramAccreditationRules.Accredited, Today.AddDays(-1), Today));
    }

    [Theory]
    [InlineData(ProgramAccreditationRules.SelfAssessed)]
    [InlineData(ProgramAccreditationRules.NotMet)]
    public void Only_accredited_records_can_expire(string status)
    {
        Assert.Equal(status, ProgramAccreditationRules.Effective(status, Today.AddYears(-1), Today));
    }
}
