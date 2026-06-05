Param(
	[string]$SmokeReport = 'smoke-test-report.json',
	[string]$SignalRReport = 'signalr-validation-report.json',
	[string]$OutputFile = 'release-gate-report.json'
)

function Read-Json($path){
	if (-not (Test-Path $path)) { return $null }
	return (Get-Content $path -Raw) | ConvertFrom-Json
}

$smoke = Read-Json $SmokeReport
$sig = Read-Json $SignalRReport

$report = [ordered]@{
	status = 'passed'
	checks = @()
	timestamp = (Get-Date).ToString('o')
	gitSha = $env:GITHUB_SHA
	version = $env.VERSION_NAME
}

function Add-Check($name,$status,$message){
	$report.checks += @{ name=$name; status=$status; message=$message }
	if ($status -ne 'passed') { $report.status = 'failed' }
}

if ($null -eq $smoke){
	Add-Check 'smoke-tests' 'failed' 'smoke report missing'
} else {
	if ($smoke.status -eq 'passed') { Add-Check 'smoke-tests' 'passed' 'smoke tests passed' } else { Add-Check 'smoke-tests' 'failed' 'smoke tests failed' }
}

if ($null -eq $sig){
	Add-Check 'signalr-validation' 'failed' 'signalr report missing'
} else {
	if ($sig.connected -and $sig.authenticated) { Add-Check 'signalr-validation' 'passed' 'signalr connected and authenticated' } else { Add-Check 'signalr-validation' 'failed' 'signalr checks failed' }
}

# TLS check included in smoke report under tls if present
if ($smoke -and $smoke.tls){
	$notAfter = [datetime]$smoke.tls.notAfter
	if ($notAfter -lt (Get-Date)) { Add-Check 'tls' 'failed' 'certificate expired' } else { Add-Check 'tls' 'passed' 'certificate valid' }
} else {
	Add-Check 'tls' 'failed' 'tls info missing'
}

Write-Output (ConvertTo-Json $report -Depth 6) | Out-File -FilePath $OutputFile -Encoding utf8
Write-Host "Release gate report written to $OutputFile"
if ($report.status -eq 'passed') { exit 0 } else { exit 2 }
