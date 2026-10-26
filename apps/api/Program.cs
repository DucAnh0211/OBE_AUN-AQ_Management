using ObeAunQa.Modules.Accreditation;
using ObeAunQa.Modules.Curriculum;
using ObeAunQa.Modules.Reporting;
using Microsoft.EntityFrameworkCore;
using ObeAunQa.Modules.Curriculum.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("reporting-access.local.json", optional: true, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();

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
            .AllowAnyMethod());
});

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services
    .AddCurriculumModule(builder.Configuration)
    .AddAccreditationModule()
    .AddReportingModule(builder.Configuration);

var app = builder.Build();

app.Use(async (context, next) => {
    if (context.Request.Path.StartsWithSegments("/api/reporting"))
        context.Response.OnStarting(() => {
            context.Response.Headers.CacheControl = "no-store";
            return Task.CompletedTask;
        });
    await next(context);
});
app.UseExceptionHandler();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    options.RoutePrefix = "swagger";
    options.SwaggerEndpoint("/openapi/v1.json", "OBE & AUN-QA API v1");
    options.DocumentTitle = "OBE & AUN-QA API";
});

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

app.MapGet("/health/live", () => Results.Ok(new
{
    status = "healthy",
    checkedAtUtc = DateTimeOffset.UtcNow
}));

app.MapGet("/health/ready", CheckReadinessAsync);

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

static async Task<IResult> CheckReadinessAsync(
    CurriculumDbContext database,
    ILogger<Program> logger,
    CancellationToken cancellationToken)
{
    try
    {
        if (!await database.Database.CanConnectAsync(cancellationToken))
            throw new InvalidOperationException("Database is unavailable.");

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
