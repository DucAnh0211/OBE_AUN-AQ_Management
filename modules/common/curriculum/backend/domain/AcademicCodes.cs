namespace ObeAunQa.Modules.Curriculum.Domain;

/// <summary>Stored code values for the academic records model (Thông tư 08/2021/TT-BGDĐT terminology).</summary>
public static class AcademicCodes
{
    public const string Faculty = "faculty";
    public const string Department = "department";
    public static readonly string[] OrgUnitKinds = [Faculty, Department];

    public const string MainLecturer = "main";
    public static readonly string[] LecturerRoles = [MainLecturer, "assistant", "practice"];
    public static readonly string[] AcademicTitles = ["GS", "PGS"];
    public static readonly string[] Degrees = ["CN", "KS", "ThS", "TS", "TSKH"];

    public const string Studying = "studying";
    public static readonly string[] StudentStatuses = [Studying, "suspended", "dropped", "graduated"];

    public const string Draft = "draft";
    public const string Submitted = "submitted";
    public const string Published = "published";

    public const string FirstAttempt = "first";
    public const string Retake = "retake";
    public const string Improve = "improve";

    /// <summary>I = chưa hoàn thiện, R = miễn/bảo lưu, P = đạt (học phần đánh giá đạt/không đạt).</summary>
    public static readonly string[] SpecialMarks = ["I", "R", "P"];

    public const string FinalExam = "final";
    public static readonly string[] ComponentKinds = ["attendance", "process", "midterm", "practice", "project", FinalExam];

    public const string Highest = "highest";
    public const string Latest = "latest";
    public static readonly string[] RetakePolicies = [Highest, Latest];
}
