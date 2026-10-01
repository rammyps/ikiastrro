---
last_updated: 2026-09-24
status: research-baseline
---

# Key Inference — unified model, sourcing status, and open-question resolutions

Two unconnected things in this project are both called "Key Inference":

1. `tbl_Rule_LifeMatterReference` (migration 087) — 96 sourced rows bridging specific life
   questions (marriage, career, children, ...) to houses/karakas/vargas. Fully sourced, but no
   C# code reads it yet (see `life-matter-reference-pvr.md`'s "Downstream" section).
2. `FEAT-KARAKA-06`, the roadmap's chara-karaka drill-down panel (`res_charakarakas.md` §2) —
   blocked because the interpretive content for the 8 chara karakas has "no source content
   exists to seed them from" (`ROADMAP.md`).

These sit inside a wider, previously-unreconciled karaka/life-area data model. This doc lays
that whole model out in one place, records new book-sourced citations that resolve several of
the project's own open questions, and states the decisions taken (rammyps, 2026-09-24) so a
future migration/doc pass can act on them without re-deriving this research.

**This is design/citation input, not a migration** — same discipline as
`chara-karaka-interpretation-statistics.md` and `lifearea-varga-charakaraka-synthesis.md`, both
of which end "design input... not yet a migration." No SQL, no `LifeAreaMap.cs` edit, and no
edits to `res_charakarakas.md`/`chara-karaka-life-area-pvr.md`/`ROADMAP.md`/`masterproduct.md`
are made here — those are the natural next phase, scoped at the end of this doc.

## 1. The data-model map — 5 pieces, mostly unreconciled

| # | Table/file | Grain | Source | Status |
|---|---|---|---|---|
| Leg A | `tbl_Dim_LifeArea` (migration 30) | chart → area (PVR Table 11, 20 rows) | PVR | Seeded |
| Leg B | `tbl_Dim_DivisionalSubject` (migration 38) | subject → chart (12 rows), prose `D1Foundation` | PVR | Seeded; reconciled against Leg A by migration 134 |
| Leg C | Vargas (`tbl_Dim_ChartType`, D1–D60) | the shared join key across A/B/D | — | Structural, not a gap |
| Leg D | `tbl_Dim_KarakaRole` (migration 103) + `tbl_Rule_InterpretiveFactorDetail` (migration 109) | chara-karaka role → House/Planet/Sign/Varga | PVR (roles only) | Role resolution done; interpretive detail rows not yet seeded — **this doc's citations unblock that** |
| — | `tbl_Rule_LifeMatterReference` (migration 087) | specific matter → house(lagna)/house(karaka)/karaka/chart | PVR (+ project synthesis, 7/96 rows) | Seeded, sourced; no C# repository reads it yet |
| — | `LifeAreaMap.cs` (Core, static) | 4 coarse UI groups → houses/karakas/vargas | B.V. Raman, "not reconciled" per its own doc comment | Live in `VargaRail.razor`; a second, independent karaka list — see §3 below for its resolution |
| — | PVR Table 13 (chara karaka "person shown") | 8 karakas → who they represent | PVR | Seeded only as 2 rows (DK/PK) in `tbl_Rule_LifeMatterReference`; AK/AmK/BK/MK/PiK/GK not yet seeded as their own rows, despite being already-cited |
| — | Sthira karaka (PVR, pg 82) | fixed planet per relative, death-timing only | PVR | Zero DB presence — **this doc's citations unblock that**, see §3 |

## 2. Resolved sourcing questions

### 2.1 D-40/D-45 divisional-chart subject — worksheet claim does not check out

`chara-karaka-life-area-pvr.md` flagged a conflict: a worksheet gave D-40 Khavedamsa =
"maternal lineage" and D-45 Akshavedamsa = "paternal lineage," while PVR's own Table 11 gives
D-40 = auspicious/inauspicious events, D-45 = all matters. The worksheet's convention is a real,
commonly-cited claim in some Parashari-tradition literature, but wasn't traceable to this
project's own sources.

**Checked against BPHS directly** — `Brihad Parasara Hora Shastra - Santhanam's/
BPHS-Santhanam-Vol-1.pdf`, Ch. 6 (Shodasavarga), PDF p.91 / printed p.92:

> "...parents from Dvadasamsa, benefits and adversities through conveyances from Shodasamsa,
> worship from Vimsamsa, learning from Chaturvimsamsa, strength and weakness from Bhamsa, evil
> effects from Trimsamsa, **auspicious and inauspicious effects from Khavedamsa, and all
> indications from both Akshavedamsa and Shashtiamsa** — these are the considerations to be
> made through the respective Vargas..."

