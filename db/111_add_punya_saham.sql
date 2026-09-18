-- =====================================================================
-- 111 - Punya Saham (PS): 5th Special Lagna, documentation-mirror layer.
--
-- PVR "Vedic Astrology: An Integrated Approach" ch. 28.8, Table 74, Part 4
-- (Tajaka Analysis - not built at all before this migration). Punya Saham
-- ("Fortune/good deeds") is the row PVR himself draws the western "Part
-- of Fortune"/Pars Fortuna parallel to (same section) - it is a Saham
-- (Arabic-Parts-style technique), not a KP (Krishnamurti Paddhati) point.
--
-- Formula: Moon - Sun + Lagna for day births, Sun - Moon + Lagna for
-- night births (PVR sec 28.8.1's general A-B+C / day-night reversal
-- rule). That same section's general correction also applies: if the
-- Lagna (C) does not lie on the zodiacal arc going forward from B to A,
-- add 30 deg. See PunyaSahamCalculator.cs for the arc-check logic,
-- verified against PVR's own two worked saham examples (vanik/samartha).
--
-- Same as migrations 28/29 (the other 4 special lagnas): the Special
-- Lagna calculators are 100% hardcoded C#
-- (SpecialPointCalculator.ComputeSeeds), dispatched by explicit call, not
-- data-driven off these tables. This migration's DB layer is
-- documentation/reference-mirror only, same "mirror" status the other 4
-- lagnas already carry (rules-engine.md) - nothing reads it at runtime.
-- PunyaSahamCalculator.cs + the SpecialPointCalculator.ComputeSeeds wire-
-- up are the actual functional change and ship as C#, not this migration.
--
--   tbl_Dim_SpecialLagnas   += Id 5, PUNYA_SAHAM/PS, CalculationType
--                              widened to add ARITHMETIC_DAYNIGHT.
--   tbl_Rule_SpecialLagnaSaham - new sibling to TimeRate/Fraction: the
--                              A/B planet pair, the C reference point
--                              (natal Lagna), the day/night reversal
--                              flag, and the arc-correction degrees.
--
-- Apply:  sqlcmd -S localhost\SQLSERVER2025 -E -d ikiastrro -b -i db/111_add_punya_saham.sql
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- --- Batch 1: widen tbl_Dim_SpecialLagnas.CalculationType + add PS row ---
IF OBJECT_ID('dbo.CK_Dim_SpecialLagnas_CalcType', 'C') IS NOT NULL
    ALTER TABLE dbo.tbl_Dim_SpecialLagnas DROP CONSTRAINT CK_Dim_SpecialLagnas_CalcType;
ALTER TABLE dbo.tbl_Dim_SpecialLagnas WITH CHECK
    ADD CONSTRAINT CK_Dim_SpecialLagnas_CalcType
    CHECK (CalculationType IN ('TIME_FROM_SUNRISE','NAKSHATRA_FRACTION','ARITHMETIC_DAYNIGHT'));
GO

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Dim_SpecialLagnas WHERE LagnaCode = 'PUNYA_SAHAM')
    INSERT dbo.tbl_Dim_SpecialLagnas
        (Id, LagnaCode, Abbreviation, LagnaName, CalculationType, UsedInBook, Significations, SortOrder, Notes)
    VALUES
        (5, 'PUNYA_SAHAM', 'PS', 'Punya Saham', 'ARITHMETIC_DAYNIGHT', 1,
            N'Fortune / good deeds; PVR''s own parallel to the western "Part of Fortune" (Pars Fortuna) — ch. 28.8, Table 74, row 1. A Tajaka Saham, not a KP technique.', 5,
            N'Formula Moon-Sun+Lagna (day) / Sun-Moon+Lagna (night), PVR sec 28.8.1''s general A-B+C rule incl. the +30 deg arc correction. Built in PunyaSahamCalculator.cs.');
GO

