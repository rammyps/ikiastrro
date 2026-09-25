-- 137 — PVR Chapter 11 source-variant inventory and subdivision foundation.
USE [ikiastrro];
GO

IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName='137_create_yoga_variant_subdivision.sql')
BEGIN
    IF OBJECT_ID('dbo.tbl_Dim_YogaSets','U') IS NULL
    CREATE TABLE dbo.tbl_Dim_YogaSets
    (
        Code VARCHAR(40) NOT NULL CONSTRAINT PK_Dim_YogaSets PRIMARY KEY,
        ParentCode VARCHAR(40) NULL,
        DisplayName NVARCHAR(100) NOT NULL,
        SourceRefCode VARCHAR(40) NULL,
        SourceLocator VARCHAR(120) NULL,
        DisplayOrder SMALLINT NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Dim_YogaSets_IsActive DEFAULT(1),
        CONSTRAINT FK_Dim_YogaSets_Parent FOREIGN KEY(ParentCode) REFERENCES dbo.tbl_Dim_YogaSets(Code),
        CONSTRAINT FK_Dim_YogaSets_Source FOREIGN KEY(SourceRefCode) REFERENCES dbo.tbl_Dim_Source(Code)
    );

    IF OBJECT_ID('dbo.tbl_Dim_YogaDependencies','U') IS NULL
    CREATE TABLE dbo.tbl_Dim_YogaDependencies
    (
        Code VARCHAR(40) NOT NULL CONSTRAINT PK_Dim_YogaDependencies PRIMARY KEY,
        DisplayName NVARCHAR(100) NOT NULL,
        KindCode VARCHAR(20) NOT NULL,
        Description NVARCHAR(500) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Dim_YogaDependencies_IsActive DEFAULT(1),
        CONSTRAINT CK_Dim_YogaDependencies_Kind CHECK(KindCode IN('CALCULATION','ANCHOR','CONTEXT'))
    );

    IF OBJECT_ID('dbo.tbl_Rule_YogaVariant','U') IS NULL
    CREATE TABLE dbo.tbl_Rule_YogaVariant
    (
        Id INT IDENTITY(1,1) CONSTRAINT PK_Rule_YogaVariant PRIMARY KEY,
        RuleSetId TINYINT NOT NULL,
        SourceRefCode VARCHAR(40) NOT NULL,
        SourceVariantCode VARCHAR(60) NOT NULL,
        YogaCode VARCHAR(40) NOT NULL,
        YogaSetCode VARCHAR(40) NOT NULL,
        SequenceNumber SMALLINT NULL,
        DisplayName NVARCHAR(150) NULL,
        SourceLocator VARCHAR(120) NOT NULL,
        RuleRoleCode VARCHAR(24) NOT NULL CONSTRAINT DF_Rule_YogaVariant_Role DEFAULT('FORMATION'),
        IdentityRelationCode VARCHAR(24) NOT NULL CONSTRAINT DF_Rule_YogaVariant_Identity DEFAULT('SOURCE_VARIANT'),
        EvaluationStatus VARCHAR(24) NOT NULL,
        Notes NVARCHAR(1000) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Rule_YogaVariant_IsActive DEFAULT(1),
        CONSTRAINT FK_Rule_YogaVariant_RuleSet FOREIGN KEY(RuleSetId) REFERENCES dbo.tbl_Rule_Sets(Id),
        CONSTRAINT FK_Rule_YogaVariant_Source FOREIGN KEY(SourceRefCode) REFERENCES dbo.tbl_Dim_Source(Code),
        CONSTRAINT FK_Rule_YogaVariant_Set FOREIGN KEY(YogaSetCode) REFERENCES dbo.tbl_Dim_YogaSets(Code),
        CONSTRAINT UQ_Rule_YogaVariant UNIQUE(RuleSetId,SourceRefCode,SourceVariantCode),
        CONSTRAINT CK_Rule_YogaVariant_Role CHECK(RuleRoleCode IN('FORMATION','QUALIFICATION','CANCELLATION','MAGNITUDE','EFFECTIVENESS','OUTCOME')),
        CONSTRAINT CK_Rule_YogaVariant_Identity CHECK(IdentityRelationCode IN('INDEPENDENT','ALIAS','SOURCE_VARIANT','ALTERNATE','UMBRELLA','QUALIFICATION')),
        CONSTRAINT CK_Rule_YogaVariant_Status CHECK(EvaluationStatus IN('EVALUATED','PARTIAL','NOT_EVALUATED','PLANNED'))
    );

    IF OBJECT_ID('dbo.tbl_Rule_YogaVariantDependency','U') IS NULL
    CREATE TABLE dbo.tbl_Rule_YogaVariantDependency
    (
        Id INT IDENTITY(1,1) CONSTRAINT PK_Rule_YogaVariantDependency PRIMARY KEY,
        YogaVariantId INT NOT NULL,
        YogaDependencyCode VARCHAR(40) NOT NULL,
        RequirementRole VARCHAR(20) NOT NULL CONSTRAINT DF_Rule_YogaVariantDependency_Role DEFAULT('REQUIRED'),
        CONSTRAINT FK_Rule_YogaVariantDependency_Variant FOREIGN KEY(YogaVariantId) REFERENCES dbo.tbl_Rule_YogaVariant(Id),
        CONSTRAINT FK_Rule_YogaVariantDependency_Dependency FOREIGN KEY(YogaDependencyCode) REFERENCES dbo.tbl_Dim_YogaDependencies(Code),
        CONSTRAINT UQ_Rule_YogaVariantDependency UNIQUE(YogaVariantId,YogaDependencyCode),
        CONSTRAINT CK_Rule_YogaVariantDependency_Role CHECK(RequirementRole IN('REQUIRED','ANCHOR','QUALIFYING','OPTIONAL'))
    );

    INSERT dbo.SchemaMigrations(ScriptName,Note) VALUES
    ('137_create_yoga_variant_subdivision.sql','PVR Chapter 11 set hierarchy, source-variant inventory, and Chara Karaka dependency tags.');
