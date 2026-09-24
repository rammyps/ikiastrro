---
last_updated: 2026-09-24
workstream: ui
component: KeyInference
route: /key-inference/{id}
togaf: C — component spec
---

# Component — Key Inference

`KeyInference.razor` — **all 6 in-page steps built**, plus step 7 (All Charts) as the separate
`/charts/{id}` route (its heading literally reads "7. ALL CHARTS" so the numbering stays
consistent across the two routes). Final step order (2026-09-16/17, superseding the round-1
6-step plan below): 1 D1-Transit, 2 About, 3 Strength (3.1 Planet Strength / 3.2 House Strength /
3.3 Asthavarga / 3.4 Amsabala), 4 Spl Lagnas (was "Karakas"), 5 Yogas, 6 Vargas. This is the
spec-of-record for the **round-2 redesign** (2026-09-11), which
restructures the original flat "KEY INFERENCE header, 8 sub-tabs" shape into a **numbered UX
flow**. Mockups:

- Round 1 (flat tabs, frozen): [`../../artifacts/ui/v2-mockup/chart-evidence-hub.html`](../../artifacts/ui/v2-mockup/chart-evidence-hub.html) `#key-inference`
- **Round 2 (this spec, under review):** [`../../artifacts/ui/v2-mockup/key-inference-v2.html`](../../artifacts/ui/v2-mockup/key-inference-v2.html)

Design rule for this page: **one step, ideally one chart (hand-rolled SVG — bar / stacked
bar / donut, no library, same discipline as `design-language.md`) + one primary table.**
Bent on purpose at several steps, each noted there: About Houses and About Planets are
table-only (no chart), Strength runs two short tables with different columns instead of one,
and Planet-Chart shares one chart + one grid across four related views of the same 16-varga
dataset, with the rest as inline tags rather than more tables.

`TIME PERIOD (DASHA)` and `SATURN TIME PERIOD` are **unchanged** by this round — still
separate headers alongside this flow, not steps in it.

**2026-09-17 — per-step spec series started:** step detail is moving out of this file's flow
table into one dedicated `specs_KI_<step>.md` per step. Step 4 is the first to move (see its row
below); the rest stay inline here until split out the same way in later work.

## The flow

