/* Promote the 277 research.*HouseLordInHouseClaim rows (migrations 089-092; 12x12 grid
   complete) into production dbo.tbl_Rule_HouseLordPlacement, record the promotion in
   research.tbl_Dim_SourceReferenceHouseLordInHouseCrosswalk, and expose the result joined
   against the per-chart tbl_Chart_HouseLords fact via vw_ChartHouseLordInterpretation --
   mirroring the tbl_Rule_Yoga / vw_ChartYogaEvaluations precedent (migration 079).

   InterpretationStatusCode is carried over as 'Proposed' (the research Claim.StatusCode):
   this is a first promotion pass, not a manually reviewed one -- see docs/research/domain/
   house-placement.md. Nothing here has been checked against a second chart or author beyond
   the single Raman passage each row paraphrases. */

DECLARE @ruleSetId TINYINT = (SELECT TOP (1) Id FROM dbo.tbl_Rule_Sets WHERE IsActive = 1 ORDER BY VersionNumber DESC);
IF @ruleSetId IS NULL
    THROW 50094, 'Migration 094 requires an active rule set.', 1;

INSERT dbo.tbl_Rule_HouseLordPlacement
    (RuleSetId, OwnedHouseNumber, OccupiedHouseNumber, BranchCode, HouseSystemCode, ReferencePointCode,
     ResultText, EvaluableTodayCode, InterpretationStatusCode, MethodCode, RuleParametersJson,
     CalculationNarrative, SourceRefCode)
SELECT @ruleSetId, owned.HouseNumber, occ.HouseNumber, c.BranchCode, x.HouseSystemCode, x.ReferencePointCode,
       c.ClaimText,
       COALESCE(UPPER(JSON_VALUE(c.RequiredConditionsJson, '$.evaluableToday')),
                CASE WHEN c.BranchCode = 'BASELINE' THEN 'YES' ELSE 'PARTIAL' END),
       c.StatusCode, 'PLACEMENT_LOOKUP', c.RequiredConditionsJson,
       CONCAT(N'Paraphrased from ', t.SourceLocator, N' (', t.Chapter, N').'),
       c.SourceRefCode
FROM research.tbl_Dim_SourceReferenceHouseLordInHouseClaim c
JOIN research.tbl_Dim_SourceReferenceHouseLordInHouse x ON x.Id = c.HouseLordInHouseId
JOIN research.tbl_Dim_SourceReferenceHouse owned ON owned.Id = x.OwnedHouseId
JOIN research.tbl_Dim_SourceReferenceHouse occ ON occ.Id = x.OccupiedHouseId
JOIN research.tbl_Dim_SourceReferenceHouseLordInHouseText t ON t.Id = c.SourceTextId
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.tbl_Rule_HouseLordPlacement r
    WHERE r.RuleSetId = @ruleSetId AND r.OwnedHouseNumber = owned.HouseNumber
      AND r.OccupiedHouseNumber = occ.HouseNumber AND r.BranchCode = c.BranchCode
      AND r.HouseSystemCode = x.HouseSystemCode AND r.ReferencePointCode = x.ReferencePointCode
);

INSERT research.tbl_Dim_SourceReferenceHouseLordInHouseCrosswalk
    (ClaimId, ProductionTargetCode, PromotionStatus, PromotionMigration)
SELECT c.Id, 'dbo.tbl_Rule_HouseLordPlacement', 'Promoted', '094_promote_house_lord_placement_claims.sql'
FROM research.tbl_Dim_SourceReferenceHouseLordInHouseClaim c
JOIN research.tbl_Dim_SourceReferenceHouseLordInHouse x ON x.Id = c.HouseLordInHouseId
JOIN research.tbl_Dim_SourceReferenceHouse owned ON owned.Id = x.OwnedHouseId
JOIN research.tbl_Dim_SourceReferenceHouse occ ON occ.Id = x.OccupiedHouseId
WHERE EXISTS (
    SELECT 1 FROM dbo.tbl_Rule_HouseLordPlacement r
    WHERE r.OwnedHouseNumber = owned.HouseNumber AND r.OccupiedHouseNumber = occ.HouseNumber
      AND r.BranchCode = c.BranchCode AND r.SourceRefCode = c.SourceRefCode
)
AND NOT EXISTS (
    SELECT 1 FROM research.tbl_Dim_SourceReferenceHouseLordInHouseCrosswalk w
    WHERE w.ClaimId = c.Id AND w.ProductionTargetCode = 'dbo.tbl_Rule_HouseLordPlacement'
);
GO

-- Joined view: every tbl_Chart_HouseLords row (one per house per chart) paired with every
-- matching interpretation branch. IsDignitySupported flags WELL_DISPOSED rows where the
-- already-computed LordDignityStatus fact plausibly supports "well disposed" -- a partial
-- signal only (aspect/strength qualification is not yet a computed fact; see
-- docs/research/domain/house-placement.md "Qualitative terms are not ready-made thresholds").
-- BASELINE rows always apply; the UI decides what to show, this view never filters branches out.
CREATE OR ALTER VIEW dbo.vw_ChartHouseLordInterpretation
AS
SELECT hl.ChartResultId, hl.HouseNumber AS OwnedHouseNumber, hl.HouseSign, hl.LordPlanet,
       hl.LordPlacedInHouseFromLagna AS OccupiedHouseNumber, hl.LordPlacedInSign, hl.LordDignityStatus,
       r.RuleSetId, r.BranchCode, r.ResultText, r.EvaluableTodayCode, r.InterpretationStatusCode,
       r.SourceRefCode, r.HouseSystemCode, r.ReferencePointCode,
       CAST(CASE WHEN r.BranchCode = 'WELL_DISPOSED'
                      AND hl.LordDignityStatus IN ('Exalted','Moolatrikona','Own Sign','Great Friend','Friend')
                 THEN 1 ELSE 0 END AS BIT) AS IsDignitySupported
FROM dbo.tbl_Chart_HouseLords hl
JOIN dbo.tbl_Rule_HouseLordPlacement r
    ON r.OwnedHouseNumber = hl.HouseNumber
   AND r.OccupiedHouseNumber = hl.LordPlacedInHouseFromLagna
   AND r.HouseSystemCode = 'WHOLE_SIGN' AND r.ReferencePointCode = 'LAGNA'
   AND r.IsActive = 1;
GO

IF OBJECT_ID(N'dbo.SchemaMigrations', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = N'094_promote_house_lord_placement_claims.sql')
    INSERT dbo.SchemaMigrations (ScriptName, Note)
    VALUES (N'094_promote_house_lord_placement_claims.sql', N'Promote all 277 research house-lord-placement claims into dbo.tbl_Rule_HouseLordPlacement, record the crosswalk, and add vw_ChartHouseLordInterpretation joining it to tbl_Chart_HouseLords.');
GO

DECLARE @rows INT = (SELECT COUNT(*) FROM dbo.tbl_Rule_HouseLordPlacement);
PRINT '094 applied: tbl_Rule_HouseLordPlacement rows=' + CAST(@rows AS VARCHAR(10));
GO
