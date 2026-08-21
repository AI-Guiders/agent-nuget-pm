param(
    [Parameter()]
    [string] $Config,

    [Parameter()]
    [string] $FeedRoot,

    [Parameter()]
    [string] $ManifestPath,

    [Parameter()]
    [string] $V3BaseUrl,

    [Parameter()]
    [string] $Urls,

    [switch] $NoRebuildIndex
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$hostProj = Join-Path $repoRoot 'Anpm.Host\Anpm.Host.csproj'

$runArgs = @()
if ($Config) {
    $runArgs += @('--config', $Config)
}
elseif (Test-Path (Join-Path $repoRoot 'config\anpm.toml.example')) {
    Write-Warning 'No -Config; pass --config anpm.toml or copy config/anpm.toml.example.'
}

if ($FeedRoot) { $env:ANPM_FEED_ROOT = $FeedRoot }
if ($ManifestPath) { $env:ANPM_MANIFEST_PATH = $ManifestPath }
if ($V3BaseUrl) { $env:ANPM_V3_BASE_URL = $V3BaseUrl }
if ($Urls) { $env:ANPM_HOST_URLS = $Urls }
if ($NoRebuildIndex) { $env:ANPM_REBUILD_INDEX_ON_START = 'false' }

Write-Host "ANPM host · config=$(if ($Config) { $Config } else { 'default/env' })"
dotnet run --project $hostProj --no-launch-profile -- @runArgs