| Step | Chart | Table | Source |
|---|---|---|---|
| **1 · D1 / Transit** | D1 South-Indian grid (D1 Birth tab) · natal+transit wheel with date/dasha selector (Current Transit tab) | D1 position table only (Planet · Sign · Degree · Nakṣatra · Pāda · Nakṣatra/Sub-lord chain · direction · house from Lagna/Moon) + D1 Birth / Current Transit toggle; analytical role/condition columns belong exclusively to 2.2 | `vw_ChartPlanetEvidence` (D1) · `tbl_TransitPositionReference` |
| **1.4 · Relationships** | `GrahaDrishtiMatrix` — D1/D9/D10 selector, focus-body summary, heat matrix, discrete ordinal badges, and selected-cell evidence breakdown | Sphuṭa percentage and discrete aspect metadata remain visually distinct; chart-matched conjunction groups follow the matrix. This is the sole full-detail relationship owner. | `vw_ChartGrahaDrishtiStrengths` via `GrahaDrishtiStrengthRepository` + `tbl_Chart_MultiGrahaConjunction(+Member)` |
| **1.2 · About Houses** | — | House Lord Placement; House Lord Key Findings; Argala &amp; Virodhargala | `tbl_Chart_HouseLords` + `vw_ChartHouseLordInterpretation` + live `ArgalaCalculator` |
| — supporting cards | — | Arudha padas (A1…A12, AL) · Upagrahas (11) · Special Lagnas — **computed, previously never surfaced** | `tbl_Chart_KeyDetails` `PointKind IN ('Arudha','Upagraha','SpecialLagna')` |
| **1.3 · About Planets** | — (closeness-to-exaltation bar dropped 2026-09-14; see note below) | functional nature + ruled houses + independent ownership flags (māraka, bādhaka, dusthāna, triṣaḍāya, Kendrādhipati doṣa) + rationale disclosure + dignity + Chara Kāraka + exaltation point + Δ + closeness, one row per planet/point | `tbl_Chart_KeyDetails` (+ Moon pañchāṅga facts card from `vw_ChartMoonContext`) |
| **1.1 · Overview** | — | `PlanetPositionsD1Transposed` — the default natal key table, with grahas as columns and Degree / Direction / House / Rāśi / Rāśi Lord / Nakshatra / Nakshatra Lord / Sub-Lord Chain / Nakshatra Pāda rows | `tbl_Chart_KeyDetails` via `ChartViewModel.BuildPlanetRows` |
| **1.2 · About Signs & Nakshatras** (new 2026-09-22) | — | `RasiNakshatraTable` — one row per graha (+ Lagna): Sign, Nakshatra, Lord Relation, Combined Character, expand-to-reveal Main Significations / Potential Benefits / Potential Disadvantages / Judgment Note / Aspecting Signs. Closes `FEAT-NAKSHATRA-02`'s last open item — the 36-row Rāśi×Nakṣatra combination table had zero Web/CLI consumer before this | `tbl_Rule_RasiNakshatraCombination` (migration 124) via `RasiNakshatraCombinationRepository.GetAll`, joined client-side against each graha's own `SignId`/`NakshatraId` from `tbl_Chart_KeyDetails` |
| **3 · Strength** | 3.1 `PlanetStrengthChart` — %-of-minimum bar (Performance) or a component-composition stacked bar (Composition toggle), plus a per-graha expandable Bala breakdown. 3.2 `HouseStrengthChart` — Rūpas bar (scale is dynamic — see 2026-09-17 note below, not the fixed 0–9 this spec originally called for), House order/Strength rank toggle, per-house expandable breakdown. 3.3 `AshtakavargaChart` — Varga dropdown (all generated Dn charts; independently persisted SAV/BAV + Piṇḍa for the selected varga) + Sarvāṣṭakavarga bar (House order/Strength rank toggle) + Bhinnāṣṭakavarga grid (7×12) + Piṇḍa table | Bar and table are one component each (not chart+table separately — the bar sits inline in the row) | `vw_ChartShadbala` + `tbl_Fact_PlanetaryStrengthComponent` · `vw_ChartBhavaBala` + `tbl_Fact_BhavaStrengthComponent` (both via `PlanetaryStrengthRepository`/`BhavaStrengthRepository`'s `GetSummaryByBirthDetailId`/`GetComponentsByBirthDetailId`) · `vw_ChartAshtakavarga` + `tbl_Fact_AshtakavargaPinda` (`AshtakavargaRepository`) |
| **4 · Spl Lagnas** | See [`specs_KI_spllagna.md`](specs_KI_spllagna.md) — full detail, including the proposed `SouthIndianGrid_Micro` Grid view, moved there 2026-09-17 | See [`specs_KI_spllagna.md`](specs_KI_spllagna.md) | See [`specs_KI_spllagna.md`](specs_KI_spllagna.md) |
| **5 · Yogas** | — (coverage donut planned, not built) | Yogas Present: Type · Yoga · Rule · Source, deduplicated by `YogaCode` (a yoga can match several classical source citations independently — e.g. Daridra against Raman combinations #148/#149/#151/#152 — which used to list it once per citation) and sorted by type in Lagna/Sun/Moon/Combination order, any other `YogaTypeCode` following alphabetically | `vw_ChartYogaEvaluations` (+ `tbl_Rule_Yoga`) via `YogaEvaluationRepository` |
| **6 · Vargas** | `AmsabalaTable` (2026-09-16 redesign, superseding the original flat `VargaLordsTable` **and** the standalone "3.4 Amsabala" Strength sub-tab) — Vargottama/Shadvarga/Saptavarga/Dasavarga/Shodasavarga scheme selector. Vargottama: D1 Sign/D9 Sign/Match per graha. Each of the 4 varga-group schemes: collapsed numeric split (GoodCount/GroupSize + %) per graha, expanding to a stacked equal-width bar (one segment per varga in that scheme, full 7-tier dignity colour — Exalted/Moolatrikona/Own/Great Friend/Friend/Enemy/Great Enemy — empty for Neutral/Debilitated) | Expanding a graha's bar also reveals a varga-lords detail table underneath it (Varga/Sign/Lord/Dignity/0–1 score) — the "chart doubles as summary, table is the drill-down" shape | `vw_ChartAmsabala` + `tbl_Rule_AmsabalaGroup`/`tbl_Rule_AmsabalaName` (`AmsabalaRepository`/`AmsabalaSchemeRepository`) + `tbl_Fact_Vargottama` (`VargottamaRepository.GetByBirthDetailId`) + `WorkspaceData.Charts` (Grahas/HouseLords, for the lords table and live dignity) — no new repository |
| **7 · All Charts** | — | Every stored divisional chart as a plain South-Indian grid (unchanged from before this round) | separate `/charts/{id}` route (`AllCharts.razor`), heading reads "7. ALL CHARTS" |

## Information ownership and deduplication

Each fact has **one full-detail owner** in Key Inference. Other tabs may show only the minimum
context needed to identify or explain their own result; they must not reproduce the owner's full
columns or recompute the fact independently.

| Information | Canonical owner | Context allowed elsewhere |
|---|---|---|
| D1 placement, longitude, nakṣatra/pāda, motion, house | 1.1 D1 Birth Chart | Planet/sign/house identifiers needed to label another result |
| House lordship, occupants, aspects, conjunctions, findings, argala | 2.1 About Houses | A compact house/lord label in strength or yoga evidence |
| Planetary functional role, owned houses, māraka/bādhaka/dusthāna/triṣaḍāya, Kendrādhipati doṣa, dignity, kāraka | 2.2 About Planets | A compact dignity/role cue only when it directly explains another score or finding |
| Rāśi–nakṣatra combined interpretation | 2.3 Sign & Nakshatras | Sign/nakṣatra as row identity only |
| Quantified planet/house strength | 3 Strength | Summary score in downstream evidence; component detail stays here |
| Special reference lagnas | 4 Spl Lagnas | Reference label in a rule finding |
| Yoga rule, source, result, interpretation | 5 Yogas | Yoga name/status summary only |
| Cross-varga comparison and confirmation | 6 Vargas | Varga name/sign needed to support a finding |

Implementation consequence: `PlanetPositionsTable` exposes `ShowAnalysis`; Key Inference 1.1
sets it false and also hides dignity. `PlanetDignityTable` is the sole full-detail planetary-role
surface and calls `LagnaFunctionalNature` once per classical planet. The old 84-row database
mirror must not return; if persisted UI provenance is later required, persist computed chart facts
from Core rather than duplicating the rule in SQL.
## New fields — sourcing status

| Field | Where | Status |
|---|---|---|
| **Exaltation point** | 2.2 | Classical Uchcha Bindu constants (Sun 10° Ari · Moon 3° Tau · Mars 28° Cap · Mercury 15° Vir · Jupiter 5° Can · Venus 27° Pis · Saturn 20° Lib). Rahu/Ketu excluded — no classical exaltation point. **Not a DB column yet** — a real build adds a small `tbl_Rule_Exaltation` (7 rows) rather than hard-coding the constants in a view. |
| **Δ from exaltation / Closeness %** | 2.2 | Derived: `Δ = min(|natal − exalt|, 360 − |natal − exalt|)` in absolute zodiacal degrees; `closeness% = round((1 − Δ/180) × 100)` (100% = exact exaltation, 0% = exact debilitation point). Computed from `tbl_Chart_KeyDetails.NirayanaLongitudeDegrees` + the exaltation rule table above — no new fact table needed, a read-time calculation. |
| **Yoga Type** (Sun / Moon / Lagna / combination — which reference point the yoga is judged from) | 6 | **DB-backed (`db/079`).** `tbl_Rule_Yoga.FormationFamilyCode`, exposed as `vw_ChartYogaEvaluations.YogaTypeCode`; constrained to `SUN`/`MOON`/`LAGNA`/`COMBINATION`. Seeded for 146 of 223 `YogaCode`s from the evaluator predicate — the rest read `NULL` (no predicate exists to derive from; see [`yoga.md`](yoga.md#2-yogas-round-2-columns--source-is-column-1-variant-dropped)). |
| **Yoga Rule** (one-line classical rule, e.g. *"7th lord in 5th"*) | 6 | **DB-backed (`db/079`).** New `tbl_Rule_Yoga.ShortFormationRule` column, exposed as `vw_ChartYogaEvaluations.YogaRule`; short-form, hand-transcribed per `YogaCode` from the actual predicate (not `CalculationNarrative`, which stays prose-length and unused here). Same 146/223 coverage as Type. |
| **Yoga Variant** | 6 | **Dropped from the table** (was `SourceVariantCode`) — stays in the underlying data, just not a visible column. `Source` (`SourceRefCode`) is column 1. |

## Also computed, now surfaced (was hidden pending this redesign)

Carried over from the round-1 review's "also computed" panel — each now has a home in the
flow above instead of sitting in a side panel:

| Data | New home | Source |
|---|---|---|
| Arudha padas (A1…A12, AL) | 2.1 supporting card | `tbl_Chart_KeyDetails · PointKind='Arudha'` |
| Upagrahas (Gulika, Māndi, +9) | 2.1 supporting card | `tbl_Chart_KeyDetails · PointKind='Upagraha'` |
| Special Lagnas (Hora Lagna, …) | 2.1 supporting card | `tbl_Chart_KeyDetails · PointKind='SpecialLagna'` |
| Per-planet nakṣatra + pāda | 1 · D1 position table | `tbl_Chart_KeyDetails.Nakshatra` / `NakshatraPada` |
| Directional aspects (graha dṛṣṭi) | 2.1, per house | `tbl_Chart_Aspects` |
| Conjunction groups (≥ 2 grahas) | 2.1, per house | `tbl_Chart_MultiGrahaConjunction(+Member)` |
| Planetary avasthās (Bālādi, Jāgradādi) | **still no table home** — deferred past this round | `tbl_Fact_PlanetaryState` |

## Open questions

- ~~Does step 1 replace the dedicated `/transit-wheel/{id}` landing, or stay a duplicate light
  view~~ **Decided (2026-09-14):** the standalone `/transit-wheel/{id}` page is gone. Its wheel
  chart, date/dasha selector, and Gochara table now render inside this page's own "Current
  Transit" tab (`Natal_Transit_Comp_WheelRepository`/`TransitSelection`/
  `Natal_Transit_Comp_WheelMath` reused as-is); the "D1 Birth" tab keeps the plain South-Indian
  grid + position table. `MainLayout`'s standalone TRANSIT nav tab was removed to match — Home's
  "open person" / post-generate navigation now lands on `/key-inference/{id}`.
- `tbl_Rule_Exaltation` is still a `workstream/database` follow-up once this flow is approved.
  The `tbl_Rule_Yoga` Type/Rule fields landed in `db/079` — see the sourcing-status table above.
- **Decided (2026-09-14):** 2.1 About Houses built as three tables, not the mockup's one merged
  "house lords & occupancy" table. Top row, side by side: `AspectsTable` (left, aspects grouped
  by the house they land in) and `HouseConjunctionsTable` (right, multi-graha groups per house —
  new, distinct from the pair-based `ConjunctionsTable` VargaView already uses). Below that,
  "House Lord Placement" (`HouseLordshipTable` + its new optional `Occupants` column) and "House
  Lord Key Findings" (new `HouseLordFindingsTable`, one row per `BranchCode`) reading
  `vw_ChartHouseLordInterpretation` (db/094) — the classical claims-per-house-lord-placement view
  that had no UI consumer yet. The Arudha/Upagraha/Special-Lagna supporting cards and the
  occupancy bar chart from the mockup are not built.
- **Built (2026-09-19) — 2.1 gains a 4th table, "Argala & Virodhargala"** (new `ArgalaTable`,
  `src/Ikiastrro.Web/Components/Charts/`): one row per house, the same 8-position shape as the
  book's own Exercise 16 worked table (2nd/4th/11th/5th argala, 12th/10th/3rd/9th virodhargala —
  see `argala-virodhargala-drishti-lifematters.md` §4), plus a Net column (`ArgalaCalculator
  .Compare`'s count-then-dignity-sum verdict) — **column order House · Sign · Net · (argala ·
  virodhargala offsets), Net moved up from last to 3rd 2026-09-22 (rammyps's correction)**.
  Computed LIVE off `lc.Grahas` via
  `ArgalaCalculator`/new `ArgalaFactBuilder.BuildOccupancy` (Core) — not read from
  `tbl_Fact_Argala`, since that fact table isn't wired into `ChartGenerationService` yet and only
  covers the 3 backfilled dev charts; live computation works for every chart, the same
  "compute at render time, don't require a backfill" choice 2.2's exaltation table already made.
  New `ArgalaRuleRepository.GetArgalaSignificanceNotes` (`tbl_Rule_Argala.SignificanceNote`,
  migration 129) feeds the 4 argala column headers' hover tooltips; the 4 virodhargala columns
  have none (PVR gives no parallel gloss for them in sec.10.7). The 3rd-from column shows a small
  "→arg" badge when the sec.10.6 "2+ malefics in 3rd" exception fires for that house; a row whose
  target sign holds Ketu gets a `*` (anti-zodiacal counting, sec.10.6 note).
- **Built (2026-09-14):** 2.2 About Planets — Moon-context fact chips (new
  `ChartMoonContextRepository` over `vw_ChartMoonContext`, loaded outside `WorkspaceData` like
  the Current Transit tab's Gochara/Dasha, since only this page needs it) above the "Planets —
  dignity, kāraka, exaltation" table (new `PlanetDignityTable`), reading one new
  `ChartViewModel.BuildExaltationRows` (Core) over `lc.KeyDetails` — the classical Uchcha Bindu
  constants from the sourcing-status table above, hard-coded there exactly as decided
  (`tbl_Rule_Exaltation` remains a `workstream/database` follow-up). Rahu/Ketu appear in the
  table with "—" in the three exaltation-derived columns.
- **Dropped (2026-09-14, rammyps):** the closeness-to-exaltation bar chart (`ExaltationClosenessChart`)
  built alongside the table above was removed from the page the same day — 2.2 is table-only
  now, joining 2.1 as an exception to this page's "one chart + one table" design rule. The
  component, its golden snapshot, and its `ChartFixture.ExaltationD1` fixture were deleted
  outright (nothing else referenced them); `ChartViewModel.BuildExaltationRows`/`ExaltationRow`
  stayed — the table still reads them.
- **Built (2026-09-14) — three-level tab restructure, rammyps's directive:** a new outer
  "master" `MudTabs` (`Position="Position.Left"`) holds one panel per step — "D1-TRANSIT" (step
  1) and "2. ABOUT" (step 2) — as a vertical rail on the page's left edge; each panel opens with
  its own `ki-stephead` (Step N of 6 pill + heading) to its right, then that step's own inner
  horizontal `MudTabs` (D1 Birth/Current Transit, or 2.1/2.2) and all its content. Every tab at
  every level now follows the new app-wide fill convention — see `design-language.md` "Tabs" —
  instead of MudBlazor's default text-plus-underline look. `ChartViewModel`'s classical
  exaltation constants also back-filled a real bug found while building 2.2:
  `ShadbalaCalculator.DeepExaltation`'s Venus entry was 327° (Aquarius 27°) instead of the
  classical 357° (Pisces 27°) every other reference in the codebase uses — fixed; a 30° error
  isolated to Venus's Uccha/Ishta/Kashta Bala, now visible in 3.1's per-planet breakdown.
- **Built (2026-09-14) — Step 3 "Strength," rammyps's directive to match the round-2 mockup
  PNGs closely (not the flatter original spec above):** `PlanetStrengthChart` (3.1) and
  `HouseStrengthChart` (3.2), new `Components/Charts/*.razor` + own `.razor.css` (no golden
  snapshot — table-shaped like PlanetDignityTable et al., not a standalone SVG chart module).
  Both read new typed repository methods — `PlanetaryStrengthRepository`/
  `BhavaStrengthRepository`'s `GetSummaryByBirthDetailId`/`GetComponentsByBirthDetailId` — over
  `vw_ChartShadbala`/`vw_ChartBhavaBala` plus the two `tbl_Fact_*Component` tables (previously
  only reachable through `AstrologerEvidenceRepository`'s generic dynamic-row query). Status
  thresholds (Planet: ≥100%/80–99%/&lt;80% Strong/Moderate/Weak; House: ≥7/5–6.99/&lt;5 Rūpas)
  and the whole `--bala-*` component-category palette are presentation-only, no `tbl_Rule_*`
  source yet — same status as the 2.2 exaltation constants above. 3.1 ranks strongest-first
  (PercentOfMinimum desc, not the mockup's unexplained order) and offers a Performance/
  Composition bar toggle (Composition reuses the per-row expand's stacked-Bala-share bar,
  clamped so a negative Dṛk/other share never draws past 100% of the track — a real bug hit
  building this, see `PlanetStrengthChart.SharePercent`'s comment). 3.2's Rank column always
  reflects Bhava Bala strength even when "House order" is toggled — only display order changes.
  Two rendering pitfalls worth remembering for the next chart-in-a-table component: Razor's
  "email address" heuristic silently drops `@expr` when glued directly to a preceding word
  character with a `.` later in the same token (`H@row.HouseNumber` rendered as literal text —
  fixed with `H@(row.HouseNumber)`); and a `<span>` bar/track needs an explicit `display:block`
  (or a flex/grid parent, which auto-blockifies it) or its `width`/`height` are silently ignored
  as an inline element (`HouseStrengthChart`'s `.hsc-track` hit this, `PlanetStrengthChart`'s
  `.psc-track` was saved by its `.psc-trackrow{display:flex}` parent).
- **Built (2026-09-16/17) — steps 3.4 Amsabala, 5 Yogas, 6 Vargas added; step 4 renamed Karakas
  → Spl Lagnas; per-panel headings dropped app-wide; chart-control convention introduced:**
  - **3.3 Asthavarga bug:** the same Razor "email address" heuristic noted above hit
    `AshtakavargaChart`'s `<small>H@HouseNumber(...)</small>` too (`H` immediately before `@`
    with no separator) — house numbers rendered as literal unevaluated text
    (`H@HouseNumber(row.SignNumber)`, clipped by the row's fixed width to just "H@House").
    Fixed the same way, `H@(HouseNumber(...))`.
  - **3.4 Amsabala** (new): scheme-tabbed Vargottama/Shadvarga/Saptavarga/Dasavarga/
    Shodasavarga. Vargottama shows D1 sign/D9 sign/Match per graha
    (`VargottamaRepository.GetByBirthDetailId`, new — the repository only had a write path
    before). The 4 varga-group schemes render a subtext ("N charts compared (Rasi chart with
    D-x,D-y,…)") and a stacked equal-width bar per graha, built from `tbl_Rule_AmsabalaGroup`'s
    actual seeded membership rather than a hardcoded copy of it — one segment per varga in that
    scheme; a segment is coloured/named only when that specific varga is Exalted or
    Own/Moolatrikona/Great Friend for that graha (`ChartViewModel.DignityToken`, read live off
    `WorkspaceData.Charts`, not from `AmsabalaRow.Detail`'s `*`-marked good/not-good boolean —
    the full dignity tier needed distinguishing "very good" from "fine"), non-matching vargas
    render as an unlabelled empty slot so the row still reads as "N out of GroupSize".
  - **5 Yogas** (new, `YogaEvaluationTable`): supersedes the coverage-donut + full-variant-list
    plan in [`yoga.md`](yoga.md) — see that file for the as-built shape (a single "Yogas
    Present" table, deduplicated and sorted by type).
  - **6 Vargas** (new, `VargaLordsTable`): needed no new repository — `WorkspaceData.Charts`
    already carries every generated chart type's `Grahas` (sign) and `HouseLords` (sign → lord),
    the same data `AllCharts.razor`'s grid already reads.
  - **4 Spl Lagnas:** renamed from "4. Karakas" — the embedded `KarakaPolarWheel` already titled
    itself "Special Lagnas & Karakas". Its own heading was then dropped too (2026-09-17, single
    tab under this step) along with its Chart-selector `<label>`/"CHART" caption; the Chart
    `<select>`, and the Wheel/Grid toggle (renamed from Polar-Wheel/SouthIND-Grid) now centre
    together where the heading used to sit. Planets were missing from the polar wheel entirely —
    `PolarGridLagnaSelect` received `PlanetPlacements` and even converted it to `PlanetsBySign`
    for the `south` grid view, but the `polar` SVG branch never referenced it; fixed by drawing
    each sector's planet glyphs (`ChartViewModel.PlanetGlyph`, dignity-coloured) at radius 260,
    between the sign ring (292) and the special-Lagna house-count ring (224). Hit the same
    Razor gotcha this file already had a workaround for once: a literal SVG `<text>` as the
    *first* tag inside an `@if(){ }` block (right after a `var` statement) is parsed as Razor's
    own reserved `<text>` markup-transition tag, which can't carry attributes
    (`RZ1023: "<text>" and "</text>" tags cannot contain attributes`) — wrap it in a `<g>` first,
    same fix the existing `pgls-point` block already used.
  - **Per-panel headings dropped everywhere** (D1-Transit/About/Strength's own `<h1>`, and each
    chart component's card `<h2>`+subtitle where it duplicated the tab pill above it) — the tab
    pill already names the step, so every panel now opens straight into its content. Where a
    panel lost its only spacing (the old heading's margin), `.ki-panelbody > .ki-tabs:first-child`
    carries a `margin-top` instead. Ashtakavarga/Amsabala kept their per-card `<h2>`s (they
    distinguish sub-sections a single step-level tab can't) but uppercased them and dropped
    their subtitle `<p>`s as redundant.
  - **Chart-control convention** (see `design-language.md` "Chart controls") replaced every
    in-chart toggle's ad hoc styling (`PlanetStrengthChart`/`HouseStrengthChart`'s toggle used
    to be a one-off midnight-bg/gold-text pair, not the app's tab tokens) with one dark-navy
    segmented-pill treatment, right-aligned and smaller than the card heading, reused identically
    by `AshtakavargaChart`, `PlanetStrengthChart`, `HouseStrengthChart`, `AmsabalaTable`'s scheme
    selector, and `KarakaPolarWheel`'s Wheel/Grid toggle + Chart `<select>`.
  - **House Strength Rūpas scale bug:** the bar/axis/column-header hardcoded a 0–9 max
    (`BarWidthPercent(...) / 9m`), but Bhavadhipati Bala alone is the house lord's whole
    Shadbala, which routinely clears 9 Rūpas for a strong lord — observed up to ~10.9 in the
    live database, silently clipped to 100% width before this fix. `ScaleMax` now rounds the
    actual max among the displayed houses up to a whole Rūpa (floor 1), and the axis/column
    header follow it instead of a fixed constant.
  - **Saved Charts hero image** moved from `saturn-rings-brand.png` to
    `ganesha-9planet-brand.png` — see [`saved-people.md`](saved-people.md) for the accompanying
    layout fix (the old full-bleed-left/flush-right hero had an asymmetric gap and cropped the
    art with `object-fit:cover`; now a normal symmetric grid gutter with `object-fit:contain`).
- **Built (2026-09-16 evening) — step 6 Vargas redesign, retiring `VargaLordsTable` and merging
  3.4 Amsabala into it (rammyps's directive):** the flat all-D-chart sign+lord table was
  reported as visually "mismatched" — on inspection its per-cell "same sign as D1" highlighting
  was actually correct, but the architecture wasn't what rammyps wanted: two different depths of
  the same Vargottama/Shadvarga/Saptavarga/Dasavarga/Shodasavarga breakdown living in two
  different steps (3.4 and 6). Consolidated into one scheme-tabbed view at step 6, `AmsabalaTable`
  relocated from 3.4 (that Strength sub-tab is now gone; a `"3.4"`/`"amsabala"` deep link routes
  to step 6 instead). Three additions beyond the relocation:
  1. **7-tier dignity coloring** (was binary good/not-good): `ChartViewModel.DignityTierToken`
     (new, `src/Ikiastrro.Core/Presentation/ChartViewModel.cs`) maps the 9-tier `DignityStatus`
     to 7 named tiers — Exalted/Moolatrikona/Own/Great Friend/Friend/Enemy/Great Enemy — added
     deliberately as a *new* function, not a change to the existing `DignityToken` (which every
     other dot-coloured view in the app — the D1 grid, polar wheel, etc. — still depends on for
     its 7-CSS-token collapse). Neutral/Debilitated aren't in rammyps's 7-tier list and still
     render as an unlabelled empty slot, same convention the bar already used. 3 new
     `--dignity-moolatrikona`/`--dignity-own`/`--dignity-great-friend` tokens (`tokens.css`)
     split back out of the old single `--dignity-good` — placeholder hex values, to be tuned
     live via claude-in-chrome against the running app.
  2. **Expand-to-detail interaction**: each graha row now collapses to just its numeric split
     ("N/GroupSize · P%") by default; clicking it expands to reveal the stacked bar and a new
     varga-lords detail table underneath (Varga/Sign/Lord/Dignity/Score) — the table reuses the
     old `VargaLordsTable.Cell()`'s exact per-chart sign+lord lookup, scoped to just the active
     scheme's chart-type membership instead of every generated D-chart.
  3. **0–1 normalized dignity score**: `ChartViewModel.DignityScoreNormalized` (new) —
     `(DignityScore + 4) / 8.0` over the existing -4..+4 scale — shown in the new detail table,
     for later statistical use (see `docs/research/domain/stat_strength.md`, the broader
     Shadbala/Amsabala/Bhavabala/Ashtakavarga normalization framework this is phase 0 of).

## 2026-09-23 — Natal/Transit navigation revision

- Master item 1 is **Natal Charts**, with a persisted-chart dropdown defaulting to D1.
- The default detail tab is 1.1 Overview (the restored key table), followed by 1.2 About Signs & Nakshatras, 1.3 About Houses, 1.4 About Planets, and 1.5 Relationships.
- Relationships includes graha drishti, rasi drishti, rasi/graha dispositor chains, and conjunctions for the selected chart.
- Master item 2 is **Transit Chart**. Its centered full-width wheel remains unchanged and has no collapse interaction.

## 2026-09-24 — master tab rail, chart toolbar, 7th step, nakshatra abbreviations

(Supersedes the 2026-09-23 "Collapse/Expand control was removed" line above — rammyps asked for
it back three revisions later; it turned out to add value after all.)

- **Master tab rail now really is evenly spread** across the 7 steps. The prior grid-stretch CSS
  targeted `.mud-tabs-toolbar-wrapper`/`-toolbar-inner`, which this MudBlazor version never
  renders (confirmed live via devtools), so it was inert and MudTabs' natural left-packed flex
  layout is what actually shipped. Corrected to the real DOM
  (`.mud-tabs-tabbar-wrapper` > each tab's `.mud-tooltip-root.mud-tooltip-inline` wrapper) — this
  does NOT trip MudTabs' overflow/scroll-arrow JS into "arrows + 2 tabs" mode, which the earlier
  investigation (2026-09-14) had worried it would.
- **7. ALL CHARTS is now its own master tab**, not just the separate `/charts/{id}` page. Both
  render the same `AllChartsGrid` component (`Components/Charts/**`) off the same already-loaded
  `WorkspaceData` — no extra query for the embedded tab. The standalone `/charts/{id}` route
  still exists (linked from the top app-bar's "ALL CHARTS") and still works for a direct
  person-scoped **Print** — see that section's own note.
- **Natal Chart picker moved.** It now sits directly above the D1 grid inside `.ki-gridwrap`
  (left-aligned with the chart, not spanning the full two-column layout), and dropped its visible
  "NATAL CHART" label — the label was redundant once the control sat right on top of what it
  controls (an `aria-label` covers it for screen readers). Still the documented dark-navy/cream
  "chart control" look (design-language.md "Chart controls").
- **Collapse/Expand Chart is back**, next to the picker in the same toolbar — same
  `_natalChartCollapsed` mechanism as before its 2026-09-23 removal.
- **Nakshatra column in 1.1 Overview now abbreviates** (`ChartViewModel.NakshatraShort`, 4
  letters, full name on the cell's `title` tooltip) instead of the full name. Full names up to
  "Purva Bhadrapada" (16 chars) were forcing either a horizontal scrollbar or a smaller font than
  the rest of the page in this 10-graha-column table — the fixed small font this table already
  uses (2026-09-23) stays consistent across every row now.

## 2026-09-23 (later) — sub-tab consolidation, moon context moved to chart view, tab palette flattened

Supersedes the "2026-09-23 — Natal/Transit navigation revision" section's sub-tab list above.

- **Master item 2 renamed "2. TRANSIT CHART" → "2. TRANSIT CHARTS"** — plural, matching "1.
  NATAL CHARTS" (was inconsistently singular).
- **Chart toolbar reordered**: EXPAND/COLLAPSE CHART now sits to the *left* of the chart-type
  dropdown (was right); the dropdown itself shrank (`max-width: 130px`, smaller font/padding —
  was `flex: 1`, stretching to fill the toolbar).
- **Natal Charts' 5 sub-tabs collapsed to 4:**
  - **1.1 GENERAL DETAILS** (renamed from "1.1 Overview") — now combines the old 1.1 Overview
    (`PlanetPositionsD1Transposed`) and 1.2 About Signs & Nakshatras (`RasiNakshatraTable`)
    tables, one after the other in a single tab, instead of two separate tabs.
  - **1.2 ABOUT PLANETS** (moved up from 1.4) — the Moon-context fact chips (Tithi/Pakṣa/
    Elongation/Birth/Moon nakṣatra + `LunarPhaseCard`) that used to open this tab were pulled
    out entirely. It now contains `PlanetDignityTable` under "Planets — dignity, kāraka,
    exaltation" followed by `PlanetaryStateTable`: Bālādi capacity + effect fraction,
    Jāgradādi availability, and Śayanādi activity for all nine grahas. Every state shows its
    Dim meaning; Śayanādi additionally resolves the source-grounded planet × state reading
    from `tbl_Rule_PostureStateInterpretation` (108 rows), keeping conditional clauses and
    `SRC_PVR_INTEGRATED` §15.4.4 provenance visible. The table explicitly treats the three
    systems as independent lenses, not a composite score or deterministic prediction.
    The Moon-context block moved to the **chart view** itself — a new
    `.ki-chart-moonfacts` block inside `.ki-gridwrap`, directly under the South-Indian grid —
    so it's visible under the chart no matter which sub-tab is open, instead of being one click
    away inside a single tab.
  - **1.3 ABOUT HOUSES** — unchanged content (House Lord Placement / House Lord Key Findings /
    Argala & Virodhargala), same tab index (2) it already had.
  - **1.4 ASPECTED (%)** (renamed from "1.5 Relationships") — unchanged content (Graha Dṛṣṭi
    strength, Rāśi Dṛṣṭi, Rāśi & graha dispositors, Conjunctions).
  - `KeyInference.razor`'s `Step` query-param routing updated to match: `overview`/`general`/
    `generaldetails`/`signs`/`nakshatras` all resolve to the new combined tab 0; `planets` → 1;
    `houses` → 2 (unchanged); `relationships`/`aspected` → 3.
- **Every nested sub-tab strip (1.1–1.4, and 3.1–3.3) now wraps its label onto two lines**
  instead of growing the pill wide — `.ki-subtabs ::deep .mud-tab` caps `max-width: 130px`,
  allows `white-space: normal`, and drops to `0.72×` the base control font size.
- **Tab palette flattened app-wide**: `--tab-active-bg`/`--tab-inactive-bg` both now resolve to
  `--brand-midnight` (dark navy) and `--tab-active-fg`/`--tab-inactive-fg` both to
  `--brand-sunset` (orange) — one flat look for every tab, active or not, replacing the
  sunset-fill-vs-cream-fill distinction from 2026-09-14. See `design-language.md` "Tabs".
- **Saved Charts (`/charts`) gained a per-row Print action**, `PrintIconButton`, to the right of
  the existing Delete icon button in each person's row-actions cell. It opens that person's
  `/charts/{id}?print=1` in a new tab; `AllCharts.razor` auto-fires `window.print()` once loaded
  when `print=1` is present, keeping printing scoped to one person (same reasoning as
  `AllChartsGrid`'s own Print button — see that component's comment).

## 2026-09-23 (evening) — further iteration: more two-line labels, tab palette reverted, Moon facts dropped, planet table reverted

Rapid follow-up round after the section above, done interactively (rammyps reviewing each step
live against a running instance before moving to the next):

- **Master rail relabeled and two-lined further**: "3. STRENGTH" → "3. ALL STRENGTH", "5. YOGAS"
  → "5. ALL YOGAS", "6. VARGAS" → "6. VARGA CHARTS" (all wrap via inline padding narrowing the
  text column within each step's already-even grid width). "4. SPL LAGNAS", "5. ALL YOGAS" and
  "7. ALL CHARTS" are short enough to otherwise fit on one line at that width, so their `Text` in
  `KeyInference.razor` carries an explicit `"\n"` at the word break rammyps wanted ("4. SPL" /
  "LAGNAS", "5. ALL" / "YOGAS", "7. ALL" / "CHARTS") — `.ki-mastertabs ::deep .ki-master-button`
  switched from `white-space: normal` to `white-space: pre-line` so that literal newline renders
  as a forced break instead of collapsing to a space (labels with no `"\n"` still wrap naturally
  when they don't fit).
- **Sub-tabs 1.2/1.3/1.4 got the same explicit-break treatment**: `Text` is now `"1.2 ABOUT\n
  PLANETS"`, `"1.3 ABOUT\nHOUSES"`, `"1.4 ASPECTED\n(%)"` — `.ki-subtabs ::deep .mud-tab` also
  moved to `white-space: pre-line` for the same reason.
- **Moon-context fact chips dropped entirely** (the `<dl class="ki-facts">` that briefly lived
  under `LunarPhaseCard` in the chart view per the section above) — rammyps's call: they were
  pure duplication, not new information. Tithi/Pakṣa/Elongation already read off
  `LunarPhaseCard` itself (its kicker, phase heading, and own Elongation/Illumination/Pakṣa Bala
  `<dl>`); Moon nakṣatra already has a full-detail home in 1.1 General Details' Sign & Nakshatra
  Combination table. Only `<LunarPhaseCard>` remains in `.ki-chart-moonfacts` now. The now-unused
  `.ki-facts`/`.ki-fact`/`.ki-moon-support` CSS and the `moonGraha`/`NakPadaLordByPlanet`-adjacent
  Moon nakṣatra lookup were removed with it.
- **1.1 General Details' planet-positions table reverted from `PlanetPositionsD1Transposed` back
  to `PlanetPositionsTable`** (`Variant="d1"`, `ShowAnalysis="false"`) — rammyps's call, the
  transposed planets-as-columns layout (added 2026-09-23 earlier the same day) was harder to
  read than the plain planets-as-rows table it replaced. `PlanetPositionsD1Transposed` (with its
  Nak Pada Lord column, read off the person's own D9 chart) still exists as a component but is
  now unused; `PlanetPositionsTable`'s only other caller remains `VargaView` (`Variant="varga"`).
- **Tab palette reverted**: `--tab-active-bg`/`--tab-inactive-bg` back to `--brand-sunset`
  (orange) and `--tab-active-fg`/`--tab-inactive-fg` back to `--brand-midnight` (dark navy) — the
  midnight-fill/sunset-text flip from earlier the same day didn't stick; still one flat look for
  every tab level in the app.

- **Built (2026-09-25) — natal chart kept visible on Strength/Yogas/Vargas; tab active-state
  color bug fixed; header nav palette flipped; rank/score badge convention introduced:**
  - **Chart column extended past NATAL CHARTS**: the toolbar (collapse/expand + chart picker) +
    `SouthIndianGrid_Detailed` that NATAL CHARTS already showed on the left is now also shown on
    STRENGTH, YOGAS and VARGAS (rammyps's call — those three read the chart while it's on
    screen too). Extracted into a shared `ChartColumn` `RenderFragment` in `KeyInference.razor`
    (same `_natalChartCollapsed`/`_natalChart` state as NATAL CHARTS, so the collapse toggle and
    chart-picker selection are shared, not reset per tab) and wrapped each of the three panels'
    existing content in the same `.ki-natal-layout`/`.ki-gridwrap`/`.ki-natal-detail` grid NATAL
    CHARTS already used. NATAL CHARTS' own inline copy was left untouched rather than refactored
    onto the new fragment, to avoid touching already-shipped, working markup for its own sake.
    TRANSIT CHARTS, SPL LAGNAS and ALL CHARTS were deliberately left alone (SPL LAGNAS already
    has its own dedicated chart view; ALL CHARTS *is* a chart grid).
  - **Real bug found and fixed: no tab anywhere on this page ever showed an active-state color.**
    `.ki-steptab[aria-selected="true"]` never matched anything — Blazor's bool attribute binding
    (`aria-selected="@(_activeTab == X)"`) renders the attribute with an *empty* value when true
    and omits it entirely when false, the same convention as `disabled`/`checked`; it never emits
    the literal string `"true"`. Selector changed to the bare `[aria-selected]` (presence check),
    which is what Blazor's own serialization actually produces. This affected every master tab
    and every sub-tab rail on the page, not something newly introduced — a pre-existing, silent
    styling no-op since the tab strip was rewritten off MudTabs (2026-09-23 evening, above).
  - **Header nav (`HOME`/`KEY INFERENCE`/`NUMEROLOGY`) active state flipped** (rammyps's call):
    `.ik-headtab.is-here` in `MainLayout.razor.css` now reads `background: var(--brand-midnight)`
    / `color: var(--brand-sunset)` — the inverse of the sub-tab rail's sunset-bg/midnight-text.
    Distinct component/token from the `--tab-active-bg` flip-and-revert noted directly above this
    entry — that one was the step-tab rail itself, which keeps its sunset-fill look; only the
    outer header nav changed here.
  - **Rank/score badge convention** (green/sunset-orange/red fills, sunrise-orange text on
    Strong/Weak, midnight text on Moderate) applied to `PlanetStrengthChart`/`HouseStrengthChart`
    rank badges, and `AshtakavargaChart`'s Sarvāṣṭavarga bar+value (cited `docs/research/domain/
    transit-events.md` SAV &gt;30/25-30/&lt;25 threshold) and Bhinnāṣṭavarga cells (existing
    presentation-only 0/1-5/≥6 bindu band, recolored). `--status-moderate` itself changed from an
    unrelated amber `#c9820a` to `var(--brand-sunset)` in `tokens.css`, so every existing
    `--status-moderate` consumer picked up the new color automatically. Piṇḍa (Rāśi/Graha/Śodhya)
    was deliberately left unbanded — those are multiplier/remainder values used to locate a
    target nakshatra/sign, not strength scores, and there's no cited threshold for what a "strong
    Piṇḍa" would even mean. Full convention + the compound-selector specificity trap hit building
    this: `docs/ui/design-language.md` "Rank/score badge convention."
  every tab, active or not, just the opposite colour pairing. See `design-language.md` "Tabs".