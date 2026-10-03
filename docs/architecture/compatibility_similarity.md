# Compatibility (marriage matching) and similarity (horoscope search)

Status: proposal, 2026-10-03. Not yet a decision record. Intended to become `decisions/010` (matching) and `decisions/011` (similarity).

Two features share one set of persisted layers (L0 to L2) and differ only in what they read from them.

| | Compatibility (marriage) | Similarity (horoscope search) |
|---|---|---|
| Question | Are A and B suited? | Which charts look like this one, or fit this pattern? |
| Reads | L3 match profile (a view) | L3b feature atoms (a table) |
| Probe | The seeker, who is persisted | Any saved chart, or a hand-built pattern |
| Searched set | Candidates, which are transient | A reference corpus, which is persisted |

The corpus is persisted because similarity search queries a large set repeatedly. Recomputing thousands of charts per query defeats the purpose. This is a deliberate exception to "candidates are not persisted", and it applies only to the corpus.

## 1. Layers

Each layer reads only the layer above it. Nothing below L0 may call the Swiss Ephemeris.

| Layer | What it is | Where it runs | Persisted |
|---|---|---|---|
| L0 Ephemeris | Raw Swiss Ephemeris output: JD/UTC, ayanamsa, Lagna and cusps, and per body longitude, latitude, speed, retrograde flag | C#, the only ephemeris caller | Yes, existing `tbl_Chart_KeyDetails` raw columns |
| L1 SQL-derived | Arithmetic or lookup over L0 plus reference tables: sign, house, nakshatra and pada, Gana, Yoni, Nadi, Varna, Vasya, Tara, Rajju, dignity, aspects, dasha periods | SQL views, computed columns, backfills | Yes, or computed on read |
| L2 Rule-evaluated | Facts that need rule logic: Kuja dosha, papasamya, Rahu-Ketu dosha, yogas, strengths, karakas, UL and DK | C# engines over L1 | Yes, written once per chart |
| L3 Match profile | One row per saved person joining L1 and L2 for matching | View `vw_Chart_MatchProfile` | No |
| L3b Feature atoms | Flattened comparable facts per chart (section 5) | SQL and C# extractors over L1 and L2 | Yes |
| L4 Family | People, relationships, life events | Tables | Yes |
| L5 Relationship analysis | Kuta scores, doshas, synastry for real relationships only | C# matching engine | Yes, existing relationships only |
| L6 Match search | Seeker criteria, transient candidates, shortlist | In memory plus small tables | Seeker and shortlist only |

## 2. What already exists

- Moon sign, nakshatra and pada in `tbl_Chart_KeyDetails` and `vw_ChartMoonContext`.
- Gana, Yoni animal and gender, Nadi seeded (migration 098, source `SRC_VASUDEV_MATCHING_CHARTS`).
- Varna and sign lord on `tbl_SignAttributes`.
- Natural planetary relationships (7 planets, enough because Moon-sign lords are never nodes).
- House lords, aspects, conjunctions, Vimshottari dashas, D9, chara karakas (DK), Upapada and Arudha special points.
- `BirthDetails.Sex`, needed because several kutas are directional.

## 3. Gaps

Classical kuta data (reference tables, sourced from Vasudev; the repo norm is one cited source, never seeded from memory):

| Need | Used by |
|---|---|
| Vasya sign groups, with the Sagittarius and Capricorn split by degree | Ashtakoota |
| Yoni score matrix (14x14) and enemy pairs | Ashtakoota, Dasakoota |
| Gana, Graha Maitri, Bhakoot, Nadi score matrices, plus Nadi and Bhakoot cancellation exceptions | Ashtakoota |
| Tara groups and good/bad classification | Ashtakoota, Dasakoota |
| Rajju groups with direction, Vedha pairs | South Indian Dasakoota |
| Dina, Mahendra, Stree-Deergha, Rasi, Rasi-Adhipati rules | South Indian Dasakoota |

Chart-level facts to compute (L2):

