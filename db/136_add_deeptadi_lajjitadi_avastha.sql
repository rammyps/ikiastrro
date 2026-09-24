-- =====================================================================
-- Dīptādi + Lajjitādi avastha layer (PVR sec 15.4.3, "state related to attitude and mood").
-- Verified directly against the raw PVR extract, not a secondhand summary — see
-- D:\@ClaudeSpace\BookExtracts\pvr-integrated-approach-raw.txt lines 7177-7225.
--
-- The book gives 9 Dīptādi states and 6 more (unnamed as a group in the book; called Lajjitādi
-- elsewhere in the literature) with NO stated precedence between them — several can hold for one
-- planet at once. Modelled as a multi-row child fact table, not more flat columns on
-- tbl_Fact_PlanetaryState, same reasoning as tbl_Fact_Argala (migration 128) flattening a
-- variable-cardinality result into a child table.
--
--   tbl_Rule_DeeptadiState     — DignityStatus -> one of Dīptādi's 6 dignity-tier states (Rule)
--   tbl_Fact_PlanetaryStateFlag — generic multi-row child of tbl_Fact_PlanetaryState: every matched
--                                 Deeptadi/Lajjitadi state per planet per chart (Fact)
--
-- Idempotent (IF NOT EXISTS / IF OBJECT_ID guards), same style as db/00_add_avastha_star_schema.sql.
-- =====================================================================
USE [ikiastrro];
GO

-- 1. tbl_Dim_PlanetaryState — seed the 9 Deeptadi + 6 Lajjitadi states -----------------------------
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Dim_PlanetaryState WHERE AvasthaSystem = 'Deeptadi')
INSERT dbo.tbl_Dim_PlanetaryState (AvasthaSystem, StateName, SequenceOrder, Meaning) VALUES
    ('Deeptadi', 'Deepta',   1, 'Bright — exaltation sign'),
    ('Deeptadi', 'Swastha',  2, 'Doing well, contented, natural — own sign (incl. moolatrikona)'),
    ('Deeptadi', 'Mudita',   3, 'Delighted — great friend''s sign'),
    ('Deeptadi', 'Saanta',   4, 'Peaceful — friend''s sign'),
    ('Deeptadi', 'Deena',    5, 'Sad, depressed — neutral planet''s sign'),
    ('Deeptadi', 'Duhkhita', 6, 'Distressed, miserable — enemy''s sign (incl. great enemy / debilitated)'),
    ('Deeptadi', 'Vikala',   7, 'Crippled, confused — joined by malefic planet(s)'),
    ('Deeptadi', 'Khala',    8, 'Mischievous, scheming — in a malefic planet''s sign'),
    ('Deeptadi', 'Kopita',   9, 'Angry — joined closely by the Sun (combust)');
GO
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Dim_PlanetaryState WHERE AvasthaSystem = 'Lajjitadi')
INSERT dbo.tbl_Dim_PlanetaryState (AvasthaSystem, StateName, SequenceOrder, Meaning) VALUES
    ('Lajjitadi', 'Lajjita',   1, 'Ashamed — in the 5th house, joined by Sun/Mars/Saturn/Rahu/Ketu'),
    ('Lajjitadi', 'Garvita',   2, 'Proud — exaltation or moolatrikona sign'),
    ('Lajjitadi', 'Kshudhita', 3, 'Hungry — enemy''s sign, or conjoined/aspected by enemies, or conjoined by Saturn'),
    ('Lajjitadi', 'Trishita',  4, 'Thirsty — watery sign, aspected by enemies, without benefic aspect'),
    ('Lajjitadi', 'Mudita',    5, 'Delighted — friend''s sign, conjoined/aspected by friends, or conjoined by Jupiter'),
    ('Lajjitadi', 'Kshobhita', 6, 'Shaken, agitated — conjoined by Sun and aspected by malefics or enemies');
GO

