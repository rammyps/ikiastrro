from ikiastrro_analytics.life_matter import build_comparisons, validate_rows


def row(subject, lens, value):
    chart = "D1" if lens == "D1_PROMISE" else "D9"
    return {"SubjectKey": subject, "LifeMatterFocusId": 10, "EvidenceLensCode": lens,
            "ChartType": chart, "ConfirmationChartType": "D9", "ChartResultId": subject + 100,
            "HouseNumber": 7, "Capacity": value, "Consistency": 50, "Context": 50,
            "OverallSupport": 50, "MissingReasonCode": None,
            "FeatureContractVersion": "KI_LIFE_MATTER_VARGA_V1"}


def test_two_lens_contract_is_valid():
    rows = [row(subject, lens, 40 + subject) for subject in (1, 2)
            for lens in ("D1_PROMISE", "VARGA_CONFIRMATION")]
    assert validate_rows(rows).is_valid


def test_comparisons_never_mix_d1_and_varga():
    rows = []
    for subject in range(31):
        rows += [row(subject, "D1_PROMISE", subject),
                 row(subject, "VARGA_CONFIRMATION", 100 - subject)]
    results = build_comparisons(rows, bootstrap_iterations=100)
    capacity = [item for item in results if item["FeatureCode"] == "KI_LM_CAPACITY_V1"]
    assert len(capacity) == 62
    assert all(item["eligible_count"] == 30 for item in capacity)
    d1 = next(item for item in capacity if item["SubjectKey"] == 0
              and item["EvidenceLensCode"] == "D1_PROMISE")
    varga = next(item for item in capacity if item["SubjectKey"] == 0
                 and item["EvidenceLensCode"] == "VARGA_CONFIRMATION")
    assert d1["percentile"] < varga["percentile"]


def test_wrong_confirmation_chart_is_rejected():
    item = row(1, "VARGA_CONFIRMATION", 50)
    item["ChartType"] = "D10"
    report = validate_rows([item])
    assert not report.is_valid
    assert any("wrong chart type" in error for error in report.errors)
