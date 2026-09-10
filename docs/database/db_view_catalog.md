---
last_updated: 2026-09-10
workstream: database
togaf: C — Data Architecture
reflects: UI table components as of master @ 0006ba0 + the Natal_Transit_Comp_Wheel rename
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

## Views not currently read by the UI

Present in `schema.md` but with no UI table consumer today — listed so a new binding is a
deliberate row here, not a silent add: `vw_Chart_Consolidated`,
`vw_ChartShadbala` / `vw_ChartBhavaBala` (CLI + evidence only), `vw_NakshatraPadaDetails`,
`vw_Dignity_Legend`, `vw_ChartYogaEvaluations` (evidence only),
`vw_ChartAshtakavarga` (BAV grid + SAV per sign; `db/074`) — reserved for a future
Ashtakavarga table component, empty until `AshtakavargaCalculator` lands.
