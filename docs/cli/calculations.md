---
last_updated: 2026-09-11
workstream: cli
togaf: C — Application Architecture (engine)
---

# CLI — calculations reference

Every calculation ikiastrro performs, its convention, and its source. All code is in
`Ikiastrro.Core`; only raw longitudes come from the ephemeris.

## 1. Ephemeris & frame

- **Engine:** `SwissEphNet` (`SwissEphemerisProvider`), **Moshier analytical mode** — no
  ephemeris data files.
- **Sidereal / ayanāṁśa:** selectable. `AyanamsaDefinition` catalogues 22 systems, each with
  its Swiss `swe_set_sid_mode` id; `tbl_Rule_Ayanamsa` holds the system default. Current
  default **`Lahiri`** (Swiss mode 1, ~23.595° for 1981 — the JHora reference frame; migration
  054). True Chitrapaksha (mode 27) is closer still but needs `sefstars.txt`, which the
  file-less Moshier build omits. Every `tbl_ChartResults` row records `AyanamshaDegrees` +
  `SiderealTimeHours`.
- **Nodes:** Rahu = **mean node** (`SE_MEAN_NODE`); Ketu = Rahu + 180° (derived, never stored
  separately). `vw_KetuSignTransitEvents` = Rahu events + 6 signs.
- **Speed & latitude:** from the same `swe_calc_ut` call (`SEFLG_SPEED`); persisted per planet
  for D1 and every varga. Speed drives retrograde + combustion-orb selection.
- **House system:** **Whole Sign** everywhere. House *n* from a reference sign =
  `AstroMath.CountFromSignToSign` (the sign holding the reference point is house 1). Confirmed
  for house placement + placement-rule interpretation.
- **Time:** birth local wall-clock + place; lat/long via Nominatim; UTC offset resolved
  offline from lat/long + date (historical DST respected). Stored `DATETIME2`, local time of
  day, no offset column.
- `ZodiacName` keeps the Latin spelling `Capricornus`.

## 2. Divisional charts (21 types)

**D1** (real sidereal longitude) + **20 vargas**: D2, D2-US, D3, D4, D5, D6, D7, D8, D9, D10,
D11, D12, D16, D20, D24, D27, D30, D40, D45, D60.

- Data-driven: `tbl_Rule_VargaScheme` is the source of truth (one row per varga per rule set —
  `DivisionFactor`, `MethodCode`, `SignRuleKind`, `SignRuleKey`).
  `ChartCalculationOrchestrator.CreateDefault(schemes)` builds one shared `VargaCalculator`
  (`IChartCalculator`) per row; every varga gets the full shared analytics.
- `SignRuleKey` → C# `IVargaSignRule` via `VargaSignRuleFactory`. `LinearVargaSignRule` covers
  D3 / D4 / D12 / D60; D2 / D6 / D9 / D10 / D11 wrap `AstroMath.Get*Sign`; the rest are bespoke
  `Special` rules (`HoraD2UmaShambu`, `PanchamsaD5`, `SaptamsaD7`, `AshtamsaD8`, `ShodasamsaD16`,
  `VimsamsaD20`, `SiddhamsaD24`, `NakshatramsaD27`, `TrimsamsaD30`, `KhavedamsaD40`,
  `AkshavedamsaD45`). Formulas transcribed from PyJHora `chart_method=1` "Traditional Parāśara".
- **D2** uses the classical two-sign Leo/Cancer Hora; **D2-US** is JHora's default Uma Shambu
  parivṛtti. **D11** uses the PVR/BPHS traditional Rudramsa (not the Sanjay Rath variant).
- Stored: `VargaLongitudeDegrees` (`Normalize(realLon × N)`), `DegreesInSignDecimal`
  (`= VargaLongitudeDegrees mod 30`, populated every chart type). The varga sign is the
  `IVargaSignRule`, **not** `FLOOR(VargaLongitudeDegrees/30)`.
- Not built → varga-of-a-varga composition (D81 / D108 / D144) and D150.

