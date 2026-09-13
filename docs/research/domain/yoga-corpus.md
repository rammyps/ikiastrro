---
last_updated: 2026-09-13
---

# Yoga corpus expansion beyond Raman's 300

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
real coded predicate.** Cross-checking those 146 against PVR's ~98 named yogas one by one:
**~87/98 (~89%) are already covered** — almost entirely because PVR draws on the same
classical corpus as `SRC_RAMAN_300_COMBINATIONS`, this project's primary yoga source, so a
yogas gets covered "for free" once its Raman form is implemented. `YOGA_VIPAREETA_RAJA` in
particular is **already implemented** (evaluated from PVR's broad dusthana-lord-in-dusthana
definition — see the Horoscope Explorer review below) — it is **not** an open item any more,
correcting this doc's previous P0 listing.

The 58 unnamed numbered combinations (§11.7.3/11.8/11.9/11.10) have **no per-rule
transcription at all** — only two generic catch-alls exist (`YOGA_DHANA`, `YOGA_DARIDRA`,
each covering "multiple classical forms" as one summarized predicate, not PVR's specific
per-item list).

## Recommended PVR additions

| Priority | YogaCode | PVR locator | Relationship to Raman | Inputs |
|---|---|---|---|---|
| P0 | `YOGA_MAALA` | §11.5.2 Dala, p.119–120 | **Not tracked anywhere yet** — not even a name-only catalog row (its pair, Sarpa Yoga, is implemented as `YOGA_SARPA`) | D1, natural nature, quadrant occupancy |
| P0 | `YOGA_SUBHA` | §11.6, p.124 | New named formation | D1, natural nature |
| P0 | `YOGA_ASUBHA` | §11.6, p.124 | New inverse formation | D1, natural nature |
| P0 | `YOGA_GURU_MANGALA` | §11.6, p.125 | New; not Raman 24 Chandra-Mangala | D1, conjunction/opposition |
| P0 | `YOGA_CHAMARA` | §11.6, p.126 | New normalized yoga | D1, dignity, aspect |
| P0 | `YOGA_KHADGA` | §11.6, p.127 | New normalized yoga | D1, exchange/lordship |
| P0 | `YOGA_LAGNAADHI` | §11.6, p.129 | New Lagna counterpart to Adhi | D1, nature, aspects |
| P0 | `YOGA_SAARADA` | §11.6, p.131 | New normalized yoga | D1, strength |
| P0 | `YOGA_DHARMA_KARMADHIPATI` | §11.7.1, p.134 | PVR Raja-yoga specialization | D1, lord association |
| ~~P0~~ **DONE** | ~~`YOGA_VIPAREETA_RAJA`~~ | §11.7.1, pp.134–135 | Implemented — see "Current status" above and the Horoscope Explorer review below | D1, lord association |
| P1 | `YOGA_BASIC_RAJA` | §11.7.1, pp.133–134 | Compare with Raman 245–263 | Any chart, lord association |
| P1 | `YOGA_HARI` | §11.6, p.129 | PVR split of Raman 51A; currently only exists merged into `YOGA_HARIHARA_BRAHMA` | D1 |
| P1 | `YOGA_HARA` | §11.6, p.129 | PVR split of Raman 51B; currently only exists merged into `YOGA_HARIHARA_BRAHMA` | D1 |
| P1 | `YOGA_BRAHMA_TRIMURTI` | §11.6, p.129 | PVR split of Raman 51C; not the already-implemented `YOGA_BRAHMA` (that code is PVR's separate *second* Brahma-yoga variant, §11.6 note (2)) | D1 |
| P1 | existing `YOGA_PARIJATHA` | §11.6, pp.127–129 | Kalpadruma/Parijata synonym; add variant, not concept | D1+D9 |

## Additional PVR variants

- `PVR_CH11_BHASKARA` adds Moon twelfth from Sun; compare Raman 159.
- `PVR_CH11_KULAVARDHANA` clarifies Raman 70: each planet may be fifth from
  Lagna, Moon, or Sun — already the shipped `YOGA_KULAVARDHANA` predicate
  (`db/079_add_yoga_type_and_rule.sql`: "a natural benefic in the 5th from each of Lagna,
  Sun and Moon"). This variant is **done**, not open.
- Keep PVR Matsya, Koorma, Kusuma, and Kalanidhi separate from Raman variants — already
  shipped as distinct codes (`YOGA_MATSYA`, `YOGA_KURMA`, `YOGA_KUSUMA`, `YOGA_KALANIDHI`).

## Unnamed numbered combinations (§11.7.3 / 11.8 / 11.9 / 11.10) — not yet transcribed

PVR leaves these four blocks unnamed (numbered lists, not individually titled yogas like
§11.2–11.6). None has per-item DB rows today; only two generic catch-alls exist. Each block
becomes its own `SourceVariantCode` set under the existing `YogaCode` where one already
exists, or a new code where the block has no home yet:

| Section | Count | Existing home | Action |
|---|---|---|---|
| §11.7.3 "More Raja Yogas" | 18 | none | new `YogaCode`s (or one umbrella `YOGA_RAJA_ADVANCED` with 18 `SourceVariantCode` rows) — items (1)–(18), pp.137–139 |
| §11.8 Raaja Sambandha Yogas | 15 | none | new umbrella `YOGA_RAJA_SAMBANDHA` with 15 `SourceVariantCode` rows — items (1)–(15), pp.139–141, all keyed off AK/AmK (chara karakas) |
| §11.9 Dhana Yogas | 12 (one per lagna) + 1 basic principle | `YOGA_DHANA` (generic catch-all, no per-lagna rows) | replace the single summary predicate with 12 `SourceVariantCode` rows, one per lagna (Ar–Pi), pp.141–142 |
| §11.10 Daridra Yogas | 13 | `YOGA_DARIDRA` (generic catch-all, no per-item rows) | replace the single summary predicate with 13 `SourceVariantCode` rows, items (1)–(13), pp.142–144 |

Transcribe these **after** the named P0 set above — they are numerous but each is a single
short conditional (lord placements/aspects), lower architectural risk than the named
formations, which is why they were deferred first.

## Classical-source wave

1. OCR edition, translator, contents, and yoga chapters for BPHS, *Saravali*, and
   *Brihat Jataka*.
2. Register edition-specific `SRC_*` rows before activating rules.
3. Compare predicates, not names, against Raman and PVR.
4. Prefer source variants for shared concepts; create new `YogaCode` only for a
   distinct formation.
5. Keep disease, death, sex, caste, and moral claims neutral and source-attributed.

## Next implementation slice

1. **Implement the 9 open P0 PVR additions** (table above): `YOGA_MAALA`, `YOGA_SUBHA`,
   `YOGA_ASUBHA`, `YOGA_GURU_MANGALA`, `YOGA_CHAMARA`, `YOGA_KHADGA`, `YOGA_LAGNAADHI`,
   `YOGA_SAARADA`, `YOGA_DHARMA_KARMADHIPATI`. (`YOGA_VIPAREETA_RAJA`, the ninth item
   from the previous pass, is done — dropped from this list, `YOGA_MAALA` takes its slot
   as a newly-confirmed gap.)
2. **Resolve the 4 P1 identity/alias cases**: split `YOGA_HARI`/`YOGA_HARA` out of
   `YOGA_HARIHARA_BRAHMA`, add `YOGA_BRAHMA_TRIMURTI` (distinct from the existing
   `YOGA_BRAHMA`), evaluate `YOGA_BASIC_RAJA` against Raman 245–263, add the
   Kalpadruma/Parijata variant row onto existing `YOGA_PARIJATHA`.
3. **Transcribe the 58 unnamed numbered combinations** (§11.7.3/11.8/11.9/11.10 — see the
   table above), lowest priority: more rows, but each is architecturally simple (a single
   lord-placement/aspect conditional) compared to the named formations in steps 1–2.
4. Prepare applicability rows for all of the above but defer database application until
   corpus review (unchanged policy).

## Horoscope Explorer parity review — Ramakrishnan P

`PVR_CH11_VIPAREETA_RAJA` is evaluated from PVR's broad definition: a lord of 6, 8 or 12 occupying a dusthana is sufficient; PVR separately describes the ideal three-lord, unjoined form. `HE_ANIVAHUPPU_STRUCTURAL` uses the comparison-source rule: one of nine grahas is alone in its sign and the other eight divide four/four between the open semicircles on its two sides.

`HE_VIDYA_NATAL` and `HE_ARISHTA_GENERIC` are deliberately `NOT_EVALUATED`. The located Raman Vidya rule is for an electional education chart, not a natal predicate. “Arishta” is a class, and the supplied report does not identify the condition that fired. Neither is inferred merely to reproduce a vendor result.
