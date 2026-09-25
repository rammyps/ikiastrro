---
last_updated: 2026-09-25
---

# Yoga corpus expansion beyond Raman's 300

## PVR Chapter 11 canonical inventory and subdivision plan

The raw extract (`D:\@ClaudeSpace\BookExtracts\pvr-integrated-approach-raw.txt`, lines
4297-5492) contains **98 named yoga headings** plus **59 numbered combinations**:
18 advanced Raja, 15 Raja Sambandha, 13 Dhana (one general principle plus twelve
lagna-specific rules), and 13 Daridra. The older "~98 named yogas" is a concept-level
estimate, not a sufficient implementation ledger: aliases, alternate definitions, and
same-name/different-formation cases must remain explicit source variants.

### Named sets as printed

| PVR set | Count | Names |
|---|---:|---|
| Ravi | 4 | Vesi; Vosi; Ubhayachara; Budha-Aaditya (Nipuna) |
| Chandra | 6 | Sunapha; Anapha; Duradhara; Kemadruma; Chandra-Mangala; Adhi |
| Pancha Mahapurusha | 5 | Ruchaka; Bhadra; Sasa; Malavya; Hamsa |
| Nabhasa / Aasraya | 3 | Rajju; Musala; Nala |
| Nabhasa / Dala | 2 | Maala; Sarpa |
| Nabhasa / Aakriti | 20 | Gadaa; Sakata; Sringaataka; Vihanga; Hala; Vajra; Yava; Kamala; Vaapi; Yoopa; Sara; Sakti; Danda; Naukaa; Koota; Chatra; Chaapa; Ardha Chandra; Chakra; Samudra |
| Nabhasa / Sankhya | 7 | Veenaa; Daama; Paasa; Kedaara; Soola; Yuga; Gola |
| Other Popular | 48 | Subha; Asubha; Gaja-Kesari; Guru-Mangala; Amala; Parvata; Kaahala; Chaamara; Sankha; Bheri; Mridanga; Sreenaatha; Matsya; Koorma; Khadga; Kusuma; Kalaanidhi; Kalpadruma; Lagnaadhi; Hari; Hara; Brahma; Vishnu; Siva; Trilochana; Gouri; Chandikaa; Lakshmi; Saarada; Bhaarathi; Saraswathi; Amsaavatara; Devendra; Indra; Ravi; Bhaaskara; Kulavardhana; Vasumati; Gandharva; Go; Vidyut; Chapa; Pushkala; Makuta; Jaya; Harsha; Sarala; Vimala |
| Named Raja | 3 | Basic Raja; Dharma-Karmadhipati; Vipareeta Raja |

### Numbered sets and engine identity

| Set code | Section | Rows | Engine identity |
|---|---|---:|---|
| `PVR_RAJA_ADVANCED` | §11.7.3 | 18 | `YOGA_RAJA_ADVANCED`; `PVR_CH11_RAJA_ADV_01..18` |
| `PVR_RAJA_SAMBANDHA` | §11.8 | 15 | `YOGA_RAJA_SAMBANDHA`; `PVR_CH11_RAJA_SAMBANDHA_01..15` |
| `PVR_DHANA` | §11.9 | 13 | `YOGA_DHANA`; basic plus twelve lagna variants |
| `PVR_DARIDRA` | §11.10 | 13 | `YOGA_DARIDRA`; `PVR_CH11_DARIDRA_01..13` |

Daridra 10 remains `NOT_EVALUATED`: it requires PVR's forward-referenced Ashtakavarga
benefic/malefic-house classification. Pisces Dhana is `PARTIAL`: the printed/extracted first
alternative names Moon in both the 5th and 11th, so only its unambiguous second alternative
is coded.

### Chara Karaka subset

Eighteen numbered rules explicitly depend on Chara Karakas:

- Advanced Raja 01-05 use AK and, in 01-02, PK.
- Raja Sambandha 01, 03-12, 14-15 use AK and/or AmK; only 02 and 13 contain no Chara
  Karaka condition.

They must be filterable as a subdivision rather than hidden inside Raja umbrella codes.
Migration 137 tags `CHARA_KARAKA` plus the specific `AK`, `AMK`, and `PK` anchors.