Significations: D2 wealth · D3 siblings/courage · D4 property · D5 fame/authority · D6 health ·
D7 children · D8 sudden events/longevity · D9 marriage/dharma/strength · D10 career · D11 gains ·
D12 parents · D16 vehicles/comforts · D20 spiritual practice · D24 education · D27
strengths/weaknesses · D30 misfortunes · D40 maternal legacy · D45 paternal legacy · D60
past-life karma.

## 3. Dignity

9-tier `DignityStatus` ∈ Exalted · Moolatrikona · Own Sign · Great Friend · Friend · Neutral ·
Enemy · Great Enemy · Debilitated. Pañchadhā Maitrī = natural (permanent) + temporary
(from-Moon) friendship; sign-dignity only, not affliction. PVR Table 6 segments live in
`tbl_Rule_GrahaDignity` (`PvrDignityEvaluator`); the Parāśari exaltation/debilitation degrees,
Moolatrikoṇa ranges and own signs are also hard-coded and cross-checked to `tbl_SignAttributes`.
**Rahu/Ketu** use the Parāśari convention (Rahu exalted Taurus, Ketu exalted Scorpio) — an
explicit choice; their `DignityStatus` is limited to Exalted / Debilitated / Neutral.
Check: `verify-dignity`.

## 4. House lordship, conjunctions, aspects

- **House lordship** per house: its sign's ruler and where that ruler sits — counted from
  Lagna, Sun and Moon.
- **Conjunctions:** grahas sharing a sign; `DegreeSeparation` D1-only. Plus an explicit
  ≥ 2-graha **group/member** layer with `MemberKey` for yoga-subset matching.
- **Aspects (graha dṛṣṭi):** every graha aspects the 7th from itself; Mars +4/8, Jupiter +5/9,
  Saturn +3/10. **Rahu/Ketu use the Jupiter-style 5/7/9 convention** — an explicit choice.

## 5. Retrograde & combustion

Retrograde = `speed < 0`. Combustion (asta) for the 6 applicable planets (not Sun / Rahu /
Ketu / Ascendant); BPHS / Phaladeepika orbs with the **narrower orb auto-selected when
retrograde** (Moon 12 · Mars 17/8 · Mercury 14/12 · Jupiter 11 · Venus 10/8 · Saturn 15).
Stored: `IsCombust`, `DistanceFromSunDegrees`, `CombustionOrbUsedDegrees`.

## 6. Nakshatras

27 nakṣatras (Abhijit excluded), 13°20′ each. `GetNakshatraLord` / `NakshatraLordOrder` is
the single source of truth (Vimśottari uses it). `Nakshatra` / `NakshatraPada` display are
**D1-only**; `NakshatraLordPlanet` + `NakshatraSubLordPlanet` (KP level-2) are populated for
every chart type. `tbl_NakshatraPadas` (108) has a verified 1:1 Pada ↔ Navamsa mapping.
Descriptive fields (Guna/Gana/Yoni/Nadi/…) left NULL pending a cited source. The KP-2 sub-lord
division (`AstroMath.GetNakshatraSubLord`) reuses `VimshottariYearsByLord` for its proportional
split within one nakshatra — same citation as §7's Vimshottari cycle (`tbl_Rule_VimshottariPeriod`).

## 7. Vimshottari Dasha

`VimshottariDashaCalculator` (pure) + `VimshottariDashaService` (write path). Not an
`IChartCalculator`; logged as a `tbl_ChartResults` row for delete-cascade consistency.
**3 levels** (Mahā → Antar → Pratyantar), **all three correctly partial-at-birth**. Cycle:
Ketu 7 · Venus 20 · Sun 6 · Moon 10 · Mars 7 · Rahu 18 · Jupiter 16 · Saturn 19 · Mercury 17
(120 yr). Real dates use 365.2425 d/yr. Storage `tbl_Chart_DashaPeriods` (self-referencing,
age-relative + absolute); reporting `vw_Chart_DashaTimeline`, `tvf_Chart_LifeWeeks` (week
1–4000). Reference: `BENCH_RAMAKRISHNAN_P_JHORA_1981` (`db/checks/check_ayanamsa_dasha_benchmarks.sql`).
The 9-planet order/120-year table is cited by `tbl_Rule_VimshottariPeriod` (migration 085,
`SRC_PVR_INTEGRATED` §16.2 Table 38) — `AstroMath.NakshatraLordOrder`/`VimshottariYearsByLord`
stay hardcoded (verified-mirror pattern); CLI `verify-dasha` cross-checks them against the
table. `tbl_Rule_DashaApplicability` is the separate, still-unseeded *conditional*-dasha
eligibility layer. The KP-2 sub-lord division (§6) reuses the same table's citation.

