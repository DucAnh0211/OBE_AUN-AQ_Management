param(
    [ValidateSet('api', 'database', 'web', 'ai')][string]$Target = 'api',
    [ValidateSet('initialize', 'migrate', 'baseline-existing', 'seed-demo', 'verify-demo', 'fingerprint')][string]$DatabaseCommand = 'initialize'
)
$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path $PSScriptRoot -Parent
Set-Location -LiteralPath $repositoryRoot
$envPath = Join-Path $repositoryRoot '.env'
if (-not (Test-Path -LiteralPath $envPath)) { throw 'Create .env from .env.example first.' }
# Read simple KEY=value assignments as data; never evaluate .env as PowerShell code.
foreach ($line in Get-Content -LiteralPath $envPath -Encoding UTF8) {
    if ($line -match '^\s*(#|$)') { continue }
    if ($line -notmatch '^\s*([A-Za-z_][A-Za-z0-9_]*)\s*=(.*)$') { throw 'Invalid .env assignment.' }
    $key = $Matches[1]
    $value = $Matches[2].Trim()
    if ($value.Length -ge 2 -and (($value.StartsWith("'") -and $value.EndsWith("'")) -or ($value.StartsWith('"') -and $value.EndsWith('"')))) {
        $value = $value.Substring(1, $value.Length - 2)
    }
    [Environment]::SetEnvironmentVariable($key, $value, 'Process')
}
$aliases = @{
    Jwt__SigningKey = 'JWT_SIGNING_KEY'
    BootstrapAdmin__Email = 'BOOTSTRAP_ADMIN_EMAIL'
    BootstrapAdmin__Password = 'BOOTSTRAP_ADMIN_PASSWORD'
    BootstrapAdmin__FullName = 'BOOTSTRAP_ADMIN_FULL_NAME'
    Database__SeedDemo = 'SEED_DEMO'
}
foreach ($key in $aliases.Keys) {
    [Environment]::SetEnvironmentVariable($key, [Environment]::GetEnvironmentVariable($aliases[$key], 'Process'), 'Process')
}
$apiPort = if ($env:API_PORT) { $env:API_PORT } else { '5099' }
switch ($Target) {
    'api' {
        if (-not $env:ConnectionStrings__Postgres) { throw 'Set ConnectionStrings__Postgres in .env to the intended local/Docker database.' }
        $env:ASPNETCORE_ENVIRONMENT = 'Development'
        $env:Cors__AllowedOrigins__0 = 'http://localhost:5173'
        dotnet run --project apps/api --no-launch-profile --urls "http://localhost:$apiPort"
    }
    'database' {
        if (-not $env:ConnectionStrings__Postgres) { throw 'Set ConnectionStrings__Postgres in .env first.' }
        dotnet run --project tools/database -- $DatabaseCommand
    }
    'web' {
        $env:VITE_API_URL = "http://localhost:$apiPort"
        npm.cmd --prefix apps/web run dev
    }
    'ai' {
        $pythonExe = Join-Path $repositoryRoot 'apps/ai-service/.venv/Scripts/python.exe'
        if (-not (Test-Path -LiteralPath $pythonExe)) { throw 'Create apps/ai-service/.venv and install apps/ai-service/requirements.txt first.' }
        $aiPort = if ($env:AI_PORT) { $env:AI_PORT } else { '8000' }
        & $pythonExe -m uvicorn app.main:app --app-dir apps/ai-service --host 127.0.0.1 --port $aiPort
    }
}
exit $LASTEXITCODE
