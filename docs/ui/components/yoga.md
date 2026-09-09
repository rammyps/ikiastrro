---
last_updated: 2026-09-09
workstream: ui
component: Yogas
route: /key-inference#yogas
togaf: C — component spec
---

# Component — yogas

The **YOGAS** header on the [Key Inference](../wkstream_UI_v2.md#key-inference-4-headers) page.
Read-only: it renders persisted yoga-evaluation rows and adds no new evaluation path
([`../../architecture/domain-contracts.md`](../../architecture/domain-contracts.md)).

Split out of the old single evidence hub ([`evidence-tables.md`](evidence-tables.md) sections
11a–11d) into its own tab because the source-variant list runs to several hundred rows and
belongs beside neither the positional tables nor the daśā timeline.

## Tables

### 1. Yoga coverage

One row — the counts across every evaluated variant.

| Column | Source |
|---|---|
| Total · Present · Absent · Not evaluated | `vw_ChartYogaEvaluations` aggregate |
| Raman variants · PVR variants | `COUNT(DISTINCT SourceVariantCode)` per `SourceRefCode` |

`Not evaluated` is never `Absent` — a missing P0 input (sex, day/night, exact longitude,
lunar phase) yields `NOT_EVALUATED` by rule.

### 2. Yoga source variants

One row per `SourceVariantCode`.

| Column | From `vw_ChartYogaEvaluations` |
|---|---|
| Variant | `SourceVariantCode` |
| Yoga | `YogaCode` |
| Source | `SourceRefCode` (`SRC_RAMAN_300_COMBINATIONS`, `SRC_PVR_INTEGRATED`, …) |
| Result | `Present` → PRESENT / ABSENT / NOT_EVALUATED (pill: green / muted / amber) |
| Status | `EvaluationStatus` |
| Locator | `SourceLocator` (page / entry reference in the cited text) |

`MissingRequirementCodesJson`, `RuleSetId`, `ComputedAtUtc` are dropped from this view; the
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
