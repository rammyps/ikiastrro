---
last_updated: 2026-09-25
workstream: ui
status: draft
---

# LifeMatters — Claude's bounded research (Phase 0B)

## A1–A12 citations

**Source found and verified against the primary text** — `SRC_PVR_INTEGRATED` (P.V.R. Narasimha
Rao, *Vedic Astrology: An Integrated Approach*), the project's already-registered canonical
source (`docs/research/sources.md`), covers the full Arudha Pada taxonomy directly. Consulted the
registered local raw extract, `D:\@ClaudeSpace\BookExtracts\pvr-integrated-approach-raw.txt`.

Chapter 9, "Arudha Padas" (pp. 85–99), §9.1 Introduction, p.87, defines the two special-case
names: *"Arudha pada of lagna is denoted as AL (arudha lagna) and arudha pada of 12th house is
denoted as UL (upapada lagna)."* Its **Table 18, "Specific names of arudha padas," p.89** gives
the complete naming/alias set. The raw-text extraction has the table's label column and content
column offset by two rows (an OCR/PDF-extraction artifact, not a book error) — realigned below by
cross-checking every pairing against standard Jaimini convention (all twelve independently
confirm; e.g. A7 = Dara Pada matches every other classical source, A12 = Upapada Lagna matches
the plan's own approved decision 1 word-for-word, confirming decision 1 was already implicitly
sourced from this same table without a formal citation):

| House | PrimaryName (proposed) | Aliases (proposed, `AliasType = ClassicalName`) |
|---|---|---|
| A1 | Arudha Lagna | Pada Lagna, Arudha, Pada |
| A2 | Dhana Pada | Dhanarudha, Vitta Pada, Vittarudha |
| A3 | Vikrama Pada | Vikramarudha, Bhratri Pada, Bhatrarudha *(possible OCR-dropped letter — verify against the PDF/scan, not just the raw-text extract, before citing)* |
| A4 | Sukha Pada | Matri Pada, Vahana Pada, Matrarudha, Vahanarudha, Sukharudha |
| A5 | Putra Pada | Mantra Pada, Mantrarudha, Putrarudha, Buddhi Pada |
| A6 | Roga Pada | Shatru Pada, Rogarudha, Shatrarudha |
| A7 | Dara Pada | Dararudha |
| A8 | Mrityu Pada | Kashta Pada, Kashtarudha, Randhrarudha |
| A9 | Bhagya Pada | Bhagyarudha, Pitri Pada, Pitrarudha, Dharma Pada, Guru Pada |
| A10 | Karma Pada | Karmarudha, Swarga Pada, Swargarudha, Rajya Pada |
| A11 | Labha Pada | Labharudha |
| A12 | Upapada Lagna | Upapada, Gaunapada, Vyayarudha, Moksha Pada |

**Proposal**: seed `tbl_Dim_ArudhaPadaNames` / `tbl_Dim_ArudhaPadaAliases` with `SourceRefCode =
SRC_PVR_INTEGRATED` and `CitationStatus = 'Cited'` rather than the plan's placeholder
`'Uncited-flagged'` — a direct, page-verified source now exists. This is a proposal for human
audit (and for whoever writes the seed migration) before the citation status actually changes.

## UL/A12 versus A7

Migration 087's `MARRIAGE_SPOUSE` row (DisplayOrder 6) currently reads: *"Upapada Lagna (arudha
of the 7th) is PVR's own stated reference for the visible/social side of marriage..."* — this is
the transcription bug the plan flags. It traces back to
`docs/research/domain/life-matter-reference-pvr.md` (lines 49–50 as read), which independently
made the same error ahead of the migration. Both need the same correction.

PVR's book directly and repeatedly contradicts the "arudha of the 7th" phrasing:

- p.87: *"arudha pada of 12th house is denoted as UL (upapada lagna)"* — UL is defined as A12 at
  first introduction, not derived from the 7th house at all.
- p.89, Table 18 (see above): A7's names are Dara Pada/Dararudha; A12's are Upapada Lagna and its
  aliases — two separate rows, never conflated in the book's own table.
- pp.91–92: *"One important arudha pada used in Jyotish is upapada lagna (UL) - the arudha pada
  of the 12th house."* — restated a second time.
- §9.7 Summary, p.99: *"Darapada or A7 shows one's relationships. Upapada (UL) shows one's
  marriage and spouse."* — the cleanest single sentence: Darapada and Upapada are named side by
  side as two different points with two different roles, directly in PVR's own chapter summary.

**Proposed corrected `CalculationNarrative`** for migration 087's `MARRIAGE_SPOUSE` row 6 (for
whoever writes the corrective migration — Phase 0A's audit already confirmed no Fact/result row
references this RuleSet, so an in-place fix is safe, no RuleSet versioning needed):

> Upapada Lagna (UL, arudha of the 12th house / A12 — not the 7th) is PVR's own stated reference
> for the visible/social side of marriage (ch. 9 "Arudha Padas," pp. 87, 91–92, 99), used
> throughout the book for marriage timing.

The row's `HouseFromLagnaText = 'Upapada Lagna'` should resolve to a `FocusKind = SpecialPoint`
row with `SpecialPointCode = 'A12'` — never a 7th-house row and never the raw alias `'UL'` (the
plan's canonicalization rule: store `A12`, display `UL`). This is exactly what the Subject/Focus
mapping proposal below already proposes for this row, independently arrived at from the same
correction.

`docs/research/domain/life-matter-reference-pvr.md`'s own "Known gap" / provenance section should
get the same correction — noted here since it's a shared/cross-cutting research doc, likely
outside this bounded artifact's edit scope; flagging for whoever owns that fix (migration author
or a shared-docs pass) rather than editing it directly.

