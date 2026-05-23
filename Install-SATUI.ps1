# SATUI Installer
# This script installs SATUI to Program Files and creates shortcuts

param(
    [string]$InstallPath = "$env:ProgramFiles\SATUI"
)

$ErrorActionPreference = "Stop"

function Write-Header {
    param([string]$Text)
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host $Text -ForegroundColor Cyan
    Write-Host "========================================`n" -ForegroundColor Cyan
}

function Test-Admin {
    $currentUser = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($currentUser)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

# Check for admin rights
if (-not (Test-Admin)) {
    Write-Host "ERROR: This installer must be run as Administrator." -ForegroundColor Red
    Write-Host "Please right-click and select 'Run as administrator'." -ForegroundColor Red
    exit 1
}

Write-Header "SATUI Installer v0.0.2"

# Find the app files (should be in the same directory as this script)
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$appFilesDir = $scriptDir

if (-not (Test-Path "$appFilesDir\SATUI.exe")) {
    Write-Host "ERROR: SATUI.exe not found in $appFilesDir" -ForegroundColor Red
    Write-Host "Make sure you extract all files from the installer package." -ForegroundColor Red
    exit 1
}

# Create install directory
Write-Host "Installing to: $InstallPath" -ForegroundColor Green
if (Test-Path $InstallPath) {
    Write-Host "Removing existing installation..."
    Remove-Item $InstallPath -Recurse -Force -ErrorAction SilentlyContinue
}

New-Item -ItemType Directory -Path $InstallPath -Force | Out-Null

# Copy files
Write-Host "Copying application files..."
Copy-Item -Path "$appFilesDir\*" -Destination $InstallPath -Recurse -Force

# Create shortcuts
$desktopPath = [System.IO.Path]::Combine([Environment]::GetFolderPath("Desktop"), "SATUI.lnk")
$startMenuPath = [System.IO.Path]::Combine([Environment]::GetFolderPath("StartMenu"), "Programs", "SATUI.lnk")

Write-Host "Creating shortcuts..."

# Desktop shortcut
$WshShell = New-Object -ComObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut($desktopPath)
$Shortcut.TargetPath = "$InstallPath\SATUI.exe"
$Shortcut.WorkingDirectory = $InstallPath
$Shortcut.IconLocation = "$InstallPath\satellite.ico"
$Shortcut.Save()

# Start Menu shortcut
New-Item -ItemType Directory -Path ([System.IO.Path]::GetDirectoryName($startMenuPath)) -Force | Out-Null
$Shortcut = $WshShell.CreateShortcut($startMenuPath)
$Shortcut.TargetPath = "$InstallPath\SATUI.exe"
$Shortcut.WorkingDirectory = $InstallPath
$Shortcut.IconLocation = "$InstallPath\satellite.ico"
$Shortcut.Save()

# Add to Add/Remove Programs
$regPath = "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\SATUI"
New-Item -Path $regPath -Force | Out-Null
New-ItemProperty -Path $regPath -Name "DisplayName" -Value "SATUI" -Force | Out-Null
New-ItemProperty -Path $regPath -Name "DisplayVersion" -Value "0.0.2" -Force | Out-Null
New-ItemProperty -Path $regPath -Name "Publisher" -Value "SATUI Project" -Force | Out-Null
New-ItemProperty -Path $regPath -Name "InstallLocation" -Value $InstallPath -Force | Out-Null
New-ItemProperty -Path $regPath -Name "UninstallString" -Value "$InstallPath\Uninstall-SATUI.ps1" -Force | Out-Null

# Create uninstall script
$uninstallScript = @"
# SATUI Uninstaller
param([switch]`$Confirm)

`$InstallPath = "$InstallPath"

if (-not `$Confirm) {
    `$response = [System.Windows.Forms.MessageBox]::Show(
        "Are you sure you want to uninstall SATUI?",
        "Uninstall SATUI",
        [System.Windows.Forms.MessageBoxButtons]::YesNo,
        [System.Windows.Forms.MessageBoxIcon]::Question
    )
    if (`$response -ne "Yes") { exit 0 }
}

# Kill running instances
Get-Process SATUI -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 1

# Remove files
Remove-Item `$InstallPath -Recurse -Force -ErrorAction SilentlyContinue

# Remove shortcuts
Remove-Item ([System.IO.Path]::Combine([Environment]::GetFolderPath("Desktop"), "SATUI.lnk")) -Force -ErrorAction SilentlyContinue
Remove-Item ([System.IO.Path]::Combine([Environment]::GetFolderPath("StartMenu"), "Programs", "SATUI.lnk")) -Force -ErrorAction SilentlyContinue

# Remove registry entry
Remove-Item "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\SATUI" -Force -ErrorAction SilentlyContinue

Write-Host "SATUI has been uninstalled." -ForegroundColor Green
"@

Set-Content -Path "$InstallPath\Uninstall-SATUI.ps1" -Value $uninstallScript

Write-Header "Installation Complete!"
Write-Host "SATUI has been installed successfully." -ForegroundColor Green
Write-Host "Launch it from your Desktop or Start Menu." -ForegroundColor Green
Write-Host ""

# Prompt to launch
$response = [System.Windows.Forms.MessageBox]::Show(
    "Launch SATUI now?",
    "Installation Complete",
    [System.Windows.Forms.MessageBoxButtons]::YesNo,
    [System.Windows.Forms.MessageBoxIcon]::Question
)

if ($response -eq "Yes") {
    Start-Process "$InstallPath\SATUI.exe" -WorkingDirectory $InstallPath
}
