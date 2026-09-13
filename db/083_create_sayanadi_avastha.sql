-- =====================================================================
-- 083 — Sayanaadi Avastha (PostureState, FEAT-AVASTHA-05): the 12-state
--       "activity" avastha. Unblocks the item masterproduct.md flagged as
--       "needs persisted janma ghatis, source-blocked" — migration 081
--       (tbl_Chart_Panchanga.JanmaGhatis) supplies the first, and
--       SRC_PVR_INTEGRATED sec 15.4.4 (Table 36) supplies the second.
--
-- Formula (PVR sec 15.4.4): index = ((C x P x A) + M + G + L) mod 12,
-- remainder 0 -> 12. Verified against the JHora export for
-- 1_Ramakrishnan's printed "Activity" table: hand-computed Sun (C=1
-- Aswini, P=1, A=3rd navamsa, M=17 Anuraadha, G=59th ghati, L=1 Aries)
-- -> index 8 = Aagama, matching JHora exactly; same for Moon -> index 11
-- = Kautuka. The C#/CLI calculator is a database-workstream follow-up.
--
-- Deliberately not built in this pass — PVR sec 15.4.4's secondary
-- Cheshta/Drishti/Vicheshta activity-strength refinement needs Table 37's
-- Sanskrit sound-to-number map, which the raw book extract renders
-- ambiguously (columns misaligned by OCR, same class of problem as
-- migration 081's deferred lunar-month table) — only the base 12-state
-- avastha (Table 36) is seeded here.
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- --- Dim: the 12 Sayanaadi state names (mirrors the existing Baaladi/Jagradadi seed) ---

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Dim_PlanetaryState WHERE AvasthaSystem = 'Sayanadi')
INSERT dbo.tbl_Dim_PlanetaryState (AvasthaSystem, StateName, SequenceOrder, Meaning) VALUES
    ('Sayanadi', 'Sayana',      1,  'Lying down, resting'),
    ('Sayanadi', 'Upavesana',   2,  'Sitting down'),
    ('Sayanadi', 'Netrapaani',  3,  'Eyes and hands'),
    ('Sayanadi', 'Prakaasana',  4,  'Shining'),
    ('Sayanadi', 'Gamana',      5,  'Going (on the move)'),
    ('Sayanadi', 'Aagamana',    6,  'Coming, returning'),
    ('Sayanadi', 'Sabhaa',      7,  'Being at an assembly'),
    ('Sayanadi', 'Aagama',      8,  'Coming, acquiring'),
    ('Sayanadi', 'Bhojana',     9,  'Eating'),
    ('Sayanadi', 'Nriyalipsaa', 10, 'Longing to dance'),
    ('Sayanadi', 'Kautuka',     11, 'Being eager'),
    ('Sayanadi', 'Nidraa',      12, 'Sleeping');
GO

