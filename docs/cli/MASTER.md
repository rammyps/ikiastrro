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
  Hora Lord)**, **Karakāṁśa (AK in D9) + Bhaava/Ghati/Sree Lagna**, source-attributed yoga
  inputs, slow-planet transits, Sade Sati / Kantaka / Ashtama, functional benefic/malefic.
- **Verification:** `verify-*` CLI modes + `tests/Ikiastrro.Yoga.Tests` (138) +
  `tests/Ikiastrro.Web.Tests` (187). `dotnet build` / `dotnet test` run from the terminal.
- **Green now:** the `verify-*` modes — `verify-schema`, `verify-vargas`, `verify-jaimini`,
  `verify-ashtakavarga`, `verify-panchanga`, `verify-dignity`, `verify-rules`, `verify-pipeline`,
  `verify-sources`, `verify-terminology`, `verify-avastha`, `verify-functional-nature`,
  `verify-upagrahas`.

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
- **`FEAT-YOGA-01`** — Raman predicates 201–300, PVR P0 additions, structured
  missing-requirement codes, source-qualified strength policy.

## Planned

- `ChartGenerationService.GenerateAll` adopts the `ChartPipeline` bundle path.
- Reserved engine seams: Dispositor, Vimśopaka, Sthira/Naisargika Karaka, additional avasthas
  (Dīptādi / Lajjitādi / Śayanādi).
- Full Ṣaḍbala port from the vendored MIT `jyotishganit` (attribution).
