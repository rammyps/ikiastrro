-- =====================================================================
-- 172 - v10 Life Matter + Varga analytics feature contract v1.
--
-- Publishes canonical house-strength statistics at the grain anonymous
-- subject x life-matter focus x evidence lens. D1 promise and subject-Varga
-- confirmation stay separate; this view never averages or reconciles them.
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER VIEW dbo.vw_AnalyticsLifeMatterFeaturesV1
AS
WITH EligibleSubject AS (
    SELECT SubjectKey, BirthDetailId, BirthTimeQuality
    FROM dbo.tbl_Dim_AnalyticsSubjects
    WHERE ResearchUseStatus = 'ELIGIBLE'
      AND SubjectClassification IN ('RESEARCH', 'PERSONAL')
      AND WithdrawnAtUtc IS NULL
),
UniqueChart AS (
    SELECT BirthDetailId, ChartType, MIN(Id) AS ChartResultId
    FROM dbo.tbl_ChartResults
    GROUP BY BirthDetailId, ChartType
    HAVING COUNT(*) = 1
),
MatterFocus AS (
    SELECT mapping.RuleSetId AS MappingRuleSetId,
           matter.Id AS LifeMatterId,
           matter.Code AS LifeMatterCode,
           matter.EnglishName AS LifeMatterName,
           matter.CategoryCode,
           matter.CategoryName,
           focus.Id AS LifeMatterFocusId,
           focus.Priority AS FocusPriority,
           focus.FocusKind,
           focus.ReferenceCode,
           focus.HouseNumber,
           focus.SpecialPointCode,
           mapping.DivisionalSubjectCode,
           divisional.SubjectName AS DivisionalSubjectName,
           chartType.Code AS ConfirmationChartType
    FROM dbo.tbl_Rule_LifeMatterSubject mapping
    JOIN dbo.tbl_Dim_LifeMatter matter
      ON matter.Id = mapping.LifeMatterId AND matter.IsActive = 1
    JOIN dbo.tbl_Dim_DivisionalSubject divisional
      ON divisional.SubjectCode = mapping.DivisionalSubjectCode
    JOIN dbo.tbl_Dim_ChartType chartType
      ON chartType.Id = divisional.PrimaryConfirmationChartId
    JOIN dbo.tbl_Rule_LifeMatterFocus focus
      ON focus.RuleSetId = mapping.RuleSetId
     AND focus.LifeMatterId = mapping.LifeMatterId
     AND focus.IsActive = 1
    WHERE mapping.IsActive = 1
),
Observation AS (
    SELECT subject.SubjectKey,
           subject.BirthDetailId,
           subject.BirthTimeQuality,
           matter.*,
           lens.EvidenceLensCode,
           lens.ChartType
    FROM EligibleSubject subject
    CROSS JOIN MatterFocus matter
    CROSS APPLY (VALUES
        (CAST('D1_PROMISE' AS VARCHAR(24)), CAST('D1' AS VARCHAR(10))),
        (CAST('VARGA_CONFIRMATION' AS VARCHAR(24)), matter.ConfirmationChartType)
    ) lens (EvidenceLensCode, ChartType)
)
SELECT observation.SubjectKey,
       observation.LifeMatterId,
       observation.LifeMatterCode,
       observation.LifeMatterName,
       observation.CategoryCode,
       observation.CategoryName,
       observation.LifeMatterFocusId,
       observation.FocusPriority,
       observation.FocusKind,
       observation.ReferenceCode,
       observation.HouseNumber,
       observation.SpecialPointCode,
       observation.DivisionalSubjectCode,
       observation.DivisionalSubjectName,
       observation.ConfirmationChartType,
       observation.EvidenceLensCode,
       observation.ChartType,
       chart.ChartResultId,
       stats.Capacity,
       stats.Consistency,
       stats.Context,
       stats.StrengthPercent AS OverallSupport,
       observation.MappingRuleSetId,
       stats.RuleSetId AS StatisticsRuleSetId,
       result.RuleSetId AS ChartRuleSetId,
       result.Ayanamsha,
       result.HouseSystem,
       result.EngineVersion,
       observation.BirthTimeQuality,
       stats.ComputedAtUtc,
       CASE
           WHEN observation.FocusKind <> 'House'
             OR observation.ReferenceCode <> 'LAGNA'
             OR observation.HouseNumber IS NULL
               THEN CAST('UNSUPPORTED_REFERENCE' AS VARCHAR(32))
           WHEN chart.ChartResultId IS NULL
               THEN CAST('CHART_NOT_AVAILABLE' AS VARCHAR(32))
           WHEN stats.ChartResultId IS NULL
               THEN CAST('HOUSE_STATISTICS_NOT_AVAILABLE' AS VARCHAR(32))
           WHEN stats.Capacity IS NULL OR stats.Consistency IS NULL
             OR stats.Context IS NULL OR stats.StrengthPercent IS NULL
               THEN CAST('SOURCE_PARTIAL' AS VARCHAR(32))
           ELSE NULL
       END AS MissingReasonCode,
       CAST('KI_LIFE_MATTER_VARGA_V1' AS VARCHAR(32)) AS FeatureContractVersion
FROM Observation observation
LEFT JOIN UniqueChart chart
  ON chart.BirthDetailId = observation.BirthDetailId
 AND chart.ChartType = observation.ChartType
LEFT JOIN dbo.tbl_ChartResults result ON result.Id = chart.ChartResultId
LEFT JOIN dbo.tbl_Fact_HouseStrengthStatistics stats
  ON stats.ChartResultId = chart.ChartResultId
 AND stats.HouseFromLagna = observation.HouseNumber
 AND observation.FocusKind = 'House'
 AND observation.ReferenceCode = 'LAGNA';
GO

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_Catalog WHERE RuleTableName = 'vw_AnalyticsLifeMatterFeaturesV1')
    INSERT dbo.tbl_Rule_Catalog (RuleTableName, EngineCode, MethodCodes, Purpose, IntroducedIn)
    VALUES ('vw_AnalyticsLifeMatterFeaturesV1', 'STATISTICS', 'LIFE_MATTER_VARGA_V1',
            'Privacy-gated long-form D1 promise and subject-Varga house-strength observations for mapped life matters.',
            'migration 172');
GO

IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '172_create_analytics_life_matter_features.sql')
    INSERT dbo.SchemaMigrations (ScriptName, AppliedAtUtc, Note)
    VALUES ('172_create_analytics_life_matter_features.sql', SYSUTCDATETIME(),
            'Adds privacy-gated v10 Life Matter feature view with separate D1/Varga lenses and explicit missing reasons.');
GO

PRINT '172 applied: v10 Life Matter + Varga analytics feature contract v1.';
GO
