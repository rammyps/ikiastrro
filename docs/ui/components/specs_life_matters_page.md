---
last_updated: 2026-09-25
workstream: ui
component: LifeMatters page
route: /life-matters/{id}
togaf: C — component spec (Phase 3A built)
catalogued_in: chart-catalog.md
---

# Specification — LifeMatters page

Written for `lifematters_plan.md` Phase 0B, after Phase 1A (repositories:
`LifeMatterReferenceRepository`, `DivisionalSubjectRepository`, `LifeMatterFocusRepository`,
`KarakaMatterRepository`), Phase 1B (`LifeMatterFocusResolver`), and part of Phase 2
(`SindHovGrid`, [`specs_sind_hov_grid.md`](specs_sind_hov_grid.md)) already landed — this spec was
the orchestration contract those pieces feed into. **Phase 3A is now built**
(`src/Ikiastrro.Web/Components/Pages/LifeMatters.razor(.css)`) against this contract; sections
below are annotated where the shipped page differs from or narrows the original design.

## Route

**Built as `/life-matters/{id}`**, matching the app's existing per-person top-level route pattern.
The header nav is actually two pills today (`KEY INFERENCE`, `NUMEROLOGY` — `ALL CHARTS` was
folded into Key Inference step 7 on 2026-09-24, per `MainLayout.razor`'s own comment; this spec's
original "alongside ALL / CHARTS · KEY / INFERENCE · NUMEROLOGY" assumed the pre-2026-09-24 nav
and was stale). `LIFE MATTERS` was added as a third pill between them, same two-word stacked-span
style as `KEY INFERENCE`.

**Gap found and fixed during the build, worth recording**: `LifeMatterReferenceRepository`,
`DivisionalSubjectRepository`, `LifeMatterFocusRepository`, and `KarakaMatterRepository` were
never registered in `Program.cs`'s DI container, despite this spec's header (written after Phase
1A) describing them as "already landed." The classes existed and compiled, but the page would
have thrown `InvalidOperationException` at first request — Phase 1A was never actually run
end-to-end before this spec called it done. Registered now, alongside the existing
`LifeAreaReferenceRepository` line.

## Layout

Three regions, left to right (stacks vertically under the sub-desktop breakpoint, below):

1. **Picker** — Category (10, from `LifeMatterReferenceRepository.GetCategories`) → Step (96,
   `GetSteps(ruleSetId, categoryCode)`), plus the Varga control (Auto / D1 / Manual — see below).
2. **Chart** — one [`SindHovGrid`](specs_sind_hov_grid.md), fed the resolved Step's focus.
3. **Evidence cards** — the simplified-column projections (below), stacked, each independently
   loading/empty/error per the plan's explicit-states rule.

## Orchestration

One page-load snapshot query, per the plan's query/performance contract ("Use one page-load
snapshot or bounded aggregate query, never a repository call per row"):

1. On page load: `GetCategories` (10 rows), plus **all** active `LifeMatterFocusRepository.
   GetSubjects`/`GetFoci` and `KarakaMatterRepository.GetForRuleSet` rows for the chart's
   `RuleSetId` — these three repositories already return the *whole* rule set unfiltered by
   LifeMatter (see their signatures), i.e. they're already shaped as bulk snapshot queries, not
   per-row calls. Project client-side from there.
2. Category select → `GetSteps(ruleSetId, categoryCode)` (cheap, already category-filtered
   server-side).
3. Step select → `LifeMatterFocusResolver.Resolve(ruleSetId, lifeMatterId, <the three snapshot
   collections>)` — pure, synchronous, no additional query. Returns `ResolvedLifeMatterFocus`
   with `IsFocusStructured` for the unstructured-focus explicit state.
4. Varga resolution (Auto/D1/Manual, below) picks the chart; its `AscendantSign` and
   `PlanetsBySign`/`SpecialPointsBySign` dictionaries — already the shape `SouthIndianGrid_
   Detailed`/`_Micro` build from `ChartViewModel` today — are reused, not recomputed, for
   `SindHovGrid`.
