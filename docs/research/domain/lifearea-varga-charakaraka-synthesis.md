---
last_updated: 2026-09-22
status: research-baseline
---

# LifeArea ↔ Varga ↔ CharaKaraka synthesis — research + detail-dimension table design

Scoped as the next release's design work, per rammyps's 2026-09-17 direction: set
`karakafix.md`'s DB-migration-sequence detail aside for now, and focus on product-level
design for what ROADMAP.md's "Now" bucket already calls out — `FEAT-VARGA-01` (divisional
charts verified but not rendered) and `FEAT-KARAKA-01` (chara karakas verified but not
rendered) both have `Web [ ]`. No "v5" tag exists in this repo's own versioning (it ships
by pure Now/Next/Later flow, `git tag` + GitHub Release per `ROADMAP.md`, not numbered
releases) — treating "v5" here as this next flow-driven ship, closing the Web gap on those
two plus the connective layer this file researches.

**Scope confirmed 2026-09-22:** rammyps's direction was the full v5-notes-index.md batch, not
just this file's slice — see that index's 2026-09-22 update. This file's own proposals landed
as `FEAT-VARGA-02` / `FEAT-KARAKA-06` / `FEAT-HOUSE-05` in `ROADMAP.md`.

## 1. What already exists — three legs, none of them connected

### Leg A — LifeArea (chart-primary)

`tbl_Dim_LifeArea` (migration 30): 20 rows, one per PVR Table 11 sphere, FK'd directly
from `tbl_Dim_ChartType.PrimaryLifeAreaId`. Already fully documented in
[`chara-karaka-life-area-pvr.md`](chara-karaka-life-area-pvr.md) §"PVR Table 11". Carries
`PlaneOfExistence` (physical/mental/sub-conscious/karmic) and a `WorkspaceGroupCode` (the
Web-tab grouping from `LifeAreaMap.cs`, explicitly *not* from PVR). No House/Planet/Sign
columns — it's chart → area only.

### Leg B — DivisionalSubject (subject-primary) — unreconciled duplicate of Leg A

`tbl_Dim_DivisionalSubject` (migration 38): 12 rows, the **inverse** direction — subject
(e.g. "Wealth", "Marriage and relationships") → `PrimaryConfirmationChartId` (one Varga).
Also PVR-sourced (`SRC_PVR_INTEGRATED`) but a **separate, never-cross-checked** table from
Leg A. This is the same shape of problem `chara-karaka-life-area-pvr.md` already flagged
for `LifeAreaMap.cs`'s Raman-sourced sthira-karaka list: two PVR-adjacent tables covering
overlapping ground, built in different migrations (30 vs 38), never reconciled against each
other. Concretely: LifeArea's D-9 = "Marriage and everything related to spouse(s), dharma
… inner self" (one chart, broad); DivisionalSubject's `MARRIAGE_RELATIONSHIPS` also points
at D9 but is *subject*-keyed and separately carries `D1Foundation` text ("7th house, 7th
lord, Venus and partnership combinations") that Leg A has no equivalent column for at all.

Crucially, `D1Foundation` is exactly where the "4 key factors" the release needs already
live today — **but only as unstructured prose**, one string per subject, not normalized
columns. E.g. `MARRIAGE_RELATIONSHIPS.D1Foundation` = "7th house, 7th lord, Venus and
partnership combinations" encodes House=7, Planet=Venus, and implicitly Sign (7th-house
sign) — but nothing can query "which subjects use Venus" or "which subjects use house 7"
without parsing English text. This is the gap the new detail-dimension table (§3) closes.

### Leg C — Vargas

21 chart types (`tbl_Dim_ChartType`, D1–D60), calculators verified (`verify-vargas` green,
`FEAT-VARGA-01` at 80%), but only D1/D9 render in the Web workspace. Leg A and Leg B both
already key off `ChartType` (`PrimaryLifeAreaId` / `PrimaryConfirmationChartId`), so Varga
is the one leg that's structurally present in both — it's the join key, not a missing piece.

### Leg D — CharaKarakas — the missing leg

