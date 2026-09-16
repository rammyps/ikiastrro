-- =====================================================================
-- 107 - tbl_Rule_RasiDrishti: sign-to-sign (rasi) aspects, keyed off
-- tbl_SignAttributes exactly the way tbl_Rule_NaturalRelationship keys off
-- tbl_Planets (RasiId/RelatedRasiId mirrors that table's PlanetId/RelatedPlanetId
-- shape). P.V.R. Narasimha Rao, Vedic Astrology: An Integrated Approach, sec.10.3
-- "Rasi Drishti" (SRC_PVR_INTEGRATED, verified against the raw book extract):
--
--   - A movable rasi aspects all fixed rasis except the one adjacent (next) to it.
--   - A fixed rasi aspects all movable rasis except the one adjacent (previous) to it.
--   - A dual rasi aspects all other dual rasis.
--   - Symmetric: rasi Y aspects rasi X whenever X aspects Y.
--
-- Distinct from tbl_Rule_AspectOffset (graha drishti: planet -> house offset).
-- This is sign -> sign, driven off tbl_SignAttributes.type_house_keyattri
-- (Chara/Sthira/Dwiswabhava already carries the movable/fixed/dual split -
-- no new classification column needed). Both directions of each pair are
-- seeded as separate rows so a query never needs to OR two conditions.
--
-- First consumer: GrahaArudhaCalculator's Stronger Rasi comparator (PVR sec.
-- 15.5.2, rule 2 - "how many of {Jupiter, Mercury, the rasi's lord}
-- occupy/aspect a rasi", which needs rasi drishti, not graha drishti) - the
-- comparator picks which of a planet's 2 owned signs is stronger for
-- computing that planet's graha arudha.
--
-- Grain: 12 rasis x 3 related rasis each = 36 rows.
-- Apply:  sqlcmd -S "localhost\SQLSERVER2025" -E -d ikiastrro -b -i db/107_create_rule_rasi_drishti.sql
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID('dbo.tbl_Rule_RasiDrishti', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Rule_RasiDrishti (
        Id             INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Rule_RasiDrishti PRIMARY KEY,
        RuleSetId      TINYINT      NOT NULL,
        RasiId         TINYINT      NOT NULL CONSTRAINT FK_Rule_RasiDrishti_Rasi        FOREIGN KEY REFERENCES dbo.tbl_SignAttributes (Id),
        RelatedRasiId  TINYINT      NOT NULL CONSTRAINT FK_Rule_RasiDrishti_RelatedRasi FOREIGN KEY REFERENCES dbo.tbl_SignAttributes (Id),
        MethodCode     VARCHAR(30)  NULL,
        SourceRefCode  VARCHAR(40)  NULL,
        IsActive       BIT          NOT NULL CONSTRAINT DF_Rule_RasiDrishti_IsActive DEFAULT 1,
        CONSTRAINT CK_Rule_RasiDrishti_NotSelf CHECK (RasiId <> RelatedRasiId),
        CONSTRAINT CK_Rule_RasiDrishti_Src     CHECK (SourceRefCode IS NULL OR SourceRefCode LIKE 'SRC[_]%'),
        CONSTRAINT FK_Rule_RasiDrishti_RuleSet FOREIGN KEY (RuleSetId) REFERENCES dbo.tbl_Rule_Sets (Id),
        CONSTRAINT UQ_Rule_RasiDrishti UNIQUE (RuleSetId, RasiId, RelatedRasiId)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_RasiDrishti)
BEGIN
    -- RasiId/RelatedRasiId are tbl_SignAttributes.Id (1=Aries..12=Pisces).
    -- Movable {1,4,7,10} -> fixed {2,5,8,11} except the next sign; fixed -> movable
    -- except the previous sign; dual {3,6,9,12} -> the other 3 dual signs.
    INSERT dbo.tbl_Rule_RasiDrishti (RuleSetId, RasiId, RelatedRasiId, MethodCode, SourceRefCode)
    SELECT 1, v.RasiId, v.RelatedRasiId, 'RASI_DRISHTI', 'SRC_PVR_INTEGRATED'
    FROM (VALUES
    -- Aries (movable, excludes Taurus)
    (1, 5), (1, 8), (1, 11),
    -- Cancer (movable, excludes Leo)
    (4, 8), (4, 11), (4, 2),
    -- Libra (movable, excludes Scorpio)
    (7, 11), (7, 2), (7, 5),
    -- Capricorn (movable, excludes Aquarius)
    (10, 2), (10, 5), (10, 8),
    -- Taurus (fixed, excludes Aries)
    (2, 4), (2, 7), (2, 10),
    -- Leo (fixed, excludes Cancer)
    (5, 7), (5, 10), (5, 1),
    -- Scorpio (fixed, excludes Libra)
    (8, 10), (8, 1), (8, 4),
    -- Aquarius (fixed, excludes Capricorn)
    (11, 1), (11, 4), (11, 7),
    -- Gemini (dual)
    (3, 6), (3, 9), (3, 12),
    -- Virgo (dual)
    (6, 3), (6, 9), (6, 12),
    -- Sagittarius (dual)
    (9, 3), (9, 6), (9, 12),
    -- Pisces (dual)
    (12, 3), (12, 6), (12, 9)
    ) v (RasiId, RelatedRasiId);
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_Catalog WHERE RuleTableName = 'tbl_Rule_RasiDrishti')
    INSERT dbo.tbl_Rule_Catalog (RuleTableName, EngineCode, MethodCodes, Purpose, IntroducedIn)
    VALUES ('tbl_Rule_RasiDrishti', 'RELATIONSHIP', 'RASI_DRISHTI',
            'Sign-to-sign (rasi) aspects (PVR sec.10.3): movable<->fixed except the adjacent sign, dual<->dual. Symmetric, both directions seeded. Feeds the Stronger Rasi comparator (sec.15.5.2 rule 2) used by GrahaArudhaCalculator.',
            '107_create_rule_rasi_drishti.sql');
GO

INSERT dbo.SchemaMigrations (ScriptName, Note)
SELECT '107_create_rule_rasi_drishti.sql',
       'tbl_Rule_RasiDrishti created + seeded (36 rows, RuleSetId 1); 1 tbl_Rule_Catalog row'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '107_create_rule_rasi_drishti.sql');
GO

PRINT '107 applied: tbl_Rule_RasiDrishti created and seeded (36 rows).';
GO
