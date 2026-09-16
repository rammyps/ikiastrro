---
last_updated: 2026-09-16
workstream: database
togaf: C — Data Architecture
reflects: UI table components as of master @ b6603ba + the planned Key Inference 6-step flow (round 2, mockup only); Yoga Type/Rule DB-backed as of db/079; Ashtakavarga live as of b6603ba; YogaEvaluationRepository + Amsabala fact layer (db/101) added, repositories ready, UI pending
---

# Database — view catalogue (UI table ⇄ view binding)

Consumer-side index of the persisted SQL views / table-valued functions the **UI table
components** read. Every UI component that renders rows appears here bound to its source; it
never recomputes (`../architecture/domain-contracts.md`). Project rule:
[`../../project_standards.md`](../../project_standards.md) § 4.

The view **definitions** live in the `database` workstream — `db/NN_*.sql` and
[`schema.md`](schema.md) § "Views & functions". This file only maps *who reads what*; update a
row in the same change that adds or repoints a table component.

## UI table components → source

| UI component / page | Source view · TVF · table | Repository | Columns surfaced | Defined by |
|---|---|---|---|---|
| `PlanetPositionsTable` — `Pages/VargaView.razor` | `vw_ChartPlanetEvidence` | `AstrologerEvidenceRepository` | House · Planet · Motion · Degree · Sign · Nakṣatra · Nak. Pada · dignity | baseline + varga migrations |
| `HouseLordshipTable` — `Pages/VargaView.razor` | `tbl_Chart_HouseLords` (per `ChartResultId` + `ChartType`) | `ChartHouseLordsRepository` | House · lord · lord's house / sign | baseline |
| `ConjunctionsTable` — `Pages/VargaView.razor` | `tbl_Chart_Conjunctions` / `tbl_Chart_MultiGrahaConjunction(+Member)` | `ChartConjunctionsRepository`, `ChartMultiGrahaConjunctionRepository` | planet set · sign · house | baseline |
| **Transit landing — D1 Birth tab** — `Pages/Natal_Transit_Comp_Wheel.razor` | `vw_ChartPlanetEvidence` (`ChartType = 'D1'`) | `Natal_Transit_Comp_WheelRepository` | House · Planet · Motion · Degree · Sign · Nakṣatra · Nak. Pada | baseline + varga migrations |
| **Transit landing — Current Transit tab** — `Pages/Natal_Transit_Comp_Wheel.razor` | `tbl_TransitPositionReference` (+ `tvf_PlanetSignAtDate`) | `GocharaRepository` | House-from-D1 · Planet · Motion · Degree · Speed °/day · In-sign-since · Next-change · **`InSignMotion` / `NextChangeMotion` (pending — `db/055`)** | `db/45` (table) + `db/055` (boundary-motion columns) |
| `GocharaPanel` — `Pages/Timing.razor` | `tbl_PlanetSignTransitEvents` · `tvf_PlanetSignAtDate` · `vw_KetuSignTransitEvents` | `GocharaRepository`, `PlanetSignTransitEventsRepository` | planet · current sign · in-sign-since · next-change | seed (Sa/Ju/Ra 1930–2060) |
| `SadeSatiTable` — `Pages/Timing.razor` | `tvf_Chart_SadeSatiPeriods(@BirthDetailId)` | `SadeSatiRepository` | round · phase · from / to · sign | function (baseline) |
| `DashaTimeline` — `Pages/Timing.razor` | `tbl_Chart_DashaPeriods` (self-referencing, 3 levels) · `vw_Chart_DashaTimeline` | `DashaPeriodsRepository` | lord · level · from / to · age span | baseline |
| `LifeWeeks` (page, retired in v2) — `Pages/LifeWeeks.razor` | `tvf_Chart_LifeWeeks(@BirthDetailId)` | (page-level query) | week index · date · Mahādaśā lord | function (baseline) |
| `AstrologerEvidence` (page) — `Pages/AstrologerEvidence.razor` | `vw_ChartPlanetEvidence` · `vw_ChartMoonContext` · `vw_ChartShadbala` · `vw_ChartBhavaBala` · `vw_ChartYogaEvaluations` · `vw_YogaChartApplicability` · `vw_YogaContextRequirements` | `AstrologerEvidenceRepository` | per evidence section — see [`../ui/components/evidence-tables.md`](../ui/components/evidence-tables.md) | rules-engine migrations |
| `HouseNakshatraSpan` helper — `Pages/VargaView.razor` | `vw_Chart_HouseNakshatraSpan` | `HouseNakshatraSpanRepository` | house · nakṣatra span · lords | baseline |