## Proposed Subject and Focus mappings

**These are proposals for human audit, not migrations.** Per `lifematters_plan.md`'s migration
policy, every candidate row here needs review before any `tbl_Rule_LifeMatterSubject` /
`tbl_Rule_LifeMatterFocus` seed migration is written. `SubjectCode` values are the 11 rows of
`tbl_Dim_DivisionalSubject` (migration 38); `ReferencePoint` values are `tbl_Dim_HouseReference`
codes (migration 32); `HouseNumber` is 1–12; special points use the plan's canonical A1–A12
codes, never aliases.

### Headline finding: Subject coverage has real gaps

The 11 `DivisionalSubject` rows cover Vargas D2, D3, D4, D7, D9 (×2), D10, D12, D16, D24, D60.
Three of the 10 `LifeMatterReference` categories use Vargas **no DivisionalSubject targets at
all** — D6, D8, D11, D20, D27, D30 — so every row in these categories is proposed **NO MATCH**:

- **SELF_HEALTH** (11 rows, D1/D6/D8/D27/D30) — no subject targets these Vargas.
- **SPIRITUALITY** (8 rows, D20/D4) — no subject targets D20.
- **TROUBLE_LOSS** (12 rows, D6/D8/D11/D30) — no subject targets these Vargas.

That's 31 of 96 rows (32%) with no viable Subject mapping before even reaching per-row
ambiguity. A handful of other rows also get **NO MATCH** or **AMBIGUOUS** individually (compound
`PrimaryChartsText`, or a subject that shares a house number but not the theme — flagged inline).
Any of these gaps is closeable in principle by adding new `DivisionalSubject` rows (health,
spirituality/moksha, litigation/loss) later — out of scope for this proposal pass.

### 96-row `PVR_LIFE_MATTER` set (`tbl_Rule_LifeMatterReference` / `tbl_Dim_LifeMatter`)

Focus `ReferencePoint` is `LAGNA` unless noted. Compound `HouseFromLagnaText` (`'X/Yth'`)
proposes two House rows.

**SELF_HEALTH** — all NO MATCH (see headline finding).

| Code | Matter | Chart | Subject proposal | Focus proposal |
|---|---|---|---|---|
| SELF_HEALTH_01 | Physical self and constitution | D1 | NO MATCH | House(LAGNA,1) |
| SELF_HEALTH_02 | General health | D1 | NO MATCH | House(LAGNA,1) |
| SELF_HEALTH_03 | Illness | D6 | NO MATCH | House(LAGNA,6) |
| SELF_HEALTH_04 | Chronic illness / longevity vulnerability | D6/D8 | NO MATCH; AMBIGUOUS compound chart | House(LAGNA,8) |
| SELF_HEALTH_05 | Accidents | D6/D8 | NO MATCH; AMBIGUOUS compound chart | House(LAGNA,6) + House(LAGNA,8) |
| SELF_HEALTH_06 | Hospitalization | D6/D30 | NO MATCH; AMBIGUOUS compound chart | House(LAGNA,12) |
| SELF_HEALTH_07 | Mind | D1 | NO MATCH | House(LAGNA,1) |
| SELF_HEALTH_08 | Peace of mind | 'D16, confirmed in D1' | AMBIGUOUS narrative text; D16=VEHICLES_COMFORTS by chart only, wrong theme — propose NO MATCH over a misleading match | House(LAGNA,4) |
| SELF_HEALTH_09 | Inherent strengths and weaknesses | D27 | NO MATCH | House(LAGNA,1) |
| SELF_HEALTH_10 | Courage and persistence | 'D27, confirmed in D1' | AMBIGUOUS narrative text; NO MATCH (SIBLINGS_COURAGE is D3, not D27 — name overlap only) | House(LAGNA,3) |
| SELF_HEALTH_11 | Subconscious disturbances | D30 | NO MATCH | House(LAGNA,1) + House(LAGNA,8) |

**WEALTH** — clean match to `WEALTH` (D2) throughout.

| Code | Matter | Chart | Subject proposal | Focus proposal |
|---|---|---|---|---|
| WEALTH_01 | Overall financial condition | D2 | WEALTH | House(LAGNA,1) |
| WEALTH_02 | Accumulated wealth | D2 | WEALTH | House(LAGNA,2) |
| WEALTH_03 | Family resources | D2 | WEALTH | House(LAGNA,2) |
| WEALTH_04 | Speculation | D2 | WEALTH | House(LAGNA,5) |
| WEALTH_05 | Loans and debt | D2/D6 | AMBIGUOUS compound; WEALTH plausible for the D2 half only | House(LAGNA,6) |
| WEALTH_06 | Gains and income | D2 | WEALTH | House(LAGNA,11) |
| WEALTH_07 | Credits / receivables | D2 | WEALTH | House(LAGNA,11) |
| WEALTH_08 | Expenditure and financial loss | D2 | WEALTH | House(LAGNA,12) |

**EDUCATION** — clean match to `EDUCATION_LEARNING` (D24) throughout.

