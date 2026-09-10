---
last_updated: 2026-09-10
workstream: ui
togaf: C — Application Architecture (UI)
---

# UI — testing

Two layers, both under `dotnet test`.

## 1. Component tests — `tests/Ikiastrro.Web.Tests` (bUnit + xunit)

Render Razor components in isolation, no browser, no server, no JS. Fast (~0.5 s for the
whole suite). This is where golden-snapshot chart tests and view-model logic live.

```
dotnet test tests/Ikiastrro.Web.Tests
```

**Blind spot:** bUnit has no JS runtime and no real MudBlazor popover layer, so it cannot see
anything that only breaks in a live interactive circuit — a provider in the wrong render
scope, a popover that never opens, a picker that stays `readonly`, layout/overflow. That is
what layer 2 is for.

## 2. Browser end-to-end — `tests/Ikiastrro.Web.E2E` (Playwright + real Chromium)

Drives a **running** app with a real browser. Collects `console.error`, uncaught page
exceptions and any `>= 500` response, and fails the test if the list isn't empty.

### One-time setup

```
dotnet build tests/Ikiastrro.Web.E2E
pwsh tests/Ikiastrro.Web.E2E/bin/Debug/net10.0/playwright.ps1 install chromium
```

### Run

```
# terminal 1 — the app
dotnet run --project src/Ikiastrro.Web            # serves http://localhost:5160

# terminal 2 — the tests
dotnet test tests/Ikiastrro.Web.E2E
```

| Env var | Default | Purpose |
|---|---|---|
| `IKIASTRRO_E2E_BASEURL` | `http://localhost:5160` | target app |
| `IKIASTRRO_E2E_HEADED` | *(unset)* | `1` → visible browser + 250 ms slow-mo for debugging |

**Skip contract:** if nothing answers on the base URL the whole E2E suite **skips**, it does
not fail — the same "manual, not CI" arrangement as `scripts/verify-*-ui.mjs`. So a plain
`dotnet test` across the solution is safe without the app up; you just get `Skipped`.

Failure artifacts (screenshots) go to `reports/e2e/` (git-ignored) — call `SnapshotAsync` in
a test, or run headed.

### What's covered

`HomePageTests` locks in the Home fixes from commit `99b19cc`, each of which was invisible to
bUnit:

- Preferences → Ayanāṁśa / Chart-type `MudSelect`s open and list their options (popover
  provider is in the interactive scope); North / West Indian are `aria-disabled`.
- Add-New: the Date-of-Birth `MudDatePicker` opens (to the year view) and is editable; a
  typed 1981 date round-trips.
- Typing an unknown name auto-reveals the pre-filled Add-New form; clearing it retracts.
- The Ganesha art is large and hugs the page's right edge, no horizontal page scroll.
- Home raises no browser console errors.

### Retired

`scripts/verify-home-ui.mjs` — a hand-rolled CDP-over-WebSocket smoke against the **v1**
Home (`.landing-page` / `#saved-filter` selectors). Superseded by `HomePageTests`; delete
once nothing references it. `scripts/verify-transit-ui.mjs` is still the manual smoke for the
Transit landing until an E2E `TransitPageTests` replaces it.
