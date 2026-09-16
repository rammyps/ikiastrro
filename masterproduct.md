---
last_updated: 2026-09-10
togaf: Requirements Management
safe: Feature / Capability register
---

# ikiastrro — Master Product (features & functionality)

Supersedes `PRODUCT.md`. The single answer to "what does the software do, and how much of
each part is done", split across the three workstreams (`STANDARDS.md` §E.1, §M.3).

IDs are `FEAT-<AREA>-<NN>`. Per-feature checklist **DB · Core · Verify · Web · Docs**
(`% = checked ÷ 5`; `—` = not applicable). Status ladder
`Planned → Designed → DB → Core → Verified → Web → Done`. The checklist boxes map to
workstreams: `DB` → **database**, `Core` / `Verify` → **cli**, `Web` → **ui**, `Docs`
cross-cutting.

Features in **Now** (see [`ROADMAP.md`](ROADMAP.md)) also carry an **expanded block** —
Outcome · Priority rationale · ABB → SBB · DB+CLI slice · UI slice · Done-evidence — which
collapses back to the checklist row once the feature leaves Now. This keeps the product-head
reasoning visible on live work without carrying it for all ~40 features.

## Product intent

ikiastrro is an **astrologer-facing horoscope analysis and decision-support engine**. Its
primary job is to turn the large set of chart calculations into a structured statistical
evidence model that surfaces planetary and house strengths, weaknesses, conflicts,
confirmations, and omissions an astrologer might miss during manual analysis.

Chart generation, divisional charts, dashas, dignities, relationships, and special points
are calculation infrastructure for that analysis. They are not the product's end objective
and should not be evaluated only by whether a chart can be displayed to an end user.

The product should therefore:

- preserve every input, rule, source, method, and intermediate value needed to explain a finding;
- combine independent signals into ranked or grouped strengths and weaknesses rather than emit
  isolated textbook rules;
- show supporting and contradicting evidence, including cross-varga confirmation and timing
  relevance;
- make uncertainty, source differences, and unresolved traditions visible to the astrologer;
- help an astrologer review a horoscope comprehensively without replacing professional judgment;
- treat UI chart views as inspection tools for the analytical model, not as the main deliverable.

The central success measure is **reduced analytical omission with traceable evidence**, not
the number of charts rendered.

## Value streams & workstreams

Two product **value streams** over three engineering **workstreams**; local worktree ↔ Git
branch ↔ path scope is 1 : 1 : 1 (`STANDARDS.md` §E.1). **Claude Code is the primary agent**
(§E.2) — it owns the cross-cutting files below and integration to `master`.

- **Value stream · DB + CLI** — the calculation product (schema → engine → CLI → verification)
  - **Database** · `workstream/database` · `…\ikiastrro.wt\database` · owns `db/`, `src/Ikiastrro.Data/` · [`docs/database/MASTER.md`](docs/database/MASTER.md)
  - **CLI** · `workstream/cli` · `…\ikiastrro.wt\cli` · owns `src/Ikiastrro.Core/`, `src/Ikiastrro.Cli/`, `tests/Ikiastrro.Yoga.Tests/` · [`docs/cli/MASTER.md`](docs/cli/MASTER.md)
- **Value stream · UI** — the astrologer-facing product
  - **UI** · `workstream/ui` · `…\ikiastrro.wt\ui` · owns `src/Ikiastrro.Web/`, `tests/Ikiastrro.Web.Tests/` · [`docs/ui/MASTER.md`](docs/ui/MASTER.md)

`Ikiastrro.Core` (the engine) rides with **cli**. A schema change that neither engine nor UI
logic needs yet is a **database**-only change.

Cross-cutting — edited on `master` by the primary agent only: `README.md` · `ARCHITECTURE.md`
· `masterproduct.md` · `ROADMAP.md` · `INFRASTRUCTURE.md` · `MASTER.md` · `personas.md` ·
`value-stream.md` · `docs/architecture/` · `docs/governance/` · `decisions/` ·
`docs/research/sources.md`.

## Cross-stream dependencies

The DB + CLI stream **publishes**; the UI stream **consumes**. The contract:

- **UI reads only persisted rows** — `tbl_Chart_*`, `tbl_Fact_*`, the `vw_*` evidence views,
  `tbl_Rule_Ayanamsa` — and never recomputes. A calculation the UI needs is a DB + CLI
  feature first (`DB` + `Core` + `Verify`), then a UI feature (`Web`).
- **Chart data must be regenerated** after any ayanamsa / rule-set / engine change before the
  UI reflects it (`compute-all <name>` / `backfill-charts`).
