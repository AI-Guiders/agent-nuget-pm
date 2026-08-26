# ANPM-ADR-0006: Dual delivery, Human View, and UI stack

**Date:** 2026-08-26  
**Status:** Accepted  
**Supersedes wording in:** [ANPM-ADR-0001](ANPM-ADR-0001-overview.md) («human UI secondary»), [ANPM-ADR-0003](ANPM-ADR-0003-forge-plugin-boundary.md) (plugin-only human surface)  
**Relates to:** [FORGE-ADR-0048](https://github.com/AI-Guiders/agent-forge/blob/main/design/FORGE-ADR-0048-human-primary-surface-and-command-palette.md), [FORGE-ADR-0049](https://github.com/AI-Guiders/agent-forge/blob/main/design/FORGE-ADR-0049-human-view-component-kit-and-deploy-profiles.md), [ANPM-ADR-0005](ANPM-ADR-0005-toml-config.md)

## Context

ANPM M0–M2 shipped **Core + MCP + v3 host + Forge zoo plugin** (JSON API + DOI). Human-visible work today is API/JSON only — not a product surface.

Operators need **two equal delivery channels**:

1. **Standalone** — NuGet PM without Forge (air-gap sync host, dedicated feed appliance).
2. **Forge mount** — same product inside Agent Forge when Forge is already deployed.

Both channels must be **human-primary** and **agent-primary** (MCP), aligned with Forge’s dual-ingress model ([FORGE-ADR-0015 §8](https://github.com/AI-Guiders/agent-forge/blob/main/design/FORGE-ADR-0015-vertical-domain-plugins-and-doi-commands.md)): MCP-first for agents ≠ MCP-only; humans get browse/mutate without installing Cursor.

Question: build UI on **Human Kit** (`AIGuiders.UI.*`) or adopt **off-the-shelf** Blazor/Razor/JS component libraries?

## Decision

### 1. Dual delivery (equal, not primary/secondary)

| Channel | Host | Ships |
|---------|------|-------|
| **Standalone** | `Anpm.Host` (Kestrel) | v3 feed + **`Anpm.View`** (`/view/*`) + health |
| **Forge** | `AgentForge.Plugin.PackageFeed` | Forge chrome + auth + **`Anpm.View`** (same pages) |
| **Agent** | `AnpmMcp` (either channel) | stdio MCP; no browser required |

```text
Anpm.Core                    ← SSOT: manifest, sync, index, verify
    │
Anpm.View                    ← SSOT: human pages (Razor + platform kit)
    │
    ├── Anpm.Host            ← standalone product host
    ├── AnpmMcp              ← agent ingress
    └── Plugin.PackageFeed   ← Forge adapter (mount View + API + DOI)
```

**Invariant:** `Anpm.View` is owned by **agent-nuget-pm**, not by Forge. The Forge plugin is a **host adapter**, not the UI owner.

**Not in scope:** a second standalone SPA (React admin template), BaGet/Nexus UI fork, or duplicate page markup in the plugin.

### 2. Human View scope (M3 DoD)

Minimum screens (both delivery channels):

| Screen | Surface |
|--------|---------|
| **Feed overview** | manifest path, feed root, v3 URL, last index rebuild |
| **Pin matrix** | `ForgeCatalogTable` dialect — package id, pinned version, present/missing, actions |
| **Sync** | dry-run + execute; flash/result banner; errors listed |
| **Restore verify** | target path input (standalone) or linked from contour docs |

Slash / palette (Forge): `/package feed status`, `/package feed sync` — already registered (M2); bindings must target **View routes**, not raw JSON, on human-primary hosts.

Journey tests: standalone `WebApplicationFactory` on `Anpm.Host`; Forge plugin reuses shared view route tests where possible.

### 3. UI stack — platform Human Kit, not Forge plugin internals

**Use:** `AIGuiders.UI.*` NuGet packages (platform kit extracted for Guiders products), specifically:

- `AIGuiders.UI.Core` — layout primitives, breadcrumbs, empty states
- `AIGuiders.UI.Web.HTMX` — SSR MPA + bounded islands (sync progress, table refresh)

**Stack shape (same as Forge Human View, without coupling to `AgentForge.Plugin.View`):**

| Layer | Choice |
|-------|--------|
| Rendering | **Razor** `.cshtml` + view models (`Microsoft.NET.Sdk.Razor`) |
| Navigation | MPA — full page POST + redirect |
| Interactivity | **HTMX islands** only where MPA is worse (sync job status) |
| CSS | Platform design tokens / shared human CSS — ANPM-branded shell |
| Primitives | Catalog table, status badge, flash banner, page shell |

**Do not reference** `AgentForge.Plugin.View` or `ForgeHuman*` from `Anpm.View` — those are Forge-coupled. Forge plugin supplies **chrome injection** (header, org context, settings nav contributor) via adapter interfaces defined in `Anpm.View.Abstractions` or thin `Plugin.PackageFeed.View` shim.

### 4. Why not «a ton of ready-made libs»?

| Option | Verdict | Reason |
|--------|---------|--------|
| **MudBlazor / Radzen / Syncfusion Blazor** | ❌ default | Implies **Blazor component model**; Forge explicitly rejected Blazor Server/WASM as Human View stack ([FORGE-ADR-0049 §5](https://github.com/AI-Guiders/agent-forge/blob/main/design/FORGE-ADR-0049-human-view-component-kit-and-deploy-profiles.md)). Different render pipeline, heavier runtime, forked from Razor+HTMX spine. |
| **React / Vue admin templates** | ❌ | SPA shell, npm build graph, CDN habits — poor **air-gap** story; second UX dialect vs Forge; duplicates MCP/DOI semantics in client-only code. |
| **Bootstrap / Tailwind via CDN** | ❌ default | Offline contours block CDN; vendored Tailwind possible but does not replace **table/shell primitives** — still need page composition SSOT. |
| **BaGet / NuGet gallery UI** | ❌ | Wrong domain model (registry browse/push), not manifest-driven pin/sync. |
| **HTMX + vanilla** | ✅ as island layer | Already platform choice; use via `AIGuiders.UI.Web.HTMX`. |
| **Markdig** | ✅ | Help/docs markdown pages only. |
| **Vendored CSS/JS widget** | ⚠️ per-row ADR | Allowed only if: SSR fallback without JS, no SPA router, MCP/API parity unchanged. |

**Nothing technically prevents** importing a library — the constraint is **product architecture**:

1. **Air-gap** — UI assets ship in the host binary/publish folder, no runtime CDN.
2. **Dual delivery** — one `Anpm.View` assembly must mount on **both** `Anpm.Host` and Forge without two front-end builds.
3. **Semantic parity** — View POST, DOI, and MCP call the same `Anpm.Core` services; fat client libs tempt duplicate logic.
4. **Visual lineage** — standalone ANPM and Forge-mounted ANPM should feel like the same product family (platform kit), not «Bootstrap admin #7».

Platform kit is the **curated subset** of the Razor/HTMX ecosystem we already standardized — not a ban on libraries, but a ban on **parallel UI stacks**.

### 5. Dependency rules (`Anpm.View.csproj`)

```
Anpm.View
  → Anpm.Core
  → AIGuiders.UI.Core
  → AIGuiders.UI.Web.HTMX
  → (optional) Markdig

Anpm.Host
  → Anpm.Core, Anpm.View, AnpmMcp (publish only)

AgentForge.Plugin.PackageFeed
  → Anpm.Core, Anpm.View
  → AgentForge.Abstractions (Forge mount only)
  → NOT a second copy of human pages
```

### 6. Configuration (unchanged)

[TOML SSOT](ANPM-ADR-0005-toml-config.md) applies to both channels: `anpm.toml` for standalone; Forge adds `appsettings [anpm]` override on plugin mount.

## Implementation waves

| Wave | Deliverable |
|------|-------------|
| **M3a** | `Anpm.View` project — feed overview + pin matrix (Razor + platform kit) |
| **M3b** | `Anpm.Host` maps `/view/*`; standalone journey tests |
| **M3c** | `Plugin.PackageFeed` — Forge chrome adapter; DOI bindings → View routes |
| **M4** | WitDB index (Core); both channels |
| **M5** | HTMX sync island (optional); restore-verify screen |

## Consequences

- **Pros:** One human surface, two equal deploy paths; offline-friendly; Forge parity without Forge lock-in.
- **Cons:** Must publish/maintain `AIGuiders.UI.*` as a real platform dependency; ANPM cannot freeload random npm templates without ADR row.
- **ADR-0001** human UI is **co-primary** with MCP (audience-split), not secondary.
- **ADR-0003** plugin remains optional for **hosting**, not optional for **UI ownership**.

## Alternatives considered

| Alternative | Rejected because |
|-------------|------------------|
| Forge-only UI | Standalone contour needs browser without Forge |
| Standalone headless only | Violates human-primary requirement |
| MudBlazor standalone app | Blazor stack divergence + weight |
| Embed BaGet UI in iframe | Wrong UX model, stale upstream |

## Links

- [ANPM-ADR-0001 Overview](ANPM-ADR-0001-overview.md)
- [ANPM-ADR-0003 Forge plugin boundary](ANPM-ADR-0003-forge-plugin-boundary.md)
- [M2 Forge plugin](../M2-FORGE-PLUGIN.md)
- Forge: [FORGE-ADR-0049 Human View kit](https://github.com/AI-Guiders/agent-forge/blob/main/design/FORGE-ADR-0049-human-view-component-kit-and-deploy-profiles.md)
