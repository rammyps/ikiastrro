-- 140 — Historical no-op marker.
-- The attempted materialization was rejected atomically by CK_Rule_Yoga_FormationFamily
-- (`DISTRIBUTION` is not an allowed family). The legacy script then recorded this ledger row.
-- No rule data was changed. Migration 142 records the correct concept/variant boundary.
USE [ikiastrro];
GO
IF NOT EXISTS(SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName='140_materialize_pvr_yoga_rule_variants.sql')
    INSERT dbo.SchemaMigrations(ScriptName,Note) VALUES
    ('140_materialize_pvr_yoga_rule_variants.sql','Historical no-op: rejected variant materialization; see migration 142.');
GO
