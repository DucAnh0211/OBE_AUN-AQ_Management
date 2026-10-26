namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record PloBalanceResponse(
    long ProgramVersionId,
    decimal AllowedDeviation,
    bool IsBalanced,
    IReadOnlyList<PloBalanceItemResponse> Items);
