-- =====================================================================
-- 169 - Personal-app analytics mode.
--
-- Keep structural feature checks, but allow explicitly prepared PERSONAL
-- subjects and do not gate extraction on chart/statistics rule-set identity.
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
    GROUP BY stats.ChartResultId
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
  AND subject.SubjectClassification IN ('RESEARCH', 'PERSONAL')
  AND subject.WithdrawnAtUtc IS NULL;
GO

CREATE OR ALTER VIEW dbo.vw_AnalyticsCohortReadiness
AS
SELECT RuleSetId,
       COUNT(DISTINCT SubjectKey) AS EligiblePeople,
       COUNT(DISTINCT SubjectKey) AS CompletePeople,
       MIN(ComputedAtUtc) AS OldestFeatureAtUtc,
       MAX(ComputedAtUtc) AS NewestFeatureAtUtc
FROM dbo.vw_AnalyticsHouseFeaturesV2
GROUP BY RuleSetId;
GO


IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '169_enable_personal_analytics_mode.sql')
    INSERT dbo.SchemaMigrations (ScriptName, AppliedAtUtc, Note)
    VALUES ('169_enable_personal_analytics_mode.sql', SYSUTCDATETIME(),
            'Allows explicitly prepared personal subjects; retains complete twelve-house feature checks without rule-set matching.');
GO

PRINT '169 applied: personal analytics mode enabled.';
GO
