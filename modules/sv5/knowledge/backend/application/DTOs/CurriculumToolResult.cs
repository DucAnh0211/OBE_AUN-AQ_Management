using System.Text.Json;
namespace ObeAunQa.Modules.Knowledge.Application;

public sealed record CurriculumToolResult(JsonElement Data, ChatDatabaseSource Source);
