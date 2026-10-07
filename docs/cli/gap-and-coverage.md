---
last_updated: 2026-10-07
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
Ghati / Sree Lagna** (`verify-jaimini` reproduces the JHora export exactly) · **Sayanaadi
Avastha (PostureState, all 12 states)** (`verify-avastha` reproduces the JHora export's
Activity table exactly, all 9 grahas) · **Dina / Horā / Tribhāga Bala + Graha Yuddha
detection** (`verify-strength` all green) · source-attributed yoga inputs · provenance
(ayanāṁśa degrees, sidereal time, rule set, method) on every chart.

**JHora gap step 4 (Core engines, `Engines/KeyInfo` + `Engines/Karakas`; UI tables not yet wired)** — **Special tārās** from Moon and Lagna (28-nakṣatra circle with Abhijit) · **Lattā nakṣatras** (the struck star; JHora's "Aspected Stars" column is undocumented and not reproduced) · **all 36 Special Tithis** (k × birth elongation) · **Lords of the 64th navamsa and 22nd drekkana** in all five D3 schemes · **all 36 Sahams** (`SahamTable`; night-birth golden, every row within an arcsecond of JHora). `KeyInfoJhoraStep4Tests` checks each against JHora's own Basics ▸ Key Info views for 1_Ramakrishnan. Still open from the same JHora view list: **Planetary drekkanas (Ayudha / Kroora / Agni / Mriga…)** — the extra attributes are not in PyJHora's table and need BPHS ch. 6 drekkana forms — and **Sahams on a day-birth golden chart** (day branches mirror the night ones per PVR Table 74 but are unverified against JHora).

**JHora Yogas / Aspect Table / Matching, captured 2026-10-07** (`cproj_win_app_explorer/docs/reference-jhora-yogas-matching.md`). JHora's Yogas list for RamakrishnanP has 20 yogas (givers, results, definitions) — a golden for the yoga engines. Its Aspect Table is the sphuta graha-dṛṣṭi percentage grid (a golden for `GrahaDrishtiStrengthCalculator`; screenshot only). Its Horoscope Matching Score gives only an Ashtakoota total out of 36; the 216-case sweep for a native in Anuradha pada 2 (`docs/artifacts/reference-charts/JHora_matching_sweep_Anuradha_p2.json`) agrees with `AshtakootaCalculator` in only 8 of 216 totals (PyJHora's own table: 56 of 216), so JHora's koota tables and exceptions differ from Vasudev's — **known divergence, the engine keeps the book**; reconciling needs the per-koota rules inferred from the sweep or from JHora's help text.

**JHora follow-up, 2026-10-07 (golden comparisons against `RamakrishnanP`).** **Chart caveat:** JHora's `RamakrishnanP.jhd` as captured by the 2026-09-06 clipboard export has place "Chennai, Australia" (TZ +10:00, **Pisces lagna**); ikiastrro's RamakrishnanP is Chennai, India, **Aries lagna** (as are JHora's 2026-09-23 Key Info views and its 2026-10-07 Yogas list). Anything compared against that export must use JHora's own numbers, not ikiastrro's chart. (1) **Vaiseshikamsa** — JHora counts a varga when the graha is in its own *or exaltation* sign over the Dasa-10 and Shodasa-16 groups; Raman's ladder (`VaiseshikamsaCalculator`) is own-sign only. `JhoraVaiseshikamsa` reproduces all 14 JHora tiers for the seven grahas, fed JHora's own varga signs (`JhoraVaiseshikamsaGoldenTests`); Rahu/Ketu not covered (JHora's node signs are a Preferences option; observed counts fit no single convention). Not yet surfaced in the UI. (2) **Aspect Table** — on ikiastrro's own Aries-lagna longitudes, only 15 of 72 planet-to-planet cells are within 1 point of JHora's table (9 if the grid is read transposed); JHora's values are not a function of angular separation (Jupiter→Saturn 93 at 2°, Mars→Mercury 100 at 358°). `GrahaDrishtiStrengthCalculator` keeps PVR; JHora's method is unidentified (screenshot only: `cproj_win_app_explorer/explorer_output/jhora/yogas-matching/aspect_table_d1.png`). (3) **Yogas** — `YogaInputRepository.Replace` built its `ChartBundle` with an empty chara-karaka map, so every AK/AmK/PK yoga persisted as `NOT_EVALUATED`; fixed (`ChartPipeline.CharaKarakaByPlanet`), 26 → 9 not-evaluated for RamakrishnanP, and the three Raja Sambandha forms JHora lists now fire (PVR_CH11_RAJA_SAMBANDHA_05/06/15). Charts persisted before the fix need `refresh-yogas <name>`. **Sarpa and Mridanga resolved — not a definitional conflict:** JHora uses PVR's wording (Sarpa §11.5.2 p.120 "three quadrants occupied by natural malefics"; Mridanga §11.6 p.126 "planets in own and exaltation signs in quadrants and trines" + strong lagna lord) and ikiastrro had only the Raman variants, whose rules differ. Added `PVR_CH11_SARPA` / `PVR_CH11_MRIDANGA` (`PvrChapter11YogaEvaluator`, migration 175); both fire on RamakrishnanP, matching JHora; the Raman variants are unchanged. **Still open:** JHora evaluates yogas in every varga (its Varga column) while ikiastrro's yoga engines are D1-only — see [`yoga-per-varga-design.md`](yoga-per-varga-design.md).

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
   **Dina / Horā / Tribhāga Bala and Graha Yuddha detection are delivered** (migration 072/076/084
   + `ShadbalaCalculator` extended + `verify-strength`) — Dina/Hora reuse `PanchangaCalculator`'s
   verified weekday lord and Hora Lord, Tribhaga reuses its Janma Ghaṭis; Graha Yuddha detects
   the five tara grahas within 1° and picks the winner by ecliptic latitude. Remaining, source-
   blocked — `SRC_RAMAN_GRAHA_BHAVA_BALAS` is a DJVU with no text extract: **Varṣa/Māsa/Ayana
   Bala** (three more Kālabala sub-components) and the **Yuddha Bala magnitude** (the
   diameter-based delta formula; detection and the winner criterion are already computed,
   1_Ramakrishnan has no war to verify a magnitude against either way). Also remaining:
   Iṣṭa/Kaṣṭa reconciliation, `MinimumRequiredRupas` population; Vimśopaka Bala + Vaiśeṣikāṁśa
   (four varga-group weights still need a cited source).