- **Schema is additive** (new typed columns, not reshaped), so a DB feature can land ahead of
  the CLI / UI that reads it.
- Full interface catalogue: [`docs/architecture/domain-contracts.md`](docs/architecture/domain-contracts.md).

## Rollup

| Area | Workstream | Features | Avg % | Missing DB | Missing Core | Missing Verify | Missing Web | Missing Docs |
|---|---|---|---|---|---|---|---|---|
| DATA | database | 5 | 76% | 1 | — | 0 | — | 1 |
| ASTRO_CALC | cli | 2 | 90% | 0 | 0 | 0 | 1 | 0 |
| POSITION | cli | 1 | 100% | 0 | 0 | 0 | 0 | 0 |
| VARGA | cli | 1 | 80% | 0 | 0 | 0 | 1 | 0 |
| HOUSE | cli | 3 | 60% | 1 | 1 | 1 | 0 | 1 |
| NAKSHATRA | cli | 1 | 100% | 0 | 0 | 0 | 0 | 0 |
| DIGNITY | cli | 1 | 100% | 0 | 0 | 0 | 0 | 0 |
| RELATIONSHIP | cli | 4 | 60% | 1 | 1 | 1 | 1 | 1 |
| KARAKA | cli | 4 | 55% | 2 | 2 | 2 | 2 | 2 |
| AVASTHA | cli | 5 | 32% | 3 | 3 | 3 | 5 | 3 |
| DISPOSITOR | cli | 1 | 0% | 1 | 1 | 1 | 1 | 1 |
| STRENGTH | cli | 2 | 40% | 1 | 2 | 2 | 2 | 1 |
| ASHTAKAVARGA | cli | 1 | 80% | 0 | 0 | 0 | 1 | 0 |
| DASHA | cli | 2 | 90% | 0 | 0 | 0 | 0 | 1 |
| YOGA | cli | 1 | 0% | 1 | 1 | 1 | 1 | 1 |
| TRANSIT | cli | 2 | 80% | 0 | 0 | 0 | 1 | 0 |
| TERM | cli | 1 | 80% | 0 | 0 | 0 | 1 | 0 |
| ENGINE | cli | 2 | 90% | 0 | 0 | 0 | 1 | 0 |
| INFRA | database + cli | 1 | 80% | 0 | 0 | 0 | 1 | 0 |
| UI | ui | 12 | 70% | — | — | 4 | 2 | 9 |
| DOCS | cross-cutting | 1 | 100% | 0 | 0 | 0 | 0 | 0 |

*(The rollup is a manual mirror — recompute from the feature rows whenever a box changes.)*

---

## DATA — workstream: database

- **FEAT-DATA-01 · Star schema + data-driven rules engine (`tbl_Dim_*` / `tbl_Rule_*` / `tbl_Fact_*`)** — Done · 100% · Verify `verify-schema`, `verify-rules`
  DB [x] · Core [x] · Verify [x] · Web [—] · Docs [x] · Research: complete
  Versioned rule layer keyed off `tbl_Rule_Sets`; every rule row round-trips to its C# rule.
- **FEAT-DATA-02 · Chart-fact schema normalization (integer FKs, rule-set versioning, chart-type vocab)** — Done · 100% · Verify `verify-schema`
  DB [x] · Core [x] · Verify [x] · Web [—] · Docs [x] · Research: complete
- **FEAT-DATA-03 · Migration ledger + `SchemaMigrations` contract** — Verified · 80% · Verify `verify-schema`
  DB [x] · Core [—] · Verify [x] · Web [—] · Docs [x] · Research: complete
  Local ledger is partial (baseline from `db/ikiastrro.sql` + migrations 22–054); orphan
  rows exist in `tbl_Dim_AyanamsaBenchmarkPositions` (parent case row missing).
- **FEAT-DATA-04 · Ayanāṁśa default = Lahiri (Swiss mode 1), matching the JHora reference** — Verified · 80% · Verify `verify-vargas`, `verify-jaimini`
  DB [x] · Core [—] · Verify [x] · Web [—] · Docs [x] · Research: complete
  `tbl_Rule_Ayanamsa` had seeded Jagannatha (mode 26 = `SE_SIDM_SS_CITRA`, 22.745° for
  1981) as the active default; migration 054 repoints it to Lahiri (mode 1, 23.595° — the
  frame every `verify-*` chart is built in). Mode 27 (True Chitrapaksha) matches the
  reference but needs `sefstars.txt`, absent from this file-less Moshier build. Same fix
  cleared a regeneration FK-547 (`GenerateAll` now deletes strength / bhava-strength /
  vargottama facts before `tbl_ChartResults`). Both saved people regenerated; 11/11
  `verify-*` green. Remaining 20%: re-seed the `BENCH_RAMAKRISHNAN_P_JHORA_1981` case row
  and add a dedicated `verify-ayanamsa` (deferred).
