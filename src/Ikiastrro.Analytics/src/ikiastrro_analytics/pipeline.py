"""Batch descriptive-comparison pipeline for canonical SQL features."""

from __future__ import annotations

from collections import defaultdict
from dataclasses import asdict

from .descriptive import describe_subject

FEATURES = {
    "Capacity": "KI_D1_HOUSE_CAPACITY_V2",
    "Consistency": "KI_D1_HOUSE_CONSISTENCY_V2",
    "Context": "KI_D1_HOUSE_CONTEXT_V2",
    "OverallSupport": "KI_D1_HOUSE_SUPPORT_V2",
}


def build_comparisons(rows: list[dict], *, seed: int = 20261001, bootstrap_iterations: int = 2000) -> list[dict]:
    """Build personal-app leave-one-person-out comparisons by house."""
    grouped: dict[int, list[dict]] = defaultdict(list)
    for row in rows:
        grouped[row["HouseFromLagna"]].append(row)
    output: list[dict] = []
    for group in grouped.values():
        for subject in group:
            for column, feature_code in FEATURES.items():
                reference = [row[column] for row in group if row["SubjectKey"] != subject["SubjectKey"]]
                comparison = describe_subject(subject[column], reference, seed=seed,
                                              bootstrap_iterations=bootstrap_iterations)
                output.append({"SubjectKey": subject["SubjectKey"],
                               "ChartResultId": subject["ChartResultId"],
                               "HouseFromLagna": subject["HouseFromLagna"],
                               "FeatureCode": feature_code, **asdict(comparison)})
    return output
