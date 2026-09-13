---
last_updated: 2026-09-13
workstream: ui
togaf: C — Component catalogue (proposal, not yet adopted)
reflects: pre-rename analysis for SouthIndianGrid_Detailed -> Grid_Normal_SouthIndStyle + new
  Grid_Micro / Grid_Macro modules. See ../architecture/flow.md for the data-flow trace this
  argues from. Nothing in this file has been executed yet — no git mv, no project_standards.md
  edit, no new component code.
---

# Chart/grid modules — Density x Style reclassification (proposal)

Parallel to [`../database/db_view_catalog.md`](../database/db_view_catalog.md) (which binds
**table** components to their view) but for the hand-rolled **chart/grid** components — and,
unlike that file, this one also proposes a naming/composition change, not just a binding.

## Why reclassify at all

Today `project_standards.md` §3.1 names a chart module on one axis only — a qualifier stating
*how it draws* (`_Detailed` = enriched CSS-grid, `_Mini` = thumbnail, `_DetailedSVG` = inline
SVG). That was enough while there was one look per chart type. It stops being enough once we
need **two independent axes**:

1. **Density** — how much of a chart's per-cell metric set is shown, and where: `Normal` /
   `Micro` / `Macro` (this conversation's terms; Micro = compact but drill-down-capable via
   tabs, Macro = high-level summary rollup).
2. **Layout style** — which classical box geometry: `SouthInd` (built) / `NorthInd` (deferred)
   / `WestInd` (deferred) — the same three options `MASTER.md` NFR-UI-02 already lists in the
   Preferences chart-style selector, just not yet wired to component-level variants.

These two axes are independent: a Micro-density South-Indian grid and a Normal-density
South-Indian grid draw the *same* fixed 4x4 sign layout, just with a different amount of
content per cell; a Normal-density North-Indian grid and a Normal-density South-Indian grid
show the *same* metric set, just in a different box geometry (diamond vs. fixed grid).
`PolarWheel` is a third, separate family — a 360-degree ring is not an "Indian style" at all,
so it sits outside the Style axis entirely (see below).

## Proposed naming (extends §3.1, does not replace it)

```
Grid_<Density>_<Style>Style.razor
```

