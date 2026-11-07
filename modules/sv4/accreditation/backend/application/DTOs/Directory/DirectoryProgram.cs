using ObeAunQa.Modules.Curriculum.Application;

namespace ObeAunQa.Modules.Accreditation.Application;

public sealed record DirectoryProgram(ProgramResponse Program, ProgramAccreditationResponse? Accreditation);
