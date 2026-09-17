---
last_updated: 2026-09-17
---

# BV Raman's 300 Important Combinations — status & plan

**Canonical source:** B. V. Raman, *300 Important Combinations* — cited as
`SRC_RAMAN_300_COMBINATIONS` (`docs/research/sources.md`; registered as a DJVU scan).
**OCR text extract used for this pass:** `D:\@ClaudeSpace\BookExtracts\300-important-combinations_p1-352_draft.md`
(not yet added to `sources.md`'s citation for `SRC_RAMAN_300_COMBINATIONS` — only the DJVU
is listed there today; worth registering once this extract is relied on further). Every
`SourceVariantCode` follows the `RAMAN_300_NNN` pattern with a `SourceLocator` of
`combination NNN; printed p.X; scan p.Y`.

This is Raman's own numbered collection — the `BVR-300` corpus referenced in
[`yoga-corpus.md`](yoga-corpus.md), which is a **separate** tracking document for PVR's
chapter-11 additions and the still-untranscribed PVR §11.7.3/11.8/11.9/11.10 unnamed
58-item list. That work is independent of this doc and not covered here.

## Snapshot (2026-09-17)

| Range | Combos | Predicate coverage | File(s) | Notes |
|---|---|---|---|---|
| 1–117 | 117 | 117/117 have a real predicate (1 partial clause gap) | `SourceAttributedYogaEngine.cs`, `VerifiedSourceYogaEngine.cs`, `MahabhagyaYogaEvaluator.cs`, `RamanContextYogaEvaluator.cs`, `MalikaYogaEvaluator.cs`, `RamanYogaBatchFourEvaluator.cs` .. `RamanYogaBatchSevenEvaluator.cs`, `RamanNabhasaBatchEvaluator.cs`, `RamanNabhasaSecondBatchEvaluator.cs` | see §1 |
| 118–143 (Dhana) | 26 | 26/26 | `RamanDhanaYogaEvaluator.cs` | closed 2026-09-17 (Vaiseshikamsa work) |
| 144–150 (Daridra) | 7 | 7/7 | `RamanDaridraYogaEvaluator.cs` | closed |
| 151–200 (Batch Eight) | 50 | 48/50 | `RamanYogaBatchEightEvaluator.cs` | 2 open, see §3 |
| 201–244, 264–300 (Final Hundred, remaining) | 81 | 0/81 | `RamanFinalHundredCatalog.cs` | pure catalog, no predicates — see §4 |
| 245–263 (Raja Yoga cluster) | 19 | 19/19 | `RamanRajaYogaEvaluator.cs` | closed 2026-09-17 — see §4 |
| **Total** | **300** | **217/300 (~72%)** | | |

Two very different reasons a combo can show `NOT_EVALUATED` for a given chart, and this
doc tracks only the first:

1. **No predicate exists** — the code gap this doc is about.
2. **A predicate exists but this specific chart lacks an input it needs** (no D9 supplied,
   birth-sex/day-night unknown, exact-degree longitude absent, Moon phase unresolved). This
   is expected per-record behaviour, not a gap — it resolves once the chart has that data.

## §1. Combos 1–117 — essentially complete

Every number 1–117 has at least one real coded predicate; several carry lettered
sub-variants (e.g. `051_A/B/C`, `079` has 8 house-start variants, `081`/`087`/`089` have
variants) that are all implemented too.

| Sub-range | File | Notes |
|---|---|---|
| 1–6 | `SourceAttributedYogaEngine.cs` | |
| 7–15 | `VerifiedSourceYogaEngine.cs` | |
| 16–24 | `SourceAttributedYogaEngine.cs` (19–23 refined by `VerifiedSourceYogaEngine.cs`) | |
| 25 | `MahabhagyaYogaEvaluator.cs` | gated on subject birth-sex being known — data-gate, not a code gap |
| 26–31 | `RamanContextYogaEvaluator.cs` | 28, 29 need D9 |
| 32–43 | `MalikaYogaEvaluator.cs` | separate `MalikaYogaResult` record, not `ContextualYogaResult` |
| 44–50 | `RamanYogaBatchFourEvaluator.cs` | 46 needs D9; 49 has `_CLASSICAL`/`_OBSERVED` variants |
| 51–60 | `RamanYogaBatchFiveEvaluator.cs` | 54 needs D9 on one variant; 58, 59 need exact-degree longitude |
| 61–70 | `RamanYogaBatchSixEvaluator.cs` | 62 needs D9; 66 needs D9 + day/night + waxing Moon; 68 needs D9 + full Moon |
| 71–80 | `RamanNabhasaBatchEvaluator.cs` | |
| 81–100 | `RamanNabhasaSecondBatchEvaluator.cs` | |
| 101–117 | `RamanYogaBatchSevenEvaluator.cs` | 113, 114 need D9; combo 111 has an open gap, below |

