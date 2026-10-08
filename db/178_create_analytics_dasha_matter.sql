-- =====================================================================
-- 178 - v10 Dasha Matter analytics: feature rows, view, comparisons.
--
-- Feature contract KI_DASHA_MATTER_V1 describes the NATAL dasha-matter rule
-- structure only (planets meeting each rule). The running period is excluded
-- on purpose: it changes daily and would make a statistical run irreproducible.
-- Rows are written by the C# materialiser (the rules live in Core); Python then
-- describes them. A rule that could not be evaluated keeps TargetCount NULL
-- with a MissingReasonCode - it is never stored as zero.
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID('dbo.tbl_Fact_AnalyticsDashaMatterFeatures', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Fact_AnalyticsDashaMatterFeatures (
        Id                     BIGINT IDENTITY(1,1) CONSTRAINT PK_Fact_AnalyticsDashaMatterFeatures PRIMARY KEY,
        SubjectKey             UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_Fact_AnalyticsDashaMatterFeatures_Subject
                                   REFERENCES dbo.tbl_Dim_AnalyticsSubjects (SubjectKey),
        RuleNumber             INT NOT NULL,
        Varga                  VARCHAR(10) NOT NULL,
        ScopeKind              VARCHAR(16) NOT NULL,
        HouseNumber            TINYINT NULL,
        KarakaCode             VARCHAR(16) NULL,
        TargetCount            TINYINT NULL,
        MissingReasonCode      VARCHAR(32) NULL,
        FeatureContractVersion VARCHAR(32) NOT NULL,
        ComputedAtUtc          DATETIME2(0) NOT NULL CONSTRAINT DF_Fact_AnalyticsDashaMatterFeatures_Computed DEFAULT SYSUTCDATETIME(),
        CONSTRAINT UQ_Fact_AnalyticsDashaMatterFeatures UNIQUE (SubjectKey, RuleNumber, FeatureContractVersion),
        CONSTRAINT CK_Fact_AnalyticsDashaMatterFeatures_Scope CHECK
            (ScopeKind IN ('EXAMPLE','CHART_THEME','HOUSE','NATURAL_KARAKA')),
        CONSTRAINT CK_Fact_AnalyticsDashaMatterFeatures_Count CHECK (TargetCount IS NULL OR TargetCount BETWEEN 0 AND 9),
        CONSTRAINT CK_Fact_AnalyticsDashaMatterFeatures_Missing CHECK
            ((TargetCount IS NULL AND MissingReasonCode IN ('CHART_NOT_AVAILABLE','SOURCE_PARTIAL'))
             OR (TargetCount IS NOT NULL AND MissingReasonCode IS NULL))
    );
END
GO

CREATE OR ALTER VIEW dbo.vw_AnalyticsDashaMatterFeaturesV1
AS
SELECT feature.SubjectKey,
       feature.RuleNumber,
       feature.Varga,
       feature.ScopeKind,
       feature.HouseNumber,
       feature.KarakaCode,
       CAST(feature.TargetCount AS FLOAT) AS TargetCount,
       CAST(CASE WHEN feature.TargetCount IS NULL THEN NULL
                 WHEN feature.TargetCount > 0 THEN 100 ELSE 0 END AS FLOAT) AS Present,
       feature.MissingReasonCode,
       subject.BirthTimeQuality,
       feature.ComputedAtUtc,
       feature.FeatureContractVersion
FROM dbo.tbl_Fact_AnalyticsDashaMatterFeatures feature
JOIN dbo.tbl_Dim_AnalyticsSubjects subject ON subject.SubjectKey = feature.SubjectKey
WHERE subject.ResearchUseStatus = 'ELIGIBLE'
  AND subject.SubjectClassification IN ('RESEARCH', 'PERSONAL')
  AND subject.WithdrawnAtUtc IS NULL
  AND feature.FeatureContractVersion = 'KI_DASHA_MATTER_V1';
GO

IF OBJECT_ID('dbo.tbl_Fact_DashaMatterStatisticalComparisons', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Fact_DashaMatterStatisticalComparisons (
        Id                     BIGINT IDENTITY(1,1) CONSTRAINT PK_Fact_DashaMatterStatisticalComparisons PRIMARY KEY,
        AnalyticsRunId         BIGINT NOT NULL CONSTRAINT FK_Fact_DashaMatterStatisticalComparisons_Run
                                   REFERENCES dbo.tbl_Fact_AnalyticsRuns (Id),
        SubjectKey             UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_Fact_DashaMatterStatisticalComparisons_Subject
                                   REFERENCES dbo.tbl_Dim_AnalyticsSubjects (SubjectKey),
        RuleNumber             INT NOT NULL,
        Varga                  VARCHAR(10) NOT NULL,
        ScopeKind              VARCHAR(16) NOT NULL,
        HouseNumber            TINYINT NULL,
        KarakaCode             VARCHAR(16) NULL,
        FeatureCode            VARCHAR(48) NOT NULL,
        FeatureContractVersion VARCHAR(32) NOT NULL,
        PersonalValue          DECIMAL(9,4) NULL,
        EligibleCount          INT NOT NULL,
        MeasuredCount          INT NOT NULL,
        MissingRate            DECIMAL(9,6) NOT NULL,
        ReferenceMedian        DECIMAL(9,4) NULL,
        ReferenceQ1            DECIMAL(9,4) NULL,
        ReferenceQ3            DECIMAL(9,4) NULL,
        ReferenceMean          DECIMAL(9,4) NULL,
        ReferenceStdDev        DECIMAL(9,4) NULL,
        RobustZ                DECIMAL(12,6) NULL,
        Percentile             DECIMAL(9,6) NULL,
        MedianCiLow            DECIMAL(9,4) NULL,
        MedianCiHigh           DECIMAL(9,4) NULL,
        SufficiencyCode        VARCHAR(24) NOT NULL,
        SourceMissingReasonCode VARCHAR(32) NULL,
        ComputedAtUtc          DATETIME2(0) NOT NULL CONSTRAINT DF_Fact_DashaMatterStatisticalComparisons_Computed DEFAULT SYSUTCDATETIME(),
        CONSTRAINT UQ_Fact_DashaMatterStatisticalComparisons UNIQUE (AnalyticsRunId, SubjectKey, RuleNumber, FeatureCode),
        CONSTRAINT CK_Fact_DashaMatterStatisticalComparisons_Scope CHECK
            (ScopeKind IN ('EXAMPLE','CHART_THEME','HOUSE','NATURAL_KARAKA')),
        CONSTRAINT CK_Fact_DashaMatterStatisticalComparisons_Feature CHECK
            (FeatureCode IN ('KI_DM_TARGET_COUNT_V1','KI_DM_PRESENT_V1')),
        CONSTRAINT CK_Fact_DashaMatterStatisticalComparisons_Sufficiency CHECK
            (SufficiencyCode IN ('INSUFFICIENT','EXPLORATORY','SUFFICIENT','INCOMPLETE')),
        CONSTRAINT CK_Fact_DashaMatterStatisticalComparisons_PercentileGate CHECK
            ((MeasuredCount < 30 AND Percentile IS NULL) OR MeasuredCount >= 30),
        CONSTRAINT CK_Fact_DashaMatterStatisticalComparisons_Counts CHECK
            (EligibleCount >= 0 AND MeasuredCount >= 0 AND MeasuredCount <= EligibleCount),
        CONSTRAINT CK_Fact_DashaMatterStatisticalComparisons_MissingRate CHECK
            (MissingRate BETWEEN 0 AND 1)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_Catalog WHERE RuleTableName = 'vw_AnalyticsDashaMatterFeaturesV1')
    INSERT dbo.tbl_Rule_Catalog (RuleTableName, EngineCode, MethodCodes, Purpose, IntroducedIn)
    VALUES ('vw_AnalyticsDashaMatterFeaturesV1', 'STATISTICS', 'DASHA_MATTER_V1',
            'Privacy-gated natal dasha-matter rule observations (planets meeting each rule) for population statistics.',
            'migration 178');
GO

IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '178_create_analytics_dasha_matter.sql')
    INSERT dbo.SchemaMigrations (ScriptName, AppliedAtUtc, Note)
    VALUES ('178_create_analytics_dasha_matter.sql', SYSUTCDATETIME(),
            'Adds v10 Dasha Matter feature table/view and descriptive-comparison persistence (natal rule structure only).');
GO
PRINT '178 applied: Dasha Matter analytics.';
GO
