-- =====================================================================
-- 084 — Register the rule tables added by migrations 081 and 083 in
--       tbl_Rule_Catalog, so verify-rules' "catalog covers every
--       tbl_Rule_* table" gate passes (a leftover gap from those two
--       turns: the table was created but never registered).
--
--   081: tbl_Rule_PanchangaFormula.
--   083: tbl_Rule_PostureStateFormula.
-- =====================================================================
USE [ikiastrro];
GO

IF OBJECT_ID('dbo.tbl_Rule_Catalog', 'U') IS NULL
    THROW 50084, 'Migration 084 requires dbo.tbl_Rule_Catalog.', 1;

INSERT dbo.tbl_Rule_Catalog
    (RuleTableName, EngineCode, MethodCodes, Purpose, IntroducedIn)
SELECT v.RuleTableName, v.EngineCode, v.MethodCodes, v.Purpose, v.IntroducedIn
FROM (VALUES
    ('tbl_Rule_PanchangaFormula', 'PANCHANGA', 'TITHI,KARANA,NITYA_YOGA,VEDIC_WEEKDAY,HORA_LORD',
     'Tithi/Karana/Nitya Yoga/Vedic weekday/Hora Lord formula narratives, one row per concept.',
     '081 (panchanga schema)'),
    ('tbl_Rule_PostureStateFormula', 'AVASTHA', 'SAYANADI_INDEX',
     'Sayanaadi (PostureState) 12-state index formula: ((C*P*A)+M+G+L) mod 12, remainder 0 -> 12.',
     '083 (sayanadi avastha)')
) v (RuleTableName, EngineCode, MethodCodes, Purpose, IntroducedIn)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.tbl_Rule_Catalog c WHERE c.RuleTableName = v.RuleTableName
);
GO

INSERT dbo.SchemaMigrations (ScriptName, Note)
SELECT '084_register_panchanga_posture_rule_catalog.sql',
       'Register tbl_Rule_PanchangaFormula (081) and tbl_Rule_PostureStateFormula (083) in tbl_Rule_Catalog.'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '084_register_panchanga_posture_rule_catalog.sql');
GO

PRINT '084 applied: tbl_Rule_Catalog coverage restored (Panchanga + PostureState).';
GO
