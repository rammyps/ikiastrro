---
last_updated: 2026-09-10
workstream: ui
togaf: C — Application Architecture (UI)
safe: Solution Intent — UX
---

# UI workstream — MASTER (App UI)

**Branch** `workstream/ui` · **worktree** `D:\@ClaudeSpace\ikiastrro.wt\ui` ·
**owns** `src/Ikiastrro.Web/`, `tests/Ikiastrro.Web.Tests/`, `tests/Ikiastrro.Web.E2E/`.

`Ikiastrro.Web` — Blazor Server, MudBlazor, warm light brand. Reads persisted rows only
([`../architecture/domain-contracts.md`](../architecture/domain-contracts.md)); never
recomputes.

## Docs

| Doc | For |
|---|---|
| [`wkstream_UI_v1.md`](wkstream_UI_v1.md) | What is **live** — MudBlazor shell, brand override, the four kept surfaces |
| [`wkstream_UI_v2.md`](wkstream_UI_v2.md) | **In scoping** — the full UI re-do: analysis-first IA + ROADMAP *Now* surfacing + `/add` / Preferences |
| [`brand.md`](brand.md) | Canonical palette, typography, lockup, preserved assets |
| [`design-language.md`](design-language.md) | Token + component authoring rules |
| [`dataviz.md`](dataviz.md) | Charting approach — hand-rolled SVG now, Syncfusion as a deferred option |
| [`components/spec_Natal_Transit_Comp_Wheel.md`](components/spec_Natal_Transit_Comp_Wheel.md) | Living specification — natal ↔ transit comparison wheel and two-tab D1 Birth / Current Transit table |
| [`components/spec_SouthIndianGrid_Detailed.md`](components/spec_SouthIndianGrid_Detailed.md) | The enriched South-Indian chart grid + template page |
| [`components/evidence-tables.md`](components/evidence-tables.md) | The astrologer evidence page |
| [`components/home.md`](components/home.md) | Home / entry screen |
| [`components/saved-people.md`](components/saved-people.md) | `/charts` — the people-management surface: list, Edit / Delete, planned Import / Export + bulk generation ([`decision 002`](../../decisions/002-import-export-and-bulk-chart-generation.md)) |
| [`testing.md`](testing.md) | The two test layers — bUnit component tests and the Playwright browser E2E suite (`tests/Ikiastrro.Web.E2E`) |
| [`components/key-inference.md`](components/key-inference.md) | `/key-inference/{id}` — round-2 spec: the 6-step flow (D1/Transit · Understanding · Strength · Planet-Chart · Ashtakavarga · Yoga), one chart + one table per step, new-field sourcing status |
| [`components/yoga.md`](components/yoga.md) | Key Inference step 6 · Yoga — coverage donut + Source/Yoga/Type/Rule/Result table |
| [`components/dasha-sade-sati.md`](components/dasha-sade-sati.md) | v2 Key Inference → TIME PERIOD (DASHA) + SATURN TIME PERIOD headers (unchanged by the round-2 flow) |
| [`components/chart-catalog.md`](components/chart-catalog.md) | **Chart-module catalogue** — every visual chart component (incl. `SouthIndianGrid_Detailed`, `Natal_Transit_Comp_WheelChart`), dasha module, UI table + helper, as `chart name / spec doc / linked files`; golden-snapshot flow + revert. Naming & versioning: [`../../project_standards.md`](../../project_standards.md) |

## Screen inventory (live routes)

| Route | Page | Shows | State |
|---|---|---|---|
| `/` | `Home` | brand lockup + Ganesha/Navagraha art; searchable name over saved people; inline Preferences (top-left) + inline Add | **v2 rebuild in progress** ([`components/home.md`](components/home.md)) |
| ~~`/add`~~ | — | folded into Home in v2 | retired |
| `/charts` | `SavedCharts` ("Saved people") | sortable person list, per-row **Edit** (rename / rebuild) + **Delete**; Import / Export + bulk-generate **planned** | edit/delete verified ([`components/saved-people.md`](components/saved-people.md), [`../../decisions/002-import-export-and-bulk-chart-generation.md`](../../decisions/002-import-export-and-bulk-chart-generation.md)) |
| `/charts/{id}` | `AllCharts` (v2) | every stored divisional chart as a plain South-Indian grid, 3 per row, divisor order; sunset card heads on the canvas | **v2 built** (`FEAT-UI-14`) — replaced the v1 `Workspace` hub |
| `/charts/{id}/varga/{code}` | `VargaView` | one varga in full — grid + wheel, `VargottamaStrip`, positions, house-lordship + conjunctions disclosure, prev/next | verified |
| `/charts/{id}/south-indian-template` | `SouthIndianTemplate` | the print-style South-Indian D1 template, light/dark toggle | verified |
| `/charts/{id}/timing` | `Timing` | Vimśottari dasha tree + Sade Sati + Gochara | verified |
| `/charts/{id}/evidence` | `AstrologerEvidence` | read-only evidence tables in reading order, chart selector | verified |
| `/charts/{id}/life-weeks` | `LifeWeeks` | 4000-week grid coloured by Mahādaśā | verified — **retired in v2** |
| `/transit-wheel/{id}` | `Natal_Transit_Comp_Wheel` | band heading `TRANSIT - D1 BIRTH CHART`, natal-transit wheel, two-tab **D1 Birth** / **Current Transit** table | See the [living specification](components/spec_Natal_Transit_Comp_Wheel.md) and `FEAT-UI-13` |

