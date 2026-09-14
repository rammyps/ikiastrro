---
last_updated: 2026-09-14
workstream: ui
togaf: C — component catalogue
---

# Component — hand-rolled chart catalogue

All chart diagrams are hand-drawn inline SVG / CSS grid — no charting library. Source lives in
`src/Ikiastrro.Web/Components/Charts/`; that folder's `README.md` holds the per-component
projection contract (viewBox, geometry math, tokens).

**Naming & versioning:** chart templates, their spec docs and their code files follow the
project chart-module convention in [`../../../project_standards.md`](../../../project_standards.md)
(§ "Chart modules"). A new look is a **new suffixed name + new spec doc**, never a rewrite in
place — so every version stays documented and revertable.

## Catalogue — chart / spec doc / linked files

Paths are repo-relative. `.razor` implies a sibling `.razor.css` isolation file unless noted.

### Visual chart modules (inline SVG / CSS grid)

| Chart / component                                                                                                                                                           | Spec doc                                                                                    | Linked files                                                                                                                                                                                                                                                |
| --------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **`SouthIndianGrid_Detailed`** — enriched 4×4 sign grid, every chart type (`PlanetChip` glyphs, dual house badges, aspect strip, `SpecialPointLabels`)                      | [`spec_SouthIndianGrid_Detailed.md`](spec_SouthIndianGrid_Detailed.md)                      | `src/Ikiastrro.Web/Components/Charts/SouthIndianGrid_Detailed.razor` · `…/GridPlanetGlyph.cs` (cell model) · golden `docs/artifacts/ui/SouthIndianGrid_Detailed-sample.svg` · consumers `Pages/AllCharts.razor`, `Pages/VargaView.razor` (via `ChartFrame`) |
| **`PolarWheel`** — 360° sidereal longitude ring, one glyph per graha, optional aspect chords                                                                                | [`spec_SouthIndianGrid_Detailed.md`](spec_SouthIndianGrid_Detailed.md) (grid ⇄ wheel pair)  | `src/Ikiastrro.Web/Components/Charts/PolarWheel.razor` · golden `docs/artifacts/ui/PolarWheel-sample.svg` · consumer `Pages/VargaView.razor` (via `ChartFrame` wheel view)                                                                                  |
| **`KarakaPolarWheelChart`** — 12-house natural-karaka wheel with selected-varga signs and Chara Karaka placements | [`spec_KarakaPolarWheelChart.md`](spec_KarakaPolarWheelChart.md) | `src/Ikiastrro.Web/Components/Charts/KarakaPolarWheelChart.razor` · repository `src/Ikiastrro.Data/NaisargikaKarakaRepository.cs` · golden `docs/artifacts/ui/KarakaPolarWheelChart-sample.svg` · consumer `Pages/KarakaPolarWheel.razor` |
| **`Natal_Transit_Comp_WheelChart`** — four-ring natal ↔ current-transit zodiac wheel (`viewBox 0 0 800 800`, data-driven `NatalPoints` / `TransitPoints` / `AscendantSign`) | [`spec_Natal_Transit_Comp_Wheel.md`](spec_Natal_Transit_Comp_Wheel.md)                      | `src/Ikiastrro.Web/Components/Charts/Natal_Transit_Comp_WheelChart.razor` · golden ⚠️ **not yet minted** (`docs/artifacts/ui/Natal_Transit_Comp_WheelChart-sample.svg`) · consumer `Pages/Natal_Transit_Comp_Wheel.razor`                                   |
| **`D1TemplateGrid`** — dense print-style D1 template (karaka header row, `Dg(±N)` dignity scores, `ASP:` + upagraha rows, own `--tmpl-*` light palette)                     | [`spec_SouthIndianGrid_Detailed.md`](spec_SouthIndianGrid_Detailed.md) (§ template page)    | `src/Ikiastrro.Web/Components/Charts/D1TemplateGrid.razor` · `…/D1TemplateCellData.cs` (`TemplatePlanetLine` / `TemplatePointLine` / `TemplateDignityMode`) · consumer `Pages/SouthIndianTemplate.razor`                                                    |
| **`MiniGrid`** — glyphs-only thumbnail grid, whole grid optionally a link                                                                                                   | [`spec_SouthIndianGrid_Detailed.md`](spec_SouthIndianGrid_Detailed.md) (§ derived variants) | `src/Ikiastrro.Web/Components/Charts/MiniGrid.razor` · golden `docs/artifacts/ui/MiniGrid-sample.svg` · consumer `Components/Workspace/VargaRail.razor`                                                                                                     |
| **`ChartFrame`** — bookmarkable `?view=grid\|wheel` toggle around a grid + a wheel fragment (not a chart)                                                                   | [`spec_SouthIndianGrid_Detailed.md`](spec_SouthIndianGrid_Detailed.md)                      | `src/Ikiastrro.Web/Components/Charts/ChartFrame.razor` · renders `<SegmentedToggle>` · golden `docs/artifacts/ui/ChartFrame-sample.svg` · consumer `Pages/VargaView.razor`                                                                                  |
| **`VargottamaStrip`** — graha chips lit when Dn sign == D1 sign                                                                                                             | [`spec_SouthIndianGrid_Detailed.md`](spec_SouthIndianGrid_Detailed.md)                      | `src/Ikiastrro.Web/Components/Charts/VargottamaStrip.razor` · golden `docs/artifacts/ui/VargottamaStrip-sample.svg` · consumer `Pages/VargaView.razor`                                                                                                      |
| **`LifeWeeks`** — 4000-week grid (52 col/row), `--dasha-*` colours, hover date/lord tooltips                                                                                | — (route only; retired in v2)                                                               | `src/Ikiastrro.Web/Components/Pages/LifeWeeks.razor` (page, not a `Charts/` component) · uses `DashaLegend`                                                                                                                                                 |

