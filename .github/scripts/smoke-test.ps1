Param(
	[string]$ApiBaseUrl,
	[string]$TestUserEmail,
	[string]$TestUserPassword
)

Write-Host "Running smoke tests against $ApiBaseUrl"

# Create a report object
$report = [ordered]@{
	timestamp = (Get-Date).ToString("o")
	git_sha = $env:GITHUB_SHA
	environment = $env:GITHUB_ENVIRONMENT
	endpoints = @()
	status = "unknown"
}

function Write-ReportAndExit([int]$code) {
	$reportJson = $report | ConvertTo-Json -Depth 5
	$reportJson | Out-File -FilePath smoke-test-report.json -Encoding utf8
	Write-Host "Smoke test report written to smoke-test-report.json"
	exit $code
}

function Fail([string]$msg) {
	Write-Host "FAIL: $msg"
	$report.status = "failed"
	$report.endpoints += @{ endpoint = "unknown"; result = "fail"; message = $msg }
	Write-ReportAndExit 1
}

function Ensure-Secret([string]$val,[string]$name){
	if ([string]::IsNullOrEmpty($val)){
		Write-Host "Missing required secret: $name" -ForegroundColor Red
		exit 1
	}
}

function Invoke-WithRetry([ScriptBlock]$action, [string]$label, [int]$maxAttempts=3, [int]$timeoutSec=15){
	$attempt=0
	$backoff=1
	while ($attempt -lt $maxAttempts){
		$attempt++
		try{
			$sw = [System.Diagnostics.Stopwatch]::StartNew()
			$result = & $action
			$sw.Stop()
			return @{ success=$true; result=$result; latency_ms = $sw.ElapsedMilliseconds }
		} catch {
			$err = $_.Exception.Message
			Write-Host "Attempt $attempt/$maxAttempts for $label failed: $err"
			if ($attempt -ge $maxAttempts){
				return @{ success=$false; error=$err }
			}
			Start-Sleep -Seconds $backoff
			$backoff = [Math]::Min($backoff * 2, 30)
		}
	}
}

function Get-TlsCertificate([string]$host, [int]$port=443){
	try{
		$tcp = New-Object System.Net.Sockets.TcpClient($host,$port)
		$stream = $tcp.GetStream()
		$ssl = New-Object System.Net.Security.SslStream($stream,$false,({$true}))
		$ssl.AuthenticateAsClient($host)
		$cert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2($ssl.RemoteCertificate)
		$ssl.Close()
		$tcp.Close()
		return @{ subject = $cert.Subject; issuer = $cert.Issuer; notBefore = $cert.NotBefore; notAfter = $cert.NotAfter }
	} catch {
		return $null
	}
}

# Ensure parameters fall back to env vars and are present
$ApiBaseUrl = if ($ApiBaseUrl) { $ApiBaseUrl } else { $env:SMOKE_API_BASEURL }
$TestUserEmail = if ($TestUserEmail) { $TestUserEmail } else { $env:SMOKE_USER_EMAIL }
$TestUserPassword = if ($TestUserPassword) { $TestUserPassword } else { $env:SMOKE_USER_PASSWORD }

Ensure-Secret $ApiBaseUrl "SMOKE_API_BASEURL"
Ensure-Secret $TestUserEmail "SMOKE_USER_EMAIL"
Ensure-Secret $TestUserPassword "SMOKE_USER_PASSWORD"

# Basic TLS cert check for the API host
try{
	$uri = [System.Uri]::new($ApiBaseUrl)
	if ($uri.Scheme -ne 'https'){
		Fail "API base url must use HTTPS"
	}
	$host = $uri.Host
	$cert = Get-TlsCertificate -host $host -port $uri.Port
	if ($null -eq $cert){
		Fail "Unable to retrieve TLS certificate from $host"
	}
	$now = Get-Date
	if ($now -gt $cert.notAfter){
		Fail "TLS certificate expired on $($cert.notAfter)"
	}
	$report.tls = @{ host=$host; notAfter = $cert.notAfter; issuer = $cert.issuer }
} catch {
	Fail "TLS validation failed: $($_.Exception.Message)"
}

