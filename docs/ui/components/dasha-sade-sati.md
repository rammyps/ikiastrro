---
last_updated: 2026-09-09
workstream: ui
component: TimePeriods
routes:
  - /key-inference#time-period-dasha
  - /key-inference#saturn-time-period
togaf: C — component spec
---

# Component — time periods (daśā & Saturn)

Two **headers** on the Key Inference page, alongside — not part of — the round-2 6-step flow
([`key-inference.md`](key-inference.md#the-flow) covers the flow itself). Read-only over
persisted rows and one table-valued function; no new calculation
([`../../architecture/domain-contracts.md`](../../architecture/domain-contracts.md)).

The old `/charts/{id}/life-weeks` route stays retired in v2 (`FEAT-DASHA-02`); the Vimśottari
timeline is served here as a drill-down tree.

## Header — TIME PERIOD (DASHA)

Vimśottari **drill-down tree**, `tbl_Chart_DashaPeriods` ⋈ `tbl_ChartResults`, three levels:

```
Mahādaśā            (level 1)
  └ Antardaśā       (level 2)
      └ Pratyantardaśā   (level 3, "prathana")
```

- Each level-1 and level-2 row is expandable (caret ▸ / ▾). Level 3 is a leaf.
- **On load** the currently-running chain is expanded — running Mahādaśā → its running
  Antardaśā → its running Pratyantardaśā — and each of those three rows is **highlighted in
  sunset** (`--brand-sunset`, midnight text).
- Row content: period lord · start → end. `StartDayOffset` / `EndDayOffset` (days from birth,
  partial-at-birth aware, `FEAT-DASHA-01`) surface on hover / expand.
- "Filter" per level = the expand toggle; a *collapse all / jump to current* control resets to
  the running chain.

## Header — SATURN TIME PERIOD

**One ascending table** combining every Saturn-over-Moon window from birth to age 75, from
`tvf_Chart_SadeSatiPeriods(@BirthDetailId)` (natal Moon sign + `tbl_PlanetSignTransitEvents`,
no new reference data — `FEAT-TRANSIT-02`):

| `PeriodType` | Shown as | Saturn is |
|---|---|---|
| `SadeSati_Dhaiya1_Rising` | Sade Sati · Rising | 12th from Moon |
| `SadeSati_Dhaiya2_Peak` | Sade Sati · Peak | over the Moon (1st) |
| `SadeSati_Dhaiya3_Setting` | Sade Sati · Setting | 2nd from Moon |
| `KantakaShani` | Kaṇṭaka Śani | 4th from Moon |
| `AshtamaShani` | Aṣṭama Śani | 8th from Moon |

Columns: **Type · Round · From Moon · Saturn sign · Start · End**.

- **Round** is a new derived column (immediately left of *From Moon*): which ~29½-year Saturn
  cycle the window falls in — `1st` / `2nd` / `3rd` across the 0–75 age span. Filter chips
  `All · 1st round · 2nd round · 3rd round` above the table.
- Rows are ordered **ascending by start date** (birth → age 75).
- The window that is **currently active, or the next one upcoming**, is highlighted in sunset.
- The TVF splits a window on each retrograde re-entry; the UI re-consolidates contiguous
  same-sign spans and notes the retro breaks.

### Ashtakavarga — Saturn

**Empty state.** Bindu-point transit strength is out of scope pending a cited source for the
contribution tables ([`../../cli/calculations.md`](../../cli/calculations.md) §9); the vendored
MIT `jyotishganit` port unblocks it. The card shows the 12-house bindu grid greyed until then.

## Rendering

MudBlazor: `MudTreeView` for the daśā drill-down, `MudTable` for the Saturn window table
(`table-layout: auto`, no horizontal scroll, dates right-aligned). Shared tokens; the running /
next-upcoming row uses `--brand-sunset` fill with midnight text — never colour alone (the row
also carries a "current" / "next" label).

## Verification

bUnit snapshots of the tree (running chain pre-expanded) and the Saturn table (each round
filter); Saturn windows reconciled against `SadeSatiRepository` CLI output; daśā bounds against
`VimshottariDashaService`. No regression in `verify-*`.