## 8. Slow-planet transit history

Saturn / Jupiter / Rahu sign-boundary-crossing **event log** (not periods, so retrograde
re-entries are correct), **1930–2060**. Built by day-walk + bisection. CLI:
`precheck-planet-transits` (dry-run vs published dates) then `backfill-planet-transits`. Ketu
derived (`vw_KetuSignTransitEvents`); point-in-time sign `tvf_PlanetSignAtDate`.

## 9. Sade Sati, Kantaka & Ashtama Shani

`tvf_Chart_SadeSatiPeriods(@BirthDetailId)` from the stored natal Moon sign +
`tbl_PlanetSignTransitEvents` — no new reference data. Sade Sati = Saturn in the 12th / 1st /
2nd from natal Moon (three Dhaiyas); Kantaka = 4th; Ashtama = 8th. The TVF splits windows on
retrograde re-entries; the UI re-consolidates. The offset rule is inlined in the T-SQL function
body — it carries no `SourceRefCode` and isn't in `tbl_Rule_Catalog`'s scope (which only tracks
`tbl_Rule_*` tables), so a non-SQL port must still reimplement it from this doc.

## 9a. Ashtakavarga (Parāśari)

`AshtakavargaCalculator.Calculate(d1)` — pure, over the D1 natal signs of the seven grahas
+ Lagna → the seven **Bhinnāṣṭakavargas** (7 recipients × 8 contributors), the
**Sarvāṣṭakavarga** (per-sign total of the seven, Lagna excluded; grand total 337),
**Ṭrikoṇa + Ekādhipatya Śodhana**, and **Rāśi / Graha / Sodhya Piṇḍa**. The benefic-places
matrix, rāśimāna `(7,10,8,4,10,5,7,8,9,5,11,12)` and grahamāna `[5,5,8,5,10,7,5]` live in
`AshtakavargaTables` (the verified mirror of `tbl_Rule_AshtakavargaContribution` /
`tbl_Rule_AshtakavargaReduction`). Moon and Venus carry the standard Parāśari corrections
(Moon ±9 from Moon/Mars, +2/−12 from Jupiter; Venus +4/−5 from Mars) — the variant JHora
uses. `SRC_BPHS_ASHTAKAVARGA`. Persisted on the D1 row: `tbl_Fact_BhinnaAshtakavarga`
(+ the 1/0 `…Contribution` explain layer), `tbl_Fact_SarvaAshtakavarga`,
`tbl_Fact_AshtakavargaPinda`; UI view `vw_ChartAshtakavarga`. Check: `verify-ashtakavarga`
reproduces the JHora export for `1_Ramakrishnan` **exactly** — every BAV cell, the SAV, and
all seven Rāśi/Graha/Sodhya Piṇḍa triples.

## 9b. Pañchāṅga (Tithi / Karaṇa / Nitya Yoga / Vedic Weekday / Hora Lord)

`PanchangaCalculator.Calculate(birth, positions, sunTimes)` — pure, over the D1 Sun/Moon
sidereal longitudes and `SunTimes`. `SRC_PVR_INTEGRATED` §1.3.8-1.3.12, the book's only
panchanga chapter:

- **Tithi** — `floor((Moon−Sun)/12°) + 1` (1-30; 1-15 Śukla, 16-30 Kṛṣṇa). `tbl_Dim_Tithi.Id`
  is seeded in the same order, so the index is the FK directly.
- **Nitya Yoga** — `floor((Sun+Moon)/13°20') + 1` (1-27), same direct-FK seeding.
- **Karaṇa** — each tithi splits into 2 half-tithis; the 7 movable karaṇas
  (`tbl_Dim_Karana.Id` 1-7) repeat 8× (56 slots) from the 2nd half of the month's 1st tithi;
  the 4 fixed karaṇas (Id 8-11) cover the 2nd half of tithi 29 through the 1st half of the
  next month's tithi 1.
