/* Isolated research corpus for planet-in-house interpretations.
   Nothing in this schema is consumed by chart calculation code. */
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'research')
    EXEC(N'CREATE SCHEMA research AUTHORIZATION dbo');

IF OBJECT_ID(N'research.tbl_Dim_SourceReferencePlanetInHouse', N'U') IS NULL
BEGIN
    CREATE TABLE research.tbl_Dim_SourceReferencePlanetInHouse
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SourceReferencePlanetInHouse PRIMARY KEY,
        PlanetId INT NOT NULL,
        HouseId INT NOT NULL,
        ResearchStatus VARCHAR(20) NOT NULL CONSTRAINT DF_SourceReferencePlanetInHouse_Status DEFAULT ('Active'),
        CONSTRAINT FK_SourceReferencePlanetInHouse_Planet FOREIGN KEY (PlanetId) REFERENCES research.tbl_Dim_SourceReferencePlanet(Id),
        CONSTRAINT FK_SourceReferencePlanetInHouse_House FOREIGN KEY (HouseId) REFERENCES research.tbl_Dim_SourceReferenceHouse(Id),
        CONSTRAINT UQ_SourceReferencePlanetInHouse UNIQUE (PlanetId, HouseId)
    );
END;

IF OBJECT_ID(N'research.tbl_Dim_SourceReferencePlanetInHouseText', N'U') IS NULL
BEGIN
    CREATE TABLE research.tbl_Dim_SourceReferencePlanetInHouseText
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SourceReferencePlanetInHouseText PRIMARY KEY,
        PlanetInHouseId INT NOT NULL,
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
        CONSTRAINT FK_SourceReferencePlanetInHouseText_Combination FOREIGN KEY (PlanetInHouseId) REFERENCES research.tbl_Dim_SourceReferencePlanetInHouse(Id)
    );
END;

IF OBJECT_ID(N'research.tbl_Dim_SourceReferencePlanetInHouseAttribute', N'U') IS NULL
BEGIN
    CREATE TABLE research.tbl_Dim_SourceReferencePlanetInHouseAttribute
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SourceReferencePlanetInHouseAttribute PRIMARY KEY,
        PlanetInHouseId INT NOT NULL,
        AttributeCode VARCHAR(100) NOT NULL,
        CategoryCode VARCHAR(40) NOT NULL,
        AttributeText NVARCHAR(1000) NOT NULL,
        PolarityCode VARCHAR(30) NULL,
        EvidenceLevelCode VARCHAR(30) NOT NULL,
        SourceTextId BIGINT NULL,
        SourceRefCode VARCHAR(80) NOT NULL,
        SourceLocator NVARCHAR(1000) NULL,
        ReviewerNotes NVARCHAR(MAX) NULL,
        CONSTRAINT FK_SourceReferencePlanetInHouseAttribute_Combination FOREIGN KEY (PlanetInHouseId) REFERENCES research.tbl_Dim_SourceReferencePlanetInHouse(Id),
        CONSTRAINT FK_SourceReferencePlanetInHouseAttribute_Text FOREIGN KEY (SourceTextId) REFERENCES research.tbl_Dim_SourceReferencePlanetInHouseText(Id),
        CONSTRAINT UQ_SourceReferencePlanetInHouseAttribute UNIQUE (PlanetInHouseId, AttributeCode, SourceRefCode)
    );
END;

IF OBJECT_ID(N'research.tbl_Dim_SourceReferencePlanetInHouseClaim', N'U') IS NULL
BEGIN
    CREATE TABLE research.tbl_Dim_SourceReferencePlanetInHouseClaim
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SourceReferencePlanetInHouseClaim PRIMARY KEY,
        PlanetInHouseId INT NOT NULL,
        ClaimCode VARCHAR(100) NOT NULL,
        ClaimText NVARCHAR(2000) NOT NULL,
        InterpretationText NVARCHAR(MAX) NULL,
        RequiredConditionsJson NVARCHAR(MAX) NULL,
        EvidenceLevelCode VARCHAR(30) NOT NULL,
        StatusCode VARCHAR(20) NOT NULL CONSTRAINT DF_SourceReferencePlanetInHouseClaim_Status DEFAULT ('Proposed'),
        SourceTextId BIGINT NULL,
        SourceRefCode VARCHAR(80) NOT NULL,
        CONSTRAINT FK_SourceReferencePlanetInHouseClaim_Combination FOREIGN KEY (PlanetInHouseId) REFERENCES research.tbl_Dim_SourceReferencePlanetInHouse(Id),
        CONSTRAINT FK_SourceReferencePlanetInHouseClaim_Text FOREIGN KEY (SourceTextId) REFERENCES research.tbl_Dim_SourceReferencePlanetInHouseText(Id),
        CONSTRAINT UQ_SourceReferencePlanetInHouseClaim UNIQUE (PlanetInHouseId, ClaimCode, SourceRefCode)
    );
END;

IF OBJECT_ID(N'research.tbl_Dim_SourceReferencePlanetInHouseCrosswalk', N'U') IS NULL
BEGIN
    CREATE TABLE research.tbl_Dim_SourceReferencePlanetInHouseCrosswalk
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SourceReferencePlanetInHouseCrosswalk PRIMARY KEY,
        ClaimId BIGINT NOT NULL,
        ProductionTargetCode VARCHAR(100) NOT NULL,
        PromotionStatus VARCHAR(20) NOT NULL CONSTRAINT DF_SourceReferencePlanetInHouseCrosswalk_Status DEFAULT ('Proposed'),
        ReviewedBy NVARCHAR(200) NULL,
        ReviewedAtUtc DATETIME2(0) NULL,
        PromotionMigration VARCHAR(100) NULL,
        CONSTRAINT FK_SourceReferencePlanetInHouseCrosswalk_Claim FOREIGN KEY (ClaimId) REFERENCES research.tbl_Dim_SourceReferencePlanetInHouseClaim(Id),
        CONSTRAINT UQ_SourceReferencePlanetInHouseCrosswalk UNIQUE (ClaimId, ProductionTargetCode)
    );
END;

IF OBJECT_ID(N'dbo.SchemaMigrations', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = N'059_create_research_planet_in_house_tables.sql')
    INSERT dbo.SchemaMigrations (ScriptName, Note)
    VALUES (N'059_create_research_planet_in_house_tables.sql', N'Create isolated research schema tables for planet-in-house source text, attributes, claims and promotion tracking.');
