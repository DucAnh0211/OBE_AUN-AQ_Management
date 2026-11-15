namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record OutcomePolicy(int Revision = 0, decimal CloThreshold = 5, decimal PloThreshold = 5,
    decimal GroupTargetRate = 70, string RetakePolicy = "highest", string FormulaVersion = "obe/1");
