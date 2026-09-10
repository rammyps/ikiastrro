---
last_updated: 2026-09-10
workstream: ui
version: v2
status: scoping
togaf: E — Solution increment
safe: Feature (full-surface)
---

# wkstream_UI_v2 — full UI re-do (table-first)

A ground-up rework of the astrologer-facing app on **one pattern: the
[`AstrologerEvidence`](components/evidence-tables.md) page**. [`wkstream_UI_v1.md`](wkstream_UI_v1.md)
stays the description of what is **live** until v2 ships; this doc is the increment.

> **Status: building.** The pattern and the design system below are decided. The delivery list
> is fixed (ROADMAP *Now* + the open `FEAT-UI` rows). Route consolidation and which v1-dropped
> surfaces return are the remaining open items — a short pass with the product head. **Built so
> far:** the v2 app shell (HOME pill + per-person tabs + context band); the Transit landing
> (`/transit-wheel/{id}`) D1 Birth tab (`FEAT-UI-13`); **All Charts (`/charts/{id}`) — 21
> divisional grids, divisor order** (`FEAT-UI-14`, the v1 `Workspace` hub retired). Home (`/`) is
> partly on this pattern; Key Inference is unbuilt.

## The pattern — AstrologerEvidence, everywhere

Every read surface in v2 is the `AstrologerEvidence` shape, restyled to MudBlazor:

- **Page = sticky header + entity/chart selector + a section index (anchor nav) + N
  collapsible sections.** Each section is a table.
- **Tables are the primary representation.** The generic `EvidenceTable` (columns + rows,
  auto-labelled headers, typed value formatting) generalises to a shared `SectionTable`.
- **Every row comes from a persisted view or table** — `vw_Chart_Consolidated`,
  `vw_ChartMoonContext`, `vw_ChartPlanetEvidence`, `vw_ChartShadbala`, `vw_ChartBhavaBala`,
  `vw_ChartYogaEvaluations`, the reference dimension tables. No page recomputes
  ([`../architecture/domain-contracts.md`](../architecture/domain-contracts.md)).
- **A chart selector** switches the position-dependent sections between D1 and any stored varga.
- **Hand-rolled SVG diagrams** (`SouthIndianGrid_Detailed`, `PolarWheel`, `Natal_Transit_Comp_WheelChart`;
  full catalogue [`components/chart-catalog.md`](components/chart-catalog.md)) are *secondary* — embedded beside the
  table where a picture aids reading, never the primary view. They stay in Codex's scope (see
  Workstream mechanics) and outside the MudBlazor restyle.

## What v2 must deliver

Each item is one or more table sections on the pattern above.

| From | Feature | As | Issue · Milestone |
|---|---|---|---|
| ROADMAP Now | Divisional charts D2–D60 (`FEAT-VARGA-01`) | the chart selector drives the Positions + Dignity/Avastha + Karaka sections across all 21 vargas | #8 · Divisional charts in the UI |
| ROADMAP Now | Jaimini chara karakas (`FEAT-KARAKA-01`) | a Karakas section — AK…DK → planet, longitude, degree-in-sign, per chart | #9 · Jaimini chara karakas panel |
| ROADMAP Now | Avastha display (`FEAT-AVASTHA-01/02`) | rows in the Dignity & Avastha section — AgeState, WakefulnessState | #10 · Planetary-state (avastha) display |
| ROADMAP Now | Slow-planet transit history (`FEAT-TRANSIT-01`) | a Transit History section — sign-ingress events (planet, from→to, date, retro) with a date-range filter | #11 · Slow-planet transit history view |
| Open `FEAT-UI` | Add / Edit person (`FEAT-UI-03`) | **inline on Home** — `Add New` unhides Name · Sex · DOB · Time · City · Country; completing Country → `/transit-wheel/{id}` | #4 |
| Open `FEAT-UI` | Preferences (`FEAT-UI-12`) | **inline on Home, top-left disclosure** — three selector groups: Ayanāṁśa (21 catalogued, *Lahiri* fixed default) · Chart style (South Indian default; North / West Indian listed, renderers deferred) · Language (English; Tamil planned). See [`components/home.md`](components/home.md) | #5 |
| `wkstream_UI_v2` | Transit landing (`/transit-wheel/{id}`) | the page a person lands on — embedded transit wheel + a two-tab **D1 Birth** / **Current Transit** table; persisted rows only. [`components/spec_Natal_Transit_Comp_Wheel.md`](components/spec_Natal_Transit_Comp_Wheel.md) | — |

