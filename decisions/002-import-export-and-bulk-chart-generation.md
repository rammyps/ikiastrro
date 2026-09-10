---
status: accepted
date: 2026-09-11
workstream: ui (page + flow); database (BirthDetails read/write, batch service)
supersedes: —
---

# 002 — Import / Export of people, and bulk chart generation

## Context

`tools/jkd-interchange/` is a standalone PowerShell utility that moves birth **inputs**
(name, date/time, city/country, lat/long, GMT offset — sex and all calculations excluded)
between Horoscope Explorer Pro `.JKD` (Microsoft Jet 4) files, CSV, and
`dbo.tbl_BirthDetails`. It was always meant to be folded into the product later. That time
is now: the user wants Import / Export in the app, and needs it to cope with importing ~100
people at once.

Two hard facts shape the design:

1. **Jet 4 needs a 32-bit process** with the Jet OLE DB provider installed. The web app is
   64-bit; it cannot open a `.JKD` in-process.
2. **Chart generation is ~3–5 s per person** (Swiss ephemeris + 21 divisional charts +
   Vimśottari dasha + persist). 100 people ≈ 7 minutes. Nominatim geocoding is additionally
   rate-limited to ~1 request/second.

## Decision

### Placement

Import / Export lives as a **toolbar on the `/charts` page** (`SavedCharts.razor`), which is
renamed **"Saved people"** — it is already the people-management surface (sortable list +
per-row Edit / Delete as of commit `29f4dfb`). Toolbar above the table:

- **`Import ▾`** → *From CSV file…* · *From Horoscope Explorer (.JKD)…*
- **`Export ▾`** → *All people as CSV* · *As .JKD*

Not a new route, not the Home Preferences panel (Preferences is for chart-calculation
settings such as ayanāṁśa). If the import UI later grows a column-mapping / preview / dedup
step it becomes a full-screen modal or a `/charts/import` sub-page — the entry point stays
on the Saved people page.

### File formats (from `tools/jkd-interchange/README.md`)

CSV is UTF-8, Excel headers:

```
Name,Sex,DateOfBirth,TimeOfBirth,City,Country,Latitude,Longitude,UtcOffset,IanaTimeZoneId
```

`DateOfBirth` = `yyyy-MM-dd`, `TimeOfBirth` = `HH:mm:ss`, `UtcOffset` = `+HH:mm` / `-HH:mm`.
`Latitude,Longitude,UtcOffset,IanaTimeZoneId` may be blank. `.JKD` `Person` table columns:
`Name, Sex, Day, Month, Year, Hour, Minutes, Seconds, AMPM, Country, LatDeg, LatMT, TextNS,
LongDeg, LongMT, TextEW, City, GMTDIFF`.

### Geocoding on import — skip it

CSV **and** `.JKD` carry `Latitude / Longitude / UtcOffset`. Import **uses those columns
directly and does not geocode**. A row is only sent to `IPlaceResolver` when its coordinates
are blank. So Nominatim's 1-req/sec limit never applies to file import — it only affects the
one-at-a-time Home form.

### Jet 4 handling — shell out to the existing script

For `.JKD` import/export the app invokes `tools/jkd-interchange/jkd-interchange.ps1` (which
already self-relaunches under 32-bit Windows PowerShell) as a child process and parses its
stdout summary. Reuses proven code; the only new dependency is a process spawn. CSV
import/export is done in-process in C#. (A dedicated in-solution 32-bit helper exe was
considered and deferred — more work for the same result.)

### Bulk chart generation — lazy now, background queue later

**Phase 1 (build now):**

- **Import = INSERT rows only.** Instant. The person appears in the list immediately.
- The Saved people list shows a **`● built / ○ not built`** indicator per person
  (D1 `tbl_ChartResults` row present or not).
- The transit page's existing *"Generate a D1 chart for this person first"* empty state
  gains a real **Generate** button — one person, ~4 s, on demand.
- A **"Build missing (N)"** button on the Saved people page runs a **foreground** loop
  (`await ChartGenerationService.GenerateAll` per person) with a progress bar, **capped**
  (e.g. 25 per press, then "Continue") — honest about the time, no new infrastructure.

**Phase 2 (only if bulk import becomes routine):**

- A hosted `BackgroundService` drains a queue, building one chart at a time, with SignalR
  progress on the Saved people page ("37 / 100 built"). `ChartGenerationService.GenerateMissing`
  already exists for the per-person step. The Phase-1 lazy path stays as the fallback.

## Consequences

- `/charts` route unchanged; page component + title renamed; `docs/ui/MASTER.md` screen
  inventory + [`components/saved-people.md`](../docs/ui/components/saved-people.md) updated.
- New `BirthDetailsRepository` bulk-insert path; a `PeopleImportService` (CSV parse + row
  validation + dedupe by Name against `UX_BirthDetails_Name`).
- `.JKD` import/export requires PowerShell + 32-bit Jet on the host — documented as a
  platform prerequisite, degrades to "CSV only" where absent.
- First open of an un-built imported person is slow (~4 s) until Phase 2 or a manual
  "Build missing" pass.
