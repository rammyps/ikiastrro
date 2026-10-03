-- =====================================================================
-- 163 - v10 statistical analytics foundation.
--
-- Adds explicit opt-in research governance, anonymous analytics views,
-- reproducible dataset/run metadata, and descriptive comparison output.
-- Existing people are intentionally NOT enrolled: no row in
-- tbl_Dim_AnalyticsSubjects means not eligible.
--
-- Apply: sqlcmd -S "localhost\SQLSERVER2025" -E -d ikiastrro -b -i db/163_create_statistical_analytics_foundation.sql
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID('dbo.tbl_Dim_AnalyticsSubjects', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Dim_AnalyticsSubjects (
        Id                    INT IDENTITY(1,1) CONSTRAINT PK_Dim_AnalyticsSubjects PRIMARY KEY,
        BirthDetailId         INT NOT NULL CONSTRAINT FK_Dim_AnalyticsSubjects_BirthDetails
                                  REFERENCES dbo.tbl_BirthDetails (Id),
        SubjectKey            UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Dim_AnalyticsSubjects_SubjectKey DEFAULT NEWID(),
        ResearchUseStatus     VARCHAR(16) NOT NULL,
        SubjectClassification VARCHAR(16) NOT NULL,
        BirthTimeQuality      VARCHAR(20) NOT NULL CONSTRAINT DF_Dim_AnalyticsSubjects_BirthTimeQuality DEFAULT 'UNKNOWN',
        ConsentRecordedAtUtc  DATETIME2(0) NULL,
        WithdrawnAtUtc        DATETIME2(0) NULL,
        CreatedAtUtc          DATETIME2(0) NOT NULL CONSTRAINT DF_Dim_AnalyticsSubjects_Created DEFAULT SYSUTCDATETIME(),
        UpdatedAtUtc          DATETIME2(0) NOT NULL CONSTRAINT DF_Dim_AnalyticsSubjects_Updated DEFAULT SYSUTCDATETIME(),
        CONSTRAINT UQ_Dim_AnalyticsSubjects_BirthDetail UNIQUE (BirthDetailId),
        CONSTRAINT UQ_Dim_AnalyticsSubjects_SubjectKey UNIQUE (SubjectKey),
        CONSTRAINT CK_Dim_AnalyticsSubjects_ResearchUseStatus CHECK
            (ResearchUseStatus IN ('PENDING','ELIGIBLE','EXCLUDED','WITHDRAWN')),
        CONSTRAINT CK_Dim_AnalyticsSubjects_Classification CHECK
            (SubjectClassification IN ('RESEARCH','PERSONAL','TEST','DEMO','SYNTHETIC')),
        CONSTRAINT CK_Dim_AnalyticsSubjects_BirthTimeQuality CHECK
            (BirthTimeQuality IN ('UNKNOWN','APPROXIMATE','RECORDED','RECTIFIED')),
        CONSTRAINT CK_Dim_AnalyticsSubjects_Consent CHECK
            ((ResearchUseStatus <> 'ELIGIBLE') OR ConsentRecordedAtUtc IS NOT NULL),
        CONSTRAINT CK_Dim_AnalyticsSubjects_Withdrawal CHECK
            ((ResearchUseStatus <> 'WITHDRAWN') OR WithdrawnAtUtc IS NOT NULL)
    );
END
GO

IF OBJECT_ID('dbo.tbl_Dim_AnalyticsDatasets', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Dim_AnalyticsDatasets (
        Id                    INT IDENTITY(1,1) CONSTRAINT PK_Dim_AnalyticsDatasets PRIMARY KEY,
        DatasetCode           VARCHAR(64) NOT NULL,
        DatasetVersion        INT NOT NULL,
        FeatureContractVersion VARCHAR(32) NOT NULL,
        Description           NVARCHAR(500) NOT NULL,
        InclusionCriteriaJson NVARCHAR(MAX) NOT NULL,
        GitCommit             VARCHAR(64) NULL,
        RuleSetId             TINYINT NOT NULL CONSTRAINT FK_Dim_AnalyticsDatasets_RuleSet REFERENCES dbo.tbl_Rule_Sets (Id),
        IsDevelopmentOnly     BIT NOT NULL CONSTRAINT DF_Dim_AnalyticsDatasets_Development DEFAULT 1,
        CreatedAtUtc          DATETIME2(0) NOT NULL CONSTRAINT DF_Dim_AnalyticsDatasets_Created DEFAULT SYSUTCDATETIME(),
        CONSTRAINT UQ_Dim_AnalyticsDatasets_CodeVersion UNIQUE (DatasetCode, DatasetVersion),
        CONSTRAINT CK_Dim_AnalyticsDatasets_CriteriaJson CHECK (ISJSON(InclusionCriteriaJson) = 1)
    );
END
GO

IF OBJECT_ID('dbo.tbl_Fact_AnalyticsRuns', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Fact_AnalyticsRuns (
        Id                    BIGINT IDENTITY(1,1) CONSTRAINT PK_Fact_AnalyticsRuns PRIMARY KEY,
        DatasetId             INT NOT NULL CONSTRAINT FK_Fact_AnalyticsRuns_Dataset REFERENCES dbo.tbl_Dim_AnalyticsDatasets (Id),
        RunKey                UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Fact_AnalyticsRuns_RunKey DEFAULT NEWID(),
        MethodVersion         VARCHAR(32) NOT NULL,
        RandomSeed            INT NOT NULL,
        BootstrapIterations   INT NOT NULL,
        StatusCode            VARCHAR(16) NOT NULL,
        StartedAtUtc          DATETIME2(0) NOT NULL CONSTRAINT DF_Fact_AnalyticsRuns_Started DEFAULT SYSUTCDATETIME(),
        CompletedAtUtc        DATETIME2(0) NULL,
        ErrorMessage          NVARCHAR(2000) NULL,
        CONSTRAINT UQ_Fact_AnalyticsRuns_RunKey UNIQUE (RunKey),
        CONSTRAINT CK_Fact_AnalyticsRuns_Status CHECK (StatusCode IN ('STARTED','COMPLETED','FAILED')),
        CONSTRAINT CK_Fact_AnalyticsRuns_Bootstrap CHECK (BootstrapIterations >= 100)
    );
END
GO

IF OBJECT_ID('dbo.tbl_Fact_StatisticalComparisons', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Fact_StatisticalComparisons (
        Id                    BIGINT IDENTITY(1,1) CONSTRAINT PK_Fact_StatisticalComparisons PRIMARY KEY,
        AnalyticsRunId        BIGINT NOT NULL CONSTRAINT FK_Fact_StatisticalComparisons_Run REFERENCES dbo.tbl_Fact_AnalyticsRuns (Id),
        SubjectKey            UNIQUEIDENTIFIER NOT NULL,
        ChartResultId         INT NOT NULL CONSTRAINT FK_Fact_StatisticalComparisons_ChartResult REFERENCES dbo.tbl_ChartResults (Id),
        HouseFromLagna        TINYINT NOT NULL,
        FeatureCode           VARCHAR(48) NOT NULL,
        PersonalValue         DECIMAL(9,4) NULL,
        EligibleCount         INT NOT NULL,
        MeasuredCount         INT NOT NULL,
        MissingRate           DECIMAL(9,6) NOT NULL,
        ReferenceMedian       DECIMAL(9,4) NULL,
        ReferenceQ1           DECIMAL(9,4) NULL,
        ReferenceQ3           DECIMAL(9,4) NULL,
        ReferenceMean         DECIMAL(9,4) NULL,
        ReferenceStdDev       DECIMAL(9,4) NULL,
        RobustZ               DECIMAL(12,6) NULL,
        Percentile            DECIMAL(9,6) NULL,
        MedianCiLow           DECIMAL(9,4) NULL,
        MedianCiHigh          DECIMAL(9,4) NULL,
        SufficiencyCode       VARCHAR(24) NOT NULL,
        ComputedAtUtc         DATETIME2(0) NOT NULL CONSTRAINT DF_Fact_StatisticalComparisons_Computed DEFAULT SYSUTCDATETIME(),
        CONSTRAINT UQ_Fact_StatisticalComparisons UNIQUE
            (AnalyticsRunId, SubjectKey, ChartResultId, HouseFromLagna, FeatureCode),
        CONSTRAINT CK_Fact_StatisticalComparisons_House CHECK (HouseFromLagna BETWEEN 1 AND 12),
        CONSTRAINT CK_Fact_StatisticalComparisons_Feature CHECK (FeatureCode IN
            ('KI_D1_HOUSE_CAPACITY_V1','KI_D1_HOUSE_CONSISTENCY_V1','KI_D1_HOUSE_CONTEXT_V1','KI_D1_HOUSE_SUPPORT_V1')),
        CONSTRAINT CK_Fact_StatisticalComparisons_Sufficiency CHECK
            (SufficiencyCode IN ('INSUFFICIENT','EXPLORATORY','SUFFICIENT','INCOMPLETE')),
        CONSTRAINT CK_Fact_StatisticalComparisons_PercentileGate CHECK
            ((MeasuredCount < 30 AND Percentile IS NULL) OR MeasuredCount >= 30)
    );
END
GO

CREATE OR ALTER VIEW dbo.vw_AnalyticsHouseFeatures
AS
SELECT subject.SubjectKey,
       stats.ChartResultId,
       chart.ChartType,
       stats.HouseFromLagna,
       stats.Capacity,
       stats.Consistency,
       stats.Context,
       stats.StrengthPercent AS OverallSupport,
       stats.RuleSetId,
       chart.Ayanamsha,
       chart.HouseSystem,
       chart.EngineVersion,
       subject.BirthTimeQuality,
       stats.ComputedAtUtc
FROM dbo.tbl_Dim_AnalyticsSubjects subject
JOIN dbo.tbl_ChartResults chart ON chart.BirthDetailId = subject.BirthDetailId
JOIN dbo.tbl_Fact_HouseStrengthStatistics stats ON stats.ChartResultId = chart.Id
WHERE subject.ResearchUseStatus = 'ELIGIBLE'
  AND subject.SubjectClassification = 'RESEARCH'
  AND subject.WithdrawnAtUtc IS NULL
  AND chart.ChartType = 'D1';
GO

CREATE OR ALTER VIEW dbo.vw_AnalyticsCohortReadiness
AS
SELECT stats.RuleSetId,
       COUNT(DISTINCT subject.SubjectKey) AS EligiblePeople,
       COUNT(DISTINCT CASE WHEN complete.ChartResultId IS NOT NULL THEN subject.SubjectKey END) AS CompletePeople,
       MIN(stats.ComputedAtUtc) AS OldestFeatureAtUtc,
       MAX(stats.ComputedAtUtc) AS NewestFeatureAtUtc
FROM dbo.tbl_Dim_AnalyticsSubjects subject
JOIN dbo.tbl_ChartResults chart ON chart.BirthDetailId = subject.BirthDetailId AND chart.ChartType = 'D1'
JOIN dbo.tbl_Fact_HouseStrengthStatistics stats ON stats.ChartResultId = chart.Id
LEFT JOIN (
    SELECT ChartResultId
    FROM dbo.tbl_Fact_HouseStrengthStatistics
    GROUP BY ChartResultId
    HAVING COUNT(*) = 12
       AND COUNT(Capacity) = 12 AND COUNT(Consistency) = 12
       AND COUNT(Context) = 12 AND COUNT(StrengthPercent) = 12
) complete ON complete.ChartResultId = chart.Id
WHERE subject.ResearchUseStatus = 'ELIGIBLE'
  AND subject.SubjectClassification = 'RESEARCH'
  AND subject.WithdrawnAtUtc IS NULL
GROUP BY stats.RuleSetId;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '163_create_statistical_analytics_foundation.sql')
    INSERT dbo.SchemaMigrations (ScriptName, AppliedAtUtc, Note)
    VALUES ('163_create_statistical_analytics_foundation.sql', SYSUTCDATETIME(),
            'v10 opt-in research governance, anonymous feature view, datasets, runs and descriptive comparisons.');
GO

PRINT '163 applied: v10 statistical analytics foundation.';
GO
