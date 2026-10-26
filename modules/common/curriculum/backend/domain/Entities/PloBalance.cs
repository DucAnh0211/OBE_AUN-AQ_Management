namespace ObeAunQa.Modules.Curriculum.Domain.Entities;

public class PloBalance
{
    public long? ProgramVersionId { get; set; }

    public string? PloCode { get; set; }

    public decimal? Score { get; set; }

    public decimal? MeanScore { get; set; }

    public decimal? Deviation { get; set; }

    public bool? Exceeds20Percent { get; set; }
}
