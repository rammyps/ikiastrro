---
last_updated: 2026-09-10
workstream: ui
component: Transit
route: /transit-wheel/{id}
togaf: C — component spec
---

# Component — transit (D1 birth chart landing)

The page a person **lands on** after being opened from Home. Band heading is
**`TRANSIT - D1 BIRTH CHART`**; there is no separate in-page "Transit" strip.

Two columns:

- **Left — the transit wheel.** The existing hand-authored SVG
  `UI_SVG_Templates/ikiastrro-transit-wheel.svg` (natal rāśi ring + zodiac-nakṣatra grid +
  transit ring), embedded where the plain D1 grid used to sit. The wheel itself stays Codex
  scope (`src/Ikiastrro.Web/Components/Charts/**`); this page only places it.
- **Right — a two-tab details table:** `D1 Birth` · `Current Transit`.

Every row is a persisted position; the page adds no calculation
([`../../architecture/domain-contracts.md`](../../architecture/domain-contracts.md)).

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

`tbl_TransitPositionReference` (via `GocharaRepository`), joined to the person's D1 for the
first column.

| Column | Source field |
|---|---|
| **House from D1** | derived — houses between the graha's **natal** house (D1) and its **transit** house; i.e. count from the D1 house to the transit house |
| Planet | `PlanetId` → name |
| Motion | `MotionDirection` (current) |
| Degree | `SignId` + `DegreeInSign` |
| Speed °/day | `SpeedDegreesPerDay` |
| In sign since | `InSignSinceUtc` |
| **In-sign motion †** | **new — see below** |
| Next change | `NextChangeUtc` |
| **Next-change motion †** | **new — see below** |

A small caption shows the transit timestamp: *as of `<AsOfUtc>` (local)*.

## † Database change required

`tbl_TransitPositionReference` currently stores **only the current `MotionDirection`**. This
page needs two more, both for the transit chart:

| New column | Meaning |
|---|---|
| `InSignMotion` | the motion direction the graha had **when it entered its current sign** (`InSignSinceUtc`) |
| `NextChangeMotion` | the motion direction it will have **at the next sign change** (`NextChangeUtc`) |

Slow planets can enter a sign retrograde and later turn direct (or vice-versa), so the current
`MotionDirection` alone does not describe the ingress or the exit. Add both to the merge in
`GocharaRepository` and to the reference table DDL (`db/`), and note them in
[`../../database/schema.md`](../../database/schema.md).

## Rendering

MudBlazor: `MudTabs` for the two tabs, `MudTable` (`table-layout: auto`, the wide transit
table scrolls inside its own container). Shared tokens; dates right-aligned. Sort order for
**both** tables — by how long a graha holds a house:
**Saturn → Jupiter → Rahu → Ketu → Mars → Venus → Mercury → Moon → Sun** (Lagna first on D1).

## Verification

bUnit snapshot of both tabs against a seeded person; transit rows reconciled against
`GocharaRepository` output; the new `InSignMotion` / `NextChangeMotion` values checked against a
Swiss-ephemeris motion trace across a known Saturn retrograde ingress.
