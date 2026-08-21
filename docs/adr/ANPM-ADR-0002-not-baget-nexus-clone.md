# ANPM-ADR-0002: Not BaGet / not Nexus clone

**Date:** 2026-08-21  
**Status:** Accepted

## Context

Common answers for private NuGet: **BaGet**, **BaGetter**, **Nexus**, **Azure Artifacts**. Some air-gap contours reject Nexus/proxy. Community reports on BaGet:

- Upstream [loic-sharma/BaGet](https://github.com/loic-sharma/BaGet) effectively stale since 2021, 250+ open issues
- .NET 10 SDK **HTTPS everywhere** vs BaGet HTTP registration URLs ([issue #804](https://github.com/loic-sharma/BaGet/issues/804))
- Human-first admin model; no MCP

## Decision

ANPM **does not fork BaGet**. Storage may be equally simple (flat folder), but:

- **Index + policy + MCP** are first-class
- v3 index is a **thin generated layer**, not a full NuGet server reimplementation
- Sync is **manifest-driven** (`Directory.Packages.props`, lock files)

## Non-goals

- Symbol server (v1)
- Read-through cache to nuget.org on every restore
- GitLab-style integrated registry UI

## Consequences

Less out-of-box than `docker run baget` — more control for agent-first contours.
