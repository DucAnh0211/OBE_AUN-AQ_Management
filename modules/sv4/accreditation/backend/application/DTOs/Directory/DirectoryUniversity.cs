namespace ObeAunQa.Modules.Accreditation.Application;

public sealed record DirectoryUniversity(
    long Id,
    string Code,
    string Name,
    long ProgramCount,
    long AccreditedProgramCount);
