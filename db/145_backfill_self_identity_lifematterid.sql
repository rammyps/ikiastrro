-- =====================================================================
-- 145 - Fix a gap in migration 144: the 5 new SELF_IDENTITY rows inserted
-- into tbl_Rule_LifeMatterReference never got LifeMatterId populated.
--
-- Migration 103 added tbl_Rule_LifeMatterReference.LifeMatterId (nullable
-- FK to tbl_Dim_LifeMatter) and backfilled it once for the original 96
-- rows via a one-time UPDATE - that UPDATE does not re-run for rows
-- inserted later. LifeMatterReferenceRepository.GetSteps() INNER JOINs on
-- this column, so the 5 SELF_IDENTITY rows (LifeMatterId IS NULL) were
-- silently excluded from the picker - confirmed live: selecting "SELF
-- IDENTITY AND PERCEPTION" showed zero Steps.
--
-- Never edit an applied migration (144) - correct forward instead, same
-- policy as migration 138. Re-runs migration 103's exact backfill logic
-- (safe/idempotent: WHERE LifeMatterId IS NULL), not a new mechanism.
--
-- Apply:  sqlcmd -S "localhost\SQLSERVER2025" -E -d ikiastrro -b -f 65001 -i db/145_backfill_self_identity_lifematterid.sql
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

UPDATE lm
SET LifeMatterId = dm.Id
FROM dbo.tbl_Rule_LifeMatterReference lm
JOIN dbo.tbl_Dim_LifeMatter dm ON dm.SourceGroup = 'PVR_LIFE_MATTER'
    AND dm.Code = lm.CategoryCode + '_' + RIGHT('0' + CAST(lm.DisplayOrder AS VARCHAR(2)), 2)
WHERE lm.LifeMatterId IS NULL;
GO

INSERT dbo.SchemaMigrations (ScriptName, Note)
SELECT '145_backfill_self_identity_lifematterid.sql',
       'Backfills tbl_Rule_LifeMatterReference.LifeMatterId for the 5 SELF_IDENTITY rows added in 144 - GetSteps() INNER JOINs on it, so they were invisible in the picker until this ran.'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '145_backfill_self_identity_lifematterid.sql');
GO

DECLARE @unlinked INT = (SELECT COUNT(*) FROM dbo.tbl_Rule_LifeMatterReference WHERE LifeMatterId IS NULL);
DECLARE @selfIdentityLinked INT = (SELECT COUNT(*) FROM dbo.tbl_Rule_LifeMatterReference WHERE CategoryCode = 'SELF_IDENTITY' AND LifeMatterId IS NOT NULL);
PRINT '145 applied: SELF_IDENTITY rows now linked=' + CAST(@selfIdentityLinked AS VARCHAR(10)) + ' (expected 5); '
    + 'total unlinked rows remaining across the whole table=' + CAST(@unlinked AS VARCHAR(10)) + ' (expected 0).';
GO
