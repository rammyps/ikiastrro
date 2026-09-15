---
last_updated: 2026-09-14
---

# Karaka types, and how they connect to Life Area / divisional charts — PVR ch. 6 & ch. 8

Research pass triggered by rammyps's "which chara karaka goes with which divisional
chart" worksheet (2026-09-14). Purpose: understand the three karaka *types* PVR defines,
what each is *for*, and — the thing the worksheet actually needs — how (or whether) any
of them formally connects to `tbl_Dim_LifeArea` (PVR Table 11, migration 30) or to
`tbl_Rule_LifeMatterReference` (migration 087). No schema change in this pass; this is
the groundwork for one.

## The three karaka types (PVR ch. 8, pg 79–83)

PVR is explicit that these are three **different tools for different questions**, not
interchangeable labels for "significator":

| Type | What it answers | How it's derived | Presided by |
|---|---|---|---|
| **Naisargika** (natural) | General Phalita Jyotish — "what does this planet always signify" | Fixed: same planet→matter mapping for everyone | — |
| **Sthira** (fixed/relative) | Timing the *death* of a specific relative | Fixed planet per relative, with a same/opposite-sex tiebreak (e.g. Sun-or-Venus, whichever stronger, for father) | Shiva (death) |
| **Chara** (movable) | A person's role in *sustenance, achievements, and spiritual evolution*; also carries karma across lifetimes | Computed per chart: rank the 8 grahas (Sun..Saturn, Rahu; Ketu excluded) by degree-advancement within their sign, highest→lowest | Vishnu (sustenance) |

Key distinction PVR states twice (pg 82–83), because it's the one people get wrong: for
naisargika **and** sthira karakas, the planet stands for the relative's physical body —
but a *house counted from* the planet answers most actual questions (e.g. Jupiter is a
female native's sthira karaka for husband's death, but the 7th-from-Venus, not Venus
himself, is read for the marriage itself). Chara karakas break that pattern: **the
karaka himself represents the person directly**, the same way a sthira karaka's body
does — "We do not take the 7th from DK for spouse, but DK himself shows spouse." This
is the correction already applied in migration 087 (see `life-matter-reference-pvr.md`).

### Sthira karaka (pg 82) — not yet in this project's DB at all

