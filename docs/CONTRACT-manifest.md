# ANPM manifest contract (v1)

Pin manifest is the SSOT mirror for org-wide package pins (e.g. `Directory.Packages.props` in a consumer repo).

**This open repo ships only the schema and an example file.** Deployment-specific pin sets live with the operator (private git, config management, CAD contour, etc.) — not in `AI-Guiders/agent-nuget-pm`.

## File

- Example: `manifest/pins.example.json` (illustrative only)
- Schema: `manifest/schema.v1.json`

## Fields

| Field | Required | Meaning |
|-------|----------|---------|
| `schema` | yes | Must be `anpm/manifest/v1` |
| `feedRoot` | no | Default feed path; `${ANPM_FEED_ROOT}` or env expansion |
| `v3BaseUrl` | no | Base URL for generated v3 index |
| `packages[]` | yes | `{ id, version }` pins |

## Resolution order

1. Tool argument `feed_root` / `manifest_path`
2. Env `ANPM_FEED_ROOT` / `ANPM_MANIFEST_PATH`
3. Manifest `feedRoot`
4. Default manifest path (when unset): `<repo>/manifest/pins.example.json`

## Operator workflow

1. Copy `pins.example.json` to your deployment manifest path.
2. Fill `packages[]` from your consumer `Directory.Packages.props` (or equivalent).
3. Set `ANPM_FEED_ROOT` to your flat feed directory (local path, UNC, volume mount).
4. Run `scripts/Sync-AnpmFeed.ps1` or MCP `anpm_feed_sync` from an internet-connected sync host.

## Generated artifacts (feed root)

| Path | Purpose |
|------|---------|
| `*.nupkg` | Flat package storage |
| `.anpm/feed-index.json` | Scan snapshot |
| `.anpm/v3/index.json` | Minimal v3 service index (static; HTTP host serves in M1) |