### Dasha / timeline modules

| Chart / component | Spec doc | Linked files |
|---|---|---|
| **`DashaTimeline`** — Vimśottari 3-level tree; `Compact` Mahādaśā-only mode; opens the current chain on first render (only stateful chart module) | [`dasha-sade-sati.md`](dasha-sade-sati.md) | `src/Ikiastrro.Web/Components/Charts/DashaTimeline.razor` · consumer `Pages/Timing.razor` |
| **`DashaLegend`** + **`DashaLordColors.cs`** — 9-hue Vimśottari lord swatches | [`dasha-sade-sati.md`](dasha-sade-sati.md) | `src/Ikiastrro.Web/Components/Charts/DashaLegend.razor` · `…/DashaLordColors.cs` · consumers `Charts/DashaTimeline.razor`, `Pages/LifeWeeks.razor` |
| **`DashaRoundBox`** — one Sade-Sati / Kaṇṭaka round box | [`dasha-sade-sati.md`](dasha-sade-sati.md) | `src/Ikiastrro.Web/Components/Charts/DashaRoundBox.razor` · consumer `Pages/SouthIndianTemplate.razor` |
| **`SadeSatiTable`** — merged Saturn-from-Moon affliction windows, date-ordered | [`dasha-sade-sati.md`](dasha-sade-sati.md) | `src/Ikiastrro.Web/Components/Charts/SadeSatiTable.razor` · source `tvf_Chart_SadeSatiPeriods` ([`../../database/db_view_catalog.md`](../../database/db_view_catalog.md)) · consumer `Pages/Timing.razor` |
| **`GocharaPanel`** — current transit sign + since / next-change | [`dasha-sade-sati.md`](dasha-sade-sati.md) | `src/Ikiastrro.Web/Components/Charts/GocharaPanel.razor` · source `tbl_PlanetSignTransitEvents` / `tvf_PlanetSignAtDate` ([`../../database/db_view_catalog.md`](../../database/db_view_catalog.md)) · consumer `Pages/Timing.razor` |

### UI tables (MudBlazor + tokens — persisted rows, each bound to a DB view)

Every table below reads a persisted view/TVF; the component ⇄ view binding is documented in
[`../../database/db_view_catalog.md`](../../database/db_view_catalog.md).

