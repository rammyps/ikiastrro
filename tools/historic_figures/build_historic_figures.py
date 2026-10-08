"""Build the historic-figures reference database (migration 179) from Wikidata.

  python build_historic_figures.py resolve   # seed.txt -> resolved.json  (Wikidata search + SPARQL; network)
  python build_historic_figures.py select    # resolved.json -> selected.json + report (49 cells x 10, geo-balanced)
  python build_historic_figures.py load      # selected.json -> tbl_Ref_HistoricFigure  (idempotent upsert)

Nothing is invented: names/years in seed.txt only *find and check* a Wikidata item; every stored date, place and
coordinate comes from that item. A candidate is accepted only if it is a human, its birth year is within one year of
the seed hint, and its birth date has day precision. Birth time is never stored (Wikidata has none we can trust).
"""

from __future__ import annotations

import json
import re
import sys
import time
import urllib.parse
import urllib.request
from collections import Counter, defaultdict
from datetime import date
from pathlib import Path

HERE = Path(__file__).parent
SEED, RESOLVED, SELECTED, REPORT = (HERE / n for n in ("seed.txt", "resolved.json", "selected.json", "report.md"))
UA = "ikiastrro-historic-figures/0.1 (research; rammyps23@gmail.com)"
GEO = {"IL": "IN_LOCAL", "IS": "IN_STATE", "IR": "IN_REGION", "IG": "IN_GLOBAL",
       "GR": "GL_REGION", "GC": "GL_CONT", "GG": "GL_GLOBAL"}
GEO_ORDER = list(GEO.values())
PER_CELL = 10
CAL_GREGORIAN, CAL_JULIAN = "http://www.wikidata.org/entity/Q1985727", "http://www.wikidata.org/entity/Q1985786"


def read_seed() -> list[dict]:
    rows = []
    for line in SEED.read_text(encoding="utf-8").splitlines():
        if not line.strip() or line.startswith("#"):
            continue
        area, sub, geo, name, year = line.split("|")
        rows.append({"area": area, "sub": sub, "geo": GEO[geo], "name": name, "year": int(year)})
    return rows


def http_json(url: str, data: bytes | None = None, accept: str = "application/json") -> dict:
    for attempt in range(4):
        try:
            request = urllib.request.Request(url, data=data, headers={"User-Agent": UA, "Accept": accept})
            with urllib.request.urlopen(request, timeout=60) as response:
                return json.load(response)
        except Exception:
            if attempt == 3:
                raise
            time.sleep(2 * (attempt + 1))
    raise RuntimeError("unreachable")


def search(name: str) -> list[str]:
    query = urllib.parse.urlencode({"action": "wbsearchentities", "search": name, "language": "en",
                                    "limit": 7, "format": "json"})
    return [hit["id"] for hit in http_json("https://www.wikidata.org/w/api.php?" + query).get("search", [])]


def julian_to_gregorian(y: int, m: int, d: int) -> date:
    a = (14 - m) // 12
    yy, mm = y + 4800 - a, m + 12 * a - 3
    jdn = d + (153 * mm + 2) // 5 + 365 * yy + yy // 4 - 32083          # Julian calendar date -> JDN
    return date.fromordinal(jdn - 1721425)                              # JDN -> proleptic Gregorian ordinal


def parse_time(value: str) -> tuple[int, int, int]:
    match = re.match(r"^(-?\d+)-(\d\d)-(\d\d)", value)
    return int(match[1]), int(match[2]), int(match[3])


