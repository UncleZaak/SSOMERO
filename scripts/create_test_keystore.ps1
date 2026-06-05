param(
	[string]$keystorePath = "test_keystore.jks",
	[string]$alias = "ssomero_test",
	[string]$storepass = "testpass",
	[string]$keypass = "testpass",
	[string]$dname = "CN=ssomero, OU=dev, O=ssomero, L=Unknown, S=Unknown, C=US"
)

# This script requires keytool from a Java JDK. If keytool is not available the script will fail.
$keytool = "keytool"
$exists = Get-Command $keytool -ErrorAction SilentlyContinue
if (-not $exists) {
	Write-Output "KEYTOOL_MISSING"
	exit 2
}

# Fail if keystore already exists
$keystoreFull = Resolve-Path -Path $keystorePath -ErrorAction SilentlyContinue
if ($keystoreFull) {
	Write-Output "KEYSTORE_EXISTS:$keystoreFull"
	exit 1
}

# Ensure directory
$dir = Split-Path $keystorePath -Parent
if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir | Out-Null }

# Build keytool argument list and call safely
$args = @(
	'-genkeypair',
	'-alias', $alias,
	'-keyalg', 'RSA',
	'-keysize', '2048',
	'-validity', '10000',
	'-keystore', $keystorePath,
	'-storepass', $storepass,
	'-keypass', $keypass,
	'-dname', $dname,
	'-storetype', 'JKS'
)

Write-Output "RUNNING: $keytool $($args -join ' ')"
try {
	& $keytool @args
	$exitCode = $LASTEXITCODE
} catch {
	Write-Output "KEYTOOL_FAILED_EXCEPTION: $($_.Exception.Message)"
	exit 3
}
if ($exitCode -ne 0) { Write-Output "KEYTOOL_FAILED:$exitCode"; exit 3 }

if (Test-Path $keystorePath) {
	Write-Output "KEYSTORE_CREATED:$keystorePath"
	$list = & $keytool -list -keystore $keystorePath -storepass $storepass 2>&1
	if ($list -match $alias) { Write-Output "ALIAS_FOUND:$alias"; exit 0 } else { Write-Output "ALIAS_NOT_FOUND"; $list; exit 4 }
} else {
	Write-Output "KEYSTORE_NOT_CREATED"
	exit 5
}