- **FEAT-DATA-05 · Source-attributed yoga corpus schema (migrations 47–49)** — Designed · 20%
  DB [ ] (migrations prepared, applied locally; apply to other envs after corpus completion) · Core [—] · Verify [x] · Web [—] · Docs [x] · Research: active
  Normalised D1/D9 requirement tracking + formation/outcome/source axes for `tbl_Rule_Yoga`.

## ASTRO_CALC — workstream: cli

- **FEAT-ASTRO_CALC-01 · Sidereal positions (Swiss Ephemeris)** — Done · 100% · Verify `verify-schema`
  DB [x] · Core [x] · Verify [x] · Web [x] · Docs [x] · Research: complete
- **FEAT-ASTRO_CALC-02 · Sunrise / sunset (`swe_rise_trans`)** — Verified · 80% · Verify `verify-jaimini`
  DB [x] · Core [x] · Verify [x] · Web [ ] · Docs [x] · Research: complete

## POSITION — workstream: cli

- **FEAT-POSITION-01 · D1 (Rāśi) chart** — Done · 100% · Verify `verify-schema`
  DB [x] · Core [x] · Verify [x] · Web [x] · Docs [x] · Research: complete

## VARGA — workstream: cli

- **FEAT-VARGA-01 · Divisional charts D1–D60 (21 types)** — Verified · 80% · Verify `verify-vargas`
  DB [x] · Core [x] · Verify [x] · Web [ ] (only D1/D9 rendered) · Docs [x] · Research: complete
  `verify-vargas` green across all 21 charts for both saved people (after FEAT-DATA-04).

## HOUSE — workstream: cli

- **FEAT-HOUSE-01 · Whole-sign houses (Lagna/Sūrya/Chandra) + house lords** — Done · 100% · Verify `verify-schema`
  DB [x] · Core [x] · Verify [x] · Web [x] · Docs [x] · Research: complete
- **FEAT-HOUSE-02 · Functional benefic / malefic by Lagna** — Verified · 80% · Verify `verify-functional-nature`
  DB [x] · Core [x] · Verify [x] · Web [x] · Docs [ ] · Research: complete
- **FEAT-HOUSE-03 · Bhāva significations + Sthira Kāraka mapping** — Designed · 0%
  DB [ ] · Core [ ] · Verify [ ] · Web [ ] · Docs [x] · Research: partial (`SRC_RAMAN_HTJH`, 3 unsourced cells)
- **FEAT-HOUSE-04 · Baadhaka sthaana / baadhaka by rasi (PVR §13.3)** — Verified · 60% · Verify `verify-baadhaka`
  DB [ ] (computed, no rule table) · Core [x] · Verify [x] · Web [ ] · Docs [x] · Research: complete
  (`SRC_PVR_INTEGRATED` Table 31; Rahu/Ketu co-baadhaka rows deliberately diverge, see `pvr-coverage.md` Ch. 3)

## NAKSHATRA — workstream: cli

- **FEAT-NAKSHATRA-01 · Nakshatra / pāda / Vimśottari lord / KP sub-lord + reference linkage** — Done · 100% · Verify `verify-schema`
  DB [x] · Core [x] · Verify [x] · Web [x] · Docs [x] · Research: complete

## DIGNITY — workstream: cli

- **FEAT-DIGNITY-01 · Pañchadhā Maitrī dignity (9-tier)** — Done · 100% · Verify `verify-schema`, `verify-dignity`
  DB [x] · Core [x] · Verify [x] · Web [x] · Docs [x] · Research: complete

## RELATIONSHIP — workstream: cli

- **FEAT-RELATIONSHIP-01 · Conjunctions (Yuti) + multi-graha conjunction (Graha Saṃyoga) groups** — Done · 100% · Verify `verify-schema`
  DB [x] · Core [x] · Verify [x] · Web [x] · Docs [x] · Research: complete
  Pair rows (`tbl_Chart_Conjunctions`) + a ≥ 2-graha group/member layer
  (`tbl_Chart_MultiGrahaConjunction` / `…Member`) with per-planet degree / longitude /
  dignity / retrograde / combust, D1 span + per-member orb, and `MemberKey` for
  yoga-subset matching. Groundwork for FEAT-YOGA-01.
- **FEAT-RELATIONSHIP-02 · Aspects (Graha Dṛṣṭi)** — Done · 100% · Verify `verify-schema`
  DB [x] · Core [x] · Verify [x] · Web [x] · Docs [x] · Research: complete