END
GO

INSERT dbo.tbl_Rule_Catalog(RuleTableName,EngineCode,MethodCodes,Purpose,IntroducedIn)
SELECT v.RuleTableName,'YOGA',v.MethodCodes,v.Purpose,'migration 137'
FROM (VALUES
('tbl_Rule_YogaVariant','SOURCE_VARIANT,SET_CLASSIFICATION',N'One inventory row per source-specific yoga predicate or modifier.'),
('tbl_Rule_YogaVariantDependency','DEPENDENCY_TAG',N'Calculation dependencies and reference anchors for a source-specific yoga variant.')
)v(RuleTableName,MethodCodes,Purpose)
WHERE NOT EXISTS(SELECT 1 FROM dbo.tbl_Rule_Catalog x WHERE x.RuleTableName=v.RuleTableName);
GO

INSERT dbo.tbl_Dim_YogaSets(Code,ParentCode,DisplayName,SourceRefCode,SourceLocator,DisplayOrder)
SELECT v.Code,v.ParentCode,v.DisplayName,'SRC_PVR_INTEGRATED',v.Locator,v.Sort
FROM (VALUES
('PVR_CH11',NULL,N'PVR Chapter 11 Yogas','ch.11',100),
('PVR_RAVI','PVR_CH11',N'Ravi Yogas','ch.11 §11.2',110),
('PVR_CHANDRA','PVR_CH11',N'Chandra Yogas','ch.11 §11.3',120),
('PVR_MAHAPURUSHA','PVR_CH11',N'Pancha Mahapurusha Yogas','ch.11 §11.4',130),
('PVR_NABHASA','PVR_CH11',N'Nabhasa Yogas','ch.11 §11.5',140),
('PVR_NABHASA_AASRAYA','PVR_NABHASA',N'Nabhasa — Aasraya','ch.11 §11.5.1',141),
('PVR_NABHASA_DALA','PVR_NABHASA',N'Nabhasa — Dala','ch.11 §11.5.2',142),
('PVR_NABHASA_AAKRITI','PVR_NABHASA',N'Nabhasa — Aakriti','ch.11 §11.5.3',143),
('PVR_NABHASA_SANKHYA','PVR_NABHASA',N'Nabhasa — Sankhya','ch.11 §11.5.4',144),
('PVR_POPULAR','PVR_CH11',N'Other Popular Yogas','ch.11 §11.6',150),
('PVR_RAJA_BASIC','PVR_CH11',N'Raja Yogas — Basics','ch.11 §11.7.1',160),
('PVR_RAJA_ADVANCED','PVR_CH11',N'Advanced Raja Yogas','ch.11 §11.7.3',170),
('PVR_RAJA_SAMBANDHA','PVR_CH11',N'Raja Sambandha Yogas','ch.11 §11.8',180),
('PVR_DHANA','PVR_CH11',N'Dhana Yogas','ch.11 §11.9',190),
('PVR_DARIDRA','PVR_CH11',N'Daridra Yogas','ch.11 §11.10',200)
)v(Code,ParentCode,DisplayName,Locator,Sort)
WHERE NOT EXISTS(SELECT 1 FROM dbo.tbl_Dim_YogaSets x WHERE x.Code=v.Code);

