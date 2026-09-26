#!/usr/bin/env python3
"""Generate seven ranked LifeMatter paths for every yoga source variant."""

from __future__ import annotations

import argparse
import json
import math
import re
import subprocess
from collections import Counter
from pathlib import Path

TOKEN = re.compile(r"[a-z0-9]+")
STOP = {"yoga", "the", "and", "or", "of", "in", "a", "an", "from", "to", "with", "is", "by"}
EXPAND = {
    "dhana": "wealth income assets prosperity family",
    "daridra": "poverty debt loss hardship wealth",
    "raja": "career status power authority fame profession",
    "roga": "health disease illness body treatment",
    "marana": "longevity death danger health",
    "putra": "children progeny intelligence education",
    "matru": "mother home property peace",
    "pitru": "father ancestry fortune dharma",
    "bhratru": "siblings courage communication",
    "kalatra": "spouse marriage relationship partnership",
    "vidya": "education learning knowledge scholarship",
    "buddhi": "intelligence learning education memory",
    "vah": "vehicle travel mobility property",
    "griha": "home property real estate residence",
    "bhagya": "fortune dharma father pilgrimage",
    "karma": "career profession work status",
    "moksha": "spirituality liberation meditation",
}

# Transparent domain priors prevent incidental shared words from outranking a
# yoga family's canonical LifeMatter. Generated rows remain PROPOSED for review.
AREA_PRIORS = {
    "dhana": ("wealth", "financial", "income", "prosperity", "assets"),
    "daridra": ("wealth", "financial", "debt", "loss", "hardship"),
    "raja": ("career", "status", "authority", "power", "fame"),
    "roga": ("health", "disease", "illness", "treatment"),
    "marana": ("longevity", "death", "danger", "health"),
    "putra": ("children", "progeny", "education", "intelligence"),
    "matru": ("mother", "home", "property"),
    "pitru": ("father", "ancestry", "fortune", "dharma"),
    "bhratru": ("sibling", "courage", "communication"),
    "kalatra": ("spouse", "marriage", "relationship", "partnership"),
    "vidya": ("education", "learning", "knowledge"),
    "moksha": ("spiritual", "liberation", "meditation"),
}

YOGA_SQL = r"""
SET NOCOUNT ON;
SELECT v.Id yoga_variant_id,v.RuleSetId rule_set_id,v.SourceRefCode source_ref_code,
 v.SourceVariantCode source_variant_code,v.YogaCode yoga_code,v.YogaSetCode yoga_set_code,
 s.DisplayName yoga_set_name,COALESCE(r1.ShortFormationRule,r0.ShortFormationRule,'') formation_rule,
 COALESCE(v.Notes,'') notes
FROM dbo.tbl_Rule_YogaVariant v JOIN dbo.tbl_Dim_YogaSets s ON s.Code=v.YogaSetCode
LEFT JOIN dbo.tbl_Rule_Yoga r1 ON r1.RuleSetId=v.RuleSetId AND r1.YogaCode=v.YogaCode AND r1.SourceRefCode=v.SourceRefCode
LEFT JOIN dbo.tbl_Rule_Yoga r0 ON r0.RuleSetId=v.RuleSetId AND r0.YogaCode=v.YogaCode AND r0.SourceRefCode IS NULL
WHERE v.IsActive=1 FOR JSON PATH;
"""

