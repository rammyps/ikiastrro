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
| Argala / Virodhargala | **Not built** | Flagged in `ROADMAP.md`'s Next bucket as `FEAT-RELATIONSHIP-04` ("Compound Maitrī, argala, sambandha") and in `pvr-coverage.md` ch.10 row. This note is direct design input for that feature slot — no separate "v5" tag needed (see the companion note on consolidating these). |

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

Positions, counted from the target house or the target karaka's own occupied sign:

| Argala | Position from target | Virodhargala | Position from target |
|---|---|---|---|
| Dhana | 2nd | (obstructs Dhana) | 12th |
| Sukha | 4th | (obstructs Sukha) | 10th |
| Labha | 11th | (obstructs Labha) | 3rd |
| Secondary | 5th | (obstructs Secondary) | 9th |

Design questions this raises, none resolved by anything currently in the codebase:

- **Dual entry point.** PVR's own worked example runs Argala against *both* the 10th house and
  Saturn (the karaka) as separate target signs — the engine needs to accept either a house
  number or a planet's current sign as "target," not just a house. `HouseEngine.GetHouseSign`
  already generalizes counting from any sign, so this is a parameter-shape decision, not a new
  counting primitive.
- **Occupants, not lords** — per the rule, count planets sitting in the Argala/Virodhargala
  sign, not the sign's lord. Straightforward once occupancy data is looked up per sign.
- **Strength comparison on opposing axes (2–12, 4–10, 11–3, 5–9)** — the rule requires comparing
  *number and strength* of occupants on each side, not just presence/absence. No scoring model
  exists yet for this. Recommend following the precedent in [[dignity-pvr]] (`DignityScore` /
  `RelationshipScore` kept as separate raw ordinals, blended only at interpretation time) rather
  than inventing a new composite number — e.g. count of occupants × (their `DignityScore` +
  functional-nature weight), summed per side, compared.
- **"Several malefics in the 3rd create their own Argala"** — an explicit classical exception
  (3rd-from-target malefics acting as Argala rather than Virodhargala under some threshold,
  typically ≥2 malefics per common Parāśari statements of this rule). Needs a specific citation
  and an explicit count threshold before encoding — flagged as `PROJECT_SYNTHESIS`-grade until
  sourced.
- **Ketu-occupied target → anti-zodiacal counting.** PVR's instruction to count backward when
  Ketu occupies the target sign has a loose precedent already in this codebase —
  `tbl_Rule_Karaka.ReverseForRahu` (chara karaka ordering) and the anti-zodiacal seed-counting
  note in `AstroMath.cs:142` (Ashtakavarga context) — but neither is the same rule. Whatever
  house-counting helper Argala uses needs its own Ketu special-case, not a reuse of either.
- **No citation pinned yet** for the Argala/Virodhargala position table itself beyond PVR's
  general treatment (referenced in `PVR_read_horoscope.md` step 5 and `pvr-coverage.md` ch.10,
  neither of which quotes page numbers) — get an exact page/section before seeding a
  `tbl_Rule_Argala`-style table, the way `dignity-pvr.md` pins Table 6 to a page.

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
final call by comparing strengths). Not encoded as a `CalculationNarrative` example anywhere —
do that once the engine exists and an exact PVR page citation is found for the Argala table.

## Open items before this becomes buildable

1. Pin an exact PVR page/section for the Argala/Virodhargala position table (currently only
   generally attributed).
2. Decide the occupant-strength scoring approach (reuse `DignityScore`-style raw ordinals).
3. Source and threshold the "malefics in 3rd become Argala" exception.
4. Extract a target-agnostic `DoesAspectSign` helper out of `RelationshipEngine` for graha
   dṛṣṭi-on-empty-sign and for Argala's own aspect checks, if Argala turns out to need graha
   dṛṣṭi as an input (PVR's framework keeps them conceptually separate, but confirm before
   assuming zero overlap).
5. Confirm target shape (house number **or** planet sign) for the Argala calculator's API.
