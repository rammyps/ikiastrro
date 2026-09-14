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
Inference; revised same day to a single pill style at every level** — not MudBlazor's default
text-plus-underline-slider look, and no longer split between a pill master rail and a
filled-segment nested style:

- **Active tab:** strong sunset-orange fill (`--tab-active-bg` = `--brand-sunset`) + midnight
  text (`--tab-active-fg` = `--brand-midnight`).
- **Inactive tab:** quiet cream fill (`--tab-inactive-bg` = a 22% `--brand-line`/`--brand-canvas`
  mix) + midnight text (`--tab-inactive-fg` = `--brand-midnight`).
- Fully rounded pill shape (`border-radius: 999px`), not the earlier rounded-top "filled
  segment" look.
- Tab text is always ALL CAPS in the markup itself (not a CSS `text-transform`, so labels like
  "1.1 D1 - BIRTH CHART" read correctly in the DOM/accessible name).
- The underline slider MudBlazor draws by default is redundant against a filled pill — hide it
  (`.mud-tab-slider { display: none; }`).

Applies uniformly to the Key Inference master step rail (1. D1-TRANSIT / 2. ABOUT / 3. STRENGTH
/ 4. KARAKAS) **and** every nested tab strip beneath it (1.1/1.2, 2.1/2.2, 3.1/3.2) — one tab
style, not two. `KeyInference.razor.css`'s `.ki-tabs ::deep .mud-tab` rule is the base pill;
`.ki-mastertabs ::deep .ki-master-button` only adds the master rail's grid-stretch layout
(`width: 100%; justify-content: center`), not its own colours.

This is a distinct convention from the "Actions" (buttons) treatment in
[`brand.md`](brand.md#actions) — tabs get their own small token family so the two can be
retuned independently.

The app header follows the same filled-pill grammar (`.ik-headtab`, matching the Key Inference
master rail). Its active state (`.is-here`) uses midnight fill for contrast on the sunset app
bar. The header spans the viewport as three zones: person tabs (ALL CHARTS / KEY INFERENCE) at
left, the compact brand line centred, and **SAVED / CHARTS** at the extreme right. All three
nav labels stack onto two lines (two `<span>`s each); "Saved Charts" retains its accessible
name.

**Implementation note — `::deep` through a MudBlazor component's `Class` parameter doesn't
work.** Blazor's CSS-isolation scope attribute is only added to elements written literally in
the `.razor` file; passing `Class="my-scope"` to `<MudTabs>` puts the class on its rendered
root but *not* the scope attribute (MudTabs doesn't capture/forward it), so
`.my-scope ::deep .mud-tab { }` compiles to a selector that never matches anything. Wrap the
component in a plain `<div class="my-scope">` instead (the div is literal markup and gets the
scope attribute) — same pattern `Natal_Transit_Comp_WheelChart.razor.css`'s
`.ki-wheel ::deep .ntw` already relied on. See `KeyInference.razor`/`.razor.css` for the
worked example (`.ki-tabs` wrapping divs around all three tab levels).

The same gotcha bit `MainLayout.razor`'s `<MudAppBar Class="ik-appbar">` until the 2026-09-14
standardization pass: `::deep .ik-appbar .mud-toolbar { display: grid; … }` compiled to a
scope-prefixed selector with no scoped ancestor anywhere above `.mud-toolbar`, so the header's
three-zone grid (centred brand, right-pinned Saved Charts) silently never applied — the layout
that *looked* right was MudAppBar's own default flex toolbar, coincidentally close but not
actually centring anything. Fixed the same way: `<div class="ik-appbar-scope"><MudAppBar
Class="ik-appbar">…</MudAppBar></div>`, selector now `.ik-appbar-scope ::deep .ik-appbar
.mud-toolbar`. **Any `::deep .some-class-passed-via-Class-param …` selector in this codebase is
suspect** — verify it actually matches (DevTools computed style, or
`document.styleSheets`/`getComputedStyle` in a console) rather than trusting that the rule
merely compiling means it applies.

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
