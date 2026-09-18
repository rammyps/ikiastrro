USE [ikiastrro];
GO

-- Rule rows: expect three — the reworded generic (SourceRefCode NULL) row plus
-- the Raman and PVR source-specific overrides, with different ShortFormationRule text.
SELECT YogaCode, SourceRefCode, FormationFamilyCode, ShortFormationRule
FROM dbo.tbl_Rule_Yoga
WHERE YogaCode = 'YOGA_BUDHA_ADITYA'
ORDER BY SourceRefCode;
GO

-- View resolution: for any chart with a YOGA_BUDHA_ADITYA evaluation row per source,
-- the Raman row and the PVR row must resolve to DIFFERENT YogaRule text (the whole
-- point of 119). If this returns 0 rows or both sources show the same YogaRule, the
-- view's COALESCE-based join isn't preferring the source-specific row.
SELECT BirthDetailId, Name, SourceRefCode, Present, YogaTypeCode, YogaRule
FROM dbo.vw_ChartYogaEvaluations
WHERE YogaCode = 'YOGA_BUDHA_ADITYA'
ORDER BY BirthDetailId, SourceRefCode;
GO
