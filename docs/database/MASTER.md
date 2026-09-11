---
last_updated: 2026-09-11
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
  tracked in `dbo.SchemaMigrations` (keyed by `ScriptName`). Migrations `22`–`079` are the
  active layer; earlier flat history is frozen under `db/_archive/`. `055`–`079` are applied
  to dev but **not yet folded forward** into `db/ikiastrro.sql` — do that once proven on the
  other environments. (`077`–`078` are reserved on `workstream/cli` for the Ashtakavarga
  engine, not yet merged — `079` was picked to avoid the collision.)
- ~95 tables: input, chart results, chart-generic analytics, dasha, reference/master,
  `tbl_Rule_*` (versioned), `tbl_Dim_*`, `tbl_Fact_*` (star-schema), plus the isolated
  `research.*` reference corpus (migrations 056–070).
- **Live rule tables** (~6 — calculators actually read these, not just seeded/mirrored):
  `tbl_Rule_VargaScheme` (orchestrator builds one `VargaCalculator` per row),
  `tbl_Rule_AgeState` (`AgeStateCalculator`), `tbl_Rule_WakefulnessState`
  (`WakefulnessStateCalculator`), `tbl_Rule_SubPlanetSunLongitude`/`SubPlanetTime`/
  `SubPlanetPartRuler` (`SubPlanetCalculator`), `tbl_Rule_Ayanamsa` (`AyanamsaDefinition`;
  system default `Lahiri`, Swiss mode 1, set by migration 054). Everything else `tbl_Rule_*` is
  either a CLI-verified mirror of hard-coded C# (Phase 2 wiring not started) or orphaned
  (seeded or unseeded, zero reads) — full per-table breakdown in
  [`rules-engine.md`](rules-engine.md), wiring backlog in
  [`../../decisions/003-rules-audit-content-model-ephemeris-interpreter.md`](../../decisions/003-rules-audit-content-model-ephemeris-interpreter.md).
  `tbl_Rule_Exaltation` doesn't exist yet — a normal Phase-1 job, exaltation degrees are already
  hardcoded in `RamanYogaBatchFiveEvaluator.DeepExaltation`/`RamanDhanaYogaEvaluator`, ready to
  transcribe, same pattern as `db/079`.

## In flight

