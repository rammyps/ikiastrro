-- =====================================================================
-- 086 - Naisargika (natural) karaka rules, PVR ch. 8 (pg 79), as two tables.
--
-- Source: rammyps's naisargika-karaka worksheet
-- (docs/research/domain/naisargika-karaka-pvr.md). SourceRefCode =
-- SRC_PVR_INTEGRATED on every row. RuleSetId 1.
--
--   tbl_Rule_Naisargika_Karakatwas - the full grid: one row per
--       (RuleSetId, GrahaId, Matter) - every matter a graha naturally
--       signifies, and the house that matter is classically read from.
--       A graha can appear against several houses (e.g. Jupiter: 2nd,
--       4th, 5th, 9th, 11th); a house can be fed by several grahas.
--   tbl_Rule_Naisargika_Karakas - the primary reduction: one row per
--       house (1-12), the single classically-cited "from planet" karaka
--       for that house, and a short summary of the matters it signifies
--       there. Each row is consistent with (drawn from) the Karakatwas
--       grid above, picking the one graha conventionally cited per house.
--
-- These are new, dedicated tables - not the reserved generic
-- tbl_Rule_Karaka (migration 18), which stays empty/reserved for a
-- future chara/sthira layer.
--
-- Idempotent: table adds guarded; seeds are IF NOT EXISTS on their table.
-- File is UTF-8 without BOM (Nyaya/Moksa carry combining diacritics) - apply with
-- -f 65001 or sqlcmd silently mis-decodes them (verified: without the flag, the two
-- diacritic rows land with wrong DATALENGTH/LEN; with it, byte lengths are exact).
-- Apply:  sqlcmd -S "localhost\SQLSERVER2025" -E -d ikiastrro -b -f 65001 -i db/086_seed_naisargika_karaka_rules.sql
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

