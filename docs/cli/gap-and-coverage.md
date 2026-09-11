---
last_updated: 2026-09-11
workstream: cli
togaf: E — gap analysis
safe: Program backlog input
---

# CLI — gap & coverage vs a full JHora natal export

Baseline: the Jagannatha Hora natal export for `1_Ramakrishnan`. It is a **feature-parity
checklist**, not the numeric source of truth (longitudes are independently verified against
Swiss-ephemeris-compatible tools). Each item below maps to a `FEAT-<AREA>` row in
[`../../masterproduct.md`](../../masterproduct.md).

## Delivered (not gaps)

Sidereal longitudes (Ascendant, 7 planets, nodes) · D1 signs/degrees, nakṣatra/pada/lord + KP
L2 sub-lord · **all 21 position charts** (D1 + 20 vargas, Plan A) with per-varga within-sign
degree · dignity, whole-sign house lordship (3 reckonings), conjunctions (+ groups), graha
dṛṣṭi, retrograde, combustion · Vimśottari dasha (3 levels, partial-at-birth) · slow-planet
transit events · Sade Sati / Kantaka / Ashtama · functional benefic/malefic · Bālādi +
Jāgradādi avasthas · Chara Karakas (Aṣṭa) · AL + 12 Arudhas + Hora Lagna + **all 11
upagrahas** · Ṣaḍbala / Bhāva Bala foundation · **Parāśari Ashtakavarga — BAV / SAV /
Ṭrikoṇa + Ekādhipatya Śodhana / Rāśi + Graha + Sodhya Piṇḍa** (`verify-ashtakavarga`
reproduces the JHora export exactly) · **Pañchāṅga — Tithi + fraction, Karaṇa + fraction,
Nitya Yoga + fraction, Vedic weekday, Hora Lord, Sunrise/Sunset, Janma Ghaṭis**
(`verify-panchanga` reproduces the JHora export exactly) · **Karakāṁśa (AK in D9) + Bhaava /
Ghati / Sree Lagna** (`verify-jaimini` reproduces the JHora export exactly) · source-attributed
yoga inputs · provenance (ayanāṁśa degrees, sidereal time, rule set, method) on every chart.

## Priority gaps

1. **Precision & config foundation** — ayanāṁśa correctness (`FEAT-DATA-04`); ayanāṁśa
   *selectable in the UI* (`FEAT-UI-12`); chart-style selectable; documented selectable D2
   Hora method if JHora's Uma Shambu output must be reconciled.
2. **Panchanga / time layer — Tithi/Karana/Nitya-Yoga/weekday/Hora-Lord/Janma-Ghatis are
   delivered** (migration 081, `PanchangaCalculator`, `verify-panchanga` exact vs the JHora
   export). Remaining, deliberately deferred — no PVR §1.3 source found: **Karana lord**,
   **Nitya Yoga lord**, **Samvatsara** (60-year cycle name), **lunar month** (PVR's own Table 4
   extract is OCR-garbled — needs a clean page-image or 2nd-edition cross-check before seeding),
   **Mahakala Hora / Kaala Lord** (JHora extensions past PVR's 24-hora scheme).
3. **Jaimini base layer — Karakāṁśa + Bhaava/Ghati/Sree Lagna are delivered** (migration 082
   `vw_ChartKarakamsa`; `BhaavaLagnaCalculator`/`GhatiLagnaCalculator`/`SreeLagnaCalculator`;
   `verify-jaimini` exact vs the JHora export). Remaining, deliberately out of scope —
   `SRC_PVR_INTEGRATED` §5.7 states outright "there are some more special lagnas defined by
   Parasara, but they are beyond the scope of this book", and no other registered source covers
   them: **Vighati Lagna**, **Varnada Lagna**, **Pranapada Lagna**, **Indu Lagna**, **Bhṛgu
   Bindu**. Jaimini rāśi dashas depend on some of these (e.g. Sudasa already uses Sree Lagna,
   which is delivered).
4. **Strength systems** — **Ashtakavarga is delivered** (BAV / SAV / Śodhana / Piṇḍa, exact
   vs the JHora export — migrations 074–078 + `AshtakavargaCalculator` + `verify-ashtakavarga`).
   Remaining: full Ṣaḍbala rūpas + the six Kālabala sub-components + planetary war +
   Iṣṭa/Kaṣṭa (DB rules seeded, migrations 071–073, 076); Vimśopaka Bala + Vaiśeṣikāṁśa
   (four varga-group weights still need a cited source).
5. **Avastha & karaka reference** — Dīptādi + Lajjitādi (need a shared benefic/malefic
   classifier); Śayanādi (needs persisted janma ghaṭis, source-blocked); apply the designed
   `tbl_Dim_HouseSignification` / Sthira / Naisargika reference data (migration 030) instead
   of the hard-coded `LifeAreaMap`; decide Sapta vs Aṣṭa Naisargika coverage.

## Parity gaps (lower priority)

- **Sphutas** — Prana, Deha, Mrityu, Sookshma Tri-Sphuta, Tithi, Yoga, Kshetra, Beeja, Tri,
  Chatus, Pancha, Kunda, Avayoga and Dhūma-derived points.
- **Additional dashas** — Ashtottari, Yogini (applicability rules); Jaimini rāśi dashas
  (Moola, Narayana, Sudasa); Kālachakra. Each uses the generic dasha-period storage, carries
  its system + rule-set identity, and has worked reference assertions. PyJHora is a
  cross-check reference only (AGPL).
- **Varga composition** — D81 / D108 / D144 (varga-of-a-varga) and D150.

## Open engineering

- Rules-engine **Phase 2** — calculators read the versioned `tbl_Rule_*` tables instead of
  hard-coded C#.
- **Yoga detection** design pass — turn source prose into seed-ready rule rows with explicit
  orbs and conflict/precedence.
- **Synthesis layer** — assemble stored facts + significations into per-house / life-area
  judgements. Deliberately distinct from calculation; its own interpretation policy.
- Automated reference regression for every new technique (JHora only where conventions match,
  plus independent ephemeris checks).
- Decide whether altitude belongs in birth details (matters only for elevation-sensitive
  rise/set).
