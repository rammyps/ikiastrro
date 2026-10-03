# Astro Facts audit: what is persisted, what is computed, what is missing

Date: 2026-10-03. Purpose: decide how much extractor work is needed before Astro Facts can feed a person-level feature store for Key Inference statistics, compatibility and similarity (see `compatibility_similarity.md` and `v10-statisticalbuild.md`).

## Method and limits

- Read from source: `AstroFacts.razor` section list and `@inject` repositories; the SQL each repository reads or writes (`FROM`/`JOIN`/`INTO` targets); component headers and the `Core/Engines` folders they call; migrations 081, 156, 163 and `db/ikiastrro.sql`.
- Sections 1 to 12 are "what the code says". Section 13 is the spot-check against the live database (`localhost\SQLSERVER2025`, database `ikiastrro`, read-only queries) for two saved people, RamakrishnanP (id 5) and RameshwariS (id 6). It confirmed most classifications and corrected two (see 13.4).

## Readiness classes

| Class | Meaning | Extractor effort |
|---|---|---|
| A | Persisted in a table or view, one row set per chart | SQL only |
| B | Derivable by SQL from persisted rows (join, computed column, lookup) | SQL view |
| C | Computed live in a Core engine from persisted rows; nothing stored | Small C# step to write fact rows |
| D | Logic lives in the Web project, not Core | Move to Core first, then C |
| E | Live or time-varying (needs ephemeris at read time, or depends on "now") | Out of scope for v1 features |
| F | Missing: no data, or data exists but is never shown | New reference data and/or engine |

## 1. Natal chart tab

| Section | Backing | Class | Notes for features |
|---|---|---|---|
| Panchanga | `tbl_Chart_Panchanga` / `vw_ChartPanchanga` (tithi, karana, nitya yoga, weekday, hora lord, sunrise/sunset, janma ghatis, night birth) | A | Ready. Categorical features. |
| Planet positions | `tbl_Chart_KeyDetails`: sign, degrees, nakshatra, pada, nakshatra and sub-lords, house from Lagna/Sun/Moon, retrograde, speed, latitude | A | Ready. Core source of most planet features. |
| Sign and nakshatra | `tbl_Rule_RasiNakshatraCombination` (reference table) | A | Reference text, not a per-person feature. |
| Upagrahas, Arudha padas, Graha arudhas | `tbl_Chart_KeyDetails` rows with `PointKind` (`SpecialLagna`, `Arudha`, `Upagraha`) | A | Position only; dignity and nakshatra null by design. |
| Yogi / Avayogi / sensitive points | `Core/Engines/KeyInfo` (`YogiAvayogi`, `SensitivePoints`) over D1 key details. Header says "Nothing is stored". | C | Cheap to persist. |
| Nava Tara | `Core/Engines/KeyInfo/NavaTara` over D1 key details | C | Same arithmetic as Tara kuta, so reuse for matching. |
| Moon / lunar phase | `vw_ChartMoonContext` | A | Ready. |
| Dignity | `ChartViewModel.BuildExaltationRows` (Core) over key details; `DignityStatus`, `IsCombust`, `OwnSigns`, exaltation and debilitation signs are also persisted columns | A for status, C for closeness-to-exaltation and functional nature | Status ready; closeness bar is a derived number. |
| Motion | `IsRetrograde`, speed, latitude in key details | A | Ready. |
| Mrityu Bhaga / Pushkara | `Core/Engines/KeyInfo/MrityuPushkara` over key details | C | Flags; cheap to persist. |
| Planetary states (avasthas) | `tbl_Fact_PlanetaryState`, `tbl_Fact_PlanetaryStateFlag`, `tbl_Dim_PlanetaryState`; also `tbl_Fact_PlanetAvastha` | A | Ready. Several avastha schemes; choose one per feature. |
| House lordship | `tbl_Chart_HouseLords` | A | Ready. |
| House findings | `vw_ChartHouseLordInterpretation` | A | Text interpretation rows; use the underlying placement codes as features, not the prose. |
| House verdicts | `HouseVerdicts.For(...)` in `Core/Engines/Houses/HouseVerdicts.cs` (moved from Web, branch `workstream/ui`), over the chart plus Argala planets per house; `ArgalaFacts.HouseVerdicts` in Data builds those from the stored Argala facts | C | Was class D. Now reachable from any C# analytics writer; persist the Raman verdict and influence counts as features. |
| Argala | `tbl_Fact_Argala` | A | Ready. Used by existing house statistics. |
| Graha drishti | `tbl_Fact_GrahaDrishtiStrengths` / `vw_ChartGrahaDrishtiStrengths`; `tbl_Chart_Aspects` | A | Ready. |
| Rasi drishti | `RasiDrishtiCalculator` (Core), a static rule over signs | B | A reference matrix; join with sign placement. |
| Dispositors | `Core/Engines/Dispositors` over key details (self-disposed, mutual reception, cycle) | C | Persist outcome and final dispositor. |
| Conjunctions | `tbl_Chart_Conjunctions`, `tbl_Chart_MultiGrahaConjunction`, `tbl_Chart_MultiGrahaConjunctionMember` | A | Stellium size derivable by SQL from members. |