## Chart (non-table) modules — data source, for completeness

The visual chart modules bind through `WorkspaceData.Load` / the same repositories, not a
dedicated view:

| Chart module | Source |
|---|---|
| `SouthIndianGrid_Detailed`, `MiniGrid`, `D1TemplateGrid`, `PolarWheel`, `ChartFrame`, `VargottamaStrip` | `tbl_ChartResults` + `tbl_Chart_KeyDetails` (+ `tbl_Chart_Aspects`, `tbl_Fact_Vargottama`) via `WorkspaceData.Load` |
| `Natal_Transit_Comp_WheelChart` | D1 `vw_ChartPlanetEvidence` (natal points) + `tbl_TransitPositionReference` via `GocharaRepository` (transit points) |

## Key Inference page — step ⇄ source (planned, round 2)

`KeyInference.razor` at `/key-inference/{id}` is **not built yet**. Superseded 2026-09-11: the
flat 13-sub-tab shape is replaced by a 6-step flow (spec:
[`../ui/components/key-inference.md`](../ui/components/key-inference.md); mockup:
[`../artifacts/ui/v2-mockup/key-inference-v2.html`](../artifacts/ui/v2-mockup/key-inference-v2.html)).
This maps each step's chart + table to the persisted view / table it reads — every one
read-only, no recompute.

