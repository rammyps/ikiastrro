---
last_updated: 2026-09-10
workstream: ui
component: SouthIndianGrid_Detailed · SouthIndianTemplate
route: /charts/{id}/south-indian-template
togaf: C — component spec
catalogued_in: chart-catalog.md
---

# Specification — SouthIndianGrid_Detailed (grid + template)

> Living specification for `SouthIndianGrid_Detailed` and the `SouthIndianTemplate` page.
> Catalogued in [`chart-catalog.md`](chart-catalog.md) (`SouthIndianGrid_Detailed`, `MiniGrid`,
> `D1TemplateGrid`, `ChartFrame` rows). Per-component projection math:
> `src/Ikiastrro.Web/Components/Charts/README.md`. Naming / versioning:
> [`../../../project_standards.md`](../../../project_standards.md) § 3.

## `SouthIndianGrid_Detailed.razor`

The fixed 4×4 sign-position grid, one component for every chart type (D1…D60). Per cell:

- Sign name (full, e.g. "Aries"), Sanskrit name below (hidden in Compact).
- **Two stacked house-number badges, top-right:** `--house-lagna` (gold) = house from the
  chart's Lagna; `--house-moon` (silver) = house from that chart's own natal Moon sign
  (`MoonSign` param; omitted when not supplied). Both via `AstroMath.CountFromSignToSign`.
- Planet glyphs (`Su` / `Mo` / …) via `PlanetChip`: dignity dot + `(D)` / `(R)` suffix +
  🔥 combust icon.
- "Aspected by" strip at the cell foot — dashed ghost chips in `Ma(a)-8` notation
  (`--aspect-faint`), only on cells receiving an aspect.
- `SpecialPointLabels` param renders AL / Arudha / HL / upagraha labels.
- Centre 2×2: chart title + `CenterMeta` (Lagna sign, Moon sign, Moon's nakṣatra).

Derived variants: `MiniGrid` (glyphs-only thumbnail), and the `ChartFrame` grid⇄wheel toggle
that swaps it with `PolarWheel`.

## `SouthIndianTemplate` page (`/charts/{id}/south-indian-template`)

The print-style single-chart template — one large `SouthIndianGrid_Detailed` for D1 with a
light/dark toggle, the Chara Karaka strip, and the chart's identity/method line. One of the
four surfaces kept in the current UI (`wkstream_UI_v1`).

## Rendering rules

Inline SVG / CSS grid, CSS-isolated, all colour from `tokens.css` (`--cell-fill`,
`--lagna-fill`, `--grid-stroke`, `--sign-text`, `--muted-text`, the dignity ramp). Golden
snapshot `docs/artifacts/ui/SouthIndianGrid_Detailed-sample.svg`. Geometry helpers version by addition
(`docs/ui/design-language.md`). North-Indian style is researched, not built.
