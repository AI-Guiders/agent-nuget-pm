# ANPM-ADR-0007: Federation registry operations (L3)

| | |
|---|---|
| **Status** | Proposed |
| **Date** | 2026-08-29 |
| **Relates to** | ANPM-ADR-0001 · ANPM-ADR-0004 · GUIDERS-ADR-0003 · GUIDERS-CORE-0002 |

## Context

AI Guiders ships **dozens** of NuGet packages across monorepos (`guiders-core`, `guiders-platform`, …). nuget.org is the public registry, but **operations are human-first**:

| Pain (2026-08) | nuget.org reality |
|----------------|-------------------|
| Org migration | No owner API — UI per package or email support (clicks move, problem stays) |
| Monorepo release | `dotnet pack` all → `--skip-duplicate` on nupkg → snupkg still uploads → symbol validation fails ([NuGet/Home#10475](https://github.com/NuGet/Home/issues/10475)) |
| Trusted Publishing | Policy on user menu; Package owner = org; workflow `user:` must match |
| Prefix reservation | Manual email to account@nuget.org |
| Desired state | No declarative “these 46 packages are owned by org X with symbols on” |

**ANPM today (L2):** offline feed sync, pin manifest, restore verify ([ADR-0001](ANPM-ADR-0001-overview.md)). Publish explicitly **out of scope** ([ADR-0004](ANPM-ADR-0004-mcp-tool-surface.md) non-goals).

**`Cdp.PackageIntelligence` (L1):** live read — audit, upgrade plans, vulnerability ([GUIDERS-CORE-0002](https://github.com/AI-Guiders/guiders-core/blob/main/docs/adr/GUIDERS-CORE-0002-package-intelligence.md)). No mutate, no registry orchestration.

Passkeys / OIDC / Trusted Publishing improve **auth**; they do not fix **registry ergonomics** or monorepo publish invariants.

## Decision

Add **ANPM L3 — Federation registry operations**: agent-operable **control plane** over NuGet lifecycle for Guiders (and optional private upstream), without cloning nuget.org.

### Layering (canonical)

```text
L3  ANPM registry ops     ← this ADR (mutate intent, publish orchestration, drift)
L2  ANPM offline feed      ← ADR-0001 (air-gap sync, v3 index)
L1  PackageIntelligence    ← guiders-core (read-only nuget.org / config)
L0  nuget.org / Forge UNC  ← upstream registries (not owned by ANPM)
```

### Principle: declare desired state, don’t click 46 times

ANPM holds a **registry manifest** (TOML/JSON, versioned in repo or operator path):

```toml
[registry]
upstream = "https://api.nuget.org/v3/index.json"
owner = "AIGuiders"                    # NuGet org username
prefix = "AIGuiders.*"
trusted_publish = { repo = "AI-Guiders/guiders-core", workflow = "release.yml" }

[[package]]
id = "AIGuiders.Cdp.Core"
version_policy = "csproj"              # or explicit pin
symbols = "snupkg"
owners = ["AIGuiders", "LonelySoul"]   # desired; drift detected vs live
```

Agents and humans drive **one manifest** → ANPM produces plans, CI patches, and operator runbooks — not 46 unrelated UI sessions.

### What L3 owns (in scope)

| Capability | Mechanism |
|------------|-----------|
| **Publish orchestration** | Pair nupkg+snupkg; push snupkg only when nupkg newly published; deterministic CI flags |
| **Release plan** | From monorepo: which PackageIds bumped, which artifacts to push, skip list |
| **Ownership drift** | Compare manifest vs NuGet search API / owners index; report missing org co-owner |
| **Symbol health** | Detect failed symbol validation on package pages; suggest rebuild/push recipe |
| **Support artifacts** | Generate bulk-request email (account@nuget.org) from manifest — optional escape, not SSOT |
| **Pin ↔ publish link** | Tie `Directory.Packages.props` / csproj versions to feed sync and registry manifest |

### What L3 does not own (non-goals)

- Replacing nuget.org or BaGet/Nexus ([ADR-0002](ANPM-ADR-0002-not-baget-nexus-clone.md))
- Undocumented scraping as primary owner-mutate API (fragile; use only with explicit operator opt-in)
- npm/OCI (separate hyperlanes later; same manifest *pattern* may reuse)
- Product package **content** — planets ship code; ANPM ships **registry mechanics**

### MCP tools (proposed wave — L3a)

| Wire name | Purpose |
|-----------|---------|
| `anpm_registry_status` | Manifest vs live: owners, latest version, symbol errors |
| `anpm_registry_plan` | Diff: what would publish / migrate on next tag |
| `anpm_registry_publish` | Orchestrated push (nupkg then snupkg rules); OIDC or API key from env |
| `anpm_registry_owners_drift` | Packages where desired owners ⊄ actual owners |
| `anpm_support_bulk_owners` | Render support ticket body from drift (escape hatch) |

Wire names follow [ADR-0004](ANPM-ADR-0004-mcp-tool-surface.md) underscore convention.

### Human View

`Anpm.View` gains **Registry** slice: fleet table (package × owner × symbol status × drift), not per-package nuget.org archaeology. Co-primary with MCP ([ADR-0006](ANPM-ADR-0006-dual-delivery-and-human-view.md)).

### Integration with existing repos

| Repo | Role |
|------|------|
| `guiders-core` / `guiders-platform` | Ship csproj versions; CI may delegate push to ANPM or adopt ANPM publish rules |
| `guiders-platform` conformance | Vectors unchanged — ANPM does not own package *semantics* |
| Forge plugin | Optional mount: registry status for operators on forge host |

## Consequences

**Pros**

- One place for “how we publish federation packages” — story for agents and humans
- Monorepo snupkg class of bugs prevented by policy, not tribal CI knowledge
- Air-gap (L2) and public registry (L3) share pin/registry vocabulary

**Cons**

- nuget.org gaps remain; L3 works *around* them, cannot fully eliminate without upstream API
- Another manifest to maintain (mitigated: generated from csproj inventory)

## Migration / waves

| Wave | Deliverable |
|------|-------------|
| **L3a** | `registry.toml` schema + `anpm_registry_status` + owners drift report |
| **L3b** | Publish orchestrator (snupkg pairing); guiders-core/platform CI adoption |
| **L3c** | View Registry slice; support ticket generator |
| **L3d** | Prefix reservation checklist in manifest; TP policy validation |

## Open questions

- Single `registry.toml` in `agent-nuget-pm` vs per-monorepo fragment merged by ANPM?
- Trusted Publishing: validate `user:` in workflow against manifest in CI?

## Links

- [NuGet snupkg skip-duplicate issue](https://github.com/NuGet/Home/issues/10475)
- [guiders-core nuget-publishing.md](https://github.com/AI-Guiders/guiders-core/blob/main/docs/nuget-publishing.md)
- [Federation Constitution — hyperlanes](https://github.com/AI-Guiders/guiders-platform/blob/main/docs/GUIDERS-FEDERATION-CONSTITUTION.md)
