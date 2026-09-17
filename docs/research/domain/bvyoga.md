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
| 201–219 (siblings/curses/vehicles cluster) | 19 | 19/19 | `RamanFamilyYogaEvaluator.cs` | closed 2026-09-17 — see §4c |
| 220–244 (progeny/intelligence/fortune cluster) | 25 | 25/25 | `RamanProgenyYogaEvaluator.cs` | closed 2026-09-17 — see §4b |
| 245–263 (Raja Yoga cluster) | 19 | 19/19 | `RamanRajaYogaEvaluator.cs` | closed 2026-09-17 — see §4a |
| 264–300 (affliction/disease/death cluster) | 37 | 37/37 | `RamanAfflictionYogaEvaluator.cs` | closed 2026-09-17 — see §4d |
| **Total** | **300** | **298/300 (~99%)** | | |

**The 201–300 catalog is fully transcribed as of 2026-09-17** — `RamanFinalHundredCatalog.cs`
is now an empty stub. The only 2 combos in this entire 300-item corpus without any predicate
are 178 and 179 (§3, genuinely OCR-ambiguous); combo 111 (§1) has one of its two alternatives
implemented and one source-blocked.

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

## §4. Combos 201–300 — build record (complete)

### Final state

`RamanFinalHundredCatalog.cs` was a pure catalog of 61 `(Start, End, YogaCode, ScanPage)`
groups with zero predicates. **All four clusters have now been transcribed out of it**, into
`RamanFamilyYogaEvaluator.cs` (§4c), `RamanProgenyYogaEvaluator.cs` (§4b),
`RamanRajaYogaEvaluator.cs` (§4a) and `RamanAfflictionYogaEvaluator.cs` (§4d). The catalog
class itself is kept as an empty stub (`Groups = []`) rather than deleted, so
`ProductionYogaEngine.cs`'s call site and this doc's history stay intact.

### Thematic clusters

The original 61 groups fell into four contiguous, thematically coherent blocks, all closed
the same session:

| Cluster | Range | Combos | Groups | Theme | Scan pages | Status |
|---|---|---|---|---|---|---|
| A | 201–219 | 19 | 12 | Siblings, deceit/curses on parents & children, vehicles, childlessness | 204–220 | **done** 2026-09-17 |
| B | 220–244 | 25 | 21 | Progeny count/quality, intelligence, spouses, fortune, father's early death | 222–248 | **done** 2026-09-17 |
| C | 245–263 | 19 | 1 (`YOGA_RAJA`) | Raja Yoga (royal/power) | 249–270 | **done** 2026-09-17 |
| D | 264–300 | 37 | 26 | Affliction, disease, deformity, manner of death, loss of status | 272–316 | **done** 2026-09-17 |

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

### §4b. Cluster B (220–244, progeny/intelligence/fortune) — closed 2026-09-17

All 25 transcribed into `RamanProgenyYogaEvaluator.cs`, keeping each combo's own named
`YogaCode` from the catalog (`YOGA_BAHUPUTRA`, `YOGA_DATTAPUTRA`, `YOGA_APUTRA`, …) rather
than one umbrella code, since — unlike the Raja cluster — this block is 21 distinct named
yogas, not one grouped formation. One combo needed a source judgement call:

- **231 (Buddhimaturya Yoga)** — the literal Definition ("the 5th lord, being a benefic...")
  only works for 7 of 12 Lagnas, which Raman himself calls out as "preposterous" in his own
  Remarks, since it would imply people born in the other 5 Lagnas can never be intelligent.
  Implemented his own stated practical alternative instead: "the 5th house... is occupied by
  benefics and 5th lord is in association with Jupiter, Mercury and Venus" — irrespective of
  the 5th lord's natural classification.

Combo 234 (Thrikalagnana Yoga) doubled as an unplanned cross-check on `ShashtiamsaDeityTable`:
Raman spells out Mrudwamsa's shashtiamsha part number explicitly ("the 19th part... in an odd
sign or the 42nd... in an even sign"), and 19 ↔ 42 is exactly what the table's odd/even
reversal rule (`61 - number`) produces for the "Mridu" entry — confirming that rule against
an independent source statement, not just the D60 reading guide.

### §4c. Cluster A (201–219, siblings/curses/vehicles) — closed 2026-09-17

All 19 transcribed into `RamanFamilyYogaEvaluator.cs`, again keeping each combo's own named
`YogaCode` (`YOGA_SAHODAREE_SANGAMA`, `YOGA_KAPATA`, `YOGA_SARPASAPA`, …). Two notes:

- **204** cites "Mandi" in Raman's text, which is this engine's **"Maandi"** upagraha — a
  distinct point from Gulika (`UpagrahaCalculator.cs` computes both; JHora/PVR naming calls
  the start-of-arc point Gulika and the midpoint Maandi). Worth double-checking on any future
  combo that says "Mandi" — it is not a synonym for Gulika in this codebase.
