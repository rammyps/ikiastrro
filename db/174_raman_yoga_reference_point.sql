-- =====================================================================
-- 174 — B.V. Raman yogas: Type is the reference point, Sun / Moon / Lagna.
--
-- Migration 079 seeded tbl_Rule_Yoga.FormationFamilyCode with SUN / MOON /
-- LAGNA / COMBINATION, and 258 of the 318 Raman 300-Combinations variants
-- ended up as COMBINATION - a catch-all, not a reference point. The Yoga tab's
-- Type column is "which point the yoga is judged from", so every Raman yoga
-- now reads Sun based, Moon based or Lagna based.
--
-- Rule used: a formation counted from the Sun (or Sun-centred) is SUN; counted
-- from the Moon (or Moon-centred) is MOON; everything counted from the Lagna -
-- house-lord yogas, kendra/trikona placements, Nabhasa and Malika patterns -
-- is LAGNA. Raman's yogas that name Sun, Moon and Lagna together (Mahabhagya,
-- Vasumathi, Kulavardhana) are LAGNA, the primary point. Yogas whose
-- generic row is already SUN / MOON / LAGNA are untouched.
--
-- Written as SOURCE-SPECIFIC override rows (mechanism from migration 119), so
-- the PVR rows for the same YogaCode, which carry their own type, do not move.
-- Raman yogas with no coded predicate (generic row absent) keep a NULL type.
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '174_raman_yoga_reference_point.sql')
BEGIN
    ;WITH Overrides (YogaCode, TypeCode) AS
    (
        SELECT * FROM (VALUES
            ('YOGA_BHASKARA',   'SUN'),   -- Sun 2nd from Mercury; Sun-centred
            ('YOGA_MARUD',      'MOON'),  -- Jupiter 5th from Moon
            ('YOGA_MATRUNASA',  'MOON'),  -- Moon afflicted or flanked by malefics
            ('YOGA_GUHYAROGA',  'MOON'),  -- Moon in navamsa Cancer/Scorpio with a malefic
            ('YOGA_GARUDA',     'MOON')   -- waxing Moon and its navamsa dispositor
        ) AS v (YogaCode, TypeCode)
    )
    INSERT dbo.tbl_Rule_Yoga (RuleSetId, YogaCode, SourceRefCode, FormationFamilyCode)
    SELECT g.RuleSetId, g.YogaCode, 'SRC_RAMAN_300_COMBINATIONS', COALESCE(o.TypeCode, 'LAGNA')
    FROM dbo.tbl_Rule_Yoga g
    LEFT JOIN Overrides o ON o.YogaCode = g.YogaCode
    WHERE g.SourceRefCode IS NULL
      AND g.FormationFamilyCode = 'COMBINATION'
      AND EXISTS (SELECT 1 FROM dbo.tbl_Rule_YogaVariant v
                  WHERE v.RuleSetId = g.RuleSetId AND v.YogaCode = g.YogaCode
                    AND v.SourceRefCode = 'SRC_RAMAN_300_COMBINATIONS')
      AND NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_Yoga s
                      WHERE s.RuleSetId = g.RuleSetId AND s.YogaCode = g.YogaCode
                        AND s.SourceRefCode = 'SRC_RAMAN_300_COMBINATIONS');

    INSERT dbo.SchemaMigrations (ScriptName, Note)
    VALUES ('174_raman_yoga_reference_point.sql',
        'Raman yogas typed Sun/Moon/Lagna based: source-specific tbl_Rule_Yoga rows replace the COMBINATION catch-all.');
END
GO

PRINT '174 applied: Raman yoga types are Sun / Moon / Lagna based.';
GO