Restated as a numbered list a few lines later: "14. Khavedamsa for auspicious and inauspicious
effects. 15. Akshavedamsa for all general indications."

**Resolution:** BPHS's own text — the root source Parashari astrology derives from — matches
PVR's Table 11, not the worksheet. There is no support anywhere in BPHS's own Shodasavarga
chapter for "D-40 = maternal lineage" / "D-45 = paternal lineage." **Decision: drop the
worksheet's claim; PVR's existing D-40/D-45 rows stand, independently cross-validated by BPHS.**
A future migration seeding this cross-validation should use a new source code for the direct
BPHS citation (distinct from `SRC_PVR_INTEGRATED`, since this is a direct citation of BPHS
itself, not PVR's paraphrase of it).

### 2.2 The "chakra" concept — resolved, and narrower than assumed

`res_charakarakas.md` §4 speculated that each chara karaka has its own "chakra" (wheel), naming
convention `<Abbrev>_ChakraLord`, but flagged this as **entirely unsourced** — zero prior art
anywhere in the codebase or (as far as it knew) in any reference text.

**Checked against `Jaimini-Upadesa-Sutras-by-S.pdf`**, confirmed to be *Jaimini Maha Rishi's
Upadesasutras*, translation/commentary by Sanjay Rath (Sagar Publications) — a translation of
Jaimini's own root sutras, the highest possible tier of citation for this domain. Table 1.5
(p.17–18), titled **"Chara Karaka Chakra,"** is literally the term the project was looking
for — but it names the single *combined* table of all 8 resolved karaka-planets ("temporary
significator chart"), not a per-individual-karaka wheel with its own chakra lord. Every other
"chakra" term in the book (Varnada Chakra, Pataki Chakra, Janma Chakra, etc.) names a distinct,
different chart/diagram type — none is a per-karaka sub-chart.

**Resolution: the "8 separate `<Karaka>_ChakraLord` wheels" idea is retired.** `res_charakarakas.md`
§4 should be reframed around the existing combined-table concept (which the app may already
effectively have, since it already resolves all 8 karakas per chart) rather than built as
originally speculated.

### 2.3 Sthira karaka — three independent citations found, ready to seed

`chara-karaka-life-area-pvr.md` documented PVR's sthira-karaka list (fixed significator per
relative, used only for death-timing) but flagged that this project's `LifeAreaMap.cs` already
carries a *different*, Raman-sourced karaka list per UI life-area group that has never been
reconciled or properly cited.

PVR's existing list (pg 82):

```
Father: Sun or Venus (stronger)    Mother: Moon or Mars (stronger)
Mars: younger siblings             Mercury: maternal relatives
Jupiter: husband/sons/paternal grandparents    Venus: wife/in-laws/maternal grandparents
Saturn: elder siblings
```

**Citation 1 — Jaimini's own root sutras** (`Jaimini-Upadesa-Sutras-by-S.pdf`, sutras
1.1.20–1.1.24, p.11–12), the strongest of the three found:

> Mars = younger siblings; Mercury = maternal relatives; Jupiter = paternal grandparents /
> husband / children; Venus = wife / in-laws / maternal grandparents; Saturn = elder siblings;
> "the stronger between Sun & Venus represents father."

This is a **near-exact match** to PVR's list, independently arrived at from the root sutras
themselves, with an explicit Parasara cross-reference in the commentary.

**Citation 2 — B.V. Raman's `Jaimini-Astro-Studies-by-B-v-Raman.pdf`**, §17 "Minor Karakas"
(printed p.11): Mars = sisters/younger brothers/step-mother/brothers-in-law; Mercury = maternal
uncles/aunts; Jupiter = paternal grandparents; Venus = wife's parents/maternal grandparents —
agrees closely with PVR on the Mercury/Jupiter/Venus rows.

**Citation 3 — Rangacharya's *A Manual of Jaimini Astrology (Made Easy)*** (Sagar
Publications, 2007), p.28, a *partial* and *partly disagreeing* fourth citation: "Jaimini
Maharshi gives us only 4 fixed karakas... Mars is the fixed karaka for sister, wife's brother,
younger brother **and mother**. Mercury... uncles, relatives, step mother. ...Jupiter [for]
father's father, husband and son... Venus... wife, parents, mother-in-law, father-in-law and
mother's father." Only 4 of 7 relatives covered; its Mars=mother claim has no PVR equivalent
and is noted as a genuine, disagreeing variant rather than corroboration.

**Resolution / decision:** seed a new, properly-cited sthira-karaka table primarily from the
Upadesa Sutras citation (Citation 1, the strongest — root text, Parasara-cross-referenced),
with Citations 2 and 3 recorded as corroborating/partially-disagreeing secondary sources.
**Reconcile `LifeAreaMap.cs`'s separate Raman-sourced karaka list into the new cited table**
once it exists, rather than leaving two uncited/unreconciled lists side by side.