-- 2. tbl_Rule_DeeptadiState — DignityStatus -> Deeptadi dignity tier, same shape as tbl_Rule_WakefulnessState
IF OBJECT_ID('dbo.tbl_Rule_DeeptadiState', 'U') IS NULL
CREATE TABLE dbo.tbl_Rule_DeeptadiState (
    Id             TINYINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Rule_DeeptadiState PRIMARY KEY,
    RuleSetId      TINYINT     NOT NULL CONSTRAINT FK_Rule_DeeptadiState_RuleSet FOREIGN KEY REFERENCES dbo.tbl_Rule_Sets (Id),
    DignityStatus  VARCHAR(20) NOT NULL,
    AvasthaStateId TINYINT     NOT NULL CONSTRAINT FK_Rule_DeeptadiState_State   FOREIGN KEY REFERENCES dbo.tbl_Dim_PlanetaryState (Id),
    MethodCode           VARCHAR(30)   NULL,
    RuleParametersJson   NVARCHAR(MAX) NULL,
    CalculationNarrative NVARCHAR(MAX) NULL,
    SourceRefCode        VARCHAR(40)   NULL,
    IsActive             BIT NOT NULL CONSTRAINT DF_Rule_DeeptadiState_IsActive DEFAULT 1,
    CONSTRAINT UQ_Rule_DeeptadiState UNIQUE (RuleSetId, DignityStatus),
    CONSTRAINT CK_RuleDeeptadiState_Json CHECK (RuleParametersJson IS NULL OR ISJSON(RuleParametersJson) = 1),
    CONSTRAINT CK_RuleDeeptadiState_Src  CHECK (SourceRefCode IS NULL OR SourceRefCode LIKE 'SRC[_]%')
);
GO
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_DeeptadiState)
INSERT dbo.tbl_Rule_DeeptadiState (RuleSetId, DignityStatus, AvasthaStateId, SourceRefCode, CalculationNarrative)
SELECT 1, v.DignityStatus, s.Id, 'SRC_PVR_INTEGRATED', v.Narrative
FROM (VALUES
    ('Exalted',      'Deepta',   N'PVR sec 15.4.3 (1): exaltation rasi.'),
    ('Moolatrikona', 'Swastha',  N'PVR sec 15.4.3 (2) names only "own rasi"; moolatrikona folded in here as the classical sub-range of the own sign — same folding shape tbl_Rule_WakefulnessState already uses for Jagradadi.'),
    ('Own Sign',     'Swastha',  N'PVR sec 15.4.3 (2): own rasi.'),
    ('Great Friend', 'Mudita',   N'PVR sec 15.4.3 (3): "a good friend''s rasi."'),
    ('Friend',       'Saanta',   N'PVR sec 15.4.3 (4): "a friend''s rasi."'),
    ('Neutral',      'Deena',    N'PVR sec 15.4.3 (5): "a neutral planet''s rasi."'),
    ('Enemy',        'Duhkhita', N'PVR sec 15.4.3 (6): "an enemy''s rasi."'),
    ('Great Enemy',  'Duhkhita', N'PVR names only "an enemy''s rasi" (6 tiers); this project''s DignityStatus splits Enemy/Great Enemy/Debilitated into 3 — folded onto Duhkhita alongside Enemy, same shape tbl_Rule_WakefulnessState uses for Jagradadi''s Sushupti tier.'),
    ('Debilitated',  'Duhkhita', N'Not named in PVR sec 15.4.3''s 6 tiers at all; folded onto Duhkhita (weakest tier) for the same reason as Great Enemy above.')
) v (DignityStatus, StateName, Narrative)
JOIN dbo.tbl_Dim_PlanetaryState s ON s.AvasthaSystem = 'Deeptadi' AND s.StateName = v.StateName;
GO

