# SIND-UNI — unified South Indian grid chart

Status: **approved, in build on `workstream/ui`** (2026-10-01). Visual reference: Claude artifact
"SIND-UNI Chart Proposal" (Ramakrishnan D1, all three themes),
https://claude.ai/artifact/KKTvbKXRmEKrtdECtv61zo.

This spec covers the **grid chart** (South Indian 4×4). The same cell vocabulary (dignity chips,
benefic / malefic, Ashtakavarga, houses-from dropdown, lenses, special-lagna colours, layer
chips) is to be carried to the polar wheel charts later; see "Extension to the polar wheel".

One chart component family replaces the separate South Indian grids on the main pages. The
existing components (`SouthIndianGrid_Detailed`, `SouthIndianGrid_Micro`, `SindHovGrid`,
`D1TemplateGrid`) stay untouched until every page has moved over (project_standards.md §3.3).

| View | Component | Used on | Typical width |
|---|---|---|---|
| SIND-UNI-1 · Compact | `SindUni1Grid` | not placed yet (built, tested) | ~460px |
| SIND-UNI-2 · Reading | `SindUni2Grid` | not placed yet (built, tested) | 640px+ |
| SIND-UNI-3 · Micro | `SindUni3Grid` | Key Inference chart and Astro Facts → Spl Lagnas grid view, both with the special lagnas (one shared view) | 260–600px |

Astro Facts keeps the master chart, `SouthIndianGrid_Detailed`, on the natal step and the All
Charts cards (rammyps, 2026-10-01); it follows all three themes through the page tokens.

All three share one cell renderer, one data model and one stylesheet; the view only decides
which parts of the cell are drawn and at what size.

## Geometry

- Always square: `aspect-ratio: 1`, 4×4 grid with the fixed South Indian sign positions,
  centre 2×2 for the chart title. Width comes from the container.
- Type scales with the chart, not the window: the chart is a size container and each view sets
  one base size in container units (`--fs`: UNI-1 2.55cqi, UNI-2 2.05cqi, UNI-3 3.7cqi); every
  inner size is in `em`.
- 1px hairline grid in the theme's structure colour inside one rounded frame. The Lagna cell is
  tinted (`--asc`) with an inset ring in the accent colour, never a coloured border.
- A faint large zodiac glyph sits in each cell's corner (`--watermark`), switchable.

## Cell anatomy

**Line 1 — the sign.** Zodiac glyph · two-letter sign code · three nature glyphs:

| Glyph | Meaning | Signs |
|---|---|---|
| 🜂 | Fire | AR LE SG |
| 🜃 | Earth | TA VI CP |
| 🜁 | Air | GE LI AQ |
| 🜄 | Water | CN SC PI |
| ↻ | Movable (chara) | AR CN LI CP |
| ■ | Fixed (sthira) | TA LE SC AQ |
| ◐ | Dual (dvisvabhava) | GE VI SG PI |
| planet glyph | Sign lord | all |

The element glyph is drawn in its element colour (`--el-fire`, `--el-earth`, `--el-air`,
`--el-water`): at chip size the alchemical triangles for earth and water (and fire and air)
are hard to tell apart by shape alone.

Sign codes: AR TA GE CN LE VI LI SC SG CP AQ PI. Possible later additions to line 1 (not in
v1): odd/even as + / −, direction (E S W N).

**Top-right corner: benefic / malefic.** `BEN`, `MAL` or `MIX` (see "Benefic and malefic signs").
While a lens is on, a source sign shows its lens badge in this slot instead.

**House badges** sit in the bottom-right corner: a filled square for the house from the Lagna
(`--house-lagna`), then a hollow pill with the reference planet's glyph and the house counted from
that planet (see "Houses from a planet"). Shape differs as well as colour.

**Line 2 — the sign's nakshatras.** The nakshatras the sign spans, by three-letter code, with
one dot per pada that falls inside the sign (always 9 dots per sign). A filled dot (accent) means
the Lagna or a planet sits in that pada. When more than one point shares a pada, the dot grows
into a small filled circle with the count inside. Its tooltip names the points. Example,
Ramakrishnan's Aries: `ASW ②●●● BHA ●●●● KRI ●`. Ashwini 1 holds the Lagna and Mercury.

**Lagna line** (Lagna cell only): `ASC 0°38′ · ASW¹`.

