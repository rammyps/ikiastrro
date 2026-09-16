---
last_updated: 2026-09-17
reflects: PolarGridLagnaSelect implementation
component: PolarGridLagnaSelect
route: /key-inference/{id}?step=4 (embedded in Key Inference step "4. Spl Lagnas")
---

# Polar/grid Lagna selector

> Tab-level entry point (controls, layout, Special Lagnas table): [`specs_KI_spllagna.md`](specs_KI_spllagna.md).
> This file is the component-level spec for `PolarGridLagnaSelect` itself.

## Purpose

Present the selected divisional chart from one of five special-Lagna counting references without Karaka overlays or a detail table. The component supports two interchangeable chart views: Wheel and Grid.

## Page contract

- **No heading** (2026-09-17, rammyps's call) — the embedded `KarakaPolarWheel`'s own "Special
  Lagnas & Karakas" `<h1>` was dropped since the Key Inference step pill above it ("4. Spl
  Lagnas") already names the step and this is the only tab under it.
- The Chart `<select>` and the Wheel/Grid toggle are **centred together** on one row where the
  heading used to sit (not right-of-heading, since there is no heading).
- Both controls follow the app-wide "chart control" convention (`design-language.md` "Chart
  controls") — dark-navy pill, ALL CAPS text, sunset-filled active state — not the quiet-cream/
  sunset navigation-tab pill. `<option>` text is uppercased via CSS.
- The Lagna choices are checkbox-styled, multi-select controls (label reads "Sign Lagna" /
  "Arudha Lagna" / "Hora Lagna" / "Sree Lagna" / "Ghati Lagna" — display purposes only, the
  underlying reference codes are `LAGNA`/`ARUDHA_LAGNA`/`HORA_LAGNA`/`SREE_LAGNA`/`GHATI_LAGNA`)
  in this order:
  1. Sign Lagna (default, pre-checked)
  2. Arudha Lagna
  3. Hora Lagna
  4. Sree Lagna
  5. Ghati Lagna
- Only references available for the selected chart are rendered.
- Selecting a reference immediately recounts houses and changes the highlighted/reference Lagna in both views.
- **Grid-display checkboxes (2026-09-17), Grid view only** — a second checkbox-styled row, same
  visual convention as the Lagna-reference row above, rendered only when `View == "south"`. Five
  independent toggles, all pre-checked, not reset on chart change (unlike the Lagna references,
  which are chart-dependent):
  1. Upagrahas — Gulika/Maandi.
  2. Gra-Arudha — Graha Arudha.
  3. Arudha-Lagna — the AL tag. A distinct control from the "Arudha Lagna" checkbox in the Lagna
     reference row above (that one picks Arudha Lagna as this grid's own ascendant reference for
     its own overlay grid — a different concept from this display toggle).
  4. HN-Moon — the house-from-Moon badge.
  5. HN-Sun — the house-from-Sun badge.

## Data contract

- Selected varga signs and graha placements come from persisted chart rows (`PlanetPlacements`).
- Lagna reference definitions come from tbl_Dim_HouseReference.
- Special-Lagna positions come from the chart's persisted SpecialLagna points.
- **Grid view only (2026-09-17):** `KeyDetails` (`ChartKeyDetail` rows — Grahas, Upagrahas, Graha
  Arudha), `Aspects` (`ChartAspect` rows), and `NaisargikaKarakas`
  (`NaisargikaKarakaRepository.LoadActive().Primary`) are threaded straight through from the host
  page's already-loaded `WorkspaceData`/`NaisargikaKarakaRules` — no new repository or query.
  `PolarGridLagnaSelect` derives Sun's/Moon's own sign, the Gulika/Maandi/Graha-Arudha labels, the
  Naisargika/Chara Karaka tags (gated to D1/D9), and the `Ma(4)`-style aspect tags from these three
  parameters and hands them to `SouthIndianGrid_Micro` — full per-field mapping in
  [`spec_SouthIndianGrid_Micro.md`](spec_SouthIndianGrid_Micro.md).

## Visual contract

The polar (Wheel) view shows signs, **graha placements** (added 2026-09-17 — see below),
houses counted from the selected Lagna, and available special-Lagna abbreviations. The Grid
view uses the same selected Lagna as its ascendant reference and shows the same chart
placements, via **`SouthIndianGrid_Micro`** (2026-09-17 —
[`spec_SouthIndianGrid_Micro.md`](spec_SouthIndianGrid_Micro.md)), replacing the
`SouthIndianGrid_Detailed` this view used before. The former right-side interpretation/detail
table is removed.

**2026-09-17 fix — planets were missing from the polar view.** `PlanetPlacements` was always
passed into this component and even converted to `PlanetsBySign` for the Grid view's
`SouthIndianGrid_Detailed`, but the polar `<svg>` branch never referenced it at all — only sign
labels, house-count lines and special-Lagna markers were drawn. Fixed by rendering each
sector's planet glyphs (`ChartViewModel.PlanetGlyph`, dignity-coloured via
`ChartViewModel.DignityToken`/`--dignity-*`) at radius 260, between the sign ring (292) and the
special-Lagna house-count ring (224). Hit a Razor parsing gotcha this file already carries a
workaround for elsewhere (the `pgls-point` block): a literal SVG `<text>` as the first tag
inside an `@if(){ }` block, directly after a `var` statement, is parsed as Razor's own reserved
`<text>` markup-transition tag and can't carry attributes
(`RZ1023: "<text>" and "</text>" tags cannot contain attributes`) — wrapped in a `<g>` to fix,
same as `pgls-point` already does.