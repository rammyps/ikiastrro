---
last_updated: 2026-09-22
reflects: FEAT-ASHTAKAVARGA-02 cross-varga read-side comparison
component: AshtakavargaVargaCompareChart
route: /key-inference/{id}?step=vargas
---

# AshtakavargaVargaCompareChart

Compares the single natal Sarvāṣṭakavarga sign matrix across stored divisional charts. It does
not recompute Ashtakavarga independently for each varga. For every selected varga, the component
uses that varga's own ascendant to map houses 1–12 to absolute signs, then looks up the natal SAV
bindus for those signs.

## Interaction

- Default frame: **By house**, because each varga's ascendant makes this the informative view.
- **By sign** is an explicit sanity-check view; every varga references the same fixed sign score.
- Varga-set selector reuses `tbl_Rule_AmsabalaGroup` membership through
  `AmsabalaSchemeRepository`; no second grouping catalogue is hard-coded.
- Each bar is normalized to 100%. Its segments are the selected vargas, sized by bindus; the
  number at right is the unnormalized sum.

## Data contract

`AshtakavargaRepository.GetByBirthDetailId` supplies the persisted natal SAV values from
`vw_ChartAshtakavarga`. `WorkspaceData.Charts` supplies each varga's ascendant. The component is
read-only and performs only the presentation lookup and house rotation.

## Empty and partial data

The parent renders this chart only when natal Ashtakavarga exists. Varga groups silently omit
charts that have not been generated, and the legend shows the charts actually compared.
