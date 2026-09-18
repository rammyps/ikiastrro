/* Promote the 108 research.*PlanetInHouseClaim rows (migration 114, SRC_RAMAN_HTJH) into
   production dbo.tbl_Rule_PlanetInHouse, record the promotion in
   research.tbl_Dim_SourceReferencePlanetInHouseCrosswalk, and expose the result joined
   against the per-chart tbl_Chart_KeyDetails fact via vw_ChartPlanetInHouseInterpretation --
   mirroring the tbl_Rule_HouseLordPlacement / vw_ChartHouseLordInterpretation precedent
   (migrations 093/094).

   InterpretationStatusCode is carried over as 'Proposed' (the research Claim.StatusCode):
   this is a first promotion pass, not a manually reviewed one. Nothing here has been checked
   against a second chart or author beyond the single Raman passage each row paraphrases. */
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

DECLARE @ruleSetId TINYINT = (SELECT TOP (1) Id FROM dbo.tbl_Rule_Sets WHERE IsActive = 1 ORDER BY VersionNumber DESC);
IF @ruleSetId IS NULL
    THROW 50115, 'Migration 115 requires an active rule set.', 1;

INSERT dbo.tbl_Rule_PlanetInHouse
    (RuleSetId, PlanetId, HouseNumber, HouseSystemCode, ResultText, InterpretationStatusCode,
     MethodCode, RuleParametersJson, CalculationNarrative, SourceRefCode)
SELECT @ruleSetId, planet.Id, h.HouseNumber, 'WHOLE_SIGN',
       c.ClaimText, c.StatusCode, 'PLANET_IN_HOUSE_LOOKUP', c.RequiredConditionsJson,
       CONCAT(N'Paraphrased from ', t.SourceLocator, N', ', t.Edition, N' p.', t.VerseOrPage, N'.'),
       c.SourceRefCode
FROM research.tbl_Dim_SourceReferencePlanetInHouseClaim c
JOIN research.tbl_Dim_SourceReferencePlanetInHouse x ON x.Id = c.PlanetInHouseId
JOIN research.tbl_Dim_SourceReferenceHouse h ON h.Id = x.HouseId
JOIN research.tbl_Dim_SourceReferencePlanetInHouseText t ON t.Id = c.SourceTextId
JOIN dbo.tbl_Planets planet ON planet.Id = x.PlanetId   -- research.tbl_Dim_SourceReferencePlanet.Id == dbo.tbl_Planets.Id (same 1-9 order)
WHERE c.SourceRefCode = 'SRC_RAMAN_HTJH'
  AND NOT EXISTS (
    SELECT 1 FROM dbo.tbl_Rule_PlanetInHouse r
    WHERE r.RuleSetId = @ruleSetId AND r.PlanetId = planet.Id AND r.HouseNumber = h.HouseNumber AND r.HouseSystemCode = 'WHOLE_SIGN'
);
GO

INSERT research.tbl_Dim_SourceReferencePlanetInHouseCrosswalk
    (ClaimId, ProductionTargetCode, PromotionStatus, PromotionMigration)
SELECT c.Id, 'dbo.tbl_Rule_PlanetInHouse', 'Promoted', '115_promote_planet_in_house_claims.sql'
FROM research.tbl_Dim_SourceReferencePlanetInHouseClaim c
WHERE c.SourceRefCode = 'SRC_RAMAN_HTJH'
  AND NOT EXISTS (
    SELECT 1 FROM research.tbl_Dim_SourceReferencePlanetInHouseCrosswalk cw
    WHERE cw.ClaimId = c.Id AND cw.ProductionTargetCode = 'dbo.tbl_Rule_PlanetInHouse'
);
GO

CREATE OR ALTER VIEW dbo.vw_ChartPlanetInHouseInterpretation
AS
SELECT kd.ChartResultId, kd.PlanetId, p.PlanetName, kd.HouseNumberFromLagna AS HouseNumber,
       r.RuleSetId, r.ResultText, r.InterpretationStatusCode, r.SourceRefCode, r.HouseSystemCode
FROM dbo.tbl_Chart_KeyDetails kd
JOIN dbo.tbl_Planets p ON p.Id = kd.PlanetId
JOIN dbo.tbl_Rule_Sets rs ON rs.IsActive = 1
JOIN dbo.tbl_Rule_PlanetInHouse r ON r.RuleSetId = rs.Id AND r.PlanetId = kd.PlanetId
    AND r.HouseNumber = kd.HouseNumberFromLagna AND r.HouseSystemCode = 'WHOLE_SIGN'
WHERE kd.PointKind = 'Graha';
GO

IF OBJECT_ID(N'dbo.SchemaMigrations', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = N'115_promote_planet_in_house_claims.sql')
    INSERT dbo.SchemaMigrations (ScriptName, Note)
    VALUES (N'115_promote_planet_in_house_claims.sql',
        N'Promotes 108 research.*PlanetInHouseClaim (SRC_RAMAN_HTJH) rows into dbo.tbl_Rule_PlanetInHouse; adds vw_ChartPlanetInHouseInterpretation; writes Crosswalk Promoted status.');
GO

PRINT '115 applied: tbl_Rule_PlanetInHouse promoted + vw_ChartPlanetInHouseInterpretation created.';
GO
