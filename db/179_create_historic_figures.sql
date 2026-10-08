-- =====================================================================
-- 179 - Historic figures reference database (leaders, thinkers, celebrities).
--
-- Reference data, deliberately NOT in tbl_BirthDetails and NOT in the consent-gated
-- analytics cohort (tbl_Dim_AnalyticsSubjects). One primary cell per person:
-- Area x SubCategory (7 x 7), plus a GeoArea tag for the reach of their influence.
-- Birth data is sourced from Wikidata (CC0) and never invented; TimeOfBirth is NULL
-- unless a rated source supplies it, and TimeQuality says so.
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID('dbo.tbl_Dim_HistoricArea', 'U') IS NULL
    CREATE TABLE dbo.tbl_Dim_HistoricArea (
        AreaCode     VARCHAR(8)  NOT NULL CONSTRAINT PK_Dim_HistoricArea PRIMARY KEY,
        AreaName     VARCHAR(48) NOT NULL,
        DisplayOrder TINYINT     NOT NULL
    );
GO
IF OBJECT_ID('dbo.tbl_Dim_HistoricSubCategory', 'U') IS NULL
    CREATE TABLE dbo.tbl_Dim_HistoricSubCategory (
        AreaCode        VARCHAR(8)  NOT NULL CONSTRAINT FK_Dim_HistoricSubCategory_Area
                            REFERENCES dbo.tbl_Dim_HistoricArea (AreaCode),
        SubCategoryCode VARCHAR(12) NOT NULL,
        SubCategoryName VARCHAR(64) NOT NULL,
        DisplayOrder    TINYINT     NOT NULL,
        CONSTRAINT PK_Dim_HistoricSubCategory PRIMARY KEY (AreaCode, SubCategoryCode)
    );
GO
IF OBJECT_ID('dbo.tbl_Dim_HistoricGeoArea', 'U') IS NULL
    CREATE TABLE dbo.tbl_Dim_HistoricGeoArea (
        GeoAreaCode  VARCHAR(10) NOT NULL CONSTRAINT PK_Dim_HistoricGeoArea PRIMARY KEY,
        GeoAreaName  VARCHAR(32) NOT NULL,
        Reach        VARCHAR(160) NOT NULL,
        DisplayOrder TINYINT     NOT NULL
    );
GO

MERGE dbo.tbl_Dim_HistoricArea AS t
USING (VALUES
    ('POL', 'Political leaders', 1), ('IND', 'Indian leaders', 2), ('MIL', 'Military & strategists', 3),
    ('SPI', 'Spiritual & religious leaders', 4), ('SCI', 'Science & thought', 5),
    ('BUS', 'Business & industry', 6), ('ART', 'Arts, sport & media', 7)
) AS s (AreaCode, AreaName, DisplayOrder) ON t.AreaCode = s.AreaCode
WHEN NOT MATCHED THEN INSERT (AreaCode, AreaName, DisplayOrder) VALUES (s.AreaCode, s.AreaName, s.DisplayOrder);
GO

