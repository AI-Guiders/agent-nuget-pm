param(
    [Parameter()]
    [string] $FeedRoot = $env:ANPM_FEED_ROOT,

    [Parameter()]
    [string] $ManifestPath = $env:ANPM_MANIFEST_PATH,

    [Parameter()]
    [string] $V3BaseUrl = $env:ANPM_V3_BASE_URL,

    [Parameter()]
    [string] $Urls = $env:ANPM_HOST_URLS,

    [switch] $NoRebuildIndex
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$hostProj = Join-Path $repoRoot 'Anpm.Host\Anpm.Host.csproj'

if (-not $FeedRoot) {
    throw 'Set ANPM_FEED_ROOT or pass -FeedRoot (flat .nupkg directory).'
}

if (-not $V3BaseUrl) { $V3BaseUrl = 'http://127.0.0.1:5088/v3' }
if (-not $Urls) { $Urls = 'http://127.0.0.1:5088' }

$env:ANPM_FEED_ROOT = $FeedRoot
if ($ManifestPath) { $env:ANPM_MANIFEST_PATH = $ManifestPath }
$env:ANPM_V3_BASE_URL = $V3BaseUrl
$env:ANPM_HOST_URLS = $Urls
$env:ANPM_REBUILD_INDEX_ON_START = $(if ($NoRebuildIndex) { 'false' } else { 'true' })

Write-Host "ANPM host · feed=$FeedRoot · v3=$V3BaseUrl · listen=$Urls"
dotnet run --project $hostProj --no-launch-profile
