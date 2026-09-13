---
last_updated: 2026-09-10
status: research-baseline
---

# Yoga validation and comparison framework

## Purpose

Create a source-aware inventory of yoga calculations before expanding the production
evaluator. Maitreya 8.2 is the first external corpus. The model can later compare JHora,
Horoscope Explorer, PyJHora, manual calculations, and other astrology domains.

External calculations are validation evidence, not replacement production rules.

## Evidence boundaries

Keep four claims separate:

1. A named yoga appears in a source or product.
2. A source gives a particular formation rule.
3. An engine reports a result for a chart and settings.
4. A yoga is statistically associated with a defined outcome.

Program agreement validates implementation consistency only. It does not prove a
classical interpretation or empirical outcome.

## Maitreya 8.2 corpus

The tagged source under `_research/Maitreya8` contains 16 JSON collections:

| File | Stated corpus | Family |
|---|---|---|
| `10AscendantMantreswar.json` | Mantreswara | Ascendant combinations |
| `15NabhasaSaravali.json` | Saravali | Nabhasa yogas |
| `16LunarSaravali.json` | Saravali | Lunar yogas |
| `20HousesSaravali.json` | Saravali | House results |
| `21HousesMantreswar.json` | Mantreswara | House results |
| `25LordshipParasara.json` | Parasara | Lordship combinations |
| `30SunSaravali.json` | Saravali | Sun placements |
| `31MoonSaravali.json` | Saravali | Moon placements |
| `32MarsSaravali.json` | Saravali | Mars placements |
| `33MercurySaravali.json` | Saravali | Mercury placements |
| `34JupiterSaravali.json` | Saravali | Jupiter placements |
| `35VenusSaravali.json` | Saravali | Venus placements |
| `36SaturnSaravali.json` | Saravali | Saturn placements |
| `40UpagrahasParasara.json` | Parasara | Upagraha placements |
| `41NakshtrasBrihatJataka.json` | Brihat Jataka | Nakshatra results |
| `50ConjunctionsSaravali.json` | Saravali | Conjunctions |

Entries normally contain `description`, `effect`, `rule`, `highervargas`, `source`, and
`group`. The rule uses Maitreya's YogaExpert expression language. Preserve it as external
evidence; it is not a canonical Sanskrit citation or automatically equivalent to an
ikiastrro predicate.

For example, Maitreya's Saravali Kemadruma rule tests that Mars, Mercury, Jupiter, Venus,
and Saturn are absent from both adjacent signs to the Moon. The Sun and nodes are not
counted. Cancellation and qualification rules require separate source variants.

## Identity and mapping

Use three identities:

- `YogaCode`: canonical concept, such as `YOGA_KEMADRUMA`.
- `SourceVariantCode`: exact textual definition, such as `SARAVALI_KEMADRUMA_BASE`.
- `ExternalDefinitionCode`: encoded calculation, such as `MAITREYA82_16LUNAR_004`.

Mapping status: `EXACT`, `PARTIAL`, `BROADER`, `NARROWER`, `ALIAS_ONLY`, `UNMAPPED`, or
`NOT_COMPARABLE`. Names alone never establish equivalence.

## Proposed validation schema

Validation data is append-only. New software versions and settings create new rows.

### `tbl_Dim_ValidationSystems`

Producer (`IKIASTRRO`, `MAITREYA`, `HOROSCOPE_EXPLORER`, `JHORA`, `MANUAL`), version,
engine family, license, and provenance URI.

### `tbl_Dim_YogaValidationMappings`

Maps an external definition to `YogaCode` and `SourceVariantCode`, with mapping status,
review state, reviewer, rationale, and evidence locator.

### `tbl_Rule_YogaValidationDefinition`

Stores `RuleSetId`, validation system, external code/name, source and locator, group,
expression language, expression or repository locator, higher-varga support, definition
hash, and import timestamp. GPL expression text retains GPL provenance.

### `tbl_Fact_YogaValidationRuns`

One reproducible execution context: chart identity, product/rule version, ayanamsa,
zodiac, house system, node mode, ephemeris, time/location settings, input hash, timestamps,
and run status.

### `tbl_Fact_YogaValidationResults`

One definition result per run: `PRESENT`, `ABSENT`, `NOT_EVALUATED`, or `ERROR`, raw
result, evidence JSON, canonical/source mapping, duration, and evaluator version.

### `vw_YogaValidationComparison`

Align by chart, canonical yoga, source variant, and compatible settings. Status values:
`AGREE_PRESENT`, `AGREE_ABSENT`, `DISAGREE`, `IKIASTRRO_ONLY`, `EXTERNAL_ONLY`,
`NOT_COMPARABLE`, `MISSING_INPUT`, and `UNMAPPED`.

Never convert `NOT_EVALUATED` or a missing external rule into `ABSENT`.

## Import and comparison process

1. Inventory each Maitreya entry with file, ordinal, hash, description, group, source,
   higher-varga flag, and expression.
2. Preserve original text; normalize spellings only as aliases.
3. Auto-propose name mappings but require review before `EXACT`.
4. Run engines from the same input snapshot and compatible settings.
5. Persist raw results first and derive comparisons through a view.
6. Diagnose differences by reference point, bodies counted, sign/house basis, dignity,
   aspects, cancellation, varga, and missing context.
7. Promote findings only after classical-source verification and regression tests.

## Initial Horoscope Explorer queue

| Yoga | Initial state |
|---|---|
| Kaahala | Alias review required |
| Ruchaka | Classical variant required |
| Kemadruma | Maitreya Saravali definition available |
| Vipreet Raja | Umbrella name; split variants |
| Vidya | Definition required |
| Anivahuppu | Spelling and definition required |
| Arishta | Umbrella name; not one predicate |
| Anthya Vayasi Dhana | Definition required |
| Shankha | Classical variant required |
| Trilochana | Definition required |
| Daridra | Variant review required |
| RogaGrastha | Definition required |
| Sada Sanchara | Definition required |

## Statistical extension

Do not create one unexplained significance score. Store prevalence/rarity, formation
strength, activation, outcome and observation window, effect size with confidence
interval, sample size/missingness, multiple-testing correction, holdout replication,
model version, and feature provenance separately.

Group correlated yogas by shared predicates before modelling. Many names may encode the
same planetary configuration and otherwise create false independent signals.

## Reuse beyond yogas

The run/result layer can later support dignity, aspects, vargas, dashas, strengths,
nakshatras, transits, and house cusps. Keep domain-specific mappings so foreign keys remain
strong.

## Acceptance criteria

- Every definition has product version, locator, and hash.
- Every result is reproducible from an input/settings snapshot.
- Canonical yoga and source variant remain distinct.
- Missing inputs remain `NOT_EVALUATED`.
- Raw evidence is immutable; comparisons are derived.
- External logic needs source review and tests before production use.
- License and provenance survive exports and reports.
