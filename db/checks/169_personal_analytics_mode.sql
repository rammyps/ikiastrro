-- Read-only verification for migration 169.
SET NOCOUNT ON;

SELECT subject.SubjectClassification,
       COUNT(DISTINCT feature.SubjectKey) AS Subjects,
       COUNT(feature.SubjectKey) AS FeatureRows
FROM dbo.tbl_Dim_AnalyticsSubjects subject
LEFT JOIN dbo.vw_AnalyticsHouseFeaturesV2 feature ON feature.SubjectKey = subject.SubjectKey
WHERE subject.ResearchUseStatus = 'ELIGIBLE'
GROUP BY subject.SubjectClassification;

SELECT CASE WHEN NOT EXISTS (
    SELECT SubjectKey
    FROM dbo.vw_AnalyticsHouseFeaturesV2
    GROUP BY SubjectKey
    HAVING COUNT(*) <> 12 OR COUNT(DISTINCT HouseFromLagna) <> 12
) THEN 'PASS' ELSE 'FAIL' END AS CompletePersonalFeatureSets;
