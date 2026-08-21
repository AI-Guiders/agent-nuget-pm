# ANPM-ADR-0004: MCP tool surface (M1)

**Date:** 2026-08-21  
**Status:** Accepted  
**Scope:** `AnpmMcp` stdio server (M1 scaffold)

## Context

ADR-0001 named DOI-style tools (`anpm.feed.status`, …). MCP hosts and git-mcp precedent use underscore wire names. M1 needs a stable, small tool set for agents and `Sync-AnpmFeed.ps1`.

## Decision

### Wire names (MCP `CallTool`)

| Wire name | DOI alias | Purpose |
|-----------|-----------|---------|
| `anpm_feed_status` | `anpm.feed.status` | Manifest pins vs feed `.nupkg` |
| `anpm_pin_list` | `anpm.pin.list` | Read pin manifest |
| `anpm_feed_sync` | `anpm.feed.sync` | Download missing pins (inet sync host) |
| `anpm_feed_index` | *(index op)* | Rebuild `.anpm/v3` static index |
| `anpm_restore_verify` | `anpm.restore.verify` | `dotnet restore --dry-run` against feed |

### Configuration

| Env | Meaning |
|-----|---------|
| `ANPM_FEED_ROOT` | Flat feed directory |
| `ANPM_MANIFEST_PATH` | Pin manifest JSON |
| `ANPM_V3_BASE_URL` | Public v3 base for generated index |
| `ANPM_REPO_ROOT` | Repo root for default manifest path |

Tool arguments override env/manifest defaults.

### CLI bridge (scripts)

`AnpmMcp --invoke <tool> [--key value ...]` — used by `scripts/Sync-AnpmFeed.ps1` and CI without MCP stdio.

### Non-goals (M1)

- HTTP MCP transport
- Push/publish tools (human `dotnet nuget push` remains escape hatch)
- npm/OCI tools

## Consequences

**Pros:** Parity with git-mcp; scriptable sync; operators configure feed/manifest via env only.

**Cons:** v3 index is static files until M1 HTTP host ships; `anpm_feed_sync` requires inet + dotnet CLI on sync machine.

## Links

- [0001 Overview](ANPM-ADR-0001-overview.md)
- [Manifest contract](../CONTRACT-manifest.md)
