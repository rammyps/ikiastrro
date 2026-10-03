-- =====================================================================
-- 168 - Versioned, contract-gated D1 analytics feature view.
--
-- SQL is the first publication boundary. Only explicitly eligible research
-- subjects with exactly one D1 chart and a complete valid twelve-house v2
-- feature set are exposed. Python validates the result again independently.
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER VIEW dbo.vw_AnalyticsHouseFeaturesV2
AS
WITH SingleD1Chart AS (
    SELECT BirthDetailId, MIN(Id) AS ChartResultId
    FROM dbo.tbl_ChartResults
    WHERE ChartType = 'D1'
    GROUP BY BirthDetailId
    HAVING COUNT(*) = 1
),
CompleteFeatureSet AS (
    SELECT stats.ChartResultId
    FROM dbo.tbl_Fact_HouseStrengthStatistics stats
    JOIN dbo.tbl_ChartResults chart ON chart.Id = stats.ChartResultId
    GROUP BY stats.ChartResultId, chart.RuleSetId
    HAVING COUNT(*) = 12
       AND COUNT(DISTINCT stats.HouseFromLagna) = 12
       AND MIN(stats.HouseFromLagna) = 1
       AND MAX(stats.HouseFromLagna) = 12
       AND COUNT(stats.Capacity) = 12
       AND COUNT(stats.Consistency) = 12
       AND COUNT(stats.Context) = 12
       AND COUNT(stats.StrengthPercent) = 12
       AND MIN(stats.Capacity) >= 0 AND MAX(stats.Capacity) <= 100
       AND MIN(stats.Consistency) >= 0 AND MAX(stats.Consistency) <= 100
       AND MIN(stats.Context) >= 0 AND MAX(stats.Context) <= 100
       AND MIN(stats.StrengthPercent) >= 0 AND MAX(stats.StrengthPercent) <= 100
       AND MIN(stats.RuleSetId) = MAX(stats.RuleSetId)
       AND MIN(stats.RuleSetId) = chart.RuleSetId
)
SELECT subject.SubjectKey,
       stats.ChartResultId,
       chart.ChartType,
       stats.HouseFromLagna,
       stats.Capacity,
       stats.Consistency,
       stats.Context,
       stats.StrengthPercent AS OverallSupport,
       stats.RuleSetId,
       chart.Ayanamsha,
       chart.HouseSystem,
       chart.EngineVersion,
       subject.BirthTimeQuality,
       stats.ComputedAtUtc,
       CAST('KI_D1_HOUSE_STRENGTH_V2' AS VARCHAR(32)) AS FeatureContractVersion
FROM dbo.tbl_Dim_AnalyticsSubjects subject
JOIN SingleD1Chart selected ON selected.BirthDetailId = subject.BirthDetailId
JOIN dbo.tbl_ChartResults chart ON chart.Id = selected.ChartResultId
JOIN CompleteFeatureSet complete ON complete.ChartResultId = chart.Id
JOIN dbo.tbl_Fact_HouseStrengthStatistics stats ON stats.ChartResultId = chart.Id
WHERE subject.ResearchUseStatus = 'ELIGIBLE'
  AND subject.SubjectClassification = 'RESEARCH'
  AND subject.ConsentRecordedAtUtc IS NOT NULL
  AND subject.WithdrawnAtUtc IS NULL;
GO

-- Compatibility name follows the current approved contract instead of
-- preserving a second, weaker cohort definition.
CREATE OR ALTER VIEW dbo.vw_AnalyticsHouseFeatures
AS
SELECT SubjectKey, ChartResultId, ChartType, HouseFromLagna,
       Capacity, Consistency, Context, OverallSupport, RuleSetId,
       Ayanamsha, HouseSystem, EngineVersion, BirthTimeQuality,
       ComputedAtUtc, FeatureContractVersion
FROM dbo.vw_AnalyticsHouseFeaturesV2;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '168_create_versioned_analytics_house_view.sql')
    INSERT dbo.SchemaMigrations (ScriptName, AppliedAtUtc, Note)
    VALUES ('168_create_versioned_analytics_house_view.sql', SYSUTCDATETIME(),
            'Adds v2 D1 analytics view gated to consented, single-chart, complete twelve-house feature sets.');
GO

PRINT '168 applied: versioned analytics house-feature view.';
GO
