# ANPM pain inventory

| | |
|---|---|
| **Status** | Living doc |
| **Date** | 2026-08-29 |
| **Relates to** | [ANPM-ADR-0001](adr/ANPM-ADR-0001-overview.md), [ANPM-ADR-0002](adr/ANPM-ADR-0002-not-baget-nexus-clone.md), [ANPM-ADR-0007](adr/ANPM-ADR-0007-federation-registry-operations.md), [guiders-core nuget-publishing](https://github.com/AI-Guiders/guiders-core/blob/main/docs/nuget-publishing.md) |
| **North star** | Friction insight → artifact; не «ещё один BaGet», а **registry mechanics** для federation (air-gap L2 + nuget.org L3 + будущий command layer) |

По образцу [FORGE-pain-inventory](https://github.com/AI-Guiders/agent-forge/blob/main/design/FORGE-pain-inventory.md): собираем шишки **в моменте** (чат, migration, CI), пока боль живая — потом они становятся ADR, скрипты, MCP tools.

## Как пользоваться

Одна строка = одна боль. Колонки:

| Колонка | Смысл |
|---------|--------|
| **ID** | `N-xxx` для ссылок из ADR/issue |
| **Боль** | Симптом своими словами |
| **Кто** | human / agent / ops / ci |
| **Откуда** | pilot, migration 2026-08, nuget.org UI, CI log |
| **ANPM-ответ** | принцип + конкретика или «upstream only» |
| **Статус** | open / in-progress / resolved / wont-fix / upstream |

**Приоритет:** боль повторяется на каждом релизе, бьёт по federation scale (десятки пакетов), или блокирует agent-driven registry ops → L3 wave; остальное — backlog.

**Фильтр фич** (из ADR-0002 / 0007):

1. **Declare, don’t click** — desired state в manifest; drift detect, не 46× UI.
2. **Paired invariants** — nupkg+snupkg, owners+TP, version+commit — policy, не tribal CI knowledge.
3. **L3 ≠ clone nuget.org** — control plane и air-gap feed; публичный gallery — отдельное решение (L4).
4. **MCP + slash co-primary** — registry ops без «открой nuget.org, вкладка Owners».

---

## Сводка по категориям

| Категория | Типичная жалоба | Лидеры боли |
|-----------|-----------------|-------------|
| **Publish** | symbols failed, PDB mismatch | monorepo CI + `--skip-duplicate` |
| **Ownership** | нет API; Add owner × N пакетов | nuget.org UI |
| **Org migration** | TP policy, prefix, co-owner invite | nuget.org account model |
| **Discoverability** | zero-config только nuget.org | .NET SDK defaults |
| **UI quirks** | `" AIGuiders"` → Owner not found | nuget.org forms |

---

## Pain inventory

### Publish & symbols

| ID | Боль | Кто | Откуда | ANPM-ответ | Статус |
|----|------|-----|--------|------------|--------|
| N-001 | `dotnet pack` all + push nupkg `--skip-duplicate` + unconditional snupkg → **symbol validation failed** (PDB ≠ published DLL) | ci | guiders-core 0.4.22, [NuGet/Home#10475](https://github.com/NuGet/Home/issues/10475) | **Paired push:** snupkg только если nupkg загружен в этом прогоне; `scripts/nuget/push-artifacts.sh`; `Deterministic` + `ContinuousIntegrationBuild` | **resolved** (federation CI) |
| N-002 | Repair failed symbols для уже опубликованной версии — неочевидный recipe (какой commit, push только snupkg) | human+agent | migration chat 2026-08 | Док: [nuget-publishing.md § Symbols](https://github.com/AI-Guiders/guiders-core/blob/main/docs/nuget-publishing.md); L3 `anpm_registry_status` symbol health | in-progress |
| N-003 | Версия в `<Version>` csproj, тег — только триггер; легко забыть bump нужных пакетов в monorepo | human | guiders-core/platform release | L3 release plan: diff bumped PackageIds vs manifest | open |

### Ownership & org migration

| ID | Боль | Кто | Откуда | ANPM-ответ | Статус |
|----|------|-----|--------|------------|--------|
| N-010 | **Нет public API** add/remove package owners — только UI или email support | human+agent | org migration 2026-08 | L3 `anpm_registry_owners_drift`; `anpm_support_bulk_owners` (escape); **не** «support кликает вместо нас» как SSOT | in-progress |
| N-011 | ~46 пакетов × Manage package → Owners → Add → accept invite | human | federation inventory | Manifest `owners = ["AIGuiders", …]`; drift report; `Add-OrgPackageOwners.ps1` (audit + open tabs) | in-progress |
| N-012 | Поле **Add owner** / **Package owner** **не триммит пробелы**: `AIGuiders` OK, ` AIGuiders` → Owner not found | human | nuget.org UI 2026-08-29 | Док + operator warning; upstream issue (если заведём) | **upstream** |
| N-013 | Prefix reservation `AIGuiders.*` — ручной email `account@nuget.org` | ops | nuget.org policy | L3d checklist в manifest; support ticket template | open |

### Trusted Publishing & auth

| ID | Боль | Кто | Откуда | ANPM-ответ | Статус |
|----|------|-----|--------|------------|--------|
| N-020 | TP policy: **Package owner** = org, но workflow `user: LonelySoul` до завершения migration | ci | release.yml всех repos | Manifest `trusted_publish.user`; L3d validate workflow vs manifest | in-progress |
| N-021 | TP создаётся из **personal menu**, не org menu — неочевидно | human | operator notes 2026-08 | Runbook в nuget-publishing + этот log | in-progress |
| N-022 | Orphaned TP policies на старых `*-core` repos после monorepo merge | ops | [nuget-tp-migration-checklist](https://github.com/AI-Guiders/guiders-core/blob/main/docs/nuget-tp-migration-checklist.md) | Manual cleanup; L3d policy inventory | open |

### Discoverability & ecosystem

| ID | Боль | Кто | Откуда | ANPM-ответ | Статус |
|----|------|-----|--------|------------|--------|
| N-030 | **Нет альтернативы** nuget.org для public zero-config `dotnet add package` | all | ecosystem | Non-goal: replace nuget.org globally ([ADR-0002](adr/ANPM-ADR-0002-not-baget-nexus-clone.md)); optional L4 scoped registry + mirror | open (positioning) |
| N-031 | Private feed (ANPM L2) ≠ public discoverability — разные продукты, один бренд путает | agent | ADR-0001 vs 0007 | Явные слои L2/L3/L4 в docs; не смешивать air-gap и federation public | in-progress |

### Command layer (желаемое)

| ID | Боль | Кто | Откуда | ANPM-ответ | Статус |
|----|------|-----|--------|------------|--------|
| N-040 | Хочется `/package AIGuiders.* owner add …`, `/organisation members add` вместо UI | human+agent | migration chat 2026-08 | L3 MCP + Platform Slash catalog; **mutate** только если registry наш (L4); иначе plan + drift | open |
| N-041 | nuget.org = portal-first 2010-х; agent-first registry ops не в их roadmap | agent | сравнение с Forge pain model | ANPM L3 control plane; pain log → ADR waves | in-progress |

---

## Волны (из боли → артефакт)

| Wave | Боли | Артефакт |
|------|------|----------|
| **L3a** | N-010, N-011, N-012 | `registry.toml`, `anpm_registry_status`, owners drift |
| **L3b** | N-001, N-002, N-003 | publish orchestrator, CI adoption (done: push-artifacts.sh) |
| **L3c** | N-011, N-040 | `Anpm.View` Registry slice |
| **L3d** | N-013, N-020, N-022 | TP validation, prefix checklist |
| **L4?** | N-030, N-040 | Scoped public registry + slash mutate |

---

## Ссылки

- [NuGet/Home#10475 — snupkg skip-duplicate](https://github.com/NuGet/Home/issues/10475)
- [Trusted Publishing](https://learn.microsoft.com/nuget/nuget-org/trusted-publishers)
- [FORGE pain inventory](https://github.com/AI-Guiders/agent-forge/blob/main/design/FORGE-pain-inventory.md) — формат-образец

---

## Твой вклад

Добавляй строки в таблицу (или в чат → агент переносит сюда):

```markdown
| N-xxx | «цитата» | human | личный опыт / CI log | предложение | open |
```

Периодически: `resolved` → ссылка на commit/ADR; top pains → следующая L3 wave.
