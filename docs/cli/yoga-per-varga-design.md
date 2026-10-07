---
last_updated: 2026-10-07
workstream: cli
togaf: C — information systems architecture (design, not built)
safe: Program backlog input
---

# Yogas in every varga — design

**Why.** Jagannatha Hora's Yogas list has a **Varga** column: each yoga is checked in the divisional
charts, and some rules are defined across several of them ("same planet aspecting lagna in D-1, D-9, D-2,
D-3, D-12, D-30"). ikiastrro evaluates yogas on **D1 only** (`ProductionYogaEngine.DetectDetailed` takes
`d1` and optionally `d9`). That is a product gap, not just a parity gap: the product's job is reduced
analytical omission with traceable evidence, and a yoga that also holds in D9 or D10 is stronger,
better-timed evidence than one that holds in D1 alone.

## What exists today (measured 2026-10-07, RamakrishnanP)

| Fact | Detail |
|---|---|
| Variants evaluated per chart | 414 (`tbl_Fact_YogaInputEvaluations`), one row per source variant (Raman 300 + PVR ch.11 + Horoscope Explorer), keyed by `ChartResultId` only |
| Chart results per person | 22 (D1 + 21 varga rows); yoga rows exist for the D1 `ChartResultId` only |
| Evaluator entry points | `Evaluate(d1[, d9])` or `Evaluate(ChartBundle)`; 12 evaluator files hard-code `"D1"` |
| Precedent for per-varga materialisation | Ashtakavarga: `ChartGenerationService` already writes SAV/BAV per `ChartResultId` for every chart type, so the UI varga selector reads distinct rows |
| View | `vw_ChartYogaEvaluations` joins on `ChartResultId` already — it carries no chart-type column yet |

So the **schema already admits per-varga rows**: the fact table is keyed by `ChartResultId`, and every
varga has its own. The work is in the engine contract and in deciding which rules are portable.

## The central problem: a D1 rule is not automatically a varga rule

Applying "Mars in a kendra in own sign" to D9 treats D9's lagna as a lagna. JHora does this; the classical
sources (Raman 300, PVR ch.11) state their rules for the rāśi chart and do not, rule by rule, license it.
Three kinds of rule:

1. **Portable geometry** — depends only on signs, houses, lordship, conjunction and aspect within one chart
   (Ruchaka, Adhi, Budha-Āditya, Sankha, the Nabhasa family, Raja/Dharma-Karmādhipati, Viparita Raja…).
   Re-basing the evaluator on another varga is mechanically sound.
2. **D1-bound** — needs birth-level facts that exist once per person: day/night, lunar phase, janma-ghaṭi,
   chara karakas (a "whole-life fact", per the existing `CharaKarakaByPlanet` comment), HL/GL/AL special
   lagnas, Shadbala/Vaiśeṣikāṁśa thresholds, dashas. These must keep reading the D1 bundle even when the
   reference chart is a varga.
