---
last_updated: 2026-09-14
reflects: KarakaPolarWheelChart initial implementation
component: KarakaPolarWheelChart
route: /charts/{id}/karaka-wheel
---

# Karaka polar wheel

## Purpose

Show the fixed Naisargika Karaka house rules beside the chart-specific placement of the
eight Jaimini Chara Karakas in any generated divisional chart.

## Data contract

- Primary natural karaka per house: `tbl_Rule_Naisargika_Karakas`.
- Full graha-to-matter detail: `tbl_Rule_Naisargika_Karakatwas`.
- Selected varga signs, houses, and Chara roles: persisted `tbl_Chart_KeyDetails` rows.
- Chara roles are assigned from the natal D1 longitudes and retained while their grahas are
  followed into each generated varga. D9 therefore exposes the AK sign as Karakamsa.

## Visual contract

The twelve sectors show the selected varga's sign, house number, primary natural karaka
(`NK`), and any Chara role-bearing grahas occupying that house. Selecting a sector opens its
primary and full natural significations plus the Chara occupants. Houses and actual varga
signs are shown together by default.

The page selector is limited to divisional charts that have persisted rows for the person;
it does not imply that Chara roles are recalculated independently for each varga.
