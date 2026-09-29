---
last_updated: 2026-09-29
workstream: ui
component: LifeMatters page
route: /life-matters/{id}
togaf: C — component spec (built)
catalogued_in: chart-catalog.md
reflects: src/Ikiastrro.Web/Components/Pages/LifeMatters.razor(.css) · Components/LifeMatters/*.cs
---

# Specification — LifeMatters page

The Life Matters page reads one area of life at a time, asks it from each lagna perspective, and
backs every answer with D1 strength statistics. Plan, decisions and history:
[`../lifematters_plan.md`](../lifematters_plan.md) (decisions 12–15 set this structure on
2026-09-29). Mockup it was built from: https://claude.ai/artifact/874M2VxM9ou4RxgHjddBn8 (v3).

## Route and navigation

`/life-matters/{id}`, the `LIFE MATTERS` pill in `MainLayout`'s header between `KEY INFERENCE`
and `NUMEROLOGY`. Unknown person or no D1 chart → "No saved charts found for that person."

## Levels

1. **Area** — the `tbl_Rule_LifeMatterReference` categories (11), as pills. Each pill shows the
   area's four D1 signals for its **core house**: the house most often seeded as a Lagna focus
   across the area's matters (ties shown, e.g. Wealth `H2·11`; the first is used for signals;
   house 1 when an area has none). "Area order | Strongest first" sorts by signal count.
2. **Question** — one per lagna perspective, phrased from the area's theme:
   "How is the self from power?" is the self read from Ghati Lagna. Perspectives, in order:
   Lagna (overall), Chandra (mind), Surya (soul), Arudha (as the world sees it), Paaka (body),
   Karakamsa (inner self), Hora (wealth), Ghati (power), Sree (prosperity), Indu
   (wealth-yielding capacity), Pranapada (vitality). Copy lives in
   `LifeMatterQuestions.cs`; each question's tooltip and gloss are `tbl_Dim_HouseReference.
   Perspective` (read by `LifeMatterFocusRepository.GetHouseReferences`), plus its
   `AppliesInVarga` note when not `Any`. A red dot marks a lagna that at least one of the area's
   matters has a seeded focus on. A lagna whose point isn't persisted for this person (Indu,
   Pranapada until their `IL`/`PP` points exist) is disabled, "not computed yet". Each row shows
   the lagna's sign, the core house from it, its signals, and the sub-question count (or
   "n/m match" under filters). "Lagna order | Strongest first".
3. **Sub-questions** — the area's matters, each read from the selected question's lagna:
   - **Houses**: the matter's own seeded houses on that lagna when it has any (**sourced**);
     otherwise its Lagna houses (or other house foci) re-counted from this lagna. Special-point
     foci (`A2`, `A10`, `AL`, `GA_AK` …) are placed where they fall, as "A10 in 7th".
   - Table columns: sub-question · house(s) · sign · SAV (banded) · Bhava Bala · lord and its
     Ṣaḍbala % · Argala outcome counts · four signals. Rows are keyboard-selectable; rows failing
     the active filters are dimmed, not hidden. A matter with no focus rows reads "Focus not yet
     structured for this matter".
   - Below the table: the chart and a **D1 statistics** panel per focus house (SAV meter with
     the 28 average tick, Bhava Bala meter with the 7-rupa tick, lord Ṣaḍbala, occupants).
     When the question's lagna is the matter's own seeded focus, the panel adds a "Why <lagna>"
     note: the `tbl_Content_Interpretation` row with SubjectType `LIFE_MATTER_FOCUS` and key
     `{LifeMatterCode}_{ReferenceCode}` (db/153 seeds Indu → WEALTH_11, Pranapada →
     SELF_HEALTH_02 / TROUBLE_LOSS_07). No row, no note.
4. **D1 analysis** — for the selected sub-question and question:
   - **Argala & Virodhargala** per focus house, from `tbl_Fact_Argala` (D1): pairs 2/12, 4/10,
     11/3, secondary 5/9, and a malefic 3rd/11th pair when present; planets on each side, an
     exception flag, and Holds / Contested / Obstructed / obstruction-only.
   - **Planet strength & avastha** — `PlanetaryStateTable` (Key Inference's own avastha table,
     Capacity carries Ṣaḍbala) scoped to the focus houses' lords then the matter's karakas, D1.
   - **Karaka relationships** — `DignityEngine.EvaluatePairRelationship` for every karaka pair
     in the displayed chart.
   - A closing line: statistics show strength (capacity), not outcomes.

## Signals and bands

Four fixed slots, always in this order: **SAV · Bhava Bala · lord Ṣaḍbala · Argala**. Each is
strong / middle / weak / nothing-to-read (`StrengthBand`), computed by `LifeMatterStatistics`:

| Signal | Strong | Weak | Source of the band |
|---|---|---|---|
| Sarva Ashtakavarga bindus of the sign | > 30 | < 25 | cited rule already used by `AshtakavargaChart` |
| Bhava Bala rupas of that house from the D1 Lagna | ≥ 7 | < 5 | `HouseStrengthChart` presentation band (unsourced) |
| Ṣaḍbala % of required minimum, sign lord | ≥ 110 | < 90 | `PlanetaryStateTable` strong/weak bands (unsourced) |
| Argala: pairs holding minus pairs obstructed | > 0 | < 0 | count comparison; planet strength not compared yet |

All statistics are D1 and sign-based, so a house counted from any lagna reads the same facts
(e.g. the 1st from Ghati Lagna in Pisces reads Pisces' SAV and the Bhava Bala of the D1 house
Pisces occupies). "Strongest first" sorts by strong-minus-weak and is never shown as a score.

## Filters

Nine D1-statistic pills, each with a count for the selected area × question: SAV above 30, SAV
below 25, Bhava Bala 7+, lord Ṣaḍbala 110%+, lord Ṣaḍbala under 90%, Argala holds, Argala
obstructed, lord in Yuva (Bālādi), lord awake (Jagrat). Match All / Any, Clear. A sub-question
matches when any of its focus houses passes each selected filter.

## Chart

`SindHovGrid` ([`specs_sind_hov_grid.md`](specs_sind_hov_grid.md)) with the
`SouthIndianGrid_Detailed` look. Chart control: **D1** (default) · **Auto** (the matter's
Subject varga, retaining the last chart when unmapped) · **Varga** (any generated chart).
Statistics stay D1 whichever chart is shown; SAV chips are drawn only on D1. The question's
lagna is the grid's track: gold house-from-track badges and a tag on its sign (not drawn for
Lagna itself). Focus signs fill sunrise with a ◆; cells can be hovered, pinned (click/Enter) and
cleared (Escape). Cell labels: Arudha padas, HL/GL/SL/IL/PP, and `GA_AK` when a matter asks for it.
Uncomputed varga → the shared `EmptyState` with the `backfill-charts` CLI hint.

Reference resolution (`ResolveReferenceSign`): Lagna = Ascendant; Chandra/Surya = Moon/Sun sign;
Arudha/Hora/Ghati/Sree/Indu/Pranapada = the persisted `AL`/`HL`/`GL`/`SL`/`IL`/`PP` point; Paaka
= sign of the D1 Lagna lord; Karakamsa = the Atmakaraka's D9 sign, for every chart; Graha Lagnas
= that graha's sign.

## Data (one page-load snapshot)

All reads happen once per person in `OnParametersSet`; every selection after that is in-memory.

| Data | Repository · method | Source |
|---|---|---|
| Person, all charts, key details, lords, aspects | `WorkspaceData.Load` | chart tables |
| Areas, all steps | `LifeMatterReferenceRepository.GetCategories` / `GetAllSteps` | `tbl_Rule_LifeMatterReference` |
| Subjects, foci, lagna perspectives | `LifeMatterFocusRepository.GetSubjects` / `GetFoci` / `GetHouseReferences` | `tbl_Rule_LifeMatterSubject` / `tbl_Rule_LifeMatterFocus` / `tbl_Dim_HouseReference` |
| Karakas | `KarakaMatterRepository.GetForRuleSet` | `tbl_Rule_KarakaMatter` |
| Focus notes | `InterpretationRepository.GetBySubjectType(1, "LIFE_MATTER_FOCUS")` | `tbl_Content_Interpretation` |
| SAV | `AshtakavargaRepository.GetByBirthDetailId` | `vw_ChartAshtakavarga` |
| Bhava Bala | `BhavaStrengthRepository.GetSummaryByBirthDetailId` | `vw_ChartBhavaBala` |
| Ṣaḍbala | `PlanetaryStrengthRepository.GetSummaryByBirthDetailId` | `vw_ChartShadbala` |
| Avastha | `PlanetaryStateRepository.GetByBirthDetailId`, `PlanetaryStateRuleRepository.GetAllStates`, `PostureStateInterpretationRepository.GetByRuleSet` | `tbl_Fact_PlanetaryState(+Flag)` |
| Argala | `ArgalaFactRepository.GetByBirthDetailId` | `tbl_Fact_Argala` |

`LifeMatterFocusResolver.Resolve` runs once per step at load (pure).

## Removed on 2026-09-29

The yoga card and the Yoga × LifeMatter 7×7 matrix view (`vw_YogaLifeMatter7x7`): the user
judged the mapping not meaningful; yogas return only after that mapping is reworked. The House
condition card became the D1 statistics panel; the Planet condition card became Planet strength
& avastha.

## Responsive and accessibility

Desktop: questions column (360px) beside sub-questions; chart (≤520px) beside the statistics
panel. Below 1150px the chart and panel stack; below 900px the whole layout is one column.
Area pills and question options carry explicit `aria-label`s (visible text, not their hover
tooltips); signal strips are `role="img"` with a spoken summary; bands always carry a word
(FAV / MID / UNFAV …), not colour alone.

## Tests

`LifeMatterStatisticsTests` (bands, per-sign D1 reads, Argala pairing), `SindHovGridTests`
(interaction + track badge + detailed cell content), `ChartSnapshotTests.SindHovGrid` (golden).
No bUnit test drives the page itself yet; verified in the browser against the dev DB
(Ramakrishnan, person 4) on 2026-09-29: area/question/sub-question switching, a filter, the
Ghati Lagna track, and the D9 chart.
