/* Isolated research corpus for house-lord-in-house interpretations (distinct rule family
   from planet-in-house, per docs/research/domain/house-placement.md "Scope and evidence").
   Nothing in this schema is consumed by chart calculation code. */
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'research')
    EXEC(N'CREATE SCHEMA research AUTHORIZATION dbo');

IF OBJECT_ID(N'research.tbl_Dim_SourceReferenceHouseLordInHouse', N'U') IS NULL
BEGIN
    CREATE TABLE research.tbl_Dim_SourceReferenceHouseLordInHouse
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SourceReferenceHouseLordInHouse PRIMARY KEY,
        OwnedHouseId INT NOT NULL,
        OccupiedHouseId INT NOT NULL,
        HouseSystemCode VARCHAR(20) NOT NULL CONSTRAINT DF_SourceReferenceHouseLordInHouse_System DEFAULT ('WHOLE_SIGN'),
        ReferencePointCode VARCHAR(20) NOT NULL CONSTRAINT DF_SourceReferenceHouseLordInHouse_RefPoint DEFAULT ('LAGNA'),
        ResearchStatus VARCHAR(20) NOT NULL CONSTRAINT DF_SourceReferenceHouseLordInHouse_Status DEFAULT ('Active'),
        CONSTRAINT FK_SourceReferenceHouseLordInHouse_OwnedHouse FOREIGN KEY (OwnedHouseId) REFERENCES research.tbl_Dim_SourceReferenceHouse(Id),
        CONSTRAINT FK_SourceReferenceHouseLordInHouse_OccupiedHouse FOREIGN KEY (OccupiedHouseId) REFERENCES research.tbl_Dim_SourceReferenceHouse(Id),
        CONSTRAINT UQ_SourceReferenceHouseLordInHouse UNIQUE (OwnedHouseId, OccupiedHouseId, HouseSystemCode, ReferencePointCode)
    );
END;

IF OBJECT_ID(N'research.tbl_Dim_SourceReferenceHouseLordInHouseText', N'U') IS NULL
BEGIN
    CREATE TABLE research.tbl_Dim_SourceReferenceHouseLordInHouseText
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SourceReferenceHouseLordInHouseText PRIMARY KEY,
        HouseLordInHouseId INT NOT NULL,
        SourceRefCode VARCHAR(80) NOT NULL,
        WorkTitle NVARCHAR(300) NOT NULL,
        Author NVARCHAR(200) NULL,
        Edition NVARCHAR(300) NULL,
        Chapter NVARCHAR(100) NULL,
        VerseOrPage NVARCHAR(100) NULL,
        LanguageCode VARCHAR(20) NULL,
        TextTypeCode VARCHAR(30) NOT NULL,
        FullText NVARCHAR(MAX) NULL,
        TranslationText NVARCHAR(MAX) NULL,
        Notes NVARCHAR(MAX) NULL,
        CopyrightStatus VARCHAR(30) NOT NULL,
        SourceLocator NVARCHAR(1000) NULL,
        SourceUrl NVARCHAR(1000) NULL,
        CONSTRAINT FK_SourceReferenceHouseLordInHouseText_Combination FOREIGN KEY (HouseLordInHouseId) REFERENCES research.tbl_Dim_SourceReferenceHouseLordInHouse(Id)
    );
END;

IF OBJECT_ID(N'research.tbl_Dim_SourceReferenceHouseLordInHouseAttribute', N'U') IS NULL
BEGIN
    CREATE TABLE research.tbl_Dim_SourceReferenceHouseLordInHouseAttribute
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SourceReferenceHouseLordInHouseAttribute PRIMARY KEY,
        HouseLordInHouseId INT NOT NULL,
        AttributeCode VARCHAR(100) NOT NULL,
        CategoryCode VARCHAR(40) NOT NULL,
        AttributeText NVARCHAR(1000) NOT NULL,
        PolarityCode VARCHAR(30) NULL,
        EvidenceLevelCode VARCHAR(30) NOT NULL,
        SourceTextId BIGINT NULL,
        SourceRefCode VARCHAR(80) NOT NULL,
        SourceLocator NVARCHAR(1000) NULL,
        ReviewerNotes NVARCHAR(MAX) NULL,
        CONSTRAINT FK_SourceReferenceHouseLordInHouseAttribute_Combination FOREIGN KEY (HouseLordInHouseId) REFERENCES research.tbl_Dim_SourceReferenceHouseLordInHouse(Id),
        CONSTRAINT FK_SourceReferenceHouseLordInHouseAttribute_Text FOREIGN KEY (SourceTextId) REFERENCES research.tbl_Dim_SourceReferenceHouseLordInHouseText(Id),
        CONSTRAINT UQ_SourceReferenceHouseLordInHouseAttribute UNIQUE (HouseLordInHouseId, AttributeCode, SourceRefCode)
    );
