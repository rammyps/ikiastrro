-- =====================================================================
-- 072 — Complete the Kala Bala rule layer and add the planetary-war
--       (Yuddha Bala) reference. Schema + rule data only; the calculators
--       that read these rows are a separate CLI change.
--
--   * Corrects the Varsha (Abda) Bala cap to the classical 15 virupas.
--   * Sets RuleParametersJson (method constants + source locator) on the
--     six Kala Bala sub-components the engine does not yet compute:
--     Tribhaga, Varsha, Masa, Dina, Hora, Ayana.
--   * Adds tbl_Rule_PlanetaryWar (orb + winner criterion + adjustment
--     method). tbl_Fact_PlanetaryStrength.YuddhaBalaVirupas already exists
--     (migration 39, default 0).
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- 1. Correct the Varsha (Abda) Bala ceiling: 15 virupas = 0.25 rupas.
UPDATE dbo.tbl_Rule_ShadbalaComponent
   SET MaxRupas = 0.250
 WHERE RuleSetId = 1
   AND StrengthProfileCode = 'PVR_INTEGRATED_STRENGTH'
   AND BalaCode = 'KALA_BALA'
   AND SubComponentCode = 'VARSHA_BALA'
   AND MaxRupas <> 0.250;
GO

-- 2. Method parameters for the six uncomputed Kala Bala sub-components.
--    Coefficients marked "transcribe" must be filled from the cited Raman
--    edition when the calculator is implemented.
;WITH params (SubComponentCode, Json) AS (
    SELECT * FROM (VALUES
        ('TRIBHAGA_BALA',
         N'{"maxVirupas":60,"parts":"day split into 3 parts ruled by Mercury/Sun/Saturn; night by Moon/Venus/Mars","jupiterAlwaysGets":60,"sourceLocator":"Raman, Graha & Bhava Balas - Kaala Bala, Tribhaga Bala"}'),
        ('VARSHA_BALA',
         N'{"maxVirupas":15,"yearLordFrom":"weekday lord of the day beginning the solar year (Ahargana / 360)","sourceLocator":"Raman, Graha & Bhava Balas - Kaala Bala, Abda (Varsha) Bala"}'),
        ('MASA_BALA',
         N'{"maxVirupas":30,"monthLordFrom":"weekday lord of the day beginning the solar month","sourceLocator":"Raman, Graha & Bhava Balas - Kaala Bala, Masa Bala"}'),
        ('DINA_BALA',
         N'{"maxVirupas":45,"lord":"weekday lord of birth reckoned sunrise-to-sunrise","sourceLocator":"Raman, Graha & Bhava Balas - Kaala Bala, Vara (Dina) Bala"}'),
        ('HORA_BALA',
         N'{"maxVirupas":60,"horaSequence":"Chaldean order from the weekday lord at sunrise, one hora per planetary hour","sourceLocator":"Raman, Graha & Bhava Balas - Kaala Bala, Hora Bala"}'),
        ('AYANA_BALA',
         N'{"maxVirupas":60,"method":"declination (kranti) based; strength proportional to (max_kranti +/- planet_kranti); result doubled; Sun''s Ayana Bala tripled","note":"transcribe exact coefficients from the cited edition","sourceLocator":"Raman, Graha & Bhava Balas - Kaala Bala, Ayana Bala"}')
    ) p (SubComponentCode, Json)
)
UPDATE r
   SET r.RuleParametersJson = p.Json
FROM dbo.tbl_Rule_ShadbalaComponent r
JOIN params p ON p.SubComponentCode = r.SubComponentCode
WHERE r.RuleSetId = 1
  AND r.StrengthProfileCode = 'PVR_INTEGRATED_STRENGTH'
  AND r.BalaCode = 'KALA_BALA'
  AND (r.RuleParametersJson IS NULL OR ISJSON(r.RuleParametersJson) = 0);
GO

