-- =====================================================================
-- 144 - New LifeMatters category: SELF_IDENTITY. 5 sub-questions framing
-- pure self-identity through the classical perception axis (rammyps,
-- 2026-09-25), none from PVR's original 96-row worksheet (migration 087) -
-- a new category the user is defining, not a gap-fill in existing data:
--
--   1. "Who am I"                          -> D1 Lagna
--   2. "What I think of myself"            -> D9 Lagna
--   3. "What I think the world thinks of me" -> Arudha Lagna (AL)
--   4. "What I think of the world"         -> Graha Arudha of the Atmakaraka
--   5. "How my power is got"               -> Ghati Lagna (GL)
--
-- Citations (BasisCode per row, same discipline as migration 087):
--   Row 1 (PVR_CROSSVALIDATED): D1 Lagna as base self/identity is the
--     universal Parashari/Jaimini starting point - same basis level as
--     SELF_HEALTH's own D1-Lagna rows ("Physical self and constitution").
--   Row 2 (PVR_CROSSVALIDATED): reuses this project's own
--     OVERALL_STRENGTH_DHARMA definition verbatim (tbl_Dim_DivisionalSubject,
--     db/38): "Lagna, Lagna lord, Sun, 9th house... planetary maturity,
--     inner strength, dharma and the deeper expression of the D1 promise" -
--     D9's own Lagna is the single-point read of that definition, and that
--     Subject already auto-switches LifeMatters to D9.
--   Row 3 (PVR_DIRECT): PVR sec 7.3.4, already cited in this project's own
--     tbl_Dim_Terminology row HREF_ARUDHA_LAGNA (db/32): "The pada of the
--     lagna used as the reference point - how the native is perceived in
--     the world; image and material status."
--   Row 4 (PROJECT_SYNTHESIS): no PVR citation in this project assigns a
--     "how I see the world" signification to any specific graha's own
--     arudha - only the sec 9.5 computation method is cited
--     (GrahaArudhaCalculator.cs), not a per-planet meaning. Proposed as the
--     natural complement to Arudha Lagna (the ascendant's own pada): the
--     Atmakaraka's Graha Arudha, the self-significator's own projected
--     image. Flagged here for correction against a firmer citation if the
--     user has one - not silently guessed.
--   Row 5 (PVR_DIRECT): PVR sec 5.4/5.6, already cited in
--     GhatiLagnaCalculator.cs: "self with respect to fame, power and
--     authority."
--
-- Engine support already in place (verified this session, zero new code
-- needed for rows 1/2/3/5):
--   - ReferenceCode 'LAGNA' (rows 1/2) and 'GHATI_LAGNA' (row 5) are both
--     already-valid tbl_Dim_HouseReference rows (db/32) and already wired
--     end-to-end in LifeMatters.razor's ResolveReferenceSign.
--   - SpecialPointCode 'AL' (row 3) is already accepted by
--     LifeMatterFocusResolver.IsCanonicalSpecialPointCode and already
--     renders on the grid (SpecialPointsBySign picks up PointKind=='Arudha').
--   - Row 2's Subject reuses OVERALL_STRENGTH_DHARMA (no new
--     tbl_Dim_DivisionalSubject row).
-- Row 4 needed two small code changes, made alongside this migration:
--   - LifeMatterFocusResolver.IsCanonicalSpecialPointCode now accepts the
--     dynamic sentinel "GA_AK" (resolved per-person at render time to
--     whichever planet is that chart's own Atmakaraka).
--   - LifeMatters.razor's SpecialPointsBySign resolves "GA_AK" to that
--     planet's actual GA_<Planet> point and surfaces it on the grid.
--
-- Rows 1/3/4/5 get no tbl_Rule_LifeMatterSubject row (NO MATCH, same
-- pattern as most of SELF_HEALTH) - Auto mode correctly stays on whichever
-- chart is already shown, the right behavior for pure-D1-frame concepts.
--
-- tbl_Dim_LifeMatter's one-time PVR_LIFE_MATTER backfill (db/103) is
-- guarded to run only once and has already run - these 5 rows are inserted
-- directly here, using its same Code = CategoryCode + '_' + zero-padded
-- DisplayOrder convention, not by re-triggering migration 103.
--
-- Apply:  sqlcmd -S "localhost\SQLSERVER2025" -E -d ikiastrro -b -f 65001 -i db/144_seed_life_matter_self_identity.sql
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

-- --- Batch 1: widen tbl_Rule_LifeMatterReference's CategoryCode CHECK ---
IF EXISTS (
    SELECT 1 FROM sys.check_constraints cc
    WHERE cc.name = 'CK_Rule_LifeMatterReference_Category'
      AND OBJECT_DEFINITION(cc.object_id) NOT LIKE '%SELF_IDENTITY%'
)
BEGIN
    ALTER TABLE dbo.tbl_Rule_LifeMatterReference DROP CONSTRAINT CK_Rule_LifeMatterReference_Category;
    ALTER TABLE dbo.tbl_Rule_LifeMatterReference WITH CHECK ADD CONSTRAINT CK_Rule_LifeMatterReference_Category
        CHECK (CategoryCode IN (
            'SELF_HEALTH','WEALTH','EDUCATION','PROPERTY_COMFORTS','FAMILY_RELATIONSHIPS',
            'MARRIAGE_SPOUSE','CHILDREN','CAREER_STATUS','SPIRITUALITY','TROUBLE_LOSS','SELF_IDENTITY'));
END
GO

-- --- Batch 2: tbl_Rule_LifeMatterReference rows ---
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_LifeMatterReference WHERE CategoryCode = 'SELF_IDENTITY')
BEGIN
    ;WITH m (CategoryCode, CategoryName, DisplayOrder, MatterText, PrimaryChartsText,
             HouseFromLagnaText, KarakaText, HouseFromKarakaText, BasisCode, Narrative) AS (
        SELECT * FROM (VALUES

        ('SELF_IDENTITY', N'Self identity and perception', 1, N'Who am I', 'D1',
            '1st', N'Lagna', N'1st house of D1 (Lagna itself)', 'PVR_CROSSVALIDATED',
            N'D1 Lagna as the base self/identity is the universal Parashari/Jaimini starting point - same basis level already used for SELF_HEALTH''s own D1-Lagna rows ("Physical self and constitution", "Mind").'),

        ('SELF_IDENTITY', N'Self identity and perception', 2, N'What I think of myself', 'D9',
            '1st', N'Lagna', N'1st house of D9 (Navamsa Lagna)', 'PVR_CROSSVALIDATED',
            N'Reuses this project''s own OVERALL_STRENGTH_DHARMA definition verbatim (tbl_Dim_DivisionalSubject, db/38): "Lagna, Lagna lord, Sun, 9th house... planetary maturity, inner strength, dharma and the deeper expression of the D1 promise" - D9''s own Lagna is the single-point read of that definition.'),

        ('SELF_IDENTITY', N'Self identity and perception', 3, N'What I think the world thinks of me', 'D1',
            'Arudha Lagna', N'Lagna lord', N'Arudha Lagna (A1) - the pada of the Lagna', 'PVR_DIRECT',
            N'PVR sec 7.3.4, already cited in this project''s tbl_Dim_Terminology row HREF_ARUDHA_LAGNA (db/32): "The pada of the lagna used as the reference point - how the native is perceived in the world; image and material status."'),

        ('SELF_IDENTITY', N'Self identity and perception', 4, N'What I think of the world', 'D1',
            'Graha Arudha of AK', N'Atmakaraka', N'Graha Arudha (GA) of the person''s own Atmakaraka', 'PROJECT_SYNTHESIS',
            N'No PVR citation in this project assigns a "how I see the world" signification to a specific graha''s own arudha - only the sec 9.5 computation method is cited. Proposed as the natural complement to Arudha Lagna: the Atmakaraka''s own Graha Arudha, the self-significator''s projected image. Flagged for correction against a firmer PVR citation if one exists.'),

        ('SELF_IDENTITY', N'Self identity and perception', 5, N'How my power is got', 'D1',
            'Ghati Lagna', N'Ghati Lagna', N'Ghati Lagna itself', 'PVR_DIRECT',
            N'PVR sec 5.4/5.6, already cited in GhatiLagnaCalculator.cs: "self with respect to fame, power and authority; weighed heavily when timing periods for a politician."')

        ) v (CategoryCode, CategoryName, DisplayOrder, MatterText, PrimaryChartsText,
             HouseFromLagnaText, KarakaText, HouseFromKarakaText, BasisCode, Narrative)
    )
    INSERT dbo.tbl_Rule_LifeMatterReference
        (RuleSetId, CategoryCode, CategoryName, DisplayOrder, MatterText, PrimaryChartsText,
         PrimaryLifeAreaId, HouseFromLagnaText, KarakaText, NaisargikaGrahaId, CharaKarakaCode,
         HouseFromKarakaText, BasisCode, CalculationNarrative, SourceRefCode, IsActive)
    SELECT 1, m.CategoryCode, m.CategoryName, m.DisplayOrder, m.MatterText, m.PrimaryChartsText,
           NULL, m.HouseFromLagnaText, m.KarakaText, NULL, NULL,
           m.HouseFromKarakaText, m.BasisCode, m.Narrative,
           CASE WHEN m.BasisCode = 'PROJECT_SYNTHESIS' THEN 'SRC_IKIASTRRO_SYNTHESIS' ELSE 'SRC_PVR_INTEGRATED' END,
           1
    FROM m;
