---
last_updated: 2026-09-11
workstream: cli
togaf: C — Application Architecture (engine)
safe: Solution Intent (fixed)
---

# CLI workstream — MASTER

**Branch** `workstream/cli` · **worktree** `D:\@ClaudeSpace\ikiastrro.wt\cli` ·
**owns** `src/Ikiastrro.Core/`, `src/Ikiastrro.Cli/`, `tests/Ikiastrro.Yoga.Tests/`.

The calculation engine (`Ikiastrro.Core`), its entry + batch + verification front end
(`Ikiastrro.Cli`), and the engine test suites. Consumes the database schema; publishes
persisted rows to the UI stream ([`../architecture/domain-contracts.md`](../architecture/domain-contracts.md)).

## Docs

| Doc | For |
|---|---|
| [`calculations.md`](calculations.md) | Every calculation — convention and source: ephemeris, vargas, dignity, houses, relationships, combustion, nakṣatras, dasha, transits, Sade Sati, functional nature, avasthas, Jaimini karakas & special points, strength, yoga |
| [`commands.md`](commands.md) | The CLI command catalogue — entry, `compute-*` / `backfill-*` / `seed-*`, and every `verify-*` mode |

## Current state

- **`Ikiastrro.Core`** — 14 named engines under `Engines/<Name>/`, `ChartPipeline` /
  `ChartBundle` DB-free façade. All classical logic original; only raw longitudes from
  `SwissEphNet` (Moshier mode).
- **21 position chart types** computed and persisted per person (D1 + 20 vargas), plus
  3-level Vimśottari dasha, dignity, house lordship, conjunctions (+ groups), aspects,
  retrograde / combustion, nakṣatra linkage, Chara Karakas + special points + 11 upagrahas,
  Bālādi + Jāgradādi avasthas, Ṣaḍbala / Bhāva Bala foundation, **Parāśari Ashtakavarga
  (BAV / SAV / Sodhya Piṇḍa)**, **Pañchāṅga (Tithi / Karaṇa / Nitya Yoga / Vedic weekday /
  Hora Lord)**, **Karakāṁśa (AK in D9) + Bhaava/Ghati/Sree Lagna**, **Sayanaadi Avastha
  (PostureState)**, source-attributed yoga inputs, slow-planet transits, Sade Sati / Kantaka /
  Ashtama, functional benefic/malefic.
- **Verification:** `verify-*` CLI modes + `tests/Ikiastrro.Yoga.Tests` (147) +
  `tests/Ikiastrro.Web.Tests` (187). `dotnet build` / `dotnet test` run from the terminal.
- **Green now:** the `verify-*` modes — `verify-schema`, `verify-vargas`, `verify-jaimini`,
  `verify-ashtakavarga`, `verify-panchanga`, `verify-strength`, `verify-dignity`, `verify-rules`,
  `verify-pipeline`, `verify-sources`, `verify-terminology`, `verify-avastha`,
  `verify-functional-nature`, `verify-upagrahas`.

## In flight

- **`FEAT-DATA-04` (with database)** — ayanāṁśa default fixed (migration 054: Jagannatha
  mode 26 → Lahiri mode 1); both saved people regenerated via `compute-all`; `verify-vargas`
  / `verify-jaimini` green. Still open: re-seed the `BENCH_RAMAKRISHNAN_P_JHORA_1981`
  benchmark case row and add a `verify-ayanamsa` mode (deferred).
- **`FEAT-STRENGTH-01`** — implement the seeded-but-uncomputed Kālabala components; planetary
  war; per-planet minimum-rūpa thresholds; reconcile Iṣṭa/Kaṣṭa/Cheṣṭā. (DB rules seeded,
  migrations 071–073, 076.)
- **`FEAT-ASHTAKAVARGA-01`** — DB + Core + Verify **done** (migrations 074–078;
  `AshtakavargaCalculator`; `verify-ashtakavarga` reproduces the JHora export exactly).
  Remaining: a UI Ashtakavarga table over `vw_ChartAshtakavarga`.