### 2.4 Chara-karaka scheme: Sapta (7) vs. Ashta (8) Karaka — a real, documented variance

While researching Leg D content (§3), two independent named sources turned out to use a
**7-karaka scheme with no Pitri Karaka**, not the 8-karaka scheme this codebase's
`CharaKarakaCalculator` computes:

- **B.V. Raman**, *Jaimini Astro Studies*, §16 "Exceptions" (printed p.8–9): discusses the
  7-vs-8 controversy directly — some commentators include Pitrukaraka (8, Sun-to-Rahu), others
  keep 7 (Sun-to-Saturn). Raman states: "the latter view appears more reasonable, as it has the
  support of Parasara," and uses 7 (Atma/Amatya/Bhratru/Matru/Putra/Gnathi/Dara) throughout the
  rest of the book, with Rahu only used as a longitude tiebreak, never ranked as a karaka
  itself.
- **Rangacharya**, *A Manual of Jaimini Astrology (Made Easy)*, p.28 (quotable): "The variable
  karakas are 7 in number. They are atma, amatya, bhratru, matru, Putra, jnati, dara." Longitudes
  taken from Sun, Moon, Mars, Mercury, Jupiter, Venus, Saturn only; Rahu explicitly excluded
  from becoming Atmakaraka.

This codebase's `CharaKarakaCalculator` (`src/Ikiastrro.Core/Engines/Karakas/CharaKaraka.cs`)
computes 8 (Ashta Karaka): ranks Sun through Saturn plus Rahu (reversed), excludes only Ketu,
and includes Pitri Karaka. This is already shipped and verified
(`FEAT-KARAKA-01`, `verify-jaimini`) — this doc does **not** propose changing it; Ashta Karaka
is also the more common convention in contemporary Jyotish software (JHora and others default
to it).

**Decision: keep the app's 8-karaka (Ashta) scheme unchanged; document the Sapta-vs-Ashta
variance inline** (in `res_charakarakas.md` or `chara-karaka-life-area-pvr.md`, in the future
doc-update phase) as the *reason* Pitri Karaka has no sourceable interpretive content below —
not a silent, unexplained gap, but a named classical fork the project has deliberately sided
with the more common modern convention on, while still being unable to source PiK-specific
content from sources that don't recognize it as a karaka at all.

## 3. Leg D interpretive content — citation coverage, all 8 karakas