Write-Host "Checking /api/health"
$res = Invoke-WithRetry({ Invoke-RestMethod -Method Get -Uri "$ApiBaseUrl/api/health" -UseBasicParsing -TimeoutSec 10 -ErrorAction Stop }, "/api/health", 3, 10)
if (-not $res.success){ Fail "/api/health failed: $($res.error)" }
Write-Host "Health OK"
$report.endpoints += @{ endpoint = "/api/health"; result = "ok"; latency_ms = $res.latency_ms }

Write-Host "Checking /api/health/ready"
 $res = Invoke-WithRetry({ Invoke-RestMethod -Method Get -Uri "$ApiBaseUrl/api/health/ready" -UseBasicParsing -TimeoutSec 10 -ErrorAction Stop }, "/api/health/ready", 3, 10)
 if (-not $res.success){ Fail "/api/health/ready failed: $($res.error)" }
 Write-Host "Readiness OK"
 $report.endpoints += @{ endpoint = "/api/health/ready"; result = "ok"; latency_ms = $res.latency_ms }

Write-Host "Attempting login"
 $res = Invoke-WithRetry({ Invoke-RestMethod -Method Post -Uri "$ApiBaseUrl/api/auth/login" -Body (@{email=$TestUserEmail; password=$TestUserPassword} | ConvertTo-Json) -ContentType 'application/json' -TimeoutSec 15 -ErrorAction Stop }, "/api/auth/login", 3, 15)
 if (-not $res.success){ Fail "/api/auth/login failed: $($res.error)" }
 $loginResp = $res.result
 if (-not $loginResp.accessToken){ Fail "Login did not return access token" }
 Write-Host "Login OK"
 $report.endpoints += @{ endpoint = "/api/auth/login"; result = "ok"; latency_ms = $res.latency_ms }

Write-Host "Attempting token refresh"
 $res = Invoke-WithRetry({ Invoke-RestMethod -Method Post -Uri "$ApiBaseUrl/api/auth/refresh" -Body (@{refreshToken=$loginResp.refreshToken} | ConvertTo-Json) -ContentType 'application/json' -TimeoutSec 15 -ErrorAction Stop }, "/api/auth/refresh", 3, 15)
 if (-not $res.success){ Fail "/api/auth/refresh failed: $($res.error)" }
 $refreshResp = $res.result
 if (-not $refreshResp.accessToken){ Fail "Refresh did not return new access token" }
 Write-Host "Refresh OK"
 $report.endpoints += @{ endpoint = "/api/auth/refresh"; result = "ok"; latency_ms = $res.latency_ms }

Write-Host "Checking SignalR negotiate"
try {
	$res = Invoke-WithRetry({ Invoke-RestMethod -Method Post -Uri "$ApiBaseUrl/hubs/notifications/negotiate" -Headers @{ Authorization = "Bearer $($loginResp.accessToken)" } -ContentType 'application/json' -TimeoutSec 15 -ErrorAction Stop }, "/hubs/notifications/negotiate", 3, 15)
	if (-not $res.success){ Fail "/hubs/notifications/negotiate failed: $($res.error)" }
	$negotiate = $res.result
	Write-Host "SignalR negotiate OK"
	$report.endpoints += @{ endpoint = "/hubs/notifications/negotiate"; result = "ok"; latency_ms = $res.latency_ms; details = @{ url = ($negotiate.url -as [string]) } }
} catch {
	Fail "SignalR negotiate failed: $_"
}

Write-Host "Smoke tests passed"
$report.status = "passed"
$report | ConvertTo-Json -Depth 5 | Out-File -FilePath smoke-test-report.json -Encoding utf8
Write-Host "Smoke test report written to smoke-test-report.json"
exit 0
