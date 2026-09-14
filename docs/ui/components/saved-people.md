---
last_updated: 2026-09-14
workstream: ui
component: SavedCharts
route: /charts
togaf: C — component spec
---

# Component — Saved Charts

`SavedCharts.razor` at `/charts`. The people-management surface: the list of everyone
in `dbo.tbl_BirthDetails`, per-row Edit / Delete, and Import / Export. Reads
persisted rows only; the one write path that recomputes goes through
`ChartGenerationService`.

Page title / `<h1>` and the global nav pill both read **"Saved Charts"** (the route
stays `/charts`). Matches `UI_SVG_Templates/V2.1-Build/SAVED-CHART-DESIGN-v5.png`: a
full-bleed Saturn-rings hero (`wwwroot/images/saturn-rings-brand.png`) on the left,
title + subtitle + "N saved charts" count + Import/Export toolbar + table on the right.

## The list

`DataTable` — sortable, client-side, small lists. Columns: **Name** (link to
`/charts/{id}`, rendered ~1.1rem, larger than the rest of the row) · Date of Birth ·
Time of Birth · Place · **Status** (the built/not-built pill) · actions. The header
row is filled sunset-orange; the table sits in a bordered, rounded card.

**Actions cell — Edit then Delete** (`EditIconButton` before `DeleteIconButton`).

- **Delete** → `ConfirmDialog` → `BirthDetailDeletionService.DeleteBirthDetail`. The
  service clears **every** table with a NO_ACTION FK to `tbl_ChartResults` (chart-generic
  analytics + strength / bhava-bala / vargottama facts — the same set `GenerateAll`
  clears); a delete that still throws is caught and shown inline, never crashes the
  circuit (commit `b36b568`).
- **Edit** → hand-rolled modal (`.edit-box`, styled like `ConfirmDialog`) pre-filled with
  name / sex / DOB / TOB / city / country.
  - Name or sex only → `BirthDetailsRepository.Update` (no recompute).
  - Any **birth field** changed (DOB / TOB / city / country) → re-resolve place via
    `IPlaceResolver`, then `ChartGenerationService.GenerateAll` (delete-first-then-
    regenerate). Modal shows a "Rebuilding charts…" state; errors surface in the modal.

## Import / Export (`decisions/002`)

Toolbar at the top-right of the page, beside the title:

- **`Import ▾`** — *From CSV file…* · *From Horoscope Explorer (.JKD)…*
- **`Export ▾`** — *All people as CSV* · *As .JKD*

CSV is handled in-process; `.JKD` (Jet 4, 32-bit only) shells out to
`tools/jkd-interchange/jkd-interchange.ps1`. Import **does not geocode** — CSV and `.JKD`
both carry lat/long/offset; `IPlaceResolver` is only used when a row's coordinates are
blank. Dedupe by Name against `UX_BirthDetails_Name`.

## Bulk chart generation (`decisions/002`)

Import writes rows only (instant). Each person row's **Status** cell shows a
**`● Built` / `○ Not built`** pill (D1 `tbl_ChartResults` present). Building:

- Per person, on demand — the transit page's *"Generate a D1 chart for this person
  first"* empty state gets a **Generate** button (~4 s).
- **"Build missing (N)"** button (next to the "N saved charts" count, above the table)
  — foreground loop over `GenerateAll`, progress bar, **capped** per press (≈25, then
  "Continue").
- Phase 2 (only if routine, still not built): a hosted `BackgroundService` queue +
  SignalR progress; `ChartGenerationService.GenerateMissing` is the per-person step.

## Retired

`MiniGrid` thumbnail column (was in the v1 table) — not part of the people-management view.
