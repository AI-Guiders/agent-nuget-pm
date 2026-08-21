[CmdletBinding()]
param(
    [string] $PlatformRoot,
    [Parameter(Mandatory)] [string] $FeedRoot,
    [string] $Configuration = 'Release',
    [string] $ManifestPath,
    [switch] $RebuildIndex,
    [switch] $DryRun
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
if (-not $PlatformRoot) {
    $PlatformRoot = (Resolve-Path (Join-Path $repoRoot '..\guiders-platform')).Path
}

$sln = Join-Path $PlatformRoot 'AIGuiders.Platform.sln'
if (-not (Test-Path $sln)) {
    throw "guiders-platform solution not found: $sln"
}

if (-not $ManifestPath) {
    $ManifestPath = Join-Path $repoRoot 'manifest\guiders-platform-0.4.0.pins.json'
}

$outDir = Join-Path $env:TEMP ("anpm-pack-" + [Guid]::NewGuid().ToString('n'))
New-Item -ItemType Directory -Force -Path $outDir | Out-Null

Write-Host "Pack guiders-platform -> $outDir" -ForegroundColor Cyan
if (-not $DryRun) {
    dotnet pack $sln -c $Configuration -o $outDir --no-restore 2>&1 | Write-Host
    if ($LASTEXITCODE -ne 0) {
        dotnet restore $sln -c $Configuration
        dotnet pack $sln -c $Configuration -o $outDir
        if ($LASTEXITCODE -ne 0) { throw "dotnet pack failed" }
    }
}

New-Item -ItemType Directory -Force -Path $FeedRoot | Out-Null
$nupkgs = if ($DryRun) { @() } else { Get-ChildItem $outDir -Filter '*.nupkg' }
foreach ($pkg in $nupkgs) {
    $dest = Join-Path $FeedRoot $pkg.Name
    Write-Host "  stage $($pkg.Name)" -ForegroundColor DarkGray
    if (-not $DryRun) { Copy-Item -Force $pkg.FullName $dest }
}

if ($RebuildIndex -and -not $DryRun) {
    $mcpProject = Join-Path $repoRoot 'AnpmMcp\AnpmMcp.csproj'
    $payload = @{ tool = 'anpm_feed_index'; arguments = @{ feed_root = $FeedRoot; manifest_path = $ManifestPath } } | ConvertTo-Json -Compress
    $runner = Join-Path $repoRoot 'scripts\_Invoke-AnpmTool.ps1'
    & $runner -Project $mcpProject -PayloadJson $payload
}

if (-not $DryRun) {
    try { Remove-Item -Recurse -Force $outDir } catch { }
}

Write-Host "Staged $(@($nupkgs).Count) package(s) to $FeedRoot" -ForegroundColor Green
