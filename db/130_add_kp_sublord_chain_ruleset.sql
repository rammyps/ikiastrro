-- =====================================================================
-- 130 -- tbl_Fact_KpSubLordChain.RuleSetId: the one tbl_Fact_* table
-- without it (migration 095) -- every sibling (tbl_Fact_PlanetaryStrength,
-- tbl_Fact_Vargottama, tbl_Fact_Argala) records which rule set produced
-- its rows so a future revision to the KP cycle-order convention could be
-- traced; this table couldn't say. FEAT-DATA-07, docs/research/domain/
-- nakshatra-lord-sublord-dasha-crossref.md SS7.
--
-- Table is not empty (162 dev rows across 3 charts as of 2026-09-22), so
-- this backfills existing rows to the current sole active rule set
-- (Id=1, 'Parashari-Classical') before promoting the column to NOT NULL --
-- same three-step add/backfill/tighten shape used whenever a NOT NULL
-- column lands on a populated table in this schema.
--
-- Apply:  sqlcmd -S "localhost\SQLSERVER2025" -E -d ikiastrro -b -i db/130_add_kp_sublord_chain_ruleset.sql
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF COL_LENGTH('dbo.tbl_Fact_KpSubLordChain', 'RuleSetId') IS NULL
    ALTER TABLE dbo.tbl_Fact_KpSubLordChain ADD RuleSetId TINYINT NULL;
GO

UPDATE dbo.tbl_Fact_KpSubLordChain
SET RuleSetId = 1
WHERE RuleSetId IS NULL;
GO

IF EXISTS (
    SELECT 1 FROM dbo.tbl_Fact_KpSubLordChain WHERE RuleSetId IS NULL
)
    RAISERROR('130: at least one tbl_Fact_KpSubLordChain row still has a NULL RuleSetId after backfill.', 16, 1);
GO

IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.tbl_Fact_KpSubLordChain') AND name = 'RuleSetId' AND is_nullable = 1
)
    ALTER TABLE dbo.tbl_Fact_KpSubLordChain ALTER COLUMN RuleSetId TINYINT NOT NULL;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Fact_KpSubLordChain_RuleSet'
)
    ALTER TABLE dbo.tbl_Fact_KpSubLordChain
        ADD CONSTRAINT FK_Fact_KpSubLordChain_RuleSet FOREIGN KEY (RuleSetId) REFERENCES dbo.tbl_Rule_Sets (Id);
GO

IF OBJECT_ID(N'dbo.SchemaMigrations', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = N'130_add_kp_sublord_chain_ruleset.sql')
    INSERT dbo.SchemaMigrations (ScriptName, Note)
    VALUES (N'130_add_kp_sublord_chain_ruleset.sql', N'Add tbl_Fact_KpSubLordChain.RuleSetId (NOT NULL, FK tbl_Rule_Sets), backfilled to RuleSetId=1 for existing rows.');
GO

PRINT '130 applied: tbl_Fact_KpSubLordChain.RuleSetId added, backfilled, and constrained NOT NULL.';
GO