- **FEAT-RELATIONSHIP-03 · Combustion (Asta)** — Done · 100% · Verify `verify-schema`
  DB [x] · Core [x] · Verify [x] · Web [x] · Docs [x] · Research: complete
- **FEAT-RELATIONSHIP-04 · Compound Maitrī / argala / sambandha** — Planned · 0%
  DB [ ] · Core [ ] · Verify [ ] · Web [ ] · Docs [ ] · Research: not started

## KARAKA — workstream: cli

- **FEAT-KARAKA-01 · Chara Karakas (8-fold Aṣṭa)** — Verified · 80% · Verify `verify-jaimini`
  DB [x] · Core [x] · Verify [x] · Web [ ] · Docs [x] · Research: complete
- **FEAT-KARAKA-02 · Special points (AL + 12 Bhāva Arudhas + HL + all 11 upagrahas)** — Done · 100% · Verify `verify-jaimini`, `verify-upagrahas`
  DB [x] · Core [x] · Verify [x] · Web [x] · Docs [x] · Research: complete
  PVR Gulika/Maandi convention; `verify-upagrahas` passes all 21 charts (stored charts
  regenerated on the Lahiri default, FEAT-DATA-04).
- **FEAT-KARAKA-03 · Sthira Kāraka** — Planned · 0%
  DB [ ] · Core [ ] · Verify [ ] · Web [ ] · Docs [ ] · Research: partial (`SRC_RAMAN_HTJH`)
- **FEAT-KARAKA-04 · Naisargika Kāraka (Sapta vs Aṣṭa — undecided)** — Planned · 0%
  DB [ ] · Core [ ] · Verify [ ] · Web [ ] · Docs [ ] · Research: partial
- **FEAT-KARAKA-05 · Special Lagnas (Bhaava / Ghati / Sree)** — Done · 100% · Verify `verify-jaimini`
  DB [x] · Core [x] · Verify [x] · Web [ ] · Docs [x] · Research: complete
  Completes tbl_Dim_SpecialLagnas (db/28) — Hora Lagna shipped first (FEAT-KARAKA-02); Bhaava/
  Ghati now share SpecialLagnaTimeRateCalculator.cs (TIME_FROM_SUNRISE, 0.25°/1.25° per min);
  Sree Lagna is the NAKSHATRA_FRACTION family, SreeLagnaCalculator.cs. Not yet surfaced in any
  chart module (see docs/ui/components/view-grid.md).

## AVASTHA — workstream: cli

- **FEAT-AVASTHA-01 · `AgeState` (Bālādi)** — Verified · 80% · Verify `verify-avastha`
  DB [x] · Core [x] · Verify [x] · Web [ ] · Docs [x] · Research: complete
- **FEAT-AVASTHA-02 · `WakefulnessState` (Jāgradādi)** — Verified · 80% · Verify `verify-avastha`
  DB [x] · Core [x] · Verify [x] · Web [ ] · Docs [x] · Research: complete
- **FEAT-AVASTHA-03 · `RadianceState` (Dīptādi)** — Planned · 0%
  DB [ ] · Core [ ] · Verify [ ] · Web [ ] · Docs [ ] · Research: partial (bands need a cited edition)
- **FEAT-AVASTHA-04 · `ShameState` (Lajjitādi)** — Planned · 0%
  DB [ ] · Core [ ] · Verify [ ] · Web [ ] · Docs [ ] · Research: partial
- **FEAT-AVASTHA-05 · `PostureState` (Śayanādi)** — Planned · 0%
  DB [ ] · Core [ ] · Verify [ ] · Web [ ] · Docs [ ] · Research: not started (needs janma-ghaṭis + a cited edition)

## DISPOSITOR — workstream: cli

- **FEAT-DISPOSITOR-01 · Dispositor chains / final dispositor / mutual reception** — Planned · 0%
  DB [ ] · Core [ ] · Verify [ ] · Web [ ] · Docs [ ] · Research: not started

## STRENGTH — workstream: cli

