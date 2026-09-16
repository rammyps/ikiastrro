---
last_updated: 2026-09-16
reflects: chart-module conventions, UI table contracts, shared typography and navigation shell,
  workstream boundary enforcement
---

# ikiastrro — project standards

Project-scoped companion to the workspace standard `D:\@ClaudeSpace\STANDARDS.md`. The
workspace rules still apply in full; this file only **refines** them for ikiastrro and records
conventions this repo already follows or has newly adopted (STANDARDS BASE-02: "documented,
scoped project exceptions refine workspace standards"). On any unresolved conflict, workspace
STANDARDS wins and the conflict is surfaced.

Lives at the repo root beside `README.md` / `CLAUDE.md` / `AGENTS.md`, the way
`STANDARDS.md` sits at the workspace root.

---

## 1. Doc layout as practised here

ikiastrro predates parts of STANDARDS §M.1 and keeps these names — do not rename to the
generic templates:

| Role | This repo | STANDARDS generic |
|---|---|---|
| Doc index | `MASTER.md` (root) | `master_<name>.md` |
| Feature + completion register | `masterproduct.md` | `PRODUCT.md` |
| Per-workstream docs | `docs/<workstream>/` trees (`ui`, `cli`, `database`, `architecture`, `governance`, `research`) | `docs/<category>-<slug>.md` |
| Non-prose artifacts | `docs/artifacts/<type>/` (`ui`, `db`, `diagrams`, `reference-charts`) | same |
| ADRs | `decisions/NNN-kebab-title.md` | same |

Every `.md` created or edited carries the §O freshness frontmatter (`last_updated:` required,
bumped in the same edit; `reflects:` when the doc tracks code). This file included.

## 2. Workstream model (refines §E.1 / §E.2)

Two value streams, **three long-lived workstream branches** — `workstream/database`,
`workstream/cli`, `workstream/ui` — each a `git worktree` under `D:\@ClaudeSpace\ikiastrro.wt\`
with the disjoint path scope declared in `docs/<ws>/MASTER.md`, consolidated here:

| Workstream | Owns |
|---|---|
| `workstream/database` | `db/`, `src/Ikiastrro.Data/` |
| `workstream/cli` | `src/Ikiastrro.Core/`, `src/Ikiastrro.Cli/`, `tests/Ikiastrro.Yoga.Tests/` |
| `workstream/ui` | `src/Ikiastrro.Web/`, `tests/Ikiastrro.Web.Tests/`, `tests/Ikiastrro.Web.E2E/` |

- **Claude Code is the primary** (AGENT-01) — owns root/cross-cutting files and integration to
  `master`.
- **Codex is a contributor** — assigned path scope `src/Ikiastrro.Web/Components/Charts/**`
  (the hand-rolled chart templates) plus the matching `Pages/`, `Data/`, test and
  `docs/ui/components/spec_*` files for a chart it owns. It does not touch shell, routing,
  tokens, or another workstream's paths.
- Every handoff recorded in-repo (AGENT-03 / §S): changed files, validation run + result,
  remaining work, next action.
- Concurrent edits to the same file: coordinate first (§R); split by file, re-read before
  editing, never two agents renaming in the same tree at once.

### 2.1 No direct commits to an owned path, no parallel rebuilds (added 2026-09-16)

Closes the gap that let `master` and `workstream/cli` independently build the same
Ashtakavarga/Jaimini/Shadbala engines — see the change log entry below.

- **WORKSTREAM-01/02 has no exceptions.** If a change touches a path owned by a workstream
  (table above), it is committed on that workstream's branch — full stop. This includes small
  fixes, "just this once," and clearing out a backlog of unrelated pending edits directly on
  `master`. Before committing to `master`, diff the changed paths against the ownership table;
  if anything falls under a workstream's ownership, move the change to that branch/worktree
  instead of committing it where it sits. This applies in both directions — a workstream branch
  reaching into another workstream's owned path (e.g. `cli` editing something under
  `src/Ikiastrro.Data/`) is the same violation as `master` reaching into `cli`'s or `ui`'s.
- **Check for a parallel build before starting a new engine, calculator, repository, or
  table.** Grep the *other* workstreams' `docs/<ws>/MASTER.md` "Current state" sections and
  `masterproduct.md`'s feature register for the feature name first. Two independently-built
  implementations of the same feature is the specific failure this rule exists to prevent.
- **Keep divergence short.** At the start of a session, or before adding new work to a
  workstream branch, run `git log master..workstream/<X> --oneline` and
  `git log workstream/<X>..master -- <X's owned paths>` for each workstream. Either command
  returning commits means unmerged or boundary-crossing work exists — integrate it before
  piling on more, rather than letting a branch sit divergent for many commits (this project has
  no PR gate to force WORKSTREAM-03's rebase-before-merge, so this check is the substitute).

## 3. Chart modules — naming, files, versioning

Applies to every chart diagram in the UI. All are **hand-rolled Razor components** (inline
SVG or CSS grid); never a static `.svg` asset, never a charting library.

### 3.1 Name

`<BaseName>` in `PascalCase`, optionally `<BaseName>_<Qualifier>` where the qualifier states
the **rendering technique or scope, accurately**:

| Qualifier | Means | Example |
|---|---|---|
| `_DetailedSVG` | a full inline-`<svg>` chart template | *(reserved — e.g. a future SVG south-indian renderer)* |
| `_Detailed` | an enriched **CSS-grid** chart (badges, dignity dots, aspect strip) | `SouthIndianGrid_Detailed` |
| `_Mini` / none | a stripped thumbnail | `MiniGrid` |
| `…Chart` suffix | the **template component**, when a page shares the base name | `Natal_Transit_Comp_WheelChart` (component) vs `Natal_Transit_Comp_Wheel` (page) |

Do not use `_DetailedSVG` for a CSS-grid chart or vice-versa — the suffix is a contract about
how it draws.

### 3.2 The file set a chart module owns

All share the base name:

| File | Path |
|---|---|
| Template component | `src/Ikiastrro.Web/Components/Charts/<Name>.razor` (+ `.razor.css` isolation file) |
| Page (if it has its own route) | `src/Ikiastrro.Web/Components/Pages/<PageName>.razor` (+ `.razor.css`) |
| Read repository | `src/Ikiastrro.Data/<Name>Repository.cs` |
| View-model / math helper | `src/Ikiastrro.Web/<Name>Math.cs` |
| Unit tests | `tests/Ikiastrro.Web.Tests/<Name>MathTests.cs` |
| Golden snapshot | `docs/artifacts/ui/<Name>-sample.svg` + a `[Fact] <Name>()` in `ChartSnapshotTests` |
| Living specification | `docs/ui/components/spec_<Name>.md` (§O frontmatter; `component:` + `route:` keys) |
| Catalogue row | `docs/ui/components/chart-catalog.md` — `chart name / spec doc / linked files` |

Adding a chart module = add the spec doc **and** the catalogue row **and** a golden in the
same change (mirrors §M.1 "add a row the same change that adds a doc").

### 3.3 Versioning (refines §E.1 WORKSTREAM-06 + the design-language additive rule)

A new visual treatment of an existing chart is a **new module with a new suffixed name**, its
own `spec_<NewName>.md`, and its own golden — **never a rewrite in place**. The old module
stays until every consumer has migrated; it is deleted in the same PR that removes its last
consumer. Rationale: a hand-rolled chart's only revert arbiter is its golden SVG; rewriting in
place destroys the comparison and the documented contract.

Supporting rules, unchanged:

- **Design tokens are additive** — never repurpose a `--wheel-*` / `--cell-*` / `--tmpl-*`
  token; a new look is a new token or a dated value change.
- **Geometry helpers version by addition** — `AngleToXy` → `AngleToXyV2` / a new parameter,
  never a changed return for the same signature.

### 3.4 Renaming an existing module

`git mv` the `.razor` + `.razor.css` together (Blazor scoped-CSS matches on filename), then
update in one pass: every `<Tag>` consumer, `Render<T>` / `nameof(T)` in tests, the golden
filename, the spec doc (`git mv` to `spec_<NewName>.md` + its `component:` key), the catalogue
row, and prose mentions in `docs/ui/`. Build + `dotnet test` before handing back.

## 4. UI tables ↔ DB views (refines domain-contracts)

Any component that renders rows reads a **persisted** SQL view / TVF / table and never
recomputes (`docs/architecture/domain-contracts.md`).

- Each table component ⇄ its backing view/TVF is one row in
  [`docs/database/db_view_catalog.md`](docs/database/db_view_catalog.md): view name, columns
  surfaced, the UI component(s) + page(s) that read it, the repository, and the migration that
  defines the view.
- Add or update that row in the **same change** that adds a table component or repoints one at
  a different view.
- The view's own definition and DDL stay in the `database` workstream (`db/NN_*.sql`,
  `docs/database/schema.md`); `db_view_catalog.md` is the consumer-side index and links to
  both.

## 5. Shared UI typography and navigation

- **Manrope is the sole interface family**, supplied by the bundled 400/500/600/700/800 files.
  Components inherit `--font-interface`; chart labels and tabular values may change weight,
  spacing or use `font-variant-numeric: tabular-nums`, but must not switch families.
- Request only bundled weights. Use 700 instead of 750 and 800 instead of 850/900 so the
  browser never synthesizes a weight differently across platforms.
- The shared `MainLayout` header is the only top-level navigation implementation. It spans the
  viewport, keeps person navigation at left, centres the compact brand line, and anchors the
  two-line **SAVED / CHARTS** pill at the extreme right on every route.

---

## Change log

- **2026-09-16** — added §2.1 after finding `master` and `workstream/cli` had independently
  built conflicting implementations of the same features. Root cause: commit `590dc72`
  ("Add Bhaava/Ghati/Sree Lagna engines; commit accumulated pending work to master") put
  `cli`-owned-path work (`src/Ikiastrro.Core/Engines/Karakas/`, `.../Strength/`) directly on
  `master` instead of `workstream/cli`; separately, `workstream/cli` had itself added
  `Insert`/`Delete*` methods to `src/Ikiastrro.Data/AshtakavargaRepository.cs`, a
  `database`-owned path. Neither crossing was caught until the two branches were merged 8
  commits later, producing add/add conflicts on `AshtakavargaRepository.cs`,
  `BhaavaLagnaCalculator.cs`, `GhatiLagnaCalculator.cs`, and `SreeLagnaCalculator.cs`, plus
  content conflicts in `ShadbalaCalculator.cs`, `ChartPipeline.cs`, `ChartBundle.cs`,
  `ChartGenerationService.cs`, and `src/Ikiastrro.Cli/Program.cs`'s `verify-*` command list.
  Resolved by hand-merging both sides (commit `b6603ba`); §2.1 adds the ownership table, the
  no-exceptions rule, the pre-build duplication check, and the divergence-check cadence so the
  next parallel-build isn't caught this late.
- **2026-09-14** — full-app standardization pass (rammyps's directive): Key Inference's nested
  tabs (1.1/1.2, 2.1/2.2, 3.1/3.2) now ALL CAPS and restyled onto the master step rail's pill
  look (one tab convention app-wide, not two — see `docs/ui/design-language.md` "Tabs");
  every data-table component (`AspectsTable`, `HouseConjunctionsTable`, `HouseLordshipTable`,
  `HouseLordFindingsTable`, `PlanetDignityTable`, `PlanetPositionsTable`, `ConjunctionsTable`,
  `SadeSatiTable`, `GocharaPanel`, `DataTable`/`EvidenceTable`, the Saved Charts table) now
  shares Key Inference "2. ABOUT"'s table typography — `font: 500 var(--font-size-control) /
  1.3 Manrope; font-size: 0.86em`, `th` `font-weight: 700` + 2px `--brand-line` bottom border,
  `td` 1px border, `--brand-midnight` text throughout, no forced `nowrap` outside tabular-numeral
  cells. Retired the remaining `--paper-*` / `--ink-*` / `--accent` / `--muted-text` legacy
  tokens still read by several of those components (plus SavedCharts' edit modal), onto
  `--brand-*`. Found and fixed two dead-CSS bugs while checking the header nav against this:
  (1) `MainLayout.razor.css`'s three-zone header grid (`::deep .ik-appbar .mud-toolbar`) never
  actually applied — `Class="ik-appbar"` on `<MudAppBar>` doesn't get this file's CSS-isolation
  scope attribute (documented MudBlazor gotcha, previously only called out for MudTabs), so the
  brand line was never centred and Saved Charts' right pin was accidental flex behaviour, not
  the intended grid; fixed by wrapping `<MudAppBar>` in a literal `.ik-appbar-scope` div, same
  pattern as `KeyInference.razor`'s `.ki-wheel`/`.ki-tabs` wrappers. (2) `KeyInference.razor.css`
  hid MudBlazor's tab-underline slider via `.mud-tabs-toolbar-wrapper`, but this MudBlazor
  version renders `.mud-tabs-tabbar-wrapper` — fixed for the slider rule; the sibling
  grid-stretch rule for the master rail turned out to fight MudTabs' own overflow/scroll-arrow
  detection when corrected the same way, so that one was deliberately left on the dead
  `-toolbar-*` name (comment explains why) rather than ship a regression.
- **2026-09-14** — added §5 after the app-wide font and shared-header audit; standardized
  Manrope inheritance/available weights and recorded the viewport-wide three-zone header.
- **2026-09-10** — created. Adopted §3 (chart-module naming / file set / versioning) and §4
  (UI table ⇄ view catalogue). First application: `SouthIndianGrid` → `SouthIndianGrid_Detailed`,
  `NatalTransitWheel` → `Natal_Transit_Comp_WheelChart` (+ page/repo/math/tests), `transit.md`
  → `spec_Natal_Transit_Comp_Wheel.md`, `south-indian-grid.md` →
  `spec_SouthIndianGrid_Detailed.md`; `docs/ui/chart_modules.md` folded into
  `docs/ui/components/chart-catalog.md`.
