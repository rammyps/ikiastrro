---
last_updated: 2026-09-10
reflects: chart-module rename to *_Detailed / *Chart naming; chart-catalog + db_view_catalog added
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
with the disjoint path scope declared in `docs/<ws>/MASTER.md`.

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

---

## Change log

- **2026-09-10** — created. Adopted §3 (chart-module naming / file set / versioning) and §4
  (UI table ⇄ view catalogue). First application: `SouthIndianGrid` → `SouthIndianGrid_Detailed`,
  `NatalTransitWheel` → `Natal_Transit_Comp_WheelChart` (+ page/repo/math/tests), `transit.md`
  → `spec_Natal_Transit_Comp_Wheel.md`, `south-indian-grid.md` →
  `spec_SouthIndianGrid_Detailed.md`; `docs/ui/chart_modules.md` folded into
  `docs/ui/components/chart-catalog.md`.