- **FEAT-STRENGTH-01 · Ṣaḍbala (6 components) + Iṣṭa/Kaṣṭa + Bhāva Bala** — In progress · 70% · Research: sourced
  DB [x] · Core [~] · Verify [x] · Web [x] (summary) · Docs [x]
  Available now: total Ṣaḍbala, Sthāna/Dig/Kāla/Cheṣṭā/Naisargika/Dṛk totals, named
  component rows, Kālabala, strongest-planet ranking, exact longitude, Bhāva Bala.
  **DB slice done** (migrations 071–073, 076): `vw_ChartShadbala` regression (069) fixed;
  `tbl_Rule_ShadbalaMinimumRupas` seeded (reproduces JHora %Strength within rounding);
  `tbl_Rule_PlanetaryWar` + the six Kālabala `RuleParametersJson` seeded;
  `tbl_Dim_ShadbalaBenchmarkValues` holds the seven-planet JHora golden totals.
  Remaining (CLI): compute the six Tribhāga/Varṣa/Māsa/Dina/Horā/Ayana Kālabala
  components; planetary-war adjustment; populate `MinimumRequiredRupas` + the Rashmi /
  Parāśara Iṣṭa/Kaṣṭa columns; a `verify-shadbala` mode against the benchmark.
- **FEAT-STRENGTH-02 · Vimśopaka Bala + Vaiśeṣikāṃśa grades** — Planned · 15% · Research: partial
  DB [ ] · Core [ ] · Verify [ ] · Web [ ] · Docs [x]
  Prerequisite varga charts exist through the Shodashavarga set. Remaining: source the four
  varga-group weights, compute dignity-weighted scores, define Vaiśeṣikāṃśa membership and
  grade names, persist, expose to Yoga.

## ASHTAKAVARGA — workstream: cli

- **FEAT-ASHTAKAVARGA-01 · Bhinna / Sarva Ashtakavarga + Sodhya Piṇḍa** — Verified · 80% · Verify `verify-ashtakavarga`
  DB [x] · Core [x] · Verify [x] · Web [ ] · Docs [x]
  Production `dbo` schema (migrations 074–078): `tbl_Rule_AshtakavargaContribution` (56-row
  Parāśari benefic-places matrix — per-recipient totals 48/49/39/54/56/52/39, SAV grand
  total 337; Moon/Venus carry the Parāśari corrections JHora uses),
  `tbl_Rule_AshtakavargaReduction` (Ṭrikoṇa / Ekādhipatya / Sodhya-Piṇḍa steps),
  `tbl_Fact_BhinnaAshtakavarga` / `…Contribution`, `tbl_Fact_SarvaAshtakavarga`,
  `tbl_Fact_AshtakavargaPinda`, `vw_ChartAshtakavarga`.
  `AshtakavargaCalculator` (Core `Engines/Ashtakavarga/`, `ChartBundle.Ashtakavarga`,
  wired into `ChartGenerationService`) + `AshtakavargaRepository`.
  `verify-ashtakavarga` reproduces the JHora export for `1_Ramakrishnan` **exactly** —
  every BAV cell, the SAV, and all seven Rāśi/Graha/Sodhya Piṇḍa triples — and checks the
  persisted `tbl_Fact_*`. `tests/Ikiastrro.Yoga.Tests/AshtakavargaCalculatorTests` (6).
  Remaining: a UI Ashtakavarga table over `vw_ChartAshtakavarga` (`Web [ ]`).

## DASHA — workstream: cli

- **FEAT-DASHA-01 · Vimśottari (3-level, partial-at-birth)** — Done · 100% · Verify `verify-schema`
  DB [x] · Core [x] · Verify [x] · Web [x] · Docs [x] · Research: complete
- **FEAT-DASHA-02 · Life-in-weeks grid (4000-week)** — Verified · 80%
  DB [x] · Core [x] · Verify [x] · Web [x] · Docs [ ] · Research: complete
  (Grid rendering is tracked under FEAT-UI-11 — retired in `wkstream_UI_v2`; the 4000-week
  calc stays, its dedicated route does not.)

## YOGA — workstream: cli

- **FEAT-YOGA-01 · Source-attributed Raman 1–300 + PVR yoga detection** — In progress · 65% · Research: active
  DB [x] (`tbl_Rule_Yoga` 146 rows, Type/Rule DB-backed since migration 079; `tbl_Fact_YogaInputEvaluations`
  covers 223 distinct `YogaCode`s per chart) · Core [x] (223 `YogaCode`s tracked, 146 with a
  real coded predicate — 77 are name-only stubs (61 Raman 201–300 tail, 14 Batch-Eight
  "Unsupported") or deliberately `NOT_EVALUATED`) · Verify [x] · Web [ ] · Docs [x]
  P0 inputs: required vargas, exact longitude, day/night, lunar phase, source-specific
  subject sex; missing P0 context yields `NOT_EVALUATED`, not `ABSENT`. Reuses
  `ChartBundle.Strengths`, `SunTimes.IsNightBirth`, D1/varga longitudes, all registered
  varga charts, dignity / conjunction / graha-aspect engines. Gaps: a source-qualified
  strong/weak policy over Ṣaḍbala thresholds; reusable waxing/waning/full-Moon
  classification; Vaiśeṣikāṃśa output; subject sex on `BirthDetails`; structured
  missing-requirement codes per result. PVR chapter 11 cross-check (2026-09-13): ~87/98
  (~89%) of PVR's individually-named yogas covered (shared Raman corpus); confirmed open —
  Maalaa, Subha, Asubha, Guru-Mangala, Chamara, Khadga, Lagnaadhi, Saarada,
  Dharma-Karmadhipati (`Vipareeta Raja` is done, dropped from this list); PVR's 58 further
  *unnamed* numbered combinations (§11.7.3/11.8/11.9/11.10) have no per-item rows at all,
  only 2 generic catch-alls. Full gap list + sequenced next steps: `yoga-corpus.md`.