- **Vedic weekday** — the calendar day `SunTimes.Sunrise` falls on; `tbl_Dim_VedicWeekday.Id`
  1=Sunday..7=Saturday matches `.NET DayOfWeek + 1`.
- **Hora Lord** — 24 equal horas from sunrise to next sunrise; hora 1 is the weekday lord,
  then the cycle Saturn→Jupiter→Mars→Sun→Venus→Mercury→Moon (decreasing geocentric speed,
  mirrors `tbl_Dim_HoraSequence`) repeats.
- **Janma Ghaṭis** — minutes elapsed since `SunTimes.Sunrise` / 24.

**Deliberately not computed** — no PVR §1.3 source found: Karaṇa lord, Nitya Yoga lord,
Samvatsara (60-year cycle name), lunar month (PVR's own Table 4 extract is OCR-garbled —
contradicts classical Pauṣa/Māgha/Phālguna nakṣatra pairings, needs a clean source pass),
Mahākāla Hora / Kāala Lord (JHora extensions past PVR's 24-hora scheme). Persisted on the D1
row: `tbl_Chart_Panchanga`; UI view `vw_ChartPanchanga`. Check: `verify-panchanga` reproduces
the JHora export for `1_Ramakrishnan` — Kṛṣṇa Tritīyā, Vanija, Vyatīpāta, Tuesday, Hora Lord
Venus **exactly**; Janma Ghaṭis within JHora's own two-decimal rounding.

## 10. Functional benefic / malefic

`LagnaFunctionalNature` — Parāśari **functional** nature for a given Lagna from which houses a
planet rules. `enum { Benefic, Malefic, Neutral, Yogakaraka }` + `IsMaraka` /
`KendradhipatiDosha` / rationale. Rahu/Ketu out of scope. Heuristic from B.V. Raman *How to
Judge a Horoscope* Vol. 1 p.14–18; the sole source of the verdict (the per-Lagna mirror table
was removed). Computed on demand, not persisted. Check: `verify-functional-nature`.

## 11. Avasthas (planetary states)

Star-schema (`tbl_Dim_PlanetaryState` + `tbl_Rule_AgeState` / `tbl_Rule_WakefulnessState` /
`tbl_Rule_PostureStateFormula` → `tbl_Fact_PlanetaryState`), written by `PlanetaryStateComputer`.

- **Bālādi** (age from within-sign degree) — odd signs 0–6 Bāla / 6–12 Kumāra / 12–18 Yuva /
  18–24 Vṛddha / 24–30 Mṛta; even signs reversed. Effect fraction from `tbl_Rule_AgeState`.
  **D1 only.**
- **Jāgradādi** (waking state from `DignityStatus`) — Exalted/MT/Own → Jāgrat; friend tiers →
  Svapna; enemy tiers → Suṣupti. **Every chart type.**
- **Sayanaadi** (`PostureStateCalculator`, PVR §15.4.4, Table 36; 12 states, Sayana .. Nidraa) —
  `index = ((C×P×A) + M + G + L) mod 12` (remainder 0 → 12): C = the planet's own nakshatra
  number, P = its index (Sun=1..Ketu=9), A = its navamsa within its own rasi (1-9), M = Moon's
  nakshatra number, G = the ghati running at birth (`tbl_Chart_Panchanga.JanmaGhatis`,
  `FEAT-DATA-06`), L = Lagna's rasi number. **D1 only** (needs the continuous within-sign
  degree and Janma Ghatis). The nakshatra lookup must index against
  `AstroMath.NakshatraCanonicalNames`, not `Enum.TryParse<ConstellationName>` — that enum's
  member spellings are a legacy form unrelated to the stored display names (its own doc
  comment says so; this cost one wrong-on-7-of-9-planets round trip before the fix).
