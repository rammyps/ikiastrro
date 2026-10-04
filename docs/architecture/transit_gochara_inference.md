---
last_updated: 2026-10-04
workstream: architecture
togaf: C — application (engine in Core, panel in Web)
reflects: built on master (slice 1) — GocharaInferenceBuilder, GocharaInferencePanel, GocharaInferenceTests.
---

# Transit inference, slice 1: Gochara verdicts

Status: accepted 2026-10-04 and built on local master. Decisions in section 10.

Today the Transit tab shows nine rows of facts from the natal Moon: house, Reads-as, Vedha, bindus. It does not say what they add up to. Slice 1 adds a reading of those rows: a per-planet net tier, a headline for the slow planets, and when each slow planet next changes. It uses only data the repo already has.

Out of scope, and left as hooks: natal promise, dasha activation, event-specific triggers (marriage, career, health), Kakshya, transit-to-natal aspects. They are slices 2 and later (section 9).

## 1. What exists

| Piece | Where | What it gives |
|---|---|---|
| `GocharaReading.Read` | `Ikiastrro.Core/Engines/Transits` | Per planet: sign, house from Moon, `Verdict` (Good, Blocked, NotFavourable), Vedha obstructors, own D1 bindus, Saturn phase |
| `GocharaVedhaCalculator` + `tbl_Rule_GocharaVedha` (migration 158) | Core and db | PVR ch.26.3 Table 63, with the Sun-Saturn and Moon-Mercury exceptions. Rahu is read by Saturn's row and Ketu by Mars's (ch.25) |
| `StrengthBands.BhinnaAshtakavargaBindus` | Core | Banding of own BAV bindus (PVR: 5 or more good, 3 or fewer bad) |
| `GocharaTable`, `SaturnFromMoonPanel` | Web | The table and the Saturn phase windows |
| `tbl_PlanetSignTransitEvents`, `tvf_PlanetSignAtDate` | db | Sign ingress dates for Saturn, Jupiter, Rahu (Ketu derived) |

Correction: `docs/research/domain/gochara-vedha-pvr.md` still shows Table 63 with the OCR-shifted labels (the Sun label sits on the header line, so every row name is one line too high, and the Saturn row 3 (12), 6 (9), 11 (5) is unlabelled). Migration 158 already realigns it, and the "no Sun row, no Saturn row" gap noted there is closed. Fix that doc as part of this slice.

## 2. Principles

1. **Gochara is secondary.** PVR and Raman both judge a transit against the natal promise and the running dasha. Slice 1 has neither, so every output says it is a Moon-reference reading only, and it never states an event.
2. **Verdicts stay as PVR gives them.** `GocharaVerdict` is not changed. The net tier below is a separate, labelled derived layer.
3. **Heuristics are labelled.** PVR gives no aggregate score. Anything that combines verdict and bindus, or weights planets, is a project heuristic. It carries that label in the UI and in code, as with Vimśopaka bands.
4. **Pure, deterministic, no stored output.** The engine is a pure function of the natal Moon sign, the transit signs, the rules and the bindus. Nothing is persisted, and the panel follows the date and dasha picker.
5. **Sentences come from structure.** There is no free prose from books in slice 1. Each line is built from the row's own facts.

## 3. The net tier (heuristic)

Per planet, combine the PVR verdict with the bindus band. The bindus are today shown beside the verdict and not counted (see the `GocharaTable` note). The tier is the one place they are combined, and the table keeps showing both raw inputs.

| Verdict | Bindus 5 or more | Bindus 4 | Bindus 3 or fewer | No BAV (Rahu, Ketu) |
|---|---|---|---|---|
| Good | Supportive | Mild | Mixed | Mild |
| Blocked | Obstructed | Obstructed | Obstructed | Obstructed |
| NotFavourable | Mixed | Unfavourable | Unfavourable | Unfavourable |

Notes:
- Blocked stays Obstructed whatever the bindus, because PVR says the planet "cannot give its good results".
- A strong bindus count does not rescue an unlisted house, so NotFavourable with 5 or more bindus is Mixed, not Mild.
- Rahu and Ketu use the verdict only, since they have no BAV.
- The 5 and 3 cut-offs come from `StrengthBands`. They are not redefined here.

## 4. The inference record

```csharp
enum GocharaTier { Supportive, Mild, Mixed, Obstructed, Unfavourable }

record GocharaPlanetReading(GocharaRow Row, GocharaTier Tier, string Line);

record GocharaSlowChange(PlanetName Planet, DateTime NextIngressUtc, ZodiacName NextSign,
    int NextHouseFromMoon, GocharaVerdict VerdictThen);   // verdict ignoring Vedha, the future is unknown

record GocharaInference(
    SaturnPhase Saturn,
    IReadOnlyList<GocharaPlanetReading> Slow,     // Saturn, Jupiter, Rahu, Ketu: the long-running influences
    IReadOnlyList<GocharaPlanetReading> Fast,     // Moon, Venus, Mars, Mercury, Sun: days to weeks
    IReadOnlyList<GocharaSlowChange> Changes,     // slow planets only, from tbl_PlanetSignTransitEvents
    IReadOnlyList<string> Qualifiers,             // always empty in slice 1; slice 2 adds natal and dasha lines
    string Caveat);
```

Engine: `GocharaInferenceBuilder.Build(GocharaReadingResult, ingressLookup)` in `Ikiastrro.Core/Engines/Transits`, beside `GocharaReading`. It is pure.

