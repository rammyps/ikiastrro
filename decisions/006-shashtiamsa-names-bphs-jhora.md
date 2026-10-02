---
status: accepted
date: 2026-10-02
workstream: cli (core `ShashtiamsaDeityTable`), ui (Astro Facts → Vargas "Ṣaṣṭyaṃśa (D60) deities")
---

# 006 — Ṣaṣṭyaṃśa (D60) names and natures follow BPHS as JHora shows them

## Decision

Core `ShashtiamsaDeityTable` carries the 60 Ṣaṣṭyaṃśa names in the BPHS order that Jagannatha
Hora shows (Basics → Amsa rulers → Shashtyamsa (D-60)), with each part's benefic / malefic
nature from Maitreya8. The part number and the odd-forward / even-reversed reading are unchanged.

rammyps chose this on 2026-10-02 when the new Astro Facts D60 table showed different deities
from JHora for the same chart.

## Why

- **The old list was unsourced and drifted.** `docs/cli/reading/d60-shashtiamsa.md` §4 said
  "classical set per BPHS" with no page. It matched BPHS through part 29, ran one place early
  from 30 to 46 (it had no Kamalākara), and listed different names from 47 to 60. On
  `1_RamakrishnanP` that gave the Moon Pāśa (JHora: Komala), Jupiter Kāla (Utpāta) and Saturn
  Viṣadagdha (Pūrṇacandra).
- **Three independent sources agree on the names.** JHora's export (29 list positions checked,
  2 to 59), PyJHora `const.py` (varga 60) and Maitreya8 `Lang::getShastiamsaName` list all 60
  identically, spelling aside.
- **The nature is sourced.** Maitreya8 `GenericTableWriter::writeShastiamsaLords` holds a
  `k_shastiamsa_benefic[60]` table indexed exactly as here. It differs from the old list beyond
  the renumbering at parts 26–28 (Ārdrā, Kalināśa, Kṣitīśa), which it marks benefic. JHora shows
  no nature.

## Consequences

- `ShashtiamsaDeityTableTests` pins the 29 JHora positions and the three even-sign placements
  above.
- The Raman yoga evaluators that read `IsMalefic` (Raja 248's "clean" exaltations, 185's cruel
  Ṣaṣṭyaṃśa, the affliction combinations' and the family evaluator's `CruelShashtiamsa`) can change
  outcome, so every person's D1 yoga evaluations were refreshed (`refresh-yogas`).
- Two synthetic test charts sat on parts whose nature flipped; their degrees moved to parts with
  the intended nature.
- Combination 234's Mṛdvaṃśa ("Mridu", part 19) is unchanged.