5. `ResolvedLifeMatterFocus.HouseAndSpecialPointFoci` maps directly to `SindHovGrid`'s
   `HouseFoci`/`SpecialPointFocusCodes` parameters: `FocusKind.House` rows → `SindHovHouseFocus
   (ReferenceSign, HouseNumber)` (the plan's Focus schema stores a relative house number off a
   `ReferencePoint`, not a sign — resolving `ReferencePoint` to a concrete sign for the *current*
   chart, e.g. `LAGNA` → that chart's own Ascendant sign, is this step's job); `FocusKind.
   SpecialPoint` rows → their `SpecialPointCode` directly.
6. Cancellation/version token on Step change, per the plan — last-selection-wins, matching
   `SindHovGrid`'s own last-hover-wins preview behavior one layer up.

Phase 3C must record concrete query counts for initial load and Step switching, per the plan —
not measurable until this orchestration exists to profile.

## Varga control (Auto / D1 / Manual)

- **Auto** (default): follows the Step's `Subject` (from `LifeMatterFocusResolver`'s
  `ResolvedLifeMatterFocus.Subject.ChartTypeCode`). If `Subject` is `null` (no active mapping —
  today, **31 of 96 `PVR_LIFE_MATTER` LifeMatters have no viable Subject mapping at all**, see
  [`lifematters_claude_research.md`](../lifematters_claude_research.md)'s headline finding),
  retain whatever chart was last displayed — never blank.
- **D1**: pins the foundation chart regardless of Step.
- **Manual**: the existing chart-type selector (already used elsewhere in the app), stays Manual
  across Step changes until the user picks Auto or D1 again.

Clicking the already-selected Step keeps it selected (no-op, not a deselect) — the fallback for
any unresolvable state is D1/Rasi, never a blank chart.

## Simplified-column projections

Each card: natural ordering preserved, unions matches across multiple foci, explicit no-rows
message, `Contribution` badge from `lifematters_claude_research.md`'s copy templates. Read paths
from that doc's Key Inference mapping table:

| Card | Focus | Read path | v1 status |
|---|---|---|---|
| Argala | house-focused | `ArgalaRuleRepository.GetArgalaSignificanceNotes` is a static rule-level dictionary (`HouseOffset -> note`), **not per-chart evidence**. The real per-chart facts live in `tbl_Fact_Argala`, but `ArgalaFactRepository` only has `InsertAll`/`DeleteForChart`/`DeleteByBirthDetailId` — no `GetByChartResultId`/`GetByBirthDetailId` read method exists anywhere in the app. **Corrected during the Phase 3B1 build** — this row previously said "ready"; it was never actually checked against the repository's real methods. | **blocked** |
| Avastha | planet-focused | `PlanetaryStateRepository`, `PlanetaryStateRuleRepository`, `PostureStateInterpretationRepository` | ready — **built** (Phase 3B1, combined with Planet condition below into one reused `PlanetaryStateTable`) |
| Strength | planet-focused, **Shadbala only** | `PlanetaryStrengthRepository.GetSummaryByBirthDetailId`/`GetComponentsByBirthDetailId` | ready — **built** (Phase 3B1, same combined table) |
| Arudha | house/special-point-focused | **gap** — no dedicated repository; today only rendered as grid labels (`SpecialPointLabels`/`GrahaArudhaLabels`) via `NaisargikaKarakaRepository`/`ArudhaCalculator` seeds, not a queryable evidence table. Needs a read path before this card can be built. | **blocked** |
| House condition | lord, lord placement, occupants, aspects, conjunctions | `ChartHouseLordsRepository`, `ChartHouseLordInterpretationRepository`, `ChartAspectsRepository`, `ChartConjunctionsRepository`, `ChartMultiGrahaConjunctionRepository` | ready — **built** (Phase 3B1: lord/placement/occupants only so far, not aspects/conjunctions yet); **does not summarize Bhava Bala**, links to Key Inference instead (copy template in the research doc). Always empty today since no House-kind Focus rows are seeded. |
| Planet condition | dignity, functional nature, owned houses, combustion/retrograde, avastha, Shadbala | `ChartViewModel.BuildPlanetRows`/`BuildExaltationRows`, `GrahaDrishtiStrengthRepository`, `RasiNakshatraCombinationRepository`, `ChartMoonContextRepository` | ready — **built** (Phase 3B1), but via the existing `PlanetaryStateTable` component (dignity from its `KeyDetails` param) rather than the listed repositories directly; `GrahaDrishtiStrengthRepository`/`RasiNakshatraCombinationRepository`/`ChartMoonContextRepository` are not yet wired in |
| Relationships | Karaka-pair-focused | **Resolved, not via a repository**: `DignityEngine.EvaluatePairRelationship(planet, planet, sign, sign)` — the same pure Core computation Dignity/Shadbala/Yoga evaluators already call internally, public and stateless. No `tbl_Rule_CompoundRelationship` read needed; sign placements come from the already-loaded `LoadedChart.Grahas`. | ready — **built** (Phase 3B2), every unique pair among the Step's Karaka planets in the currently displayed chart |
| Relevant yogas (filtered) | — | `YogaEvaluationRepository.GetByBirthDetailId` — ready. **"Filtered" is not achievable in v1**: `YogaEvaluationRow` (from `vw_ChartYogaEvaluations`) carries no involved-planets/involved-houses columns, only free-text `YogaRule`/`Notes`, so there is no structured way to filter by relevance to a matter's Karakas/Focus. `InterpretationRepository.GetBySubjectType` was never actually needed/used. | **built, narrowed**: lists every `Present = true` yoga, honestly labeled as unfiltered, rather than inventing a text-matching heuristic |
| D1/Varga comparison | explicit `Confirms\|Modifies\|Contradicts\|Insufficient` | no engine yet — Phase 3B3, deferred | Phase 3B3 |
| Contradictions/missing evidence | — | derived client-side from the cards above, no new repository | Phase 3B3 |
| Provenance | source code/locator, RuleSet/version, calculation method, claim type | `SourceRefCode`/`RuleSetId` columns already on every rule table read above — surfaced, not separately queried | ready |

Bhava Bala, Ashtakavarga, Amsabala cards are **Phase 4**, deliberately excluded from v1 despite
their repositories (`BhavaStrengthRepository`, `AshtakavargaRepository`, `AmsabalaRepository`)
already existing and being persisted — don't build these cards early just because the data is
available.

## Drill-through

`/key-inference/{id}?step=about-houses&chart=D9&house=7` per the plan's example — **not supported
by `KeyInference.razor` today** (Phase 0A audit: it only reads `step` via an if-chain, `about-
houses` isn't a recognized value — only `houses` is — and there's no `chart=`/`house=` query
param at all). Building this page's drill-through links is safe to do on schedule; wiring
`KeyInference.razor` to receive them is separate, currently-unscheduled work that must land before
the links are useful — flag this dependency explicitly in Phase 3B3 planning rather than
discovering it at integration time.

## Responsive rules

Desktop: three-region layout above, side by side (chart region sized to `SindHovGrid`'s own
480px max-width). Sub-desktop breakpoint: stack Picker → Chart → Evidence cards vertically,
full-width, each evidence card individually collapsible (not required open) to keep the stack
scannable on a phone-width viewport — a new rule for this page, since `SindHovGrid`'s own
720px breakpoint only resizes the grid, it doesn't restack anything above it.

## Explicit states

Per the plan's required list, each with the copy from `lifematters_claude_research.md`:
unknown chart ID, missing workspace/birth data, uncomputed Varga (reuse the chart's existing
empty state), repository failure, unstructured focus (`!IsFocusStructured`), empty evidence card
(per card), invalid route, rapid-selection race (last-selection-wins, silent). Page-level v1
disclaimer (copy template in the research doc) renders once, persistently, not per card.

## Testing note

`LifeMatterFocusResolverTests` and `SindHovGridTests` already cover their own units in isolation
(Phase 1B/2 fixture testing, per the plan). This page has no bUnit test yet — **Gap, tracked for
Phase 3C**. It was instead verified live in a browser (Ramakrishnan, person id 4) against the real
dev DB: Category → Step selection, the "Inherent strengths and weaknesses" deliberately-unmapped
Step showing "Relevant evidence not yet structured for this matter," Manual Varga switching to D9
with correct house-from-Lagna recomputation, and Auto correctly retaining the last-displayed chart
(D9) rather than blanking, since `tbl_Rule_LifeMatterSubject` has no seeded rows yet. No console
errors, no failed requests. Still needed before Phase 3C sign-off: a bUnit suite for the
lifecycle transitions above, drill-through URL construction (once Key Inference accepts it), and
the same chain scripted as an automated (not just manual-browser) regression.
