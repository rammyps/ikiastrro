#!/usr/bin/env python3
"""Rank yoga variants for UI prominence and interpretation research.

The program uses descriptive statistics and TF-IDF semantic similarity. It does not claim
that a yoga predicts a life outcome; that requires an outcome-labelled, independently
validated chart cohort.
"""

from __future__ import annotations

import argparse
import csv
import json
import math
import re
import subprocess
from collections import Counter, defaultdict
from dataclasses import dataclass, asdict
from pathlib import Path
from typing import Iterable

TOKEN = re.compile(r"[a-z0-9]+")
STOP = {"yoga", "the", "and", "or", "of", "in", "a", "an", "from", "to", "with", "is", "by"}
SENSITIVE = {
    "death", "marana", "disease", "roga", "blind", "andha", "deform", "disability",
    "widow", "sex", "caste", "infertility", "childless", "curse", "sapa", "adultery",
}


@dataclass
class YogaRow:
    source_ref_code: str
    source_variant_code: str
    yoga_code: str
    yoga_set_code: str = ""
    yoga_set_name: str = ""
    evaluation_status: str = "EVALUATED"
    formation_rule: str = ""
    variant_notes: str = ""
    standard_text: str = ""
    short_text: str = ""
    evaluated_count: int = 0
    present_count: int = 0


def words(row: YogaRow) -> list[str]:
    text = " ".join((row.yoga_code, row.yoga_set_name, row.formation_rule, row.variant_notes))
    return [t for t in TOKEN.findall(text.lower().replace("yoga_", "")) if t not in STOP and len(t) > 1]


def tfidf(rows: list[YogaRow]) -> list[dict[str, float]]:
    documents = [Counter(words(row)) for row in rows]
    document_frequency = Counter(token for doc in documents for token in doc)
    count = max(len(rows), 1)
    vectors: list[dict[str, float]] = []
    for doc in documents:
        vector = {token: (1 + math.log(freq)) * (math.log((1 + count) / (1 + document_frequency[token])) + 1)
                  for token, freq in doc.items()}
        norm = math.sqrt(sum(value * value for value in vector.values())) or 1.0
        vectors.append({token: value / norm for token, value in vector.items()})
    return vectors


def cosine(left: dict[str, float], right: dict[str, float]) -> float:
    if len(left) > len(right):
        left, right = right, left
    return sum(value * right.get(token, 0.0) for token, value in left.items())


def wilson(successes: int, trials: int, z: float = 1.96) -> tuple[float, float]:
    if trials <= 0:
        return 0.0, 1.0
    p = successes / trials
    denominator = 1 + z * z / trials
    centre = p + z * z / (2 * trials)
    margin = z * math.sqrt((p * (1 - p) + z * z / (4 * trials)) / trials)
    return max(0.0, (centre - margin) / denominator), min(1.0, (centre + margin) / denominator)


class UnionFind:
    def __init__(self, size: int) -> None:
        self.parent = list(range(size))

    def find(self, value: int) -> int:
        while self.parent[value] != value:
            self.parent[value] = self.parent[self.parent[value]]
            value = self.parent[value]
        return value

    def union(self, left: int, right: int) -> None:
        left, right = self.find(left), self.find(right)
        if left != right:
            self.parent[right] = left