END
GO

-- --- Batch 3: tbl_Dim_LifeMatter rows (db/103's one-time backfill already ran; insert directly) ---
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Dim_LifeMatter WHERE CategoryCode = 'SELF_IDENTITY')
    INSERT dbo.tbl_Dim_LifeMatter (Code, EnglishName, CategoryCode, CategoryName, SourceGroup, DisplayOrder)
    SELECT lm.CategoryCode + '_' + RIGHT('0' + CAST(lm.DisplayOrder AS VARCHAR(2)), 2),
           lm.MatterText, lm.CategoryCode, lm.CategoryName, 'PVR_LIFE_MATTER', lm.DisplayOrder
    FROM dbo.tbl_Rule_LifeMatterReference lm
    WHERE lm.CategoryCode = 'SELF_IDENTITY';
GO

-- --- Batch 4: tbl_Rule_LifeMatterSubject - row 2 (D9-Lagna) reuses OVERALL_STRENGTH_DHARMA ---
IF NOT EXISTS (
    SELECT 1 FROM dbo.tbl_Rule_LifeMatterSubject s
    JOIN dbo.tbl_Dim_LifeMatter lm ON lm.Id = s.LifeMatterId
    WHERE lm.Code = 'SELF_IDENTITY_02'
)
    INSERT dbo.tbl_Rule_LifeMatterSubject (RuleSetId, LifeMatterId, DivisionalSubjectCode, SourceRefCode)
    SELECT 1, lm.Id, 'OVERALL_STRENGTH_DHARMA', 'SRC_IKIASTRRO_SYNTHESIS'
    FROM dbo.tbl_Dim_LifeMatter lm
    WHERE lm.Code = 'SELF_IDENTITY_02';
