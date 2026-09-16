---
last_updated: 2026-09-17
workstream: ui
component: KeyInference step 4 · Spl Lagnas
route: /key-inference/{id} (step 4)
togaf: C — tab spec (first of the specs_KI_<step> series)
reflects: PolarGridLagnaSelect embedded in Key Inference
---

# Key Inference · 4. Spl Lagnas

> First of a new **per-step spec series** for Key Inference (`specs_KI_<step>.md`), one file per
> step, superseding that step's row in [`key-inference.md`](key-inference.md)'s "The flow" table
> as the detail source. Only step 4 is split out so far — the other steps stay inline in
> `key-inference.md` until they're split out the same way in later work.
>
> This is a **tab-level** spec: what step 4 shows and where its pieces come from. It sits one
> layer above the **component-level** spec for the embedded chart component itself,
> [`spec_KarakaPolarWheelChart.md`](spec_KarakaPolarWheelChart.md) (`PolarGridLagnaSelect` —
> the Chart select / Wheel-Grid toggle / Lagna checkboxes), which stays the spec of record for
> that component per `project_standards.md` §3.2 ("every chart module has its own
> `spec_<Name>.md`").

## Purpose

Compare the five special-Lagna references (Sign/Arudha/Hora/Sree/Ghati Lagna) for the selected
divisional chart, as either a polar wheel or a South-Indian grid, plus a reference table of all
four computed Special Lagnas.

## Layout

- No step-level heading — the "4. Spl Lagnas" step pill above this panel already names it (this is
  the only tab under step 4).
- Chart `<select>` and the Wheel/Grid toggle, centred together on one row.
- **Grid view only:** a second checkbox row beneath that — Upagrahas / Gra-Arudha / Arudha-Lagna /
  HN-Moon / HN-Sun (all on by default) — controlling which optional Grid-view content shows; see
  Controls below. Not rendered in Wheel view, where it has no effect.
- Below that: the selected view (Wheel or Grid), then the Special Lagnas table.

## Views

- **Wheel** — unchanged; documented by reference to
  [`spec_KarakaPolarWheelChart.md`](spec_KarakaPolarWheelChart.md#visual-contract).
- **Grid** — composes [`SouthIndianGrid_Micro`](spec_SouthIndianGrid_Micro.md), replacing the
  `SouthIndianGrid_Detailed` this view used before 2026-09-17. All per-cell content rules (no
  one-planet-per-row limit, no chip background, `Ju`/`(Ju)` direction convention, Gulika/Maandi,
  Graha Arudha, Arudha Lagna, both karaka schemes gated to D1/D9, house-from-Sun badge, inline
  `Ma(4)`-style aspect tags, the no-overlap/row-grouping layout rules) live in that spec, not
  duplicated here.

## Controls — Grid-display checkboxes (2026-09-17)

Five independent toggles, Grid view only, each defaulting on: **Upagrahas** (Gulika/Maandi row),
**Gra-Arudha** (Graha Arudha entries), **Arudha-Lagna** (the AL entry — a separate control from
the existing "Arudha Lagna" checkbox in the Lagna-reference group, which instead picks Arudha
Lagna as this grid's own ascendant reference), **HN-Moon** (house-from-Moon badge), **HN-Sun**
(house-from-Sun badge). Full behaviour: `spec_KarakaPolarWheelChart.md`.

## Special Lagnas table

All 4 — Bhaava/Hora/Ghati/Sree — with Sign/House/Degree/Signifies, not just the ones marked on the
wheel. Carried over unchanged from `key-inference.md`'s previous step-4 row.

## Data contract

- Selected varga signs and graha placements: persisted chart rows (`PlanetPlacements`).
- Lagna reference definitions: `tbl_Dim_HouseReference`.
- Special-Lagna positions: the chart's persisted SpecialLagna points.
- Special Lagnas table: `tbl_Rule_LifeMatterReference` + Naisargika/relationship rule tables
  through `NaisargikaKarakaRepository`; persisted varga placements through `WorkspaceData`.
- Grid-view additions (see `spec_SouthIndianGrid_Micro.md` for the full per-source breakdown):
  Gulika/Maandi and Graha Arudha via `ChartKeyDetail` (`PointKind` `'Upagraha'`/`'GrahaArudha'`),
  Naisargika Karaka via `NaisargikaKarakaRepository.LoadActive().Primary` (already loaded on this
  page, not a new query), Chara Karaka via `ChartKeyDetail.CharaKaraka` (already persisted, no
  calculator call in the UI layer), aspects via `ChartAspect` through
  `ChartViewModel.BuildAspectedByMicro`.