5. **Avastha & karaka reference — Śayanādi (PostureState) is delivered** (migration 083
   `tbl_Rule_PostureStateFormula`; `PostureStateCalculator`; `verify-avastha` reproduces the
   JHora export's Activity table exactly, all 9 grahas). It turned out to be a fully-specified,
   unambiguous formula once `FEAT-DATA-06` supplied Janma Ghaṭis — not source-blocked after all.
   Not seeded: the secondary Cheṣṭā/Dṛṣṭi/Vicheṣṭā strength refinement (PVR's Table 37
   sound-to-number map renders OCR-ambiguously in the raw extract). Remaining, genuinely
   harder — PVR §15.4.3's 9 Dīptādi + 6 Lajjitādi states depend on conjunction/aspect
   precedence the passage doesn't fully order (e.g. a planet both exalted and Sun-conjoined),
   so they need a shared benefic/malefic classifier and a closer source read before schema, not
   just missing input data. Also still open: apply the designed `tbl_Dim_HouseSignification` /
   Sthira / Naisargika reference data (migration 030) instead of the hard-coded `LifeAreaMap`;
   decide Sapta vs Aṣṭa Naisargika coverage.

## Parity gaps (lower priority)

- **Sphutas — 13 delivered** (`Engines/KeyInfo/Sphutas.cs`: Prāṇa, Deha, Mṛtyu, Sūkṣma Tri-, Tithi, Yoga (Sun–Moon), Rāhu Tithi, Kṣetra, Bīja, Tri-, Catus-, Pañca-sphuṭa, Yogi, Avayoga; all match JHora to under 0.1″, `SphutasJhoraTests`). Remaining: **Kuṇḍa** — no formula found; no integer-multiple combination of Lagna/grahas/Gulika/Māndi (± constant) fits JHora's five reference charts, so it needs a source. Vighati Lagna stays deliberately unbuilt (equals Pranapada in the reference chart).
- **Additional dashas — checked against JHora's export for 1_Ramakrishnan (`DashaJhoraGoldenTests`).** Ashtottari maha dates match JHora within 3 days; Narayana (12 maha signs and starts) match; Sudasa shares JHora's kendra cycle and first-dasa balance. **Known divergences, engine keeps PVR:** Ashtottari antardasas — PVR §17.2.2 puts the maha lord's own antardasa last, JHora first; Sudasa seed — PVR §20.2 starts in the Sree Lagna sign (Taurus here), JHora in its 7th (Scorpio, which holds the Moon). **Still open:** Yogini (applicability rules), **Moola** (JHora's Lagna-Kendradi Graha dasa — strength-ranked, not in PVR) and **Kālachakra** (JHora's output labels each maha by a nakṣatra pāda in a way PyJHora's own port notes does not reproduce — needs reverse-engineering from the five reference charts). PyJHora is a cross-check reference only (AGPL).
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