-- --- Batch 1: tbl_Rule_Naisargika_Karakatwas ---
IF OBJECT_ID('dbo.tbl_Rule_Naisargika_Karakatwas', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Rule_Naisargika_Karakatwas (
        Id            INT IDENTITY(1,1) NOT NULL
                          CONSTRAINT PK_Rule_Naisargika_Karakatwas PRIMARY KEY,
        RuleSetId     TINYINT      NOT NULL
                          CONSTRAINT FK_Rule_NaisargikaKarakatwas_RuleSet FOREIGN KEY REFERENCES dbo.tbl_Rule_Sets (Id),
        GrahaId       TINYINT      NOT NULL
                          CONSTRAINT FK_Rule_NaisargikaKarakatwas_Planet FOREIGN KEY REFERENCES dbo.tbl_Planets (Id),
        HouseNumber   TINYINT      NOT NULL,
        Matter        NVARCHAR(200) NOT NULL,   -- the matter/domain the graha signifies at this house
        DisplayOrder  TINYINT      NOT NULL CONSTRAINT DF_Rule_NaisargikaKarakatwas_Order DEFAULT 1,
        SourceRefCode VARCHAR(40)  NULL,
        IsActive      BIT          NOT NULL CONSTRAINT DF_Rule_NaisargikaKarakatwas_IsActive DEFAULT 1,
        Notes         NVARCHAR(400) NULL,
        CONSTRAINT CK_Rule_NaisargikaKarakatwas_House CHECK (HouseNumber BETWEEN 1 AND 12),
        CONSTRAINT CK_Rule_NaisargikaKarakatwas_Src   CHECK (SourceRefCode IS NULL OR SourceRefCode LIKE 'SRC[_]%')
    );
    CREATE NONCLUSTERED INDEX IX_Rule_NaisargikaKarakatwas_Graha
        ON dbo.tbl_Rule_Naisargika_Karakatwas (RuleSetId, GrahaId, DisplayOrder);
    CREATE NONCLUSTERED INDEX IX_Rule_NaisargikaKarakatwas_House
        ON dbo.tbl_Rule_Naisargika_Karakatwas (RuleSetId, HouseNumber);
END
GO
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_Naisargika_Karakatwas)
BEGIN
    ;WITH k (PlanetName, DisplayOrder, HouseNumber, Matter) AS (
        SELECT * FROM (VALUES
        -- Sun
        ('Sun',      1,  1, N'Self, soul, constitution, health'),
        ('Sun',      2,  5, N'Fame and power'),
        ('Sun',      3,  9, N'Father and boss'),
        ('Sun',      4, 10, N'Career and achievements'),
        -- Moon
        ('Moon',     1,  1, N'Mind'),
        ('Moon',     2,  4, N'Mother and peace of mind'),
        ('Moon',     3, 11, N'Friends'),
        -- Mars
        ('Mars',     1,  3, N'Courage and younger siblings'),
        ('Mars',     2,  4, N'Real estate'),
        ('Mars',     3,  5, N'Nyāya scholarship and speculation'),
        ('Mars',     4,  6, N'Enemies, disease, accidents and loans'),
        -- Mercury
        ('Mercury',  1,  2, N'Speech'),
        ('Mercury',  2,  4, N'Learning'),
        ('Mercury',  3,  5, N'Memory, scholarship and students'),
        ('Mercury',  4, 10, N'Work, achievements and honours'),
        ('Mercury',  5, 11, N'Credits'),
        -- Jupiter
        ('Jupiter',  1,  2, N'Family and wealth'),
        ('Jupiter',  2,  4, N'Traditional learning'),
        ('Jupiter',  3,  5, N'Children and intelligence'),
        ('Jupiter',  4,  9, N'Teacher, religion and fortune'),
        ('Jupiter',  5, 11, N'Elder brother and gains'),
        -- Venus
        ('Venus',    1,  4, N'Vehicles'),
        ('Venus',    2,  7, N'Spouse and marital happiness'),
        ('Venus',    3, 12, N'Bed pleasures'),
        -- Saturn
        ('Saturn',   1,  5, N'Followers'),
        ('Saturn',   2,  6, N'Servants'),
        ('Saturn',   3,  8, N'Longevity and troubles'),
        ('Saturn',   4, 12, N'Loss and hospitalization'),
        -- Rahu
        ('Rahu',     1,  6, N'Accidents'),
        ('Rahu',     2,  8, N'Occult knowledge'),
        ('Rahu',     3,  9, N'Pilgrimage and foreign travel'),
        -- Ketu
        ('Ketu',     1,  8, N'Occult knowledge'),
        ('Ketu',     2,  9, N'Pilgrimage and foreign travel'),
        ('Ketu',     3, 12, N'Mokṣa')
        ) v (PlanetName, DisplayOrder, HouseNumber, Matter)
    )
    INSERT dbo.tbl_Rule_Naisargika_Karakatwas
        (RuleSetId, GrahaId, HouseNumber, Matter, DisplayOrder, SourceRefCode, IsActive)
    SELECT 1, p.Id, k.HouseNumber, k.Matter, k.DisplayOrder, 'SRC_PVR_INTEGRATED', 1
    FROM k
    JOIN dbo.tbl_Planets p ON p.PlanetName = k.PlanetName;
END
GO

