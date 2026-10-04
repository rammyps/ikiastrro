---
last_updated: 2026-09-18
status: reference-capture
---

# Gochara Vedha (transit obstruction) — PVR ch. 26.3, Table 63

Triggered by a pasted Vedha table checked against Naisargika Karaka (2026-09-18). Two
findings: this is not Naisargika Karaka content (different technique entirely — see below),
and the pasted numbers are not PVR's own — a different, unidentified source, itself OCR-
garbled in places ("the ~ sign", "E, 7"). Per the "stick to PVR" direction, this file captures
PVR's own real Table 63 instead of the pasted text.

## Not Naisargika Karaka

Naisargika Karaka (PVR ch. 8, seeded migration 086) is a **static** planet → matter/house
assignment — Sun signifies father/soul, Moon signifies mother/mind, etc., fixed for every
chart. Gochara Vedha is a **dynamic** transit technique — which houses counted from a
person's own natal Moon a transiting planet gives good results in, and which "obstruction"
house another planet transiting in cancels that result for the moment. Different questions,
different chapters (8 vs. 26). Do not merge into the karaka tables.

This is `pvr-coverage.md`'s Part 3 (Transit Analysis) gap, already flagged there: "§25
techniques (vedha, murti, argala on transits) not built." (The book's own numbering has this
specific table at §26.3, not §25 — Part 3's row spans both chapters 25–26.)

## PVR Table 63: Vedha Sthaanas (ch. 26.3, pg 347–348)

Source: `SRC_PVR_INTEGRATED`, direct read of
`D:\@ClaudeSpace\BookExtracts\pvr-integrated-approach-raw.txt:11980–12018`.

> "We listed the good and bad houses for all planets to transit from natal Moon in a previous
> chapter. Even when a planet is transiting in a favorable house from natal Moon, it may be
> 'obstructed' by another planet transiting in the vedha sthana (house of obstruction). In
> that case, the planet cannot give its good results."

| Transiting planet | Auspicious houses from natal Moon (each one's Vedha/obstruction house in parentheses) |
|---|---|
| Sun | 3 (9), 6 (12), 10 (4), 11 (5) |
| Moon | 1 (5), 3 (9), 6 (12), 7 (2), 10 (4), 11 (8) |
| Mars | 3 (12), 6 (9), 11 (5) |
| Mercury | 2 (5), 4 (3), 6 (9), 8 (1), 10 (8), 11 (12) |
| Jupiter | 2 (12), 5 (4), 7 (3), 9 (10), 11 (8) |
| Venus | 1 (8), 2 (7), 3 (1), 4 (10), 5 (9), 8 (5), 9 (11), 11 (6), 12 (3) |
| Saturn | 3 (12), 6 (9), 11 (5) |

**Row labels realigned (2026-10-04)**: the extract's OCR puts the "Sun" label on the header line, so every label sat one row too high and the last row (Saturn: 3 (12), 6 (9), 11 (5)) had none. Realigned above and in migration 158; PVR's Mercury-in-4th example (Mercury 4 (3)) only fits the realigned table. The table has no Rahu or Ketu row (read as Saturn and Mars, ch.25).

**Two stated exceptions** (ch. 26.3): Sun and Saturn never cause Vedha on each other; Moon
and Mercury never cause Vedha on each other.

**Worked example (PVR's own, Bill Gates)**: natal Moon in Pisces. Mercury transiting Gemini
(the 4th house from natal Moon) in June 2000 is an auspicious transit per the table above. The
Vedha house for that 4th-house Mercury transit is the 3rd house (Taurus, from Pisces). Several
planets transited Taurus around June 8, 2000 — causing Vedha on Mercury, which PVR ties to a
legal setback for Gates' company that day. (Mercury is additionally a "loha murthi" per an
earlier chapter — not reproduced here, out of scope for this note.)

## Scope of this note

Reference capture only — this file transcribes Table 63 so a future engine has a correct,
cited source to build from. No `tbl_Rule_GocharaVedha` table, no calculator, no wiring into
the existing `tbl_PlanetSignTransitEvents`/Gochara panel exists yet. That remains future work,
tracked at `pvr-coverage.md`'s Part 3 row.
