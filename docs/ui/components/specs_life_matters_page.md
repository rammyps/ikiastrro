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

## Deep links

Every reading has its own URL: `/life-matters/{id}?matter=WEALTH_02&lagna=MO&chart=D9`
(`LifeMatterLink.cs`, `LifeMatterLinkTests`).

| Parameter | Value | Left out when |
|---|---|---|
| `matter` | `tbl_Dim_LifeMatter.Code` (unique; its area follows from it) | no question selected |
| `lagna` | a perspective's tag (`MO`, `SU`, `AL`, `HL` …) or its `tbl_Dim_HouseReference` code | Lagna |
| `chart` | a generated chart code (`D9`, `D2-US`) | it is the question's own chart |

Values are case-insensitive. A value that names nothing falls back to the default (the first
area's first question, Lagna, the question's own chart) — never an error. The page applies the
URL on load and whenever it changes (a link opened inside the app, back/forward); every
selection rewrites the URL in place (`replace`, no extra history entries), so the address bar is
always a link to the reading on screen. **Copy link** in the context bar copies it; when the
browser refuses the clipboard, the link appears in a read-only field to copy by hand.

### Links to and from Key Inference

- **In:** Key Inference → Natal → About Houses → *House lord placement* has a **Life matters**
  column: for each house, the matters whose Lagna house focus is that house
  (`LifeMatterHouseIndex`, `tbl_Rule_LifeMatterFocus` ReferenceCode `LAGNA`), in focus priority
  then step order. Two show; the rest sit under **n more**. Each opens `?matter=…`, plus
  `&chart=` when the table shows a chart other than D1.
- **Out:** each Investigate section ends with a *… in Key Inference →* link to the table behind it
  (`KeyInferenceLink`): Chart → `step=overview`, House-by-house evidence and Argala →
  `step=houses`, Kāraka relationships → `step=relationships`, all with `&chart=` for the chart on
  screen; Every perspective → `step=spllagnas`; Planet strength → `step=strength`. Key Inference
  reads `?chart=` (any case) to open its chart picker on that chart.

## Layout — customer flow

Rebuilt 2026-10-01 from the Life Matters UX audit (`reports/life-matters-audit/`): the answer
comes first, the evidence is one click away. Wording and bands come from `LifeMatterReading.cs`
(presentation only); every number from `LifeMatterStatistics`.

0. **Context bar** (sticky under the app bar; at the very top on phones) —
   `Area → Question → Chart → Perspective` and the selected question's %.
   **Foundation strip** below it (`LifeMatterFoundation`, D1 only, PVR's first stages): rising
   sign and degree; Lagna lord, its house and dignity, toned by Ṣaḍbala % of the required
   minimum (≥100% good, else weak); Moon sign, nakshatra and house; **birth-time check** — the
   Lagna's distance to the nearer sign edge (<1° weak: the rising sign itself is in doubt; <3°
   caution; else good). Each card opens the matching Key Inference step. Four across on desktop,
   two on phones.
