[CmdletBinding()]
param(
    [string] $Config,
    [string] $ManifestPath,
    [string] $FeedRoot = $env:ANPM_FEED_ROOT,
    [string] $V3BaseUrl,
    [switch] $DryRun,
    [switch] $SkipIndex,
    [switch] $StatusOnly,
    [switch] $StageGuidersPlatform
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot

if (-not $Config) {
    $Config = Join-Path $repoRoot 'config\anpm.cad-pilot.toml.example'
    Write-Warning "No -Config; using example $Config (copy outside git for production)."
}

if (-not $ManifestPath) {
    $ManifestPath = Join-Path $repoRoot 'manifest\cad-pilot.pins.json'
}

if (-not $FeedRoot) {
    throw @"
CAD feed root is required.
Set ANPM_FEED_ROOT (e.g. \\dpc-av-m-cms\Repository\nuget-feed) or pass -FeedRoot.
"@
}

$env:ANPM_FEED_ROOT = $FeedRoot
$env:ANPM_MANIFEST_PATH = $ManifestPath

Write-Host "CAD pilot ANPM feed" -ForegroundColor Cyan
Write-Host "  config:   $Config"
Write-Host "  manifest: $ManifestPath"
Write-Host "  feed:     $FeedRoot"

if ($StageGuidersPlatform) {
    $stage = Join-Path $repoRoot 'scripts\Stage-GuidersPlatformFeed.ps1'
    if (-not (Test-Path $stage)) {
        throw "Missing staging script: $stage"
    }

    & $stage -FeedRoot $FeedRoot -RebuildIndex:(-not $SkipIndex) -DryRun:$DryRun
    if ($LASTEXITCODE -ne 0) { throw "Stage-GuidersPlatformFeed failed" }
}

$sync = Join-Path $repoRoot 'scripts\Sync-AnpmFeed.ps1'
& $sync `
    -Config $Config `
    -ManifestPath $ManifestPath `
    -FeedRoot $FeedRoot `
    -V3BaseUrl $V3BaseUrl `
    -DryRun:$DryRun `
    -SkipIndex:$SkipIndex `
    -StatusOnly:$StatusOnly

if ($LASTEXITCODE -ne 0) { throw "Sync-AnpmFeed failed" }
