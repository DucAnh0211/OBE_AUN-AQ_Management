using ObeAunQa.Modules.Accreditation;
using ObeAunQa.Modules.Curriculum;
using ObeAunQa.Modules.Reporting;

var builder = WebApplication.CreateBuilder(args);

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? ["http://localhost:5173"];

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services
    .AddCurriculumModule()
    .AddAccreditationModule()
    .AddReportingModule();

var app = builder.Build();

app.UseCors();

app.MapGet("/", () => Results.Ok(new
{
    service = "OBE & AUN-QA API",
    version = "0.1.0",
    environment = app.Environment.EnvironmentName
}));

app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    checkedAtUtc = DateTimeOffset.UtcNow
}));

app.MapGet("/api/modules", () => Results.Ok(new[]
{
    CurriculumModule.Descriptor,
    AccreditationModule.Descriptor,
    ReportingModule.Descriptor
}));

app.MapCurriculumModule();
app.MapAccreditationModule();
app.MapReportingModule();

app.Run();

public partial class Program;