-- 3. Planetary-war (Graha Yuddha) reference.
IF OBJECT_ID('dbo.tbl_Rule_PlanetaryWar', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Rule_PlanetaryWar (
        Id                   INT IDENTITY(1,1) CONSTRAINT PK_Rule_PlanetaryWar PRIMARY KEY,
        RuleSetId            TINYINT NOT NULL CONSTRAINT FK_Rule_PlanetaryWar_RuleSet REFERENCES dbo.tbl_Rule_Sets (Id),
        StrengthProfileCode  VARCHAR(40) NOT NULL,
        OrbDegrees           DECIMAL(4,2) NOT NULL CONSTRAINT DF_Rule_PlanetaryWar_Orb DEFAULT (1.00),
        ParticipatingPlanetsCsv VARCHAR(80) NOT NULL,
        WinnerCriterionCode  VARCHAR(40) NOT NULL,
        AdjustmentMethodCode VARCHAR(60) NOT NULL,
        RuleParametersJson   NVARCHAR(MAX) NULL,
        SourceRefCode        VARCHAR(40) NULL,
        FormulaSourceRefCode VARCHAR(40) NULL,
        CalculationNarrative NVARCHAR(MAX) NULL,
        IsActive             BIT NOT NULL CONSTRAINT DF_Rule_PlanetaryWar_IsActive DEFAULT (1),
        CONSTRAINT CK_Rule_PlanetaryWar_Json        CHECK (RuleParametersJson IS NULL OR ISJSON(RuleParametersJson) = 1),
        CONSTRAINT CK_Rule_PlanetaryWar_Src         CHECK (SourceRefCode IS NULL OR SourceRefCode LIKE 'SRC[_]%'),
        CONSTRAINT CK_Rule_PlanetaryWar_FormulaSrc  CHECK (FormulaSourceRefCode IS NULL OR FormulaSourceRefCode LIKE 'SRC[_]%'),
        CONSTRAINT CK_Rule_PlanetaryWar_Orb         CHECK (OrbDegrees > 0 AND OrbDegrees <= 5),
        CONSTRAINT UQ_Rule_PlanetaryWar UNIQUE (RuleSetId, StrengthProfileCode)
    );
END
GO

INSERT dbo.tbl_Rule_PlanetaryWar
    (RuleSetId, StrengthProfileCode, OrbDegrees, ParticipatingPlanetsCsv,
     WinnerCriterionCode, AdjustmentMethodCode, RuleParametersJson,
     SourceRefCode, FormulaSourceRefCode, CalculationNarrative)
SELECT 1, 'PVR_INTEGRATED_STRENGTH', 1.00, 'Mars,Mercury,Jupiter,Venus,Saturn',
       'NORTHERN_OR_LARGER_DIAMETER', 'SHADBALA_DIFFERENCE_DIV_DIAMETER_SUM',
       N'{"orbDegrees":1.0,"winner":"planet further north in latitude, or with the larger apparent diameter","adjustment":"delta = |ShadbalaVirupas(a) - ShadbalaVirupas(b)| / (diameter(a) + diameter(b)); winner + delta, loser - delta","excludes":["Sun","Moon","Rahu","Ketu"],"sourceLocator":"Raman, Graha & Bhava Balas - Yuddha Bala"}',
       'SRC_PVR_INTEGRATED', 'SRC_RAMAN_GRAHA_BHAVA_BALAS',
       N'Graha Yuddha among the five tara grahas when within 1 degree of longitude. The winner gains, and the loser loses, the Shadbala (virupa) difference divided by the sum of the two apparent diameters. Applied as YuddhaBalaVirupas on tbl_Fact_PlanetaryStrength.'
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.tbl_Rule_PlanetaryWar
    WHERE RuleSetId = 1 AND StrengthProfileCode = 'PVR_INTEGRATED_STRENGTH'
);
GO

INSERT dbo.SchemaMigrations (ScriptName, Note)
SELECT '072_shadbala_kalabala_params_and_planetary_war.sql',
       'Correct Varsha Bala cap to 15 virupas; set RuleParametersJson on the six uncomputed Kala Bala sub-components; add tbl_Rule_PlanetaryWar (Yuddha Bala reference).'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '072_shadbala_kalabala_params_and_planetary_war.sql');
GO

PRINT '072 applied: Kala Bala params + planetary-war rule ready.';
GO
