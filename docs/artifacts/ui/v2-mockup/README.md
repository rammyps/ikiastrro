# docs/artifacts/ui/v2-mockup — the v2 UI reference mockup

**Frozen visual reference, not a build target.** `chart-evidence-hub.html` is the
self-contained HTML mockup the `wkstream_UI_v2` screens were designed and signed off in
(HTML-first, then MudBlazor). It is kept here so "verified in HTML" points at something
concrete once implementation diverges.

- **Date signed off:** 2026-09-10
- **Published artifact:** https://claude.ai/code/artifact/a2166036-9a54-4447-8a10-4191e1d275f7
  (single URL, republished each design round — this file is the last round)
- **Spec of record:** [`../../../ui/wkstream_UI_v2.md`](../../../ui/wkstream_UI_v2.md) and the
  `docs/ui/components/*.md` files. Where the mockup and a component doc disagree, the **doc
  wins** — the mockup is a picture, not a contract.

## What's in it

One page, hash-routed, four views:

| Hash | View id | Screen |
|---|---|---|
| `#home` | `view-home` | Home / entry — search, Preferences, Add |
| `#transit` | `view-transit` | Transit — D1 birth chart landing (wheel + 2-tab table) |
| `#all-charts` | `view-charts` | All Charts — 21 divisional grids |
| `#key-inference` | `view-infer` | Key Inference — 4 headers, 8 sub-tabs |

The transit wheel is an embedded base64 SVG (a ChatGPT-authored placeholder; the real
`ikiastrro-transit-wheel.svg` supersedes it). Data in every table is illustrative.

## Viewing it

Open `chart-evidence-hub.html` directly in a browser (it is fully self-contained — no server,
no assets) and switch views with the hash, e.g. `…/chart-evidence-hub.html#transit`. Static
PNGs are intentionally not committed; the HTML renders identically everywhere and is the
authority. Capture one into this folder if a PR needs a still.

## Acceptance

Each v2 screen is tracked to done in the **Acceptance matrix** in
[`../../../ui/wkstream_UI_v2.md`](../../../ui/wkstream_UI_v2.md#acceptance-matrix) — reference
here, route/component, data source, responsive/empty/error, a11y, bUnit, browser verify.
