-- Expected: 319 emitted variants, 300 numbered combinations, 15 child sets,
-- one PARTIAL variant (111), and two NOT_EVALUATED variants (178/179).
SELECT SourceRefCode,COUNT(*) VariantRows,COUNT(DISTINCT SequenceNumber) NumberedCombinations,
       SUM(CASE WHEN EvaluationStatus='EVALUATED' THEN 1 ELSE 0 END) Evaluated,
       SUM(CASE WHEN EvaluationStatus='PARTIAL' THEN 1 ELSE 0 END) Partial,
       SUM(CASE WHEN EvaluationStatus='NOT_EVALUATED' THEN 1 ELSE 0 END) NotEvaluated
FROM dbo.tbl_Rule_YogaVariant
WHERE SourceRefCode='SRC_RAMAN_300_COMBINATIONS'
GROUP BY SourceRefCode;

SELECT s.Code,s.DisplayName,COUNT(v.Id) VariantRows
FROM dbo.tbl_Dim_YogaSets s
LEFT JOIN dbo.tbl_Rule_YogaVariant v ON v.YogaSetCode=s.Code
WHERE s.ParentCode='BVR_300'
GROUP BY s.Code,s.DisplayName,s.DisplayOrder
ORDER BY s.DisplayOrder;

SELECT v.SourceVariantCode,v.EvaluationStatus,d.YogaDependencyCode,d.RequirementRole,v.Notes
FROM dbo.tbl_Rule_YogaVariant v
JOIN dbo.tbl_Rule_YogaVariantDependency d ON d.YogaVariantId=v.Id
WHERE v.SourceRefCode='SRC_RAMAN_300_COMBINATIONS'
ORDER BY v.SequenceNumber,d.YogaDependencyCode;