Books checked (`D:\Vedic Astrology\Vedic Astology Books\`), against the project's citation bar
(page-numbered, quotable, named author/translator/publisher):

| Book | Verdict |
|---|---|
| `Light-on-Life.pdf` (de Fouw & Svoboda) | **Not usable.** Assumed to be a Jaimini-karaka deep-dive; is actually a general 12-chapter Parashari-tradition intro text. Jaimini named once in passing. Zero chara-karaka content. |
| `Jaimini-Astro-Studies-by-B-v-Raman.pdf` | **Usable — basic/definitional tier**, all 7 (of its own 7-karaka scheme) roles, plus two real behavioral statements for AK. |
| `Jaimini-Upadesa-Sutras-by-S.pdf` (*Jaimini Maha Rishi's Upadesasutras*, tr. Sanjay Rath) | **Usable — strongest source**, especially for AK (sutra + commentary); real but thin for AmK/BK/DK; rank-definition only for MK/PiK/PK/GK. Root-text tier, above secondary commentators. |
| *A Manual of Jaimini Astrology (Made Easy)* (Rangacharya) | **Usable, narrowly** — extensive Atmakaraka-specific content (karakamsa sign/house-placement rules, Ch.3), nothing for the other 6 recognized karakas. |

Combined per-karaka coverage:

| Karaka | Tier | Sources | Representative citation |
|---|---|---|---|
| **AK** | Strong | Rath, Raman, Rangacharya | Rath, sutra 1.1.11 + commentary (p.9): "enjoys a pre-eminent position... representsdivinity... controller of all the affairs of the native... like a king." Raman (p.7): "the strength or weakness of the Atmakaraka seems to reflect the general strength or weakness of the entire horoscope." Rangacharya Ch.3 (pp.29–31): extensive karakamsa-sign and house-from-karakamsa result tables. |
| **AmK** | Moderate | Raman, Rath | Rath (p.74): "the sixth house from Amatyakarak" shows Karmayoga — "the sustainer in the present world." Raman (p.7): definitional, natural AmK assumed Mercury. |
| **BK** | Moderate | Raman, Rath | Rath (p.74): "shows Bhakti or the undying attachment of the heart." Raman (p.7): definitional, naisargika BK Mars. |
| **DK** | Moderate | Raman, Rath | Raman (p.8): definitional, natural Dara/Kalatrakaraka Venus. Rath: recurs through the spouse-death-timing ("Sthira darakarak") technique, p.8786, thin standalone temperament content. |
| **MK** | Thin | Raman only | Raman (p.8): "lord of mother... natural karaka Moon." No behavioral statement found. |
| **PK** | Thin | Raman only | Raman (p.8): "lord of children... natural indicator Jupiter." No behavioral statement found. |
| **GK** | Thin | Raman only | Raman (p.8): "lord of relations/cousins... natural karaka Mars." No behavioral statement found. |
| **PiK** | **None** | — | Not a recognized karaka in 2 of the 4 sources checked (Sapta-karaka scheme, §2.4); no content found in any book. |

**Decision: seed all 7 sourced roles now**, at their stated confidence tier — not just the
4 with 2+ independent sources. This mirrors the project's own established `BasisCode` pattern
(migration 087 already mixes `PVR_DIRECT`/`PVR_CROSSVALIDATED`/`PROJECT_SYNTHESIS` tiers within
one table), so a "Thin/single-source" tier for MK/PK/GK is consistent practice, not a new
standard. **PiK stays explicitly unsourced** — no content is fabricated for it — with the
Sapta-vs-Ashta scheme note (§2.4) explaining why, once that gets written into
`res_charakarakas.md`.

## 4. PVR Table 13 — "person shown," the other 6 karakas

Separately from the behavioral content in §3: PVR's Table 13 (ch. 8, pg 80–81) gives the
*person represented* by each of the 8 chara karakas — already fully cited and already used for
2 of the 8 (DK→"Individual spouse-role," PK→"Particular child-role," both `PVR_DIRECT` rows in
`tbl_Rule_LifeMatterReference`). The other 6 (AK/AmK/BK/MK/PiK/GK) are not yet seeded as their
own rows anywhere, despite the source already being in hand:

```
AK  Atma Karaka    — Self
AmK Amatya Karaka  — Ministers/advisors
BK  Bhratri Karaka — Siblings
MK  Matri Karaka   — Mother
PiK Pitri Karaka   — Father
GK  Jnaati Karaka  — Rivals/enemies (footnote: literally "paternal cousin")
```

Worth seeding alongside the §3 content in the same future migration pass — it's independent of
the behavioral-content sourcing question (already PVR-cited, just not yet in a table) and
completes the picture Table 13 already provides in full.

## 5. Operational note for future book research

Several PDFs in `D:\Vedic Astrology\Vedic Astology Books\` have a corrupted text layer —
`pdftotext -layout`/PyMuPDF `get_text()` returns scrambled characters (not a tool
misconfiguration; the underlying font/CMap is broken). Workaround: render the specific needed
pages as images (PyMuPDF `page.get_pixmap()`, no poppler dependency needed) and read them
visually instead of extracting text. Always try `pdftotext -layout` on a handful of pages
first — it's far cheaper when the text layer is intact (most of the library's PDFs are fine;
`Light-on-Life.pdf` was the specific one found broken during this pass).

## 6. Not done in this pass — next phase

- **No SQL migration written.** A future `workstream/database` migration should: (a) seed the
  sthira-karaka table (§2.3, Upadesa Sutras primary citation), (b) seed Leg D interpretive rows
  for the 7 sourced chara-karaka roles into `tbl_Rule_InterpretiveFactorDetail` (§3), tiered by
  confidence, (c) seed the remaining 6 PVR Table 13 "person shown" rows into
  `tbl_Rule_LifeMatterReference` (§4), (d) add a new source citation for the direct BPHS
  reference (§2.1), distinct from `SRC_PVR_INTEGRATED`.
- **No `LifeAreaMap.cs` edit.** Reconciling it against the new sthira-karaka table (§2.3) is
  `workstream/cli` follow-up work, once the table exists.
- **No edits to `res_charakarakas.md`, `chara-karaka-life-area-pvr.md`, `ROADMAP.md`, or
  `masterproduct.md`.** These should be updated once the migration above lands: retire
  `res_charakarakas.md` §4's per-karaka chakra-wheel plan (§2.2), add the Sapta-vs-Ashta scheme
  note (§2.4), and move `FEAT-KARAKA-06` off "blocked" in `masterproduct.md`/`ROADMAP.md` to
  reflect that a first, honestly-tiered pass is now unblocked.
- The separate "Key Inference" UX picker (linking a user's question to a Astro Facts flow) is
  unaffected by this doc and remains a later, separate implementation task.
