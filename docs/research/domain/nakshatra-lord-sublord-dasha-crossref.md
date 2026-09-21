---
last_updated: 2026-09-22
aliases: [nakshatra_sublords]
---

# Nakshatra/pada lord ↔ KP sub-lord chain, nakshatra color, dasha ↔ rasi/nakshatra

Status check across four threads from rammyps's 2026-09-18 discussion, plus a follow-up
Q&A the same day drilling into how `tbl_Fact_KpSubLordChain` actually works (§§5–7). Three
of the original four threads are already built (in whole or in part) and just need
surfacing; one (nakshatra color scheme) doesn't exist anywhere in the repo yet.

## 1. Nakshatra lord → L1–L7 sub-lord chain — built, mostly surfaced

This connection isn't a missing feature — it's the literal mechanism the chain algorithm
uses. `AstroMath.GetKpSubLordChain` (`AstroMath.cs:287-319`) sets `cycleStart = nakshatraIndex
% 9` before the loop starts: **the nakshatra's own lord opens level 1's 9-way Vimshottari
cycle**, and each subsequent level reseeds from the previous level's chosen lord
(`cycleStart = NakshatraLordCycleIndex[lord]`). So "nakshatra lord connects to L1–L7" is true
by construction, not something to build.

What's stored/surfaced:

| Level | Storage | Surfaced where |
|---|---|---|
| Nakshatra lord (star lord) | `tbl_Chart_KeyDetails.NakshatraLordPlanetId` | Web: `PlanetPositionsTable.razor` ("Nak Lord" column) |
| L1 (classical KP Sub Lord) | `tbl_Chart_KeyDetails.NakshatraSubLordPlanetId` | Web: same table ("Sub Lord" column); also joined in `tvf_Chart_DashaLordRelationship` (migration 110, see §4) |
| L2–L7 | `tbl_Fact_KpSubLordChain` (migration 095, schema-only until the 2026-09-18 DI-wiring fix — see [[res_charakarakas]]'s "resolved" note) | **Not surfaced anywhere** — no Web column, no CLI report beyond `verify-*` checks, not joined into `tvf_Chart_DashaLordRelationship` yet |

**Gap:** L2–L7 are now populated (162 rows across the 3 dev-DB charts as of 2026-09-18) but
have zero read-side consumer. Nearest precedent for how to expose them: the stacked-bar
`tbl_Fact_KpSubLordChainDistribution` rollup (migration 117) built for a lord-frequency chart —
that's an aggregate view, not the per-planet chain itself. A `PlanetPositionsTable` extension
(columns Sub², Sub³, … or an expandable chain cell) or a dedicated KP panel is the missing
piece, not new computation.

## 2. Nakshatra pada lord → L1–L7 sub-lords — resolved 2026-09-22: out of scope, not built further

**Decision (rammyps, 2026-09-22):** rejected — "not usually done." `FEAT-NAKSHATRA-02` covers
only the plain Nakshatra Lord → Sub-Lord (L1–L7) chain below (§1), which is definitionally the
same mechanism and already computed straight from exact longitude via Swiss Ephemeris
(`AstroMath.GetKpSubLordChain`). The Pada Lord chain discussed in this section stays exactly
where migrations 125–126 left it — committed, unconsumed, no further work planned. Kept below
for the record of what was considered and why it was turned down.

### (superseded) partially built, and the *pada-lord* half is missing entirely

Two different things are being connected here, and only one of them currently exists as data:

- **Pada number** — stored (`tbl_Chart_KeyDetails` derives it) and shown in the Web table
  ("Pada" column, `PlanetPositionsTable.razor:12`).
- **Pada lord** — meaning the sign lord of that pada's **navamsa sign**
  (`tbl_NakshatraPadas.NavamsaSignId` → `HouseEngine.GetSignLord`). **This does not exist as a
  stored or displayed value anywhere.** `tbl_NakshatraPadas` only exposes `RulingPlanetId`
  as a computed column, but that's `fn_GetNakshatraRulingPlanetId(NakshatraId)` — i.e. it just
  repeats the *nakshatra's* star lord on every one of its 4 pada rows, not the pada's own
  navamsa-sign lord. So today there is no query anywhere that says "pada lord."

**Open question, same shape as the undefined "chakra" concept in [[res_charakarakas]]:**
connecting a D9 pada lord (a Parāśari/varga concept) to the KP sub-lord chain (a Vimshottari-
proportional subdivision) isn't a link either system defines on its own — they're
independent lineages, unlike nakshatra-lord→chain which is definitionally the same
mechanism. Before this is buildable as an *interpretive* rule, it needs either:
- a citation for a technique that explicitly does this (KP literature sometimes cross-reads
  Navamsa against sub-lords, but this repo has no such source registered yet — check against
  `sources.md` before assuming one exists), or
- confirmation this is meant as a plain **side-by-side cross-reference table** (pada lord |
  L1 | L2 | … | L7, no interpretive claim, purely descriptive), which is mechanically trivial
  to add right now since every input already exists (`NavamsaSignId`, `HouseEngine.GetSignLord`,
  `tbl_Fact_KpSubLordChain`) — this is the more likely intent and the cheap path if so.

**Important distinction confirmed in the 2026-09-18 follow-up (§6):** Pada Lord is **one
resolved value** (108-way static boundary lookup: longitude → pada → `NavamsaSignId` → sign
lord), not a multi-level chain. There is no classical technique found anywhere in this repo's
sources for running the KP-style recursive 9-fold Vimshottari split *starting from* a pada or
its navamsa lord to produce an analogous "pada L1–L7." If "Pada & their sub-lords" is meant
literally (a chain rooted in the pada, mirroring the nakshatra one), that's a new, undefined
construct needing its own citation/definition first — same status as the undefined "chakra"
concept. If it means "compute and persist Pada Lord per planet per chart, the same
engine-driven way `tbl_Fact_KpSubLordChain` persists L2–L7," that's straightforward and
buildable today (see §7).

## 3. Nakshatra color scheme — not built, no prior art

Checked `tbl_Nakshatras` schema (`RulingPlanetId`, `RulingDeity`, `Symbol`, `Guna`, `Gana`,
`YoniAnimal`/`YoniGender`, `Nadi`, `Varna`, `Tatva`, `Direction`, plus `TamilName` added in
migration 118) — no color column, and no UI spec (`docs/ui/`) or design-language doc mentions
a nakshatra color scheme. This is genuinely pending, not a surfacing gap like §1/§4.

Nearest precedent for *how* to add it: migration 118's `TamilName` — added as
`NVARCHAR NULL`, explicitly marked informal/display data, exempted from the strict
classical-attribute sourcing bar (see [[ikiastrro-tamil-nakshatra-no-source]] memory / the
migration's own comment). A color column would likely follow the same pattern **if** it's a
cosmetic/display attribute — but "color scheme" is ambiguous enough to need a decision before
building:
- Per-nakshatra color derived from **Tatva** (element, already a column) — 5-color palette.
- Per-nakshatra color derived from **ruling planet** (`RulingPlanetId`) — 9-color palette,
  would want to match whatever planet-color convention (if any) exists in `docs/ui/brand.md`
  or `design-language.md` (neither currently defines planet colors either — checked, no hits).
- An independent classical 27-color scheme from a specific source (would need a `SRC_*`
  citation like every other classical attribute here — none identified yet).

Flagging as **undecided**, not just unbuilt — needs a source or an explicit "treat as
cosmetic like TamilName" call before a migration is written.

## 4. Maha-dasha / sub-dasha ↔ Rasi/Nakshatra — built, missing from Web only

`tvf_Chart_DashaLordRelationship` (migration 110) already does exactly this, and covers **all
three** Vimshottari levels, not just Mahadasha: for every dasha period at L1_MAHA / L2_ANTAR /
L3_PRAT, it cross-references that period's Lord planet against the Lord's own D1 placement —
Rasi, Rasi lord, Nakshatra, Nakshatra lord, and KP sub-lord L1 (`SubLordL1PlanetId`). "What
rules the ruler," per dasha level, in one TVF.

Consumed today only by `DashaLordRelationshipRepository` → `Ikiastrro.Cli/Program.cs`
(verification mode). **No Web consumer** — same "Missing Web" pattern `ROADMAP.md`'s Now
bucket already tracks for other verified-but-unrendered engine output. This is a UI-only gap,
not an engine gap.

**Known follow-up already flagged** (migration 110's own comment + [[res_charakarakas]]):
extending the TVF's join to `tbl_Fact_KpSubLordChain` for L2–L7, deliberately deferred when
written because that table was still unpopulated. **Done 2026-09-22** (migration 131) —
`tvf_Chart_DashaLordRelationship` now returns `SubLordL2PlanetId`…`SubLordL7PlanetId` alongside
the original `SubLordL1PlanetId`. Still no Web consumer of this TVF — that stays a separate,
un-slotted UI-placement decision.

## 5. Correcting a premise: the chain is *not* just "nakshatra lord → fixed outcome"

Follow-up question: since `cycleStart` in `GetKpSubLordChain` is seeded from the nakshatra
alone, could L1–L7 be a static internal mapping keyed by Nakshatra (or Nakshatra Lord), with
no dependence on `NirayanaLongitudeDegrees`? **No** — `cycleStart` only fixes which *order*
the 9 lords appear in for level 1's cycle. Which of those 9 slots the planet actually lands
in is decided by `positionInSpan`, the continuous 0–1 fraction of exactly where in the
nakshatra the longitude falls — and every level after that reseeds from both the previous
level's chosen lord *and* the repositioned fraction. So two planets in the *same nakshatra*
at different degrees can resolve to different L1, and diverge further by L2–L7.

Concrete case — Ashwini (0°–13°20′ Aries, cycle order Ke→Ve→Su→Mo→Ma→Ra→Ju→Sa→Me), level-1
slot widths roughly Ketu 0°–0.81°, Venus 0.81°–3.13°, Sun 3.13°–3.82°, … : a planet at 1°
Aries and one at 3.5° Aries are both in Ashwini, both in **pada 1** (0°–3°20′) even, yet the
first resolves to Venus as L1 and the second to Sun. Pada doesn't pin it down either — a pada
(3°20′) is wider than several level-1 sub-divisions it overlaps.

**What actually is static:** the boundary layout itself (which degree ranges within a
nakshatra belong to which of the 9 lords) — already materialized for levels 1–2 as
`tbl_Rule_SignNakshatra` (243 rows = 27 × 9, migration 096). That's a lookup by *degree
range*, not "nakshatra → lord." Migration 095 explicitly rejected extending that
boundary-table approach to L2–7 (≈27 × 9⁶ ≈ 14.3M rows, bands down to ~1e-8°) — hence the
recursive-arithmetic + persisted-result design instead.

## 6. Is this Moon-only? No — every planet gets a nakshatra and its own chain

Follow-up question: since only the Moon's nakshatra matters for astrology, is this activity
Moon-specific? **No.** A nakshatra is a fixed 13°20′ zodiac slice — any body's longitude falls
into one, not just the Moon's. Confirmed in code:
- `ChartAnalyzer.cs:62` — `foreach (var planet in input.Planets)` computes
  Nakshatra/NakshatraLordPlanet/Pada for every planet, unconditional on which one.
- `KpSubLordChainRepository.InsertAll` takes all 9 grahas' longitudes.
- `PlanetPositionsTable.razor` shows Nakshatra/Pada/Nak Lord/Sub Lord for every planet row.

This is structural to KP: significator chains depend on *every* planet's star lord and sub
lord (e.g. "occupies a nakshatra ruled by a planet that owns/occupies a given house"), not
just the Moon's.

**Where the Moon genuinely is unique:** Vimshottari Dasha sequencing.
`AstroMath`'s "nakshatra index + fraction elapsed" helper is explicitly documented as the
basis for the dasha system's "balance of first dasha" — the Moon's nakshatra and exactly how
far through it the Moon sits determines which planet's dasha runs first at birth and for how
much of its period. That's specific to dasha-sequencing, not to nakshatra/sub-lord
computation generally.

**Adjacent, currently out of scope:** classical KP also has *cuspal* sub-lords (of house
cusps, not planets) — a second, separate sub-lord concept. [[res_charakarakas]] already
resolved that `tbl_Fact_KpSubLordChain` is the **planet** chain only; cuspal sub-lords are a
distinct, unbuilt thing if ever wanted.

## 7. `tbl_Fact_KpSubLordChain` design — why persisted, not a derived view, and one real gap

Follow-up question: since the chain is a pure function of `NirayanaLongitudeDegrees` (already
stored on `tbl_Chart_KeyDetails`), could/should this be a derived table instead of one that
"has to change over time"? Findings:

- **The value itself never needs recalculating** for a stable, correctly-generated chart —
  same longitude always produces the same chain (verified byte-for-byte before wiring in).
  It would only ever need regenerating the way every other position-derived fact in this
  schema does: on a birth-data correction, or on an ayanamsa correction (see below) — both
  already handled via `KpSubLordChainRepository.DeleteByChartResultId`/`DeleteByBirthDetailId`
  + regenerate, no special case needed.
- **It's derivable in the app tier with zero storage** for single-chart reads — the Web UI or
  CLI could call `AstroMath.GetKpSubLordChain` directly off the already-loaded longitude, no
  need to consult the table at all for that consumer.
- **Persistence is justified by two other consumers that need SQL-side set operations over
  it:** `tbl_Fact_KpSubLordChainDistribution`'s cross-chart `GROUP BY` rollup (migration 117),
  and the planned `tvf_Chart_DashaLordRelationship` extension to L2–L7 (a TVF `JOIN`). Neither
  is expressible in pure SQL without either re-implementing the recursive Vimshottari-split
  logic in T-SQL (duplicated logic, drift risk) or a SQL CLR function (not used anywhere else
  in this codebase). This matches the repo's established pattern: non-trivial procedural
  computation → persisted `tbl_Fact_*` written once by a `*Computer`/repository at
  chart-generation time; TVFs stay reserved for joining across already-persisted facts.
- **Real gap found — closed 2026-09-22 (migration 130).** The repo's own stated Facts convention is "`tbl_Fact_*` rows record
  which `RuleSetId` produced them, so a chart's evidence is traceable to the exact rule
  version" (`rules-engine.md`). Checked the siblings — `tbl_Fact_PlanetaryStrength`,
  `tbl_Fact_PlanetaryStrengthComponent`, and `tbl_Fact_Vargottama` **all** carry a `RuleSetId`
  FK. `tbl_Fact_KpSubLordChain` (migration 095) does **not** — only `ChartResultId, PlanetId,
  Level, LordPlanetId, ComputedAtUtc`. If the KP cycle-order convention
  (`NakshatraLordOrder`) is ever revised or a second school's variant is added, this table
  can't say which version produced a given row, unlike every other Fact here. Adding
  `RuleSetId NOT NULL` is the one thing about this table's *design* that should change,
  independent of any ayanamsa question.
- **Ayanamsa correction is a whole-schema concern, not specific to this table.** A corrected
  ayanamsa shifts every `NirayanaLongitudeDegrees`, cascading into Sign, Nakshatra, Pada,
  Nakshatra Lord, this chain, Vargottama, dignity, strength — everything longitude-derived,
  across every existing chart. This is exactly why the ayanamsa value was hardcoded and
  deferred on 2026-09-13 (see [[ikiastrro-ayanamsa-hardcode-decision]] memory) rather than
  something this table's design needs to solve on its own.
- **Pada Lord would follow the identical persistence pattern if built** (§2): computed at
  chart-generation time from the same `NirayanaLongitudeDegrees` (→ `NavamsaSignId` → sign
  lord, a single value, no recursion), persisted per planet per chart — same shape as the
  existing chain, just one column instead of six.

## Summary — pending vs. available

| Thread | Available | Pending |
|---|---|---|
| Nakshatra lord → L1–L7 | **Done 2026-09-22.** Algorithm inherently connects them (nakshatra lord seeds the cycle *order*; exact longitude still decides the outcome, §5); L1 stored + shown; L2–L7 stored (2026-09-18); `RuleSetId` added (migration 130, §7); L2–L7 now shown live in `PlanetPositionsTable` (d1 variant, "Sub-Lord Chain (L2–L7)" column); Web-generation gap closed (`Ikiastrro.Web/Program.cs` now registers `KpSubLordChainRepository`) | A dedicated KP panel beyond the compact table column, if wanted later |
| Pada lord → L1–L7 | **Out of scope, resolved 2026-09-22 (rammyps) — "not usually done."** Migrations 125–126 built the connection anyway (2026-09-19, before this was raised) and stay committed/unconsumed; no `FEAT-*` work planned. | — |
| Nakshatra color scheme | Nothing | Everything — needs a source or an explicit cosmetic-data call (TamilName precedent) before any migration |
| Dasha (Maha/Antar/Pratyantar) ↔ Rasi/Nakshatra | Fully built for L1–L7 sub-lord via `tvf_Chart_DashaLordRelationship` (migration 110, extended to L2–L7 by migration 131, 2026-09-22) | Web UI consumer — no page surfaces this TVF at all yet, own UI-placement decision |
| Scope check | Confirmed: computed for **every** planet, not Moon-only; Moon is uniquely load-bearing only for Vimshottari dasha sequencing (§6) | — |
