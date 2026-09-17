---
last_updated: 2026-09-17
status: research-baseline
---

# Chara karaka interpretation rules — statistical validation framework

## Purpose

Design the rules table for chara-karaka *interpretations* — what AK/AmK/BK/MK/PiK/PK/GK/DK
mean once resolved to a planet for a chart (behavior, life-matter, varga/nakshatra/chakra
readings scoped in [res_charakarakas.md](res_charakarakas.md)) — using the same
source-aware, statistically-honest pattern already established for yogas in
[yoga-validation-framework.md](../yoga-validation-framework.md) and for Ashtakavarga in
`db/067`–`070`. Same discipline, new domain: multiple sources disagree on karaka
interpretation the same way they disagree on yoga formation, so the evidence layers stay
separate from the start rather than being merged into one "the meaning of AK is..." table.

## Evidence boundaries

Keep four claims separate, same as the yoga framework:

1. A named interpretation for a chara-karaka role appears in a source (PVR, or another
   named text/product).
2. A source gives a particular behavioral/life-matter rule for that role.
3. An engine (ikiastrro, JHora, Horoscope Explorer, PyJHora, Maitreya) reports a resolved
   planet and derived reading for a chart and settings.
4. A chara-karaka reading is statistically associated with a defined outcome.

Program agreement validates resolution-mechanics consistency (does everyone compute the
same planet for AK) only. It does not prove a classical interpretation or empirical
outcome — those are separate, weaker claims that need their own evidence.

## Identities

- `KarakaInterpretationCode`: canonical concept, e.g. `CHARA_AK_SELF_IDENTITY`,
  `CHARA_DK_SPOUSE_ROLE`.
- `SourceVariantCode`: exact textual definition, e.g. `PVR_CH8_TABLE13_AK`.
- `ExternalDefinitionCode`: encoded external-engine or corpus entry, e.g.
  `MAITREYA82_...` (once/if a chara-karaka corpus is identified — none is mapped yet;
  the yoga framework's Maitreya corpus in `_research/Maitreya8` does not cover chara
  karakas directly).

Mapping status: `EXACT`, `PARTIAL`, `BROADER`, `NARROWER`, `ALIAS_ONLY`, `UNMAPPED`, or
`NOT_COMPARABLE` — reused verbatim from the yoga framework. Names alone (e.g. two sources
both calling something "AK shows self") never establish equivalence without checking the
underlying rule.

## Proposed schema

Research-schema tables, additive to production, mirroring the yoga framework's Dim/Fact
split and the Ashtakavarga benchmark tables' case/run shape:

### `research.tbl_Dim_KarakaInterpretationDefinition`

`RuleSetId`, `KarakaRoleId` (FK to `tbl_Dim_KarakaRole`, migration 103 — reuses the 8
existing `CHARA`-typed rows rather than re-deriving them), `SourceVariantCode`, source and
page/locator, interpretation category (behavior / life-matter / varga-lord / nakshatra-pada
/ chakra-lord — the five layers from `res_charakarakas.md` §1–§4), free-text/expression
content, definition hash, import timestamp.

### `research.tbl_Dim_KarakaInterpretationBenchmarkCase`

Same shape as `tbl_Dim_SourceReferenceAshtakavargaBenchmarkCase` (`db/068`): a named chart
(subject label, birth datetime/location, ayanamsa, chart convention) used as a fixed test
case across sources.

### `research.tbl_Fact_KarakaInterpretationRun`

One reproducible resolution per case: which karaka role, which planet it resolved to,
product/rule version, ayanamsa/zodiac/house-system/node-mode settings, input hash,
timestamps, run status — parallel to `tbl_Fact_YogaValidationRuns`.

### `research.tbl_Fact_KarakaInterpretationResult`

One interpretation-definition result per run: resolved reading (raw text/expression
result), evidence JSON, canonical/source mapping, duration, evaluator version. Never
collapse "not evaluated for this case" into a negative/absent result — same rule the yoga
framework states explicitly, and it applies here too since not every source treats every
karaka role.

### `research.vw_KarakaInterpretationComparison`

Align by case, canonical `KarakaInterpretationCode`, source variant, and compatible
settings. Same status vocabulary as `vw_YogaValidationComparison`: `AGREE`, `DISAGREE`,
`IKIASTRRO_ONLY`, `EXTERNAL_ONLY`, `NOT_COMPARABLE`, `MISSING_INPUT`, `UNMAPPED`.

## Statistical extension

Do not create one unexplained "how accurate is AK" score. If/when outcome data exists,
store prevalence/rarity of each resolved-planet-per-role combination, activation
conditions (dasa/transit if that's ever layered in), outcome and observation window,
effect size with confidence interval, sample size/missingness, multiple-testing
correction, holdout replication, model version, and feature provenance as separate
columns/tables — not one composite number. Same reasoning as the yoga framework: chara
karakas are correlated with each other by construction (they're a ranking of the same 8
planets), so treat correlated roles as one modelling group rather than 8 independent
signals — e.g. AK and AmK conclusions from the same chart are not independent evidence.

## Reuse and precedent

- Karaka-role identity: reuse `tbl_Dim_KarakaRole` (migration 103) rather than a new
  role table — this framework only adds an interpretation layer on top of an already-FK'd
  role.
- Case/run/result shape: copied from the Ashtakavarga benchmark tables (`db/067`–`070`),
  which already prove this pattern works for a different PVR-sourced technique.
- Comparison view and status vocabulary: copied from `yoga-validation-framework.md`
  rather than inventing a second vocabulary for the same kind of claim.

## Open questions

- No external corpus is currently mapped for chara-karaka interpretation (unlike yogas,
  which have the Maitreya 8.2 corpus). Needs a source inventory pass before
  `ExternalDefinitionCode` has anything to point at — JHora/PyJHora's chara-karaka output
  (`_research/PyJHora/.../karaka.py`) is the most likely first candidate since it's already
  vendored in-repo.
- Interpretation category taxonomy (behavior / life-matter / varga-lord / nakshatra-pada /
  chakra-lord) is provisional, taken directly from `res_charakarakas.md`'s section
  headings. The chakra-lord category specifically inherits that file's open question of
  whether "chakra" is a named classical technique or project-original — this framework
  cannot validate an interpretation category against sources until that's answered.
- No production schema or data change is implied by this document, consistent with
  `karakafix.md`'s and `yoga-validation-framework.md`'s own scoping — this is design input
  for a future database-workstream migration.
