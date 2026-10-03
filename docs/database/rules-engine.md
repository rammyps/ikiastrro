---
last_updated: 2026-10-03
workstream: database
togaf: C — Data Architecture
safe: Solution Intent (fixed)
---

# Database — rules engine

Classical rules as versioned data. Decision: [`decisions/001-star-schema-rules-engine.md`](../../decisions/001-star-schema-rules-engine.md).

## The three infixes (`STANDARDS.md` §D.1)

| Infix | Holds | Example |
|---|---|---|
| `tbl_Dim_*` | vocabulary / catalogue dimensions | `tbl_Dim_PlanetaryState`, `tbl_Dim_Source`, `tbl_Dim_ChartType` |
| `tbl_Rule_*` | a classical rule, versioned by `RuleSetId` | `tbl_Rule_NaturalRelationship`, `tbl_Rule_VargaScheme` |
| `tbl_Fact_*` | a computed result for one chart | `tbl_Fact_PlanetaryState`, `tbl_Fact_PlanetaryStrength` |

## Versioning contract

- `tbl_Rule_Sets` is the version dimension every rule table hangs off. Active set:
  `Parashari-Classical` (Id 1).
- **A rule row is immutable once any fact references it.** A convention change ships a new
  `RuleSetId` and its full row set — never an `UPDATE`.
- Every `tbl_Rule_*` row carries a **portability tail**: `MethodCode`, `RuleParametersJson`,
  `CalculationNarrative`, `SourceRefCode` (→ `tbl_Dim_Source`), `IsActive`.
- `tbl_Rule_Catalog` is the one-page index of "what a port must reimplement": per table, the
  consuming engine, the `MethodCode` families its rows use, where it was introduced.

## Rule tables

`Live?` is one of three states, audited against actual repository/calculator code (not
inferred from naming) — see [decision 003](../../decisions/003-rules-audit-content-model-ephemeris-interpreter.md)
Part A:

- **live** — a calculator reads the table at runtime; the rule row actually drives chart output.
- **mirror** — seeded and cross-checked by a CLI `verify-*`/`show-rules` check against the
  hardcoded C#, but the runtime calculator still reads its own hardcoded dictionary, not the
  table.
- **orphaned** — table exists (seeded or unseeded), zero C# reads it, no CLI check either.

