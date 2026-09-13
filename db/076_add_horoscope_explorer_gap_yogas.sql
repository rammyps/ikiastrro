USE [ikiastrro];
GO
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Dim_Source WHERE Code='SRC_HOROSCOPE_EXPLORER') INSERT dbo.tbl_Dim_Source(Code,Title,Author,Edition,Tradition,Notes) VALUES('SRC_HOROSCOPE_EXPLORER',N'Horoscope Explorer comparison output',NULL,N'Ramakrishnan P report screenshots, 2026-09-10','Comparison',N'Parity evidence only. Effects do not establish a classical formation rule.');
GO
DECLARE @ruleSetId TINYINT=(SELECT TOP(1) Id FROM dbo.tbl_Rule_Sets WHERE IsActive=1 ORDER BY VersionNumber DESC),@d1 TINYINT=(SELECT Id FROM dbo.tbl_Dim_ChartType WHERE Code='D1');DECLARE @v TABLE(SourceRefCode VARCHAR(60),SourceVariantCode VARCHAR(60),Note NVARCHAR(500));
INSERT @v VALUES('SRC_PVR_INTEGRATED','PVR_CH11_VIPAREETA_RAJA',N'PVR broad Vipareeta Raja definition; D1 foundation.'),('SRC_HOROSCOPE_EXPLORER','HE_ANIVAHUPPU_STRUCTURAL',N'Comparison-source structural predicate; D1 foundation.'),('SRC_HOROSCOPE_EXPLORER','HE_VIDYA_NATAL',N'Catalogued but not evaluated until a natal predicate is sourced.'),('SRC_HOROSCOPE_EXPLORER','HE_ARISHTA_GENERIC',N'Catalogued but not evaluated until the specific Arishta predicate is identified.');
INSERT dbo.tbl_Rule_YogaChartApplicability(RuleSetId,SourceRefCode,SourceVariantCode,ChartTypeId,RequirementRole,EvaluationScope,MissingBehavior,Notes) SELECT @ruleSetId,v.SourceRefCode,v.SourceVariantCode,@d1,'FOUNDATION','NATAL','NOT_EVALUATED',v.Note FROM @v v WHERE NOT EXISTS(SELECT 1 FROM dbo.tbl_Rule_YogaChartApplicability x WHERE x.RuleSetId=@ruleSetId AND x.SourceRefCode=v.SourceRefCode AND x.SourceVariantCode=v.SourceVariantCode AND x.ChartTypeId=@d1);
GO
INSERT dbo.SchemaMigrations(ScriptName,Note) SELECT '076_add_horoscope_explorer_gap_yogas.sql','Register PVR Vipareeta Raja and Horoscope Explorer parity variants.' WHERE NOT EXISTS(SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName='076_add_horoscope_explorer_gap_yogas.sql');
GO
