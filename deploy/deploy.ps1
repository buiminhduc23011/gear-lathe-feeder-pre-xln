#Requires -Version 5.1
<#
.SYNOPSIS
    Gear Lathe Feeder Server - Full Deployment (wraps setup.ps1)

.DESCRIPTION
    Convenience wrapper that runs setup.ps1 to configure and install
    the Gear Lathe Feeder Server as a Windows Service.

    Prerequisites (must be installed before running this script):
      - Microsoft SQL Server 2019+ (or SQL Server Express)
      - .NET 10 Runtime (if not using self-contained build)

    Run as Administrator.

.PARAMETER InstallDir
    Target installation directory. Default: C:\GearLatheFeeder

.PARAMETER SetupOnly
    Run only setup.ps1 (configuration wizard + service install).

.EXAMPLE
    # Full deployment from the dist/ folder as Administrator:
    powershell -ExecutionPolicy Bypass -File deploy.ps1

.EXAMPLE
    # Install to a custom directory:
    powershell -ExecutionPolicy Bypass -File deploy.ps1 -InstallDir D:\GearLatheFeeder

.EXAMPLE
    # Re-run just to update config (e.g. change connection string):
    powershell -ExecutionPolicy Bypass -File deploy.ps1 -SetupOnly
#>

param(
    [string]$InstallDir = 'C:\GearLatheFeeder',
    [switch]$SetupOnly
)

$ErrorActionPreference = 'Stop'
$ScriptDir = $PSScriptRoot

# --- Check Administrator privileges ------------------------------------------
$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole(
             [Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host ''
    Write-Host '  Error: This script must be run as Administrator.' -ForegroundColor Red
    Write-Host "  Right-click PowerShell and select 'Run as administrator'" -ForegroundColor Yellow
    exit 1
}

# --- Banner -------------------------------------------------------------------
Write-Host ''
Write-Host '================================================================' -ForegroundColor Cyan
Write-Host '|      Gear Lathe Feeder Server - Deployment Script            |' -ForegroundColor Cyan
Write-Host '================================================================' -ForegroundColor Cyan
Write-Host ''
Write-Host "  Install Dir : $InstallDir" -ForegroundColor Gray

if ($SetupOnly) {
    Write-Host '  Mode        : Setup/Configure only' -ForegroundColor Gray
} else {
    Write-Host '  Mode        : Full deploy' -ForegroundColor Gray
}
Write-Host ''

# --- Run setup ----------------------------------------------------------------
$setupScript = Join-Path $ScriptDir 'setup.ps1'
if (-not (Test-Path $setupScript)) {
    Write-Host "  [FAIL] setup.ps1 not found at: $setupScript" -ForegroundColor Red
    exit 1
}

& powershell -ExecutionPolicy Bypass -File $setupScript -InstallDir $InstallDir

if ($LASTEXITCODE -ne 0) {
    Write-Host ''
    Write-Host '  [FAIL] Setup script reported an error.' -ForegroundColor Red
    exit 1
}

# --- Final summary ------------------------------------------------------------
Write-Host ''
Write-Host '================================================================' -ForegroundColor Green
Write-Host '|        Gear Lathe Feeder Deployment Complete!                |' -ForegroundColor Green
Write-Host '================================================================' -ForegroundColor Green
Write-Host ''
