# Reference — PVR's Horoscope-Reading Path (Ch. 13 "Interpreting Charts")

**Source:** `SRC_PVR_INTEGRATED`, P.V.R. Narasimha Rao, *Vedic Astrology: An Integrated
Approach*, Chapter 13 "Interpreting Charts," §13.2–13.5, printed pp. 166–179. Evidence key
`PVR-H4` (`docs/research/sources.md`) already cites §13.4.1 pp. 169–170 for the core steps;
this file expands that into a full walkthrough.

**Relation to `method.md`:** [`method.md`](method.md) is this project's own JHora-anchored
*operational* loop (11 steps run against the engine's grid output). This file is the
**book's own path** — what PVR actually tells the reader to do, in his own order, before
any tool-specific adaptation. Read this first to understand *why* the project's loop is
shaped the way it is; read `method.md` for *how* to execute it against `ikiastrro`/JHora
output. Where the two diverge, `method.md` wins for engine behaviour and this file wins for
book fidelity — divergences should be recorded in `docs/research/domain/pvr-coverage.md`
Ch. 13 row, not silently here.

**As of:** 2026-09-13. **Status:** living.

---

## 0. What must already be in hand

PVR treats Chapter 13 as a *synthesis* chapter — it assumes chapters 1–12 (signs, planets,
dignity, upagrahas, special lagnas, divisional charts, houses, karakas, arudhas, aspects/
argalas) have already been computed for the chart. Nothing here replaces those; it tells you
how to combine their outputs.

---

## 1. The six-step path (§13.4.1 "Basic Guidelines")

This is PVR's own numbered list, printed almost verbatim. Run it **per question**, not once
per chart — a single horoscope answers many separate questions (career, marriage,
education, siblings, …), and each pass through the six steps is scoped to one matter.

1. **Pick the correct divisional chart for the matter of interest.**
   The varga is chosen by subject, not by convention alone — vehicle happiness → D16;
   a criminal's psychology → D30; marriage → D9 (or rasi, if the culture in question does
   not treat marriage as dharma/union of souls); religious activity → D20; learning → D24;
   career and societal achievement → D10.

2. **Pick the correct house inside that chart.**
   The right varga is not enough — the question must be narrowed to a house within it.
   Example PVR gives: inside D24 (learning), education is the 4th house, intelligence/
   scholarship/academic reputation/awards/students is the 5th, and how one interacts with
   others while pursuing knowledge is the 7th.

3. **Pick the correct reference point for counting houses.**
   Houses are always counted from *something* — but it need not be the chart's own lagna.
   Use lagna for the "true self" reading of a matter (e.g. intelligence/scholarship = 5th
   from lagna in D24). Use an arudha when the matter is about *perception* rather than
   substance (e.g. academic reputation = 5th from AL in D24, because reputation is how the
   world perceives the self). Use a strong karaka as the counting point when it outranks
   lagna for that sub-topic (scholarship = 5th from Mercury; intelligence = 5th from
   Jupiter; academic reputation = 5th from Sun, all still inside D24).

4. **Decide house vs. arudha for the matter itself, not just the reference.**
   Some matters are better represented by an arudha pada than by a house outright — e.g.
   what kind of people one deals with in one's learning life is Darapada (A7) in D24, not
   the 7th house; one's academic distinctions/awards are A5 (illusion/maya layered on
   intelligence and scholarship — the world's impression built from scores, ranks, grades),
   not the 5th house.

5. **Analyze the influences on the chosen house/arudha.**
   - Graha drishti and rasi drishti onto it.
   - Argala on it.
   - Relative-house reasoning *from* that house/arudha as the new reference point:
     planets in **quadrants** from it sustain the matter, in **trines** let it prosper, in
     **upachayas** let it grow, in **dusthanas** bring obstacles. (PVR's worked example:
     for an author's book-writing shown by A3 in D10 — a planet in a quadrant from A3 can
     bring book-writing during its periods; a planet in the 8th from A3 can bring
     obstacles to it; a baadhaka from A3 can create trouble around it.)
   - Whether any influencing planet is a **baadhaka** for that house/arudha (§13.3, below).