## 2. Transit tab

| Section | Backing | Class |
|---|---|---|
| Transit wheel, Gochara | `GocharaRepository.GetSnapshots(asOf)`: live ephemeris at read time | E |
| Dasha selector | `tbl_Chart_DashaPeriods` (persisted tree) | A |
| Sade Sati | `tvf_Chart_SadeSatiPeriods` | A (function over persisted rows) |

Dasha tree is persisted, so "dasha lord at birth" and "dasha lord at a given date" are SQL-only. Transits are time-varying and stay out of the v1 person-level store.

## 3. Strength tab

| Section | Backing | Class |
|---|---|---|
| Ṣaḍbala | `tbl_Fact_PlanetaryStrength`, `tbl_Fact_PlanetaryStrengthComponent`, `vw_ChartShadbala` | A |
| Bhava bala | `tbl_Fact_BhavaStrength`, `tbl_Fact_BhavaStrengthComponent`, `vw_ChartBhavaBala` | A |
| Ashtakavarga | `tbl_Fact_BhinnaAshtakavarga`, `tbl_Fact_BhinnaAshtakavargaContribution`, `tbl_Fact_SarvaAshtakavarga`, `tbl_Fact_AshtakavargaPinda`, `vw_ChartAshtakavarga` | A |

All three are already inputs to the existing house statistics (`tbl_Fact_HouseStrengthStatistics`, migration 156).

## 4. Karakas and special lagnas tab

| Section | Backing | Class | Notes |
|---|---|---|---|
| Chara karakas | `CharaKaraka` column in key details | A | Ready (AK to DK). |
| Special lagnas (Hora, Ghati, Indu, Pranapada, Sree, Bhaava) | Core engines in `Engines/Karakas`; results written as `SpecialLagna` rows | A for the rows the pipeline writes | Check the code list in `tbl_Chart_KeyDetails` for exactly which special lagnas are persisted; the SPT codes seeded in `ikiastrro.sql` are AL, BL, GL, GULIKA, HL, MAANDI, SL. |
| Karakamsa | `vw_ChartKarakamsa` | A | Ready. |
| Upapada Lagna | Arudha rows (`SPT_` codes) | A | Confirm the UL row is among the persisted Arudhas before relying on it. |

## 5. Yogas

`vw_ChartYogaEvaluations` over `tbl_Fact_YogaInputEvaluations`, rule tables and `YOGA_*` outcome nature (migration 159). Class A. The yoga evaluators are in `Core/Engines/Yoga`. Features: present flag per yoga code, and counts by outcome nature (auspicious, inauspicious). The yoga rules are still being standardised, so keep them out of v1 population statistics (as `v10-statisticalbuild.md` already says).

## 6. Vargas

| Section | Backing | Class |
|---|---|---|
| Per-varga positions | `tbl_Chart_KeyDetails` for every `ChartType`; `VargaLongitudeDegrees` persisted | A |
| Amsabala | `tbl_Fact_Amsabala`, `vw_ChartAmsabala` (four schemes; Key Inference reads `SHODASAVARGA` only) | A |
| Vargottama | `tbl_Fact_Vargottama` | A |
| Cross-varga confirmation | Not built (parked, see `ROADMAP.md`) | F |