**Planets.** One chip per planet that carries both dignity and direction:

- Label is the two-letter code: SU MO MA ME JU VE SA RA KE.
- Direct: `JU`. Retrograde: `(JU)`. Rahu and Ketu are always `(RA)` / `(KE)`.
- Chip colour is the dignity (`--dignity-*` tokens): text in the dignity colour on a tint of it,
  with a hairline outline. There is no separate dignity dot.
- Combust: a small flame icon (inline SVG, `--flame`) after the degree.
- Optional planet orb (photo) before the chip; see "Planet images".
- UNI-1 / UNI-2 rows also show the degree in sign and the planet's nakshatra as code + pada
  number (`UPH⁴`); UNI-2 can add the chara karaka (`AK`).

**Row rule.** Up to 3 planets in a sign: one per row with full detail. 4 or more: two per
row, chip + degree only (UNI-1 drops the degree too); the padas still show on line 2 and the
full detail is in the hover popover.

**Bottom row.** Left: the Sarvashtakavarga score (see "Ashtakavarga"). Middle: tags. Right: the
house badges.

**Footer tags.** UNI-2: arudha padas (outlined tags, `--ref-arudha`) and, when switched on, the
planets aspecting the sign. UNI-3 (Spl Lagnas): special lagnas BL HL GL SL IL PP PS, the
Arudha Lagna and Gulika / Maandi as outlined tags in the existing `--ref-*` colours.

**Centre.** View label (e.g. `D1 · RASI`), person name, Lagna and Moon with nakshatra code and
pada; UNI-2 adds the dignity legend as chips.

## Houses from a planet

A dropdown in the toolbar ("Houses from ☽ Moon ▾") chooses the second house count. Options, in
this order: Moon (default), Sun, Jupiter, Saturn, Mars, Venus, Mercury. The choice is remembered
per viewer and per view, and the default itself is a setting. The hollow badge shows the chosen
planet's glyph before the number so the reference is never ambiguous.

## Ashtakavarga

Each cell shows the sign's Sarvashtakavarga (`tbl_Fact_SarvaAshtakavarga.TotalBindus`, the
current PVR Parasara method, without the Lagna), as it is stored today, with a small bar drawn
against 56.

- Range per sign: 0–56 in principle (seven planet tables of 0–8 bindus each; the table's check
  constraint enforces exactly this). The twelve signs always total 337, so the average is
  about 28.
- Observed: across the six people saved in the app (D1), 17 to 43. Ramakrishnan: 17 (Libra) to
  43 (Aquarius).
- Colour bands: the app's existing `StrengthBands.SarvaAshtakavargaBindus` (PVR: above 30
  favourable, below 25 unfavourable), so the chart and Astro Facts step 3 never disagree:
  31 and above strong (`--ben`), 25–30 moderate (`--mix`), 24 and below weak (`--mal`).

## Benefic and malefic signs

The verdict is the existing `HouseBeneficMaleficCalculator` (B.V. Raman, How to Judge a
Horoscope vol. 1, p.14-15), the same one Astro Facts → About Houses shows. It counts the lord's
functional nature for this Lagna, the planets in the sign and the planets aspecting it, giving
Benefic, Malefic, Mixed or Neutral. Nothing new is invented in the chart.

- **Corner tag**: `BEN` (`--ben`), `MAL` (`--mal`), `MIX` (`--mix`); Neutral shows nothing.
- **Hover wash**: hovering or pinning a sign tints it with the verdict colour (`--wash`: 9%
  light, 14% dark).
- **Sign card**: names the count, e.g. "Lord Mercury · Malefic; benefics —; malefics Saturn (asp)".
- The verdict depends on the Lagna of the chart shown, so a D9 is judged from the D9 Lagna.
- Layer "Benefic / malefic" switches the tag and the wash together.

## Aspects, Argala and Virodhargala: the lens

The chart stays clean at rest. A segmented control "Lens: Off · Aspects · Argala" draws what acts
on the **pinned** sign (click to pin; on UNI-1 the lens opens in the popover):

