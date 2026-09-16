---
last_updated: 2026-09-16
reflects: master (Vargas step-6 redesign in progress, see "Build roadmap" below)
---

# Planet strength statistics — normalization framework & build roadmap

Source: rammyps's research conversation (2026-09-16), transcribed and organized here so it's a
living spec rather than chat scrollback. This is a **design/architecture doc**, not a classical
astrological source — every formula below is either (a) a direct restatement of a measure this
project already computes, standardized for comparison, or (b) an explicitly-labeled analytical
policy (a chosen weight, blend, or interpretation band) that is **not** a classical rule and must
stay visible, auditable, and configurable rather than buried as a hardcoded constant.

## 0 · The core principle

Shadbala, Amsabala, Bhavabala, and Ashtakavarga answer different questions and must be
**standardized for display and comparison, never treated as interchangeable measurements, and
never silently averaged together** (their domains overlap — e.g. Bhavabala already contains a
planet's Shadbala inside `Bhavadhipati Bala`, so summing Shadbala + Bhavabala double-counts it).

| Measure | Subject | Core question |
|---|---|---|
| Shadbala | Planet | How much operational power does the planet possess in the natal context? |
| Bhavabala | House | How well-supported is this field of life? |
| Amsabala | Planet across vargas | How consistently does the planet retain high dignity across divisional contexts? |
| Ashtakavarga | Sign/house | How supportive is the sign, particularly for manifestation and transits? |

Mental model: **Shadbala = power; Amsabala = reliability; Bhavabala = field support; Ashtakavarga
= rāśi support and transit receptivity.**

## 1 · Standardize each measure separately

### 1.1 Shadbala normalization

Raw rupas aren't comparable across planets — each has a different classical minimum requirement.

$$S_p = 100 \times \frac{\text{Shadbala Rupas}_p}{\text{Required Rupas}_p}$$

100 = meets the planet-specific requirement; 120 = 20% above; 80 = 20% below. For a bounded UI
score, cap the display at 150% of minimum without discarding the uncapped value:

$$S^{*}_p = 100 \times \min\left(\frac{S_p}{150}, 1\right)$$

| % of minimum | State |
|---|---|
| < 75% | Very weak |
| 75–89% | Weak |
| 90–109% | Adequate |
| 110–129% | Strong |
| ≥ 130% | Very strong |

Keep both `PercentOfMinimum` (already computed, `PlanetaryStrengthRepository`) and the uncapped
rupas — `S*` is a display transform, not a replacement.

### 1.2 Amsabala normalization

A raw count can't be compared across schemes with different denominators (4/6 Shadvarga ≠ 4/16
Shodasavarga in consistency, even though both count "4"):

$$A_{p,g} = 100 \times \frac{\text{Good Vargas}_{p,g}}{\text{Vargas in Group}_g}$$

For an overall score, don't average all four schemes — they overlap (the same varga appears in
multiple groups), which would repeatedly count it. Either pick one canonical scheme (Shodasavarga
when all 16 charts are available) or report every scheme independently. The named amsa
(`AmsaName`) stays a classical classification — never convert it into an invented numerical
weight.

### 1.3 Bhavabala normalization

Needs either a source-defined required value or a chart-relative normalization. If a classical
minimum exists:

$$B_h = 100 \times \frac{\text{Bhavabala}_h}{\text{Required Bhavabala}_h}$$

Otherwise, a within-chart z-score-based relative score:

$$B^{relative}_h = 50 + 10 \times \frac{B_h - \operatorname{mean}(B_1,\ldots,B_{12})}{\operatorname{SD}(B_1,\ldots,B_{12})}$$

A percentile rank among the 12 houses is an even simpler UI presentation.

