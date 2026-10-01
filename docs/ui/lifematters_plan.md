---
last_updated: 2026-10-01
status: approved
phase: 1A/1B/2 (in progress; 0B specs retrofitted — see note below)
---

# LifeMatters page & SIND-HOV-GRID — roadmap

Revision 10 (2026-09-29): the page is restructured as Area → lagna-perspective Question →
Sub-questions → D1 analysis, read through D1 strength statistics — decisions 12–15; built and
specified in [`specs_life_matters_page.md`](components/specs_life_matters_page.md). Revision 9: this plan incorporates the structural evidence-assembly review, its schema
corrections, the LifeMatter Interpretation Contract, and the subsequent Subject/Focus,
canonical-code, Contribution, Bhava Bala, query, drill-through, and safety corrections.

Phase 0A is complete — see [`lifematters_phase0a_audit.md`](lifematters_phase0a_audit.md) for
the repository/schema audit, migration-087 reference-audit result, coverage counts, and taxonomy
citation status.

Claude's bounded research output for Phase 0B (A1–A12 citations, the UL/A12-vs-A7 correction,
proposed Subject/Focus mappings, plain-language copy, and the Key Inference read-path mapping) is
in [`lifematters_claude_research.md`](lifematters_claude_research.md) — proposals for human audit,
not implementation. Codex/ChatGPT owns the SQL/C#/Razor implementation this feeds into.

**Out-of-order execution, noted for the record**: Codex built Phase 1A (repositories), Phase 1B
(`LifeMatterFocusResolver`), and part of Phase 2 (`SindHovGrid`) before the Phase 0B specs
existed. The two component specs — [`specs_sind_hov_grid.md`](components/specs_sind_hov_grid.md)
and [`specs_life_matters_page.md`](components/specs_life_matters_page.md) — were written
afterward, documenting what was actually built (and flagging gaps against this plan, e.g. no
Escape-key test, no golden SVG yet, the Arudha evidence-repository gap) rather than a clean
before-implementation contract. Treat them as the live contract going forward.

## Goal and scope

LifeMatters is a new evidence-assembly page. Selecting a Category and Step loads the relevant
Varga into a South-Indian grid, highlights the Step's relevant house or point, and shows compact
Argala, Avastha, Shadbala, Arudha, house/planet condition, relationship, yoga, and D1-to-Varga
evidence. v1 assembles evidence; it does not automate astrologer judgment or timing.

The principal implementation risk is the data contract between stable LifeMatter identity,
Subject/Varga selection, focus resolution, and fixed-sign chart highlighting. Most astronomical
facts are already computed and persisted.

## Existing capabilities and genuine gaps

Existing capabilities include sign attributes; nakshatra/pada/sub-lord data; per-planet
nakshatra data; AL, A2–A12, Graha Arudhas, HL, GL, BL, SL; all 11 upagrahas; Argala; all five
avastha schemes; Shadbala, Bhava Bala, Ashtakavarga, and Amsabala; the 96-row
`tbl_Rule_LifeMatterReference`; `tbl_Dim_LifeArea`; `tbl_Dim_DivisionalSubject`;
`tbl_Rule_InterpretiveFactorDetail`; the 128-row `tbl_Dim_LifeMatter` plus FK added by migration
103; and `tbl_Rule_KarakaMatter`.

Migration 103 establishes `LifeMatterId` as the stable identity. `tbl_Rule_KarakaMatter`
already covers the Planet/Chara-karaka half for all 96 reference matters, but its `HouseNumber`
is populated only for the separate 32-row Karakatwa-grid batch. The remaining blockers are a
step-level Subject/Varga mapping, structured House/SpecialPoint focus, normalized A1–A12 naming,
house-from-reference resolution, and house-to-sign translation for the displayed Varga.

## Migration and schema policy

Never edit applied migration `db/087_seed_life_matter_reference.sql`. It incorrectly describes
Upapada Lagna as the arudha of the 7th; UL is A12, the arudha of the 12th, while Darapada is A7.
A new corrective migration must first audit whether any Fact/result references the affected
RuleSet. If unreferenced, correct the transcription in place through the new migration. If
referenced, create a new RuleSet version and corrected row set; do not mutate history.

The resolver combines three independent sources:

```
Subject/Varga      <- tbl_Rule_LifeMatterSubject (new)
Planet/Karaka      <- tbl_Rule_KarakaMatter      (existing)
House/SpecialPoint <- tbl_Rule_LifeMatterFocus   (new)
```

`tbl_Rule_LifeMatterSubject` contains `Id`, `RuleSetId`, `LifeMatterId`,
`DivisionalSubjectId`, `SourceRefCode`, and `IsActive`. v1 permits at most one active mapping
per `(RuleSetId, LifeMatterId)`.

`tbl_Rule_LifeMatterFocus` contains `Id`, `RuleSetId`, `LifeMatterId`, `FocusKind`,
`ReferencePoint`, `HouseNumber`, `SpecialPointCode`, `Priority`, `SourceRefCode`, and `IsActive`.
`FocusKind` is only `House` or `SpecialPoint`; Planet belongs to `tbl_Rule_KarakaMatter`.
House rows require an existing-vocabulary `ReferencePoint` and `HouseNumber` 1–12, with no
special-point code. Special-point rows require a canonical code and no reference point or house.
Use CHECK constraints for these shapes and an appropriate uniqueness constraint over RuleSet,
matter, kind, and payload. A matter may have zero-to-many focus rows.

Before inventing a reference-point vocabulary, Phase 0A must inspect `tbl_Dim_HouseReference`
and the contract cited by `specs_KI_spllagna.md`. Stored special-point codes are canonical only:
store `A12`, never `UL`; aliases are display concerns.

No row in either `tbl_Rule_KarakaMatter` or `tbl_Rule_LifeMatterFocus` means “focus not yet
structured.” Missing Subject is different: it merely prevents an automatic chart switch.
Free-text parsing of `HouseFromLagnaText` is a one-time offline proposal generator; every
candidate row is human-audited before a migration is committed.

## Terminology and Varga behavior

The hierarchy is Category -> Step. Subject is a separate cross-cutting lookup.

- Category: 10 values from `LifeMatterReference.Category`.
- Step: 96 values, one per reference `MatterText` and keyed by `LifeMatterId`.
- Subject: 11 values from `tbl_Dim_DivisionalSubject`, resolved per Step solely to select the
  confirmation Varga.

Varga control has three explicit modes:

- Auto follows the selected Step's Subject/Varga; if unmapped, retain the last chart.
- D1 pins the foundation chart.
- Manual exposes the existing chart-type selector and remains Manual across Step changes.

An uncomputed Varga uses the chart's existing empty state.

South-Indian cells are signs, not houses. A House focus stores the relative `HouseNumber`, never
a sign. On every chart change, resolve the focused sign from that chart's Ascendant:
`signAt((ascendantSignIndex + houseNumber - 1) % 12)`.

The inspector must distinguish the sign's overlapping nakshatra spans, each occupant's own
nakshatra/pada, and a focused special point's own nakshatra. Never label these collectively as
“the house's nakshatra.”

Special lagnas and upagrahas are computed from D1 sidereal longitude and projected into every
Varga; label that in a non-D1 chart. Arudha padas (bhava and graha) are computed inside each
Varga from its own placements (PVR §9.2 / §9.5; since 2026-10-01), so a Varga's A12 is that
Varga's own pada.

## Simplified evidence projections

- Argala is house-focused.
- Avastha is planet-focused.
- Strength in v1 means Shadbala only and is planet-focused.
- Arudha is house/special-point-focused.

Each card preserves its natural ordering, unions matches across multiple foci, and displays an
explicit no-rows message. Sarva Ashtakavarga and Bhava Bala were brought forward into v1 on
2026-09-29 (decision 13); Amsabala remains Phase 4 work.

The dossier also includes compact projections for house condition (lord, lord placement,
occupants, aspects, conjunctions), planet condition (dignity, functional nature, owned houses,
combustion/retrograde, avastha, Shadbala), relationships, relevant yogas, direct D1/Varga
comparison, contradictions/missing evidence, and provenance. House condition does not summarize
Bhava Bala in v1; it links to Key Inference for that detail.

LifeMatters owns compact projections only. Canonical detail stays in Key Inference, with an
addressable drill-through preserving chart, owning tab/step, and focused entity, e.g.
`/key-inference/{id}?step=about-houses&chart=D9&house=7`.