- **Argala**: every source sign is outlined, its corner badge shows its place from the target.
  Argala is green (`--arg`) for the 2nd, 4th, 11th and 5th, plus the 3rd when malefics give
  argala there. Virodhargala is red (`--vir`) for the 12th, 10th, 3rd and 9th. Its planets are
  ringed and an arrow runs to the target: solid for argala, dashed for virodhargala. The centre
  of the chart shows the net result from the Argala table, e.g. "Gemini: Argala 7–0". Data:
  `ArgalaFacts` / `tbl_Fact_Argala`, the same rows as the Astro Facts Argala table.
- **Aspects**: every sign holding an aspecting planet is outlined with an `ASP` badge, those
  planets' chips are ringed, and accent-coloured arrows run to the target. The centre lists
  them. Data: `tbl_Chart_Aspects` (graha drishti as the app stores it).
- Rasi drishti can be added as a third lens later.

## Special lagnas (UNI-3: Key Inference and the Spl Lagnas tab)

Several lagnas can be on at once. Each one is a toggle chip with its colour swatch; each selected
lagna draws a colour bar across the top of its sign (stacked when two share a sign) and an
outlined tag in the same colour.

| Code | Lagna | Light | Cosmic dark |
|---|---|---|---|
| ASC | Lagna | theme accent (`--accent`) | theme accent |
| MOON | Chandra lagna | slate #64748b | #cbd5e1 |
| SUN | Surya lagna | orange #c2410c | #fb923c |
| AL | Arudha Lagna | rose #be123c | #fb7185 |
| HL | Hora Lagna | amber #b45309 | #fbbf24 |
| GL | Ghati Lagna | green #15803d | #4ade80 |
| BL | Bhava Lagna | indigo #4338ca | #a5b4fc |
| SL | Sree Lagna | violet #7e22ce | #d8b4fe |
| IL | Indu Lagna | pink #be185d | #f9a8d4 |
| PP | Pranapada | teal #0f766e (cosmic light #0d9488) | #5eead4 |
| PS | Punya Saham | brown #78350f | #d6a26c |
| GK | Gulika · Maandi | stone #57534e | #a8a29e |

Only the Lagna follows the theme. The rest are fixed hue families, kept apart from each other,
from the accent and from the benefic and malefic colours. These replace the current `--ref-*`
tokens as `--sl-*`. Default on: ASC, MOON, AL, HL, GL, SL.

## How the sign's nature reaches the reader

Four layers, from glance to detail:

1. **Element colour rail**: a thin top edge on every cell in its element colour, so fire,
   earth, air and water are visible across the whole chart at once (layer "Element colour").
2. **Line 1 glyphs**: element, modality and lord.
3. **Glyph key**: one strip under UNI-1 and UNI-2, listing the element colours and glyphs, the
   modality glyphs, the lord glyph and the numbered pada dot.
4. **Sign card**: on hover, focus or tap, drawn in the chart's own centre (the 2×2 middle), so
   no page needs room beside the chart. It shows the sign name and code, element,
   modality (with chara / sthira / dvisvabhava), odd / even, lord, nakshatra span
   (`ASW 1–4, BHA 1–4, KRI 1`), the benefic / malefic count, the net Argala, the Ashtakavarga
   score, the house from the Lagna and from the chosen planet, and the occupants. At rest the
   centre shows the title, name and Lagna / Moon line. Line 1 also carries
   a one-line tooltip: `Aries · Fire · Movable · Odd · Lord Mars`.

## Nakshatra codes

English, unique, Purva/Uttara pairs start with P/U.

| # | Code | Nakshatra | # | Code | Nakshatra | # | Code | Nakshatra |
|---|---|---|---|---|---|---|---|---|
| 1 | ASW | Ashwini | 10 | MAG | Magha | 19 | MUL | Mula |
| 2 | BHA | Bharani | 11 | PPH | Purva Phalguni | 20 | PAS | Purva Ashadha |
| 3 | KRI | Krittika | 12 | UPH | Uttara Phalguni | 21 | UAS | Uttara Ashadha |
| 4 | ROH | Rohini | 13 | HAS | Hasta | 22 | SRA | Shravana |
| 5 | MRI | Mrigashira | 14 | CHI | Chitra | 23 | DHA | Dhanishta |
| 6 | ARD | Ardra | 15 | SWA | Swati | 24 | SAT | Shatabhisha |
| 7 | PUN | Punarvasu | 16 | VIS | Vishakha | 25 | PBH | Purva Bhadrapada |
| 8 | PUS | Pushya | 17 | ANU | Anuradha | 26 | UBH | Uttara Bhadrapada |
| 9 | ASL | Ashlesha | 18 | JYE | Jyeshtha | 27 | REV | Revati |

