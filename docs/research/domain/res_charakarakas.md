---
last_updated: 2026-09-17
status: outline
---

# Chara karaka deep-dive — research reading list + UX plan

Dictated outline (2026-09-17), captured verbatim-in-structure for follow-up research and
implementation. Nothing in this file is sourced/verified content yet — it is the reading
list and page plan for a future chara-karaka drill-down feature. Existing sourced research
on chara karakas lives in [chara-karaka-life-area-pvr.md](chara-karaka-life-area-pvr.md)
(PVR ch. 8, Table 13) and [naisargika-karaka-pvr.md](naisargika-karaka-pvr.md) — read those
first; this file is the next layer on top.

## 1. Background reading (to do)

For each of the 8 chara karakas — AK, AmK, BK, MK, PiK, PK, GK, DK — research:

- How the karaka *behaves* (temperament/role it plays for the native, not just the person
  it signifies — Table 13 in `chara-karaka-life-area-pvr.md` already covers "person shown"
  but not behavioral characteristics).
- What PVR says specifically, beyond Table 13 (ch. 8 pg 79–83 already mined; check for
  further chara-karaka treatment elsewhere in the book — e.g. karakamsa/Jaimini dasa
  chapters — not yet cross-checked).
- General characteristics/attributes per karaka (strength effects, house placement effects,
  sign placement effects) — source TBD, needs a citable reference before it goes in the DB
  (same bar `chara-karaka-life-area-pvr.md` already applies).

## 2. UX plan — "Life Matters" drill-down

Top-level page: **Life Matters**. Drill-down pattern, applied identically to all 8 chara
karakas:

```
Life Matters
  -> AK   -> (resolved planet for this chart, e.g. Rahu) -> life-matter interpretation
  -> AmK  -> (resolved planet)                            -> life-matter interpretation
  -> BK   -> ...
  -> MK   -> ...
  -> PiK  -> ...
  -> PK   -> ...
  -> GK   -> ...
  -> DK   -> ...
```

Each karaka resolves per-chart via `CharaKarakaCalculator` (already implemented — see
`chara-karaka-life-area-pvr.md`); the interpretation layer is the new part.

## 3. Key Inference pages (per-karaka detail view)

Two-page structure once a karaka is selected and resolved to a planet:

### Page 1 — Vargas

- The vargas (divisional charts) relevant to this karaka.
- The varga lord (sign lord of the karaka planet's position in each relevant varga).
- The varga lord's own sub-lord.

### Page 2 — Rasi / Nakshatra interactions

- How the varga lord interacts with Rasi (D-1 sign) and Nakshatra.
- Nakshatra Pada of the karaka planet.
- The Pada lord.
- SubLords L1–L7 (chain — **open question below**, need to pin down which scheme).

## 4. Chakras

- Each chara karaka has an associated "chakra" (wheel) and its signs.
- Naming convention given: `<KarakaAbbrev>_ChakraLord`, e.g. `AM_ChakraLord` for the
  Amatyakaraka chakra's lord.
- Apply the same pattern to all 8: `AK_ChakraLord`, `AmK_ChakraLord`, `BK_ChakraLord`,
  `MK_ChakraLord`, `PiK_ChakraLord`, `PK_ChakraLord`, `GK_ChakraLord`, `DK_ChakraLord`.

## Open questions (blockers before this becomes buildable)

- **"Chakra" concept is undefined in this codebase.** `grep -ri chakralord` across the repo
  returns zero hits — no prior art, no source citation. Is this a PVR/KP-sourced technique
  (name it) or a project-original construct being designed now? Needs a definition of what
  a per-karaka "chakra" *is* (a chart variant? a subset of signs? something else) before it
  can be researched or scoped for the DB/UI.
- **SubLords L1–L7 — which scheme?** Two candidates already exist in the codebase and they
  are not the same thing:
  - The KP 5-level sign/star/sub/sub-sub chain, partially modeled in
    `db/095_create_kp_sublord_chain_fact.sql` and `KpSubLordChainRepository.cs`.
  - House-cusp sub-lords numbered by house (1st cusp sub-lord … 7th cusp sub-lord), a
    different KP convention (significators per house).
  L1–L7 reads more like the second (7 houses), but confirm — the existing sublord-chain
  table may not extend to 7 levels as modeled.
- **`AM_ChakraLord` naming vs. the codebase's `AmK` enum.** `CharaKaraka.cs` uses `AmK` for
  Amatyakaraka (`AK, AmK, BK, MK, PiK, PK, GK, DK`). `AM` in the dictated naming is
  inconsistent with that — confirm before it lands in schema/UI: either the chakra-lord
  naming adopts `AmK_ChakraLord` for consistency, or there's a deliberate reason for the
  shorter `AM` form.
