---
last_updated: 2026-09-15
reflects: planned generated-data reset and rebuild workflow; not yet implemented
workstream: database
togaf: C — Data Architecture
status: planned
---

# Generated-data rebuild launcher — implementation plan

## Outcome

Provide a safe, restartable PowerShell workflow that can clear and rebuild derived chart data
without duplicating calculation logic outside the application. PowerShell coordinates the
operation; `ChartGenerationService` remains the sole owner of calculation order and
persistence.

This document is a plan only. No launcher, CLI command, or database change has been made yet.

## Current constraint

`db/checks/Reset-AllTransactionalData.ps1` currently deletes `tbl_BirthDetails` together with
chart results and facts. Once those input rows are gone, the application has nothing from
which to rebuild charts. A normal rebuild must therefore preserve birth details. A true
everything reset must require a separate import or other authoritative input source.

## Proposed interface

### Reset modes

Extend `Reset-AllTransactionalData.ps1` with an explicit mode:

- `GeneratedOnly` — default. Preserve `tbl_BirthDetails`; delete chart results, analytics,
  dashas, and generated facts.
- `Everything` — delete birth details as well as generated data. Require the operator to type
  `DELETE EVERYTHING` and warn that a rebuild is impossible without an import/source file.

### Bulk CLI command

Add one application-owned entry point:

```powershell
dotnet run --project src/Ikiastrro.Cli -- rebuild-all
```

The command will:

1. Load every saved `tbl_BirthDetails` row.
2. Call `ChartGenerationService.GenerateAll(person)` once per person.
3. Print `current/total` progress.
4. Capture failures per person and continue unless `--fail-fast` is supplied.
5. Exit nonzero if any person failed.
6. Summarize succeeded, failed, and skipped people.

The durable work key is `BirthDetailId`; a person's name is display-only.

### PowerShell launcher

Create `db/checks/Rebuild-AllGeneratedData.ps1` with these proposed parameters:

```powershell
-RebuildMode Full|Missing|Analytics
-ResetGenerated
-Server
-Database
-Configuration Release
-FailFast
-SkipBuild
-SkipVerification
-LogDirectory
```

| Mode | Application operation | Intended use |
|---|---|---|
| `Full` | `GenerateAll` for every saved person | Normal post-reset rebuild and refresh after calculation changes |
| `Missing` | `GenerateMissing` | Fill absent chart types without refreshing existing facts |
| `Analytics` | `RecomputeAnalytics` | Refresh derived analytics while preserving chart-result headers and dashas |

`Full` is the default after a generated-data reset.

## Execution sequence

```text
Preflight
  → optional GeneratedOnly reset
  → build CLI once
  → rebuild each saved person
  → run verification commands
  → compare expected and live counts
  → write completion report
```

Do not invoke `dotnet run` separately for every person. Build once, then execute one bulk CLI
command so startup cost, configuration, and error handling stay consistent.

## Preflight gates

Before any deletion or rebuild, verify:

- `sqlcmd` and `dotnet` are available.
- The requested SQL Server and database exist.
- The target is the intended environment; non-dev targets require explicit confirmation.
- `tbl_BirthDetails` contains inputs, unless the selected operation is a dry run.
- Required migrations for the current executable are applied.
- The CLI builds successfully unless `-SkipBuild` is explicitly supplied.
- The benchmark foreign-key corruption recorded in `action-required-audit.md` is surfaced
  separately and not misreported as a chart-rebuild failure.

The launcher should print the resolved server, database, configuration, reset mode, person
count, and planned verification commands before requesting confirmation.

## Restartability and failure handling

`ChartGenerationService.GenerateAll` is delete-first and idempotent per person. If one person
fails midway, rerunning that person safely replaces their partial output. The bulk command
should isolate failures per person and retain enough information for a targeted retry.

Create one run directory, for example:

```text
artifacts/rebuild/2026-09-15_143000/
  rebuild.log
  results.csv
  before-counts.csv
  after-counts.csv
  failures.txt
```

