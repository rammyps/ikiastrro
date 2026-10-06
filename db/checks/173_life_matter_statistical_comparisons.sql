USE [ikiastrro];
GO
SET NOCOUNT ON;
GO
IF OBJECT_ID('dbo.tbl_Fact_LifeMatterStatisticalComparisons', 'U') IS NULL
    THROW 51000, 'Life Matter statistical-comparison table is missing.', 1;
IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE parent_object_id = OBJECT_ID('dbo.tbl_Fact_LifeMatterStatisticalComparisons')
      AND referenced_object_id = OBJECT_ID('dbo.tbl_Dim_AnalyticsSubjects'))
    THROW 51000, 'Life Matter comparisons are not tied to analytics-subject governance.', 1;
IF EXISTS (
    SELECT 1 FROM dbo.tbl_Fact_LifeMatterStatisticalComparisons
    WHERE EligibleCount < 0 OR MeasuredCount < 0 OR MeasuredCount > EligibleCount
       OR MissingRate NOT BETWEEN 0 AND 1
       OR (MeasuredCount < 30 AND Percentile IS NOT NULL))
    THROW 51000, 'Persisted Life Matter comparison violates statistical gates.', 1;
SELECT COUNT(*) AS PersistedComparisonCount
FROM dbo.tbl_Fact_LifeMatterStatisticalComparisons;
PRINT '173 verification passed.';
GO
