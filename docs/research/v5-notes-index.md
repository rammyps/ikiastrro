---
last_updated: 2026-09-22
---

# "v5" notes index

Informal label only — this repo has no numbered-release convention (see `ROADMAP.md`: pure
Now/Next/Later flow, `git tag` + GitHub Release per ship, no version numbers). "v5" is rammyps's
own shorthand across a run of chat sessions on 2026-09-18 for a batch of domain-research notes.
**2026-09-22: scope confirmed** — rammyps's direction was to triage the full batch below (not
just one note) into real `FEAT-*` rows and Now/Next/Later slots, per this repo's existing
system, with no version number introduced. That triage is done; every row below now has a real
anchor. This file has accordingly shrunk to a pointer, per its own "If this grows" note — the
roadmap is the source of truth from here.

| Note | Scope | `FEAT-*` anchor | Status |
|---|---|---|---|
| [`domain/lifearea-varga-charakaraka-synthesis.md`](domain/lifearea-varga-charakaraka-synthesis.md) | Closing the "Missing Web" gap for divisional charts + chara karakas, plus a connective layer between them | `FEAT-VARGA-02` / `FEAT-KARAKA-06` / `FEAT-HOUSE-05` (all ROADMAP Now/Next, added 2026-09-22) | Triaged; `FEAT-VARGA-02`/`FEAT-KARAKA-06` still have open sub-questions flagged in `masterproduct.md` |
| [`domain/sign-benefic-malefic.md`](domain/sign-benefic-malefic.md) | Whether dignity color implies sign benefic/malefic (it doesn't); proposed synthesis method (lord's functional nature + occupants + aspects + lord's dignity) | `FEAT-HOUSE-06` (ROADMAP Later, added 2026-09-22) | Triaged to Later — needs a source pass before schema work, per the note's own caution |
| [`domain/argala-virodhargala-drishti-lifematters.md`](domain/argala-virodhargala-drishti-lifematters.md) | Rāśi dṛṣṭi + graha dṛṣṭi + Argala/Virodhargala as a four-relationship reading method, tied to the life-matter table | `FEAT-RELATIONSHIP-04` (ROADMAP Next) | Built 2026-09-19 incl. UI; `masterproduct.md` row was stale at Planned·0%, corrected 2026-09-22 to In progress·70% |
| [`domain/nakshatra-lord-sublord-dasha-crossref.md`](domain/nakshatra-lord-sublord-dasha-crossref.md) (aka "nakshatra_sublords") | Nakshatra/pada lord ↔ KP L1–L7 sub-lord chain, nakshatra color scheme, dasha (Maha/Antar/Pratyantar) ↔ Rasi/Nakshatra, plus a design-review follow-up on `tbl_Fact_KpSubLordChain` (why persisted vs. derived, missing `RuleSetId`, confirms scope is all planets not just Moon) | `FEAT-NAKSHATRA-02` / `FEAT-DATA-07` (ROADMAP Now, added 2026-09-22) | Triaged; **Pada Lord chain rejected 2026-09-22 (rammyps) — "not usually done,"** `FEAT-NAKSHATRA-02` narrowed to the plain Nakshatra Lord→L1–L7 chain only; migrations 125–126 stay committed/unconsumed. Nakshatra color scheme stays unslotted in ROADMAP Later — genuinely undecided, needs rammyps |
| [`domain/vedic_reading_layers.md`](domain/vedic_reading_layers.md) | A 12-stage reading→research architecture (data/calc integrity → foundation → strength/condition → relational combinations → natal promise → life-matter application → dasha activation → transit/annual trigger → context → synthesis/confidence → validation → source/version record), cross-linking most existing PVR notes; corrects an earlier chat mislabel ("Systems' Approach" is Choudhry's, not PVR's) | No single anchor — most stages already map onto existing `FEAT-*` rows (cross-linked in the doc). Stage 01 → ROADMAP Later "Data & Calculation Integrity surfacing" (new); Stage 12 → judged already satisfied by the existing `SRC_*`/`RuleSetId`/`pvr-coverage.md` discipline, no slot needed; Stages 05/09/11 judged not to need their own engine feature (ride on `FEAT-HOUSE-05`, low-priority UI input, or a research discipline rather than a product feature, respectively) | Triaged 2026-09-22; 5 "suggested additions" parked in ROADMAP Later as an explicit unconfirmed list, not real candidates yet |
| [`domain/ashtakavarga-varga-extension.md`](domain/ashtakavarga-varga-extension.md) | Extends Stage 03's Ashtakavarga item two ways: (1) apply the single natal SAV/BAV bindu table to every varga chart's placements, read-side, rather than recomputing per varga (flags an unconfirmed cross-reference-vs-recompute scope decision) — and (2) a new `AshtakavargaVargaCompareChart` stacked-bar chart, By House / By Sign toggle, % bindu share per varga, same shape as Astro Facts 3.4 Amsabala's per-graha stacked bar | `FEAT-ASHTAKAVARGA-02` (ROADMAP Now, added 2026-09-22) | The note's premise ("Ch.12 not built") was stale — `FEAT-ASHTAKAVARGA-01` shipped before 2026-09-16; `pvr-coverage.md` Ch.12 corrected 2026-09-22. Now unblocked and triaged to Now |
