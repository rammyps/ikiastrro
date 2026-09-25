-- 141 — Historical no-op marker.
-- The corrective attempt was rejected atomically by UQ_Rule_Yoga_RuleSet_YogaCode_Source:
-- tbl_Rule_Yoga is one concept row per YogaCode/source, not one row per source variant.
-- No rule data was changed. Migration 142 adds the correct variant-to-concept view.
USE [ikiastrro];
GO
IF NOT EXISTS(SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName='141_correct_pvr_yoga_rule_materialization.sql')
    INSERT dbo.SchemaMigrations(ScriptName,Note) VALUES
    ('141_correct_pvr_yoga_rule_materialization.sql','Historical no-op: concept uniqueness rejected variant rows; see migration 142.');
GO
