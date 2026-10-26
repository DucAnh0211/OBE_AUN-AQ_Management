using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ObeAunQa.Modules.Reporting.Infrastructure;

public sealed class ReportingUser
{
    public string Id { get; set; } = "";
    public string TokenSha256 { get; set; } = "";
    public long[] ProgramIds { get; set; } = [];
    public bool CanGenerate { get; set; }
}


