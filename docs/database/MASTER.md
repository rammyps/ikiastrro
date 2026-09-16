---
last_updated: 2026-09-16
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
| [`karakafix.md`](karakafix.md) | Proposed normalized connection across Naisargika, Sthira and Chara karakas, life matters, and chart-level friendship evaluation |
| [`db_view_catalog.md`](db_view_catalog.md) | UI table component ⇄ backing view / TVF binding — consumer-side index (definitions stay in `schema.md`); see [`../../project_standards.md`](../../project_standards.md) § 4 |
| [`action-required-audit.md`](action-required-audit.md) | Live zero-row/suspended-population sweep: cause, priority, required action, and completion evidence |
| [`rebuild-generated-data-plan.md`](rebuild-generated-data-plan.md) | Planned safe reset + bulk CLI + PowerShell orchestration for regenerating supported facts |

## Current state

- **Baseline** `db/ikiastrro.sql` + numbered migrations `db/NN_*.sql` applied in order,
  tracked in `dbo.SchemaMigrations` (keyed by `ScriptName`). Migrations `22`–`089` are the
  active layer; earlier flat history is frozen under `db/_archive/`. `055`–`089` are applied
  to dev but **not yet folded forward** into `db/ikiastrro.sql` — do that once proven on the
  other environments. **The `workstream/cli` → `master` merge (2026-09-16) landed
  `077`_tighten_ashtakavarga_reduction_rules and `078`_correct_ashtakavarga_moon_venus_benefic_places
  from cli alongside master's own independently-numbered `071`_fix_shadbala_view_and_minimum_rupas,
  `072`_shadbala_kalabala_params_and_planetary_war, and `076`_register_strength_ashtakavarga_rule_catalog —
  the intended `079` collision-avoidance gap worked, but `071`, `072`, and `076` now each have
  **two distinct scripts sharing the same numeric prefix** (e.g. `071_fix_shadbala_view_and_minimum_rupas.sql`
  and `071_seed_bphs_planet_sanskrit_text.sql`). `dbo.SchemaMigrations` keys on full `ScriptName`
  so both apply and neither silently overwrites the other, but the numeric ordering promise is
  broken for those three numbers — **do not renumber without first checking which of each pair is
  already applied on dev/staging**, since `SchemaMigrations` rows would no longer match a renamed
  file.
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
- **2026-09-11 rule-mapping audit** — every calculator under `Engines/` (`workstream/cli`)
  cross-referenced against `tbl_Rule_Catalog` (38 tables). Full write-up, including the
  "Formulas computed in C#/SQL with no `tbl_Rule_*` citation at all" section, is in
  [`rules-engine.md`](rules-engine.md). Findings, and same-day closure:
  - The exaltation degrees above were duplicated **four** independent times in C# (one had no
    shared field name — found by grepping the magic numbers) — consolidated onto one shared
    `AstroMath.DeepExaltationPoints` constant; no new table needed, `tbl_Rule_GrahaDignity`
    already carries the degree. `tbl_Rule_Exaltation` itself still doesn't exist (unchanged).
  - Vimshottari Dasha's own core table (9-planet order, 120-year cycle, per-lord years) had
    **no `tbl_Rule_*` row anywhere** — closed: new `tbl_Rule_VimshottariPeriod` (migration 085,
    SRC_PVR_INTEGRATED §16.2 Table 38, verified against the raw extract).
  - `tbl_Rule_Karaka` (reserved since migration 18, 0 rows) was schema-designed for exactly
    the Chara Karaka assignment rule `CharaKarakaCalculator` hardcodes — closed: seeded by
    migration 085 (SRC_PVR_INTEGRATED §8.2 Table 13, verified against the raw extract).
  - Arudha Pada's counting rule had no reserved table shape at all — closed: new
    `tbl_Rule_ArudhaFormula` (migration 085, SRC_PVR_INTEGRATED §9.2, verified against the raw
    extract).
  - **Deliberately left open** — both source-honesty calls, not oversights: Sade Sati/Kantaka/
    Ashtama's `SourceRefCode` (no registered source's raw text actually discusses the offset
    rule) and `tbl_Rule_YogaValidationDefinition`'s reproducibility gap (`dbo.SchemaMigrations`
    records `054_add_yoga_validation_tables.sql` as applied — 1,002 rows live in dev — but no
    file by that name exists under `db/`; a from-scratch build can't reproduce it. Needs
    recovering the script or clearing the stale migration record).
  - CLI side: `verify-dasha` (new), `verify-jaimini` and `verify-dignity` (extended) all cross-
    check the new/consolidated citations against their hardcoded C#. 356 tests + all 15
    `verify-*` modes green.

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
  the migration header for the full narrative. CLI side (calculator + `verify-panchanga`) is
  **done** on `workstream/cli` — see that workstream's `MASTER.md`; `tbl_Rule_PanchangaFormula`
  and `tbl_Rule_PostureStateFormula` (migration 083) were registered in `tbl_Rule_Catalog` by
  migration 084 after `verify-rules` caught them missing (a leftover gap from these two turns).
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
  Dina/Hora/Tribhaga Bala and Graha Yuddha *detection* (latitude winner criterion) are
  **done** (`workstream/cli`) — `ShadbalaCalculator` reuses `PanchangaCalculator`'s own
  verified weekday/Hora Lord rather than re-deriving them; `verify-strength` checks both the
  engine and the persisted `tbl_Fact_PlanetaryStrengthComponent` rows. Still open: Varsha/Masa/
  Ayana Bala, and the Yuddha Bala *magnitude* (`tbl_Rule_PlanetaryWar`'s diameter-based delta
  formula) — `SRC_RAMAN_GRAHA_BHAVA_BALAS` is a DJVU with no text extract, so both stay
  deliberately unquantified rather than guessed. **`db/100`** (2026-09-15) fixed a wrong Sun
  minimum (`tbl_Rule_ShadbalaMinimumRupas` had 5.000, contradicting its own narrative + BPHS
  27.32-33 — corrected to 6.500) and added `tbl_Rule_ShadbalaMaximumRupas` (new sibling table,
  7 rows, engine-reachable ceiling per planet — not a BPHS figure, BPHS has no maximum concept,
  see the migration's own header for the derivation); `vw_ChartShadbala` now computes
  Minimum/Maximum/`PercentOfMinimum`/`PercentOfMaximum` live via `JOIN` instead of the
  `tbl_Fact_PlanetaryStrength.MinimumRequiredRupas` stored column, which `InsertAll` never
  populates (only a one-time 071 backfill ever did) — every chart computed since then had a
  silently-NULL minimum until this fix.
- **`FEAT-ASHTAKAVARGA-01`** — migrations 074–078: production `dbo` schema
  (`tbl_Rule_AshtakavargaContribution` — 56-row Parāśari matrix, SAV total 337;
  `tbl_Rule_AshtakavargaReduction` — Ṭrikoṇa / Ekādhipatya / Sodhya-Piṇḍa;
  `tbl_Fact_BhinnaAshtakavarga` / `…Contribution`, `tbl_Fact_SarvaAshtakavarga`,
  `tbl_Fact_AshtakavargaPinda`; `vw_ChartAshtakavarga`). Matrix hand-verified against the
  JHora export (Saturn BAV row reproduced exactly); JHora BAV grid + Piṇḍa seeded into the
  `research.*` benchmark. `077` tightens the reduction rules, `078` corrects the Moon/Venus
  benefic places to the Parāśari variant JHora uses. The `cli` `AshtakavargaCalculator` +
  `verify-ashtakavarga` now reproduce the JHora export for `1_Ramakrishnan` exactly (BAV,
  SAV, all seven Rāśi/Graha/Sodhya Piṇḍa) — **done**, merged into master 2026-09-16.
- **`verify-sources`** — the `SourceRefCode` tripwire loop is now scoped to schema `dbo`
  (it was crashing on the `research.*` tables from migrations 056/067/070); green again,
  and it now also checks the new Ashtakavarga / strength `SourceRefCode`s.

## Planned

- Rules-engine **Phase 2** — calculators read `tbl_Rule_*` instead of hard-coded C#.
- Wire the reference dimension tables (`tbl_Planets`, `tbl_SignAttributes`, `tbl_Nakshatras*`)
  into the engine (currently seeded + cross-checked, not read).
- `tbl_Dim_HouseSignification` / `tbl_Dim_PlanetSignification` / `tbl_Dim_PlanetHouseKaraka`
  (migration 030) — reserved, unapplied; `LifeAreaMap` hardcodes their data today.
- Vimśopaka Bala / Vaiśeṣikāṁśa (`FEAT-STRENGTH-02`) schema — four varga-group weights still
  need a cited source before the shape is settled.
