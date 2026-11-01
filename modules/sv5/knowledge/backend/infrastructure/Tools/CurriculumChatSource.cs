using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ObeAunQa.Modules.Curriculum.Application;
using ObeAunQa.Modules.Identity;
using ObeAunQa.Modules.Identity.Infrastructure;
using ObeAunQa.Modules.Knowledge.Application;
using ObeAunQa.SharedKernel;
namespace ObeAunQa.Modules.Knowledge.Infrastructure;

public sealed class CurriculumChatSource(CurriculumService curriculum, IIdentityRepository users, IdentityDbContext identityDb) : ICurriculumChatSource
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    public async Task<CurriculumToolResult> ReadAsync(long ownerId, CurriculumToolRequest query, CancellationToken ct)
    {
        if (query is null || query.Page is < 1 or > 1000 || query.Search?.Length > 100
            || query.ProgramId <= 0 || query.VersionId <= 0 || query.CourseId <= 0 || query.CloId <= 0)
            throw new KnowledgeException(400, "Tham số tra cứu chương trình không hợp lệ.");
        var user = await users.FindByIdAsync(ownerId, ct);
        if (user is null || user.Status != "active" || user.MustChangePassword || user.Role is not (SystemRoles.Admin or SystemRoles.Lecturer))
            throw new KnowledgeException(403, "Tài khoản không còn quyền sử dụng công cụ tra cứu.");
        var access = new AccessControlService(identityDb, new ChatCurrentUser(user.Id, user.Role));
        object data;
        string title, url;
        bool Match(string value) => string.IsNullOrWhiteSpace(query.Search) || value.Contains(query.Search.Trim(), StringComparison.OrdinalIgnoreCase);
        object Page<T>(IEnumerable<T> rows) {
            var all = rows.ToArray();
            return new { items = all.Skip((query.Page - 1) * 25).Take(25), page = query.Page, pageSize = 25, totalItems = all.Length, hasMore = query.Page * 25 < all.Length };
        }
        if (query.Tool == "programs")
        {
            var allowed = (await access.GetReadableProgramIdsAsync(ct)).ToHashSet();
            var programs = new List<ProgramResponse>();
            for (var page = 1; ; page++) {
                var batch = await curriculum.GetProgramsAsync(page, 100, query.Search, false, ct);
                programs.AddRange(batch.Items.Where(p => user.Role == SystemRoles.Admin || allowed.Contains(p.Id)));
                if (page >= batch.TotalPages) break;
            }
            data = Page(programs.OrderBy(p => p.Id)); title = "Danh mục chương trình đào tạo"; url = "/curriculum";
        }
        else if (query.Tool == "versions")
        {
            if (query.ProgramId is not long programId || !await access.CanReadProgramAsync(programId, ct)) throw Denied();
            var versions = new List<ProgramVersionResponse>();
            foreach (var v in await curriculum.GetProgramVersionsAsync(programId, false, ct))
                if (await access.CanReadVersionAsync(v.Id, ct) && Match(v.VersionCode)) versions.Add(v);
            data = Page(versions.OrderBy(v => v.Id)); title = $"Phiên bản CTĐT #{programId}"; url = $"/curriculum/programs/{programId}";
        }
        else
        {
            if (query.VersionId is not long versionId || !await access.CanReadVersionAsync(versionId, ct)) throw Denied();
            var version = await curriculum.GetProgramVersionAsync(versionId, ct);
            if (version.ArchivedAt is not null) throw new KnowledgeException(410, "Phiên bản đã được lưu trữ.");
            title = $"CTĐT {version.VersionCode} · {version.Status} · {query.Tool}";
            url = $"/curriculum/versions/{versionId}";
            switch (query.Tool)
            {
                case "courses":
                    var allowed = (await access.GetReadableCourseIdsAsync(versionId, ct)).ToHashSet();
                    var courses = new List<ProgramCourseResponse>();
                    for (var page = 1; ; page++) {
                        var batch = await curriculum.GetProgramCoursesAsync(versionId, page, 100, query.Search, null, false, ct);
                        courses.AddRange(batch.Items.Where(c => user.Role == SystemRoles.Admin || allowed.Contains(c.Id)));
                        if (page >= batch.TotalPages) break;
                    }
                    data = Page(courses.OrderBy(c => c.DisplayOrder).ThenBy(c => c.Id)); break;
                case "plos":
                    data = Page((await curriculum.GetPlosAsync(versionId, ct)).Where(p => Match(p.Code + " " + p.Statement)).OrderBy(p => p.Id));
                    url += "/plos"; break;
                case "clos": case "course_plo": case "clo_plo":
                    if (query.CourseId is not long courseId || !await access.CanReadCourseAsync(courseId, ct)) throw Denied();
                    var course = await curriculum.GetProgramCourseAsync(versionId, courseId, ct);
                    if (course.ArchivedAt is not null) throw new KnowledgeException(410, "Học phần đã được lưu trữ.");
                    title += $" · {course.InstitutionalCode} {course.Name}";
                    url += $"/courses/{courseId}/outcomes";
                    if (query.Tool == "clos")
                        data = Page((await curriculum.GetClosAsync(versionId, courseId, ct)).Where(c => Match(c.Code + " " + c.Statement)).OrderBy(c => c.Id));
                    else if (query.Tool == "course_plo")
                        data = Page(await curriculum.GetCoursePloMappingsAsync(versionId, courseId, ct));
                    else {
                        if (query.CloId is not long cloId) throw new KnowledgeException(400, "Cần xác định CLO trước khi đọc mapping CLO–PLO.");
                        data = Page(await curriculum.GetCloPloMappingsAsync(versionId, courseId, cloId, ct));
                    }
                    break;
                default: throw new KnowledgeException(400, "Công cụ không được phép. Chỉ hỗ trợ tra cứu chương trình, phiên bản, học phần, PLO, CLO và mapping.");
            }
        }
        var json = JsonSerializer.SerializeToElement(data, Json);
        if (json.GetRawText().Length > 50000) throw new KnowledgeException(422, "Kết quả quá dài. Hãy thu hẹp từ khóa tra cứu.");
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(json.GetRawText()))).ToLowerInvariant();
        return new(json, new(query, hash, title, url));
    }
    private static KnowledgeException Denied() => new(403, "Chưa xác định được dữ liệu hoặc bạn không có quyền đọc dữ liệu này.");
}