| Code | Matter | Chart | Subject proposal | Focus proposal |
|---|---|---|---|---|
| EDUCATION_01 | Overall learning | D24 | EDUCATION_LEARNING | House(LAGNA,1) |
| EDUCATION_02 | Basic / formal education | D24 | EDUCATION_LEARNING | House(LAGNA,4) |
| EDUCATION_03 | Traditional learning | D24 | EDUCATION_LEARNING | House(LAGNA,4) |
| EDUCATION_04 | Memory | D24 | EDUCATION_LEARNING | House(LAGNA,5) |
| EDUCATION_05 | Intelligence | D24 | EDUCATION_LEARNING | House(LAGNA,5) |
| EDUCATION_06 | Scholarship | D24 | EDUCATION_LEARNING | House(LAGNA,5) |
| EDUCATION_07 | Nyāya / logical scholarship | D24 | EDUCATION_LEARNING | House(LAGNA,5) |
| EDUCATION_08 | Students | D24 | EDUCATION_LEARNING | House(LAGNA,5) |
| EDUCATION_09 | Writing and communication | D24 | EDUCATION_LEARNING | House(LAGNA,3) |
| EDUCATION_10 | Teachers and gurus | D24 | EDUCATION_LEARNING | House(LAGNA,9) |
| EDUCATION_11 | Higher education | D24 | EDUCATION_LEARNING | House(LAGNA,9) |
| EDUCATION_12 | Educational achievement | D24 | EDUCATION_LEARNING | House(LAGNA,10) + House(LAGNA,11) |
| EDUCATION_13 | Academic recognition | D24 | EDUCATION_LEARNING | **House(ARUDHA_LAGNA,5)** — '5th from AL' |

**PROPERTY_COMFORTS** — splits cleanly between `PROPERTY_RESIDENCE` (D4) and `VEHICLES_COMFORTS`
(D16); not a single per-category subject.

| Code | Matter | Chart | Subject proposal | Focus proposal |
|---|---|---|---|---|
| PROPERTY_COMFORTS_01 | House and immovable property | D4 | PROPERTY_RESIDENCE | House(LAGNA,4) |
| PROPERTY_COMFORTS_02 | General residence | D4 | PROPERTY_RESIDENCE | House(LAGNA,4) |
| PROPERTY_COMFORTS_03 | Fortune connected with property | D4 | PROPERTY_RESIDENCE | House(LAGNA,9) |
| PROPERTY_COMFORTS_04 | Foreign residence | D4 | PROPERTY_RESIDENCE | House(LAGNA,9) + House(LAGNA,12) |
| PROPERTY_COMFORTS_05 | Expenditure on property | D4 | PROPERTY_RESIDENCE | House(LAGNA,3) |
| PROPERTY_COMFORTS_06 | Vehicles | D16 | VEHICLES_COMFORTS | House(LAGNA,4) |
| PROPERTY_COMFORTS_07 | Pleasure from vehicles | D16 | VEHICLES_COMFORTS | House(LAGNA,4) |
| PROPERTY_COMFORTS_08 | General comforts | D16 | VEHICLES_COMFORTS | House(LAGNA,4) |
| PROPERTY_COMFORTS_09 | Loss of comfort | D16 | VEHICLES_COMFORTS | House(LAGNA,12) |
| PROPERTY_COMFORTS_10 | Expenditure on vehicles | D16 | VEHICLES_COMFORTS | House(LAGNA,3) |

**FAMILY_RELATIONSHIPS** — splits across four subjects by matter, not by category.

| Code | Matter | Chart | Subject proposal | Focus proposal |
|---|---|---|---|---|
| FAMILY_RELATIONSHIPS_01 | Family | D2 | WEALTH (chart match; cross-category) | House(LAGNA,2) |
| FAMILY_RELATIONSHIPS_02 | Mother | D12 | MOTHER_PARENTS | House(LAGNA,4) |
| FAMILY_RELATIONSHIPS_03 | Father | D12 | MOTHER_PARENTS | House(LAGNA,9) |
| FAMILY_RELATIONSHIPS_04 | Younger siblings | D3 | SIBLINGS_COURAGE | House(LAGNA,3) |
| FAMILY_RELATIONSHIPS_05 | Elder siblings | D3 | SIBLINGS_COURAGE | House(LAGNA,11) |
| FAMILY_RELATIONSHIPS_06 | Friends | D1/D9 | NO MATCH; AMBIGUOUS compound chart | House(LAGNA,11) |
| FAMILY_RELATIONSHIPS_07 | Followers | D5/D10 | AMBIGUOUS compound; CAREER_STATUS plausible for D10 half only | House(LAGNA,5) |
| FAMILY_RELATIONSHIPS_08 | Servants / subordinates | D10 | CAREER_STATUS (chart match; cross-category) | House(LAGNA,6) |
| FAMILY_RELATIONSHIPS_09 | Boss or authority figure | D10 | CAREER_STATUS (chart match; cross-category) | House(LAGNA,9) |

**MARRIAGE_SPOUSE** — clean match to `MARRIAGE_RELATIONSHIPS` (D9); row 6 is the Upapada Lagna
special point.

| Code | Matter | Chart | Subject proposal | Focus proposal |
|---|---|---|---|---|
| MARRIAGE_SPOUSE_01 | Marriage | D9 | MARRIAGE_RELATIONSHIPS | House(LAGNA,7) |
| MARRIAGE_SPOUSE_02 | Spouse | D9 | MARRIAGE_RELATIONSHIPS | House(LAGNA,7) |
| MARRIAGE_SPOUSE_03 | Marital happiness | D9 | MARRIAGE_RELATIONSHIPS | House(LAGNA,7) |
| MARRIAGE_SPOUSE_04 | Interaction with others | D9 | MARRIAGE_RELATIONSHIPS | House(LAGNA,7) |
| MARRIAGE_SPOUSE_05 | Sexual / bed pleasures | D9/D16 | AMBIGUOUS compound; MARRIAGE_RELATIONSHIPS or VEHICLES_COMFORTS | House(LAGNA,12) |
| MARRIAGE_SPOUSE_06 | Public manifestation of marriage | D9/D1 | AMBIGUOUS compound; MARRIAGE_RELATIONSHIPS plausible for D9 half | **SpecialPoint A12** — per the plan's UL/A12 canonicalization, store `A12`, not a 7th-house row and not `UL`. This is the row whose narrative text still says "arudha of the 7th" (migration-087 bug; see the Phase 0A audit) — the focus proposal here is already corrected. |
| MARRIAGE_SPOUSE_07 | Individual spouse-role | D9 | MARRIAGE_RELATIONSHIPS | House(LAGNA,7) — `HouseFromKarakaText` ('DK himself') is a Planet/Karaka-source concern via `tbl_Rule_KarakaMatter`, not this Focus table |

