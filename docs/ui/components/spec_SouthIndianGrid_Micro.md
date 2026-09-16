---
last_updated: 2026-09-17
workstream: ui
component: SouthIndianGrid_Micro
route: /key-inference/{id} (embedded, step "4. Spl Lagnas" Grid view — see specs_KI_spllagna.md)
togaf: C — component spec (built)
catalogued_in: chart-catalog.md
---

# Specification — SouthIndianGrid_Micro

> Sibling of [`SouthIndianGrid_Detailed`](spec_SouthIndianGrid_Detailed.md) — same fixed 4×4
> sign-position geometry, denser per-cell content. New module per
> [`../../../project_standards.md`](../../../project_standards.md) §3.3 ("a new visual treatment
> of an existing chart module is a new module with its own suffixed name, never a rewrite in
> place"): `SouthIndianGrid_Detailed` is untouched and keeps every other consumer (`AllCharts`,
> `VargaView`, the D1/D9 `SouthIndianTemplate`). This component has exactly **one** consumer: the
> Grid view of `PolarGridLagnaSelect`, embedded in Key Inference step "4. Spl Lagnas" — tab-level
> contract in [`specs_KI_spllagna.md`](specs_KI_spllagna.md); component-level contract for
> `PolarGridLagnaSelect` itself (the Chart select / Wheel-Grid toggle / Lagna checkboxes / the
> new Grid-display checkboxes below) stays in
> [`spec_KarakaPolarWheelChart.md`](spec_KarakaPolarWheelChart.md).
>
> Related prior art: [`view-grid.md`](view-grid.md)'s Micro-density proposal. This component is a
> concrete, narrower first build of that idea (one consumer, South-Indian style only), not the
> general 3-density × 3-style system that doc proposes.

## Why a separate module

The Detailed grid's cell content was sized for one generic reading (planets + house badges +
occasional aspect). Step 4 needs a single cell to carry the regular planets *and* Gulika/Maandi
*and* Graha Arudha *and* Arudha Lagna *and*, for D1/D9, two independent karaka tags per planet —
several times denser than Detailed was ever designed to hold. Rather than overload Detailed with
a density switch, §3.3 calls for a new named module with its own spec and its own golden.

## Geometry (unchanged from Detailed)

Same fixed 4×4 sign grid, same `AscendantSign`-driven house-from-Lagna numbering
(`AstroMath.CountFromSignToSign`). No Sanskrit sign name (Micro drops it — already dense enough
without it). Only per-cell *content* rules differ, below.

## Per-cell content — differences from `SouthIndianGrid_Detailed`

- **No "one planet per row" constraint.** Detailed's cells were only ever occupied by 0–2 planets
  in practice; a Micro cell routinely holds many more entries (planets, Gulika/Maandi, Graha
  Arudha, Arudha Lagna, karaka tags, aspect tags) and must wrap several onto the same line.
- **No chip background fill.** Neither `PlanetChip` nor its `--planet-<name>-bg` fill is used —
  the glyph is plain text (`ChartViewModel.PlanetGlyph`), not the chip component. The dignity dot
  is unchanged from Detailed (still rendered, still `--dignity-<token>` coloured).
- **Direction convention changes.** Detailed renders the glyph plus a separate `(D)`/`(R)` suffix
  span. Micro drops the suffix: a **direct** planet is the bare glyph (`Ju`); a **retrograde**
  planet is the whole glyph parenthesized (`(Ju)`).
- **Gulika and Maandi added** as first-class entries in their sign's cell — same visual slot as a
  planet glyph, not a corner label. Source: `ChartKeyDetail` rows where `PointKind == "Upagraha"`
  (the only two upagrahas with a working calculator today — `UpagrahaCalculator`; the other 9
  have no engine yet per `view-grid.md`), short-coded `"Gulika"→"Gk"` / `"Maandi"→"Md"` — the same
  switch already used by `VargaView.razor`/`SouthIndianTemplate.razor`.
- **Graha Arudha added** — per-sign label(s) from `ChartKeyDetail` rows where
  `PointKind == "GrahaArudha"` (computed by `GrahaArudhaCalculator`, PVR §9.5; code format
  `GA_<Planet>`), rendered as `GA-<glyph>` (e.g. `GA-Su`) — kept visually distinct from the
  house-Arudha codes (`A1`..`A12`/`AL`).
- **Arudha Lagna** — the existing `AL` label, carried over unchanged; called out here only because
  it now shares a row with Graha Arudha (see row grouping below) and has its own independent
  show/hide toggle (below) rather than always rendering.
