-- =====================================================================
-- 153 - Connect Indu Lagna + Pranapada Lagna into Life Matters.
--
-- Follow-on to db/150-152. Only two life-matter linkages are structured
-- here as Focus rows - the ones the special-lagna research pass found a
-- specific, defensible house-number claim for. Everything else that
-- research turned up (Bhaava Lagna's only-weak ties; Sree Lagna->Marriage
-- and Ghatika Lagna->Children/Education's soft or weakly-sourced claims)
-- is deliberately left unseeded - see the note at the end of this header,
-- same "still open, not attempted here" convention as earlier LifeMatter
-- migrations.
--
-- (1) Indu Lagna -> Wealth (Koteeswara Yoga). No existing WEALTH matter
--     covers "wealth-yielding capacity" specifically (WEALTH_01-08 cover
--     overall condition, accumulation, family resources, speculation,
--     debt, gains, credits, expenditure - db/087; WEALTH_09/10 are a
--     separate in-progress seeding pass's "world perception"/"prosperity"
--     rows) - a new WEALTH_11 row is added. Focus: House(INDU_LAGNA, 1) -
--     the classical read is planets IN Indu Lagna itself (house 1 counted
--     from it), not a fixed house elsewhere.
-- (2) Pranapada Lagna -> Health and -> Longevity. Both use the SAME
--     mechanism (Pranapada's own house placement from the Lagna), so both
--     get a House(PRANAPADA_LAGNA, 1) completion Focus row - one on
--     SELF_HEALTH_02 ("General health"), one on TROUBLE_LOSS_07
--     ("Longevity") - both confirmed against the live schema, not
--     guessed, and each already carries an unrelated LAGNA-based Focus
--     row this one sits alongside rather than replaces.
--
-- Neither claim is a PVR "Integrated Approach" citation (PVR ch.5 doesn't
-- cover Indu/Pranapada at all) - BasisCode/SourceRefCode = PROJECT_SYNTHESIS
-- / SRC_IKIASTRRO_SYNTHESIS throughout, the same code db/087 already
-- registers and uses for this project's own non-PVR-direct reasoning.
--
-- Deliberately NOT seeded (backlog, not a gap):
--   - Bhaava Lagna: no Focus or interpretation row for anything. Research
--     found only weak/supplementary ties everywhere, and tbl_Dim_
--     SpecialLagnas.UsedInBook=0 for Bhaava already reflects PVR treating
--     it as "for completeness" only.
--   - Sree Lagna -> Marriage: a soft finding with no specific house-number
--     claim ("secondary traditions extend it, never replacing Upapada
--     Lagna"). Sree Lagna already has a Wealth Focus row from the
--     "world perception" pass (WEALTH_10); no new row added here.
--   - Ghatika Lagna -> Children/Education (5th house): the one source for
--     this was a search-snippet claim NOT confirmed on full-page fetch -
--     too weak to promote to a structural Focus row.
--
-- Idempotent throughout.
-- Apply:  sqlcmd -S localhost -E -d ikiastrro -b -i db/153_seed_indu_pranapada_lifematters.sql
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

-- --- Batch 1: WEALTH_11 - new tbl_Rule_LifeMatterReference + tbl_Dim_LifeMatter row ---
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_LifeMatterReference WHERE CategoryCode = 'WEALTH' AND MatterText = N'Wealth-yielding capacity (Koteeswara Yoga)')
    INSERT dbo.tbl_Rule_LifeMatterReference
        (RuleSetId, CategoryCode, CategoryName, DisplayOrder, MatterText, PrimaryChartsText,
         HouseFromLagnaText, KarakaText, HouseFromKarakaText, BasisCode, CalculationNarrative, SourceRefCode, IsActive)
    VALUES (1, 'WEALTH', N'Wealth and financial matters', 11, N'Wealth-yielding capacity (Koteeswara Yoga)', 'D1',
        N'1st from Indu Lagna', N'Indu Lagna', N'1st from Indu Lagna (own/exalted planet in it, or 2nd/11th from it, forms Koteeswara Yoga)',
        'PROJECT_SYNTHESIS',
        N'Distinct from WEALTH_01 (overall financial condition, Jupiter-based) and WEALTH_02/03 (accumulated wealth/family resources, also Jupiter-based): Indu Lagna is the classical technique specifically for assessing wealth-YIELDING CAPACITY, independent of the standard Jupiter/2nd/11th axis. Not a PVR "Integrated Approach" citation - PVR ch.5 does not cover Indu Lagna.',
        'SRC_IKIASTRRO_SYNTHESIS', 1);
GO

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Dim_LifeMatter WHERE Code = 'WEALTH_11')
    INSERT dbo.tbl_Dim_LifeMatter (Code, EnglishName, CategoryCode, CategoryName, SourceGroup, DisplayOrder)
    SELECT 'WEALTH_11', lm.MatterText, lm.CategoryCode, lm.CategoryName, 'PVR_LIFE_MATTER', lm.DisplayOrder
    FROM dbo.tbl_Rule_LifeMatterReference lm
    WHERE lm.CategoryCode = 'WEALTH' AND lm.MatterText = N'Wealth-yielding capacity (Koteeswara Yoga)';
GO

UPDATE lm
   SET LifeMatterId = dm.Id
FROM dbo.tbl_Rule_LifeMatterReference lm
JOIN dbo.tbl_Dim_LifeMatter dm ON dm.Code = 'WEALTH_11'
WHERE lm.CategoryCode = 'WEALTH' AND lm.MatterText = N'Wealth-yielding capacity (Koteeswara Yoga)' AND lm.LifeMatterId IS NULL;
GO

-- --- Batch 2: Focus row - WEALTH_11 x Indu Lagna ---
IF NOT EXISTS (
    SELECT 1 FROM dbo.tbl_Rule_LifeMatterFocus f JOIN dbo.tbl_Dim_LifeMatter lm ON lm.Id = f.LifeMatterId
    WHERE lm.Code = 'WEALTH_11' AND f.IsActive = 1 AND f.FocusKind = 'House' AND f.ReferenceCode = 'INDU_LAGNA' AND f.HouseNumber = 1
)
    INSERT dbo.tbl_Rule_LifeMatterFocus (RuleSetId, LifeMatterId, FocusKind, ReferenceCode, HouseNumber, SourceRefCode)
    SELECT 1, lm.Id, 'House', 'INDU_LAGNA', 1, 'SRC_IKIASTRRO_SYNTHESIS'
    FROM dbo.tbl_Dim_LifeMatter lm WHERE lm.Code = 'WEALTH_11';
GO

-- --- Batch 3: completion Focus rows on existing matters - Pranapada Lagna ---
IF NOT EXISTS (
    SELECT 1 FROM dbo.tbl_Rule_LifeMatterFocus f JOIN dbo.tbl_Dim_LifeMatter lm ON lm.Id = f.LifeMatterId
    WHERE lm.Code = 'SELF_HEALTH_02' AND f.IsActive = 1 AND f.FocusKind = 'House' AND f.ReferenceCode = 'PRANAPADA_LAGNA' AND f.HouseNumber = 1
)
    INSERT dbo.tbl_Rule_LifeMatterFocus (RuleSetId, LifeMatterId, FocusKind, ReferenceCode, HouseNumber, SourceRefCode)
    SELECT 1, lm.Id, 'House', 'PRANAPADA_LAGNA', 1, 'SRC_IKIASTRRO_SYNTHESIS'
    FROM dbo.tbl_Dim_LifeMatter lm WHERE lm.Code = 'SELF_HEALTH_02';
GO

IF NOT EXISTS (
    SELECT 1 FROM dbo.tbl_Rule_LifeMatterFocus f JOIN dbo.tbl_Dim_LifeMatter lm ON lm.Id = f.LifeMatterId
    WHERE lm.Code = 'TROUBLE_LOSS_07' AND f.IsActive = 1 AND f.FocusKind = 'House' AND f.ReferenceCode = 'PRANAPADA_LAGNA' AND f.HouseNumber = 1
)
    INSERT dbo.tbl_Rule_LifeMatterFocus (RuleSetId, LifeMatterId, FocusKind, ReferenceCode, HouseNumber, SourceRefCode)
    SELECT 1, lm.Id, 'House', 'PRANAPADA_LAGNA', 1, 'SRC_IKIASTRRO_SYNTHESIS'
    FROM dbo.tbl_Dim_LifeMatter lm WHERE lm.Code = 'TROUBLE_LOSS_07';
GO

-- --- Batch 4: tbl_Content_Interpretation rows (SubjectType='LIFE_MATTER_FOCUS') ---
-- SourceRefCode left NULL (the generic tier) - each SubjectCode has exactly one
-- narrative variant, so there is no competing classical-source text to key on;
-- attribution instead lives in the prose itself and on the Focus row above.
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Content_Interpretation WHERE RuleSetId = 1 AND SubjectType = 'LIFE_MATTER_FOCUS' AND SubjectCode = 'WEALTH_11_INDU_LAGNA' AND SourceRefCode IS NULL)
    INSERT dbo.tbl_Content_Interpretation (RuleSetId, SubjectType, SubjectCode, StandardText, ShortText)
    VALUES (1, 'LIFE_MATTER_FOCUS', 'WEALTH_11_INDU_LAGNA',
        N'Indu Lagna measures wealth-yielding capacity rather than accumulated wealth. An own-sign or exalted planet placed in Indu Lagna, or in the 2nd/11th house counted from it, forms Koteeswara Yoga - classically associated with exceptional financial capacity. Not a PVR "Integrated Approach" citation; this project''s own synthesis of secondary Jyotish sources.',
        N'Own/exalted planet in, or 2nd/11th from, Indu Lagna = Koteeswara Yoga (wealth-yielding capacity).');
