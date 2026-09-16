---
last_updated: 2026-09-17
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
3.3 Astavarga / 3.4 Amsabala), 4 Spl Lagnas (was "Karakas"), 5 Yogas, 6 Vargas. This is the
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

## The flow

| Step | Chart | Table | Source |
|---|---|---|---|
| **1 · D1 / Transit** | D1 South-Indian grid (D1 Birth tab) · natal+transit wheel with date/dasha selector (Current Transit tab) | D1 position table (House · Planet · Sign · Degree · **Nakṣatra · Pāda** — moved here from Planet Dignity) + D1 Birth / Current Transit toggle | `vw_ChartPlanetEvidence` (D1) · `tbl_TransitPositionReference` |
| **2.1 · About Houses** | — (occupancy bar dropped; see note below) | Aspects (left) + multi-graha Conjunctions (right), side by side; House Lord Placement (lords + occupants); House Lord Key Findings — three tables, not one | `tbl_Chart_Aspects` + `tbl_Chart_MultiGrahaConjunction(+Member)` + `tbl_Chart_HouseLords` + `vw_ChartHouseLordInterpretation` |
| — supporting cards | — | Arudha padas (A1…A12, AL) · Upagrahas (11) · Special Lagnas — **computed, previously never surfaced** | `tbl_Chart_KeyDetails` `PointKind IN ('Arudha','Upagraha','SpecialLagna')` |
| **2.2 · About Planets** | — (closeness-to-exaltation bar dropped 2026-09-14; see note below) | dignity + Chara Kāraka + exaltation point + Δ + closeness, one row per planet/point | `tbl_Chart_KeyDetails` (+ Moon pañchāṅga facts card from `vw_ChartMoonContext`) |
| **3 · Strength** | 3.1 `PlanetStrengthChart` — %-of-minimum bar (Performance) or a component-composition stacked bar (Composition toggle), plus a per-graha expandable Bala breakdown. 3.2 `HouseStrengthChart` — Rūpas bar (scale is dynamic — see 2026-09-17 note below, not the fixed 0–9 this spec originally called for), House order/Strength rank toggle, per-house expandable breakdown. 3.3 `AshtakavargaChart` — Sarvāṣṭakavarga bar (House order/Strength rank toggle) + Bhinnāṣṭakavarga grid (7×12) + Piṇḍa table. 3.4 `AmsabalaTable` — Vargottama/Shadvarga/Saptavarga/Dasavarga/Shodasavarga scheme selector; the 4 varga-group schemes each render a stacked equal-width bar per graha (one segment per varga in that scheme, coloured by that varga's actual planetary dignity, empty for a non-matching varga) | Bar and table are one component each (not chart+table separately — the bar sits inline in the row) | `vw_ChartShadbala` + `tbl_Fact_PlanetaryStrengthComponent` · `vw_ChartBhavaBala` + `tbl_Fact_BhavaStrengthComponent` (both via `PlanetaryStrengthRepository`/`BhavaStrengthRepository`'s `GetSummaryByBirthDetailId`/`GetComponentsByBirthDetailId`) · `vw_ChartAshtakavarga` + `tbl_Fact_AshtakavargaPinda` (`AshtakavargaRepository`) · `vw_ChartAmsabala` + `tbl_Rule_AmsabalaGroup`/`tbl_Rule_AmsabalaName` (`AmsabalaRepository`/`AmsabalaSchemeRepository`) + `tbl_Fact_Vargottama` (`VargottamaRepository.GetByBirthDetailId`) |
| **4 · Spl Lagnas** | Interactive Karaka wheel (`KarakaPolarWheel`/`PolarGridLagnaSelect`) with house, planet and special-Lagna selection; planets now render on the polar wheel itself (2026-09-17 fix — `PlanetPlacements` was already passed in but never drawn in the `polar` view branch, only the `south` grid view used it) | Special Lagnas table (all 4 — Bhaava/Hora/Ghati/Sree — with Sign/House/Degree/Signifies, not just the 3 marked on the wheel) | `tbl_Rule_LifeMatterReference` + Naisargika/relationship rule tables through `NaisargikaKarakaRepository`; persisted varga placements through `WorkspaceData` |
| **5 · Yogas** | — (coverage donut planned, not built) | Yogas Present: Type · Yoga · Rule · Source, deduplicated by `YogaCode` (a yoga can match several classical source citations independently — e.g. Daridra against Raman combinations #148/#149/#151/#152 — which used to list it once per citation) and sorted by type in Lagna/Sun/Moon/Combination order, any other `YogaTypeCode` following alphabetically | `vw_ChartYogaEvaluations` (+ `tbl_Rule_Yoga`) via `YogaEvaluationRepository` |
| **6 · Vargas** | `VargaLordsTable` — sign + varga lord per planet across every generated divisional chart, highlighting Vargottama (D9 sign = D1 sign) and same-sign-as-D1 elsewhere | (chart doubles as the table — no separate table) | No new repository — reads `WorkspaceData.Charts` (every chart type's `Grahas` + `HouseLords`, already loaded for `AllCharts.razor`'s grid) |
| **7 · All Charts** | — | Every stored divisional chart as a plain South-Indian grid (unchanged from before this round) | separate `/charts/{id}` route (`AllCharts.razor`), heading reads "7. ALL CHARTS" |

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
  - **3.3 Astavarga bug:** the same Razor "email address" heuristic noted above hit
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
