-- Verification for migration 172. Read-only; throws on a contract breach.
USE [ikiastrro];
GO
SET NOCOUNT ON;
GO

IF OBJECT_ID('dbo.vw_AnalyticsLifeMatterFeaturesV1', 'V') IS NULL
    THROW 51000, 'vw_AnalyticsLifeMatterFeaturesV1 is missing.', 1;

IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.vw_AnalyticsLifeMatterFeaturesV1')
      AND name IN ('BirthDetailId', 'Name', 'DateOfBirth', 'TimeOfBirth'))
    THROW 51000, 'Analytics view exposes a direct identity or birth-input field.', 1;

IF EXISTS (
    SELECT SubjectKey, LifeMatterFocusId, EvidenceLensCode, COUNT(*)
    FROM dbo.vw_AnalyticsLifeMatterFeaturesV1
    GROUP BY SubjectKey, LifeMatterFocusId, EvidenceLensCode
    HAVING COUNT(*) > 1)
    THROW 51000, 'Analytics Life Matter observation grain is not unique.', 1;

IF EXISTS (
    SELECT 1 FROM dbo.vw_AnalyticsLifeMatterFeaturesV1
    WHERE EvidenceLensCode NOT IN ('D1_PROMISE', 'VARGA_CONFIRMATION')
       OR FeatureContractVersion <> 'KI_LIFE_MATTER_VARGA_V1'
       OR MissingReasonCode NOT IN
          ('UNSUPPORTED_REFERENCE', 'CHART_NOT_AVAILABLE',
           'HOUSE_STATISTICS_NOT_AVAILABLE', 'SOURCE_PARTIAL'))
    THROW 51000, 'Analytics Life Matter controlled vocabulary is invalid.', 1;

IF EXISTS (
    SELECT 1 FROM dbo.vw_AnalyticsLifeMatterFeaturesV1
    WHERE Capacity NOT BETWEEN 0 AND 100
       OR Consistency NOT BETWEEN 0 AND 100
       OR Context NOT BETWEEN 0 AND 100
       OR OverallSupport NOT BETWEEN 0 AND 100)
    THROW 51000, 'Analytics Life Matter feature lies outside 0-100.', 1;

IF EXISTS (
    SELECT 1 FROM dbo.vw_AnalyticsLifeMatterFeaturesV1
    WHERE MissingReasonCode IS NULL
      AND (ChartResultId IS NULL OR Capacity IS NULL OR Consistency IS NULL
           OR Context IS NULL OR OverallSupport IS NULL))
    THROW 51000, 'A measured observation is incomplete without a missing reason.', 1;

SELECT EvidenceLensCode, ChartType, MissingReasonCode, COUNT(*) AS ObservationCount
FROM dbo.vw_AnalyticsLifeMatterFeaturesV1
GROUP BY EvidenceLensCode, ChartType, MissingReasonCode
ORDER BY EvidenceLensCode, ChartType, MissingReasonCode;

PRINT '172 verification passed.';
GO