GO

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Content_Interpretation WHERE RuleSetId = 1 AND SubjectType = 'LIFE_MATTER_FOCUS' AND SubjectCode = 'SELF_HEALTH_02_PRANAPADA_LAGNA' AND SourceRefCode IS NULL)
    INSERT dbo.tbl_Content_Interpretation (RuleSetId, SubjectType, SubjectCode, StandardText, ShortText)
    VALUES (1, 'LIFE_MATTER_FOCUS', 'SELF_HEALTH_02_PRANAPADA_LAGNA',
        N'Pranapada ("seat of the life-breath") is read for vitality: favourable house placement from the Lagna (2nd, 4th, 5th, 9th, 10th or 11th) is taken as a sign of good general health. Not a PVR "Integrated Approach" citation; this project''s own synthesis of secondary Jyotish sources.',
        N'Pranapada Lagna in houses 2/4/5/9/10/11 from the Lagna favours general health.');
GO

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Content_Interpretation WHERE RuleSetId = 1 AND SubjectType = 'LIFE_MATTER_FOCUS' AND SubjectCode = 'TROUBLE_LOSS_07_PRANAPADA_LAGNA' AND SourceRefCode IS NULL)
    INSERT dbo.tbl_Content_Interpretation (RuleSetId, SubjectType, SubjectCode, StandardText, ShortText)
    VALUES (1, 'LIFE_MATTER_FOCUS', 'TROUBLE_LOSS_07_PRANAPADA_LAGNA',
        N'The same Pranapada house-placement check used for general health is also read for longevity: favourable placement from the Lagna (2nd, 4th, 5th, 9th, 10th or 11th) is taken as supportive of long life; unfavourable placement (6th, 8th, 12th) as a caution. Not a PVR "Integrated Approach" citation; this project''s own synthesis of secondary Jyotish sources.',
        N'Pranapada Lagna house placement from the Lagna is also read for longevity.');
