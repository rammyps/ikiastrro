---
last_updated: 2026-09-25
workstream: ui
component: SindHovGrid
route: embedded — LifeMatters page (see specs_life_matters_page.md)
togaf: C — component spec (built)
catalogued_in: chart-catalog.md
---

# Specification — SindHovGrid (SIND-HOV-GRID)

Written after Codex's first implementation pass (`src/Ikiastrro.Web/Components/Charts/
SindHovGrid.razor(.cs/.css)`, `tests/Ikiastrro.Web.Tests/SindHovGridTests.cs`) landed ahead of
this spec — `lifematters_plan.md`'s Phase 0B calls for the spec before implementation, but Codex
built Phase 1A/1B/part-of-2 in one pass. This spec documents what was actually built, flags where
it diverges from or leaves gaps against `lifematters_plan.md`, and is the contract going forward.

> New module, not a `SouthIndianGrid_Detailed`/`_Micro` variant — different job, not a denser
> restyle. Those two show a chart's full content (planets, upagrahas, arudhas, aspects,
> dignity). SindHovGrid shows only what a LifeMatter's evidence needs (planet occupants +
> special-point labels) plus **hover/keyboard preview, click/Enter pin, and house/special-point
> relevance highlighting** — state neither sibling component has. Per `project_standards.md`
> §3.3, a genuinely new behavior is a new named module, not a prop bolted onto an existing one.

## Geometry

Fixed 4×4 South-Indian sign grid (signs, not houses — cells never rotate). `GridCells` hardcodes
the 12 sign positions plus a 2×2 center block for `CenterTitle`/`CenterNote`. House numbering is
computed per cell from `AscendantSign` via `AstroMath.CountFromSignToSign` — this is what makes
the component chart-agnostic: the caller passes whichever chart's Ascendant sign is on screen
(D1, D9, ...), and every cell's house-from-Lagna number and relevance recompute against it. This
is the mechanism behind the plan's `signAt((ascendantSignIndex + houseNumber - 1) % 12)` rule.

## Parameters

| Parameter | Type | Purpose |
|---|---|---|
| `AscendantSign` (required) | `string` | Drives house-from-Lagna numbering for every cell |
| `PlanetsBySign` | `IReadOnlyDictionary<string, IReadOnlyList<GridPlanetGlyph>>` | Occupants per sign |
| `SpecialPointsBySign` | `IReadOnlyDictionary<string, IReadOnlyList<string>>` | Canonical codes (`A1`..`A12`/`AL`) per sign — **never** display aliases (`UL`) |
| `HouseFoci` | `IReadOnlyList<SindHovHouseFocus>` (`ReferenceSign`, `HouseNumber`) | Houses to highlight, resolved to a sign via `LifeMatterSignMath.ResolveHouseSign` |
| `SpecialPointFocusCodes` | `IReadOnlyList<string>` | Special-point codes to highlight, matched against `SpecialPointsBySign` |
| `PinnedSign` / `PinnedSignChanged` | `string?` / `EventCallback<string?>` | The pinned (clicked/Enter-Space'd) selection — caller-owned state |
| `PreviewSignChanged` | `EventCallback<string?>` | Fires on hover/keyboard-focus preview; preview itself is internal state (`_previewSign`), only the callback surfaces it |
| `CenterTitle` / `CenterNote` | `string` / `string?` | Center-block content — the page supplies the Step/Varga label here |
| `AriaLabel` | `string` | Grid `aria-label`, defaults to "South Indian horoscope grid" |

A cell is `relevant` when its sign is in `HouseFoci` (translated) or holds a special point whose
code is in `SpecialPointFocusCodes` — both focus kinds union onto the same highlight, matching
the plan's "unions matches across multiple foci" rule for evidence cards, applied here to the
chart itself.

## Interaction (built and tested)

- **Hover / keyboard-focus preview**: `Preview(sign)` on `@onmouseenter`/`@onfocus`, cleared on
  `@onmouseleave`/`@onblur`. Preview never touches `PinnedSign` — `HoverPreview_
  DoesNotReplacePinnedSelection` confirms hovering a different cell doesn't disturb the pinned
  cell's `.pinned` class.
- **Click or Enter/Space pins**: `Pin(sign)` invokes `PinnedSignChanged`; `EnterAndSpace_
  PinKeyboardFocusedCell` covers both keys.
- **Escape**: `OnGridKeyDown` clears preview first if one is active, otherwise clears the pin —
  matches the plan's "Escape clears preview first, then the pin" exactly. **Gap**: no test
  exercises this path yet (`SindHovGridTests` has no Escape case) — add one before Phase 3C
  regression coverage is signed off.
- **Touch tap**: no separate handler — the cell is a native `<button>`, so tap already fires the
  same click/`Pin` path. Correct by construction, not a gap.
- **Non-color selection marker**: `.selection-mark` (◆) renders whenever a cell is `relevant` or
  `pinned`, independent of the sunset-outline `box-shadow` — satisfies the plan's "selection uses
  a non-color indicator" rule twice over (icon + inset border).
- **Keyboard operability / ARIA**: every cell is `role="gridcell"` inside `role="grid"`, with a
  computed `aria-label` (`CellAriaLabel`) stating sign, house, occupants, and special points, and
  `aria-selected` reflecting the pin. `EverySignCell_IsAKeyboardOperableGridCellWithNonColorMarker`
  covers cell count, marker presence, and the grid's own `aria-label`.
- **`prefers-reduced-motion`**: `.chart-cell` transitions are disabled under that media query.

## Not this component's job (page-level, see specs_life_matters_page.md)

- Auto/D1/Manual Varga selection and "retain last chart if unmapped" — the page decides which
  chart's Ascendant sign and evidence dictionaries to pass in; SindHovGrid has no chart-type
  awareness at all.
- Uncomputed-Varga empty state — the plan says this "uses the chart's existing empty state"; the
  page substitutes its own empty-state UI in place of this component rather than SindHovGrid
  rendering one itself.
- "Clicking the selected Step keeps it selected; meaningful fallback is D1/Rasi" — Step/Varga
  lifecycle, not grid state.
- Full chart display (aspects, upagrahas, dignity, karaka tags) — deliberately out of scope; that
  remains `SouthIndianGrid_Detailed`/`_Micro`'s job elsewhere in the app.

## Rendering

Inline CSS grid (`grid-template-columns: repeat(4, 1fr)`), CSS-isolated
(`SindHovGrid.razor.css`), `--brand-*`/`--font-size-*` tokens (no chart-specific
`--dignity-*`/`--house-*` tokens — those belong to Detailed/Micro's richer display). Sub-desktop
breakpoint at 720px (row height 92px → 76px) — satisfies the plan's "page specification must
define a sub-desktop stacked/scrollable breakpoint" at the component level; the page must still
define its own layout breakpoint around the grid (card stacking), which is separate.

**Gap**: no golden-SVG snapshot test yet (`ChartSnapshotTests` has no `SindHovGrid` case). The
plan requires one before Phase 3C sign-off ("Add and visually review a golden SVG, recognizing
that a golden validates rendering, not behavior").

## Files

`src/Ikiastrro.Web/Components/Charts/SindHovGrid.razor` (+ `.razor.cs`, `.razor.css`) ·
`SindHovGrid.razor.cs` also hosts `SindHovHouseFocus` and the static `LifeMatterSignMath.
ResolveHouseSign` helper (duplicates `LifeMatterFocusResolver.ResolveHouseSign` in
`Ikiastrro.Core` — same formula, two call sites; worth collapsing to one before Phase 3C so the
resolver and the grid can't drift). Tests: `tests/Ikiastrro.Web.Tests/SindHovGridTests.cs`.
