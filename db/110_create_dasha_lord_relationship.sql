-- =====================================================================
-- 110 — Dasha-lord cross-reference: for each Vimshottari dasha level,
--       the period's Lord planet cross-referenced against that planet's
--       own D1 placement (Rasi + Rasi lord + Nakshatra + Nakshatra lord +
--       KP level-1 sub-lord). "What rules the ruler," per dasha level.
--
--   tbl_Dim_DashaLevel              - 3-row catalogue labeling the 3
--                                      tbl_Chart_DashaPeriods.LevelNumber
--                                      values: L1_MAHA / L2_ANTAR /
--                                      L3_PRAT. No RuleSetId (pure
--                                      catalogue, matches 109's
--                                      tbl_Dim_InterpretiveFactor).
--   tvf_Chart_DashaLordRelationship - per-person TVF, computed on demand
--                                      (not persisted), same category as
--                                      tvf_Chart_LifeWeeks (398) and
--                                      105's tvf_Chart_SignNakshatraRasiRelationship.
--                                      Almost everything it needs already
--                                      exists as chart-fact data —
--                                      tbl_Chart_KeyDetails already carries
--                                      SignId/SignLordPlanetId/NakshatraId/
--                                      NakshatraLordPlanetId/
--                                      NakshatraSubLordPlanetId per planet
--                                      per chart. This is a join/labeling
--                                      layer, not new storage of classical
--                                      facts.
--
-- Load-bearing gotcha this TVF exists to get right: dasha periods and the
-- D1 position chart live under two DIFFERENT tbl_ChartResults rows for the
-- same person (CalculationKind = 'VimshottariDasha' vs 'PositionChart'),
-- bridged only by shared BirthDetailId (src/Ikiastrro.Data/VimshottariDashaService.cs).
-- A naive join on ChartResultId directly between tbl_Chart_DashaPeriods and
-- tbl_Chart_KeyDetails returns zero rows. Follows tvf_Chart_LifeWeeks's own
-- OUTER APPLY ... ORDER BY Id DESC resolution pattern for exactly this,
-- not migration 105's (which takes an already-resolved @ChartResultId).
--
-- KP sub-lord levels 2-7 (tbl_Fact_KpSubLordChain, migration 095) are
-- deliberately NOT joined here: that table is still schema-only/unpopulated
-- (KpSubLordChainRepository is an optional, never-registered constructor
-- param in ChartGenerationService) — joining it today would add 6 more
-- LEFT JOINs against empty data. Only the already-populated KP level-1
-- sub-lord (tbl_Chart_KeyDetails.NakshatraSubLordPlanetId) is included.
-- Extending this TVF once level 2-7 population lands is a follow-up, not a
-- redesign. This also resolves the "SubLords L1-L7" open question from
-- docs/research/domain/res_charakarakas.md — it's the KP chain, not
-- house-cusp sub-lords.
--
-- Follow-up landed 2026-09-22 (migration 131): the function now joins
-- tbl_Fact_KpSubLordChain for levels 2-7, per this comment's own framing.
-- The CREATE OR ALTER body below is what migration 110 originally shipped
-- (L1 only) — kept as the historical record; 131 is the current definition.
--
-- Scale: DashaPeriodsRepository.InsertTree persists the full recursive
-- tree with no truncation — a full Vimshottari tree is up to 819 rows
-- (9 Maha x 9 Antar x 9 Pratyantar) per person, not just current periods.
--
-- Apply:  sqlcmd -S localhost\SQLSERVER2025 -E -d ikiastrro -b -i db/110_create_dasha_lord_relationship.sql
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- --- Batch 1: tbl_Dim_DashaLevel ---
IF OBJECT_ID('dbo.tbl_Dim_DashaLevel', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Dim_DashaLevel (
        LevelNumber  TINYINT      NOT NULL CONSTRAINT PK_Dim_DashaLevel PRIMARY KEY,  -- matches tbl_Chart_DashaPeriods.LevelNumber's own domain (1-3)
        LevelCode    VARCHAR(12)  NOT NULL CONSTRAINT UQ_Dim_DashaLevel_Code UNIQUE,
        LevelName    NVARCHAR(40) NOT NULL,
        SortOrder    TINYINT      NOT NULL,
        CONSTRAINT CK_Dim_DashaLevel_LevelNumber CHECK (LevelNumber BETWEEN 1 AND 3)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Dim_DashaLevel)
INSERT dbo.tbl_Dim_DashaLevel (LevelNumber, LevelCode, LevelName, SortOrder)
VALUES
    (1, 'L1_MAHA',  N'Mahadasha',        1),
    (2, 'L2_ANTAR', N'Antardasha',       2),
    (3, 'L3_PRAT',  N'Pratyantardasha',  3);
GO

-- --- Batch 2: tvf_Chart_DashaLordRelationship ---
CREATE OR ALTER FUNCTION dbo.tvf_Chart_DashaLordRelationship(@BirthDetailId INT)
RETURNS TABLE
AS
RETURN (
    SELECT
        dl.LevelCode, dl.LevelName, dp.Id AS DashaPeriodId, dp.ParentDashaPeriodId,
        dp.SequenceInParent, dp.StartDate, dp.EndDate,
        dp.LordId AS DashaLordPlanetId, lp.PlanetName AS DashaLordName,
        kd.SignId, sa.SignName AS RasiName,
        kd.SignLordPlanetId AS RasiLordPlanetId, rlp.PlanetName AS RasiLordName,
        kd.NakshatraId, nk.NakshatraName,
        kd.NakshatraLordPlanetId, nlp.PlanetName AS NakshatraLordName,
        kd.NakshatraSubLordPlanetId AS SubLordL1PlanetId, sl.PlanetName AS SubLordL1Name
    FROM dbo.tbl_BirthDetails bd
    OUTER APPLY (
        SELECT TOP 1 cr.Id
        FROM dbo.tbl_ChartResults cr
        WHERE cr.BirthDetailId = bd.Id AND cr.CalculationKind = 'VimshottariDasha'
        ORDER BY cr.Id DESC
    ) dashaChart(ChartResultId)
    OUTER APPLY (
        SELECT TOP 1 cr.Id
        FROM dbo.tbl_ChartResults cr
        WHERE cr.BirthDetailId = bd.Id AND cr.CalculationKind = 'PositionChart'
          AND cr.ChartTypeId = (SELECT Id FROM dbo.tbl_Dim_ChartType WHERE Code = 'D1')
        ORDER BY cr.Id DESC
    ) d1Chart(ChartResultId)
    JOIN dbo.tbl_Chart_DashaPeriods dp ON dp.ChartResultId = dashaChart.ChartResultId
    JOIN dbo.tbl_Dim_DashaLevel dl ON dl.LevelNumber = dp.LevelNumber
    JOIN dbo.tbl_Planets lp ON lp.Id = dp.LordId
    LEFT JOIN dbo.tbl_Chart_KeyDetails kd ON kd.ChartResultId = d1Chart.ChartResultId
        AND kd.PlanetId = dp.LordId AND kd.PointKind = 'Graha'
    LEFT JOIN dbo.tbl_SignAttributes sa ON sa.Id = kd.SignId
    LEFT JOIN dbo.tbl_Planets rlp ON rlp.Id = kd.SignLordPlanetId
    LEFT JOIN dbo.tbl_Nakshatras nk ON nk.Id = kd.NakshatraId
    LEFT JOIN dbo.tbl_Planets nlp ON nlp.Id = kd.NakshatraLordPlanetId
    LEFT JOIN dbo.tbl_Planets sl ON sl.Id = kd.NakshatraSubLordPlanetId
    WHERE bd.Id = @BirthDetailId
);
GO

-- --- tbl_Rule_Catalog registration (TVFs are cataloged too — see 105) ---
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_Catalog WHERE RuleTableName = 'tbl_Dim_DashaLevel')
    INSERT dbo.tbl_Rule_Catalog (RuleTableName, EngineCode, MethodCodes, Purpose, IntroducedIn)
    VALUES ('tbl_Dim_DashaLevel', 'DASHA', 'CATALOG_LOOKUP',
            'Labels for the 3 Vimshottari dasha levels (Maha/Antar/Pratyantar) stored as tbl_Chart_DashaPeriods.LevelNumber.',
            '110_create_dasha_lord_relationship.sql');
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_Catalog WHERE RuleTableName = 'tvf_Chart_DashaLordRelationship')
    INSERT dbo.tbl_Rule_Catalog (RuleTableName, EngineCode, MethodCodes, Purpose, IntroducedIn)
    VALUES ('tvf_Chart_DashaLordRelationship', 'DASHA', 'DASHA_LORD_CROSSREF',
            'Per-BirthDetailId TVF: each dasha period''s Lord planet cross-referenced against its own D1 Rasi, Rasi lord, Nakshatra, Nakshatra lord and KP level-1 sub-lord.',
            '110_create_dasha_lord_relationship.sql');
GO

INSERT dbo.SchemaMigrations (ScriptName, Note)
SELECT '110_create_dasha_lord_relationship.sql',
       'Adds tbl_Dim_DashaLevel (3 rows) + tvf_Chart_DashaLordRelationship(@BirthDetailId): dasha Lord vs its D1 Rasi/Nakshatra lord + KP sub-lord L1 only (L2-7 pending KpSubLordChain population).'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '110_create_dasha_lord_relationship.sql');
GO

PRINT '110 applied: tbl_Dim_DashaLevel + tvf_Chart_DashaLordRelationship ready.';
GO