## Interaction, accessibility, responsiveness, and failures

Hover and keyboard focus preview without disturbing the pinned selection. Mouse-leave restores
the pin. Escape clears preview first, then the pin. Clicking the selected Step keeps it selected;
the meaningful fallback is D1/Rasi, never a blank state. Touch tap selects.

Every chart cell and Step row is keyboard-operable with Enter/Space, has visible focus, and has
appropriate ARIA labeling. Selection uses a non-color indicator such as a border or icon.
Respect `prefers-reduced-motion`. Desktop cards have stable dimensions; the page specification
must define a sub-desktop stacked/scrollable breakpoint.

Explicit states are required for unknown chart ID, missing workspace/birth data, uncomputed
Varga, repository failure, unstructured focus, empty evidence cards, and rapid selection races.
Async work is last-selection-wins; stale responses are discarded.

## Arudha Pada naming taxonomy

Add normalized dimensions after confirming naming conventions and citation infrastructure:

```
tbl_Dim_ArudhaPadaNames
    Id
    HouseNumber
    PrimaryName
    SourceRefCode
    CitationStatus
    UNIQUE (HouseNumber)

tbl_Dim_ArudhaPadaAliases
    Id
    HouseNumber
    Alias
    AliasType
    SourceRefCode
    UNIQUE (HouseNumber, Alias)
```

A1 displays as Arudha Lagna. Upapada Lagna, Upapada, Gaunapada, Vyayarudha, and Moksha pada are
A12 names/aliases, not a second calculated point. Seed the A1–A12 taxonomy as
`Uncited-flagged` pending a book/page source.

## LifeMatter Interpretation Contract

The reading seam is natal promise -> LifeMatter application -> timing activation ->
synthesis/confidence. v1 implements evidence assembly only and must never claim to be a complete
interpretation layer.

Every displayed fact carries `Contribution = Supportive | Obstructive | Mixed | Contextual |
NotEvaluated | NotApplicable`. Populate a directional value only from an existing typed, sourced
interpretation. Otherwise default to `NotEvaluated` and display “Relevant evidence;
interpretive contribution not evaluated.” Never infer contribution from dignity, Shadbala,
benefic/malefic status, or row presence.

Keep Capacity, Quality, Delivery, and Activation distinct. A strong planet is not necessarily a
favorable one. Natal-promise judgment is astrologer-recorded, not auto-scored, with possible
conclusions `Strongly promised`, `Conditionally promised`, `Mixed`, `Weakly indicated`,
`Substantially denied`, and `Indeterminate`.

Evidence must carry an `EvidenceFamilyCode`/provenance key so derived restatements, modifiers,
independent confirmations, and contradictions are not double-counted. Confidence is qualitative,
never a sum of rows.

D1/Varga comparison is explicit:
`D1 promise | Relevant Varga evidence | Agreement: Confirms|Modifies|Contradicts|Insufficient |
Reason`. Maintain a visible contradiction ledger later rather than flattening disagreements.

Timing activation is reserved for Phase 4, including Dasha/transit activation and statuses
`Dormant | BroadlyActive | SpecificallyTriggered`. Birth-time provenance, rectification,
calculation settings, sensitivity, and “Indeterminate because of birth-time uncertainty” are
also reserved first-class concerns. User context is stored separately and never silently used as
a rule input.

Every interpretive claim preserves source code/locator, RuleSet/version, calculation method,
claim type (classical, practitioner-derived, project synthesis, experimental, or
astrologer-authored), and competing interpretations. UI copy must distinguish computed facts,
sourced traditional interpretation, project synthesis, astrologer judgment, and unresolved or
unavailable evidence. Deterministic and diagnostic language is prohibited, especially for
health, marriage, family, and career.

## v1 read model

Do not freeze a speculative judgment/timing model. v1 defines:

```
LifeMatterEvidenceItem
    LifeMatterId
    FocusId
    EvidenceType
    EvidenceRecordId
    EvidenceFamilyCode
    Applicability
    Contribution
    SourceRefCode
    RuleSetId
    CalculationVersion
    Explanation

EvidenceLocator
    EvidenceType
    ChartResultId
    SubjectCode
    EntityCode
    RuleCode
    SourceRecordId?
```