- Kuja dosha: Mars in houses 1, 2, 4, 7, 8, 12 from Lagna, Moon and Venus, with a cancellation table. Stored as separate flags so cancellation logic is not baked in.
- Papasamya: malefic point score by house from Lagna, compared between the two charts.
- Rahu-Ketu (sarpa) dosha.
- Marriage-house health: 7th house, 7th lord, Venus status.
- Jaimini markers: DK, Upapada Lagna and the 2nd from UL (PVR is already cited in the life-matter reference).
- Navamsa: D9 Lagna and D9 7th.
- Dasha: current and upcoming lords for both people, dasha sandhi.
- Cross-chart synastry (tier 2): Moon-to-Moon, Venus-to-Mars, inter-chart aspects, house overlays.

A second source is needed for the dosha rules (for example PVR or BPHS). Vasudev covers kutas.

## 4. Compatibility design

### 4.1 L4 family and relationships

- `tbl_Dim_RelationType`: spouse, parent-of and child-of, sibling, step, adopted, in-law, and so on. Each row has an inverse type, a role label (father, mother, son, daughter), and a tier: Core (spouse, parent, child), Extended, or Other.
- `tbl_Person_Relationship`: direct edges only (spouse, parent to child), plus `IsAdopted`, `IsStep`, `Start`, `End`, `Status`.
- Siblings, grandparents, uncles and in-laws are derived by a recursive view, `vw_Family`, over those edges. Birth order comes from DOB. Edges stay the source of truth, so tiers cannot go stale.
- `tbl_Person_LifeEvent`: marriage, divorce, child birth, death, with date and place. It enables dasha-at-marriage and dasha-at-child-birth, and it is the main ground-truth table for validation and the Analytics project.

### 4.2 L5 relationship analysis (persisted for real relationships only)

- `tbl_Relationship_Analysis`: `RelationshipId`, scheme, rule-set version, total score, run date.
- `tbl_Relationship_Kuta`: per-kuta score, max, status, reason.
- `tbl_Relationship_Dosha`: dosha comparison and cancellations.
- Synastry rows: cross-chart aspects and house overlays, for parent-child pairs as well as spouses.
- Keyed to the rule-set version, so a rule change is handled by a rebuild by version.
- There is no pair table for arbitrary pairs. A pair result is a pure function of two profiles and the rule tables, so storing it for N people means N-squared rows that go stale.

### 4.3 L6 match seeking

- `tbl_MatchSeeker`: `PersonId` plus criteria (sex sought, age range, scheme, hard filters such as Nadi and Kuja). The seeker already has a persisted full chart.
- Candidates are not persisted. They come from an imported list or CSV and are scored in memory. A live pass computes Lagna and the nine grahas through one Swiss Ephemeris pass and builds an in-memory profile. If a candidate must survive a refresh, use a batch-staging table with an expiry.
- One scorer, two data sources, behind `IChartFacts`:
  - `StoredChartFacts` reads L0 to L2 for saved people and the seeker.
  - `LiveChartFacts` computes from birth data for candidates.
  - `verify-matching` asserts both give the same answer for a saved person.
- `tbl_MatchShortlist`: seeker, candidate birth data, status, note. No scores, so it cannot go stale. Promoting a candidate means "Save as person", which runs full chart generation and persists L0 to L2. They can then become a real relationship and get L5 analysis.

### 4.4 Scoring engine

- `Score(profileA, profileB, scheme, roles)` returns per-kuta score, status, reason, cancellations and source code.
- Roles (bride and groom) come from `Sex`. Same-sex or missing `Sex` makes the user choose roles.
- Schemes: `Ashtakoota8` (36 points), then `Dasakoota10` (South Indian), behind an `IKutaScheme` interface.
- The engine is the only implementation. SQL does cheap pre-filtering only (sex, Nadi mismatch, Kuja match, age range).
- Two tiers:
  - Tier 1, profile-only: kuta score plus dosha comparison, run for the seeker against the whole list and ranked.
  - Tier 2, on demand for one chosen pair: synastry, D9 and UL comparison, dasha compatibility.
- The dosha and synastry layers are shown beside the kuta total and never merged into it.

## 5. Similarity design

### 5.1 Feature atoms (L3b)

Every chart is flattened into small comparable atoms, derived from L1 and L2 only, with no ephemeris call.

