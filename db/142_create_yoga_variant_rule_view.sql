-- 142 — Expose source variants with their concept-level yoga rule without duplicating concepts.
USE [ikiastrro];
GO
SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS(SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName='142_create_yoga_variant_rule_view.sql')
BEGIN
    INSERT dbo.SchemaMigrations(ScriptName,Note) VALUES
    ('142_create_yoga_variant_rule_view.sql','Variant-to-concept yoga rule projection; preserves one concept row per YogaCode/source.');
END
GO

CREATE OR ALTER VIEW dbo.vw_YogaVariantRules
AS
SELECT v.RuleSetId,v.SourceRefCode,v.SourceVariantCode,v.YogaCode,v.YogaSetCode,
       v.SequenceNumber,v.DisplayName,v.SourceLocator,v.RuleRoleCode,v.IdentityRelationCode,
       v.EvaluationStatus,v.Notes AS VariantNotes,
       y.Id AS SourceRuleId,COALESCE(y.FormationFamilyCode,c.FormationFamilyCode) AS FormationFamilyCode,
       COALESCE(y.ShortFormationRule,c.ShortFormationRule) AS ShortFormationRule,
       COALESCE(y.CalculationNarrative,c.CalculationNarrative) AS CalculationNarrative,
       CASE WHEN y.Id IS NOT NULL THEN 'SOURCE' WHEN c.Id IS NOT NULL THEN 'CANONICAL' ELSE 'UNRESOLVED' END AS ConceptRuleResolution
FROM dbo.tbl_Rule_YogaVariant v
LEFT JOIN dbo.tbl_Rule_Yoga y ON y.RuleSetId=v.RuleSetId AND y.YogaCode=v.YogaCode AND y.SourceRefCode=v.SourceRefCode
LEFT JOIN dbo.tbl_Rule_Yoga c ON c.RuleSetId=v.RuleSetId AND c.YogaCode=v.YogaCode AND c.SourceRefCode IS NULL;
GO