MERGE dbo.tbl_Dim_HistoricSubCategory AS t
USING (VALUES
    ('POL','PRES','Presidents',1), ('POL','PM','Prime ministers',2), ('POL','MON','Monarchs & emperors',3),
    ('POL','FREEDOM','Freedom & independence leaders',4), ('POL','REVOL','Revolutionaries & reformers',5),
    ('POL','DICT','Dictators & authoritarian rulers',6), ('POL','DIPL','Diplomats & statespeople',7),
    ('IND','PMIN','Prime ministers of India',1), ('IND','PRESGOV','Presidents & governors',2),
    ('IND','FFIGHT','Freedom fighters',3), ('IND','CM','Chief ministers',4),
    ('IND','CONST','Constitution makers & jurists',5), ('IND','PRINCE','Princely rulers & regional kings',6),
    ('IND','REFORM','Social reformers',7),
    ('MIL','GEN','Generals',1), ('MIL','ADM','Admirals',2), ('MIL','CONQ','Conquerors',3),
    ('MIL','RESIST','Resistance & guerrilla leaders',4), ('MIL','WARLEAD','War-time leaders',5),
    ('MIL','STRAT','Military strategists & theorists',6), ('MIL','INTEL','Intelligence chiefs',7),
    ('SPI','HINDU','Hindu saints & gurus',1), ('SPI','BUDD','Buddhist leaders',2),
    ('SPI','CHRIST','Christian leaders & popes',3), ('SPI','ISLAM','Islamic scholars & leaders',4),
    ('SPI','SIKHJAIN','Sikh & Jain leaders',5), ('SPI','MYSTIC','Mystics & philosophers of spirit',6),
    ('SPI','FOUNDER','Founders of movements',7),
    ('SCI','PHYS','Physicists & mathematicians',1), ('SCI','INVENT','Inventors & engineers',2),
    ('SCI','ASTRO','Astronomers & astrologers',3), ('SCI','MED','Medical pioneers',4),
    ('SCI','ECON','Economists & social scientists',5), ('SCI','PHIL','Philosophers',6),
    ('SCI','EDU','Educators',7),
    ('BUS','TECH','Tech founders',1), ('BUS','INDUS','Industrialists',2), ('BUS','INVEST','Investors & financiers',3),
    ('BUS','RETAIL','Retail & consumer brands',4), ('BUS','MEDIA','Media & publishing moguls',5),
    ('BUS','AUTO','Auto & energy',6), ('BUS','PHIL','Philanthropists',7),
    ('ART','ACTOR','Film actors',1), ('ART','DIRECT','Directors & producers',2), ('ART','MUSIC','Musicians',3),
    ('ART','AUTHOR','Authors & poets',4), ('ART','CRICK','Cricketers',5), ('ART','ATHLETE','Other athletes',6),
    ('ART','JOURN','Journalists & broadcasters',7)
) AS s (AreaCode, SubCategoryCode, SubCategoryName, DisplayOrder)
  ON t.AreaCode = s.AreaCode AND t.SubCategoryCode = s.SubCategoryCode
WHEN NOT MATCHED THEN INSERT (AreaCode, SubCategoryCode, SubCategoryName, DisplayOrder)
    VALUES (s.AreaCode, s.SubCategoryCode, s.SubCategoryName, s.DisplayOrder);
GO

MERGE dbo.tbl_Dim_HistoricGeoArea AS t
USING (VALUES
    ('IN_LOCAL',  'India-Local',      'City, district or small princely-state level', 1),
    ('IN_STATE',  'India-States',     'One Indian state', 2),
    ('IN_REGION', 'India-Regions',    'A multi-state region of India (North, South, Deccan, East ...)', 3),
    ('IN_GLOBAL', 'India-Global',     'Pan-India leaders, and Indians known worldwide', 4),
    ('GL_REGION', 'Global-Regions',   'A world region (South Asia, Middle East, Western Europe ...)', 5),
    ('GL_CONT',   'Global-Continents','A whole continent', 6),
    ('GL_GLOBAL', 'Global-Global',    'World-wide influence', 7)
) AS s (GeoAreaCode, GeoAreaName, Reach, DisplayOrder) ON t.GeoAreaCode = s.GeoAreaCode
WHEN NOT MATCHED THEN INSERT (GeoAreaCode, GeoAreaName, Reach, DisplayOrder)
    VALUES (s.GeoAreaCode, s.GeoAreaName, s.Reach, s.DisplayOrder);
GO

