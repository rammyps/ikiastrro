-- =====================================================================
-- 105 - tvf_Chart_SignNakshatraRasiRelationship(@ChartResultId): connects
-- tbl_Rule_SignNakshatra (243 rows, KP levels 1-2) to all 12 Rasis, not
-- just each Nakshatra's own primary Rasi (which vw_Rule_SignNakshatraRelationship,
-- migration 097, already exposes - natural-relationship-only, 3-tier).
--
-- This computes the full five-tier Pancadha Maitri (Great Friend..Great
-- Enemy) for both the Nakshatra-lord (KP level 1) and the Sub-lord (KP
-- level 2) against every Rasi's lord, AS ACTUALLY PLACED in the given
-- chart. Pancadha Maitri is chart-specific by classical definition -
-- temporary friendship needs a live sign-distance between two placed
-- planets (docs/research/domain/dignity-pvr.md, PVR's own method - no
-- context-free convention exists). tbl_Rule_NaturalRelationship +
-- tbl_Rule_TemporaryFriendshipDistance + tbl_Rule_CompoundRelationship
-- already encode the identical 2x3 combination formula
-- DignityEngine.CombineToPanchadha hardcodes in C# (verified byte-for-byte
-- equal by direct query) - this is the first genuinely DB-driven
-- implementation of that formula, closing part of the "Phase 2" gap
-- db/24_add_rule_compound_relationship.sql's own header flagged (that
-- gap otherwise remains open - DignityEngine itself is untouched, it is
-- cli-workstream code).
--
-- tbl_Chart_KeyDetails.PlanetId/SignId (integer FK columns) are used for
-- the join, not the Sign varchar column - tbl_Chart_KeyDetails.Sign
-- stores 'Capricornus' while tbl_SignAttributes.SignName stores
-- 'Capricorn'; a string join would silently drop every Capricorn row.
--
-- Rahu/Ketu as a Nakshatra-lord or Sub-lord: tbl_Rule_NaturalRelationship
-- only covers the 7 classical grahas (42 = 7x6 rows), so any row whose
-- lord is Rahu/Ketu returns NULL relationship/compound columns. This is
-- not a bug - it mirrors the same still-open gap already flagged in
-- docs/research/domain/dignity-pvr.md ("Panchadha Maitri tiers for a
-- node are still out of scope").
--
-- Grain: 243 x 12 = 2,916 rows per call, computed on demand (matches the
-- tvf_Chart_SadeSatiPeriods/tvf_PlanetSignAtDate inline-TVF pattern) -
-- not persisted, no new Fact-table storage growth.
--
-- Apply:  sqlcmd -S "localhost\SQLSERVER2025" -E -d ikiastrro -b -i db/105_create_tvf_sign_nakshatra_rasi_relationship.sql
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER FUNCTION dbo.tvf_Chart_SignNakshatraRasiRelationship(@ChartResultId INT)
RETURNS TABLE
AS
RETURN (
    SELECT
        sn.NakshatraId, nk.NakshatraName, sn.SubSequenceNumber,
        sn.NakshatraLordPlanetId, nlp.PlanetName AS NakshatraLordName,
        sn.SubLordPlanetId, slp.PlanetName AS SubLordName,
        rasi.Id AS RasiId, rasi.SignName AS RasiName,
        rasi.RulingPlanetId AS RasiLordPlanetId, rlp.PlanetName AS RasiLordName,
        CASE WHEN sn.NakshatraLordPlanetId = rasi.RulingPlanetId THEN 'Same' ELSE natNL.RelationshipType END AS NakshatraLord_Natural,
        CASE WHEN sn.NakshatraLordPlanetId = rasi.RulingPlanetId THEN 'ADHIMITRA' ELSE crNL.CompoundCode END AS NakshatraLord_Compound,
        CASE WHEN sn.NakshatraLordPlanetId = rasi.RulingPlanetId THEN 'Great Friend' ELSE crNL.EnglishName END AS NakshatraLord_Relationship,
        CASE WHEN sn.NakshatraLordPlanetId = rasi.RulingPlanetId THEN CAST(2 AS SMALLINT) ELSE crNL.RelationshipScore END AS NakshatraLord_Score,
        CASE WHEN sn.SubLordPlanetId = rasi.RulingPlanetId THEN 'Same' ELSE natSL.RelationshipType END AS SubLord_Natural,
        CASE WHEN sn.SubLordPlanetId = rasi.RulingPlanetId THEN 'ADHIMITRA' ELSE crSL.CompoundCode END AS SubLord_Compound,
        CASE WHEN sn.SubLordPlanetId = rasi.RulingPlanetId THEN 'Great Friend' ELSE crSL.EnglishName END AS SubLord_Relationship,
        CASE WHEN sn.SubLordPlanetId = rasi.RulingPlanetId THEN CAST(2 AS SMALLINT) ELSE crSL.RelationshipScore END AS SubLord_Score
    FROM dbo.tbl_Rule_SignNakshatra sn
    JOIN dbo.tbl_Nakshatras nk ON nk.Id = sn.NakshatraId
    JOIN dbo.tbl_Planets nlp ON nlp.Id = sn.NakshatraLordPlanetId
    JOIN dbo.tbl_Planets slp ON slp.Id = sn.SubLordPlanetId
    CROSS JOIN dbo.tbl_SignAttributes rasi
    JOIN dbo.tbl_Planets rlp ON rlp.Id = rasi.RulingPlanetId
    CROSS JOIN (SELECT TOP (1) Id AS RuleSetId FROM dbo.tbl_Rule_Sets WHERE IsActive = 1) rs
    JOIN dbo.tbl_Chart_KeyDetails nlKd ON nlKd.ChartResultId = @ChartResultId AND nlKd.PointKind = 'Graha' AND nlKd.PlanetId = sn.NakshatraLordPlanetId
    JOIN dbo.tbl_Chart_KeyDetails slKd ON slKd.ChartResultId = @ChartResultId AND slKd.PointKind = 'Graha' AND slKd.PlanetId = sn.SubLordPlanetId
    JOIN dbo.tbl_Chart_KeyDetails rlKd ON rlKd.ChartResultId = @ChartResultId AND rlKd.PointKind = 'Graha' AND rlKd.PlanetId = rasi.RulingPlanetId
    OUTER APPLY (SELECT CAST((((CAST(rlKd.SignId AS INT) - CAST(nlKd.SignId AS INT) + 12) % 12) + 1) AS TINYINT) AS Dist) dNL
    OUTER APPLY (SELECT CAST((((CAST(rlKd.SignId AS INT) - CAST(slKd.SignId AS INT) + 12) % 12) + 1) AS TINYINT) AS Dist) dSL
    LEFT JOIN dbo.tbl_Rule_NaturalRelationship natNL ON natNL.RuleSetId = rs.RuleSetId AND natNL.PlanetId = sn.NakshatraLordPlanetId AND natNL.RelatedPlanetId = rasi.RulingPlanetId
    LEFT JOIN dbo.tbl_Rule_TemporaryFriendshipDistance tfNL ON tfNL.RuleSetId = rs.RuleSetId AND tfNL.SignDistance = dNL.Dist
    LEFT JOIN dbo.tbl_Rule_CompoundRelationship crNL ON crNL.RuleSetId = rs.RuleSetId AND crNL.NaturalRelation = natNL.RelationshipType AND crNL.IsTemporaryFriend = tfNL.IsFriend
    LEFT JOIN dbo.tbl_Rule_NaturalRelationship natSL ON natSL.RuleSetId = rs.RuleSetId AND natSL.PlanetId = sn.SubLordPlanetId AND natSL.RelatedPlanetId = rasi.RulingPlanetId
    LEFT JOIN dbo.tbl_Rule_TemporaryFriendshipDistance tfSL ON tfSL.RuleSetId = rs.RuleSetId AND tfSL.SignDistance = dSL.Dist
    LEFT JOIN dbo.tbl_Rule_CompoundRelationship crSL ON crSL.RuleSetId = rs.RuleSetId AND crSL.NaturalRelation = natSL.RelationshipType AND crSL.IsTemporaryFriend = tfSL.IsFriend
);
GO

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_Catalog WHERE RuleTableName = 'tvf_Chart_SignNakshatraRasiRelationship')
    INSERT dbo.tbl_Rule_Catalog (RuleTableName, EngineCode, MethodCodes, Purpose, IntroducedIn)
    VALUES ('tvf_Chart_SignNakshatraRasiRelationship', 'KARAKA', 'PANCHADHA_MAITRI',
            'Per-chart TVF: full 5-tier compound relationship (Great Friend..Great Enemy) between each of the 243 tbl_Rule_SignNakshatra rows (Nakshatra-lord + Sub-lord) and all 12 Rasi-lords, using this chart''s actual planet placements. Extends vw_Rule_SignNakshatraRelationship (natural-only, primary Rasi only) to every Rasi with the genuine chart-specific compound formula.',
            '105_create_sign_nakshatra_rasi_tvf.sql');
GO

INSERT dbo.SchemaMigrations (ScriptName, Note)
SELECT '105_create_tvf_sign_nakshatra_rasi_relationship.sql',
       'tvf_Chart_SignNakshatraRasiRelationship(@ChartResultId): 243 SignNakshatra rows x 12 Rasis, full compound relationship for Nakshatra-lord + Sub-lord; 1 tbl_Rule_Catalog row'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '105_create_tvf_sign_nakshatra_rasi_relationship.sql');
GO

PRINT '105 applied: tvf_Chart_SignNakshatraRasiRelationship created.';
GO