1. **Explore** (left on desktop):
   - **Area pills** — the 11 `tbl_Rule_LifeMatterReference` categories, each with its
     **area overview** %: the strength of its **core house** in D1 (the house most often seeded as
     a Lagna focus across the area's matters; house 1 when none). On phones the pills scroll
     sideways in one row.
   - **Questions** — the area's first 5 matters (`LifeMatterReading.PrimaryMatterCount`) with their
     % in their own chart; the rest behind **More questions (n)**. Selecting a later question keeps
     the full list open. A new question always opens in its own chart.
2. **Understand** — one answer card:
   - The selected question ("Accumulated wealth overall?"), its % large, and its band in words
     (`LifeMatterReading.Band`, around the 50% midpoint: 65+ strong, 55+ good, 45–54 moderate,
     35–44 limited, under 35 weak — the app's display copy, not classical cut-offs).
   - An **i** button (tappable, `aria-expanded`) opens the explanation: 50% is the ordinary
     midpoint, and the figure's three axes (ruling planets' strength, consistency, the house's
     setting) with their values. It replaces hover-only tooltips as the way to learn what a figure
     means.
   - **Foundation** (D1) vs **Confirmation** (the question's own chart), plus **Chart shown** when
     another chart is selected.
   - **Strongest factors** and **Limiting or uncertain** — two each, from six signals in customer
     words with the technical name beneath (`LifeMatterReading.Factors`): ruling planets' strength
     (Ṣaḍbala), consistency (Amsabala), support the house receives (SAV), ruler's comfort in the
     house (BAV), strength of the house itself (Bhava Bala), help or hindrance from other planets
     (Argala). A signal a chart does not measure fills in as uncertain.
   - **What acts on the …** — for the question's first house (or point) in the shown chart, up
     to two planets that **help** and two that **hinder** it, each with why in plain words
     ("its bādhaka, sits in it"; "aspects it, 4th from it (sustains)"). Only planets that sit in,
     aspect or intervene on the target, or rule it, are named (`TargetInfluenceText`).
   - **Area overview** line, labelled separately from the selected question so the two numbers
     are never confused.
   - **Read from** — Overall (Lagna), Mind, Soul, Public image; the other seven lagnas under
     **Advanced perspectives** (open when one of them is selected). A gold edge marks a
     perspective whose houses the texts name for this question (sourced); the rest count the
     same houses from that point.
   - **Chart** — **relevant charts** only (the question's own charts, then D1, named "Wealth ·
     D2", "Main · D1" from `tbl_Dim_ChartType.ChartShortDescription`) plus an **All charts…** menu
     with every generated chart.
   - The capacity-not-outcome statement.
3. **Investigate** — collapsible sections (`<details>`), the chart open by default, the rest
   closed:
   - **Chart** — `SindHovGrid` for the question in the shown chart from the chosen lagna.
   - **Every perspective in every chart** — lagna (rows) × chart (columns) %. Columns: D1, each
     chart the matter names (`own`, `also named`), then D40 (auspicious / inauspicious), D45 and
     D60 (all indications) for every matter — BPHS ch. 6, Santhanam vol. 1 p. 92, see
     `docs/research/domain/life-matters-unified-model.md` §2.1. A matter seeded `Relevant varga`
     (Loss) shows every generated chart. A cell selects that lagna and chart.
   - **House-by-house evidence** — per focus house, one column each for D1, the question's own
     charts and the shown chart. Rows in customer words with the technical name beneath (support
     the house receives · SAV, strength of the house · Bhava Bala, ruling planet, significator
     planets, help or hindrance · Argala, the three axes, overall support). A sourced
     special-lagna focus adds the "Why <lagna>" note (`tbl_Content_Interpretation`, SubjectType
     `LIFE_MATTER_FOCUS`, key `{LifeMatterCode}_{ReferenceCode}`).
   - **What acts on it** — PVR step 5 (see "Influences on the target" below): every planet
     against one target, with a **Target** chip per house / point when the question reads more
     than one, and **The house / Its Āruḍha** for a house.
   - **Help or hindrance from other planets** — Argala & Virodhargala pairs per focus house.
   - **How the significator planets relate** — kāraka pair relationships
     (`DignityEngine.EvaluatePairRelationship`), **rendered only when the question has two or
     more kārakas** (20 of 106 matters); otherwise the section is absent, not empty.
   - **Planet strength & condition** — `PlanetaryStateTable` (D1, focus lords then kārakas).
   - **How these figures are calculated** — the method, bands and sources.

### Influences on the target (PVR step 5)

`TargetInfluences.Read` (Core, pure, computed live — nothing stored, and it does **not** change
the strength %) reads every planet of the shown chart against a target sign: the focus house
counted from the chosen lagna, a special point's sign, or that house's **Āruḍha pada computed
inside the shown chart** (`ArudhaCalculator.PadaOf`, the same rule as the stored padas). For
each planet:

| Column | From |
|---|---|
| Where from it | house counted from the target; quadrant 1/4/7/10 **sustains**, trine 1/5/9 **prospers**, upachaya 3/6/10/11 **grows**, dusthāna 6/8/12 **obstacles** (PVR Ch. 13 step 5; classes overlap) |
| Reaches it by | sits in it · graha dṛṣṭi (`RelationshipEngine.AspectsSign`) · rāśi dṛṣṭi (`RasiDrishtiCalculator`) · Argala / blocks an intervention (the same `LifeMatterStatistics` Argala the % uses) |
| Nature for the Lagna | `LagnaFunctionalNature` from the shown chart's Lagna (none for Rahu/Ketu) and natural nature (`ArgalaCalculator.IsNaturalMalefic`: waning Moon, afflicted Mercury) — shown for judgement, not counted |
| Role | ruler · kāraka · bādhaka (`BaadhakaCalculator.For(target)`, §13.3) · in the bādhaka sign |
| Reads as | **Helps** (a quadrant, trine or upachaya from it), **Hinders** (a dusthāna from it, or bādhaka-linked), **Mixed** (both), — (the 2nd) |

Rows: the ruler, the kārakas, then planets that act on the target (bold), then the rest (they act
through their periods). The note above the table names the ruler's position from the target and
the bādhaka. Deferred: a sourced per-matter "read from the Āruḍha" flag (PVR step 4 — A7 for
dealings, A5 for distinctions), standard placement results (step 6), yogada, and graha-dṛṣṭi
strength onto a house (stored dṛṣṭi strength is planet-to-planet).

Stored varga padas (`tbl_Chart_KeyDetails` PointKind `Arudha` in D2…D60) are computed inside
each varga since 2026-10-01 (`docs/cli/calculations.md` §12), so they agree with the pada this
section computes; both use the stronger co-lord for Scorpio / Aquarius.

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
The evidence table shows Capacity, Consistency and Context as rows above overall support; the answer
card's **i** panel shows the axes in words. Presentation scales, not sourced rules; it describes strength,
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

Desktop: Explore beside the answer card, Investigate full width below. Below 1000px the answer
card drops under Explore. Phones (≤760px): `MainLayout`'s app bar wraps to two rows (tabs, then
the person switcher and SAVED CHARTS) and scrolls away instead of being fixed, so the tabs no
longer overlap the person's name (app-wide; `tokens.css` drops MudBlazor's matching top padding).
The context bar then sticks to the very top, the area pills scroll sideways and the factor lists
stack. Area pills are `role="tab"`, questions `role="option"` in a listbox, perspective and chart
chips `aria-pressed` toggles, the **i** button and More questions `aria-expanded`. Every figure
carries a word band, never colour alone.

## Tests

`TargetInfluencesTests` (lord, bādhaka, links, position classes, leans, functional nature,
`ArudhaCalculator.PadaOf` and its exception, answer-card wording), `LifeMatterFoundationTests` (the four cards, Ṣaḍbala tone, sign-edge thresholds),
`LifeMatterHouseIndexTests` (Lagna foci only, priority order, one entry per matter),
`KeyInferenceLinkTests`, `LifeMatterLinkTests`, `LifeMatterReadingTests` (word bands around 50%, strongest / limiting factors with unmeasured ones
as uncertain, the four primary perspectives), `LifeMatterStatisticsTests` (bands, per-sign D1
reads, varga SAV and empty Bhava Bala, live varga Argala, Argala pairing), `SindHovGridTests`,
`ChartSnapshotTests.SindHovGrid` (golden). No bUnit test drives the page itself; the customer flow
was verified in the browser against the dev DB (Ramya, person 2) on 2026-10-01 at desktop and
390px widths: area → question → More questions, the i panel, All charts menu, Advanced
perspectives, the kāraka section hidden for single-kāraka questions, no horizontal scroll, page
height about 1,900px desktop / 2,700px phone (was about 3,000 / 5,000).

## Saved statistics (2026-10-01)

`LifeMatterStatistics` / `HouseStatistics` now live in `Ikiastrro.Data.Statistics` (moved from the
Web project) so the page and the CLI share one implementation; `ArgalaFacts` moved to
`Ikiastrro.Data` with them. `HouseStrengthStatisticsService` saves every generated chart × 12 signs
to `tbl_Fact_HouseStrengthStatistics` (migration 156), queryable through
`vw_ChartHouseStrengthStatistics`:

- Filled at the end of `ChartGenerationService.GenerateAll` and `RecomputeAnalytics`; people
  generated earlier are filled by CLI `backfill-strength-statistics`.
- Deleted first by `GenerateAll` and by `BirthDetailDeletionService` (FK to `tbl_ChartResults`,
  no cascade).
- Axes are saved for the sign's **lord only**. A matter's kārakas join Capacity / Consistency on
  the page, but kāraka resolution is matter- and chart-specific, so it is not stored; join
  `vw_ChartShadbala` / `vw_ChartAmsabala` for a kāraka's own figures.
- The page still computes live from the same class, so the saved lord-only rows and the page's
  lord-only figures cannot disagree.
