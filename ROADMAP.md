---
last_updated: 2026-09-23
---

# ikiastrro — Roadmap

**Owner:** rammyps

The **flow** view: theme-based **Now / Next / Later**, pure flow — no sprints, no named
increments, no fixed dates. Work moves continuously; something ships when it's ready.
Feature-by-feature state is [`masterproduct.md`](masterproduct.md); the private
prioritisation working doc is `../methods_prodmag.md`. State vs flow: `STANDARDS.md` §E.1.

## How priority is decided

- **Opportunities** are filed as [Feature / opportunity](.github/ISSUE_TEMPLATE/01-feature-opportunity.yml) issues.
- Scored with **ICE** — Impact / Confidence / Ease, each 1–10, `Score = average`. Not RICE:
  at this user count "Reach" is a constant that only adds noise.
- Slotted into Now / Next / Later at triage; re-scored after each ship, not on a calendar.
- Each Now / Next item is a GitHub **Milestone** (an epic) and a `FEAT-<AREA>-<NN>` row in
  `masterproduct.md`, worked as issues on the flow board and closed along the ladder
  `Planned → Designed → DB → Core → Verified → Web → Done`.

## Velocity

Derived, never hand-tracked: **rolling 4-week merged-PR average**, plus feature-boxes
closed per week (diff the `masterproduct.md` rollup over git). Read on demand:

```
gh pr list --state merged --search "merged:>=$(date -d '-28 days' +%F)" --json number | jq length
```

## Now

Close the gap between verified engine logic and what the web app actually shows — the
"Missing Web" column in the `masterproduct.md` rollup.

- **Divisional charts in the UI** — render D2–D60 (21 varga types), not just D1/D9, plus a
  per-scheme dignity-tier stacked-bar summary · `FEAT-VARGA-01`/`FEAT-VARGA-02` (new
  2026-09-22). Depends on `FEAT-HOUSE-05`'s bridge for interpretation content. **Open, needs
  rammyps** (design: `lifearea-varga-charakaraka-synthesis.md`): stacked-bar dedup policy,
  one-bar-per-scheme vs. one combined bar, segment ordering.
- **Jaimini chara karakas panel** — surface the 8-fold Aṣṭa already computed, the "Life
  Matters" panel from `res_charakarakas.md` §2 · `FEAT-KARAKA-01`/`FEAT-KARAKA-06` (new
  2026-09-22). Depends on `FEAT-HOUSE-05`.
- **Planetary-state (avastha) display** — `AgeState`, `WakefulnessState` · `FEAT-AVASTHA-01/02`
- **Slow-planet transit history view** — 1930–2060 sign-transit timeline · `FEAT-TRANSIT-01`
- ~~**Ashtakavarga cross-varga extension + comparison chart**~~ **Extension half done, closed
  2026-09-23** · `FEAT-ASHTAKAVARGA-02` (new 2026-09-22) — this bullet's premise was stale in
  the opposite direction from what it assumed: `ChartGenerationService.PersistAnalytics`
  already independently recomputes Ashtakavarga per chart type (not a D1 cross-reference
  lookup), and `AshtakavargaChart.razor` already has a live "Varga" selector over it. Also
  wasn't actually uncited: P.V.R.'s *Integrated Approach* Example 39 (p.155) states directly
  that this is the correct method, worked through PM A.B. Vajpayee's D-10 career case study.
  **Decided 2026-09-23 (rammyps): keep the shipped behavior, now cited.**
  `verify-ashtakavarga` Phase 6 guards the per-varga independence. Design:
  `docs/research/domain/ashtakavarga-varga-extension.md`. Still open (Web-only, deferred):
  `AshtakavargaVargaCompareChart` stacked bar — its 3 sub-questions decided 2026-09-23: dedupe
  varga-code lists to unique codes, one stacked bar per scheme tab, fixed Exalted→Great Enemy
  segment order. Moving to `masterproduct.md`.
- ~~**Nakshatra Lord → Sub-Lord chain (L1–L7) surfacing + Rāśi/Nakṣatra combination**~~ **Done,
  closed 2026-09-22** · `FEAT-NAKSHATRA-02` — `PlanetPositionsTable` (d1 variant) shows a
  live-computed "Sub-Lord Chain (L2–L7)" column; the Web-generation gap
  (`tbl_Fact_KpSubLordChain` never populated for Web-created charts) is closed
  (`Ikiastrro.Web/Program.cs` registers `KpSubLordChainRepository`);
  `tvf_Chart_DashaLordRelationship`'s join extended to L2–L7 (migration 131). New Key Inference
  tab "1.1 ABOUT SIGNS & NAKSHATRAS" (`RasiNakshatraTable`) surfaces `tbl_Rule_RasiNakshatraCombination`
  (migration 124, 36 rows) — one row per graha, expand-to-reveal the narrative fields. Both
  slices browser-verified. Design: `docs/research/domain/nakshatra-lord-sublord-dasha-crossref.md`.
  Moving to `masterproduct.md` — this bucket only tracks what's still in flight.
  **Scope decision 2026-09-22 (rammyps):** the Nakshatra *Pada* Lord chain
  (`vw_Rule_NakshatraPadaLordConnection` / `tbl_Rule_NakshatraPadaCombination`, migrations
  125–126) is explicitly **out of scope** — not a standard/classical technique, "not usually
  done." Those two objects stay committed (harmless, unconsumed) but get no Web/CLI work under
  this feature or any other planned one.