**Open gap — combo 111 (`YOGA_ROGAGRASTHA`), second alternative.** Code comment: *"The
second alternative requires an authoritative weak-ascendant-lord Shadbala threshold."*
**Requirement to close:** `SRC_RAMAN_GRAHA_BHAVA_BALAS` — the source this threshold would
come from — is a DJVU with no text extract (`gap-and-coverage.md` §4). Needs that DJVU
OCR'd (or an alternate registered source for the same threshold) before this clause can be
implemented; everything else in 1–117 is done.

## §2. Combos 118–150 (Dhana + Daridra) — complete

Both closed as of this session (2026-09-17).

- **Dhana (118–143, `YOGA_DHANA` + 3 named variants):** last 4 items (130, 137, 139, 140)
  needed Raman's own Vaiseshikamsa qualifier ("lord of Lagna... should join Vaiseshikamsa")
  — closed by building `VaiseshikamsaCalculator` (§3 below) and wiring it in. 5 more items
  (131, 133, 134, 141, 142) had been over-conservatively marked `NOT_EVALUATED` in an
  earlier pass despite being computable from a D9 dispositor chain, the project's existing
  Kalabala day/night rule, and the standard `Strong()`/`Favoured()` dignity convention —
  fixed the same session.
- **Daridra (144–150, `YOGA_DARIDRA`):** was already fully evaluated; no gaps found.

## §3. Combos 151–200 (Batch Eight) — 48/50, 2 open

Originally 15 of 50 were `NOT_EVALUATED`. 13 were closed this session: 6 (175, 176, 177,
182, 196, 197) turned out to be computable from existing primitives (sign-lordship
benefic/malefic classification — "cruel/benefic navamsa" just means the navamsa sign's
lord is a natural malefic/benefic — and the `Strong()` convention); 5 (155, 156, 167, 170,
171) needed the Vaiseshikamsa calculator; and 2 more (183, 185) closed right after, once the
calculator existed to extend:

- **183 (Yuddha Praveena Yoga)** — needed "majority of own Shadvarga" on a 3-hop
  navamsa-dispositor chain, a **different** varga grouping (6 charts) than Vaiseshikamsa's
  16. Closed by confirming Shadvarga-6 = D1, D2, D3, D9, D12, D30 (directly from
  `ShadbalaCalculator.cs`'s registered Saptavarga-7 list minus D7) and adding a
  `VaiseshikamsaCalculator.SwavargaCount(charts, planet, chartTypes)` overload taking an
  explicit chart-type list, exposed as the `Shadvarga` constant.
- **185 (Yuddhatpaschaddrudha Yoga)** — needed a benefic/cruel *nature* table for the 60
  named Shashtiamsa (D60) divisions. The table was already fully documented in
  `docs/cli/reading/d60-shashtiamsa.md` §4 ("Classical set per BPHS") but not yet in code;
  ported it into `ShashtiamsaDeityTable.cs` (name + nature by shashtiamsha number, with the
  odd/even-sign reversal rule from that doc's §2).

| Combo | Blocker | Status |
|---|---|---|
| 178 | OCR clause contradicts itself: the Definition's "(3rd, 5th or 7th)" parenthetical doesn't match the Remarks paragraph's "3rd, 4th and 7th" | genuinely blocked — needs visual adjudication of the original scan page (188–189), not OCR-fixable |
| 179 | Grammar doesn't cleanly map "Mercury, the lord of the 3rd and Mars" (3 subjects) onto "the 3rd house, the Moon and Saturn" (3 objects) — ambiguous which subject pairs with which object | genuinely blocked — same, scan page 184 |

## §4. Combos 201–300 — consolidated plan

### Current state

`RamanFinalHundredCatalog.cs` was a pure catalog of 61 `(Start, End, YogaCode, ScanPage)`
groups with zero predicates; **Cluster C (245–263, Raja Yoga) has been transcribed out of
it** into `RamanRajaYogaEvaluator.cs` (§4a below), leaving 81 combos across 60 groups still
in the catalog, each with the identical generic note — *"Source entry catalogued; predicate,
qualifications and chart requirements await visual verification."* Scan-page numbers are
recorded per remaining group, so each one can be located in the OCR draft without
re-deriving page numbers from scratch.

### Thematic clusters

The original 61 groups fell into four contiguous, thematically coherent blocks:

| Cluster | Range | Combos | Groups | Theme | Scan pages | Status |
|---|---|---|---|---|---|---|
| A | 201–219 | 19 | 13 | Siblings, deceit/curses on parents & children, vehicles, childlessness | 204–222 | open |
| B | 220–244 | 25 | 19 | Progeny count/quality, intelligence, spouses, fortune, father's early death | 222–248 | open |
| C | 245–263 | 19 | 1 (`YOGA_RAJA`) | Raja Yoga (royal/power) | 249–270 | **done** 2026-09-17 |
| D | 264–300 | 37 | 26 | Affliction, disease, deformity, manner of death, loss of status | 272–316 | open |

Full group-by-group scan-page breakdown for the three still-open clusters is in the code
comments of `RamanFinalHundredCatalog.cs`'s `Groups` table — use that directly rather than
duplicating it here (it would drift out of sync otherwise).

### §4a. Cluster C (245–263, Raja Yoga) — closed 2026-09-17

All 19 transcribed into `RamanRajaYogaEvaluator.cs` (`YOGA_RAJA`), each predicate quoting
Raman's Definition verbatim in a code comment with its exact scan-page locator (249–270,
recomputed per-combo from the OCR draft rather than reusing the catalog's single group-level
page). Two combos needed a source judgement call, both resolved from Raman's own text rather
than invented:

- **250** — the Definition itself offers two alternative readings of "the lord of the sign a
  planet is debilitated in, **or** the planet who would be exalted there." Raman's Remarks
  flag this as a known controversy ("thaduchchanatha") but settle it in his own worked
  example (Sun neecha in Libra; Saturn, who exalts in Libra, in a kendra) — implemented as
  both alternatives, matching the Definition's own "or".
- **253** — "Mercury... aspected... by the lord of the 11th" is auto-satisfied when Mercury
  himself is the 11th lord, per Raman's own worked-example note ("Since Mercury himself
  happens to be lord of the 11th, the yoga can be assumed to be present").

One new reusable primitive came out of this: `PvrDignityEvaluator.IsNaturalFriend(a, b)` —
exposes the project's existing (previously private) Parashari natural-friendship table,
needed for combo 263's "aspected by or associated with **friendly** planets" (a technical
term distinct from "benefic").

### Requirements (apply to every cluster)

1. **OCR retrieval per group.** Pull the exact Raman definition text for each group from
   `300-important-combinations_p1-352_draft.md` using the scan-page locator already
   recorded in code — the same method used for the Dhana/Batch-Eight closures this session.