```
Father:   Sun or Venus, whichever stronger
Mother:   Moon or Mars, whichever stronger
Mars:     younger siblings, brother-/sister-in-law
Mercury:  maternal relatives (uncles, aunts)
Jupiter:  husband, sons, paternal grandparents/relatives
Venus:    wife, in-laws, maternal grandparents
Saturn:   elder siblings
```
Used only for death-timing of that relative; PVR explicitly says *don't* substitute it
for naisargika karaka in general prediction (his example: 7th-from-Jupiter is wrong for
marriage even though Jupiter is a female's sthira karaka for husband).

`LifeAreaMap.cs` has a **different** sthira-like `Karakas` list per UI life-area group,
but it's sourced from B.V. Raman, not PVR, and per its own doc comment is "not
reconciled against" the PVR-sourced tables (`naisargika-karaka-pvr.md` says the same).
So there are currently **two unreconciled, differently-sourced karaka lists** in the
codebase already, before this worksheet adds a third.

### Naisargika karaka — already fully seeded (migration 086)

Documented in `naisargika-karaka-pvr.md`. Not repeated here.

### Chara karaka (pg 80–81, Table 13) — algorithmic only, no rule table

```
Table 13: Chara karakas — rank order and the "persons shown" each one covers
  AK   Atma Karaka      — Self
  AmK  Amatya Karaka    — Ministers / advisors (people who give advice)
  BK   Bhratri Karaka   — Siblings
  MK   Matri Karaka     — Mother
  PiK  Pitri Karaka     — Father
  PK   Putra Karaka     — Children (footnote: can also show subordinates/followers)
  GK   Jnaati Karaka (also "GK (JK)") — Rivals/enemies (footnote: literally "paternal cousin")
  DK   Dara Karaka      — Spouse
```
Computed by `CharaKarakaCalculator` (degree-in-sign ranking, Rahu reversed, Ketu
excluded) into the `CharaKaraka` enum — **never persisted as a rule table**. Per
`pvr-coverage.md`'s ch. 8 entry, the reserved generic `tbl_Rule_Karaka` (migration 18)
was deliberately left empty for "a future chara/sthira layer" — migration 086 chose
purpose-built tables for naisargika instead of that generic one, which is the
precedent to follow if/when chara karaka gets a table too. Note `tbl_Rule_Karaka`'s
columns (`KarakaScheme`, `PlanetOrHouse`, `TargetValue`, `OrderIndex`,
`ReverseForRahu`) are shaped for the *ranking algorithm*, not for "what does AK mean" —
so it isn't actually a fit for a persons/subjects-per-karaka table even if it were used.

Today the *only* place chara-karaka interpretive meaning touches the DB is two rows in
`tbl_Rule_LifeMatterReference` (087): DK→"Individual spouse-role" and PK→"Particular
child-role", both PVR_DIRECT-cited to pg 83. **Table 13's full 8-karaka person list has
never been cross-referenced against Table 11's 20-chart life-area list.** That's the gap
the worksheet is trying to fill.

## PVR Table 11 (pg 60–61) — divisional chart → area of life

Already seeded verbatim in `tbl_Dim_LifeArea` (migration 30) — cross-checked here
against the raw book text directly (the raw OCR extraction has the chart/description
rows visually offset by one line; migration 30's seeding already has this corrected):

| Chart | Area of life (PVR's own wording) |
|---|---|
| D-1 Rasi | Existence at the physical level |
| D-2 Hora | Wealth and money |
| D-3 Drekkana | Everything related to brothers and sisters |
| D-4 Chaturthamsa | Residence, houses owned, properties and fortune |
| D-5 Panchamsa | Fame, authority and power |
| D-6 Shashthamsa | Health troubles |
| D-7 Saptamsa | Everything related to children (and grand-children) |
| D-8 Ashtamsa | Sudden and unexpected troubles, litigation etc |
| **D-9 Navamsa** | **Marriage and everything related to spouse(s), dharma (duty and righteousness), interaction with other people, basic skills, inner self** |
| D-10 Dasamsa | Career, activities and achievements in society |
| D-11 Rudramsa | Death and destruction |
| D-12 Dwadasamsa | Everything related to parents (also uncles, aunts, grand-parents) |
| D-16 Shodasamsa | Vehicles, pleasures, comforts and discomforts |
| D-20 Vimsamsa | Religious activities and spiritual matters |
| D-24 Chaturvimsamsa | Learning, knowledge and education |
| D-27 Nakshatramsa | Strengths and weaknesses, inherent nature |
| D-30 Trimsamsa | Evils and punishment, sub-conscious self, some diseases |
| **D-40 Khavedamsa** | **Auspicious and inauspicious events** |
| **D-45 Akshavedamsa** | **All matters** |
| **D-60 Shashtyamsa** | **Karma of past life, all matters** |

PVR groups these by "plane" (§6.4, also in `tbl_Dim_LifeArea.PlaneOfExistence`): D1–D12
physical, D16/D20/D24 mental, D27/D30 sub-conscious, D40/D45/D60 **kaarmic** — "based on
karma from previous lives... existence at a level that goes beyond body, mind and
sub-consciousness."

### Important finding: the worksheet's D-40/D-45/D-60 subjects don't match PVR

The worksheet gives D-40 = "maternal lineage," D-45 = "paternal lineage," D-60 = "past
karma and the root of life's experiences." D-60 lines up with PVR (bolded row above).
**D-40 and D-45 don't** — PVR's own Table 11 gives D-40 = auspicious/inauspicious
events and D-45 = all matters, nothing about maternal/paternal lineage specifically.

"D-40 = maternal lineage, D-45 = paternal lineage" is a real, commonly-cited
classical convention (it shows up in Parashari-tradition sources like BPHS-derived
literature) — but it isn't *this project's* cited source for it. If those two rows are
wanted, they need either (a) a different `tbl_Dim_Source` citation than
`SRC_PVR_INTEGRATED`, or (b) reframing under PVR's actual D-40/D-45 subjects
(auspicious/inauspicious events; all matters) instead.

## Where the worksheet's 18 rows land against what's already seeded

Cross-referencing chart+subject against the 96 rows already in `tbl_Rule_LifeMatterReference`
(full detail was posted earlier in this conversation) comes down to three buckets:

1. **Already correct, no-op** (2 of 18): D-9 marriage → DK, D-7 children → PK — these
   are the two PVR_DIRECT rows already described above.
2. **Conflicts with an existing single-naisargika-graha row** (~12 of 18): e.g. D-1 self
   is already Sun-cited (PVR_CROSSVALIDATED); D-3 siblings splits into Mars (younger)/
   Jupiter (elder), neither generically "BK"; D-12 parents already uses Moon/Sun via
   PVR's own Table 12 graha-lagna technique. Table 13's AK/BK/MK/etc. would be a
   *second*, equally PVR-legitimate technique on the same matter — same shape as the
   DK/PK rows, which the project resolved by adding a **second row**, not a second
   field on the existing row. That's the established precedent, not a new call.
3. **No matching row exists at all** (4 of 18): D-9 "dharma/inner self" (Table 11
   literally names this for D-9, but `tbl_Rule_LifeMatterReference`'s D-9 rows are all
   marriage-specific — this is a genuine, easy-to-source gap); D-40/D-45/D-60 (not
   sourceable to PVR as given, per the finding above).

## Open questions for whoever picks this up next

- Does chara-karaka-to-life-area data deserve its own purpose-built table (mirroring
  migration 086's approach for naisargika) rather than more rows in 087? 087 is scoped
  to "specific interpretive matters," not "which karaka type applies to this chart in
  general" — Table 13 cross-referenced against Table 11 is closer to the latter.
- Sthira karaka has zero DB presence and a second, unreconciled hard-coded list
  (`LifeAreaMap.cs`, B.V. Raman-sourced) already sitting next to the PVR-sourced
  naisargika tables. Worth a reconciliation pass on its own.
- D-40/D-45 subject labels: confirm intended source before seeding anything under
  those two charts.
