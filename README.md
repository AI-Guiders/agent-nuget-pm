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
| **M1** | Standalone host: feed + minimal v3 index + MCP |
| **M2** | Optional **Forge zoo plugin** (`Plugin.PackageFeed`) for LDAP/catalog |

## Docs

- [ADR index](docs/adr/README.md)
- [Overview ADR](docs/adr/ANPM-ADR-0001-overview.md)

## Related (AI-Guiders)

- [agent-forge](https://github.com/AI-Guiders/agent-forge) — optional plugin host (M2)
- [git-mcp](https://github.com/AI-Guiders/git-mcp) — MCP-first tooling pattern

## Status

**Concept / M0** — repository bootstrap. No production host yet.

