"""SQL extraction and atomic persistence for Dasha Matter analytics."""

from __future__ import annotations

import json


def fetch_features(connection) -> list[dict]:
    cursor = connection.execute("""
        SELECT SubjectKey, RuleNumber, Varga, ScopeKind, HouseNumber, KarakaCode,
               TargetCount, Present, MissingReasonCode, BirthTimeQuality,
               ComputedAtUtc, FeatureContractVersion
        FROM dbo.vw_AnalyticsDashaMatterFeaturesV1
        ORDER BY RuleNumber, SubjectKey
    """)
    columns = [column[0] for column in cursor.description]
    return [dict(zip(columns, row)) for row in cursor.fetchall()]


def ensure_development_dataset(connection, *, git_commit: str | None = None) -> int:
    """Return the stable v1 Dasha Matter dataset, creating it when absent."""
    cursor = connection.cursor()
    existing = cursor.execute("""
        SELECT Id FROM dbo.tbl_Dim_AnalyticsDatasets
        WHERE DatasetCode = 'KI_DASHA_MATTER' AND DatasetVersion = 1
    """).fetchone()
    if existing:
        return int(existing[0])
    criteria = json.dumps({
        "researchUseStatus": "ELIGIBLE",
        "subjectClassification": "PERSONAL_OR_RESEARCH",
        "observationGrain": "SUBJECT_RULE",
        "scope": "NATAL_RULE_STRUCTURE",
        "featureContract": "KI_DASHA_MATTER_V1",
    }, separators=(",", ":"))
    dataset_id = cursor.execute("""
        INSERT dbo.tbl_Dim_AnalyticsDatasets
            (DatasetCode, DatasetVersion, FeatureContractVersion, Description,
             InclusionCriteriaJson, GitCommit, RuleSetId, IsDevelopmentOnly)
        OUTPUT INSERTED.Id
        VALUES ('KI_DASHA_MATTER', 1, 'KI_DASHA_MATTER_V1',
                'Natal dasha-matter rule structure: how many planets meet each rule, compared across charts.',
                ?, ?, (SELECT TOP (1) Id FROM dbo.tbl_Rule_Sets
                       WHERE IsActive=1 ORDER BY Id DESC), 1)
    """, criteria, git_commit).fetchval()
    connection.commit()
    return int(dataset_id)


def publish_comparisons(connection, dataset_id: int, comparisons: list[dict], *,
                        seed: int, bootstrap_iterations: int) -> int:
    """Persist one completed Dasha Matter run atomically."""
    cursor = connection.cursor()
    try:
        run_id = cursor.execute("""
            INSERT dbo.tbl_Fact_AnalyticsRuns
                (DatasetId, MethodVersion, RandomSeed, BootstrapIterations, StatusCode)
            OUTPUT INSERTED.Id
            VALUES (?, 'DESCRIPTIVE_DM_V1', ?, ?, 'STARTED')
        """, dataset_id, seed, bootstrap_iterations).fetchval()
        statement = """
            INSERT dbo.tbl_Fact_DashaMatterStatisticalComparisons
                (AnalyticsRunId, SubjectKey, RuleNumber, Varga, ScopeKind, HouseNumber,
                 KarakaCode, FeatureCode, FeatureContractVersion, PersonalValue,
                 EligibleCount, MeasuredCount, MissingRate, ReferenceMedian, ReferenceQ1,
                 ReferenceQ3, ReferenceMean, ReferenceStdDev, RobustZ, Percentile,
                 MedianCiLow, MedianCiHigh, SufficiencyCode, SourceMissingReasonCode)
            VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
        """
        for row in comparisons:
            cursor.execute(
                statement, run_id, row["SubjectKey"], row["RuleNumber"], row["Varga"],
                row["ScopeKind"], row["HouseNumber"], row["KarakaCode"], row["FeatureCode"],
                row["FeatureContractVersion"], row["personal_value"], row["eligible_count"],
                row["measured_count"], row["missing_rate"], row["median"], row["q1"], row["q3"],
                row["mean"], row["standard_deviation"], row["robust_z"], row["percentile"],
                row["median_ci_low"], row["median_ci_high"], row["sufficiency"],
                row["SourceMissingReasonCode"],
            )
        cursor.execute("""
            UPDATE dbo.tbl_Fact_AnalyticsRuns
            SET StatusCode='COMPLETED', CompletedAtUtc=SYSUTCDATETIME()
            WHERE Id=?
        """, run_id)
        connection.commit()
        return int(run_id)
    except Exception:
        connection.rollback()
        raise
