---
last_updated: 2026-09-11
workstream: ui
component: KeyInference
route: /key-inference/{id}
togaf: C — component spec
---

# Component — Key Inference

`KeyInference.razor` — **step 1 and step 2.1 built** (2026-09-14); steps 2.2 and 3–6 land in
later passes. This is the spec-of-record for the **round-2 redesign** (2026-09-11), which
restructures the original flat "KEY INFERENCE header, 8 sub-tabs" shape into a **numbered UX
flow**. Mockups:

- Round 1 (flat tabs, frozen): [`../../artifacts/ui/v2-mockup/chart-evidence-hub.html`](../../artifacts/ui/v2-mockup/chart-evidence-hub.html) `#key-inference`
- **Round 2 (this spec, under review):** [`../../artifacts/ui/v2-mockup/key-inference-v2.html`](../../artifacts/ui/v2-mockup/key-inference-v2.html)

Design rule for this page: **one step, ideally one chart (hand-rolled SVG — bar / stacked
bar / donut, no library, same discipline as `design-language.md`) + one primary table.**
Bent twice on purpose, both noted at the step: Strength (two short tables, different
columns) and Planet-Chart (four related views of one 16-varga dataset share one chart + one
grid, with the rest as inline tags rather than more tables).

`TIME PERIOD (DASHA)` and `SATURN TIME PERIOD` are **unchanged** by this round — still
separate headers alongside this flow, not steps in it.

## The flow

| Step | Chart | Table | Source |
|---|---|---|---|
| **1 · D1 / Transit** | D1 South-Indian grid (D1 Birth tab) · natal+transit wheel with date/dasha selector (Current Transit tab) | D1 position table (House · Planet · Sign · Degree · **Nakṣatra · Pāda** — moved here from Planet Dignity) + D1 Birth / Current Transit toggle | `vw_ChartPlanetEvidence` (D1) · `tbl_TransitPositionReference` |
| **2.1 · About Houses** | — (occupancy bar dropped; see note below) | Aspects (left) + multi-graha Conjunctions (right), side by side; House Lord Placement (lords + occupants); House Lord Key Findings — three tables, not one | `tbl_Chart_Aspects` + `tbl_Chart_MultiGrahaConjunction(+Member)` + `tbl_Chart_HouseLords` + `vw_ChartHouseLordInterpretation` |
| — supporting cards | — | Arudha padas (A1…A12, AL) · Upagrahas (11) · Special Lagnas — **computed, previously never surfaced** | `tbl_Chart_KeyDetails` `PointKind IN ('Arudha','Upagraha','SpecialLagna')` |
| **2.2 · About Planets** | closeness-to-exaltation bar (7 grahas) | dignity + Chara Kāraka + exaltation point + Δ + closeness, one row per planet/point | `tbl_Chart_KeyDetails` (+ Moon pañchāṅga facts card from `vw_ChartMoonContext`) |
| **3 · Strength** | Shadbala bar chart with a minimum-required reference line | Planet Strength table + House Strength table (two, not merged — different columns) | `vw_ChartShadbala` · `vw_ChartBhavaBala` |
| **4 · Planet-Chart** | Vaiśeṣikāṁśa stacked bar (exalt/own/MT · friend · debil/enemy · neutral, of 16) | Ṣoḍaśavarga sign grid (9 grahas × 16 vargas, vargottama tinted) | `tbl_Chart_KeyDetails` across the 16 divisional `ChartType`s |
| — inline | — | Vargottama list + Varga-Dignity highlight tags (not separate tables) | `tbl_Fact_Vargottama` · `tbl_Chart_KeyDetails.DignityStatus` |
| **5 · Ashtakavarga** | Sarvāṣṭakavarga bar (bindus per sign) | Bhinnāṣṭakavarga grid (7×12) + Piṇḍa table | `vw_ChartAshtakavarga` · `tbl_Fact_AshtakavargaPinda` |
| **6 · Yoga** | coverage donut (Present / Absent / Not evaluated) | Source · Yoga · Type · Rule · Result | `vw_ChartYogaEvaluations` (+ `tbl_Rule_Yoga`) |

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