- Not built: Dīptādi, Lajjitādi (PVR §15.4.3 gives 9 + 6 states, but several depend on
  conjunction/aspect precedence the passage doesn't fully order — needs a closer source read,
  not just a shared benefic/malefic classifier); Sayanaadi's own secondary Cheṣṭā/Dṛṣṭi/
  Vicheṣṭā strength refinement (Table 37's sound-to-number map is OCR-ambiguous in the raw
  extract). Check: `verify-avastha`.

## 12. Jaimini Chara Karakas & special points

`CharaKaraka` + `PointKind` columns on `tbl_Chart_KeyDetails` (migration 14). Check:
`verify-jaimini` (against the `1_Ramakrishnan` JHora export).

- **Chara Karakas** (Aṣṭa, 8-karaka) — rank the 8 grahas (Sun…Saturn, Rahu) by degree within
  sign, descending; **Rahu's key is `30 − degreeInSign`**. AK → AmK BK MK PiK PK GK → DK.
  Ketu not ranked. Computed once per person from D1, stamped on every chart type. Cited by
  `tbl_Rule_Karaka` WHERE `KarakaScheme='Chara'` (migration 085, `SRC_PVR_INTEGRATED` §8.2
  Table 13) — the table was schema-designed for exactly this rule
  (`KarakaScheme`/`OrderIndex`/`ReverseForRahu`) but sat reserved and empty since migration 18.
  `CharaKarakaCalculator` stays hardcoded; CLI `verify-jaimini` cross-checks its order against
  the table.
- **Special points** — one D1 longitude each, then projected into all 21 vargas with the same
  `IVargaSignRule` a planet uses:
  - **AL + 12 Bhāva Arudhas** (`ArudhaCalculator`, `PointKind = Arudha`) — Parāśari pada per
    house, with the 1st/7th → 10th exception; A1 emitted as `AL`. Cited by
    `tbl_Rule_ArudhaFormula` (migration 085, `SRC_PVR_INTEGRATED` §9.2); CLI `verify-jaimini`
    asserts the row exists and cites the right source.
  - **Hora / Bhaava / Ghati Lagna** (`HoraLagnaCalculator` / `BhaavaLagnaCalculator` /
    `GhatiLagnaCalculator`, `PointKind = SpecialLagna`, codes `HL`/`BL`/`GL`) — Sun's sidereal
    longitude at the Vedic day's opening sunrise + 0.5° / 0.25° / 1.25° per clock-minute since
    it (PVR §5.2-5.4; `tbl_Rule_SpecialLagnaTimeRate`). The time-of-day-difference shortcut is
    valid for all three because each rate × 1440min lands on an exact multiple of 360°.
  - **Sree Lagna** (`SreeLagnaCalculator`, code `SL`) — natal Lagna + (Moon's fraction of its
    own nakshatra) × 360° (PVR §5.7; `tbl_Rule_SpecialLagnaFraction`). Pure — no sunrise
    dependency, unlike BL/HL/GL.
  - **All 11 upagrahas** (`SubPlanetCalculator`, `PointKind = Upagraha`) from migration-27
    rules under `SRC_PVR_INTEGRATED`: Sun chain (Dhūma / Vyatīpāta / Parivesha / Indrachāpa /
    Upaketu) + 6 time points (Kāla / Mṛtyu / Ardhaprahara / Yamaghaṇṭaka / Gulika = Saturn's
    eighth midpoint, Maandi = its start). Check: `verify-upagrahas` (in-memory);
    `verify-jaimini` expects stored charts regenerated under PVR names.