### Subdivision axes

Each source variant needs orthogonal classifications for: (1) book set and Nabhasa subset;
(2) computational dependency; (3) reference anchor such as Sun, Moon, Lagna, AK, AmK, PK,
AL, A7, A9, HL or GL; (4) evaluation scope; (5) rule role — formation, qualification,
cancellation, magnitude, effectiveness or outcome; and (6) identity relationship —
independent, alias, source variant, alternate predicate, umbrella or qualification.
Advanced Raja 18 is an effectiveness modifier, not a peer formation.

### Implementation sequence

1. Migration 137: yoga-set hierarchy, source-variant inventory and dependency/anchor tags;
   seed the 9 named PVR additions and all 59 numbered entries; tag all 18 Chara Karaka rules.
   Migration 139 backfills the 18 original PVR variants plus Vipareeta Raja, so every
   currently emitted PVR engine variant has an inventory row.
   `tbl_Rule_Yoga` remains concept-level (its unique key allows one row per YogaCode/source),
   while `tbl_Rule_YogaVariant` owns individual source predicates. Migration 142 exposes both
   through `vw_YogaVariantRules`. Migrations 140-141 are retained as historical no-op markers:
   their attempted inserts were atomically rejected and only their legacy ledger rows landed.
2. Resolve identity: Basic Raja; independent Hari/Hara; PVR's first Brahma formation versus
   the existing second form; Kalpadruma/Parijata; Nipuna/Budha-Aaditya; Nabhasa Chaapa versus
   Popular Chapa; named Ravi versus the Ravi set; lunar Sakata versus Nabhasa Sakata.
3. Complete predicates, Daridra 10 after its Ashtakavarga source, and the damaged Pisces
   Dhana alternative from a clean scan.
4. Model Raja magnitude separately: closeness, affliction, combustion, dignity, avastha and
   Dasavarga Amsabala.
5. Add positive/broken-condition tests for every Chara Karaka rule, including Rahu-as-AK,
   missing-karaka, dispositor, AL/A9, special-Lagna and multi-varga cases.
6. Group the UI by book set, allow dependency filters, and report inventory coverage
   separately from present/absent chart results.

Additions in this document use the `OTHERS` source corpus unless a definition is
specifically sourced to P. V. R. Narasimha Rao, in which case it uses
`PVR-SPECIFIC`. Raman's original numbered collection remains `BVR-300`.

## Scope and method

Candidates were taken first from `SRC_PVR_INTEGRATED`, chapter 11, and compared
by exact name with the complete Raman OCR. Name absence is only a discovery signal:
every formula must still be compared semantically to detect aliases and variants.

Local BPHS, *Saravali*, and *Brihat Jataka* scans need OCR plus edition/translator
metadata before they can support active rule rows. They form a second research wave.

## Current status (2026-09-13, verified against the DB + engine source, not just docs)

PVR chapter 11 (§11.2–11.10) describes **~98 individually-named yogas** plus **58 further
numbered combinations that PVR himself leaves unnamed** (§11.7.3 "More Raja Yogas" ×18,
§11.8 Raaja Sambandha ×15, §11.9 Dhana ×12 per-lagna, §11.10 Daridra ×13). Full breakdown:
[`../../cli/reading/PVR_read_horoscope.md`](../../cli/reading/PVR_read_horoscope.md) is
about *reading method*, not the yoga list — the count sits here and in `pvr-coverage.md`
Ch. 11.