IF OBJECT_ID('dbo.tbl_Ref_HistoricFigure', 'U') IS NULL
    CREATE TABLE dbo.tbl_Ref_HistoricFigure (
        Id               INT IDENTITY(1,1) CONSTRAINT PK_Ref_HistoricFigure PRIMARY KEY,
        WikidataQid      VARCHAR(16)   NOT NULL CONSTRAINT UQ_Ref_HistoricFigure_Qid UNIQUE,
        Name             NVARCHAR(160) NOT NULL,
        Description      NVARCHAR(300) NULL,
        Sex              VARCHAR(16)   NULL,
        DateOfBirth      DATE          NOT NULL,   -- proleptic Gregorian, as the engine expects
        OriginalCalendar VARCHAR(12)   NOT NULL,   -- GREGORIAN | JULIAN (as Wikidata stored it)
        TimeOfBirth      TIME(0)       NULL,       -- NULL: no rated source supplied one
        TimeQuality      VARCHAR(16)   NOT NULL CONSTRAINT DF_Ref_HistoricFigure_TimeQuality DEFAULT 'UNKNOWN',
        BirthPlace       NVARCHAR(160) NULL,
        BirthCountry     NVARCHAR(100) NULL,
        Latitude         DECIMAL(9,6)  NULL,
        Longitude        DECIMAL(9,6)  NULL,
        DateOfDeath      DATE          NULL,
        AreaCode         VARCHAR(8)    NOT NULL,
        SubCategoryCode  VARCHAR(12)   NOT NULL,
        GeoAreaCode      VARCHAR(10)   NOT NULL CONSTRAINT FK_Ref_HistoricFigure_Geo
                             REFERENCES dbo.tbl_Dim_HistoricGeoArea (GeoAreaCode),
        SourceCode       VARCHAR(16)   NOT NULL CONSTRAINT DF_Ref_HistoricFigure_Source DEFAULT 'WIKIDATA',
        SourceUrl        VARCHAR(200)  NOT NULL,
        ReviewStatus     VARCHAR(16)   NOT NULL CONSTRAINT DF_Ref_HistoricFigure_Review DEFAULT 'UNREVIEWED',
        RetrievedAtUtc   DATETIME2(0)  NOT NULL CONSTRAINT DF_Ref_HistoricFigure_Retrieved DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_Ref_HistoricFigure_SubCategory FOREIGN KEY (AreaCode, SubCategoryCode)
            REFERENCES dbo.tbl_Dim_HistoricSubCategory (AreaCode, SubCategoryCode),
        CONSTRAINT CK_Ref_HistoricFigure_Calendar CHECK (OriginalCalendar IN ('GREGORIAN','JULIAN')),
        CONSTRAINT CK_Ref_HistoricFigure_TimeQuality CHECK (TimeQuality IN ('UNKNOWN','APPROXIMATE','RECORDED','RECTIFIED')),
        CONSTRAINT CK_Ref_HistoricFigure_TimeConsistency CHECK ((TimeOfBirth IS NULL AND TimeQuality = 'UNKNOWN') OR (TimeOfBirth IS NOT NULL AND TimeQuality <> 'UNKNOWN')),
        CONSTRAINT CK_Ref_HistoricFigure_Review CHECK (ReviewStatus IN ('UNREVIEWED','VERIFIED','DISPUTED')),
        CONSTRAINT CK_Ref_HistoricFigure_Coords CHECK
            ((Latitude IS NULL AND Longitude IS NULL) OR (Latitude BETWEEN -90 AND 90 AND Longitude BETWEEN -180 AND 180))
    );
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Ref_HistoricFigure_Cell')
    CREATE INDEX IX_Ref_HistoricFigure_Cell ON dbo.tbl_Ref_HistoricFigure (AreaCode, SubCategoryCode, GeoAreaCode);
GO

IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '179_create_historic_figures.sql')
    INSERT dbo.SchemaMigrations (ScriptName, AppliedAtUtc, Note)
    VALUES ('179_create_historic_figures.sql', SYSUTCDATETIME(),
            'Adds the historic-figures reference database: 7 areas x 7 subcategories, 7 geo-reach levels, Wikidata-sourced births.');
GO
PRINT '179 applied: historic figures reference database.';
GO
