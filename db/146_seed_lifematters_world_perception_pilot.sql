-- =====================================================================
-- 146 - Pilot: extend the "actual matter vs. how the world perceives it"
-- pattern (built for SELF_IDENTITY in migration 144) into 3 existing
-- categories - WEALTH, PROPERTY_COMFORTS, CAREER_STATUS (rammyps,
-- 2026-09-25) - plus complete two special-lagna citations that were
-- already written into this project's data but never given a Focus row.
--
-- Mechanism (zero new engine code - confirmed this session):
-- PVR's Arudha Pada technique already computes A1..A12
-- (ArudhaCalculator.cs), already whitelisted
-- (LifeMatterFocusResolver.IsCanonicalSpecialPointCode accepts "A2".."A12")
-- and already renders on the grid (PointKind == 'Arudha'). This project
-- already treats individual padas as per-house significators (A7 =
-- Darapada/relationships, A12 = Upapada Lagna/marriage, PVR sec 9.7). So
-- "what will the world think of my X" = the Arudha Pada of that matter's
-- own dominant house.
--
-- New rows (BasisCode PVR_CROSSVALIDATED unless noted):
--   WEALTH: "What the world thinks of my wealth" -> A2 (2nd is WEALTH's
--     dominant house: rows 2/3/7 of 8 all key off the 2nd/11th-ish wealth
--     axis; 2nd specifically backs rows 2/3).
--   PROPERTY_COMFORTS: "What the world thinks of my vehicle" -> A4 (the
--     user's own example; 4th backs 6 of PROPERTY_COMFORTS's 10 rows).
--   CAREER_STATUS: "What the world thinks of my career and status" -> A10
--     (10th backs rows 7/8).
--   WEALTH: "Prosperity and fortune" (BasisCode PROJECT_SYNTHESIS) -> House
--     (SREE_LAGNA, 1). PVR sec 5.7 ("Sree Lagna... signifies prosperity;
--     the reference point from which Sudasa is reckoned" - already cited
--     in tbl_Dim_Terminology row SPT_SL, db/29) - flagged honestly because
--     PVR's own stated primary use is the Sudasa dasha system, not a
--     direct life-matter reading; this is this project's own extension of
--     that citation to a LifeMatters row.
--
-- Completions (no new MatterText row - a second Focus row on an EXISTING
-- LifeMatterId, since the citation already names an existing matter):
--   CAREER_STATUS_09 ("Authority and power") already carries, verbatim in
--     its own CalculationNarrative (db/087): "PVR also names Ghati Lagna
--     for this... 'Ghati lagna shows self, from the point of view of
--     power, authority and fame.'" - cited but never given a Focus row
--     until now -> adds House(GHATI_LAGNA, 1) alongside its existing
--     Sun-based Focus. tbl_Rule_LifeMatterFocus already supports multiple
--     simultaneous rows per LifeMatterId (HouseAndSpecialPointFoci is a
--     list) - both render together, no new code.
--   WEALTH_01 ("Overall financial condition") gets House(HORA_LAGNA, 1)
--     added, citing tbl_Dim_Terminology row SPT_HL (db/29): "Shows the
--     self with respect to wealth and money; weighed when timing
--     prosperity periods" (PVR sec 5.3/5.6) - a direct, dedicated
--     citation for Hora Lagna that this project had never actually wired
--     into a LifeMatters row.
--
-- ResolveReferenceSign (LifeMatters.razor) already has HORA_LAGNA/
-- GHATI_LAGNA/SREE_LAGNA cases - confirmed this session, no code change
-- needed for this migration.
--
-- Follows the exact tbl_Rule_LifeMatterReference / tbl_Dim_LifeMatter /
-- tbl_Rule_LifeMatterFocus pattern as migrations 087/103/144. No CHECK-
-- constraint change needed - these are new rows in EXISTING categories,
-- not a new category.
--
-- Apply:  sqlcmd -S "localhost\SQLSERVER2025" -E -d ikiastrro -b -f 65001 -i db/146_seed_lifematters_world_perception_pilot.sql
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

