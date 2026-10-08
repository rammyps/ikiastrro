"""Validation and descriptive comparisons for natal Dasha Matter rule observations."""

from __future__ import annotations

from collections import defaultdict
from dataclasses import asdict

from .descriptive import describe_subject
from .validation import ValidationReport

CONTRACT = "KI_DASHA_MATTER_V1"
SCOPES = {"EXAMPLE", "CHART_THEME", "HOUSE", "NATURAL_KARAKA"}
MISSING_REASONS = {"CHART_NOT_AVAILABLE", "SOURCE_PARTIAL"}
# column in the fetched row -> persisted feature code, with the inclusive range each value must sit in
FEATURES = {"TargetCount": ("KI_DM_TARGET_COUNT_V1", 9), "Present": ("KI_DM_PRESENT_V1", 100)}


def validate_rows(rows: list[dict]) -> ValidationReport:
    by_subject: dict[str, list[dict]] = defaultdict(list)
    errors: list[str] = []
    seen: set[tuple[str, object]] = set()
    for number, row in enumerate(rows, start=1):
        subject = str(row.get("SubjectKey"))
        rule = row.get("RuleNumber")
        if (subject, rule) in seen:
            errors.append(f"duplicate Dasha Matter observation at row {number}: {(subject, rule)}")
        seen.add((subject, rule))
        by_subject[subject].append(row)
        if row.get("FeatureContractVersion") != CONTRACT:
            errors.append(f"subject={subject}, rule={rule} has incompatible feature contract")
        if row.get("ScopeKind") not in SCOPES:
            errors.append(f"subject={subject}, rule={rule} has invalid scope {row.get('ScopeKind')}")
        reason = row.get("MissingReasonCode")
        if reason is not None and reason not in MISSING_REASONS:
            errors.append(f"subject={subject}, rule={rule} has invalid missing reason {reason}")
        for column, (_, upper) in FEATURES.items():
            value = row.get(column)
            if reason is None and value is None:
                errors.append(f"subject={subject}, rule={rule} has missing {column}")
            elif reason is not None and value is not None:
                errors.append(f"subject={subject}, rule={rule} has {column} despite missing reason {reason}")
            elif value is not None and (not isinstance(value, (int, float)) or isinstance(value, bool)
                                        or not 0 <= value <= upper):
                errors.append(f"subject={subject}, rule={rule} has invalid {column}={value}")
    common: set[object] | None = None
    for subject, subject_rows in by_subject.items():
        keys = {row.get("RuleNumber") for row in subject_rows}
        if common is None:
            common = keys
        elif keys != common:
            errors.append(f"subject={subject} does not have the common rule set")
    return ValidationReport(len(by_subject), len(rows), tuple(errors))


def build_comparisons(rows: list[dict], *, seed: int = 20261001,
                      bootstrap_iterations: int = 2000) -> list[dict]:
    grouped: dict[object, list[dict]] = defaultdict(list)
    for row in rows:
        grouped[row["RuleNumber"]].append(row)
    output: list[dict] = []
    for group in grouped.values():
        for subject in group:
            for column, (code, _) in FEATURES.items():
                reference = [row[column] for row in group if row["SubjectKey"] != subject["SubjectKey"]]
                comparison = describe_subject(subject[column], reference, seed=seed,
                                              bootstrap_iterations=bootstrap_iterations)
                output.append({"SubjectKey": subject["SubjectKey"],
                               "RuleNumber": subject["RuleNumber"],
                               "Varga": subject["Varga"],
                               "ScopeKind": subject["ScopeKind"],
                               "HouseNumber": subject["HouseNumber"],
                               "KarakaCode": subject["KarakaCode"],
                               "FeatureCode": code,
                               "FeatureContractVersion": subject["FeatureContractVersion"],
                               "SourceMissingReasonCode": subject["MissingReasonCode"],
                               **asdict(comparison)})
    return output