- **Karakāṁśa** (AK's D9 sign, PVR §7.3.6) — no dedicated calculator: `CharaKaraka` is already
  stamped onto every chart type's `tbl_Chart_KeyDetails` rows, D9 included, so the D9 row with
  `CharaKaraka = 'AK'` already carries it. `vw_ChartKarakamsa` (migration 082) is the read.
- Sunrise/sunset from `SwissEphemerisProvider.GetSunTimes` (`swe_rise_trans`,
  `SE_BIT_DISC_CENTER | SE_BIT_NO_REFRACTION`).
- **Deliberately not built** — `SRC_PVR_INTEGRATED` §5.7 states outright these are "beyond the
  scope of this book", and no other registered source covers them: Vighati Lagna, Varnada
  Lagna, Pranapada Lagna, Indu Lagna, Bhṛgu Bindu. Also not built: Swāṁśa chart, Jaimini rāśi
  dashas.

## 13. Strength (Ṣaḍbala / Bhāva Bala)

`ShadbalaCalculator` / `BhavaBalaCalculator`, PVR-first profile in `tbl_Rule_ShadbalaComponent`
/ `tbl_Rule_BhavaBalaComponent` with formula provenance, → `tbl_Fact_PlanetaryStrength*` /
`tbl_Fact_BhavaStrength*`. Available: total Ṣaḍbala, the six sub-totals, named component rows,
Kālabala, strongest-planet ranking, Bhāva Bala, `tbl_Fact_Vargottama` (explicit D1/D9 same-sign
facts). **DB contracts landed** (migrations 071–073): `tbl_Rule_ShadbalaMinimumRupas` (the
classical 5/6/5/7/6.5/5.5/5 rūpa thresholds — reproduce JHora %Strength within rounding),
`tbl_Rule_PlanetaryWar` (Yuddha orb + adjustment), `RuleParametersJson` on the six deferred
Kālabala sub-components, and `tbl_Dim_ShadbalaBenchmarkValues` (JHora golden totals).
**Dina / Horā / Tribhāga Bala and Graha Yuddha detection landed** (`FEAT-STRENGTH-01`, CLI
slice): `ShadbalaCalculator` reuses `PanchangaCalculator`'s own verified weekday lord and Hora
Lord (no re-derivation) for Dina Bala (45 virupas to the weekday lord) and Hora Bala (60 to the
running hora lord); Tribhāga Bala (day/night split into thirds ruled Mercury/Sun/Saturn by day,
Moon/Venus/Mars by night, 60 virupas to the part's lord, Jupiter classically exempt and always
scoring 60) reuses the Panchanga layer's Janma Ghaṭis. Graha Yuddha (`ComputeYuddha`) detects
the five tara grahas within 1° of D1 longitude and picks the winner by ecliptic latitude
(further north wins) — the diameter-based delta *magnitude* is deliberately left at 0 virupas
(no war present in `1_Ramakrishnan`'s chart either way): `SRC_RAMAN_GRAHA_BHAVA_BALAS` is a
DJVU with no text extract, so fabricating a number for it would violate this engine's
source-honesty rule. `verify-strength` checks both the engine (self-consistency against the
JHora export's own sunrise/sunset/birth-time data — the export prints only the Shadbala grand
total, no per-component breakdown to cross-check against) and the persisted
`tbl_Fact_PlanetaryStrengthComponent` rows; `tests/Ikiastrro.Yoga.Tests/ShadbalaKalaBalaTests`
(5) cover the same ground plus a synthetic war-winner case. Remaining (engine): Varṣa / Māsa /
Ayana Bala (need the cited Raman edition); the Yuddha Bala magnitude (same source blocker);
populate `MinimumRequiredRupas` + the Rashmi / Parāśara Iṣṭa/Kaṣṭa columns; Cheṣṭā
reconciliation; Vimśopaka Bala.

## 14. Yoga

`ProductionYogaEngine` is the composition root over every implemented Raman/PVR evaluator +
the Raman 201–300 ledger. Chart generation persists all source variants per chart
(`tbl_Fact_YogaInputEvaluations`, `tbl_Rule_Yoga` + applicability/context-requirement tables).
P0 inputs: required vargas, exact longitude, day/night, lunar phase, source-specific subject
sex — missing P0 context yields `NOT_EVALUATED`, not `ABSENT`. In progress: predicates
201–300, PVR P0 additions (Subha, Asubha, Guru-Mangala, Chamara, Khadga, Lagnaadhi, Saarada,
Dharma-Karmādhipati, Vipareeta Rāja), structured missing-requirement codes.

## Chart-reading method (JHora-anchored)

The universal varga-reading loop, per-varga significators, and worked lenses over the
`1_Ramakrishnan` reference export are consolidated here at a high level; the deep per-chart
reading guides (D1 / D9 / D10 / D60 + the short index for the rest) are retained under
`docs/research/domain/` as reference material.