**v2 route targets** (not yet built — see [`wkstream_UI_v2.md`](wkstream_UI_v2.md#routes)):
`/transit-wheel/{id}` → Transit landing · `/charts/{id}` → All Charts (21 grids) ·
`/key-inference/{id}` → Key Inference — round-2 6-step flow (D1/Transit · Understanding ·
Strength · Planet-Chart · Ashtakavarga · Yoga), plus the still-separate TIME PERIOD (DASHA) /
SATURN TIME PERIOD headers; see [`components/key-inference.md`](components/key-inference.md).
Retired in v2: `/add`, `/charts/{id}/evidence`, `/charts/{id}/varga/{code}`, the
`/charts/{id}` hub, `/charts/{id}/life-weeks`, `/charts/{id}/timing`.

## Navigation

Shared MudBlazor header (`MudAppBar`). **`HOME`** is a fixed navy pill straddling the app-bar /
context-band edge, hard left. The per-person tabs — **`TRANSIT` · `ALL CHARTS` · `KEY INFERENCE`**
— sit next to it and are **hidden until a person is opened** (every inner page is per-person).
Brand lockup **Iki-Astrro | Where Passion, Purpose & Planets Align.** on the right. Preferences
is an inline Home control, not a nav item. Person name in the band is a `▾` switch back to Home.

## Non-functional requirements (UI)

Quality attributes the app should hold, tracked apart from feature rows. Backlog status is on
`ROADMAP.md`.

### NFR-UI-01 — Runtime-reorderable tabs · **deferred (backlog: Later)**

The top-level tabs (and, by extension, the Key Inference 6-step flow) should be
**re-orderable at runtime** — drag-to-reposition like browser tabs, order remembered per user.
Routes/URLs don't change (the tabs are hash-routed), so reordering is purely presentational and
low-risk. `HOME` stays fixed.

| Scope | Effort | Notes |
|---|---|---|
| Nav strip only · drag-reorder · `localStorage` persistence · reset control · bUnit snapshot | **~1.5–2 dev-days** | shares the localStorage layer planned for Preferences (`FEAT-UI-12`) |
| + Key-Inference headers & sub-tabs (3 strips) · keyboard reorder (a11y) · stale-order degradation | **+1–1.5 days** | drag-only is not accessible — needs move-left/right controls in an "edit tabs" mode |
| + DB-backed per-user persistence (`tbl_UserUiPreferences` + repo + migration + tests) | **+1.5–2 days** | pulls in the `database` workstream |

Full build ≈ **4–6 dev-days**. **Deferred** until the tab inventory stops changing (it churned
repeatedly during `wkstream_UI_v2`); every added/renamed tab otherwise means maintaining
stored-order migration logic. When taken up: the *minimal localStorage* form first; it then
graduates to a [`design-language.md`](design-language.md) rule.

### NFR-UI-02 — Chart-style choice · **default South Indian, N/W Indian deferred**

The divisional / varga charts render **South Indian by default**, with **North Indian** and
**West Indian** styles selectable from Preferences. Only South Indian renders today; the other
two renderers are on `ROADMAP.md` *Later* and are a `Components/Charts/**` (Codex-scope) job.
The Preferences selector lists all three for forward-compatibility, but **North Indian and West
Indian are shown disabled and labelled "Planned"** until their renderer ships — an unrenderable
style can never be stored as the active choice.

### NFR-UI-03 — Ayanāṁśa choice

Preferences exposes the **full ayanāṁśa catalogue — 21 systems** (`AyanamsaDefinition.Catalog`),
with **Lahiri fixed as the default** (the active `tbl_Rule_Ayanamsa` row). Chart generation
takes the chosen system for that person's next run; the project baseline is unchanged.

### NFR-UI-04 — Localisation (Tamil) · **deferred (backlog: Later)**

The app should support **Tamil** — either Tamil script or a Tamil transliteration (decision
deferred). A **Language** selector in Preferences, with **Tamil shown disabled and labelled
"Planned"** until the localisation ships (English is the only selectable value today). This is
broad: UI chrome strings, astrology terminology (`tbl_Dim_*` display names / a term table),
number and date formatting, and right-to-left is not needed but glyph coverage in Manrope is
(Tamil needs a fallback face). Scope this as its own design pass before estimating.

## In flight — `wkstream_UI_v2`, the Home page

- **`FEAT-UI-02`** — Home rebuilt on MudBlazor: `MudAutocomplete` name search; two-column
  canvas layout with Ganesha art right; the three size tokens; sunset-orange button fill.
- **`FEAT-UI-03`** — Add folded into Home: `Add New` unhides Name · Sex · DOB · Time · City ·
  Country; completing Country generates + routes to `/transit-wheel/{id}`.
- **`FEAT-UI-12`** — Preferences disclosure at Home top-left, three selector groups:
  **Ayanāṁśa** (21 systems, *Lahiri* fixed default — NFR-UI-03) · **Chart style** (South Indian
  default; North / West Indian listed, renderers deferred — NFR-UI-02) · **Language** (Tamil
  planned — NFR-UI-04). `localStorage` for now; DB-backed default is a `database` follow-up.

## Planned

- **`wkstream_UI_v2`** — full re-do, now in scoping ([`wkstream_UI_v2.md`](wkstream_UI_v2.md)).
  Absorbs `FEAT-UI-03` / `FEAT-UI-12` and the "Missing Web" column of the `masterproduct.md`
  rollup — divisional charts D2–D60, Chara Karakas, avastha states, the slow-planet transit
  timeline, strength (Ṣaḍbala / Bhāva Bala).
- Codex works one path scope on `workstream/ui` — proposed `src/Ikiastrro.Web/Components/Charts/**`
  (confirm); Claude owns the shell and integrates.
