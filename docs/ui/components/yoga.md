---
last_updated: 2026-09-27
reflects: variant-aware yoga UI with expandable LifeMatter 7x7 paths through migrations 148-149
workstream: ui
component: YogaEvaluationTable
route: /key-inference/{id} — step 5 · Yogas
togaf: C — component spec
---

# Component — yogas

**Step 5 · Yogas** of the [Key Inference](key-inference.md) flow — built 2026-09-16/17 as a
single `YogaEvaluationTable` component, a materially smaller shape than the round-2 plan below
(§ As-built): no coverage donut, no visible per-source-variant row list, no Source-as-column-1
layout. Mostly read-only: it renders persisted yoga-evaluation rows and adds no new *evaluation*
path ([`../../architecture/domain-contracts.md`](../../architecture/domain-contracts.md)) — the
one exception is the Interpretation column (2026-09-18), which is editorial content, not a
calculation, and is genuinely editable in the UI (see below).

## As-built (updated 2026-09-27)

Migration 148 and the current component replace concept-level deduplication with a
source-variant view. Every returned `SourceVariantCode` is a separate row, so numbered BVR
and PVR formations remain independently reviewable. The toolbar provides text search and a
Show all toggle; by default, present variants are emphasized while catalog/evaluation totals
remain visible in the header.

The table now renders Set, Yoga, Variant, Status, Rule, Interpretation, Life matters and
Source. Set comes from `YogaSetCode`/`YogaSetName`; Variant uses `VariantDisplayName` with
`SourceVariantCode` as its stable fallback. Status distinguishes Present, Evaluated,
Partial and Not evaluated rather than collapsing those states into one concept row.
Life matters expands into seven ranked paths with Area/SA1, SA2-SA6 and review status.

Interpretations resolve in this order: exact source variant, source-level override, then
generic concept text. Saves include `SourceVariantCode`, which prevents an interpretation
for one numbered Dhana/Daridra/Raja formation from leaking into another.

Migration 149 adds the related seven-path LifeMatter prioritization model. Each yoga variant
has seven ranked `tbl_Rule_YogaLifeMatterPath` rows, each directly referencing
`tbl_Rule_LifeMatterFocus`. `vw_YogaLifeMatter7x7` exposes Area and SA1-SA6; generated rows
start as `PROPOSED` and can be promoted to `REVIEWED` or `VERIFIED` after source review.
`YogaEvaluationRepository.GetLifeMatterPathsByBirthDetailId` joins the matrix to the chart's
evaluated variants and `KeyInference` passes the typed rows to the component.

### Coverage snapshot

- 411 active source variants across 235 concepts.
- 2,877 proposed yoga-to-LifeMatter paths: exactly seven for every active variant.
- 406 evaluated, 2 partial and 3 not evaluated variants at the implementation snapshot.
- Matrix scores are research/UI ordering signals, not claims of predictive validity.

## Original as-built state (2026-09-18)

One table, "Yogas Present": every `YogaCode` that matched, deduplicated (see below), Type +
Yoga + Rule + Interpretation + Source columns, sorted Type-first.

| Column | Source |
|---|---|
| Type | `YogaTypeCode` (`tbl_Rule_Yoga.FormationFamilyCode`, via `vw_ChartYogaEvaluations`) — **column 1**, not Yoga. Default sort order is Lagna → Sun → Moon → Combination (rammyps's directive), any other `YogaTypeCode` that shows up follows alphabetically after those four. |
| Yoga | `YogaCode`, title-cased and `YOGA_` stripped for display |
| Rule | `YogaRule` (`tbl_Rule_Yoga.ShortFormationRule`) — a one-line classical rule, now source-specific where a `YogaCode`'s sources genuinely disagree (`db/119`, e.g. `YOGA_BUDHA_ADITYA`: Raman needs Sun–Mercury >10°, PVR only needs same-sign). Two secondary lines under Rule when applicable: the chart's Lagna lord (`Lagna lord: <planet>`, only for `YogaTypeCode = LAGNA`, from `WorkspaceData.Charts["D1"].HouseLords`, no new query) and `Notes` (`vw_ChartYogaEvaluations.Notes` — the engine's per-chart "what matched" text, e.g. Budha Aditya's combustion caveat; only populated by `SourceAttributedYogaEngine`/`VerifiedSourceYogaEngine`/`PvrChapter11YogaEvaluator`/`PvrChapter11NumberedYogaEvaluator` today, blank for the other evaluators). |
| Interpretation | `tbl_Content_Interpretation` (`db/080`, source-override support `db/121`) via `InterpretationRepository`, resolved source-specific-with-generic-fallback. **Editable by any user** — an `EditIconButton` opens an inline Standard/Short text editor; Save is gated by the shared `ConfirmDialog` ("This will be visible to everyone using this tool."), then round-trips through `KeyInference.razor`'s `OnSaveInterpretation` callback to `InterpretationRepository.Upsert` (the table component itself injects no repository, keeping it presentational like every other chart component — the page owns the write, same as `SavedCharts.razor`'s edit flow). A local override dictionary shows the save immediately without waiting for the page to re-fetch. |
| Source | `SourceRefCode`, title-cased and `SRC_` stripped for display |

