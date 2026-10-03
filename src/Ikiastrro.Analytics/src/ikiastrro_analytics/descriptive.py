"""Pure, deterministic descriptive statistics; no astrology is recalculated here."""

from __future__ import annotations

from dataclasses import dataclass
import math
import random
import statistics
from typing import Iterable


@dataclass(frozen=True)
class Comparison:
    personal_value: float | None
    eligible_count: int
    measured_count: int
    missing_rate: float
    median: float | None
    q1: float | None
    q3: float | None
    mean: float | None
    standard_deviation: float | None
    robust_z: float | None
    percentile: float | None
    median_ci_low: float | None
    median_ci_high: float | None
    sufficiency: str


def _quantile(values: list[float], probability: float) -> float:
    """Linear interpolation using the (n-1)*p index (R/Python inclusive style)."""
    if len(values) == 1:
        return values[0]
    position = (len(values) - 1) * probability
    lower = math.floor(position)
    upper = math.ceil(position)
    if lower == upper:
        return values[lower]
    return values[lower] + (values[upper] - values[lower]) * (position - lower)


def _bootstrap_median_ci(values: list[float], iterations: int, seed: int) -> tuple[float, float]:
    rng = random.Random(seed)
    medians = sorted(
        statistics.median(rng.choices(values, k=len(values))) for _ in range(iterations)
    )
    return _quantile(medians, 0.025), _quantile(medians, 0.975)


def describe_subject(
    personal_value: float | None,
    reference_values: Iterable[float | None],
    *,
    seed: int = 20261001,
    bootstrap_iterations: int = 2000,
) -> Comparison:
    """Compare one subject with a leave-one-person-out reference group.

    The caller owns cohort selection and must omit the scored subject. Percentiles use
    the Phase 0 mid-rank tie rule and are suppressed below n=30.
    """
    raw = list(reference_values)
    values = sorted(float(value) for value in raw if value is not None)
    eligible = len(raw)
    measured = len(values)
    missing_rate = 0.0 if eligible == 0 else (eligible - measured) / eligible
    incomplete = missing_rate > 0.20
    if measured == 0:
        return Comparison(personal_value, eligible, 0, missing_rate, None, None, None,
                          None, None, None, None, None, None, "INCOMPLETE" if incomplete else "INSUFFICIENT")

    median = statistics.median(values)
    q1, q3 = _quantile(values, 0.25), _quantile(values, 0.75)
    mean = statistics.fmean(values)
    standard_deviation = statistics.pstdev(values)
    iqr = q3 - q1
    robust_z = None if personal_value is None or iqr == 0 else (float(personal_value) - median) / (iqr / 1.349)
    percentile = None
    if personal_value is not None and measured >= 30:
        below = sum(value < personal_value for value in values)
        equal = sum(value == personal_value for value in values)
        percentile = 100.0 * (below + 0.5 * equal) / measured

    ci_low, ci_high = _bootstrap_median_ci(values, bootstrap_iterations, seed)
    sufficiency = "INCOMPLETE" if incomplete else (
        "INSUFFICIENT" if measured < 30 else "EXPLORATORY" if measured < 100 else "SUFFICIENT"
    )
    return Comparison(personal_value, eligible, measured, missing_rate, median, q1, q3,
                      mean, standard_deviation, robust_z, percentile, ci_low, ci_high, sufficiency)
