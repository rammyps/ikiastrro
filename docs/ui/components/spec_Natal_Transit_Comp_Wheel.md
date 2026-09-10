---
last_updated: 2026-09-10
workstream: ui
component: Natal_Transit_Comp_Wheel
route: /transit-wheel/{id}
togaf: C — component spec
---

# Specification — Natal Transit Comparison Wheel

This is the single living specification for the chart. Update this document in place as the
design or implementation changes; do not create versioned successor specifications. The chart
and its directly related implementation files use the canonical name
**`Natal_Transit_Comp_Wheel`**. The public route remains `/transit-wheel/{id}`.

Current delivery state belongs in the project feature register and Git history, not in a
version banner in this specification.

The page a person **lands on** after being opened from Home. Band heading is
**`TRANSIT - D1 BIRTH CHART`**; there is no separate in-page "Transit" strip. It restores
the v1 comparison interaction: one selected instant is controlled by a current-transit date,
Mahadasha, or Antardasha selection.

Two columns:

- **Top — controls.** Current transit date, Mahadasha, and Antardasha are arranged in one
  horizontal row; at narrow widths they stack vertically.
- **Middle — centred transit wheel.** `Natal_Transit_Comp_WheelChart` is centred and rendered
  up to 810 px wide (1.5× the former 540 px placement). It remains responsive below that width.
  The chart component (Codex scope
  `src/Ikiastrro.Web/Components/Charts/**`) — a four-ring data-driven `<svg>` (transit / nakṣatra
  / natal / core), natal glyphs on the inner ring and current-transit glyphs on the outer,
  house numbers from the stored Ascendant. All nine transit grahas — Sun, Moon, Mars,
  Mercury, Jupiter, Venus, Saturn, Rahu, and Ketu — move to their calculated longitude for
  the selected instant. Superseded the hand-authored
  `UI_SVG_Templates/ikiastrro-transit-wheel.svg` placeholder. Full contract:
  [`chart-catalog.md`](chart-catalog.md). This page only places it and supplies the points.
- **Bottom, full span — a two-tab details table:** `D1 Birth` · `Current Transit`. Tables fit
  the available width using fixed layout and wrapped headings; there is no internal scrollbar.

Natal rows and dasha periods are persisted. Transit longitude and speed are calculated by
`GocharaRepository` for the selected instant; stored slow-planet boundary history supplies
ingress and next-change metadata where available.

## Date and dasha interaction

- The date initializes to the current date in IST and resolves the active Mahadasha and
  Antardasha from the saved Vimshottari tree.
- Changing the date updates both dasha selectors and recalculates all nine transit markers.
- Choosing a Mahadasha moves the selected instant to that period's exact stored start and
  selects its active Antardasha.
- Choosing an Antardasha moves the selected instant to that sub-period's exact stored start.
- Each dasha option includes its ruling planet glyph, name, and date range.
- The same active instant drives the wheel, timestamp, and Current Transit table; the three
  controls must never drift into independent states.
- The wheel centre contains only the selected date and India time (`IST`); it does not show
  the product name, `NATAL ↔ TRANSIT`, Lagna text, or UTC time.

## Tab 1 — D1 Birth

`vw_ChartPlanetEvidence` (ChartType `D1`).

| Column | Source field |
|---|---|
| House | `HouseNumberFromLagna` |
| Planet | `Planet` (Lagna row first) |
| Motion | `IsRetrograde` → Direct / Retro (+ "· combust" from `IsCombust`) |
| Degree | `DegreesInSignDisplay` |
| Sign | `Sign` |
| Nakṣatra | `Nakshatra` |
| Nak. Pad | `NakshatraPada` |

## Tab 2 — Current Transit

Calculated all-graha positions (via `GocharaRepository`), joined to the person's D1 for the
first column. Slow-planet boundary fields come from `tbl_TransitPositionReference` when present.

