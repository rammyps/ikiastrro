-- =====================================================================
-- 108 — CK_KeyDetails_PointKind gains 'GrahaArudha'. Migration 107 wired
--       GrahaArudhaCalculator (PVR sec.9.5) into the pipeline emitting
--       PointKind = 'GrahaArudha' (see SpecialPointSeed.cs), but never
--       widened this constraint — every insert of a graha-arudha row has
--       been failing with a CK_KeyDetails_PointKind violation since.
--       Idempotent.
-- Apply:  sqlcmd -S localhost\SQLSERVER2025 -E -d ikiastrro -b -i db/108_add_grahaarudha_pointkind.sql
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID('dbo.CK_KeyDetails_PointKind', 'C') IS NOT NULL
    ALTER TABLE dbo.tbl_Chart_KeyDetails DROP CONSTRAINT CK_KeyDetails_PointKind;
ALTER TABLE dbo.tbl_Chart_KeyDetails WITH CHECK
    ADD CONSTRAINT CK_KeyDetails_PointKind
    CHECK (PointKind IN ('Graha', 'SpecialLagna', 'Arudha', 'Upagraha', 'GrahaArudha'));
GO

INSERT dbo.SchemaMigrations (ScriptName, Note)
SELECT '108_add_grahaarudha_pointkind.sql',
       'CK_KeyDetails_PointKind += GrahaArudha (GrahaArudhaCalculator, PVR sec.9.5)'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '108_add_grahaarudha_pointkind.sql');
GO

PRINT '108 applied: CK_KeyDetails_PointKind allows GrahaArudha.';
GO
