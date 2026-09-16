---
last_updated: 2026-09-17
workstream: ui
component: YogaEvaluationTable
route: /key-inference/{id} — step 5 · Yogas
togaf: C — component spec
---

# Component — yogas

**Step 5 · Yogas** of the [Key Inference](key-inference.md) flow — built 2026-09-16/17 as a
single `YogaEvaluationTable` component, a materially smaller shape than the round-2 plan below
(§ As-built): no coverage donut, no visible per-source-variant row list, no Source-as-column-1
layout. Read-only: it renders persisted yoga-evaluation rows and adds no new evaluation path
([`../../architecture/domain-contracts.md`](../../architecture/domain-contracts.md)).

## As-built (2026-09-16/17)

One table, "Yogas Present": every `YogaCode` that matched, deduplicated (see below), Type +
Yoga + Rule + Source columns, sorted Type-first.

| Column | From `vw_ChartYogaEvaluations` (+ `tbl_Rule_Yoga`) |
|---|---|
| Type | `YogaTypeCode` (`tbl_Rule_Yoga.FormationFamilyCode`) — **column 1**, not Yoga. Default sort order is Lagna → Sun → Moon → Combination (rammyps's directive), any other `YogaTypeCode` that shows up follows alphabetically after those four. |
| Yoga | `YogaCode`, title-cased and `YOGA_` stripped for display |
| Rule | `YogaRule` (`tbl_Rule_Yoga.ShortFormationRule`) — a one-line classical rule |
| Source | `SourceRefCode`, title-cased and `SRC_` stripped for display |

A header line above the table reads "*N* formed of *M* evaluated (*K* not yet evaluated)".

**Deduplication:** the same `YogaCode` is independently evaluated against several classical
source citations — e.g. `YOGA_DARIDRA` matches Raman's combinations #148/#149/#151/#152
separately, each its own row in `vw_ChartYogaEvaluations` sharing the same Name/Type/Rule. Rows
are grouped by `YogaCode` (`YogaEvaluationTable.ByYoga`); a yoga counts as **present** if *any*
of its source-citation rows is `Present`, and **evaluated** if *any* of them has
`EvaluationStatus = EVALUATED` — a non-matching citation can never hide one that did match, and
a not-yet-evaluated citation can never hide one that was already ruled present/absent.

`SourceVariantCode`, `EvaluationStatus` detail, `SourceLocator`, `MissingRequirementCodesJson`,
`RuleSetId`, `ComputedAtUtc` stay in the underlying data but are not shown — the table only ever
displays one representative row per `YogaCode`.

Read by `YogaEvaluationRepository.GetByBirthDetailId` (typed, added alongside this build — the
generic `AstrologerEvidenceRepository` dynamic-row query was the only prior reader of this view).

## Round-2 plan (2026-09-11, superseded by the above)

Split out of the old single evidence hub ([`evidence-tables.md`](evidence-tables.md) sections
11a–11d) into its own tab because the source-variant list runs to several hundred rows and
belongs beside neither the positional tables nor the daśā timeline. Called for a coverage-donut
summary table (Present/Absent/Not-evaluated counts, Raman/PVR variant counts) plus a full
per-`SourceVariantCode` row list with Source as column 1 and Variant dropped from view. Kept for
history; not what got built — see § As-built.

Seeded for **146 of the 223** evaluated `YogaCode`s — every Type/Rule value transcribed from the
actual coded predicate, never freehand recall. The other 77 read `NULL` for Type/Rule by design:
61 are the uncoded Raman 201–300 tail, 14 are `RamanYogaBatchEightEvaluator`'s explicit
unsupported set, and `YOGA_VIDYA`/`YOGA_ARISHTA` are `NOT_EVALUATED`. A handful of `YogaCode`s
cover more than one classical form (`YOGA_DARIDRA`, `YOGA_DHANA`, `YOGA_CHAPA`,
`YOGA_DEHASTHOULYA`, …); their Rule text is a short summary of the family, not an exhaustive
enumeration of every form — this is also why they need the deduplication described above.

## Rendering

Plain HTML table (`YogaEvaluationTable.razor.css`'s `.yg-*` classes), shared brand tokens,
`.yg-card-head` matching every other inline chart's card-head bar. No horizontal scroll — four
columns, `Rule` wraps.

## Verification

`tests/Ikiastrro.Web.Tests` bUnit snapshot against a seeded person; no regression in `verify-*`
(UI is read-only). Counts reconciled against the yoga engine's own CLI output.