## TRANSIT — workstream: cli

- **FEAT-TRANSIT-01 · Slow-planet sign-transit history (1930–2060)** — Verified · 80% · Verify `precheck-planet-transits`
  DB [x] · Core [x] · Verify [x] · Web [ ] · Docs [x] · Research: complete
- **FEAT-TRANSIT-02 · Sade Sati / Kantaka / Ashtama Shani** — Done · 100%
  DB [x] · Core [x] · Verify [x] · Web [x] · Docs [x] · Research: complete

## TERM — workstream: cli

- **FEAT-TERM-01 · Terminology catalogue (Sanskrit / English / Tamil)** — Verified · 80% · Verify `verify-terminology`
  DB [x] (`tbl_Astro_Terminology` + `_Text`) · Core [x] (`TerminologyCatalog`) · Verify [x] · Web [ ] · Docs [x] · Research: complete
  236 concepts seeded `sa` + `en` (`Latn`); Tamil (`ta`/`Taml`) + Devanagari (`sa`/`Deva`)
  are pure inserts later — schema needs no rework.

## ENGINE — workstream: cli

- **FEAT-ENGINE-01 · Engine-stack layering (`Engines/<Name>/`)** — Verified · 80% · Verify `verify-schema` + `verify-pipeline`
  DB [x] · Core [x] (13 engines + `ChartPipeline` / `ChartBundle`; `ChartAnalyzer` split
  into House / Nakshatra / Relationship engines, behaviour-preserving) · Verify [x] ·
  Web [ ] · Docs [x] · Research: complete
  Reserved interface seams: Dispositor, Strength, Yoga, Sthira/Naisargika Karaka.
  `ChartGenerationService.GenerateAll` pipeline adoption deferred.
- **FEAT-ENGINE-02 · Rule-table portability (`tbl_Rule_Catalog` + interpreters)** — Verified · 80% · Verify `verify-rules`
  DB [x] (portability tail on 7 `tbl_Rule_*`; `tbl_Rule_Catalog` + 12-row index; 5 reserved
  rule tables) · Core [x] (3 `IVargaMethodInterpreter` families: `LINEAR_VARGA` /
  `GRID_VARGA` / `BAND_VARGA`; `seed-rule-params` backfills all 20 `RuleParametersJson`) ·
  Verify [x] · Web [—] · Docs [x]

## INFRA — workstream: database + cli

- **FEAT-INFRA-01 · Multi-environment config (dev/stage/uat/prod)** — Verified · 80%
  DB [x] (`:setvar`, `tbl_Dim_Source`) · Core [x] (`SqlConnectionFactory.Create` + CLI
  `--db` + `:setvar DbName`) · Verify [x] · Web [ ] (per-env `appsettings.{Environment}.json`
  land with a real stage/uat deploy) · Docs [x] · Research: complete

## UI — workstream: ui

Checklist for UI features: `DB` / `Core` are `—`. `Docs` = a `docs/ui/` component doc exists.
`Verify [x]` requires **all** of: correct persisted-data mapping (value ↔ planet / chart /
date), a structural or golden snapshot, interaction behaviour, empty / loading / error states,
keyboard + focus, and one real browser smoke case. `Web [x]` = route live **and** matches its
`docs/ui/components/*.md` spec — not merely "it renders". (Full bar: `wkstream_UI_v2.md` →
*Verification*.)

- **FEAT-UI-01 · App shell + brand system (MudBlazor, `tokens.css`, shared header)** — Verified · 60%
  DB [—] · Core [—] · Verify [x] · Web [x] · Docs [ ]
