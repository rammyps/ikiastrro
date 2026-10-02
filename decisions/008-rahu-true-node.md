---
status: accepted
date: 2026-10-02
workstream: cli (SwissEphemeris.Interpreter, core chart computers, transit backfill)
---

# 008 — Rahu and Ketu are the true lunar node, as Jagannatha Hora uses

## Decision

`SwissEphemerisInterpreter.GetPositions` computes Rahu from Swiss Ephemeris's true (osculating)
lunar node, `SE_TRUE_NODE`, instead of the mean node `SE_MEAN_NODE`. Ketu stays Rahu + 180°.
Rahu and Ketu are always flagged retrograde, whatever the sign of the true node's speed.

rammyps asked on 2026-10-02 for the node to match JHora, after decisions 005 and 007 left Rahu
and Ketu's D60 as the only chart placements still off JHora's for `RamakrishnanP`.

## Why

- **The true node is what JHora shows.** For 22 Apr 1981 05:30 IST, Chennai, JHora gives Rahu
  12°54′52.7″ Cancer. The true node gives 12°55′10.5″ (18″ off), the mean node 13°02′36.5″
  (7′44″ off). With the true node, Rahu's D60 part (26) and every varga placement of Rahu and
  Ketu match JHora's "Rasis occupied in all vargas" export, so all 20 charts now match for
  every body.
- **This reverses an earlier choice.** The mean node was picked because Prokerala and
  AstroSage use it (within 0.6′ on this chart). The project now reconciles against JHora, as
  in decisions 005–007.

## Consequences

- **Retrograde flag.** The true node's speed occasionally turns positive. `D1ChartComputer` and
  `VargaChartComputer` therefore set `IsRetrograde` for Rahu and Ketu unconditionally (the
  classical rule). The stored speed is the real true-node speed.
- **Transit events.** `tbl_PlanetSignTransitEvents` was cleared and re-walked
  (`backfill-planet-transits`). Saturn's and Jupiter's 338 events came back identical. Rahu has
  90 events instead of 84, including 6 re-entries where the true node wobbles back across a
  sign boundary; the mean node never re-enters.
- **Regeneration.** All 8 people were rebuilt (`rebuild-all`, `backfill-strength-statistics`).
  Everything read from the nodes moved — Rahu and Ketu's degree, nakshatra pada, varga signs,
  chara-kāraka degree, Bhṛgu Bindu and Ṣaṣṭyaṃśa — typically by minutes of arc, never by more
  than the ~1.7° the true and mean nodes can drift apart.
- `GetPositionsTests` pins the new Rahu / Ketu longitudes.
