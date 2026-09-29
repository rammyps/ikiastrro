---
last_updated: 2026-09-29
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

> A separate module from `SouthIndianGrid_Detailed`/`_Micro` because of its behaviour:
> **hover/keyboard preview, click/Enter pin, and house/special-point relevance highlighting**,
> state neither sibling has. Since 2026-09-29 it borrows `SouthIndianGrid_Detailed`'s visual
> language (sign + Sanskrit name, house badges, dignity dots, `(R)`/`(D)`, combust, aspect strip)
> so Life Matters reads like the rest of the app's charts. Upagrahas and karaka-scheme tags stay
> out; they remain Detailed/Micro's job.

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
| `CenterTitle` / `CenterNote` / `CenterMeta` | `string` / `string?` / `string?` | Center-block lines: matter, "7th from Ghati Lagna", chart + reference signs |
| `TrackSign` / `TrackTag` / `TrackLabel` | `string?` | A second reference point (special lagna). Each cell adds a gold house-from-track badge; the track's own sign carries `TrackTag` (e.g. `GL`). Null → Ascendant badge only |
| `AspectedBySign` | `IReadOnlyDictionary<string, IReadOnlyList<string>>` | Dashed aspect strip per sign (`ChartViewModel.BuildAspectedByMicro`, `Ma(4)` labels) |
| `SavBySign` | `IReadOnlyDictionary<string, int>` | D1 Sarva Ashtakavarga bindus per sign, a corner chip banded by `LifeMatterStatistics.SavBand` (>30 / <25). The page passes it only when D1 is displayed |
| `KarakaPlanets` | `IReadOnlySet<string>` | Planets outlined as the matter's karakas |
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
  matches the plan's "Escape clears preview first, then the pin" exactly, covered by
  `Escape_ClearsPreviewFirstThenPin`.
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
- Upagrahas and karaka-scheme tags — out of scope; `SouthIndianGrid_Detailed`/`_Micro` show them.
- Deciding which reference sign is the track, or which houses are the focus — the page resolves
  both and passes signs in.

## Rendering

Inline CSS grid (`repeat(4, minmax(0, 1fr))`, max 520px), CSS-isolated (`SindHovGrid.razor.css`),
tokens only: `--brand-*`, `--house-lagna`/`--house-moon` (Ascendant and track badges),
`--dignity-*` dots, `--status-strong`/`--status-weak` (SAV chip), `--asc-glow` (Lagna cell, layered
over the cell's own fill). Relevant cells fill with `--brand-sunrise`; the pin adds a midnight
inset. Sub-desktop breakpoint at 720px (row height 112px → 88px, Sanskrit names hidden).

Golden SVG: `ChartSnapshotTests.SindHovGrid` (fixture includes a Ghati Lagna track, SAV chips and
an aspect strip), baseline at `docs/artifacts/ui/SindHovGrid-sample.svg`. Tests for the new
parameters: `TrackSign_AddsASecondHouseBadgeAndTagsTheReferenceSign`,
`WithoutTrackSign_OnlyTheAscendantHouseBadgeRenders`,
`DetailedCellContent_DignityDirectionCombustAspectsAndSavBand`.

## Files

`src/Ikiastrro.Web/Components/Charts/SindHovGrid.razor` (+ `.razor.cs`, `.razor.css`) ·
`SindHovGrid.razor.cs` also hosts `SindHovHouseFocus` and the static `LifeMatterSignMath.
ResolveHouseSign` helper, now a thin string-keyed wrapper over `LifeMatterFocusResolver.
ResolveHouseSign` in `Ikiastrro.Core` — one formula, one call site. Tests:
`tests/Ikiastrro.Web.Tests/SindHovGridTests.cs`; golden SVG at
`docs/artifacts/ui/SindHovGrid-sample.svg`.
