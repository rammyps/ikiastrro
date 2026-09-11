-- =====================================================================
-- 081 — Panchanga / time layer: Tithi, Karana, Nitya Yoga, Vedic Weekday,
--       Hora Lord, and the birth-moment sunrise/sunset/janma-ghatis that
--       anchor them. Schema + reference seed; the calculator that fills
--       tbl_Chart_Panchanga is a later `cli` change (masterproduct.md
--       docs/cli/gap-and-coverage.md priority gap #2, pvr-coverage.md ch.1).
--
-- Source: SRC_PVR_INTEGRATED SS1.3.8-1.3.12 (Tithis and Lunar Calendar /
-- Yogas / Karanas / Hora / Panchaanga), the book's only panchanga chapter.
-- Every formula below is transcribed from that chapter and cross-checked
-- against the JHora export for 1_Ramakrishnan (SRC_JHORA_EXPORT_RAMAKRISHNAN):
--   - Krishna Tritiya (tithi 18, lord Mars) matches JHora's printed tithi.
--   - Vyatipaata (yoga index 17) matches JHora's printed nitya yoga.
--   - Hora Lord Venus at the birth moment (23h33m21s since the prior day's
--     sunrise 5:56:39, i.e. the 24th hora; sequence starting from Tuesday's
--     lord Mars: Mars,Sun,Venus,Mercury,Moon,Saturn,Jupiter cycling ->
--     hora 24 = Venus) matches JHora's printed "Hora Lord: Venus" exactly.
--   - Janma Ghatis = minutes since sunrise / 24 = 1413.35 / 24 = 58.89,
--     matches JHora's printed "Janma Ghatis: 58.8892".
--
-- Deliberately NOT built here (no PVR ch.1 coverage found, so no source to
-- cite yet):
--   - Karana lord (PVR lists the 11 names + the movable/fixed repeat
--     pattern, SS1.3.10, but assigns no planetary lord).
--   - Nitya Yoga lord (PVR Table 5 gives name + meaning only; JHora prints
--     one (e.g. "Yoga: Vyatipata (Ra)") from a convention PVR ch.1 doesn't
--     state).
--   - Samvatsara (60-year Jupiter-cycle name, e.g. JHora's "Durmati") —
--     not in PVR ch.1 at all.
--   - Lunar month (JHora's "Lunar Yr-Mo: ... - Chaitra") — PVR Table 4
--     (rasi of Sun-Moon conjunction -> month name) exists, but the raw
--     book extract's table is OCR-garbled (a month/nakshatra misalignment
--     that contradicts classical Pausha/Magha/Phalguna nakshatra pairings)
--     and needs a clean page-image or second-edition cross-check before
--     seeding — deferred rather than seeding a guess.
--   - Mahakala Hora / Kaala Lord (JHora's 5-minute hora subdivision) — a
--     JHora extension beyond PVR ch.1's 24-hora scheme; not sourced.
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- --- Dimension layer -------------------------------------------------------

-- 30 tithis (lunar days): Sukla 1-15, Krishna 16-30. Each paksha repeats
-- the same 15 names; the planetary lord cycles Sun,Moon,Mars,Mercury,
-- Jupiter,Venus,Saturn,Rahu,[Sun..Saturn again] across the 15 slots.
-- SRC_PVR_INTEGRATED SS1.3.8.1 Table 3.
IF OBJECT_ID('dbo.tbl_Dim_Tithi', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Dim_Tithi (
        Id            TINYINT NOT NULL CONSTRAINT PK_Dim_Tithi PRIMARY KEY,
        Paksha        VARCHAR(10) NOT NULL,
        TithiNumber   TINYINT NOT NULL,
        Name          VARCHAR(30) NOT NULL,
        LordPlanetId  TINYINT NOT NULL CONSTRAINT FK_Dim_Tithi_Lord REFERENCES dbo.tbl_Planets (Id),
        SourceRefCode VARCHAR(40) NOT NULL CONSTRAINT FK_Dim_Tithi_Source REFERENCES dbo.tbl_Dim_Source (Code),
        CONSTRAINT CK_Dim_Tithi_Paksha CHECK (Paksha IN ('Shukla', 'Krishna')),
        CONSTRAINT CK_Dim_Tithi_Number CHECK (TithiNumber BETWEEN 1 AND 15),
        CONSTRAINT UQ_Dim_Tithi_PakshaNumber UNIQUE (Paksha, TithiNumber)
    );
END
GO

-- 11 karanas (half-tithis). The first 7 (movable) repeat 8x starting from
-- the 2nd half of the month's 1st tithi (56 half-tithi slots); the last 4
-- (fixed) each occur once, covering the 2nd half of the 29th tithi through
-- the 1st half of the next month's 1st tithi. No planetary lord in PVR.
-- SRC_PVR_INTEGRATED SS1.3.10.
IF OBJECT_ID('dbo.tbl_Dim_Karana', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Dim_Karana (
        Id                TINYINT NOT NULL CONSTRAINT PK_Dim_Karana PRIMARY KEY,
        Name              VARCHAR(30) NOT NULL,
        KaranaType        VARCHAR(10) NOT NULL,
        SequenceInCycle   TINYINT NOT NULL,
        SourceRefCode     VARCHAR(40) NOT NULL CONSTRAINT FK_Dim_Karana_Source REFERENCES dbo.tbl_Dim_Source (Code),
        CONSTRAINT CK_Dim_Karana_Type CHECK (KaranaType IN ('MOVABLE', 'FIXED')),
        CONSTRAINT CK_Dim_Karana_Seq CHECK (
            (KaranaType = 'MOVABLE' AND SequenceInCycle BETWEEN 1 AND 7)
            OR (KaranaType = 'FIXED' AND SequenceInCycle BETWEEN 1 AND 4)
        ),
        CONSTRAINT UQ_Dim_Karana_TypeSeq UNIQUE (KaranaType, SequenceInCycle)
    );
END
GO

-- 27 nitya yogas (Sun+Moon longitude sum, divided into 27 nakshatra-width
-- spans). Name + meaning only; PVR gives no planetary lord (JHora prints
-- one from an unstated convention). SRC_PVR_INTEGRATED SS1.3.9 Table 5.
IF OBJECT_ID('dbo.tbl_Dim_NityaYoga', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Dim_NityaYoga (
        Id            TINYINT NOT NULL CONSTRAINT PK_Dim_NityaYoga PRIMARY KEY,
        Name          VARCHAR(30) NOT NULL,
        Meaning       VARCHAR(100) NULL,
        SourceRefCode VARCHAR(40) NOT NULL CONSTRAINT FK_Dim_NityaYoga_Source REFERENCES dbo.tbl_Dim_Source (Code),
        CONSTRAINT CK_Dim_NityaYoga_Id CHECK (Id BETWEEN 1 AND 27)
    );
END
GO

-- 7 vaaras (weekdays), each ruled by a planet in the classical Sun..Saturn
-- order. SRC_PVR_INTEGRATED SS1.3.11.
IF OBJECT_ID('dbo.tbl_Dim_VedicWeekday', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Dim_VedicWeekday (
        Id            TINYINT NOT NULL CONSTRAINT PK_Dim_VedicWeekday PRIMARY KEY,
        Name          VARCHAR(10) NOT NULL,
        LordPlanetId  TINYINT NOT NULL CONSTRAINT FK_Dim_VedicWeekday_Lord REFERENCES dbo.tbl_Planets (Id),
        SourceRefCode VARCHAR(40) NOT NULL CONSTRAINT FK_Dim_VedicWeekday_Source REFERENCES dbo.tbl_Dim_Source (Code),
        CONSTRAINT CK_Dim_VedicWeekday_Id CHECK (Id BETWEEN 1 AND 7)
    );
END
GO

-- The 7-planet hora order (decreasing geocentric speed), the sequence a
-- day's 24 horas cycle through starting from the weekday lord.
-- SRC_PVR_INTEGRATED SS1.3.11.
IF OBJECT_ID('dbo.tbl_Dim_HoraSequence', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Dim_HoraSequence (
        Id            TINYINT NOT NULL CONSTRAINT PK_Dim_HoraSequence PRIMARY KEY,
        OrderIndex    TINYINT NOT NULL,
        PlanetId      TINYINT NOT NULL CONSTRAINT FK_Dim_HoraSequence_Planet REFERENCES dbo.tbl_Planets (Id),
        SourceRefCode VARCHAR(40) NOT NULL CONSTRAINT FK_Dim_HoraSequence_Source REFERENCES dbo.tbl_Dim_Source (Code),
        CONSTRAINT CK_Dim_HoraSequence_Order CHECK (OrderIndex BETWEEN 1 AND 7),
        CONSTRAINT UQ_Dim_HoraSequence_Order UNIQUE (OrderIndex),
        CONSTRAINT UQ_Dim_HoraSequence_Planet UNIQUE (PlanetId)
    );
END
GO

-- --- Rule layer: the derivation formulas, versioned + cited -----------------

IF OBJECT_ID('dbo.tbl_Rule_PanchangaFormula', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Rule_PanchangaFormula (
        Id                 INT IDENTITY(1,1) CONSTRAINT PK_Rule_PanchangaFormula PRIMARY KEY,
        RuleSetId          TINYINT NOT NULL CONSTRAINT FK_Rule_PanchangaFormula_RuleSet REFERENCES dbo.tbl_Rule_Sets (Id),
        ElementCode        VARCHAR(20) NOT NULL,
        FormulaNarrative   NVARCHAR(MAX) NOT NULL,
        SourceRefCode      VARCHAR(40) NOT NULL CONSTRAINT FK_Rule_PanchangaFormula_Source REFERENCES dbo.tbl_Dim_Source (Code),
        SourceLocator      NVARCHAR(200) NULL,
        IsActive           BIT NOT NULL CONSTRAINT DF_Rule_PanchangaFormula_IsActive DEFAULT (1),
        CONSTRAINT CK_Rule_PanchangaFormula_Element CHECK (ElementCode IN ('TITHI', 'KARANA', 'NITYA_YOGA', 'HORA_LORD')),
        CONSTRAINT UQ_Rule_PanchangaFormula UNIQUE (RuleSetId, ElementCode)
    );
END
GO

-- --- Fact layer: one row per D1 ChartResultId (the birth moment, not a
--     per-varga concept — mirrors tbl_Chart_DashaPeriods' ChartResultId key).
IF OBJECT_ID('dbo.tbl_Chart_Panchanga', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Chart_Panchanga (
        Id                      INT IDENTITY(1,1) CONSTRAINT PK_Chart_Panchanga PRIMARY KEY,
        ChartResultId           INT NOT NULL CONSTRAINT FK_Chart_Panchanga_ChartResult REFERENCES dbo.tbl_ChartResults (Id) ON DELETE CASCADE,
        RuleSetId               TINYINT NOT NULL CONSTRAINT FK_Chart_Panchanga_RuleSet REFERENCES dbo.tbl_Rule_Sets (Id),
        SunriseUtc              DATETIME2(0) NOT NULL,
        SunsetUtc               DATETIME2(0) NOT NULL,
        NextSunriseUtc          DATETIME2(0) NOT NULL,
        IsNightBirth            BIT NOT NULL,
        JanmaGhatis             DECIMAL(9, 4) NOT NULL,
        SunMoonDeltaDegrees     DECIMAL(9, 6) NOT NULL,
        TithiId                 TINYINT NOT NULL CONSTRAINT FK_Chart_Panchanga_Tithi REFERENCES dbo.tbl_Dim_Tithi (Id),
        TithiPercentRemaining   DECIMAL(6, 3) NOT NULL,
        KaranaId                TINYINT NOT NULL CONSTRAINT FK_Chart_Panchanga_Karana REFERENCES dbo.tbl_Dim_Karana (Id),
        KaranaPercentRemaining  DECIMAL(6, 3) NOT NULL,
        SunMoonSumDegrees       DECIMAL(9, 6) NOT NULL,
        NityaYogaId             TINYINT NOT NULL CONSTRAINT FK_Chart_Panchanga_NityaYoga REFERENCES dbo.tbl_Dim_NityaYoga (Id),
        NityaYogaPercentRemaining DECIMAL(6, 3) NOT NULL,
        VedicWeekdayId          TINYINT NOT NULL CONSTRAINT FK_Chart_Panchanga_Weekday REFERENCES dbo.tbl_Dim_VedicWeekday (Id),
        HoraLordPlanetId        TINYINT NOT NULL CONSTRAINT FK_Chart_Panchanga_HoraLord REFERENCES dbo.tbl_Planets (Id),
        ComputedAtUtc           DATETIME2(0) NOT NULL CONSTRAINT DF_Chart_Panchanga_Computed DEFAULT (sysutcdatetime()),
        CONSTRAINT CK_Chart_Panchanga_Dates CHECK (SunriseUtc < SunsetUtc AND SunsetUtc < NextSunriseUtc),
        CONSTRAINT CK_Chart_Panchanga_Ghatis CHECK (JanmaGhatis BETWEEN 0 AND 60),
        CONSTRAINT CK_Chart_Panchanga_Delta CHECK (SunMoonDeltaDegrees >= 0 AND SunMoonDeltaDegrees < 360),
        CONSTRAINT CK_Chart_Panchanga_Sum CHECK (SunMoonSumDegrees >= 0 AND SunMoonSumDegrees < 360),
        CONSTRAINT CK_Chart_Panchanga_TithiPct CHECK (TithiPercentRemaining BETWEEN 0 AND 100),
        CONSTRAINT CK_Chart_Panchanga_KaranaPct CHECK (KaranaPercentRemaining BETWEEN 0 AND 100),
        CONSTRAINT CK_Chart_Panchanga_YogaPct CHECK (NityaYogaPercentRemaining BETWEEN 0 AND 100),
        CONSTRAINT UQ_Chart_Panchanga_ChartResult UNIQUE (ChartResultId)
    );
END
GO

-- --- UI read view (project_standards.md section 4: table <-> view) --------

CREATE OR ALTER VIEW dbo.vw_ChartPanchanga
AS
SELECT c.BirthDetailId, p.ChartResultId, p.RuleSetId,
       p.SunriseUtc, p.SunsetUtc, p.NextSunriseUtc, p.IsNightBirth, p.JanmaGhatis,
       t.Paksha, t.Name AS TithiName, t.LordPlanetId AS TithiLordPlanetId, p.TithiPercentRemaining,
       k.Name AS KaranaName, k.KaranaType, p.KaranaPercentRemaining,
       y.Name AS NityaYogaName, y.Meaning AS NityaYogaMeaning, p.NityaYogaPercentRemaining,
       w.Name AS VedicWeekdayName,
       hl.PlanetName AS HoraLordName,
       p.ComputedAtUtc
FROM dbo.tbl_Chart_Panchanga p
JOIN dbo.tbl_ChartResults c ON c.Id = p.ChartResultId
JOIN dbo.tbl_Dim_Tithi t ON t.Id = p.TithiId
JOIN dbo.tbl_Dim_Karana k ON k.Id = p.KaranaId
JOIN dbo.tbl_Dim_NityaYoga y ON y.Id = p.NityaYogaId
JOIN dbo.tbl_Dim_VedicWeekday w ON w.Id = p.VedicWeekdayId
JOIN dbo.tbl_Planets hl ON hl.Id = p.HoraLordPlanetId;
GO

-- --- Seed: dimensions --------------------------------------------------

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Dim_Tithi)
BEGIN
    ;WITH names (TithiNumber, Name, LordPlanetId) AS (
        SELECT * FROM (VALUES
            (1,  'Pratipat',   1), (2,  'Dwitiya',    2), (3,  'Tritiya',    3),
            (4,  'Chaturthi',  4), (5,  'Panchami',   5), (6,  'Shashti',    6),
            (7,  'Saptami',    7), (8,  'Ashtami',    8), (9,  'Navami',     1),
            (10, 'Dasami',     2), (11, 'Ekadasi',    3), (12, 'Dwadasi',    4),
            (13, 'Trayodasi',  5), (14, 'Chaturdasi', 6), (15, 'Purnima',    7)
        ) v (TithiNumber, Name, LordPlanetId)
    )
    INSERT dbo.tbl_Dim_Tithi (Id, Paksha, TithiNumber, Name, LordPlanetId, SourceRefCode)
    SELECT TithiNumber, 'Shukla', TithiNumber, Name, LordPlanetId, 'SRC_PVR_INTEGRATED' FROM names
    UNION ALL
    SELECT 15 + TithiNumber, 'Krishna', TithiNumber,
           CASE WHEN TithiNumber = 15 THEN 'Amavasya' ELSE Name END,
           LordPlanetId, 'SRC_PVR_INTEGRATED'
    FROM names;
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Dim_Karana)
BEGIN
    INSERT dbo.tbl_Dim_Karana (Id, Name, KaranaType, SequenceInCycle, SourceRefCode) VALUES
        (1,  'Bava',        'MOVABLE', 1, 'SRC_PVR_INTEGRATED'),
        (2,  'Balava',      'MOVABLE', 2, 'SRC_PVR_INTEGRATED'),
        (3,  'Kaulava',     'MOVABLE', 3, 'SRC_PVR_INTEGRATED'),
        (4,  'Taitula',     'MOVABLE', 4, 'SRC_PVR_INTEGRATED'),
        (5,  'Garija',      'MOVABLE', 5, 'SRC_PVR_INTEGRATED'),
        (6,  'Vanija',      'MOVABLE', 6, 'SRC_PVR_INTEGRATED'),
        (7,  'Vishti',      'MOVABLE', 7, 'SRC_PVR_INTEGRATED'),
        (8,  'Sakuna',      'FIXED',   1, 'SRC_PVR_INTEGRATED'),
        (9,  'Chatushpada', 'FIXED',   2, 'SRC_PVR_INTEGRATED'),
        (10, 'Naga',        'FIXED',   3, 'SRC_PVR_INTEGRATED'),
        (11, 'Kimstughna',  'FIXED',   4, 'SRC_PVR_INTEGRATED');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Dim_NityaYoga)
BEGIN
    INSERT dbo.tbl_Dim_NityaYoga (Id, Name, Meaning, SourceRefCode) VALUES
        (1,  'Vishkambha', 'Door bolt/supporting pillar',            'SRC_PVR_INTEGRATED'),
        (2,  'Preeti',     'Love/affection',                         'SRC_PVR_INTEGRATED'),
        (3,  'Aayushmaan', 'Long-lived',                             'SRC_PVR_INTEGRATED'),
        (4,  'Saubhaagya', 'Long life of spouse (good fortune)',     'SRC_PVR_INTEGRATED'),
        (5,  'Sobhana',    'Splendid, bright',                       'SRC_PVR_INTEGRATED'),
        (6,  'Atiganda',   'Great danger',                           'SRC_PVR_INTEGRATED'),
        (7,  'Sukarman',   'One with good deeds',                    'SRC_PVR_INTEGRATED'),
        (8,  'Dhriti',     'Firmness',                                'SRC_PVR_INTEGRATED'),
        (9,  'Shoola',     N'Shiva''s weapon of destruction (pain)',  'SRC_PVR_INTEGRATED'),
        (10, 'Ganda',      'Danger',                                  'SRC_PVR_INTEGRATED'),
        (11, 'Vriddhi',    'Growth',                                  'SRC_PVR_INTEGRATED'),
        (12, 'Dhruva',     'Fixed, constant',                        'SRC_PVR_INTEGRATED'),
        (13, 'Vyaaghaata', 'Great blow',                              'SRC_PVR_INTEGRATED'),
        (14, 'Harshana',   'Cheerful',                                'SRC_PVR_INTEGRATED'),
        (15, 'Vajra',      'Diamond (strong)',                       'SRC_PVR_INTEGRATED'),
        (16, 'Siddhi',     'Accomplishment',                         'SRC_PVR_INTEGRATED'),
        (17, 'Vyatipaata', 'Great fall',                              'SRC_PVR_INTEGRATED'),
        (18, 'Variyan',    'Chief/best',                              'SRC_PVR_INTEGRATED'),
        (19, 'Parigha',    'Obstacle/hindrance',                     'SRC_PVR_INTEGRATED'),
        (20, 'Shiva',      N'Lord Shiva (purity)',                    'SRC_PVR_INTEGRATED'),
        (21, 'Siddha',     'Accomplished/ready',                     'SRC_PVR_INTEGRATED'),
        (22, 'Saadhya',    'Possible',                                'SRC_PVR_INTEGRATED'),
        (23, 'Subha',      'Auspicious',                              'SRC_PVR_INTEGRATED'),
        (24, 'Sukla',      'White, bright',                          'SRC_PVR_INTEGRATED'),
        (25, 'Brahma',     'Creator (good knowledge and purity)',    'SRC_PVR_INTEGRATED'),
        (26, 'Indra',      'Ruler of gods',                          'SRC_PVR_INTEGRATED'),
        (27, 'Vaidhriti',  'A class of gods',                        'SRC_PVR_INTEGRATED');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Dim_VedicWeekday)
BEGIN
    INSERT dbo.tbl_Dim_VedicWeekday (Id, Name, LordPlanetId, SourceRefCode) VALUES
        (1, 'Sunday',    1, 'SRC_PVR_INTEGRATED'),
        (2, 'Monday',    2, 'SRC_PVR_INTEGRATED'),
        (3, 'Tuesday',   3, 'SRC_PVR_INTEGRATED'),
        (4, 'Wednesday', 4, 'SRC_PVR_INTEGRATED'),
        (5, 'Thursday',  5, 'SRC_PVR_INTEGRATED'),
        (6, 'Friday',    6, 'SRC_PVR_INTEGRATED'),
        (7, 'Saturday',  7, 'SRC_PVR_INTEGRATED');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Dim_HoraSequence)
BEGIN
    -- Decreasing geocentric speed: Saturn, Jupiter, Mars, Sun, Venus, Mercury, Moon.
    INSERT dbo.tbl_Dim_HoraSequence (Id, OrderIndex, PlanetId, SourceRefCode) VALUES
        (1, 1, 7, 'SRC_PVR_INTEGRATED'),  -- Saturn
        (2, 2, 5, 'SRC_PVR_INTEGRATED'),  -- Jupiter
        (3, 3, 3, 'SRC_PVR_INTEGRATED'),  -- Mars
        (4, 4, 1, 'SRC_PVR_INTEGRATED'),  -- Sun
        (5, 5, 6, 'SRC_PVR_INTEGRATED'),  -- Venus
        (6, 6, 4, 'SRC_PVR_INTEGRATED'),  -- Mercury
        (7, 7, 2, 'SRC_PVR_INTEGRATED');  -- Moon
END
GO

-- --- Seed: formulas (RuleSetId 1 = the active Parashari-Classical set) -----

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_PanchangaFormula WHERE RuleSetId = 1)
BEGIN
    INSERT dbo.tbl_Rule_PanchangaFormula (RuleSetId, ElementCode, FormulaNarrative, SourceRefCode, SourceLocator) VALUES
    (1, 'TITHI',
        N'delta = normalize360(MoonLongitude - SunLongitude); index = floor(delta / 12) + 1 (1-30); index 1-15 = Shukla, 16-30 = Krishna (tbl_Dim_Tithi.TithiNumber = ((index-1) mod 15) + 1). PercentRemaining = 100 - ((delta mod 12) / 12 * 100).',
        'SRC_PVR_INTEGRATED', N'SS1.3.8.1, Table 3, Example 2'),
    (1, 'NITYA_YOGA',
        N'sum = normalize360(SunLongitude + MoonLongitude); index = floor(sum / (800/60)) + 1 (1-27, nakshatra-width span = 13d20'' = 800''). PercentRemaining = 100 - ((sum mod (800/60)) / (800/60) * 100).',
        'SRC_PVR_INTEGRATED', N'SS1.3.9, Table 5, Example 3'),
    (1, 'KARANA',
        N'halfTithiPosition (0-based, 0 = 2nd half of tithi 1) = ((tithiIndex-1)*2 + (secondHalf ? 1 : 0)) - 1, wrapped into the current lunar month. Positions 0-55 (56 slots, 8 full cycles) = the 7 movable karanas (tbl_Dim_Karana.SequenceInCycle = (position mod 7) + 1); positions 56-59 (last 4 half-tithis of the month, i.e. 2nd half of tithi 29 through 1st half of tithi 1 of the next month) = the 4 fixed karanas in order. PercentRemaining mirrors the half-tithi''s own remaining fraction (delta mod 6 / 6 * 100).',
        'SRC_PVR_INTEGRATED', N'SS1.3.10'),
    (1, 'HORA_LORD',
        N'A day runs sunrise-to-next-sunrise, split into 24 equal horas. horaNumber = floor(minutesSinceSunrise / (dayLengthMinutes/24)) + 1 (1-24). The hora order cycles tbl_Dim_HoraSequence (Saturn,Jupiter,Mars,Sun,Venus,Mercury,Moon), starting the cycle at the day''s VedicWeekday lord: lordIndex = position of weekday lord in the sequence; hora N''s lord = sequence[(lordIndex + N - 1) mod 7]. Cross-checked: Ramakrishnan''s 24th hora from the Tuesday (Mars-start) cycle resolves to Venus, matching JHora''s printed Hora Lord exactly.',
        'SRC_PVR_INTEGRATED', N'SS1.3.11, worked example');
END
GO

INSERT dbo.SchemaMigrations (ScriptName, Note)
SELECT '081_create_panchanga_schema.sql',
       'Panchanga schema+seed: Tithi/Karana/NityaYoga/VedicWeekday/HoraSequence, PanchangaFormula rule, tbl_Chart_Panchanga, vw_ChartPanchanga. SRC_PVR_INTEGRATED SS1.3.8-12. Calculator is a later cli change.'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '081_create_panchanga_schema.sql');
GO

PRINT '081 applied: Panchanga schema created and seeded (Tithi/Karana/NityaYoga/VedicWeekday/HoraSequence + formulas).';
GO
