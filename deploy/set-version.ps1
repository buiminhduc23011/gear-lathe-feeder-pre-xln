<#
.SYNOPSIS
    Updates the version number across Directory.Build.props and Web.React/package.json.

.DESCRIPTION
    Single-command version management for the entire project.

.PARAMETER Version
    The new version string, e.g. "1.0.1" or "1.1.0".

.EXAMPLE
    .\deploy\set-version.ps1 -Version "1.1.0"
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$Version
)

$ErrorActionPreference = 'Stop'

# Clean version format (e.g., 1.0.0 or 1.0.0.0)
$SemVer = $Version.Trim()
if ($SemVer -notmatch '^\d+\.\d+\.\d+(\.\d+)?$') {
    Write-Error "Invalid version format: '$Version'. Version must be in format X.Y.Z or X.Y.Z.W (e.g. 1.0.1)"
    exit 1
}

# Ensure 3-part version for SemVer/React and 4-part for Assembly/File Version
$parts = $SemVer.Split('.')
$Version3 = ($parts[0..2] -join '.')
$Version4 = if ($parts.Count -ge 4) { $SemVer } else { "$Version3.0" }

$ScriptDir  = $PSScriptRoot
$RepoRoot   = $ScriptDir | Split-Path -Parent
$PropsFile  = Join-Path $RepoRoot 'Directory.Build.props'
$PackageJsonFile = Join-Path $RepoRoot 'src\Web.React\package.json'

Write-Host "Updating version to $Version3 across project..." -ForegroundColor Cyan

# 1. Update Directory.Build.props
if (Test-Path $PropsFile) {
    $xmlContent = Get-Content $PropsFile -Raw
    $xmlContent = $xmlContent -replace '<Version>.*?</Version>', "<Version>$Version3</Version>"
    Set-Content -Path $PropsFile -Value $xmlContent -NoNewline
    Write-Host "  [OK] Updated Directory.Build.props -> Version $Version3" -ForegroundColor Green
} else {
    Write-Warning "Directory.Build.props not found at $PropsFile"
}

# 2. Update Web.React package.json
if (Test-Path $PackageJsonFile) {
    $jsonContent = Get-Content $PackageJsonFile -Raw
    $jsonContent = $jsonContent -replace '"version":\s*".*?"', """version"": ""$Version3"""
    Set-Content -Path $PackageJsonFile -Value $jsonContent -NoNewline
    Write-Host "  [OK] Updated Web.React/package.json -> version $Version3" -ForegroundColor Green
} else {
    Write-Warning "package.json not found at $PackageJsonFile"
}

Write-Host "Version update complete!" -ForegroundColor Cyan
