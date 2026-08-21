# agent-nuget-pm (ANPM)

**Agent-first NuGet package manager** for .NET teams in **offline / air-gap** contours — without Nexus, without treating BaGet as production spine.

| | |
|---|---|
| **Primary interface** | MCP tools + DOI-style commands |
| **Human UI** | Thin view / admin (optional Forge plugin) |
| **Storage** | Flat `.nupkg` feed (UNC, volume, object store) |
| **Index** | WitDB (planned) or JSON manifest (M0) |

## Problem

Many repos, no `nuget.org`, no corporate proxy — but `dotnet restore` still needs a **managed local feed** and an **org-wide pin/sync process**. BaGet/Nexus are human-first servers; UNC shares alone have no agent surface.

## Non-goals

- Clone GitLab/Nexus feature matrix
- Replace `nuget.org` on the public internet
- npm/OCI feeds (v1)
- BaGet fork
- Per-deployment consumer manifests in this repo (operators maintain their own pin files)

## Roadmap

| Phase | Deliverable |
|-------|-------------|
| **M0** | Spec + sync script pattern | **done** |
| **M1** | Standalone host: feed + v3 index + MCP + HTTP | **done** |
| **M2** | Optional **Forge zoo plugin** (`Plugin.PackageFeed`) | **done** |

## Docs

- [ADR index](docs/adr/README.md)
- [Overview ADR](docs/adr/ANPM-ADR-0001-overview.md)
- [MCP tool surface ADR](docs/adr/ANPM-ADR-0004-mcp-tool-surface.md)
- [Manifest contract](docs/CONTRACT-manifest.md)
- [M1 scaffold plan](docs/M1-SCAFFOLD.md)
- [M0 spec](docs/M0-SPEC.md)
- [M2 Forge plugin](docs/M2-FORGE-PLUGIN.md)
- [Roadmap](docs/ROADMAP.md)

## Quick start (M1 scaffold)

```powershell
cd agent-nuget-pm
dotnet test
$env:ANPM_FEED_ROOT = 'C:\local\nuget-feed'
$env:ANPM_MANIFEST_PATH = 'C:\path\to\your\pins.json'   # copy from manifest/pins.example.json
./scripts/Sync-AnpmFeed.ps1 -FeedRoot $env:ANPM_FEED_ROOT -ManifestPath $env:ANPM_MANIFEST_PATH -DryRun
./scripts/Start-AnpmHost.ps1 -FeedRoot $env:ANPM_FEED_ROOT
# MCP manifest for Cursor (~/.cursor/mcp.json):
./scripts/_Invoke-AnpmTool.ps1 anpm_mcp_export --command_path D:/path/to/AnpmMcp.exe
```

Point `nuget.config` package source at `http://127.0.0.1:5088/v3/index.json`.

## Related (AI-Guiders)

- [agent-forge](https://github.com/AI-Guiders/agent-forge) — optional plugin host (M2)
- [git-mcp](https://github.com/AI-Guiders/git-mcp) — MCP-first tooling pattern

## Status

**M0–M2 shipped** — standalone ANPM + optional Forge plugin; WitDB index + CAD e2e = next.
