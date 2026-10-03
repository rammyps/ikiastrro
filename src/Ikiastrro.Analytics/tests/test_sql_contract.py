from pathlib import Path


def test_anonymous_view_does_not_select_direct_identifiers():
    migration = (Path(__file__).parents[3] / "db" /
                 "163_create_statistical_analytics_foundation.sql").read_text(encoding="utf-8")
    view = migration.split("CREATE OR ALTER VIEW dbo.vw_AnalyticsHouseFeatures", 1)[1]
    view = view.split("GO", 1)[0]
    assert "bd.Name" not in view
    assert "DateOfBirth" not in view
    assert "PlaceCity" not in view
    assert "SubjectKey" in view


def test_database_gate_suppresses_small_cohort_percentiles():
    migration = (Path(__file__).parents[3] / "db" /
                 "163_create_statistical_analytics_foundation.sql").read_text(encoding="utf-8")
    assert "MeasuredCount < 30 AND Percentile IS NULL" in migration