3. **Cross-varga by definition** — the rule names its own vargas (D-1/9/2/3/12/30 aspecting lagna;
   Kalpadruma's navamsa dispositors; Vaiśeṣikāṁśa-conditioned rules). These are evaluated once, against the
   whole bundle, and are not repeated per varga.

Because the sources do not authorise kind-1 reuse, a varga result must be **labelled as a varga
confirmation, never merged into the source-attributed D1 row**. This matches the existing norm ("one
source variant is one result; authorities … are never collapsed"). The citation for the per-varga layer is
*JHora's documented behaviour* (View ▸ Yogas, Varga column), recorded as a convention, not as a classical
attribution.

## Proposed design

### Engine

- Add `YogaPortability { Portable, D1Bound, CrossVarga }` per source variant, held in one table in Core
  (`YogaPortabilityCatalog`) and defaulting to **D1Bound** — a variant must be opted in as Portable after
  review, so nothing silently runs on a varga by accident.
- `ProductionYogaEngine.DetectDetailed(bundle, referenceChartType = "D1")`. For a varga reference, build a
  *re-based bundle*: `Charts` unchanged, the evaluator's "d1" parameter replaced by the reference chart,
  the D1-bound inputs (birth, sun times, karakas, strengths) passed through untouched. Evaluators that only
  need `(d1, d9)` get `(reference, D9)`; that pairing is itself a rule decision and is recorded in the
  catalogue per variant.
- Variants that are not `Portable` are skipped for non-D1 references (no row), not marked NOT_EVALUATED —
  "not applicable to this varga" is a different statement from "could not be computed".
- **Invariance test (gate):** `DetectDetailed(bundle, "D1")` must be byte-identical to today's output for
  every saved chart. This is the regression guard for the refactor and is written first.

### Persistence

- Write the per-varga rows to the same `tbl_Fact_YogaInputEvaluations`, one `ChartResultId` per varga — no
  new table. Rows for a varga carry only portable variants.
- Add `ChartType` to `vw_ChartYogaEvaluations` (a join to the chart-result's type) so consumers filter
  `D1` (unchanged behaviour) vs. varga rows. Additive, per the schema policy.
- `YogaInputRepository.Replace` is currently called inside the `input is D1` branch of
  `ChartGenerationService`; add a sibling call for each varga result, using the already-built `charts`
  list. `refresh-yogas` gains an optional `--vargas` flag; the default stays D1-only until the UI is ready.

### Evidence model / UI

- A yoga's "confirmation" is the set of vargas where it holds (e.g. `D1, D9, D10`). Surface it as a column
  on the existing yoga table and as a strength input to the Life Matters matrix — D10 confirmation of a
  career yoga should outweigh D1-only. Weighting is a product decision and is **not** proposed here.
- The varga selector pattern from the Ashtakavarga chart is the model.

## Slices, in order

1. **Invariance harness** — refactor `DetectDetailed` to take a reference chart; prove D1 output unchanged
   for all saved people. No behaviour change.
2. **Portability catalogue** — classify the 414 variants (start with the ~20 JHora lists for RamakrishnanP,
   since each has a JHora row to compare against). Everything unreviewed stays D1Bound.
3. **Persist + view** — per-varga rows behind `--vargas`; `ChartType` on the view.
4. **JHora golden** — run a chart whose JHora Yogas list includes non-D1 rows (RamakrishnanP's are all D-1,
   so a second chart must be captured) and diff per varga.
5. **UI** — confirmation column and the varga selector. Needs a product call on weighting.

## Decisions

- **Vargas (decided 2026-10-07, rammyps): only the vargas JHora's rules name — D1, D2, D3, D9, D12, D30.**
  This is the Shad-Varga set plus none of the others; D4/D7/D10/D16… are not evaluated. The re-based
  evaluation therefore runs on at most six charts per person (D1 unchanged).
- **Sarpa / Mridanga (resolved 2026-10-07):** both were missing PVR variants, not definitional conflicts —
  added as `PVR_CH11_SARPA` / `PVR_CH11_MRIDANGA` (migration 175). The per-varga diff no longer inherits
  the discrepancy.
- **Still open — authority.** Is "JHora's behaviour" an acceptable cited basis for the per-varga layer,
  given the classical sources don't license it rule by rule? (Recommended: yes, labelled as a confirmation
  layer.)
- **Still open — a second golden chart** with JHora Yogas captured, ideally with non-D1 rows. Capture it on
  a chart whose place/time match ikiastrro's (RamakrishnanP's JHora file once carried an Australian place).

## Golden pass, 2026-10-07 (built: first cut of slices 1–2)

JHora's Yogas list was captured for RamakrishnanP (Chennai, India, Aries lagna) in each of D1/D2/D3/D9/D12/D30
(`cproj_win_app_explorer/src/jhora_yogas_per_varga.py` → `explorer_output/jhora/yogas-per-varga/`). The list is
**per-varga in JHora**: right-click ▸ "Select a varga". Built: `ProductionYogaEngine.DetectForVarga(bundle, chartType)`
(single-chart geometry evaluators only — Verified source engine, Malika, both Nabhasa batches, PVR ch.11 named yogas;
D1-bound and multi-chart rules are not run) and `ProductionYogaEngine.VargaReferenceCharts`. Not built: persistence,
`--vargas`, the view's `ChartType`, the UI. `JhoraYogaPerVargaGoldenTests` fixes ikiastrro's own varga positions and
checks the layer against JHora.

**Confirmed from JHora's output**
- Cross-varga rules ("same planet aspecting lagna in D-1, D-9, D-2, D-3, D-12, D-30") and Kalpadruma's dispositor
  chain print **identically in every varga** — JHora does not re-base them. Our "evaluate once" stands.
- Karaka rules (Raja Sambandha, Raja AK-PiK) take karakas from D1 but positions from the varga chart.
- HL/GL rules (Yogada, Maha Yogada) use the HL/GL projected into that varga.

**Agreement.** The 13 yogas where JHora and ikiastrro share a definition (Ruchaka, Sasa, Sunapha, Anapha,
Gaja-Kesari, Sarpa, Sula, Paasa, Kedara, Daama, Dharma-Karmadhipati, Budha-Aditya, Ubhayachara) match JHora in all
six vargas.

**Divergences found (definitional; ikiastrro keeps its PVR/Raman reading — none decided yet)**

| Yoga | JHora | ikiastrro |
|---|---|---|
| Adhi | fires only with **two or more** benefics in 6/7/8 from Moon | any one of Jupiter/Mercury/Venus (Raman 007, `PVR_CH11_ADHI`); PVR's text says "benefics occupy the 6th, 7th and 8th" |
| Subha | benefics in **2nd and 12th** only | also benefic in lagna (PVR p.124) |
| Mridanga | needs **≥2** own/exalted planets in kendra/kona; lagna-lord strength read from **D1** (fires in D12 with Mars weak there) | ≥1 planet; lagna lord strong in the same chart |
| Chaamara | two benefics **collectively** across 7th/9th/10th | joined in one house |
| Vesi / Vosi / Ubhayachara | mutually exclusive (Ubhayachara when both sides) | all three fire together |
| Parvata, Kaahala, Vimala | listed (PVR wording) | no PVR variant — only Raman variants (different rules), or none |
| Kemadruma | not listed in D1 | present in D1 (Moon alone in 2nd/12th) |

Missing PVR variants (Parvata, Kaahala, Vimala) are the same kind of gap as Sarpa/Mridanga (migration 175) and are
the next cheap wins. The Mridanga row is the one design-relevant finding: some "strong lagna lord" conditions are
read from D1 even in a varga, so the portability catalogue needs a *mixed* kind (varga geometry + D1 strength), not
just Portable/D1-bound.

## Out of scope

Weighting or ranking varga confirmations; yoga dating/timing; changing any existing D1 row.
