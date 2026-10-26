param(
    [Parameter(Mandatory = $true)][ValidatePattern('^[a-zA-Z0-9._@-]{1,100}$')][string]$UserId,
    [Parameter(Mandatory = $true)][long[]]$ProgramIds,
    [switch]$ReadOnly,
    [string]$ConfigPath = (Join-Path $PSScriptRoot '..\apps\api\reporting-access.local.json')
)
$ErrorActionPreference = 'Stop'
if ($ProgramIds.Count -eq 0 -or @($ProgramIds | Where-Object { $_ -le 0 }).Count -gt 0) {
    throw 'ProgramIds must contain positive program IDs.'
}
$ConfigPath = [IO.Path]::GetFullPath($ConfigPath)
if (Test-Path -LiteralPath $ConfigPath) {
    $config = Get-Content -LiteralPath $ConfigPath -Raw -Encoding UTF8 | ConvertFrom-Json
    if ($null -eq $config.Reporting.Access.Users) { throw 'Existing file has no Reporting.Access.Users array.' }
    if (@($config.Reporting.Access.Users | Where-Object { $_.Id -eq $UserId }).Count -gt 0) {
        throw 'User already exists. Remove the old entry explicitly before issuing a replacement key.'
    }
} else {
    $config = [pscustomobject]@{ Reporting = [pscustomobject]@{ Access = [pscustomobject]@{ Users = @() } } }
}
$rng = [Security.Cryptography.RandomNumberGenerator]::Create()
$sha = [Security.Cryptography.SHA256]::Create()
try {
    $bytes = New-Object byte[] 32
    $rng.GetBytes($bytes)
    $token = [BitConverter]::ToString($bytes).Replace('-', '').ToLowerInvariant()
    $hash = [BitConverter]::ToString($sha.ComputeHash([Text.Encoding]::UTF8.GetBytes($token))).Replace('-', '').ToLowerInvariant()
} finally { $rng.Dispose(); $sha.Dispose() }
$config.Reporting.Access.Users = @($config.Reporting.Access.Users) + [pscustomobject]@{
    Id = $UserId; TokenSha256 = $hash; ProgramIds = @($ProgramIds | Sort-Object -Unique); CanGenerate = !$ReadOnly
}
[IO.File]::WriteAllText($ConfigPath, ($config | ConvertTo-Json -Depth 10), [Text.UTF8Encoding]::new($false))
# The raw key is returned once; only its SHA-256 hash is written to configuration.
[pscustomobject]@{ UserId = $UserId; AccessToken = $token; ConfigPath = $ConfigPath }
