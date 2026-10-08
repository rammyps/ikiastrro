from ikiastrro_analytics.dasha_matter import build_comparisons, validate_rows


def row(subject, rule=1, count=1, present=100, reason=None, scope="EXAMPLE"):
    return {"SubjectKey": subject, "RuleNumber": rule, "Varga": "D9", "ScopeKind": scope,
            "HouseNumber": None, "KarakaCode": None, "TargetCount": count, "Present": present,
            "MissingReasonCode": reason, "FeatureContractVersion": "KI_DASHA_MATTER_V1"}


def test_valid_rows_pass():
    assert validate_rows([row("a"), row("b", count=0, present=0)]).is_valid


def test_rejects_duplicates_bad_counts_and_zero_for_missing():
    report = validate_rows([row("a"), row("a"), row("b", count=12),
                            row("c", count=0, present=0, reason="SOURCE_PARTIAL"),
                            row("d", count=None, present=None)])
    text = " ".join(report.errors)
    assert "duplicate" in text
    assert "invalid TargetCount=12" in text
    assert "despite missing reason" in text
    assert "missing TargetCount" in text


def test_rejects_unknown_scope_and_uneven_rule_sets():
    report = validate_rows([row("a", scope="MYSTERY"), row("a", rule=2), row("b")])
    assert any("invalid scope" in e for e in report.errors)
    assert any("common rule set" in e for e in report.errors)


def test_comparisons_cover_both_features_and_gate_percentile_below_30():
    rows = [row(f"s{i}", count=i % 3, present=0 if i % 3 == 0 else 100) for i in range(5)]
    out = build_comparisons(rows, bootstrap_iterations=100)
    assert len(out) == 5 * 2
    assert {c["FeatureCode"] for c in out} == {"KI_DM_TARGET_COUNT_V1", "KI_DM_PRESENT_V1"}
    assert all(c["percentile"] is None for c in out)
    assert all(c["ScopeKind"] == "EXAMPLE" for c in out)


def test_missing_subject_is_excluded_from_reference_not_counted_as_zero():
    rows = [row(f"s{i}") for i in range(3)] + [row("m", count=None, present=None, reason="SOURCE_PARTIAL")]
    out = build_comparisons(rows, bootstrap_iterations=100)
    one = next(c for c in out if c["SubjectKey"] == "s0" and c["FeatureCode"] == "KI_DM_PRESENT_V1")
    assert one["eligible_count"] == 3 and one["measured_count"] == 2
