"""Command-line validation for the v10 analytics cohort."""

from __future__ import annotations

import argparse

from .pipeline import build_comparisons
from .life_matter import build_comparisons as build_life_matter_comparisons
from .life_matter import validate_rows as validate_life_matter_rows
from .life_matter_sql import (
    ensure_development_dataset as ensure_life_matter_dataset,
    fetch_features as fetch_life_matter_features,
    publish_comparisons as publish_life_matter_comparisons,
)
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

def validate_life_matters() -> int:
    with connect() as connection:
        rows = fetch_life_matter_features(connection)
    report = validate_life_matter_rows(rows)
    print(f"eligible_subjects={report.subject_count} life_matter_rows={report.row_count}")
    if not report.is_valid:
        for error in report.errors:
            print(f"validation_error={error}")
        return 1
    print(f"percentiles_publishable={'yes' if report.subject_count >= 30 else 'no'}")
    return 0


def describe_life_matters(git_commit: str | None) -> int:
    seed = 20261001
    bootstrap_iterations = 2000
    with connect() as connection:
        rows = fetch_life_matter_features(connection)
        validate_life_matter_rows(rows).require_valid()
        dataset_id = ensure_life_matter_dataset(connection, git_commit=git_commit)
        comparisons = build_life_matter_comparisons(rows, seed=seed, bootstrap_iterations=bootstrap_iterations)
        run_id = publish_life_matter_comparisons(
            connection, dataset_id, comparisons, seed=seed,
            bootstrap_iterations=bootstrap_iterations)
    print(f"dataset_id={dataset_id} analytics_run_id={run_id} comparisons={len(comparisons)}")
    return 0




def main() -> int:
    parser = argparse.ArgumentParser(prog="ikiastrro-analytics")
    parser.add_argument(
        "command", choices=["prepare-personal-cohort", "validate-dataset", "describe",
                            "validate-life-matters", "describe-life-matters"])
    parser.add_argument("--git-commit")
    args = parser.parse_args()
    if args.command == "prepare-personal-cohort":
        return prepare()
    if args.command == "validate-dataset":
        return validate_dataset()
    if args.command == "validate-life-matters":
        return validate_life_matters()
    if args.command == "describe-life-matters":
        return describe_life_matters(args.git_commit)
    return describe(args.git_commit)


if __name__ == "__main__":
    raise SystemExit(main())
