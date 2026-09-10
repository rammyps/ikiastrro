-- =====================================================================
-- 076 — Register the rule tables added by migrations 070, 072 and 074-075
--       in tbl_Rule_Catalog, so verify-rules' "catalog covers every
--       tbl_Rule_* table" gate passes.
--
--   070 (research schema, pre-existing gap): SourceReferenceAshtakavarga
--        Method / Contributor / Reduction.
--   072: tbl_Rule_PlanetaryWar.
--   074/075: tbl_Rule_AshtakavargaContribution / Reduction.
--
-- tbl_Rule_ShadbalaMinimumRupas is also registered here (added by 071).
-- verify-rules matches by bare table name, so the research.* tables are
-- listed by name only.
-- =====================================================================
USE [ikiastrro];
GO

IF OBJECT_ID('dbo.tbl_Rule_Catalog', 'U') IS NULL
    THROW 50076, 'Migration 076 requires dbo.tbl_Rule_Catalog.', 1;

INSERT dbo.tbl_Rule_Catalog
    (RuleTableName, EngineCode, MethodCodes, Purpose, IntroducedIn)
SELECT v.RuleTableName, v.EngineCode, v.MethodCodes, v.Purpose, v.IntroducedIn
FROM (VALUES
    ('tbl_Rule_ShadbalaMinimumRupas', 'STRENGTH', 'MINIMUM_REQUIRED_RUPAS',
     'Per-planet minimum required Shadbala (rupas) for the PercentOfMinimum ratio.',
     '071 (shadbala min-rupa)'),
    ('tbl_Rule_PlanetaryWar', 'STRENGTH', 'GRAHA_YUDDHA',
     'Planetary-war orb, winner criterion and Shadbala adjustment method (Yuddha Bala).',
     '072 (planetary war)'),
    ('tbl_Rule_AshtakavargaContribution', 'ASHTAKAVARGA', 'PVR_PARASARA_BAV',
     'Parasari benefic-places (bindu) matrix: 7 recipients x 8 contributors, SAV total 337.',
     '074 (ashtakavarga schema)'),
    ('tbl_Rule_AshtakavargaReduction', 'ASHTAKAVARGA', 'TRIKONA_SODHANA,EKADHIPATYA_SODHANA',
     'Trikona and Ekadhipatya Sodhana reduction algorithms for the Sodhya Pinda pipeline.',
     '074 (ashtakavarga schema)'),
    ('tbl_Rule_SourceReferenceAshtakavargaMethod', 'ASHTAKAVARGA', 'RESEARCH_REFERENCE',
     'Research-only: canonical Ashtakavarga method identity and scope.',
     '070 (research ashtakavarga)'),
    ('tbl_Rule_SourceReferenceAshtakavargaContributor', 'ASHTAKAVARGA', 'RESEARCH_REFERENCE',
     'Research-only: per-source contributor benefic-place rule rows pending verification.',
     '070 (research ashtakavarga)'),
    ('tbl_Rule_SourceReferenceAshtakavargaReduction', 'ASHTAKAVARGA', 'RESEARCH_REFERENCE',
     'Research-only: per-source reduction-step rule rows pending verification.',
     '070 (research ashtakavarga)')
) v (RuleTableName, EngineCode, MethodCodes, Purpose, IntroducedIn)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.tbl_Rule_Catalog c WHERE c.RuleTableName = v.RuleTableName
);
GO

INSERT dbo.SchemaMigrations (ScriptName, Note)
SELECT '076_register_strength_ashtakavarga_rule_catalog.sql',
       'Register the strength (min-rupa, planetary-war) and Ashtakavarga (production + research) rule tables in tbl_Rule_Catalog.'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '076_register_strength_ashtakavarga_rule_catalog.sql');
GO

PRINT '076 applied: tbl_Rule_Catalog coverage restored.';
GO
