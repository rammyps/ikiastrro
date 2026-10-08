---
last_updated: 2026-10-08
reflects: planet-bands.css, PlanetBand.cs, /design/planet-bands
---

# Planet strength bands

One colour standard for "how strong / how well placed", app-wide.

- **Stops.** Each graha has nine stops, -4..+4, as shades of its identity colour (`--planet-*`).
  0 is the planet colour itself; + lightens toward the theme's light colour, - darkens toward its dark
  colour. The eight bands are the gaps between adjacent stops. Derived per theme, so all five themes
  (Default Light, Nebula-Light, Cosmic Light, Cosmic Dark, Nebula Violet Dark 7BAND) get their own ramp.
- **Golden.** Everything that is not a graha (Ashtakavarga, houses, signs, totals) uses one Golden ramp,
  tuned per theme and named "Golden - (<Theme> Theme)" (`--golden`, `--golden-name`).
- **Use.** `class="@PlanetBand.Class(planet, stop)"` (planet null = Golden); `PlanetBand.Var(...)` for
  borders, bars and SVG fills. Text colour flips automatically.
- **One axis.** `PlanetBand` normalises dignity (Exalted +4 … Debilitated -4), Shadbala (% of minimum,
  100 = 0), Bhinnashtavarga (bindus-4), Sarvashtakavarga (28 = 0, 3 bindus a stop), Bhava Bala (6 rupas = 0)
  and Vimsopaka (10 = 0, 2.5 a stop).
- **Not covered.** Verdict chips that are judgements rather than strength measures (yoga present/absent,
  favourable/unfavourable, benefic/malefic, argala good/bad, compatibility) keep the `--status-*` tokens.

Reference page: `/design/planet-bands`.
