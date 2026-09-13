---
last_updated: 2026-09-11
workstream: natal-wheel-v2
component: Natal_Transit_Comp_Wheel
route: /transit-wheel/{id}
status: experimental
preview_route: /transit-wheel-v2/{id}
---

# Specification — Natal Transit Comparison Wheel v2

This is an isolated successor to `spec_Natal_Transit_Comp_Wheel.md`. The existing specification
and implementation remain the fallback until this workstream is reviewed and merged.

## Scope

Split the comparison experience into independently readable **Natal** and **Transit** modes. This
increment implements the Natal presentation while preserving the existing persisted-data contract.

## Natal wheel contract

- Rotate the zodiac so the stored Ascendant sign begins at wheel longitude 0° (top).
- Draw complete radial boundaries through the natal ring for all twelve 30° signs.
- Label every sign outside the circle with its zodiac glyph, two-letter code, and full name:
  Aries through Pisces. The complete label uses the standard midnight-blue token.
- Keep each planet on its true longitude, expressed relative to the rotated Ascendant-sign origin.
- Do not print numeric degrees beside planets. Each natal planet circle carries a short
  midnight-blue radial notch aligned to the innermost 0°/10°/20°/30° sign scale.
- For Exalted, Moolatrikona, or Own Sign placements, show a compact dignity code (`Ex`, `MT`,
  `Own`). Exalted placements also show the rule table's `DeepDegree` as `peak n°`.
- Mercury follows the persisted dignity segmentation: Virgo 0°–15° Exalted, 15°–20°
  Moolatrikona, and the remaining own-sign segment as Own.
- Keep deterministic radial collision lanes; rotation and added labels must not change the true angle.
- Show the eight Chara Karakas in the core as compact pairs such as `AK–Sa · AmK–Ve`, ordered
  AK, AmK, BK, MK, PiK, PK, GK, DK. Ketu and Lagna are excluded.

## Compact time controls

One compact strip contains month/year, Mahadasha, Antardasha, and Pratyantardasha (level 3).
All displayed date and period boundaries use MM/yyyy. Changing any
control updates the single shared active instant. Date selection resolves all three active levels;
selecting a parent resets its descendants to the period active at the parent's stored start.

## Data contract

Natal positions come from `vw_ChartPlanetEvidence` D1 rows. The focused repository additionally
reads `DignityStatus`, `CharaKaraka`, and the active rule set's exaltation `DeepDegree` from
`tbl_Rule_GrahaDignity`. The UI performs formatting and rotation only; it does not calculate dignity.

## Acceptance

- Ascendant sign boundary is at the top and all twelve sign sectors are visibly closed.
- Glyph, two-letter code, and full sign name remain legible at supported responsive widths.
- Planet notches align correctly against the repeated sign-degree scale, including Rahu/Ketu.
- Center Karaka pairs match persisted D1 assignments.
- Date and all three dasha selectors remain synchronized.
- Existing transit positions, tables, and fallback design continue to build and test.
