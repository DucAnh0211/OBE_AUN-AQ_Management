namespace ObeAunQa.Modules.Identity;

public sealed class BootstrapAdminOptions
{
    public const string SectionName = "BootstrapAdmin";
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = "Quản trị viên";
    public string Password { get; set; } = string.Empty;
}
