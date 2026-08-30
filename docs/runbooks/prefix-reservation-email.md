# Runbook: ID prefix reservation `AIGuiders.*`

**To:** account@nuget.org  
**From:** email tied to nuget.org account **LonelySoul** (org admin)

**Subject:** ID prefix reservation request — AIGuiders

```
Owner display name: AIGuiders
Requested prefix: AIGuiders.*

Authorized to publish packages matching this prefix:
  All current and future members of the AIGuiders organization
  on nuget.org (administrators and collaborators), as managed
  by org admins.

Current org administrators: LonelySoul

GitHub organization: AI-Guiders (https://github.com/AI-Guiders)
Existing packages: ~70 public packages with prefix AIGuiders.*
Publishing: GitHub Actions Trusted Publishing (OIDC)

Representative packages:
https://www.nuget.org/packages/AIGuiders.Cdp.Core
https://www.nuget.org/packages/AIGuiders.Platform.Abstractions
https://www.nuget.org/packages/AIGuiders.AgentNotes.Core
https://www.nuget.org/packages/AIGuiders.McpToolManifest
https://www.nuget.org/packages/AIGuiders.PluginHost.Runtime
```

## Before send

- Run [owners drift](nuget-org-owners-batch.md) — minimize LonelySoul-only packages.
- Do **not** cite packages that still lack `AIGuiders` co-owner.

## Verification (when NuGet replies)

Unlisted package with GUID in a `.txt` file; push as **LonelySoul**; package owners should include **AIGuiders**.

## Does not change

- `user: LonelySoul` in release workflows (policy creator — [N-045](../ANPM-pain-inventory.md))
- Manual Add owner for new PackageIds ([N-043](../ANPM-pain-inventory.md))

Prefix blocks squatters; it does not remove your publish rights if you are listed as authorized publisher.
