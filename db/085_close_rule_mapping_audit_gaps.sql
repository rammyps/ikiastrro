-- =====================================================================
-- 085 — Close three of the gaps found by the 2026-09-11 rule-mapping audit
--       (docs/database/rules-engine.md, "Formulas computed in C#/SQL with
--       no tbl_Rule_* citation at all"): Chara Karaka assignment,
--       Vimshottari Dasha's core table, and the Arudha Pada formula.
--       Schema + seed only; the CLI calculators stay hardoded per the
--       project's "verified mirror" pattern (see rules-engine.md's Live?
--       legend) — this migration gives each formula a citable row and a
--       CLI verify-* check something to cross-reference.
--
--   * Chara Karaka: tbl_Rule_Karaka has been reserved and empty since
--     migration 18; its schema (KarakaScheme/OrderIndex/ReverseForRahu)
--     was already shaped for exactly this rule. Seeded from
--     SRC_PVR_INTEGRATED sec.8.2, Table 13 (verified against the raw
--     extract): rank the 8 grahas by degree-in-sign descending, Rahu's
--     degree measured from the sign's END; highest -> AK ... lowest -> DK.
--   * Vimshottari Dasha: new tbl_Rule_VimshottariPeriod. The 9-planet
--     order + 120-year split were already hardcoded in
--     AstroMath.NakshatraLordOrder / VimshottariYearsByLord with no DB
--     citation at all (not even an "orphaned" table). Seeded from
--     SRC_PVR_INTEGRATED sec.16.2, Table 38 (verified against the raw
--     extract — years match exactly: Sun 6, Moon 10, Mars 7, Rahu 18,
--     Jupiter 16, Saturn 19, Mercury 17, Ketu 7, Venus 20, total 120).
--     SequenceOrder starts at Ashwini's lord (Ketu), matching
--     NakshatraLordOrder's declaration order.
--   * Arudha Pada: new tbl_Rule_ArudhaFormula, single-narrative shape
--     (same as migration 083's tbl_Rule_PostureStateFormula). Seeded from
--     SRC_PVR_INTEGRATED sec.9.2 (verified against the raw extract): count
--     signs from the house to its lord's sign, count the same number
--     onward from the lord's sign; if the result lands in the 1st or 7th
--     from the house, take the 10th sign from that result instead.
--
--   NOT addressed by this migration (deliberately, source-honesty --
--   see the audit's own writeup for why):
--   * Sade Sati / Kantaka / Ashtama's SourceRefCode — no registered
--     source's raw text extract actually discusses the 12th/1st/2nd-from-
--     Moon offset rule; migration 32's existing SRC_PVR_INTEGRATED
--     citation on a narrative field predates this project's
--     verify-against-raw-text discipline and was not independently
--     re-verified. Left open rather than repeat that citation.
--   * tbl_Rule_YogaValidationDefinition's missing migration script — a
--     reproducibility gap, not a mapping gap; out of scope here.
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- 1. Chara Karaka — seed the already-reserved tbl_Rule_Karaka.
IF OBJECT_ID('dbo.tbl_Rule_Karaka', 'U') IS NULL
    THROW 50085, 'Migration 085 requires dbo.tbl_Rule_Karaka (migration 18).', 1;

;WITH karakas (OrderIndex, TargetValue, Narrative) AS (
    SELECT * FROM (VALUES
        (1, 'AK',  'Atma Karaka — highest degree-in-sign advancement.'),
        (2, 'AmK', 'Amatya Karaka.'),
        (3, 'BK',  'Bhratri Karaka.'),
        (4, 'MK',  'Matri Karaka.'),
        (5, 'PiK', 'Pitri Karaka.'),
        (6, 'PK',  'Putra Karaka.'),
        (7, 'GK',  'Jnaati/Gnaati Karaka (GK/JK).'),
        (8, 'DK',  'Dara Karaka — lowest degree-in-sign advancement.')
    ) k (OrderIndex, TargetValue, Narrative)
)
INSERT dbo.tbl_Rule_Karaka
    (RuleSetId, KarakaScheme, PlanetOrHouse, TargetValue, OrderIndex, ReverseForRahu,
     MethodCode, RuleParametersJson, CalculationNarrative, SourceRefCode)
SELECT 1, 'Chara', NULL, k.TargetValue, k.OrderIndex, 1,
       'DEGREE_RANK_DESC',
       N'{"scope":"Sun,Moon,Mars,Mercury,Jupiter,Venus,Saturn,Rahu (Ketu not ranked)","rank":"advancement (degree) within own sign, descending","rahu":"measured from the END of Rahu''s sign, i.e. 30 - degreeInSign","tie":"exact-longitude ties share the karakatva; PVR then falls back to the sthira karaka (not implemented in CharaKarakaCalculator — never fires on real ephemeris data)","sourceLocator":"SRC_PVR_INTEGRATED sec.8.2, Table 13"}',
       k.Narrative,
       'SRC_PVR_INTEGRATED'
FROM karakas k
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.tbl_Rule_Karaka r
    WHERE r.RuleSetId = 1 AND r.KarakaScheme = 'Chara' AND r.OrderIndex = k.OrderIndex
);
GO

-- 2. Vimshottari Dasha — the core 9-planet/120-year table.
IF OBJECT_ID('dbo.tbl_Rule_VimshottariPeriod', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Rule_VimshottariPeriod (
        Id                   INT IDENTITY(1,1) CONSTRAINT PK_Rule_VimshottariPeriod PRIMARY KEY,
        RuleSetId            TINYINT NOT NULL CONSTRAINT FK_Rule_VimshottariPeriod_RuleSet REFERENCES dbo.tbl_Rule_Sets (Id),
        PlanetId             TINYINT NOT NULL CONSTRAINT FK_Rule_VimshottariPeriod_Planet REFERENCES dbo.tbl_Planets (Id),
        SequenceOrder        TINYINT NOT NULL,   -- 1-9, cyclic order starting at Ashwini's lord (Ketu)
        YearsInCycle         TINYINT NOT NULL,   -- classical dasha length; sums to 120 across the 9 rows
        SourceRefCode        VARCHAR(40) NOT NULL CONSTRAINT FK_Rule_VimshottariPeriod_Source REFERENCES dbo.tbl_Dim_Source (Code),
        SourceLocator        NVARCHAR(200) NULL,
        CalculationNarrative NVARCHAR(MAX) NULL,
        IsActive             BIT NOT NULL CONSTRAINT DF_Rule_VimshottariPeriod_IsActive DEFAULT (1),
        CONSTRAINT UQ_Rule_VimshottariPeriod_Order  UNIQUE (RuleSetId, SequenceOrder),
        CONSTRAINT UQ_Rule_VimshottariPeriod_Planet UNIQUE (RuleSetId, PlanetId),
        CONSTRAINT CK_Rule_VimshottariPeriod_Years  CHECK (YearsInCycle > 0 AND YearsInCycle <= 20)
    );
END
GO

;WITH cycle (SequenceOrder, PlanetName, YearsInCycle) AS (
    SELECT * FROM (VALUES
        (1, 'Ketu',    7),
        (2, 'Venus',   20),
        (3, 'Sun',     6),
        (4, 'Moon',    10),
        (5, 'Mars',    7),
        (6, 'Rahu',    18),
        (7, 'Jupiter', 16),
        (8, 'Saturn',  19),
        (9, 'Mercury', 17)
    ) c (SequenceOrder, PlanetName, YearsInCycle)
)
INSERT dbo.tbl_Rule_VimshottariPeriod
    (RuleSetId, PlanetId, SequenceOrder, YearsInCycle, SourceRefCode, SourceLocator, CalculationNarrative)
SELECT 1, p.Id, c.SequenceOrder, c.YearsInCycle, 'SRC_PVR_INTEGRATED',
       'sec.16.2, Table 38 ("Vimsottari Dasa Lengths")',
       N'Total 120-year cycle across the 9 planets. SequenceOrder 1 (Ketu) is the lord of ' +
       N'Ashwini, the first nakshatra — dasa order cycles from whichever lord the birth Moon''s ' +
       N'nakshatra starts at. The same YearsInCycle proportion drives the KP-2 sub-lord division ' +
       N'within one nakshatra (AstroMath.GetNakshatraSubLord).'
FROM cycle c
JOIN dbo.tbl_Planets p ON p.PlanetName = c.PlanetName
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.tbl_Rule_VimshottariPeriod r WHERE r.RuleSetId = 1 AND r.PlanetId = p.Id
);
GO

-- 3. Arudha Pada — single-narrative formula row (same shape as migration
--    083's tbl_Rule_PostureStateFormula).
IF OBJECT_ID('dbo.tbl_Rule_ArudhaFormula', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Rule_ArudhaFormula (
        Id                 INT IDENTITY(1,1) CONSTRAINT PK_Rule_ArudhaFormula PRIMARY KEY,
        RuleSetId          TINYINT NOT NULL CONSTRAINT FK_Rule_ArudhaFormula_RuleSet REFERENCES dbo.tbl_Rule_Sets (Id),
        FormulaNarrative   NVARCHAR(MAX) NOT NULL,
        SourceRefCode      VARCHAR(40) NOT NULL CONSTRAINT FK_Rule_ArudhaFormula_Source REFERENCES dbo.tbl_Dim_Source (Code),
        SourceLocator      NVARCHAR(200) NULL,
        IsActive           BIT NOT NULL CONSTRAINT DF_Rule_ArudhaFormula_IsActive DEFAULT (1),
        CONSTRAINT UQ_Rule_ArudhaFormula UNIQUE (RuleSetId)
    );
END
GO

INSERT dbo.tbl_Rule_ArudhaFormula (RuleSetId, FormulaNarrative, SourceRefCode, SourceLocator)
SELECT 1,
       N'Arudha pada of a house: (1) take the sign containing the house. (2) find the sign ' +
       N'occupied by that house''s lord (Aquarius/Scorpio: use the stronger of the two co-lords). ' +
       N'(3) count signs from the house''s sign to the lord''s sign (zodiacal direction, inclusive). ' +
       N'(4) count that same number of signs onward from the lord''s sign; this is the candidate ' +
       N'pada. (5) exception: if the candidate lands in the 1st or 7th sign from the house itself, ' +
       N'take the 10th sign from the candidate instead. A1 (arudha of the 1st house / lagna) is ' +
       N'denoted AL; A12 is denoted UL (upapada lagna). Computed once per house per divisional ' +
       N'chart, at the natal Lagna''s degree-in-sign for varga projection.',
       'SRC_PVR_INTEGRATED', 'sec.9.2 ("Computation of Bhava Arudhas")'
WHERE NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_ArudhaFormula WHERE RuleSetId = 1);
GO

-- 4. Register the two new tables in tbl_Rule_Catalog.
INSERT dbo.tbl_Rule_Catalog
    (RuleTableName, EngineCode, MethodCodes, Purpose, IntroducedIn)
SELECT v.RuleTableName, v.EngineCode, v.MethodCodes, v.Purpose, v.IntroducedIn
FROM (VALUES
    ('tbl_Rule_VimshottariPeriod', 'DASHA', 'FIXED_CYCLE_TABLE',
     'Vimshottari Dasha''s 9-planet order and 120-year split (also the KP-2 sub-lord division''s source).',
     '085 (rule-mapping audit)'),
    ('tbl_Rule_ArudhaFormula', 'KARAKA', 'HOUSE_COUNTING_ALGORITHM',
     'Arudha pada counting rule (house -> lord -> pada, with the 1st/7th -> 10th exception).',
     '085 (rule-mapping audit)')
) v (RuleTableName, EngineCode, MethodCodes, Purpose, IntroducedIn)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.tbl_Rule_Catalog c WHERE c.RuleTableName = v.RuleTableName
);
GO

INSERT dbo.SchemaMigrations (ScriptName, Note)
SELECT '085_close_rule_mapping_audit_gaps.sql',
       'Seed tbl_Rule_Karaka (Chara Karaka); add + seed tbl_Rule_VimshottariPeriod and tbl_Rule_ArudhaFormula; register both in tbl_Rule_Catalog.'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '085_close_rule_mapping_audit_gaps.sql');
GO

PRINT '085 applied: Chara Karaka / Vimshottari Dasha / Arudha Pada now have citable rule rows.';
GO
