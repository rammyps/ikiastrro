-- 139 — Backfill migration-137 inventory for the original PVR variants and Vipareeta Raja.
USE [ikiastrro];
GO
SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS(SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName='139_backfill_existing_pvr_yoga_variants.sql')
BEGIN
    DECLARE @rs TINYINT=(SELECT TOP(1) Id FROM dbo.tbl_Rule_Sets WHERE IsActive=1 ORDER BY VersionNumber DESC,Id DESC);
    DECLARE @d1 TINYINT=(SELECT Id FROM dbo.tbl_Dim_ChartType WHERE Code='D1');
    IF @rs IS NULL OR @d1 IS NULL THROW 50141,'Migration 139 requires an active rule set and D1.',1;

    DECLARE @v TABLE(Variant VARCHAR(60),Yoga VARCHAR(40),SetCode VARCHAR(40),Seq SMALLINT,Name NVARCHAR(150),Locator VARCHAR(120));
    INSERT @v VALUES
    ('PVR_CH11_VESI','YOGA_VESI','PVR_RAVI',1,N'Vesi Yoga','ch.11 §11.2.1'),
    ('PVR_CH11_VOSI','YOGA_VASI','PVR_RAVI',2,N'Vosi Yoga','ch.11 §11.2.2'),
    ('PVR_CH11_UBHAYACHARA','YOGA_UBHAYACHARI','PVR_RAVI',3,N'Ubhayachara Yoga','ch.11 §11.2.3'),
    ('PVR_CH11_BUDHA_ADITYA','YOGA_BUDHA_ADITYA','PVR_RAVI',4,N'Budha-Aaditya (Nipuna) Yoga','ch.11 §11.2.4'),
    ('PVR_CH11_SUNAPHA','YOGA_SUNAPHA','PVR_CHANDRA',1,N'Sunapha Yoga','ch.11 §11.3.1'),
    ('PVR_CH11_ANAPHA','YOGA_ANAPHA','PVR_CHANDRA',2,N'Anapha Yoga','ch.11 §11.3.2'),
    ('PVR_CH11_DURADHARA','YOGA_DURADHARA','PVR_CHANDRA',3,N'Duradhara Yoga','ch.11 §11.3.3'),
    ('PVR_CH11_KEMADRUMA','YOGA_KEMADRUMA','PVR_CHANDRA',4,N'Kemadruma Yoga','ch.11 §11.3.4'),
    ('PVR_CH11_CHANDRA_MANGALA','YOGA_CHANDRA_MANGALA','PVR_CHANDRA',5,N'Chandra-Mangala Yoga','ch.11 §11.3.5'),
    ('PVR_CH11_ADHI','YOGA_ADHI','PVR_CHANDRA',6,N'Adhi Yoga','ch.11 §11.3.6'),
    ('PVR_CH11_RUCHAKA','YOGA_RUCHAKA','PVR_MAHAPURUSHA',1,N'Ruchaka Yoga','ch.11 §11.4.1'),
    ('PVR_CH11_BHADRA','YOGA_BHADRA','PVR_MAHAPURUSHA',2,N'Bhadra Yoga','ch.11 §11.4.2'),
    ('PVR_CH11_SASA','YOGA_SASA','PVR_MAHAPURUSHA',3,N'Sasa Yoga','ch.11 §11.4.3'),
    ('PVR_CH11_MALAVYA','YOGA_MALAVYA','PVR_MAHAPURUSHA',4,N'Malavya Yoga','ch.11 §11.4.4'),
    ('PVR_CH11_HAMSA','YOGA_HAMSA','PVR_MAHAPURUSHA',5,N'Hamsa Yoga','ch.11 §11.4.5'),
    ('PVR_CH11_GAJAKESARI','YOGA_GAJAKESARI','PVR_POPULAR',3,N'Gaja-Kesari Yoga','ch.11 §11.6'),
    ('PVR_CH11_AMALA','YOGA_AMALA','PVR_POPULAR',5,N'Amala Yoga','ch.11 §11.6'),
    ('PVR_CH11_PARVATA','YOGA_PARVATA','PVR_POPULAR',6,N'Parvata Yoga','ch.11 §11.6'),
    ('PVR_CH11_VIPAREETA_RAJA','YOGA_VIPAREETA_RAJA','PVR_RAJA_BASIC',3,N'Vipareeta Raja Yoga','ch.11 §11.7.1; pp.134-135');

    INSERT dbo.tbl_Rule_YogaVariant
        (RuleSetId,SourceRefCode,SourceVariantCode,YogaCode,YogaSetCode,SequenceNumber,DisplayName,
         SourceLocator,RuleRoleCode,IdentityRelationCode,EvaluationStatus)
    SELECT @rs,'SRC_PVR_INTEGRATED',v.Variant,v.Yoga,v.SetCode,v.Seq,v.Name,v.Locator,
           'FORMATION','SOURCE_VARIANT','EVALUATED'
    FROM @v v
    WHERE NOT EXISTS(SELECT 1 FROM dbo.tbl_Rule_YogaVariant x WHERE x.RuleSetId=@rs AND x.SourceRefCode='SRC_PVR_INTEGRATED' AND x.SourceVariantCode=v.Variant);

    INSERT dbo.tbl_Rule_YogaChartApplicability
        (RuleSetId,SourceRefCode,SourceVariantCode,ChartTypeId,RequirementRole,EvaluationScope,MissingBehavior,Notes)
    SELECT @rs,'SRC_PVR_INTEGRATED',v.Variant,@d1,'FOUNDATION','NATAL','NOT_EVALUATED',N'PVR Chapter 11 foundational chart.'
    FROM @v v
    WHERE NOT EXISTS(SELECT 1 FROM dbo.tbl_Rule_YogaChartApplicability a WHERE a.RuleSetId=@rs AND a.SourceRefCode='SRC_PVR_INTEGRATED' AND a.SourceVariantCode=v.Variant AND a.ChartTypeId=@d1);

    INSERT dbo.tbl_Dim_YogaDependencies(Code,DisplayName,KindCode,Description)
    SELECT * FROM (VALUES
    ('SUN',N'Sun','ANCHOR',N'Reckoning or conjunction from the Sun.'),
    ('MOON',N'Moon','ANCHOR',N'Reckoning or conjunction from the Moon.'),
    ('LAGNA',N'Lagna','ANCHOR',N'Reckoning from the ascendant.'),
    ('DIGNITY',N'Planetary dignity','CALCULATION',N'Own, exaltation or relationship dignity is required.'),
    ('COMBUSTION',N'Combustion','CALCULATION',N'Combustion qualifies the source result.'),
    ('NATURAL_NATURE',N'Natural benefic/malefic nature','CALCULATION',N'Natural planetary nature is required.'))v(Code,DisplayName,KindCode,Description)
    WHERE NOT EXISTS(SELECT 1 FROM dbo.tbl_Dim_YogaDependencies d WHERE d.Code=v.Code);

    DECLARE @t TABLE(Variant VARCHAR(60),Code VARCHAR(40),Role VARCHAR(20));
    INSERT @t SELECT Variant,'SUN','ANCHOR' FROM @v WHERE SetCode='PVR_RAVI';
    INSERT @t SELECT Variant,'MOON','ANCHOR' FROM @v WHERE SetCode='PVR_CHANDRA' OR Variant IN('PVR_CH11_GAJAKESARI','PVR_CH11_AMALA');
    INSERT @t SELECT Variant,'LAGNA','ANCHOR' FROM @v WHERE SetCode IN('PVR_MAHAPURUSHA','PVR_RAJA_BASIC') OR Variant IN('PVR_CH11_AMALA','PVR_CH11_PARVATA');
    INSERT @t SELECT Variant,'DIGNITY','REQUIRED' FROM @v WHERE SetCode='PVR_MAHAPURUSHA' OR Variant='PVR_CH11_GAJAKESARI';
    INSERT @t VALUES('PVR_CH11_BUDHA_ADITYA','COMBUSTION','QUALIFYING'),('PVR_CH11_GAJAKESARI','COMBUSTION','QUALIFYING'),
    ('PVR_CH11_ADHI','NATURAL_NATURE','REQUIRED'),('PVR_CH11_AMALA','NATURAL_NATURE','REQUIRED'),('PVR_CH11_PARVATA','NATURAL_NATURE','REQUIRED');
    INSERT dbo.tbl_Rule_YogaVariantDependency(YogaVariantId,YogaDependencyCode,RequirementRole)
    SELECT y.Id,t.Code,t.Role FROM @t t JOIN dbo.tbl_Rule_YogaVariant y ON y.RuleSetId=@rs AND y.SourceRefCode='SRC_PVR_INTEGRATED' AND y.SourceVariantCode=t.Variant
    WHERE NOT EXISTS(SELECT 1 FROM dbo.tbl_Rule_YogaVariantDependency x WHERE x.YogaVariantId=y.Id AND x.YogaDependencyCode=t.Code);

    INSERT dbo.SchemaMigrations(ScriptName,Note) VALUES
    ('139_backfill_existing_pvr_yoga_variants.sql','Inventory and dependency tags for the 18 original PVR variants plus Vipareeta Raja.');
END
GO
