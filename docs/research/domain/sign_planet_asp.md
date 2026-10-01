---
last_updated: 2026-09-22
reflects: Core, D1/D9/D10 persistence, and first Astro Facts Relationships UI slice implemented
---

# Sign and planetary aspects

Deferred implementation note for connecting Rasi drishti, Graha drishti, conjunction,
combustion, and dispositor chains in **Astro Facts**. No implementation is included here.

## Intended Astro Facts structure

Rename `2.1 ABOUT HOUSES` to `2.1 RELATIONSHIPS`, use one shared varga selector, and provide:

1. **GRAHA DRISHTI** — discrete Parashari aspects plus sphuta strength percentages.
2. **RASI DRISHTI** — sign-to-sign aspects and the bodies carried by those signs.
3. **CONJUNCTION & COMBUSTION** — existing calculations as relationship evidence.
4. **DISPOSITORS** — sign lord, full chain, terminal dispositor, mutual reception, and cycles.

Preserve the selected body across views. Connect the evidence as:

`body -> occupied sign -> dispositor chain -> incoming/outgoing drishti -> conjunction/combustion`.

## Discrete aspect versus percentage strength

| Result | Question answered | Shape |
|---|---|---|
| Discrete Graha drishti | Does the planet cast a recognized whole-house aspect? | Boolean relationship and aspect ordinal |
| Sphuta drishti strength | How strong is the longitude-based aspect at this exact point? | `0.00%` to `100.00%` |

A non-zero sphuta percentage is not automatically a recognized discrete aspect.

### PVR rule for Rahu and Ketu

`SRC_PVR_INTEGRATED` section 10.2 defines the baseline Parashari rule:

- all planets have the 7th-house Graha drishti;
- Mars additionally has the 4th and 8th;
- Jupiter additionally has the 5th and 9th;
- Saturn additionally has the 3rd and 10th;
- Rahu and Ketu do **not** receive special 5th or 9th discrete aspects in this rule set.

Therefore, label a non-zero Rahu/Ketu percentage at a trinal point as partial sphuta strength,
not as a "Rahu/Ketu 9th aspect". Rasi drishti remains a separate sign-based relationship.

## Sphuta Graha-drishti calculation

The Ramakrishnan JHora export (`SRC_JHORA_EXPORT_RAMAKRISHNAN`) is the golden fixture.
PyJHora's `planet_aspect_relationship_table` and `__drik_bala_calc_1` (`SRC_PYJHORA`) are the
open-source calculation reference; the two-decimal JHora export is the final parity authority.

For aspecting longitude `A` and aspected longitude `B`:

```text
directedSeparation = (B - A + 360) mod 360
percentage = min(totalVirupas, 60) / 60 * 100
```

The total contains the ordinary graduated contribution and the applicable graduated special
contribution for Mars, Jupiter, or Saturn. The special contribution is neither a fixed bonus
nor a replacement for the ordinary contribution. Preserve full precision until the final
two-decimal percentage. Display exact zero as an em dash.

Fixture example, Mars to the fourth from Lagna:

```text
directed separation ~= 86.714 degrees
ordinary contribution ~= 41.714 virupas
Mars fourth-aspect contribution ~= (86.714 - 60) / 2 = 13.357 virupas
total ~= 55.071 virupas
strength ~= 91.79%
```

## Scope of aspected bodies

- Lagna and twelve Lagna-relative points.
- Sun through Ketu and their twelve relative points.
- Maandi, Gulika, special lagnas, sphutas, Arudha Lagna, A2-A11, and UL.
- V2-V12 and other persisted special points when their longitudes are available.

Started with D-1 and the Ramakrishnan fixture; extended to every other registered varga
2026-09-24 by feeding each chart's own stored longitudes through the same calculator — never
shows reused D-1 percentages under a different varga label (`verify-graha-drishti` checks this
per chart type).

## Implementation slices

1. **Implemented:** pure Core `GrahaDrishtiStrengthCalculator` returns separation, ordinary
   virupas, special virupas, capped total, percentage, and discrete-aspect metadata.
2. Reproduce the Ramakrishnan fixture to `+/-0.01%`, including special-aspect boundaries and
   Rahu/Ketu cases.
3. **Implemented:** results persist in `tbl_Fact_GrahaDrishtiStrengths` for every registered
   chart type (D1..D60, widened from D1/D9/D10 2026-09-24 — same independent-per-varga
   recompute pattern as Ashtakavarga), carrying chart/rule provenance, input longitudes, and the
   calculation breakdown.
4. **Implemented:** `vw_ChartGrahaDrishtiStrengths` exposes the evidence; existing discrete
   `ChartAspect` facts remain canonical and separate.
5. **Implemented (first UI slice):** relationship summary and strength matrix with sticky
   identity columns, heat colouring, focus-body selection, and cell evidence detail, for whichever
   varga the shared Astro Facts chart picker selects.
6. Connect selected results to Rasi drishti, conjunction, combustion, and dispositor chains.
7. **Implemented 2026-09-24:** each varga's rows are recomputed independently from that varga's
   own longitudes (not reused D1 percentages) and `verify-graha-drishti` confirms every
   dropdown/chart-type change end-to-end.

## Acceptance criteria for later implementation

- Discrete aspects and percentage strength are visually and semantically distinct.
- Rahu/Ketu have only the 7th discrete Graha drishti under the PVR rule set.
- Rahu/Ketu may show partial sphuta strength elsewhere without being mislabelled.
- The Ramakrishnan D-1 fixture matches to `+/-0.01%`.
- Each percentage exposes its inputs and ordinary/special virupa breakdown.
- Changing the varga recalculates from that varga's positions.
- Relationship evidence links to the selected body's complete dispositor chain.

## Sources

- `SRC_PVR_INTEGRATED` — P. V. R. Narasimha Rao, *Vedic Astrology: An Integrated Approach*,
  sections 10.2 and 10.3.
- `SRC_JHORA` — Jagannatha Hora v8.x percentage relationship-table behavior.
- `SRC_JHORA_EXPORT_RAMAKRISHNAN` — supplied Ramakrishnan D-1 aspect-strength export.
- `SRC_PYJHORA` — PyJHora `strength.py` and `house.py`; reference only where it differs from
  the JHora golden export.
