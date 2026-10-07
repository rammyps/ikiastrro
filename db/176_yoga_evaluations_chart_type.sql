-- =====================================================================
-- 176 — vw_ChartYogaEvaluations exposes ChartType.
--
-- Per-varga yoga confirmation rows (D2/D3/D9/D12/D30; docs/cli/yoga-per-varga-design.md) are stored in
-- tbl_Fact_YogaInputEvaluations against each divisional chart's own ChartResultId, exactly like the D1 rows.
-- Every reader of this view that means "the full source-attributed set" now filters ChartType = 'D1'.
-- Additive: ChartType is appended as the last column.
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER VIEW dbo.vw_ChartYogaEvaluations
AS
SELECT c.BirthDetailId,b.Name,y.ChartResultId,y.RuleSetId,
       y.SourceRefCode,y.SourceVariantCode,y.YogaCode,y.SourceLocator,
       y.Present,y.EvaluationStatus,y.MissingRequirementCodesJson,
       y.SubjectSex,y.IsNightBirth,y.ElongationDegrees,
       y.IsWaxingMoon,y.IsFullMoon,y.LunarPhasePolicyCode,
       y.SunriseMethodCode,y.Notes,y.ComputedAtUtc,
       COALESCE(rSrc.FormationFamilyCode,rGen.FormationFamilyCode) AS YogaTypeCode,
       COALESCE(rSrc.ShortFormationRule,rGen.ShortFormationRule) AS YogaRule,
       v.YogaSetCode,s.DisplayName AS YogaSetName,v.DisplayName AS VariantDisplayName,
       COALESCE(rSrc.OutcomeNatureCode,rGen.OutcomeNatureCode) AS OutcomeNatureCode,
       COALESCE(rSrc.InferenceText,rGen.InferenceText) AS InferenceText,
       COALESCE(rSrc.InferenceSourceRefCode,rGen.InferenceSourceRefCode) AS InferenceSourceRefCode,
       COALESCE(rSrc.InferenceSourceLocator,rGen.InferenceSourceLocator) AS InferenceSourceLocator,
       c.ChartType
FROM dbo.tbl_Fact_YogaInputEvaluations y
JOIN dbo.tbl_ChartResults c ON c.Id=y.ChartResultId
JOIN dbo.tbl_BirthDetails b ON b.Id=c.BirthDetailId
LEFT JOIN dbo.tbl_Rule_Yoga rSrc ON rSrc.YogaCode=y.YogaCode AND rSrc.RuleSetId=y.RuleSetId AND rSrc.SourceRefCode=y.SourceRefCode
LEFT JOIN dbo.tbl_Rule_Yoga rGen ON rGen.YogaCode=y.YogaCode AND rGen.RuleSetId=y.RuleSetId AND rGen.SourceRefCode IS NULL
LEFT JOIN dbo.tbl_Rule_YogaVariant v ON v.RuleSetId=y.RuleSetId AND v.SourceRefCode=y.SourceRefCode AND v.SourceVariantCode=y.SourceVariantCode
LEFT JOIN dbo.tbl_Dim_YogaSets s ON s.Code=v.YogaSetCode;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName='176_yoga_evaluations_chart_type.sql')
    INSERT dbo.SchemaMigrations (ScriptName, Note)
    VALUES ('176_yoga_evaluations_chart_type.sql','vw_ChartYogaEvaluations exposes ChartType (per-varga yoga rows).');
GO
PRINT '176 applied: vw_ChartYogaEvaluations.ChartType.';
GO
