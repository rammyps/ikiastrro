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
column** (still in the data — see below). Two columns are new and **not yet backed by a DB
field**; full sourcing status in [`key-inference.md`](key-inference.md#new-fields--sourcing-status).

| Column | From `vw_ChartYogaEvaluations` (+ `tbl_Rule_Yoga`) |
|---|---|
| Source | `SourceRefCode` (`SRC_RAMAN_300_COMBINATIONS`, `SRC_PVR_INTEGRATED`, …) — **now column 1** |
| Yoga | `YogaCode` |
| Type *(new)* | which reference point the yoga is judged from — Sun / Moon / Lagna / a combination. **Not a DB column** — needs a `FormationFamilyCode`/`RequirementJson` parser or a new column on `tbl_Rule_Yoga`. |
| Rule *(new)* | a one-line classical rule, e.g. *"Jupiter in kendra (1,4,7,10) from Moon"*. **Not a DB column** — `CalculationNarrative` is prose-length, not this short form; needs a new column or per-yoga authoring. |
| Result | `Present` → PRESENT / ABSENT / NOT_EVALUATED (pill: green / muted / amber) |

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
