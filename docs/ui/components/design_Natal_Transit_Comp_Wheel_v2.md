---
last_updated: 2026-09-11
workstream: natal-wheel-v2
component: Natal_Transit_Comp_Wheel
status: experimental
---

# Design — Natal Transit Comparison Wheel v2

## Intent

Make the natal chart readable at a glance without turning the wheel into a report. Geometry answers
where; short labels answer what; deeper evidence remains in the tables.

## Information hierarchy

1. Twelve sign sectors and Ascendant-relative orientation.
2. Natal planet glyph and radial degree-notch alignment.
3. Chara Karaka map in the center.
4. Compact Date → Maha → Antar → Pratyantar controls.

## Visual treatment

- Preserve the current Manrope/token palette and circular four-ring construction.
- Use full radial sign rules from the core edge through the natal ring.
- Place glyph, two-letter code, and full sign name outside the circle as one midnight-blue label.
- Planet glyph remains the strongest mark. Numeric degrees are omitted; a midnight-blue notch on
  each planet circle aligns to an inner 0°/10°/20°/30° scale.
- Only Exalted, Moolatrikona, and Own Sign receive dignity labels to control density.
- The center uses a maximum of four short rows, two Karaka pairs per row, leaving the date/time clear.
- Display only month/year in the date and all dasha ranges. Keep the controls one thin line on wide
  screens and wrap naturally on narrow screens; labels and option
  text remain compact, with no separate cards or explanatory copy.

## Fallback and release

This design lives only on `workstream/natal-wheel-v2`. The existing master implementation is the
fallback. Merge only after visual comparison at desktop and narrow widths plus seeded-data checks.
