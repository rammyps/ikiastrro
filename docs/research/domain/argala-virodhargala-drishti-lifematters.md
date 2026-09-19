# Argala/Virodhargala + Rāśi/Graha Dṛṣṭi — life-matter synthesis framework

Research note capturing rammyps's two-part framework (2026-09-18): (1) house+karaka selection
per life matter → Argala/Virodhargala on both, then (2) extending it with Rāśi dṛṣṭi and Graha
dṛṣṭi as two more "kinds of involvement." This is a **synthesis note**, not a book extract —
most of the individual mechanics below already have a cited source elsewhere in this repo
(linked per section); the *combination* into one four-relationship reading method and the
life-matter table are rammyps's own restatement of PVR's method, on the `SRC_IKIASTRRO_SYNTHESIS`
pattern already used for [[life-matter-reference-pvr]]. No new `SRC_*` code registered here.

## What's already built vs. what this note is designing for

| Piece | Status | Where |
|---|---|---|
| Rāśi dṛṣṭi | **Built** 2026-09-17 | `RasiDrishtiCalculator.cs` (movable↔fixed except adjacent, dual↔dual), backed by `db/107_create_rule_rasi_drishti.sql`. Cites PVR §10.3, `SRC_PVR_INTEGRATED`. |
| Graha dṛṣṭi | **Built** | `RelationshipEngine.cs`'s `AspectOffsets` (7th universal; Mars +4/8; Jupiter +5/9; Saturn +3/10) + `tbl_Rule_AspectOffset`. Cites `SRC_BPHS_26`. Today it only records aspects **planet→planet** (both ends occupied) — a planet aspecting an *empty* target sign/house isn't currently surfaced by this engine, which the Argala design below needs. |
| Life matter → house + karaka | **Built, more thoroughly than the pasted table** | `tbl_Rule_LifeMatterReference` (migration 087), 96 rows / 10 categories, see [[life-matter-reference-pvr]]. The 12-row table in this note's prompt is a coarser restatement of the same PVR method (§7.2 houses, ch.8 naisargika karakas) — reconcile against the 96-row table rather than re-seeding a second, less granular one. |
| Argala / Virodhargala | **Built** 2026-09-19 | `ArgalaCalculator.cs` (sign-based target, count comparator + dignity-sum tie-break, Ketu anti-zodiacal handling, 3rd-house malefic exception), backed by `db/127_create_rule_argala.sql` (`tbl_Rule_Argala`, 8 rows). Verified against Exercise 16/Chart 5 in `ArgalaCalculatorTests.cs` (8/8 pass). Was flagged in `ROADMAP.md`'s Next bucket as `FEAT-RELATIONSHIP-04` ("Compound Maitrī, argala, sambandha") and in `pvr-coverage.md` ch.10 row. |

**Correction made to `pvr-coverage.md` while researching this:** its ch.10 row read "rasi
drishti + argala not built" — stale as of the 2026-09-17 `RasiDrishtiCalculator` commit. Fixed
to say only argala/virodhargala remain unbuilt.

## The four relationships, and PVR's analogy

| Relationship | Represents | Built? |
|---|---|---|
| Rāśi dṛṣṭi | Structural/permanent influence ("continuing place in the ecosystem") | ✓ |
| Graha dṛṣṭi | A planet's desire/attention/intention | ✓ (planet→planet only, see gap above) |
| Argala | Decisive intervention that materially supports or redirects | ✗ |
| Virodhargala | Counter-intervention obstructing a specific Argala — not automatically negative | ✗ |

PVR's analogy, as given: karaka = project manager; house lord = manager responsible for the
matter; house occupants = people directly working on it; Rāśi dṛṣṭi = parties with continuing
influence; Graha dṛṣṭi = parties wishing to act; Argala = parties able to intervene decisively.

## 1. Choosing the life matter — house, lord, karaka, varga

For any life matter, the four things to gather are the house, its lord (`HouseEngine.GetSignLord`),
its natural/chara karaka (`tbl_Rule_Naisargika_Karakatwas` / `tbl_Rule_LifeMatterReference`), and
the relevant divisional chart (`tbl_Dim_LifeArea`). This is exactly the four-axis bridge
[[life-matter-reference-pvr]] already built for 96 matters — nothing new to design here. The
worked example's "10th house + Saturn (karaka) + Sun (authority) + Mercury (skills) + D-10" for
career is a `tbl_Rule_LifeMatterReference` lookup, not a new computation.

## 2. Rāśi dṛṣṭi — already computable end to end