- `tbl_Dim_FeatureType`: code, scope (D1 or a varga), extractor (SQL view or C# engine). A new kind of fact is a new row plus an extractor, with no schema change.
- `tbl_Chart_Feature`: `(ChartResultId, FeatureTypeId, SubjectKey, ValueKey, Varga)`, indexed on `(FeatureTypeId, SubjectKey, ValueKey)` as an inverted index.

Feature types, simple to rich:

- Placement: planet in sign, house, nakshatra or pada, Lagna sign.
- Dignity and state: exalted, debilitated, combust, retrograde, vargottama.
- Relations: conjunction sets, mutual aspects, house-lord placements, karaka roles (AK, DK).
- Yogas and strength bands.
- Stelliums at three levels of abstraction:
  - Exact: Sun, Mars, Mercury and Venus in Aries.
  - Relative: the same four in the same house from Lagna.
  - Shape: any N or more grahas sharing a sign, in any sign.
- Event-anchored atoms from `tbl_Person_LifeEvent`, for example the dasha lord at marriage.

### 5.2 Query modes

1. Pattern search (exact): a criteria builder (planet set, sign, house or nakshatra, "at least k of n", varga, dignity, conjunction, yoga, Lagna) that compiles to SQL over the atom index. Patterns are saved in `tbl_Search_Saved` and `tbl_Search_Term`.
2. Seed from a person: pick a chart, the app lists its most distinctive atoms, the user ticks the ones that matter and sets weights, and the app builds the pattern.
3. Similarity ranking (fuzzy): score = sum of weights of shared atoms divided by the sum over the probe's selected atoms. Atom weight = user weight multiplied by rarity, `-log(baseRate)`. Output is a ranked list with chips showing the shared atoms and each atom's base rate.

### 5.3 Base rates

- Sun, Mercury and Venus sit close together, so "three of them in one sign" is common. Rare atoms must count for more.
- `tbl_Feature_BaseRate`: frequency of each atom in a baseline population.
- The baseline is generated in bulk by the CLI from L0 for many birth times spread over decades (sign-level positions do not depend on place), using the existing Swiss path.
- The UI shows "this pattern occurs in about x% of the baseline" so a coincidence is not presented as evidence.
- Patterns are zodiac-dependent. The app is Lahiri-locked (decision 004), so all matches are in Lahiri. Check that any worked example (for instance the four-planet Aries stellium) holds under Lahiri when the data is seeded.

### 5.4 Reference corpus and tags

- `tbl_Dim_PersonSource` separates Personal, Family and Reference (public figures). Reference people carry a Rodden-style data-quality rating and a source citation.
- Check the licence of any dataset before bulk import. The existing CSV and JKD import covers the mechanism.
- `tbl_Person_Tag`: controlled vocabulary for occupation, notable traits and outcomes. This allows the search to run both ways: patterns to people, and charts tagged X to the patterns they share. It connects to the parked base-rate and Validation items in `ROADMAP.md`.
- Privacy: only public figures go in the reference corpus. Personal and family charts are never mixed into a public search unless the user opts in.

## 6. Roadmap

Shared foundation:

1. L4 family and relationship tables, `tbl_Person_LifeEvent`, `vw_Family`.
2. L1 and L2 additions: Vasya, Tara, Rajju, Vedha lookups, Kuja and papasamya facts.

Compatibility track:

3. `vw_Chart_MatchProfile`, `IChartFacts`, the Ashtakoota engine, `verify-matching` against the book's worked examples.
4. Pair UI and L5 persistence for real relationships.
5. Seeker, live candidates, shortlist; then Dasakoota and synastry.

Similarity track:

6. Feature-type dimension, extractors, `tbl_Chart_Feature`, `verify-features` against known charts.
7. Pattern search UI, including "seed from a person".
8. Corpus import, source and rating fields, tags.
9. Base rates and similarity ranking with rarity weights.
10. Outcome-tag statistics, event-anchored atoms, side-by-side wheel compare.

Steps 6 and 7 can run in parallel with step 3 once step 1 is done. Feature work goes on workstream branches: database and CLI work on `workstream/database` and `workstream/cli`, UI on `workstream/ui`.

## 7. Open decisions

1. Corpus source: a named dataset (for example Astro-Databank, subject to licence), or only the user's own imports.
2. Atom depth for v1: placements, stelliums and conjunctions first, with yogas and event atoms later?
3. Order: compatibility first or similarity first. Similarity needs less new reference data and could ship earlier.
4. Schemes: Ashtakoota only first, or both Ashtakoota and Dasakoota.
5. Sources: Vasudev for the kuta matrices, and which second source for the dosha rules.
6. Scope of "kids" in v1: birth order, adopted and step flags and life events are included. Per-child notes or putra-bhava prediction versus actual children are not.

## 8. Build log

### 8.1 Slice 1 (2026-10-03): Ashtakoota calculator in Core

Branch `workstream/cli` (worktree `ikiastrro.wt/cli`), uncommitted at the time of writing.

- `Core/Engines/Matching/AshtakootaCalculator.cs` with `KutaModels.cs`: a pure calculator over a `MatchPerson` (Moon sign, nakshatra number, Gana, Yoni animal, Nadi). Gana, Yoni and Nadi stay inputs read from `tbl_Nakshatras` (migration 098), so the stored table is the single source.
- `DignityEngine.NaturalAttitude(planet, other)`: a small public accessor over the existing natural-friendship table, for Grahamaitra.
- `tests/Ikiastrro.Yoga.Tests/AshtakootaCalculatorTests.cs`: 43 tests built from the book's own examples, all passing. The suite is 473/473.
- No database or UI changes yet. The rule tables are in code, with page citations, following the existing precedent where a C# truth table is checked against reference rows.

### 8.2 What the source book actually says (read from the scanned pages, Ch. VI, pp.65-83)

This corrects assumptions made earlier in this document.

| Factor | What the book gives | How the calculator treats it |
|---|---|---|
| Varna (1) | Rasi groups, planet Varnas, compensation by sign rulers (p.66-67) | Implemented as written. |
| Vashya (2) | A list of "signs - sign" rows (p.67), not the five-class scheme assumed earlier | Implemented as boy's sign in the girl's sign's list. The direction is fixed only by the book's one example. **To confirm.** |
| Dina / Tara (3) | Count from the girl's star to the boy's, remainder 2, 4, 6, 8 or 0 is good (p.68) | Implemented. |
| Yoni (4) | Animal table (p.69), 4 for the same animal, 0 for hostile, "2 to 3" for passable. **No hostile-pair table.** | Same animal 4; different animals left **Unscored**, which widens the total to a min-max range. **Needs a second source.** |
| Grahamaitra (5) | 5 / 4 / 3 / 2 / 0 by the two rulers' attitudes (p.70-71) | Implemented from the existing natural-friendship table. |
| Gana (6) | Nine-way score table (p.72) | Implemented. |
| Rasi / Bhoo (7) | Qualitative verdicts per distance, directional, with exceptions (pp.73-75), maximum 7 | Favourable scores 7, unfavourable 0. **That 7/0 mapping is this project's reading.** |
| Nadi (8) | 8 if different, 0 if same (p.75) | Implemented. |
| Rajju | Five groups, no numeric value (p.76-77) | Reported as Present/Absent, no score. No ascending/descending direction is given. |
| Stree Deergha, Mahendra | Rules with no numeric value (p.77) | Reported as Present/Absent. |
| Vedha | Named as the tenth factor (p.66, p.78). **No table in the scanned chapter.** | Not computed. **Needs a second source.** |
| Dasakoota | The book lists ten factors but scores only the eight Ashtakoota ones | The 10-factor scheme is therefore not a separate scored scheme here. |

The book's pass mark is 18 of 36. Its worked example (Jyeshta boy, Anuradha girl, both Scorpio) totals 28, or 37 of 45 if Rajju is given 9; the calculator reproduces 28.

The book also says Kuta agreement "is not decisive" and should come only after longevity, health, balance of doshas and Kuja dosha have been cleared (p.79). This supports keeping the dosha comparison separate from the Kuta total, as designed above.

### 8.3 Next slices

1. Read the saved Moon nakshatra, Gana, Yoni and Nadi into `MatchPerson` for two saved people, and show the Kuta table on a pair page.
2. A second source for the Yoni hostile pairs and for Vedha; a decision on the Vashya direction and the Rasi 7/0 reading.
3. Kuja dosha, papasamya and Rahu-Ketu dosha (the book's Ch. on Dosha Samya, around p.55-61, is already in the same PDF).
4. The L4 relationship tables and the match-profile view.
