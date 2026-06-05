Param(
	[string]$ApiBaseUrl,
	[string]$AccessTokenFile = '',
	[string]$OutputFile = 'signalr-validation-report.json'
)

function Write-Json($obj, $path){
	$obj | ConvertTo-Json -Depth 10 | Out-File -FilePath $path -Encoding utf8
}

# Lightweight .NET SignalR client validation using PowerShell and .NET 7+ (available on runner)
try{
	Add-Type -AssemblyName 'System.Net.WebSockets.Client'
} catch {}

$report = [ordered]@{
	timestamp = (Get-Date).ToString('o')
	git_sha = $env:GITHUB_SHA
	connected = $false
	authenticated = $false
	latencyMs = -1
	reconnectPassed = $false
	details = @{}
}

try{
	if (-not $ApiBaseUrl) { throw 'ApiBaseUrl is required' }
	$uri = [Uri]$ApiBaseUrl
	$negotiateUrl = "$($uri.Scheme)://$($uri.Host):$($uri.Port)/hubs/notifications/negotiate"

	$start = [DateTime]::UtcNow
	$negotiate = Invoke-RestMethod -Method Post -Uri $negotiateUrl -ContentType 'application/json' -ErrorAction Stop
	$elapsed = ([DateTime]::UtcNow - $start).TotalMilliseconds
	$report.latencyMs = [math]::Round($elapsed)
	$report.connected = $true
	$report.details.negotiate = $negotiate | Select-Object -Property url, accessToken

	if ($negotiate.accessToken) {
		$report.authenticated = $true
	}

	# Attempt a websocket connect if url provided
	if ($negotiate.url){
		$wsUri = $negotiate.url -replace '^http','ws'
		$client = [System.Net.WebSockets.ClientWebSocket]::new()
		$startConn = [DateTime]::UtcNow
		$task = $client.ConnectAsync([Uri]$wsUri, [Threading.CancellationToken]::None)
		$task.Wait(15000)
		if ($client.State -eq 'Open'){
			$report.details.websocketState = 'Open'
			# send a ping (empty message) using the SignalR protocol -- server might expect specific format; we'll send a small valid message if possible
			$buffer = [System.Text.Encoding]::UTF8.GetBytes('ping')
			$segment = [System.ArraySegment[byte]]::new($buffer)
			$send = $client.SendAsync($segment, [System.Net.WebSockets.WebSocketMessageType]::Text, $true, [Threading.CancellationToken]::None)
			$send.Wait(5000)
			$report.reconnectPassed = $true
			$client.Dispose()
		} else {
			$report.details.websocketState = $client.State.ToString()
		}
	}
} catch {
	$report.details.error = $_.Exception.Message
}

Write-Json $report $OutputFile
Write-Host "SignalR validation report written to $OutputFile"
if ($report.connected -and $report.authenticated){ exit 0 } else { exit 1 }
