-- =====================================================================
-- 125 -- Rasi lord / Nakshatra lord / Nakshatra Pada lord three-way
-- comparison, at the 108-row pada grain (tbl_NakshatraPadas).
--
-- tbl_Planets.ShortCode: new 2-letter abbreviation column (Su/Mo/Ma/Me/
-- Ju/Ve/Sa/Ra/Ke), the standard Vedic/KP planet-code convention -- single
-- source of truth for building notation strings like "Ve-Ke-Ke" (rammyps's
-- own example, 2026-09-19), instead of hardcoding the same 9-way CASE in
-- every function that wants one.
--
-- vw_Rule_NakshatraPadaLordConnection: a READ VIEW, not a new physical
-- table -- every column here is either an existing FK (RasiId/NakshatraId/
-- NakPadaLord, already on tbl_NakshatraPadas since migrations 022/123) or
-- computed live from tbl_Rule_NaturalRelationship. Materializing this
-- would just duplicate rows already governed by those tables and let them
-- drift apart -- same reasoning as vw_Rule_SignNakshatraRelationship
-- (migration 097), which this view is the pada-grain sibling of: 097
-- compares Rasi-lord vs Nakshatra-lord/Sub-lord at the 243-row KP
-- sub-division grain; this compares Rasi-lord vs Nakshatra-lord vs
-- Nakshatra-PADA-lord (migration 123's NakPadaLord) at the 108-row pada
-- grain -- a different lord (D9 pada lord, not KP sub-lord) at a
-- different grain, not a duplicate of 097.
--
-- NaturalConnectionCode: "<RasiLord>-<NakshatraLord>-<PadaLord>" 2-letter
-- codes, e.g. Ve-Ke-Ke. Purely notational, no new data.
--
-- Three pairwise relations (RasiLord<->NakshatraLord, RasiLord<->PadaLord,
-- NakshatraLord<->PadaLord) all use the same generic
-- fn_GetPlanetNaturalRelation helper (RuleSetId 1). Structural fact worth
-- recording: PadaLord (D9 navamsa-sign lord) is ALWAYS one of the 7
-- classical grahas -- same domain as RasiLord -- because
-- tbl_SignAttributes.RulingPlanetId (what NakPadaLord resolves through,
-- migration 123) never contains Rahu/Ketu. Only NakshatraLord can be a
-- node (9-lord Vimshottari cycle). So RasiLord_PadaLord_Relation can
-- never be 'Node'; the other two pairs can.
--
-- Deliberately NOT included: any predictive/interpretive "combined
-- reading" text for these 108 rows. The 36-row
-- tbl_Rule_RasiNakshatraCombination (migration 124) synthesis text was
-- transcribed from an already-drafted, already-reviewed research doc
-- (rasi-nakshatra-36-combination-matrix.md) -- no equivalent doc exists
-- yet at this pada grain, and 108 rows x several text fields is 3x that
-- table's size. Structure is ready (this view supplies every input a
-- synthesis pass would need); text needs the same draft-then-review step
-- migration 124's source doc went through, not a same-turn fabrication.
--
-- Apply:  sqlcmd -S "localhost\SQLSERVER2025" -E -d ikiastrro -b -i db/125_nakshatra_pada_lord_connection.sql
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF COL_LENGTH('dbo.tbl_Planets', 'ShortCode') IS NULL
    ALTER TABLE dbo.tbl_Planets ADD ShortCode CHAR(2) NULL;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Planets WHERE ShortCode IS NOT NULL)
BEGIN
    ;WITH src (PlanetName, ShortCode) AS
    (
        SELECT * FROM (VALUES
            (N'Sun', 'Su'), (N'Moon', 'Mo'), (N'Mars', 'Ma'), (N'Mercury', 'Me'),
            (N'Jupiter', 'Ju'), (N'Venus', 'Ve'), (N'Saturn', 'Sa'), (N'Rahu', 'Ra'), (N'Ketu', 'Ke')
        ) AS v (PlanetName, ShortCode)
    )
    UPDATE p SET p.ShortCode = src.ShortCode
    FROM dbo.tbl_Planets p JOIN src ON src.PlanetName = p.PlanetName;

    IF (SELECT COUNT(*) FROM dbo.tbl_Planets WHERE ShortCode IS NULL) <> 0
        RAISERROR('125: not all 9 tbl_Planets rows got a ShortCode -- check for a PlanetName spelling mismatch.', 16, 1);
END
GO

IF OBJECT_ID('dbo.fn_GetPlanetNaturalRelation', 'FN') IS NOT NULL
    DROP FUNCTION dbo.fn_GetPlanetNaturalRelation;
GO

