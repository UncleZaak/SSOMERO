param(
	[string]$apk
)
if ([string]::IsNullOrWhiteSpace($apk) -or -not (Test-Path $apk)) {
	Write-Output "APK_NOT_FOUND"
	exit 2
}
$out = Join-Path $env:TEMP ("ssomero_unzip_" + (Get-Random))
Remove-Item -Recurse -Force $out -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $out | Out-Null
Expand-Archive -LiteralPath $apk -DestinationPath $out -Force
Write-Output "APK_PATH:$apk"
$info = Get-Item $apk
Write-Output ("APK_SIZE:" + $info.Length)
Write-Output "META-INF:"
Get-ChildItem -Path (Join-Path $out 'META-INF') -ErrorAction SilentlyContinue | ForEach-Object { Write-Output ("  " + $_.Name) }
$netfile = Join-Path $out 'res\xml\network_security_config.xml'
if (Test-Path $netfile) { Write-Output "NETWORK_CONFIG_CONTENT:"; Get-Content $netfile -Raw } else { Write-Output "NO_NETWORK_CONFIG" }
$patterns = @('localhost','127.0.0.1','10.0.2.2','http://','devsettings','cleartextTrafficPermitted="true"')
foreach ($p in $patterns) {
	$m = Select-String -Path (Join-Path $out '*') -Pattern $p -SimpleMatch -Recurse -ErrorAction SilentlyContinue
	$count = 0
	if ($m) { $count = $m.Count }
	Write-Output ("PATTERN:" + $p + ":COUNT:" + $count)
	if ($count -gt 0) { $m | Select-Object -First 5 | ForEach-Object { Write-Output ("  " + $_.Path + ':' + $_.LineNumber + ':' + ($_.Line.Trim())) } }
}
Write-Output "SIGNATURE_FILES_IN_META_INF:"
Get-ChildItem -Path (Join-Path $out 'META-INF') -ErrorAction SilentlyContinue | Where-Object { $_.Name -match '\.(SF|RSA|DSA)$' -or $_.Name -eq 'MANIFEST.MF' } | ForEach-Object { Write-Output ("  " + $_.Name) }
Write-Output "EXTRACT_DIR:$out"
exit 0
