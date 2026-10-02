---
status: accepted
date: 2026-10-02
workstream: database (tbl_Rule_VargaScheme, migration 166), cli (verify-vargas / verify-rules), core (sign rules)
---

# 007 — D10, D16, D24 and D60 use Jagannatha Hora's schemes

## Decision

App-wide, four more vargas run the schemes Jagannatha Hora uses (as tagged in its export for
`RamakrishnanP`), following [decision 005](005-d2-d3-d7-jhora-default-schemes.md)'s D2/D3/D7:

| Varga | Was (Traditional Parāśara) | Now | JHora tag |
|---|---|---|---|
| D10 | odd from the sign, even from the 9th, both forward | even signs start from the 9th counted backward (= the 5th) and count backward: `(r − 8 − l) mod 12` | `D-10 (5-8)` |
| D16 | Aries / Leo / Sagittarius base by modality, forward | even signs run the 16 parts reversed: `(base + 15 − l) mod 12` | `D-16 (Rev)` |
| D24 | odd from Leo, even from Cancer, forward | even signs from Cancer backward: `(Cancer − l) mod 12` | `D-24 (Rev)` |
| D60 | from the sign forward | odd signs from Aries forward, even from Pisces backward: `l` / `(11 − l) mod 12` | `D-60 (RvAr)` |

`r` is the 0-based rasi sign (even `r` = odd sign) and `l` the 0-based part. Odd-sign placements
are unchanged except in D60, where they now count from Aries instead of from the sign.
rammyps asked for D10 and D60 on 2026-10-02 and chose to include D16 and D24 in the same change.

## Why

- **These were the only charts still off JHora.** After migration 164, comparing every stored
  chart with JHora's "Rasis occupied in all vargas" export for `RamakrishnanP` left exactly these
  four differing, always for bodies in even signs.
- **Each rule reproduces JHora completely.** Every new rule matches all 68 bodies of that export
  (grahas, special lagnas, upagrahas, sphutas, ārūḍhas), computed from JHora's own longitudes.
  PyJHora names the D10, D24 and D60 schemes: `dasamsa_chart` method 3,
  `chaturvimsamsa_chart` method 2 and `shashtyamsa_chart` method 3 (parivṛtti alternate).
- **Vaiśeṣikāṃśa now matches.** With these charts, the Daśavarga and Ṣoḍaśavarga own /
  mūlatrikoṇa / exalted counts equal JHora's "Vaiseshikamsas in four varga schemes" for all
  seven grahas (Jupiter and Saturn differed before).
- **D60 agrees with its own deity names.** The RvAr chart and the Ṣaṣṭyaṃśa name list
  ([decision 006](006-shashtiamsa-names-bphs-jhora.md)) use the same odd-forward /
  even-reversed reading.

## Consequences

- Four new `IVargaSignRule` classes: `DasamsaD10EvenReverse`, `ShodasamsaD16EvenReverse`,
  `SiddhamsaD24EvenReverse` and `ShashtyamsaD60EvenReverseFromAries`. The old classes stay for any
  other rule set. `JhoraDefaultVargaRuleTests` and `verify-vargas` pin them to the export, and
  `verify-rules` proves the migration's sampled GRID_VARGA maps.
- All 8 people were regenerated (`rebuild-all`, `backfill-strength-statistics`). Shadbala is
  unaffected, because Saptavarga uses none of these charts. Vimśopaka, Aṁśabala, the D10 / D16 /
  D24 / D60 charts and everything read from them changed.
- One gap remains in these charts: Rahu and Ketu's D60, caused by our node longitude (~8′ off JHora's).
