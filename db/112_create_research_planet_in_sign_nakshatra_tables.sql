-- =====================================================================
-- 112 - Research-schema scaffolding for Planet-in-Sign and
-- Planet-in-Nakshatra, mirroring migration 059's Planet-in-House shape
-- exactly (Dim combination -> Text -> Attribute -> Claim -> Crosswalk).
--
-- Unlike Planet-in-House (migration 059/064/065, now corrected and
-- promoted via 111+ real content), there is NO confirmed source for
-- either of these two in this project's 4 currently-extracted books
-- (confirmed this session, see docs/research/domain/ for the sourcing
-- note). So this migration seeds ONLY the combination dictionaries
-- (108 planet x sign, 243 planet x nakshatra) - no Text/Attribute/Claim
-- rows, unlike Planet-in-House which at least had (wrongly-cited)
-- placeholder Text rows. Don't add citation-pointer rows pointing at
-- nothing; wait for a real source before seeding anything past the
-- combination dictionaries.
--
-- research.tbl_Dim_SourceReferenceSign is new (the research schema had
-- standalone Planet/Nakshatra/House dims already, migrations 056-058, but
-- no Sign one - tbl_SignAttributes is production, not research).
-- research.tbl_Dim_SourceReferenceNakshatra already exists (057) and is
-- reused as-is.
--
-- Isolated research corpus. Nothing in this schema is consumed by chart
-- calculation code.
-- Apply:  sqlcmd -S localhost\SQLSERVER2025 -E -d ikiastrro -b -i db/112_create_research_planet_in_sign_nakshatra_tables.sql
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'research')
    EXEC(N'CREATE SCHEMA research AUTHORIZATION dbo');
GO

-- --- Batch 1: research.tbl_Dim_SourceReferenceSign (new - 12 rasis) ---
IF OBJECT_ID(N'research.tbl_Dim_SourceReferenceSign', N'U') IS NULL
BEGIN
    CREATE TABLE research.tbl_Dim_SourceReferenceSign
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SourceReferenceSign PRIMARY KEY,
        SignNumber TINYINT NOT NULL,
        SignCode VARCHAR(40) NOT NULL,
        SanskritName NVARCHAR(100) NULL,
        EnglishName NVARCHAR(100) NOT NULL,
        ResearchStatus VARCHAR(20) NOT NULL CONSTRAINT DF_SourceReferenceSign_Status DEFAULT ('Active'),
        CONSTRAINT UQ_SourceReferenceSign_Number UNIQUE (SignNumber),
        CONSTRAINT UQ_SourceReferenceSign_Code UNIQUE (SignCode),
        CONSTRAINT CK_SourceReferenceSign_Number CHECK (SignNumber BETWEEN 1 AND 12)
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM research.tbl_Dim_SourceReferenceSign)
INSERT research.tbl_Dim_SourceReferenceSign (SignNumber, SignCode, SanskritName, EnglishName)
VALUES
    (1,  'ARIES',       N'Mesha',      N'Aries'),
    (2,  'TAURUS',      N'Vrishabha',  N'Taurus'),
    (3,  'GEMINI',      N'Mithuna',    N'Gemini'),
    (4,  'CANCER',      N'Karka',      N'Cancer'),
    (5,  'LEO',         N'Simha',      N'Leo'),
    (6,  'VIRGO',       N'Kanya',      N'Virgo'),
    (7,  'LIBRA',       N'Tula',       N'Libra'),
    (8,  'SCORPIO',     N'Vrischika',  N'Scorpio'),
    (9,  'SAGITTARIUS', N'Dhanu',      N'Sagittarius'),
    (10, 'CAPRICORN',   N'Makara',     N'Capricorn'),
    (11, 'AQUARIUS',    N'Kumbha',     N'Aquarius'),
    (12, 'PISCES',      N'Meena',      N'Pisces');
GO

-- --- Batch 2: Planet-in-Sign combination + siblings (mirrors 059 exactly) ---
IF OBJECT_ID(N'research.tbl_Dim_SourceReferencePlanetInSign', N'U') IS NULL
BEGIN
    CREATE TABLE research.tbl_Dim_SourceReferencePlanetInSign
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SourceReferencePlanetInSign PRIMARY KEY,
        PlanetId INT NOT NULL,
        SignId INT NOT NULL,
        ResearchStatus VARCHAR(20) NOT NULL CONSTRAINT DF_SourceReferencePlanetInSign_Status DEFAULT ('Active'),
        CONSTRAINT FK_SourceReferencePlanetInSign_Planet FOREIGN KEY (PlanetId) REFERENCES research.tbl_Dim_SourceReferencePlanet(Id),
        CONSTRAINT FK_SourceReferencePlanetInSign_Sign FOREIGN KEY (SignId) REFERENCES research.tbl_Dim_SourceReferenceSign(Id),
        CONSTRAINT UQ_SourceReferencePlanetInSign UNIQUE (PlanetId, SignId)
    );