FOCUS_SQL = r"""
SET NOCOUNT ON;
SELECT f.Id focus_id,f.RuleSetId rule_set_id,f.LifeMatterId life_matter_id,f.Priority focus_priority,
 COALESCE(r.CategoryName,lm.CategoryName,'General') area,
 COALESCE(r.MatterText,lm.EnglishName) sub_area1,COALESCE(ds.SubjectName,'Natal promise') sub_area2,
 f.FocusKind sub_area3,f.ReferenceCode sub_area4,
 CASE WHEN f.FocusKind='House' THEN CONCAT('House ',f.HouseNumber) ELSE COALESCE(f.SpecialPointCode,f.ReferenceCode) END sub_area5,
 COALESCE(k.Karakas,'No specific karaka') sub_area6
FROM dbo.tbl_Rule_LifeMatterFocus f JOIN dbo.tbl_Dim_LifeMatter lm ON lm.Id=f.LifeMatterId
LEFT JOIN dbo.tbl_Rule_LifeMatterReference r ON r.RuleSetId=f.RuleSetId AND r.LifeMatterId=lm.Id AND r.IsActive=1
LEFT JOIN dbo.tbl_Rule_LifeMatterSubject ls ON ls.RuleSetId=f.RuleSetId AND ls.LifeMatterId=lm.Id AND ls.IsActive=1
LEFT JOIN dbo.tbl_Dim_DivisionalSubject ds ON ds.SubjectCode=ls.DivisionalSubjectCode
OUTER APPLY(SELECT STRING_AGG(z.KarakaName,', ') Karakas FROM
 (SELECT DISTINCT CASE WHEN kr.KarakaTypeCode='CHARA' THEN kr.CharaKarakaCode ELSE p.PlanetName END KarakaName
  FROM dbo.tbl_Rule_KarakaMatter km JOIN dbo.tbl_Dim_KarakaRole kr ON kr.Id=km.KarakaRoleId
  LEFT JOIN dbo.tbl_Planets p ON p.Id=kr.FixedGrahaId
  WHERE km.RuleSetId=f.RuleSetId AND km.LifeMatterId=f.LifeMatterId AND km.IsActive=1) z) k
WHERE f.IsActive=1 FOR JSON PATH;
"""


def query_json(server: str, database: str, sql: str) -> list[dict]:
    command = ["sqlcmd", "-S", server, "-E", "-d", database, "-y", "0", "-Q", sql]
    result = subprocess.run(command, check=True, capture_output=True, text=True, encoding="utf-8")
    output = "".join(line.strip() for line in result.stdout.splitlines() if line.strip())
    payload = output[output.find("["):output.rfind("]") + 1]
    return json.loads(payload)


def tokens(text: str) -> list[str]:
    base = [word for word in TOKEN.findall(text.lower().replace("_", " ")) if word not in STOP]
    expanded = list(base)
    for word in base:
        for key, addition in EXPAND.items():
            if key in word:
                expanded.extend(addition.split())
    return expanded


def vectors(documents: list[str]) -> list[dict[str, float]]:
    counts = [Counter(tokens(document)) for document in documents]
    frequency = Counter(token for count in counts for token in count)
    total = len(documents)
    result = []
    for count in counts:
        vector = {token: (1 + math.log(value)) * (math.log((1 + total) / (1 + frequency[token])) + 1)
                  for token, value in count.items()}
        norm = math.sqrt(sum(value * value for value in vector.values())) or 1
        result.append({token: value / norm for token, value in vector.items()})
    return result


def cosine(left: dict[str, float], right: dict[str, float]) -> float:
    if len(left) > len(right):
        left, right = right, left
    return sum(value * right.get(token, 0) for token, value in left.items())


def domain_prior(yoga: dict, focus: dict) -> float:
    yoga_text = " ".join(str(yoga.get(key, "")) for key in
                         ("yoga_code", "yoga_set_code", "yoga_set_name")).lower()
    focus_text = " ".join(str(focus.get(key, "")) for key in
                          ("area", "sub_area1")).lower()
    matched = [terms for marker, terms in AREA_PRIORS.items() if marker in yoga_text]
    if not matched:
        return 0.0
    return 1.0 if any(term in focus_text for terms in matched for term in terms) else 0.0


