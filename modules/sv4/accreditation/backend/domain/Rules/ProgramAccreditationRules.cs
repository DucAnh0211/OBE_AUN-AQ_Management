namespace ObeAunQa.Modules.Accreditation.Domain.Rules;

public static class ProgramAccreditationRules
{
    public const string SelfAssessed = "self_assessed";
    public const string Accredited = "accredited";
    public const string NotMet = "not_met";
    public const string Expired = "expired";

    public static readonly string[] Statuses = [SelfAssessed, Accredited, NotMet];

    // Expiry is derived from the date, so a stored record never goes stale.
    public static string Effective(string status, DateOnly? validUntil, DateOnly today) =>
        status == Accredited && validUntil is { } until && until < today ? Expired : status;
}
