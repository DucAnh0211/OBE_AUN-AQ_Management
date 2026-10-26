using System.Security.Claims;

namespace ObeAunQa.Modules.Reporting.Application;

public static class ReportPermissions
{
    public static long[] Programs(ClaimsPrincipal user) => user.FindAll("reporting:program")
        .Select(c => long.TryParse(c.Value, out var id) ? id : 0).Where(id => id > 0).Distinct().ToArray();
    public static bool CanGenerate(ClaimsPrincipal user) => user.HasClaim("reporting:generate", "true");
    public static void Demand(ClaimsPrincipal user, long programId, bool generate = false)
    {
        if (user.Identity?.IsAuthenticated != true) throw new Application.ReportException(401, "Cần đăng nhập để sử dụng báo cáo.");
        if (!Programs(user).Contains(programId) || (generate && !CanGenerate(user)))
            throw new Application.ReportException(403, "Bạn không có quyền thực hiện thao tác cho chương trình này.");
    }
}
