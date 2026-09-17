/* Isolated research corpus for "planet transit counted from natal Moon" (Gochara) results --
   same shape as research.tbl_Dim_SourceReferenceHouseLordInHouse* (migrations 059/089-092):
   a combination dimension (TransitPlanetId x HouseFromMoon), a source-text pointer table, a
   paraphrased claim table, and a promotion crosswalk. Nothing here is consumed by chart
   calculation code -- see docs/research/domain/transit-events.md "Rules found in the
   extracts" row "Transit from natal Moon (Janma Rasi)" for the DB-consequence this fills.

   Source: `SRC_KP_TRANSIT_GOCHARA` -- Krishnamurti Padhdhati (K. S. Krishnamurti), front-matter
   chapter "Transit (Gocharaphala Nirnayam)", pp. xxii-xxx (OCR, roman-numeral front matter;
   edition/volume and a local file path are not yet confirmed -- treat as provisional until
   cross-checked against a physical copy, same caution class as SRC_RAMAN_HINDU_PREDICTIVE). */
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'research')
    EXEC(N'CREATE SCHEMA research AUTHORIZATION dbo');
GO

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Dim_Source WHERE Code = 'SRC_KP_TRANSIT_GOCHARA')
    INSERT dbo.tbl_Dim_Source (Code, Title, Author, Edition, Tradition, Notes)
    VALUES ('SRC_KP_TRANSIT_GOCHARA', N'Krishnamurti Padhdhati', N'K. S. Krishnamurti', NULL, 'KP',
        N'Front-matter chapter "Transit (Gocharaphala Nirnayam)", pp. xxii-xxx (roman-numeral pages, OCR). Sign-by-sign Sun/Moon/Mars/Mercury/Jupiter/Venus/Saturn transit results counted from natal Moon (Rahu=Mars-equivalent, Ketu=Saturn-equivalent per source). Local file/edition unconfirmed.');
GO

IF OBJECT_ID(N'research.tbl_Dim_SourceReferencePlanetTransitFromMoon', N'U') IS NULL
BEGIN
    CREATE TABLE research.tbl_Dim_SourceReferencePlanetTransitFromMoon
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SourceReferencePlanetTransitFromMoon PRIMARY KEY,
        PlanetId INT NOT NULL,
        HouseFromMoon TINYINT NOT NULL,
        ResearchStatus VARCHAR(20) NOT NULL CONSTRAINT DF_SourceReferencePlanetTransitFromMoon_Status DEFAULT ('Active'),
        CONSTRAINT FK_SourceReferencePlanetTransitFromMoon_Planet FOREIGN KEY (PlanetId) REFERENCES research.tbl_Dim_SourceReferencePlanet(Id),
        CONSTRAINT UQ_SourceReferencePlanetTransitFromMoon UNIQUE (PlanetId, HouseFromMoon),
        CONSTRAINT CK_SourceReferencePlanetTransitFromMoon_House CHECK (HouseFromMoon BETWEEN 1 AND 12)
    );
END;
GO

IF OBJECT_ID(N'research.tbl_Dim_SourceReferencePlanetTransitFromMoonText', N'U') IS NULL
BEGIN
    CREATE TABLE research.tbl_Dim_SourceReferencePlanetTransitFromMoonText
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SourceReferencePlanetTransitFromMoonText PRIMARY KEY,
        PlanetTransitFromMoonId INT NOT NULL,
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
        CONSTRAINT FK_SourceReferencePlanetTransitFromMoonText_Combination FOREIGN KEY (PlanetTransitFromMoonId) REFERENCES research.tbl_Dim_SourceReferencePlanetTransitFromMoon(Id)
    );
END;
GO

IF OBJECT_ID(N'research.tbl_Dim_SourceReferencePlanetTransitFromMoonClaim', N'U') IS NULL
BEGIN
    CREATE TABLE research.tbl_Dim_SourceReferencePlanetTransitFromMoonClaim
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SourceReferencePlanetTransitFromMoonClaim PRIMARY KEY,
        PlanetTransitFromMoonId INT NOT NULL,
        ClaimCode VARCHAR(100) NOT NULL,
        ClaimText NVARCHAR(2000) NOT NULL,
        InterpretationText NVARCHAR(MAX) NULL,
        RequiredConditionsJson NVARCHAR(MAX) NULL,
        EvidenceLevelCode VARCHAR(30) NOT NULL,
        StatusCode VARCHAR(20) NOT NULL CONSTRAINT DF_SourceReferencePlanetTransitFromMoonClaim_Status DEFAULT ('Proposed'),
        SourceTextId BIGINT NULL,
        SourceRefCode VARCHAR(80) NOT NULL,
        CONSTRAINT FK_SourceReferencePlanetTransitFromMoonClaim_Combination FOREIGN KEY (PlanetTransitFromMoonId) REFERENCES research.tbl_Dim_SourceReferencePlanetTransitFromMoon(Id),
        CONSTRAINT FK_SourceReferencePlanetTransitFromMoonClaim_Text FOREIGN KEY (SourceTextId) REFERENCES research.tbl_Dim_SourceReferencePlanetTransitFromMoonText(Id),
        CONSTRAINT UQ_SourceReferencePlanetTransitFromMoonClaim UNIQUE (PlanetTransitFromMoonId, ClaimCode, SourceRefCode)
    );
END;
GO

IF OBJECT_ID(N'research.tbl_Dim_SourceReferencePlanetTransitFromMoonCrosswalk', N'U') IS NULL
BEGIN
    CREATE TABLE research.tbl_Dim_SourceReferencePlanetTransitFromMoonCrosswalk
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SourceReferencePlanetTransitFromMoonCrosswalk PRIMARY KEY,
        ClaimId BIGINT NOT NULL,
        ProductionTargetCode VARCHAR(100) NOT NULL,
        PromotionStatus VARCHAR(20) NOT NULL CONSTRAINT DF_SourceReferencePlanetTransitFromMoonCrosswalk_Status DEFAULT ('Proposed'),
        ReviewedBy NVARCHAR(200) NULL,
        ReviewedAtUtc DATETIME2(0) NULL,
        PromotionMigration VARCHAR(100) NULL,
        CONSTRAINT FK_SourceReferencePlanetTransitFromMoonCrosswalk_Claim FOREIGN KEY (ClaimId) REFERENCES research.tbl_Dim_SourceReferencePlanetTransitFromMoonClaim(Id),
        CONSTRAINT UQ_SourceReferencePlanetTransitFromMoonCrosswalk UNIQUE (ClaimId, ProductionTargetCode)
    );
END;
GO

IF OBJECT_ID(N'dbo.SchemaMigrations', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = N'112_create_research_planet_transit_moon_tables.sql')
    INSERT dbo.SchemaMigrations (ScriptName, Note)
    VALUES (N'112_create_research_planet_transit_moon_tables.sql', N'Create isolated research schema tables for planet-transit-from-Moon source text and claims, and register SRC_KP_TRANSIT_GOCHARA.');
GO

PRINT '112 applied: research.tbl_Dim_SourceReferencePlanetTransitFromMoon* created; SRC_KP_TRANSIT_GOCHARA registered.';
GO
