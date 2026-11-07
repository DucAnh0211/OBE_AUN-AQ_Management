namespace ObeAunQa.Modules.Curriculum.Application;

public interface IAcademicSheetReader
{
    IReadOnlyList<AcademicImportRow> Read(byte[] bytes, string fileName, string? sheetName, string mode, AssessmentScheme scheme);
    IReadOnlyList<string> Sheets(byte[] bytes, string fileName);
    byte[] Template(string mode, AssessmentScheme scheme);
}
