---
last_updated: 2026-09-14
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

## Typography

**Manrope everywhere**, inherited from `--font-interface`. Only the bundled 400, 500, 600,
700 and 800 weights may be requested; do not use synthetic 750/850/900 weights. Dates,
degrees, scores and periods use `font-variant-numeric: tabular-nums` rather than changing to
a monospace family. Display hierarchy comes from the three `--font-size-*` tokens plus weight,
spacing and colour—not a second typeface.

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

## Tabs

**Decided 2026-09-14 (rammyps's directive), rolling out app-wide starting with Key
Inference:** every tab strip, at every nesting level, is a filled segment, not MudBlazor's
default text-plus-underline-slider look —

- **Active tab:** dark-blue fill (`--tab-active-bg` = `--brand-midnight`) + gold text
  (`--tab-active-fg`).
- **Inactive tab:** sunset-orange fill (`--tab-inactive-bg` = `--brand-sunset`) + dark-blue
  text (`--tab-inactive-fg` = `--brand-midnight`).
- The underline slider MudBlazor draws by default is redundant against a filled tab — hide it
  (`.mud-tab-slider { display: none; }`).

This is a distinct convention from the "Actions" (buttons) treatment in
[`brand.md`](brand.md#actions) — tabs get their own small token family so the two can be
retuned independently.

The app header follows the same filled-pill grammar. Its active state uses midnight fill for
contrast on the sunset app bar. The header spans the viewport as three zones: Home/person tabs
at left, the compact brand line centred, and **SAVED / CHARTS** at the extreme right. The two
words are visually stacked but retain the accessible name “Saved Charts”.

**Implementation note — `::deep` through a MudBlazor component's `Class` parameter doesn't
work.** Blazor's CSS-isolation scope attribute is only added to elements written literally in
the `.razor` file; passing `Class="my-scope"` to `<MudTabs>` puts the class on its rendered
root but *not* the scope attribute (MudTabs doesn't capture/forward it), so
`.my-scope ::deep .mud-tab { }` compiles to a selector that never matches anything. Wrap the
component in a plain `<div class="my-scope">` instead (the div is literal markup and gets the
scope attribute) — same pattern `Natal_Transit_Comp_WheelChart.razor.css`'s
`.ki-wheel ::deep .ntw` already relied on. See `KeyInference.razor`/`.razor.css` for the
worked example (`.ki-tabs` wrapping divs around all three tab levels).

**Vertical tabs (`Position="Position.Left"`) need one extra wrapper.** MudBlazor's vertical
mode makes `.mud-tabs-panels` itself a flex row and gives the active `.mud-tab-panel`
`display:contents` — which promotes that panel's own direct children into the row instead of
letting them stack as a normal block column. Wrap everything inside the `MudTabPanel` in one
element (Key Inference's `.ki-panelbody`, `flex: 1 1 auto; min-width: 0;`) so only that single
wrapper gets promoted, not its grandchildren.

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
| `--tab-active-bg` / `-fg`, `--tab-inactive-bg` / `-fg` | tab-strip fills — see "Tabs" above |

## Additive-change discipline (keeps a revert mechanical)

1. **Chart tokens are namespaced and additive.** Never repurpose a token's meaning — a
   different look is a *new* token (`--wheel-ring-sq`) or a documented value change.
2. **Geometry helpers version by addition.** If a shared projection (`AngleToXy`, …) must
   change behaviour, add `…V2` or a parameter — old components keep compiling and rendering.
3. **A recurring "A vs B" look is a variant, not a bug** — add `PolarWheelSquare.razor` /
   a `Shape` parameter and let `ChartFrame` pick; each variant carries its own golden snapshot.