**Zero presence in either Leg A or Leg B.** Chara karaka's only DB footprint today is the
two `tbl_Rule_LifeMatterReference` rows (DK/PK) and the 8-row `tbl_Dim_KarakaRole` (chara
type) from migration 103 — neither joins to `tbl_Dim_LifeArea` or `tbl_Dim_DivisionalSubject`.
This confirms the gap `chara-karaka-life-area-pvr.md` already named ("Table 13's full
8-karaka person list has never been cross-referenced against Table 11's 20-chart life-area
list") extends to Leg B too, and is the third connection this release needs to design, not
just Leg A↔Leg B.

## 2. What "connecting" should mean

Not a merge of Leg A and Leg B into one table — PVR treats "which chart shows this subject"
(Leg A, B) and "which planet-role shows this person/matter" (Leg D, per Table 13) as
different techniques answering different questions, the same distinction
`chara-karaka-life-area-pvr.md` already established for naisargika/sthira/chara karaka
types generally. The synthesis is a **bridge**, not a collapse:

```
tbl_Dim_LifeArea (chart -> area)  <-\
                                      >-- reconciliation pass, not yet designed
tbl_Dim_DivisionalSubject (subject -> chart) <-/
                    |
                    | shared ChartType key (Leg C, already structural)
                    v
   detail-dimension bridge (House, Planet, Sign/Lagna, Varga) -- new, §3
                    |
                    v
       tbl_Dim_KarakaRole (Leg D — 8 chara roles, migration 103)
```

## 3. Detail dimension rules table — 4 key factors

Design, not yet a migration (same discipline as `karakafix.md` and
`chara-karaka-interpretation-statistics.md`): normalize what `DivisionalSubject.D1Foundation`
currently buries in prose into queryable rows, then let a chara-karaka role resolve into
the same shape so both legs and Leg D speak one structure.

### `tbl_Dim_InterpretiveFactor` — the 4-factor vocabulary

One row per factor *type*, not value — a small catalogue table:

| FactorCode | What it holds |
|---|---|
| `HOUSE` | A house number (1–12) or house-from-reference |
| `PLANET` | A graha (including nodes) |
| `SIGN_LAGNA` | A rasi, or "Lagna" as the zero-house reference point |
| `VARGA` | A `ChartType` (D1–D60) |

This reuses `tbl_Dim_InterpretationDimension`'s pattern (migration 38: small seeded
catalogue, `DimensionCode`/`Description`/`SortOrder`) but is a **different axis** — that
table classifies *how* a finding is interpreted (dignity, dispositor, house-lord,
divisional-confirmation); this one classifies *what kind of chart element* a rule cites.
They're complementary, not competing — don't conflate them.

### `tbl_Rule_InterpretiveFactorDetail` — the bridge/fact rows

One row per (subject-or-area-or-karaka-role, factor, value) triple:

- `Id`, `RuleSetId`
- `ReferenceTypeCode`: `LIFE_AREA` | `DIVISIONAL_SUBJECT` | `CHARA_KARAKA_ROLE` — which of
  the three legs this row details (keeps Leg A/B/D distinct per §2, doesn't merge them)
- `ReferenceId`: FK into whichever table `ReferenceTypeCode` points at
- `FactorCode`: FK to `tbl_Dim_InterpretiveFactor`
- `HouseNumber` (nullable), `GrahaId` (nullable), `SignId` (nullable), `ChartTypeId`
  (nullable) — exactly one populated, matching `FactorCode`; a check constraint enforces it
  (same "exactly the target appropriate to the type" pattern `karakafix.md` used for
  `tbl_Dim_KarakaRole`'s `FixedGrahaId`/`CharaKarakaCode` split)
- `IsPrimary`, `DisplayOrder`, `SourceRefCode`, `Notes`

This is what turns `MARRIAGE_RELATIONSHIPS.D1Foundation` = "7th house, 7th lord, Venus and
partnership combinations" into three queryable rows (`HOUSE=7`, `PLANET=Venus`,
`VARGA=D9`) instead of one opaque string — and gives chara-karaka roles (Leg D) the same
shape, so e.g. `DK`'s detail can carry `VARGA=D9`, `SIGN_LAGNA=<DK's own sign>` alongside
its existing `tbl_Rule_LifeMatterReference` narrative row, queryable the same way as a
`DIVISIONAL_SUBJECT` row.

## 4. Release framing — features needed

Proposed feature slices, in the project's `masterproduct.md` `FEAT-<AREA>-<NN>` shape (not
yet filed as GitHub Milestones — draft for triage):

- **`FEAT-VARGA-02` (Web)** — render D2–D60 in the workspace, not just D1/D9. Closes
  `FEAT-VARGA-01`'s `Web [ ]` gap. No new DB work; calculators already verified.
- **`FEAT-KARAKA-06` (Web)** — the "Life Matters" chara-karaka panel from `res_charakarakas.md`
  §2. Closes `FEAT-KARAKA-01`'s `Web [ ]` gap. Depends on Leg D detail rows (§3) existing
  for interpretation content, not just the already-computed role→planet resolution.
- **`FEAT-HOUSE-05` (DB, new)** — the Leg A↔Leg B reconciliation pass (§2) plus
  `tbl_Dim_InterpretiveFactor` / `tbl_Rule_InterpretiveFactorDetail` (§3). Feeds both of the
  above; this is the one actual migration in scope, and per this file's own discipline, not
  written yet — design input only.
- Relationship to `FEAT-HOUSE-03` (Bhāva significations + Sthira Kāraka mapping, Designed·0%
  in `masterproduct.md`): overlapping DB surface (both touch house/karaka mapping) but a
  distinct concern — `FEAT-HOUSE-03` is sthira karaka (Leg outside this file's scope, see
  `chara-karaka-life-area-pvr.md` §"Sthira karaka"), not chara karaka. Sequence them, don't
  merge them; `FEAT-HOUSE-05` should land first since sthira karaka's own doc already flags
  it needs a cited edition before any table work, whereas Leg A/B/D are all already PVR-cited.

### FEAT-VARGA-02 detail — divisional-chart dignity summary chart

New requirement from rammyps (2026-09-18), scoped inside `FEAT-VARGA-02`: alongside rendering
D2–D60 in the Web workspace, add a **higher-level stats** view per varga-group scheme — a
hand-rolled stacked-bar-chart component (`project_standards.md` §3 "Chart modules": inline
SVG/CSS grid, never a charting library) that summarizes how many (graha × varga) placements
land in each classical dignity tier, across that scheme's member vargas.

- **Scope — which "varga tabs":** the existing `AmsabalaTable` scheme tabs
  (`key-inference.md` §"6 Vargas") — Vargottama / Shadvarga / Saptavarga / Dasavarga /
  Shodasavarga. Applies to **every scheme except Vargottama** — that tab is a 2-chart (D1 vs
  D9) Match/no-Match comparison, not an N-varga membership group, so it has no per-tier
  "which vargas" breakdown to stack.
- **Categories:** the same 7-tier vocabulary `ChartViewModel.DignityTierToken` already
  introduced for the existing per-graha stacked bar (`key-inference.md` §"6 Vargas redesign,"
  point 1) — Exalted, Moolatrikona, Own, Great Friend, Friend, Enemy, Great Enemy.
  Neutral/Debilitated stay excluded from the count, same convention that bar already uses.
- **Shape:** one stacked bar per scheme tab, one segment per dignity tier present in that
  scheme, each segment labelled `<Tier> — <count> (<varga codes>)` — e.g. `Exalted — 2
  (D1, D24)`, `Own — 3 (D9, D2, D10)`. `count` = number of (graha, varga) pairs at that tier
  within the scheme; the varga-code list names which member vargas contributed.
- **Placement:** sits above the existing per-graha rows in each non-Vargottama scheme tab, as
  the at-a-glance summary before drilling into individual grahas.
- **Open sub-questions (not resolved here, need a call before this is built):**
  - De-duplicate the varga-code list per tier to unique codes (draft assumption, matching the
    worked example above), or list a code once per contributing graha even if that repeats a
    code?
  - One stacked bar per scheme tab (draft assumption — matches the tab it renders inside), or
    one combined bar spanning every rendered D-chart the person has, across all schemes?
  - Segment order: fixed Exalted→Great Enemy (matches `DignityScore`'s -4..+4 ranking), or
    count-descending?
  - Component name + spec doc, once designed, per `chart-catalog.md`'s naming convention
    (e.g. `VargaDignitySummaryChart` + `spec_VargaDignitySummaryChart.md`) — not filed yet.

## Open questions

- Reconciliation policy for Leg A vs Leg B when they disagree (e.g. does D-9's LifeArea
  wording get amended to match DivisionalSubject's narrower "Marriage" framing, or do both
  stay and a view unions them?) — not decided here, needs a call before `FEAT-HOUSE-05`.
- `tbl_Rule_InterpretiveFactorDetail`'s `ReferenceTypeCode` polymorphic FK (`ReferenceId`
  meaning depends on `ReferenceTypeCode`) is a normalization trade-off — three separate
  bridge tables (one per leg) would give real FKs instead but triple the table count for a
  first pass. Revisit once row counts are known (Leg A ×4 factors ≤ 80 rows, Leg B similar,
  Leg D ×4 ≤ 32 — all small; a single polymorphic table is likely fine at this scale, but
  flagging the trade-off since `karakafix.md` deliberately avoided a similar shortcut for
  `KarakaRole`).
- ~~Confirm "v5" scope~~ **Resolved 2026-09-22** — see the top-of-file note; the full
  v5-notes-index.md batch, not just this file's slice.
