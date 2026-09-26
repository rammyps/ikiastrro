-- 148 — Expose yoga set/variant metadata in chart evaluations and permit variant-level copy.
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF COL_LENGTH('dbo.tbl_Content_Interpretation','SourceVariantCode') IS NULL
    ALTER TABLE dbo.tbl_Content_Interpretation ADD SourceVariantCode VARCHAR(60) NULL;
GO

IF NOT EXISTS(SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName='148_expose_yoga_variants_and_interpretations.sql')
BEGIN
    IF EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.tbl_Content_Interpretation') AND name='UQ_Content_Interpretation_RuleSet_Subject')
        DROP INDEX UQ_Content_Interpretation_RuleSet_Subject ON dbo.tbl_Content_Interpretation;
    IF EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.tbl_Content_Interpretation') AND name='UQ_Content_Interpretation_RuleSet_Subject_Source')
        DROP INDEX UQ_Content_Interpretation_RuleSet_Subject_Source ON dbo.tbl_Content_Interpretation;

    CREATE UNIQUE INDEX UQ_Content_Interpretation_RuleSet_Subject
        ON dbo.tbl_Content_Interpretation(RuleSetId,SubjectType,SubjectCode)
        WHERE SourceRefCode IS NULL AND SourceVariantCode IS NULL;
    CREATE UNIQUE INDEX UQ_Content_Interpretation_RuleSet_Subject_Source
        ON dbo.tbl_Content_Interpretation(RuleSetId,SubjectType,SubjectCode,SourceRefCode)
        WHERE SourceRefCode IS NOT NULL AND SourceVariantCode IS NULL;
    CREATE UNIQUE INDEX UQ_Content_Interpretation_RuleSet_Subject_Variant
        ON dbo.tbl_Content_Interpretation(RuleSetId,SubjectType,SubjectCode,SourceRefCode,SourceVariantCode)
        WHERE SourceRefCode IS NOT NULL AND SourceVariantCode IS NOT NULL;

    INSERT dbo.SchemaMigrations(ScriptName,Note) VALUES
    ('148_expose_yoga_variants_and_interpretations.sql','Yoga evaluation UI metadata and source-variant interpretation key.');
END
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
       v.YogaSetCode,s.DisplayName AS YogaSetName,v.DisplayName AS VariantDisplayName
FROM dbo.tbl_Fact_YogaInputEvaluations y
JOIN dbo.tbl_ChartResults c ON c.Id=y.ChartResultId
JOIN dbo.tbl_BirthDetails b ON b.Id=c.BirthDetailId
LEFT JOIN dbo.tbl_Rule_Yoga rSrc ON rSrc.YogaCode=y.YogaCode AND rSrc.RuleSetId=y.RuleSetId AND rSrc.SourceRefCode=y.SourceRefCode
LEFT JOIN dbo.tbl_Rule_Yoga rGen ON rGen.YogaCode=y.YogaCode AND rGen.RuleSetId=y.RuleSetId AND rGen.SourceRefCode IS NULL
LEFT JOIN dbo.tbl_Rule_YogaVariant v ON v.RuleSetId=y.RuleSetId AND v.SourceRefCode=y.SourceRefCode AND v.SourceVariantCode=y.SourceVariantCode
LEFT JOIN dbo.tbl_Dim_YogaSets s ON s.Code=v.YogaSetCode;
GO
