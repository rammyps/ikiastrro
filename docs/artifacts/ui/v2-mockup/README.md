# docs/artifacts/ui/v2-mockup — the v2 UI reference mockup

**Frozen visual reference, not a build target.** Self-contained HTML mockups the
`wkstream_UI_v2` screens are designed and signed off in (HTML-first, then MudBlazor). Kept
here so "verified in HTML" points at something concrete once implementation diverges.

## Files

### `chart-evidence-hub.html` — Home · Transit · All Charts · Key Inference (round 1, frozen)

- **Date signed off:** 2026-09-10 · **last touched:** 2026-09-11 (Key Inference round-1
  divisional + Ashtakavarga sub-tabs added; round 2 below has since superseded the Key
  Inference portion)
- **Published artifact:** https://claude.ai/code/artifact/a2166036-9a54-4447-8a10-4191e1d275f7
  (single URL, republished each design round — this file is the last round)

One page, hash-routed, four views:

| Hash | View id | Screen |
|---|---|---|
| `#home` | `view-home` | Home / entry — search, Preferences, Add |
| `#transit` | `view-transit` | Transit — D1 birth chart landing (wheel + 2-tab table) |
| `#all-charts` | `view-charts` | All Charts — 21 divisional grids |
| `#key-inference` | `view-infer` | Key Inference **round 1** — 4 headers; KEY INFERENCE has 13 flat sub-tabs. Historical — see `key-inference-v2.html` for the current design. |

The transit wheel is an embedded base64 SVG (a ChatGPT-authored placeholder; the real
`ikiastrro-transit-wheel.svg` supersedes it). Most tables are illustrative.

### `key-inference-v2.html` — Key Inference, round 2 (active, under review)

- **Round:** 2026-09-11 · restructures the flat 13-sub-tab Key Inference into a **6-step
  numbered flow** (D1/Transit → Understanding [2.1 Houses / 2.2 Planets] → Strength →
  Planet-Chart → Ashtakavarga → Yoga), one hand-rolled chart + one table per step.
- **Published artifact:** https://claude.ai/code/artifact/f0ae5be5-b293-4273-9a78-3f0fa7f7a84d
- **Spec of record:** [`../../../ui/components/key-inference.md`](../../../ui/components/key-inference.md)
  — field sourcing (what's real vs. needs a new DB column), the "also computed, now surfaced"
  list, and open questions.
- Stands alone (own copy of the app chrome/tokens) so it reviews independently of the frozen
  hub above. Sample person: **Ananya (id 89)**, the fully-computed reference row — every
  number is real, read from the DB on 2026-09-11, except where a card says "illustrative".

## Spec of record

[`../../../ui/wkstream_UI_v2.md`](../../../ui/wkstream_UI_v2.md) and the `docs/ui/components/*.md`
files. Where a mockup and a component doc disagree, the **doc wins** — the mockup is a
picture, not a contract.

## Viewing it

Open either file directly in a browser (fully self-contained — no server, no assets) and, for
`chart-evidence-hub.html`, switch views with the hash, e.g. `…/chart-evidence-hub.html#transit`.
Static PNGs are intentionally not committed; the HTML renders identically everywhere and is
the authority. Capture one into this folder if a PR needs a still.

## Acceptance

Each v2 screen is tracked to done in the **Acceptance matrix** in
[`../../../ui/wkstream_UI_v2.md`](../../../ui/wkstream_UI_v2.md#acceptance-matrix) — reference
here, route/component, data source, responsive/empty/error, a11y, bUnit, browser verify.