- **`tbl_Fact_KpSubLordChain.RuleSetId`** · `FEAT-DATA-07` (new 2026-09-22, **implemented +
  verified 2026-09-22**) — the one real schema gap found in the KP crossref note: every sibling
  `tbl_Fact_*` records `RuleSetId`; this table (migration 095) didn't. Migration 130 applied
  (backfilled the 162 existing dev rows to `RuleSetId=1`) and
  `KpSubLordChainRepository`/`ChartGenerationService` now pass it through — confirmed via a
  fresh `backfill-analytics` run.

## Next

Scoped, not started. Ordering set at the next ICE pass.

- ~~**Bhāva significations + Sthira Kāraka mapping**~~ **Done, closed 2026-09-23** ·
  `FEAT-HOUSE-03`/`FEAT-KARAKA-03` — this bullet was stale on two counts: house significations
  (`tbl_Rule_HouseSignification`, 12/12 houses) already shipped via migration 31, and the
  "needs a cited edition" note for Sthira Kāraka was resolved by re-reading B.V. Raman's *How
  to Judge a Horoscope*, which confirms 6 of the 12 house roles via 20+ case studies (the other
  6 have no source — left unassigned). Migration 133 seeds the `STHIRA` slot
  `tbl_Dim_KarakaRole` reserved since migration 103; `verify-sthira-karaka` cross-checks
  agreement with the independently-sourced PVR Naisargika primary table. Moving to
  `masterproduct.md`.
- ~~**Naisargika Kāraka — Sapta vs Aṣṭa**~~ **Not actually undecided, corrected 2026-09-23** ·
  `FEAT-KARAKA-04` — migrations 086/103 already give all 9 grahas (7 classical + Rahu + Ketu
  separately) their own role; confirmed as the intended scope, not a live decision. Moving to
  `masterproduct.md`.
- ~~**Dispositor chains / final dispositor / mutual reception**~~ **Done, closed 2026-09-23**
  · `FEAT-DISPOSITOR-01` — this bullet was stale: `DispositorEngine.cs` (chain-following,
  final-dispositor resolution, mutual-reception/cycle detection) and a live `DispositorTable.razor`
  had already shipped on `master`, just undocumented (the `workstream/cli`/`workstream/ui`
  worktrees were behind). Deliberately live-only, no fact table — same pattern as Argala/RasiDrishti
  until something needs to cross-reference it in SQL. Closed out with `verify-dispositor` CLI
  coverage (checks every saved person's D1 chart against independently-stamped `SignLordPlanet`
  and structural chain invariants). Moving to `masterproduct.md`.
- **Compound Maitrī, sambandha** · `FEAT-RELATIONSHIP-04` — argala/virodhargala split out and
  built 2026-09-19: rule layer (`ArgalaCalculator` + `tbl_Rule_Argala`, migration 127), fact
  layer (`tbl_Fact_Argala` migration 128 + `ArgalaFactBuilder`/`ArgalaFactRepository` +
  `backfill-argala` CLI mode), and a live Key Inference 2.1 "Argala & Virodhargala" table
  (`ArgalaTable`, computed off `ChartKeyDetail`) all done, 28/28 tests. **Bug found + fixed
  2026-09-22:** `tbl_Fact_Argala` had no delete-wiring at all, so once `backfill-argala` had run,
  every RECALCULATE and person-delete threw `FK_Fact_Argala_ChartResult` — fixed, matching
  `KpSubLordChainRepository`'s optional-dependency pattern in both `ChartGenerationService` and
  `BirthDetailDeletionService`. **Insert-side wired 2026-09-23:** `ArgalaFactBuilder`'s output is
  now written on every live `GenerateAll`/`GenerateMissing`/`RecomputeAnalytics` call, so
  `tbl_Fact_Argala` no longer needs a manual `backfill-argala` re-run after a rebuild. Still open:
  the career worked-example `CalculationNarrative`; sambandha not started. **Compound Maitrī —
  not a gap:** `tbl_Rule_CompoundRelationship` (migration 24) is deliberately cited-but-not-read,
  same pattern as `ArgalaCalculator`/`RasiDrishtiCalculator` — `verify-rules` already proves it
  matches `DignityEngine.CombineToPanchadha`'s hardcoded truth table
