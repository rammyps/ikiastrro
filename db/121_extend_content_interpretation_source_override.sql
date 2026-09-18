-- =====================================================================
-- 121 — tbl_Content_Interpretation: allow a source-specific override row
-- alongside the generic row, same pattern as tbl_Rule_Yoga (119).
--
-- db/080_create_content_interpretation.sql's UNIQUE (RuleSetId, SubjectType,
-- SubjectCode) leaves no room for both a Raman-specific and a PVR-specific
-- interpretation of the same SubjectCode (e.g. YOGA_BUDHA_ADITYA, whose two
-- sources genuinely mean different things per 119). Replace it with two
-- filtered unique indexes: one for the generic (SourceRefCode IS NULL) row,
-- one for source-specific override rows — same shape as tbl_Rule_Yoga's
-- UQ_Rule_Yoga_RuleSet_YogaCode / UQ_Rule_Yoga_RuleSet_YogaCode_Source.
--
-- Also: db/080's own header comment claims
-- src/Ikiastrro.Web/Components/Shared/InterpretationText.razor already
-- exists as this table's reader. It did not — that comment was aspirational
-- (decision 003, Part B, was never finished). The UI counterpart landing
-- alongside this migration is what actually builds it. 080 itself is an
-- applied migration and is not edited (db/README.md: "Never edit an applied
-- migration. Add a corrective NN+1 script instead.") — this comment is the
-- correction.
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '121_extend_content_interpretation_source_override.sql')
BEGIN
    IF OBJECT_ID('dbo.UQ_Content_Interpretation', 'UQ') IS NOT NULL
        ALTER TABLE dbo.tbl_Content_Interpretation DROP CONSTRAINT UQ_Content_Interpretation;

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_Content_Interpretation_RuleSet_Subject' AND object_id = OBJECT_ID('dbo.tbl_Content_Interpretation'))
        CREATE UNIQUE INDEX UQ_Content_Interpretation_RuleSet_Subject ON dbo.tbl_Content_Interpretation (RuleSetId, SubjectType, SubjectCode)
            WHERE SourceRefCode IS NULL; -- one canonical row per subject

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_Content_Interpretation_RuleSet_Subject_Source' AND object_id = OBJECT_ID('dbo.tbl_Content_Interpretation'))
        CREATE UNIQUE INDEX UQ_Content_Interpretation_RuleSet_Subject_Source ON dbo.tbl_Content_Interpretation (RuleSetId, SubjectType, SubjectCode, SourceRefCode)
            WHERE SourceRefCode IS NOT NULL; -- one row per subject+source override

    INSERT dbo.SchemaMigrations (ScriptName, Note)
    VALUES ('121_extend_content_interpretation_source_override.sql',
        'tbl_Content_Interpretation: filtered indexes now allow one row per SourceRefCode override per subject, same pattern as tbl_Rule_Yoga (119).');
END
GO

PRINT '121 applied: tbl_Content_Interpretation supports source-specific override rows.';
GO
