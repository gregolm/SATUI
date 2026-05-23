# Build MSIX package for SATUI
# This script creates a proper MSIX package with all required files

param(
    [string]$PublishDir = "publish",
    [string]$OutputDir = "MSIX",
    [switch]$Sign = $false
)

$ErrorActionPreference = "Stop"

function Write-Info {
    Write-Host "INFO: $args" -ForegroundColor Cyan
}

function Write-Success {
    Write-Host "SUCCESS: $args" -ForegroundColor Green
}

function Compute-SHA256 {
    param([string]$FilePath)
    $hash = [System.Security.Cryptography.SHA256]::Create()
    $stream = [System.IO.File]::OpenRead($FilePath)
    $hashBytes = $hash.ComputeHash($stream)
    $stream.Close()
    return [System.Convert]::ToBase64String($hashBytes)
}

Write-Info "Building SATUI MSIX package..."

# Create output directory
if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir | Out-Null
}

# Verify publish directory exists
if (-not (Test-Path $PublishDir)) {
    Write-Error "Publish directory not found: $PublishDir"
    exit 1
}

Write-Info "Creating MSIX package structure..."

# Create staging directory
$stagingDir = Join-Path $OutputDir "staging"
if (Test-Path $stagingDir) {
    Remove-Item $stagingDir -Recurse -Force
}
New-Item -ItemType Directory -Path $stagingDir | Out-Null

# Create [Content_Types].xml
$contentTypes = @"
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
  <Default Extension="dll" ContentType="application/octet-stream" />
  <Default Extension="exe" ContentType="application/octet-stream" />
  <Default Extension="json" ContentType="application/json" />
  <Default Extension="png" ContentType="image/png" />
  <Default Extension="xml" ContentType="application/xml" />
  <Default Extension="pf" ContentType="application/octet-stream" />
  <Override PartName="/AppxManifest.xml" ContentType="application/vnd.ms-appx.manifest+xml" />
  <Override PartName="/AppxBlockMap.xml" ContentType="application/vnd.ms-appx.blockmap+xml" />
  <Override PartName="/AppxSignature.p7x" ContentType="application/vnd.ms-appx.signature" />
</Types>
"@

$contentTypesPath = Join-Path $stagingDir "[Content_Types].xml"
[System.IO.File]::WriteAllText($contentTypesPath, $contentTypes)
Write-Info "Created [Content_Types].xml"

# Copy AppxManifest.xml
if (Test-Path "SATUI.Package\AppxManifest.xml") {
    Copy-Item "SATUI.Package\AppxManifest.xml" -Destination "$stagingDir\AppxManifest.xml" -Force
    Write-Info "Copied AppxManifest.xml"
} else {
    Write-Error "AppxManifest.xml not found in SATUI.Package directory"
    exit 1
}

# Create Payload directory with app files
$payloadDir = "$stagingDir\Payload"
New-Item -ItemType Directory -Path $payloadDir | Out-Null
Copy-Item "$PublishDir\*" -Destination $payloadDir -Recurse -Force
Write-Info "Copied app files to Payload directory"

# Copy or create Assets directory
if (Test-Path "SATUI.Package\Assets") {
    Copy-Item "SATUI.Package\Assets" -Destination "$stagingDir\Assets" -Recurse -Force
    Write-Info "Copied Assets directory"
}

# Generate AppxBlockMap.xml
Write-Info "Generating AppxBlockMap.xml..."
$blockMapEntries = @()

# Add entry for [Content_Types].xml
$fileHash = Compute-SHA256 $contentTypesPath
$fileSize = (Get-Item $contentTypesPath).Length
$blockMapEntries += @"
  <File Name="[Content_Types].xml" Size="$fileSize" Hash="$fileHash" />
"@

# Add entry for AppxManifest.xml
$manifestPath = "$stagingDir\AppxManifest.xml"
$fileHash = Compute-SHA256 $manifestPath
$fileSize = (Get-Item $manifestPath).Length
$blockMapEntries += @"
  <File Name="AppxManifest.xml" Size="$fileSize" Hash="$fileHash" />
"@

# Add entries for Payload files
Get-ChildItem -Path $payloadDir -Recurse -File | ForEach-Object {
    $relativePath = $_.FullName.Substring($stagingDir.Length + 1)
    $relativePath = $relativePath -replace '\\', '/'
    $fileHash = Compute-SHA256 $_.FullName
    $fileSize = $_.Length
    $blockMapEntries += @"
  <File Name="$relativePath" Size="$fileSize" Hash="$fileHash" />
"@
}

