namespace ObeAunQa.Modules.Knowledge.Application;
public sealed record DocumentSearchPlan(string Question, IReadOnlyList<string> Queries);