END;
GO

IF OBJECT_ID(N'research.tbl_Dim_SourceReferencePlanetInSignText', N'U') IS NULL
BEGIN
    CREATE TABLE research.tbl_Dim_SourceReferencePlanetInSignText
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SourceReferencePlanetInSignText PRIMARY KEY,
        PlanetInSignId INT NOT NULL,
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
        CONSTRAINT FK_SourceReferencePlanetInSignText_Combination FOREIGN KEY (PlanetInSignId) REFERENCES research.tbl_Dim_SourceReferencePlanetInSign(Id)
    );
END;
GO

IF OBJECT_ID(N'research.tbl_Dim_SourceReferencePlanetInSignAttribute', N'U') IS NULL
BEGIN
    CREATE TABLE research.tbl_Dim_SourceReferencePlanetInSignAttribute
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SourceReferencePlanetInSignAttribute PRIMARY KEY,
        PlanetInSignId INT NOT NULL,
        AttributeCode VARCHAR(100) NOT NULL,
        CategoryCode VARCHAR(40) NOT NULL,
        AttributeText NVARCHAR(1000) NOT NULL,
        PolarityCode VARCHAR(30) NULL,
        EvidenceLevelCode VARCHAR(30) NOT NULL,
        SourceTextId BIGINT NULL,
        SourceRefCode VARCHAR(80) NOT NULL,
        SourceLocator NVARCHAR(1000) NULL,
        ReviewerNotes NVARCHAR(MAX) NULL,
        CONSTRAINT FK_SourceReferencePlanetInSignAttribute_Combination FOREIGN KEY (PlanetInSignId) REFERENCES research.tbl_Dim_SourceReferencePlanetInSign(Id),
        CONSTRAINT FK_SourceReferencePlanetInSignAttribute_Text FOREIGN KEY (SourceTextId) REFERENCES research.tbl_Dim_SourceReferencePlanetInSignText(Id),
        CONSTRAINT UQ_SourceReferencePlanetInSignAttribute UNIQUE (PlanetInSignId, AttributeCode, SourceRefCode)
    );
END;
GO

IF OBJECT_ID(N'research.tbl_Dim_SourceReferencePlanetInSignClaim', N'U') IS NULL
BEGIN
    CREATE TABLE research.tbl_Dim_SourceReferencePlanetInSignClaim
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SourceReferencePlanetInSignClaim PRIMARY KEY,
        PlanetInSignId INT NOT NULL,
        ClaimCode VARCHAR(100) NOT NULL,
        ClaimText NVARCHAR(2000) NOT NULL,
        InterpretationText NVARCHAR(MAX) NULL,
        RequiredConditionsJson NVARCHAR(MAX) NULL,
        EvidenceLevelCode VARCHAR(30) NOT NULL,
        StatusCode VARCHAR(20) NOT NULL CONSTRAINT DF_SourceReferencePlanetInSignClaim_Status DEFAULT ('Proposed'),
        SourceTextId BIGINT NULL,
        SourceRefCode VARCHAR(80) NOT NULL,
        CONSTRAINT FK_SourceReferencePlanetInSignClaim_Combination FOREIGN KEY (PlanetInSignId) REFERENCES research.tbl_Dim_SourceReferencePlanetInSign(Id),
        CONSTRAINT FK_SourceReferencePlanetInSignClaim_Text FOREIGN KEY (SourceTextId) REFERENCES research.tbl_Dim_SourceReferencePlanetInSignText(Id),
        CONSTRAINT UQ_SourceReferencePlanetInSignClaim UNIQUE (PlanetInSignId, ClaimCode, SourceRefCode)
    );
END;
GO

