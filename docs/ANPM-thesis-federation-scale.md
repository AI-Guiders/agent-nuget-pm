# Thesis: federation-scale NuGet vs solo-author gallery

**Status:** Living note (2026-08-30)  
**Relates to:** [ANPM-pain-inventory](ANPM-pain-inventory.md) N-042–N-045

## Observation

AI-era **solocorp / small federation** ships **many** small packages (vertical slices, monorepo SemVer). That is a feature, not a smell.

nuget.org flow assumes:

- one human author,
- few package IDs,
- org as **display name**, not **publisher principal**.

## Structural gaps (not misconfiguration)

| Expected (team/org) | nuget.org reality |
|---------------------|-------------------|
| Org publishes | Only **members** push via personal identity |
| `user: AIGuiders` in CI | OIDC uses **policy creator** username |
| New ID inherits org owners | Owner = **who pushed** |
| Bulk owner fix | UI only ([#9675](https://github.com/NuGet/NuGetGallery/issues/9675), open since 2023) |

## ANPM role

Not replace nuget.org — **operational layer**:

1. **Manifest** desired state (`registry/guiders-federation.toml`)
2. **Drift detect** (`Get-OwnersDrift.ps1`)
3. **Runbooks** for unavoidable UI (owners batch, prefix email)
4. Later: MCP `anpm_registry_owners_drift`, release gate

Federation growth is **O(packages)** on nuget.org; ANPM keeps that **O(1) operator attention** per release via declare + drift, not memory + archaeology.
