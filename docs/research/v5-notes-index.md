---
last_updated: 2026-09-19
---

# "v5" notes index

Informal label only — this repo has no numbered-release convention (see `ROADMAP.md`: pure
Now/Next/Later flow, `git tag` + GitHub Release per ship, no version numbers). "v5" is rammyps's
own shorthand across a run of chat sessions on 2026-09-18 for a batch of domain-research notes;
this index exists so a later "what did v5 mean" question has one place to look, without
promoting the label into the roadmap itself. Each entry below cross-links its real `FEAT-*`
anchor where one exists.

| Note | Scope | `FEAT-*` anchor | Status |
|---|---|---|---|
| [`domain/lifearea-varga-charakaraka-synthesis.md`](domain/lifearea-varga-charakaraka-synthesis.md) | Closing the "Missing Web" gap for divisional charts + chara karakas, plus a connective layer between them | `FEAT-VARGA-01` / `FEAT-KARAKA-01` (ROADMAP Now) | Design note; itself flags "v5" as ambiguous and asks to confirm scope — unconfirmed |
| [`domain/sign-benefic-malefic.md`](domain/sign-benefic-malefic.md) | Whether dignity color implies sign benefic/malefic (it doesn't); proposed synthesis method (lord's functional nature + occupants + aspects + lord's dignity) | **none yet** — new scope, not in ROADMAP | Research/proposal only, unbuilt, not yet triaged |
| [`domain/argala-virodhargala-drishti-lifematters.md`](domain/argala-virodhargala-drishti-lifematters.md) | Rāśi dṛṣṭi + graha dṛṣṭi + Argala/Virodhargala as a four-relationship reading method, tied to the life-matter table | `FEAT-RELATIONSHIP-04` (ROADMAP Next: "Compound Maitrī, argala, sambandha") | Design sketch; rāśi/graha dṛṣṭi already built, argala/virodhargala not |
| [`domain/nakshatra-lord-sublord-dasha-crossref.md`](domain/nakshatra-lord-sublord-dasha-crossref.md) (aka "nakshatra_sublords") | Nakshatra/pada lord ↔ KP L1–L7 sub-lord chain, nakshatra color scheme, dasha (Maha/Antar/Pratyantar) ↔ Rasi/Nakshatra, plus a design-review follow-up on `tbl_Fact_KpSubLordChain` (why persisted vs. derived, missing `RuleSetId`, confirms scope is all planets not just Moon) | "KP system" bare bullet, ROADMAP Later (no `FEAT-*` code yet); dasha↔rasi/nakshatra piece is a "Missing Web" item under the Now theme | Mostly a Web-surfacing gap, not an engine gap — nakshatra lord→chain and dasha→rasi/nakshatra are built (engine-side); pada lord and nakshatra color are genuinely unbuilt; one real schema gap found (`RuleSetId` missing) |
| [`domain/vedic_reading_layers.md`](domain/vedic_reading_layers.md) | A 12-stage reading→research architecture (data/calc integrity → foundation → strength/condition → relational combinations → natal promise → life-matter application → dasha activation → transit/annual trigger → context → synthesis/confidence → validation → source/version record), cross-linking most existing PVR notes; corrects an earlier chat mislabel ("Systems' Approach" is Choudhry's, not PVR's) | none — architecture/design note, not a single feature | Pure synthesis, no engine implications yet; 5 further additions flagged as unconfirmed suggestions (rule lifecycle, synastry module, base-rate validation, end-user confidence surfacing, muhurta reverse-query) |
| [`domain/ashtakavarga-varga-extension.md`](domain/ashtakavarga-varga-extension.md) | Extends Stage 03's Ashtakavarga item two ways: (1) apply the single natal SAV/BAV bindu table to every varga chart's placements, read-side, rather than recomputing per varga (flags an unconfirmed cross-reference-vs-recompute scope decision) — and (2) a new `AshtakavargaVargaCompareChart` stacked-bar chart, By House / By Sign toggle, % bindu share per varga, same shape as Key Inference 3.4 Amsabala's per-graha stacked bar | none — sub-scope of Ashtakavarga itself, which also has no `FEAT-*` slot yet ([[pvr-coverage]] Ch.12, "not built") | Research/design proposal only, unbuilt; both items depend on Ch.12's engine landing first, so neither is independently buildable yet |

## If this grows

If more notes accumulate under "v5," either add rows here or — better, once scope is
confirmed — give `sign-benefic-malefic.md` its own `FEAT-*` slot in `ROADMAP.md` and let this
index shrink to a pointer, since the roadmap's Now/Next/Later + `FEAT-*` system is this repo's
actual source of truth for planned work, not this file.
