[CmdletBinding()]
param(
    [string] $Config,
    [string] $FeedRoot = $env:ANPM_FEED_ROOT,
    [string] $ManifestPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
if (-not $Config) {
    $Config = Join-Path $repoRoot 'config\anpm.cad-pilot.toml.example'
}

if (-not $ManifestPath) {
    $ManifestPath = Join-Path $repoRoot 'manifest\cad-pilot.pins.json'
}

if (-not $FeedRoot) {
    throw "Set ANPM_FEED_ROOT or pass -FeedRoot for CAD pilot host."
}

$env:ANPM_FEED_ROOT = $FeedRoot
$env:ANPM_MANIFEST_PATH = $ManifestPath

$start = Join-Path $repoRoot 'scripts\Start-AnpmHost.ps1'
& $start -Config $Config -FeedRoot $FeedRoot -ManifestPath $ManifestPath
