-- =====================================================================
-- 138 - Fix a transcription error in migration 087's MARRIAGE_SPOUSE row:
-- Upapada Lagna is the arudha of the 12th house (A12), not the 7th. The
-- 7th's own arudha is Darapada (A7) - a different point entirely.
--
-- Never edit an applied migration (087) - correct forward instead. Phase
-- 0A's reference audit (docs/ui/lifematters_phase0a_audit.md) confirmed
-- no FK anywhere in the schema targets tbl_Rule_LifeMatterReference, so
-- no Fact/result row can be reading this claim structurally; the error
-- is confined to this one free-text CalculationNarrative column. Per
-- lifematters_plan.md's migration policy, an unreferenced RuleSet gets
-- corrected in place - no RuleSet version bump needed.
--
-- Source: SRC_PVR_INTEGRATED ch.9 "Arudha Padas" - p.87 ("arudha pada of
-- 12th house is denoted as UL (upapada lagna)"), p.89 Table 18 (A7 =
-- Dara Pada/Dararudha; A12 = Upapada Lagna, distinct rows), pp.91-92
-- (restated), and sec 9.7 Summary p.99 ("Darapada or A7 shows one's
-- relationships. Upapada (UL) shows one's marriage and spouse.").
-- Cross-checked against this project's own ArudhaCalculator.cs, which
-- already computes code "A{house}" generically per house number - A12
-- is definitionally the arudha of house 12 in this codebase too.
--
-- Full citation trail: docs/ui/lifematters_claude_research.md, "UL/A12
-- versus A7". The plan's own approved decision 1 (UL is A12, not a
-- 7th-house derivation) already assumed this correction.
-- Apply:  sqlcmd -S "localhost\SQLSERVER2025" -E -d ikiastrro -b -f 65001 -i db/138_fix_upapada_lagna_arudha_house_transcription.sql
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

UPDATE dbo.tbl_Rule_LifeMatterReference
SET CalculationNarrative = N'Upapada Lagna (UL, arudha of the 12th house / A12 - not the 7th) is PVR''s own stated reference for the visible/social side of marriage (ch. 9 "Arudha Padas," pp. 87, 91-92, 99), used throughout the book for marriage timing.'
WHERE CategoryCode = 'MARRIAGE_SPOUSE'
  AND KarakaText = 'Venus'
  AND HouseFromLagnaText = 'Upapada Lagna'
  AND CalculationNarrative = N'Upapada Lagna (arudha of the 7th) is PVR''s own stated reference for the visible/social side of marriage, used throughout the book for marriage timing.';
GO

INSERT dbo.SchemaMigrations (ScriptName, Note)
SELECT '138_fix_upapada_lagna_arudha_house_transcription.sql',
       'Corrects tbl_Rule_LifeMatterReference''s MARRIAGE_SPOUSE/Upapada Lagna row: UL is the arudha of the 12th (A12), not the 7th; unreferenced by any FK, so corrected in place per the migration-087 policy.'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '138_fix_upapada_lagna_arudha_house_transcription.sql');
GO

DECLARE @corrected INT = (
    SELECT COUNT(*) FROM dbo.tbl_Rule_LifeMatterReference
    WHERE CategoryCode = 'MARRIAGE_SPOUSE' AND HouseFromLagnaText = 'Upapada Lagna'
      AND CalculationNarrative LIKE N'%arudha of the 12th house / A12%'
);
PRINT '138 applied: MARRIAGE_SPOUSE/Upapada Lagna rows now reading A12 = ' + CAST(@corrected AS VARCHAR(10)) + ' (expected 1).';
GO
