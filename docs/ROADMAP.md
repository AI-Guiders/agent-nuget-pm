# ANPM roadmap

| Phase | Scope | Status |
|-------|--------|--------|
| **M0** | Spec, manifest contract, sync script | **done** — [M0-SPEC.md](M0-SPEC.md) |
| **M1** | Core, MCP, v3 index, HTTP host, CI, export | **done** — [M1-SCAFFOLD.md](M1-SCAFFOLD.md) |
| **M2** | Optional Forge `Plugin.PackageFeed` | **done** — [M2-FORGE-PLUGIN.md](M2-FORGE-PLUGIN.md) |
| **M1+** | WitDB feed index (optional) | backlog |
| **CAD** | Pilot restore e2e, LUS, UNC manifest | operator contour — KB only |

## Quick matrix

```text
M0  manifest + Sync-AnpmFeed.ps1
M1  Anpm.Core · AnpmMcp · Anpm.Host · anpm_mcp_export · CI
M2  AgentForge.Plugin.PackageFeed (sibling agent-forge)
```
