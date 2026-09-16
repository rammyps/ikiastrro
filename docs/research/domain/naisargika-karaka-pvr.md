---
last_updated: 2026-09-16
---

# Naisargika karaka — PVR ch. 8 (pg 79)

Normalized form of rammyps's naisargika-karaka worksheet. Backs
`db/086_seed_naisargika_karaka_rules.sql`. Source content is P. V. R. Narasimha Rao,
*Vedic Astrology: An Integrated Approach*, ch. 8 (Karakas — chara, sthira, naisargika).

`SourceRefCode = SRC_PVR_INTEGRATED` on every seeded row. RuleSetId 1.

Two distinct, dedicated tables — not the reserved generic `tbl_Rule_Karaka` (migration 18),
which stays empty/reserved for a future chara/sthira layer:

The follow-on normalization and connection plan across Naisargika, Sthira, Chara, life-matter,
and friendship evaluation is documented in [`../../database/karakafix.md`](../../database/karakafix.md).

| Table | Shape | Rows |
|---|---|---|
| `tbl_Rule_Naisargika_Karakatwas` | one row per (graha, matter) — every matter a graha naturally signifies, and the house it is classically read from | 34 |
| `tbl_Rule_Naisargika_Karakas` | one row per house (1–12) — the single, classically-cited primary karaka graha for that house + a short summary of matters | 12 |

The primary table is a reduction of the grid: for each house it names the one graha
conventionally cited as *the* karaka, drawn from (and consistent with) the fuller grid.

## `tbl_Rule_Naisargika_Karakatwas` — the full grid

| Graha | Matter | House |
|---|---|--:|
| Sun | Self, soul, constitution, health | 1st |
| Sun | Fame and power | 5th |
| Sun | Father and boss | 9th |
| Sun | Career and achievements | 10th |
| Moon | Mind | 1st |
| Moon | Mother and peace of mind | 4th |
| Moon | Friends | 11th |
| Mars | Courage and younger siblings | 3rd |
| Mars | Real estate | 4th |
| Mars | Nyāya scholarship and speculation | 5th |
| Mars | Enemies, disease, accidents and loans | 6th |
| Mercury | Speech | 2nd |
| Mercury | Learning | 4th |
| Mercury | Memory, scholarship and students | 5th |
| Mercury | Work, achievements and honours | 10th |
| Mercury | Credits | 11th |
| Jupiter | Family and wealth | 2nd |
| Jupiter | Traditional learning | 4th |
| Jupiter | Children and intelligence | 5th |
| Jupiter | Teacher, religion and fortune | 9th |
| Jupiter | Elder brother and gains | 11th |
| Venus | Vehicles | 4th |
| Venus | Spouse and marital happiness | 7th |
| Venus | Bed pleasures | 12th |
| Saturn | Followers | 5th |
| Saturn | Servants | 6th |
| Saturn | Longevity and troubles | 8th |
| Saturn | Loss and hospitalization | 12th |
| Rahu | Accidents | 6th |
| Rahu | Occult knowledge | 8th |
| Rahu | Pilgrimage and foreign travel | 9th |
| Ketu | Occult knowledge | 8th |
| Ketu | Pilgrimage and foreign travel | 9th |
| Ketu | Mokṣa | 12th |

## `tbl_Rule_Naisargika_Karakas` — primary karaka per house

| House | From planet | Matters signified |
|---|---|---|
| 1st | Sun | Self, physical constitution, soul, health |
| 2nd | Jupiter | Family, wealth |
| 3rd | Mars | Younger siblings, courage |
| 4th | Moon | Mother |
| 5th | Jupiter | Children |
| 6th | Mars | Enemies |
| 7th | Venus | Wife, husband, marital bliss, relationships |
| 8th | Saturn | Longevity, troubles |
| 9th | Jupiter | Teacher, religion, fortune |
| 10th | Mercury | Work, achievements, honors |
| 11th | Jupiter | Elder siblings |
| 12th | Saturn | Losses |

Cross-check: every `(house, planet)` pair in the primary table also appears in the full
grid — the migration's summary `PRINT` asserts this (`orphanHouse` must be 0).

## Downstream

- `LifeAreaMap.cs` hard-codes a *different* scheme — Sthira karaka per life-area group,
  sourced from B.V. Raman rather than PVR — and is not reconciled against either table here.
- Chara karaka (Atmakaraka..) is computed by `CharaKarakaCalculator`, not stored as a rule
  table; it is unrelated to naisargika (natural, fixed) karaka.
- `docs/research/domain/pvr-coverage.md` ch. 8 row tracks this against the book.
