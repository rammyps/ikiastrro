---
last_updated: 2026-09-17
reflects: PolarGridLagnaSelect implementation
component: PolarGridLagnaSelect
route: /key-inference/{id}?step=4 (embedded in Key Inference step "4. Spl Lagnas")
---

# Polar/grid Lagna selector

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

## Data contract

- Selected varga signs and graha placements come from persisted chart rows (`PlanetPlacements`).
- Lagna reference definitions come from tbl_Dim_HouseReference.
- Special-Lagna positions come from the chart's persisted SpecialLagna points.
- No Naisargika Karaka or Chara Karaka data is accepted or rendered.

## Visual contract

The polar (Wheel) view shows signs, **graha placements** (added 2026-09-17 — see below),
houses counted from the selected Lagna, and available special-Lagna abbreviations. The Grid
view uses the same selected Lagna as its ascendant reference and shows the same chart
placements. The former right-side interpretation/detail table is removed.

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