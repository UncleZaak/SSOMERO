# start-all.ps1 — Launch backend API and MAUI app together
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path

Write-Host "Starting Ssomero.Api..." -ForegroundColor Cyan
$api = Start-Process -FilePath "dotnet" `
    -ArgumentList "run --project `"$root\Ssomero.Api\Ssomero.Api.csproj`" --launch-profile http" `
    -PassThru -NoNewWindow

Write-Host "Waiting for API to be ready..."
$ready = $false
for ($i = 0; $i -lt 30; $i++) {
    Start-Sleep -Seconds 1
    try {
        $null = Invoke-WebRequest -Uri "http://localhost:5136/api/health" -UseBasicParsing -TimeoutSec 2
        $ready = $true
        break
    } catch { }
}

if ($ready) {
    Write-Host "API is ready!" -ForegroundColor Green
} else {
    Write-Host "WARNING: API may not be ready yet, starting MAUI anyway..." -ForegroundColor Yellow
}

Write-Host "Starting Ssomero MAUI app..." -ForegroundColor Cyan
$maui = Start-Process -FilePath "dotnet" `
    -ArgumentList "run --project `"$root\Ssomero\Ssomero.csproj`"" `
    -PassThru -NoNewWindow

Write-Host "Both projects started. Press Ctrl+C to stop." -ForegroundColor Green
Write-Host "API PID: $($api.Id)  |  MAUI PID: $($maui.Id)"

try { Wait-Process -Id $api.Id } catch { }