- **`FEAT-DATA-06` — Pañchāṅga / time layer** — DB + Core + Verify **done** (migration 081;
  `Engines/Panchanga/{PanchangaModels,PanchangaCalculator}`; `PanchangaRepository`; wired into
  `ChartBundle.Panchanga`, `ChartPipeline.Run`, and `ChartGenerationService`'s D1 branch;
  `verify-panchanga` reproduces the JHora export exactly — Krishna Tritiya, Vanija, Vyatipaata,
  Tuesday, Hora Lord Venus, Janma Ghatis 58.89; `tests/Ikiastrro.Yoga.Tests/PanchangaCalculatorTests`
  (6) use JHora's own printed longitudes directly, no ephemeris round-trip). Deliberately not
  computed (no PVR §1.3 source): Karana lord, Nitya Yoga lord, Samvatsara, lunar month, Mahakala
  Hora, Kaala Lord — see the migration-081 header. Remaining: a UI Panchanga strip over
  `vw_ChartPanchanga`.
- **Jaimini special lagnas / Karakamsa** — DB + Core + Verify **done** (migration 082
  `vw_ChartKarakamsa`; `Engines/Karakas/{BhaavaLagnaCalculator,GhatiLagnaCalculator,
  SreeLagnaCalculator}`, wired into `SpecialPointCalculator.ComputeSeeds` alongside
  `HoraLagnaCalculator` — BL/GL/HL/SL all project into every varga via the existing
  `SpecialPointProjector`; `verify-jaimini` reproduces the JHora export exactly — BL/GL/SL
  D1+D9 signs and longitudes, Karakamsa AK=Rahu -> Libra;
  `tests/Ikiastrro.Yoga.Tests/SreeLagnaCalculatorTests` (2) against PVR's own worked example
  + the JHora export). Karakamsa needed no new computation at all — `CharaKaraka` was already
  stamped onto every chart type including D9, `vw_ChartKarakamsa` just surfaces it. Deliberately
  out of scope — `SRC_PVR_INTEGRATED` §5.7 states outright these are "beyond the scope of this
  book", no other registered source covers them: Vighati Lagna, Varnada Lagna, Pranapada Lagna,
  Indu Lagna, Bhṛgu Bindu. Remaining: a UI special-lagnas strip.
- **`FEAT-AVASTHA-05` — Sayanaadi (PostureState)** — DB + Core + Verify **done** (migration 083;
  `PostureStateCalculator`, wired into `PlanetaryStateComputer.Compute` behind a new
  `janmaGhatis` parameter threaded from `ChartPipeline.Run` / `ChartGenerationService`'s D1
  branch via the new `SwissEphemerisProvider.JanmaGhatis` helper; `verify-avastha` reproduces
  the JHora export's Activity table exactly, all 9 grahas;
  `tests/Ikiastrro.Yoga.Tests/PostureStateCalculatorTests` (4)). Bug caught along the way: the
  lookup from `tbl_Chart_KeyDetails.Nakshatra` (AstroMath's canonical display names, e.g.
  "Ashwini") must not go through `Enum.TryParse<ConstellationName>` — that enum's member
  spellings are an unrelated legacy form ("Aswini") per its own doc comment; fixed to index
  against `AstroMath.NakshatraCanonicalNames` instead. Deliberately not built: the secondary
  Cheṣṭā/Dṛṣṭi/Vicheṣṭā strength refinement (PVR's Table 37 sound-map is OCR-ambiguous in the
  raw extract). Deeptādi/Lajjitādi (`FEAT-AVASTHA-03/04`) remain unbuilt — real precedence-order
  ambiguity in the source, not a missing-input blocker like this one was.
- **`FEAT-STRENGTH-01` (CLI slice)** — Dina / Horā / Tribhāga Bala + Graha Yuddha detection
  **done** (migrations 072/076/084 on `workstream/database`; `ShadbalaCalculator` extended,
  `verify-strength` new, `tests/Ikiastrro.Yoga.Tests/ShadbalaKalaBalaTests` (5)). Dina Bala (45
  virupas to the weekday lord) and Hora Bala (60 to the running hora lord) reuse
  `PanchangaCalculator`'s own verified `VedicWeekdayId`/`HoraLordPlanetId` rather than
  re-deriving them — `ShadbalaCalculator.Calculate` now takes a `PanchangaResult` parameter;
  Tribhaga Bala (day/night thirds, Mercury/Sun/Saturn by day, Moon/Venus/Mars by night, 60
  virupas to the part's lord, Jupiter classically exempt and always 60) reuses `JanmaGhatis`.
  `ComputeYuddha` detects the five tara grahas within 1° of D1 longitude and picks the winner
  by ecliptic latitude; the diameter-based delta *magnitude* is deliberately left at 0 — the
  Raman DJVU has no text extract, and 1_Ramakrishnan has no war either way (Mars/Mercury/Venus
  are all >2° apart in Aries; Jupiter/Saturn are 2°14' apart in Virgo), so nothing forced a
  guess. Also fixed along the way: `verify-rules` (tbl_Rule_PanchangaFormula/PostureStateFormula
  were never registered in `tbl_Rule_Catalog` — migration 084) and `verify-terminology` (the 12
  Sayanadi `tbl_Dim_PlanetaryState` rows from migration 083 were never terminology-seeded — ran
  `seed-terminology` + added their English glosses to `TerminologySeed.cs`), both leftover gaps
  from the two prior turns, not from this one. Remaining: Varsa/Masa/Ayana Bala and the Yuddha
  Bala magnitude both need `SRC_RAMAN_GRAHA_BHAVA_BALAS` (DJVU, no text extract); populate
  `MinimumRequiredRupas` on newly-computed rows; Cheshta reconciliation; Vimsopaka Bala.
- **`FEAT-YOGA-01`** — Raman predicates 201–300, PVR P0 additions, structured
  missing-requirement codes, source-qualified strength policy.

## Planned

- `ChartGenerationService.GenerateAll` adopts the `ChartPipeline` bundle path.
- Reserved engine seams: Dispositor, Vimśopaka, Sthira/Naisargika Karaka, Dīptādi/Lajjitādi
  avasthas.
- Full Ṣaḍbala port from the vendored MIT `jyotishganit` (attribution).