## 7. All charts

Grid view of the same per-chart key details. Nothing additional to persist.

## 8. Key Inference (statistical layer)

| Item | State |
|---|---|
| House Capacity, Consistency, Context, Overall Support | Persisted in `tbl_Fact_HouseStrengthStatistics` for D1 **and 20 vargas** (12 rows per chart, 21 charts per person; spot-check 13.4). `vw_AnalyticsHouseFeatures` filters to D1 and to opted-in, eligible research subjects. |
| Matter-specific observation (house lord plus kāraka planets) | Live only; the feature dictionary defers it to a later contract. |
| Population comparison | `tbl_Fact_StatisticalComparisons`; percentiles suppressed below 30 measured people. |
| Person-level, planet-level and chart-level feature views | Do not exist (Phase 1 of `v10-statisticalbuild.md`). |
| SIND-UNI special-lagna grid | Shared view with Astro Facts; no stored statistics. |

## 9. Compatibility inputs (marriage)

| Input | State |
|---|---|
| Moon sign, nakshatra, pada | A (key details) |
| Gana, Yoni animal and gender, Nadi | In `tbl_Nakshatras` (migration 098) and in the Core `NakshatraReference` model. **Not shown anywhere in the Astro Facts UI.** Class F for display. |
| Varna | `tbl_SignAttributes.Varna_Class` (A), not shown in the UI. |
| Sign lord, natural relationships | A |
| Tara | C. Nava Tara math exists in Core. |
| Vasya, Rajju, Vedha, Dina, Mahendra, Stree-Deergha | F. No reference data. |
| Kuja dosha, papasamya, Rahu-Ketu dosha | F. No code found. |
| 7th lord, Venus status, DK, UL, D9 7th | A or B from existing rows. |
| Gender | `BirthDetails.Sex` (A) |

## 10. Similarity inputs

Most atoms are class A or B from key details (planet in sign, house, nakshatra, dignity, retrograde, combust), conjunction tables (stelliums), and yoga evaluations. No work is needed beyond a feature-store extractor.

## 11. Findings

1. **Most facts are already persisted.** Positions, panchanga, strengths, ashtakavarga, argala, drishti, conjunctions, avasthas, vargottama, amsabala, yogas and dashas are class A. The feature store is mainly SQL.
2. **Five sections are live-only (class C):** Yogi/Avayogi, sensitive points, Nava Tara, Mrityu/Pushkara, dispositors. Each is a small engine over key details; persisting them is cheap.
3. **House verdicts were stuck in the Web project (class D).** Moved to Core on 2026-10-03 (workstream/ui, uncommitted); now class C.
4. **Gana, Yoni, Nadi and Varna are stored but never displayed.** They are the first compatibility facts to surface.
5. **Genuinely missing:** Vasya, Rajju, Vedha, the other Dasakoota rules, Kuja/papasamya/Rahu-Ketu dosha, cross-varga confirmation, and matter-specific observations.
6. **Transits and "as of" dashas are time-varying** and should not enter the v1 person-level feature store.
7. **Key Inference statistics cover one thing:** D1 house strength. Every other group of facts is a candidate feature.

## 12. Proposed next steps (from this audit)

1. ~~Spot-check one real person~~ Done, section 13.
2. ~~Move `HouseVerdicts` from Web to Core.~~ Done 2026-10-03 on `workstream/ui` (uncommitted): `Core/Engines/Houses/HouseVerdicts.cs`, `ArgalaFacts.HouseVerdicts`, one added test.
3. Write the feature dictionary (`tbl_Dim_FeatureDefinition`) with one row per candidate feature from sections 1 to 6, flagged by class.
4. Build `tbl_Fact_PersonFeature` and `vw_AnalyticsPerson`, starting with the class A and B features.
5. Add small persistence steps for the five class C sections.
6. Surface Gana, Yoni, Nadi and Varna in Astro Facts.
7. Add the missing compatibility reference data and dosha engines (see `compatibility_similarity.md`).

