---
last_updated: 2026-09-11
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
| `tbl_Rule_ShadbalaMinimumRupas` | 7 | per-planet minimum required Ṣaḍbala (rūpas) → `PercentOfMinimum`; reproduces JHora %Strength | seeded (mig. 071); read pending — `PlanetaryStrengthRepository.InsertAll` still never populates `MinimumRequiredRupas` on freshly-computed rows |
| `tbl_Rule_PlanetaryWar` | 1 | Graha Yuddha orb + winner criterion + Ṣaḍbala adjustment (Yuddha Bala) | orphaned for the magnitude (diameter-based delta — `SRC_RAMAN_GRAHA_BHAVA_BALAS` DJVU has no text extract) — detection + the latitude winner criterion are computed by `ShadbalaCalculator.ComputeYuddha` (100% hardcoded) and checked by CLI `verify-strength` (no war for Ramakrishnan; a synthetic case for the winner logic) |
| `tbl_Rule_AshtakavargaContribution` | 56 | Parāśari benefic-places (bindu) matrix — 7 recipients × 8 contributors, SAV total 337 (BPHS, cross-checked vs MIT `jyotishganit`; hand-verified against the JHora export) | seeded (mig. 074/075); `AshtakavargaCalculator` pending |
| `tbl_Rule_AshtakavargaReduction` | 2 | Trikoṇa + Ekādhipatya Śodhana algorithms for the Sodhya Piṇḍa pipeline | seeded (mig. 074/075); read pending |
| `tbl_Rule_VimsopakaWeight` | reserved | four varga-group weights | unseeded |
| `tbl_Rule_SubPlanetSunLongitude` / `SubPlanetTime` / `SubPlanetPartRuler` | — | 11 upagraha longitude/time-point rules (PVR: Gulika = midpoint, Maandi = start) | live (`SubPlanetCalculator`) |
| `tbl_Rule_SpecialLagnaFraction` / `SpecialLagnaTimeRate` | — | HL/BL/GL rate-per-clock-minute (`SpecialLagnaTimeRate`) and SL's nakshatra-fraction method (`SpecialLagnaFraction`) | mirror — CLI `verify-jaimini` reproduces the JHora export exactly (BL/GL/SL D1+D9 signs and longitudes); `HoraLagnaCalculator`/`BhaavaLagnaCalculator`/`GhatiLagnaCalculator`/`SreeLagnaCalculator` are 100% hardcoded, zero DB reads |
| `tbl_Rule_Ayanamsa` | 22 | JHora ayanāṁśa catalogue + system default | live (`AyanamsaDefinition`) |
| `tbl_Rule_Yoga` / `tbl_Rule_YogaChartApplicability` / `tbl_Rule_YogaContextRequirement` | — | source-attributed Raman 1–300 + PVR yoga corpus, D1/D9 requirements, sex/day-night/phase/exact-longitude context | in progress |
| `tbl_Rule_Karaka` | reserved | chara/sthira/naisargika kāraka assignment schemes (Sthira/Naisargika still hardcoded in `LifeAreaMap`) | unseeded — reserved by migration 18 (P2); zero rows |
| `tbl_Rule_HouseSignification` / `tbl_Rule_HouseReferenceMatter` / `tbl_Rule_HouseAttribute` | — | house reference rules | seeded |
| `tbl_Rule_DashaApplicability` | reserved | source-attributed applicability conditions for conditional dasha systems | unseeded — table created by migration 46, zero rows, no source cited |
| `tbl_Rule_PanchangaFormula` | 4 | Tithi / Karana / Nitya Yoga / Hora Lord derivation formulas (PVR §1.3.8–1.3.11); cross-checked against the JHora Ramakrishnan export | mirror — CLI `verify-panchanga` reproduces the JHora export exactly; `PanchangaCalculator` is 100% hardcoded, zero DB reads |
| `tbl_Rule_SourceReferenceAshtakavargaMethod` / `…Contributor` / `…Reduction` | — | `research.*` schema: per-source Ashtakavarga method identity + contributor/reduction rule rows, pending verification against a cited edition before promotion to the production `tbl_Rule_Ashtakavarga*` tables above | orphaned by design — research staging area, not read by any calculator |
| `tbl_Dim_DivisionalSubject` | 12 | Which subject (career, marriage, …) each varga primarily confirms, and what D1 already establishes vs. what the varga adds | seeded (migration 38); not read by any calculator yet — reference data for a future synthesis-layer UI |
| `tbl_Dim_InterpretationDimension` | — | The taxonomy of interpretation axes (strength, timing, …) a synthesis layer would classify findings under | seeded (migration 38); not read by any calculator yet — same synthesis-layer backlog as `DivisionalSubject` |
| `tbl_Rule_YogaValidationDefinition` | 1,002 | An imported external yoga-expression corpus (`ExpressionLanguage`/`ExpressionText`, `ValidationSystemId`) for cross-checking `ProductionYogaEngine`'s own evaluators | **reproducibility gap, not a Live? question** — `dbo.SchemaMigrations` records `054_add_yoga_validation_tables.sql` as applied, but no file by that name exists under `db/` (only `054_set_lahiri_ayanamsa_default.sql` does — the pre-existing duplicate-054 numbering). A from-scratch `db/ikiastrro.sql` build cannot currently reproduce this table's 1,002 rows. Needs either recovering/recreating that script or removing the stale `SchemaMigrations` row if the table is to be re-seeded fresh. |

