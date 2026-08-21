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
| **M0** | Spec + sync script pattern |
| **M1** | Standalone host: feed + minimal v3 index + MCP ← **scaffold started** |
| **M2** | Optional **Forge zoo plugin** (`Plugin.PackageFeed`) for LDAP/catalog |

## Docs

- [ADR index](docs/adr/README.md)
- [Overview ADR](docs/adr/ANPM-ADR-0001-overview.md)
- [MCP tool surface ADR](docs/adr/ANPM-ADR-0004-mcp-tool-surface.md)
- [Manifest contract](docs/CONTRACT-manifest.md)
- [M1 scaffold plan](docs/M1-SCAFFOLD.md)

## Quick start (M1 scaffold)

```powershell
cd agent-nuget-pm
dotnet test
$env:ANPM_FEED_ROOT = 'C:\local\nuget-feed'
$env:ANPM_MANIFEST_PATH = 'C:\path\to\your\pins.json'   # copy from manifest/pins.example.json
./scripts/Sync-AnpmFeed.ps1 -FeedRoot $env:ANPM_FEED_ROOT -ManifestPath $env:ANPM_MANIFEST_PATH -DryRun
```

## Related (AI-Guiders)

- [agent-forge](https://github.com/AI-Guiders/agent-forge) — optional plugin host (M2)
- [git-mcp](https://github.com/AI-Guiders/git-mcp) — MCP-first tooling pattern

## Status

**Concept / M1 scaffold** — MCP + Core + manifest contract; HTTP host next.
