"""SQL extraction and atomic persistence for Life Matter/Varga analytics."""

from __future__ import annotations

import json


def fetch_features(connection) -> list[dict]:
    cursor = connection.execute("""
        SELECT SubjectKey, LifeMatterFocusId, LifeMatterCode, EvidenceLensCode,
               ChartType, ConfirmationChartType, ChartResultId, HouseNumber,
               Capacity, Consistency, Context, OverallSupport, MissingReasonCode,
               MappingRuleSetId, StatisticsRuleSetId, ChartRuleSetId,
               Ayanamsha, HouseSystem, EngineVersion, BirthTimeQuality,
               ComputedAtUtc, FeatureContractVersion
        FROM dbo.vw_AnalyticsLifeMatterFeaturesV1
        ORDER BY LifeMatterFocusId, EvidenceLensCode, SubjectKey
    """)
    columns = [column[0] for column in cursor.description]
    return [dict(zip(columns, row)) for row in cursor.fetchall()]


def ensure_development_dataset(connection, *, git_commit: str | None = None) -> int:
    """Return the stable v1 Life Matter dataset, creating it when absent."""
    cursor = connection.cursor()
    existing = cursor.execute("""
        SELECT Id FROM dbo.tbl_Dim_AnalyticsDatasets
        WHERE DatasetCode = 'KI_LIFE_MATTER_VARGA' AND DatasetVersion = 1
    """).fetchone()
    if existing:
        return int(existing[0])
    criteria = json.dumps({
        "researchUseStatus": "ELIGIBLE",
        "subjectClassification": "PERSONAL_OR_RESEARCH",
        "observationGrain": "SUBJECT_FOCUS_LENS",
        "lenses": ["D1_PROMISE", "VARGA_CONFIRMATION"],
        "featureContract": "KI_LIFE_MATTER_VARGA_V1",
    }, separators=(",", ":"))
    dataset_id = cursor.execute("""
        INSERT dbo.tbl_Dim_AnalyticsDatasets
            (DatasetCode, DatasetVersion, FeatureContractVersion, Description,
             InclusionCriteriaJson, GitCommit, RuleSetId, IsDevelopmentOnly)
        OUTPUT INSERTED.Id
        VALUES ('KI_LIFE_MATTER_VARGA', 1, 'KI_LIFE_MATTER_VARGA_V1',
                'Separate D1-promise and subject-Varga comparisons by Life Matter focus.',
                ?, ?, (SELECT TOP (1) Id FROM dbo.tbl_Rule_Sets
                       WHERE IsActive=1 ORDER BY Id DESC), 1)
    """, criteria, git_commit).fetchval()
    connection.commit()
    return int(dataset_id)


def publish_comparisons(connection, dataset_id: int, comparisons: list[dict], *,
                        seed: int, bootstrap_iterations: int) -> int:
    """Persist one completed Life Matter run atomically."""
    cursor = connection.cursor()
    try:
        run_id = cursor.execute("""
            INSERT dbo.tbl_Fact_AnalyticsRuns
                (DatasetId, MethodVersion, RandomSeed, BootstrapIterations, StatusCode)
            OUTPUT INSERTED.Id
            VALUES (?, 'DESCRIPTIVE_LM_V1', ?, ?, 'STARTED')
        """, dataset_id, seed, bootstrap_iterations).fetchval()
        statement = """
            INSERT dbo.tbl_Fact_LifeMatterStatisticalComparisons
                (AnalyticsRunId, SubjectKey, LifeMatterFocusId, EvidenceLensCode,
                 ChartResultId, ChartType, HouseFromLagna, FeatureCode,
                 FeatureContractVersion, PersonalValue, EligibleCount, MeasuredCount,
                 MissingRate, ReferenceMedian, ReferenceQ1, ReferenceQ3, ReferenceMean,
                 ReferenceStdDev, RobustZ, Percentile, MedianCiLow, MedianCiHigh,
                 SufficiencyCode, SourceMissingReasonCode)
            VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
        """
        for row in comparisons:
            cursor.execute(
                statement, run_id, row["SubjectKey"], row["LifeMatterFocusId"],
                row["EvidenceLensCode"], row["ChartResultId"], row["ChartType"],
                row["HouseFromLagna"], row["FeatureCode"], row["FeatureContractVersion"],
                row["personal_value"], row["eligible_count"], row["measured_count"],
                row["missing_rate"], row["median"], row["q1"], row["q3"], row["mean"],
                row["standard_deviation"], row["robust_z"], row["percentile"],
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