INSERT dbo.tbl_Dim_YogaDependencies(Code,DisplayName,KindCode,Description)
SELECT * FROM (VALUES
('CHARA_KARAKA',N'Chara Karaka assignment','CALCULATION',N'One or more Jaimini Chara Karakas are required.'),
('AK',N'Atmakaraka (AK)','ANCHOR',N'Placement or reckoning from AK.'),
('AMK',N'Amatyakaraka (AmK)','ANCHOR',N'Placement or reckoning from AmK.'),
('PK',N'Putrakaraka (PK)','ANCHOR',N'Placement or reckoning from PK.'),
('ARUDHA',N'Arudha Pada','CALCULATION',N'AL, A7 or A9 is required.'),
('SPECIAL_LAGNA',N'Special Lagna','CALCULATION',N'HL, GL or another special Lagna is required.'),
('MULTI_VARGA',N'Multiple divisional charts','CALCULATION',N'The predicate compares multiple charts.'),
('ASHTAKAVARGA',N'Ashtakavarga classification','CALCULATION',N'An Ashtakavarga-derived classification is required.')
)v(Code,DisplayName,KindCode,Description)
WHERE NOT EXISTS(SELECT 1 FROM dbo.tbl_Dim_YogaDependencies x WHERE x.Code=v.Code);
GO

DECLARE @rs TINYINT=(SELECT TOP(1) Id FROM dbo.tbl_Rule_Sets WHERE IsActive=1 ORDER BY VersionNumber DESC,Id DESC);
IF @rs IS NULL THROW 50137,'Migration 137 requires an active rule set.',1;
DECLARE @v TABLE(Variant VARCHAR(60),Yoga VARCHAR(40),SetCode VARCHAR(40),Seq SMALLINT,Name NVARCHAR(150),Locator VARCHAR(120),Role VARCHAR(24),Status VARCHAR(24),Notes NVARCHAR(1000));
INSERT @v VALUES
('PVR_CH11_MAALA','YOGA_MAALA','PVR_NABHASA_DALA',1,N'Maala Yoga','ch.11 §11.5.2; pp.119-120','FORMATION','EVALUATED',NULL),
('PVR_CH11_SUBHA','YOGA_SUBHA','PVR_POPULAR',1,N'Subha Yoga','ch.11 §11.6; p.124','FORMATION','EVALUATED',NULL),
('PVR_CH11_ASUBHA','YOGA_ASUBHA','PVR_POPULAR',2,N'Asubha Yoga','ch.11 §11.6; p.124','FORMATION','EVALUATED',NULL),
('PVR_CH11_GURU_MANGALA','YOGA_GURU_MANGALA','PVR_POPULAR',4,N'Guru-Mangala Yoga','ch.11 §11.6; p.125','FORMATION','EVALUATED',NULL),
('PVR_CH11_CHAMARA','YOGA_CHAMARA','PVR_POPULAR',8,N'Chaamara Yoga','ch.11 §11.6; pp.125-126','FORMATION','EVALUATED',NULL),
('PVR_CH11_KHADGA','YOGA_KHADGA','PVR_POPULAR',15,N'Khadga Yoga','ch.11 §11.6; pp.126-127','FORMATION','EVALUATED',NULL),
('PVR_CH11_LAGNAADHI','YOGA_LAGNAADHI','PVR_POPULAR',19,N'Lagnaadhi Yoga','ch.11 §11.6; p.129','FORMATION','EVALUATED',NULL),
('PVR_CH11_SAARADA','YOGA_SAARADA','PVR_POPULAR',29,N'Saarada Yoga','ch.11 §11.6; p.131','FORMATION','EVALUATED',NULL),
('PVR_CH11_DHARMA_KARMADHIPATI','YOGA_DHARMA_KARMADHIPATI','PVR_RAJA_BASIC',2,N'Dharma-Karmadhipati Yoga','ch.11 §11.7.1; p.134','FORMATION','EVALUATED',NULL);