| Chart / component | Spec doc | Linked files |
|---|---|---|
| **`PlanetPositionsTable`** — D1 reference table | [`evidence-tables.md`](evidence-tables.md) | `src/Ikiastrro.Web/Components/Charts/PlanetPositionsTable.razor` · view `vw_ChartPlanetEvidence` · consumer `Pages/VargaView.razor` |
| **`HouseLordshipTable`** — per-varga house-lord disclosure | [`evidence-tables.md`](evidence-tables.md) | `src/Ikiastrro.Web/Components/Charts/HouseLordshipTable.razor` · consumer `Pages/VargaView.razor` |
| **`ConjunctionsTable`** — per-varga conjunctions | [`evidence-tables.md`](evidence-tables.md) | `src/Ikiastrro.Web/Components/Charts/ConjunctionsTable.razor` · consumer `Pages/VargaView.razor` |
| Transit landing tables — **D1 Birth** / **Current Transit** (two `MudTabPanel`s on the wheel page) | [`spec_Natal_Transit_Comp_Wheel.md`](spec_Natal_Transit_Comp_Wheel.md) | `src/Ikiastrro.Web/Components/Pages/Natal_Transit_Comp_Wheel.razor` · `src/Ikiastrro.Web/Natal_Transit_Comp_WheelMath.cs` (house/motion math) · `src/Ikiastrro.Data/Natal_Transit_Comp_WheelRepository.cs` · tests `tests/Ikiastrro.Web.Tests/Natal_Transit_Comp_WheelMathTests.cs` · views `vw_ChartPlanetEvidence`, `tbl_TransitPositionReference` (via `GocharaRepository`) |
| **`PlanetStrengthChart`** — Key Inference 3.1, Shadbala rank/bar table (Performance %-of-minimum or Composition stacked-Bala bar) + per-graha expandable component breakdown | [`key-inference.md`](key-inference.md) | `src/Ikiastrro.Web/Components/Charts/PlanetStrengthChart.razor` · view `vw_ChartShadbala` + `tbl_Fact_PlanetaryStrengthComponent` (via `PlanetaryStrengthRepository.GetSummaryByBirthDetailId`/`GetComponentsByBirthDetailId`) · consumer `Pages/KeyInference.razor` |
| **`HouseStrengthChart`** — Key Inference 3.2, Bhava Bala rank/bar table (House order/Strength rank toggle) + per-house expandable component breakdown | [`key-inference.md`](key-inference.md) | `src/Ikiastrro.Web/Components/Charts/HouseStrengthChart.razor` · view `vw_ChartBhavaBala` + `tbl_Fact_BhavaStrengthComponent` (via `BhavaStrengthRepository.GetSummaryByBirthDetailId`/`GetComponentsByBirthDetailId`) · consumer `Pages/KeyInference.razor` |

### Shared helpers

| Helper | Role | File |
|---|---|---|
| `ChartViewModel.PlanetGlyph(string)` | canonical glyph for a planet name — every glyphs-only module | `src/Ikiastrro.Core/Presentation/ChartViewModel.cs` |
| `GridPlanetGlyph` (record + component) | one dignity-dot glyph inside a `SouthIndianGrid_Detailed` cell | `src/Ikiastrro.Web/Components/Charts/GridPlanetGlyph.cs` |
| `AstroMath.CountFromSignToSign` | whole-sign house count — `SouthIndianGrid_Detailed`, `D1TemplateGrid` | `src/Ikiastrro.Core/Engines/Astronomy/` |

## Golden-snapshot flow

Every visual component commits `docs/artifacts/ui/<Component>-sample.svg`, rendered from one
fixed fixture (never changed) so a diff is a pure rendering change. Harness:
`tests/Ikiastrro.Web.Tests` (bUnit `ChartSnapshotTests` + `ChartFixture` + `SnapshotAssert`),
run from VS Test Explorer or `dotnet test`. Mint / update with env
`IKIASTRRO_UPDATE_SNAPSHOTS=1`; review the SVG diff before committing. Full flow:
[`../../artifacts/ui/README.md`](../../artifacts/ui/README.md).

Outstanding: `Natal_Transit_Comp_WheelChart` has **no golden yet** — add a `[Fact]` to
`ChartSnapshotTests` and mint one.

## Revert

`git checkout <ref> -- src/Ikiastrro.Web/Components/Charts/<C>.razor <C>.razor.css`, render,
diff against `git show <ref>:docs/artifacts/ui/<C>-sample.svg`. Byte-match ⇒ done; mismatch ⇒
a token or geometry helper moved — reconcile those (they version by addition, so this is rare).