## 13. Spot-check: RamakrishnanP (5) and RameshwariS (6), 2026-10-03

### 13.1 Coverage: every class A source is populated for both

Both people have 22 chart results (D1, 20 vargas, `VimshottariDasha`) and near-identical row counts per source.

| Source | Rows per person (5 / 6) |
|---|---|
| `tbl_Chart_KeyDetails` (all charts) | 1029 / 1029 |
| D1 key details, `PointKind = Graha` (9 grahas + Ascendant) | 10 / 10 |
| D1 Arudha (AL + A2 to A12), GrahaArudha, SpecialLagna, Upagraha | 12, 9, 7, 11 for both |
| `tbl_Chart_HouseLords`, `tbl_Fact_HouseStrengthStatistics` | 252 / 252 (12 houses x 21 charts) |
| `tbl_Chart_Aspects`, `tbl_Chart_Conjunctions`, `tbl_Chart_MultiGrahaConjunction` | 241/237, 87/85, 51/44 |
| `tbl_Fact_PlanetaryStrength` (7 planets), `tbl_Fact_BhavaStrength` | 7 / 7, 12 / 12 |
| `tbl_Fact_BhinnaAshtakavarga`, `tbl_Fact_SarvaAshtakavarga`, `tbl_Fact_AshtakavargaPinda` | 1764, 252, 147 for both |
| `tbl_Fact_Amsabala`, `tbl_Fact_Vargottama`, `tbl_Fact_GrahaDrishtiStrengths` | 28, 10, 1701 for both |
| `tbl_Fact_Argala`, `tbl_Fact_PlanetaryState` | 104 / 124, 189 / 189 |
| `vw_ChartYogaEvaluations` | 414 / 414 (388 / 386 EVALUATED, 26 / 28 NOT_EVALUATED) |
| `tbl_Chart_Panchanga`, `vw_ChartMoonContext`, `vw_ChartKarakamsa` | 1 / 1 each |
| `tbl_Chart_DashaPeriods` | 892 / 836, all under chart type `VimshottariDasha` |

Neither person is enrolled in `tbl_Dim_AnalyticsSubjects`, as designed.

### 13.2 Spot-checked values (computed independently of the app)

- Moon longitude to nakshatra and pada: 217.21 degrees is Anuradha pada 2 (5) and 291.35 degrees is Shravana pada 4 (6). Both match the stored values.
- Vedic weekday: born 1981-04-22 05:30, before local sunrise, stored as Tuesday (the previous civil day); 1983-12-09 stored as Friday. Both correct.
- Tithi from elongation: 209.0 degrees is Krishna Tritiya (5); 58.0 degrees is Shukla Panchami (6). Both match.
- Upapada Lagna is stored as Arudha `A12`: Sagittarius (5), Pisces (6). Class A confirmed.
- Special lagnas persisted: BL, GL, HL, IL, PP, PS, SL (7 of them).

### 13.3 Side-by-side comparison

| Fact | RamakrishnanP | RameshwariS |
|---|---|---|
| Lagna | Aries (Ashwini 1) | Gemini (Ardra 4) |
| Moon | Scorpio, Anuradha 2, Debilitated, H8 | Capricorn, Shravana 4, Friend, H8 |
| Sun, Mars, Mercury, Venus | All four in Aries (H1): Sun exalted, Mars Moolatrikona, Mercury and Venus Enemy; Mars, Mercury and Venus combust | Scorpio H6, Virgo H4, Sagittarius H7, Libra H5 |
| Jupiter / Saturn | Virgo H6 retrograde (Great Enemy) / Virgo H6 retrograde | Scorpio H6 combust (Great Friend) / Libra H5 Exalted |
| Rahu / Ketu | Cancer H4 / Capricorn H10 | Taurus H12 / Scorpio H6 |
| Chara karakas (8-karaka scheme, Rahu included) | AK Rahu, AmK Venus, BK Saturn, MK Jupiter, PiK Sun, PK Moon, GK Mars, DK Mercury | AK Jupiter, AmK Sun, BK Moon, MK Mars, PiK Saturn, PK Mercury, GK Venus, DK Rahu |
| 7th sign / lord | Libra / Venus in Aries H1 (Enemy) | Sagittarius / Jupiter in Scorpio H6 (Great Friend) |
| 7th-house strength (Capacity, Consistency, Context, Support) | 65, 25, 25, 38 | 37, 13, 50, 33 |
| Upapada (A12), Arudha Lagna (AL) | Sagittarius, Capricorn | Pisces, Pisces |
| Karakamsa | Libra (AK Rahu) | Pisces (AK Jupiter) |
| Panchanga | Krishna Tritiya, Vanija, Vyatipaata, Tuesday, night birth | Shukla Panchami, Balava, Vyaaghaata, Friday, night birth |
| Birth Mahadasha; Mahadasha on 2026-10-03 | Saturn; Venus (2018 to 2038) | Moon; Saturn (from 2026-06-06) |
| Present yogas, by outcome nature | 58 (31 auspicious, 23 inauspicious, 3 mixed, 1 contextual) | 32 (22 auspicious, 9 inauspicious, 1 mixed) |

