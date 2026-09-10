---
last_updated: 2026-09-09
workstream: ui
togaf: C — UI standards
---

# UI — design language

One language, everywhere. Detail on colours/type: [`brand.md`](brand.md).

## The rules

- **MudBlazor is the component system.** Light theme. Every page uses `MudLayout` /
  `MudAppBar` / `MudMainContent` and MudBlazor controls for chrome, forms, tables, dialogs.
- **Tokens, not raw values.** `wwwroot/css/tokens.css` holds real `:root` custom properties
  (the `--brand-*` set + semantic astrology tokens). Components read them via `var(--…)` —
  never a raw hex, never a CSS named colour, never an inline `<style>` in `.razor` markup.
- **CSS isolation per component** (`Component.razor.css`). Isolation is what makes bare
  `table` / `th` / `td` / `.cell` selectors safe inside a chart component.
- **Chart diagrams stay hand-rolled** inline SVG / CSS grid — `SouthIndianGrid_Detailed`, `PolarWheel`,
  `Natal_Transit_Comp_WheelChart`, `MiniGrid`, `ChartFrame`, `LifeWeeks`. MudBlazor does not draw these. See
  [`dataviz.md`](dataviz.md); full catalogue [`components/chart-catalog.md`](components/chart-catalog.md).
- **`dotnet format`** before committing.

## Tables — headers vs. horizontal scroll (implementation note)

A column's width should be set by its **values**, not by a verbose header. Horizontal scroll is
the last resort, not the first reflex.

When a header is longer than the widest value in its column — e.g. Current Transit's
`House from D1` (values `7th`, `11th`) — resolve it in this order:

1. **Shorten the header** if a shorter label stays unambiguous in context
   (`House from D1` → `From D1` / `Δ house`; `In-sign motion` → `In-sign`; `Speed °/day` → `°/day`).
   A one- or two-word header that the surrounding section already disambiguates is fine.
2. **If it can't be shortened without losing meaning, wrap it onto 2–3 lines** — `th`
   gets `white-space: normal; overflow-wrap: break-word; line-height: 1.15;` and a soft cap of
   ~3 lines. The column then sizes to its values and the header stacks above them.
3. **Only then** allow horizontal scroll, and confine it to the table's own
   `overflow-x: auto` container (never the page body) — reserved for genuinely wide tables
   (many independent numeric columns).

`table-layout: auto` everywhere so step 2 actually narrows the column. Applies to every
MudBlazor table and every hand-rolled `<table>`.

## Semantic tokens (over the warm canvas)

| Token family | Use |
|---|---|
| `--dignity-*` (7-tier green→red ramp) | classical dignity dots/labels |
| `--dasha-*` (9-hue categorical, Vimśottari cycle order) | dasha lord swatches, life-weeks grid |
| `--house-lagna` / `--house-moon` (+ `-fg`) | the two stacked house-number badges (from Lagna / from Moon) |
| `--aspect-faint` | "aspected by" ghost chips |
| `--vargottama` | `VargottamaStrip` lit-chip state |
| `--wheel-ring` / `--wheel-tick` | `PolarWheel` ring + degree ticks |
| `--brand-peach` / `--brand-canvas` / `--brand-midnight` / `--brand-sunset` / `--transit-paper` | `Natal_Transit_Comp_WheelChart` rings, spokes, glyphs (no namespaced `--ntw-*` set — reads brand tokens directly) |
| `--cell-fill` / `--lagna-fill` / `--grid-stroke` / `--sign-text` | `SouthIndianGrid_Detailed` cell ground, Lagna cell, borders, labels |
| `--tmpl-*` (+ `--tmpl-rashi-highlight`) | `D1TemplateGrid` light "chart card" palette |

## Additive-change discipline (keeps a revert mechanical)

1. **Chart tokens are namespaced and additive.** Never repurpose a token's meaning — a
   different look is a *new* token (`--wheel-ring-sq`) or a documented value change.
2. **Geometry helpers version by addition.** If a shared projection (`AngleToXy`, …) must
   change behaviour, add `…V2` or a parameter — old components keep compiling and rendering.
3. **A recurring "A vs B" look is a variant, not a bug** — add `PolarWheelSquare.razor` /
   a `Shape` parameter and let `ChartFrame` pick; each variant carries its own golden snapshot.