**CHILDREN** — clean match to `CHILDREN_PROGENY` (D7); row 6 needs a derived-reference decision.

| Code | Matter | Chart | Subject proposal | Focus proposal |
|---|---|---|---|---|
| CHILDREN_01 | Children / progeny | D7 | CHILDREN_PROGENY | House(LAGNA,5) |
| CHILDREN_02 | Particular child-role | D7 | CHILDREN_PROGENY | House(LAGNA,5) |
| CHILDREN_03 | Conception / union | D7 | CHILDREN_PROGENY | House(LAGNA,7) |
| CHILDREN_04 | Fortune of children | D7 | CHILDREN_PROGENY | House(LAGNA,9) |
| CHILDREN_05 | Gains or fulfilment through children | D7 | CHILDREN_PROGENY | House(LAGNA,11) |
| CHILDREN_06 | Grandchildren | D7 | CHILDREN_PROGENY | **UNCLEAR** — `'9th from 5th'` is a house-from-house derived reference; the Focus schema (one `ReferencePoint` + one `HouseNumber`) can't express chained derivation directly. 9th-from-5th resolves to the 1st house from Lagna arithmetically, but storing `House(LAGNA,1)` loses the "derived from the 5th" provenance. Needs a project decision before Phase 1A: either extend the schema for derived references, or store the flattened result with an explanatory note. |

**CAREER_STATUS** — clean match to `CAREER_STATUS` (D10) except rows 9–10.

| Code | Matter | Chart | Subject proposal | Focus proposal |
|---|---|---|---|---|
| CAREER_STATUS_01 | Overall career direction | D10 | CAREER_STATUS | House(LAGNA,1) |
| CAREER_STATUS_02 | Initiative at work | D10 | CAREER_STATUS | House(LAGNA,3) |
| CAREER_STATUS_03 | Employment and service | D10 | CAREER_STATUS | House(LAGNA,6) |
| CAREER_STATUS_04 | Professional competition | D10 | CAREER_STATUS | House(LAGNA,6) |
| CAREER_STATUS_05 | Business partnership | D10 | CAREER_STATUS | House(LAGNA,7) |
| CAREER_STATUS_06 | Boss and mentor | D10 | CAREER_STATUS | House(LAGNA,9) |
| CAREER_STATUS_07 | Career and actions | D10 | CAREER_STATUS | House(LAGNA,10) |
| CAREER_STATUS_08 | Achievements and honours | D10 | CAREER_STATUS | House(LAGNA,10) |
| CAREER_STATUS_09 | Authority and power | D5/D10 | AMBIGUOUS compound; CAREER_STATUS plausible for the D10 half only | House(LAGNA,5) + House(LAGNA,10) |
| CAREER_STATUS_10 | Fame | D5 | NO MATCH (no D5 subject; CAREER_STATUS is D10) | House(LAGNA,5) |
| CAREER_STATUS_11 | Professional gains | D10 | CAREER_STATUS | House(LAGNA,11) |
| CAREER_STATUS_12 | Professional loss / retirement | D10 | CAREER_STATUS | House(LAGNA,12) |

**SPIRITUALITY** — all NO MATCH (see headline finding).

| Code | Matter | Chart | Subject proposal | Focus proposal |
|---|---|---|---|---|
| SPIRITUALITY_01 | Overall spiritual life | D20 | NO MATCH | House(LAGNA,1) |
| SPIRITUALITY_02 | Devotion and mantra | D20 | NO MATCH | House(LAGNA,5) |
| SPIRITUALITY_03 | Teacher / guru | D20 | NO MATCH | House(LAGNA,9) |
| SPIRITUALITY_04 | Religion and fortune | D20 | NO MATCH | House(LAGNA,9) |
| SPIRITUALITY_05 | Pilgrimage | D20/D4 | NO MATCH; AMBIGUOUS compound chart | House(LAGNA,9) |
| SPIRITUALITY_06 | Foreign spiritual journey | D20/D4 | NO MATCH; AMBIGUOUS compound chart | House(LAGNA,9) + House(LAGNA,12) |
| SPIRITUALITY_07 | Renunciation | D20 | NO MATCH | House(LAGNA,12) |
| SPIRITUALITY_08 | Mokṣa | D20 | NO MATCH | House(LAGNA,12) |

**TROUBLE_LOSS** — all NO MATCH (see headline finding).