IF OBJECT_ID(N'research.tbl_Dim_SourceReferencePlanetInSignCrosswalk', N'U') IS NULL
BEGIN
    CREATE TABLE research.tbl_Dim_SourceReferencePlanetInSignCrosswalk
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SourceReferencePlanetInSignCrosswalk PRIMARY KEY,
        ClaimId BIGINT NOT NULL,
        ProductionTargetCode VARCHAR(100) NOT NULL,
        PromotionStatus VARCHAR(20) NOT NULL CONSTRAINT DF_SourceReferencePlanetInSignCrosswalk_Status DEFAULT ('Proposed'),
        ReviewedBy NVARCHAR(200) NULL,
        ReviewedAtUtc DATETIME2(0) NULL,
        PromotionMigration VARCHAR(100) NULL,
        CONSTRAINT FK_SourceReferencePlanetInSignCrosswalk_Claim FOREIGN KEY (ClaimId) REFERENCES research.tbl_Dim_SourceReferencePlanetInSignClaim(Id),
        CONSTRAINT UQ_SourceReferencePlanetInSignCrosswalk UNIQUE (ClaimId, ProductionTargetCode)
    );
END;
GO

-- --- Batch 3: Planet-in-Nakshatra combination + siblings (same shape, existing Nakshatra dim) ---
IF OBJECT_ID(N'research.tbl_Dim_SourceReferencePlanetInNakshatra', N'U') IS NULL
BEGIN
    CREATE TABLE research.tbl_Dim_SourceReferencePlanetInNakshatra
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SourceReferencePlanetInNakshatra PRIMARY KEY,
        PlanetId INT NOT NULL,
        NakshatraId INT NOT NULL,
        ResearchStatus VARCHAR(20) NOT NULL CONSTRAINT DF_SourceReferencePlanetInNakshatra_Status DEFAULT ('Active'),
        CONSTRAINT FK_SourceReferencePlanetInNakshatra_Planet FOREIGN KEY (PlanetId) REFERENCES research.tbl_Dim_SourceReferencePlanet(Id),
        CONSTRAINT FK_SourceReferencePlanetInNakshatra_Nakshatra FOREIGN KEY (NakshatraId) REFERENCES research.tbl_Dim_SourceReferenceNakshatra(Id),
        CONSTRAINT UQ_SourceReferencePlanetInNakshatra UNIQUE (PlanetId, NakshatraId)
    );
END;
GO

IF OBJECT_ID(N'research.tbl_Dim_SourceReferencePlanetInNakshatraText', N'U') IS NULL
BEGIN
    CREATE TABLE research.tbl_Dim_SourceReferencePlanetInNakshatraText
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SourceReferencePlanetInNakshatraText PRIMARY KEY,
        PlanetInNakshatraId INT NOT NULL,
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
        CONSTRAINT FK_SourceReferencePlanetInNakshatraText_Combination FOREIGN KEY (PlanetInNakshatraId) REFERENCES research.tbl_Dim_SourceReferencePlanetInNakshatra(Id)
    );
END;
GO

IF OBJECT_ID(N'research.tbl_Dim_SourceReferencePlanetInNakshatraAttribute', N'U') IS NULL
BEGIN
    CREATE TABLE research.tbl_Dim_SourceReferencePlanetInNakshatraAttribute
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SourceReferencePlanetInNakshatraAttribute PRIMARY KEY,
        PlanetInNakshatraId INT NOT NULL,
        AttributeCode VARCHAR(100) NOT NULL,
        CategoryCode VARCHAR(40) NOT NULL,
        AttributeText NVARCHAR(1000) NOT NULL,
        PolarityCode VARCHAR(30) NULL,
        EvidenceLevelCode VARCHAR(30) NOT NULL,
        SourceTextId BIGINT NULL,
        SourceRefCode VARCHAR(80) NOT NULL,
        SourceLocator NVARCHAR(1000) NULL,
        ReviewerNotes NVARCHAR(MAX) NULL,
        CONSTRAINT FK_SourceReferencePlanetInNakshatraAttribute_Combination FOREIGN KEY (PlanetInNakshatraId) REFERENCES research.tbl_Dim_SourceReferencePlanetInNakshatra(Id),
        CONSTRAINT FK_SourceReferencePlanetInNakshatraAttribute_Text FOREIGN KEY (SourceTextId) REFERENCES research.tbl_Dim_SourceReferencePlanetInNakshatraText(Id),
        CONSTRAINT UQ_SourceReferencePlanetInNakshatraAttribute UNIQUE (PlanetInNakshatraId, AttributeCode, SourceRefCode)
    );
END;
GO

