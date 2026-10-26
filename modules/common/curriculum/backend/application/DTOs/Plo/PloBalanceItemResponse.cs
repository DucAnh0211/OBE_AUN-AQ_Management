namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record PloBalanceItemResponse(
    string PloCode,
    decimal Score,
    decimal MeanScore,
    decimal? Deviation,
    bool ExceedsTwentyPercent);
