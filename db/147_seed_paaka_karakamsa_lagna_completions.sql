-- =====================================================================
-- 147 - Phase 3 (special-lagna audit): completes two more already-cited-
-- but-unseeded special-lagna references, now that Paaka Lagna and
-- Karakamsa Lagna are resolvable (LifeMatters.razor's ResolveReferenceSign,
-- added alongside this migration - both are "sign lookups against an
-- already-known planet position," not new astronomical computation: Paaka
-- Lagna = wherever D1's own Lagna-lord currently sits; Karakamsa Lagna =
-- wherever the Atmakaraka sits specifically in D9. Neither needs a new
-- calculator or a persisted SpecialPoint row - confirmed by checking
-- KarakaPolarWheel.razor, which never asks for either as a chart point).
--
-- Same "completion, not a new row" pattern as migration 146's CAREER_
-- STATUS_09/WEALTH_01 rows - both citations already sat unseeded in
-- tbl_Rule_LifeMatterReference's own CalculationNarrative:
--   EDUCATION_04 ("Memory"): "PVR (sec 7.3 discussion of paaka lagna):
--     'the 5th house from paaka lagna shows memory the best'" (db/087) -
--     adds House(PAAKA_LAGNA, 5) alongside its existing Mercury-based Focus.
--   SPIRITUALITY_08 ("Mokṣa"): its own db/32 terminology citation
--     (HREF_KARAKAMSA_LAGNA, PVR sec 7.3.6): "the 12th from it shows
--     moksha" - adds House(KARAKAMSA_LAGNA, 12) alongside its existing
--     Ketu-based Focus.
--
-- Still open (not attempted here): the 7 Graha Lagnas (PVR sec 7.3.9,
-- Table 12) are also now resolvable in code (GRAHA_LAGNA_* reference
-- codes), but every Table-12 pairing checked so far (e.g. Mars-Lagna ->
-- house 3, matching FAMILY_RELATIONSHIPS_04) already has an equivalent
-- fact expressed through tbl_Rule_KarakaMatter's "Nth from this planet"
-- mechanism, so seeding a redundant Focus row there wasn't judged to add
-- real value. The resolver support exists and is ready whenever a genuine
-- non-redundant Graha-Lagna-specific matter is identified.
--
-- Apply:  sqlcmd -S "localhost\SQLSERVER2025" -E -d ikiastrro -b -f 65001 -i db/147_seed_paaka_karakamsa_lagna_completions.sql
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS (
    SELECT 1 FROM dbo.tbl_Rule_LifeMatterFocus f
    JOIN dbo.tbl_Dim_LifeMatter lm ON lm.Id = f.LifeMatterId
    WHERE lm.Code = 'EDUCATION_04' AND f.ReferenceCode = 'PAAKA_LAGNA'
)
    INSERT dbo.tbl_Rule_LifeMatterFocus (RuleSetId, LifeMatterId, FocusKind, ReferenceCode, HouseNumber, SpecialPointCode, SourceRefCode)
    SELECT 1, lm.Id, 'House', 'PAAKA_LAGNA', 5, NULL, 'SRC_PVR_INTEGRATED'
    FROM dbo.tbl_Dim_LifeMatter lm
    WHERE lm.Code = 'EDUCATION_04';
GO

IF NOT EXISTS (
    SELECT 1 FROM dbo.tbl_Rule_LifeMatterFocus f
    JOIN dbo.tbl_Dim_LifeMatter lm ON lm.Id = f.LifeMatterId
    WHERE lm.Code = 'SPIRITUALITY_08' AND f.ReferenceCode = 'KARAKAMSA_LAGNA'
)
    INSERT dbo.tbl_Rule_LifeMatterFocus (RuleSetId, LifeMatterId, FocusKind, ReferenceCode, HouseNumber, SpecialPointCode, SourceRefCode)
    SELECT 1, lm.Id, 'House', 'KARAKAMSA_LAGNA', 12, NULL, 'SRC_PVR_INTEGRATED'
    FROM dbo.tbl_Dim_LifeMatter lm
    WHERE lm.Code = 'SPIRITUALITY_08';
GO

INSERT dbo.SchemaMigrations (ScriptName, Note)
SELECT '147_seed_paaka_karakamsa_lagna_completions.sql',
       'Completes EDUCATION_04/Paaka Lagna and SPIRITUALITY_08/Karakamsa Lagna Focus rows, now that both references are resolvable in LifeMatters.razor.'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '147_seed_paaka_karakamsa_lagna_completions.sql');
GO

DECLARE @paaka INT = (
    SELECT COUNT(*) FROM dbo.tbl_Rule_LifeMatterFocus f JOIN dbo.tbl_Dim_LifeMatter lm ON lm.Id = f.LifeMatterId
    WHERE lm.Code = 'EDUCATION_04' AND f.ReferenceCode = 'PAAKA_LAGNA');
DECLARE @karakamsa INT = (
    SELECT COUNT(*) FROM dbo.tbl_Rule_LifeMatterFocus f JOIN dbo.tbl_Dim_LifeMatter lm ON lm.Id = f.LifeMatterId
    WHERE lm.Code = 'SPIRITUALITY_08' AND f.ReferenceCode = 'KARAKAMSA_LAGNA');
PRINT '147 applied: EDUCATION_04/Paaka Lagna rows=' + CAST(@paaka AS VARCHAR(10)) + ' (expected 1), '
    + 'SPIRITUALITY_08/Karakamsa Lagna rows=' + CAST(@karakamsa AS VARCHAR(10)) + ' (expected 1).';
GO