6. **Apply the standard placement results.**
   The classical literature (PVR names Dr. B.V. Raman's *How to Judge a Horoscope* Vols
   I–II, `SRC_RAMAN_HTJH` in this project) gives standard results for "planet X in house Y"
   and "lord of house Y in house Z." These should already be internalised — Ch. 13 does not
   restate them, it tells you when in the sequence to bring them in (last, once the right
   chart/house/reference/arudha/influence set is fixed — otherwise the standard result is
   answering the wrong question).

Throughout all six steps, also weigh:

- **Strength and avasthas** of the planets involved (dignity, combustion, retrograde,
  baladi/jagradadi states).
- **Ashtakavarga strength** of the house in question.
- **Presence of yogas** touching the house/arudha or its significators.

---

## 2. Two modifiers to fold into step 5/6

### 2.1 Functional nature (§13.2)

Before judging whether a planet's influence (step 5) or standard placement (step 6) is
good or bad, classify the planet **for this lagna**, not just by its fixed natural
character:

- **Natural** benefics: Jupiter, Venus, waxing Moon, well-associated Mercury.
  **Natural** malefics: Sun, Mars, Saturn, Rahu, Ketu, waning Moon, ill-associated Mercury.
- **Functional benefics:** lords of trines (1/5/9) from lagna.
- **Functional malefics:** lords of 3rd, 6th, 11th.
- **Quadrant (1/4/7/10) lords:** functionally malefic if the planet is a natural benefic;
  functionally neutral if the planet is a natural malefic.
- **2nd/8th/12th lords:** functionally neutral (8th more malefic than 2nd/12th).
- **Yogakaraka:** a single planet owning both a quadrant and a trine — an excellent planet
  for that lagna. (Full per-lagna table: PVR Table 30, printed p. 167.)
- A functional benefic in a quadrant/trine is good; a functional malefic there is bad
  unless very strong. A functional malefic in the 3rd or a dusthana (6/8/12) is *good* —
  it spoils a house that should be spoiled.
- **Yogada:** any planet that aspects, conjoins, or owns both Hora Lagna and lagna becomes
  a yogada (giver of luck) for money; the same relationship with Ghati Lagna and lagna
  makes it a yogada for power/authority — irrespective of its functional nature otherwise.
- Inherent (natural) vs. functional nature is PVR's own analogy: natural = whether a person
  is inherently good or bad; functional = whether that person's acts, in this context, help
  or harm you. A nice person can still harm you; a natural malefic can still do good here.

### 2.2 Baadhaka (§13.3)

For any house being read (not just lagna), find its **baadhaka sthaana** and **baadhaka**
lord before finishing step 5/6 — a planet or period tied to the baadhaka can obstruct that
house's matter even when everything else looks favourable:

| House's own modality | Baadhaka sthaana | 
|---|---|
| Movable (Ar/Cn/Li/Cp) | 11th from that house |
| Fixed (Ta/Le/Sc/Aq) | 9th from that house |
| Dual (Ge/Vi/Sg/Pi) | 7th from that house |

Full per-rasi lord table: PVR Table 31, printed p. 168–169. Apply this to *any* house or
arudha pada in *any* divisional chart — not only to lagna in D1 — exactly as step 5 uses it.

**Built:** `BaadhakaCalculator` (`src/Ikiastrro.Core/Engines/Houses/BaadhakaCalculator.cs`) —
`For(sign)` is Lagna-agnostic (reads Table 31 directly off any rasi); `For(lagnaSign,
houseNumber)` also reports which house from that Lagna the sthaana falls in. Verified against
all 12 rasis and both worked examples from this section (`dotnet run -- verify-baadhaka`).
Diverges from the book on the two rows where the sthaana lands in Aquarius/Scorpio — PVR's
Table 31 also names Rahu/Ketu there (matching his Table 6 co-ownership of those signs); this
project keeps the classical 7-planet-only rulership used everywhere else, so only Saturn/Mars
are returned (same divergence tracked for Ch. 3 in `docs/research/domain/pvr-coverage.md`).

---

## 3. Extending the path to other people (§13.4.2 "Family Members")

The six-step path also answers questions about people other than the native, by choosing a
different divisional chart and re-rooting the count:

| Person | Chart | Re-rooting rule |
|---|---|---|
| Parents, grandparents, uncles, aunts | D12 | Find the house showing that relative in D1 (e.g. 9th = father, 4th = mother); take the **rasi containing that house's lord** (in D12) as the new lagna for the relative, or use the corresponding D12 arudha pada. |
| Children, children-in-law, grandchildren | D7 | 5th house = first child; 7th (3rd-from-5th) = that child's next sibling. Count 5th, 7th, 9th, … lords as successive children, direction (forward/backward) set by whether D7 lagna is odd or even; after exhausting one parity, continue into the other rather than wrapping. |
| Brothers, sisters, in-laws | D3 | 3rd house = next younger sibling, 11th = next elder sibling; 5th/7th/9th… lords give 2nd/3rd/… younger or elder sibling by the same odd/even counting rule as D7. |
| Spouse and spouse's family | D9 | Read as the spouse's own chart, rooted the same way. |

This is the same six-step path — step 1 just resolves to a person-specific varga, and step
3's "reference point" becomes the re-rooted lagna for that relative rather than the native's
own lagna.

---

## 4. Worked shape (condensed from PVR's Examples 44–46)

PVR does not give a separate checklist for his worked examples — they apply §13.1–13.4
directly. The shape every example follows:

1. State what lagna (and lagna lord's placement) suggests about the person/matter in broad
   terms.
2. Walk the yogas touching lagna lord, the house lord relevant to the question, and the
   karaka — noting exaltation/Uttamaamsa, AL involvement, and any conjunction/aspect that
   forms a named yoga (raja yoga, Tapaswi yoga, Bhadra yoga, Budha-Aaditya yoga, etc.).
3. Re-run the same reading in the varga specific to the question (D20 for religious life,
   D24/D27 for learning and inherent nature, D3 for a sibling) — same six steps, new chart.
4. Reconcile: a strong D1 promise plus a strong varga confirmation is stated as the
   probable outcome; a conflicting signal (e.g. exalted yoga-forming lord *and* a
   renunciation indicator) is stated as a tension to be weighed, not silently dropped.
5. Cross-chart correlation between related natives (Example 45: Rajiv Gandhi's D3 vs. Sanjay
   Gandhi's own D1) is offered as a corroborating check, not a required step.

---

## 5. Quick checklist form

- [ ] Chart chosen for this matter (step 1)
- [ ] House chosen inside that chart (step 2)
- [ ] Reference point chosen — lagna / arudha / karaka (step 3)
- [ ] House vs. arudha decided for the matter itself (step 4)
- [ ] Influences read: graha + rasi drishti, argala, quadrant/trine/upachaya/dusthana from
      the house or arudha, baadhaka check (step 5, §13.3)
- [ ] Functional nature applied to every influencing/owning planet for this lagna (§13.2)
- [ ] Standard placement results applied last (step 6)
- [ ] Strength/avastha, Ashtakavarga, and yogas on the house/significators weighed in
- [ ] If reading for a relative: chart and re-rooted reference chosen per §13.4.2 table

---

## 6. Open reconciliation

Per `docs/research/domain/pvr-coverage.md` Ch. 13 row: this project's `method.md` loop is
recorded as **partial** against §13 — it was written JHora-first and has not yet been
checked step-for-step against this path. Known gaps to reconcile:

- `method.md` does not yet separate "functional nature" (§13.2) or "baadhaka" (§13.3) into
  their own numbered steps — they are folded implicitly into its step 4 (dignity) and step 6
  (yoga), which is looser than PVR's explicit two-part treatment here.
- `method.md`'s step 3 (varga lagna) and step 7 (house-by-house checklist) do not yet state
  PVR's step 3/4 distinction (lagna vs. arudha vs. karaka as the counting reference; house
  vs. arudha as the matter itself) as generally as this file does.
- Rasi drishti and argala are flagged **not built** in the engine (`pvr-coverage.md` Ch. 10
  row) — step 5 above cannot yet be run in full against computed output, only graha drishti
  and baadhaka (`BaadhakaCalculator`, §2.2) are.
- §13.4.2 family-member re-rooting has no CLI/engine support yet — it is a manual reading
  technique only at this time.
