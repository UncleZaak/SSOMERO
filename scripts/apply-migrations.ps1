param(
    [Parameter(Mandatory=$true)]
    [string]$ConnectionString,

    [string]$Configuration = "Release",

    [string]$InfrastructureProject = "src/AcademicSystem.Infrastructure",
    [string]$StartupProject = "src/AcademicSystem.API"
)

Write-Host "Applying EF Core migrations..."
Write-Host "Infrastructure project: $InfrastructureProject"
Write-Host "Startup project: $StartupProject"

$dotnet = "dotnet"

$cmd = "$dotnet ef database update --project `"$InfrastructureProject`" --startup-project `"$StartupProject`" --connection `"$ConnectionString`" -c ApplicationDbContext -v"

Write-Host "Running: $cmd"

$proc = Start-Process -FilePath $dotnet -ArgumentList @("ef","database","update","--project", $InfrastructureProject, "--startup-project", $StartupProject, "--connection", $ConnectionString, "-c", "ApplicationDbContext", "-v") -NoNewWindow -Wait -PassThru

if ($proc.ExitCode -ne 0) {
    Write-Error "dotnet ef failed with exit code $($proc.ExitCode). Check the output above for details."
    exit $proc.ExitCode
}

Write-Host "Migrations applied successfully."