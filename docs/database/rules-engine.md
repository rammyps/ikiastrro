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
| `tbl_Rule_PostureStateFormula` | 1 | Sayanaadi activity-index formula (`(C×P×A + M + G + L) mod 12`), cross-checked against the JHora Ramakrishnan export | seeded (mig. 083); calculator pending |
| `tbl_Rule_GrahaDignity` | — | PVR Table 6 dignity segments + special degrees | mirror — CLI `verify-dignity` cross-checks seeded segments; `PvrDignityEvaluator` itself is 100% hardcoded, zero DB reads |
| `tbl_Rule_CompoundRelationship` | — | Pañchadhā Maitrī compound tiers | mirror — same `verify-dignity` check; consumed by `PvrDignityEvaluator`'s hardcoded `CompoundRelationshipCode`, not the table |
| `tbl_Rule_GrahaAttribute` / `tbl_Dim_GrahaAttribute` | — | normalized graha character grid | seeded |
| `tbl_Rule_DigBala` | — | directional-strength reference points | seeded |
| `tbl_Rule_ShadbalaComponent` / `tbl_Rule_BhavaBalaComponent` | — | PVR-first strength formula profile + provenance (mig. 072 sets `RuleParametersJson` on the six deferred Kālabala sub-components + corrects the Varṣa cap) | orphaned — `ShadbalaCalculator`/`BhavaBalaCalculator` are 100% hardcoded, zero DB reads, no CLI check |
| `tbl_Rule_ShadbalaMinimumRupas` | 7 | per-planet minimum required Ṣaḍbala (rūpas) → `PercentOfMinimum`; reproduces JHora %Strength | seeded (mig. 071); read pending |
| `tbl_Rule_PlanetaryWar` | 1 | Graha Yuddha orb + winner criterion + Ṣaḍbala adjustment (Yuddha Bala) | seeded (mig. 072); calculator pending |
| `tbl_Rule_AshtakavargaContribution` | 56 | Parāśari benefic-places (bindu) matrix — 7 recipients × 8 contributors, SAV total 337 (BPHS, cross-checked vs MIT `jyotishganit`; hand-verified against the JHora export) | seeded (mig. 074/075); `AshtakavargaCalculator` pending |
| `tbl_Rule_AshtakavargaReduction` | 2 | Trikoṇa + Ekādhipatya Śodhana algorithms for the Sodhya Piṇḍa pipeline | seeded (mig. 074/075); read pending |
| `tbl_Rule_VimsopakaWeight` | reserved | four varga-group weights | unseeded |
| `tbl_Rule_SubPlanetSunLongitude` / `SubPlanetTime` / `SubPlanetPartRuler` | — | 11 upagraha longitude/time-point rules (PVR: Gulika = midpoint, Maandi = start) | live (`SubPlanetCalculator`) |
| `tbl_Rule_SpecialLagnaFraction` / `SpecialLagnaTimeRate` | — | HL and other special-lagna rates | orphaned — `HoraLagnaCalculator` is 100% hardcoded, zero DB reads, no CLI check |
| `tbl_Rule_Ayanamsa` | 22 | JHora ayanāṁśa catalogue + system default | live (`AyanamsaDefinition`) |
| `tbl_Rule_Yoga` / `tbl_Rule_YogaChartApplicability` / `tbl_Rule_YogaContextRequirement` | — | source-attributed Raman 1–300 + PVR yoga corpus, D1/D9 requirements, sex/day-night/phase/exact-longitude context | in progress |
| `tbl_Rule_Karaka` | reserved | chara/sthira/naisargika kāraka assignment schemes (Sthira/Naisargika still hardcoded in `LifeAreaMap`) | unseeded — reserved by migration 18 (P2); zero rows |
| `tbl_Rule_HouseSignification` / `tbl_Rule_HouseReferenceMatter` / `tbl_Rule_HouseAttribute` | — | house reference rules | seeded |
| `tbl_Rule_DashaApplicability` | reserved | source-attributed applicability conditions for conditional dasha systems | unseeded — table created by migration 46, zero rows, no source cited |
| `tbl_Rule_PanchangaFormula` | 4 | Tithi / Karana / Nitya Yoga / Hora Lord derivation formulas (PVR §1.3.8–1.3.11); cross-checked against the JHora Ramakrishnan export | seeded (mig. 081); calculator pending |

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
