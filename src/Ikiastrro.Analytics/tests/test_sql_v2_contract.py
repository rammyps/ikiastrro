from pathlib import Path


def test_v2_view_enforces_complete_consented_single_chart_cohort():
    migration = (Path(__file__).parents[3] / "db" /
                 "168_create_versioned_analytics_house_view.sql").read_text(encoding="utf-8")
    assert "CREATE OR ALTER VIEW dbo.vw_AnalyticsHouseFeaturesV2" in migration
    assert "HAVING COUNT(*) = 1" in migration
    assert "COUNT(DISTINCT stats.HouseFromLagna) = 12" in migration
    assert "COUNT(stats.Capacity) = 12" in migration
    assert "MIN(stats.RuleSetId) = chart.RuleSetId" in migration
    assert "subject.ConsentRecordedAtUtc IS NOT NULL" in migration
    assert "subject.WithdrawnAtUtc IS NULL" in migration
    assert "KI_D1_HOUSE_STRENGTH_V2" in migration
