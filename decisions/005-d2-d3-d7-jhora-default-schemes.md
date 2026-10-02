---
status: accepted
date: 2026-10-02
workstream: database (tbl_Rule_VargaScheme, migration 164), cli (verify-vargas / verify-strength), core (sign rules)
supersedes: decision D-1 (classical two-sign Leo/Cancer Hora for D2)
---

# 005 — D2, D3 and D7 use Jagannatha Hora's default schemes

## Decision

App-wide, the three vargas run the schemes Jagannatha Hora uses by default:

| Varga | Was | Now | JHora tag |
|---|---|---|---|
| D2 | Classical two-sign Hora (Cancer / Leo only) | Uma-Shambhu Hora — Sun's hora in an odd sign, Moon's in an even one: `(2r + h) mod 12` | `D-2 (US)` |
| D3 | Parāśara 1st / 5th / 9th | Uma-Shambhu Drekkana — first drekkana `r + 4·⌈r/2⌉` (Ar, Vi, Li, Pi cycle), odd signs forward, even backward | `D-3 (US)` |
| D7 | Odd from the sign, even from the 7th, both forward | Same starts, even signs counted backward | `D-7 (7-1)` |

rammyps chose this on 2026-10-02 over keeping Parāśara (and over using JHora's schemes only
inside Shadbala), so the D2/D3/D7 the app shows are the charts its strengths are computed from.

## Why

- **Saptavargaja Bala matches JHora.** With the Rāśi-chart relationship and D1-only
  moolatrikoṇa fixed, `ShadbalaCalculator`'s Saptavargaja for `1_RamakrishnanP` equals JHora's to
  the 0.001 for all seven grahas once D2/D3/D7 use these schemes — and differs by up to 55
  virūpas without them. Sthāna Bala now matches JHora except one Drekkana Bala cell (Venus).
- **D2 becomes a readable chart.** The classical Hora puts every graha in Cancer or Leo, so the
  Life Matters wealth steps that count houses in D2 (2nd / 11th from Jupiter …) had only two
  possible answers.

## How it is enforced

- `tbl_Rule_VargaScheme` rows 1 (D2), 3 (D3) and 7 (D7) point at `HoraD2UmaShambu`,
  `DrekkanaD3UmaShambu` and `SaptamsaD7EvenReverse` (migration 164). Every consumer reads the
  chart by type, so All Charts, Amsabala, Vimśopaka, Ashtakavarga, yogas, Key Inference and
  Shadbala follow without code changes.
- The rules were fitted to and verified against all 67 bodies of the RamakrishnanP
  "Rasis occupied in all vargas" export (2026-09-23). CLI `verify-vargas` checks the stored
  charts against that export; `verify-strength` checks Sthāna Bala against JHora's.
- The `D2-US` chart type (scheme row 2) now computes the same chart as D2 and is kept.
- The classical `HoraD2Classic`, Parāśara `DrekkanaD3` and `SaptamsaD7` rules stay in the factory
  for a future rule set.

## Changing it later

Repoint the three scheme rows in a migration and run CLI `rebuild-all` +
`backfill-strength-statistics`. The `verify-vargas` JHora grid and `verify-strength` Phase 1c
expectations move with it.
