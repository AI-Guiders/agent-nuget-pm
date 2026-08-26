# ANPM-ADR-0001: Overview — agent-first NuGet PM

**Date:** 2026-08-21  
**Status:** Accepted  
**Scope:** agent-nuget-pm side project (AI-Guiders GitHub)

## Context

.NET orgs without internet need:

1. Local `.nupkg` feed (all repos)
2. Sync from a connected machine after pin bumps
3. **Agent-operable** verify/sync — not only human `dotnet nuget push`

Multiple repos may share the same pin set (`Directory.Packages.props`, central package management).

## Decision

Build **ANPM** as a **separate open repo** on GitHub (`AI-Guiders/agent-nuget-pm`):

- **Consumer-neutral** — no deployment-specific manifests or paths in GitHub
- **Optional** Forge plugin later (M2) — mount on customer forge instances
- **MCP-first** API; **human View co-primary** (dual delivery — [0006](ANPM-ADR-0006-dual-delivery-and-human-view.md))

### Architecture

```text
Sync (inet) ──► Feed storage (*.nupkg)
                    │
                    ├── Index (WitDB / manifest)
                    ├── v3 index (dotnet restore)
                    └── MCP: list | sync | verify | pin-propose
```

### Planned MCP tools (M1)

| Tool | Purpose |
|------|--------|
| `anpm.feed.status` | Packages on feed vs manifest |
| `anpm.feed.sync` | Download missing from nuget.org (sync host only) |
| `anpm.restore.verify` | Dry-run restore for path/solution |
| `anpm.pin.list` | Org pin set (Directory.Packages.props SSOT) |

## Alternatives

| Option | Rejected because |
|--------|------------------|
| BaGet on DCMS | Unmaintained lineage, HTTP/v3 quirks, not agent-first |
| Nexus | Policy / ops preference in some contours |
| UNC only | No index, no MCP, no audit |
| Consumer manifests in open repo | Couples GitHub to private deployments |

## Consequences

**Pros:** Reusable across air-gap teams; aligns with agent-forge plugin model.

**Cons:** Another product to maintain; operators own pin manifests outside this repo.

## Links

- [0002 Not BaGet](ANPM-ADR-0002-not-baget-nexus-clone.md)
- [0003 Forge boundary](ANPM-ADR-0003-forge-plugin-boundary.md)
