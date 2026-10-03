"""Validation gate for canonical D1 analytics feature rows."""

from __future__ import annotations

from collections import defaultdict
from dataclasses import dataclass


FEATURE_COLUMNS = ("Capacity", "Consistency", "Context", "OverallSupport")
EXPECTED_HOUSES = set(range(1, 13))


@dataclass(frozen=True)
class ValidationReport:
    subject_count: int
    row_count: int
    errors: tuple[str, ...]

    @property
    def is_valid(self) -> bool:
        return not self.errors

    def require_valid(self) -> None:
        if self.errors:
            raise ValueError("Invalid analytics dataset: " + "; ".join(self.errors))


def validate_feature_rows(rows: list[dict]) -> ValidationReport:
    """Validate the extraction contract before any analytics run is created.

    An empty opted-in cohort is valid. Every present subject must contribute exactly
    one D1 chart with one complete numeric 0-100 row for each house.
    """
    by_subject: dict[str, list[dict]] = defaultdict(list)
    errors: list[str] = []
    seen_observations: set[tuple[str, object, object]] = set()

    for row_number, row in enumerate(rows, start=1):
        subject = str(row.get("SubjectKey"))
        chart_result = row.get("ChartResultId")
        house = row.get("HouseFromLagna")
        observation = (subject, chart_result, house)
        if observation in seen_observations:
            errors.append(
                f"duplicate observation at row {row_number}: "
                f"subject={subject}, chart={chart_result}, house={house}"
            )
        seen_observations.add(observation)
        by_subject[subject].append(row)

        if house not in EXPECTED_HOUSES:
            errors.append(f"subject={subject} has invalid house {house}")
        for column in FEATURE_COLUMNS:
            value = row.get(column)
            if value is None:
                errors.append(f"subject={subject}, house={house} has missing {column}")
            elif (not isinstance(value, (int, float)) or isinstance(value, bool)
                  or not 0 <= value <= 100):
                errors.append(f"subject={subject}, house={house} has invalid {column}={value}")

    for subject, subject_rows in by_subject.items():
        houses = {row.get("HouseFromLagna") for row in subject_rows}
        if len(subject_rows) != 12 or houses != EXPECTED_HOUSES:
            missing = sorted(EXPECTED_HOUSES - houses)
            errors.append(
                f"subject={subject} does not have one complete 12-house set; missing={missing}"
            )
        charts = {row.get("ChartResultId") for row in subject_rows}
        if len(charts) != 1:
            errors.append(f"subject={subject} spans {len(charts)} chart results")
    return ValidationReport(len(by_subject), len(rows), tuple(errors))
