-- =====================================================================
-- 078 — Correct the Moon and Venus rows of tbl_Rule_AshtakavargaContribution
--       to the standard Parāśari variant that Jagannatha Hora uses.
--
-- Migration 075 seeded the benefic-places matrix from jyotishganit's static
-- table, whose Moon and Venus rows omit the classical Parāśari corrections:
--   Moon    benefic in the 9th from Moon, malefic in the 9th from Mars,
--           benefic in the 2nd and malefic in the 12th from Jupiter
--   Venus   benefic in the 4th, malefic in the 5th from Mars
-- With these four cells corrected, every Bhinnāṣṭakavarga row of the JHora
-- export for 1_Ramakrishnan is reproduced exactly (verify-ashtakavarga).
-- Row totals (Moon 49, Venus 52) and the SAV grand total (337) are unchanged.
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

;WITH fix (RecipientCode, ContributorCode, BeneficPlacesJson) AS (
    SELECT * FROM (VALUES
        ('MOON',  'MOON',    '[1,3,6,7,9,10,11]'),
        ('MOON',  'MARS',    '[2,3,5,6,10,11]'),
        ('MOON',  'JUPITER', '[1,2,4,7,8,10,11]'),
        ('VENUS', 'MARS',    '[3,4,6,9,11,12]')
    ) v (RecipientCode, ContributorCode, BeneficPlacesJson)
)
UPDATE r
   SET r.BeneficPlacesJson = f.BeneficPlacesJson,
       r.SourceLocator = N'BPHS Ashtakavarga Adhyaya; Parāśari Moon/Venus corrections (mig. 078).'
FROM dbo.tbl_Rule_AshtakavargaContribution r
JOIN fix f ON f.RecipientCode = r.RecipientCode AND f.ContributorCode = r.ContributorCode
WHERE r.RuleSetId = 1 AND r.MethodCode = 'PVR_PARASARA_BAV'
  AND r.BeneficPlacesJson <> f.BeneficPlacesJson;
GO

INSERT dbo.SchemaMigrations (ScriptName, Note)
SELECT '078_correct_ashtakavarga_moon_venus_benefic_places.sql',
       'Correct the Moon/Venus benefic-places rows to the Parāśari variant JHora uses (row totals + SAV 337 unchanged).'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '078_correct_ashtakavarga_moon_venus_benefic_places.sql');
GO

PRINT '078 applied: Ashtakavarga Moon/Venus benefic-places corrected.';
GO
