# CAD pilot (F2) — offline feed rollout

Operator contour for **PdfExtract** and SSCAD air-gap restore. Open repo ships schema + example manifest only; deployment paths stay in CAD.

## Prerequisites

- Windows host with access to UNC feed (sync host) or writable local mirror
- `dotnet` SDK 10+
- Internet on sync host (for `anpm_feed_sync` downloads)

## 1. Environment

```powershell
$env:ANPM_FEED_ROOT = '\\dpc-av-m-cms\Repository\nuget-feed'
$env:ANPM_MANIFEST_PATH = 'D:\anpm\cad-pilot\cad-pilot.pins.json'
```

Copy from repo:

- `manifest\cad-pilot.pins.json` → operator path above
- `config\anpm.cad-pilot.toml.example` → `D:\anpm\cad-pilot\anpm.toml` (edit if needed)

Default pins (v1):

| Package | Version |
|---------|---------|
| `OutWit.Database.Core` | 14.0.1 |
| `OutWit.Database.EntityFramework` | 14.0.1 |

Optional: stage Guiders Platform quarry packages into the same feed:

```powershell
.\scripts\Invoke-CadPilotFeed.ps1 -StageGuidersPlatform
```

## 2. Sync feed

Dry-run:

```powershell
cd agent-nuget-pm
.\scripts\Invoke-CadPilotFeed.ps1 -DryRun
```

Execute sync + v3 index rebuild:

```powershell
.\scripts\Invoke-CadPilotFeed.ps1
```

Status only:

```powershell
.\scripts\Invoke-CadPilotFeed.ps1 -StatusOnly
```

## 3. Start ANPM host

```powershell
.\scripts\Start-CadPilotHost.ps1
```

Human UI: `http://127.0.0.1:5088/view/feed`  
NuGet v3: `http://127.0.0.1:5088/v3/index.json`

Point consumer `nuget.config` package source at the v3 URL (or UNC + local ANPM host on CAD server).

## 4. MCP smoke (Cursor)

```powershell
.\scripts\_Invoke-AnpmTool.ps1 `
  -Project AnpmMcp\AnpmMcp.csproj `
  -Config D:\anpm\cad-pilot\anpm.toml `
  -PayloadJson '{"tool":"anpm_feed_status"}'
```

## 5. Verify restore (PdfExtract / solution)

On a machine with feed access:

```powershell
dotnet restore path\to\PdfExtract.sln
```

Or MCP `anpm_restore_verify` with `target_path`.

## Troubleshooting

| Symptom | Check |
|---------|-------|
| `manifest_path is required` | `ANPM_MANIFEST_PATH` or `[feed].manifest_path` in `anpm.toml` |
| UNC write denied | run sync as feed admin; confirm share ACL |
| Missing WitDB 14.0.1 | re-run sync; confirm nuget.org reachable on sync host |
| View shows config error | host started without `--config` / env overrides |

## Related

- [ANPM-ADR-0006](adr/ANPM-ADR-0006-dual-delivery-and-human-view.md) — Human View
- KB: `agent-notes` → `agent-nuget-pm/README.md` (CAD contour boundary)