`RasiDrishtiCalculator.Aspects(from, to)` gives the structural relationship directly. To read
"Jupiter's Rāśi dṛṣṭi on the 5th house": take Jupiter's occupied sign, call `Aspects(jupiterSign,
houseSign)`. Nothing to build; this is a read-path/UI gap (surfacing it in the interpretation
panel), not an engine gap.

## 3. Graha dṛṣṭi — engine exists, needs a target-agnostic form

`RelationshipEngine`'s current aspect pass only reports a hit when another **planet** occupies
the aspected sign (`target.Sign != aspectedSign` filter, `RelationshipEngine.cs:160`). Argala
and "graha dṛṣṭi on an empty house" both need the same offset logic reusable against **any**
sign, occupied or not — i.e., extracting `AspectOffsets` + `HouseEngine.GetHouseSign` into a
`DoesAspectSign(planet, fromSign, toSign)` helper that `RelationshipEngine` calls internally and
Argala can call independently. Small refactor, not a new rule set.

## 4. Argala / Virodhargala — design sketch (unbuilt)

**Citation pinned (2026-09-19):** `SRC_PVR_INTEGRATED`, §10.5 "Argala (Intervention)" /
§10.6 "Virodhargala (Obstruction)", book pp. 104–107 (raw extract lines 4051–4129 of
`pvr-integrated-approach-raw.txt`). Satisfies open item #1 below.

Positions, counted from the target house or the target karaka's own occupied sign:

| Argala | Position from target | Virodhargala | Position from target |
|---|---|---|---|
| Dhana | 2nd | (obstructs Dhana) | 12th |
| Sukha | 4th | (obstructs Sukha) | 10th |
| Labha | 11th | (obstructs Labha) | 3rd |
| Secondary | 5th | (obstructs Secondary) | 9th |

Benefic occupant of an argala position = *subhaargala*; malefic occupant = *paapaargala*.

### Exercise 16 (§10.6, pp.110–111) — worked example, Chart 5 (Lagna Scorpio)

The book's own answer-key table for this exercise is present in the raw `pdftotext -layout`
extract but badly scrambled by the source grid's column wrapping (not usable verbatim).
Reconstructed instead from Chart 5's planetary positions — derived from the book's Exercise 14
(graha-dṛṣṭi) answer table (each planet's own house is recoverable from its aspected-house set
and offset) plus the explicit "Ketu is in Aq" note in the text — then computed by applying the
§10.5–10.6 rule above. Cross-validated against the still-legible rows of the raw OCR (houses 1,
4, 10, 11, 12 matched exactly, including the malefic-in-3rd exception on house 11), so treat as
book-accurate despite not being a literal transcription.

**Chart 5 positions:** Mars & Saturn — Sc(1); Mercury — Sg(2); Venus — Cp(3); Ketu — Aq(4);
Moon — Ar(6); Sun — Ta(7); Rahu — Le(10); Jupiter — Vi(11). Houses 5, 8, 9, 12 empty.

| House | Argala‑2nd | Argala‑4th | Argala‑5th | Argala‑11th | Virodh‑12th | Virodh‑10th | Virodh‑9th | Virodh‑3rd |
|---|---|---|---|---|---|---|---|---|
| 1 (Sc) | Mercury | Ketu | — | Jupiter | — | Rahu | — | Venus |
| 2 (Sg) | Venus | — | Moon | — | Mars,Sat | Jupiter | Rahu | Ketu |
| 3 (Cp) | Ketu | Moon | Sun | Mars,Sat | Mercury | — | Jupiter | — |
| 4 (Aq)* | Venus | Mars,Sat | — | Moon | — | Sun | — | Mercury |
| 5 (Pi) | Moon | — | — | Venus | Ketu | Mercury | Mars,Sat | Sun |
| 6 (Ar) | Sun | — | Rahu | Ketu | — | Venus | Mercury | — |
| 7 (Ta) | — | Rahu | Jupiter | — | Moon | Ketu | Venus | — |
| 8 (Ge) | — | Jupiter | — | Moon | Sun | — | Ketu | Rahu |
| 9 (Cn) | Rahu | — | Mars,Sat | Sun | — | Moon | — | Jupiter |
| 10 (Le) | Jupiter | Mars,Sat | Mercury | — | — | Sun | Moon | — |
| 11 (Vi) | — | Mercury | Venus | — | Rahu | — | Sun | Mars,Sat → **argala** (exception) |
| 12 (Li) | Mars,Sat | Venus | Ketu | Rahu | Jupiter | — | — | Mercury |

\* House 4 uses anti-zodiacal (reverse) counting since Ketu occupies it (§10.6 NOTE). House 11's
3rd-from position holds 2 malefics (Mars+Saturn) — per the "several malefics in 3rd" exception
(§10.6, immediately after Exercise 16) this acts as argala, not virodhargala, on house 11.

Design questions this raised, all resolved 2026-09-19 and built into `ArgalaCalculator.cs`
(`src/Ikiastrro.Core/Engines/Houses/`) + `tbl_Rule_Argala` (migration 127):

- **Dual entry point → resolved: sign-based target.** `Evaluate(ZodiacName target, occupancy)`
  is the primary API (works for both a house's sign and a karaka's occupied sign, exactly as
  PVR's own worked example needs); `Evaluate(ZodiacName ascendantSign, int houseNumber,
  occupancy)` is a house-number convenience overload built on `HouseEngine.GetHouseSign`. No new
  counting primitive needed, as anticipated.
- **Occupants, not lords** — implemented directly: `ArgalaEvaluation.Argala`/`.Virodhargala`
  carry the actual occupant planets per position, looked up from the caller's own
  sign→occupants map.
- **Strength comparison → resolved: count first, dignity-sum tie-break, kept as separate raw
  fields.** `ArgalaCalculator.Compare(evaluation, dignityScoreLookup)` implements sec.10.7's own
  words literally — "see if more planets cause argala or virodhargala" is the primary
  comparator; PVR gives no formula for the "compare the strengths" tie-break, so the *aggregation
  method* (summing an injected `DignityScore` per side) is this project's own synthesis
  (`PROJECT_SYNTHESIS`), while the ingredient itself (`DignityScore`) reuses the already-sourced
  ordinal from [[dignity-pvr]] rather than inventing a new number. `dignityScoreLookup` is
  injected by the caller (not looked up internally) so the calculator itself stays
  degree-independent and pure, matching `RasiDrishtiCalculator`. The "functional-nature weight"
  idea floated below was dropped — not sourced anywhere, and sec.10.7's own text never goes
  beyond count + a qualitative "guess the meaning."
- **"Several malefics in the 3rd create their own Argala" → resolved,** see item 3 in Open
  items below (`>= 2`, sourced to Exercise 16's own house-11 answer).
- **Ketu-occupied target → anti-zodiacal counting → resolved,** implemented as its own
  `reverse` branch inside `ArgalaCalculator.CountFrom` (checks whether the target sign's
  occupants include Ketu) — a fresh implementation, not a reuse of `tbl_Rule_Karaka.ReverseForRahu`
  or the Ashtakavarga anti-zodiacal note, confirming the anticipation that neither was the same
  rule.
- ~~No citation pinned yet for the Argala/Virodhargala position table~~ **Resolved 2026-09-19**
  — §10.5–10.6, pp.104–107; see §4 above.

## Combined interpretation (reference table, no engine implication)

| Combination | Likely meaning |
|---|---|
| Rāśi dṛṣṭi only | Enduring background influence, limited active involvement |
| Graha dṛṣṭi only | Strong interest/intention, limited leverage |
| Argala only | Decisive practical intervention, even without a direct aspect |
| Rāśi + Graha dṛṣṭi | Permanent influence + active desire |
| Graha dṛṣṭi + Argala | Wants to act and has the means |
| Rāśi dṛṣṭi + Argala | Structurally connected and materially decisive |
| All three | Deeply embedded, actively motivated, able to determine outcomes |
| Strong Virodhargala | Intervention meets an organized counterforce (not inherently bad — can protect) |

## Planet-to-life-matter vocabulary

This table restates the same content already seeded in `tbl_Rule_Naisargika_Karakatwas`
(migration 086, see [[naisargika-karaka-pvr]]) in prose form for interpretation write-ups — not
new data, no separate table needed. Use the existing grid as the source of truth; this vocabulary
is a rendering convenience for narrative text generation only.

## Worked example (career) — reading-method illustration only

Kept here as PVR's illustrative walkthrough (10th house + Saturn; Mercury's 11th-from-Saturn
Argala → gains through analysis/communication/commerce; a 3rd-from-Saturn Mars → Virodhargala;
final call by comparing strengths). Not yet encoded as a `CalculationNarrative` example anywhere
— now that the engine exists (`ArgalaCalculator`) and the citation is pinned, this is a good
first `CalculationNarrative` candidate, but doing so is a separate content-authoring task, not
part of this build.

## Status: built 2026-09-19

`tbl_Rule_Argala` (migration 127, 8 rows, RuleSetId 1, `SRC_PVR_INTEGRATED`) + `ArgalaCalculator`
(`src/Ikiastrro.Core/Engines/Houses/ArgalaCalculator.cs`, pure C#, mirrors the rule table the same
way `RasiDrishtiCalculator` mirrors `tbl_Rule_RasiDrishti`) + `ArgalaCalculatorTests`
(`tests/Ikiastrro.Web.Tests/`, 8/8 pass, Exercise 16/Chart 5 verification fixture covering houses
1, 4, 10, 11, 12). `verify-rules` passes (no new failures; the 4 pre-existing failures it reports
are unrelated tables). All 5 items below are resolved.

**Fact table added, same day:** `tbl_Fact_Argala` (migration 128) — one row per occupant of an
occupied argala/virodhargala position, for either a house (1–12) or a planet's own occupied sign
as target (PVR's own dual usage, §10.7). Not cataloged in `tbl_Rule_Catalog` — that catalog is
`tbl_Rule_*`-only (`tbl_Fact_HouseFromReference`, migration 32, sets the precedent of leaving
Fact tables out of it). Filled by:
- `ArgalaFactBuilder` (`src/Ikiastrro.Core/Engines/Houses/`) — flattens `ArgalaCalculator`'s
  output into `ChartArgalaFact` rows for all 12 houses (`BuildForHouses`) and all 9 grahas'
  own occupied signs (`BuildForPlanets`); pure C#, no DB read, mirrors `HouseEngine.BuildHouseLords`.
- `ArgalaFactRepository` (`src/Ikiastrro.Data/`) — Dapper delete-then-reinsert per chart, same
  pattern as `BhavaStrengthRepository`.
- `dotnet run --project src/Ikiastrro.Cli -- backfill-argala` — reads `tbl_Chart_KeyDetails`
  Graha rows per D1 `tbl_ChartResults`, builds + persists facts. **Not yet wired into
  `ChartGenerationService`** (the live chart-generation/recompute pipeline), so it has to be
  re-run by hand for new charts — same status as `ArgalaCalculator` itself, just one layer up.

Run against the 3 real D1 charts in dev (`ChartResultId` 197/219/241): 120/104/124 fact rows
respectively (72 house rows each — 9 placed grahas × 8 positions, an exact-match sanity
invariant — plus 32–52 graha-target rows depending on how many of a chart's 9 planet targets'
8 positions land on an occupied sign). House-1 rows for chart 197 (Lagna Cancer) hand-verified
against the chart's actual positions — all 6 rows correct. 4–6 exception rows fired per chart
(the sec.10.6 "2+ malefics in 3rd" rule triggering on real data, not just the Chart 5 fixture).
`ArgalaFactBuilderTests` (4 tests, `tests/Ikiastrro.Web.Tests/`) cover the flattening logic,
including that targeting by a planet and by the house it occupies produce identical results.

**Noted in passing, not acted on:** `tbl_Planets.NaturalNature` already carries a sourced
Benefic/Malefic/Conditional classification (Moon and Mercury are `Conditional`, not flatly
Benefic) — more precise than `ArgalaCalculator.NaturalMalefics`' simplification (Moon/Mercury
always benefic, matching `LagnaFunctionalNature`'s same simplification). Resolving "Conditional"
needs paksha (Moon) and conjunction data (Mercury) that `ArgalaCalculator` doesn't take as input
today — a real follow-up if better precision is wanted, not done here to keep this build's scope
to what was asked.

1. ~~Pin an exact PVR page/section~~ **Resolved 2026-09-19** — §10.5–10.6, pp.104–107.
2. ~~Decide the occupant-strength scoring approach~~ **Resolved 2026-09-19** — count-first
   comparator (sourced to §10.7's own text) with an injected `DignityScore` sum as the tie-break
   (the aggregation method itself is `PROJECT_SYNTHESIS`, the ingredient is sourced); see the
   design-questions block above and `ArgalaCalculator.Compare`.
3. ~~Source and threshold the "malefics in 3rd become Argala" exception~~ **Resolved 2026-09-19**
   — sourced to §10.6 (same page as the position table, immediately after Exercise 16); text
   says "several malefics" without a numeric threshold, Exercise 16's own worked answer (house
   11) confirms **2 malefics is sufficient** to trigger it — encoded as `>= 2` in
   `ArgalaCalculator.Evaluate` and as `ExceptionMinMaleficCount = 2` on `tbl_Rule_Argala`'s
   3rd/VIRODHARGALA row.
4. Extract a target-agnostic `DoesAspectSign` helper out of `RelationshipEngine` for graha
   dṛṣṭi-on-empty-sign and for Argala's own aspect checks, if Argala turns out to need graha
   dṛṣṭi as an input (PVR's framework keeps them conceptually separate, and the built
   `ArgalaCalculator` did not end up needing it — leaving this open only if a future consumer
   needs graha dṛṣṭi and Argala combined).
5. ~~Confirm target shape~~ **Resolved 2026-09-19** — sign-based (`ZodiacName`) primary API,
   house-number convenience overload; see the design-questions block above.

**Not yet done** (separate follow-up work, not part of this build): wiring `ArgalaCalculator`
into the actual chart-analysis pipeline/UI (today it's a standalone, tested primitive with no
caller); the career worked-example `CalculationNarrative`; item 4 above if it turns out to be
needed.
