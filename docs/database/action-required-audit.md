---
last_updated: 2026-09-15
reflects: live ikiastrro dev database and current checkout at 2026-09-15
workstream: database
togaf: C — Data Architecture
---

# Database — action-required audit

Read-only sweep of `localhost\SQLSERVER2025 / ikiastrro`. The audit compared every user
table's live row count with its migration intent, current C# readers/writers, and the feature
register. It is an action register, not a claim that every empty table is defective.

Snapshot: 3 birth records, 66 chart results, 71 recorded migrations. Of 155 user tables,
38 `dbo`/`research` tables are empty (excluding the SQL Server diagram table). Empty tables
are grouped below by the kind of action they need.

## Priority actions

| Priority | Table(s) | Live evidence / cause | Required action | Completion evidence |
|---|---|---|---|---|
| P0 | `tbl_Dim_AyanamsaBenchmarkPositions`, `tbl_Dim_DashaBenchmarkPeriods` | `DBCC CHECKCONSTRAINTS` reports both tables orphaned: all child rows reference `AyanamsaBenchmarkCaseId = 1`, while the only parent case is now `Id = 2`. Both FKs are enabled but untrusted. | Add a forward, code-keyed repair migration that maps the 10 position and 9 period rows to `BENCH_RAMAKRISHNAN_P_JHORA_1981`, then re-check/trust both constraints. Fix the seed so it resolves the case ID by `Code`, never by assumed identity. | Zero DBCC violations; both FKs `is_not_trusted = 0`; the benchmark check resolves one case with 10 positions and 9 periods. |
| P0 | `tbl_Chart_Panchanga` | 0 rows although 3 saved people have D1 results. `PanchangaRepository.Insert` is now called for D1 by `ChartGenerationService.PersistAnalytics`; the live charts predate that wiring. | Regenerate or recompute all saved D1 charts using the current pipeline. Do not hand-insert derived values. | 3 rows (one per saved D1), `verify-panchanga` no longer skips its persisted-row check, and generation remains idempotent. |
| P0 | `tbl_Fact_PlanetAvastha` | 0 rows; current `tbl_Fact_PlanetaryState` has 567. This is the obsolete pre-migration-16 name, but it was recreated by the later-applied `00_add_avastha_star_schema.sql`. Views use the current table. | Add a forward migration that removes the obsolete table after checking all environments; also remove it from reset/deletion compatibility code. | Object absent, current avastha facts/views unchanged, schema verification green. |
| P1 | `tbl_Fact_HouseFromReference` | 0 rows. Migration 32 explicitly created a schema-only target; no repository or generation writer exists. The UI currently recalculates a small reference subset locally. | Implement one shared house-reference calculator + repository and persist it in chart generation/recompute; then make consumers read the persisted result. | Rows for the agreed reference/subject matrix, regeneration idempotent, reference cases verified. |
| P1 | `tbl_Fact_KpSubLordChain` | 0 rows. Migration 95 says schema only. `AstroMath.GetKpSubLordChain` already computes levels 1–7, but levels 2–7 are never persisted. | Add repository and generation/recompute wiring for levels 2–7; keep level 1 on `tbl_Chart_KeyDetails` as designed. | Expected 6 rows per persisted graha/point in scope, uniqueness holds, `verify-kp` covers persisted values. |
| P1 | `tbl_Fact_BhinnaAshtakavarga`, `tbl_Fact_BhinnaAshtakavargaContribution`, `tbl_Fact_SarvaAshtakavarga`, `tbl_Fact_AshtakavargaPinda` | All 0. Production schema/rules and a research benchmark exist, but this checkout has no `AshtakavargaCalculator` or writer. Some docs say the calculator is done on an unmerged CLI workstream, while `masterproduct.md` correctly reports Core/Verify/Web incomplete. | Reconcile/merge the CLI implementation or implement it here; wire all four facts into `GenerateAll` and recompute; run the benchmark before exposing the view. | BAV/SAV/Piṇḍa facts for all saved D1 charts, benchmark exact, `vw_ChartAshtakavarga` non-empty, docs agree. |
| P1 | `tbl_Content_Interpretation` | 0 rows by migration-80 design; copy authoring was deferred. | Define the first governed content slice, source/locale policy, and seed it in a new migration—or retire the table until a consumer exists. | Seeded content has a documented consumer and provenance, or the unused schema is removed. |

