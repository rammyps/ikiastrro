---
last_updated: 2026-09-11
workstream: ui
component: Yogas
route: /key-inference/{id} — step 6 · Yoga
togaf: C — component spec
---

# Component — yogas

**Step 6 · Yoga** of the [Key Inference](key-inference.md) flow (round-2 redesign,
2026-09-11 — was the standalone **YOGAS** header before the flow restructure). Read-only: it
renders persisted yoga-evaluation rows and adds no new evaluation path
([`../../architecture/domain-contracts.md`](../../architecture/domain-contracts.md)).

Split out of the old single evidence hub ([`evidence-tables.md`](evidence-tables.md) sections
11a–11d) into its own tab because the source-variant list runs to several hundred rows and
belongs beside neither the positional tables nor the daśā timeline.

## Tables

### 1. Yoga coverage

One row — the counts across every evaluated variant. Drives step 6's coverage **donut chart**
(Present / Absent / Not evaluated) as well as this summary line.

| Column | Source |
|---|---|
| Total · Present · Absent · Not evaluated | `vw_ChartYogaEvaluations` aggregate |
| Raman variants · PVR variants | `COUNT(DISTINCT SourceVariantCode)` per `SourceRefCode` |

`Not evaluated` is never `Absent` — a missing P0 input (sex, day/night, exact longitude,
lunar phase) yields `NOT_EVALUATED` by rule.

### 2. Yogas (round-2 columns — `Source` is column 1, `Variant` dropped)

One row per `SourceVariantCode` underneath, but `SourceVariantCode` is **no longer a visible
column** (still in the data — see below). Type and Rule are DB-backed as of `db/079`
([`db_view_catalog.md`](../../database/db_view_catalog.md#key-inference-page--step--source-planned-round-2));
full sourcing status in [`key-inference.md`](key-inference.md#new-fields--sourcing-status).

| Column | From `vw_ChartYogaEvaluations` (+ `tbl_Rule_Yoga`) |
|---|---|
| Source | `SourceRefCode` (`SRC_RAMAN_300_COMBINATIONS`, `SRC_PVR_INTEGRATED`, …) — **now column 1** |
| Yoga | `YogaCode` |
| Type *(new)* | `YogaTypeCode` — which reference point the yoga is judged from: `SUN` / `MOON` / `LAGNA` / `COMBINATION`. `tbl_Rule_Yoga.FormationFamilyCode`, one row per `YogaCode`, left-joined in. |
| Rule *(new)* | `YogaRule` — a one-line classical rule, e.g. *"Jupiter in kendra (1,4,7,10) from Moon"*. `tbl_Rule_Yoga.ShortFormationRule`, transcribed from the evaluator predicate in `src/Ikiastrro.Core/Engines/Yoga/*.cs`. |
| Result | `Present` → PRESENT / ABSENT / NOT_EVALUATED (pill: green / muted / amber) |

Seeded for **146 of the 223** evaluated `YogaCode`s — every value transcribed from the actual
coded predicate, never freehand recall. The other 77 read `NULL` for Type/Rule by design (no UI
placeholder needed beyond the existing empty-cell treatment): 61 are the uncoded Raman 201–300
tail, 14 are `RamanYogaBatchEightEvaluator`'s explicit unsupported set, and
`YOGA_VIDYA`/`YOGA_ARISHTA` are `NOT_EVALUATED`. A handful of `YogaCode`s cover more than one
classical form (`YOGA_DARIDRA`, `YOGA_DHANA`, `YOGA_CHAPA`, `YOGA_DEHASTHOULYA`, …); their Rule
text is a short summary of the family, not an exhaustive enumeration of every form.

`Variant` (`SourceVariantCode`), `Status` (`EvaluationStatus`), `Locator` (`SourceLocator`),
`MissingRequirementCodesJson`, `RuleSetId`, `ComputedAtUtc` stay in the underlying data (the
coverage-summary table below still counts by variant) but are not shown in the row table. The
NOT_EVALUATED reason expands from [`vw_YogaChartApplicability`](../../database/schema.md) /
`vw_YogaContextRequirements` on demand.

## Rendering

MudBlazor table, shared tokens, tabular numerals. `table-layout: auto` — columns follow
content; no horizontal scroll. The variant list is long: default filter is
**Present + Not evaluated** with an "all" toggle (deferred to the first build).

## Verification

`tests/Ikiastrro.Web.Tests` bUnit snapshot of both tables against a seeded person; no
regression in `verify-*` (UI is read-only). Counts reconciled against the yoga engine's own
CLI output.
