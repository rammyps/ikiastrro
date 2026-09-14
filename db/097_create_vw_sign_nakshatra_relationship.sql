-- =====================================================================
-- 097 — vw_Rule_SignNakshatraRelationship: how the rasi's own lord "gets along"
-- (Naisargika Maitri / natural friendship, tbl_Rule_NaturalRelationship, RuleSetId 1
-- Parashari-Classical) with the nakshatra lord (KP L1) and sub-lord (KP L2) sitting on
-- tbl_Rule_SignNakshatra (migration 096). Per rammyps's 2026-09-13 call.
--
-- Pure read, no new storage: a view over tbl_Rule_SignNakshatra join tbl_SignAttributes
-- join tbl_Rule_NaturalRelationship, same "no new computation or storage" shape as
-- vw_ChartKarakamsa (migration 082). Materializing this as a table would just duplicate
-- rows already governed by tbl_Rule_SignNakshatra + tbl_Rule_NaturalRelationship and let
-- the two drift apart.
--
-- Rahu/Ketu gap (rammyps's 2026-09-13 call, left open, not silently guessed): signs are
-- only ever ruled by the 7 classical grahas, but nakshatra lords and sub-lords cycle
-- through all 9 (Rahu/Ketu included). tbl_Rule_NaturalRelationship has no rows for
-- Rahu/Ketu at all (42 rows = 7x6 classical pairs only) -- there is no natural-
-- relationship convention for the nodes anywhere in this codebase to borrow (the only
-- precedent, RelationshipEngine.AspectOffsets, is about aspect houses, not friendship, and
-- is itself flagged "genuinely disputed across texts"). So *_Relationship is NULL
-- whenever the nakshatra lord or sub-lord is Rahu(8)/Ketu(9) -- left NULL rather than
-- guessing a dispositor proxy (Rahu->Saturn, Ketu->Mars) that this project hasn't sourced
-- or decided on.
--
-- Same-planet case: tbl_Rule_NaturalRelationship excludes self-pairs (a planet isn't
-- listed as its own friend/enemy), so RasiLord = NakshatraLord/SubLord is surfaced
-- explicitly as 'Same' rather than falling through to NULL and reading like a data gap.
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER VIEW dbo.vw_Rule_SignNakshatraRelationship
AS
SELECT
    sn.RasiId,
    rasi.SignName                          AS RasiName,
    rasi.RulingPlanetId                    AS RasiLordPlanetId,
    rasiLord.PlanetName                    AS RasiLordName,
    sn.NakshatraId,
    nak.NakshatraName,
    sn.NakshatraLordPlanetId,
    nakLord.PlanetName                     AS NakshatraLordName,
    CASE
        WHEN rasi.RulingPlanetId = sn.NakshatraLordPlanetId THEN 'Same'
        ELSE nakRel.RelationshipType
    END                                     AS RasiLord_NakshatraLord_Relationship,
    sn.SubSequenceNumber,
    sn.SubLordPlanetId,
    subLord.PlanetName                     AS SubLordName,
    CASE
        WHEN rasi.RulingPlanetId = sn.SubLordPlanetId THEN 'Same'
        ELSE subRel.RelationshipType
    END                                     AS RasiLord_SubLord_Relationship,
    sn.SubLordStartDegree,
    sn.SubLordEndDegree
FROM dbo.tbl_Rule_SignNakshatra sn
JOIN dbo.tbl_SignAttributes rasi     ON rasi.Id = sn.RasiId
JOIN dbo.tbl_Planets rasiLord        ON rasiLord.Id = rasi.RulingPlanetId
JOIN dbo.tbl_Nakshatras nak          ON nak.Id = sn.NakshatraId
JOIN dbo.tbl_Planets nakLord         ON nakLord.Id = sn.NakshatraLordPlanetId
JOIN dbo.tbl_Planets subLord         ON subLord.Id = sn.SubLordPlanetId
LEFT JOIN dbo.tbl_Rule_NaturalRelationship nakRel
       ON nakRel.RuleSetId = 1
      AND nakRel.PlanetId = rasi.RulingPlanetId
      AND nakRel.RelatedPlanetId = sn.NakshatraLordPlanetId
LEFT JOIN dbo.tbl_Rule_NaturalRelationship subRel
       ON subRel.RuleSetId = 1
      AND subRel.PlanetId = rasi.RulingPlanetId
      AND subRel.RelatedPlanetId = sn.SubLordPlanetId;
GO

INSERT dbo.SchemaMigrations (ScriptName, Note)
SELECT '097_create_vw_sign_nakshatra_relationship.sql',
       'vw_Rule_SignNakshatraRelationship: rasi lord vs nakshatra-lord/sub-lord natural friendship (RuleSetId 1). Rahu/Ketu lords left NULL, no sourced convention yet.'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '097_create_vw_sign_nakshatra_relationship.sql');
GO

PRINT '097 applied: vw_Rule_SignNakshatraRelationship created.';
GO