2. **Dependency triage per group**, before writing any predicate: does it reuse an
   already-built primitive, or does it need new source work? Reusable primitives now
   available:
   - D9 navamsa-dispositor chain (used repeatedly across Dhana/Batch Eight/Raja)
   - `VaiseshikamsaCalculator` — Shodasavarga-16 *and* Shadvarga-6 own-sign count/grade
     (`SwavargaCount(charts, planet, chartTypes)` overload, `Shadvarga` constant)
   - `ShashtiamsaDeityTable` — D60 shashtiamsha number/name/benefic-malefic nature lookup
   - Kalabala day/night rule (`HasKalabala` in `RamanDhanaYogaEvaluator.cs`)
   - `Strong()`/`Favoured()` dignity convention (OWN/MOOLATRIKONA/EXALTED, optionally
     relationship-favoured)
   - `PvrDignityEvaluator.IsNaturalFriend(a, b)` — natural Parashari friendship, distinct
     from benefic/malefic
   - "cruel/benefic navamsa" = navamsa sign's lord is a natural malefic/benefic
   - deep-exaltation ("paramochha") exact-degree check (`AstroMath.DeepExaltationPoints`)
   - Digbala-house table (`Sun/Mars→10, Moon/Venus→4, Mercury/Jupiter→1, Saturn→7`, same as
     `ShadbalaCalculator.DigBalaHouses`)
   - `CombustionEngine.Evaluate(...)` for "free from combustion" clauses
   - reverse-exaltation lookup (`AstroMath.DeepExaltationPoints` inverted: sign → the planet
     that exalts there) for "the planet who would be exalted in that sign" clauses
   New source-backed concepts should only be invented when a group's text genuinely needs
   one — most groups in a numbered-combination block turn out to be a single lord
   placement/aspect conditional, architecturally simple compared to the named formations.
3. **Sensitive-content discipline.** Cluster D (and parts of A/B) covers disease, violent
   death, deformity, and moral/status claims. Per `yoga-corpus.md`'s existing policy: *"Keep
   disease, death, sex, caste, and moral claims neutral and source-attributed"* — quote
   Raman's own wording in `Notes`, never embellish or editorialize beyond it.
4. **OCR-ambiguity triage.** Batch Eight hit 2 genuinely unresolvable clauses out of 50
   (~4%) — expect a similar rate here. Defer and mark `NOT_EVALUATED` with an exact quote of
   the ambiguity rather than forcing an interpretation, same as combos 178/179.
5. **Test coverage.** Each new evaluator needs xUnit coverage on the established pattern —
   a positive-path chart fixture per predicate plus an absence/boundary case — following
   `RamanDhanaYogaEvaluatorTests.cs` / `RamanYogaBatchEightTests.cs`.
6. **Source citation.** Every new predicate: `SourceRefCode = SRC_RAMAN_300_COMBINATIONS`,
   `SourceVariantCode = RAMAN_300_NNN`, `SourceLocator = "combination NNN; printed p.X; scan
   p.Y"` — matching the convention already used throughout this corpus.

### Recommended phasing

1. ~~Cluster C (245–263, Raja Yoga, 19 items, single group)~~ — **done** 2026-09-17, see §4a.
2. **Cluster B (220–244, progeny/intelligence/fortune, 25 items)** — largest remaining item
   count; expect mostly simple lord-placement/aspect predicates in the Dhana/Daridra
   pattern. Next target.
3. **Cluster A (201–219, family/relationship, 19 items)** — moderate size; some groups
   (curses, deceit) may need careful neutral framing per requirement 3 above.
4. **Cluster D (264–300, affliction/disease/death-manner/status-loss, 37 items)** — largest
   and most sensitive remaining cluster; save for last so the neutral-framing discipline is
   applied with the most established precedent behind it.

This ordering is a recommendation, not a commitment — re-prioritize once OCR review of a
cluster reveals its actual complexity or blockers.

## §5. Cross-bucket requirements summary

| Need | Blocks | State |
|---|---|---|
| Shadvarga-6 `VaiseshikamsaCalculator` overload | combo 183 | **done** 2026-09-17 |
| D60 shashtiamsha nature lookup (`ShashtiamsaDeityTable.cs`) | combo 185 | **done** 2026-09-17 |
| OCR of `SRC_RAMAN_GRAHA_BHAVA_BALAS` (DJVU) or an alternate source for a weak-ascendant-lord Shadbala threshold | combo 111 (2nd alternative) | source-blocked, no text extract exists |
| Visual scan adjudication (not OCR-fixable) | combos 178, 179 | genuinely blocked |
| Full OCR pass + dependency triage across the remaining 60 groups | clusters A, B, D (81 combos) | cluster C (19 combos) **done** 2026-09-17; A/B/D not started |
| Register `300-important-combinations_p1-352_draft.md` as a citation source for `SRC_RAMAN_300_COMBINATIONS` in `docs/research/sources.md` | documentation hygiene, not a code blocker | only the DJVU is listed there today |
