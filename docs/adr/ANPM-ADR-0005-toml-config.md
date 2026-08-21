# ANPM-ADR-0005 — TOML config (MCP / host parity)

**Status:** Accepted  
**Date:** 2026-08-21

## Context

Operator stack already uses TOML SSOT for MCP bridges (`cdp-mcp.toml`, `forge-mcp.toml`) with `--config` and env as escape hatch. ANPM M1 shipped with `ANPM_*` env-only config — inconsistent and noisy in Cursor `mcp.json`.

## Decision

1. **Primary SSOT:** `anpm.toml` with sections `[feed]`, `[host]`, `[mcp]`.
2. **CLI:** `AnpmMcp` and `Anpm.Host` accept `--config|-c PATH` (same pattern as CDP / Forge MCP).
3. **Default path:** `config/anpm.toml` next to executable when file exists; otherwise empty defaults until operator supplies config or env.
4. **Precedence:** tool args → `ANPM_*` env override → TOML → manifest field defaults → built-in defaults.
5. **Forge plugin (M2):** host `appsettings` section `[anpm]` (`feed_root`, `manifest_path`, `v3_base_url`); env override only. Plugin mount TOML does **not** carry ANPM secrets/paths.
6. **MCP export:** `anpm_mcp_export` emits `args: ["--config", "..."]` and empty `env`.

## Consequences

- Operators copy `config/anpm.toml.example` outside git once; Cursor `mcp.json` stays thin.
- Scripts take `-Config` instead of exporting env blocks.
- Breaking: env-only setups still work as override layer, but docs/examples no longer treat env as SSOT.

## References

- `config/anpm.toml.example`
- `Anpm.Core/Config/AnpmConfigLoader.cs`
- CDP `CdpSettings.Load`, Forge `ForgeMcpConfigLoader`