-- --- Batch 1: 4 new tbl_Rule_LifeMatterReference rows (next DisplayOrder per category) ---
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_LifeMatterReference WHERE MatterText = N'What the world thinks of my wealth')
BEGIN
    ;WITH m (CategoryCode, CategoryName, DisplayOrder, MatterText, PrimaryChartsText,
             HouseFromLagnaText, KarakaText, HouseFromKarakaText, BasisCode, Narrative) AS (
        SELECT * FROM (VALUES

        ('WEALTH', N'Wealth and financial matters', 9, N'What the world thinks of my wealth', 'D1',
            'Arudha of 2nd', N'Lagna lord (2nd house)', N'Arudha Pada of the 2nd house (A2)', 'PVR_CROSSVALIDATED',
            N'Generalizes PVR sec 7.3.4''s Arudha-Lagna "how the native is perceived" principle to the 2nd house (WEALTH''s own dominant house, rows 2/3), the same technique already used for A7/Darapada and A12/Upapada Lagna (sec 9.7).'),

        ('WEALTH', N'Wealth and financial matters', 10, N'Prosperity and fortune', 'D1',
            'Sree Lagna', N'Sree Lagna', N'Sree Lagna itself', 'PROJECT_SYNTHESIS',
            N'PVR sec 5.7, already cited in tbl_Dim_Terminology row SPT_HL/SPT_SL (db/29): "Sree Lagna... signifies prosperity; the reference point from which Sudasa is reckoned." Flagged: PVR''s own stated primary use is the Sudasa dasha system, not a direct life-matter reading - this is this project''s own extension of that citation to a LifeMatters row.'),

        ('PROPERTY_COMFORTS', N'Property, residence, vehicles and comforts', 11, N'What the world thinks of my vehicle', 'D1',
            'Arudha of 4th', N'Venus (4th house)', N'Arudha Pada of the 4th house (A4)', 'PVR_CROSSVALIDATED',
            N'Generalizes PVR sec 7.3.4''s Arudha-Lagna principle to the 4th house (PROPERTY_COMFORTS'' own dominant house, rows 1/2/6/7/8/9) - the user''s own example ("what will the world think of my vehicle").'),

        ('CAREER_STATUS', N'Career, status and authority', 13, N'What the world thinks of my career and status', 'D1',
            'Arudha of 10th', N'Sun/Mercury (10th house)', N'Arudha Pada of the 10th house (A10)', 'PVR_CROSSVALIDATED',
            N'Generalizes PVR sec 7.3.4''s Arudha-Lagna principle to the 10th house (CAREER_STATUS'' own dominant house, rows 7/8).')

        ) v (CategoryCode, CategoryName, DisplayOrder, MatterText, PrimaryChartsText,
             HouseFromLagnaText, KarakaText, HouseFromKarakaText, BasisCode, Narrative)
    )
    INSERT dbo.tbl_Rule_LifeMatterReference
        (RuleSetId, CategoryCode, CategoryName, DisplayOrder, MatterText, PrimaryChartsText,
         PrimaryLifeAreaId, HouseFromLagnaText, KarakaText, NaisargikaGrahaId, CharaKarakaCode,
         HouseFromKarakaText, BasisCode, CalculationNarrative, SourceRefCode, IsActive)
    SELECT 1, m.CategoryCode, m.CategoryName, m.DisplayOrder, m.MatterText, m.PrimaryChartsText,
           NULL, m.HouseFromLagnaText, m.KarakaText, NULL, NULL,
           m.HouseFromKarakaText, m.BasisCode, m.Narrative,
           CASE WHEN m.BasisCode = 'PROJECT_SYNTHESIS' THEN 'SRC_IKIASTRRO_SYNTHESIS' ELSE 'SRC_PVR_INTEGRATED' END,
           1
    FROM m;
END
GO