## Benchmark and validation harnesses

These result tables need an explicit runner, not chart-generation writes.

| Tables | Current state | Required action |
|---|---|---|
| `tbl_Fact_AyanamsaComparisonRuns`, `tbl_Fact_AyanamsaPositionComparisons`, `tbl_Fact_DashaBenchmarkComparisons` | 0 rows; benchmark case/position/period seeds exist (1/10/9). | Implement or finish the comparison runner, execute the seeded case, and persist a reproducible run. |
| `tbl_Fact_DashaApplicabilityResults` + `tbl_Rule_DashaApplicability` | Both 0. The fact is additionally blocked because the applicability rule table has no cited rows. | Source and seed conditional-dasha applicability first; then extend the comparison runner. Keep zero results until rules exist. |
| `tbl_Fact_YogaValidationRuns`, `tbl_Fact_YogaValidationResults` | 0 rows while validation definitions/mappings each have 1,002 rows. | Wire/execute the validation harness against a named saved or benchmark chart and retain the run provenance. Confirm migration 54 remains reproducible from the checked-in script. |

## Source-blocked rule tables

| Table | Current state | Required action |
|---|---|---|
| `tbl_Rule_VimsopakaWeight` | 0 rows. The registered PVR source names the technique but does not contain the numeric weight table. | Find and register an authoritative source, decide the supported scheme(s), then seed a new rule set and implement the consumer. Do not infer weights. |
| `tbl_Rule_DashaApplicability` | 0 rows and no source cited. | Research each conditional dasha's eligibility conditions, register sources, then seed versioned rules before enabling result persistence. |

## Research-schema scaffolds

The following empty `research` tables are not production failures. They are unfinished
normalisation scaffolds around otherwise-populated raw/source entities:

- Ashtakavarga: `tbl_Dim_SourceReferenceAshtakavarga`, `…Contribution`, `…Interpretation`,
  `…Reduction`, `…Text`, `tbl_Rule_SourceReferenceAshtakavargaContributor`, and
  `…Reduction`. `…BenchmarkDifference` is also empty; zero may be a valid successful
  comparison, but only if the benchmark runner explicitly records that outcome elsewhere.
- House: `tbl_Dim_SourceReferenceHouseAttribute`, `…Claim`, `…Crosswalk`, and
  `…HouseLordInHouseAttribute`.
- Nakshatra: `tbl_Dim_SourceReferenceNakshatraAttribute`, `…Claim`, and `…Crosswalk`.
- Planet: `tbl_Dim_SourceReferencePlanetAttribute`, `…Claim`, `…Crosswalk`, plus the
  corresponding three `…PlanetInHouse*` tables.

Action: create a source-corpus milestone that names which of these normalized layers will be
populated and consumed. For any scaffold with no planned query or transformation, remove it
through a forward migration rather than preserving indefinite empty schema. This decision is
lower priority than the production fact writers above.

## Empty but no product action

- `dbo.sysdiagrams` is SQL Server tooling metadata and may validly remain empty.
- A run/result fact table may return to zero after the documented transactional reset script;
  judge it together with its input rows and runner availability, not on row count alone.

## Documentation corrections exposed by the sweep

- `schema.md` still says the Panchanga calculator is pending, but calculator, repository, and
  generation wiring exist; only the live backfill is pending.
- `docs/database/MASTER.md` contains an older note that the ayanamsa benchmark case is empty;
  it now has 1 case with 10 positions and 9 dasha periods.
- Ashtakavarga status differs between the database workstream notes and the current checkout/
  feature register. Reconcile it when the CLI workstream is merged; until then the live facts
  and current source tree are authoritative.

## Repeat the sweep

Use a read-only `sys.tables` + `sys.partitions` row-count query, then trace every zero-row
table through `rg` across `db/`, `src/`, `masterproduct.md`, and `docs/`. Row counts alone do
not prove a defect: require a missing writer, stale prerequisite, source blocker, or explicit
deferred migration note before adding an action.
