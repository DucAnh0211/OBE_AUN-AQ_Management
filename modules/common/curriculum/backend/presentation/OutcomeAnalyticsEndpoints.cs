using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ObeAunQa.Modules.Curriculum.Application;

namespace ObeAunQa.Modules.Curriculum.Presentation;

internal static class OutcomeAnalyticsEndpoints
{
    public static void MapOutcomeAnalytics(this RouteGroupBuilder root)
    {
        root.MapGet("/audit-events", (int? page, IOutcomeAnalyticsService s, CancellationToken ct) => s.AuditAsync(page??1,ct));
        var g = root.MapGroup("/program-versions/{versionId:long:min(1)}");
        g.AddEndpointFilter(async (context,next) => { context.HttpContext.Response.Headers.CacheControl="no-store"; return await next(context); });
        g.MapPost("/copy-outcomes/{sourceVersionId:long:min(1)}", async (long versionId, long sourceVersionId, IOutcomeCloneService s, CancellationToken ct) => {
            await s.CopyAsync(versionId,sourceVersionId,ct); return Results.NoContent();
        });
        g.MapGet("/outcome-policy", (long versionId, IOutcomeAnalyticsService s, CancellationToken ct) => s.PolicyAsync(versionId, ct));
        g.MapGet("/outcome-options", (long versionId, IOutcomeAnalyticsService s, CancellationToken ct) => s.OptionsAsync(versionId,ct));
        g.MapGet("/outcome-summary", (long versionId,long? offeringId,long? programCourseId,string? period,string? cohort,bool? preview,IOutcomeAnalyticsService s,CancellationToken ct) =>
            s.SummaryAsync(new(versionId,offeringId,programCourseId,period,cohort,preview??false),ct));
        g.MapGet("/outcome-students", (long versionId,long? offeringId,long? programCourseId,string? period,string? cohort,bool? preview,int? page,string? search,IOutcomeAnalyticsService s,CancellationToken ct) =>
            s.StudentsAsync(new(versionId,offeringId,programCourseId,period,cohort,preview??false),page??1,search,ct));
        g.MapPut("/outcome-policy", (long versionId, OutcomePolicy p, IOutcomeAnalyticsService s, CancellationToken ct) => s.SavePolicyAsync(versionId, p, ct));
        foreach (var path in new[] { "/outcome-readiness", "/clo-results", "/plo-results" })
            g.MapGet(path, (long versionId, long? offeringId, long? programCourseId, string? period, string? cohort, bool? preview, IOutcomeAnalyticsService s, CancellationToken ct) =>
                s.ReadAsync(new(versionId, offeringId, programCourseId, period, cohort, preview ?? false), ct));
        g.MapGet("/offerings/{offeringId:long:min(1)}/clo-results", (long versionId, long offeringId, bool? preview, IOutcomeAnalyticsService s, CancellationToken ct) => s.ReadAsync(new(versionId, offeringId, Preview: preview ?? false), ct));
        g.MapGet("/outcome-comparisons", (long versionId, string period, string otherPeriod, long? offeringId, long? programCourseId, string? cohort, IOutcomeAnalyticsService s, CancellationToken ct) =>
            s.CompareAsync(new(versionId, offeringId, programCourseId, period, cohort), otherPeriod, ct));
        g.MapPost("/outcome-snapshots", (long versionId, OutcomeScope scope, IOutcomeAnalyticsService s, CancellationToken ct) => {
            if (versionId != scope.VersionId) AcademicValidator.Invalid("versionId", "Phạm vi không khớp phiên bản.");
            return s.CreateSnapshotAsync(scope, ct);
        });
        g.MapGet("/outcome-snapshots/latest", async (long versionId,long? offeringId,long? programCourseId,string? period,string? cohort,bool? preview,IOutcomeAnalyticsService s,CancellationToken ct) => {
            var result=await s.LatestAsync(new(versionId,offeringId,programCourseId,period,cohort,preview??false),ct);
            return result==null?Results.NotFound():Results.Ok(result);
        });
        g.MapGet("/outcome-snapshots/{id:guid}", (long versionId, Guid id, IOutcomeAnalyticsService s, CancellationToken ct) => s.SnapshotAsync(versionId, id, ct));
        g.MapGet("/outcome-snapshots/{id:guid}/export", async (long versionId, Guid id, IOutcomeAnalyticsService s, CancellationToken ct) =>
            Results.File(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(await s.SnapshotAsync(versionId, id, ct), new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web)), "application/json", $"obe-{id}.json"));
    }
}
