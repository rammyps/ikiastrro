-- =====================================================================
-- 119 — Source-specific Rule override for tbl_Rule_Yoga (general mechanism)
-- + the Budha Aditya split that motivated it.
--
-- Bug: SourceAttributedYogaEngine.AddBudhaAditya
-- (src/Ikiastrro.Core/Engines/Yoga/SourceAttributedYogaEngine.cs) correctly
-- computes DIFFERENT outcomes per source for YOGA_BUDHA_ADITYA — Raman
-- requires Sun-Mercury separation >10 degrees, PVR requires only same-sign
-- conjunction (with a combustion caveat noted when separation <=14 degrees)
-- — but 079's single source-independent Rule row only states Raman's >10
-- degree qualification, next to BOTH sources' rows in the UI. That
-- misleadingly implies PVR also needs >10 degrees, when the PVR-sourced
-- predicate never checks that.
--
-- tbl_Rule_Yoga already has an unused SourceRefCode column (migration 47)
-- and 079's filtered unique index only covers the generic
-- (SourceRefCode IS NULL) case. This migration adds a second filtered
-- index so any YogaCode can carry a source-specific override row
-- alongside its generic row, then uses it for YOGA_BUDHA_ADITYA. The
-- generic row is kept (reworded, not deleted) as the fallback for any
-- future SourceRefCode that shows up against this YogaCode without its
-- own override row — vw_ChartYogaEvaluations below prefers a
-- source-specific match, falling back to the generic row otherwise, so
-- every other already-seeded YogaCode (no override row yet) is unaffected.
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '119_split_budha_aditya_source_specific_rules.sql')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_Rule_Yoga_RuleSet_YogaCode_Source' AND object_id = OBJECT_ID('dbo.tbl_Rule_Yoga'))
        CREATE UNIQUE INDEX UQ_Rule_Yoga_RuleSet_YogaCode_Source ON dbo.tbl_Rule_Yoga (RuleSetId, YogaCode, SourceRefCode)
            WHERE SourceRefCode IS NOT NULL; -- one row per (RuleSetId, YogaCode, Source) override; general, not Budha-Aditya-specific
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '119_split_budha_aditya_source_specific_rules.sql')
BEGIN
    UPDATE dbo.tbl_Rule_Yoga
        SET ShortFormationRule = 'Sun and Mercury conjunct - see source-specific rows for the >10 degree (Raman) vs same-sign (PVR) qualification'
    WHERE RuleSetId = 1 AND YogaCode = 'YOGA_BUDHA_ADITYA' AND SourceRefCode IS NULL;

    INSERT dbo.tbl_Rule_Yoga (RuleSetId, YogaCode, SourceRefCode, FormationFamilyCode, ShortFormationRule)
    SELECT 1, 'YOGA_BUDHA_ADITYA', 'SRC_RAMAN_300_COMBINATIONS', 'SUN',
           'Sun and Mercury conjunct, more than 10 degrees apart'
    WHERE NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_Yoga WHERE RuleSetId = 1 AND YogaCode = 'YOGA_BUDHA_ADITYA' AND SourceRefCode = 'SRC_RAMAN_300_COMBINATIONS');

    INSERT dbo.tbl_Rule_Yoga (RuleSetId, YogaCode, SourceRefCode, FormationFamilyCode, ShortFormationRule)
    SELECT 1, 'YOGA_BUDHA_ADITYA', 'SRC_PVR_INTEGRATED', 'SUN',
           'Sun and Mercury conjunct (same sign); combust and weakened if separation is 14 degrees or less'
    WHERE NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_Yoga WHERE RuleSetId = 1 AND YogaCode = 'YOGA_BUDHA_ADITYA' AND SourceRefCode = 'SRC_PVR_INTEGRATED');

    EXEC(N'
    CREATE OR ALTER VIEW dbo.vw_ChartYogaEvaluations
    AS
    SELECT c.BirthDetailId, b.Name, y.ChartResultId, y.RuleSetId,
           y.SourceRefCode, y.SourceVariantCode, y.YogaCode, y.SourceLocator,
           y.Present, y.EvaluationStatus, y.MissingRequirementCodesJson,
           y.SubjectSex, y.IsNightBirth, y.ElongationDegrees,
           y.IsWaxingMoon, y.IsFullMoon, y.LunarPhasePolicyCode,
           y.SunriseMethodCode, y.Notes, y.ComputedAtUtc,
           COALESCE(rSrc.FormationFamilyCode, rGen.FormationFamilyCode) AS YogaTypeCode,
           COALESCE(rSrc.ShortFormationRule, rGen.ShortFormationRule) AS YogaRule
    FROM dbo.tbl_Fact_YogaInputEvaluations y
    JOIN dbo.tbl_ChartResults c ON c.Id = y.ChartResultId
    JOIN dbo.tbl_BirthDetails b ON b.Id = c.BirthDetailId
    LEFT JOIN dbo.tbl_Rule_Yoga rSrc ON rSrc.YogaCode = y.YogaCode AND rSrc.RuleSetId = y.RuleSetId AND rSrc.SourceRefCode = y.SourceRefCode
    LEFT JOIN dbo.tbl_Rule_Yoga rGen ON rGen.YogaCode = y.YogaCode AND rGen.RuleSetId = y.RuleSetId AND rGen.SourceRefCode IS NULL;
    ');

    INSERT dbo.SchemaMigrations (ScriptName, Note)
    VALUES ('119_split_budha_aditya_source_specific_rules.sql',
        'tbl_Rule_Yoga: general filtered index for source-specific Rule rows; YOGA_BUDHA_ADITYA split into Raman (>10 deg) / PVR (same-sign) rows; view prefers source-specific row when present.');
END
GO

PRINT '119 applied: Budha Aditya Rule text is source-specific; any YogaCode can now carry a source override.';
GO
