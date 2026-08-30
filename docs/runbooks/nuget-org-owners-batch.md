# Runbook: batch Add owner `AIGuiders` (nuget.org UI)

No public API ([#9675](https://github.com/NuGet/NuGetGallery/issues/9675)). ANPM detects drift; humans fix in UI.

## Drift check

```powershell
./scripts/Get-OwnersDrift.ps1
# optional: open Manage → Owners tabs
./scripts/Get-OwnersDrift.ps1 -OpenManageUrls
```

Exit code `1` = drift remains.

## Per package (same every time)

1. Open `https://www.nuget.org/packages/{PackageId}/manage/owners`
2. **Add owner** → `AIGuiders` (no leading/trailing spaces — [N-012](../ANPM-pain-inventory.md))
3. Accept invite as org admin if prompted

## Current drift set (2026-08-30)

Platform slice — owner `LonelySoul` only:

| PackageId |
|-----------|
| AIGuiders.Platform.InputNotation.All |
| AIGuiders.Platform.InputNotation.Emacs |
| AIGuiders.Platform.InputNotation.Neovim |
| AIGuiders.Platform.InputNotation.Quarry |
| AIGuiders.Platform.InputNotation.Vim |
| AIGuiders.Platform.MCPlane |
| AIGuiders.Platform.Notations |
| AIGuiders.Platform.Notations.Argument.Kv |
| AIGuiders.Platform.Notations.Command.Console |
| AIGuiders.Platform.Notations.Command.Slash |
| AIGuiders.Platform.Utilities.Adoption |
| AIGuiders.Platform.Utilities.Adoption.Reports.Markdown |
| AIGuiders.Platform.Utilities.Adoption.Sources |

Re-run drift after batch. Before [prefix reservation](prefix-reservation-email.md): prefer zero drift on representative packages.

## After new PackageId ships

First push assigns **pusher** as owner ([N-043](../ANPM-pain-inventory.md)). CI does not need org in owners; add `AIGuiders` when:

- prefix / team access matters, or
- drift gate before release milestone.