def rank(rows: list[YogaRow], similarity_threshold: float = 0.58) -> list[dict]:
    if not rows:
        return []
    vectors = tfidf(rows)
    union = UnionFind(len(rows))
    similarities: list[list[float]] = [[] for _ in rows]
    for left in range(len(rows)):
        for right in range(left + 1, len(rows)):
            score = cosine(vectors[left], vectors[right])
            if score >= similarity_threshold:
                union.union(left, right)
            if score >= 0.25:
                similarities[left].append(score)
                similarities[right].append(score)

    cluster_numbers: dict[int, int] = {}
    sources_by_concept: dict[str, set[str]] = defaultdict(set)
    for row in rows:
        sources_by_concept[row.yoga_code].add(row.source_ref_code)

    ranked = []
    for index, row in enumerate(rows):
        root = union.find(index)
        cluster_id = cluster_numbers.setdefault(root, len(cluster_numbers) + 1)
        lower, upper = wilson(row.present_count, row.evaluated_count)
        centrality = sum(sorted(similarities[index], reverse=True)[:5]) / 5
        readiness = {"EVALUATED": 1.0, "PARTIAL": 0.45}.get(row.evaluation_status, 0.1)
        interpreted = bool(row.standard_text.strip() and row.short_text.strip())
        cross_source = min((len(sources_by_concept[row.yoga_code]) - 1) / 2, 1.0)
        support = min(math.log1p(row.evaluated_count) / math.log(101), 1.0)
        semantic_text = " ".join(words(row))
        sensitive = any(term in semantic_text for term in SENSITIVE)

        ui_score = 100 * (
            0.34 * lower + 0.22 * readiness + 0.16 * centrality +
            0.12 * cross_source + 0.10 * support + 0.06 * interpreted
        )
        research_score = 100 * (
            0.30 * (not interpreted) + 0.20 * upper + 0.16 * centrality +
            0.14 * support + 0.12 * cross_source + 0.08 * (1 - readiness)
        )
        lane = ("SOURCE_ADJUDICATION" if readiness < 0.2 else
                "SENSITIVE_REVIEW" if sensitive else
                "INTERPRETATION" if not interpreted else
                "CROSS_SOURCE" if cross_source else "VALIDATION")
        ranked.append({
            **asdict(row),
            "prevalence": round(row.present_count / row.evaluated_count, 6) if row.evaluated_count else None,
            "wilson_lower": round(lower, 6),
            "wilson_upper": round(upper, 6),
            "semantic_centrality": round(centrality, 6),
            "semantic_cluster": cluster_id,
            "cross_source_count": len(sources_by_concept[row.yoga_code]),
            "has_interpretation": interpreted,
            "sensitive_review": sensitive,
            "research_lane": lane,
            "ui_priority_score": round(ui_score, 3),
            "research_priority_score": round(research_score, 3),
        })

    ui_order = {id(row): rank_no for rank_no, row in enumerate(
        sorted(ranked, key=lambda item: (-item["ui_priority_score"], item["source_variant_code"])), 1)}
    research_order = {id(row): rank_no for rank_no, row in enumerate(
        sorted(ranked, key=lambda item: (-item["research_priority_score"], item["source_variant_code"])), 1)}
    for row in ranked:
        row["ui_rank"] = ui_order[id(row)]
        row["research_rank"] = research_order[id(row)]
    return sorted(ranked, key=lambda item: item["research_rank"])


SQL = r"""
SET NOCOUNT ON;
SELECT v.SourceRefCode source_ref_code,v.SourceVariantCode source_variant_code,v.YogaCode yoga_code,
       v.YogaSetCode yoga_set_code,s.DisplayName yoga_set_name,v.EvaluationStatus evaluation_status,
       COALESCE(rSrc.ShortFormationRule,rGen.ShortFormationRule,'') formation_rule,
       COALESCE(v.Notes,'') variant_notes,COALESCE(i.StandardText,'') standard_text,
       COALESCE(i.ShortText,'') short_text,COALESCE(f.EvaluatedCount,0) evaluated_count,
       COALESCE(f.PresentCount,0) present_count
FROM dbo.tbl_Rule_YogaVariant v
JOIN dbo.tbl_Dim_YogaSets s ON s.Code=v.YogaSetCode
LEFT JOIN dbo.tbl_Rule_Yoga rSrc ON rSrc.RuleSetId=v.RuleSetId AND rSrc.YogaCode=v.YogaCode AND rSrc.SourceRefCode=v.SourceRefCode
LEFT JOIN dbo.tbl_Rule_Yoga rGen ON rGen.RuleSetId=v.RuleSetId AND rGen.YogaCode=v.YogaCode AND rGen.SourceRefCode IS NULL
OUTER APPLY(SELECT TOP(1) x.StandardText,x.ShortText FROM dbo.tbl_Content_Interpretation x
 WHERE x.RuleSetId=v.RuleSetId AND x.SubjectType='YOGA' AND x.SubjectCode=v.YogaCode AND x.IsActive=1
 AND (x.SourceRefCode=v.SourceRefCode OR x.SourceRefCode IS NULL)
 AND (x.SourceVariantCode=v.SourceVariantCode OR x.SourceVariantCode IS NULL)
 ORDER BY CASE WHEN x.SourceVariantCode=v.SourceVariantCode THEN 0 WHEN x.SourceRefCode=v.SourceRefCode THEN 1 ELSE 2 END) i
LEFT JOIN(SELECT RuleSetId,SourceRefCode,SourceVariantCode,
 COUNT(*) EvaluatedCount,SUM(CASE WHEN Present=1 THEN 1 ELSE 0 END) PresentCount
 FROM dbo.tbl_Fact_YogaInputEvaluations WHERE EvaluationStatus='EVALUATED'
 GROUP BY RuleSetId,SourceRefCode,SourceVariantCode) f
 ON f.RuleSetId=v.RuleSetId AND f.SourceRefCode=v.SourceRefCode AND f.SourceVariantCode=v.SourceVariantCode
WHERE v.IsActive=1
FOR JSON PATH;
"""


