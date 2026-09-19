-- =====================================================================
-- 129 - tbl_Rule_Argala.SignificanceNote: PVR's own gloss on what each
-- primary/secondary argala position signifies (sec.10.7 "Use of Argala",
-- pp.106-107, SRC_PVR_INTEGRATED, raw extract lines 4131-4157) — distinct
-- from CalculationNarrative, which elsewhere in this schema documents HOW
-- a value is derived, not the classical interpretive meaning of a position.
--
-- Populated for the 4 ARGALA rows only (2nd/4th/11th/5th). PVR gives no
-- parallel per-house-of-origin meaning for the 4 VIRODHARGALA rows in this
-- section — it only says virodhargala obstructs the corresponding argala —
-- so those stay NULL rather than inventing text.
--
-- Apply:  sqlcmd -S "localhost\SQLSERVER2025" -E -d ikiastrro -b -i db/129_add_argala_significance_note.sql
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- dev-only cleanup: an earlier, never-committed pass of this migration added the column under
-- the working name MeaningText before it was renamed to SignificanceNote.
IF COL_LENGTH('dbo.tbl_Rule_Argala', 'MeaningText') IS NOT NULL
    EXEC sp_rename 'dbo.tbl_Rule_Argala.MeaningText', 'SignificanceNote', 'COLUMN';
GO

IF COL_LENGTH('dbo.tbl_Rule_Argala', 'SignificanceNote') IS NULL
    ALTER TABLE dbo.tbl_Rule_Argala ADD SignificanceNote NVARCHAR(500) NULL;
GO

UPDATE dbo.tbl_Rule_Argala
SET SignificanceNote = N'Shows the basic ingredient for the sustenance of a matter. E.g. the 2nd house shows food, a basic ingredient for the sustenance of self (1st); the 5th house shows intelligence, a basic ingredient for the sustenance of learning (4th).'
WHERE RelationTypeCode = 'ARGALA' AND HouseOffset = 2;

UPDATE dbo.tbl_Rule_Argala
SET SignificanceNote = N'Shows the basic factor that drives the mood, state and progress of a matter. E.g. the 4th house shows comfort and drives the mood and state of self (1st); the 7th house shows interaction and drives one''s learning (4th).'
WHERE RelationTypeCode = 'ARGALA' AND HouseOffset = 4;

UPDATE dbo.tbl_Rule_Argala
SET SignificanceNote = N'Shows the catalyst that can result in gains for a matter. E.g. the 2nd house shows character, grooming and samskara, a catalyst in the process of learning (4th).'
WHERE RelationTypeCode = 'ARGALA' AND HouseOffset = 11;

UPDATE dbo.tbl_Rule_Argala
SET SignificanceNote = N'Secondary argala - shows additional contributing factors. E.g. the 5th house shows emotional situation, contributing to the state of self (1st); the 8th house shows hard work, contributing to one''s learning (4th).'
WHERE RelationTypeCode = 'ARGALA' AND HouseOffset = 5;
GO

INSERT dbo.SchemaMigrations (ScriptName, Note)
SELECT '129_add_argala_significance_note.sql',
       'tbl_Rule_Argala.SignificanceNote added; populated for the 4 ARGALA rows (sec.10.7, pp.106-107)'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '129_add_argala_significance_note.sql');
GO

PRINT '129 applied: tbl_Rule_Argala.SignificanceNote added and populated.';
GO