CREATE FUNCTION dbo.fn_GetPlanetNaturalRelation (@PlanetAId TINYINT, @PlanetBId TINYINT)
RETURNS VARCHAR(20)
AS
BEGIN
    DECLARE @Relation VARCHAR(20);

    IF @PlanetAId = @PlanetBId
        SET @Relation = 'Same';
    ELSE IF @PlanetAId IN (8, 9) OR @PlanetBId IN (8, 9) -- Rahu, Ketu: no tbl_Rule_NaturalRelationship rows for the nodes
        SET @Relation = 'Node';
    ELSE
        SELECT @Relation = RelationshipType
        FROM dbo.tbl_Rule_NaturalRelationship
        WHERE RuleSetId = 1 AND PlanetId = @PlanetAId AND RelatedPlanetId = @PlanetBId;

    RETURN @Relation;
END
GO

IF OBJECT_ID('dbo.fn_GetPadaNaturalConnectionCode', 'FN') IS NOT NULL
    DROP FUNCTION dbo.fn_GetPadaNaturalConnectionCode;
GO

CREATE FUNCTION dbo.fn_GetPadaNaturalConnectionCode (@NakshatraPadaId INT)
RETURNS VARCHAR(11) -- "xx-xx-xx"
AS
BEGIN
    DECLARE @Code VARCHAR(11);

    SELECT @Code = rasiLordP.ShortCode + '-' + nakLordP.ShortCode + '-' + padaLordP.ShortCode
    FROM dbo.tbl_NakshatraPadas p
    JOIN dbo.tbl_SignAttributes sa ON sa.Id = p.RasiId
    JOIN dbo.tbl_Nakshatras nk     ON nk.Id = p.NakshatraId
    JOIN dbo.tbl_Planets rasiLordP ON rasiLordP.Id = sa.RulingPlanetId
    JOIN dbo.tbl_Planets nakLordP  ON nakLordP.Id = nk.RulingPlanetId
    JOIN dbo.tbl_Planets padaLordP ON padaLordP.Id = p.NakPadaLord
    WHERE p.Id = @NakshatraPadaId;

    RETURN @Code;
END
GO

CREATE OR ALTER VIEW dbo.vw_Rule_NakshatraPadaLordConnection
AS
SELECT
    p.Id                                                              AS NakshatraPadaId,
    p.RasiId,
    sa.SignName                                                       AS RasiName,
    sa.RulingPlanetId                                                 AS RasiLordPlanetId,
    rasiLordP.PlanetName                                              AS RasiLordName,
    p.NakshatraId,
    nk.NakshatraName,
    nk.RulingPlanetId                                                 AS NakshatraLordPlanetId,
    nakLordP.PlanetName                                                AS NakshatraLordName,
    p.PadaNumber,
    p.NavamsaSignId,
    p.NakPadaLord                                                     AS PadaLordPlanetId,
    padaLordP.PlanetName                                               AS PadaLordName,
    p.NakPadaSubLord                                                  AS PadaSubLordPlanetId,
    dbo.fn_GetPadaNaturalConnectionCode(p.Id)                         AS NaturalConnectionCode,
    dbo.fn_GetPlanetNaturalRelation(sa.RulingPlanetId, nk.RulingPlanetId) AS RasiLord_NakshatraLord_Relation,
    dbo.fn_GetPlanetNaturalRelation(sa.RulingPlanetId, p.NakPadaLord)     AS RasiLord_PadaLord_Relation,
    dbo.fn_GetPlanetNaturalRelation(nk.RulingPlanetId, p.NakPadaLord)     AS NakshatraLord_PadaLord_Relation
FROM dbo.tbl_NakshatraPadas p
JOIN dbo.tbl_SignAttributes sa  ON sa.Id = p.RasiId
JOIN dbo.tbl_Nakshatras nk      ON nk.Id = p.NakshatraId
JOIN dbo.tbl_Planets rasiLordP  ON rasiLordP.Id = sa.RulingPlanetId
JOIN dbo.tbl_Planets nakLordP   ON nakLordP.Id = nk.RulingPlanetId
JOIN dbo.tbl_Planets padaLordP  ON padaLordP.Id = p.NakPadaLord;
GO

INSERT dbo.SchemaMigrations (ScriptName, Note)
SELECT '125_nakshatra_pada_lord_connection.sql',
       'tbl_Planets.ShortCode (9 rows); fn_GetPlanetNaturalRelation, fn_GetPadaNaturalConnectionCode; vw_Rule_NakshatraPadaLordConnection (108 rows: Rasi/Nakshatra/Pada lord 3-way + Ve-Ke-Ke notation)'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '125_nakshatra_pada_lord_connection.sql');
GO

PRINT '125 applied: vw_Rule_NakshatraPadaLordConnection ready (108 rows).';
GO
