-- Read-only verification for migration 137.
SET NOCOUNT ON;

SELECT YogaSetCode,
       COUNT(*) AS VariantCount,
       SUM(CASE WHEN EvaluationStatus='EVALUATED' THEN 1 ELSE 0 END) AS EvaluatedCount,
       SUM(CASE WHEN EvaluationStatus<>'EVALUATED' THEN 1 ELSE 0 END) AS OpenCount
FROM dbo.tbl_Rule_YogaVariant
WHERE SourceRefCode='SRC_PVR_INTEGRATED'
GROUP BY YogaSetCode
ORDER BY YogaSetCode;

SELECT d.YogaDependencyCode, COUNT(*) AS TaggedVariants
FROM dbo.tbl_Rule_YogaVariantDependency d
GROUP BY d.YogaDependencyCode
ORDER BY d.YogaDependencyCode;

SELECT v.SourceVariantCode,v.YogaCode,v.YogaSetCode,v.EvaluationStatus,v.Notes
FROM dbo.tbl_Rule_YogaVariant v
WHERE v.EvaluationStatus<>'EVALUATED'
ORDER BY v.YogaSetCode,v.SequenceNumber;

SELECT v.SourceVariantCode AS MissingApplicability
FROM dbo.tbl_Rule_YogaVariant v
WHERE v.SourceRefCode='SRC_PVR_INTEGRATED'
AND NOT EXISTS
(
    SELECT 1 FROM dbo.tbl_Rule_YogaChartApplicability a
    WHERE a.RuleSetId=v.RuleSetId AND a.SourceRefCode=v.SourceRefCode
      AND a.SourceVariantCode=v.SourceVariantCode
);

SELECT ConceptRuleResolution,COUNT(*) AS VariantCount
FROM dbo.vw_YogaVariantRules
WHERE SourceRefCode='SRC_PVR_INTEGRATED'
GROUP BY ConceptRuleResolution
ORDER BY ConceptRuleResolution;

SELECT SourceVariantCode AS UnresolvedConceptRule
FROM dbo.vw_YogaVariantRules
WHERE SourceRefCode='SRC_PVR_INTEGRATED' AND ConceptRuleResolution='UNRESOLVED';