Matching attributes from the Moon (all read from stored tables):

| Attribute | RamakrishnanP | RameshwariS |
|---|---|---|
| Nakshatra / sign | Anuradha / Scorpio | Shravana / Capricorn |
| Varna | Brahmanas | Vaisyas |
| Gana | Deva | Deva |
| Yoni | Deer (female) | Monkey (female) |
| Nadi | Pitta | Kapha |

Facts derivable by hand from persisted rows, as a demonstration that these are class B. They are not engine-verified:

- Mars house from Lagna, Moon and Venus: husband 1, 6, 1; wife 4, 9, 12. Counting the houses 1, 2, 4, 7, 8, 12, both have a Kuja flag from Lagna and from Venus, and neither from the Moon.
- Moon-sign distance: Scorpio to Capricorn is the 3rd, and Capricorn to Scorpio is the 11th (the 3/11 pair, not 2/12, 5/9 or 6/8).
- Tara count from the husband's nakshatra to the wife's is 6, and from the wife's to the husband's is 5.
- Nadi differ (no Nadi match); Gana are the same.

Two of these results depend on a choice the app makes once and persists:

- **Chara karaka scheme.** The 8-karaka scheme with Rahu is stored, so the husband's Atmakaraka is Rahu and the wife's Darakaraka is Rahu. Any feature that uses AK or DK must record the scheme.
- **Combustion.** The stored `IsCombust` flag marks Mars, Mercury and Venus as combust in the husband's chart. Features should record the combustion rule set.

### 13.4 Corrections and new findings

1. **Correction: house statistics are not D1 only.** `tbl_Fact_HouseStrengthStatistics` holds 12 rows for each of 21 chart types. Only `vw_AnalyticsHouseFeatures` restricts to D1. This makes varga-level features cheap (section 8 updated).
2. **Correction: dashas are not on the D1 chart result.** The tree is stored under chart type `VimshottariDasha`. `SequenceInParent` is the position in the 120-year cycle, not chronological order, so queries must order by `StartDate`.
3. **`vw_ChartKarakamsa` is misleading.** The column `AtmaKarakaD9Longitude` holds the planet's real nirayana longitude (the same value for every chart type), not a D9-space longitude. `AtmaKarakaD9Nakshatra` and `AtmaKarakaD9Pada` are always NULL because varga key-detail rows carry no nakshatra. The Karakamsa sign itself is correct (both verified by hand). Rename the column or compute the D9 nakshatra before using these as features.
4. **Yoga coverage is partial.** 26 to 28 of 414 yoga rows per person are `NOT_EVALUATED` (`REQUIRED_CALCULATION_NOT_IMPLEMENTED`). A feature must treat these as missing, not absent.
5. **The Aries stellium from the similarity example is in your data.** The husband's chart has Sun, Mars, Mercury and Venus all in Aries (rows in `tbl_Chart_KeyDetails`), so that pattern is directly queryable with no new extraction.
6. **Kuja dosha and Tara are derivable from persisted rows** (`HouseNumberFromLagna`, `HouseNumberFromMoon`, nakshatra ids), so they move from "missing" to class B. The cancellation rules still need a cited source.
