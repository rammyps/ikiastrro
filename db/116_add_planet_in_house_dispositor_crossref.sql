/* Cross-reference each Planet-in-House row (migration 113-115) with its dispositor: the lord
   of the sign the placed planet occupies (tbl_Chart_KeyDetails.SignLordPlanetId), plus where
   that dispositor itself sits (its own house/sign/dignity, via a self-join back onto
   tbl_Chart_KeyDetails for the same chart). Under WHOLE_SIGN houses the dispositor is also
   the lord of the house the planet occupies, so this doubles as a cross-check against
   tbl_Chart_HouseLords (migrations 093/094) without duplicating that table's own content.

   No new table: this is a read-time join, same category as tvf_Chart_DashaLordRelationship
   (migration 110), which already cross-references a dasha lord against its own Rasi lord the
   same way. */
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER VIEW dbo.vw_ChartPlanetInHouseInterpretation
AS
SELECT
    kd.ChartResultId, kd.PlanetId, p.PlanetName, kd.HouseNumberFromLagna AS HouseNumber,
    r.RuleSetId, r.ResultText, r.InterpretationStatusCode, r.SourceRefCode, r.HouseSystemCode,
    kd.SignLordPlanetId AS DispositorPlanetId,
    dp.PlanetName AS DispositorPlanetName,
    dkd.HouseNumberFromLagna AS DispositorHouseNumber,
    dkd.SignId AS DispositorSignId,
    dsa.SignName AS DispositorSignName,
    dkd.DignityStatus AS DispositorDignityStatus,
    CASE WHEN kd.SignLordPlanetId = kd.PlanetId THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS IsSelfDisposed
FROM dbo.tbl_Chart_KeyDetails kd
JOIN dbo.tbl_Planets p ON p.Id = kd.PlanetId
JOIN dbo.tbl_Rule_Sets rs ON rs.IsActive = 1
JOIN dbo.tbl_Rule_PlanetInHouse r ON r.RuleSetId = rs.Id AND r.PlanetId = kd.PlanetId
    AND r.HouseNumber = kd.HouseNumberFromLagna AND r.HouseSystemCode = 'WHOLE_SIGN'
LEFT JOIN dbo.tbl_Planets dp ON dp.Id = kd.SignLordPlanetId
LEFT JOIN dbo.tbl_Chart_KeyDetails dkd ON dkd.ChartResultId = kd.ChartResultId
    AND dkd.PlanetId = kd.SignLordPlanetId AND dkd.PointKind = 'Graha'
LEFT JOIN dbo.tbl_SignAttributes dsa ON dsa.Id = dkd.SignId
WHERE kd.PointKind = 'Graha';
GO

IF OBJECT_ID(N'dbo.SchemaMigrations', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = N'116_add_planet_in_house_dispositor_crossref.sql')
    INSERT dbo.SchemaMigrations (ScriptName, Note)
    VALUES (N'116_add_planet_in_house_dispositor_crossref.sql',
        N'Extends vw_ChartPlanetInHouseInterpretation with each placed planet''s dispositor (sign lord) and that dispositor''s own house/sign/dignity.');
GO

PRINT '116 applied: vw_ChartPlanetInHouseInterpretation now includes dispositor cross-reference.';
GO
