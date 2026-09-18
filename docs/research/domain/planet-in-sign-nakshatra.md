---
last_updated: 2026-09-18
status: no-source-identified
---

# Planet-in-Sign / Planet-in-Nakshatra — sourcing status

Scaffolding only (migration 112): `research.tbl_Dim_SourceReferencePlanetInSign` (108 combos)
and `research.tbl_Dim_SourceReferencePlanetInNakshatra` (243 combos), same Dim→Text→Attribute→
Claim→Crosswalk shape as Planet-in-House (migration 059). **No Text/Attribute/Claim rows exist
for either** — unlike Planet-in-House, no source has been confirmed for this content yet.

Checked this session (2026-09-18) against the 4 currently-extracted books in
`D:\@ClaudeSpace\BookExtracts\`:

- `pvr-integrated-approach-raw.txt` — no dedicated planet-in-sign or planet-in-nakshatra
  chapter found (PVR's ch. 3 covers planet *characteristics* generally, not a sign-by-sign or
  nakshatra-by-nakshatra breakdown).
- `how-to-judge-a-horoscope-1.md` / `-2-ocr.md` — organized by house (see
  `docs/database/karakafix.md`-adjacent Planet-in-House work), not by sign or nakshatra.
- `300-important-combinations_p1-352_draft.md` — yoga combinations, not placement-by-placement
  significations.

**Do not seed Text rows citing a source that hasn't actually been checked.** When a source is
found (a Phaladeepika-style sign-by-sign treatise is the classical candidate for Planet-in-
Sign; nakshatra-specific planet effects are typically BPHS or Saravali material), add it here
first, then follow the same research→production pipeline `db/093`/`db/094` and the corrected
Planet-in-House pass (migration 111+) already established.
