-- =====================================================================
-- 150 - Special Lagna reference layer, part 2: Indu Lagna + Pranapada Lagna.
--
-- Follow-on to db/28 (Bhaava/Hora/Ghati/Sree). Two new calculation
-- families in tbl_Dim_SpecialLagnas:
--   KALA_SIGN_COUNT       - Indu Lagna (IL). B.V. Raman method: sum the
--                           Kala value of the 9th-lord-from-Lagna and the
--                           9th-lord-from-Moon, reduce mod 12 (0 -> 12),
--                           count that many signs forward from the Moon's
--                           sign, inclusive. tbl_Rule_SpecialLagnaKala
--                           holds the 7 planets' Kala values.
--   PRANA_MODALITY_OFFSET - Pranapada Lagna (PP). Reuses the same
--                           TIME_FROM_SUNRISE rate family as db/28's
--                           Bhaava/Hora/Ghati (5.0 deg/min - the classical
--                           "vighati" rate, algebraically identical to
--                           this project's own Vighati Lagna), then adds a
--                           fixed offset for the modality (movable/fixed/
--                           dual) of the sign the Sun occupies at sunrise.
--                           tbl_Rule_SpecialLagnaModality holds the 3
--                           offsets.
--
-- Both built in C#: InduLagnaCalculator.cs, PranapadaLagnaCalculator.cs,
-- wired into SpecialPointCalculator.ComputeSeeds. Indu Lagna verified
-- exactly against docs/artifacts/reference-charts/Rammy_Jagannatha.txt
-- (7 Sg 17'33.70"). Pranapada Lagna is verified to within ~0.85 deg of
-- that same chart's printed value (24 Sc 54'52.36") - a documented
-- residual, not a formula error; see PranapadaLagnaCalculator.cs's doc
-- comment. Vighati Lagna itself is deliberately NOT built as its own
-- calculator - no genuine classical life-matter linkage was found for it.
--
-- Kala values and the modality-offset rule are cross-checked against the
-- vendored PyJHora reference (_research/PyJHora/src/jhora/panchanga/
-- drik.py: indu_lagna's il_factors, pranapada_lagna's movable/fixed/dual
-- branch) - SourceRefCode SRC_PYJHORA on every seeded row here, matching
-- how db/36 already cites PyJHora for the ayanamsa rule table.
--
-- Idempotent: table / catalog adds guarded; seeds are IF NOT EXISTS on
-- their table.
-- Apply:  sqlcmd -S localhost -E -d ikiastrro -b -i db/150_add_indu_pranapada_lagna_rules.sql
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

-- --- Batch 1: widen tbl_Dim_SpecialLagnas.CalculationType (column + CHECK) ---
-- NOTE: db/111 already widened the CHECK once to add ARITHMETIC_DAYNIGHT (Punya Saham, Id 5) -
-- that value must be preserved here, not just db/28's original two. 'PRANA_MODALITY_OFFSET'
-- (21 chars) also needs the column itself widened past db/28's original VARCHAR(20).
IF COL_LENGTH('dbo.tbl_Dim_SpecialLagnas', 'CalculationType') < 24
    ALTER TABLE dbo.tbl_Dim_SpecialLagnas ALTER COLUMN CalculationType VARCHAR(24) NOT NULL;
GO
IF EXISTS (SELECT 1 FROM sys.check_constraints
           WHERE name = 'CK_Dim_SpecialLagnas_CalcType'
             AND OBJECT_DEFINITION(object_id) NOT LIKE '%KALA_SIGN_COUNT%')
BEGIN
    ALTER TABLE dbo.tbl_Dim_SpecialLagnas DROP CONSTRAINT CK_Dim_SpecialLagnas_CalcType;
    ALTER TABLE dbo.tbl_Dim_SpecialLagnas WITH CHECK ADD CONSTRAINT CK_Dim_SpecialLagnas_CalcType
        CHECK (CalculationType IN ('TIME_FROM_SUNRISE','NAKSHATRA_FRACTION','ARITHMETIC_DAYNIGHT','KALA_SIGN_COUNT','PRANA_MODALITY_OFFSET'));
END
GO

-- --- Batch 2: 2 new tbl_Dim_SpecialLagnas rows ---
-- Id 5 is taken by PUNYA_SAHAM (db/111) - Indu/Pranapada take 6 and 7.
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Dim_SpecialLagnas WHERE Id IN (6, 7))
    INSERT dbo.tbl_Dim_SpecialLagnas
        (Id, LagnaCode, Abbreviation, LagnaName, CalculationType, UsedInBook, Significations, SortOrder, Notes)
    VALUES
        (6, 'INDU_LAGNA', 'IL', 'Indu Lagna', 'KALA_SIGN_COUNT', 0,
            N'Wealth-yielding capacity. An own-sign or exalted planet placed in Indu Lagna, or in the 2nd/11th house counted from it, forms Koteeswara Yoga.', 6,
            N'Not covered by PVR''s Integrated Approach ch.5 - UsedInBook=0 here means "not in this book", unlike Bhaava Lagna''s "book calls it unused". B.V. Raman method (InduLagnaCalculator.cs); verified exactly against docs/artifacts/reference-charts/Rammy_Jagannatha.txt.'),
        (7, 'PRANAPADA_LAGNA', 'PP', 'Pranapada Lagna', 'PRANA_MODALITY_OFFSET', 0,
            N'Vitality and life-force; house placement from the Lagna read for health and longevity.', 7,
            N'Not covered by PVR''s Integrated Approach ch.5. Reuses the Vighati-Lagna time-rate base (5 deg/min) plus a movable/fixed/dual modality offset (PranapadaLagnaCalculator.cs); verified to within a documented ~0.85 deg residual against docs/artifacts/reference-charts/Rammy_Jagannatha.txt.');
GO

-- --- Batch 3: tbl_Rule_SpecialLagnaKala (Indu Lagna's 7 planet Kala values) ---
IF OBJECT_ID('dbo.tbl_Rule_SpecialLagnaKala', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Rule_SpecialLagnaKala (
        Id             INT IDENTITY(1,1) NOT NULL
                           CONSTRAINT PK_Rule_SpecialLagnaKala PRIMARY KEY,
        RuleSetId      TINYINT     NOT NULL
                           CONSTRAINT FK_Rule_SpecialLagnaKala_RuleSet FOREIGN KEY REFERENCES dbo.tbl_Rule_Sets (Id),
        SpecialLagnaId TINYINT     NOT NULL
                           CONSTRAINT FK_Rule_SpecialLagnaKala_Lagna FOREIGN KEY REFERENCES dbo.tbl_Dim_SpecialLagnas (Id),
        PlanetId       TINYINT     NOT NULL
                           CONSTRAINT FK_Rule_SpecialLagnaKala_Planet FOREIGN KEY REFERENCES dbo.tbl_Planets (Id),
        KalaValue      TINYINT     NOT NULL,        -- Sun 30, Moon 16, Mars 6, Mercury 8, Jupiter 10, Venus 12, Saturn 1
        CalculationNarrative NVARCHAR(MAX) NULL,
        SourceRefCode  VARCHAR(40) NULL,
        IsActive       BIT NOT NULL CONSTRAINT DF_Rule_SpecialLagnaKala_IsActive DEFAULT 1,
        CONSTRAINT CK_Rule_SpecialLagnaKala_Value CHECK (KalaValue > 0),
        CONSTRAINT CK_Rule_SpecialLagnaKala_Src   CHECK (SourceRefCode IS NULL OR SourceRefCode LIKE 'SRC[_]%'),
        CONSTRAINT UQ_Rule_SpecialLagnaKala UNIQUE (RuleSetId, SpecialLagnaId, PlanetId)
    );
END
GO
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_SpecialLagnaKala)
    INSERT dbo.tbl_Rule_SpecialLagnaKala (RuleSetId, SpecialLagnaId, PlanetId, KalaValue, CalculationNarrative, SourceRefCode)
    SELECT 1, l.Id, p.Id, v.KalaValue,
           N'Indu Lagna (IL) Kala/rayi value for ' + v.PlanetName + N': sum the Kala of the lord of the 9th-from-Lagna and the lord of the 9th-from-Moon, reduce mod 12 (0 -> 12), count that many signs forward from the Moon''s sign, inclusive. No dignity modifier in the standard method.',
           'SRC_PYJHORA'
    FROM (VALUES ('Sun',30),('Moon',16),('Mars',6),('Mercury',8),('Jupiter',10),('Venus',12),('Saturn',1))
        v (PlanetName, KalaValue)
    JOIN dbo.tbl_Planets p ON p.PlanetName = v.PlanetName
    CROSS JOIN dbo.tbl_Dim_SpecialLagnas l
    WHERE l.LagnaCode = 'INDU_LAGNA';
GO

-- --- Batch 4: tbl_Rule_SpecialLagnaModality (Pranapada Lagna's 3 modality offsets) ---
IF OBJECT_ID('dbo.tbl_Rule_SpecialLagnaModality', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Rule_SpecialLagnaModality (
        Id             INT IDENTITY(1,1) NOT NULL
                           CONSTRAINT PK_Rule_SpecialLagnaModality PRIMARY KEY,
        RuleSetId      TINYINT      NOT NULL
                           CONSTRAINT FK_Rule_SpecialLagnaModality_RuleSet FOREIGN KEY REFERENCES dbo.tbl_Rule_Sets (Id),
        SpecialLagnaId TINYINT      NOT NULL
                           CONSTRAINT FK_Rule_SpecialLagnaModality_Lagna FOREIGN KEY REFERENCES dbo.tbl_Dim_SpecialLagnas (Id),
        ModalityCode   VARCHAR(10)  NOT NULL,        -- Movable | Fixed | Dual
        DegreesOffset  DECIMAL(5,1) NOT NULL,        -- Movable 0, Dual 120, Fixed 240
        CalculationNarrative NVARCHAR(MAX) NULL,
        SourceRefCode  VARCHAR(40)  NULL,
        IsActive       BIT NOT NULL CONSTRAINT DF_Rule_SpecialLagnaModality_IsActive DEFAULT 1,
        CONSTRAINT CK_Rule_SpecialLagnaModality_Code CHECK (ModalityCode IN ('Movable','Fixed','Dual')),
        CONSTRAINT CK_Rule_SpecialLagnaModality_Src  CHECK (SourceRefCode IS NULL OR SourceRefCode LIKE 'SRC[_]%'),
        CONSTRAINT UQ_Rule_SpecialLagnaModality UNIQUE (RuleSetId, SpecialLagnaId, ModalityCode)
    );
END
GO
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_SpecialLagnaModality)
    INSERT dbo.tbl_Rule_SpecialLagnaModality (RuleSetId, SpecialLagnaId, ModalityCode, DegreesOffset, CalculationNarrative, SourceRefCode)
    SELECT 1, l.Id, v.ModalityCode, v.DegreesOffset,
           N'Pranapada Lagna (PP) modality offset when the Sun at sunrise occupies a ' + v.ModalityCode + N' sign: add ' + CAST(v.DegreesOffset AS VARCHAR(10)) + N' deg to the time-rate base value (movable = Sun''s own longitude, dual = 5th from Sun, fixed = 9th from Sun).',
           'SRC_PYJHORA'
    FROM (VALUES ('Movable',0.0),('Dual',120.0),('Fixed',240.0)) v (ModalityCode, DegreesOffset)
    CROSS JOIN dbo.tbl_Dim_SpecialLagnas l
    WHERE l.LagnaCode = 'PRANAPADA_LAGNA';
GO

-- --- Batch 5: tbl_Rule_Catalog rows ---
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_Catalog WHERE RuleTableName = 'tbl_Rule_SpecialLagnaKala')
    INSERT dbo.tbl_Rule_Catalog (RuleTableName, EngineCode, MethodCodes, Purpose, IntroducedIn)
    VALUES ('tbl_Rule_SpecialLagnaKala', 'SPECIALLAGNA', 'KALA_SIGN_COUNT',
            'Indu Lagna: Kala/rayi value per planet (Sun 30 .. Saturn 1), used to reduce the summed Kala of the 9th-lord-from-Lagna and 9th-lord-from-Moon mod 12 and count that many signs from the Moon.',
            '150_add_indu_pranapada_lagna_rules.sql');
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_Catalog WHERE RuleTableName = 'tbl_Rule_SpecialLagnaModality')
    INSERT dbo.tbl_Rule_Catalog (RuleTableName, EngineCode, MethodCodes, Purpose, IntroducedIn)
    VALUES ('tbl_Rule_SpecialLagnaModality', 'SPECIALLAGNA', 'PRANA_MODALITY_OFFSET',
            'Pranapada Lagna: fixed degree offset (0/120/240) added to the time-rate base value depending on the modality (movable/fixed/dual) of the sign the Sun occupies at sunrise.',
            '150_add_indu_pranapada_lagna_rules.sql');
GO

-- --- Batch 6: ledger + summary ---
INSERT dbo.SchemaMigrations (ScriptName, Note)
SELECT '150_add_indu_pranapada_lagna_rules.sql',
       'tbl_Dim_SpecialLagnas +2 (Indu/Pranapada) + 2 new CalculationType values; tbl_Rule_SpecialLagnaKala(7) + tbl_Rule_SpecialLagnaModality(3); 2 catalog rows.'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '150_add_indu_pranapada_lagna_rules.sql');
GO

DECLARE @master INT = (SELECT COUNT(*) FROM dbo.tbl_Dim_SpecialLagnas WHERE Id IN (6,7));
DECLARE @kala   INT = (SELECT COUNT(*) FROM dbo.tbl_Rule_SpecialLagnaKala);
DECLARE @modal  INT = (SELECT COUNT(*) FROM dbo.tbl_Rule_SpecialLagnaModality);
DECLARE @cat    INT = (SELECT COUNT(*) FROM dbo.tbl_Rule_Catalog WHERE RuleTableName IN ('tbl_Rule_SpecialLagnaKala','tbl_Rule_SpecialLagnaModality'));
PRINT '150 applied: ' + CAST(@master AS VARCHAR(10)) + ' new special lagnas (expect 2), '
    + CAST(@kala AS VARCHAR(10)) + ' Kala rows (expect 7), '
    + CAST(@modal AS VARCHAR(10)) + ' modality rows (expect 3), '
    + CAST(@cat AS VARCHAR(10)) + ' catalog rows (expect 2).';
GO
