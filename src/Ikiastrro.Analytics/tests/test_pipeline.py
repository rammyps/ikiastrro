from ikiastrro_analytics.pipeline import build_comparisons


def test_pipeline_is_leave_one_person_out_and_separates_houses():
    rows = []
    for subject in range(31):
        for house in (1, 2):
            rows.append({
                "SubjectKey": subject, "ChartResultId": subject + 100,
                "HouseFromLagna": house, "RuleSetId": 1, "Ayanamsha": "Lahiri",
                "HouseSystem": "WholeSign", "Capacity": subject + house,
                "Consistency": 50, "Context": 50, "OverallSupport": 50,
            })
    results = build_comparisons(rows, bootstrap_iterations=100)
    capacity = [row for row in results if row["FeatureCode"] == "KI_D1_HOUSE_CAPACITY_V2"]
    assert len(capacity) == 62
    assert all(row["eligible_count"] == 30 for row in capacity)
    assert all(row["percentile"] is not None for row in capacity)