| Code | Matter | Chart | Subject proposal | Focus proposal |
|---|---|---|---|---|
| TROUBLE_LOSS_01 | Enemies | D6/D8 | NO MATCH; AMBIGUOUS compound chart | House(LAGNA,6) |
| TROUBLE_LOSS_02 | Disease | D6/D30 | NO MATCH; AMBIGUOUS compound chart | House(LAGNA,6) |
| TROUBLE_LOSS_03 | Debt | D2/D6 | AMBIGUOUS compound; WEALTH plausible for the D2 half only | House(LAGNA,6) |
| TROUBLE_LOSS_04 | Accidents | D6/D8 | NO MATCH; AMBIGUOUS compound chart | House(LAGNA,6) + House(LAGNA,8) |
| TROUBLE_LOSS_05 | Litigation | D8 | NO MATCH | House(LAGNA,6) + House(LAGNA,8) |
| TROUBLE_LOSS_06 | Sudden trouble | D8 | NO MATCH | House(LAGNA,8) |
| TROUBLE_LOSS_07 | Longevity | D1/D8 | NO MATCH; AMBIGUOUS compound chart | House(LAGNA,8) |
| TROUBLE_LOSS_08 | General troubles | D8/D30 | NO MATCH; AMBIGUOUS compound chart | House(LAGNA,8) |
| TROUBLE_LOSS_09 | Loss | 'Relevant varga' | AMBIGUOUS — no literal chart code given at all in the source text | House(LAGNA,12) |
| TROUBLE_LOSS_10 | Hospitalization / confinement | D6/D30 | NO MATCH; AMBIGUOUS compound chart | House(LAGNA,12) |
| TROUBLE_LOSS_11 | Destruction / death | D11 | NO MATCH | House(LAGNA,8) |
| TROUBLE_LOSS_12 | Occult knowledge | D8/D20 | NO MATCH; AMBIGUOUS compound chart | House(LAGNA,8) |

### 32-row `PVR_KARAKATWA_GRID` set (natural per-house significations, `tbl_Dim_LifeMatter`)

These 32 rows have no `PrimaryChartsText`/`HouseFromLagnaText` columns of their own — they come
from the dropped `tbl_Rule_Naisargika_Karakatwas` grid (migration 086, now only surfacing via
`tbl_Dim_LifeMatter.SourceGroup='PVR_KARAKATWA_GRID'` and the compatibility views). Their
`HouseNumber` is already a literal house-from-Lagna position (natural signification, no divisional
chart specified), so **Focus is uniform and unambiguous**: `House(LAGNA, HouseNumber)` for every
row. Subject mapping is far weaker here — these are single-house natural correspondences, not
matched to any specific Varga, so a "clean" Subject match only exists where a DivisionalSubject's
own cited house number coincides *and* the theme agrees; a same-house-different-theme coincidence
is flagged AMBIGUOUS or NO MATCH rather than accepted.