`db/079_add_yoga_type_and_rule.sql` transcribes every evaluator predicate directly from
`src/Ikiastrro.Core/Engines/Yoga/*.cs`: **223 distinct `YogaCode`s are tracked; 146 have a
real coded predicate** (this count predates the 2026-09-17 P0 closure below, which adds 9
more directly-coded `YogaCode`s outside that migration's row set). Cross-checking those 146
against PVR's ~98 named yogas one by one: **~87/98 (~89%) are already covered** — almost
entirely because PVR draws on the same classical corpus as `SRC_RAMAN_300_COMBINATIONS`,
this project's primary yoga source, so a yogas gets covered "for free" once its Raman form is
implemented. `YOGA_VIPAREETA_RAJA` in particular is **already implemented** (evaluated from
PVR's broad dusthana-lord-in-dusthana definition — see the Horoscope Explorer review below).
As of 2026-09-17, the 9 confirmed-gap P0 formations are also implemented (see "Recommended
PVR additions" below), bringing PVR's named-yoga coverage to **~96/98 (~98%)**. The same day,
all 59 of PVR's unnamed numbered combinations (§11.7.3/11.8/11.9/11.10) were also transcribed
(see "Unnamed numbered combinations" below) — between the named-yoga and numbered-combination
work, chapter 11 of `SRC_PVR_INTEGRATED` is now transcribed. The P1 identity/alias cases were
closed on 2026-09-25; one Daridra item remains source-blocked and the Pisces Dhana first
alternative remains partial because its printed/extracted planet name is damaged.

The 59 numbered combinations (§11.7.3/11.8/11.9/11.10) now have individual engine results
and migration-137 inventory rows; the umbrella YogaCodes remain grouping identities only.

## Recommended PVR additions

All 9 P0 items below were transcribed 2026-09-17 into
[`../../../src/Ikiastrro.Core/Engines/Yoga/PvrChapter11YogaEvaluator.cs`](../../../src/Ikiastrro.Core/Engines/Yoga/PvrChapter11YogaEvaluator.cs)
(tests: `PvrChapter11YogaEvaluatorTests.cs`, 10 cases) and wired into `ProductionYogaEngine.cs`.
This closes the P0 slice of the plan — the confirmed-gap set is now fully coded. See §"P0
closure" below for notes on the two predicates that needed a specific reading choice.

| Priority | YogaCode | PVR locator | Relationship to Raman | Inputs |
|---|---|---|---|---|
| ~~P0~~ **DONE** | ~~`YOGA_MAALA`~~ | §11.5.2 Dala, p.119–120 | Was not tracked anywhere — not even a name-only catalog row (its pair, Sarpa Yoga, is implemented as `YOGA_SARPA`) | D1, natural nature, quadrant occupancy |
| ~~P0~~ **DONE** | ~~`YOGA_SUBHA`~~ | §11.6, p.124 | New named formation | D1, natural nature |
| ~~P0~~ **DONE** | ~~`YOGA_ASUBHA`~~ | §11.6, p.124 | New inverse formation | D1, natural nature |
| ~~P0~~ **DONE** | ~~`YOGA_GURU_MANGALA`~~ | §11.6, p.125 | New; not Raman 24 Chandra-Mangala | D1, conjunction/opposition |
| ~~P0~~ **DONE** | ~~`YOGA_CHAMARA`~~ | §11.6, p.125–126 | New normalized yoga | D1, dignity, aspect |
| ~~P0~~ **DONE** | ~~`YOGA_KHADGA`~~ | §11.6, p.126–127 | New normalized yoga | D1, exchange/lordship |
| ~~P0~~ **DONE** | ~~`YOGA_LAGNAADHI`~~ | §11.6, p.129 | New Lagna counterpart to Adhi | D1, nature, aspects |
| ~~P0~~ **DONE** | ~~`YOGA_SAARADA`~~ | §11.6, p.131 | New normalized yoga | D1, strength |
| ~~P0~~ **DONE** | ~~`YOGA_DHARMA_KARMADHIPATI`~~ | §11.7.1, p.134 | PVR Raja-yoga specialization | D1, lord association |
| ~~P0~~ **DONE** | ~~`YOGA_VIPAREETA_RAJA`~~ | §11.7.1, pp.134–135 | Implemented — see "Current status" above and the Horoscope Explorer review below | D1, lord association |

### P0 closure notes (2026-09-17)

- **`YOGA_LAGNAADHI`** is the 7th/8th-from-Lagna counterpart to classical Adhi Yoga (Moon-reckoned,
  6th/7th/8th) — implemented as its own predicate, not a variant row on the existing Adhi Yoga
  code, since the reckoning point and house set both differ.
- **`YOGA_DHARMA_KARMADHIPATI`** reuses §11.7.1's own 3-way "association" definition (conjunction,
  mutual graha drishti, or parivartana/exchange) applied to the 9th and 10th lords specifically.
  The same `RajaAssociation` helper now powers completed `YOGA_BASIC_RAJA` across every
  distinct quadrant/trine lord pair.
- **`YOGA_CHAMARA`** implements both of PVR's stated alternate forms (lagna-lord exaltation +
  Jupiter aspect, OR two benefics joined in 7th/9th/10th) as an OR, matching the Definition's own
  "or" wording — no reading ambiguity here, unlike several Raman combos in `bvyoga.md`.
| **DONE** | `YOGA_BASIC_RAJA` | §11.7.1, pp.133–134 | General kendra/trikona association | D1 |
| **DONE** | `YOGA_HARI` | §11.6, p.129 | Split from Raman's combined concept | D1 |
| **DONE** | `YOGA_HARA` | §11.6, p.129 | Split from Raman's combined concept | D1 |
| **DONE** | `YOGA_BRAHMA_TRIMURTI` | §11.6, pp.129–130 | First Brahma/Trimurti form; distinct from existing `YOGA_BRAHMA` | D1 |
| **DONE** | existing `YOGA_PARIJATHA` | §11.6, pp.127–129 | `PVR_CH11_KALPADRUMA` alias/source variant | D1+D9 |

## Additional PVR variants

- `PVR_CH11_BHASKARA` adds Moon twelfth from Sun; compare Raman 159.
- `PVR_CH11_KULAVARDHANA` clarifies Raman 70: each planet may be fifth from
  Lagna, Moon, or Sun — already the shipped `YOGA_KULAVARDHANA` predicate
  (`db/079_add_yoga_type_and_rule.sql`: "a natural benefic in the 5th from each of Lagna,
  Sun and Moon"). This variant is **done**, not open.
- Keep PVR Matsya, Koorma, Kusuma, and Kalanidhi separate from Raman variants — already
  shipped as distinct codes (`YOGA_MATSYA`, `YOGA_KURMA`, `YOGA_KUSUMA`, `YOGA_KALANIDHI`).

## Unnamed numbered combinations (§11.7.3 / 11.8 / 11.9 / 11.10) — DONE 2026-09-17

All 4 blocks (59 items) were transcribed into
[`../../../src/Ikiastrro.Core/Engines/Yoga/PvrChapter11NumberedYogaEvaluator.cs`](../../../src/Ikiastrro.Core/Engines/Yoga/PvrChapter11NumberedYogaEvaluator.cs)
(tests: `PvrChapter11NumberedYogaEvaluatorTests.cs`, 60 cases — one per item plus a coverage
check) and wired into `ProductionYogaEngine.cs`.

| Section | Count | Home | Result |
|---|---|---|---|
| §11.7.3 "More Raja Yogas" | 18 | new `YOGA_RAJA_ADVANCED`, 18 `SourceVariantCode` rows (`PVR_CH11_RAJA_ADV_01`..`18`) | done |
| §11.8 Raaja Sambandha Yogas | 15 | new `YOGA_RAJA_SAMBANDHA`, 15 rows (`PVR_CH11_RAJA_SAMBANDHA_01`..`15`), all keyed off AK/AmK (chara karakas) | done |
| §11.9 Dhana Yogas | 12 per-lagna + 1 basic principle = 13 | extra `SourceVariantCode` rows under existing `YOGA_DHANA` (`PVR_CH11_DHANA_BASIC` + `PVR_CH11_DHANA_<SIGN>` ×12) | done |
| §11.10 Daridra Yogas | 13 | extra rows under existing `YOGA_DARIDRA` (`PVR_CH11_DARIDRA_01`..`13`) | done — 12/13 coded, 1 `NOT_EVALUATED` |

Notes on the build:
- Reused already-existing-but-previously-unsurfaced engine primitives: `bundle.CharaKarakaByPlanet`
  (AK/AmK/PK reverse lookup), the "A7"/"A9" bhava arudhas and "HL"/"GL" special lagnas (all
  already computed by `SpecialPointCalculator`, confirming `pvr-coverage.md`'s Ch.5 "not built"
  note for Ghati/Sree/Bhaava Lagna was stale — all 4 special lagnas were built 2026-09-13),
  `VaiseshikamsaCalculator.Shadvarga` (§11.7.3 item 7's 6-chart aspect check), and
  `bundle.Vargottama` (item 15).
- §11.10 Daridra item (10) — "benefics are in malefic houses and malefics are in benefic
  houses" — is the corpus's one genuine `NOT_EVALUATED` row here: PVR's own text forward-references
  the Bhinnashtakavarga benefic/malefic house tables from the immediately following chapter
  (§12.2) without restating an operational rule in Ch.11 itself. Not guessed.
- §11.9 Dhana item (12), Pisces lagna — the printed 5th/11th-house clause reads "Moon is in the
  5th house and in the 11th house" with no second planet named for the 11th house, an apparent
  OCR/print gap (every other of the 12 lagna rows names two distinct planets). Only the fully-
  stated second (lagna) alternative is coded for Pisces; the first is left unimplemented rather
  than guessed.
- Found and fixed a real null-reference bug in `Dispositor()` while writing tests: it assumed the
  looked-up planet was always present in `c.Planets`, which crashed on any dusthana-lord chain
  (§11.10 item 7) or AmK-dispositor lookup (§11.8 item 1) against a chart missing that planet.
  Made it return `PlanetName?` and fixed both call sites — this was a latent crash risk, not
  merely a test-fixture issue, since nothing in the type signature guaranteed the planet's
  presence.

## Classical-source wave

1. OCR edition, translator, contents, and yoga chapters for BPHS, *Saravali*, and
   *Brihat Jataka*.
2. Register edition-specific `SRC_*` rows before activating rules.
3. Compare predicates, not names, against Raman and PVR.
4. Prefer source variants for shared concepts; create new `YogaCode` only for a
   distinct formation.
5. Keep disease, death, sex, caste, and moral claims neutral and source-attributed.

## Next implementation slice

1. ~~**Implement the 9 open P0 PVR additions**~~ **DONE 2026-09-17** — `YOGA_MAALA`,
   `YOGA_SUBHA`, `YOGA_ASUBHA`, `YOGA_GURU_MANGALA`, `YOGA_CHAMARA`, `YOGA_KHADGA`,
   `YOGA_LAGNAADHI`, `YOGA_SAARADA`, `YOGA_DHARMA_KARMADHIPATI` — see "P0 closure notes" above.
2. ~~**Resolve the P1 identity/alias cases**~~ **DONE 2026-09-25**: split `YOGA_HARI`/`YOGA_HARA` out of
   `YOGA_HARIHARA_BRAHMA`, add `YOGA_BRAHMA_TRIMURTI` (distinct from the existing
   `YOGA_BRAHMA`), evaluate `YOGA_BASIC_RAJA` against Raman 245–263 (reuse the `RajaAssociation`
   helper introduced for `YOGA_DHARMA_KARMADHIPATI`), add the Kalpadruma/Parijata variant row
   onto existing `YOGA_PARIJATHA`. Implemented in `PvrChapter11YogaEvaluator` and registered
   by migration 138; Kalpadruma correctly requires D9 and is `NOT_EVALUATED` when D9 is absent.
3. ~~**Transcribe the 58 unnamed numbered combinations**~~ **DONE 2026-09-17** (actually 59 —
   see the "Unnamed numbered combinations" section above for the per-block breakdown).
4. Prepare applicability rows for all of the above but defer database application until
   corpus review (unchanged policy).

## Horoscope Explorer parity review — Ramakrishnan P

`PVR_CH11_VIPAREETA_RAJA` is evaluated from PVR's broad definition: a lord of 6, 8 or 12 occupying a dusthana is sufficient; PVR separately describes the ideal three-lord, unjoined form. `HE_ANIVAHUPPU_STRUCTURAL` uses the comparison-source rule: one of nine grahas is alone in its sign and the other eight divide four/four between the open semicircles on its two sides.

`HE_VIDYA_NATAL` and `HE_ARISHTA_GENERIC` are deliberately `NOT_EVALUATED`. The located Raman Vidya rule is for an electional education chart, not a natal predicate. “Arishta” is a class, and the supplied report does not identify the condition that fired. Neither is inferred merely to reproduce a vendor result.
