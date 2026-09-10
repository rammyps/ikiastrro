/* Isolated research corpus for nakshatra/pada source text and reviewed claims.
   Nothing in this schema is consumed by chart calculation code. */
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'research')
    EXEC(N'CREATE SCHEMA research AUTHORIZATION dbo');

IF OBJECT_ID(N'research.tbl_Dim_SourceReferenceNakshatra', N'U') IS NULL
BEGIN
    CREATE TABLE research.tbl_Dim_SourceReferenceNakshatra
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SourceReferenceNakshatra PRIMARY KEY,
        NakshatraCode VARCHAR(40) NOT NULL,
        SanskritName NVARCHAR(100) NOT NULL,
        EnglishName NVARCHAR(100) NULL,
        SequenceNumber TINYINT NULL,
        ResearchStatus VARCHAR(20) NOT NULL CONSTRAINT DF_SourceReferenceNakshatra_Status DEFAULT ('Active'),
        CONSTRAINT UQ_SourceReferenceNakshatra_Code UNIQUE (NakshatraCode)
    );
END;

IF OBJECT_ID(N'research.tbl_Dim_SourceReferenceNakshatraText', N'U') IS NULL
BEGIN
    CREATE TABLE research.tbl_Dim_SourceReferenceNakshatraText
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SourceReferenceNakshatraText PRIMARY KEY,
        NakshatraId INT NOT NULL,
        PadaNumber TINYINT NULL,
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
        CONSTRAINT FK_SourceReferenceNakshatraText_Nakshatra FOREIGN KEY (NakshatraId) REFERENCES research.tbl_Dim_SourceReferenceNakshatra(Id),
        CONSTRAINT CK_SourceReferenceNakshatraText_Pada CHECK (PadaNumber IS NULL OR PadaNumber BETWEEN 1 AND 4)
    );
END;

IF OBJECT_ID(N'research.tbl_Dim_SourceReferenceNakshatraAttribute', N'U') IS NULL
BEGIN
    CREATE TABLE research.tbl_Dim_SourceReferenceNakshatraAttribute
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SourceReferenceNakshatraAttribute PRIMARY KEY,
        NakshatraId INT NOT NULL,
        PadaNumber TINYINT NULL,
        AttributeCode VARCHAR(100) NOT NULL,
        CategoryCode VARCHAR(40) NOT NULL,
        AttributeText NVARCHAR(1000) NOT NULL,
        PolarityCode VARCHAR(30) NULL,
        EvidenceLevelCode VARCHAR(30) NOT NULL,
        SourceTextId BIGINT NULL,
        SourceRefCode VARCHAR(80) NOT NULL,
        SourceLocator NVARCHAR(1000) NULL,
        ReviewerNotes NVARCHAR(MAX) NULL,
        CONSTRAINT FK_SourceReferenceNakshatraAttribute_Nakshatra FOREIGN KEY (NakshatraId) REFERENCES research.tbl_Dim_SourceReferenceNakshatra(Id),
        CONSTRAINT FK_SourceReferenceNakshatraAttribute_Text FOREIGN KEY (SourceTextId) REFERENCES research.tbl_Dim_SourceReferenceNakshatraText(Id),
        CONSTRAINT CK_SourceReferenceNakshatraAttribute_Pada CHECK (PadaNumber IS NULL OR PadaNumber BETWEEN 1 AND 4),
        CONSTRAINT UQ_SourceReferenceNakshatraAttribute UNIQUE (NakshatraId, PadaNumber, AttributeCode, SourceRefCode)
    );
END;

IF OBJECT_ID(N'research.tbl_Dim_SourceReferenceNakshatraClaim', N'U') IS NULL
BEGIN
    CREATE TABLE research.tbl_Dim_SourceReferenceNakshatraClaim
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SourceReferenceNakshatraClaim PRIMARY KEY,
        NakshatraId INT NOT NULL,
        PadaNumber TINYINT NULL,
        ClaimCode VARCHAR(100) NOT NULL,
        ClaimText NVARCHAR(2000) NOT NULL,
        InterpretationText NVARCHAR(MAX) NULL,
        RequiredConditionsJson NVARCHAR(MAX) NULL,
        EvidenceLevelCode VARCHAR(30) NOT NULL,
        StatusCode VARCHAR(20) NOT NULL CONSTRAINT DF_SourceReferenceNakshatraClaim_Status DEFAULT ('Proposed'),
        SourceTextId BIGINT NULL,
        SourceRefCode VARCHAR(80) NOT NULL,
        CONSTRAINT FK_SourceReferenceNakshatraClaim_Nakshatra FOREIGN KEY (NakshatraId) REFERENCES research.tbl_Dim_SourceReferenceNakshatra(Id),
        CONSTRAINT FK_SourceReferenceNakshatraClaim_Text FOREIGN KEY (SourceTextId) REFERENCES research.tbl_Dim_SourceReferenceNakshatraText(Id),
        CONSTRAINT CK_SourceReferenceNakshatraClaim_Pada CHECK (PadaNumber IS NULL OR PadaNumber BETWEEN 1 AND 4),
        CONSTRAINT UQ_SourceReferenceNakshatraClaim UNIQUE (NakshatraId, PadaNumber, ClaimCode, SourceRefCode)
    );
END;

IF OBJECT_ID(N'research.tbl_Dim_SourceReferenceNakshatraCrosswalk', N'U') IS NULL
BEGIN
    CREATE TABLE research.tbl_Dim_SourceReferenceNakshatraCrosswalk
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SourceReferenceNakshatraCrosswalk PRIMARY KEY,
        ClaimId BIGINT NOT NULL,
        ProductionTargetCode VARCHAR(100) NOT NULL,
        PromotionStatus VARCHAR(20) NOT NULL CONSTRAINT DF_SourceReferenceNakshatraCrosswalk_Status DEFAULT ('Proposed'),
        ReviewedBy NVARCHAR(200) NULL,
        ReviewedAtUtc DATETIME2(0) NULL,
        PromotionMigration VARCHAR(100) NULL,
        CONSTRAINT FK_SourceReferenceNakshatraCrosswalk_Claim FOREIGN KEY (ClaimId) REFERENCES research.tbl_Dim_SourceReferenceNakshatraClaim(Id),
        CONSTRAINT UQ_SourceReferenceNakshatraCrosswalk UNIQUE (ClaimId, ProductionTargetCode)
    );
END;

IF OBJECT_ID(N'dbo.SchemaMigrations', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = N'057_create_research_nakshatra_source_tables.sql')
    INSERT dbo.SchemaMigrations (ScriptName, Note)
    VALUES (N'057_create_research_nakshatra_source_tables.sql', N'Create isolated research schema tables for nakshatra and pada source text, attributes, claims and promotion tracking.');