| House | Matter(s) at this house | Subject proposal |
|---|---|---|
| 1 | Self, soul, constitution, health | NO MATCH |
| 1 | Mind | NO MATCH |
| 2 | Speech | NO MATCH (house coincides with WEALTH's 2nd/11th, theme doesn't) |
| 2 | Family and wealth | WEALTH |
| 3 | Courage and younger siblings | SIBLINGS_COURAGE |
| 4 | Real estate | PROPERTY_RESIDENCE |
| 4 | Learning | NO MATCH (EDUCATION_LEARNING is D24, not house-4-themed here) |
| 4 | Traditional learning | NO MATCH |
| 4 | Vehicles | AMBIGUOUS — thematically VEHICLES_COMFORTS, but that subject's Varga is D16, not this generic house-4 entry |
| 4 | Mother and peace of mind | AMBIGUOUS — thematically MOTHER_PARENTS, but that subject's Varga is D12 |
| 5 | Children and intelligence | CHILDREN_PROGENY (loosely; CHILDREN_PROGENY's Varga is D7, this is a generic house-5 entry) |
| 5 | Fame and power | NO MATCH |
| 5 | Nyāya scholarship and speculation | NO MATCH |
| 5 | Memory, scholarship and students | NO MATCH |
| 5 | Followers | NO MATCH |
| 6 | Enemies, disease, accidents and loans | NO MATCH |
| 6 | Servants | NO MATCH |
| 6 | Accidents | NO MATCH |
| 7 | Spouse and marital happiness | MARRIAGE_RELATIONSHIPS (loosely; that subject's Varga is D9) |
| 8 | Longevity and troubles | NO MATCH |
| 8 | Occult knowledge | NO MATCH |
| 9 | Father and boss | AMBIGUOUS — house coincides with both OVERALL_STRENGTH_DHARMA ("9th house") and MOTHER_PARENTS ("9th/10th... for parents"); theme doesn't cleanly fit either |
| 9 | Teacher, religion and fortune | NO MATCH |
| 9 | Pilgrimage and foreign travel | NO MATCH |
| 10 | Career and achievements | CAREER_STATUS |
| 10 | Work, achievements and honours | CAREER_STATUS |
| 11 | Credits | WEALTH |
| 11 | Elder brother and gains | AMBIGUOUS — WEALTH by house-11 gains theme, or SIBLINGS_COURAGE by "elder brother" theme; two candidates |
| 11 | Friends | NO MATCH |
| 12 | Bed pleasures | NO MATCH |
| 12 | Loss and hospitalization | NO MATCH |
| 12 | Mokṣa | NO MATCH |

## Karaka/InterpretiveFactor coverage discrepancies

**These are audit findings for human review, not migrations.**

### Karaka coverage

Cross-referenced all 128 `tbl_Dim_LifeMatter` rows against `tbl_Rule_KarakaMatter` (149 rows,
migration 103). Coverage is near-complete:

- **32-row `PVR_KARAKATWA_GRID` set: 32/32 covered (100%).** Every row originates directly from a
  (Graha, House, Matter) triple in the source Karakatwa grid, so there's no possibility of an
  ungrounded Matter here (34 `KarakaMatter` rows across 32 distinct `LifeMatterId`s — the two
  Rahu/Ketu shared matters get two rows each).
- **96-row `PVR_LIFE_MATTER` set: 95/96 covered.** Migration 103 draws coverage from three
  sources on the 087 seed data: `SinglePlanet` (73 rows), `CharaKaraka` (2 rows —
  `MARRIAGE_SPOUSE_07` 'DK', `CHILDREN_02` 'PK'), and a compound-text parse of `KarakaText` for
  the remaining 21 rows (split on `/` and `' and '`, each token matched against
  `tbl_Planets.PlanetName`). 20 of those 21 parse cleanly into two valid planet names each (40
  `KarakaMatter` rows — this is where "40 parsed-compound" comes from). **Exactly one row has
  zero Karaka coverage: `SELF_HEALTH_09` ("Inherent strengths and weaknesses"),
  `KarakaText = 'Lagna lord'`** — "Lagna lord" is chart-dependent (which planet it resolves to
  depends on the natal Ascendant), not a fixed Naisargika graha or a Chara role, so it can't join
  `tbl_Dim_KarakaRole`'s 17 rows (9 Naisargika + 8 Chara). Migration 103's own comment already
  calls this an intentional exclusion ("per karakafix.md's own 'non-karaka reference'
  allowance"), not an oversight — but it means this one LifeMatter will show "unstructured focus"
  on its Karaka evidence card in v1 unless `tbl_Dim_KarakaRole` grows a functional/derived role
  type for "Lagna lord" (a schema question, out of scope here — flag for whoever owns Phase 1A).

**Total: 127/128 LifeMatters have at least one Karaka row.** A much smaller gap than the
Subject-mapping gap found above (31/96, 32%) — Karaka coverage is essentially solved already.

### InterpretiveFactorDetail coverage

Migration 109 seeded `tbl_Rule_InterpretiveFactorDetail` House/Planet/Varga facts for 10 of the
11 `tbl_Dim_DivisionalSubject` rows (41 seed rows total) — `KARMIC_ROOTS` legitimately gets only a
VARGA row since its own `D1Foundation` text names no specific house or planet. Its header
comment explicitly deferred LifeArea-scoped and Chara-karaka-role-scoped details to a follow-up
migration.

**That follow-up partially happened.** Migration 134 ("FEAT-HOUSE-05") added LifeArea **VARGA**
facts for all 20 `tbl_Dim_LifeArea` rows (21 rows, mechanically derived from
`tbl_Dim_ChartType.PrimaryLifeAreaId` — no manual transcription) and reconciled 10
`LifeArea.Description` rows' wording to match the corresponding `DivisionalSubject`. It does
**not** touch LifeArea House/Planet facts, and its own comment says Chara-karaka-role
(`KarakaRoleId`-scoped) rows stay **entirely unseeded**: *"CHARA_KARAKA_ROLE (Leg D) stays
unseeded, same as 109 left it — no source content to seed from yet... explicit follow-up, not
guessed."* Confirmed no migration after 134 touches this table (searched every `db/*.sql` file
above 109; the only other hit, migration 110, is an unrelated table name collision).

**Current coverage, concretely:**

| Factor | DivisionalSubject (11 rows) | LifeArea (20 rows) | KarakaRole / Chara (8 roles) |
|---|---|---|---|
| VARGA | 11/11 | 20/20 (mig. 134) | not applicable |
| HOUSE | 10/11 (`KARMIC_ROOTS` has none by design) | **0/20 — unseeded** | not applicable |
| PLANET | 10/11 (`KARMIC_ROOTS` has none by design) | **0/20 — unseeded** | not applicable |
| Any factor at all | 11/11 | 20/20 (VARGA only) | **0/8 — unseeded** |

**Does this block LifeMatters dossier assembly?** Only partially, and less than it first appears.
`tbl_Rule_InterpretiveFactorDetail` isn't itself a runtime evidence source for the dossier —
nothing in the Key Inference read-path mapping below cites it; per its own migration comment, it
exists to make LifeArea/DivisionalSubject/KarakaRole's own prose facts queryable for
cross-checking ("nothing can query 'which subjects cite Venus' today"). Its real use for
LifeMatters is as an **audit tool** — cross-checking the Subject/Focus mapping proposals above
against each DivisionalSubject's own recorded House/Planet facts. That cross-check is only
possible today for the 10 covered DivisionalSubjects; it can't validate LifeArea-level or
Chara-karaka-role claims at all yet. This doesn't block Phase 1A (the new Subject/Focus/Karaka
tables don't depend on it), but it does mean several findings from the Subject/Focus mapping
above — the 31 NO-MATCH rows, and the AMBIGUOUS rows touching chara themes like "elder brother"
or "Grandchildren" (`CHILDREN_06`) — can't be cross-validated against a structured source yet,
only against free-text `D1Foundation` / `tbl_Dim_LifeArea.Description` prose.

## Plain-language labels and safe copy

Templates derived directly from `lifematters_plan.md`'s "LifeMatter Interpretation Contract" and
its explicit-states requirements — wording only, no new rules. Proposals for review before any
Razor implementation.

**Contribution badges** (never inferred from dignity/Shadbala/benefic-malefic/row presence —
`NotEvaluated` is the only safe default absent a sourced rule):

| Value | Badge label | Helper copy |
|---|---|---|
| `Supportive` | Supportive | "Sourced interpretation reads this as supportive." |
| `Obstructive` | Obstructive | "Sourced interpretation reads this as obstructive." |
| `Mixed` | Mixed | "Sourced interpretation reads this as mixed." |
| `Contextual` | Context only | "Relevant context; not itself supportive or obstructive." |
| `NotEvaluated` | Not yet evaluated | "Relevant evidence; interpretive contribution not evaluated." (verbatim, per plan) |
| `NotApplicable` | Not applicable | "Present but not applicable to this matter." |

**Provenance distinction** (the plan requires UI copy to separate computed fact from
interpretation — proposed short tags, not full sentences, so they read well on dense cards):
`Computed` (chart fact, no interpretation layer) · `Sourced` (a cited traditional interpretation)
· `Project synthesis` (`SRC_IKIASTRRO_SYNTHESIS`-style reasoned extension) · `Astrologer note`
(user-entered judgment) · `Unresolved` (evidence exists but isn't structured/sourced yet).

**Projected-point label** (non-D1 chart, per the plan's example): `"{Code} projected into {Varga}
from its D1-derived longitude"`, e.g. *"A12 projected into D9 from its D1-derived longitude."*
Never phrase this as if the point were independently recalculated in that Varga.

**D1/Varga comparison row**: `"D1: {d1Promise} · {Varga} evidence: {vargaEvidence} ·
{Agreement}"` where `Agreement ∈ {Confirms, Modifies, Contradicts, Insufficient}`, each followed
by its stored `Reason` text, never left as a bare label.

**Nakshatra distinction** (the plan explicitly forbids collapsing these into "the house's
nakshatra"): label the sign's own overlapping spans as *"Nakshatra spans in this sign"*; an
occupant's as *"{Planet}'s nakshatra: {Nakshatra} pada {n}"*; a focused special point's as
*"{Code}'s own nakshatra: {Nakshatra}"* — three visually distinct rows, never merged into one.

**Deterministic-language guardrail**: no card may state an outcome as certain, especially for
health/marriage/family/career. Prefer *"evidence points toward…"* / *"classically associated
with…"* over *"this means…"* / *"you will…"*. This applies to every `Contribution ≠
NotEvaluated` card, not just synthesis rows.

**Explicit-state copy** (one line each, per the plan's required states):
- Unknown chart ID: *"This chart isn't available."*
- Missing workspace/birth data: *"No birth data on file for this person yet."*
- Uncomputed Varga: reuse the chart's existing empty state (no new copy needed).
- Repository failure: *"Couldn't load this evidence right now — try again."*
- Unstructured focus (no Karaka/Focus row yet): *"This matter's chart position isn't
  structured yet — showing what's available."*
- Empty evidence card: *"No {cardName} evidence for this selection."*
- Rapid-selection race: no visible message — last selection silently wins, stale responses
  discarded (per plan; nothing to word here).

**Page-level v1 disclaimer** (the plan: v1 "must never claim to be a complete interpretation
layer"): a persistent, low-key line, not a modal — e.g. *"LifeMatters assembles evidence for a
matter; it does not judge natal promise, timing, or outcome."*

**Bhava Bala link-out** (house condition explicitly excludes a Bhava Bala summary in v1): *"See
Key Inference → Strength for this house's Bhava Bala."* — a link, not a partial number.

**Natal-promise conclusions** (`Strongly promised` / `Conditionally promised` / `Mixed` /
`Weakly indicated` / `Substantially denied` / `Indeterminate`) are Phase 4 — this copy set
intentionally has no v1 label for them; don't surface partial wording ahead of that phase.

## Spec consistency, visual/accessibility review, regression review

Unblocked — both component specs now exist (`specs_sind_hov_grid.md`, `specs_life_matters_page.md`,
2026-09-25). Reviewed against the actual shipped code (`SindHovGrid.razor(.cs/.css)`,
`SindHovGridTests.cs`, `LifeMatterFocusResolver.cs`), not just the specs' own prose, so this is
independent verification, not a restatement.

### Spec consistency — verified against source, no undocumented discrepancies

Every behavior `specs_sind_hov_grid.md` claims was checked directly against
`SindHovGrid.razor`/`.razor.cs`/`.razor.css` and holds exactly as described: Escape clears preview
before pin (`OnGridKeyDown`), the non-color `.selection-mark` renders on `isRelevant || isPinned`,
special-point props carry canonical codes only, and CSS uses only `--brand-*`/`--font-size-*`
tokens (no `--wheel-*`/`--cell-*`/`--tmpl-*` reuse — `project_standards.md` §3.3 respected) plus a
`prefers-reduced-motion` rule and the 720px breakpoint. Both flagged gaps are confirmed real, not
overstated: `SindHovGridTests.cs` (91 lines, 5 tests) has no Escape-key case, and there is no
`SindHovGrid` entry in `ChartSnapshotTests` — no golden SVG.

The flagged `LifeMatterSignMath.ResolveHouseSign` (Web) / `LifeMatterFocusResolver.ResolveHouseSign`
(Core) duplication is confirmed **cosmetic**, not a correctness risk: both call the identical
`AstroMath.CountFromSignToSign(origin, sign) == houseNumber` search, just wrapped for a string vs.
`ZodiacName` input. Collapsing to one is safe housekeeping, not urgent before Phase 3C.

`specs_life_matters_page.md`'s read-path table was checked against the actual `src/Ikiastrro.Data`
repositories. The Arudha gap is real — no repository or view exposes Arudha placements as queryable
per-chart evidence; `SpecialPointLabels`/`GrahaArudhaLabels` are grid-render props, not a data read
path. The Relationships row undersells what exists, one correction: `NaturalRelationshipRuleRepository`
**does exist** (`src/Ikiastrro.Data/NaturalRelationshipRuleRepository.cs`) and already backs
`DignityEngine`/`ShadbalaCalculator`/the Yoga evaluators — but only as an internal rule lookup
(planet-pair friendship tier), never exposed as a standalone chart-evidence query. Whoever builds
the Relationships card has a real repository to wrap, not a blank slate — worth stating precisely
so it isn't rebuilt from scratch.

### Accessibility review (code-level; no built page exists yet to click through)

`SindHovGrid`'s ARIA structure is sound as shipped: `role="grid"` / `role="gridcell"`, a computed
per-cell `aria-label` naming sign, house, occupants and special points, `aria-selected` tracking
the pin, and keyboard operability via native `<button>` + Enter/Space + Escape handlers (no custom
tabindex management needed since every cell is a real button). Selection is marked two independent
ways (icon + inset border), satisfying "non-color indicator" even though the border is
brand-sunset-colored. Nothing found to flag. A full a11y pass (screen-reader run, page-level
landmark structure, disclosure semantics) has to wait for the LifeMatters page itself — Phase 3A —
since a component in isolation can't be reviewed for page-level nav/landmark correctness.

### Regression review — one real finding: `workstream/ui` is stale against `master`

`git diff master..workstream/ui` on shared files turned up `tokens.css`, `MainLayout.razor(.css)`,
`Add.razor`, `KeyInference.razor(.css)`, `SavedCharts.razor`, and `DataTable.razor.css` all showing
as changed alongside the LifeMatters commits — first read as a possible scope violation (Codex
touching shell/tokens, which `wkstream_UI_v2.md`'s workstream mechanics explicitly forbid). Checked
`git merge-base workstream/ui master`: the branch point is `c211206`, and `master` has since gained
four commits `workstream/ui` doesn't have (`b5d186d` four-level type scale, `eb3c789` all-caps
headers/dropdowns, `f1db06a` Saved Charts header fix, `2e7fd15` Add-page `ActivePerson` clear).
Every "changed" line in the diff is one of those four commits' effect disappearing when diffed the
other direction — confirmed on `Add.razor` and `MainLayout.razor.css` directly, not assumed. **Not
a Codex scope violation** — Codex's two commits (`d5bd1b5`, `8216b95`) don't touch any of those
files themselves. But it is a real integration risk: merging `workstream/ui` to `master` today, or
building Phase 3A on top of it without rebasing first, would silently reintroduce the pre-caps,
single-font-scale CSS and drop the three other master-only fixes. **Rebase `workstream/ui` onto
`master` before any further LifeMatters work lands**, per the recurring drift this project has
already hit before (branch last rebased 2026-09-16).

No regression risk found in `SindHovGrid` itself against `SouthIndianGrid_Detailed`/`_Micro` —
it's CSS-isolated, shares no component file, and reuses only the existing `PlanetChip` component
unmodified.

## Mapping Key Inference read paths to dossier ingredients

From the Phase 0A code audit: `KeyInference.razor`'s code-behind wires roughly twenty
repositories directly into page state per master tab — there is **no separable evidence-assembly
service** to import. LifeMatters will call the same repositories directly (or a new service that
wraps them) rather than reuse an existing bundler. Mapping, dossier ingredient → existing
read path:

| Dossier ingredient | Existing repository / read path | v1 status |
|---|---|---|
| House condition (lord, lord placement, occupants, aspects, conjunctions) | `ChartHouseLordsRepository`, `ChartHouseLordInterpretationRepository`, `ChartAspectsRepository`, `ChartConjunctionsRepository`, `ChartMultiGrahaConjunctionRepository` | reusable as-is |
| Planet condition (dignity, functional nature, combustion/retrograde) | `ChartViewModel.BuildPlanetRows` / `BuildExaltationRows`, `GrahaDrishtiStrengthRepository`, `RasiNakshatraCombinationRepository`, `ChartMoonContextRepository` | reusable as-is |
| Strength (Shadbala only, per plan) | `PlanetaryStrengthRepository.GetSummaryByBirthDetailId` / `GetComponentsByBirthDetailId` | reusable as-is |
| Avastha | `PlanetaryStateRepository.GetByBirthDetailId`, `PlanetaryStateRuleRepository.GetAllStates`, `PostureStateInterpretationRepository.GetByRuleSet` | reusable as-is |
| Argala | **Correction (Phase 3B1 build, 2026-09-25): not reusable.** `ArgalaRuleRepository.GetArgalaSignificanceNotes` is a static `HouseOffset -> note` dictionary, not per-chart evidence. The real facts (`tbl_Fact_Argala`) have no read method anywhere — `ArgalaFactRepository` is write/delete only (chart-generation plumbing). This row was wrong; verified by reading the repository source, not just its name. | gap — needs a `GetByChartResultId`/`GetByBirthDetailId` read method before this card can be built |
| Arudha | **No dedicated repository.** Comes through `NaisargikaKarakaRepository` / Core `ArudhaCalculator` seeds, rendered only as `SpecialPointLabels`/`GrahaArudhaLabels` grid props today — not a queryable evidence table | gap — needs a read path, not just a render prop |
| Yogas (filtered) | `YogaEvaluationRepository.GetByBirthDetailId`, `InterpretationRepository.GetBySubjectType` | reusable as-is |
| Relationships | Not confirmed in the Phase 0A audit — likely `tbl_Rule_CompoundRelationship` (migration 24) plus the aspect/conjunction repositories above, but no dedicated "relationships" repository was found | needs confirmation before Phase 3B2 |
| Bhava Bala, Ashtakavarga, Amsabala | `BhavaStrengthRepository`, `AshtakavargaRepository`, `AmsabalaRepository`/`AmsabalaSchemeRepository`/`VargottamaRepository` — all exist, persisted and repository-backed | **Phase 4 deferred**, not v1 (house condition links out instead) |
| Drill-through (`/key-inference/{id}?step=...&chart=...&house=...`) | `KeyInference.razor` only reads `step` today (if-chain; `about-houses` isn't a recognized value, only `houses` is); no `chart=`/`house=` query param exists | **needs new work** — the plan's example URL isn't supported by the current page |
