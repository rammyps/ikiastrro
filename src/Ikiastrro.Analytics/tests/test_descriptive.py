from ikiastrro_analytics.descriptive import describe_subject


def test_suppresses_percentile_below_thirty():
    result = describe_subject(50, range(1, 7), bootstrap_iterations=100)
    assert result.measured_count == 6
    assert result.percentile is None
    assert result.sufficiency == "INSUFFICIENT"


def test_midrank_ties_and_leave_one_out_population():
    reference = [10] * 10 + [20] * 10 + [30] * 10
    result = describe_subject(20, reference, bootstrap_iterations=100)
    assert result.percentile == 50.0
    assert result.sufficiency == "EXPLORATORY"


def test_missingness_over_twenty_percent_marks_incomplete():
    result = describe_subject(50, [10, 20, 30, None], bootstrap_iterations=100)
    assert result.missing_rate == 0.25
    assert result.sufficiency == "INCOMPLETE"
    assert result.percentile is None


def test_robust_z_uses_iqr_scale():
    result = describe_subject(40, [10, 20, 30, 40, 50], bootstrap_iterations=100)
    assert result.median == 30
    assert round(result.robust_z, 4) == round(10 / (20 / 1.349), 4)
