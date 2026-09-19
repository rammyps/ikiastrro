-- =====================================================================
-- 127 - tbl_Rule_Argala: house-offset positions that cause argala
-- (intervention) and virodhargala (obstruction) on a target house or
-- planet's occupied sign. P.V.R. Narasimha Rao, Vedic Astrology: An
-- Integrated Approach, sec.10.5 "Argala (Intervention)" / sec.10.6
-- "Virodhargala (Obstruction)", pp.104-107 (SRC_PVR_INTEGRATED, verified
-- against the raw book extract, lines 4051-4129; Exercise 16's own worked
-- answer for Chart 5, pp.110-111, cross-checked the position table below).
--
--   - A planet/house in the 2nd, 4th or 11th from a house or planet causes
--     PRIMARY argala on it (subhaargala if benefic, paapaargala if malefic).
--   - The 5th causes SECONDARY argala.
--   - Virodhargala from the 12th, 10th, 3rd and 9th obstructs the argala
--     from the 2nd, 4th, 11th and 5th respectively (CountersHouseOffset).
--   - Exception (sec.10.6, stated immediately after Exercise 16): if
--     several (>=2, per Exercise 16's own house-11 answer) malefics occupy
--     the 3rd-from position, they cause ARGALA instead of virodhargala —
--     encoded here as ExceptionMinMaleficCount/ExceptionBecomesRelationTypeCode
--     on that one row rather than a hardcoded engine special-case.
--
-- NOT encoded as data (engine-level behavior, mirrors how tbl_Rule_RasiDrishti's
-- symmetric logic lives in RasiDrishtiCalculator.cs, not the grid): the
-- anti-zodiacal counting rule when the TARGET sign holds Ketu (sec.10.6 NOTE).
-- This table is offset-shaped like tbl_Rule_AspectOffset (graha drishti),
-- not a full 12x12 grid like tbl_Rule_RasiDrishti, because argala positions
-- are relative to whichever house/sign is the target, not planet-specific.
--
-- Grain: 4 argala rows + 4 virodhargala rows = 8 rows.
-- Apply:  sqlcmd -S "localhost\SQLSERVER2025" -E -d ikiastrro -b -i db/127_create_rule_argala.sql
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID('dbo.tbl_Rule_Argala', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Rule_Argala (
        Id                                INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Rule_Argala PRIMARY KEY,
        RuleSetId                         TINYINT      NOT NULL,
        RelationTypeCode                  VARCHAR(20)  NOT NULL,
        HouseOffset                       TINYINT      NOT NULL,
        OffsetLabel                       VARCHAR(10)  NOT NULL,
        IsPrimary                         BIT          NOT NULL,
        CountersHouseOffset               TINYINT      NULL,
        ExceptionMinMaleficCount          TINYINT      NULL,
        ExceptionBecomesRelationTypeCode  VARCHAR(20)  NULL,
        MethodCode                        VARCHAR(30)  NULL,
        RuleParametersJson                NVARCHAR(MAX) NULL,
        CalculationNarrative               NVARCHAR(MAX) NULL,
        SourceRefCode                     VARCHAR(40)  NULL,
        IsActive                          BIT          NOT NULL CONSTRAINT DF_Rule_Argala_IsActive DEFAULT 1,
        CONSTRAINT CK_Rule_Argala_RelType   CHECK (RelationTypeCode IN ('ARGALA','VIRODHARGALA')),
        CONSTRAINT CK_Rule_Argala_Offset    CHECK (HouseOffset BETWEEN 1 AND 12),
        CONSTRAINT CK_Rule_Argala_Except    CHECK (ExceptionBecomesRelationTypeCode IS NULL OR ExceptionBecomesRelationTypeCode IN ('ARGALA','VIRODHARGALA')),
        CONSTRAINT CK_Rule_Argala_Json      CHECK (RuleParametersJson IS NULL OR ISJSON(RuleParametersJson) = 1),
        CONSTRAINT CK_Rule_Argala_Src       CHECK (SourceRefCode IS NULL OR SourceRefCode LIKE 'SRC[_]%'),
        CONSTRAINT FK_Rule_Argala_RuleSet   FOREIGN KEY (RuleSetId) REFERENCES dbo.tbl_Rule_Sets (Id),
        CONSTRAINT UQ_Rule_Argala           UNIQUE (RuleSetId, RelationTypeCode, HouseOffset)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_Argala)
BEGIN
    INSERT dbo.tbl_Rule_Argala
        (RuleSetId, RelationTypeCode, HouseOffset, OffsetLabel, IsPrimary, CountersHouseOffset,
         ExceptionMinMaleficCount, ExceptionBecomesRelationTypeCode, MethodCode, SourceRefCode)
    VALUES
        (1, 'ARGALA',       2,  '2nd',  1, NULL, NULL, NULL,       'ARGALA_OFFSET', 'SRC_PVR_INTEGRATED'),
        (1, 'ARGALA',       4,  '4th',  1, NULL, NULL, NULL,       'ARGALA_OFFSET', 'SRC_PVR_INTEGRATED'),
        (1, 'ARGALA',       11, '11th', 1, NULL, NULL, NULL,       'ARGALA_OFFSET', 'SRC_PVR_INTEGRATED'),
        (1, 'ARGALA',       5,  '5th',  0, NULL, NULL, NULL,       'ARGALA_OFFSET', 'SRC_PVR_INTEGRATED'),
        (1, 'VIRODHARGALA', 12, '12th', 1, 2,    NULL, NULL,       'ARGALA_OFFSET', 'SRC_PVR_INTEGRATED'),
        (1, 'VIRODHARGALA', 10, '10th', 1, 4,    NULL, NULL,       'ARGALA_OFFSET', 'SRC_PVR_INTEGRATED'),
        (1, 'VIRODHARGALA', 3,  '3rd',  1, 11,   2,    'ARGALA',   'ARGALA_OFFSET', 'SRC_PVR_INTEGRATED'),
        (1, 'VIRODHARGALA', 9,  '9th',  0, 5,    NULL, NULL,       'ARGALA_OFFSET', 'SRC_PVR_INTEGRATED');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_Catalog WHERE RuleTableName = 'tbl_Rule_Argala')
    INSERT dbo.tbl_Rule_Catalog (RuleTableName, EngineCode, MethodCodes, Purpose, IntroducedIn)
    VALUES ('tbl_Rule_Argala', 'RELATIONSHIP', 'ARGALA_OFFSET',
            'Argala/virodhargala house-offset positions (PVR sec.10.5-10.6): 2nd/4th/11th primary + 5th secondary argala; 12th/10th/3rd/9th virodhargala counters each respectively; the 3rd-position 2+-malefic exception flips it to argala. Feeds the planned ArgalaCalculator.',
            '127_create_rule_argala.sql');
GO

INSERT dbo.SchemaMigrations (ScriptName, Note)
SELECT '127_create_rule_argala.sql',
       'tbl_Rule_Argala created + seeded (8 rows, RuleSetId 1); 1 tbl_Rule_Catalog row'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '127_create_rule_argala.sql');
GO

PRINT '127 applied: tbl_Rule_Argala created and seeded (8 rows).';
GO
