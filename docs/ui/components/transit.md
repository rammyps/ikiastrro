---
last_updated: 2026-09-10
workstream: ui
component: Transit
status: v2 — approved (not yet built)
route: /transit-wheel/{id}
togaf: C — component spec
---

# Component — transit (D1 birth chart landing)

> **v2 — approved, not yet built.** This supersedes [`transit-wheel.md`](transit-wheel.md) as
> the target for `/transit-wheel/{id}`. `transit-wheel.md` stays the live spec until this ships.
> The right-hand two-tab table and the `tbl_TransitPositionReference` motion columns are
> **Codex's** work (`Components/Charts/**` + the reference-table merge); Claude places the page
> and integrates.

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

## † Database contract — ingress / next-change motion (Codex + `database`)

`tbl_TransitPositionReference` stored **only the current `MotionDirection`**. Slow planets can
enter a sign retrograde and later turn direct (or vice-versa), so the current direction alone
describes neither the ingress nor the exit. The Current Transit tab needs two more:

| New column | Type | Meaning |
|---|---|---|
| `InSignMotion` | `VARCHAR(10) NULL`, CHECK `IN ('Direct','Retrograde','Stationary')` | motion the graha had **when it entered its current sign** (`InSignSinceUtc`) |
| `NextChangeMotion` | `VARCHAR(10) NULL`, same CHECK | motion it will have **at the next sign change** (`NextChangeUtc`) |

**Status:**

- **DDL — done.** `db/055_add_transit_boundary_motion.sql` (Codex) adds both columns + CHECK
  constraints, idempotent, self-recording. **To fold forward:** `db/45_create_transit_position_reference.sql`
  (the table's own CREATE) is not yet in the `db/ikiastrro.sql` baseline — fold both `45` and
  `055` in the same pass.
- **Merge + model — pending (Codex).** `GocharaRepository.SaveSnapshots` MERGE and the
  `PlanetTransitSnapshot` record must carry `InSignMotion` / `NextChangeMotion`.
- **Flag.** `GocharaRepository` hard-codes `AyanamsaRuleId = 7` (Jagannatha) while migration
  `054` made Lahiri the DB default — reconcile, or document why, in the same change.
- **Open question (`database`).** Existing `tbl_TransitPositionReference` rows are written
  lazily on transit-page render; they carry NULL motion fields until re-fetched. Decide: a
  one-off backfill, or accept lazy refill.

Noted in [`../../database/schema.md`](../../database/schema.md).

## Rendering

MudBlazor: `MudTabs` for the two tabs, `MudTable` (`table-layout: auto`, the wide transit
table scrolls inside its own container). Shared tokens; dates right-aligned. Sort order for
**both** tables — by how long a graha holds a house:
**Saturn → Jupiter → Rahu → Ketu → Mars → Venus → Mercury → Moon → Sun** (Lagna first on D1).

## Verification

bUnit snapshot of both tabs against a seeded person; transit rows reconciled against
`GocharaRepository` output; the new `InSignMotion` / `NextChangeMotion` values checked against a
Swiss-ephemeris motion trace across a known Saturn retrograde ingress.
