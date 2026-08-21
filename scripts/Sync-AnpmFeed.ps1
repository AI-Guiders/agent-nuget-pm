[CmdletBinding()]
param(
    [string] $Config,
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
if (-not $Config) {
    $defaultConfig = Join-Path $repoRoot 'config\anpm.toml.example'
    if (Test-Path $defaultConfig) {
        Write-Warning "No -Config; using example $defaultConfig (copy to anpm.toml for production)."
        $Config = $defaultConfig
    }
}

if (-not $ManifestPath -and $Config) {
    # Manifest path usually lives in TOML; tool args below can still override per call.
    $ManifestPath = $null
}
if (-not $ManifestPath) {
    $ManifestPath = Join-Path $repoRoot 'manifest\pins.example.json'
}

if (-not $FeedRoot -and -not $Config -and -not $env:ANPM_FEED_ROOT) {
    throw 'FeedRoot is required: set [feed].root in anpm.toml (-Config), pass -FeedRoot, or ANPM_FEED_ROOT override.'
}

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

    & $runner -Project $mcpProject -PayloadJson $payload -Config $Config
}

Write-Host "ANPM sync" -ForegroundColor Cyan
Write-Host "  config:   $(if ($Config) { $Config } else { '(default / env)' })"
Write-Host "  manifest: $ManifestPath"
Write-Host "  feed:     $FeedRoot"

$statusArgs = @{}
if ($ManifestPath) { $statusArgs.manifest_path = $ManifestPath }
if ($FeedRoot) { $statusArgs.feed_root = $FeedRoot }
$statusJson = Invoke-AnpmTool -Tool 'anpm_feed_status' -Args $statusArgs
Write-Host $statusJson

if ($StatusOnly) {
    return
}

if ($DryRun) {
    $dryArgs = @{ dry_run = $true; rebuild_index = $false }
    if ($ManifestPath) { $dryArgs.manifest_path = $ManifestPath }
    if ($FeedRoot) { $dryArgs.feed_root = $FeedRoot }
    $syncJson = Invoke-AnpmTool -Tool 'anpm_feed_sync' -Args $dryArgs
    Write-Host $syncJson
    return
}

$syncArgs = @{
    dry_run       = $false
    rebuild_index = (-not $SkipIndex)
}
if ($ManifestPath) { $syncArgs.manifest_path = $ManifestPath }
if ($FeedRoot) { $syncArgs.feed_root = $FeedRoot }
if ($V3BaseUrl) { $syncArgs.v3_base_url = $V3BaseUrl }

$syncJson = Invoke-AnpmTool -Tool 'anpm_feed_sync' -Args $syncArgs
Write-Host $syncJson

$finalStatus = Invoke-AnpmTool -Tool 'anpm_feed_status' -Args $statusArgs
Write-Host $finalStatus
