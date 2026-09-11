# SwissEphemeris.Interpreter

A thin, domain-agnostic wrapper over [SwissEphNet](https://www.nuget.org/packages/SwissEphNet)
(a managed C# port of Astrodienst's Swiss Ephemeris) for sidereal planetary/Ascendant positions
and sunrise/sunset events. Uses Moshier analytical mode (`SEFLG_MOSEPH`) — no external `.se1`
ephemeris data files to bundle or configure, ~1 arcsecond accuracy.

Extracted out of [ikiastrro](https://github.com/rammyps23/ikiastrro)'s `Ikiastrro.Core`
(`decisions/003-rules-audit-content-model-ephemeris-interpreter.md` Part C) — pure extraction,
no behavior change. No `Ikiastrro.*` (or any other caller-domain) type appears anywhere in this
project's public surface: everything is a plain record or primitive, so the project folder is
droppable into another .NET project as-is.

## What it wraps

- **`SwissEphemerisInterpreter.GetPositions(localMoment, latitudeDeg, longitudeDeg, ayanamsa, houseSystem)`**
  — sidereal longitude/latitude/daily-motion-speed for the Sun, Moon, Mars, Mercury, Jupiter,
  Venus, Saturn, Rahu (mean lunar node) and Ketu (Rahu + 180°), plus the Ascendant longitude,
  the ayanamsha applied, and local sidereal time. `houseSystem` only supports
  `HouseSystem.WholeSign` today (`swe_houses_ex`'s `'W'` system char) — the parameter exists so
  other systems are additive later.
- **`SwissEphemerisInterpreter.GetSunEvents(localMoment, latitudeDeg, longitudeDeg)`** —
  sunrise/sunset/next-sunrise framing the day arc containing `localMoment`, via
  `swe_rise_trans` with `SE_BIT_DISC_CENTER | SE_BIT_NO_REFRACTION` (geometric disc-centre
  crossing the true horizon, no refraction — matches JHora/Parashara's Light's default).
- **`AyanamsaDefinition`** — the JHora-style named-ayanamsha catalogue (`AyanamsaDefinition.Catalog`),
  each entry either backed by a Swiss Ephemeris sidereal mode or left unimplemented pending a
  custom fixed-star formula.

See the XML doc comments on `SwissEphemerisInterpreter` for the exact flags and the
accuracy/agreement notes (mean vs. true node, Moshier vs. full-ephemeris sunrise bias) behind
each choice.

## Taking a dependency on this

No publish pipeline exists yet — this is a project/source reference only. A NuGet pack step is
a later addition if reuse outside ikiastrro actually materializes, not built speculatively now.

To use it from another .NET project (targets `net8.0`, consumable from anything ≥ net8.0):

1. Copy this folder (`src/SwissEphemeris.Interpreter/`) into the target repo, or add it as a
   git submodule / subtree.
2. `<ProjectReference Include="path/to/SwissEphemeris.Interpreter.csproj" />` in the consuming
   project.
3. Call `SwissEphemerisInterpreter.GetPositions` / `GetSunEvents` directly — both take plain
   `DateTimeOffset`/`double` primitives and return plain records (`EphemerisSnapshot`,
   `PlanetPosition`, `SunEvents`), no adaptation needed.

## Tests

`tests/SwissEphemeris.Interpreter.Tests` (in the ikiastrro repo) carries the Moshier-accuracy
spot-checks against ikiastrro's established reference chart (22 Apr 1981, Chennai). ikiastrro's
own CLI `verify-*` checks remain the ikiastrro-level regression net for this wrapper's output
via `Ikiastrro.Core`'s adapter (`SwissEphemerisProvider`).