`EvidenceLocator` supports computed evidence without a persisted row ID and lets a future,
separate judgment model reference evidence without copying it.

The v1 flow is: select Category/Step; review D1 foundation; review planet/house condition;
review relationships; compare the relevant Varga; review Arudha/manifestation evidence; review
applicable yogas. Recording natal-promise judgment/contradictions, timing activation, and final
confidence/notes are reserved.

## Query and performance contract

Use one page-load snapshot or bounded aggregate query, never a repository call per row. Project
client-side from loaded evidence where practical. Use cancellation/version tokens for Step
changes. Phase 3C must record concrete query counts for initial load and Step switching.

## Specification ownership

- `docs/ui/lifematters_plan.md`: domain flow, data contract, phases, and decisions.
- [`docs/ui/components/specs_sind_hov_grid.md`](components/specs_sind_hov_grid.md): component
  API, rendering rules, interaction state.
- [`docs/ui/components/specs_life_matters_page.md`](components/specs_life_matters_page.md): page
  layout, orchestration, responsive rules, and simplified-column projections.

Both written 2026-09-25 (Phase 0B). As noted above, implementation ran ahead of them for
`SindHovGrid`/the resolver/repositories — `specs_sind_hov_grid.md` documents what was built and
its gaps; `specs_life_matters_page.md` is still a forward design (the page itself isn't built
yet), so it remains the before-implementation contract for Phase 3A onward.

## Phase sequence

- Phase 0A (complete): repository/schema audit; real coverage counts by LifeArea,
  DivisionalSubject, and KarakaRole; migration-087 application/reference audit; taxonomy citation
  status. See [`lifematters_phase0a_audit.md`](lifematters_phase0a_audit.md).
- Phase 0B (complete, retrofitted after 1A/1B/2 started — see note above): freeze terminology and
  write all three specifications, including the narrow v1 read model and explicit deferral of
  judgment/timing.
- Phase 1A (schema + resolver built; audited seed population still pending): repositories for
  LifeMatterReference, DivisionalSubject, and KarakaMatter — built. Subject/Focus tables
  (migration 137) — built, deliberately empty; audited seed population from
  `lifematters_claude_research.md`'s proposals is still pending.
- Phase 1B (built): pure, UI-independent `LifeMatterFocusResolver`, including unstructured-focus
  tests — built and tested (`LifeMatterFocusResolverTests.cs`).
