---
last_updated: 2026-09-19
workstream: ui
component: Numerology
route: /numerology/{id}
togaf: C — component spec
---

# Component — Numerology

Cheiro's name-number method (the Chaldean letter-value table Cheiro published and popularised),
brought in from the standalone `ikinumero` prototype (`D:\@ChatGPT\ikinumero`) as a new
per-person header tab, not a linked/embedded second app. Only the calculation logic was ported;
ikiastrro's own stack is used throughout — no EF Core, no SQLite, no second database.

## Purpose

Show the active person's Cheiro compound total and reduced root number, computed from their
saved `Name`, alongside the letter-by-letter breakdown.

## Page contract

- Route `/numerology/{id}` — a per-person page, same shape as `AllCharts` (`/charts/{id}`) and
  `KeyInference` (`/key-inference/{id}`): resolves the person via `BirthDetailsRepository
  .GetById(id)`, calls `Active.Set(...)` to populate the header/band, and shows a "no saved
  person found" state (with a link back to `/`) when the id doesn't resolve.
- Header tab: **NUMEROLOGY**, added to the per-person nav strip in `MainLayout.razor` alongside
  **ALL CHARTS** / **KEY INFERENCE**. Single word, so it renders as plain text like `HOME`
  rather than the two-word stacked-span pill those two use.
- No inputs, no controls — the page is read-only and fully determined by the active person's
  `Name`; there is no free-text "type any name" form like the `ikinumero` prototype had.
- Layout: one `MudPaper` card — name, compound total → root number (with an arrow between
  them), the per-letter value tiles, and a one-line method caption ("Cheiro's Chaldean method ·
  9 unassigned").

## Data contract

- Input: `BirthDetails.Name` (already persisted — no new column, table or migration).
- Calculation: `Ikiastrro.Core.Numerology.CheiroNumerology.Calculate(name)` — pure, stateless,
  in-process. The letter → value table (`A,I,J,Q,Y`=1 · `B,K,R`=2 · `C,G,L,S`=3 · `D,M,T`=4 ·
  `E,H,N,X`=5 · `U,V,W`=6 · `O,Z`=7 · `F,P`=8; `9` is sacred/unassigned) is hardcoded — a fixed
  classical standard nobody edits, so it isn't DB reference data (same reasoning as the
  ayanamsa-hardcode decision: a migration/repo would only add indirection).
- Nothing is persisted. Unlike `ikinumero`'s `CalculationHistory` table, there's no history to
  show — the result is fully reproducible from the person's stored `Name` every time, so caching
  or logging past calculations would be redundant state.

## Visual contract

Matches the `AllCharts`/`KeyInference` canvas-card look: `--brand-canvas` card on the warm
canvas, `--brand-line` border, `--brand-midnight` heading in caps (`num-heading`), Manrope only,
`--font-size-control`-derived sizes only — no raw hex/px literals. The root number is the one
accent element, in `--brand-sunset`; the compound total stays midnight. Per-letter tiles are
small bordered chips, one per counted letter, value shown in sunset beneath the letter.

## Not ported from `ikinumero`

- The marketing-style landing hero, "Explore the Cheiro method" scroll copy, and the letter
  mapping/history sections — this page is one focused card inside ikiastrro's own shell, not a
  standalone site.
- `NumerologySystem`/`LetterMapping`/`CalculationHistory` EF Core entities and the SQLite
  database (`ikinumero.db`) — the letter table is hardcoded (see Data contract) and nothing
  needs persisting.
- Pythagorean (or any other) system selector — `ikinumero` scaffolded a `NumerologySystem.Code`
  concept for multiple systems, but only ever shipped Chaldean/Cheiro. This page hardcodes
  Cheiro's method; a second system is a future addition, not a retained-but-unused seam.