-- 3. tbl_Rule_Catalog — index the new rule table (verify-rules requires every tbl_Rule_* here) -----
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_Catalog WHERE RuleTableName = 'tbl_Rule_DeeptadiState')
INSERT dbo.tbl_Rule_Catalog (RuleTableName, EngineCode, MethodCodes, Purpose, IntroducedIn)
VALUES ('tbl_Rule_DeeptadiState', 'AVASTHA', 'MAP_LOOKUP',
    'Deeptadi avastha''s 6 dignity-tier states (Deepta..Duhkhita) keyed by the planet''s dignity status; the other 3 Deeptadi states and all 6 Lajjitadi states are computed directly (no rule row).',
    'migration 136');
GO

-- 4. tbl_Fact_PlanetaryStateFlag — generic multi-row child of tbl_Fact_PlanetaryState --------------
IF OBJECT_ID('dbo.tbl_Fact_PlanetaryStateFlag', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Fact_PlanetaryStateFlag (
        Id                    INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Fact_PlanetaryStateFlag PRIMARY KEY,
        PlanetaryStateFactId  INT     NOT NULL CONSTRAINT FK_Fact_PlanetaryStateFlag_Fact  FOREIGN KEY REFERENCES dbo.tbl_Fact_PlanetaryState (Id) ON DELETE CASCADE,
        AvasthaStateId        TINYINT NOT NULL CONSTRAINT FK_Fact_PlanetaryStateFlag_State FOREIGN KEY REFERENCES dbo.tbl_Dim_PlanetaryState (Id),
        CONSTRAINT UQ_Fact_PlanetaryStateFlag UNIQUE (PlanetaryStateFactId, AvasthaStateId)
    );
    CREATE NONCLUSTERED INDEX IX_Fact_PlanetaryStateFlag_FactId ON dbo.tbl_Fact_PlanetaryStateFlag (PlanetaryStateFactId);
END
GO

-- 5. vw_Chart_Consolidated — add DeeptadiStates / LajjitadiStates (comma-joined) to the current
-- view body (db/ikiastrro.sql's consolidated copy, post migration 16's PlanetId-keyed rebuild) -----
IF OBJECT_ID('dbo.vw_Chart_Consolidated', 'V') IS NOT NULL
    DROP VIEW dbo.vw_Chart_Consolidated;
GO
EXEC dbo.sp_executesql @statement = N'CREATE VIEW [dbo].[vw_Chart_Consolidated] AS
SELECT
    bd.Id                       AS BirthDetailId,
    bd.Name,
    bd.DateOfBirth,
    bd.TimeOfBirth,
    bd.PlaceCity,
    bd.PlaceCountry,
    cr.Id                       AS ChartResultId,
    cr.ChartType,
    cr.Ayanamsha,
    cr.HouseSystem,
    cr.EngineVersion,
    kd.Planet,
    kd.PointKind,
    kd.CharaKaraka,
    kd.NirayanaLongitudeDegrees,
    kd.VargaLongitudeDegrees,
    kd.EclipticLatitudeDegrees,
    kd.SpeedLongitudeDegPerDay,
    kd.IsRetrograde,
    kd.Sign,
    kd.DegreesInSignDecimal,
    kd.DegreesInSignDisplay,
    kd.Nakshatra,
    kd.NakshatraPada,
    kd.NakshatraLordPlanet,
    kd.IsCombust,
    kd.DistanceFromSunDegrees,
    kd.CombustionOrbUsedDegrees,
    kd.HouseNumberFromLagna,
    kd.HouseNumberFromSun,
    kd.HouseNumberFromMoon,
    kd.OwnSigns,
    kd.ExaltationSign,
    kd.DebilitationSign,
    kd.MoolatrikonaSign,
    kd.MoolatrikonaRange,
    kd.SignLordPlanet,
    kd.DignityStatus,
    ageState.StateName           AS AgeState,
    av.AgeEffectFraction,
    wakeState.StateName          AS WakefulnessState,
    Deeptadi.StateList           AS DeeptadiStates,
    Lajjitadi.StateList          AS LajjitadiStates,
    RulesHouses.HouseList        AS RulesHouseNumbers,
    Conjunct.PlanetList           AS ConjunctWith,
    AspectsCast.TargetList        AS Aspects,
    kd.AspectingPlanets          AS AspectedBy,
    cr.ComputedAt
FROM dbo.tbl_Chart_KeyDetails kd
JOIN dbo.tbl_ChartResults cr  ON cr.Id = kd.ChartResultId
JOIN dbo.tbl_BirthDetails bd  ON bd.Id = cr.BirthDetailId
LEFT JOIN dbo.tbl_Fact_PlanetaryState av ON av.ChartResultId = kd.ChartResultId AND av.PlanetId = kd.PlanetId
LEFT JOIN dbo.tbl_Dim_PlanetaryState  ageState  ON ageState.Id  = av.AgeStateId
LEFT JOIN dbo.tbl_Dim_PlanetaryState  wakeState ON wakeState.Id = av.WakefulnessStateId
OUTER APPLY (
    SELECT STRING_AGG(s.StateName, '','') WITHIN GROUP (ORDER BY s.SequenceOrder) AS StateList
    FROM dbo.tbl_Fact_PlanetaryStateFlag f
    JOIN dbo.tbl_Dim_PlanetaryState s ON s.Id = f.AvasthaStateId AND s.AvasthaSystem = ''Deeptadi''
    WHERE f.PlanetaryStateFactId = av.Id
) Deeptadi
OUTER APPLY (
    SELECT STRING_AGG(s.StateName, '','') WITHIN GROUP (ORDER BY s.SequenceOrder) AS StateList
    FROM dbo.tbl_Fact_PlanetaryStateFlag f
    JOIN dbo.tbl_Dim_PlanetaryState s ON s.Id = f.AvasthaStateId AND s.AvasthaSystem = ''Lajjitadi''
    WHERE f.PlanetaryStateFactId = av.Id
) Lajjitadi
OUTER APPLY (
    SELECT STRING_AGG(CAST(hl.HouseNumber AS VARCHAR(2)), '','') WITHIN GROUP (ORDER BY hl.HouseNumber) AS HouseList
    FROM dbo.tbl_Chart_HouseLords hl
    WHERE hl.ChartResultId = kd.ChartResultId AND hl.LordPlanetId = kd.PlanetId
) RulesHouses
OUTER APPLY (
    SELECT STRING_AGG(other_planet, '', '') AS PlanetList
    FROM (
        SELECT Planet2 AS other_planet FROM dbo.tbl_Chart_Conjunctions
            WHERE ChartResultId = kd.ChartResultId AND Planet1Id = kd.PlanetId
        UNION ALL
        SELECT Planet1 FROM dbo.tbl_Chart_Conjunctions
            WHERE ChartResultId = kd.ChartResultId AND Planet2Id = kd.PlanetId
    ) x
) Conjunct
OUTER APPLY (
    SELECT STRING_AGG(CONCAT(AspectedTarget, '' ('', AspectType, '')''), '', '') AS TargetList
    FROM dbo.tbl_Chart_Aspects
    WHERE ChartResultId = kd.ChartResultId AND AspectingPlanetId = kd.PlanetId
) AspectsCast;';
GO

INSERT dbo.SchemaMigrations (ScriptName, Note)
SELECT '136_add_deeptadi_lajjitadi_avastha.sql',
       'Deeptadi (9 states) + Lajjitadi (6 states) avastha per PVR sec 15.4.3. tbl_Rule_DeeptadiState + multi-row tbl_Fact_PlanetaryStateFlag (no book precedence, several states can co-occur).'
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.SchemaMigrations
    WHERE ScriptName = '136_add_deeptadi_lajjitadi_avastha.sql'
);
GO

PRINT 'Deeptadi/Lajjitadi avastha layer ready. Now run:  dotnet run --project src/Ikiastrro.Cli -- recompute-keydetails';
GO