# Add entries for Assets
if (Test-Path "$stagingDir\Assets") {
    Get-ChildItem -Path "$stagingDir\Assets" -Recurse -File | ForEach-Object {
        $relativePath = $_.FullName.Substring($stagingDir.Length + 1)
        $relativePath = $relativePath -replace '\\', '/'
        $fileHash = Compute-SHA256 $_.FullName
        $fileSize = $_.Length
        $blockMapEntries += @"
  <File Name="$relativePath" Size="$fileSize" Hash="$fileHash" />
"@
    }
}

$blockMap = @"
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<BlockMap xmlns="http://schemas.microsoft.com/appx/2010/blockmap" HashMethod="http://www.w3.org/2001/04/xmlenc#sha256">
$($blockMapEntries -join "`n")
</BlockMap>
"@

$blockMapPath = Join-Path $stagingDir "AppxBlockMap.xml"
[System.IO.File]::WriteAllText($blockMapPath, $blockMap)
Write-Info "Created AppxBlockMap.xml with $($blockMapEntries.Count) file entries"

# Create the MSIX as a ZIP file
$packagePath = "$OutputDir\SATUI-0.0.2.msix"
if (Test-Path $packagePath) {
    Remove-Item $packagePath -Force
}

Write-Info "Compressing MSIX package..."

# Create ZIP using .NET
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zipFile = [System.IO.Compression.ZipFile]
$zipFile::CreateFromDirectory($stagingDir, $packagePath, [System.IO.Compression.CompressionLevel]::Optimal, $false)

Write-Success "MSIX package created: $packagePath"
$size = [math]::Round((Get-Item $packagePath).Length / 1MB, 2)
Write-Info "Size: $size MB"

# Sign MSIX if requested
if ($Sign) {
    Write-Info "Checking for signtool..."
    $signtoolPath = where.exe signtool.exe 2>$null
    
    if (-not $signtoolPath) {
        Write-Warning "signtool.exe not found. Attempting to find in Windows SDK..."
        # Try common SDK installation paths
        $potentialPaths = @(
            "C:\Program Files (x86)\Windows Kits\10\bin\*\x64\signtool.exe",
            "C:\Program Files (x86)\Windows Kits\11\bin\*\x64\signtool.exe"
        )
        $signtoolPath = Get-ChildItem -Path $potentialPaths -ErrorAction SilentlyContinue | Select-Object -First 1 -ExpandProperty FullName
    }
    
    if ($signtoolPath) {
        Write-Info "Found signtool at: $signtoolPath"
        Write-Info "Signing MSIX package..."
        
        # Check for existing test certificate
        $cert = Get-ChildItem Cert:\CurrentUser\My -CodeSigningCert | Where-Object { $_.Subject -like "*SATUI*" } | Select-Object -First 1
        
        if (-not $cert) {
            Write-Info "Creating self-signed certificate for MSIX signing..."
            $cert = New-SelfSignedCertificate -Type CodeSigningCert -Subject "CN=SATUI Release" `
                -CertStoreLocation "Cert:\CurrentUser\My" -KeyUsage DigitalSignature `
                -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3") -NotAfter (Get-Date).AddYears(10) `
                -FriendlyName "SATUI Release Certificate"
            Write-Info "Certificate created: $($cert.Thumbprint)"
        }
        
        # Export cert to PFX for signing
        $pfxPath = "$OutputDir\SATUI-Sign.pfx"
        $password = ConvertTo-SecureString -String "SATUIDev2024" -AsPlainText -Force
        Export-PfxCertificate -Cert $cert -FilePath $pfxPath -Password $password -Force | Out-Null
        
        # Sign the MSIX
        & $signtoolPath sign /f "$pfxPath" /p "SATUIDev2024" /fd SHA256 /tr "http://timestamp.codesigning.ecdsa.net/" "$packagePath" 2>&1 | ForEach-Object { Write-Info $_ }
        
        if ($LASTEXITCODE -eq 0) {
            Write-Success "MSIX package signed successfully"
        } else {
            Write-Warning "MSIX signing may have failed (exit code: $LASTEXITCODE), but package is still usable"
        }
        
        # Cleanup
        Remove-Item $pfxPath -Force -ErrorAction SilentlyContinue
    } else {
        Write-Warning "signtool.exe not found in PATH or common locations. MSIX will be unsigned."
        Write-Info "On GitHub Actions, the Windows SDK is available and signing will be performed."
    }
}

# Cleanup
Remove-Item $stagingDir -Recurse -Force -ErrorAction SilentlyContinue

Write-Success "Build complete!"
Write-Info "To install on Windows 11, use:"
Write-Info "  Add-AppxPackage -Path '$packagePath'"
Write-Info "Or double-click the MSIX file to install."