GO

-- --- Batch 5: ledger + summary ---
INSERT dbo.SchemaMigrations (ScriptName, Note)
SELECT '153_seed_indu_pranapada_lifematters.sql',
       'WEALTH_11 (Indu Lagna) + Pranapada completions on SELF_HEALTH_02/TROUBLE_LOSS_07; 3 LIFE_MATTER_FOCUS rows. SRC_IKIASTRRO_SYNTHESIS, not PVR. Bhaava/Sree-Marriage/Ghatika left unseeded.'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '153_seed_indu_pranapada_lifematters.sql');
GO

DECLARE @matter INT = (SELECT COUNT(*) FROM dbo.tbl_Dim_LifeMatter WHERE Code = 'WEALTH_11');
DECLARE @backfill INT = (SELECT COUNT(*) FROM dbo.tbl_Rule_LifeMatterReference WHERE CategoryCode = 'WEALTH' AND MatterText = N'Wealth-yielding capacity (Koteeswara Yoga)' AND LifeMatterId IS NOT NULL);
DECLARE @focus INT = (SELECT COUNT(*) FROM dbo.tbl_Rule_LifeMatterFocus f JOIN dbo.tbl_Dim_LifeMatter lm ON lm.Id = f.LifeMatterId
    WHERE f.IsActive = 1 AND f.ReferenceCode IN ('INDU_LAGNA','PRANAPADA_LAGNA'));
DECLARE @interp INT = (SELECT COUNT(*) FROM dbo.tbl_Content_Interpretation WHERE SubjectType = 'LIFE_MATTER_FOCUS');
PRINT '153 applied: ' + CAST(@matter AS VARCHAR(10)) + ' WEALTH_11 row (expect 1), '
    + CAST(@backfill AS VARCHAR(10)) + ' backfilled LifeMatterId (expect 1), '
    + CAST(@focus AS VARCHAR(10)) + ' Indu/Pranapada Focus rows (expect 3), '
    + CAST(@interp AS VARCHAR(10)) + ' LIFE_MATTER_FOCUS interpretation rows (expect 3).';
GO