## Formulas computed in C#/SQL with no `tbl_Rule_*` citation at all

Distinct from the "orphaned" tables above (which exist, cite a source, but aren't read at
runtime) — these are delivered, verified data points where the classical convention itself
has **no row anywhere** in the star schema, found by cross-referencing every calculator under
`Engines/` against `tbl_Rule_Catalog` (2026-09-11 audit):

- **Vimshottari Dasha's core table** — the 9-planet order (Ketu→Venus→Sun→Moon→Mars→Rahu→
  Jupiter→Saturn→Mercury), the 120-year total, and the per-lord year count are hardcoded in
  `AstroMath.NakshatraLordOrder` / `VimshottariYearsByLord` — the single source of truth
  `VimshottariDashaCalculator` **and** the KP-2 sub-lord division (`AstroMath.GetNakshatraSubLord`,
  same proportional-division formula applied within one nakshatra) both depend on. Only the
  *conditional-dasha applicability* layer has a table (`tbl_Rule_DashaApplicability`, unseeded);
  the base Vimshottari cycle itself has none. `tbl_Dim_DashaSystems` is a vocabulary catalogue
  (which systems exist, implementation status) — not the year table.
- **Chara Karaka (Aṣṭa) assignment** — `CharaKarakaCalculator`'s rank-by-degree-in-sign order
  and the Rahu `30° − degree` reversal rule are hardcoded, zero DB row. `tbl_Rule_Karaka` exists
  and its schema (`KarakaScheme`, `OrderIndex`, `ReverseForRahu`) was clearly designed for
  exactly this rule (migration 18's own purpose text: "chara / sthira / naisargika karaka
  assignment schemes") — but it has been reserved and empty (0 rows) since migration 18.
- **Arudha Pada counting rule** — `ArudhaCalculator`'s house→lord→pada counting algorithm and
  its "pada lands on the house itself or its 7th → take the 10th from the pada" exception are
  hardcoded, zero DB row and no reserved table shape exists for it yet (unlike Chara Karaka).
- **Sade Sati / Kantaka / Ashtama Śani** — computed by a T-SQL function,
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

**Also found — a duplication-drift risk, not a missing-row gap**: the classical exaltation
degrees (Sun 10° Aries, Moon 3° Taurus, …) are hardcoded **three separate times** —
`DignityEngine.ExaltationSign`, `ShadbalaCalculator.DeepExaltation`,
`RamanYogaBatchFiveEvaluator.DeepExaltation` — with no shared C# constant and no
`tbl_Rule_Exaltation` table either. `tbl_Rule_GrahaDignity` already carries dignity segments
including exaltation, so consolidating onto one source (C# constant citing that table, or the
table itself once `tbl_Rule_Exaltation` exists) would remove the risk of the three drifting
apart if one is ever corrected without the others.

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
