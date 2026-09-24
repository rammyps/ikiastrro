---
last_updated: 2026-09-23
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
a monospace family. Display hierarchy comes from the `--font-size-*` tokens plus weight,
spacing and colour — not a second typeface.

### Four-level type scale (2026-09-24, rammyps's directive)

Every information-dense chart/table screen (Key Inference and everything shaped like it) uses
one compact four-level scale, `16px = 100%`. Jumps stay small on purpose — a dense interface
where navigation and data panels already compete for attention gets worse with a dramatic
scale, not better:

| Level | Name | % | Size | Token | Weight | Use |
|---|---|---|---|---|---|---|
| 1 | Metadata | 75% | 12px | `--font-size-meta` | 400–500 | Translations, secondary chart details, card labels |
| 2 | Supporting | 87.5% | 14px | `--font-size-support` | 400 | Table content, dates, supplementary values |
| 3 | Primary UI | 100% | 16px | `--font-size-control` | 600 | Tabs, buttons, navigation, important card values |
| 4 | Heading | 125% | 20px | `--font-size-heading` | 600–700 | "Birth Pañchāṅga," "Planet Positions," the open person's name, the footer |

All four tokens live in `tokens.css`. `--font-size-control` keeps its existing name and its
existing role as the one token nearly every table/tab/control already reads off via
`calc(var(--font-size-control) * X)` — it now simply **is** Level 3 (16px) rather than an
independent base the four levels multiply against, so that whole existing multiplier chain
across the app scales down with it automatically. `--font-size-meta` / `-support` / `-heading`
are new siblings for the app bar and any component whose sizing needs to name a level directly
instead of via a multiplier. This does not apply to `--font-size-display` / `--font-size-tagline`
(Home's own hero/tagline clamp() sizes) — those are a separate, bigger "hero" tier, not part of
this four-level scale.

The app bar (`MainLayout.razor`) is the reference implementation: nav tabs (`.ik-headtab`) are
Level 3, the open person's name (`.ik-person`) is Level 4 and rendered ALL CAPS via
`text-transform` (it's per-person data, not a fixed label, so the markup keeps the name's
original case for accessibility — see the Tabs section below for why that differs from a tab
label's caps convention), and the footer (`.ik-footer-inner`, brand lockup included) is Level 4
throughout. The person switcher dropdown's row name is Level 2, its birth-date detail and the
empty-state message are Level 1.

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

### Standard table format (2026-09-22)

Every table on Key Inference now follows one reference format —
`PlanetPositionsTable.razor.css`'s `.ppt` (the "1.1 D1-Birth Chart" table). New tables copy
this pattern into their own scoped CSS (there is no shared stylesheet; each `Component.razor.css`
repeats it):

- **Font**: `font: 500 var(--font-size-control) / 1.3 'Manrope', 'Segoe UI', sans-serif;` +
  `font-size: 0.86em;` declared **once**, at the `<table>` level only — `th`/`td` inherit it,
  they never redeclare `font-size`. **Exception**: a table rendered inside `MudSimpleTable`
  (only `.ki-table`, tab 1.2, does this) needs `th`/`td` to explicitly re-assert the same
  `font: ...; font-size: 0.86em;` pair, because MudBlazor ships its own
  `.mud-simple-table table * tr > td` rule that sets an absolute font-size directly on `td` —
  an explicit declaration always beats inheritance, regardless of specificity, so relying on
  inheritance there silently reverts to MudBlazor's own (larger, Roboto) type. Two things to
  get right if you touch that table again: don't also leave `font-size` on the table-level rule
  (it compounds 0.86em twice), and pass `Hover="false"` to `MudSimpleTable` (see next point).
- **Header**: bold, `color: var(--brand-midnight)`, `border-bottom: 2px solid var(--brand-line)`,
  `position: sticky; top: 0; background: var(--brand-surface);` so it stays visible while the
  page scrolls. A two-row `rowspan`'d header (`ArgalaTable`) sticks the whole `<thead>` instead
  of each `th` individually — simpler than computing a per-row `top` offset, and keeps both
  header rows pinned together.
- **Row hover**: `tbody tr:hover td { background: var(--brand-sunrise); }` — `--brand-sunrise`
  (`#fcd7bd`, tokens.css), not `--brand-peach`. `--brand-peach` stays reserved for its other
  uses (e.g. the All Charts Lagna box, detail/expand panels); `--brand-sunrise` exists
  specifically so table hover reads as visibly orange rather than the older, barely-tinted
  peach. If the table is a `MudSimpleTable`, pass `Hover="false"` — MudBlazor's own
  `.mud-table-hover` hover rule otherwise wins (a plain gray tint) regardless of what your
  scoped CSS says, since it only activates once the `Hover` parameter adds that class.
- **Scroll wrapper**: `<div class="xx-scroll">` with `max-width: 100%; overflow-x: auto;
  border: 1px solid var(--brand-line); border-radius: 10px;` around the `<table>` — every
  table gets one, even ones that don't currently overflow, for the same reason `.ppt-scroll`
  does (§ above).

## Tabs

**Decided 2026-09-14 (rammyps's directive), rolling out app-wide starting with Key
Inference; revised same day to a single pill style at every level; briefly flipped to a flat
midnight/sunset look on 2026-09-23, then reverted the same evening back to sunset/midnight**
— not MudBlazor's default text-plus-underline-slider look, and no longer split between a pill
master rail and a filled-segment nested style:

- **Every tab, active or inactive:** sunset-orange fill (`--tab-active-bg`/`--tab-inactive-bg` =
  `--brand-sunset`) + dark-navy text (`--tab-active-fg`/`--tab-inactive-fg` = `--brand-midnight`)
  — one flat look, no colour swap for the selected tab (rammyps's directive, 2026-09-23 evening;
  earlier that same day this was briefly midnight-fill/sunset-text, and before that active used
  a sunset fill/midnight text and inactive a quiet-cream fill/midnight text).
- Fully rounded pill shape (`border-radius: 999px`), not the earlier rounded-top "filled
  segment" look.
- Tab text is always ALL CAPS in the markup itself (not a CSS `text-transform`, so labels like
  "1.1 D1 - BIRTH CHART" read correctly in the DOM/accessible name).
- The underline slider MudBlazor draws by default is redundant against a filled pill — hide it
  (`.mud-tab-slider { display: none; }`).

Applies uniformly to the Key Inference master step rail (1. NATAL CHARTS / 2. TRANSIT CHART / 3. STRENGTH
/ 4. SPL LAGNAS / 5. YOGAS / 6. VARGAS) **and** every nested tab strip beneath it (1.1/1.2,
2.1/2.2, 3.1/3.2/3.3/3.4) — one tab style, not two. `KeyInference.razor.css`'s
`.ki-tabs ::deep .mud-tab` rule is the base pill; `.ki-mastertabs ::deep .ki-master-button` only
adds the master rail's grid-stretch layout (`width: 100%; justify-content: center`), not its own
colours.

This is a distinct convention from the "Actions" (buttons) treatment in
[`brand.md`](brand.md#actions) — tabs get their own small token family so the two can be
retuned independently.

### Chart controls (in-chart toggles) — distinct from navigation tabs

**Decided 2026-09-17 (rammyps's directive).** A control that changes how one chart *inside* a
step displays itself — Sarvāṣṭakavarga's House order/Strength rank toggle, Planet Strength's
Performance/Composition toggle, House Strength's House order/Strength rank toggle, Amsabala's
Vargottama/Shadvarga/Saptavarga/Dasavarga/Shodasavarga scheme selector, SPL LAGNAS's Wheel/Grid
toggle and Chart `<select>` — is **not** styled as a navigation tab. It's a dark-navy
(`--brand-midnight`) segmented pill, smaller than the card heading next to it, with the same
sunset-fill selected state:

- Pill container: `background: var(--brand-midnight); border-radius: 999px; padding: 3px;` —
  segments have no gap/border between them beyond `gap: 2px`.
- Inactive option: transparent background, `color: var(--brand-canvas)` (cream text on navy).
- Active/selected option: `background: var(--brand-sunset); color: var(--brand-midnight)` — same
  active pairing as a nav tab, so "selected = sunset" still reads consistently app-wide.
- Smaller than a card `<h2>`: `font: 700 .68rem–.78rem/1 var(--font-interface)`, ALL CAPS,
  `padding: 4-6px 10-14px` (vs a nav tab's `--font-size-control`/6-20px).
- Positioned on the **right** of the card header, opposite the `<h2>` (and before any trailing
  stat like Sarvāṣṭakavarga's grand total) — never left-aligned under the heading.
- A `<select>` that chooses or drives a chart (including Key Inference's Natal Chart dropdown
  and SPL LAGNAS's divisional-chart picker) uses a dark-blue `--brand-midnight` surface with
  cream `--brand-canvas` text. This navy/cream/caps look identifies chart-selection controls
  consistently and keeps them visually distinct from ordinary data-entry form fields.

Rationale: this reuses the app's own dark-navy brand color as a visual signal that a control
"belongs to a chart" (part of that card's own display state) rather than to page navigation
(which stays the quiet-cream/strong-sunset MudTabs pill above). Each chart component
re-declares this rule set under its own CSS-isolation scope (`.av-toggle`, `.psc-toggle`,
`.hsc-toggle`, `.am-schemes`, `.karaka-view-tabs`/`.karaka-chart-select`) — same duplication
every `.pill`/`.dot` status class in this codebase already carries per component.

The app header follows the same filled-pill grammar (`.ik-headtab`, matching the Key Inference
master rail). Its active state (`.is-here`) uses midnight fill for contrast on the sunset app
bar. The header spans the viewport as three zones: person tabs (KEY INFERENCE / NUMEROLOGY) at
left, and **SAVED / CHARTS** at the extreme right. All nav labels stack onto two lines (two
`<span>`s each); "Saved Charts" retains its accessible name.

**Header centre column and the brand lockup (revised 2026-09-26, rammyps's directive).** The
Ganesha-icon + "Iki-Astrro" + tagline lockup (`.ik-brand`) no longer lives in the app bar at
all — it moved to the footer's bottom-left, alongside the dedication line, on every screen. In
its place, the header's centre column now shows the currently-open person's name
(`.ik-appbar-person`, MainLayout.razor — reuses `.ik-person-block`) as soon as a person is
opened; "the top where iki-astro used to be" is now the person's own identity, not the brand.
On Home, before any person is opened, that centre column is simply empty. Downstream of this,
the context band below the app bar (`.ik-band`) now renders only when a page supplies
`Active.HeaderExtra` (currently just Key Inference's master step rail) — it no longer has a
"just the person name, centred" mode, since that content moved up into the app bar.

**Name only, no DOB line (2026-09-24, rammyps's directive).** `.ik-person-block` used to stack
the name over a `.ik-person-birth` DOB/time/location line; that second line is gone from the
app bar (it's still shown per-row in the person-switcher dropdown, where it disambiguates
between saved people). The name itself now renders ALL CAPS at Level 4 (see "Typography" above)
via `text-transform: uppercase` on `.ik-person` — CSS transform rather than caps in the markup,
because unlike a static tab label this is arbitrary per-person data and the accessible name
(`@Active.Name`) should keep its real casing.

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
| `--planet-*` (+ `-bg`, 9-hue categorical, PlanetChip glyph order) | **the one canonical 9-graha identity palette** — `PlanetChip` (nakshatra-lord badges included), `VargaView`/`PolarWheel`, `D1TemplateGrid` (via `--tmpl-planet-*` aliases in `SouthIndianTemplate.razor.css`) |
| `--dasha-*` (9-hue categorical, Vimśottari cycle order) | dasha lord swatches, life-weeks grid. **Derived from `--planet-*`** via `color-mix(in srgb, var(--planet-x) 70%, var(--ink) 30%)` (2026-09-25) — not independent hex; a planet's dasha color and its identity color always share the same hue family now |
| `--strength-fill` / `--strength-fill-soft` | Graha-Dṛṣṭi matrix's graded gradient fill / Rāśi-Dṛṣṭi matrix's flat boolean-aspect tint — both `color-mix(in srgb, var(--brand-sunset) N%, transparent)` |
| `--house-lagna` / `--house-moon` (+ `-fg`) | the two stacked house-number badges (from Lagna / from Moon), `SouthIndianGrid_Detailed` |
| `--house-sun` / `--house-micro-moon` (+ `-fg`) | `SouthIndianGrid_Micro`'s own house-badge pair — a genuinely separate "from Sun" counting axis, not a `--house-lagna` duplicate; kept additive per project_standards.md §3.3 |
| `--ref-lagna` / `-arudha` / `-hora` / `-sree` / `-ghati` | `PolarGridLagnaSelect`'s 5 special-Lagna reference-point colors (promoted from a local `.pgls` selector to `tokens.css` 2026-09-25 — values unchanged) |
| `--aspect-faint` | "aspected by" ghost chips |
| `--vargottama` | `VargottamaStrip` lit-chip state |
| `--wheel-ring` / `--wheel-tick` | `PolarWheel` ring + degree ticks |
| `--brand-peach` / `--brand-canvas` / `--brand-midnight` / `--brand-sunset` / `--transit-paper` | `Natal_Transit_Comp_WheelChart` rings, spokes, glyphs (no namespaced `--ntw-*` set — reads brand tokens directly) |
| `--cell-fill` / `--lagna-fill` / `--grid-stroke` / `--sign-text` | `SouthIndianGrid_Detailed` cell ground, Lagna cell, borders, labels |
| `--tmpl-*` (+ `--tmpl-rashi-highlight`) | `D1TemplateGrid` light "chart card" palette. `--tmpl-planet-*`/`--tmpl-housemoon-*`/`--tmpl-lagna-text` are aliases of the app-wide `--planet-*`/`--house-moon`/`--house-lagna` tokens (2026-09-25); the dark-theme block keeps its own literal values (the template's standalone "🌙 Dark" toggle, the one documented exception to "no hard-coded hex") |
| `--status-strong` / `-moderate` / `-weak` (+ `-bg`) | strength traffic light — `PlanetStrengthChart`/`HouseStrengthChart` rank badges + status pills, `PlanetaryStateTable` verdict pills, `AshtakavargaChart`'s Sarvāṣṭavarga bars/Bhinnāṣṭavarga bands. `-moderate` is `var(--brand-sunset)` (2026-09-25, was an unrelated amber `#c9820a`) — see "Rank/score badge convention" below |
| `--tab-active-bg` / `-fg`, `--tab-inactive-bg` / `-fg` | tab-strip fills — see "Tabs" above |
| `--brand-sunrise` | table row-hover, every Key Inference table (see "Standard table format" above) **and** the light text on a Strong/Weak rank badge's solid fill (see "Rank/score badge convention" below). Distinct from `--brand-peach`, which keeps its other uses |

## Rank/score badge convention (2026-09-25, rammyps's call)

Every place a table ranks or bands a numeric strength/score — `PlanetStrengthChart`/
`HouseStrengthChart`'s rank badges, `AshtakavargaChart`'s Sarvāṣṭavarga bars+value and
Bhinnāṣṭavarga cells — reads the same 3-tier scale:

- **Strong** — solid `--status-strong` (green) fill, `--brand-sunrise` text.
- **Moderate** — solid `--status-moderate` (= `--brand-sunset`) fill, `--brand-midnight` text
  (readable on sunset-orange, the same pairing `--tab-active-bg`/`-fg` already uses).
- **Weak** — solid `--status-weak` (red) fill, `--brand-sunrise` text.

This is a *different* visual treatment from the pale-tint "status pill" next to it in the same
row (`.psc-pill`/`PlanetaryStateTable`'s verdict pill: `-bg` fill + solid-color text) — a rank
badge or score cell is a solid-fill circle/pill, a status label is a pale pill. Don't conflate
the two components' CSS.

Per-component thresholds (cite a source when one exists; don't invent one):

- Planet/House Strength: ≥100%/80-99%/&lt;80% of minimum (already-cited Parāśari %-of-minimum
  bands, see `PlanetStrengthChart.StatusOf`'s doc comment).
- Sarvāṣṭavarga: &gt;30/25-30/&lt;25 total bindus — cited from
  `docs/research/domain/transit-events.md`'s Ashtakavarga row.
- Bhinnāṣṭavarga: 0/1-5/≥6 bindus for one recipient in one sign — presentation-only band, no
  classical citation, same status this app's other presentation-only thresholds carry.
- **Piṇḍa (Rāśi/Graha/Śodhya) is deliberately NOT banded** — those are multiplier/remainder
  values used to locate a target nakshatra or sign (same transit-events.md citation), not
  strength scores. Banding them would misrepresent what the number means; don't add one without
  a real citation for what "strong Piṇḍa" is supposed to mean.

**Specificity trap, learned the hard way building this**: a rank badge's own base rule
(`.psc-rankbadge { background: var(--brand-peach); … }`) and its status-color rule
(`.psc-status-strong { background: var(--status-strong); }`) are equal-specificity single-class
selectors — CSS falls back to source order, so whichever is declared *later in the file* wins,
regardless of which one you intended to win. `HouseStrengthChart` never had this bug (its base
`.hsc-rankbadge` rule never set a `background`), but `PlanetStrengthChart`'s did. Fix/pattern to
follow for any new badge like this: give the status override a compound selector
(`.psc-rankbadge.psc-status-strong { background: …; }`), one class more specific than either
rule alone, so it wins regardless of declaration order.

## Related chart drop-downs (2026-09-25, rammyps's call)

When two chart-picker drop-downs on the same page represent the *same underlying selection* —
"which chart am I looking at" — they must be two-way linked, not two silently independent copies
of `_selectedChart` that can disagree. Reference implementation: Key Inference's STRENGTH tab.
The left-side natal chart picker (`KeyInference.razor`'s `_natalChart` field, rendered by the
shared `ChartColumn` fragment) and `AshtakavargaChart`'s own "Chart" drop-down in its
ASHTAVARGA sub-tab are the same selection wearing two hats, so changing either now moves both.

**Pattern**: the child component exposes the standard Blazor two-way-bind pair —
`[Parameter] public string SelectedChart { get; set; }` +
`[Parameter] public EventCallback<string> SelectedChartChanged { get; set; }` — and its own
`<select>` sets the parameter directly and invokes the callback on change, instead of owning a
private `_selectedChart` field. The caller wires it with `@bind-SelectedChart="_natalChart"`
rather than one-way `SelectedChart="@_natalChart"`. `GrahaDrishtiMatrix` already declared this
same parameter pair earlier (2026-09-24) but nothing ever bound to `SelectedChartChanged` — it
has no picker of its own, so there was nothing to link; `AshtakavargaChart` is the first live
use of the pair.

**Before doing this to another pair of drop-downs**, confirm they really are the same selection
and not two axes that only look similar — e.g. a "which varga" picker is not automatically the
same concept as a "which recipient" or "which dasha level" picker just because both are
`<select>`s in the same panel. Link only when picking one value in either place should change
what the *other* is also showing.

## Additive-change discipline (keeps a revert mechanical)

1. **Chart tokens are namespaced and additive.** Never repurpose a token's meaning — a
   different look is a *new* token (`--wheel-ring-sq`) or a documented value change.
2. **Geometry helpers version by addition.** If a shared projection (`AngleToXy`, …) must
   change behaviour, add `…V2` or a parameter — old components keep compiling and rendering.
3. **A recurring "A vs B" look is a variant, not a bug** — add `PolarWheelSquare.razor` /
   a `Shape` parameter and let `ChartFrame` pick; each variant carries its own golden snapshot.
