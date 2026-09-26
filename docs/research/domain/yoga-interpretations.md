---
last_updated: 2026-09-27
reflects: 411-variant corpus, migrations 148-149, generated 7x7 LifeMatter matrix
---

# Yoga interpretations — research and delivery plan

## Current coverage

- Corpus: 411 source variants representing 235 concepts.
- Interpretation rows: 2 pilot rows, both for Budha-Aditya Yoga.
- UI: source variants remain separate; a source-specific interpretation may fall back to a
  concept-level interpretation, but never to another authority's text.
- Variant-level interpretation keys are required for umbrella concepts such as Dhana,
  Daridra and Raja, whose numbered combinations have different stated effects.

## Recommended research sources

1. **B. V. Raman, _Three Hundred Important Combinations_.** Primary source for all
   `RAMAN_300_*` definitions, results and Raman's qualifications. This is the first pass
   because every BVR variant already has an exact printed/scan locator.
2. **P. V. R. Narasimha Rao, _Vedic Astrology: An Integrated Approach_.** Primary source
   for `PVR_CH11_*`, particularly Jaimini/Chara-Karaka and Arudha-dependent variants.
3. **Brihat Parashara Hora Shastra.** Use chapters on Yoga Karakas, Nabhasa Yogas, Raja
   Yogas, Dhana Yogas, Daridra Yogas and special combinations. Record edition plus chapter
   and verse because modern recensions differ.
4. **Phaladeepika by Mantresvara.** Strong cross-check for named yogas and their stated
   effects; a local English translation is already registered as `SRC_PHALADEEPIKA`.
5. **Saravali by Kalyana Varma.** Valuable for solar/lunar yogas, planetary combinations
   and outcome language. Register the chosen edition before importing claims.
6. **Jataka Parijata by Vaidyanatha Dikshita.** Useful for later synthesis, cancellation,
   strength qualifications and source comparison. Keep its claims separate from BPHS.
7. **Brihat Jataka by Varahamihira.** Early control text for compact named-yoga and
   multi-planet combination results; `SRC_BRIHAT_JATAKA_1` already exists but must be
   expanded beyond chapter 1 for this work.
8. **Hora Sara by Prithuyasas** and **Uttara Kalamrita.** Secondary comparison pass for
   yogas absent or terse in the first six sources.

## Authoring rules

- Store a concise paraphrase, not copied modern translation prose.
- Preserve `SourceRefCode`, `SourceVariantCode`, chapter/verse or printed/scan locator.
- Separate formation, promised result, qualifications/cancellations and timing.
- Use neutral modern language for disease, disability, sex, caste, death and morality.
- Mark conflicts as `Disputed`; do not silently merge authorities.
- Do not convert a source's promised result into certainty. Phrase it as a traditional
  indication whose expression depends on strength, repetition, cancellation and dasha.

## Delivery order

1. BVR 1–24 and PVR equivalents (shared foundational yogas).
2. BVR/PVR Nabhasa sets.
3. Dhana, Daridra and Raja variants at source-variant level.
4. Family, progeny and fortune groups.
5. Affliction/adversity group with sensitive-content review.
6. Cross-source comparison against BPHS, Phaladeepika, Saravali and Jataka Parijata.

## Yoga × LifeMatter 7×7 matrix

Each active source variant has seven ranked paths. Every path stores a direct foreign key to
`tbl_Rule_LifeMatterFocus`; the hierarchy is exposed through `vw_YogaLifeMatter7x7`:

1. Area — LifeMatter category.
2. SA1 — specific LifeMatter.
3. SA2 — divisional subject, or Natal promise when none is assigned.
4. SA3 — focus kind.
5. SA4 — focus reference code.
6. SA5 — house number or special-point code.
7. SA6 — applicable fixed or Chara karakas.

The generator combines TF-IDF similarity, an explicit Vedic-domain taxonomy prior and the
existing focus priority. Generated mappings start as `PROPOSED`; research review advances
them to `REVIEWED` and textual/source verification to `VERIFIED`. Regeneration replaces only
`PROPOSED` rows, so reviewed decisions are retained. `REJECTED` is reserved for mappings
that should not appear in readings.

Current generated coverage is 2,877 paths across 411 variants: seven paths for every variant,
with no incomplete variants. These scores prioritize research and UI ordering; they are not
claims of predictive validity. Statistical validation requires outcome-labelled chart cohorts.

### Review order

1. Review each yoga's rank 1 path and canonical family match (Dhana, Raja, Roga, etc.).
2. Review ranks 2–7 for redundancy and adjacent LifeMatter coverage.
3. Verify Chara-karaka and special-point paths against the PVR source locator.
4. Promote accepted rows to `REVIEWED`, then `VERIFIED` after interpretation sourcing.
5. Use matrix-coverage checks in release validation; all active variants must retain seven paths.
## Implementation record

- Migration 148 makes variant identity available to the UI and permits interpretation rows
  at generic, source and exact-source-variant scopes.
- Migration 149 creates `tbl_Rule_YogaLifeMatterPath`, `vw_YogaLifeMatter7x7` and the
  completeness view; the applied local database contains 2,877 active path rows.
- `scripts/yoga_priority.py` ranks UI/research work using coverage, readiness, source overlap,
  Wilson intervals and interpretation gaps.
- `scripts/yoga_lifematter_matrix.py` combines TF-IDF similarity, explicit yoga-family priors
  and existing LifeMatter focus priority, then generates reviewable JSON and SQL artifacts.
- Focused Python tests cover seven distinct ranks and the canonical Dhana-to-wealth mapping.
- The matrix is intentionally human-in-the-loop: generated rows are proposals and reviewed or
  verified rows are not overwritten by normal regeneration.