| Column | Source field |
|---|---|
| **House from D1** | derived — houses between the graha's **natal** house (D1) and its **transit** house; i.e. count from the D1 house to the transit house |
| Planet | `PlanetId` → name |
| Motion | `MotionDirection` (current) |
| Degree | `SignId` + `DegreeInSign` |
| Speed °/day | `SpeedDegreesPerDay` |
| In sign since | `InSignSinceUtc` |
| **In-sign motion †** | `InSignMotion` — direction at ingress |
| Next change | `NextChangeUtc` |
| **Next-change motion †** | `NextChangeMotion` — direction at the next boundary |

The duplicate `as of … IST` caption above the Current Transit table is omitted because the
selected date and India time already appear in the wheel centre.

## † Database contract — ingress / next-change motion (Codex + `database`)

`tbl_TransitPositionReference` stored **only the current `MotionDirection`**. Slow planets can
enter a sign retrograde and later turn direct (or vice-versa), so the current direction alone
describes neither the ingress nor the exit. The Current Transit tab needs two more:

| New column | Type | Meaning |
|---|---|---|
| `InSignMotion` | `VARCHAR(10) NULL`, CHECK `IN ('Direct','Retrograde','Stationary')` | motion the graha had **when it entered its current sign** (`InSignSinceUtc`) |
| `NextChangeMotion` | `VARCHAR(10) NULL`, same CHECK | motion it will have **at the next sign change** (`NextChangeUtc`) |

**Implementation:**

- **DDL — done.** `db/055_add_transit_boundary_motion.sql` (Codex) adds both columns + CHECK
  constraints, backfills existing reference rows from boundary events, and is idempotent and
  self-recording. The columns and constraints are also folded into
  `db/45_create_transit_position_reference.sql` so a recreated table has the complete shape.
- **Merge + model — done.** `PlanetTransitSnapshot`, boundary-event queries, and
  `GocharaRepository.SaveSnapshots` carry `InSignMotion` / `NextChangeMotion`; the page renders
  the values. Existing slow-planet rows are refreshed when their selected instant is loaded.
- **Flag.** `GocharaRepository` hard-codes `AyanamsaRuleId = 7` (Jagannatha) while migration
  `054` made Lahiri the DB default — reconcile, or document why, in the same change.
- Existing stored slow-planet rows are backfilled by migration 055 and refreshed whenever the
  page loads their selected instant.

Noted in [`../../database/schema.md`](../../database/schema.md).

## Rendering

MudBlazor: `MudTabs` for the two tabs, `MudTable` (`table-layout: fixed`; headings wrap and
the table has no internal horizontal scroll). Shared tokens; dates right-aligned. Sort order for
**both** tables — by how long a graha holds a house:
**Saturn → Jupiter → Rahu → Ketu → Mars → Venus → Mercury → Moon → Sun** (Lagna first on D1).

### Planet placement and collision rule

- Planet markers must never overlap one another, their labels, or the ring boundaries.
- When two or more planets occupy the same or visually adjacent longitude, retain their true
  longitude alignment but place their markers on separate radial rows.
- Each planet gets one complete row of its own; do not combine multiple planet glyphs or labels
  into a single row.
- The row order must be deterministic so the same inputs always render identically. Use the
  standard slow-to-fast order: Saturn, Jupiter, Rahu, Ketu, Mars, Venus, Mercury, Moon, Sun;
  Lagna precedes the natal rows.
- Collision spacing must work independently for the natal and transit rings and at every
  responsive size supported by the page.
- Collision layout uses deterministic radial lanes while preserving every marker's true
  longitude angle; natal and transit points use separate lane sets.

## Verification

bUnit snapshot of both tabs against a seeded person; chart snapshot verifies that conjunctions
use separate, non-overlapping planet rows; transit rows reconciled against
`GocharaRepository` output; the new `InSignMotion` / `NextChangeMotion` values checked against a
Swiss-ephemeris motion trace across a known Saturn retrograde ingress.
