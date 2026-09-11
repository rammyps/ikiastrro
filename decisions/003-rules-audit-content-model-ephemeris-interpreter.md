---
status: accepted
date: 2026-09-11
workstream: database (Parts A, B-schema); cli (Part C); ui (Part B-component)
supersedes: —
---

# 003 — Rules-table audit, standard/short interpretation content, Swiss Ephemeris extraction

## Context

Three asks bundled into one planning pass, scoped by three parallel research passes:

1. **Rules audit.** `docs/database/rules-engine.md`'s per-table "Live?" column is wrong in both
   directions — it undersells `tbl_Rule_VargaScheme`/`AgeState`/`WakefulnessState`/`SubPlanet*`/
   `Ayanamsa` (genuinely DB-wired) as one table, and it mislabels `GrahaDignity`,
   `CompoundRelationship`, `DigBala`, `ShadbalaComponent`, `BhavaBalaComponent`,
   `SpecialLagnaFraction/TimeRate` as "live" when **zero** C# code reads them — those engines
   (`DignityEngine.cs`, `PvrDignityEvaluator.cs`, `RelationshipEngine.cs`, `CombustionEngine.cs`,
   `ShadbalaCalculator.cs`, `HoraLagnaCalculator.cs`) are 100% hardcoded dictionaries, same as
   the day they were written.
2. **Standard/short content.** Nothing like this exists today. The closest thing,
   `tbl_Rule_Yoga.ShortFormationRule` (`db/079`, prior session pass), is a *trigger rule*
   ("what fires this yoga"), not *interpretation* ("what it means for the person") — those are
   different content, and no interpretation copy (long or short) exists anywhere in the schema.
   `CalculationNarrative` is developer-facing provenance prose, not reader copy.
3. **Swiss Ephemeris "interpreter."** Already wired in — `SwissEphNet 2.8.0.2`
   (`src/Ikiastrro.Core/Engines/Astronomy/SwissEphemerisProvider.cs`), Moshier analytical mode,
   genuinely computing planet positions, Ascendant, and sunrise/sunset (not a stub). None of the
   documented gaps need a *new* ephemeris capability — they need DB-wiring (above) or
   content-authoring (interpretation text; classical constants like exaltation degrees that are
   already hardcoded in C# and just need transcribing, same pattern as every other rule table).

## Decision

### Part A — Rules audit: fix the docs, build the wiring backlog

Fix `docs/database/rules-engine.md`'s "Live?" column per-table (live / mirror-only-verified-by-
CLI / orphaned-no-repository). Flag `tbl_Rule_Karaka`, `tbl_Rule_VimsopakaWeight`,
`tbl_Rule_YogaValidationDefinition`, `tbl_Rule_DashaApplicability` as unseeded (tables exist,
zero rows). Fix `docs/database/MASTER.md`'s "Live rule table" line to list all ~6 genuinely-wired
tables; note `tbl_Rule_Exaltation` still doesn't exist but is a normal Phase-1 job (exaltation
degrees are already hardcoded in `RamanYogaBatchFiveEvaluator.DeepExaltation` /
`RamanDhanaYogaEvaluator`, ready to transcribe, same pattern as `db/079`). Append a postscript to
`decisions/001-star-schema-rules-engine.md` noting its cited class names
(`ClassicalRelationships.cs`/`ClassicalCombustion.cs`/`ClassicalDignity.cs`) were renamed in the
2026-09-02 engine reorg to `RelationshipEngine.cs`/`CombustionEngine.cs`/`DignityEngine.cs`.

New wiring backlog, ordered by cost:

| Tier | Domain | Why this tier |
|---|---|---|
| Quick win — Phase 1 done & CLI-verified, just needs the calculator rewired | Dignity (`tbl_Rule_GrahaDignity` + `tbl_Rule_CompoundRelationship` → `DignityEngine`/`PvrDignityEvaluator`), Aspects (`tbl_Rule_AspectOffset` → `RelationshipEngine`), Combustion (`tbl_Rule_CombustionOrb` → `CombustionEngine`) | Seeded, CLI-cross-checked already (`show-rules`); this is literally the last step of the documented Phase-1→Phase-2 handoff |
| Needs new engine code, table already seeded | Ashtakavarga (`AshtakavargaCalculator` — tracked separately, its own standing plan), Special Lagnas beyond HL (`tbl_Rule_SpecialLagnaFraction/TimeRate`), Houses (`tbl_Rule_HouseSignification`/`HouseAttribute`/`HouseReferenceMatter` → currently `LifeAreaMap` hardcodes a subset) | Table exists and is seeded, but the reading code was never written, not just never rewired |
| Blocked on a cited source before the shape is even final | `tbl_Rule_VimsopakaWeight` (four varga-group weights, source not picked), `tbl_Rule_Karaka` (Sthira/Naisargika still hardcoded in `LifeAreaMap`, migration 030 `tbl_Dim_HouseSignification`/`PlanetSignification`/`PlanetHouseKaraka` designed-not-applied), `tbl_Rule_DashaApplicability` (empty, no source cited) | Seeding itself is blocked, wiring is moot until then |

### Part B — Standard/short interpretation content

New table `db/080_create_content_interpretation.sql`:

```sql
CREATE TABLE dbo.tbl_Content_Interpretation (
    Id            INT IDENTITY PRIMARY KEY,
    RuleSetId     INT NOT NULL REFERENCES dbo.tbl_Rule_Sets(Id),
    SubjectType   VARCHAR(30)  NOT NULL,   -- 'YOGA' | 'DIGNITY' | 'HOUSE' | 'GRAHA_IN_SIGN' | ...
    SubjectCode   VARCHAR(60)  NOT NULL,   -- e.g. 'YOGA_GAJAKESARI', 'EXALTED', 'HOUSE_1'
    StandardText  NVARCHAR(500) NOT NULL,  -- full reader-facing paragraph (desktop)
    ShortText     NVARCHAR(160) NOT NULL,  -- compact version (tablet/mobile, badges, list rows)
    SourceRefCode VARCHAR(40)  NULL,
    IsActive      BIT NOT NULL DEFAULT 1,
    CONSTRAINT UQ_Content_Interpretation UNIQUE (RuleSetId, SubjectType, SubjectCode),
    CONSTRAINT CK_Content_Interpretation_Src CHECK (SourceRefCode IS NULL OR SourceRefCode LIKE 'SRC[_]%')
);
```

`(SubjectType, SubjectCode)` is a generic key so one table serves every domain without a new
table per domain. First content pass seeds `SubjectType='YOGA'` rows for the 146 already-grounded
`YogaCode`s from `db/079` — actual copy authoring is a separate content-writing pass; rows without
real copy stay unseeded, same "deliberately NULL pending a cited source" discipline used
throughout this project.

Responsive side: today there is no shared breakpoint contract (`tokens.css` has none; 7
`.razor.css` files each pick their own pixel breakpoint: 480/620/700/760/900/1000). Add
`--bp-tablet: 900px` / `--bp-mobile: 480px` to `src/Ikiastrro.Web/wwwroot/css/tokens.css`, and a
new shared component `src/Ikiastrro.Web/Components/Shared/InterpretationText.razor`
(`StandardText`/`ShortText` params) that renders both and hides one via CSS at `--bp-tablet` —
matching the existing hide/show precedent (`SouthIndianGrid_Detailed.razor.css`), not a
truncate/tooltip pattern. Document the token pair + component pattern in
`docs/ui/design-language.md`. Existing 7 files keep their ad hoc breakpoints — no forced
migration, opportunistic only.

### Part C — Swiss Ephemeris Interpreter (new sub-project)

New project `src/SwissEphemeris.Interpreter/SwissEphemeris.Interpreter.csproj` — class library,
domain-agnostic (no `Ikiastrro.*` types in its public surface, plain records/primitives only), so
the folder is droppable into another .NET project as-is. Target a widely compatible TFM
(`net8.0`, not `net10.0`-only) if `SwissEphNet 2.8.0.2` supports it.

Extraction, not a rebuild — moves out of `Ikiastrro.Core`:
- `SwissEphemerisProvider.GetSiderealPositions` → plain `PlanetPosition(string Body, double
  LongitudeDeg, double LatitudeDeg, double SpeedDegPerDay)` API.
- `SwissEphemerisProvider.GetSunTimes` → sunrise/sunset API, same flags preserved exactly
  (`SEFLG_MOSEPH | SE_BIT_DISC_CENTER | SE_BIT_NO_REFRACTION` — no behavior change; CLI
  `verify-*` tolerance checks depend on today's exact output).
- `AyanamsaDefinition.Catalog` → a generic sidereal-mode catalog (already domain-neutral).
- Ascendant/house-cusp retrieval — Whole Sign only for now (matches today), but shaped around a
  `HouseSystem` enum so other systems are additive later, not breaking (`swe_houses_ex` already
  returns the full cusp array today; it's just discarded after `ascmc[0]`).

`Ikiastrro.Core.csproj` drops its direct `SwissEphNet` reference and references
`SwissEphemeris.Interpreter` instead; `SwissEphemerisProvider` in Core becomes a thin adapter
mapping the interpreter's plain records to ikiastrro's domain types. Call sites elsewhere don't
change. New `tests/SwissEphemeris.Interpreter.Tests` carries the Moshier-accuracy spot-checks;
existing CLI `verify-*` checks stay as the ikiastrro-level regression net and must stay green,
byte-identical to before the extraction. `src/SwissEphemeris.Interpreter/README.md` documents
what it wraps and how another .NET project would take a dependency on it (project/source
reference for now — no publish pipeline; a NuGet pack step is a later addition if reuse actually
materializes, not built speculatively now).

## Sequencing

Three independent slices, on the workstreams that already own the touched files:

1. **Part A** (docs only) → `workstream/database`. Cheapest, highest immediate value. First.
2. **Part C** (extraction) → `workstream/cli` (owns `src/Ikiastrro.Core/**` per existing
   precedent — the Ashtakavarga engine plan lived there). Pure refactor, no behavior change.
3. **Part B** → split: `db/080` migration on `workstream/database`; the Razor component +
   `tokens.css` + `design-language.md` update on `workstream/ui`.

## Verification

- **Part A**: no code changes — verify by re-reading the corrected `rules-engine.md` table; no
  build/test impact.
- **Part B**: `dotnet build` clean; run `db/080` the same way `db/079` was applied, confirm the
  unique constraint; a bUnit snapshot of `InterpretationText.razor` above/below `--bp-tablet`.
- **Part C**: `dotnet build` solution-wide; `dotnet test tests/SwissEphemeris.Interpreter.Tests`;
  re-run existing CLI `verify-*` checks touching sunrise/positions, confirm byte-identical output.
