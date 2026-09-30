---
status: accepted
date: 2026-10-01
workstream: database (tbl_Rule_Ayanamsa), cli (generation), ui (Saved Charts preferences)
supersedes: the per-generation ayanāṁśa choice (NFR-UI-03)
---

# 004 — The ayanāṁśa is Traditional Lahiri, locked

## Decision

Every chart is calculated with **Traditional Lahiri** (`AYANAMSA_LAHIRI`, Swiss sidereal mode 1).
It is not a user preference and not a per-call parameter.

## Why

- **It matches the reference.** For the Jagannatha Hora benchmark chart
  (`BENCH_RAMAKRISHNAN_P_JHORA_1981`, 22 Apr 1981, Chennai) JHora gives 23.59495°; the stored
  Traditional Lahiri value is 23.59591° — 3.4″ apart, far below anything that moves a sign,
  nakshatra pada or even a D60 boundary except at a knife edge.
- **A switchable ayanāṁśa invalidates stored data.** Every `tbl_ChartResults` row, every fact
  table and `tbl_Fact_HouseStrengthStatistics` are computed once at generation. Changing the
  ayanāṁśa would silently disagree with all of them until a full regeneration — the problem
  class the 2026-09-13 hardcode decision set out to remove before the population is imported.
- **Statistics need one frame.** Cross-person statistics (Life Matters) are only comparable when
  every chart shares the same zodiac.

## How it is enforced

- `tbl_Rule_Ayanamsa` keeps the catalogue for provenance (`SRC_PYJHORA`); its one `IsDefault = 1`
  row (unique index `UX_Rule_Ayanamsa_Default`) is `AYANAMSA_LAHIRI`.
- `AyanamsaRuleRepository.GetActiveDefault` **refuses** any other default: it throws unless the
  row's code equals `AyanamsaDefinition.Default` (Traditional Lahiri). DB and code must agree.
- `ChartGenerationService.GenerateAll` / `GenerateMissing` / `RecomputeAnalytics` take no
  ayanāṁśa parameter; they always resolve the locked default.
- Saved Charts → Preferences shows *Ayanāṁśa: Traditional Lahiri — fixed for every chart*; the
  22-system picker is gone.

## Changing it later

Only as a deliberate project decision: supersede this record, repoint `tbl_Rule_Ayanamsa`'s
default in a migration, change `AyanamsaDefinition.Default` in the same commit, then run CLI
`rebuild-all` to regenerate every person. The benchmark comparison runner (`verify-ayanamsa`,
not yet built — `docs/database/action-required-audit.md`) is the tool for that analysis.