- Phase 2 (complete): isolated fixture-tested SIND-HOV-GRID, including the state machine and
  house-to-sign translation — built and tested (`SindHovGridTests.cs`, including the Escape-key
  case), golden SVG minted (`docs/artifacts/ui/SindHovGrid-sample.svg`, pending a human visual
  look per the plan's "a golden validates rendering, not behavior" note). See
  [`specs_sind_hov_grid.md`](components/specs_sind_hov_grid.md).
- Phase 3A (complete): page shell, picker, Auto/D1/Manual lifecycle, and Steps panel — built at
  `Pages/LifeMatters.razor` (`/life-matters/{id}`), browser-verified against a real person (D1
  and D9, one mapped and one deliberately unmapped Step). No bUnit page tests yet (tracked for
  Phase 3C). Evidence cards are not built — that is 3B1-3B3, not this phase.
- Phase 3B1 (partial): Avastha/Shadbala/planet condition built as one reused
  `PlanetaryStateTable`, scoped to the Step's Karaka planets; house condition built (lord, lord
  placement, occupants — not yet aspects/conjunctions), always empty pending Focus seed data.
  Argala and Arudha are explicitly **blocked**, not built: neither has a read path over its facts
  today (Argala's gap found during this build, correcting `specs_life_matters_page.md`'s and
  `lifematters_claude_research.md`'s prior "ready" claim — see those docs).
- Phase 3B2 (complete, yogas narrowed): Relationships built via `DignityEngine.
  EvaluatePairRelationship` (no repository needed — a reused Core computation) between every pair
  of the Step's Karaka planets. Yogas built but **not filtered by matter relevance** — no yoga row
  carries structured planet/house involvement today (only free-text `YogaRule`/`Notes`), so v1
  lists every yoga present in the chart, labeled as unfiltered rather than faking a text-match
  filter. See [`specs_life_matters_page.md`](components/specs_life_matters_page.md).
- Phase 3B3: explicit D1/Varga comparison, projected-point labels, and provenance.
- Phase 3C: integration, visual, accessibility, viewport, regression, and measured query-count
  verification.
- Phase 4 (deferred): Bhava Bala, Ashtakavarga, Amsabala, richer Arudha prose, multi-select,
  remaining unified-model gaps, astrologer judgment, contradiction ledger, timing activation,
  and birth-time sensitivity.

## Testing and verification

Cover repository mappings; independent Subject/Planet/House/SpecialPoint joins and absence
combinations; resolver precedence; house-to-sign translation across two or more Vargas; UL-to-A12
canonicalization; Contribution defaulting; auto/manual selection behavior; hover/focus preview
and pinned selection; ARIA and keyboard operation; invalid routes; query counts; drill-through
URL construction; empty/loading/error/race states; and regressions to
`SouthIndianGrid_Detailed`, `SouthIndianGrid_Micro`, `PolarGridLagnaSelect`, and SPL LAGNAS.

Run `dotnet build` and `dotnet test`. Add and visually review a golden SVG, recognizing that a
golden validates rendering, not behavior. Manually verify at roughly 1366x768, 1920x1080, and at
least one sub-desktop breakpoint. Exercise the complete picker -> Varga switch -> Step selection
-> chart highlight -> table highlight chain for at least two LifeMatters plus one deliberately
unmapped Step.

## Concurrency and ownership

Follow `docs/ui/wkstream_UI_v2.md` and `STANDARDS.md` sections E.1/E.2. Claude remains primary
for the broader UI workstream and integration to `master`. LifeMatters is scoped inside the
existing `workstream/ui` worktree — per `STANDARDS.md` WORKSTREAM-04/AGENT-02, a new feature is a
new doc under an existing workstream, not a new branch/worktree (decided 2026-09-25; see
`lifematters_phase0a_audit.md`).

**Superseded 2026-09-25 (rammyps):** No Codex ownership for LifeMatters. Claude owns the full
stack for this feature going forward — SQL/C#/Razor implementation included, not just the
bounded research artifact described below. Phase 1A/1B/part-of-2 were already built by Codex
before this decision (kept as-is, reviewed rather than redone); everything from here — closing
the Phase 2 gaps, Phase 3A onward, and any corrective migrations — is Claude's directly, still
on `workstream/ui`, still reviewed by the user before anything reaches `master`.

Shared page files, shared documentation indexes, migration application, and final integration
remain single-writer resources; the user reviews diffs before integration.

Claude's bounded research (superseded above for implementation, kept as the record of what was
produced under the earlier split) covered: A1–A12 citations; UL/A12 versus A7; proposed Subject
and Focus mappings; Karaka/InterpretiveFactor coverage discrepancies; plain-language labels and
safe copy; spec consistency; visual/accessibility review; regression review; and mapping Key
Inference read paths to dossier ingredients — all in `lifematters_claude_research.md`.

## Approved decisions

1. A1–A12 seeds `Uncited-flagged` pending a source.
2. Correct migration 087 only after the RuleSet-reference audit; version when referenced.
3. v1 Strength is Shadbala only.
4. Varga control is Auto / D1 / Manual.
5. Subject, Planet/Karaka, and House/SpecialPoint are three independent resolver sources.
6. Canonical special-point codes only; UL displays over stored A12.
7. Contribution defaults to `NotEvaluated` without a sourced rule.
8. Claude remains UI primary; Codex receives only a declared, scoped feature worktree.
9. Compound/ambiguous `PrimaryChartsText` (e.g. `"D2/D6"`) resolves Subject to whichever half
   already has a matching `DivisionalSubject`, when exactly one does — never guessed when neither
   or both plausibly match. `MARRIAGE_SPOUSE_05` ("Sexual / bed pleasures," D9/D16) resolves to
   `MARRIAGE_RELATIONSHIPS`, staying inside its own category's theme rather than
   `VEHICLES_COMFORTS`. `TROUBLE_LOSS_09` ("Loss," cited only as `'Relevant varga'`, no literal
   chart code) resolves to `NO MATCH` for Subject; its Focus row still seeds independently.