def facts(qids: list[str]) -> dict[str, dict]:
    out: dict[str, dict] = {}
    for i in range(0, len(qids), 50):
        values = " ".join(f"wd:{q}" for q in qids[i:i + 50])
        sparql = f"""
        SELECT ?p ?pLabel ?desc ?dob ?prec ?cal ?pobLabel ?coord ?countryLabel ?sexLabel ?dod WHERE {{
          VALUES ?p {{ {values} }}
          ?p wdt:P31 wd:Q5 .
          ?p p:P569/psv:P569 ?n . ?n wikibase:timeValue ?dob ; wikibase:timePrecision ?prec ; wikibase:timeCalendarModel ?cal .
          OPTIONAL {{ ?p wdt:P19 ?pob . OPTIONAL {{ ?pob wdt:P625 ?coord }} OPTIONAL {{ ?pob wdt:P17 ?country }} }}
          OPTIONAL {{ ?p wdt:P21 ?sex }}
          OPTIONAL {{ ?p wdt:P570 ?dod }}
          OPTIONAL {{ ?p schema:description ?desc FILTER(lang(?desc) = "en") }}
          SERVICE wikibase:label {{ bd:serviceParam wikibase:language "en". }}
        }}"""
        result = http_json("https://query.wikidata.org/sparql", urllib.parse.urlencode({"query": sparql}).encode(),
                           "application/sparql-results+json")
        for row in result["results"]["bindings"]:
            qid = row["p"]["value"].rsplit("/", 1)[1]
            rec_prec = int(row["prec"]["value"])
            if qid in out and out[qid]["prec"] >= rec_prec:
                continue                                              # keep the most precise birth statement
            get = lambda k: row[k]["value"] if k in row else None      # noqa: E731
            out[qid] = {"name": get("pLabel"), "desc": get("desc"), "dob": get("dob"), "prec": int(get("prec")),
                        "cal": get("cal"), "place": get("pobLabel"), "coord": get("coord"),
                        "country": get("countryLabel"), "sex": get("sexLabel"), "dod": get("dod")}
        time.sleep(1)
    return out


def to_record(qid: str, f: dict) -> dict | None:
    if f["prec"] < 11:
        return None
    y, m, d = parse_time(f["dob"])
    calendar = "JULIAN" if f["cal"] == CAL_JULIAN else "GREGORIAN"
    try:
        born = julian_to_gregorian(y, m, d) if calendar == "JULIAN" else date(y, m, d)
    except ValueError:
        return None
    lat = lon = None
    if f["coord"]:
        m2 = re.match(r"Point\(([-\d.]+) ([-\d.]+)\)", f["coord"])
        if m2:
            lon, lat = float(m2[1]), float(m2[2])
    died = None
    if f["dod"]:
        try:
            dy, dm, dd = parse_time(f["dod"])
            died = date(dy, dm, dd).isoformat()
        except ValueError:
            pass
    place = f["place"] if f["place"] and not re.match(r"^Q\d+$", f["place"]) else None
    clock = None                                   # only when Wikidata itself stores an hour (precision 13+)
    if f["prec"] >= 13:
        t = re.search(r"T(\d\d):(\d\d):(\d\d)", f["dob"])
        if t and t[0] != "T00:00:00":
            clock = f"{t[1]}:{t[2]}:{t[3]}"
    return {"qid": qid, "name": f["name"], "description": f["desc"], "sex": f["sex"],
            "birth_year_raw": y, "dob": born.isoformat(), "time_utc_as_stored": clock,
            "calendar": calendar, "place": place,
            "country": f["country"], "lat": lat, "lon": lon, "dod": died}


def resolve() -> None:
    seed = read_seed()
    print(f"searching {len(seed)} seed names ...")
    hits = {}
    for i, row in enumerate(seed, 1):
        hits[row["name"]] = search(row["name"])
        if i % 50 == 0:
            print(f"  {i}/{len(seed)}")
        time.sleep(0.2)
    all_qids = sorted({q for qs in hits.values() for q in qs})
    print(f"fetching facts for {len(all_qids)} candidate items ...")
    fact = facts(all_qids)
    resolved, failed = [], []
    for row in seed:
        chosen = None
        for qid in hits[row["name"]]:
            f = fact.get(qid)
            if not f:
                continue
            record = to_record(qid, f)
            if record and abs(record["birth_year_raw"] - row["year"]) <= 1:
                chosen = record
                break
        if chosen:
            resolved.append({**row, **chosen})
        else:
            failed.append(row)
    RESOLVED.write_text(json.dumps({"resolved": resolved, "failed": failed}, ensure_ascii=False, indent=1), "utf-8")
    print(f"resolved {len(resolved)}, failed {len(failed)} -> {RESOLVED.name}")


