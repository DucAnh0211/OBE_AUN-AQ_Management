namespace ObeAunQa.Modules.Curriculum.Application;

public sealed record EnrollmentInfo(long Id, string StudentCode, string FullName, string AcademicPeriod, int Revision,
    int GradedComponents, decimal? TotalScore);
