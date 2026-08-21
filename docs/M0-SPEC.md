# M0 — spec + sync pattern

**Status:** done

## Delivered

| Artifact | Purpose |
|----------|---------|
| [ANPM-ADR-0001](adr/ANPM-ADR-0001-overview.md) | Problem, architecture, MCP tool names |
| [ANPM-ADR-0002](adr/ANPM-ADR-0002-not-baget-nexus-clone.md) | Non-goals vs BaGet/Nexus |
| [CONTRACT-manifest.md](CONTRACT-manifest.md) | Pin manifest v1 schema + resolution |
| `manifest/pins.example.json` | Consumer-neutral example only |
| `scripts/Sync-AnpmFeed.ps1` | Operator sync entry (MCP `--invoke` bridge) |

## Operator loop (M0+)

1. Maintain pins outside GitHub (private manifest / DCP props mirror).
2. `ANPM_FEED_ROOT` → flat `.nupkg` directory.
3. Sync host runs `anpm_feed_sync` or `Sync-AnpmFeed.ps1`.
4. M1+ serves v3 index via `Anpm.Host` for `dotnet restore`.
