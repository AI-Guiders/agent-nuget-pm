# ANPM roadmap

| Phase | Scope | Status |
|-------|--------|--------|
| **M0** | Spec, manifest contract, sync script | **done** — [M0-SPEC.md](M0-SPEC.md) |
| **M1** | Core, MCP, v3 index, HTTP host, CI, export | **done** — [M1-SCAFFOLD.md](M1-SCAFFOLD.md) |
| **M2** | Optional Forge `Plugin.PackageFeed` | **done** — [M2-FORGE-PLUGIN.md](M2-FORGE-PLUGIN.md) |
| **F2** | CAD pilot manifest + sync runbook (SSCAD UNC) | **done** — [CAD-PILOT.md](CAD-PILOT.md) |
| **M3a–c** | `Anpm.View` + Host `/view/*` + Forge mount | **done** (M3a–c baseline) — [ADR-0006](adr/ANPM-ADR-0006-dual-delivery-and-human-view.md) |
| **M4** | WitDB feed index (optional) | backlog |
| **CAD** | Pilot restore e2e, LUS, UNC manifest | operator contour — KB only |

## Quick matrix

```text
M0  manifest + Sync-AnpmFeed.ps1
M1  Anpm.Core · AnpmMcp · Anpm.Host · anpm_mcp_export · CI
M2  AgentForge.Plugin.PackageFeed (sibling agent-forge)
```
