param(
	[string]$apkPath,
	[string]$keystorePath,
	[string]$alias = 'ssomero_test',
	[string]$storepass = 'testpass',
	[string]$keypass = 'testpass'
)
if (-not (Test-Path $apkPath)) { Write-Output 'APK_NOT_FOUND'; exit 2 }
if (-not (Test-Path $keystorePath)) { Write-Output 'KEYSTORE_NOT_FOUND'; exit 3 }
# Try to find apksigner in common SDK locations
$possible = @(
	"$env:ANDROID_HOME\build-tools\*\apksigner.exe",
	"$env:ANDROID_SDK_ROOT\build-tools\*\apksigner.exe",
	"C:\Program Files\Android\Android SDK\build-tools\*\apksigner.exe",
	"C:\Program Files (x86)\Android\android-sdk\build-tools\*\apksigner.exe"
)
$apksigner = $null
foreach ($p in $possible) { $found = Get-ChildItem -Path $p -ErrorAction SilentlyContinue | Select-Object -First 1; if ($found) { $apksigner = $found.FullName; break } }
if (-not $apksigner) { Write-Output 'APKSIGNER_NOT_FOUND'; exit 4 }
Write-Output ("APKSIGNER:" + $apksigner)
# Use apksigner to sign the APK
& $apksigner sign --ks $keystorePath --ks-key-alias $alias --ks-pass pass:$storepass --key-pass pass:$keypass $apkPath
if ($LASTEXITCODE -ne 0) { Write-Output 'APKSIGN_FAIL'; exit 5 }
# Verify
& $apksigner verify --print-certs $apkPath
if ($LASTEXITCODE -ne 0) { Write-Output 'APKSIGN_VERIFY_FAIL'; exit 6 }
Write-Output 'APKSIGN_OK'
exit 0
