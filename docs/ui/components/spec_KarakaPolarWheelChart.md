---
last_updated: 2026-09-15
reflects: PolarGridLagnaSelect implementation
component: PolarGridLagnaSelect
route: /key-inference/{id}?step=karakas
---

# Polar/grid Lagna selector

## Purpose

Present the selected divisional chart from one of five special-Lagna counting references without Karaka overlays or a detail table. The component supports two interchangeable chart views: Polar-Wheel and SouthIND-Grid.

## Page contract

- Heading: **Special Lagnas & Karakas**.
- Polar-Wheel and SouthIND-Grid controls sit to the right of the heading and switch the main visualization.
- The divisional-chart selector is centered on the next line.
- The Lagna choices are checkbox-styled, single-select controls in this order:
  1. Sign Lagna (default)
  2. Arudha Lagna
  3. Hora Lagna
  4. Sree Lagna
  5. Ghati Lagna
- Only references available for the selected chart are rendered.
- Selecting a reference immediately recounts houses and changes the highlighted/reference Lagna in both views.

## Data contract

- Selected varga signs and graha placements come from persisted chart rows.
- Lagna reference definitions come from tbl_Dim_HouseReference.
- Special-Lagna positions come from the chart's persisted SpecialLagna points.
- No Naisargika Karaka or Chara Karaka data is accepted or rendered.

## Visual contract

The polar view shows signs, houses counted from the selected Lagna, and available special-Lagna abbreviations. The South Indian view uses the same selected Lagna as its ascendant reference and shows the same chart placements. The former right-side interpretation/detail table is removed.