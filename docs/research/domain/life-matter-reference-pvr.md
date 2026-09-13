---
last_updated: 2026-09-12
---

# Life-matter reference — bridging PVR's areas-of-life, houses, and karaka

Normalized form of rammyps's "Area-of-life master table" worksheet. Backs
`db/087_seed_life_matter_reference.sql` — `tbl_Rule_LifeMatterReference`, 96 rows across
10 categories.

## What this bridges

PVR gives three separate axes for reading a chart, each already partly modelled in this
project, but never tied together at the grain of a *specific* interpretive matter:

| Axis | PVR source | Existing table |
|---|---|---|
| Which divisional chart shows this life area | Table 11, §6.3 | `tbl_Dim_LifeArea` (migration 30) |
| Which house (from Lagna) shows this matter | §7.2 | `tbl_Rule_HouseSignification` (migration 31) |
| Which planet is the natural/chara significator, and which house counted from *it* also shows the matter | ch. 8 (naisargika) / Table 12, §7.3.9 (graha lagna) | `tbl_Rule_Naisargika_Karakatwas` (migration 086) / `tbl_Rule_HouseReferenceMatter` (migration 32) |

PVR states the bridging *method* explicitly (§7.3, pg 69–70): *"We have to note the area
of life seen in the divisional chart under examination. We have to choose the meanings of
houses that are relevant in that area of life."* He works this out with exactly one
concrete example (the 4th house: education→D-24, vehicle→D-16, house→D-4, mother→D-12) and
otherwise leaves it as an exercise. `tbl_Rule_LifeMatterReference` is that exercise, carried
out for 96 specific matters across 10 categories.

## Provenance per row — `BasisCode`

Every row is tagged with how solid its citation is — this is a synthesis, not a book
transcription, and the table says so at the row level:

| `BasisCode` | Meaning | Count |
|---|---|---|
| `PVR_DIRECT` | An exact statement found in the raw PVR text | 9 |
| `PVR_CROSSVALIDATED` | The (karaka, house) pair independently matches data already seeded from the book (the naisargika grid or Table 12), even though this exact "specific matter" phrasing is the worksheet's own | 80 |
| `PROJECT_SYNTHESIS` | Neither of the above — a reasoned extension of PVR's method to a case the book doesn't spell out (`SRC_IKIASTRRO_SYNTHESIS`, a new `tbl_Dim_Source` row registered by this migration) | 7 |

### The `PVR_DIRECT` rows and their citations

- **Marriage / Spouse / Marital happiness / Interaction with others** (7th from Venus) —
  pg 83, verbatim: *"the 7th from Venus (and not Venus himself) shows husband"*, used for
  both male and female charts.
- **Individual spouse-role** (Dārakāraka) — pg 83, verbatim: *"We do not take the 7th from
  DK for spouse, but DK himself shows spouse."*
- **Particular child-role** (Putrakāraka) — generalises the same pg 83 rule ("chara karakas
  are also similar to sthira karakas in this aspect") from DK to PK.
- **Public manifestation of marriage** (Upapada Lagna) — UL as the arudha of the 7th is
  PVR's own stated reference for the visible/social side of marriage.
- **Academic recognition** — PVR's stated distinction between actual ability (5th from
  Mercury/Jupiter/Lagna) and perceived achievement (5th from Sun or Arudha Lagna).
- **Destruction/death** — D-11's Table 11 description is verbatim "Death and destruction".

### Correction applied before seeding

The worksheet originally read "houses from DK" / "examine PK and houses from it" for the
individual spouse-role and particular child-role rows. PVR explicitly contradicts this
(pg 83): chara karakas (DK, PK, MK, PiK, ...) represent the person **directly**, the same
way sthira karakas do — unlike naisargika karakas, where a house *is* counted from the
karaka. Both rows were corrected to "DK/PK himself shows..." before seeding, with the
correction and citation recorded in each row's `CalculationNarrative`.

## Schema notes

- `PrimaryLifeAreaId` FKs to `tbl_Dim_LifeArea` only when `PrimaryChartsText` is a single,
  unambiguous chart (e.g. `D9`); it's `NULL` for compound picks like `D6/D8` (23 of 96 rows) —
  the compound text is preserved in `PrimaryChartsText` rather than split.
- `NaisargikaGrahaId` FKs to `tbl_Planets` only when `KarakaText` names one fixed graha;
  `CharaKarakaCode` (`DK`, `PK`, ...) covers the two chara-karaka rows instead. Both are
  `NULL` together for compound karakas (21 of 96 rows) — again, the text is kept as given
  rather than force-split into an artificial single value.
- `HouseFromLagnaText` / `HouseFromKarakaText` keep PVR's derived-house phrasing verbatim
  where it's not a simple house number (`Upapada Lagna`, `9th from 5th`, `12th from the
  vehicle house`) rather than normalizing it — some of these are themselves genuine PVR
  techniques ("houses from houses", confirmed via `H03_VEHICLE_HOUSE_EXPENSE`'s "12th from
  the 4th" already in `tbl_Rule_HouseSignification`).

## Known gap found while researching this

`dotnet run -- verify-sources` currently crashes (not fails — an unhandled exception) on a
**pre-existing, unrelated bug**: its dynamic `SourceRefCode`-column scan (`Program.cs`,
the `verify-sources` block) discovers every table with a `SourceRefCode` column via
`sys.tables` without schema-qualifying it, then hardcodes `dbo.[{tbl}]` when querying it.
Migration 056's `research.tbl_Dim_SourceReferencePlanetText` lives in the `research` schema,
so the check dies with "Invalid object name 'dbo.tbl_Dim_SourceReferencePlanetText'" before
it ever reaches this migration's table. Confirmed unrelated to this work: `SourceRefCode`
resolution for `tbl_Rule_LifeMatterReference` was checked manually instead (0 unresolved).
Not fixed here — belongs to whatever branch owns the `research.*` schema work.

## Downstream

- No C# engine reads this table yet — it's reference data, same stage the naisargika karaka
  tables were seeded at.
- Several referenced techniques have no engine at all yet (Ghati Lagna, Upapada Lagna, SAV /
  Ashtakavarga) — see `pvr-coverage.md` ch. 5 / ch. 12. The table is still valid reference
  data ahead of those engines, same precedent as `tbl_Rule_SpecialLagnaTimeRate`.
- `docs/research/domain/pvr-coverage.md` should get a line for this once a broader ch. 6/7/8
  reconciliation pass happens; not added as its own row yet since this migration spans three
  chapters rather than one.