Also folded in (the "Missing Web" rollup column): Ṣaḍbala / Bhāva Bala already have sections
7–8 in the AstrologerEvidence plan.

## Design system (decided — enforced, no exceptions)

Extends [`brand.md`](brand.md) / [`design-language.md`](design-language.md). v1's
`AstrologerEvidence.razor.css` (hard-coded `.82rem`, `var(--surface,#fff)` fallbacks) is
**not** compliant and is the first thing v2 fixes.

- **Font family** — Manrope only, every element. No second family.
- **Exactly three sizes** — the existing tokens in `tokens.css`, nothing else in the app:
  - `--font-size-display` — page title only
  - `--font-size-tagline` — section titles, table captions, the lockup tagline
  - `--font-size-control` — table cells, controls, body, nav
  Weight, colour and spacing carry all other hierarchy. *(Values may be retuned for a dense
  table app during the pass; the token names do not change.)*
- **Sunset orange (`--brand-sunset` `#F47A24`) is the highlight / background accent** —
  **button backgrounds** (`Primary`), active section in the index, selected chart in the
  selector, table row hover / selected, focus ring. Text on it is midnight. (`brand.md` updated;
  the old midnight-fill / orange-text rule is retired.)
- **Midnight blue (`--brand-midnight`)** — headings, body text, table structure, nav text.
- **One background colour** — the warm canvas (`--brand-canvas`) for page **and** surface:
  the MudBlazor theme (`Components/IkiastrroTheme.cs`) sets `Background` and `Surface` both to
  canvas, so panels / menus / dropdowns match the app and the artwork (no white boxes);
  elevation shadow separates.
