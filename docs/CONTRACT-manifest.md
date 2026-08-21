# ANPM manifest contract (v1)

Pin manifest is the SSOT mirror for org-wide package pins (e.g. `Directory.Packages.props` in a consumer repo).

**This open repo ships only the schema and an example file.** Deployment-specific pin sets live with the operator (private git, config management, CAD contour, etc.) — not in `AI-Guiders/agent-nuget-pm`.

## Guiders Platform (SSOT quarry)

- Manifest: `manifest/guiders-platform-0.4.0.pins.json` — pins all `AIGuiders.Platform.*` packages at **0.4.0** (ADR GUIDERS-0003).
- Stage local packs (not on nuget.org): `scripts/Stage-GuidersPlatformFeed.ps1 -FeedRoot <path> [-RebuildIndex]`.
- Then `anpm_feed_index` or `Sync-AnpmFeed.ps1` with `-ManifestPath manifest/guiders-platform-0.4.0.pins.json`.

## File

- Example: `manifest/pins.example.json` (illustrative only)
- Schema: `manifest/schema.v1.json`

## Fields

| Field | Required | Meaning |
|-------|----------|---------|
| `schema` | yes | Must be `anpm/manifest/v1` |
| `feedRoot` | no | Optional per-manifest feed default (prefer `[feed].root` in `anpm.toml`) |
| `v3BaseUrl` | no | Base URL for generated v3 index |
| `packages[]` | yes | `{ id, version }` pins |

## Resolution order

1. Tool argument `feed_root` / `manifest_path`
2. `ANPM_*` env override (escape hatch)
3. `anpm.toml` `[feed]` (`AnpmConfigLoader` → `AnpmBootstrap`)
4. Manifest `feedRoot` / `v3BaseUrl`
5. Default manifest path (when unset): `<repo>/manifest/pins.example.json`

## Operator workflow

1. Copy `pins.example.json` to your deployment manifest path.
2. Fill `packages[]` from your consumer `Directory.Packages.props` (or equivalent).
3. Copy `config/anpm.toml.example` → `anpm.toml`; set `[feed].root` (local path, UNC, volume mount).
4. Run `scripts/Sync-AnpmFeed.ps1 -Config …` or MCP `anpm_feed_sync`.

## Generated artifacts (feed root)

| Path | Purpose |
|------|---------|
| `*.nupkg` | Flat package storage |
| `.anpm/feed-index.json` | Scan snapshot |
| `.anpm/v3/index.json` | Minimal v3 service index (static; HTTP host serves in M1) |