**Double-counting risk:** this project's Bhavabala is `Bhavadhipati Bala + Bhava Dig Bala + Bhava
Drik Bala` (`BhavaBalaCalculator`, persisted as 3 separate rows in
`tbl_Fact_BhavaStrengthComponent`). `Bhavadhipati Bala` **is** the house lord's own Shadbala, so
combining raw Bhavabala with a planet's Shadbala elsewhere double-counts it. Split it:

$$B^{independent}_h = \text{Bhava Dig Bala}_h + \text{Bhava Drik Bala}_h$$

Use only `B^independent` when combining Bhavabala with planetary Shadbala in any composite score.

## 2 · Connect Shadbala/Bhavabala through planet–house roles

Bhavabala shouldn't be assigned to a planet as if it were another planetary bala — connect them
relationally through the houses the planet is placed in, owns, or signifies (kāraka):

$$C_p = w_o \cdot \operatorname{mean}(B^{independent}_{H_{owned}}) + w_p \cdot B^{independent}_{H_{placed}} + w_k \cdot \operatorname{mean}(B^{independent}_{H_{karaka}})$$

Suggested default weights (**analytical policy, not classical — keep configurable**): ownership
50%, placement 30%, kāraka 20%.

$H_{placed}$ = `ChartKeyDetail.HouseFromLagna`. $H_{owned}$ = reverse lookup on
`tbl_Chart_HouseLords`. $H_{karaka}$ = `CharaKaraka` / `NaisargikaKarakaRepository` (already used
by step 4 Spl Lagnas).

## 3 · Ashtakavarga as a second context layer

Ashtakavarga overlaps Bhavabala in subject (both ultimately trace back to planetary positions)
but not in method, and the two answer different questions — **neither replaces the other**:

| Layer | Measure | Question |
|---|---|---|
| Planetary capacity | Shadbala | How much power does the planet have to act? |
| Divisional consistency | Amsabala | How consistently is its dignity preserved across vargas? |
| Natal house support | Independent Bhavabala | How intrinsically supported is the relevant house? |
| Rāśi support & transit receptivity | Ashtakavarga | How supportive is the sign, particularly for manifestation/transits? |

**Why they aren't the same**, despite both starting from planetary positions:
- Bhavabala is **vertical** — it gathers several factors (lord strength, direction, aspects)
  around *one* natal house.
- Ashtakavarga is **horizontal** — each planet's Bhinnashtakavarga (BAV) distributes favourable
  points across *all twelve* signs using fixed relative-position rules from 8 contributors (7
  classical planets + Lagna); a planet occupying a sign doesn't automatically give that sign a
  bindu — the contributor's position is only the reference point rules are counted from.
  Sarvashtakavarga (SAV) is the combined bindu total across all contributors' BAVs for a sign.
- Analogy: *the bhava is a workplace (Bhavabala = the workplace's structural strength); the
  occupying planet is a worker (Shadbala = the worker's capability); Ashtakavarga maps how
  favourable different locations are for each worker's activity.*

Under whole-sign houses, one sign = one house, so Taurus SAV and the 10th-house Bhavabala (if
Taurus is the 10th) describe the *same life domain* through *different mathematics* — preserve
both identities rather than treating "sign support" and "house support" as identical, especially
if bhava cusps or unequal houses are ever introduced.

Don't mix SAV and BAV indiscriminately: SAV = overall support in a sign; BAV = how *one specific
planet's* framework supports a sign (important for that planet or its transits).

$$AV_{p,h} = w_s \cdot N(SAV_h) + w_b \cdot N(BAV_{p,h})$$

Suggested default (**analytical policy**): $w_s = 0.60$, $w_b = 0.40$. Reference-centred
normalization (100 ≈ average support, not maximum) reads more clearly than %-of-theoretical-max:

$$N(SAV_h) = 100 \times \frac{SAV_h}{28} \qquad N(BAV_{p,h}) = 100 \times \frac{BAV_{p,h}}{4}$$

Preserve the raw bindu counts beside the normalized score. Full context-per-house blend
(**analytical policy**, suggested default 40/35/25):

$$Context(p,h) = w_{bh} \cdot N(B^{independent}_h) + w_{sav} \cdot N(SAV_h) + w_{bav} \cdot N(BAV_{p,h})$$

Then aggregate across the planet's owned/placed/kāraka houses with the §2 weights.

## 4 · Recommended 3-axis planet-strength profile

Three separate axes, each independently meaningful:

| Axis | Source | Meaning |
|---|---|---|
| Capacity | Normalized Shadbala ($S^*$) | Can the planet act strongly? |
| Consistency | Normalized Amsabala ($A_{p,g}$) | Does its dignity persist across vargas? |
| Context | Independent Bhavabala + Ashtakavarga of connected houses | Are its relevant life areas supported? |

Optional composite via **geometric mean** (preferred over arithmetic — a serious weakness in one
axis can't be fully hidden by strength in another). **Analytical policy**, suggested weights
0.50/0.30/0.20:

$$P_p = 100 \times \left(\frac{S^*_p}{100}\right)^{0.50} \left(\frac{A_p}{100}\right)^{0.30} \left(\frac{C^{independent}_p}{100}\right)^{0.20}$$

The 3-axis profile is more informative than the single composite — e.g. 85/30/75 (powerful but
inconsistent across vargas) vs. 55/90/80 (reliable promise, limited immediate power) vs. 90/85/25
(strong planet in poorly supported life areas) tell very different stories a single number hides.

### Interpretation matrix

| Shadbala | Amsabala | Connected Bhavabala | Reading |
|---|---|---|---|
| High | High | High | Strong, durable and well-supported delivery |
| High | Low | High | Powerful in D1, but results may not persist across domains |
| Low | High | High | Good underlying promise, but weak present capacity to execute |
| High | High | Low | Strong planet facing weak environmental/house support |
| Low | Low | High | House has support, but this planet isn't its effective executor |
| Low | Low | Low | Consistently weak indication requiring substantial counterevidence |

### Full timing model

$$\text{Planetary outcome} = \text{Capacity} \times \text{Consistency} \times \text{Natal context} \times \text{Timing}$$

Timing = dasha activation + transit BAV/SAV and other timing evidence — out of scope here, noted
for completeness.

**Naming discipline:** call this a "planet strength profile," never a new classical bala. Raw
classical measures, the normalization method, the chosen Amsabala scheme, connected houses, and
every weight must stay visible and auditable in the UI — this is a derived interpretation layered
on top of the classical calculations, not a replacement for them.

## 5 · Amsabala + varga-lord extension

Classical Amsabala asks a narrow question: *in how many relevant vargas does the planet occupy
Exaltation/Moolatrikona/Own sign?* It doesn't measure the condition of the sign's *dispositor*
(varga lord) — e.g. Jupiter in Libra in D9 earns no classical qualification (not own/moolatrikona/
exalted), but if Libra's lord Venus is exceptionally strong in D9, Jupiter may still get
meaningful support through its dispositor. Conversely, a qualified placement can still be
afflicted by conjunction/combustion Amsabala alone won't show.

**Keep classical Amsabala unchanged; attach the following as separate explanatory evidence, never
merged into the classical count:**

1. **Classical qualification** — binary, unchanged: $A_{P,V} \in \{0,1\}$
   (Exalted/Moolatrikona/Own).
2. **Placement dignity** — the broader 9-tier scale already computed by `DignityEngine`
   (`ChartViewModel.DignityScore`, -4..+4; normalized 0..1 via `DignityScoreNormalized`).
3. **Varga-lord condition** — evaluate the lord $L$ of the occupied sign, in the *same* varga:
   $L$'s own dignity there is already available the same way ($Charts[V].Grahas[L].DignityStatus$)
   — cheaper to compute than it first appears. Fuller versions could add $L$'s house position,
   conjunctions/aspects, combustion, and avasthas — not built yet.
4. **Relationship with the varga lord** — natural/temporary/compound relationship between $P$ and
   $L$. Caution: if placement dignity already encodes the friend/enemy relationship, don't add it
   again — keep placement dignity and dispositor condition as separate, non-overlapping signals.
5. **Varga-specific house relevance** — interpret the placement per that varga's principal
   context (D1 = overall life, D2 = wealth, D3 = siblings/courage, D7 = children, D9 =
   dharma/marriage, D10 = profession, D12 = parents/ancestry).
6. **Dispositor-chain analysis** — follow $P \to L \to L\text{'s own lord} \to \ldots$ until it
   terminates in own/exalted/moolatrikona dignity (self-supported), a strong dispositor
   (supported), a weak/debilitated/afflicted one (fragile), a mix (mixed), or a mutual/longer loop
   (circular). Prefer categorical evidence or a weakest-link + terminal-condition rule over
   multiplying scores down long chains (which would unfairly penalize length).

Optional per-varga composite (**analytical policy**, suggested weights 0.35/0.25/0.20/0.20):

$$VPS_{P,V} = 0.35\,\text{Placement} + 0.25\,\text{Dispositor} + 0.20\,\text{House} + 0.20\,\text{Influence}$$

Aggregate across vargas by intended question, not blanket averaging — career → D1+D10, marriage →
D1+D9, children → D1+D7; or a weighted mean over one selected, sourced scheme
($\text{Varga Support}_P = \sum_V w_V \cdot VPS_{P,V} / \sum_V w_V$), never an unweighted average
across overlapping schemes.

## 6 · Build roadmap

This section is the confirmed, in-progress build plan (not a proposal) — see the Key Inference
step-6 redesign (`docs/ui/components/key-inference.md`, 2026-09-16 evening changelog entry) for
the shipping UI work this roadmap's Tier 0 rides alongside.

**Confirmed feasible with zero new migrations** (verified against the codebase, not assumed):
`DignityStatus` (9-tier) is already computed per graha per chart type and loaded into
`WorkspaceData.Charts` for every D-chart; `tbl_Fact_BhavaStrengthComponent` already stores
`BHAVADHIPATI_BALA`/`BHAVA_DIG_BALA`/`BHAVA_DRIK_BALA` as separate rows; `PercentOfMinimum` is
already computed for Shadbala; Ashtakavarga SAV/BAV are already computed and stored. Every Tier 0
and Tier 1 item below is a presentation-layer or read-time-aggregation addition over data that
already exists — no new fact tables, no new migrations.

### Tier 0 — shipped alongside the Vargas (step 6) redesign

- Shadbala `S*` (min(S/150,1)×100) + the 5-band label — add to `PlanetStrengthChart` (already has
  `PercentOfMinimum`).
- Amsabala `A_{p,g}` = GoodCount/GroupSize×100 — added to the step-6 `AmsabalaTable` alongside the
  existing "N/GroupSize" badge.
- Bhavabala **independent** score (`BHAVA_DIG_BALA + BHAVA_DRIK_BALA`) — add to
  `HouseStrengthChart`'s existing component breakdown.
- Dignity `DignityScoreNormalized` (0–1, `ChartViewModel`) — shipped in step 6's varga-lords
  detail table.

### Tier 1 — next, moderate new join/aggregation logic, still no new persistence

- Connected-house `Context_p` — join $H_{placed}$/$H_{owned}$/$H_{karaka}$ against Tier-0
  independent Bhavabala per house.
- Ashtakavarga `N(SAV)`/`N(BAV)` normalization, blended into that context score.

### Tier 2 — later, genuine new engine work, only after Tier 1 ships and is reviewed

- Varga-lord dispositor condition (cheap — same live `DignityStatus` lookup Tier 0 already uses)
  and full dispositor-chain traversal + classification (needs loop detection — genuinely new
  logic).
- The composite geometric-mean Planet Strength Profile ($P_p$) and its 3-axis UI, with a small
  configurable-weights surface (weights must stay visible/adjustable, never buried constants,
  per §4's naming-discipline requirement) — build last, once Capacity/Consistency/Context all
  exist independently.

Tier 2 is out of scope for the current implementation pass.
