/* Isolated research corpus for the twelve bhava/house significations.
   Nothing in this schema is consumed by chart calculation code. */
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'research')
    EXEC(N'CREATE SCHEMA research AUTHORIZATION dbo');

IF OBJECT_ID(N'research.tbl_Dim_SourceReferenceHouse', N'U') IS NULL
BEGIN
    CREATE TABLE research.tbl_Dim_SourceReferenceHouse
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SourceReferenceHouse PRIMARY KEY,
        HouseNumber TINYINT NOT NULL,
        HouseCode VARCHAR(40) NOT NULL,
        SanskritName NVARCHAR(100) NULL,
        EnglishName NVARCHAR(100) NOT NULL,
        ResearchStatus VARCHAR(20) NOT NULL CONSTRAINT DF_SourceReferenceHouse_Status DEFAULT ('Active'),
        CONSTRAINT UQ_SourceReferenceHouse_Number UNIQUE (HouseNumber),
        CONSTRAINT UQ_SourceReferenceHouse_Code UNIQUE (HouseCode),
        CONSTRAINT CK_SourceReferenceHouse_Number CHECK (HouseNumber BETWEEN 1 AND 12)
    );
END;

IF OBJECT_ID(N'research.tbl_Dim_SourceReferenceHouseText', N'U') IS NULL
BEGIN
    CREATE TABLE research.tbl_Dim_SourceReferenceHouseText
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SourceReferenceHouseText PRIMARY KEY,
        HouseId INT NOT NULL,
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
        CONSTRAINT FK_SourceReferenceHouseText_House FOREIGN KEY (HouseId) REFERENCES research.tbl_Dim_SourceReferenceHouse(Id)
    );
END;

IF OBJECT_ID(N'research.tbl_Dim_SourceReferenceHouseAttribute', N'U') IS NULL
BEGIN
    CREATE TABLE research.tbl_Dim_SourceReferenceHouseAttribute
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SourceReferenceHouseAttribute PRIMARY KEY,
        HouseId INT NOT NULL,
        AttributeCode VARCHAR(100) NOT NULL,
        CategoryCode VARCHAR(40) NOT NULL,
        AttributeText NVARCHAR(1000) NOT NULL,
        PolarityCode VARCHAR(30) NULL,
        EvidenceLevelCode VARCHAR(30) NOT NULL,
        SourceTextId BIGINT NULL,
        SourceRefCode VARCHAR(80) NOT NULL,
        SourceLocator NVARCHAR(1000) NULL,
        ReviewerNotes NVARCHAR(MAX) NULL,
        CONSTRAINT FK_SourceReferenceHouseAttribute_House FOREIGN KEY (HouseId) REFERENCES research.tbl_Dim_SourceReferenceHouse(Id),
        CONSTRAINT FK_SourceReferenceHouseAttribute_Text FOREIGN KEY (SourceTextId) REFERENCES research.tbl_Dim_SourceReferenceHouseText(Id),
        CONSTRAINT UQ_SourceReferenceHouseAttribute UNIQUE (HouseId, AttributeCode, SourceRefCode)
    );
END;

IF OBJECT_ID(N'research.tbl_Dim_SourceReferenceHouseClaim', N'U') IS NULL
BEGIN
    CREATE TABLE research.tbl_Dim_SourceReferenceHouseClaim
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SourceReferenceHouseClaim PRIMARY KEY,
        HouseId INT NOT NULL,
        ClaimCode VARCHAR(100) NOT NULL,
        ClaimText NVARCHAR(2000) NOT NULL,
        InterpretationText NVARCHAR(MAX) NULL,
        RequiredConditionsJson NVARCHAR(MAX) NULL,
        EvidenceLevelCode VARCHAR(30) NOT NULL,
        StatusCode VARCHAR(20) NOT NULL CONSTRAINT DF_SourceReferenceHouseClaim_Status DEFAULT ('Proposed'),
        SourceTextId BIGINT NULL,
        SourceRefCode VARCHAR(80) NOT NULL,
        CONSTRAINT FK_SourceReferenceHouseClaim_House FOREIGN KEY (HouseId) REFERENCES research.tbl_Dim_SourceReferenceHouse(Id),
        CONSTRAINT FK_SourceReferenceHouseClaim_Text FOREIGN KEY (SourceTextId) REFERENCES research.tbl_Dim_SourceReferenceHouseText(Id),
        CONSTRAINT UQ_SourceReferenceHouseClaim UNIQUE (HouseId, ClaimCode, SourceRefCode)
    );
END;

IF OBJECT_ID(N'research.tbl_Dim_SourceReferenceHouseCrosswalk', N'U') IS NULL
BEGIN
    CREATE TABLE research.tbl_Dim_SourceReferenceHouseCrosswalk
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SourceReferenceHouseCrosswalk PRIMARY KEY,
        ClaimId BIGINT NOT NULL,
        ProductionTargetCode VARCHAR(100) NOT NULL,
        PromotionStatus VARCHAR(20) NOT NULL CONSTRAINT DF_SourceReferenceHouseCrosswalk_Status DEFAULT ('Proposed'),
        ReviewedBy NVARCHAR(200) NULL,
        ReviewedAtUtc DATETIME2(0) NULL,
        PromotionMigration VARCHAR(100) NULL,
        CONSTRAINT FK_SourceReferenceHouseCrosswalk_Claim FOREIGN KEY (ClaimId) REFERENCES research.tbl_Dim_SourceReferenceHouseClaim(Id),
        CONSTRAINT UQ_SourceReferenceHouseCrosswalk UNIQUE (ClaimId, ProductionTargetCode)
    );
END;

IF OBJECT_ID(N'dbo.SchemaMigrations', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = N'058_create_research_house_significance_tables.sql')
    INSERT dbo.SchemaMigrations (ScriptName, Note)
    VALUES (N'058_create_research_house_significance_tables.sql', N'Create isolated research schema tables for the twelve bhava house significations and promotion tracking.');
