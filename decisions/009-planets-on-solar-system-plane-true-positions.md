---
status: accepted
date: 2026-10-03
workstream: cli (SwissEphemeris.Interpreter, dasha calculators, transit backfill, verify-jaimini)
---

# 009 — Planets on the solar-system plane, as true positions, as Jagannatha Hora computes them

## Decision

`SwissEphemerisInterpreter.GetPositions` computes the grahas the way Jagannatha Hora does for
the Traditional Lahiri reference export:

- **Solar-system plane.** The sidereal mode carries `SE_SIDBIT_SSY_PLANE`, so Swiss Ephemeris
  projects each position onto the solar system's invariable plane (about 1.58° from the
  ecliptic). The returned latitude is measured from that plane.
- **True positions.** `SEFLG_TRUEPOS`: no light-time, aberration or light deflection. JHora's
  Planet Calculation Options show "True positions" (geocentric, true nodes).
- **The ascendant is not projected.** Houses and the reported ayanamsa use the plain sidereal
  mode. With the projection, the Lagna lands about 1.1° from JHora's.

The ayanamsa is still Traditional Lahiri, Swiss mode 1 ([decision 004](004-ayanamsa-traditional-lahiri.md)).
rammyps asked on 2026-10-03 to look into the Moon's ~4′ difference from JHora, then to apply
the fix.

## Why

- **It closes the Moon gap and the "~1′" gap on every planet.** For 22 Apr 1981 05:30:01 IST,
  80E17 13N05 (JHora's own data for `RamakrishnanP`):

  | | Before (ecliptic, apparent) | After |
  |---|---|---|
  | Moon | +246.6″ | −3.1″ |
  | Sun / Mars / Mercury / Venus | −64.6″ / −68.9″ / −75.5″ / −67.0″ | −3.4″ / −3.3″ / −3.3″ / −3.3″ |
  | Jupiter / Saturn | −36.4″ / −55.1″ | −3.2″ / −3.5″ |

  The −3.3″ left on every planet equals the difference between JHora's printed ayanamsa
  (23°35′41.83″) and Traditional Lahiri's (23°35′45.28″).
- **JHora's latitudes prove the projection.** The "Latitudes, speeds etc" export gives the Sun
  1.530°, the Moon 3.410°, Jupiter 0.036° and so on. Projected on the solar-system plane, ours
  match all seven to 0.001°; on the ecliptic they were up to 1.5° off. The Moon's speed matches
  too (11.865°/day, against 11.876° on the ecliptic).
- **The projection follows JHora's ayanamsa choice.** The older JHora export of the same chart,
  made with True Chitrapaksha (`docs/artifacts/reference-charts/Rammy_Jagannatha.txt`), matches
  the plain ecliptic with true positions to 0.3″ for every planet. The 2026-09-23 export, made
  with Traditional Lahiri and used for every reconciliation since decision 005, matches the
  solar-system plane. The project locks Traditional Lahiri, so it follows that export.
- **Daśā dates.** The Moon drives Vimśottarī. `RamakrishnanP`'s Mercury mahādaśā now starts on
  1994-10-14; JHora gives 1994-10-17 (39 days apart before).

## Lahiri variants checked

None of the Swiss Ephemeris Lahiri definitions reproduces JHora's 23°35′41.83″ exactly.
Mode 1 is the N.C. Lahiri / Indian Astronomical Ephemeris value already in use:

| Swiss mode | Definition | Ayanamsa | vs JHora |
|---|---|---|---|
| 1 Lahiri (in use) | 23°15′00.658″ at 21 Mar 1956 less nutation, IAU 1976 precession | 23°35′45.28″ | +3.45″ |
| 46 Lahiri ICRC | 23°15′ at 1956 less nutation, Newcomb precession (before the 0.658″ IAE 1985 correction) | 23°35′44.45″ | +2.62″ |
| 43 Lahiri 1940 | 22°26′45.50″ + 50.25748″T + 0.00011115″T² from 1900 | 23°34′52.40″ | −49.4″ |
| 44 Lahiri VP285 | zero at 285 CE | 23°36′08.42″ | +26.6″ |
| 1, with nutation | Swiss "true" ayanamsa | 23°35′30.02″ | −11.8″ |

Modes 43, 44 and 46 are newer than SwissEphNet 2.8, which falls back to Fagan-Bradley for
them, so they were computed from their published definitions. A hand calculation of mode 1 from
its definition lands 0.08″ from Swiss's. ICRC is 0.8″ closer than mode 1, but neither is exact
and the 3″ difference moves nothing visible, so decision 004 stands.

## Consequences

- **Every planet moved**, by about 1′ (the Moon by 4′). No nakshatra or pada changed for any of
  the 8 people. Varga signs changed mostly for Śrī Lagna (built from the Moon) in the higher
  vargas, plus a few grahas (e.g. IshwaryaG's Mars in D20 and D40). All 20 charts still match
  JHora's "Rasis occupied in all vargas" export for every body.
- **Latitude.** `EclipticLatitudeDegrees` now holds the latitude from the solar-system plane,
  as JHora shows it. Graha Yuddha's "further north wins" reads it.
- **Daśā rounding.** A birth within half a day of a pratyantar's end used to round its last day
  before its first (`CK_DashaPeriods_Offsets`); AnanyaR hit it after this change. Vimśottarī and
  Aṣṭottarī now clamp the last day to the first.
- **Regeneration.** `rebuild-all`, `backfill-strength-statistics`, and the transit table cleared
  and re-walked (`backfill-planet-transits`: Saturn 109, Jupiter 229, Rahu 92 events).
- **Verifiers.** `GetPositionsTests` pins the new longitudes and a JHora-parity test checks all
  seven planets within 5″ and their latitudes. `verify-jaimini`'s Śrī Lagna now checks the
  2026-09-23 export (15 Cn 17′37.70″, D9 Scorpio); its other special-lagna checks still use the
  older export.