-- --- Batch 2: tbl_Rule_Naisargika_Karakas (primary reduction, one row per house) ---
IF OBJECT_ID('dbo.tbl_Rule_Naisargika_Karakas', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Rule_Naisargika_Karakas (
        Id               INT IDENTITY(1,1) NOT NULL
                             CONSTRAINT PK_Rule_Naisargika_Karakas PRIMARY KEY,
        RuleSetId        TINYINT      NOT NULL
                             CONSTRAINT FK_Rule_NaisargikaKarakas_RuleSet FOREIGN KEY REFERENCES dbo.tbl_Rule_Sets (Id),
        HouseNumber      TINYINT      NOT NULL,
        GrahaId          TINYINT      NOT NULL
                             CONSTRAINT FK_Rule_NaisargikaKarakas_Planet FOREIGN KEY REFERENCES dbo.tbl_Planets (Id),
        MattersSignified NVARCHAR(200) NOT NULL,
        SourceRefCode    VARCHAR(40)  NULL,
        IsActive         BIT          NOT NULL CONSTRAINT DF_Rule_NaisargikaKarakas_IsActive DEFAULT 1,
        Notes            NVARCHAR(400) NULL,
        CONSTRAINT CK_Rule_NaisargikaKarakas_House CHECK (HouseNumber BETWEEN 1 AND 12),
        CONSTRAINT CK_Rule_NaisargikaKarakas_Src   CHECK (SourceRefCode IS NULL OR SourceRefCode LIKE 'SRC[_]%'),
        CONSTRAINT UQ_Rule_NaisargikaKarakas UNIQUE (RuleSetId, HouseNumber)
    );
END
GO
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_Naisargika_Karakas)
    INSERT dbo.tbl_Rule_Naisargika_Karakas (RuleSetId, HouseNumber, GrahaId, MattersSignified, SourceRefCode)
    SELECT 1, v.HouseNumber, p.Id, v.MattersSignified, 'SRC_PVR_INTEGRATED'
    FROM (VALUES
        ( 1, 'Sun',     N'Self, physical constitution, soul, health'),
        ( 2, 'Jupiter', N'Family, wealth'),
        ( 3, 'Mars',    N'Younger siblings, courage'),
        ( 4, 'Moon',    N'Mother'),
        ( 5, 'Jupiter', N'Children'),
        ( 6, 'Mars',    N'Enemies'),
        ( 7, 'Venus',   N'Wife, husband, marital bliss, relationships'),
        ( 8, 'Saturn',  N'Longevity, troubles'),
        ( 9, 'Jupiter', N'Teacher, religion, fortune'),
        (10, 'Mercury', N'Work, achievements, honors'),
        (11, 'Jupiter', N'Elder siblings'),
        (12, 'Saturn',  N'Losses')
    ) v (HouseNumber, PlanetName, MattersSignified)
    JOIN dbo.tbl_Planets p ON p.PlanetName = v.PlanetName;
GO

-- --- Batch 3: tbl_Rule_Catalog rows ---
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_Catalog WHERE RuleTableName = 'tbl_Rule_Naisargika_Karakatwas')
    INSERT dbo.tbl_Rule_Catalog (RuleTableName, EngineCode, MethodCodes, Purpose, IntroducedIn)
    VALUES ('tbl_Rule_Naisargika_Karakatwas', 'KARAKA', 'MAP_LOOKUP',
            'Naisargika (natural) karakatwa (PVR ch. 8, pg 79): the full graha-to-matter-to-house grid - every matter a graha naturally signifies, and the house it is classically read from. One row per (rule-set, graha, matter).',
            '086_seed_naisargika_karaka_rules.sql');
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_Catalog WHERE RuleTableName = 'tbl_Rule_Naisargika_Karakas')
    INSERT dbo.tbl_Rule_Catalog (RuleTableName, EngineCode, MethodCodes, Purpose, IntroducedIn)
    VALUES ('tbl_Rule_Naisargika_Karakas', 'KARAKA', 'HOUSE_LOOKUP',
            'Primary naisargika karaka per house (PVR ch. 8, pg 79): the single classically-cited graha for each of the 12 houses, with a short summary of matters signified. Reduction of tbl_Rule_Naisargika_Karakatwas. One row per (rule-set, house).',
            '086_seed_naisargika_karaka_rules.sql');
GO

-- --- Batch 4: ledger + summary ---
INSERT dbo.SchemaMigrations (ScriptName, Note)
SELECT '086_seed_naisargika_karaka_rules.sql',
       'tbl_Rule_Naisargika_Karakatwas (34 rows, full grid) + tbl_Rule_Naisargika_Karakas (12 rows, primary per-house reduction); 2 tbl_Rule_Catalog rows'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '086_seed_naisargika_karaka_rules.sql');
GO

DECLARE @grid INT    = (SELECT COUNT(*) FROM dbo.tbl_Rule_Naisargika_Karakatwas);
DECLARE @primary INT = (SELECT COUNT(*) FROM dbo.tbl_Rule_Naisargika_Karakas);
DECLARE @orphanHouse INT = (SELECT COUNT(*) FROM dbo.tbl_Rule_Naisargika_Karakas p
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.tbl_Rule_Naisargika_Karakatwas g
        WHERE g.GrahaId = p.GrahaId AND g.HouseNumber = p.HouseNumber));
PRINT '086 applied: ' + CAST(@grid AS VARCHAR(10)) + ' karakatwa rows (expect 34), '
    + CAST(@primary AS VARCHAR(10)) + ' primary-karaka rows (expect 12), '
    + CAST(@orphanHouse AS VARCHAR(10)) + ' primary rows not backed by the karakatwa grid (expect 0).';
GO