| Axis | Values | Status |
|---|---|---|
| `<Density>` | `Normal` (today's `_Detailed`) · `Micro` · `Macro` | Normal built; Micro/Macro proposed |
| `<Style>` | `SouthInd` · `NorthInd` · `WestInd` | SouthInd built; North/West deferred per `MASTER.md` NFR-UI-02, same as the page-level chart-style selector |

So: `Grid_Normal_SouthIndStyle` (renamed from `SouthIndianGrid_Detailed`),
`Grid_Micro_SouthIndStyle`, `Grid_Macro_SouthIndStyle` now; `Grid_Normal_NorthIndStyle` etc.
later, once a North-Indian geometry is actually built (unchanged from today: North/West stay
disabled selections until then).

`PolarWheel` keeps its own name — it is not `Grid_*` at all (no sign-position layout to vary by
Style). If it ever needs its own density tiers, that's `PolarWheel_Micro` /
`PolarWheel_Macro`, independent of this table.

## Recommended composition (the actual "ideal way" this asks for)

**Do not build 9 monolithic components** (3 density x 3 style, each duplicating the other's
cell logic). That inflates the existing §3.3 "new module = new file set = new golden" rule into
9x the maintenance for what is really a 3+3 problem. Instead, factor along the same seam
`ChartFrame` already uses to compose grid <-> wheel:

- **One geometry provider per Style** — pure cell-position math, no content awareness. Today
  this already exists *inside* `SouthIndianGrid_Detailed` as the private `GridCells` table (12
  fixed `(sign, col, row)` tuples). Proposal: extract it to a shared, density-agnostic piece
  (e.g. `SouthIndGridGeometry`) so `NorthIndGridGeometry` / `WestIndGridGeometry` can be added
  later without touching density logic at all.
- **One cell-content template per Density** — given a sign's planets/badges/metrics, renders
  what that density tier shows in a cell (and, for Micro, what its tabs expose). Density-aware,
  style-agnostic: the same `Grid_MicroCellContent` renders inside a South-Indian box or a
  future North-Indian diamond without change.
- **The consumer-facing `Grid_<Density>_<Style>Style.razor`** is then a thin composition of the
  two (style geometry + density content), the way `ChartFrame` is a thin toggle around a grid
  fragment and a wheel fragment today — not a from-scratch reimplementation.

This keeps the golden-snapshot revert story intact (§3.3): each of the 9 *possible* combinations
still gets its own golden SVG when it's actually built, but the code behind it is geometry x
content, not 9 independent implementations.

## Density tiers x the 5 per-cell metric categories

Mapping each category from the earlier chart-metrics inventory to (a) its Fact/Dim/Rule source
and (b) where each density tier proposes to show it. **Normal = what `SouthIndianGrid_Detailed`
already ships today** (checked = built and visible now); **Micro/Macro = proposed**, not yet
built — flag any change here before building against it.

| # | Category | Source table(s) | Normal (today, in cell / in center) | Micro (proposed) | Macro (proposed) |
|---|---|---|---|---|---|
| 1 | Lagna & special Lagnas | `tbl_Chart_KeyDetails` (`PointKind='Graha'` Ascendant row); AL/A2-A12 + HL rows `PointKind IN ('Arudha','SpecialLagna')` | ✅ Ascendant sign drives the whole layout + gold house-from-Lagna badge, **in cell**; Lagna sign name **in center** (`CenterMeta`) | proposed: dedicated "Lagna" tab — AL/HL/A2-A12 rows, **in tab**, not cell | proposed: Lagna sign + AL only, **in center summary line** |
| 2 | Dignity / dignity strength | `tbl_Chart_KeyDetails.DignityStatus` (+ `OwnSigns`/`ExaltationSign`/etc.); Shadbala/Bhava Bala are `tbl_Fact_PlanetaryStrength*` / `tbl_Fact_BhavaStrength*` — **not read by any grid today** | ✅ dignity dot per glyph via `PlanetChip`, **in cell**; nothing from Shadbala/Bhava Bala | proposed: dignity dot **in cell** (unchanged) + a "Strength" tab surfacing Shadbala/Bhava Bala numbers not shown anywhere in a grid today | proposed: no per-planet dignity in cell; one aggregate "chart strength" indicator **in center** only |
| 3 | Conjunction / Aspect / Combustion | `tbl_Chart_Conjunctions`(+Multi), `tbl_Chart_Aspects`, `tbl_Chart_KeyDetails.IsCombust`/`AspectingPlanets` | ✅ combust icon on glyph + retrograde `(R)`, **in cell**; "aspected by" ghost-chip strip at cell foot, **in cell** | proposed: same in-cell icons (unchanged) + a dedicated "Aspects & Combustion" tab with full separation/orb numbers | proposed: a single count badge ("2 combust, 3 aspects") **in center**, no per-planet detail |
| 4 | Upagrahas | `tbl_Chart_KeyDetails` (`PointKind='Upagraha'`) — only Gulika/Maandi populated today; the other 9 are `tbl_Rule_Sub*` reference data with no engine (see `flow.md`) | not shown today (no `SpecialPointLabels` param wired for Upagrahas in `AllCharts.razor`'s usage, though the component supports it) | proposed: "Upagrahas" tab, Gulika/Maandi now, the other 9 the moment `SubPlanetCalculator` is wired into the pipeline | proposed: omitted entirely at Macro density |
| 5 | Others (Nakshatra/pada, Chara Karaka, Vargottama, Baadhaka, house-from-Sun/Moon, functional Lagna nature) | `tbl_Chart_KeyDetails` (Nakshatra*, CharaKaraka, HouseNumberFrom*), `tbl_Fact_Vargottama` | ✅ house-from-Moon silver badge only, **in cell**; nothing else from this row shown in any grid | proposed: "Details" tab — Nakshatra/pada, Chara Karaka, Vargottama flag, house-from-Sun | proposed: none — Macro stays sign + Lagna + aggregate counts only |

## Open decisions before any of this is built

1. **Confirm density tier names** — `Normal` / `Micro` / `Macro` used throughout this doc per
   this conversation; not yet written into `project_standards.md`.
2. **Confirm the Micro tab set** — the "Aspects & Combustion" / "Strength" / "Upagrahas" /
   "Details" grouping above is a first-cut proposal from the metric inventory, not a decision.
3. **View vs. direct Fact read** — per `flow.md`'s finding: recommend `Grid_Micro`/`Grid_Macro`
   read new `vw_Chart_GridMicro` / `vw_Chart_GridMacro` views (registered in
   `db_view_catalog.md`) rather than raw `tbl_Chart_*`, since their column needs genuinely
   differ from Normal's; `Grid_Normal` keeps its current direct-Fact-read path unchanged.
4. **Execution order** — none of the rename or new-component work has started. When it does,
   it follows project_standards.md §3.4 (rename) / §3.2 (new module file set) as-is; this doc
   only adds the Density axis on top.

## Cross-references

- Data-flow grounding: [`../../architecture/flow.md`](../../architecture/flow.md)
- Current (pre-rename) catalogue row: [`chart-catalog.md`](chart-catalog.md)
- Current (pre-rename) spec: [`spec_SouthIndianGrid_Detailed.md`](spec_SouthIndianGrid_Detailed.md)
- Naming rule to be extended: [`../../../project_standards.md`](../../../project_standards.md) § 3
- Chart-style selector this Style axis matches: `docs/ui/MASTER.md` NFR-UI-02
