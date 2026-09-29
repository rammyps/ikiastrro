-- =====================================================================
-- 152 - House reference points, part 2: Indu Lagna + Pranapada Lagna.
--
-- Follow-on to db/32 (17 rows). Two new tbl_Dim_HouseReference rows,
-- BasisKind='SpecialLagna' pointing at the db/150 tbl_Dim_SpecialLagnas
-- rows - the CK_Dim_HouseReference_Basis CHECK already permits
-- 'SpecialLagna', no schema change needed. AppliesInVarga='Rasi', matching
-- Sree Lagna's precedent (a D1-only classical technique), not Hora/Ghati's
-- 'Any' (which PVR sec 7.1 explicitly uses as house references in any
-- chart). No tbl_Rule_HouseReferenceMatter rows - that table is
-- specifically PVR Table 12 (graha lagnas), not this family. No new
-- terminology needed here - db/151's SPT_IL/SPT_PP already cover it, the
-- same reuse Ghati/Hora/Bhaava/Sree already make of db/29's SPT_* rows
-- (see db/32's own header comment).
--
-- Idempotent: seed is IF NOT EXISTS on the two rows.
-- NOTE: db/40 later tightened EnglishName/SanskritName (bilingual taxonomy fields) to NOT NULL
-- on this table - both are supplied here, following db/40's own convention of the Sanskrit
-- column simply repeating the transliterated term for reference points named in Sanskrit already
-- (e.g. its own 'GHATI_LAGNA' -> N'Ghati Lagna' row).
-- Apply:  sqlcmd -S localhost -E -d ikiastrro -b -i db/152_add_indu_pranapada_house_reference.sql
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Dim_HouseReference WHERE ReferenceCode IN ('INDU_LAGNA', 'PRANAPADA_LAGNA'))
    INSERT dbo.tbl_Dim_HouseReference
        (ReferenceCode, ReferenceName, BasisKind, BasisSpecialLagnaId, Perspective, AppliesInVarga, SortOrder, SourceRefCode, EnglishName, SanskritName)
    SELECT v.ReferenceCode, v.ReferenceName, 'SpecialLagna', sl.Id, v.Perspective, 'Rasi', v.SortOrder, 'SRC_PYJHORA', v.ReferenceName, v.ReferenceName
    FROM (VALUES
        ('INDU_LAGNA',      N'Indu Lagna',      'INDU_LAGNA',
            N'Wealth-yielding capacity - an own-sign or exalted planet placed in Indu Lagna, or in the 2nd/11th house counted from it, forms Koteeswara Yoga.', 18),
        ('PRANAPADA_LAGNA', N'Pranapada Lagna', 'PRANAPADA_LAGNA',
            N'Vitality and life-force - house placement counted from the Lagna is read for health and longevity.', 19)
    ) v (ReferenceCode, ReferenceName, LagnaCode, Perspective, SortOrder)
    JOIN dbo.tbl_Dim_SpecialLagnas sl ON sl.LagnaCode = v.LagnaCode;
GO

-- --- Batch 2: ledger + summary ---
INSERT dbo.SchemaMigrations (ScriptName, Note)
SELECT '152_add_indu_pranapada_house_reference.sql',
       'tbl_Dim_HouseReference +2 (INDU_LAGNA, PRANAPADA_LAGNA), BasisKind=SpecialLagna, AppliesInVarga=Rasi.'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '152_add_indu_pranapada_house_reference.sql');
GO

DECLARE @refs  INT = (SELECT COUNT(*) FROM dbo.tbl_Dim_HouseReference WHERE ReferenceCode IN ('INDU_LAGNA','PRANAPADA_LAGNA'));
DECLARE @refsl INT = (SELECT COUNT(*) FROM dbo.tbl_Dim_HouseReference
    WHERE ReferenceCode IN ('INDU_LAGNA','PRANAPADA_LAGNA') AND (BasisKind <> 'SpecialLagna' OR BasisSpecialLagnaId IS NULL));
PRINT '152 applied: ' + CAST(@refs AS VARCHAR(10)) + ' new references (expect 2), '
    + CAST(@refsl AS VARCHAR(10)) + ' missing SpecialLagna basis (expect 0).';
GO
