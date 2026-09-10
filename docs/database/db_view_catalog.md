---
last_updated: 2026-09-11
workstream: database
togaf: C — Data Architecture
reflects: UI table components as of master @ cc08ed8 + the planned Key Inference sub-tabs (mockup only)
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

## Key Inference page — header / sub-tab ⇄ source (planned)

`KeyInference.razor` at `/key-inference/{id}` is **not built yet**. This maps each header and
KEY-INFERENCE sub-tab (the *category*) to the persisted view / table it will read — every one
read-only, no recompute. Mockup: the published `#key-inference` view
([`../artifacts/ui/v2-mockup/chart-evidence-hub.html`](../artifacts/ui/v2-mockup/chart-evidence-hub.html));
spec: [`../ui/wkstream_UI_v2.md`](../ui/wkstream_UI_v2.md#key-inference-4-headers).

### KEY INFERENCE header

| Sub-tab (category) | Source view · TVF · table | Repository | Notes |
|---|---|---|---|
| About Sign | `tbl_SignAttributes` (+ sign lord) | `SignAttributesRepository` | dimension, chart-agnostic |
| About Planet | `tbl_Chart_KeyDetails` + karaka / graha-nature rules | `ChartKeyDetailsRepository` | Planet · Kāraka · House Lord · Nature · Conditional rule |
| About Moon | `vw_ChartMoonContext` | `AstrologerEvidenceRepository` | 4 facts |
| About Houses | `tbl_Chart_HouseLords` + `tbl_Chart_Conjunctions` / `tbl_Chart_MultiGrahaConjunction(+Member)` + `tbl_Chart_Aspects` | `ChartHouseLordsRepository` · `ChartConjunctionsRepository` · `ChartMultiGrahaConjunctionRepository` · `ChartAspectsRepository` | one house-keyed table; chart dropdown |
| Planet Dignity | `vw_ChartPlanetEvidence` (+ `tbl_Fact_PlanetaryState` for Bālādi / Jāgradādi avasthās) | `AstrologerEvidenceRepository` · `PlanetaryStateRepository` | chart dropdown; combust rows sort under the Sun |
| Planet Strength · Shadbala | `vw_ChartShadbala` | `AstrologerEvidenceRepository` | **first UI table consumer** (was CLI + evidence only) |
| House Strength · Bhava Bala | `vw_ChartBhavaBala` | `AstrologerEvidenceRepository` | **first UI table consumer** |
| Vargottama · D1 & D9 | `tbl_Fact_Vargottama` | `VargottamaRepository` | |
| **Ṣoḍaśavarga** | `tbl_Chart_KeyDetails.Sign` across the 16 divisional `ChartType`s | `ChartKeyDetailsRepository` | one row per graha × 16 vargas; vargottama = varga sign == D1 |
| **Vaiśeṣikāṁśa** | `tbl_Chart_KeyDetails.DignityStatus` aggregated over the 16 vargas | `ChartKeyDetailsRepository` | own / friend / debil count — no dedicated view, roll up on read |
| **Varga Dignity** | `tbl_Chart_KeyDetails.DignityStatus` (per `ChartResultId` + `ChartType`) | `ChartKeyDetailsRepository` | exalt / own / debil per divisional chart |
| **Ashtakavarga** | `vw_ChartAshtakavarga` (BAV grid + SAV per sign) + `tbl_Fact_AshtakavargaPinda` (Rāśi / Graha / Sodhya Piṇḍa) | `AshtakavargaRepository` (read side — currently on `workstream/cli` `be7e37d`, not on `master`) | needs the Ashtakavarga engine merged so Web-generated people have the facts |
| **Chara Karaka** | `tbl_Chart_KeyDetails.CharaKaraka` (D1) | `ChartKeyDetailsRepository` | Jaimini 8-karaka (AK…DK) |
| *Chara Karaka → "Also computed" panel* | `tbl_Chart_KeyDetails` WHERE `PointKind IN ('Arudha','Upagraha','SpecialLagna')` · `tbl_Chart_Aspects` · `tbl_Chart_MultiGrahaConjunction(+Member)` | `ChartKeyDetailsRepository` · `ChartAspectsRepository` · `ChartMultiGrahaConjunctionRepository` | DB has these; listed, not yet rendered as tables |

### Other headers

| Header | Source view · TVF · table | Repository |
|---|---|---|
| YOGAS | `vw_ChartYogaEvaluations` (+ `vw_YogaChartApplicability` · `vw_YogaContextRequirements`) | `AstrologerEvidenceRepository` |
| TIME PERIOD (DASHA) | `tbl_Chart_DashaPeriods` (3-level self-ref) · `vw_Chart_DashaTimeline` | `DashaPeriodsRepository` |
| SATURN TIME PERIOD | `tvf_Chart_SadeSatiPeriods(@BirthDetailId)` (+ Kaṇṭaka / Aṣṭama Śani) | `SadeSatiRepository` |

## Views not currently read by the UI

Present in `schema.md` but with no UI table consumer today — listed so a new binding is a
deliberate row here, not a silent add: `vw_Chart_Consolidated`,
`vw_NakshatraPadaDetails`, `vw_Dignity_Legend`.

Spoken-for by the **planned** Key Inference page above, but not yet read by shipped code:
`vw_ChartShadbala` · `vw_ChartBhavaBala` · `vw_ChartYogaEvaluations` (CLI + evidence today) ·
`vw_ChartMoonContext` · `vw_ChartAshtakavarga` (BAV grid + SAV per sign; `db/074`) +
`tbl_Fact_AshtakavargaPinda` (`db/074`) — live once `KeyInference.razor` and the
`workstream/cli` Ashtakavarga engine land on `master`.
