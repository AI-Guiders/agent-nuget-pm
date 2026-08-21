# M1 scaffold plan

**Status:** in progress (2026-08-21)

## Solution layout

```text
agent-nuget-pm/
  Anpm.Core/          # feed scan, manifest, v3 index, sync, restore verify
  AnpmMcp/            # MCP stdio + --invoke CLI
  AnpmMcp.Tests/
  manifest/           # pins.example.json (schema sample only)
  scripts/            # Sync-AnpmFeed.ps1
  docs/adr/           # ANPM-ADR-0004 MCP surface
```

## M1 deliverables

| Item | M1 scaffold | M1 complete |
|------|-------------|-------------|
| Flat feed + manifest contract | done | done |
| Static v3 index (`.anpm/v3`) | done | tune for HTTP host |
| MCP tools | 5 tools | + publish guardrails |
| HTTP feed host | — | Kestrel minimal v3 |
| WitDB index | — | optional M1+ |

## Consumer wiring (out of repo)

Deployment manifests, LUS, UNC paths, and consumer `nuget.config` belong to **operator / private CAD contour** — not this GitHub repo. ANPM exposes contract + tools; operators point env at their feed and manifest.

## Next leaves

- [ ] Kestrel host project `Anpm.Host` serving `.anpm/v3` + package bytes
- [ ] Export MCP manifest tool (git-mcp parity)
- [ ] CI workflow build + test