def generate(yogas: list[dict], foci: list[dict]) -> list[dict]:
    yoga_docs = [" ".join(str(y.get(key, "")) for key in
                 ("yoga_code", "yoga_set_name", "formation_rule", "notes")) for y in yogas]
    focus_docs = [" ".join(str(f.get(key, "")) for key in
                  ("area", "sub_area1", "sub_area2", "sub_area3", "sub_area4", "sub_area5", "sub_area6")) for f in foci]
    all_vectors = vectors(yoga_docs + focus_docs)
    yoga_vectors = all_vectors[:len(yogas)]
    focus_vectors = all_vectors[len(yogas):]
    rows = []
    for yoga, yoga_vector in zip(yogas, yoga_vectors):
        candidates = []
        for focus, focus_vector in zip(foci, focus_vectors):
            if yoga["rule_set_id"] != focus["rule_set_id"]:
                continue
            semantic = cosine(yoga_vector, focus_vector)
            priority = 1 / max(int(focus.get("focus_priority", 1)), 1)
            relevance = min(1.0, 0.5 * semantic + 0.4 * domain_prior(yoga, focus) + 0.1 * priority)
            candidates.append((relevance, semantic, priority, focus))
        candidates.sort(key=lambda item: (-item[0], item[3]["focus_id"]))
        for rank, (relevance, semantic, priority, focus) in enumerate(candidates[:7], 1):
            rows.append({**yoga, **focus, "path_rank": rank,
                         "relevance_score": round(relevance, 6),
                         "semantic_score": round(semantic, 6),
                         "priority_score": round(min(priority, 1.0), 6)})
    return rows


def sql_literal(value: str) -> str:
    return "N'" + value.replace("'", "''") + "'"


def write_sql(rows: list[dict], path: Path) -> None:
    lines = ["USE [ikiastrro];", "SET NOCOUNT ON;", "BEGIN TRANSACTION;",
             "DELETE FROM dbo.tbl_Rule_YogaLifeMatterPath WHERE ReviewStatusCode='PROPOSED';"]
    for row in rows:
        note = f"Area={row['area']}; SA1={row['sub_area1']}"
        lines.append(f"""MERGE dbo.tbl_Rule_YogaLifeMatterPath AS t
USING(SELECT {row['rule_set_id']} RuleSetId,{row['yoga_variant_id']} YogaVariantId,{row['path_rank']} PathRank,{row['focus_id']} LifeMatterFocusId) s
ON t.RuleSetId=s.RuleSetId AND t.YogaVariantId=s.YogaVariantId AND t.PathRank=s.PathRank
WHEN MATCHED AND t.ReviewStatusCode='PROPOSED' THEN UPDATE SET LifeMatterFocusId=s.LifeMatterFocusId,
 RelevanceScore={row['relevance_score']},SemanticScore={row['semantic_score']},PriorityScore={row['priority_score']},
 MappingMethodCode='TFIDF_V1',Notes={sql_literal(note)},UpdatedAtUtc=SYSUTCDATETIME()
WHEN NOT MATCHED THEN INSERT(RuleSetId,YogaVariantId,PathRank,LifeMatterFocusId,RelevanceScore,SemanticScore,PriorityScore,MappingMethodCode,ReviewStatusCode,Notes)
 VALUES(s.RuleSetId,s.YogaVariantId,s.PathRank,s.LifeMatterFocusId,{row['relevance_score']},{row['semantic_score']},{row['priority_score']},'TFIDF_V1','PROPOSED',{sql_literal(note)});""")
    lines.extend(["IF EXISTS(SELECT 1 FROM dbo.vw_YogaLifeMatterMatrixCoverage WHERE PathCount<>7) THROW 50149,'Yoga LifeMatter matrix is incomplete.',1;",
                  "COMMIT TRANSACTION;"])
    path.write_text("\n".join(lines) + "\n", encoding="utf-8")


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--server", default=r"localhost\SQLSERVER2025")
    parser.add_argument("--database", default="ikiastrro")
    parser.add_argument("--output", type=Path, default=Path("artifacts/yoga-lifematter-7x7"))
    parser.add_argument("--apply", action="store_true")
    args = parser.parse_args()
    yogas = query_json(args.server, args.database, YOGA_SQL)
    foci = query_json(args.server, args.database, FOCUS_SQL)
    rows = generate(yogas, foci)
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.with_suffix(".json").write_text(json.dumps(rows, indent=2, ensure_ascii=False), encoding="utf-8")
    sql_path = args.output.with_suffix(".sql")
    write_sql(rows, sql_path)
    if args.apply:
        subprocess.run(["sqlcmd", "-S", args.server, "-E", "-d", args.database, "-b", "-i", str(sql_path)], check=True)
    print(f"Generated {len(rows)} paths for {len(yogas)} yoga variants ({len(rows)//max(len(yogas),1)} per yoga).")


if __name__ == "__main__":
    main()