- ~~**LifeArea ↔ DivisionalSubject ↔ CharaKaraka bridge**~~ **Mostly done, closed 2026-09-23**
  · `FEAT-HOUSE-05` (new 2026-09-22) — this bullet's own premise was stale: migration 109
  (2026-09-16) had already built `tbl_Dim_InterpretiveFactor` / `tbl_Rule_InterpretiveFactorDetail`
  and normalized all 11 `tbl_Dim_DivisionalSubject` rows into it, with a repository and CLI
  `verify-interpretive-factors` already in place — the design doc proposing this feature
  (written 2026-09-22) missed that 109 existed. What 109 had explicitly deferred: **Leg A/B
  reconciliation, decided 2026-09-23 (rammyps):** amend Leg A (`tbl_Dim_LifeArea`, 10 of 20
  rows) wording to match Leg B's narrower framing — done, migration 134, plus the missing
  LifeArea VARGA facts (21 rows) seeded into 109's table. Design:
  `docs/research/domain/lifearea-varga-charakaraka-synthesis.md`. Still open, not guessed:
  Chara-karaka-role (Leg D) detail rows — no source content exists to seed them from. Moving to
  `masterproduct.md`.

## Later

Acknowledged, deliberately deferred.

- **Strength engine** — Ṣaḍbala (6 components), Vimśopaka Bala, Bhāva Bala. Blocked on sourcing a
  cited reference edition · `FEAT-STRENGTH-01/02`
- **Yoga detection** — Pañcha Mahāpuruṣa + Rāja/Dhana first slice. Needs its own design pass to turn
  case-study prose into enumerable rule rows · `FEAT-YOGA-01`
- **Remaining avasthas** — `RadianceState`, `ShameState`; each needs a cited edition and
  janma-ghaṭi inputs · `FEAT-AVASTHA-03/04`. `PostureState` (`FEAT-AVASTHA-05`) shipped
  2026-09-22 correction — migration 083, `PostureStateCalculator.cs`, cited
  `SRC_PVR_INTEGRATED` §15.4.4; `masterproduct.md` was stale, moved off this line
- **Selectable house system** — beyond whole-sign
- **KP system** — sub-lords, significators as a layered sub-system
- **North-Indian & West-Indian varga chart styles** — renderers beside the default South-Indian
  grid, picked from Preferences (`FEAT-UI-12`). Only South-Indian renders today
- **Tamil localisation** — UI strings + astrology terms; Tamil script or transliteration TBD.
  A Language selector in Preferences. Broad i18n scope — see `docs/ui/MASTER.md` NFRs
- **Runtime-reorderable tabs** — drag-to-reposition the header tabs / Key-Inference sub-tabs,
  order remembered per user. Deferred NFR — effort + rationale in `docs/ui/MASTER.md`
- ~~**Sign/house-level benefic-malefic synthesis**~~ **Done, closed 2026-09-23** ·
  `FEAT-HOUSE-06` — B.V. Raman's *How to Judge a Horoscope* "Considerations in Judging a
  House" checklist (p.14-15, `SRC_RAMAN_HTJH`) — same passage `LagnaFunctionalNature` already
  cites. Built `HouseBeneficMaleficCalculator` (`workstream/cli`): sign-lord functional nature
  (dominant) + occupants + discrete graha dṛṣṭi (new `RelationshipEngine.AspectsSign`) + lord
  dignity/combustion as a separate, non-voting modifier. Live-only, no new table — same pattern
  as `LagnaFunctionalNature` itself. `verify-house-benefic-malefic` ALL PASS across every D1
  chart on file. See `masterproduct.md` HOUSES.
- **Data & Calculation Integrity surfacing** (new 2026-09-22) — birth-time sensitivity
  (±1/±2/±5/±10 min placement drift), rectification-status flag, ayanāṁśa/settings
  declaration shown per-reading. Genuinely new ground, no existing engine gap to close; no
  urgency signal yet. From Stage 01 of `docs/research/domain/vedic_reading_layers.md`.
- ~~**Nakshatra color scheme**~~ **Decided 2026-09-23 (rammyps):** ruling-planet / 9-color
  basis — colored by each nakshatra's Vimshottari lord, reusing the app's existing planet-color
  tokens. Treated as display/cosmetic, same call already made for `TamilName`; no classical
  citation pursued. Stays a Web-only task (derivable purely from the already-stored
  `NakshatraLordPlanet`, no new DB work) — not built this session, out of DB/CLI scope.
- **Reading-layers "suggested additions"** — 5 ideas flagged unconfirmed by their own author,
  not triaged: rule-lifecycle log, cross-chart/synastry module, base-rate comparison in
  Validation, surfacing confidence/source-stratum to the end reader, the reverse muhūrta
  query. Parking lot only — see `vedic_reading_layers.md`'s "Suggested additions" section.
  **Open, needs rammyps** before any of these become real `FEAT-*` candidates.

## Cadence

One dated line per ship event (a `git tag` + GitHub Release). Capped at the last ~6 —
`git log --first-parent origin/master` and the Releases page hold the rest. This is the
only time-phased block in the repo's prose (`STANDARDS.md` §E.1 WORKSTREAM-05).

- 2026-09-06 — Roadmap set to pure-flow Now/Next/Later; "Now" = UI-surfacing of verified
  engine features.
