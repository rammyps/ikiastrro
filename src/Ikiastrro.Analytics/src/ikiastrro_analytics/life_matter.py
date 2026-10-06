"""Validation and descriptive comparisons for Life Matter/Varga observations."""

from __future__ import annotations

from collections import defaultdict
from dataclasses import asdict

from .descriptive import describe_subject
from .validation import FEATURE_COLUMNS, ValidationReport

LENSES = {"D1_PROMISE", "VARGA_CONFIRMATION"}
MISSING_REASONS = {"UNSUPPORTED_REFERENCE", "CHART_NOT_AVAILABLE",
                   "HOUSE_STATISTICS_NOT_AVAILABLE", "SOURCE_PARTIAL"}
FEATURES = {"Capacity": "KI_LM_CAPACITY_V1", "Consistency": "KI_LM_CONSISTENCY_V1",
            "Context": "KI_LM_CONTEXT_V1", "OverallSupport": "KI_LM_SUPPORT_V1"}


def validate_rows(rows: list[dict]) -> ValidationReport:
    by_subject: dict[str, list[dict]] = defaultdict(list)
    errors: list[str] = []
    seen: set[tuple[str, object, object]] = set()
    for number, row in enumerate(rows, start=1):
        subject = str(row.get("SubjectKey"))
        focus = row.get("LifeMatterFocusId")
        lens = row.get("EvidenceLensCode")
        key = (subject, focus, lens)
        if key in seen:
            errors.append(f"duplicate Life Matter observation at row {number}: {key}")
        seen.add(key)
        by_subject[subject].append(row)
        if lens not in LENSES:
            errors.append(f"subject={subject}, focus={focus} has invalid lens {lens}")
        if row.get("FeatureContractVersion") != "KI_LIFE_MATTER_VARGA_V1":
            errors.append(f"subject={subject}, focus={focus} has incompatible feature contract")
        if lens == "D1_PROMISE" and row.get("ChartType") != "D1":
            errors.append(f"subject={subject}, focus={focus} D1 lens is not D1")
        if lens == "VARGA_CONFIRMATION" and row.get("ChartType") != row.get("ConfirmationChartType"):
            errors.append(f"subject={subject}, focus={focus} Varga lens has wrong chart type")
        reason = row.get("MissingReasonCode")
        if reason is not None and reason not in MISSING_REASONS:
            errors.append(f"subject={subject}, focus={focus} has invalid missing reason {reason}")
        for column in FEATURE_COLUMNS:
            value = row.get(column)
            if reason is None and value is None:
                errors.append(f"subject={subject}, focus={focus}, lens={lens} has missing {column}")
            elif value is not None and (not isinstance(value, (int, float)) or isinstance(value, bool)
                                        or not 0 <= value <= 100):
                errors.append(f"subject={subject}, focus={focus}, lens={lens} has invalid {column}={value}")
        if reason is None and row.get("ChartResultId") is None:
            errors.append(f"subject={subject}, focus={focus}, lens={lens} has no chart result")
    common: set[tuple[object, object]] | None = None
    for subject, subject_rows in by_subject.items():
        keys = {(row.get("LifeMatterFocusId"), row.get("EvidenceLensCode")) for row in subject_rows}
        if common is None:
            common = keys
        elif keys != common:
            errors.append(f"subject={subject} does not have the common focus/lens set")
    return ValidationReport(len(by_subject), len(rows), tuple(errors))


def build_comparisons(rows: list[dict], *, seed: int = 20261001,
                      bootstrap_iterations: int = 2000) -> list[dict]:
    grouped: dict[tuple[int, str], list[dict]] = defaultdict(list)
    for row in rows:
        grouped[(row["LifeMatterFocusId"], row["EvidenceLensCode"])].append(row)
    output: list[dict] = []
    for group in grouped.values():
        for subject in group:
            for column, code in FEATURES.items():
                reference = [row[column] for row in group if row["SubjectKey"] != subject["SubjectKey"]]
                comparison = describe_subject(subject[column], reference, seed=seed,
                                              bootstrap_iterations=bootstrap_iterations)
                output.append({"SubjectKey": subject["SubjectKey"],
                               "LifeMatterFocusId": subject["LifeMatterFocusId"],
                               "EvidenceLensCode": subject["EvidenceLensCode"],
                               "ChartResultId": subject["ChartResultId"],
                               "ChartType": subject["ChartType"],
                               "HouseFromLagna": subject["HouseNumber"],
                               "FeatureCode": code,
                               "FeatureContractVersion": subject["FeatureContractVersion"],
                               "SourceMissingReasonCode": subject["MissingReasonCode"],
                               **asdict(comparison)})
    return output
