-- =====================================================================
-- 160 - tbl_Rule_StrengthBand: Strong / Moderate / Weak for Vimsopaka
-- Bala (out of 20), so its bars read the same green / accent / red as
-- Shadbala on Astro Facts step 6 (rammyps, 2026-10-02).
--
-- No classical cut-off on file, so both boundaries are the project's own
-- (SRC_IKIASTRRO_SYNTHESIS), read off BPHS's dignity factors themselves
-- (Varga Viveka - the factors VimsopakaCalculator uses): a score is the
-- weighted average factor, so 15 = friend's factor on average across the
-- scheme's vargas, 10 = neutral's. At or above 15 Strong; 10 to under 15
-- Moderate; below 10 (worse than neutral on average) Weak.
--
-- Mirrored in code by StrengthBands.VimsopakaScore; CLI verify-strength
-- checks the two agree.
--
-- Apply:  sqlcmd -S "localhost\SQLSERVER2025" -E -d ikiastrro -b -i db/160_add_vimsopaka_band.sql
-- =====================================================================
USE [ikiastrro];
GO

;WITH seed (ScaleCode, BandCode, LowerBound, SourceRefCode, Narrative) AS (
    SELECT * FROM (VALUES
        ('VIMSOPAKA_SCORE', 'STRONG',   15.000, 'SRC_IKIASTRRO_SYNTHESIS',
            N'Project heuristic: 15 of 20 and above - friend''s dignity factor (15) or better on average across the scheme''s vargas.'),
        ('VIMSOPAKA_SCORE', 'MODERATE', 10.000, 'SRC_IKIASTRRO_SYNTHESIS',
            N'Project heuristic: 10 to under 15 - between neutral''s factor (10) and friend''s on average; below 10 Weak.')
    ) s (ScaleCode, BandCode, LowerBound, SourceRefCode, Narrative)
)
INSERT dbo.tbl_Rule_StrengthBand (RuleSetId, ScaleCode, BandCode, LowerBound, SourceRefCode, CalculationNarrative)
SELECT 1, s.ScaleCode, s.BandCode, s.LowerBound, s.SourceRefCode, s.Narrative
FROM seed s
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.tbl_Rule_StrengthBand r
    WHERE r.RuleSetId = 1 AND r.ScaleCode = s.ScaleCode AND r.BandCode = s.BandCode
);
GO

IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '160_add_vimsopaka_band.sql')
    INSERT dbo.SchemaMigrations (ScriptName, AppliedAtUtc, Note)
    VALUES ('160_add_vimsopaka_band.sql', SYSUTCDATETIME(),
            'Strong/Moderate/Weak cut-offs for Vimsopaka Bala (15 / 10 of 20), project heuristic.');
GO

DECLARE @rows INT = (SELECT COUNT(*) FROM dbo.tbl_Rule_StrengthBand WHERE RuleSetId = 1 AND IsActive = 1 AND ScaleCode = 'VIMSOPAKA_SCORE');
PRINT '160 applied: ' + CAST(@rows AS VARCHAR(10)) + ' VIMSOPAKA_SCORE rows (expect 2).';
GO
