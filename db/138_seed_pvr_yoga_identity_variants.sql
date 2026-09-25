-- 138 — Complete the five PVR Chapter 11 identity/predicate variants left after migration 137.
USE [ikiastrro];
GO
SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS(SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName='138_seed_pvr_yoga_identity_variants.sql')
BEGIN
    DECLARE @rs TINYINT=(SELECT TOP(1) Id FROM dbo.tbl_Rule_Sets WHERE IsActive=1 ORDER BY VersionNumber DESC,Id DESC);
    IF @rs IS NULL THROW 50139,'Migration 138 requires an active rule set.',1;

    DECLARE @rules TABLE(YogaCode VARCHAR(40),Family VARCHAR(30),ShortRule NVARCHAR(500),Variant VARCHAR(60),Name NVARCHAR(150),Locator VARCHAR(120));
    INSERT @rules VALUES
    ('YOGA_BASIC_RAJA','COMBINATION',N'A distinct kendra lord and trikona lord conjoin, mutually aspect, or exchange signs','PVR_CH11_BASIC_RAJA',N'Basic Raja Yoga','ch.11 §11.7.1; pp.133-134'),
    ('YOGA_HARI','COMBINATION',N'Natural benefics occupy the 2nd, 12th and 8th signs counted from the 2nd lord','PVR_CH11_HARI',N'Hari Yoga','ch.11 §11.6; p.129'),
    ('YOGA_HARA','COMBINATION',N'Natural benefics occupy the 4th, 9th and 8th signs counted from the 7th lord','PVR_CH11_HARA',N'Hara Yoga','ch.11 §11.6; p.129'),
    ('YOGA_BRAHMA_TRIMURTI','COMBINATION',N'Natural benefics occupy the 4th, 10th and 11th signs counted from the Lagna lord','PVR_CH11_BRAHMA_TRIMURTI',N'Brahma Yoga — Trimurti form','ch.11 §11.6; pp.129-130'),
    ('YOGA_PARIJATHA','LAGNA',N'Lagna lord, its dispositor, and that planet''s Rasi and Navamsa dispositors are each in kendra, trikona or exaltation','PVR_CH11_KALPADRUMA',N'Kalpadruma (Parijata) Yoga','ch.11 §11.6; pp.127-129');

    INSERT dbo.tbl_Rule_Yoga
        (RuleSetId,YogaCode,FormationFamilyCode,ShortFormationRule,SourceRefCode,SourceLocator,
         SourceVariantCode,SourceCategoryCode,SourceCorpusCode,MethodCode,IsActive)
    SELECT @rs,r.YogaCode,r.Family,r.ShortRule,'SRC_PVR_INTEGRATED',r.Locator,r.Variant,
           CASE WHEN r.YogaCode='YOGA_BASIC_RAJA' THEN 'RAJA_BASIC' ELSE 'POPULAR' END,
           'PVR-SPECIFIC','PREDICATE',1
    FROM @rules r
    WHERE NOT EXISTS(SELECT 1 FROM dbo.tbl_Rule_Yoga y WHERE y.RuleSetId=@rs AND y.SourceRefCode='SRC_PVR_INTEGRATED' AND y.SourceVariantCode=r.Variant);

    INSERT dbo.tbl_Rule_YogaVariant
        (RuleSetId,SourceRefCode,SourceVariantCode,YogaCode,YogaSetCode,SequenceNumber,
         DisplayName,SourceLocator,RuleRoleCode,IdentityRelationCode,EvaluationStatus,Notes)
    SELECT @rs,'SRC_PVR_INTEGRATED',r.Variant,r.YogaCode,
           CASE WHEN r.YogaCode='YOGA_BASIC_RAJA' THEN 'PVR_RAJA_BASIC' ELSE 'PVR_POPULAR' END,
           CASE r.YogaCode WHEN 'YOGA_BASIC_RAJA' THEN 1 WHEN 'YOGA_HARI' THEN 20 WHEN 'YOGA_HARA' THEN 21 WHEN 'YOGA_BRAHMA_TRIMURTI' THEN 22 ELSE 18 END,
           r.Name,r.Locator,'FORMATION',
           CASE WHEN r.YogaCode='YOGA_PARIJATHA' THEN 'ALIAS' ELSE 'INDEPENDENT' END,
           'EVALUATED',CASE WHEN r.YogaCode='YOGA_BRAHMA_TRIMURTI' THEN N'Distinct from PVR''s second Brahma variation already represented by YOGA_BRAHMA.' END
    FROM @rules r
    WHERE NOT EXISTS(SELECT 1 FROM dbo.tbl_Rule_YogaVariant y WHERE y.RuleSetId=@rs AND y.SourceRefCode='SRC_PVR_INTEGRATED' AND y.SourceVariantCode=r.Variant);

    DECLARE @d1 TINYINT=(SELECT Id FROM dbo.tbl_Dim_ChartType WHERE Code='D1');
    DECLARE @d9 TINYINT=(SELECT Id FROM dbo.tbl_Dim_ChartType WHERE Code='D9');
    IF @d1 IS NULL OR @d9 IS NULL THROW 50140,'Migration 138 requires D1 and D9.',1;
    INSERT dbo.tbl_Rule_YogaChartApplicability(RuleSetId,SourceRefCode,SourceVariantCode,ChartTypeId,RequirementRole,EvaluationScope,MissingBehavior,Notes)
    SELECT @rs,'SRC_PVR_INTEGRATED',r.Variant,@d1,'FOUNDATION','NATAL','NOT_EVALUATED',N'PVR Chapter 11 foundational chart.' FROM @rules r
    WHERE NOT EXISTS(SELECT 1 FROM dbo.tbl_Rule_YogaChartApplicability a WHERE a.RuleSetId=@rs AND a.SourceRefCode='SRC_PVR_INTEGRATED' AND a.SourceVariantCode=r.Variant AND a.ChartTypeId=@d1);
    INSERT dbo.tbl_Rule_YogaChartApplicability(RuleSetId,SourceRefCode,SourceVariantCode,ChartTypeId,RequirementRole,EvaluationScope,MissingBehavior,Notes)
    SELECT @rs,'SRC_PVR_INTEGRATED','PVR_CH11_KALPADRUMA',@d9,'REQUIRED','DIVISIONAL','NOT_EVALUATED',N'D9 identifies the Navamsa dispositor in the fourth link.'
    WHERE NOT EXISTS(SELECT 1 FROM dbo.tbl_Rule_YogaChartApplicability a WHERE a.RuleSetId=@rs AND a.SourceRefCode='SRC_PVR_INTEGRATED' AND a.SourceVariantCode='PVR_CH11_KALPADRUMA' AND a.ChartTypeId=@d9);

    DECLARE @deps TABLE(Variant VARCHAR(60),Code VARCHAR(40),Role VARCHAR(20));
    INSERT @deps VALUES
    ('PVR_CH11_BASIC_RAJA','LORDSHIP','REQUIRED'),('PVR_CH11_BASIC_RAJA','GRAHA_DRISHTI','QUALIFYING'),('PVR_CH11_BASIC_RAJA','EXCHANGE','QUALIFYING'),
    ('PVR_CH11_HARI','LORDSHIP','ANCHOR'),('PVR_CH11_HARA','LORDSHIP','ANCHOR'),('PVR_CH11_BRAHMA_TRIMURTI','LORDSHIP','ANCHOR'),
    ('PVR_CH11_KALPADRUMA','DISPOSITOR','REQUIRED'),('PVR_CH11_KALPADRUMA','MULTI_VARGA','REQUIRED');
    INSERT dbo.tbl_Dim_YogaDependencies(Code,DisplayName,KindCode,Description)
    SELECT * FROM (VALUES
    ('LORDSHIP',N'House lordship','CALCULATION',N'House rulers are required.'),
    ('GRAHA_DRISHTI',N'Graha drishti','CALCULATION',N'Planetary aspect is a qualifying association.'),
    ('EXCHANGE',N'Parivartana','CALCULATION',N'Mutual sign exchange is a qualifying association.'),
    ('DISPOSITOR',N'Dispositor chain','CALCULATION',N'One or more sign-dispositor links are required.'))v(Code,DisplayName,KindCode,Description)
    WHERE NOT EXISTS(SELECT 1 FROM dbo.tbl_Dim_YogaDependencies d WHERE d.Code=v.Code);
    INSERT dbo.tbl_Rule_YogaVariantDependency(YogaVariantId,YogaDependencyCode,RequirementRole)
    SELECT y.Id,d.Code,d.Role FROM @deps d JOIN dbo.tbl_Rule_YogaVariant y ON y.RuleSetId=@rs AND y.SourceRefCode='SRC_PVR_INTEGRATED' AND y.SourceVariantCode=d.Variant
    WHERE NOT EXISTS(SELECT 1 FROM dbo.tbl_Rule_YogaVariantDependency x WHERE x.YogaVariantId=y.Id AND x.YogaDependencyCode=d.Code);

    INSERT dbo.SchemaMigrations(ScriptName,Note) VALUES
    ('138_seed_pvr_yoga_identity_variants.sql','Basic Raja, Hari, Hara, first Brahma/Trimurti form, and Kalpadruma/Parijata PVR variants.');
END
GO
