---
last_updated: 2026-10-04
workstream: architecture
togaf: C — application (engine in Core, panel in Web)
reflects: proposal on master 460a1eb, nothing built yet. Slice 3 of docs/architecture/transit_gochara_inference.md §9.
---

# Transit inference, slice 3: event triggers

Status: proposal. Decisions needed are in section 8.

Slices 1 and 2 read the Gochara from the natal Moon and qualified each slow planet by the natal house it crosses and the running dasha. Slice 3 asks the next question: which life matters is a transit touching right now, and is the dasha behind it? Marriage is the textbook case. It is one matter among 138 that the repo already models.

## 1. The rule, from PVR

PVR ch.25.3 gives one general rule, not a list of events:

> A planet occupying or aspecting a rasi in transit influences the matters signified by the houses and planets stationed in that rasi in the natal chart.

It then gives examples that all fit that rule. Jupiter in the 7th house, or aspecting the 7th house, the 7th lord's sign, or Venus's sign, may give marriage (Example 104 checks it on a wedding date). A malefic over the 5th lord's sign in the 8th is a children caution. Saturn in A10 is bad for career. Transit planets near a Saham (Vivaha, Kali) matter. Raman adds that all of this is secondary to the natal promise and the dasha.

So slice 3 needs no per-event rule table. It applies the one rule to the targets each life matter already declares.

## 2. What exists

| Piece | Where | Use |
|---|---|---|
| 138 life matters, 152 focus rows | `tbl_Dim_LifeMatter`, `tbl_Rule_LifeMatterFocus` (House or SpecialPoint, priority) | The matter → house list, and special points |
| Karaka rules per matter | `LifeMatterKarakaRule` (Core) | The matter's karakas (Venus for the spouse, and so on) |
| `LifeMatterFocusResolver` | Core | Joins subject, karaka and focus rules per matter |
| `RelationshipEngine.AspectsSign(planet, fromSign, toSign)` | Core | The same whole-sign aspect table the natal chart uses (7th for all, Mars 4/8, Jupiter 5/9, Saturn 3/10; Rahu and Ketu 7th only under the PVR rule set) |
| Slice 2 `GocharaNatalContext` and `GocharaDashaLords` | Core | House lords, natal signs, strength bands, running dasha |
| Punya Saham only | `PunyaSahamCalculator` | Vivaha and Kali Sahams are **not** computed yet |

## 3. Engine

`TransitTriggerBuilder.Build(natal, dasha, transitSigns, matters)` in `Ikiastrro.Core/Engines/Transits`. Pure.

For each life matter M that has focus rows:

1. **Targets.** Resolve M to natal targets, each with a natal sign:
   - each focus house (from the Lagna): its sign;
   - each focus house's lord: the sign that lord sits in;
   - each karaka graha of M: its natal sign;
   - natal planets sitting in a focus house's sign (they are the sign's occupants, so they come with the house).
2. **Contacts.** For each transit planet P in sign S, find the targets whose sign P occupies, or aspects from S via `AspectsSign`. Each contact records `(Target, Contact = Occupies | Aspects, Planet)`.
3. **Dasha support.** The matter is dasha-supported when a running Mahā/Antar/Pratyantar lord is the lord of a focus house, a karaka of M, or sits in a focus house. This is the same idea as slice 2's links, applied to the matter.
4. **Promise.** The focus house's promise from slice 2 (`PromiseOf(lord band, house band)`), the weakest across the matter's houses.

Output per matter with at least one contact:

```csharp
record TriggerContact(string TargetLabel, ZodiacName TargetSign, TargetKind Kind,   // House, HouseLord, Karaka
                      PlanetName Planet, ContactKind Contact);                      // Occupies, Aspects
record MatterTrigger(LifeMatterRef Matter, IReadOnlyList<TriggerContact> Contacts,
                     bool DashaSupported, IReadOnlyList<DashaLink> Links, GocharaPromise Promise,
                     TriggerStrength Strength, string Line);
enum TriggerStrength { Background, Supported, Strong }
```

`TriggerStrength` is a labelled heuristic, not a score:
- **Strong:** dasha-supported and the house promise is Promised.
- **Supported:** dasha-supported, or two or more distinct slow planets in contact, with the promise Promised or Mixed.
- **Background:** everything else, including a Weak promise. The UI still shows it, collapsed.

## 4. Planet nature, and wording

The line states the contact and the planet's natural nature (Jupiter benefic; Saturn, Rahu and Ketu malefic, the natural classification already used for houses). It does not say "marriage will happen". Example:

> Marriage and spouse — Jupiter (benefic) aspects Taurus, your natal 7th house, and Virgo, where the 7th lord sits. Antardaśā lord Venus is the spouse karaka. House promised.

Malefic contacts use the same pattern with a caution tag, for example "Saturn (malefic) occupies the sign of the 5th lord". The caveat from slice 1 stays at the top of the panel.

## 5. UI

A new `TransitTriggersPanel` under the Gochara reading on the Transit tab. It follows the date and dasha selector.

- Matters with contacts, grouped by life-matter category, Strong first, then Supported.
- Each row: the matter, a strength chip, the contacts as short lines, the dasha links.
- Background matters sit behind an expand control, as the fast planets do.
- A filter chip row for category, so "Marriage and spouse" is one click.

## 6. Data and tests

No migration. Reads the existing life-matter tables and slice 2 context. A small repository method returns the matters with their houses and karakas for rule set 1.

Tests:
- Example 104 from PVR as a fixture: Scorpio Lagna, Jupiter in Pisces aspecting Virgo and Cancer, Mercury 7th lord in Cancer, Venus in Leo.
- Occupy versus aspect for each of Jupiter, Saturn, Rahu and Ketu, including the nodes' 7th-only aspect.
- A matter with two focus houses gets contacts from both.
- Dasha support from a house lord, a karaka and an occupant.
- Strength ladder, including a Weak promise forcing Background.
- GowriShankarC on 2026-10-04 as a regression fixture.

## 7. Not in this slice

- **Sahams (Vivaha, Kali, others):** PVR names them as sensitive points. Only Punya Saham is computed, so a Saham table and calculator are a separate step, then added as `SpecialPoint` targets. Marriage is the main loser until then.
- **Arudhas (A10, A9):** targets exist natally, but the transit contact needs the arudha signs in the context.
- **Divisional charts (transit Rasi on natal D9, D10):** PVR §25.4, a different layer.
- **Degree-aware contact (orb, exact conjunction), Kakshya, Sodhya Pinda:** need the full per-graha transit position row.
- **Fast planets (Sun, Moon, Mars, Mercury, Venus) as triggers:** see decision 1.

## 8. Decisions needed

1. **Which planets trigger?** Slow four only (Saturn, Jupiter, Rahu, Ketu), or also Mars? PVR's marriage example uses Mercury and Venus transits near Saham, but those need degrees. Recommended: slow four now, Mars later with degrees.
2. **Which matters?** All 138 with contacts, grouped, or a curated shortlist (marriage, career, children, health and accidents, wealth, property, foreign travel, education) shown first with the rest behind an expand control? Recommended: all, with the shortlist pinned first.
3. **Strength ladder in section 3:** acceptable as a labelled heuristic?
4. **Should I add the Vivaha and Kali Saham calculator next**, so marriage and accident triggers are complete?