-- --- Batch 2: tbl_Rule_SpecialLagnaSaham (Punya Saham only) ---
IF OBJECT_ID('dbo.tbl_Rule_SpecialLagnaSaham', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Rule_SpecialLagnaSaham (
        Id                    INT IDENTITY(1,1) NOT NULL
                                  CONSTRAINT PK_Rule_SpecialLagnaSaham PRIMARY KEY,
        RuleSetId             TINYINT      NOT NULL
                                  CONSTRAINT FK_Rule_SpecialLagnaSaham_RuleSet FOREIGN KEY REFERENCES dbo.tbl_Rule_Sets (Id),
        SpecialLagnaId        TINYINT      NOT NULL
                                  CONSTRAINT FK_Rule_SpecialLagnaSaham_Lagna FOREIGN KEY REFERENCES dbo.tbl_Dim_SpecialLagnas (Id),
        FormulaAPlanetId      TINYINT      NOT NULL           -- day-formula "A" (Moon for Punya Saham)
                                  CONSTRAINT FK_Rule_SpecialLagnaSaham_A FOREIGN KEY REFERENCES dbo.tbl_Planets (Id),
        FormulaBPlanetId      TINYINT      NOT NULL           -- day-formula "B" (Sun for Punya Saham)
                                  CONSTRAINT FK_Rule_SpecialLagnaSaham_B FOREIGN KEY REFERENCES dbo.tbl_Planets (Id),
        FormulaCReferenceCode VARCHAR(20)  NOT NULL,          -- NATAL_LAGNA
        ReversesForNightBirth BIT          NOT NULL CONSTRAINT DF_Rule_SpecialLagnaSaham_Reversal DEFAULT 1,  -- PVR sec 28.8.1 general rule
        ArcCorrectionDegrees  DECIMAL(9,6) NOT NULL CONSTRAINT DF_Rule_SpecialLagnaSaham_Arc DEFAULT 30,      -- +30 if C not on the B->A arc
        NormalizationMethod   VARCHAR(12)  NOT NULL CONSTRAINT DF_Rule_SpecialLagnaSaham_Norm DEFAULT 'MOD_360',
        MethodCode            VARCHAR(30)  NULL,              -- 'SAHAM_ARITHMETIC'
        RuleParametersJson    NVARCHAR(MAX) NULL,
        CalculationNarrative  NVARCHAR(MAX) NULL,
        SourceRefCode         VARCHAR(40)  NULL,
        IsActive              BIT          NOT NULL CONSTRAINT DF_Rule_SpecialLagnaSaham_IsActive DEFAULT 1,
        CONSTRAINT CK_Rule_SpecialLagnaSaham_CRef CHECK (FormulaCReferenceCode IN ('NATAL_LAGNA')),
        CONSTRAINT CK_Rule_SpecialLagnaSaham_Arc  CHECK (ArcCorrectionDegrees > 0),
        CONSTRAINT CK_Rule_SpecialLagnaSaham_Json CHECK (RuleParametersJson IS NULL OR ISJSON(RuleParametersJson) = 1),
        CONSTRAINT CK_Rule_SpecialLagnaSaham_Src  CHECK (SourceRefCode IS NULL OR SourceRefCode LIKE 'SRC[_]%'),
        CONSTRAINT UQ_Rule_SpecialLagnaSaham UNIQUE (RuleSetId, SpecialLagnaId)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_SpecialLagnaSaham)
    INSERT dbo.tbl_Rule_SpecialLagnaSaham
        (RuleSetId, SpecialLagnaId, FormulaAPlanetId, FormulaBPlanetId, FormulaCReferenceCode,
         ReversesForNightBirth, ArcCorrectionDegrees, NormalizationMethod, MethodCode, CalculationNarrative, SourceRefCode)
    SELECT 1, l.Id, moon.Id, sun.Id, 'NATAL_LAGNA', 1, CONVERT(DECIMAL(9,6), 30.000000), 'MOD_360', 'SAHAM_ARITHMETIC',
        N'Punya Saham (PS), PVR sec 28.8 Table 74 row 1 + sec 28.8.1''s general method: day formula Moon - Sun + Lagna; night formula reverses to Sun - Moon + Lagna. If Lagna (C) does not lie on the zodiacal arc going forward from B to A, add 30 deg. Verified against PVR''s own vanik saham (arc found, no +30) and samartha saham (arc not found, +30) worked examples, sec 28.8.1.',
        'SRC_PVR_INTEGRATED'
    FROM dbo.tbl_Dim_SpecialLagnas l
    JOIN dbo.tbl_Planets moon ON moon.PlanetName = 'Moon'
    JOIN dbo.tbl_Planets sun ON sun.PlanetName = 'Sun'
    WHERE l.LagnaCode = 'PUNYA_SAHAM';
GO

-- --- Batch 3: tbl_Rule_Catalog registration ---
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_Catalog WHERE RuleTableName = 'tbl_Rule_SpecialLagnaSaham')
    INSERT dbo.tbl_Rule_Catalog (RuleTableName, EngineCode, MethodCodes, Purpose, IntroducedIn)
    VALUES ('tbl_Rule_SpecialLagnaSaham', 'SPECIALLAGNA', 'SAHAM_ARITHMETIC',
            'Punya Saham: A-B+C Tajaka Saham arithmetic with day/night reversal and the +30 deg arc correction. Reference data - built in C# (PunyaSahamCalculator.cs), not read live.',
            '111_add_punya_saham.sql');
GO

-- --- Batch 4: ledger + summary ---
INSERT dbo.SchemaMigrations (ScriptName, Note)
SELECT '111_add_punya_saham.sql',
       'tbl_Dim_SpecialLagnas += PUNYA_SAHAM (5th row); tbl_Rule_SpecialLagnaSaham (1 row); 1 catalog row. PVR ch 28.8. C#: PunyaSahamCalculator.cs.'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '111_add_punya_saham.sql');
GO

DECLARE @master INT = (SELECT COUNT(*) FROM dbo.tbl_Dim_SpecialLagnas);
DECLARE @saham  INT = (SELECT COUNT(*) FROM dbo.tbl_Rule_SpecialLagnaSaham);
PRINT '111 applied: ' + CAST(@master AS VARCHAR(10)) + ' special-lagna master rows (expect 5), '
    + CAST(@saham AS VARCHAR(10)) + ' saham rule rows (expect 1).';
GO