`VerdictThen` on a change is the verdict ignoring Vedha, because tomorrow's other planets are not known. It is labelled "before Vedha".

## 5. What the panel says

The panel is `GocharaInferencePanel`, placed above `GocharaTable` on the Transit tab. It follows the same date and dasha selector.

```
GOCHARA READING · from the natal Moon (Capricorn) · 04 Oct 2026
Moon-reference only. It confirms or tempers the natal promise and the dasha; it is not a prediction.

SLOW (months to years)
  Saturn   Pisces, 3rd    Supportive   Good house, bindus 5/8
           Next: enters Aries (4th, Kaṇṭaka Śani) on 02 Jun 2027
  Jupiter  Cancer, 7th    Unfavourable ...
  Rāhu (as Saturn) ...    Ketu (as Mars) ...
FAST (days to weeks) ▸ expand
```

- Slow planets get a full line each. Saturn is always first and carries the phase flag.
- The fast planets are not in the main reading; an expand control lists the five rows.
- "Next" lines come from the ingress events. For retrograde re-entries the lookup returns the next event after the date, whatever its direction.
- Tier uses the existing status colours (strong, moderate, weak, neutral). Obstructed is hatched, matching the Aṣṭama look, so tiers are not carried by colour alone.
- The caveat line sits at the top, not in the footer.

There is no single overall score and no "good day or bad day" headline. Section 8 explains why.

## 6. Data and code

No migration in slice 1.

| Item | Change |
|---|---|
| `GocharaInferenceBuilder`, `GocharaTier`, records | New, `Ikiastrro.Core/Engines/Transits/GocharaInference.cs` |
| Ingress lookup | `PlanetSignTransitEventRepository` gains `NextIngress(planet, afterUtc)`. Check first whether `TransitStrengthRuleRepository` or the Gochara repository already exposes it |
| `GocharaInferencePanel.razor` (+ scoped css) | New, in `Components/Charts`, fed by `AstroFacts.GocharaJudgement` |
| `astro-facts.md` and `pvr-coverage.md` | Update the Part 3 row. Fix `gochara-vedha-pvr.md` (section 1) |

Branch: UI work on `workstream/ui`. The engine and tests could go on the same branch, since there is no db change.

## 7. Tests

Extend `GocharaReadingTests` with a new `GocharaInferenceTests`:
- Tier matrix: one case per cell of section 3, including the Rahu and Ketu no-BAV column.
- Blocked beats strong bindus: Obstructed with 7 bindus.
- PVR's own Bill Gates example (Moon in Pisces, Mercury in Gemini, 4th from Moon, Vedha house Taurus) as a fixture. It is already used in `GocharaReadingTests`.
- Slow and Fast grouping, and Saturn first.
- `Changes`: next ingress after a date, the retrograde re-entry case, and Ketu derived from Rahu.
- GowriShankarC fixture: Moon in Capricorn, so on 2026-10-04 Saturn is in Pisces, the 3rd, with no phase. On 2024-01-01 Saturn is in Aquarius, the 2nd, Sāḍe Sātī setting.

## 8. Alternatives considered

- **One overall score.** Rejected. PVR gives no weights, a number would imply precision the method does not have, and it would hide the Obstructed case, which is the interesting one.
- **Count bindus into the verdict itself.** Rejected. The 2026-10-01 decision kept bindus beside the verdict. The tier is a separate derived layer, so the PVR verdict stays citable.
- **Store daily readings.** Rejected. Transits are cheap to compute and the picker already drives them. Persisting would need invalidation when rules change.
- **Add event text (marriage, career) now.** Deferred. That needs the natal promise and dasha qualifiers, or it becomes the generic "Jupiter good" flag `transit-events.md` warns against.

## 9. Later slices

1. **Qualifiers (built 2026-10-04):** `GocharaQualifierBuilder` (Core) qualifies each slow planet against the natal D1 and the running dasha. For the natal house its sign is (whole-sign from the Lagna) it reads the house lord, natal occupants, and a *promise* label from the lord's Ṣaḍbala band and the house's independent Bhava Bala z-band (Promised: neither weak; Weak: both weak; Mixed otherwise — a project heuristic); for the planet itself its lordships, natal house and Ṣaḍbala band; and for each running Mahā/Antar/Pratyantar lord whether it is the transit planet, rules the house crossed, or sits natally in the sign crossed. Linked planets are flagged Dasha-linked; unlinked ones are background, not unimportant. The panel shows a Promise chip, a Dasha-linked chip and a "Natal house and dasha" expander per slow planet, plus a headline naming the running dasha. Fast planets are not qualified.
2. **Event triggers:** Jupiter over or aspecting the 7th house, 7th lord or Venus for marriage, with the rule table in `workstream/database`.
3. **Degree-aware transits:** Kakshya, nakshatra of transit planets, Sodhya Pinda. This needs the full per-graha transit observation row noted as a gap in `transit-events.md`.
4. **Forward timeline:** dates over the next N years when slow-planet tiers change.

## 10. Decisions (rammyps, 2026-10-04)

1. Bindus feed into the tier (section 3 as written).
2. The five fast planets are excluded from the main reading and sit behind an expand control; there is no fast summary line.
3. The UI does not cite PVR for the bindus cut-offs.
4. Built directly on local master, not a workstream branch.
