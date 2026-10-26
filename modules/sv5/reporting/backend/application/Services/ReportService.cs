using System.Security.Claims;
using ObeAunQa.Modules.Curriculum.Application;

namespace ObeAunQa.Modules.Reporting.Application;

public sealed class ReportService(CurriculumService curriculum, ICurriculumReportSource source,
    IReportRepository repository, IReportExporter exporter)
{
    public async Task<ReportDocument> CreateAsync(CreateReportRequest request, ClaimsPrincipal user, CancellationToken ct)
    {
        ReportBuilder.Validate(request);
        var version = await curriculum.GetProgramVersionAsync(request.VersionId, ct);
        ReportPermissions.Demand(user, version.ProgramId, true);
        var snapshot = await source.ReadAsync(request.VersionId, ct) ?? throw new ReportException(404, "Phiên bản không tồn tại hoặc đã lưu trữ.");
        ReportPermissions.Demand(user, snapshot.Program.Id, true);
        var report = ReportBuilder.Build(request, snapshot, Actor(user), DateTimeOffset.UtcNow);
        await repository.SaveAsync(report, ct);
        return report;
    }
    public Task<ReportPage> ListAsync(ClaimsPrincipal user, int page, int pageSize, CancellationToken ct)
    {
        if (page is < 1 or > 100000 || pageSize is < 1 or > 100) throw new ReportException(400, "Phân trang không hợp lệ.");
        return repository.ListAsync(ReportPermissions.Programs(user), page, pageSize, ct);
    }
    public async Task<ReportDocument> GetAsync(Guid id, ClaimsPrincipal user, CancellationToken ct)
    {
        var report = await Read(id, user, ct);
        await repository.AuditAsync(id, Actor(user), "viewed", ct);
        return report;
    }
    public async Task<ReportDownload> DownloadAsync(Guid id, string format, ClaimsPrincipal user, CancellationToken ct)
    {
        if (format is not ("docx" or "pdf")) throw new ReportException(400, "Định dạng phải là docx hoặc pdf.");
        var report = await Read(id, user, ct);
        var bytes = format == "docx" ? exporter.Docx(report) : exporter.Pdf(report);
        await repository.AuditAsync(id, Actor(user), "download-" + format, ct);
        return new(bytes, format == "pdf" ? "application/pdf" : "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            $"UC{(report.Kind == "course" ? "25" : report.Kind == "clo" ? "26" : "27")}-{id}.{format}");
    }
    public async Task RevokeAsync(Guid id, ClaimsPrincipal user, CancellationToken ct)
    {
        var report = await Read(id, user, ct);
        ReportPermissions.Demand(user, report.ProgramId, true);
        if (!await repository.RevokeAsync(id, ReportPermissions.Programs(user), Actor(user), ct))
            throw new ReportException(404, "Báo cáo không tồn tại hoặc đã thu hồi.");
    }
    private static string Actor(ClaimsPrincipal user) => user.FindFirstValue(ClaimTypes.NameIdentifier)!;
    private async Task<ReportDocument> Read(Guid id, ClaimsPrincipal user, CancellationToken ct) =>
        await repository.GetAsync(id, ReportPermissions.Programs(user), ct) ?? throw new ReportException(404, "Báo cáo không tồn tại, đã thu hồi hoặc ngoài phạm vi truy cập.");
}