IF OBJECT_ID(N'research.tbl_Dim_SourceReferencePlanetInNakshatraClaim', N'U') IS NULL
BEGIN
    CREATE TABLE research.tbl_Dim_SourceReferencePlanetInNakshatraClaim
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SourceReferencePlanetInNakshatraClaim PRIMARY KEY,
        PlanetInNakshatraId INT NOT NULL,
        ClaimCode VARCHAR(100) NOT NULL,
        ClaimText NVARCHAR(2000) NOT NULL,
        InterpretationText NVARCHAR(MAX) NULL,
        RequiredConditionsJson NVARCHAR(MAX) NULL,
        EvidenceLevelCode VARCHAR(30) NOT NULL,
        StatusCode VARCHAR(20) NOT NULL CONSTRAINT DF_SourceReferencePlanetInNakshatraClaim_Status DEFAULT ('Proposed'),
        SourceTextId BIGINT NULL,
        SourceRefCode VARCHAR(80) NOT NULL,
        CONSTRAINT FK_SourceReferencePlanetInNakshatraClaim_Combination FOREIGN KEY (PlanetInNakshatraId) REFERENCES research.tbl_Dim_SourceReferencePlanetInNakshatra(Id),
        CONSTRAINT FK_SourceReferencePlanetInNakshatraClaim_Text FOREIGN KEY (SourceTextId) REFERENCES research.tbl_Dim_SourceReferencePlanetInNakshatraText(Id),
        CONSTRAINT UQ_SourceReferencePlanetInNakshatraClaim UNIQUE (PlanetInNakshatraId, ClaimCode, SourceRefCode)
    );
END;
GO

IF OBJECT_ID(N'research.tbl_Dim_SourceReferencePlanetInNakshatraCrosswalk', N'U') IS NULL
BEGIN
    CREATE TABLE research.tbl_Dim_SourceReferencePlanetInNakshatraCrosswalk
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SourceReferencePlanetInNakshatraCrosswalk PRIMARY KEY,
        ClaimId BIGINT NOT NULL,
        ProductionTargetCode VARCHAR(100) NOT NULL,
        PromotionStatus VARCHAR(20) NOT NULL CONSTRAINT DF_SourceReferencePlanetInNakshatraCrosswalk_Status DEFAULT ('Proposed'),
        ReviewedBy NVARCHAR(200) NULL,
        ReviewedAtUtc DATETIME2(0) NULL,
        PromotionMigration VARCHAR(100) NULL,
        CONSTRAINT FK_SourceReferencePlanetInNakshatraCrosswalk_Claim FOREIGN KEY (ClaimId) REFERENCES research.tbl_Dim_SourceReferencePlanetInNakshatraClaim(Id),
        CONSTRAINT UQ_SourceReferencePlanetInNakshatraCrosswalk UNIQUE (ClaimId, ProductionTargetCode)
    );
END;
GO

-- --- Batch 4: seed the combination dictionaries only (no Text/Attribute/Claim - no source yet) ---
IF NOT EXISTS (SELECT 1 FROM research.tbl_Dim_SourceReferencePlanetInSign)
INSERT research.tbl_Dim_SourceReferencePlanetInSign (PlanetId, SignId)
SELECT p.Id, s.Id
FROM research.tbl_Dim_SourceReferencePlanet p
CROSS JOIN research.tbl_Dim_SourceReferenceSign s;
GO

IF NOT EXISTS (SELECT 1 FROM research.tbl_Dim_SourceReferencePlanetInNakshatra)
INSERT research.tbl_Dim_SourceReferencePlanetInNakshatra (PlanetId, NakshatraId)
SELECT p.Id, n.Id
FROM research.tbl_Dim_SourceReferencePlanet p
CROSS JOIN research.tbl_Dim_SourceReferenceNakshatra n;
GO

-- --- Batch 5: ledger + summary ---
IF OBJECT_ID(N'dbo.SchemaMigrations', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = N'112_create_research_planet_in_sign_nakshatra_tables.sql')
    INSERT dbo.SchemaMigrations (ScriptName, Note)
    VALUES (N'112_create_research_planet_in_sign_nakshatra_tables.sql',
        N'research.*PlanetInSign* (108 combos) + *PlanetInNakshatra* (243 combos) scaffolding, mirrors 059. No source identified yet - combination dictionaries only, no Text/Attribute/Claim rows.');
GO

DECLARE @signCombos INT = (SELECT COUNT(*) FROM research.tbl_Dim_SourceReferencePlanetInSign);
DECLARE @nakCombos  INT = (SELECT COUNT(*) FROM research.tbl_Dim_SourceReferencePlanetInNakshatra);
PRINT '112 applied: ' + CAST(@signCombos AS VARCHAR(10)) + ' planet-in-sign combos (expect 108), '
    + CAST(@nakCombos AS VARCHAR(10)) + ' planet-in-nakshatra combos (expect 243).';
GO