-- --- Rule: the one formula, versioned + cited (mirrors migration 081's tbl_Rule_PanchangaFormula) ---

IF OBJECT_ID('dbo.tbl_Rule_PostureStateFormula', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Rule_PostureStateFormula (
        Id               INT IDENTITY(1,1) CONSTRAINT PK_Rule_PostureStateFormula PRIMARY KEY,
        RuleSetId        TINYINT NOT NULL CONSTRAINT FK_Rule_PostureStateFormula_RuleSet REFERENCES dbo.tbl_Rule_Sets (Id),
        FormulaNarrative NVARCHAR(MAX) NOT NULL,
        SourceRefCode    VARCHAR(40) NOT NULL CONSTRAINT FK_Rule_PostureStateFormula_Source REFERENCES dbo.tbl_Dim_Source (Code),
        SourceLocator    NVARCHAR(200) NULL,
        IsActive         BIT NOT NULL CONSTRAINT DF_Rule_PostureStateFormula_IsActive DEFAULT (1),
        CONSTRAINT UQ_Rule_PostureStateFormula UNIQUE (RuleSetId)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_PostureStateFormula WHERE RuleSetId = 1)
INSERT dbo.tbl_Rule_PostureStateFormula (RuleSetId, FormulaNarrative, SourceRefCode, SourceLocator) VALUES
(1, N'index = ((C * P * A) + M + G + L) mod 12, remainder 0 read as 12. C = the planet''s own nakshatra number (1-27, Aswini=1). P = the planet''s index (Sun=1, Moon=2, Mars=3, Mercury=4, Jupiter=5, Venus=6, Saturn=7, Rahu=8, Ketu=9 - matches tbl_Planets.Id). A = the navamsa the planet occupies within its own rasi (1-9) = floor(degreeInSign / (30/9)) + 1. M = Moon''s own nakshatra number (1-27). G = the ghati running at birth (1-60) = floor(JanmaGhatis) + 1 (tbl_Chart_Panchanga.JanmaGhatis, migration 081). L = the rasi occupied by Lagna (1-12, Aries=1). Look up the resulting index against tbl_Dim_PlanetaryState (AvasthaSystem=''Sayanadi'') for the state name.',
'SRC_PVR_INTEGRATED', N'§15.4.4, Table 36, worked example (footnotes 51-52)');
GO

-- --- Fact: one more nullable column on the existing per-chart avastha fact row ---

IF COL_LENGTH('dbo.tbl_Fact_PlanetaryState', 'PostureStateId') IS NULL
BEGIN
    ALTER TABLE dbo.tbl_Fact_PlanetaryState ADD PostureStateId TINYINT NULL
        CONSTRAINT FK_Fact_PlanetaryState_Posture REFERENCES dbo.tbl_Dim_PlanetaryState (Id);
END
GO

-- --- UI read view: extend the existing evidence view (project_standards.md section 4) ---

CREATE OR ALTER VIEW dbo.vw_ChartPlanetEvidence
AS
SELECT c.BirthDetailId, c.Id AS ChartResultId, c.ChartType, c.RuleSetId,
       k.Planet, k.PointKind, k.Sign, k.NirayanaLongitudeDegrees,
       k.VargaLongitudeDegrees, k.DegreesInSignDisplay,
       k.HouseNumberFromLagna, k.HouseNumberFromMoon, k.HouseNumberFromSun,
       k.Nakshatra, k.NakshatraPada, k.NakshatraLordPlanet,
       k.NakshatraSubLordPlanet, k.IsRetrograde, k.IsCombust,
       k.DistanceFromSunDegrees, k.SignLordPlanet, k.DignityStatus,
       k.CharaKaraka, ageState.StateName AS AgeState,
       ps.AgeEffectFraction, wakeState.StateName AS WakefulnessState,
       postureState.StateName AS PostureState
FROM dbo.tbl_ChartResults c
JOIN dbo.tbl_Chart_KeyDetails k ON k.ChartResultId = c.Id
LEFT JOIN dbo.tbl_Fact_PlanetaryState ps ON ps.ChartResultId = c.Id AND ps.Planet = k.Planet
LEFT JOIN dbo.tbl_Dim_PlanetaryState ageState ON ageState.Id = ps.AgeStateId
LEFT JOIN dbo.tbl_Dim_PlanetaryState wakeState ON wakeState.Id = ps.WakefulnessStateId
LEFT JOIN dbo.tbl_Dim_PlanetaryState postureState ON postureState.Id = ps.PostureStateId
WHERE c.CalculationKind = 'PositionChart';
GO

INSERT dbo.SchemaMigrations (ScriptName, Note)
SELECT '083_create_sayanadi_avastha.sql',
       'Sayanaadi (PostureState) avastha: 12-state Dim seed, tbl_Rule_PostureStateFormula, tbl_Fact_PlanetaryState.PostureStateId, vw_ChartPlanetEvidence extended. SRC_PVR_INTEGRATED sec 15.4.4.'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '083_create_sayanadi_avastha.sql');
GO

PRINT '083 applied: Sayanaadi avastha schema created and seeded.';
GO