def select() -> None:
    data = json.loads(RESOLVED.read_text("utf-8"))
    by_cell: dict[tuple[str, str], list[dict]] = defaultdict(list)
    for r in data["resolved"]:
        by_cell[(r["area"], r["sub"])].append(r)
    taken, geo_count, chosen = set(), Counter(), []
    short = []
    for cell in by_cell:                                              # seed order = Area/SubCategory order
        pool = [r for r in by_cell[cell] if r["qid"] not in taken]
        picked = []
        while pool and len(picked) < PER_CELL:
            best = min(pool, key=lambda r: (geo_count[r["geo"]], pool.index(r)))   # fill the thinnest GeoArea first
            pool.remove(best)
            picked.append(best)
            taken.add(best["qid"])
            geo_count[best["geo"]] += 1
        if len(picked) < PER_CELL:
            short.append((cell, len(picked)))
        chosen.extend(picked)
    SELECTED.write_text(json.dumps(chosen, ensure_ascii=False, indent=1), "utf-8")
    lines = [f"# Historic figures selection\n", f"Selected **{len(chosen)}** people in {len(by_cell)} cells "
             f"(target {PER_CELL} per cell).\n", "## GeoArea distribution\n", "| GeoArea | People |", "|---|---|"]
    lines += [f"| {g} | {geo_count[g]} |" for g in GEO_ORDER]
    lines += ["\n## Cells below target\n"]
    lines += [f"- {a}/{s}: {n}" for (a, s), n in short] or ["- none"]
    lines += ["\n## Seed names that did not resolve (no Wikidata match, wrong birth year, or no day-precise date)\n"]
    lines += [f"- {r['area']}/{r['sub']}: {r['name']} ({r['year']})" for r in data["failed"]] or ["- none"]
    lines += ["\n## Julian-calendar births converted to Gregorian (review these)\n"]
    lines += [f"- {r['name']}: {r['dob']}" for r in chosen if r["calendar"] == "JULIAN"] or ["- none"]
    lines += ["\n## Selected without a birthplace or coordinates\n"]
    lines += [f"- {r['name']}" for r in chosen if r["lat"] is None] or ["- none"]
    REPORT.write_text("\n".join(lines) + "\n", "utf-8")
    print(f"selected {len(chosen)}; short cells: {len(short)}; geo: {dict(geo_count)}")


def load() -> None:
    from ikiastrro_analytics.sql import connect
    people = json.loads(SELECTED.read_text("utf-8"))
    with connect() as connection:
        cursor = connection.cursor()
        for p in people:
            cursor.execute("""
                MERGE dbo.tbl_Ref_HistoricFigure AS t
                USING (SELECT ? AS WikidataQid) AS s ON t.WikidataQid = s.WikidataQid
                WHEN MATCHED THEN UPDATE SET Name=?, Description=?, Sex=?, DateOfBirth=?, OriginalCalendar=?,
                    BirthPlace=?, BirthCountry=?, Latitude=?, Longitude=?, DateOfDeath=?, AreaCode=?,
                    SubCategoryCode=?, GeoAreaCode=?, SourceUrl=?, RetrievedAtUtc=SYSUTCDATETIME()
                WHEN NOT MATCHED THEN INSERT (WikidataQid, Name, Description, Sex, DateOfBirth, OriginalCalendar,
                    BirthPlace, BirthCountry, Latitude, Longitude, DateOfDeath, AreaCode, SubCategoryCode,
                    GeoAreaCode, SourceUrl)
                    VALUES (?,?,?,?,?,?,?,?,?,?,?,?,?,?,?);
                """,
                p["qid"],
                p["name"], p["description"], p["sex"], p["dob"], p["calendar"], p["place"], p["country"],
                p["lat"], p["lon"], p["dod"], p["area"], p["sub"], p["geo"], f"https://www.wikidata.org/wiki/{p['qid']}",
                p["qid"], p["name"], p["description"], p["sex"], p["dob"], p["calendar"], p["place"], p["country"],
                p["lat"], p["lon"], p["dod"], p["area"], p["sub"], p["geo"], f"https://www.wikidata.org/wiki/{p['qid']}")
        connection.commit()
    print(f"upserted {len(people)} historic figures")


if __name__ == "__main__":
    {"resolve": resolve, "select": select, "load": load}[sys.argv[1] if len(sys.argv) > 1 else "resolve"]()