10. The 32-row `PVR_KARAKATWA_GRID` batch seeds Focus only, never Subject. These are single-house
    natural correspondences with no Varga in PVR's own text — forcing a same-house thematic
    Subject match would manufacture precision the source doesn't have. Focus stays unambiguous:
    `House(LAGNA, HouseNumber)` for all 32.
11. A derived reference the Focus schema can't express directly (`CHILDREN_06`, `'9th from 5th'`)
    seeds its arithmetically-flattened result (`House(LAGNA, 1)`) with the derivation preserved in
    the row's own `SourceRefCode`/note, rather than extending the schema for one row.
12. **(2026-09-29) Four levels, lagna-first.** An area (category) is split into one question per
    lagna perspective — "How is the self from power?" is the self read from Ghati Lagna — and the
    area's matters become sub-questions re-read from that lagna. A matter's own seeded houses on
    a lagna are "sourced"; otherwise its Lagna houses are re-counted from the lagna. Supersedes
    the earlier Category → Step picker with separate lagna tracks.
13. **(2026-09-29) D1 is read through strength statistics.** Sarva Ashtakavarga (>30 / <25, the
    cited band), Bhava Bala (7 / 5 rupas), the lord's Ṣaḍbala (100% / 80% of minimum since 2026-10-01 — the shared `StrengthBands`; since 2026-10-01 grouped into Capacity / Consistency / Context axes with independent Bhava Bala, the lord's BAV and kāraka Ṣaḍbala/Amsabala — see `components/specs_life_matters_page.md`), Argala /
    Virodhargala (`tbl_Fact_Argala`, count comparison) and avastha drive signals, filters and a
    "Strongest first" sort. Bhava Bala and Ashtakavarga move from Phase 4 into v1. Decision 7
    still holds: signals describe strength (capacity) and never set `Contribution`, and the sort
    is never presented as a score.
14. **(2026-09-29) Yoga × LifeMatter 7×7 matrix removed from the page.** The user judged the
    mapping (`vw_YogaLifeMatter7x7`, migration 149) not meaningful; the page shows no yogas until
    it is reworked. The table and view stay in the database.
15. **(2026-09-29) Chart look.** `SindHovGrid` adopts `SouthIndianGrid_Detailed`'s visuals (dignity
    dots, `(R)`/`(D)`, combust, aspects, Sanskrit names) plus a gold house-from-track badge for
    the question's lagna and a SAV chip. Statistics follow the shown chart (decision 16).
16. **(2026-09-29) Drill-down in the matter's own varga; D1 as promise.** The user found "too many
    drop downs": area pills now open straight into matter tiles, and a matter opens in the main
    chart in its own varga (divisional subject's confirmation chart, then `PrimaryChartsText`).
    Chart and lagna are chips; the question column, sub-question table, filters, sorts and varga
    dropdown are gone. Statistics are computed per chart: varga SAV, live varga Argala
    (`ArgalaFactBuilder`, pure sign arithmetic on stored placements, no ephemeris call), the
    lord's Ṣaḍbala, and Bhava Bala in D1 only. They are shown beside D1 as promise vs.
    confirmation. The four signal swatches became one strength label (Strong … Weak, strong
    minus weak signals), still never an outcome. This supersedes the filters and "Strongest
    first" of decision 13 and the question column of decision 12. The page is full width, with
    no heading.

17. **(2026-10-01) Strength %, lagna questions × every chart.** The user found the summary did
    not include all the charts and asked for percentages instead of Strong … Weak. The strength
    label became a 0–100% index (mean of SAV/56, Bhava Bala/12, lord Ṣaḍbala/200%, Argala hold
    share). The lagna chips became matter-specific questions ("Enemies from power?") that show
    the houses read, sourced vs re-counted, and a %. The Statistics section gained a summary
    matrix of every lagna question × every chart the matter reads: D1, each named chart (not only
    the first), and D40/D45/D60 for every matter per BPHS ch. 6 (Khavedamsa: auspicious and
    inauspicious; Akshavedamsa and Shashtiamsa: all indications). `Relevant varga` (Loss) reads
    every chart. Chart chips offer all generated charts. No new seed rows; the worksheet's
    maternal/paternal D40/D45 claim stays rejected (unified-model §2.1).