- **FEAT-UI-02 · Home / entry (`/`) — searchable name, saved people, inline Preferences + Add** — In progress · 40%
  DB [—] · Core [—] · Verify [x] · Web [ ] · Docs [x] (`docs/ui/components/home.md`)
  `wkstream_UI_v2` rebuild: MudBlazor shell; `MudAutocomplete` name search; Preferences
  disclosure top-left (three selector groups — Ayanāṁśa / Chart style / Language); `Add New`
  unhides the entry fields inline; completing Country → `/transit-wheel/{id}`. Absorbs
  FEAT-UI-03 and FEAT-UI-12. **`Web [ ]`** — the live page still differs from `components/home.md`:
  explanatory text not stripped, search shows a list not one matched row, and Preferences has
  2 selectors not 3 (North/West Indian + Tamil not yet shown disabled). Picking a person now
  lands on `/transit-wheel/{id}` (the v2 Transit landing — `FEAT-UI-13`).
- **FEAT-UI-03 · Add person — inline on Home (Name · Sex · DOB · Time · City · Country)** — In progress · 20%
  DB [—] · Core [—] · Verify [ ] · Web [ ] · Docs [x] (`docs/ui/components/home.md`)
  No longer a `/add` route — folded into Home (`FEAT-UI-02`). **Sex** is now an always-visible
  option box (`MudRadioGroup` Male/Female, `tbl_BirthDetails.Sex`, migration 052) and **every**
  field — Sex included — is validated non-blank before generation. Typing an unknown name copies
  it into the Add-New Name. **`Web [ ]`** until the full mockup Add flow ships — remaining gap:
  geocoding-failure fallback (manual lat/long/offset).
- **FEAT-UI-04 · Saved charts list (`/charts`) + inline delete** — Verified · 60%
  DB [—] · Core [—] · Verify [x] · Web [x] · Docs [ ]
- **FEAT-UI-05 · Chart workspace (`/charts/{id}`) — D1 hero + grouped varga rail** — **retired in `wkstream_UI_v2`** (replaced by `FEAT-UI-14`)
  DB [—] · Core [—] · Verify [x] · Web [x] · Docs [ ]
  `Workspace.razor` + `.razor.css` removed; `/charts/{id}` now serves the v2 All Charts page.
  `WorkspaceData` / `WorkspaceHeader` / `VargaBundles` are kept — `VargaView` still uses them.
- **FEAT-UI-06 · Varga view (`/charts/{id}/varga/{code}`) — grid + polar-wheel toggle** — Verified · 60%
  DB [—] · Core [—] · Verify [x] · Web [x] · Docs [ ]
- **FEAT-UI-07 · South Indian template (`/charts/{id}/south-indian-template`)** — Verified · 80%
  DB [—] · Core [—] · Verify [x] · Web [x] · Docs [x] (`src/Ikiastrro.Web/Components/Charts/README.md`)
- **FEAT-UI-08 · Timing (`/charts/{id}/timing`) — 3-level dasha tree** — Verified · 60%
  DB [—] · Core [—] · Verify [x] · Web [x] · Docs [ ]
- **FEAT-UI-09 · Astrologer evidence tables (`/charts/{id}/evidence`)** — Verified · 80%
  DB [—] · Core [—] · Verify [x] · Web [x] · Docs [x] (`docs/ui/components/evidence-tables.md`)
- **FEAT-UI-10 · Transit wheel (`/transit-wheel/{id}`) — natal ↔ transit + dasha selector** — Verified · 80% · **v1; being replaced by `FEAT-UI-13`**
  DB [—] · Core [—] · Verify [x] · Web [x] · Docs [x] (`docs/ui/components/spec_Natal_Transit_Comp_Wheel.md`)
  The v2 route now renders the `FEAT-UI-13` landing (D1 Birth tab live). The v1 natal↔transit
  wheel + dasha/date selectors + comparison table are gone from the page; `TransitSelection.cs`
  and its tests are kept for the moment (a later slice may reuse the date logic).
- **FEAT-UI-11 · Life-in-weeks grid (`/charts/{id}/life-weeks`)** — Verified · 60% · **retired in `wkstream_UI_v2`**
  DB [—] · Core [—] · Verify [x] · Web [x] · Docs [ ]
  Live in v1; the v2 re-do drops the route (the Vimśottari timeline is served by
  `/charts/{id}/timing`). `LifeWeeks.razor` + its golden snapshot go when v2 lands.
