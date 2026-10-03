"""Command-line validation for the v10 analytics cohort."""

from __future__ import annotations

import argparse

from .pipeline import build_comparisons
from .sql import (connect, ensure_development_dataset, fetch_features,
                  prepare_personal_cohort, publish_comparisons)
from .validation import validate_feature_rows


def validate_dataset() -> int:
    with connect() as connection:
        rows = fetch_features(connection)
    report = validate_feature_rows(rows)
    print(f"eligible_subjects={report.subject_count} feature_rows={report.row_count}")
    if not report.is_valid:
        for error in report.errors:
            print(f"validation_error={error}")
        return 1
    publishable = report.subject_count >= 30
    print(f"percentiles_publishable={'yes' if publishable else 'no'}")
    return 0


def describe(git_commit: str | None) -> int:
    seed = 20261001
    bootstrap_iterations = 2000
    with connect() as connection:
        rows = fetch_features(connection)
        validate_feature_rows(rows).require_valid()
        dataset_id = ensure_development_dataset(connection, git_commit=git_commit)
        comparisons = build_comparisons(rows, seed=seed,
                                        bootstrap_iterations=bootstrap_iterations)
        run_id = publish_comparisons(connection, dataset_id, comparisons, seed=seed,
                                     bootstrap_iterations=bootstrap_iterations)
    print(f"dataset_id={dataset_id} analytics_run_id={run_id} comparisons={len(comparisons)}")
    return 0


def prepare() -> int:
    with connect() as connection:
        inserted = prepare_personal_cohort(connection)
    print(f"personal_subjects_added={inserted}")
    return 0



def main() -> int:
    parser = argparse.ArgumentParser(prog="ikiastrro-analytics")
    parser.add_argument(
        "command", choices=["prepare-personal-cohort", "validate-dataset", "describe"])
    parser.add_argument("--git-commit")
    args = parser.parse_args()
    if args.command == "prepare-personal-cohort":
        return prepare()
    return validate_dataset() if args.command == "validate-dataset" else describe(args.git_commit)


if __name__ == "__main__":
    raise SystemExit(main())