The codes belong in the database (a short-code column on the nakshatra dimension) rather than
in the component, so the planned language switch can replace them with the names and short
forms together.

## Language

English only for now: sign codes, nakshatra codes, planet codes and every label. No Sanskrit
sub-labels in v1. A later language switch replaces the language and its short forms as a whole.

## Layer toggles

Not checkboxes. Each layer is a chip whose icon previews what it adds: off is a dashed outline
with a muted icon, on is a solid outline with a filled icon; `aria-pressed` carries the state.
Choices of one-of-many (preset, theme) are segmented controls.

| Layer | UNI-1 default | UNI-2 default | UNI-3 |
|---|---|---|---|
| Benefic / malefic (corner tag + hover wash) | on | on | on |
| Ashtakavarga | on | on | — |
| Sign nature (line 1 glyphs) | on | on | — (glyph only) |
| Element colour (rail) | on | on | — |
| Nakshatras (line 2) | on | on | — |
| Planet pada | on | on | — |
| Degrees | on | on | — |
| Planet images | off | on | on (All Charts) |
| Houses from (dropdown) | Moon | Moon | — |
| Lens (Off / Aspects / Argala) | popover | Off | — |
| Karakas | — | off | icon toggle |
| Arudhas / special points | — | on | icon toggles (Spl Lagnas) |
| Aspects | — | off | — |

UNI-2 presets: **Calm** (benefic / malefic, sign nature, element colour, degrees), **Reading**
(default), **Everything**. UNI-3
toggles are icon-only with the name on hover / focus. The viewer's choices are remembered per
view the same way the theme is (`localStorage`, read defensively).

Toggles only show or hide content inside the square; they never move or resize the grid.

## Themes

