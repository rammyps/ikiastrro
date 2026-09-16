-- =====================================================================
-- 077 — Tighten the Ashtakavarga reduction rules to the exact, verified
--       variant now implemented by AshtakavargaCalculator (verify-ashtakavarga
--       reproduces the JHora export for 1_Ramakrishnan: BAV, SAV and every
--       Rasi/Graha/Sodhya Pinda triple).
--
--   * TRIKONA_SODHANA / EKADHIPATYA_SODHANA AlgorithmJson -> the concrete
--     rules (Ekadhipatya "3c" equal-value case included).
--   * new SODHYA_PINDA step carrying the rasimana / grahamana multipliers
--     (JHora / Horosoft variant, Virgo = 5).
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- Allow the SODHYA_PINDA step.
IF OBJECT_ID('dbo.CK_Rule_AshtakavargaReduction_Step', 'C') IS NOT NULL
    ALTER TABLE dbo.tbl_Rule_AshtakavargaReduction DROP CONSTRAINT CK_Rule_AshtakavargaReduction_Step;
ALTER TABLE dbo.tbl_Rule_AshtakavargaReduction ADD CONSTRAINT CK_Rule_AshtakavargaReduction_Step
    CHECK (StepCode IN ('TRIKONA_SODHANA', 'EKADHIPATYA_SODHANA', 'SODHYA_PINDA'));
GO

UPDATE dbo.tbl_Rule_AshtakavargaReduction
   SET AlgorithmJson = N'{"appliesTo":"each Bhinnashtakavarga","trines":[[1,5,9],[2,6,10],[3,7,11],[4,8,12]],"rules":["(1) if any of the three has 0 bindus, no reduction","(2) if all three are equal (and non-zero), set all three to 0","(3) otherwise subtract the lowest of the three from all three"],"sourceLocator":"BPHS Ashtakavarga - Trikona Sodhana"}',
       SourceLocator = N'BPHS Ashtakavarga Adhyaya - Trikona Sodhana.'
 WHERE RuleSetId = 1 AND MethodCode = 'PVR_PARASARA_BAV' AND StepCode = 'TRIKONA_SODHANA';

UPDATE dbo.tbl_Rule_AshtakavargaReduction
   SET AlgorithmJson = N'{"appliesTo":"each Bhinnashtakavarga, after Trikona Sodhana","sameLordPairs":[[1,8],[2,7],[3,6],[9,12],[10,11]],"singleLordSigns":[4,5],"occupancy":"D1 rasi occupancy by the seven grahas","rules":["(1) if either rasi has 0 bindus, no reduction","(2) if both rasis are occupied, no reduction","(3) exactly one occupied: the empty rasi -> 0 if its value <= the occupied rasi''s value, else -> the occupied rasi''s value (the equal-value case reduces to 0)","(4) both empty: if values differ, both take the lower value; if equal, both -> 0"],"sourceLocator":"BPHS Ashtakavarga - Ekadhipatya Sodhana"}',
       SourceLocator = N'BPHS Ashtakavarga Adhyaya - Ekadhipatya Sodhana (3c equal-value variant).'
 WHERE RuleSetId = 1 AND MethodCode = 'PVR_PARASARA_BAV' AND StepCode = 'EKADHIPATYA_SODHANA';
GO

INSERT dbo.tbl_Rule_AshtakavargaReduction
    (RuleSetId, MethodCode, StepCode, StepOrder, AlgorithmJson, SourceRefCode, SourceLocator)
SELECT 1, 'PVR_PARASARA_BAV', 'SODHYA_PINDA', 3,
       N'{"appliesTo":"each Bhinnashtakavarga, after Trikona + Ekadhipatya Sodhana","rasimana":[7,10,8,4,10,5,7,8,9,5,11,12],"rasimanaNote":"Aries..Pisces; JHora / Horosoft variant (Virgo = 5, not the PVR-book 6)","grahamana":{"Sun":5,"Moon":5,"Mars":8,"Mercury":5,"Jupiter":10,"Venus":7,"Saturn":5},"formula":["RasiPinda = sum over signs of reducedBindu[sign] * rasimana[sign]","GrahaPinda = sum over the seven grahas g of grahamana[g] * reducedBindu[natalSignOf(g)]","SodhyaPinda = RasiPinda + GrahaPinda"],"sourceLocator":"BPHS Ashtakavarga - Sodhya Pinda"}',
       'SRC_BPHS_ASHTAKAVARGA', N'BPHS Ashtakavarga Adhyaya - Sodhya Pinda; multipliers verified against the JHora export for 1_Ramakrishnan.'
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.tbl_Rule_AshtakavargaReduction
    WHERE RuleSetId = 1 AND MethodCode = 'PVR_PARASARA_BAV' AND StepCode = 'SODHYA_PINDA'
);
GO

INSERT dbo.SchemaMigrations (ScriptName, Note)
SELECT '077_tighten_ashtakavarga_reduction_rules.sql',
       'Tighten Trikona/Ekadhipatya AlgorithmJson to the implemented variant; add the SODHYA_PINDA step with rasimana/grahamana multipliers.'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '077_tighten_ashtakavarga_reduction_rules.sql');
GO

PRINT '077 applied: Ashtakavarga reduction rules tightened.';
GO
