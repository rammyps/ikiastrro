---
last_updated: 2026-09-25
workstream: ui
status: complete
---

# LifeMatters — Phase 0A audit

Findings for `lifematters_plan.md`'s Phase 0A ("repository/schema audit; real coverage counts
by LifeArea, DivisionalSubject, and KarakaRole; migration-087 application/reference audit;
taxonomy citation status"). Read-only verification against the live repo, 2026-09-25.

## Schema and row counts

| Table | Migration | Rows | Notes |
|---|---|---|---|
| `tbl_Dim_LifeMatter` | 103 | 128 | 32 `PVR_KARAKATWA_GRID` + 96 `PVR_LIFE_MATTER` (1:1 with `tbl_Rule_LifeMatterReference`) |
| `tbl_Rule_LifeMatterReference` | 087, FK added by 103 | 96 | `RuleSetId = 1` for every row; has both `CategoryName` and `MatterText` |
| `tbl_Dim_LifeArea` | 30 | 20 | PVR Table-11 sphere-per-Varga dimension |
| `tbl_Dim_DivisionalSubject` | 38 | 11 | Matches plan's expected count exactly |
| `tbl_Rule_InterpretiveFactorDetail` | 109 (+ `tbl_Dim_InterpretiveFactor`, 4 rows) | seeded only for the 11 `DivisionalSubject` House/Planet/Varga facts | LifeArea and chara-karaka-role details explicitly deferred in the migration's own header comment |
| `tbl_Rule_KarakaMatter` | 103 | 149 (12 `IsPrimary`) | `HouseNumber TINYINT NULL` populated only for the 34-row Karakatwa-grid batch (32 distinct matters); the other 115 rows leave it NULL — matches the plan's own caveat |
| `tbl_Dim_HouseReference` | 32 | 17 | **Already exists** — not a Phase-0A gap. Canonical `ReferencePoint` vocabulary: `LAGNA, CHANDRA_LAGNA, RAVI_LAGNA, ARUDHA_LAGNA, PAAKA_LAGNA, KARAKAMSA_LAGNA, GHATI_LAGNA, HORA_LAGNA, BHAAVA_LAGNA, SREE_LAGNA` + 7 `GRAHA_LAGNA_*`. Confirmed as the intended source by `specs_KI_spllagna.md`'s own data contract. |
| `tbl_Rule_LifeMatterSubject` | — | — | Does not exist. Confirmed new work. |
| `tbl_Rule_LifeMatterFocus` | — | — | Does not exist. Confirmed new work. |

`HouseFromLagnaText VARCHAR(30) NOT NULL` (free text, e.g. `'7th'`, `'6th/8th'`, `'5th from AL'`,
`'Upapada Lagna'`) lives on `tbl_Rule_LifeMatterReference` (migration 087) and already exists
today — it's the source the plan's "one-time offline proposal generator" will parse.

Latest live migration is **136** (`136_add_deeptadi_lajjitadi_avastha.sql`), contiguous 1–136.
`db/_archive` is a separately-numbered legacy set, not part of the live sequence.

## Migration-087 reference audit

`tbl_Rule_LifeMatterReference` rows all carry `RuleSetId = 1`, including the row with the known
transcription bug: `MARRIAGE_SPOUSE`, DisplayOrder 6, `HouseFromLagnaText = 'Upapada Lagna'`,
narrative text stating "Upapada Lagna (arudha of the 7th)" — incorrect; UL is A12, arudha of the
12th, while Darapada is A7.

Searched every `db/*.sql` file for `LifeMatterReferenceId` and `LifeMatterId`. Only three files
match: `087_seed_life_matter_reference.sql`, `103_create_karaka_role_matter_model.sql`, and
`133_seed_sthira_karaka.sql` — the migrations that define these columns themselves. No
`tbl_Fact_*` table, and no other migration, references this RuleSet or row.

**Conclusion:** no Fact/result row references RuleSetId 1 of `tbl_Rule_LifeMatterReference`
(unsurprising — no LifeMatters evidence table exists yet). Per the plan's migration policy, a
future correction **may edit the transcription in place** via a new migration; a new RuleSet
version is not required. Writing that corrective migration is deferred to a later phase; this
audit only unblocks it.

## Taxonomy citation status

No `tbl_Dim_ArudhaPadaNames`/`Aliases`-equivalent table exists. A1–A12 exist only as hardcoded
string literals in `ArudhaCalculator`, `ChartAnalyzer`, `SpecialPointSeed`,
`RamanAfflictionYogaEvaluator`, `PvrChapter11NumberedYogaEvaluator`, and
`D1TemplateCellData` — a wider call-site surface than the seed migration alone. Approved decision
1 (seed as `Uncited-flagged` pending a source) stands; the new tables are genuinely new
normalization, not a replacement of an existing table.

## Repository/service/component gaps (for Phase 1A/2/3 planning, not blocking Phase 0A)

- Only `LifeAreaReferenceRepository` and `InterpretiveFactorDetailRepository` exist as real C#
  repositories among the six entities named in Phase 1A — both currently unused by any page.
  `LifeMatterReference` is read only incidentally, via `NaisargikaKarakaRepository.LoadActive()`
  (doesn't select `Id`/`LifeMatterId`). `DivisionalSubject`, `KarakaMatter`, and
  `tbl_Dim_LifeMatter` have zero repository layer — only raw SQL in migrations/CLI.
- `SindUniDtlGrid`, `SouthIndianGrid_Micro`, and `PolarGridLagnaSelect` have no
  highlight/focus parameter today. SIND-HOV-GRID's house/sign highlighting is new component
  surface, not an extension of unused existing props.
- `AstroFacts.razor` only reads a `step` query param via an if-chain; `about-houses` is not a
  recognized step value (only `houses` is), and there is no `chart=`/`house=` query param at all.
  The plan's example drill-through URL (`/astro-facts/{id}?step=about-houses&chart=D9&house=7`)
  is not supported today and will need to be added.
- No single reusable "evidence-assembly service" exists — `AstroFacts.razor`'s code-behind
  wires ~20 separate repositories directly into page state per master tab (house/planet
  condition, Shadbala, Bhava Bala, Ashtakavarga, Amsabala, Avastha, Argala, yogas). LifeMatters
  will need to extract or duplicate this repository set, not import an existing bundler.
- Shadbala, Bhava Bala, Ashtakavarga, and Amsabala are confirmed already computed, persisted, and
  repository-backed (`PlanetaryStrengthRepository`, `BhavaStrengthRepository`,
  `AshtakavargaRepository`, `AmsabalaRepository`/`AmsabalaSchemeRepository`) — no gap here.

## Git/worktree state (corrects stale prior notes)

Three active workstream worktrees exist, not one: `database` (`ikiastrro.wt\database`, 21 commits
ahead of master), `ui` (`ikiastrro.wt\ui`, 4 ahead), `cli` (`ikiastrro.wt\cli`, 22 ahead), plus the
root `master` checkout. An orphan local branch `codex/karaka-implementation` has no worktree
attached — noted, no action taken.

## Ownership resolution

`STANDARDS.md` §E.1 WORKSTREAM-04: a new feature is a new doc under an existing workstream,
linked from that workstream's `MASTER.md` — not a new branch/worktree. `workstream/ui`'s own doc
(`wkstream_UI_v2.md`) already proposes (not yet confirmed) Codex ownership of
`src/Ikiastrro.Web/Components/Charts/**`. **Decision: LifeMatters is scoped inside the existing
`workstream/ui` worktree** — no new branch/worktree. See `lifematters_plan.md`'s "Concurrency and
ownership" section, updated accordingly.
