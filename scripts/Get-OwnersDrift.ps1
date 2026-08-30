#!/usr/bin/env pwsh
# Compare live nuget.org owners vs registry/guiders-federation.toml desired state.
# Uses NuGet search API (owners field). No mutate — report only.
param(
    [string]$ManifestPath = (Join-Path $PSScriptRoot '..\registry\guiders-federation.toml'),
    [string[]]$PackageId,
    [switch]$OpenManageUrls
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-TomlOwnersRequired {
    param([string]$Path)
    $required = @()
    $inOwners = $false
    foreach ($line in Get-Content -LiteralPath $Path) {
        if ($line -match '^\[owners\]\s*$') { $inOwners = $true; continue }
        if ($line -match '^\[' -and $inOwners) { break }
        if ($inOwners -and $line -match '^\s*required\s*=\s*\[(.+)\]\s*$') {
            $inner = $Matches[1]
            foreach ($m in [regex]::Matches($inner, '"([^"]+)"')) {
                $required += $m.Groups[1].Value
            }
        }
    }
    if ($required.Count -eq 0) { throw "No [owners].required in $Path" }
    return $required
}

function Get-PackageOwners {
    param([string]$Id)
    $uri = "https://azuresearch-usnc.nuget.org/query?q=packageid:$Id&take=1"
    $r = Invoke-RestMethod -Uri $uri
    if (-not $r.data -or $r.data.Count -eq 0) { return $null }
    return [string[]]@($r.data[0].owners)
}

function Get-DefaultPackageIds {
    param([string]$ManifestPath)
    # Curated drift set: platform satellites known to miss org co-owner (2026-08-30).
    # Expand via -PackageId or future manifest [[package]] entries.
    @(
        'AIGuiders.Platform.InputNotation.All',
        'AIGuiders.Platform.InputNotation.Emacs',
        'AIGuiders.Platform.InputNotation.Neovim',
        'AIGuiders.Platform.InputNotation.Quarry',
        'AIGuiders.Platform.InputNotation.Vim',
        'AIGuiders.Platform.MCPlane',
        'AIGuiders.Platform.Notations',
        'AIGuiders.Platform.Notations.Argument.Kv',
        'AIGuiders.Platform.Notations.Command.Console',
        'AIGuiders.Platform.Notations.Command.Slash',
        'AIGuiders.Platform.Utilities.Adoption',
        'AIGuiders.Platform.Utilities.Adoption.Reports.Markdown',
        'AIGuiders.Platform.Utilities.Adoption.Sources'
    )
}

$requiredOwners = Get-TomlOwnersRequired -Path $ManifestPath
$ids = if ($PackageId) { $PackageId } else { Get-DefaultPackageIds -ManifestPath $ManifestPath }

$drift = @()
$ok = @()
$missing = @()

foreach ($id in $ids) {
    $live = Get-PackageOwners -Id $id
    if (-not $live) {
        $missing += [pscustomobject]@{ PackageId = $id; LiveOwners = '(not published)'; Missing = $requiredOwners }
        continue
    }
    $lack = @($requiredOwners | Where-Object { $live -notcontains $_ })
    if ($lack.Count -gt 0) {
        $drift += [pscustomobject]@{
            PackageId    = $id
            LiveOwners   = ($live -join ', ')
            Missing      = ($lack -join ', ')
            ManageUrl    = "https://www.nuget.org/packages/$id/manage/owners"
        }
    }
    else {
        $ok += $id
    }
}

Write-Host "Required owners: $($requiredOwners -join ', ')"
Write-Host "Checked: $($ids.Count) | OK: $($ok.Count) | Drift: $($drift.Count) | Not on gallery: $($missing.Count)"
Write-Host ''

if ($drift.Count -gt 0) {
    Write-Host '=== DRIFT (add missing owners on nuget.org) ===' -ForegroundColor Yellow
    $drift | Format-Table -AutoSize
    if ($OpenManageUrls) {
        foreach ($row in $drift) {
            Start-Process $row.ManageUrl
            Start-Sleep -Milliseconds 400
        }
    }
}

if ($missing.Count -gt 0) {
    Write-Host '=== NOT PUBLISHED ===' -ForegroundColor DarkYellow
    $missing | Format-Table -AutoSize
}

if ($drift.Count -eq 0 -and $missing.Count -eq 0) {
    Write-Host 'No owner drift.' -ForegroundColor Green
}

exit $(if ($drift.Count -gt 0) { 1 } else { 0 })
