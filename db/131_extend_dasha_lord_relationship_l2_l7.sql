-- =====================================================================
-- 131 -- Extend tvf_Chart_DashaLordRelationship (migration 110) to KP
-- sub-lord levels 2-7, per that migration's own comment: "deliberately
-- NOT joined here... joining it today would add 6 more LEFT JOINs
-- against empty data... Extending this TVF once level 2-7 population
-- lands is a follow-up, not a redesign." tbl_Fact_KpSubLordChain is now
-- populated by both the CLI and Web composition roots (migration 130
-- added RuleSetId; Ikiastrro.Web/Program.cs registered the repository
-- 2026-09-22) -- that precondition has landed.
--
-- Not filtered by RuleSetId: no other join in this function rule-set-
-- filters either, and tbl_Rule_Sets currently has exactly one IsActive
-- row. Revisit if a second rule set is ever introduced.
--
-- No Web consumer of this TVF exists yet (CLI verification mode only,
-- per docs/research/domain/nakshatra-lord-sublord-dasha-crossref.md
-- SS4) -- this is a DB-slice-ahead-of-UI step, not a UI change.
--
-- Apply:  sqlcmd -S "localhost\SQLSERVER2025" -E -d ikiastrro -b -i db/131_extend_dasha_lord_relationship_l2_l7.sql
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

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
        kd.NakshatraSubLordPlanetId AS SubLordL1PlanetId, sl.PlanetName AS SubLordL1Name,
        kp2.LordPlanetId AS SubLordL2PlanetId, sl2.PlanetName AS SubLordL2Name,
        kp3.LordPlanetId AS SubLordL3PlanetId, sl3.PlanetName AS SubLordL3Name,
        kp4.LordPlanetId AS SubLordL4PlanetId, sl4.PlanetName AS SubLordL4Name,
        kp5.LordPlanetId AS SubLordL5PlanetId, sl5.PlanetName AS SubLordL5Name,
        kp6.LordPlanetId AS SubLordL6PlanetId, sl6.PlanetName AS SubLordL6Name,
        kp7.LordPlanetId AS SubLordL7PlanetId, sl7.PlanetName AS SubLordL7Name
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
    LEFT JOIN dbo.tbl_Fact_KpSubLordChain kp2 ON kp2.ChartResultId = d1Chart.ChartResultId
        AND kp2.PlanetId = dp.LordId AND kp2.Level = 2
    LEFT JOIN dbo.tbl_Planets sl2 ON sl2.Id = kp2.LordPlanetId
    LEFT JOIN dbo.tbl_Fact_KpSubLordChain kp3 ON kp3.ChartResultId = d1Chart.ChartResultId
        AND kp3.PlanetId = dp.LordId AND kp3.Level = 3
    LEFT JOIN dbo.tbl_Planets sl3 ON sl3.Id = kp3.LordPlanetId
    LEFT JOIN dbo.tbl_Fact_KpSubLordChain kp4 ON kp4.ChartResultId = d1Chart.ChartResultId
        AND kp4.PlanetId = dp.LordId AND kp4.Level = 4
    LEFT JOIN dbo.tbl_Planets sl4 ON sl4.Id = kp4.LordPlanetId
    LEFT JOIN dbo.tbl_Fact_KpSubLordChain kp5 ON kp5.ChartResultId = d1Chart.ChartResultId
        AND kp5.PlanetId = dp.LordId AND kp5.Level = 5
    LEFT JOIN dbo.tbl_Planets sl5 ON sl5.Id = kp5.LordPlanetId
    LEFT JOIN dbo.tbl_Fact_KpSubLordChain kp6 ON kp6.ChartResultId = d1Chart.ChartResultId
        AND kp6.PlanetId = dp.LordId AND kp6.Level = 6
    LEFT JOIN dbo.tbl_Planets sl6 ON sl6.Id = kp6.LordPlanetId
    LEFT JOIN dbo.tbl_Fact_KpSubLordChain kp7 ON kp7.ChartResultId = d1Chart.ChartResultId
        AND kp7.PlanetId = dp.LordId AND kp7.Level = 7
    LEFT JOIN dbo.tbl_Planets sl7 ON sl7.Id = kp7.LordPlanetId
    WHERE bd.Id = @BirthDetailId
);
GO

UPDATE dbo.tbl_Rule_Catalog
SET Purpose = 'Per-BirthDetailId TVF: each dasha period''s Lord planet cross-referenced against its own D1 Rasi, Rasi lord, Nakshatra, Nakshatra lord and KP sub-lord chain L1-L7.'
WHERE RuleTableName = 'tvf_Chart_DashaLordRelationship';
GO

INSERT dbo.SchemaMigrations (ScriptName, Note)
SELECT '131_extend_dasha_lord_relationship_l2_l7.sql',
       'tvf_Chart_DashaLordRelationship: extends the KP sub-lord cross-reference from L1 only to L1-L7, joining tbl_Fact_KpSubLordChain (migration 095/130) now that it is populated by both composition roots.'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '131_extend_dasha_lord_relationship_l2_l7.sql');
GO

PRINT '131 applied: tvf_Chart_DashaLordRelationship now includes KP sub-lord chain L1-L7.';
GO
