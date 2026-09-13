---
last_updated: 2026-09-10
togaf: Preliminary — landscape scan
safe: Exploration enabler
---

# Research — competitor tools

What other Vedic-astrology tools do, and what ikiastrro borrowed. Reference clones and
screenshots live under `_research/` (git-ignored); captures under `reports/`.

## The field

| Tool | Kind | What it's good at | Relevance |
|---|---|---|---|
| **Jagannatha Hora (JHora)** | free desktop (Windows) | the de-facto completeness benchmark — panchanga, every varga, all dashas, Ashtakavarga, Shadbala, sphutas, special lagnas | the parity checklist (`docs/cli/gap-and-coverage.md`); the golden export for `verify-*` |
| **PyJHora** | Python port of JHora (AGPL) | reference for varga formulas, panchanga, Ashtakavarga | formulas transcribed with attribution (varga `chart_method=1`); not a code dependency |
| **jyotishganit** | Python (MIT), BPHS-based, test-covered | Bhinna/Sarva Ashtakavarga, all six Ṣaḍbalas, panchanga | port target for the strength + Ashtakavarga gap, with attribution |
| **VedAstro** | open-source .NET + web | broad API surface; its own chart SVGs | the v1 engine (`VedAstro.Library`) — dropped for four confirmed defects; UI is a reference only |
| **jyotish-dashboard** | web app (MIT) | closest structural analogue — chart-centric workspace | UI reference for layout |
| **AstroSage / Cosmic Insights / Prokerala** | consumer web/app | polished consumer UX; independent longitude cross-check | position cross-checks only; consumer framing is a non-goal |
| **almamesh** | web app (MIT) | North-Indian chart SVG geometry, predictive-engine plan | North-Indian chart geometry reference (style not built) |
| **Maitreya 8.2** | native desktop, C/C++ (GPL-2.0-or-later) | cross-platform charts, dashas, Ashtakavarga, Shadbala, configurable scripture-attributed yoga DSL | installed comparison tool; source pinned at `_research/Maitreya8` tag `v8.2`, commit `1d845bc` |

## Maitreya 8.2 technology assessment

- **Runtime/UI:** native C/C++ desktop application using wxWidgets 3.2.8 in the published binaries.
- **Astronomy:** bundled Swiss Ephemeris C sources.
- **Persistence and documents:** bundled SQLite/wxSQLite3, wxJSON configuration, and wxPdfDocument output.
- **Build:** ten Visual C++ projects in a Visual Studio 2022 solution; alternative autotools/MinGW paths exist. A source build requires an external wxWidgets development tree through `WXWIN`.
- **Rule model:** sixteen shipped JSON yoga files contain description, effect, source, group, higher-varga flag and an executable expression string. A native Yoga Editor loads and saves the same format.
- **Testing:** no general automated unit-test suite or CI workflow is present in the pinned checkout; `swetest.c` is an upstream Swiss Ephemeris utility, not application coverage.
- **License boundary:** GPL-2.0-or-later, including bundled Swiss Ephemeris under its GPL option. Keep it as a process/reference tool; do not copy or link its implementation into ikiastrro without accepting reciprocal GPL obligations.

The strongest reusable idea is the human-editable, source-labelled yoga definition format and editor. For ikiastrro, retain typed C# predicates and database provenance for production, but consider a validated declarative rule layer for simple sign/house predicates. Maitreya's free-form expression strings are flexible, yet weaker than ikiastrro's typed results, explicit `NOT_EVALUATED` state, missing-input codes, normalized source variants and automated tests.

For the Ramakrishnan comparison, the shipped corpus contains Kemadruma under Saravali. Exact-name inspection found no bundled Vipareeta Raja, Anivahuppu, natal Vidya, generic Arishta, Ruchaka or Kahala entries, so Maitreya cannot independently settle those disputes without custom Yoga Editor definitions.

## What ikiastrro took

- **Completeness target** from JHora — but presented as an *evidence model*, not a report.
- **Varga + panchanga formulas** from PyJHora / jyotishganit, reimplemented as original
  `AstroMath` with worked examples and `verify-*` assertions.
- **Chart-centric workspace** layout idea from jyotish-dashboard.
- **Swiss Ephemeris (Moshier)** as the precision source, same as JHora / Parashara's Light.

## What ikiastrro deliberately does not copy

Consumer "your day ahead" framing · auto-generated interpretation / prediction text · a
single blended score · closed calculation internals · North-Indian chart as the default style.