GO

-- --- Batch 5: tbl_Rule_LifeMatterFocus ---
IF NOT EXISTS (
    SELECT 1 FROM dbo.tbl_Rule_LifeMatterFocus f
    JOIN dbo.tbl_Dim_LifeMatter lm ON lm.Id = f.LifeMatterId
    WHERE lm.CategoryCode = 'SELF_IDENTITY'
)
BEGIN
    INSERT dbo.tbl_Rule_LifeMatterFocus (RuleSetId, LifeMatterId, FocusKind, ReferenceCode, HouseNumber, SpecialPointCode, SourceRefCode)
    SELECT 1, lm.Id, 'House', 'LAGNA', 1, NULL, lm2.SourceRefCode
    FROM dbo.tbl_Dim_LifeMatter lm
    JOIN dbo.tbl_Rule_LifeMatterReference lm2 ON lm2.CategoryCode = 'SELF_IDENTITY' AND lm2.DisplayOrder = 1
    WHERE lm.Code = 'SELF_IDENTITY_01';

    INSERT dbo.tbl_Rule_LifeMatterFocus (RuleSetId, LifeMatterId, FocusKind, ReferenceCode, HouseNumber, SpecialPointCode, SourceRefCode)
    SELECT 1, lm.Id, 'House', 'LAGNA', 1, NULL, lm2.SourceRefCode
    FROM dbo.tbl_Dim_LifeMatter lm
    JOIN dbo.tbl_Rule_LifeMatterReference lm2 ON lm2.CategoryCode = 'SELF_IDENTITY' AND lm2.DisplayOrder = 2
    WHERE lm.Code = 'SELF_IDENTITY_02';

    INSERT dbo.tbl_Rule_LifeMatterFocus (RuleSetId, LifeMatterId, FocusKind, ReferenceCode, HouseNumber, SpecialPointCode, SourceRefCode)
    SELECT 1, lm.Id, 'SpecialPoint', NULL, NULL, 'AL', lm2.SourceRefCode
    FROM dbo.tbl_Dim_LifeMatter lm
    JOIN dbo.tbl_Rule_LifeMatterReference lm2 ON lm2.CategoryCode = 'SELF_IDENTITY' AND lm2.DisplayOrder = 3
    WHERE lm.Code = 'SELF_IDENTITY_03';

    INSERT dbo.tbl_Rule_LifeMatterFocus (RuleSetId, LifeMatterId, FocusKind, ReferenceCode, HouseNumber, SpecialPointCode, SourceRefCode)
    SELECT 1, lm.Id, 'SpecialPoint', NULL, NULL, 'GA_AK', lm2.SourceRefCode
    FROM dbo.tbl_Dim_LifeMatter lm
    JOIN dbo.tbl_Rule_LifeMatterReference lm2 ON lm2.CategoryCode = 'SELF_IDENTITY' AND lm2.DisplayOrder = 4
    WHERE lm.Code = 'SELF_IDENTITY_04';

    INSERT dbo.tbl_Rule_LifeMatterFocus (RuleSetId, LifeMatterId, FocusKind, ReferenceCode, HouseNumber, SpecialPointCode, SourceRefCode)
    SELECT 1, lm.Id, 'House', 'GHATI_LAGNA', 1, NULL, lm2.SourceRefCode
    FROM dbo.tbl_Dim_LifeMatter lm
    JOIN dbo.tbl_Rule_LifeMatterReference lm2 ON lm2.CategoryCode = 'SELF_IDENTITY' AND lm2.DisplayOrder = 5
    WHERE lm.Code = 'SELF_IDENTITY_05';
