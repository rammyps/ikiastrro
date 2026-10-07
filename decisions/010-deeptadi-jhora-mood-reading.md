---
status: accepted
date: 2026-10-07
workstream: cli (PlanetaryStates engine)
---

# 010 — Dīptādi read the way Jagannatha Hora prints its Mood column

## Decision

`DeeptadiStateCalculator` (PVR §15.4.3's nine states) follows JHora's "Mood (D-1)" output:

- **Deepta / Swastha** are placement flags (exalted; own sign or moolatrikona).
- **Mudita / Saanta / Deena / Duhkhita** are the planet's compound (Panchadha) relationship to its
  **sign lord**, kept *alongside* Deepta/Swastha. Sun exalted in Aries (lord Mars: natural friend,
  temporary enemy → neutral) is Deepta **and** Deena. A planet in its own sign has no relationship tier.
- **Vikala**: at least **two** natural malefics in the sign (PVR: "joined by malefic planets").
- **Khala**: the sign lord is Sun, Mars or Saturn.
- **Kopita**: the Sun within **5°** (real separation, so nodes qualify; combustion's orb is not used).
- "Malefic" is the **fixed** set Sun, Mars, Saturn, Rahu, Ketu (`ClassicalMalefics`). The Moon and
  Mercury are never conditional here, unlike `ArgalaCalculator.IsNaturalMalefic`, which the
  Lajjitādi Trishita/Kshobhita tests still use.

## Why

The previous reading (single dignity tier, one malefic joins Vikala, conditional Moon/Mercury,
Kopita = combust) disagreed with JHora on 18 of 36 planet cells across the four reference exports
(Rammy = 1_Ramakrishnan, Ananya, Sundari, Ramya). The new reading reproduces all 36 except the
relationship tier of three node cells (`AvasthaJhoraMoodGoldenTests`). Each rule above is the
smallest one consistent with every JHora cell, and each reads PVR's wording, not a contradiction of it.

## Open

- **Kopita orb**: JHora flags the Sun at 1.2°, 3.8°, 3.9° and 4.3° and not at 6.4°. 5° is a round
  value in that gap, not a measured cut-off.
- **Node tiers**: JHora gives Rahu/Ketu real friend/enemy tiers (observed: Rahu–Venus friend,
  Rahu–Jupiter neutral, Ketu–Saturn friend, Ketu–Mars enemy). `DignityEngine` has no node
  Naisargika table and labels nodes Neutral, so Rammy's Rahu (JHora Duhkhita) and Ananya's Rahu
  (Saanta) and Ketu (Duhkhita) still differ. Needs JHora's table, not four observations.
- **Lajjitādi**: JHora prints only Lajjita and Garvita; both match. The other four (Kshudhita,
  Trishita, Mudita, Kshobhita) follow PVR and have no JHora counterpart to check.
- Stored facts need `recompute-keydetails` to pick the change up.
