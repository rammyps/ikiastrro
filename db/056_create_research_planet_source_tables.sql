/* Isolated research corpus for planetary source text and reviewed claims.
   Nothing in this schema is consumed by chart calculation code. */
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'research')
    EXEC(N'CREATE SCHEMA research AUTHORIZATION dbo');

IF OBJECT_ID(N'research.tbl_Dim_SourceReferencePlanet', N'U') IS NULL
BEGIN
    CREATE TABLE research.tbl_Dim_SourceReferencePlanet
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SourceReferencePlanet PRIMARY KEY,
        PlanetCode VARCHAR(40) NOT NULL,
        SanskritName NVARCHAR(100) NULL,
        EnglishName NVARCHAR(100) NOT NULL,
        ResearchStatus VARCHAR(20) NOT NULL CONSTRAINT DF_SourceReferencePlanet_Status DEFAULT ('Active'),
        CONSTRAINT UQ_SourceReferencePlanet_Code UNIQUE (PlanetCode)
    );
END;

IF OBJECT_ID(N'research.tbl_Dim_SourceReferencePlanetText', N'U') IS NULL
BEGIN
    CREATE TABLE research.tbl_Dim_SourceReferencePlanetText
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SourceReferencePlanetText PRIMARY KEY,
        PlanetId INT NOT NULL,
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
        CONSTRAINT FK_SourceReferencePlanetText_Planet FOREIGN KEY (PlanetId) REFERENCES research.tbl_Dim_SourceReferencePlanet(Id)
    );
END;

IF OBJECT_ID(N'research.tbl_Dim_SourceReferencePlanetAttribute', N'U') IS NULL
BEGIN
    CREATE TABLE research.tbl_Dim_SourceReferencePlanetAttribute
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SourceReferencePlanetAttribute PRIMARY KEY,
        PlanetId INT NOT NULL,
        AttributeCode VARCHAR(100) NOT NULL,
        CategoryCode VARCHAR(40) NOT NULL,
        AttributeText NVARCHAR(1000) NOT NULL,
        PolarityCode VARCHAR(30) NULL,
        EvidenceLevelCode VARCHAR(30) NOT NULL,
        SourceTextId BIGINT NULL,
        SourceRefCode VARCHAR(80) NOT NULL,
        SourceLocator NVARCHAR(1000) NULL,
        ReviewerNotes NVARCHAR(MAX) NULL,
        CONSTRAINT FK_SourceReferencePlanetAttribute_Planet FOREIGN KEY (PlanetId) REFERENCES research.tbl_Dim_SourceReferencePlanet(Id),
        CONSTRAINT FK_SourceReferencePlanetAttribute_Text FOREIGN KEY (SourceTextId) REFERENCES research.tbl_Dim_SourceReferencePlanetText(Id),
        CONSTRAINT UQ_SourceReferencePlanetAttribute UNIQUE (PlanetId, AttributeCode, SourceRefCode)
    );
END;

IF OBJECT_ID(N'research.tbl_Dim_SourceReferencePlanetClaim', N'U') IS NULL
BEGIN
    CREATE TABLE research.tbl_Dim_SourceReferencePlanetClaim
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SourceReferencePlanetClaim PRIMARY KEY,
        PlanetId INT NOT NULL,
        ClaimCode VARCHAR(100) NOT NULL,
        ClaimText NVARCHAR(2000) NOT NULL,
        InterpretationText NVARCHAR(MAX) NULL,
        RequiredConditionsJson NVARCHAR(MAX) NULL,
        EvidenceLevelCode VARCHAR(30) NOT NULL,
        StatusCode VARCHAR(20) NOT NULL CONSTRAINT DF_SourceReferencePlanetClaim_Status DEFAULT ('Proposed'),
        SourceTextId BIGINT NULL,
        SourceRefCode VARCHAR(80) NOT NULL,
        CONSTRAINT FK_SourceReferencePlanetClaim_Planet FOREIGN KEY (PlanetId) REFERENCES research.tbl_Dim_SourceReferencePlanet(Id),
        CONSTRAINT FK_SourceReferencePlanetClaim_Text FOREIGN KEY (SourceTextId) REFERENCES research.tbl_Dim_SourceReferencePlanetText(Id),
        CONSTRAINT UQ_SourceReferencePlanetClaim UNIQUE (PlanetId, ClaimCode, SourceRefCode)
    );
END;

IF OBJECT_ID(N'research.tbl_Dim_SourceReferencePlanetCrosswalk', N'U') IS NULL
BEGIN
    CREATE TABLE research.tbl_Dim_SourceReferencePlanetCrosswalk
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SourceReferencePlanetCrosswalk PRIMARY KEY,
        ClaimId BIGINT NOT NULL,
        ProductionTargetCode VARCHAR(100) NOT NULL,
        PromotionStatus VARCHAR(20) NOT NULL CONSTRAINT DF_SourceReferencePlanetCrosswalk_Status DEFAULT ('Proposed'),
        ReviewedBy NVARCHAR(200) NULL,
        ReviewedAtUtc DATETIME2(0) NULL,
        PromotionMigration VARCHAR(100) NULL,
        CONSTRAINT FK_SourceReferencePlanetCrosswalk_Claim FOREIGN KEY (ClaimId) REFERENCES research.tbl_Dim_SourceReferencePlanetClaim(Id),
        CONSTRAINT UQ_SourceReferencePlanetCrosswalk UNIQUE (ClaimId, ProductionTargetCode)
    );
END;

IF OBJECT_ID(N'dbo.SchemaMigrations', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = N'056_create_research_planet_source_tables.sql')
    INSERT dbo.SchemaMigrations (ScriptName, Note)
    VALUES (N'056_create_research_planet_source_tables.sql', N'Create isolated research schema for planetary source text, attributes, claims and promotion tracking.');