A header line above the table reads "*N* formed of *M* evaluated (*K* not yet evaluated)".

**Deduplication:** the same `YogaCode` is independently evaluated against several classical
source citations — e.g. `YOGA_DARIDRA` matches Raman's combinations #148/#149/#151/#152
separately, each its own row in `vw_ChartYogaEvaluations` sharing the same Name/Type/Rule. Rows
are grouped by `YogaCode` (`YogaEvaluationTable.ByYoga`); a yoga counts as **present** if *any*
of its source-citation rows is `Present`, and **evaluated** if *any* of them has
`EvaluationStatus = EVALUATED` — a non-matching citation can never hide one that did match, and
a not-yet-evaluated citation can never hide one that was already ruled present/absent. The
Rule/Notes/Interpretation shown are whichever source-citation row is picked as the group's
single representative (existing behaviour, unchanged by 2026-09-18's additions).

`SourceVariantCode`, `EvaluationStatus` detail, `SourceLocator`, `MissingRequirementCodesJson`,
`RuleSetId`, `ComputedAtUtc` stay in the underlying data but are not shown — the table only ever
displays one representative row per `YogaCode`.

Read by `YogaEvaluationRepository.GetByBirthDetailId` (typed — the generic
`AstrologerEvidenceRepository` dynamic-row query was the only prior reader of this view) and
`InterpretationRepository.GetBySubjectType`.

**Coverage:** `tbl_Rule_Yoga` (Type/Rule) is now populated for 230 of 223+ evaluated `YogaCode`s
— see `tbl_Rule_Catalog.Purpose` for the live count, not a number here (079 originally shipped
with a coverage claim that went stale within a day; `db/120`'s header comment has the history).
Only `YOGA_VIDYA`/`YOGA_ARISHTA` (`NOT_EVALUATED`, no predicate exists) still read `NULL` by
design. `tbl_Content_Interpretation` (Interpretation column) has no seed content beyond a
`YOGA_BUDHA_ADITYA` pilot pair (`db/122`, explicitly placeholder pending review) — every other
cell reads blank until an astrologer fills it in through the UI.

## Round-2 plan (2026-09-11, superseded by the above)

Split out of the old single evidence hub ([`evidence-tables.md`](evidence-tables.md) sections
11a–11d) into its own tab because the source-variant list runs to several hundred rows and
belongs beside neither the positional tables nor the daśā timeline. Called for a coverage-donut
summary table (Present/Absent/Not-evaluated counts, Raman/PVR variant counts) plus a full
per-`SourceVariantCode` row list with Source as column 1 and Variant dropped from view. Kept for
history; not what got built — see § As-built.

Originally seeded (079) for 146 of 223 evaluated `YogaCode`s; see § As-built above for the
current, corrected coverage (079's own "146/223, 61-tail, 14-unsupported" claim went stale
within a day — history in `db/120`'s header comment). A handful of `YogaCode`s cover more than
one classical form (`YOGA_DARIDRA`, `YOGA_DHANA`, `YOGA_CHAPA`, `YOGA_DEHASTHOULYA`, …); their
Rule text is a short summary of the family, not an exhaustive enumeration of every form — this
is also why they need the deduplication described above.

## Rendering

Plain HTML table (`YogaEvaluationTable.razor.css`'s `.yg-*` classes), shared brand tokens,
`.yg-card-head` matching every other inline chart's card-head bar. No horizontal scroll — five
columns, `Rule`/`Interpretation` wrap. The interpretation editor and its `ConfirmDialog` reuse
the shared `EditIconButton`/`ConfirmDialog` components (`Components/Shared`), same pattern as
`SavedCharts.razor`'s edit/delete flows, styled with their own `.yg-edit-*` classes.

## Verification

`tests/Ikiastrro.Web.Tests` — `YogaEvaluationTableTests` (bUnit): Notes/Lagna-lord render only
when present, Interpretation resolves source-specific-then-generic, and the edit → confirm →
save flow round-trips through `OnSaveInterpretation` without touching a repository (the
component takes no DB dependency, so this is a pure presentational test). The matrix test
verifies seven ranked paths bind only to their matching source variant and expose SA6. No regression in
`verify-*` (evaluation itself is still read-only). Counts reconciled against the yoga engine's
own CLI output.