`results.csv` should include `BirthDetailId`, name, start/end UTC, elapsed time, status,
chart count, dasha status, and error summary. Logs must not contain secrets or connection
credentials.

## Post-rebuild verification

Run the relevant CLI checks automatically:

- `verify-schema`
- `verify-sources`
- `verify-rules`
- `verify-dignity`
- `verify-avastha`
- `verify-vargas`
- `verify-jaimini`
- `verify-upagrahas`
- `verify-panchanga`

Then assert database cardinality and integrity:

- One D1 Panchanga row per saved person.
- The registered position-chart set per person, plus the expected dasha result.
- Expected key-detail and house-lord rows per chart.
- No saved person with a partial chart set.
- No orphaned generated facts.
- No violated, disabled, or untrusted foreign keys.

The launcher must return a nonzero exit code if rebuild or required verification fails.

## Honest coverage

The first launcher can rebuild only data with a current application writer. It must list the
following as unsupported rather than silently treating their zero row counts as success:

- `tbl_Fact_HouseFromReference`
- `tbl_Fact_KpSubLordChain`
- `tbl_Fact_BhinnaAshtakavarga`
- `tbl_Fact_BhinnaAshtakavargaContribution`
- `tbl_Fact_SarvaAshtakavarga`
- `tbl_Fact_AshtakavargaPinda`
- Ayanamsa/dasha/yoga benchmark and validation run tables
- Dasha-applicability facts blocked by empty applicability rules
- `tbl_Content_Interpretation`

As writers and runners are implemented, register them with the application pipeline and add
their completion assertions to the launcher. Do not place their calculation formulas in
PowerShell.

## Implementation slices

### Slice 1 — safe reset contract

- Add `GeneratedOnly` and `Everything` reset modes.
- Make `GeneratedOnly` the default.
- Add target/environment confirmation and before-count output.
- Verify exact deletion scope with a disposable or transaction-rolled-back test database.

### Slice 2 — bulk application command

- Add `rebuild-all` and `--fail-fast` to the CLI.
- Use `ChartGenerationService.GenerateAll` without duplicating repository order.
- Add deterministic progress, summary, and exit codes.
- Add automated tests for empty inputs, success, one-person failure, and retry.

### Slice 3 — PowerShell orchestration

- Add `Rebuild-AllGeneratedData.ps1` with the proposed modes and parameters.
- Build once and invoke the bulk CLI command once.
- Add structured run artifacts and interruption-safe failure reporting.
- Keep reset optional; rebuilding must also work without resetting first.

### Slice 4 — verification and documentation

- Add the verification sweep and SQL cardinality/integrity checks.
- Update `db/README.md` and `docs/cli/commands.md` with examples and recovery guidance.
- Update `action-required-audit.md` as unsupported targets gain writers.

## Acceptance scenarios

1. Dry-run/preflight against the current three saved people changes nothing.
2. `GeneratedOnly` reset preserves all birth details and removes only derived data.
3. A full rebuild restores every currently supported chart, dasha, analytics, and Panchanga
   row for all saved people.
4. A forced failure for one person is recorded; other people continue unless fail-fast was
   requested.
5. Rerunning after that failure completes the failed person without duplicate data.
6. An empty `tbl_BirthDetails` produces a clear no-input result rather than apparent success.
7. A non-dev or mistyped database target is stopped before deletion.
8. A second full rebuild produces the same logical results and passes uniqueness constraints.
9. Verification detects the currently broken ayanamsa/dasha benchmark foreign keys until a
   separate forward migration repairs them.

## Deferred decisions

- Whether a future `Everything` reset should automatically export/import birth details.
- Whether rebuild run history belongs only in filesystem artifacts or also in a database
  operations table.
- Whether parallel person-level generation is safe. Start sequentially because repositories
  use independent connections and generation is not one cross-repository transaction.
- Whether to add a dedicated resume selector such as `--birth-detail-id` after the first
  sequential implementation proves its failure-report format.
