---
last_updated: 2026-10-04
workstream: ui
component: SindUniDtlGrid (SIND-UNI-DTL) · SouthIndianTemplate
route: /charts/{id}/south-indian-template
togaf: C — component spec
catalogued_in: chart-catalog.md
---

# Specification — SIND-UNI-DTL (`SindUniDtlGrid`)

> Living specification for `SindUniDtlGrid`, the detailed South Indian grid, and the
> `SouthIndianTemplate` page. Renamed from `SouthIndianGrid_Detailed` on 2026-10-04; the old
> content-driven (non-square) layout and its spec are gone. The charts are to be unified under one
> family later: see [`spec_SIND-UNI_GridChart.md`](spec_SIND-UNI_GridChart.md).
> Catalogued in [`chart-catalog.md`](chart-catalog.md). Per-component projection math:
> `src/Ikiastrro.Web/Components/Charts/README.md`. Naming / versioning:
> [`../../../project_standards.md`](../../../project_standards.md) § 3.

## `SindUniDtlGrid.razor`

**The master chart (rammyps, 2026-10-04).** The one South Indian grid that follows all three themes
through the global tokens, and the one to use wherever placements are compared. Used on: Astro Facts
natal step, All Charts cards (`Compact`), Varga view, and the Compatibility page's "Charts: D1 and D9"
tab (each person's D1 and D9 one above the other, groom left, bride right, so the same sign sits in
the same place in both columns). SIND-UNI-2 was not adopted for it.

The fixed 4×4 sign-position grid, one component for every chart type (D1…D60).

### Geometry

- **Always a perfect square**: `aspect-ratio: 1`, four equal rows and columns, max 480px, width from
  the container. Cells never grow with content; a cell that is too full clips instead.
- **Type scales with the chart**: the chart is a size container and every size is in container units
  (`cqi`), each with a small pixel floor (`max(Npx, Xcqi)`) so gallery-size cards stay legible.

### Per cell

- Sign name (English, full, e.g. "Aries"). No Sanskrit name, no LAGNA tag; the Lagna cell is tinted
  (`--asc-glow` over `--paper-raised`) with an inset accent ring.
- **Two house-number badges side by side, top-right:** `--house-lagna` (gold) = house from the
  chart's Lagna; `--house-moon` (silver) = house from that chart's own natal Moon sign (`MoonSign`
  param; omitted when not supplied). Both via `AstroMath.CountFromSignToSign`.
- **One chip per planet, planet and direction combined:** `Ju` when direct, `(Ju)` when retrograde;
  no separate `(D)`/`(R)` suffix. Dignity dot (`--dignity-*`) and the combust flame stay on the chip.
  Several planets share a row and the row wraps inside the cell. Tooltip names the planet and the
  direction.
- `SpecialPointLabels`: AL / Arudha / HL / upagraha codes in a small strip under the sign name.
- "Asp" strip at the cell foot (hidden when `Compact`): dashed ghost chips of the planets casting an
  aspect into the sign (`--aspect-faint`), only on cells receiving an aspect.
- Centre 2×2: chart title + `CenterMeta` (Lagna sign, Moon sign, Moon's nakṣatra).

### Parameters

`AscendantSign`, `MoonSign?`, `PlanetsBySign`, `CenterTitle`, `CenterMeta` (render fragment),
`Compact` (drops the aspect strips), `AspectedByGlyphs`, `SpecialPointLabels`.

Derived variants: `MiniGrid` (glyphs-only thumbnail), and the `ChartFrame` grid⇄wheel toggle that
swaps it with `PolarWheel`.

## `SouthIndianTemplate` page (`/charts/{id}/south-indian-template`)

The print-style single-chart template — one large grid for D1 with a light/dark toggle, the Chara
Karaka strip, and the chart's identity/method line. It uses its own `D1TemplateGrid`, not this
component. One of the four surfaces kept in the current UI (`wkstream_UI_v1`).

## Rendering rules

CSS grid, CSS-isolated, all colour from `tokens.css` (`--paper-raised`, `--paper-line`,
`--accent-line`, `--ink-*`, the dignity ramp). Golden snapshot
`docs/artifacts/ui/SindUniDtlGrid-sample.svg`. Geometry helpers version by addition
(`docs/ui/design-language.md`). North-Indian style is researched, not built.
