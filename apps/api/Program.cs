using ObeAunQa.Modules.Accreditation;
using ObeAunQa.Modules.Curriculum;
using ObeAunQa.Modules.Identity;
using ObeAunQa.Modules.Reporting;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? ["http://localhost:5173"];

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services
    .AddIdentityModule(builder.Configuration)
    .AddCurriculumModule(builder.Configuration)
    .AddAccreditationModule()
    .AddReportingModule();

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
    app.UseSwaggerUI(options =>
    {
        options.RoutePrefix = "swagger";
        options.SwaggerEndpoint("/openapi/v1.json", "OBE & AUN-QA API v1");
        options.DocumentTitle = "OBE & AUN-QA API";
    });
}

app.MapGet("/", () => Results.Ok(new
{
    service = "OBE & AUN-QA API",
    version = "0.1.0",
    environment = app.Environment.EnvironmentName
})).AllowAnonymous();

app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    checkedAtUtc = DateTimeOffset.UtcNow
})).AllowAnonymous();

app.MapGet("/health/live", () => Results.Ok(new
{
    status = "healthy",
    checkedAtUtc = DateTimeOffset.UtcNow
})).AllowAnonymous();

app.MapGet("/health/ready", CheckReadinessAsync).AllowAnonymous();

app.MapGet("/api/modules", () => Results.Ok(new[]
{
    CurriculumModule.Descriptor,
    AccreditationModule.Descriptor,
    ReportingModule.Descriptor
}));

app.MapCurriculumModule();
app.MapAccreditationModule();
app.MapReportingModule();
app.MapIdentityModule();

app.Run();

static async Task<IResult> CheckReadinessAsync(
    NpgsqlDataSource dataSource,
    ILogger<Program> logger,
    CancellationToken cancellationToken)
{
    try
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand("SELECT 1", connection);
        await command.ExecuteScalarAsync(cancellationToken);

        return Results.Ok(new
        {
            status = "ready",
            database = "available",
            checkedAtUtc = DateTimeOffset.UtcNow
        });
    }
    catch (Exception exception)
    {
        logger.LogWarning(exception, "PostgreSQL readiness check failed.");
        return Results.Problem(
            statusCode: StatusCodes.Status503ServiceUnavailable,
            title: "Service unavailable",
            detail: "PostgreSQL is not ready.");
    }
}

public partial class Program;
