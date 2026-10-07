-- =====================================================================
-- 175 — PVR variants of Sarpa and Mridanga (SRC_PVR_INTEGRATED).
--
-- Found by diffing JHora's View > Yogas list for RamakrishnanP against ikiastrro: JHora lists both,
-- ikiastrro had only the Raman variants, whose definitions differ (Raman 102: all 3 natural malefics in
-- kendras; Raman 046: exalted graha's navamsa dispositor in a kendra). JHora's wording is PVR's:
--   Sarpa    §11.5.2 p.120 — "three quadrants are occupied by natural malefics"
--   Mridanga §11.6   p.126 — "planets in own and exaltation signs in quadrants and trines" + strong lagna lord
-- Source-specific tbl_Rule_Yoga rows (mechanism: migration 119) so each source keeps its own definition.
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '175_pvr_sarpa_mridanga_variants.sql')
BEGIN
    DECLARE @rs TINYINT = (SELECT TOP(1) Id FROM dbo.tbl_Rule_Sets WHERE IsActive=1 ORDER BY VersionNumber DESC, Id DESC);
    DECLARE @d1 TINYINT = (SELECT Id FROM dbo.tbl_Dim_ChartType WHERE Code='D1');
    IF @rs IS NULL OR @d1 IS NULL THROW 50175, 'Migration 175 requires an active rule set and D1.', 1;

    INSERT dbo.tbl_Rule_Yoga (RuleSetId, YogaCode, SourceRefCode, FormationFamilyCode, ShortFormationRule)
    SELECT @rs, v.YogaCode, 'SRC_PVR_INTEGRATED', 'LAGNA', v.RuleText
    FROM (VALUES
        ('YOGA_SARPA',    'Three of the four kendras occupied by natural malefics (nodes included)'),
        ('YOGA_MRIDANGA', 'A planet in own or exaltation sign in a kendra or trikona, with a strong lagna lord')
    ) v (YogaCode, RuleText)
    WHERE NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_Yoga x
                      WHERE x.RuleSetId=@rs AND x.YogaCode=v.YogaCode AND x.SourceRefCode='SRC_PVR_INTEGRATED');

    DECLARE @v TABLE (Variant VARCHAR(60), Yoga VARCHAR(40), SetCode VARCHAR(40), Seq SMALLINT, Name NVARCHAR(150), Locator VARCHAR(120));
    INSERT @v VALUES
    ('PVR_CH11_SARPA',    'YOGA_SARPA',    'PVR_NABHASA_DALA', 2,  N'Sarpa Yoga',    'ch.11 §11.5.2; p.120'),
    ('PVR_CH11_MRIDANGA', 'YOGA_MRIDANGA', 'PVR_POPULAR',      14, N'Mridanga Yoga', 'ch.11 §11.6; p.126');

    INSERT dbo.tbl_Rule_YogaVariant
        (RuleSetId,SourceRefCode,SourceVariantCode,YogaCode,YogaSetCode,SequenceNumber,DisplayName,
         SourceLocator,RuleRoleCode,IdentityRelationCode,EvaluationStatus)
    SELECT @rs,'SRC_PVR_INTEGRATED',v.Variant,v.Yoga,v.SetCode,v.Seq,v.Name,v.Locator,'FORMATION','SOURCE_VARIANT','EVALUATED'
    FROM @v v
    WHERE NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_YogaVariant x
                      WHERE x.RuleSetId=@rs AND x.SourceRefCode='SRC_PVR_INTEGRATED' AND x.SourceVariantCode=v.Variant);

    INSERT dbo.tbl_Rule_YogaChartApplicability
        (RuleSetId,SourceRefCode,SourceVariantCode,ChartTypeId,RequirementRole,EvaluationScope,MissingBehavior,Notes)
    SELECT @rs,'SRC_PVR_INTEGRATED',v.Variant,@d1,'FOUNDATION','NATAL','NOT_EVALUATED',N'PVR Chapter 11 foundational chart.'
    FROM @v v
    WHERE NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_YogaChartApplicability x
                      WHERE x.RuleSetId=@rs AND x.SourceRefCode='SRC_PVR_INTEGRATED'
                        AND x.SourceVariantCode=v.Variant AND x.ChartTypeId=@d1);

    INSERT dbo.SchemaMigrations (ScriptName, Note)
    VALUES ('175_pvr_sarpa_mridanga_variants.sql', 'PVR Sarpa (§11.5.2) and Mridanga (§11.6) variants; source-specific rule rows.');
END
GO

PRINT '175 applied: PVR Sarpa and Mridanga variants registered.';
GO