- **`FEAT-AVASTHA-05` — Sayanaadi (PostureState, DB slice)** — migration 083: 12-state
  `tbl_Dim_PlanetaryState` seed (`AvasthaSystem = 'Sayanadi'`), `tbl_Rule_PostureStateFormula`
  (the `(C×P×A + M + G + L) mod 12` index, `SRC_PVR_INTEGRATED` §15.4.4), a nullable
  `PostureStateId` column on `tbl_Fact_PlanetaryState`, `vw_ChartPlanetEvidence` extended.
  This was the avastha masterproduct.md flagged "needs persisted janma ghaṭis, source-blocked"
  — migration 081's `tbl_Chart_Panchanga.JanmaGhatis` removes the first blocker, and §15.4.4
  turned out to already be a fully-specified, unambiguous formula (hand-verified against the
  JHora export's printed Activity table: Sun → Aagama, Moon → Kautuka, both exact). Deliberately
  not seeded: the secondary Cheṣṭā/Dṛṣṭi/Vicheṣṭā strength refinement (PVR's Table 37 sound-map)
  — the raw book extract renders that table's columns OCR-ambiguously, same class of problem as
  migration 081's deferred lunar month. Deeptādi and Lajjitādi (`FEAT-AVASTHA-03/04`) are
  **not** unblocked by this — PVR §15.4.3 gives 9 + 6 states but several depend on conjunction/
  aspect precedence the passage doesn't fully order (e.g. a planet that is both exalted and
  Sun-conjoined), unlike Sayanaadi's clean arithmetic; still needs a closer read before schema.
- **Jaimini special lagnas / Karakamsa** — migration 082: `vw_ChartKarakamsa`, the D9 sign of
  AK (PVR sec 7.3.6). No new storage: `ChartGenerationService.PersistAnalytics` already stamps
  `CharaKaraka` onto every chart type, D9 included, so this view is a plain read over
  `tbl_Chart_KeyDetails`. Verified against the JHora export exactly (Rahu AK -> Libra in D9).
  Bhaava / Ghati / Sree Lagna need **no DB work at all** — `tbl_Dim_SpecialLagnas` +
  `tbl_Rule_SpecialLagnaTimeRate` / `SpecialLagnaFraction` (migrations 28–30) already carry
  their full PVR-cited formulas; only the `cli` calculators (mirroring `HoraLagnaCalculator`)
  are missing. Vighati Lagna, Varnada Lagna, Pranapada Lagna, Indu Lagna, and Bhrigu Bindu are
  explicitly out of scope — `SRC_PVR_INTEGRATED` §5.7 says outright "there are some more
  special lagnas defined by Parasara, but they are beyond the scope of this book"; no other
  registered source covers them yet.
- **`FEAT-DATA-06` — Panchanga / time layer (DB slice)** — migration 081: `tbl_Dim_Tithi`
  (30), `tbl_Dim_Karana` (11), `tbl_Dim_NityaYoga` (27), `tbl_Dim_VedicWeekday` (7),
  `tbl_Dim_HoraSequence` (7), `tbl_Rule_PanchangaFormula` (Tithi/Karana/NityaYoga/HoraLord
  derivation formulas, `RuleSetId` 1), `tbl_Chart_Panchanga` (one row per D1 `ChartResultId`,
  mirroring `tbl_Chart_DashaPeriods`' key), `vw_ChartPanchanga`. Sourced
  `SRC_PVR_INTEGRATED` §1.3.8–1.3.12 (the book's only panchanga chapter); every seeded value
  cross-checked against `SRC_JHORA_EXPORT_RAMAKRISHNAN` (Krishna Tritiya, Vyatipaata, Tuesday,
  Hora Lord Venus, Janma Ghatis 58.89 all reproduce exactly). Applied clean to dev, idempotent
  on rerun. **Deliberately not built** (no PVR ch.1 source found): Karana lord, Nitya Yoga
  lord, Samvatsara, lunar month (PVR Table 4's raw extract is OCR-garbled — needs a clean
  source pass), Mahakala Hora / Kaala Lord (JHora extensions past PVR's 24-hora scheme). See
  the migration header for the full narrative. Remaining (CLI): the calculator that reads
  Sun/Moon longitudes + `SunTimes` and populates `tbl_Chart_Panchanga`; `verify-panchanga`.
- **`FEAT-DATA-04`** — ayanāṁśa default fixed (migration 054 repoints `tbl_Rule_Ayanamsa`
  from Jagannatha mode 26 to Lahiri mode 1; `verify-vargas` / `verify-jaimini` green).
  Still open: the reference benchmark harness — `tbl_Dim_AyanamsaBenchmarkCases` is empty
  while `tbl_Dim_AyanamsaBenchmarkPositions` (10) + `tbl_Dim_DashaBenchmarkPeriods` (9) are
  orphaned on `CaseId = 1`; re-seed `BENCH_RAMAKRISHNAN_P_JHORA_1981`
  (`ReferenceAyanamsaDegrees` 23.595, `SRC_JHORA_EXPORT_RAMAKRISHNAN`).
- **`FEAT-DATA-05`** — source-attributed yoga corpus schema (migrations 47–49): applied
  locally; roll to other environments after the corpus completes. **`db/079`** adds the Type
  (`FormationFamilyCode`, reused from migration 48) + Rule (`ShortFormationRule`, new) axes to
  `tbl_Rule_Yoga` and wires them into `vw_ChartYogaEvaluations` (`YogaTypeCode`/`YogaRule`);
  seeded for 146 of 223 evaluated `YogaCode`s from the actual evaluator predicate — see
  [`db_view_catalog.md`](db_view_catalog.md#key-inference-page--step--source-planned-round-2).
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
