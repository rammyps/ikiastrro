---
last_updated: 2026-09-09
workstream: ui
component: Home
route: /
togaf: C — component spec
---

# Component — home & entry

In `wkstream_UI_v2` **one page** at `/` does everything — it absorbs **Preferences** and
**Add person**; there is no `/preferences` or `/add` route. Nothing on Home navigates until a
chart is generated. Two states of the same page, toggled in place:

- **default** — Preferences disclosure + searchable Name + saved-people list
- **Add-New** — the entry fields, hidden at start, revealed in place when `＋ Add New` is picked

## Default state — select / search

Shell: MudBlazor `MudLayout` / `MudAppBar` with the brand lockup + tagline
([`../brand.md`](../brand.md)). The canonical main screen is the visual authority.

Layout is two columns on the warm canvas — controls left, art right — plus the dedication
footer. The Ganesha / Navagraha illustration is a **first-class part of the Home layout**
(right column), not tied to any toggle.

- **Preferences — top-left of the content area.** A collapsed disclosure (`MudCollapse` behind
  a text button). Expands **in place on Home**; nothing navigates. Three selector groups:
  1. **Choose Ayanāṁśa** — `MudSelect` over `AyanamsaDefinition.Catalog` (**21 systems**);
     default is the active `tbl_Rule_Ayanamsa` row, **Lahiri, fixed**, shown as *Default (Lahiri)*.
     The choice is passed to `ChartGenerationService.GenerateAll(birth, ayanamsa)` for the next
     generation. (`docs/ui/MASTER.md` NFR-UI-03)
  2. **Choose Chart style** — `MudSelect`: *South Indian* (default) · *North Indian* ·
     *West Indian*. All three are listed so the setting is forward-compatible; **only the
     South-Indian renderer exists today** — North / West Indian fall back to it until their
     `Components/Charts/**` renderers ship (ROADMAP *Later*; `docs/ui/MASTER.md` NFR-UI-02).
  3. **Choose Language** — `MudSelect`: *English* (default) · *Tamil* (planned — Tamil script or
     transliteration TBD). Deferred i18n (`docs/ui/MASTER.md` NFR-UI-04).

  **Selecting any preference applies it and collapses the panel** (the dropdowns hide again).
  Persistence: per-browser (`localStorage`) for now; a DB-backed default is a `database`-workstream
  follow-up. **Header / sub-tab order is fixed** for now — user-reorderable tabs is a deferred
  NFR (`docs/ui/MASTER.md` NFR-UI-01).
- **Discover Your Path** — heading (`--font-size-display`), no subheading.
- **Name — a search, never a full list.** `MudAutocomplete` over saved-people names; the saved
  list is only ever *searched*, so it can grow without breaking the page.
  - Typing filters saved people (contains match); each match renders as **one row** — name ·
    birth line · *View chart →* — styled like the person rows.
  - **`＋ Add New`** sits below the results (always available).
  - **Selecting a person identifies the active person** → the `TRANSIT`, `ALL CHARTS` and
    `KEY INFERENCE` nav tabs appear in the header (hidden until now — every inner page is
    per-person) and it **lands on `/transit-wheel/{id}`**. The context band's person name is a
    `▾` control back here to switch person; `HOME` keeps the person active.
- The **Ganesha / Navagraha illustration** is the right column of the two-column Home layout
  (`../brand.md` — a first-class part of the page, not tied to any toggle).

## Header &amp; band on Home

`HOME` is the left-most item in the app bar; the brand lockup is on the right; the nav tabs are
hidden. The context band shows only the `HOME` title + descriptor on the left — **centre
(person) and right (meta chips) are empty until a person is opened.** The `21 charts / 611 rows`
chip is removed everywhere. Footer is a single centred line:
*Dedicated to my guru (Sundari Hemachandran) — By Ramakrishnan P* (the "By…" in small type).

## Add-New state — entry fields revealed in place

`＋ Add New` sets `_adding = true`: the **Name search stays visible**, and the entry fields
appear below it under a *New person* caption. Whatever was typed into the search seeds the new
person's Name. `Cancel` hides the fields again. Fields, in order:

| Field | Control | Notes |
|---|---|---|
| Name | `MudTextField` | required |
| Sex | `MudSelect` (option box) | Male · Female |
| Date of Birth | `MudDatePicker` | required |
| Time of Birth | `MudTimePicker` | **required** — critical for the Lagna; no chart without it |
| City | `MudTextField` | required; feeds `IPlaceResolver` |
| Country | `MudTextField` | required; **the last step** |

**Flow:** completing **Country** is the trigger — on a valid form it resolves the place,
runs `ChartGenerationService.GenerateAll` (with the chosen ayanāṁśa), sets the new row as the
active person (revealing the nav tabs), and navigates to **`/charts/{id}`**. A `Generate Chart`
button is the explicit / accessible fallback.

Geocoding-failure fallback (manual lat / long / offset) is a follow-up, not in the first cut.

## Design system

Per [`../wkstream_UI_v2.md`](../wkstream_UI_v2.md): Manrope only; the three size tokens
`--font-size-display` / `--font-size-tagline` / `--font-size-control` — nothing else;
**sunset orange (`--brand-sunset`) is the button background** and the active/hover/focus
accent; midnight blue for text and structure; tokens only, no raw hex or px literals.
`Home.razor.css` is rewritten from scratch against these rules (the v1 file is the worst
offender — stacked override blocks, raw px, `!important` MudBlazor patches).

## Retired from v1

`+ Add new` as a route jump; the `/?preferences=1` query-string panel; the native `<select>` /
`<input>` / `<details>` markup; the `landing-nav` custom nav (→ `MudAppBar`).