- **Tabular numerals** on every numeric column (degrees, scores, dates, periods).
- **Column width follows the values, not the header.** When a header is longer than its widest
  value (e.g. `House from D1` → `7th`, `11th`): shorten the header if it stays unambiguous, else
  wrap it onto 2–3 lines; horizontal scroll is the last resort and stays inside the table's own
  container. Full rule in [`design-language.md`](design-language.md#tables--headers-vs-horizontal-scroll-implementation-note).
- **Tokens only in CSS** — `var(--…)` from `wwwroot/css/tokens.css`. No raw hex, no named
  colours, no inline `<style>`, no per-component size literals. (`IkiastrroTheme.cs` is the one
  place brand hexes are repeated, because a `MudTheme` is C#; keep it in sync with the tokens.)

## MudBlazor mapping

| v1 | v2 |
|---|---|
| `<table class="dt">` | `MudTable` / `MudSimpleTable` (dense) |
| `<select>` chart picker | `MudSelect` |
| `<details>` / `<summary>` sections | `MudExpansionPanels` / `MudExpansionPanel` |
| hand-rolled sticky top nav | `MudAppBar` in `MudLayout` |
| section index `<nav>` | `MudNavMenu` or an anchor `MudChipSet` |
| `EmptyState` | `MudAlert` / `MudPaper` empty pattern |

## Navigation (v2 route map)

Person detail is **three pages**, all in the header whenever a person is open. `AstrologerEvidence`
is not a hub route any more — its table set is redistributed across the three pages below. Home
absorbs Preferences and Add — no `/preferences`, no `/add`.

### Header bar

`HOME` is the **left-most** app-bar item — a navy pill straddling the app-bar / band edge; then
the per-person tabs `TRANSIT · ALL CHARTS · KEY INFERENCE` in caps; the brand lockup is on the
**right**. Each page's sub-heading (`TRANSIT - D1 BIRTH CHART` / `ALL CHARTS` / the active
Key-Inference header) is caps, left-aligned above the content. (`brand.md`'s lockup copy is
unchanged; only its position moves.)

The person strip and the sub-heading are one compact **band** (page title + descriptor left ·
person name + birth centre · meta chips right — the *21 charts / 611 rows* chip removed). One
content width and gutter (`--maxw` / `--pad-x`) run through the app bar, the band, every page
and the footer. The footer is a **single centred line** — *Dedicated to my guru (Sundari
Hemachandran) — By Ramakrishnan P* (the "By…" small).

**The nav tabs are hidden until a person is opened**, and on Home the centre + right of the
band are empty. Every inner page is per-person. Opening a person reveals the tabs and lands on
`/transit-wheel/{id}`. The band's person name is a **▾ switch** back to Home; `HOME` keeps the
person active. Deep links `/transit-wheel/{id}`, `/charts/{id}`, `/key-inference/{id}` load a
person cold.

### Routes

- `/` — **Home**: a saved-people **search** (never a full list, so it can grow) + Preferences
  disclosure + `Add New`; the Ganesha / Navagraha illustration is the right column. Each match
  is one row; *View chart* opens the person and lands on `/transit-wheel/{id}`. Full spec:
  [`components/home.md`](components/home.md).
- `/transit-wheel/{id}` — **Transit** (the landing), band heading `TRANSIT - D1 BIRTH CHART`.
  Left: the `Natal_Transit_Comp_WheelChart` component ([`components/chart-catalog.md`](components/chart-catalog.md)), fed the
  person's D1 + current-transit points — superseded the `ikiastrro-transit-wheel.svg` placeholder.
  Right: a two-tab table — **D1 Birth**
  (House · Planet · Motion · Degree · Sign · Nakṣatra · Nak. Pad) and **Current Transit**
  (House from D1 · Planet · Motion · Degree · Speed °/day · In sign since · **In-sign motion†** ·
  Next change · **Next-change motion†**). Both sorted Saturn → Jupiter → Rahu → Ketu → Mars →
  Venus → Mercury → Moon → Sun (Lagna first on D1). Full spec + the **†** DB additions
  (`InSignMotion`, `NextChangeMotion` on `tbl_TransitPositionReference`):
  [`components/spec_Natal_Transit_Comp_Wheel.md`](components/spec_Natal_Transit_Comp_Wheel.md).
- `/charts/{id}` — **All Charts**: all 21 divisional charts as plain South-Indian grids (not the
  SVG template), 3 per row, divisor order. **Only the card heading bar is sunset**; card, grid
  and margins are on the canvas. Each cell: full sign name (top-left), house-from-Lagna over
  house-from-Moon (top-right), colour-coded graha glyphs, `LAGNA` label, Lagna box = peach fill
  + sunset corner tick. Replaces the old "Saved Charts" nav slot.
- `/key-inference/{id}` — **Key Inference**, four headers (below).
- `/charts/{id}/south-indian-template` — the one print-style visual (Codex scope)

### Key Inference — 4 headers

| Header | Content |
|---|---|
| **KEY INFERENCE** | 8 sub-tabs: *About Sign* (`tbl_SignAttributes` + sign lord) · *About Planet* (Planet · Kāraka · House Lord · Nature · Conditional rule) · *About Moon* (4 facts, `vw_ChartMoonContext`) · *About Houses* (one house-keyed table = lords + conjunctions + aspects; chart dropdown) · *Planet Dignity* (`vw_ChartPlanetEvidence`; chart dropdown; combust rows sorted under the Sun) · *Planet Strength* (Shadbala) · *House Strength* (Bhava Bala) · *Vargottama* (D1 & D9) |
| **YOGAS** | coverage summary + source variants ([`components/yoga.md`](components/yoga.md)) |
| **TIME PERIOD (DASHA)** | Vimśottari drill-down: Mahā → Antar → Pratyantar, current chain pre-expanded and sunset-highlighted ([`components/dasha-sade-sati.md`](components/dasha-sade-sati.md)) |
| **SATURN TIME PERIOD** | Sade Sati + Kaṇṭaka + Aṣṭama Śani in one ascending table (birth → age 75) with a *Round* column + 1st/2nd/3rd-round filter; current / next window sunset-highlighted. Ashtakavarga out of scope. |

Retired: `/preferences` and `/add` (inline on Home); `/charts/{id}/evidence`,
`/charts/{id}/varga/{code}` and the single `/charts/{id}` hub (redistributed across the two
pages); **`/charts/{id}/life-weeks`** — the 4000-week grid is dropped in v2 (the Vimśottari
timeline is the daśā drill-down). `wkstream_UI_v1`'s already-dropped surfaces stay dropped
unless the pass re-introduces one.

## Workstream mechanics

- Branch `workstream/ui`, worktree `…\ikiastrro.wt\ui`, path scope `src/Ikiastrro.Web/` +
  `tests/Ikiastrro.Web.Tests/` (`STANDARDS.md` §E.1). **Claude Code is primary** — owns the
  shell, layout, pages, routing, `wwwroot`, tokens, and integration to `master`.
- **Codex — one assigned path scope** (§E.2 AGENT-02): **`src/Ikiastrro.Web/Components/Charts/**`**
  — the hand-rolled SVG diagrams, bounded and golden-snapshot-guarded, outside the MudBlazor
  restyle. *(Proposed subtree; confirm before Codex starts.)* Codex does not touch shell /
  pages / routing / tokens; Claude reviews and integrates every Codex change.

## Acceptance matrix

The HTML mockup ([`../artifacts/ui/v2-mockup/chart-evidence-hub.html`](../artifacts/ui/v2-mockup/chart-evidence-hub.html))
is the frozen visual reference — open it and switch views by hash. "Verified in HTML" is not a
completion state — each screen is done only when its row below is fully checked. Test / verify
cells start `☐` and are checked per slice.

| Screen | Reference | Route / component | Data source | Responsive · empty · error | A11y | bUnit / snapshot | Browser verify |
|---|---|---|---|---|---|---|---|
| Home | mockup `#home` | `/` · `Home.razor` | `BirthDetailsRepository` (search only) | ☐ narrow-column · ☐ no-match · ☐ resolver fail | ☐ keyboard search + focus ring | ☐ | ☐ `verify-home-ui.mjs` |
| Transit landing | mockup `#transit` | `/transit-wheel/{id}` · `Natal_Transit_Comp_Wheel.razor` + `Natal_Transit_Comp_WheelChart` ([`components/chart-catalog.md`](components/chart-catalog.md)) | `vw_ChartPlanetEvidence` via `Natal_Transit_Comp_WheelRepository` (D1 Birth) · `tbl_TransitPositionReference` via `GocharaRepository` (Current Transit) | ☑ wide-table scroll-in-container · ☑ no transit rows → CLI hint · ☐ no D1 chart | ☐ tab keyboard nav | ☐ both tabs (`Natal_Transit_Comp_WheelMath` unit-tested; render harness pending) · ☐ golden snapshot not yet minted | ☑ MCP browser smoke 2026-09-10 · ☐ `verify-transit-ui.mjs` headless run |
| All Charts | mockup `#all-charts` | `/charts/{id}` · `AllCharts.razor` + `SouthIndianGrid_Detailed` (re-skinned via token overrides) | `WorkspaceData.Load` — `tbl_ChartResults` + `tbl_Chart_KeyDetails`, all 21 vargas, divisor order | ☑ 3→2→1-per-row reflow · ☑ varga not generated → `EmptyState` card | ☐ grid landmark labels | ☐ per varga (page-DI harness pending) | ☑ MCP browser smoke 2026-09-10 |
| Key Inference | mockup `#key-inference` | `/key-inference/{id}` · `KeyInference.razor` | evidence views + dimension tables (per sub-tab) | ☐ auto table widths, no page scroll · ☐ empty section | ☐ header + sub-tab keyboard nav | ☐ per header | ☐ smoke |
| Preferences | mockup `#home` (disclosure) | inline on Home · `Home.razor` | `AyanamsaDefinition.Catalog` · `localStorage` | ☐ collapse on select · ☐ `localStorage` unavailable → DB default | ☐ disclosure ARIA; disabled "Planned" options not focusable-as-selectable | ☐ | ☐ smoke |

## Verification

- `tests/Ikiastrro.Web.Tests` (bUnit) — a `Verify [x]` needs **all** of: correct persisted-data
  mapping (value ↔ planet / chart / date), a structural or golden snapshot, interaction
  behaviour, empty / loading / error states, keyboard + focus, **and** one real browser smoke
  case. SVG goldens re-minted as components land (`IKIASTRRO_UPDATE_SNAPSHOTS=1`); each diff
  noted against its `FEAT-…` row.
- `Web [x]` means the route is live **and** matches its `docs/ui/components/*.md` spec — not
  merely "it renders".
- A token-lint check (or review gate): no raw hex, no size literals, one font family.
- Live browser smoke test against every route + a dense chart + a recent-birth chart.
- No regression in the 11 `verify-*` CLI modes (UI is read-only over persisted rows).

## Done when

Every row in *What v2 must deliver* is `Web [x]` in `masterproduct.md` under the definition
above; every *Acceptance matrix* row is fully checked; the hub renders all table sections on
the design system with zero token violations; retired routes are gone or recorded; snapshots
re-minted; browser smoke passes; `brand.md` updated for the action-colour change.