| Step | Chart source | Table source | Repository | Notes |
|---|---|---|---|---|
| 1 · D1 / Transit | `vw_ChartPlanetEvidence` (D1, for the grid) | same, + `tbl_TransitPositionReference` for the transit toggle | `Natal_Transit_Comp_WheelRepository` / `AstrologerEvidenceRepository` · `GocharaRepository` | table now includes `Nakshatra`/`NakshatraPada` inline (moved from Planet Dignity) |
| 2.1 · About Houses | derived: graha count per house | `tbl_Chart_HouseLords` + `tbl_Chart_Conjunctions`/`tbl_Chart_MultiGrahaConjunction(+Member)` + `tbl_Chart_Aspects` | `ChartHouseLordsRepository` · `ChartConjunctionsRepository` · `ChartMultiGrahaConjunctionRepository` · `ChartAspectsRepository` | + 3 supporting cards below |
| 2.1 → supporting cards | — | `tbl_Chart_KeyDetails` WHERE `PointKind IN ('Arudha','Upagraha','SpecialLagna')` | `ChartKeyDetailsRepository` | Arudha padas / Upagrahas / Special Lagnas — computed, first time surfaced |
| 2.2 · About Planets | derived: closeness-to-exaltation % | `tbl_Chart_KeyDetails` (+ `vw_ChartMoonContext` facts card) | `ChartKeyDetailsRepository` · `AstrologerEvidenceRepository` | exaltation point / Δ / closeness need a new **`tbl_Rule_Exaltation`** (7 rows) — not built |
| 3 · Strength | `vw_ChartShadbala` (%-of-minimum/composition bar inline in 3.1's table) | `vw_ChartShadbala` + `tbl_Fact_PlanetaryStrengthComponent` (3.1) · `vw_ChartBhavaBala` + `tbl_Fact_BhavaStrengthComponent` (3.2) | `PlanetaryStrengthRepository`/`BhavaStrengthRepository` (new `GetSummaryByBirthDetailId`/`GetComponentsByBirthDetailId`, `PlanetStrengthChart`/`HouseStrengthChart`) | built 2026-09-14; first UI consumer of both views outside `AstrologerEvidenceRepository`'s generic dynamic-row dump |
| 4 · Planet-Chart | derived: Vaiśeṣikāṁśa stacked bar, from `tbl_Chart_KeyDetails.DignityStatus` over 16 vargas | `tbl_Chart_KeyDetails.Sign` across the 16 divisional `ChartType`s (Ṣoḍaśavarga grid) | `ChartKeyDetailsRepository` | Vargottama (`tbl_Fact_Vargottama`) + Varga-Dignity highlights are inline tags, not tables |
| 5 · Ashtakavarga | `vw_ChartAshtakavarga` (SAV bar) | `vw_ChartAshtakavarga` (BAV grid) + `tbl_Fact_AshtakavargaPinda` (Piṇḍa) | `AshtakavargaRepository` (read + write, merged onto `master` `b6603ba` 2026-09-16) | live — `verify-ashtakavarga` all-pass; every saved person backfilled via `recompute-keydetails` |
| 6 · Yoga | `vw_ChartYogaEvaluations` aggregate (coverage donut) | `vw_ChartYogaEvaluations` (+ `tbl_Rule_Yoga`) | `YogaEvaluationRepository` (new, typed `GetByBirthDetailId`, 2026-09-16) — previously only `AstrologerEvidenceRepository`'s generic dynamic-row query | **Type** (`YogaTypeCode`) + **Rule** (`YogaRule`) are DB-backed as of `db/079` — `tbl_Rule_Yoga.FormationFamilyCode`/`ShortFormationRule`, one row per YogaCode, left-joined in; `SourceVariantCode` dropped from display. Data already live (compute wired into `ChartGenerationService` since before this pass); repository built, UI tab still pending (Key Inference Phase 2) |
| 3.4 · Amsabala | — | `vw_ChartAmsabala` (per-planet named amsa across 4 varga-group schemes) | `AmsabalaRepository` (new, `db/101`, 2026-09-16) | Distinct from Vimśopaka Bala (`tbl_Rule_VimsopakaWeight`, still empty/reserved — PVR never publishes its numeric weight table, a separate open sourcing gap). Amsabala is the amsa-naming layer only, verified against PVR's worked Example 27 in `AmsabalaCalculatorTests`. Every saved person backfilled via `recompute-keydetails`; UI sub-tab pending (Key Inference Phase 2) |

`Chara Karaka` (`tbl_Chart_KeyDetails.CharaKaraka`, D1) moved from its own sub-tab into the
step 2.2 Planets table as a column.

### Unchanged headers (not part of the 6-step flow)

| Header | Source view · TVF · table | Repository |
|---|---|---|
| TIME PERIOD (DASHA) | `tbl_Chart_DashaPeriods` (3-level self-ref) · `vw_Chart_DashaTimeline` | `DashaPeriodsRepository` |
| SATURN TIME PERIOD | `tvf_Chart_SadeSatiPeriods(@BirthDetailId)` (+ Kaṇṭaka / Aṣṭama Śani) | `SadeSatiRepository` |

## Views not currently read by the UI

Present in `schema.md` but with no UI table consumer today — listed so a new binding is a
deliberate row here, not a silent add: `vw_Chart_Consolidated`,
`vw_NakshatraPadaDetails`, `vw_Dignity_Legend`.

Spoken-for by the **planned** Key Inference page above, but not yet read by shipped code:
`vw_ChartShadbala` · `vw_ChartBhavaBala` · `vw_ChartYogaEvaluations` (repository ready, UI
pending) · `vw_ChartAmsabala` (`db/101`, repository ready, UI pending) ·
`vw_ChartMoonContext`. `vw_ChartAshtakavarga` (BAV grid + SAV per sign; `db/074`) +
`tbl_Fact_AshtakavargaPinda` (`db/074`) are live in `KeyInference.razor` "3.3 ASTAVARGA" as of
`master` `b6603ba` (2026-09-16) — see row 5 above.

**Not yet created:** `tbl_Rule_Exaltation` (7 rows — the classical Uchcha Bindu sign+degree
per graha, needed for step 2.2's closeness-to-exaltation column) — a `workstream/database`
follow-up, still hard-coded in the `key-inference-v2.html` mockup only.

**Done — `db/079_add_yoga_type_and_rule.sql`:** `tbl_Rule_Yoga.FormationFamilyCode` (unused
since migration 48) is now the Type axis (`SUN`/`MOON`/`LAGNA`/`COMBINATION`, CHECK-constrained);
a new `ShortFormationRule NVARCHAR(300)` column carries Rule. One canonical row per
`(RuleSetId, YogaCode)` — both are source-independent properties of the yoga concept, not of one
`SourceVariantCode` row. Seeded for **146 of 223** evaluated `YogaCode`s, transcribed from the
actual predicate in `src/Ikiastrro.Core/Engines/Yoga/*.cs` (never freehand recall). The other 77
are deliberately left with no row — Type/Rule read `NULL` through the view — because no predicate
exists to transcribe: 61 are the uncoded Raman 201–300 tail (`RamanFinalHundredCatalog.cs`), 14
are `RamanYogaBatchEightEvaluator`'s explicit unsupported set, and `YOGA_VIDYA`/`YOGA_ARISHTA`
are `NOT_EVALUATED` by design. `vw_ChartYogaEvaluations` now exposes `YogaTypeCode`/`YogaRule`
via a `LEFT JOIN` on `(YogaCode, RuleSetId)`.
