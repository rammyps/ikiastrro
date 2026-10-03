---
last_updated: 2026-10-04
reflects: IkiAstrro v10 descriptive analytics package 0.1
---

# IkiAstrro Analytics

Batch analytics for the v10 descriptive population-comparison increment. This package consumes
canonical values from the contract-gated `dbo.vw_AnalyticsHouseFeaturesV2`; it does not recalculate astrology.

Feature contract v2 begins after local master commit `f5ac02e`, which corrected Saptavargaja,
Oja-Yugma and the default D2/D3/D7 schemes. Dataset v1 remains historical and is never pooled
with v2.

The pure statistics layer implements leave-one-person-out comparison, mid-rank percentiles,
median/IQR, population standard deviation, IQR-scaled robust z-scores, deterministic bootstrap
median intervals, missingness checks and the Phase 0 sample-size gates.

Both `validate-dataset` and `describe` use the same integrity gate. Each subject must have one
D1 chart and numeric 0–100 values for houses 1–12. Personal mode does not gate or split results
by rule-set or feature-contract version; duplicates, incomplete sets, mixed chart results and
invalid values still stop the run.

Eligible `PERSONAL` and `RESEARCH` subjects appear in the SQL
view. Names, birth inputs, place text and notes are not exposed. Fewer than 30 measured reference
people always produces a null percentile and `INSUFFICIENT` status.

After installing the package and its `pyodbc` dependency, run:

```text
ikiastrro-analytics prepare-personal-cohort
ikiastrro-analytics validate-dataset
ikiastrro-analytics describe --git-commit <commit>
```

Set `IKIASTRRO_SQL_CONNECTION` to override the default trusted local SQL Server connection.

`describe` creates or reuses the versioned development dataset, computes every eligible
subject's comparisons with that subject removed from the reference group, and atomically records
the run. A zero-subject run is valid and proves the guarded pipeline without enrolling anyone.
