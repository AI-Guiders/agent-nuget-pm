# agent-nuget-pm (ANPM)

**Agent-first NuGet package manager** for .NET teams in **offline / air-gap** contours — without Nexus, without treating BaGet as production spine.

| | |
|---|---|
| **Primary interface** | MCP tools + DOI-style commands |
| **Human UI** | **`Anpm.View`** — co-primary; standalone `Anpm.Host` or Forge mount ([ADR-0006](docs/adr/ANPM-ADR-0006-dual-delivery-and-human-view.md)) |
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
| **F2** | CAD pilot (SSCAD UNC feed) | **done** — [CAD-PILOT.md](docs/CAD-PILOT.md) |
| **M3** | **`Anpm.View`** standalone + Forge mount | **done** (baseline) — [ADR-0006](docs/adr/ANPM-ADR-0006-dual-delivery-and-human-view.md) |
| **L3** | Federation **registry ops** (publish orchestration, owners drift, symbols) | **proposed** — [ADR-0007](docs/adr/ANPM-ADR-0007-federation-registry-operations.md) |

## Docs

- [ADR index](docs/adr/README.md)
- [Overview ADR](docs/adr/ANPM-ADR-0001-overview.md)
- [MCP tool surface ADR](docs/adr/ANPM-ADR-0004-mcp-tool-surface.md)
- [TOML config ADR](docs/adr/ANPM-ADR-0005-toml-config.md)
- [Manifest contract](docs/CONTRACT-manifest.md)
- [M1 scaffold plan](docs/M1-SCAFFOLD.md)
- [M0 spec](docs/M0-SPEC.md)
- [M2 Forge plugin](docs/M2-FORGE-PLUGIN.md)
- [CAD pilot (F2)](docs/CAD-PILOT.md)
- [Roadmap](docs/ROADMAP.md)

## Quick start (M1 scaffold)

```powershell
cd agent-nuget-pm
dotnet test
# Copy config/anpm.toml.example → D:/anpm/anpm.toml and edit [feed] paths.
./scripts/Sync-AnpmFeed.ps1 -Config D:/anpm/anpm.toml -DryRun
./scripts/Start-AnpmHost.ps1 -Config D:/anpm/anpm.toml
# Human View: http://127.0.0.1:5088/view/feed
# CAD pilot: ./scripts/Invoke-CadPilotFeed.ps1 (see docs/CAD-PILOT.md)
# MCP manifest for Cursor (~/.cursor/mcp.json):
./scripts/_Invoke-AnpmTool.ps1 -Project AnpmMcp/AnpmMcp.csproj -PayloadJson '{"tool":"anpm_mcp_export"}' -Config D:/anpm/anpm.toml
# Or: args ["--config","D:/anpm/anpm.toml"], env {}
```

Point `nuget.config` package source at `http://127.0.0.1:5088/v3/index.json`.

## Related (AI-Guiders)

- [agent-forge](https://github.com/AI-Guiders/agent-forge) — optional plugin host (M2)
- [git-mcp](https://github.com/AI-Guiders/git-mcp) — MCP-first tooling pattern

## Status

**M0–M2 + F2 + M3 baseline shipped** — standalone ANPM + Forge plugin + Human View; WitDB index = next.
