[CmdletBinding()]
param(
    [string] $ManifestPath,
    [string] $FeedRoot,
    [string] $V3BaseUrl,
    [switch] $DryRun,
    [switch] $SkipIndex,
    [switch] $StatusOnly
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
if (-not $ManifestPath) {
    $ManifestPath = $env:ANPM_MANIFEST_PATH
}
if (-not $ManifestPath) {
    $ManifestPath = Join-Path $repoRoot 'manifest\pins.example.json'
}

if (-not $FeedRoot) {
    $FeedRoot = $env:ANPM_FEED_ROOT
}

if (-not $FeedRoot) {
    throw 'FeedRoot is required: -FeedRoot or env ANPM_FEED_ROOT.'
}

$env:ANPM_REPO_ROOT = $repoRoot
$env:ANPM_FEED_ROOT = $FeedRoot
$env:ANPM_MANIFEST_PATH = $ManifestPath
if ($V3BaseUrl) { $env:ANPM_V3_BASE_URL = $V3BaseUrl }

$mcpProject = Join-Path $repoRoot 'AnpmMcp\AnpmMcp.csproj'
if (-not (Test-Path $mcpProject)) {
    throw "ANPM MCP project not found: $mcpProject"
}

function Invoke-AnpmTool {
    param(
        [Parameter(Mandatory)] [string] $Tool,
        [hashtable] $Args = @{}
    )

    $payload = @{ tool = $Tool; arguments = $Args } | ConvertTo-Json -Depth 6 -Compress
    $runner = Join-Path $repoRoot 'scripts\_Invoke-AnpmTool.ps1'
    if (-not (Test-Path $runner)) {
        throw "Missing runner: $runner"
    }

    & $runner -Project $mcpProject -PayloadJson $payload
}

Write-Host "ANPM sync" -ForegroundColor Cyan
Write-Host "  manifest: $ManifestPath"
Write-Host "  feed:     $FeedRoot"

$statusJson = Invoke-AnpmTool -Tool 'anpm_feed_status' -Args @{
    manifest_path = $ManifestPath
    feed_root     = $FeedRoot
}
Write-Host $statusJson

if ($StatusOnly) {
    return
}

if ($DryRun) {
    $syncJson = Invoke-AnpmTool -Tool 'anpm_feed_sync' -Args @{
        manifest_path = $ManifestPath
        feed_root     = $FeedRoot
        dry_run       = $true
        rebuild_index = $false
    }
    Write-Host $syncJson
    return
}

$syncJson = Invoke-AnpmTool -Tool 'anpm_feed_sync' -Args @{
    manifest_path = $ManifestPath
    feed_root     = $FeedRoot
    dry_run       = $false
    rebuild_index = (-not $SkipIndex)
    v3_base_url   = $V3BaseUrl
}
Write-Host $syncJson

$finalStatus = Invoke-AnpmTool -Tool 'anpm_feed_status' -Args @{
    manifest_path = $ManifestPath
    feed_root     = $FeedRoot
}
Write-Host $finalStatus
