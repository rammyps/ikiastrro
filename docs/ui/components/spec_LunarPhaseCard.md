---
last_updated: 2026-09-22
reflects: Astro Facts Moon context with persisted Paksha Bala
component: LunarPhaseCard
route: /astro-facts/{id}
---

# LunarPhaseCard

Moon-context summary inside Astro Facts 2.2 About Planets. It combines existing persisted
facts rather than introducing a second lunar calculator.

## Content

- Pakṣa: Śukla/Kṛṣṇa and waxing/waning direction from `vw_ChartMoonContext`.
- Lunar phase: an eight-phase presentation label derived from the persisted Sun–Moon elongation.
- Illumination: `(1 − cos(elongation)) / 2`, displayed as a percentage.
- Lunar strength: the Moon's persisted `PAKSHA_BALA` Shadbala subcomponent, shown on a
  0–60-virūpa meter.
- Tithi, elongation, birth day/night, and Moon nakṣatra remain visible in the surrounding Moon
  context.

The phase label and illumination are explanatory display transformations. The strength value is
read from `tbl_Fact_PlanetaryStrengthComponent`; it is not recalculated by the component.