-- --- Batch 2: tbl_Dim_LifeMatter rows for the 4 new matters (mirrors migration 103/144's Code convention) ---
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Dim_LifeMatter WHERE Code = 'WEALTH_09')
    INSERT dbo.tbl_Dim_LifeMatter (Code, EnglishName, CategoryCode, CategoryName, SourceGroup, DisplayOrder)
    SELECT lm.CategoryCode + '_' + RIGHT('0' + CAST(lm.DisplayOrder AS VARCHAR(2)), 2),
           lm.MatterText, lm.CategoryCode, lm.CategoryName, 'PVR_LIFE_MATTER', lm.DisplayOrder
    FROM dbo.tbl_Rule_LifeMatterReference lm
    WHERE lm.MatterText IN (
        N'What the world thinks of my wealth', N'Prosperity and fortune',
        N'What the world thinks of my vehicle', N'What the world thinks of my career and status');
GO

-- --- Batch 3: link the 4 new rows' LifeMatterId (migration 103's backfill; migration 144 forgot this - see 145) ---
UPDATE lm
SET LifeMatterId = dm.Id
FROM dbo.tbl_Rule_LifeMatterReference lm
JOIN dbo.tbl_Dim_LifeMatter dm ON dm.SourceGroup = 'PVR_LIFE_MATTER'
    AND dm.Code = lm.CategoryCode + '_' + RIGHT('0' + CAST(lm.DisplayOrder AS VARCHAR(2)), 2)
WHERE lm.LifeMatterId IS NULL;
GO

-- --- Batch 4: Focus rows for the 4 new matters ---
IF NOT EXISTS (
    SELECT 1 FROM dbo.tbl_Rule_LifeMatterFocus f JOIN dbo.tbl_Dim_LifeMatter lm ON lm.Id = f.LifeMatterId
    WHERE lm.Code = 'WEALTH_09'
)
BEGIN
    INSERT dbo.tbl_Rule_LifeMatterFocus (RuleSetId, LifeMatterId, FocusKind, ReferenceCode, HouseNumber, SpecialPointCode, SourceRefCode)
    SELECT 1, lm.Id, 'SpecialPoint', NULL, NULL, 'A2', lm2.SourceRefCode
    FROM dbo.tbl_Dim_LifeMatter lm
    JOIN dbo.tbl_Rule_LifeMatterReference lm2 ON lm2.MatterText = N'What the world thinks of my wealth'
    WHERE lm.Code = 'WEALTH_09';

    INSERT dbo.tbl_Rule_LifeMatterFocus (RuleSetId, LifeMatterId, FocusKind, ReferenceCode, HouseNumber, SpecialPointCode, SourceRefCode)
    SELECT 1, lm.Id, 'House', 'SREE_LAGNA', 1, NULL, lm2.SourceRefCode
    FROM dbo.tbl_Dim_LifeMatter lm
    JOIN dbo.tbl_Rule_LifeMatterReference lm2 ON lm2.MatterText = N'Prosperity and fortune'
    WHERE lm.Code = 'WEALTH_10';

    INSERT dbo.tbl_Rule_LifeMatterFocus (RuleSetId, LifeMatterId, FocusKind, ReferenceCode, HouseNumber, SpecialPointCode, SourceRefCode)
    SELECT 1, lm.Id, 'SpecialPoint', NULL, NULL, 'A4', lm2.SourceRefCode
    FROM dbo.tbl_Dim_LifeMatter lm
    JOIN dbo.tbl_Rule_LifeMatterReference lm2 ON lm2.MatterText = N'What the world thinks of my vehicle'
    WHERE lm.Code = 'PROPERTY_COMFORTS_11';

    INSERT dbo.tbl_Rule_LifeMatterFocus (RuleSetId, LifeMatterId, FocusKind, ReferenceCode, HouseNumber, SpecialPointCode, SourceRefCode)
    SELECT 1, lm.Id, 'SpecialPoint', NULL, NULL, 'A10', lm2.SourceRefCode
    FROM dbo.tbl_Dim_LifeMatter lm
    JOIN dbo.tbl_Rule_LifeMatterReference lm2 ON lm2.MatterText = N'What the world thinks of my career and status'
    WHERE lm.Code = 'CAREER_STATUS_13';
END
GO