- **FEAT-UI-12 · Preferences — inline Home disclosure (top-left)** — In progress · 20%
  DB [—] · Core [—] · Verify [ ] · Web [ ] · Docs [x] (`docs/ui/components/home.md`)
  Not a route. A `MudCollapse` at Home top-left with three selector groups:
  **Ayanāṁśa** (all 21 catalogued options; default = active `tbl_Rule_Ayanamsa` = *Lahiri*,
  fixed) · **Chart style** (South Indian default; North Indian / West Indian listed for
  forward-compat — renderers deferred to ROADMAP *Later*) · **Language** (English now; Tamil —
  script or transliteration TBD — deferred). Choices apply to the next generation; per-browser
  persistence now, DB-backed default is a `database` follow-up. See `docs/ui/MASTER.md`
  NFR-UI-02 / -03 / -04, and NFR-UI-01 (runtime tab reorder — deferred).
- **FEAT-UI-13 · Transit landing (`/transit-wheel/{id}`) — v2, embedded wheel + two-tab table** — In progress · 40%
  DB [ ] · Core [—] · Verify [ ] · Web [ ] · Docs [x] (`docs/ui/components/spec_Natal_Transit_Comp_Wheel.md`)
  First `wkstream_UI_v2` implementation slice (Home → select person → Transit). Landed: the v2
  app shell (HOME pill hard-left, per-person `TRANSIT · ALL CHARTS · KEY INFERENCE` tabs revealed
  once a person is open, context band, `--maxw` / `--pad-x`); a scoped `ActivePerson`; the page
  at `/transit-wheel/{id}` with band heading `TRANSIT - D1 BIRTH CHART`, a static wheel placement,
  and the **D1 Birth** tab against `vw_ChartPlanetEvidence` via a focused `Natal_Transit_Comp_WheelRepository`
  (persisted rows only, sorted Lagna → Saturn…Sun — deliberately not `AstrologerEvidenceRepository`,
  which fans out ~20 sections); browser smoke + `Natal_Transit_Comp_WheelMath` unit tests. **`Web [ ]`** —
  open: the **Current Transit** tab is wired to `GocharaRepository` but that source only covers
  Saturn/Jupiter/Rahu and returns nothing for "now" (degrades to the `backfill-planet-transits`
  hint); `InSignMotion` / `NextChangeMotion` columns show `—` pending Codex's
  `tbl_TransitPositionReference` boundary-motion merge (`db/055`); the live natal↔transit wheel
  overlay is a Codex `Components/Charts/**` component; `Verify [ ]` until a page-DI bUnit harness
  covers both tabs. `ALL CHARTS` and `KEY INFERENCE` tabs both fall back to `/charts/{id}` (the v1
  workspace) until their slices — the v1 `/charts/{id}/evidence` route is 500ing on a
  `database`-workstream regression (migration `db/069` recreated `vw_ChartShadbala` without
  `BirthDetailId` / `Planet` / `PercentOfMinimum`), and `GocharaRepository.SaveSnapshots` has a
  latent datetime-precision MERGE bug that can dup-key `tbl_TransitPositionReference`.
  Supersedes `FEAT-UI-10` for `/transit-wheel/{id}` once `Web [x]`.
- **FEAT-UI-14 · All Charts (`/charts/{id}`) — v2, every divisional chart as a South-Indian grid** — In progress · 40%
  DB [—] · Core [—] · Verify [ ] · Web [ ] · Docs [x] (`docs/ui/wkstream_UI_v2.md`)
  Second `wkstream_UI_v2` slice. `AllCharts.razor` at `/charts/{id}` (the v1 `Workspace` hub is
  retired — `FEAT-UI-05`): all 21 divisional chart types in **divisor order**, 3 per row (2 ≤1000px,
  1 ≤620px), fed by `WorkspaceData.Load` (`tbl_ChartResults` + `tbl_Chart_KeyDetails`). Only the
  card heading bar is sunset; card, grid and margins on the canvas. The grid is the Codex
  `SouthIndianGrid` (`Components/Charts/**`) **re-skinned from the page via CSS-custom-property
  overrides** — no change to that component: sign name top-left, house-from-Lagna (sunset) over
  house-from-Moon (muted) top-right, colour-coded graha glyphs, `LAGNA` tag, peach + sunset Lagna
  box. Ungenerated vargas render an `EmptyState` card. Browser-verified (both test people, all 21
  grids, sunset heads, divisor order, no horizontal overflow, no console errors). **`Verify [ ]`**
  until a page-DI bUnit harness covers a per-varga render; `SouthIndianGrid`'s LAGNA-tag / sign-name
  crowding in narrow cells is a `Components/Charts/**` (Codex) polish item.

## DOCS — cross-cutting

- **FEAT-DOCS-01 · Documentation taxonomy + `masterproduct.md` + citation registry** — Done · 100% · Verify `verify-sources`
  DB [x] · Core [x] · Verify [x] · Web [x] · Docs [x] · Research: complete
