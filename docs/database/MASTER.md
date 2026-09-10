---
last_updated: 2026-09-10
workstream: database
togaf: C — Data Architecture
safe: Solution Intent (fixed) — data model
---

# Database workstream — MASTER

**Branch** `workstream/database` · **worktree** `D:\@ClaudeSpace\ikiastrro.wt\database` ·
**owns** `db/`, `src/Ikiastrro.Data/`.

The versioned SQL Server schema, its Dapper read/write layer, and the rules-engine tables.
Publishes to the CLI and UI streams under
[`../architecture/domain-contracts.md`](../architecture/domain-contracts.md).

## Docs

| Doc | For |
|---|---|
| [`schema.md`](schema.md) | Table inventory, chart-generic analytics, reference/master data, views & functions, migration policy |
| [`rules-engine.md`](rules-engine.md) | `tbl_Dim_*` / `tbl_Rule_*` / `tbl_Fact_*` model — every rule table, its columns, its source rule |
| [`db_view_catalog.md`](db_view_catalog.md) | UI table component ⇄ backing view / TVF binding — consumer-side index (definitions stay in `schema.md`); see [`../../project_standards.md`](../../project_standards.md) § 4 |

## Current state

- **Baseline** `db/ikiastrro.sql` + numbered migrations `db/NN_*.sql` applied in order,
  tracked in `dbo.SchemaMigrations` (keyed by `ScriptName`). Migrations `22`–`076` are the
  active layer; earlier flat history is frozen under `db/_archive/`. `055`–`076` are applied
  to dev but **not yet folded forward** into `db/ikiastrro.sql` — do that once proven on the
  other environments.
- ~95 tables: input, chart results, chart-generic analytics, dasha, reference/master,
  `tbl_Rule_*` (versioned), `tbl_Dim_*`, `tbl_Fact_*` (star-schema), plus the isolated
  `research.*` reference corpus (migrations 056–070).
- **Live rule table:** `tbl_Rule_VargaScheme` (the orchestrator builds one `VargaCalculator`
  per row). All other `tbl_Rule_*` are a verified mirror of the hard-coded C# — Phase 2
  (calculators reading them) not started.
- `tbl_Rule_Ayanamsa` — JHora ayanāṁśa catalogue + system default (`Lahiri`, Swiss mode 1;
  set by migration 054).

## In flight

- **`FEAT-DATA-04`** — ayanāṁśa default fixed (migration 054 repoints `tbl_Rule_Ayanamsa`
  from Jagannatha mode 26 to Lahiri mode 1; `verify-vargas` / `verify-jaimini` green).
  Still open: the reference benchmark harness — `tbl_Dim_AyanamsaBenchmarkCases` is empty
  while `tbl_Dim_AyanamsaBenchmarkPositions` (10) + `tbl_Dim_DashaBenchmarkPeriods` (9) are
  orphaned on `CaseId = 1`; re-seed `BENCH_RAMAKRISHNAN_P_JHORA_1981`
  (`ReferenceAyanamsaDegrees` 23.595, `SRC_JHORA_EXPORT_RAMAKRISHNAN`).
- **`FEAT-DATA-05`** — source-attributed yoga corpus schema (migrations 47–49): applied
  locally; roll to other environments after the corpus completes.
- **`FEAT-STRENGTH-01` (DB slice)** — migrations 071–073, 076: `vw_ChartShadbala` regression
  (069) fixed; `tbl_Rule_ShadbalaMinimumRupas` seeded (reproduces JHora %Strength within
  rounding); `tbl_Rule_PlanetaryWar` + the six deferred Kālabala `RuleParametersJson`
  seeded. `tbl_Dim_ShadbalaBenchmarkValues` holds the seven-planet JHora golden totals.
  Calculators that read these are a `cli` follow-up.
- **`FEAT-ASHTAKAVARGA-01` (DB slice)** — migrations 074–076: production `dbo` schema
  (`tbl_Rule_AshtakavargaContribution` — 56-row Parāśari matrix, SAV total 337;
  `tbl_Rule_AshtakavargaReduction`; `tbl_Fact_BhinnaAshtakavarga` / `…Contribution`,
  `tbl_Fact_SarvaAshtakavarga`, `tbl_Fact_AshtakavargaPinda`; `vw_ChartAshtakavarga`).
  Matrix hand-verified against the JHora export (Saturn BAV row reproduced exactly). JHora
  BAV grid + Piṇḍa seeded into the `research.*` benchmark. `AshtakavargaCalculator` pending.
- **Known pre-existing break:** `verify-sources` crashes on `Invalid object name
  'dbo.tbl_Dim_SourceReferencePlanetText'` — its `SourceRefCode` tripwire loop hard-codes
  `dbo.` but migrations 056/067/070 put those tables in `research.*`. A `cli` one-liner
  (schema-filter the loop). Not introduced by 071–076.

## Planned

- Rules-engine **Phase 2** — calculators read `tbl_Rule_*` instead of hard-coded C#.
- Wire the reference dimension tables (`tbl_Planets`, `tbl_SignAttributes`, `tbl_Nakshatras*`)
  into the engine (currently seeded + cross-checked, not read).
- `tbl_Dim_HouseSignification` / `tbl_Dim_PlanetSignification` / `tbl_Dim_PlanetHouseKaraka`
  (migration 030) — reserved, unapplied; `LifeAreaMap` hardcodes their data today.
- Vimśopaka Bala / Vaiśeṣikāṁśa (`FEAT-STRENGTH-02`) schema — four varga-group weights still
  need a cited source before the shape is settled.