- **216 (Pitrusapa Sutakshaya Yoga)** is the single most complex predicate transcribed so far
  in this corpus: Raman gives **six** alternative dispositions (three in the main Definition,
  three more introduced as "the following dispositions... also constitute Yoga No. 216").
  All six are unambiguous once separated out — implemented as an OR of all six, no source
  gaps. Raman's Remarks include a page of manual degree-arc arithmetic for locating one
  sub-condition's Navamsa boundary by hand; that arithmetic is irrelevant to the engine,
  which already carries the computed D9 chart and can just read the navamsa sign directly.

Cluster A used two more already-existing-but-newly-surfaced primitives: `TemporalFriendDistance`
(Tatkalika/temporary friendship — sign-distance 2/3/4/10/11/12 between two planets' current
placements, distinct from natural friendship) and a `CruelShashtiamsa` / `HemmedByMalefics`
(Papakartari) pair, both small local helpers rather than new shared infrastructure, since nothing
else has needed them yet.

### §4d. Cluster D (264–300, affliction/disease/death/status-loss) — closed 2026-09-17

All 37 transcribed into `RamanAfflictionYogaEvaluator.cs`, the largest and most sensitive
cluster, again keeping each combo's own named `YogaCode`. Per the project's sensitive-content
policy every predicate's code comment quotes Raman's own Definition wording rather than
editorializing. Notable points:

- **297 (Rajabhrashta Yoga)** needs the lords of "Aroodha Lagna" and "Aroodha Dwadasa" — these
  turned out to already exist as the `"AL"` and `"A12"` special points (gap-and-coverage.md's
  delivered "AL + 12 Arudhas"), so the predicate just reads their signs and finds the lords —
  no Arudha math needed in this file at all.
- **273 (Sirachcheda Yoga)** repeats the same kind of self-resolved ambiguity seen in Cluster
  C's combo 250: Raman quotes a debated Sanskrit line about whether "cruel shashtiamsa"
  applies to Rahu or to the planet conjoining Rahu, then settles it himself in the very next
  sentence ("the Sun or Saturn being in conjunction with Rahu should occupy a cruel
  shashtiamsa") — implemented exactly as he resolves it.
- **275 (Yuddhe Marana Yoga)** needed a new per-call chart lookup (D3, the Drekkana chart) for
  its stated alternate reading ("the lord of the drekkana occupied by Saturn..."); fetched
  directly from `bundle.Charts` inside the predicate rather than threading a new parameter
  through the evaluator's signature.
- **281 (Putrakalatraheena Yoga)** and **293 (Matibhramana, 3rd form)** both needed "waning
  Moon" — computed locally from Sun/Moon longitude elongation (`IsWaning`) rather than reusing
  any Moon-phase data from the pipeline, since this evaluator only receives the D1/D9 charts,
  not the full `ChartBundle`'s phase helpers.
- **264 (Galakarna Yoga)** cites a shashtiamsha division by name ("Preta Puriha") that isn't in
  the registered 60-name table — Raman's own Remarks give a generalized practical reading
  ("Rahu's disposition in the 3rd in a cruel shashtiamsa") used instead of guessing a name
  match.

With this, the entire Raman 201–300 catalog — all 100 numbers — has a real coded predicate.
Combined with §1–§3, **298 of the 300 combinations in this whole corpus now have a real
predicate**; only 178 and 179 (§3) remain with none at all, both genuinely OCR-ambiguous
rather than unattempted.

### Methodology notes (for reference — this cluster work is now complete)

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

### Phasing (as executed)

1. ~~Cluster C (245–263, Raja Yoga, 19 items, single group)~~ — **done** 2026-09-17, see §4a.
2. ~~Cluster B (220–244, progeny/intelligence/fortune, 25 items)~~ — **done** 2026-09-17, see
   §4b.
3. ~~Cluster A (201–219, family/relationship, 19 items)~~ — **done** 2026-09-17, see §4c.
4. ~~Cluster D (264–300, affliction/disease/death-manner/status-loss, 37 items)~~ — **done**
   2026-09-17, see §4d.

All four clusters closed in one session, in the order originally recommended.

## §5. Cross-bucket requirements summary — what's left in this corpus

Everything below is now genuinely blocked on outside source work, not on more engineering —
there is no more untranscribed material in Raman's 300 combinations.

| Need | Blocks | State |
|---|---|---|
| OCR of `SRC_RAMAN_GRAHA_BHAVA_BALAS` (DJVU) or an alternate source for a weak-ascendant-lord Shadbala threshold | combo 111 (2nd alternative) | source-blocked, no text extract exists |
| Visual scan adjudication (not OCR-fixable) | combos 178, 179 | genuinely blocked |
| Register `300-important-combinations_p1-352_draft.md` as a citation source for `SRC_RAMAN_300_COMBINATIONS` in `docs/research/sources.md` | documentation hygiene, not a code blocker | only the DJVU is listed there today |

Everything else this doc originally listed as a requirement (Shadvarga-6, D60 shashtiamsha
nature, natural-friendship exposure, Digbala table, combustion, reverse-exaltation lookup,
waning-Moon check, D3 lookup, Arudha points) is now built and used somewhere in this corpus —
see §2–§4d for where each one landed.