Only theme tokens, no literals: `--brand-*` / `--cosmos-*` for surfaces, ink, lines and accent;
`--dignity-*`, `--planet-*`, `--ref-*`, `--house-lagna` for meaning colours (all have
cosmic-dark values in tokens.css since the 2026-10-01 theme audit). New tokens this component
needs: `--asc` (Lagna cell tint), `--watermark`, `--flame`, `--chip-tint` (13% light, 20% dark),
`--ben` / `--mal` / `--mix`, `--wash`, `--arg` / `--vir`, the `--sl-*` set above,
`--el-fire` / `--el-earth` / `--el-air` / `--el-water` (light: #c2410c #4d7c0f #a16207 #0e7490;
cosmic-dark: #fb923c #a3e635 #facc15 #22d3ee).

## Glyph font

Planet, zodiac and nature glyphs render differently per OS and some fall back to colour emoji.
Bundle Noto Sans Symbols (and Symbols 2 for 🜁–🜄) subset to the ~35 glyphs used, self-hosted in
`wwwroot/fonts`, and append U+FE0E after each glyph.

## Planet images

`wwwroot/images/planets/orb-*.png` — 256px transparent orbs built from NASA public-domain
photographs (sources and credits in `wwwroot/images/planets/CREDITS.md`). Rahu is the 2017 total
solar eclipse, Ketu a copper lunar-eclipse Moon. In the chart the orb is optional decoration
before the chip: a photo carries no dignity, so the chip stays the thing to read. Also suitable
for the strength and dasha tables and the planet picker.

## Interaction

A Micro chart is static (no pin, no centre card) unless the page binds `PinnedSign`; Key
Inference and the Spl Lagnas grid do, so there it pins and shows the sign card in the centre at
reading size. The lens (aspects / Argala) stays Compact and Reading only.

Keeps `SindHovGrid`'s contract: hover / focus previews a cell, click / Enter pins it, arrow
keys move, focused houses and relevant signs are highlighted, `PinnedSignChanged` /
`PreviewSignChanged` callbacks. The hover popover shows full degrees, nakshatra name and lord,
pada, dignity name and aspects received.

## Build status (workstream/ui, 2026-10-01)

Built: `SindUniGlyphs`, `SindUniModel` (`SindUniChart` + `SindUniBuilder`), `SindUniGrid`
(one renderer, toolbar, centre card, lens overlay, per-viewer prefs in `localStorage`
`ikiastrro-sinduni-{view}-{page}`), wrappers `SindUni1Grid` / `SindUni2Grid` / `SindUni3Grid`,
tokens, tests `tests/Ikiastrro.Web.Tests/SindUniTests.cs`. In use: Key Inference chart (UNI-3
with special lagnas, replacing `SindHovGrid` there) and Astro Facts → Spl Lagnas grid view (the
same UNI-3, rendered by `KarakaPolarWheel` in place of `PolarGridLagnaSelect`'s grid; the wheel
view is unchanged). Astro Facts natal and All Charts keep `SouthIndianGrid_Detailed`.
Not yet: the bundled glyph font, arrow-key movement between cells.

## Page links

The charts connect the two pages:

- Astro Facts → Key Inference: the natal chart toolbar's **KEY INFERENCE →** opens the chart on
  show (`KeyInferenceLink.Url(id, null, "LAGNA", chart)` → `/key-inference/{id}?chart=D9`); every
  All Charts card and the Spl Lagnas grid carry "Read D9 in Key Inference →".
- Key Inference → Astro Facts: the existing `AstroFactsLink` lines under each evidence section
  ("Planet positions for D9 in Astro Facts →", special lagnas, houses, aspects, strength).

## Build plan (workstream/ui)

| Piece | File |
|---|---|
| Cell model and chart-level inputs | `Components/Charts/SindUni/SindUniModel.cs` (`SindUniChart`, `SindUniSign`, `SindUniPlanet`, `SindUniLens`) |
| Builder from a loaded chart | `Components/Charts/SindUni/SindUniBuilder.cs` (KeyDetails, SAV, Argala, aspects, Raman verdicts) |
| Static tables (sign codes, nature glyphs, nakshatra codes, lagna colours) | `Components/Charts/SindUni/SindUniGlyphs.cs` |
| Shared renderer | `Components/Charts/SindUni/SindUniGrid.razor(.css)` with `View` = Compact / Reading / Micro |
| Named views | `SindUni1Grid.razor`, `SindUni2Grid.razor`, `SindUni3Grid.razor` (thin wrappers fixing `View` and defaults) |
| Toolbar | `Components/Charts/SindUni/SindUniToolbar.razor` (layer chips, presets, houses-from dropdown, lens, special-lagna chips) |
| Tokens | `wwwroot/css/tokens.css` (`--asc`, `--watermark`, `--flame`, `--chip-tint`, `--ben/--mal/--mix`, `--wash`, `--arg/--vir`, `--el-*`, `--sl-*`) |
| Glyph font | `wwwroot/fonts/` Noto Sans Symbols subset |
| Planet images | `wwwroot/images/planets/orb-*.png` (done) |

Rollout (revised 2026-10-01): UNI-3 with special lagnas on Key Inference and the Spl Lagnas grid
tab; Astro Facts stays on the master chart. The old grids stay until every consumer has moved.

## Extension to the polar wheel (later)

`PolarWheel`, `PolarGridLagnaSelect` (wheel view) and `Natal_Transit_Comp_WheelChart` should take
the same vocabulary so a reader moving between grid and wheel sees one language:

- planet marks as dignity chips with the bracket rule for retrograde; optional orbs;
- sign sectors carrying the element rail (outer ring segment), line-1 glyphs and the BEN / MAL /
  MIX tag with the same hover wash;
- the houses-from dropdown and the Lagna house in the inner ring;
- the lens drawn as chords between sectors (same `--arg` / `--vir` / accent colours, solid vs
  dashed);
- special lagnas as coloured ticks or arcs in the `--sl-*` colours, multi-select;
- the same toolbar component and presets.

A separate spec (`spec_SIND-UNI_PolarWheel.md`) will be written when that work starts.

## Data

Everything comes from `tbl_Chart_KeyDetails` (sign, degree, nakshatra, pada, retrograde,
combust, dignity, chara karaka, house from Lagna / Moon, arudhas, special lagnas, upagrahas),
`tbl_Chart_Aspects`, `tbl_Fact_SarvaAshtakavarga`, the Argala facts, and
`HouseBeneficMaleficCalculator`; nothing is computed in the component except the nakshatra-span of a sign
(9 padas from the sign index) and the house counts it already derives today.
