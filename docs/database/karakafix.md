---
last_updated: 2026-09-16
workstream: database
status: proposed
---

# Karaka data-model connection plan

## Decision summary

The karaka tables and planetary-friendship tables belong to two related pipelines, not one
table family. Karaka tables define which role signifies a life matter. Friendship tables
evaluate the condition of the graha that fulfils that role in a particular chart.

Current live row counts reviewed on 2026-09-16:

| Table | Rows | Responsibility |
|---|---:|---|
| `tbl_Rule_LifeMatterReference` | 96 | How a life matter is examined |
| `tbl_Rule_Naisargika_Karakatwas` | 34 | Detailed graha–matter–house mappings |
| `tbl_Rule_Naisargika_Karakas` | 12 | Primary Naisargika karaka per house |
| `tbl_Rule_NaturalRelationship` | 42 | Directed permanent-friendship grid |
| `tbl_Rule_TemporaryFriendshipDistance` | 12 | Temporary friendship by sign distance |
| `tbl_Rule_CompoundRelationship` | 6 | Pañcadhā relationship result matrix |

## Problems in the present shape

- `tbl_Rule_Naisargika_Karakas` duplicates a 12-row reduction of
  `tbl_Rule_Naisargika_Karakatwas`, but no FK structurally proves the supporting assignment.
- Matters are repeated as `Matter`, `MattersSignified`, and `MatterText`, so equivalent
  concepts cannot be joined by a stable code.
- `tbl_Rule_LifeMatterReference` mixes reference instructions with karaka assignments.
- Chara roles use an unchecked semantic string (`CharaKarakaCode`) rather than an FK.
- Compound karakas such as `Mars/Rahu` are stored only in `KarakaText`. Of the 96 current
  life-matter rows, 73 have one fixed graha, 2 have a Chara role, and 21 remain text-only.
- Sthira karakas remain coarse, hard-coded arrays in `LifeAreaMap` rather than sourced rules.
- The current repository loads the three karaka datasets independently and does not enforce
  a single shared `RuleSetId` across the result.

## Target model

```text
tbl_Dim_LifeMatter
        |
        +-- tbl_Rule_KarakaMatter -- tbl_Dim_KarakaRole
        |                                  |
        |                                  +-- fixed GrahaId (Naisargika/Sthira)
        |                                  +-- dynamic role (AK/AmK/.../DK)
        |
        +-- tbl_Rule_LifeMatterReference
                         |
                         +-- chart, house and reference-frame instructions
```

### `tbl_Dim_LifeMatter`

Create a stable vocabulary with `Id`, `Code`, `EnglishName`, and optional category/life-area
metadata. Codes such as `MATTER_MOTHER`, `MATTER_WEALTH`, and `MATTER_ACCIDENTS` become the
join contract instead of prose.

### `tbl_Dim_KarakaRole`

Create one catalogue for every karaka role:

- `Id`
- `KarakaTypeCode`: `NAISARGIKA`, `STHIRA`, or `CHARA`
- `RoleCode`: for example `GRAHA_SUN`, `GRAHA_MARS`, `CHARA_AK`, `CHARA_DK`
- nullable `FixedGrahaId`
- nullable `CharaKarakaCode`
- a check constraint requiring exactly the target appropriate to the type

This gives Chara roles a real FK and permits later karaka types without adding another
nullable role column to `tbl_Rule_LifeMatterReference`.

### `tbl_Rule_KarakaMatter`

Create the canonical many-to-many rule bridge with:

- `Id`, `RuleSetId`, `KarakaRoleId`, and `LifeMatterId`
- nullable `HouseNumber`
- `IsPrimary` and `DisplayOrder`
- `BasisCode`, `SourceRefCode`, and `IsActive`

Single and compound karakas then use the same structure. “Mars and Rahu” becomes two rows
for one life matter instead of an unqueryable string.

### Narrow `tbl_Rule_LifeMatterReference`

Keep this table for how a matter is examined: divisional-chart applicability, Lagna or
reference-point method, house/relative-house instruction, narrative, and provenance.
Replace its semantic dependence on `KarakaText`, `NaisargikaGrahaId`, `CharaKarakaCode`,
`PrimaryChartsText`, and `HouseFromLagnaText` with normalized child rows and FKs. Keep the
legacy columns during consumer migration only.

## Naisargika primary reduction

Treat `tbl_Rule_Naisargika_Karakatwas` as the canonical knowledge currently represented by
the two Naisargika tables. Migrate the 12 primary choices into
`tbl_Rule_KarakaMatter.IsPrimary`, then replace `tbl_Rule_Naisargika_Karakas` with a
compatibility view such as `vw_Rule_PrimaryNaisargikaKaraka`. A primary row will then be
backed structurally by the same canonical assignment instead of only by a migration check.

## Friendship and dignity connection

The relationship rules connect only after a karaka role resolves to an actual planet:

```text
resolved karaka role
      -> actual GrahaId
      -> tbl_Rule_NaturalRelationship
       + tbl_Rule_TemporaryFriendshipDistance
      -> tbl_Rule_CompoundRelationship
      -> karaka-planet condition for the chart
```

Do not add direct FKs from karaka-definition rows to these relationship tables. Their join is
chart-dependent: the resolved karaka planet is compared with the relevant occupant, dispositor,
or house lord. Implement that integration through a function such as
`tvf_ChartKarakaCondition(@ChartResultId, @LifeMatterId, @RuleSetId)`, returning karaka type,
resolved planet, relevant house/lord, natural relation, temporary relation, compound code,
and relationship score.

Every join across rule tables must use the same explicit `RuleSetId`; independently joining
each input to whichever rule set is active can mix versions.

## Migration sequence

1. Create `tbl_Dim_LifeMatter`, `tbl_Dim_KarakaRole`, and `tbl_Rule_KarakaMatter`.
2. Seed canonical matter and role codes with source provenance.
3. Migrate the 34 Naisargika rows and mark the 12 primary assignments.
4. Normalize all 96 life-matter rows, including the 21 text-only karaka definitions.
5. Seed Sthira assignments from a verified source; do not copy the coarse `LifeAreaMap`
   groupings blindly.
6. Connect all eight Chara roles through FKs.
7. Add compatibility views and update `NaisargikaKarakaRepository`.
8. Add and verify the chart-level karaka-condition TVF.
9. Retire duplicate/free-text columns only after every consumer has migrated.

## Verification requirements

- All 34 current Naisargika mappings survive with the same source and house.
- Exactly 12 active primary Naisargika assignments exist for the active rule set.
- All 96 life matters resolve to at least one normalized karaka role or an explicitly modeled
  non-karaka reference such as Lagna lord.
- The 21 current text-only cases no longer depend on parsing `KarakaText`.
- Chara assignments resolve to the planet computed for the requested chart.
- Natural + temporary inputs reproduce all six compound outcomes and scores.
- Compatibility reads match the current `NaisargikaKarakaRepository` output before cutover.

No schema or data change is implemented by this document; it is the design input for a future
database-workstream migration and matching CLI/UI consumer changes.