def from_database(server: str, database: str) -> list[YogaRow]:
    command = ["sqlcmd", "-S", server, "-E", "-d", database, "-y", "0", "-Q", SQL]
    completed = subprocess.run(command, check=True, capture_output=True, text=True, encoding="utf-8")
    payload = "".join(line.strip() for line in completed.stdout.splitlines() if line.strip())
    return [YogaRow(**item) for item in json.loads(payload)]


def from_csv(path: Path) -> list[YogaRow]:
    with path.open(encoding="utf-8-sig", newline="") as handle:
        rows = []
        for item in csv.DictReader(handle):
            item["evaluated_count"] = int(item.get("evaluated_count") or 0)
            item["present_count"] = int(item.get("present_count") or 0)
            rows.append(YogaRow(**{key: item.get(key, "") for key in YogaRow.__dataclass_fields__}))
        return rows


def write_outputs(rows: list[dict], output: Path) -> None:
    output.parent.mkdir(parents=True, exist_ok=True)
    output.with_suffix(".json").write_text(json.dumps(rows, indent=2, ensure_ascii=False), encoding="utf-8")
    if rows:
        with output.with_suffix(".csv").open("w", encoding="utf-8-sig", newline="") as handle:
            writer = csv.DictWriter(handle, fieldnames=rows[0].keys())
            writer.writeheader()
            writer.writerows(rows)
    lines = ["# Yoga priority report", "", "## Research priority", "",
             "| Rank | Variant | Set | Lane | Score | Present / evaluated | Interpretation |",
             "|---:|---|---|---|---:|---:|---|"]
    for row in rows[:100]:
        lines.append(f"| {row['research_rank']} | `{row['source_variant_code']}` | {row['yoga_set_name']} | "
                     f"{row['research_lane']} | {row['research_priority_score']:.3f} | "
                     f"{row['present_count']} / {row['evaluated_count']} | "
                     f"{'yes' if row['has_interpretation'] else 'no'} |")
    output.with_suffix(".md").write_text("\n".join(lines) + "\n", encoding="utf-8")


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    source = parser.add_mutually_exclusive_group(required=True)
    source.add_argument("--csv", type=Path, help="Input feature CSV")
    source.add_argument("--database", action="store_true", help="Read the local SQL Server database")
    parser.add_argument("--server", default=r"localhost\SQLSERVER2025")
    parser.add_argument("--db-name", default="ikiastrro")
    parser.add_argument("--output", type=Path, default=Path("artifacts/yoga-priority"))
    parser.add_argument("--similarity-threshold", type=float, default=0.58)
    args = parser.parse_args()
    rows = from_csv(args.csv) if args.csv else from_database(args.server, args.db_name)
    ranked = rank(rows, args.similarity_threshold)
    write_outputs(ranked, args.output)
    print(f"Ranked {len(ranked)} yoga variants -> {args.output.with_suffix('.csv')}")


if __name__ == "__main__":
    main()