DECLARE @n INT=1;
WHILE @n<=18 BEGIN INSERT @v VALUES(CONCAT('PVR_CH11_RAJA_ADV_',RIGHT(CONCAT('00',@n),2)),'YOGA_RAJA_ADVANCED','PVR_RAJA_ADVANCED',@n,CONCAT(N'Advanced Raja Yoga ',@n),'ch.11 §11.7.3; pp.137-139',CASE WHEN @n=18 THEN 'EFFECTIVENESS' ELSE 'FORMATION' END,'EVALUATED',CASE WHEN @n=18 THEN N'Modifies other Raja Yogas; not an independent formation.' END); SET @n+=1; END;
SET @n=1;
WHILE @n<=15 BEGIN INSERT @v VALUES(CONCAT('PVR_CH11_RAJA_SAMBANDHA_',RIGHT(CONCAT('00',@n),2)),'YOGA_RAJA_SAMBANDHA','PVR_RAJA_SAMBANDHA',@n,CONCAT(N'Raja Sambandha Yoga ',@n),'ch.11 §11.8; pp.139-140','FORMATION','EVALUATED',NULL); SET @n+=1; END;
INSERT @v VALUES('PVR_CH11_DHANA_BASIC','YOGA_DHANA','PVR_DHANA',0,N'Dhana basic principle','ch.11 §11.9; p.141','QUALIFICATION','EVALUATED',NULL);
DECLARE @s TABLE(N SMALLINT,Suffix VARCHAR(12),Name NVARCHAR(30));
INSERT @s VALUES(1,'ARIES',N'Aries'),(2,'TAURUS',N'Taurus'),(3,'GEMINI',N'Gemini'),(4,'CANCER',N'Cancer'),(5,'LEO',N'Leo'),(6,'VIRGO',N'Virgo'),(7,'LIBRA',N'Libra'),(8,'SCORPIO',N'Scorpio'),(9,'SAGITTARIUS',N'Sagittarius'),(10,'CAPRICORNUS',N'Capricornus'),(11,'AQUARIUS',N'Aquarius'),(12,'PISCES',N'Pisces');
INSERT @v SELECT CONCAT('PVR_CH11_DHANA_',Suffix),'YOGA_DHANA','PVR_DHANA',N,CONCAT(N'Dhana Yoga — ',Name,N' Lagna'),'ch.11 §11.9; pp.141-142','FORMATION',CASE WHEN N=12 THEN 'PARTIAL' ELSE 'EVALUATED' END,CASE WHEN N=12 THEN N'First alternative is textually damaged; only the second is coded.' END FROM @s;
SET @n=1;
WHILE @n<=13 BEGIN INSERT @v VALUES(CONCAT('PVR_CH11_DARIDRA_',RIGHT(CONCAT('00',@n),2)),'YOGA_DARIDRA','PVR_DARIDRA',@n,CONCAT(N'Daridra Yoga ',@n),'ch.11 §11.10; pp.142-143','FORMATION',CASE WHEN @n=10 THEN 'NOT_EVALUATED' ELSE 'EVALUATED' END,CASE WHEN @n=10 THEN N'Requires PVR Ashtakavarga benefic/malefic-house classification.' END); SET @n+=1; END;

INSERT dbo.tbl_Rule_YogaVariant(RuleSetId,SourceRefCode,SourceVariantCode,YogaCode,YogaSetCode,SequenceNumber,DisplayName,SourceLocator,RuleRoleCode,IdentityRelationCode,EvaluationStatus,Notes)
SELECT @rs,'SRC_PVR_INTEGRATED',v.Variant,v.Yoga,v.SetCode,v.Seq,v.Name,v.Locator,v.Role,'SOURCE_VARIANT',v.Status,v.Notes FROM @v v
WHERE NOT EXISTS(SELECT 1 FROM dbo.tbl_Rule_YogaVariant x WHERE x.RuleSetId=@rs AND x.SourceRefCode='SRC_PVR_INTEGRATED' AND x.SourceVariantCode=v.Variant);

DECLARE @d1 TINYINT=(SELECT Id FROM dbo.tbl_Dim_ChartType WHERE Code='D1');
IF @d1 IS NULL THROW 50138,'Migration 137 requires D1.',1;
INSERT dbo.tbl_Rule_YogaChartApplicability(RuleSetId,SourceRefCode,SourceVariantCode,ChartTypeId,RequirementRole,EvaluationScope,MissingBehavior,Notes)
SELECT @rs,'SRC_PVR_INTEGRATED',v.Variant,@d1,'FOUNDATION','NATAL','NOT_EVALUATED',N'PVR Chapter 11 foundational chart.' FROM @v v
WHERE NOT EXISTS(SELECT 1 FROM dbo.tbl_Rule_YogaChartApplicability x WHERE x.RuleSetId=@rs AND x.SourceRefCode='SRC_PVR_INTEGRATED' AND x.SourceVariantCode=v.Variant AND x.ChartTypeId=@d1);

