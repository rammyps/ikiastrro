-- =====================================================================
-- 155 - Two more tbl_Rule_StrengthBand scales (migration 154), for the
-- Life Matters three-axis statistics (docs/research/domain/stat_strength.md
-- §1.3, §3, §4):
--
--   BAV_BINDUS                 5 / 4   SRC_PVR_INTEGRATED
--       One planet's Bhinnashtakavarga bindus in a sign: 5 or more good,
--       3 or fewer bad (docs/research/domain/transit-events.md, the same PVR
--       row the SAV band cites). 4 is the middle.
--   BHAVA_BALA_INDEPENDENT_Z   1 / -1  SRC_IKIASTRRO_SYNTHESIS
--       Independent Bhava Bala = Bhava Dig + Bhava Drik, WITHOUT Bhavadhipati
--       Bala (which is the lord's own Shadbala - combining raw Bhava Bala with
--       the lord's Shadbala counted the lord twice). Scored as a z-score
--       against the same chart's 12 houses. No classical cut-off exists;
--       +-1 SD is the project's heuristic (rammyps, 2026-10-01).
--
-- Mirrored by Ikiastrro.Core.Engines.Strength.StrengthBands; CLI
-- `verify-strength` Phase 3 checks every scale.
--
-- Apply:  sqlcmd -S "localhost\SQLSERVER2025" -E -d ikiastrro -b -i db/155_add_bav_and_independent_bhava_bands.sql
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

;WITH seed (ScaleCode, BandCode, LowerBound, SourceRefCode, Narrative) AS (
    SELECT * FROM (VALUES
        ('BAV_BINDUS',               'STRONG',    5.000, 'SRC_PVR_INTEGRATED',
            N'A planet''s Bhinnashtakavarga with 5 or more bindus in a sign is good (docs/research/domain/transit-events.md).'),
        ('BAV_BINDUS',               'MODERATE',  4.000, 'SRC_PVR_INTEGRATED',
            N'3 or fewer bindus is bad (same source); 4 is the middle.'),
        ('BHAVA_BALA_INDEPENDENT_Z', 'STRONG',    1.000, 'SRC_IKIASTRRO_SYNTHESIS',
            N'Project heuristic: independent Bhava Bala (Dig + Drik) at least 1 SD above the chart''s own 12-house mean reads Strong.'),
        ('BHAVA_BALA_INDEPENDENT_Z', 'MODERATE', -1.000, 'SRC_IKIASTRRO_SYNTHESIS',
            N'Project heuristic: more than 1 SD below the chart''s 12-house mean reads Weak.')
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

IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '155_add_bav_and_independent_bhava_bands.sql')
    INSERT dbo.SchemaMigrations (ScriptName, AppliedAtUtc, Note)
    VALUES ('155_add_bav_and_independent_bhava_bands.sql', SYSUTCDATETIME(),
            'BAV and chart-relative independent Bhava Bala cut-offs for the Life Matters three-axis statistics.');
GO

DECLARE @rows INT = (SELECT COUNT(*) FROM dbo.tbl_Rule_StrengthBand WHERE RuleSetId = 1 AND IsActive = 1);
PRINT '155 applied: ' + CAST(@rows AS VARCHAR(10)) + ' tbl_Rule_StrengthBand rows (expect 10).';
GO
