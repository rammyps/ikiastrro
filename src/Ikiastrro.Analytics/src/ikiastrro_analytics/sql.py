"""SQL Server extraction and persistence boundary."""

from __future__ import annotations

import json
import os


def connect():
    import pyodbc

    connection_string = os.environ.get(
        "IKIASTRRO_SQL_CONNECTION",
        "Driver={ODBC Driver 18 for SQL Server};Server=localhost\\SQLSERVER2025;"
        "Database=ikiastrro;Trusted_Connection=yes;TrustServerCertificate=yes",
    )
    return pyodbc.connect(connection_string)


def fetch_features(connection) -> list[dict]:
    cursor = connection.execute("""
        SELECT SubjectKey, ChartResultId, HouseFromLagna, Capacity, Consistency,
               Context, OverallSupport, RuleSetId, Ayanamsha, HouseSystem,
               EngineVersion, BirthTimeQuality, ComputedAtUtc, FeatureContractVersion
        FROM dbo.vw_AnalyticsHouseFeaturesV2
        ORDER BY RuleSetId, HouseFromLagna, SubjectKey
    """)
    columns = [column[0] for column in cursor.description]
    return [dict(zip(columns, row)) for row in cursor.fetchall()]


def prepare_personal_cohort(connection) -> int:
    """Make saved charts available without changing existing analytics choices."""
    cursor = connection.cursor()
    cursor.execute("""
        INSERT dbo.tbl_Dim_AnalyticsSubjects
            (BirthDetailId, SubjectClassification, ResearchUseStatus,
             ConsentRecordedAtUtc, BirthTimeQuality)
        SELECT birth.Id, 'PERSONAL', 'ELIGIBLE', SYSUTCDATETIME(), 'UNKNOWN'
        FROM dbo.tbl_BirthDetails birth
        WHERE NOT EXISTS (
                  SELECT 1 FROM dbo.tbl_Dim_AnalyticsSubjects existing
                  WHERE existing.BirthDetailId = birth.Id)
          AND EXISTS (
                  SELECT 1 FROM dbo.tbl_ChartResults chart
                  WHERE chart.BirthDetailId = birth.Id AND chart.ChartType = 'D1')
    """)
    inserted = cursor.rowcount
    connection.commit()
    return int(inserted)


def ensure_development_dataset(connection, *, git_commit: str | None = None) -> int:
    """Return the stable v1 development dataset, creating it when absent."""
    cursor = connection.cursor()
    existing = cursor.execute("""
        SELECT Id FROM dbo.tbl_Dim_AnalyticsDatasets
        WHERE DatasetCode = 'KI_D1_HOUSE_STRENGTH' AND DatasetVersion = 2
    """).fetchone()
    if existing:
        return int(existing[0])
    criteria = json.dumps({
        "chartType": "D1",
        "researchUseStatus": "ELIGIBLE",
        "subjectClassification": "PERSONAL_OR_RESEARCH",
        "completeHouseCount": 12,
        "featureContract": "KI_D1_HOUSE_STRENGTH_V2",
    }, separators=(",", ":"))
    dataset_id = cursor.execute("""
        INSERT dbo.tbl_Dim_AnalyticsDatasets
            (DatasetCode, DatasetVersion, FeatureContractVersion, Description,
             InclusionCriteriaJson, GitCommit, RuleSetId, IsDevelopmentOnly)
        OUTPUT INSERTED.Id
        VALUES ('KI_D1_HOUSE_STRENGTH', 2, 'KI_D1_HOUSE_STRENGTH_V2',
                'D1 house-strength comparison after f5ac02e strength and varga corrections.',
                ?, ?, (SELECT TOP (1) Id FROM dbo.tbl_Rule_Sets WHERE IsActive=1 ORDER BY Id DESC), 1)
    """, criteria, git_commit).fetchval()
    connection.commit()
    return int(dataset_id)


def publish_comparisons(connection, dataset_id: int, comparisons: list[dict], *,
                        seed: int, bootstrap_iterations: int) -> int:
    """Persist an atomic completed run; failed work is rolled back and never published."""
    cursor = connection.cursor()
    try:
        run_id = cursor.execute("""
            INSERT dbo.tbl_Fact_AnalyticsRuns
                (DatasetId, MethodVersion, RandomSeed, BootstrapIterations, StatusCode)
            OUTPUT INSERTED.Id
            VALUES (?, 'DESCRIPTIVE_V1', ?, ?, 'STARTED')
        """, dataset_id, seed, bootstrap_iterations).fetchval()
        statement = """
            INSERT dbo.tbl_Fact_StatisticalComparisons
                (AnalyticsRunId, SubjectKey, ChartResultId, HouseFromLagna, FeatureCode,
                 PersonalValue, EligibleCount, MeasuredCount, MissingRate,
                 ReferenceMedian, ReferenceQ1, ReferenceQ3, ReferenceMean, ReferenceStdDev,
                 RobustZ, Percentile, MedianCiLow, MedianCiHigh, SufficiencyCode)
            VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
        """
        for row in comparisons:
            cursor.execute(statement, run_id, row["SubjectKey"], row["ChartResultId"],
                           row["HouseFromLagna"], row["FeatureCode"], row["personal_value"],
                           row["eligible_count"], row["measured_count"], row["missing_rate"],
                           row["median"], row["q1"], row["q3"], row["mean"],
                           row["standard_deviation"], row["robust_z"], row["percentile"],
                           row["median_ci_low"], row["median_ci_high"], row["sufficiency"])
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