-- --- Batch 5: completion Focus rows on EXISTING matters (CAREER_STATUS_09, WEALTH_01) ---
IF NOT EXISTS (
    SELECT 1 FROM dbo.tbl_Rule_LifeMatterFocus f
    JOIN dbo.tbl_Dim_LifeMatter lm ON lm.Id = f.LifeMatterId
    WHERE lm.Code = 'CAREER_STATUS_09' AND f.ReferenceCode = 'GHATI_LAGNA'
)
    INSERT dbo.tbl_Rule_LifeMatterFocus (RuleSetId, LifeMatterId, FocusKind, ReferenceCode, HouseNumber, SpecialPointCode, SourceRefCode)
    SELECT 1, lm.Id, 'House', 'GHATI_LAGNA', 1, NULL, 'SRC_PVR_INTEGRATED'
    FROM dbo.tbl_Dim_LifeMatter lm
    WHERE lm.Code = 'CAREER_STATUS_09';
GO

IF NOT EXISTS (
    SELECT 1 FROM dbo.tbl_Rule_LifeMatterFocus f
    JOIN dbo.tbl_Dim_LifeMatter lm ON lm.Id = f.LifeMatterId
    WHERE lm.Code = 'WEALTH_01' AND f.ReferenceCode = 'HORA_LAGNA'
)
    INSERT dbo.tbl_Rule_LifeMatterFocus (RuleSetId, LifeMatterId, FocusKind, ReferenceCode, HouseNumber, SpecialPointCode, SourceRefCode)
    SELECT 1, lm.Id, 'House', 'HORA_LAGNA', 1, NULL, 'SRC_PVR_INTEGRATED'
    FROM dbo.tbl_Dim_LifeMatter lm
    WHERE lm.Code = 'WEALTH_01';
GO

-- --- Batch 6: ledger + summary ---
INSERT dbo.SchemaMigrations (ScriptName, Note)
SELECT '146_seed_lifematters_world_perception_pilot.sql',
       'Pilot: adds 4 world-perception rows (WEALTH x2, PROPERTY_COMFORTS, CAREER_STATUS) via A2/A4/A10 + Sree Lagna; completes CAREER_STATUS_09/Ghati Lagna and WEALTH_01/Hora Lagna Focus rows.'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '146_seed_lifematters_world_perception_pilot.sql');
GO

DECLARE @newRows INT = (SELECT COUNT(*) FROM dbo.tbl_Rule_LifeMatterReference WHERE MatterText IN (
    N'What the world thinks of my wealth', N'Prosperity and fortune',
    N'What the world thinks of my vehicle', N'What the world thinks of my career and status'));
DECLARE @unlinked INT = (SELECT COUNT(*) FROM dbo.tbl_Rule_LifeMatterReference WHERE LifeMatterId IS NULL);
DECLARE @newFoci INT = (
    SELECT COUNT(*) FROM dbo.tbl_Rule_LifeMatterFocus f
    JOIN dbo.tbl_Dim_LifeMatter lm ON lm.Id = f.LifeMatterId
    WHERE lm.Code IN ('WEALTH_09','WEALTH_10','PROPERTY_COMFORTS_11','CAREER_STATUS_13'));
DECLARE @completions INT = (
    SELECT COUNT(*) FROM dbo.tbl_Rule_LifeMatterFocus f
    JOIN dbo.tbl_Dim_LifeMatter lm ON lm.Id = f.LifeMatterId
    WHERE (lm.Code = 'CAREER_STATUS_09' AND f.ReferenceCode = 'GHATI_LAGNA')
       OR (lm.Code = 'WEALTH_01' AND f.ReferenceCode = 'HORA_LAGNA'));
PRINT '146 applied: new reference rows=' + CAST(@newRows AS VARCHAR(10)) + ' (expected 4), '
    + 'unlinked LifeMatterId rows remaining=' + CAST(@unlinked AS VARCHAR(10)) + ' (expected 0), '
    + 'new-matter focus rows=' + CAST(@newFoci AS VARCHAR(10)) + ' (expected 4), '
    + 'completion focus rows=' + CAST(@completions AS VARCHAR(10)) + ' (expected 2).';
GO