- **Karakas added, both schemes, attached to each planet's own glyph** (not a separate row):
  - *Naisargika Karaka (basis 9)* — the fixed natural karakatva per graha. Source:
    `NaisargikaKarakaRepository.LoadActive().Primary` (already queried on this page as
    `KarakaPolarWheel.razor`'s `_rules`, just not previously threaded down) — 12 rows,
    HouseNumber → primary Graha; grouped the other way (by Graha) for this tag, formatted `NK<house
    numbers>` (e.g. `NK1`, or `NK3,6` when a graha is primary for more than one house).
  - *Chara Karaka* — read directly off `ChartKeyDetail.CharaKaraka` (already persisted per graha,
    every chart type — no calculator call needed in the UI layer). Codes `AK`..`DK`, **shown only
    where applicable** — the column is `NULL` for Ketu (and the Ascendant/special points), so
    nothing renders for Ketu, matching `CharaKarakaCalculator.Ranked` never including it.
  - **Both karaka tags render only when the active chart is D1 or D9.** Every other divisional
    chart in this tab still shows planets/Gulika/Maandi/Graha Arudha/aspects as above, but with no
    karaka tags — a deliberate density/scope call for this tab, not a data limitation (both are
    chart-invariant facts).
- **Aspects added, inline in the cell** — Detailed's dedicated dashed "Aspected by" strip at the
  cell foot is not reused here. Instead, a cell whose occupant receives a graha's special aspect
  (Drishti) gets a compact `<PlanetAbbrev>(<N>)` tag, where `N` is that aspect's house-count
  distance from the aspecting planet's own placement. Example: Mars sitting in house 1 casts its
  4th-house special aspect onto Saturn's cell (house 4) → that cell shows `Ma(4)`. Source:
  `ChartViewModel.BuildAspectedByMicro(keyDetails, aspects)` — a third formatter alongside
  `FormatAspectChip`/`FormatAspectPlain`, reusing the same `BuildAspectedBy` grouping helper
  Detailed's strip already uses.

## House-number badges (extends Detailed's Lagna/Moon pair)

Same corner badge stack as Detailed (`--house-lagna` gold, always shown), plus:

- **House from Moon** (`--house-moon`) — same as Detailed, but here gated by a **"HN-Moon"**
  checkbox (below) rather than simply whether a `MoonSign` was supplied.
- **House from Sun** (`--house-sun`/`--house-sun-fg`, new additive tokens in `tokens.css`) — a
  third badge Detailed has no concept of, gated by an **"HN-Sun"** checkbox. Both Sun's and
  Moon's own sign are read straight off the caller's `KeyDetails` (`Planet == "Sun"/"Moon"`) — no
  new parameter needed beyond `SunSign`/`MoonSign` themselves, which are simply `null` when their
  checkbox is off (same null-gating convention `MoonSign` already used in Detailed).

## Grid-display checkboxes (in `PolarGridLagnaSelect`, not this component)

Five of this cell content's pieces are independently toggleable from a checkbox row rendered only
in Grid view — full behaviour documented in
[`spec_KarakaPolarWheelChart.md`](spec_KarakaPolarWheelChart.md), since the toggles live one layer
up. This component itself only knows "empty dictionary / null sign" vs. "populated" — it has no
checkbox awareness of its own:

| Checkbox | Controls |
|---|---|
| Upagrahas | Gulika/Maandi row |
| Gra-Arudha | Graha Arudha entries in the arudha row |
| Arudha-Lagna | AL entry in the arudha row |
| HN-Moon | house-from-Moon badge |
| HN-Sun | house-from-Sun badge |

All five default on. Not the same control as the existing "Arudha Lagna" checkbox in the Lagna
reference group (that one picks Arudha Lagna as a *reference sign* for its own overlay grid — a
different concept from this display toggle).

## Layout rules

- **No overlaps.** Every optional row (`.row.planets`/`.row.upagrahas`/`.row.arudhas`/
  `.row.aspects`) is a normal-flow flex child, never absolutely positioned — only the corner LAGNA
  tag and the house-number badge stack are absolutely positioned, unchanged from Detailed. This is
  what guarantees no collision: rows wrap and the cell grows instead of anything clipping.
- **Preferred row grouping (soft rule, overridable under space pressure).** When a cell has room,
  entries group into up to 4 separate rows, in this order:
  1. Planets — glyph + direction convention + attached karaka tag(s).
  2. Upagrahas — Gulika, Maandi.
  3. Graha Arudha + Arudha Lagna, together (independently toggleable, same visual row).
  4. Aspects — the `Ma(4)`-style tags.

  Each row wraps independently (`flex-wrap: wrap`) — in a crowded cell this is what lets rows
  squeeze/wrap on their own without extra layout logic; density wins over the grouping when the
  two conflict, exactly as the spec calls for.

## Rendering rules

Inline CSS grid, CSS-isolated (`SouthIndianGrid_Micro.razor.css`), same `tokens.css` custom
properties as `SouthIndianGrid_Detailed` (`--paper`/`--ink`/`--dignity-*`/etc.) plus the two new
`--house-sun`/`--house-sun-fg` tokens. Golden snapshot:
`docs/artifacts/ui/SouthIndianGrid_Micro-sample.svg`, `[Fact] SouthIndianGrid_Micro()` in
`ChartSnapshotTests`, fixture data in `ChartFixture.cs` (`MicroGridGlyphs`/`MicroUpagrahaLabels`/
`MicroGrahaArudhaLabels`/`MicroArudhaLagnaLabels`/`MicroAspectLabels`).

## Files

`src/Ikiastrro.Web/Components/Charts/SouthIndianGrid_Micro.razor` (+ `.razor.css`) ·
`…/MicroPlanetGlyph.cs` (cell model, sibling of `GridPlanetGlyph.cs`) · consumer
`Charts/PolarGridLagnaSelect.razor` (Grid view).
