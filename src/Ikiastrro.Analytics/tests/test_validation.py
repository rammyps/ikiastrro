import pytest

from ikiastrro_analytics.validation import validate_feature_rows


def row(subject=1, house=1, chart=100, **overrides):
    value = {
        "SubjectKey": subject,
        "ChartResultId": chart,
        "HouseFromLagna": house,
        "Capacity": 50,
        "Consistency": 50,
        "Context": 50,
        "OverallSupport": 50,
        "RuleSetId": 1,
        "Ayanamsha": "Traditional Lahiri",
        "HouseSystem": "WholeSign",
        "EngineVersion": "test",
        "FeatureContractVersion": "KI_D1_HOUSE_STRENGTH_V2",
    }
    value.update(overrides)
    return value


def complete_subject(subject=1):
    return [row(subject=subject, house=house, chart=subject + 100)
            for house in range(1, 13)]


def test_empty_opted_in_cohort_is_valid():
    report = validate_feature_rows([])
    assert report.is_valid
    assert report.subject_count == 0


def test_complete_single_chart_subject_is_valid():
    report = validate_feature_rows(complete_subject())
    assert report.is_valid
    assert report.row_count == 12


def test_missing_house_blocks_publication():
    report = validate_feature_rows(complete_subject()[:-1])
    assert not report.is_valid
    with pytest.raises(ValueError, match="complete 12-house set"):
        report.require_valid()


def test_duplicate_house_blocks_publication():
    rows = complete_subject()
    rows[-1] = row(house=11, chart=101)
    report = validate_feature_rows(rows)
    assert not report.is_valid
    assert any("duplicate observation" in error for error in report.errors)


@pytest.mark.parametrize("column,value", [
    ("Capacity", None),
    ("Consistency", -1),
    ("Context", 101),
    ("OverallSupport", "50"),
])
def test_missing_or_out_of_range_feature_blocks_publication(column, value):
    rows = complete_subject()
    rows[0][column] = value
    report = validate_feature_rows(rows)
    assert not report.is_valid
    assert any(column in error for error in report.errors)


def test_mixed_chart_results_block_publication():
    rows = complete_subject()
    rows[-1]["ChartResultId"] = 999
    report = validate_feature_rows(rows)
    assert not report.is_valid
    assert any("spans 2 chart results" in error for error in report.errors)


def test_personal_mode_does_not_gate_on_feature_contract():
    rows = complete_subject()
    rows[-1]["FeatureContractVersion"] = "KI_D1_HOUSE_STRENGTH_V1"
    report = validate_feature_rows(rows)
    assert report.is_valid