END;

IF OBJECT_ID(N'research.tbl_Dim_SourceReferenceHouseLordInHouseClaim', N'U') IS NULL
BEGIN
    CREATE TABLE research.tbl_Dim_SourceReferenceHouseLordInHouseClaim
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SourceReferenceHouseLordInHouseClaim PRIMARY KEY,
        HouseLordInHouseId INT NOT NULL,
        ClaimCode VARCHAR(100) NOT NULL,
        BranchCode VARCHAR(30) NOT NULL,
        ClaimText NVARCHAR(2000) NOT NULL,
        InterpretationText NVARCHAR(MAX) NULL,
        RequiredConditionsJson NVARCHAR(MAX) NULL,
        EvidenceLevelCode VARCHAR(30) NOT NULL,
        StatusCode VARCHAR(20) NOT NULL CONSTRAINT DF_SourceReferenceHouseLordInHouseClaim_Status DEFAULT ('Proposed'),
        SourceTextId BIGINT NULL,
        SourceRefCode VARCHAR(80) NOT NULL,
        CONSTRAINT FK_SourceReferenceHouseLordInHouseClaim_Combination FOREIGN KEY (HouseLordInHouseId) REFERENCES research.tbl_Dim_SourceReferenceHouseLordInHouse(Id),
        CONSTRAINT FK_SourceReferenceHouseLordInHouseClaim_Text FOREIGN KEY (SourceTextId) REFERENCES research.tbl_Dim_SourceReferenceHouseLordInHouseText(Id),
        CONSTRAINT UQ_SourceReferenceHouseLordInHouseClaim UNIQUE (HouseLordInHouseId, ClaimCode, SourceRefCode),
        CONSTRAINT CK_SourceReferenceHouseLordInHouseClaim_Branch CHECK (BranchCode IN ('BASELINE','WELL_DISPOSED','AFFLICTED'))
    );
END;

IF OBJECT_ID(N'research.tbl_Dim_SourceReferenceHouseLordInHouseCrosswalk', N'U') IS NULL
BEGIN
    CREATE TABLE research.tbl_Dim_SourceReferenceHouseLordInHouseCrosswalk
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SourceReferenceHouseLordInHouseCrosswalk PRIMARY KEY,
        ClaimId BIGINT NOT NULL,
        ProductionTargetCode VARCHAR(100) NOT NULL,
        PromotionStatus VARCHAR(20) NOT NULL CONSTRAINT DF_SourceReferenceHouseLordInHouseCrosswalk_Status DEFAULT ('Proposed'),
        ReviewedBy NVARCHAR(200) NULL,
        ReviewedAtUtc DATETIME2(0) NULL,
        PromotionMigration VARCHAR(100) NULL,
        CONSTRAINT FK_SourceReferenceHouseLordInHouseCrosswalk_Claim FOREIGN KEY (ClaimId) REFERENCES research.tbl_Dim_SourceReferenceHouseLordInHouseClaim(Id),
        CONSTRAINT UQ_SourceReferenceHouseLordInHouseCrosswalk UNIQUE (ClaimId, ProductionTargetCode)
    );
END;

IF OBJECT_ID(N'dbo.SchemaMigrations', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = N'089_create_research_house_lord_in_house_tables.sql')
    INSERT dbo.SchemaMigrations (ScriptName, Note)
    VALUES (N'089_create_research_house_lord_in_house_tables.sql', N'Create isolated research schema tables for house-lord-in-house source text, attributes, claims and promotion tracking (12x12 combinations, whole-sign/Lagna default).');
