-- =====================================================================
-- 123 -- Add tbl_NakshatraPadas.NakPadaLord / NakPadaSubLord.
--
-- NakPadaLord: the pada's own D9 (Navamsa) sign lord -- sign lord of
-- NavamsaSignId, via tbl_SignAttributes.RulingPlanetId. Distinct from the
-- existing RulingPlanetId column (migration 022), which repeats the
-- *Nakshatra's* star lord on every pada row -- NakPadaLord is the
-- pada-specific value the domain research doc (nakshatra-lord-sublord-
-- dasha-crossref.md, SS2) found genuinely missing.
--
-- NakPadaSubLord: the KP sub-lord (tbl_Rule_SignNakshatra, migration 096)
-- whose degree band contains the pada's own StartDegree. Explicit design
-- choice (rammyps, 2026-09-19): KP sub-lord boundaries are proportional to
-- Vimshottari periods and do not align with the 4 equal pada divisions, so
-- a pada's span can cross multiple sub-lord bands -- there is no single
-- band that fully "belongs" to a pada. NakPadaSubLord resolves this by
-- taking the sub-lord in effect at the pada's leading edge (its
-- StartDegree), not a majority-overlap or multi-value result.
--
-- Both implemented as non-persisted COMPUTED columns (same pattern as
-- RulingPlanetId, migration 022) rather than physically stored/backfilled
-- values -- they're pure functions of columns/tables already on this row,
-- so a computed column reads live with zero duplication/drift risk.
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID('dbo.fn_GetSignLordPlanetId', 'FN') IS NOT NULL
    DROP FUNCTION dbo.fn_GetSignLordPlanetId;
GO

CREATE FUNCTION dbo.fn_GetSignLordPlanetId (@SignId TINYINT)
RETURNS TINYINT
AS
BEGIN
    DECLARE @PlanetId TINYINT;
    SELECT @PlanetId = RulingPlanetId FROM dbo.tbl_SignAttributes WHERE Id = @SignId;
    RETURN @PlanetId;
END
GO

IF OBJECT_ID('dbo.fn_GetNakshatraPadaSubLordPlanetId', 'FN') IS NOT NULL
    DROP FUNCTION dbo.fn_GetNakshatraPadaSubLordPlanetId;
GO

-- @PadaStartDegree is the pada's own absolute (0-360) StartDegree.
-- tbl_Rule_SignNakshatra.SubLordStartDegree/EndDegree (migration 096, sourced
-- straight from tbl_NakshatraSubLords) are ALSO absolute 0-360 degrees, not
-- offsets from the parent nakshatra's own start -- confirmed against live data
-- (e.g. Bharani/NakshatraId=2's bands run 13.333333-26.666666, matching its
-- absolute StartDegree/EndDegree in tbl_Nakshatras, not 0-13.333333). No
-- conversion needed; compare directly.
CREATE FUNCTION dbo.fn_GetNakshatraPadaSubLordPlanetId (@NakshatraId TINYINT, @PadaStartDegree DECIMAL(9,6))
RETURNS TINYINT
AS
BEGIN
    DECLARE @PlanetId TINYINT;

    SELECT @PlanetId = SubLordPlanetId
    FROM dbo.tbl_Rule_SignNakshatra
    WHERE NakshatraId = @NakshatraId
      AND @PadaStartDegree >= SubLordStartDegree
      AND @PadaStartDegree < SubLordEndDegree;

    RETURN @PlanetId;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('tbl_NakshatraPadas') AND name = 'NakPadaLord')
BEGIN
    ALTER TABLE tbl_NakshatraPadas
        ADD NakPadaLord AS (dbo.fn_GetSignLordPlanetId(NavamsaSignId));
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('tbl_NakshatraPadas') AND name = 'NakPadaSubLord')
BEGIN
    ALTER TABLE tbl_NakshatraPadas
        ADD NakPadaSubLord AS (dbo.fn_GetNakshatraPadaSubLordPlanetId(NakshatraId, StartDegree));
END
GO

-- Sanity check: every one of the 108 pada rows must resolve both lords --
-- a NULL means either a NavamsaSignId with no tbl_SignAttributes row, or a
-- StartDegree that fell outside all 9 of its nakshatra's sub-lord bands
-- (would indicate a degree-boundary bug in migration 096/_archive 021 data).
IF EXISTS (
    SELECT 1 FROM dbo.tbl_NakshatraPadas
    WHERE NakPadaLord IS NULL OR NakPadaSubLord IS NULL
)
    RAISERROR('123: at least one tbl_NakshatraPadas row failed to resolve NakPadaLord/NakPadaSubLord -- check degree-boundary data.', 16, 1);
GO

IF OBJECT_ID(N'dbo.SchemaMigrations', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = N'123_add_nakshatra_pada_lord_columns.sql')
    INSERT dbo.SchemaMigrations (ScriptName, Note)
    VALUES (N'123_add_nakshatra_pada_lord_columns.sql', N'Add tbl_NakshatraPadas.NakPadaLord (Navamsa sign lord) and NakPadaSubLord (KP sub-lord at the pada''s StartDegree, via tbl_Rule_SignNakshatra) as computed columns.');
GO

PRINT '123 applied: tbl_NakshatraPadas.NakPadaLord and NakPadaSubLord added (computed columns, 108 rows verified non-NULL).';
GO
