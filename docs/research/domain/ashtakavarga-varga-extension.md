# Ashtakavarga — cross-varga application + a summary comparison chart

**Correction, 2026-09-22:** this note's premise below is stale. `FEAT-ASHTAKAVARGA-01` had
already shipped before 2026-09-16 (migrations 074–078, `AshtakavargaCalculator`,
`verify-ashtakavarga` green) — `pvr-coverage.md` Ch.12 just hadn't been updated to say so until
today. Both items below are **not** blocked on an unbuilt engine; they're pure read-side/UI work
on top of an already-verified one. Triaged to `FEAT-ASHTAKAVARGA-02`, `ROADMAP.md` Now.

**Second correction, 2026-09-23:** §1's premise below is *also* stale, and in the opposite
direction from what it assumed. Reading (a) (cross-reference, "the lower-risk, working
assumption") was never built. Reading (b) (independent recompute per varga) is what's actually
shipped — `ChartGenerationService.PersistAnalytics` runs `AshtakavargaCalculator.Calculate`
separately for every chart type, and `AshtakavargaChart.razor` already has a live "Varga"
selector reading distinct per-chart SAV/BAV. This was found *not* uncited, either: P.V.R.
Narasimha Rao's *Integrated Approach*, Example 39 (Part 1, p.155) states directly —
"Ashtakavarga of divisional charts is prepared in the same manner as that of rasi chart...
we can find SAV of a divisional chart too" — and works the method through India PM A.B.
Vajpayee's D-10 (Examples 39/101/108: D-10 lagna's 35 rekhas and the 8th house's 33 rekhas
explain both his career success and its struggles, where D1's own lagna/10th bindus were only
average). **Decided 2026-09-23 (rammyps):** keep (b) as-is, now cited to `SRC_PVR_INTEGRATED`
Example 39/p.155 — no engine or schema change needed. `verify-ashtakavarga` Phase 6 (added
2026-09-23) guards that D9's persisted SAV is a genuine independent recompute, not a relabeled
D1 copy. §2's comparison chart remains unbuilt Web work — its three open sub-questions were
decided the same day (see `masterproduct.md` `FEAT-ASHTAKAVARGA-02` / ROADMAP).

Research/design note (2026-09-19, chat session) extending [[vedic_reading_layers]] Stage 03
("Capacity"). Two additions proposed here, both unbuilt:

1. Apply the natal Ashtakavarga bindu table to every varga (Dn) chart, not just D1.
2. A new stacked-bar chart summarizing that cross-varga bindu picture as a %, toggled By House /
   By Sign.

This note scopes what to build against the now-shipped Ch.12 engine, so the extension and the
chart aren't an afterthought bolted on later. `SRC_IKIASTRRO_SYNTHESIS` pattern (no PVR/JHora
citation confirmed for item 1 yet — flagged below, not assumed).

## 1. Extending Ashtakavarga to all varga charts — a scope decision, not a default

"Extend Ashtakavarga to all varga charts" reads two different ways, and they're not the same
amount of work or the same citation risk:

| Reading | What it means | Citation status |
|---|---|---|
| **(a) Cross-reference (recommended default)** | Compute Bhinnashtakavarga/Sarvashtakavarga exactly once, from D1 graha + Lagna positions, per the classical contribution rules (unchanged). Then, for every stored Dn chart, look up each occupied sign's already-computed SAV/BAV(planet) bindu score and surface it against that varga's placements. One engine, one set of tables, N read-side joins. | Standard interpretive practice (checking a varga sign's bindu strength) — needs the exact citation added to `pvr-coverage.md` Ch.12 rather than assumed, but the *mechanism* (bindus are a per-rasi score, looked up regardless of which chart placed a planet there) isn't in question. |
| **(b) Independent recompute per varga** | Re-run the full 8-contributor BAV algorithm using each varga's *own* graha and lagna sign positions as input, producing a separate Ashtakavarga table per Dn (16–21 tables instead of one). | No PVR/JHora citation found so far. Classical Ashtakavarga texts (and PVR's own treatment, per Ch.12) define the contribution rules against rāśi (D1) specifically — flagging this as **unconfirmed scope**, not something to build without a source. |

**Working assumption for design purposes: (a).** It's the lower-risk, classically-grounded
reading, it reuses the single engine Ch.12 will build rather than multiplying it by chart count,
and it matches this project's existing rigor norm (flag before assuming — see [[vedic-books]]'s
Choudhry/PVR correction and the `ikiastrro-tamil-nakshatra-no-source` memory's *opposite* case,
where a source was knowingly skipped for cosmetic data; this is not cosmetic data, so the norm
here is "cite before building," not "skip is fine"). (b) stays listed only so it isn't silently
lost if rammyps has a specific source in mind for it.

### Consequence for scope

Under (a), "Ashtakavarga extended to all vargas" is a **read-side/interpretation feature**, not a
second calculation engine:

- Ch.12 still only builds one BAV (×7 planets) + one SAV, 12 signs each — as already scoped in
  [[pvr-coverage]].
