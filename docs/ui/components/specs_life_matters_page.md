---
last_updated: 2026-10-01
workstream: ui
component: LifeMatters page
route: /life-matters/{id}
togaf: C — component spec (built)
catalogued_in: chart-catalog.md
reflects: src/Ikiastrro.Web/Components/Pages/LifeMatters.razor(.css) · Components/LifeMatters/*.cs
---

# Specification — LifeMatters page

The Life Matters page reads one matter of life at a time in the chart that matter belongs to,
from a chosen lagna, and backs it with strength statistics compared against D1. Plan, decisions
and history: [`../lifematters_plan.md`](../lifematters_plan.md). Mockup of the earlier
four-level version: https://claude.ai/artifact/874M2VxM9ou4RxgHjddBn8.

## Route and navigation

`/life-matters/{id}`, the `LIFE MATTERS` pill in `MainLayout`'s header between `KEY INFERENCE`
and `NUMEROLOGY`. Unknown person or no D1 chart → "No saved charts found for that person." The
page runs edge to edge: it renders a hidden `.lm-full` marker and `MainLayout.razor.css` drops
the centered column's max width for `.ik-page:has(.lm-full)` (16px side padding). No page
heading; the active header pill names the page.

## Layout

One row, then two full-width blocks:

1. **Chart (left, ≤560px)** — `SindHovGrid` for the selected matter in the selected chart.
2. **Picker (right, sticky under the app bar, sized to stay in view without scrolling)**:
   - **Area pills** — the 11 `tbl_Rule_LifeMatterReference` categories, each with the strength %
     of its **core house** in D1 (the house most often seeded as a Lagna focus across the
     area's matters; house 1 when none).
   - **Matter tiles** — the selected area's matters open directly under the pills. Each tile
     shows the matter, its own chart tag (e.g. `D10`) and its strength % in that chart from
     the selected lagna. Clicking a tile shows the matter in the main chart.
   - **Chart chips** — the matter's own charts (the first marked `· own`), then D1, then every
     other generated chart in varga order: any matter can be viewed in any chart. A new matter
     always opens in its own chart.
   - **Questions** — the drill-down under the matter: one row per lagna perspective (LAG, MO, SU,
     AL, PAAKA, KL, HL, GL, SL, IL, PP; copy in `LifeMatterQuestions.cs`), asked of the matter
     itself ("Enemies from power?"; a matter already worded as a question keeps the area form,
     "How is my identity from power?"). Each row names the houses/points it reads ("10th, A10"),
     whether they are **sourced** (lagna-gold rule) or **re-counted**, and the strength % in the
     shown chart. Tooltips are `tbl_Dim_HouseReference.Perspective`; a lagna that can't be
     resolved in the shown chart is disabled.
3. **Statistics (full width)** — the question and the matter, then:
   - **Summary** — every lagna question (rows) × every chart the matter reads (columns), each cell
     the strength %. Columns: **D1 · promise**, each chart the matter names (`own`, `also named`),
     then **D40** (auspicious / inauspicious), **D45** and **D60** (all indications) for every
     matter — BPHS ch. 6, Santhanam vol. 1 p. 92, see
     `docs/research/domain/life-matters-unified-model.md` §2.1. A matter seeded `Relevant varga`
     (Loss) shows every generated chart. Clicking a cell selects that lagna and that chart;
     clicking a question selects the lagna only.
   - **Detail** — per focus house, one column each for D1, the matter's own charts and the shown
     chart when it is none of those. Rows: sign (house from that chart's Lagna), SAV (meter with
     the 28 tick, banded), Bhava Bala (D1 only), lord and its Ṣaḍbala %, the lord's dignity in
     that chart, occupants, Argala outcome counts, strength %. Under each table one line lists
     the strength % per chart ("D1 64% · D6 66% · D8 47%"). A sourced special-lagna focus adds
     the "Why <lagna>" note (`tbl_Content_Interpretation`, SubjectType `LIFE_MATTER_FOCUS`, key
     `{LifeMatterCode}_{ReferenceCode}`).
4. **Analysis (full width)** — Argala & Virodhargala per focus house in the shown chart; karaka
   relationships in the shown chart (`DignityEngine.EvaluatePairRelationship`); planet strength &
   avastha (`PlanetaryStateTable`, D1, the focus lords then karakas). Closing line: statistics
   show strength (capacity), not outcomes.

### Which chart is a matter's own

`MatterCharts`: the divisional subject's confirmation chart
(`tbl_Dim_DivisionalSubject.PrimaryConfirmationChartId` via `tbl_Rule_LifeMatterSubject`) first,
then every `D<n>` named in `PrimaryChartsText` ("D6/D8", "D16, confirmed in D1"), limited to
charts generated for the person. No chart → D1. So career opens in D10, marriage in D9, illness
in D6, education in D24, spirituality in D20.

### Houses read

The matter's own seeded houses on the chosen lagna when it has any (**sourced**); otherwise its
Lagna houses (or other house foci) re-counted from that lagna. Special-point foci (`A10`, `AL`,
`GA_AK` …) are placed where they fall in each chart.

## Statistics per chart

`LifeMatterStatistics(chartType, ascendantSign, …)` answers per sign, read as a house from that
chart's Lagna; `ForSign(sign, karakas)` adds the matter's kāraka planets. Signals are grouped into
`docs/research/domain/stat_strength.md` §4's three axes (2026-10-01, rammyps's choice), each kept
separate:

| Axis | Signal | In D1 | In a varga | Index (≈50% = reference) | Strong | Weak | Band source |
|---|---|---|---|---|---|---|---|
| Capacity | Ṣaḍbala of the sign's lord and the matter's kārakas | % of minimum | same planet figure | % ÷ 200 (minimum → 50%) | ≥ 100 | < 80 | `StrengthBands.ShadbalaPercentOfMinimum` (100% BPHS; 80% heuristic) |
| Consistency | the same planets' Amsabala | Shodasavarga good/16 | same | the % itself | — | — | none (stat_strength.md §1.2: one canonical scheme, never an average of the four) |
| Context | Sarva Ashtakavarga | D1 SAV | that varga's SAV | ÷ 56 (28 → 50%) | > 30 | < 25 | `StrengthBands.SarvaAshtakavargaBindus` (PVR) |
| Context | the lord's own BAV in the sign | D1 BAV | that varga's BAV | ÷ 8 (4 → 50%) | ≥ 5 | ≤ 3 | `StrengthBands.BhinnaAshtakavargaBindus` (PVR) |
| Context | independent Bhava Bala (Dig + Drik) | z against the chart's 12 houses | nothing (D1-only) | 50 + 10z | z ≥ +1 | z < −1 | `StrengthBands.IndependentBhavaBalaZ` (heuristic) |
| Context | Argala | pairs holding minus obstructed | same, varga's own placements | share of pairs that hold | > 0 | < 0 | count comparison |

**Why independent Bhava Bala:** raw Bhava Bala includes Bhavadhipati Bala, which *is* the lord's
Ṣaḍbala — averaging it with the lord's Ṣaḍbala counted the lord twice (stat_strength.md §0, §1.3).
Raw Bhava Bala is still shown on the detail row as "total", not scored. `IndependentBhavaBala`
(`Ikiastrro.Data`) is the one definition, shared with Key Inference 3.2.

**Axes:** Capacity and Consistency are the mean over the lord and the matter's kārakas (each
planet once); Context is the mean of its readable parts. **Strength %**
(`HouseStatistics.StrengthPercent`) is the mean of the readable axes, so each axis counts
equally however many signals it holds. A read covering several houses/points shows their mean.
The detail table shows Capacity, Consistency and Context as rows above Strength; tooltips carry the
per-planet and per-part breakdowns. Presentation scales, not sourced rules; it describes strength,
never outcomes.

Reference resolution (`ResolveReferenceSign`, per chart): Lagna = Ascendant; Chandra/Surya =
Moon/Sun sign; Arudha/Hora/Ghati/Sree/Indu/Pranapada = the chart's persisted
`AL`/`HL`/`GL`/`SL`/`IL`/`PP` point (vargas carry their own); Paaka = sign of the D1 Lagna lord in
that chart; Karakamsa = the Atmakaraka's D9 sign, for every chart; Graha Lagnas = that graha's
sign.

## Chart

`SindHovGrid` ([`specs_sind_hov_grid.md`](specs_sind_hov_grid.md)), `SouthIndianGrid_Detailed`
look. The chosen lagna is the grid's track (gold house-from-track badges, a tag on its sign; not
drawn for Lagna itself). SAV chips show the shown chart's own SAV. Focus signs fill sunrise with
a ◆; cells can be hovered, pinned (click/Enter) and cleared (Escape). Cell labels: Arudha padas,
HL/GL/SL/IL/PP, and `GA_AK` when a matter asks for it. An ungenerated chart → the shared
`EmptyState` with the `backfill-charts` CLI hint.

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
| Argala | `ArgalaFactRepository.GetByBirthDetailId`; a chart with no stored rows (every varga, and D1 generated before db/128) uses `ArgalaFacts.ForChart` (shared with Key Inference's ArgalaTable) | `tbl_Fact_Argala`, or `ArgalaFactBuilder` live |

`LifeMatterFocusResolver.Resolve` runs once per step at load (pure). `LifeMatterStatistics` is
built lazily once per chart type and cached.

## Removed on 2026-09-29

The yoga card and the Yoga × LifeMatter 7×7 matrix view (`vw_YogaLifeMatter7x7`): the user
judged the mapping not meaningful; yogas return only after that mapping is reworked. The House
condition card became the D1 statistics panel; the Planet condition card became Planet strength
& avastha. Also removed the same day, when the page became a drill-down (the user found "too many drop
downs"): the Question column, the sub-question table, the nine D1-statistic filter pills with
Match All/Any, both "Strongest first" toggles, the D1/Auto/Varga chart control with its varga
dropdown, the LIFE MATTERS heading, and the four coloured signal squares (replaced by one
strength label).

## Responsive and accessibility

Desktop: chart (≤560px) beside the sticky picker; statistics and analysis full width below.
Below 1150px the picker moves above the chart and stops being sticky; below 900px the analysis
cards are one column. Area pills are `role="tab"`, matter tiles `role="option"` in a listbox,
chart and lagna chips are `aria-pressed` toggles. Strength labels always carry a word, never
colour alone.

## Tests

`LifeMatterStatisticsTests` (bands, per-sign D1 reads, varga SAV and empty Bhava Bala, live varga
Argala, the strength label, Argala pairing), `SindHovGridTests`, `ChartSnapshotTests.SindHovGrid`
(golden). No bUnit test drives the page itself; verified in the browser against the dev DB
(Ramakrishnan, person 4) on 2026-09-29: area → matter drill-down, Career in D10 against D1,
Wealth in D2, full-width layout.
