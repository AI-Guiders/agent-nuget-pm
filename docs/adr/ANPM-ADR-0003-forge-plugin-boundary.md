# ANPM-ADR-0003: Forge plugin boundary (optional M2)

**Date:** 2026-08-21  
**Status:** Accepted

## Context

[Agent Forge](https://github.com/AI-Guiders/agent-forge) ADR-0019 sketches marketplace/registry for **Forge plugins** (zip/fpkg), not dotnet libraries. ANPM serves **dotnet restore** feeds.

Private forge deployments may mount optional plugins; open ANPM stays consumer-neutral on GitHub.

## Decision

| Layer | Repo | Host |
|-------|------|------|
| **ANPM core + MCP + View** | `AI-Guiders/agent-nuget-pm` | Standalone `Anpm.Host` **or** optional Forge mount |
| **Forge plugin** | Same repo, `AgentForge.Plugin.PackageFeed/` | Thin host adapter over shared `Anpm.View` ([0006](ANPM-ADR-0006-dual-delivery-and-human-view.md)) |
| **Consumer wiring** | Operator / private contour | `nuget.config`, LUS, pin manifest — **not** in open repo |

Plugin provides: LDAP auth, catalog link, `/package sync` commands, WitDB co-location — **not** required for M1 standalone.

## Integration sketch

```text
AI-Guiders/agent-nuget-pm  ──publish──►  NuGet package / docker
       │
       └── Plugin.PackageFeed.dll  ──optional──►  forge.plugins.toml
```

## Consequences

Forge stays thin; ANPM can be used without Forge (other air-gap customers).