| Table | Rows | Rule captured | Live? |
|---|---|---|---|
| `tbl_Rule_VargaScheme` | 20 | per varga: `DivisionFactor`, `MethodCode`, `SignRuleKind`, `SignRuleKey` | live — orchestrator builds one `VargaCalculator` per row |
| `tbl_Rule_AspectOffset` | 19 | graha dṛṣṭi house offsets | mirror |
| `tbl_Rule_CombustionOrb` | 6 | direct / retrograde combustion orbs per planet | mirror |
| `tbl_Rule_NaturalRelationship` | 42 | Naisargika Maitrī friend/neutral/enemy grid | mirror |
| `tbl_Rule_TemporaryFriendshipDistance` | 12 | Tatkālika Maitrī sign-distance rule | mirror |
| `tbl_Rule_AgeState` | — | Bālādi degree bands + effect fraction | live (`AgeStateCalculator`) |
| `tbl_Rule_WakefulnessState` | — | Jāgradādi dignity → waking-state map | live (`WakefulnessStateCalculator`) |
| `tbl_Rule_PostureStateFormula` | 1 | Sayanaadi activity-index formula (`(C×P×A + M + G + L) mod 12`), cross-checked against the JHora Ramakrishnan export | mirror — CLI `verify-avastha` reproduces the JHora export's Activity table exactly (all 9 grahas); `PostureStateCalculator` is 100% hardcoded, zero DB reads |
| `tbl_Rule_GrahaDignity` | — | PVR Table 6 dignity segments + special degrees | mirror — CLI `verify-dignity` cross-checks seeded segments; `PvrDignityEvaluator` itself is 100% hardcoded, zero DB reads |
| `tbl_Rule_CompoundRelationship` | — | Pañchadhā Maitrī compound tiers | mirror — same `verify-dignity` check; consumed by `PvrDignityEvaluator`'s hardcoded `CompoundRelationshipCode`, not the table |
| `tbl_Rule_GrahaAttribute` / `tbl_Dim_GrahaAttribute` | — | normalized graha character grid | seeded |
| `tbl_Rule_DigBala` | — | directional-strength reference points | seeded |
| `tbl_Rule_ShadbalaComponent` / `tbl_Rule_BhavaBalaComponent` | — | PVR-first strength formula profile + provenance (mig. 072 sets `RuleParametersJson` on the six deferred Kālabala sub-components + corrects the Varṣa cap) | orphaned for Varṣa/Māsa/Āyana Bala (still uncomputed) — Dina/Hora/Tribhāga Bala are now computed by `ShadbalaCalculator` (100% hardcoded, zero DB reads) and checked by CLI `verify-strength` (self-consistency against the JHora export's own sunrise/sunset/birth-time data, not a `RuleParametersJson` round-trip) |
| `tbl_Rule_ShadbalaMinimumRupas` | 7 | per-planet minimum required Ṣaḍbala (rūpas) → `PercentOfMinimum`; reproduces JHora %Strength | seeded (mig. 071); `vw_ChartShadbala` computes `PercentOfMinimum`/`PercentOfMaximum` live via `JOIN` as of `db/100` — see `MASTER.md`'s `FEAT-STRENGTH-01` entry |
| `tbl_Rule_PlanetaryWar` | 1 | Graha Yuddha orb + winner criterion + Ṣaḍbala adjustment (Yuddha Bala) | orphaned for the magnitude (diameter-based delta — `SRC_RAMAN_GRAHA_BHAVA_BALAS` DJVU has no text extract) — detection + the latitude winner criterion are computed by `ShadbalaCalculator.ComputeYuddha` (100% hardcoded) and checked by CLI `verify-strength` (no war for Ramakrishnan; a synthetic case for the winner logic) |
| `tbl_Rule_AshtakavargaContribution` | 56 | Parāśari benefic-places (bindu) matrix — 7 recipients × 8 contributors, SAV total 337 (BPHS; Moon/Venus carry the Parāśari corrections JHora uses — mig. 078) | seeded (mig. 074/075/078); mirrored by `AshtakavargaTables`, checked by `verify-ashtakavarga` |
| `tbl_Rule_AshtakavargaReduction` | 3 | Ṭrikoṇa + Ekādhipatya Śodhana + Sodhya-Piṇḍa (rāśimāna / grahamāna) for the Piṇḍa pipeline | seeded (mig. 074/075/077); mirrored by `AshtakavargaTables` |
| `tbl_Rule_AmsabalaGroup` | 39 | PVR §6.6: which varga chart types belong to each amsabala scheme (Shadvarga/Saptavarga/Dasavarga/Shodasavarga) | live — `AmsabalaCalculator` (seeded mig. 099) |
| `tbl_Rule_AmsabalaName` | 35 | PVR §6.6: named amsa (Kimsukamsa..Sree Vallabhamsa) per good-placement count within a scheme | live — `AmsabalaCalculator`; verified against PVR's own Example 27 (Bill Cosby/Jupiter) in `AmsabalaCalculatorTests` |
| `tbl_Rule_VimsopakaWeight` | reserved | four varga-group weights, summing to 20 | unseeded — §6.6 amsabala reconciliation (above) was the blocking prerequisite and is now done, but PVR itself never gives Vimsopaka's numeric per-varga weight table (only names the concept, pp.188-189); still needs a different source |
| `tbl_Rule_SubPlanetSunLongitude` / `SubPlanetTime` / `SubPlanetPartRuler` | — | 11 upagraha longitude/time-point rules (PVR: Gulika = midpoint, Maandi = start) | live (`SubPlanetCalculator`) |
| `tbl_Rule_SpecialLagnaFraction` / `SpecialLagnaTimeRate` | — | HL/BL/GL rate-per-clock-minute (`SpecialLagnaTimeRate`) and SL's nakshatra-fraction method (`SpecialLagnaFraction`) | mirror — CLI `verify-jaimini` reproduces the JHora export exactly (BL/GL/SL D1+D9 signs and longitudes); `HoraLagnaCalculator`/`BhaavaLagnaCalculator`/`GhatiLagnaCalculator`/`SreeLagnaCalculator` are 100% hardcoded, zero DB reads |
| `tbl_Rule_Ayanamsa` | 22 | JHora ayanāṁśa catalogue + system default | live (`AyanamsaDefinition`) |
| `tbl_Rule_Yoga` / `tbl_Rule_YogaChartApplicability` / `tbl_Rule_YogaContextRequirement` | — | source-attributed Raman 1–300 + PVR yoga corpus, D1/D9 requirements, sex/day-night/phase/exact-longitude context | in progress |
| `tbl_Rule_Karaka` | 8 (Chara) | Chara Kāraka rank order (`KarakaScheme='Chara'`, `OrderIndex`/`TargetValue`/`ReverseForRahu`), SRC_PVR_INTEGRATED §8.2 Table 13, verified against the raw extract; Sthira scheme still reserved/empty (still hardcoded in `LifeAreaMap`) — Naisargika is normalized separately, see `tbl_Rule_KarakaMatter` below | mirror — seeded by migration 085 (closing the 2026-09-11 rule-mapping audit's "no DB citation at all" gap); CLI `verify-jaimini` cross-checks the order against `CharaKarakaCalculator`'s hardcoded enum; `CharaKarakaCalculator` itself stays 100% hardcoded, zero DB reads |
| `tbl_Dim_KarakaRole` | 17 (9 Naisargika + 8 Chara) | Shared karaka-role catalogue: one row per Naisargika graha, one per Chara role (driven from `tbl_Rule_Karaka`'s 8 Chara rows). Sthira reserved/unseeded. | seeded (migration 103); read by `NaisargikaKarakaRepository` via `vw_Rule_PrimaryNaisargikaKaraka`/`vw_Rule_NaisargikaKarakatwa` |
| `tbl_Dim_LifeMatter` | 128 (32 Karakatwa grid + 96 life-matter set) | Life-matter vocabulary: a stable `Code` join target replacing free-text matter names, kept as two separate vocabularies (no cross-linking between the grid's 32 and the specific-matter set's 96) | seeded (migration 103) |
| `tbl_Rule_KarakaMatter` | 149 (12 `IsPrimary`) | Canonical karaka-role↔life-matter bridge (`docs/database/karakafix.md`): replaces `tbl_Rule_Naisargika_Karakas`/`Karakatwas` (dropped, migration 103) and the free-text `KarakaText` parsing in `tbl_Rule_LifeMatterReference` (which keeps its legacy columns, now also linked via a new `LifeMatterId` FK). Compound karakas (e.g. Mars+Rahu) are separate rows sharing one `LifeMatterId`. | mirror — read by `NaisargikaKarakaRepository` via the two compatibility views; no calculator reads it live yet (same status the superseded tables had) |
| `tbl_Rule_LifeMatterClaim` / `tbl_Rule_LifeMatterClaimScope` | 35 initial claims / 38 scopes | Source-attributed PVR statements retained with quote/paraphrase form, claim type, source locator and verification state; one claim can apply to several life matters or a divisional subject. Migration 167 seeds the 34 ch. 8 karakatwa rows plus “Saturn is the significator of livelihood and karma.” | built; the Saturn statement remains `LOCATOR_PENDING` until its exact PVR publication/page is verified |
| `tbl_Rule_HouseSignification` / `tbl_Rule_HouseReferenceMatter` / `tbl_Rule_HouseAttribute` | — | house reference rules | seeded |
| `tbl_Rule_SignNakshatra` | 243 (27 Nakshatras × 9 KP sub-divisions) | KP levels 1-2 (Nakshatra-lord + Sub-lord) per sub-division, migration 096 | seeded; `vw_Rule_SignNakshatraRelationship` (mig. 097) reads it for each row's own primary Rasi, natural-relationship only; `tvf_Chart_SignNakshatraRasiRelationship(@ChartResultId)` (mig. 105) extends it to all 12 Rasis with the full chart-specific 5-tier compound relationship — see `schema.md` "Views & functions" |
| `tbl_Rule_RasiNakshatraCombination` | 36 (the spatially valid Rasi×Nakshatra pairs, not the full 12×27 grid) | Degree span (derived from `tbl_NakshatraPadas`); computed `LordRelation`/`AspectingRasis` columns (via `tbl_Rule_NaturalRelationship`/`tbl_Rule_RasiDrishti`); plus project-synthesis `CombinedCharacter`/`MainSignifications`/`PotentialBenefits`/`PotentialDisadvantages`/`JudgmentNote` (`SRC_IKIASTRRO_SYNTHESIS`), transcribed from `docs/research/domain/rasi-nakshatra-36-combination-matrix.md` (migration 124) | seeded; no calculator or Web/CLI consumer yet |
| `vw_Rule_NakshatraPadaLordConnection` | 108 (one per `tbl_NakshatraPadas` row) | Read view, pada-grain sibling of `vw_Rule_SignNakshatraRelationship` (097): Rasi lord vs. Nakshatra lord vs. Nakshatra **Pada** lord (D9, migration 123's `NakPadaLord` — distinct from 097's KP sub-lord), all 3 pairwise `tbl_Rule_NaturalRelationship` comparisons, and a `NaturalConnectionCode` notation string (e.g. `Ma-Ve-Ma`) built from the new `tbl_Planets.ShortCode` column (migration 125) | no new storage — pure computed view; **out of scope 2026-09-22 (rammyps) — Pada Lord chain rejected as "not usually done," left committed/unconsumed, no further work planned** |
| `tbl_Rule_NakshatraPadaCombination` | 108 (one per `tbl_NakshatraPadas` row) | Pada-grain "further analysis": `PadaCharacterModifier`/`PadaJudgmentNote`, generated by a documented SQL CASE template off `vw_Rule_NakshatraPadaLordConnection`'s 3-way lord relations (migration 126) — mechanical/reproducible, not freehand-written. `SRC_IKIASTRRO_SYNTHESIS`. `vw_Rule_NakshatraPadaAnalysis` joins this + `vw_Rule_NakshatraPadaLordConnection` + the parent 36-row `tbl_Rule_RasiNakshatraCombination` into one row per pada | seeded; **out of scope 2026-09-22 (rammyps) — same Pada Lord rejection, left committed/unconsumed** |
| `tbl_Rule_DashaApplicability` | reserved | source-attributed applicability conditions for conditional dasha systems | unseeded — table created by migration 46, zero rows, no source cited |
| `tbl_Rule_PanchangaFormula` | 4 | Tithi / Karana / Nitya Yoga / Hora Lord derivation formulas (PVR §1.3.8–1.3.11); cross-checked against the JHora Ramakrishnan export | mirror — CLI `verify-panchanga` reproduces the JHora export exactly; `PanchangaCalculator` is 100% hardcoded, zero DB reads |
| `tbl_Rule_SourceReferenceAshtakavargaMethod` / `…Contributor` / `…Reduction` | — | `research.*` schema: per-source Ashtakavarga method identity + contributor/reduction rule rows, pending verification against a cited edition before promotion to the production `tbl_Rule_Ashtakavarga*` tables above | orphaned by design — research staging area, not read by any calculator |
| `tbl_Dim_DivisionalSubject` | 11 | Which subject (career, marriage, …) each varga primarily confirms, and what D1 already establishes vs. what the varga adds | seeded (migration 38); its House/Planet/Varga facts are now normalized by `tbl_Rule_InterpretiveFactorDetail` (migration 109) — see below |
| `tbl_Dim_InterpretationDimension` | — | The taxonomy of interpretation axes (strength, timing, …) a synthesis layer would classify findings under | seeded (migration 38); not read by any calculator yet — same synthesis-layer backlog as `DivisionalSubject` |
| `tbl_Dim_InterpretiveFactor` | 4 | Catalogue of factor types (`HOUSE`/`PLANET`/`SIGN_LAGNA`/`VARGA`) that `tbl_Rule_InterpretiveFactorDetail` rows are typed against | seeded (migration 109), no `RuleSetId` (pure catalogue, same as `DivisionalSubject`/`InterpretationDimension`) |
| `tbl_Rule_InterpretiveFactorDetail` | 41 | Normalizes House/Planet/Varga facts for `tbl_Dim_LifeArea`, `tbl_Dim_DivisionalSubject` and `CHARA`-typed `tbl_Dim_KarakaRole` rows into queryable rows instead of free text (e.g. `DivisionalSubject.D1Foundation`'s prose) — three nullable typed FKs (`LifeAreaId`/`DivisionalSubjectCode`/`KarakaRoleId`), exactly one populated, same discriminated-by-null shape `tbl_Dim_KarakaRole` uses for `FixedGrahaId`/`CharaKarakaCode` | seeded for all 11 `DivisionalSubject` rows (migration 109); LifeArea and Chara-karaka-role details are a follow-up migration — CLI `verify-interpretive-factors` |
| `tbl_Dim_DashaLevel` | 3 | Labels `tbl_Chart_DashaPeriods.LevelNumber` (1/2/3) as `L1_MAHA`/`L2_ANTAR`/`L3_PRAT` | seeded (migration 110), no `RuleSetId` |
| `tbl_Rule_PlanetInHouse` | 108 (9 grahas × 12 houses, whole-sign) | Source-attributed planet-in-house interpretations, B.V. Raman *How to Judge a Horoscope* (`SRC_RAMAN_HTJH`) — distinct from house-lord placement (`tbl_Rule_HouseLordPlacement`, migration 093) and correcting the wrongly-cited `SRC_BVRAMAN_PLANET_IN_HOUSE` placeholder rows from migrations 064/065 | promoted from `research.*PlanetInHouseClaim` (migration 113 schema, 114 seed, 115 promotion); read by `PlanetInHouseRepository` via `vw_ChartPlanetInHouseInterpretation`, which also cross-references each placed graha's dispositor (sign lord) and that dispositor's own house/sign/dignity (migration 116) — CLI `verify-planet-in-house` |
| `tbl_Rule_YogaValidationDefinition` | 1,002 | An imported external yoga-expression corpus (`ExpressionLanguage`/`ExpressionText`, `ValidationSystemId`) for cross-checking `ProductionYogaEngine`'s own evaluators | **reproducibility gap, not a Live? question** — `dbo.SchemaMigrations` records `054_add_yoga_validation_tables.sql` as applied, but no file by that name exists under `db/` (only `054_set_lahiri_ayanamsa_default.sql` does — the pre-existing duplicate-054 numbering). A from-scratch `db/ikiastrro.sql` build cannot currently reproduce this table's 1,002 rows. Needs either recovering/recreating that script or removing the stale `SchemaMigrations` row if the table is to be re-seeded fresh. **Not addressed by migration 085** — deliberately out of scope (see that migration's header). |
| `tbl_Rule_VimshottariPeriod` | 9 | Vimshottari Dasha's core 9-planet order + 120-year split, SRC_PVR_INTEGRATED §16.2 Table 38, verified against the raw extract | mirror — added + seeded by migration 085; CLI `verify-dasha` cross-checks `SequenceOrder`/`YearsInCycle` against `AstroMath.NakshatraLordOrder`/`VimshottariYearsByLord` (also the KP-2 sub-lord division's source); `VimshottariDashaCalculator` itself stays 100% hardcoded, zero DB reads |
| `tbl_Rule_ArudhaFormula` | 1 | Arudha pada counting rule (house → lord's sign → pada, 1st/7th → 10th exception), SRC_PVR_INTEGRATED §9.2, verified against the raw extract | mirror — added + seeded by migration 085; CLI `verify-jaimini` asserts the row exists and cites the right source (a narrative row, same shape as `tbl_Rule_PostureStateFormula`/`PanchangaFormula` — nothing to numerically round-trip); `ArudhaCalculator` itself stays 100% hardcoded, zero DB reads |

The Naisargika/Chara half of this normalization (shared life-matter vocabulary, `tbl_Rule_KarakaMatter`
bridge) is implemented — migration 103, see [`karakafix.md`](karakafix.md)'s implementation record.
Sthira roles and applying friendship only during chart evaluation (`tvf_ChartKarakaCondition`)
remain open, same doc.

## Formulas computed in C#/SQL with no `tbl_Rule_*` citation at all

Distinct from the "orphaned" tables above (which exist, cite a source, but aren't read at
runtime) — these are delivered, verified data points where the classical convention itself
has **no row anywhere** in the star schema, found by cross-referencing every calculator under
`Engines/` against `tbl_Rule_Catalog` (2026-09-11 audit). **Three of the four below are now
closed** (migration 085 + CLI checks, same day) — kept here as a record of what was found and
how each was closed, not as an open list:

- ~~**Vimshottari Dasha's core table**~~ **Closed.** The 9-planet order (Ketu→Venus→Sun→Moon→
  Mars→Rahu→Jupiter→Saturn→Mercury) and 120-year split, hardcoded in `AstroMath.NakshatraLordOrder`
  / `VimshottariYearsByLord` (also the KP-2 sub-lord division's source), now cite
  `tbl_Rule_VimshottariPeriod` (migration 085, SRC_PVR_INTEGRATED §16.2 Table 38 — verified
  against the raw extract: years match exactly, total 120). CLI `verify-dasha` cross-checks the
  hardcoded order/years against the table; the calculators stay hardcoded (verified-mirror
  pattern). `tbl_Rule_DashaApplicability` (conditional-dasha eligibility) is a separate, still-
  unseeded concern.
- ~~**Chara Karaka (Aṣṭa) assignment**~~ **Closed.** `tbl_Rule_Karaka` — reserved and empty since
  migration 18, schema-shaped for exactly this rule — now carries the 8-row rank order
  (migration 085, SRC_PVR_INTEGRATED §8.2 Table 13 — verified against the raw extract, including
  the Rahu sign-end-measurement convention). CLI `verify-jaimini` cross-checks it against
  `CharaKarakaCalculator`'s hardcoded enum order.
- ~~**Arudha Pada counting rule**~~ **Closed.** New `tbl_Rule_ArudhaFormula` (migration 085,
  SRC_PVR_INTEGRATED §9.2 — verified against the raw extract), single-narrative shape like
  `tbl_Rule_PostureStateFormula`/`PanchangaFormula`. CLI `verify-jaimini` asserts the row exists
  and cites the right source.
- **Sade Sati / Kantaka / Ashtama Śani** — still open. Computed by a T-SQL function,
  `dbo.tvf_Chart_SadeSatiPeriods`, not a `tbl_Rule_*` table at all: the sign-offset rule
  (natal-Moon-sign ± the Dhaiya/Kantaka/Ashtama offsets) is inlined directly in the function
  body. It's "in the database" in the sense that matters for portability (a non-C# port must
  still reimplement it, same as a hardcoded C# formula would), but carries no `SourceRefCode`
  and isn't in `tbl_Rule_Catalog`'s scope (which only tracks `tbl_Rule_*` tables).
- **`LagnaFunctionalNature`** is the one deliberate exception, not a gap: a `tbl_Dim_
  LagnaFunctionalNature` mirror table existed (migration 031) and was **intentionally dropped**
  (see `db/00_drop_lagna_functional_nature.sql`) in favor of the computed classifier alone. Its
  source (`SRC_RAMAN_HTJH`, B.V. Raman's *How to Judge a Horoscope*) is still registered in
  `tbl_Dim_Source`, just not cited by any rule row — a decision already made, not an oversight.

**Also found — a duplication-drift risk, not a missing-row gap. Closed same day.** The classical
exaltation degrees (Sun 10° Aries, Moon 3° Taurus, …) were hardcoded four separate times —
`DignityEngine.ExaltationSign`, `ShadbalaCalculator.DeepExaltation`,
`RamanYogaBatchFiveEvaluator.DeepExaltation`, and an unnamed local dictionary inside
`RamanDhanaYogaEvaluator.DeeplyExalted` (found by grepping for the magic numbers themselves,
since it had no shared field name to search for). No new table was needed: `tbl_Rule_GrahaDignity`
already carries the exaltation degree (`DeepDegree` WHERE `DignityTypeCode='EXALTED'`), so the
fix was a pure C# consolidation onto one shared constant, `AstroMath.DeepExaltationPoints`, that
the other three now read instead of hardcoding their own copy. CLI `verify-dignity` gained a
direct cross-check of that constant against `tbl_SignAttributes` (itself already cross-checked
against `tbl_Rule_GrahaDignity`'s RuleSetId 2 rows). Note the pre-existing ruleset-wiring
oddity this surfaced: `tbl_Rule_GrahaDignity` is seeded only under RuleSetId 2/3, not RuleSetId 1
(the nominal "global active set") — out of scope to fix here, just worth knowing before anyone
tries to make a calculator read `tbl_Rule_GrahaDignity` directly under RuleSetId 1.

None of these block anything the JHora-parity checklist (`../cli/gap-and-coverage.md`)
cares about — they're all CLI-verified against the JHora export or a book's worked example
already. This section tracks a different axis: **portability** (`tbl_Rule_Catalog`'s own stated
purpose, "what a port must reimplement") and **provenance** (every seeded classical fact citing
a `SRC_*` code) for data points the engine already produces correctly.

## Divisional-chart portability

`RuleParametersJson` on `tbl_Rule_VargaScheme` carries a `"method"` key so a non-C# port
copies the table and reimplements **three** interpreters, not 20 rule classes:

- `LINEAR_VARGA` — `{factor, stride}` (D3, D4, D12, D60)
- `GRID_VARGA` — a `parts × 12` sampled sign grid (the bespoke `Special` rules)
- `BAND_VARGA` — `{edges, map}` (D30 unequal 5-part)

`verify-rules` proves every `RuleParametersJson` round-trips to its C# rule's output over the
full 360°.

## Facts

`tbl_Fact_*` rows record which `RuleSetId` produced them, so a chart's evidence is traceable
to the exact rule version. Written by the `*Computer` classes inside
`ChartGenerationService.PersistAnalytics`.

`tbl_Fact_KpSubLordChain` (migration 095, `RuleSetId` added migration 130) — levels 2-7 of
`AstroMath.GetKpSubLordChain` (level 1 stays on `tbl_Chart_KeyDetails.NakshatraSubLordPlanetId`),
D1 only. Has a repository (`KpSubLordChainRepository`), wired into `PersistAnalytics`'s D1 block
as an **optional** constructor dependency (default `null`). **Both workstream gaps now closed**:
`Ikiastrro.Cli/Program.cs`'s composition root has passed a real `KpSubLordChainRepository`
instance since 2026-09-18; **`Ikiastrro.Web/Program.cs` registered it 2026-09-22**
(`builder.Services.AddScoped<KpSubLordChainRepository>();`) — .NET DI resolves the same optional
constructor parameter on both `ChartGenerationService` and `BirthDetailDeletionService`
automatically once the type is registered, no call-site change needed. Charts generated through
the Web UI now populate this table too, not just CLI-driven generation.
Verified correct end-to-end via a throwaway harness (9 planets × 6 levels, byte-for-byte match
against `AstroMath` computed independently) before the repository was wired in.

`tbl_Fact_NakshatraLordDistribution` / `tbl_Fact_KpSubLordChainDistribution` (migration 117) —
on-demand statistical rollups (TRUNCATE + reinsert snapshot, not per-chart facts): how many D1
graha placements across every currently-generated chart resolve to each planet as Nakshatra
Lord, and the same breakdown per KP sub-lord-chain level (L1 from `tbl_Chart_KeyDetails`, L2-7
from `tbl_Fact_KpSubLordChain`). Zero-count
planet/level combinations are included, not omitted, so a stacked-bar-chart consumer never
silently drops a category. Refreshed via `dbo.usp_RefreshNakshatraKpStatDistributions`; 9 and
63 rows respectively as of this pass (9 planets, 9 planets × 7 levels).

`tbl_Fact_HouseFromReference` (migration 32) remains schema-only — out of scope this round (the
"4. KARAKAS" UI page's house numbers are a live house-from-Lagna calculation in
`KarakaPolarWheel.razor`/`PolarGridLagnaSelect`, unrelated to this table).
