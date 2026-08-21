# M2 — Forge plugin (optional mount)

**Status:** done (2026-08-21)

## Project

`AgentForge.Plugin.PackageFeed/` — zoo-tier plugin; **not required** for standalone ANPM.

## Build

Requires sibling [agent-forge](https://github.com/AI-Guiders/agent-forge) checkout:

```text
open/
  agent-forge/
  agent-nuget-pm/
```

Or set `ForgeAbstractionsProject` to `AgentForge.Abstractions.csproj`.

## Mount (operator)

Copy `plugins/forge.package-feed.example.toml` into forge `plugins/` and merge `config/forge.anpm.example.json` into forge host `appsettings`:

- `anpm.feed_root`
- `anpm.manifest_path`
- optional `anpm.v3_base_url`

`ANPM_*` env vars remain escape overrides only.

## Surfaces

| Surface | Path |
|---------|------|
| HTTP | `GET /api/v1/package-feed/status` |
| HTTP | `POST /api/v1/package-feed/sync?dry_run=true` |
| DOI | `/package feed status`, `/package feed sync` |
| Feature | `anpm_package_feed` |

Core logic delegates to `Anpm.Core` (same as MCP tools).
