-- Read-only verification for migration 168.
SET NOCOUNT ON;

SELECT FeatureContractVersion,
       COUNT(DISTINCT SubjectKey) AS Subjects,
       COUNT(*) AS FeatureRows,
       MIN(HouseFromLagna) AS MinHouse,
       MAX(HouseFromLagna) AS MaxHouse
FROM dbo.vw_AnalyticsHouseFeaturesV2
GROUP BY FeatureContractVersion;

SELECT CASE WHEN NOT EXISTS (
    SELECT SubjectKey
    FROM dbo.vw_AnalyticsHouseFeaturesV2
    GROUP BY SubjectKey
    HAVING COUNT(*) <> 12
       OR COUNT(DISTINCT HouseFromLagna) <> 12
       OR MIN(HouseFromLagna) <> 1
       OR MAX(HouseFromLagna) <> 12
) THEN 'PASS' ELSE 'FAIL' END AS EverySubjectHasTwelveDistinctHouses,
CASE WHEN NOT EXISTS (
    SELECT 1 FROM dbo.vw_AnalyticsHouseFeaturesV2
    WHERE Capacity NOT BETWEEN 0 AND 100
       OR Consistency NOT BETWEEN 0 AND 100
       OR Context NOT BETWEEN 0 AND 100
       OR OverallSupport NOT BETWEEN 0 AND 100
) THEN 'PASS' ELSE 'FAIL' END AS EveryFeatureIsInRange,
CASE WHEN NOT EXISTS (
    SELECT 1
    FROM dbo.vw_AnalyticsHouseFeaturesV2 feature
    JOIN dbo.tbl_Dim_AnalyticsSubjects subject ON subject.SubjectKey = feature.SubjectKey
    WHERE subject.ResearchUseStatus <> 'ELIGIBLE'
       OR subject.SubjectClassification <> 'RESEARCH'
       OR subject.ConsentRecordedAtUtc IS NULL
       OR subject.WithdrawnAtUtc IS NOT NULL
) THEN 'PASS' ELSE 'FAIL' END AS EverySubjectIsConsentedAndEligible;
