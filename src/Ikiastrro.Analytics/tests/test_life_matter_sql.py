import pytest

from ikiastrro_analytics.life_matter_sql import (
    ensure_development_dataset, fetch_features, publish_comparisons,
)


class Cursor:
    def __init__(self, connection):
        self.connection = connection
        self.description = [("SubjectKey",), ("LifeMatterFocusId",)]
        self._mode = None

    def execute(self, statement, *parameters):
        self.connection.calls.append((statement, parameters))
        if self.connection.fail_on_comparison and "tbl_Fact_LifeMatterStatisticalComparisons" in statement:
            raise RuntimeError("write failed")
        self._mode = "existing" if "SELECT Id FROM dbo.tbl_Dim_AnalyticsDatasets" in statement else (
            "dataset" if "INSERT dbo.tbl_Dim_AnalyticsDatasets" in statement else (
            "run" if "INSERT dbo.tbl_Fact_AnalyticsRuns" in statement else None))
        return self

    def fetchone(self):
        return (41,) if self.connection.existing_dataset else None

    def fetchval(self):
        return 42 if self._mode == "dataset" else 84

    def fetchall(self):
        return [("subject-1", 10)]


class Connection:
    def __init__(self, *, existing_dataset=False, fail_on_comparison=False):
        self.existing_dataset = existing_dataset
        self.fail_on_comparison = fail_on_comparison
        self.calls = []
        self.commits = 0
        self.rollbacks = 0
        self._cursor = Cursor(self)

    def cursor(self):
        return self._cursor

    def execute(self, statement, *parameters):
        return self._cursor.execute(statement, *parameters)

    def commit(self):
        self.commits += 1

    def rollback(self):
        self.rollbacks += 1


def comparison():
    return {
        "SubjectKey": "subject-1", "LifeMatterFocusId": 10,
        "EvidenceLensCode": "D1_PROMISE", "ChartResultId": 100,
        "ChartType": "D1", "HouseFromLagna": 7,
        "FeatureCode": "KI_LM_CAPACITY_V1",
        "FeatureContractVersion": "KI_LIFE_MATTER_VARGA_V1",
        "personal_value": 50, "eligible_count": 30, "measured_count": 30,
        "missing_rate": 0, "median": 50, "q1": 40, "q3": 60,
        "mean": 50, "standard_deviation": 10, "robust_z": 0,
        "percentile": 50, "median_ci_low": 45, "median_ci_high": 55,
        "sufficiency": "EXPLORATORY", "SourceMissingReasonCode": None,
    }


def test_fetch_uses_anonymous_life_matter_view():
    connection = Connection()
    assert fetch_features(connection) == [{"SubjectKey": "subject-1", "LifeMatterFocusId": 10}]
    assert "vw_AnalyticsLifeMatterFeaturesV1" in connection.calls[0][0]
    assert "BirthDetailId" not in connection.calls[0][0]


def test_dataset_is_reused_without_commit():
    connection = Connection(existing_dataset=True)
    assert ensure_development_dataset(connection) == 41
    assert connection.commits == 0


def test_dataset_creation_is_versioned_and_committed():
    connection = Connection()
    assert ensure_development_dataset(connection, git_commit="abc123") == 42
    assert connection.commits == 1
    insert = next(call for call in connection.calls if "INSERT dbo.tbl_Dim_AnalyticsDatasets" in call[0])
    assert "KI_LIFE_MATTER_VARGA_V1" in insert[0]
    assert insert[1][1] == "abc123"


def test_publication_completes_and_commits_atomically():
    connection = Connection()
    assert publish_comparisons(connection, 42, [comparison()], seed=7, bootstrap_iterations=100) == 84
    assert connection.commits == 1
    assert connection.rollbacks == 0
    assert any("StatusCode='COMPLETED'" in call[0] for call in connection.calls)


def test_publication_rolls_back_on_comparison_failure():
    connection = Connection(fail_on_comparison=True)
    with pytest.raises(RuntimeError, match="write failed"):
        publish_comparisons(connection, 42, [comparison()], seed=7, bootstrap_iterations=100)
    assert connection.commits == 0
    assert connection.rollbacks == 1