- The extension is: for every varga chart already rendered (`VargaView`, `AllCharts`), join each
  placed graha's occupied sign — and the varga's own lagna sign — against that single natal
  Ashtakavarga table, and surface the bindu count inline (e.g. alongside `PlanetPositionsTable` /
  `HouseLordshipTable` rows, the same tables `chart-catalog.md` already lists per-varga on
  `VargaView`).
- Ch.12's engine already shipped (2026-09-22 correction above) — this is buildable now, tracked
  as `FEAT-ASHTAKAVARGA-02`.

## 2. Cross-varga bindu comparison chart — By House / By Sign, % stacked bar

Requested directly: "a summary stacked bar chart to compare house & sign vs different vargas, %
across vargas." This is the same *shape* of chart Key Inference 3.4 Amsabala already built for
Vimśopaka-style varga-group comparison — [[key-inference]] describes it as "a stacked equal-width
bar per graha, built from `tbl_Rule_AmsabalaGroup`'s actual seeded membership... one segment per
varga in that scheme" — generalized here from *one bar per graha* to *one bar per house or per
sign*, and from Vimśopaka weight to Ashtakavarga bindu count.

### Why "By House" and "By Sign" are genuinely different views, not two labels on one dataset

- **By Sign** — a fixed rāśi frame (Aries…Pisces, 12 bars). Each bar = one absolute sign; each
  segment in the stack = that sign's Sarvashtakavarga bindu count *as looked up from whichever
  varga is being compared* (D1's own bindus for the sign, D9's bindus for whichever sign D9 places
  there, etc. — under reading (a) above, that's the same 12-sign SAV table each time, so this view
  mainly answers "does the classical D1 bindu strength of a sign persist as meaningful once other
  vargas are stacked alongside it," which is a wash under (a) unless (b) is later confirmed and
  built — worth stating plainly so the chart's honesty doesn't get oversold ahead of that
  decision).
- **By House** — a relative frame anchored to *each varga's own lagna* (Stage 02's "vargas as
  complete charts… each gets its own lagna," already in [[vedic_reading_layers]]). Bar 1 = house 1
  *of that varga* (its own ascendant sign), bar 12 = house 12 of that varga. Because a varga's
  lagna sign generally differs from D1's, the same physical rāśi lands in a different house number
  per varga — so this view is where the comparison is actually informative under reading (a): "how
  much Ashtakavarga bindu support does the 10th house get, across D1/D9/D10/…, once each varga's
  own house-1 is used as the anchor."

Recommend **By House as the primary/default toggle state** given the above — By Sign stays useful
mainly as a sanity check on whichever recompute-vs-lookup decision (a)/(b) lands on.

### Chart mechanics (hand-rolled SVG, per `dataviz.md` — no charting library)

- 12 equal-width bars (houses 1–12, or signs Aries–Pisces per toggle), each stacked to a fixed
  100%-height track — same normalization discipline as `PlanetStrengthChart.SharePercent`'s
  clamp-to-100% fix already documented in [[key-inference]] (a real bug hit building that
  component; this chart should reuse the same clamped-share helper rather than re-deriving it).
- One segment per varga included in the comparison — default to the same varga groupings already
  seeded in `tbl_Rule_AmsabalaGroup` (Vargottama/Shadvarga/Saptavarga/Dasavarga/Shodasavarga)
  rather than inventing a new grouping, so a chart-toggle for "which varga set" is free reuse, not
  new seed data.
- Segment height = that varga's bindu count at that house/sign, normalized to % of the bindu total
  summed across the included vargas for that bar (mirrors 3.4 Amsabala's per-graha normalization,
  applied per-bar instead of per-graha).
- Colour: one hue per varga, reused from whatever token set Ch.12/3.4 Amsabala settles on for
  varga identity — do not invent a second varga colour scheme; `design-language.md` additive-only
  rule applies.
- Naming, once this reaches a build pass: `AshtakavargaVargaCompareChart` (PascalCase, `...Chart`
  suffix per `project_standards.md` §3.1, since a page will likely share the base name). Adding it
  needs the spec doc + catalogue row + golden snapshot in the *same* change per §3.2 — not done
  here, since Ch.12's engine isn't built yet and a spec doc for an uncomputed dataset would be
  premature.

## Open items before either half is buildable

1. ~~Confirm reading (a) vs (b) above with a citation~~ **Resolved 2026-09-23** — (b) is what's
   shipped, and it's cited: `SRC_PVR_INTEGRATED` Example 39/p.155 (Vajpayee D-10 case study).
2. ~~Neither item has a `FEAT-*` slot~~ **Resolved 2026-09-22** — `FEAT-ASHTAKAVARGA-02`,
   `ROADMAP.md` Now (Ch.12/`FEAT-ASHTAKAVARGA-01` was already shipped, not actually blocking).
3. Add a row to [`v5-notes-index.md`](../v5-notes-index.md) for this file (done alongside this
   note, not left as a follow-up).
4. `dataviz.md`'s existing "Ashtakavarga heatmaps" mention (under the deferred-Syncfusion section)
   predates this note — reconcile that mention against the stacked-bar approach here when
   `FEAT-ASHTAKAVARGA-02` gets designed (a heatmap and a stacked bar answer different questions;
   decide whether the project wants one, the other, or both rather than letting the old mention
   imply a heatmap is still the plan).