END
GO

-- --- Batch 6: ledger + summary ---
INSERT dbo.SchemaMigrations (ScriptName, Note)
SELECT '144_seed_life_matter_self_identity.sql',
       'Adds LifeMatters SELF_IDENTITY category (5 rows: D1-Lagna/D9-Lagna/Arudha-Lagna/Graha-Arudha-of-AK/Ghati-Lagna), widens CategoryCode CHECK, reuses OVERALL_STRENGTH_DHARMA as row 2''s Subject.'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '144_seed_life_matter_self_identity.sql');
GO

DECLARE @refRows INT = (SELECT COUNT(*) FROM dbo.tbl_Rule_LifeMatterReference WHERE CategoryCode = 'SELF_IDENTITY');
DECLARE @dimRows INT = (SELECT COUNT(*) FROM dbo.tbl_Dim_LifeMatter WHERE CategoryCode = 'SELF_IDENTITY');
DECLARE @subjRows INT = (
    SELECT COUNT(*) FROM dbo.tbl_Rule_LifeMatterSubject s
    JOIN dbo.tbl_Dim_LifeMatter lm ON lm.Id = s.LifeMatterId WHERE lm.CategoryCode = 'SELF_IDENTITY'
);
DECLARE @focusRows INT = (
    SELECT COUNT(*) FROM dbo.tbl_Rule_LifeMatterFocus f
    JOIN dbo.tbl_Dim_LifeMatter lm ON lm.Id = f.LifeMatterId WHERE lm.CategoryCode = 'SELF_IDENTITY'
);
PRINT '144 applied: SELF_IDENTITY reference rows=' + CAST(@refRows AS VARCHAR(10)) + ' (expected 5), '
    + 'dim rows=' + CAST(@dimRows AS VARCHAR(10)) + ' (expected 5), '
    + 'subject rows=' + CAST(@subjRows AS VARCHAR(10)) + ' (expected 1), '
    + 'focus rows=' + CAST(@focusRows AS VARCHAR(10)) + ' (expected 5).';
GO