DECLARE @c TABLE(Variant VARCHAR(60),Ak BIT,Amk BIT,Pk BIT);
INSERT @c VALUES
('PVR_CH11_RAJA_ADV_01',1,0,1),('PVR_CH11_RAJA_ADV_02',1,0,1),('PVR_CH11_RAJA_ADV_03',1,0,0),('PVR_CH11_RAJA_ADV_04',1,0,0),('PVR_CH11_RAJA_ADV_05',1,0,0),
('PVR_CH11_RAJA_SAMBANDHA_01',0,1,0),('PVR_CH11_RAJA_SAMBANDHA_03',1,1,0),('PVR_CH11_RAJA_SAMBANDHA_04',0,1,0),('PVR_CH11_RAJA_SAMBANDHA_05',0,1,0),('PVR_CH11_RAJA_SAMBANDHA_06',1,1,0),('PVR_CH11_RAJA_SAMBANDHA_07',1,0,0),('PVR_CH11_RAJA_SAMBANDHA_08',1,0,0),('PVR_CH11_RAJA_SAMBANDHA_09',1,0,0),('PVR_CH11_RAJA_SAMBANDHA_10',1,0,0),('PVR_CH11_RAJA_SAMBANDHA_11',1,0,0),('PVR_CH11_RAJA_SAMBANDHA_12',1,0,0),('PVR_CH11_RAJA_SAMBANDHA_14',1,0,0),('PVR_CH11_RAJA_SAMBANDHA_15',1,0,0);
DECLARE @t TABLE(Variant VARCHAR(60),Dependency VARCHAR(40),Role VARCHAR(20));
INSERT @t SELECT Variant,'CHARA_KARAKA','REQUIRED' FROM @c;
INSERT @t SELECT Variant,'AK','ANCHOR' FROM @c WHERE Ak=1;
INSERT @t SELECT Variant,'AMK','ANCHOR' FROM @c WHERE Amk=1;
INSERT @t SELECT Variant,'PK','ANCHOR' FROM @c WHERE Pk=1;
INSERT @t VALUES('PVR_CH11_RAJA_ADV_06','SPECIAL_LAGNA','REQUIRED'),('PVR_CH11_RAJA_ADV_07','MULTI_VARGA','REQUIRED'),('PVR_CH11_RAJA_ADV_08','SPECIAL_LAGNA','REQUIRED'),('PVR_CH11_RAJA_ADV_09','MULTI_VARGA','REQUIRED'),('PVR_CH11_RAJA_ADV_18','ARUDHA','REQUIRED'),('PVR_CH11_RAJA_SAMBANDHA_07','ARUDHA','REQUIRED'),('PVR_CH11_RAJA_SAMBANDHA_11','ARUDHA','REQUIRED'),('PVR_CH11_DARIDRA_10','ASHTAKAVARGA','REQUIRED');
INSERT dbo.tbl_Rule_YogaVariantDependency(YogaVariantId,YogaDependencyCode,RequirementRole)
SELECT y.Id,t.Dependency,t.Role FROM @t t JOIN dbo.tbl_Rule_YogaVariant y ON y.RuleSetId=@rs AND y.SourceRefCode='SRC_PVR_INTEGRATED' AND y.SourceVariantCode=t.Variant
WHERE NOT EXISTS(SELECT 1 FROM dbo.tbl_Rule_YogaVariantDependency x WHERE x.YogaVariantId=y.Id AND x.YogaDependencyCode=t.Dependency);
GO

CREATE OR ALTER VIEW dbo.vw_YogaVariantInventory AS
SELECT v.RuleSetId,v.SourceRefCode,v.SourceVariantCode,v.YogaCode,v.YogaSetCode,s.ParentCode ParentYogaSetCode,s.DisplayName YogaSetName,v.SequenceNumber,v.DisplayName,v.SourceLocator,v.RuleRoleCode,v.IdentityRelationCode,v.EvaluationStatus,v.Notes,v.IsActive
FROM dbo.tbl_Rule_YogaVariant v JOIN dbo.tbl_Dim_YogaSets s ON s.Code=v.YogaSetCode;
GO
