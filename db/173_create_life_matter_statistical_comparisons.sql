-- =====================================================================
-- 173 - Persist v10 Life Matter + Varga descriptive comparisons.
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID('dbo.tbl_Fact_LifeMatterStatisticalComparisons', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Fact_LifeMatterStatisticalComparisons (
        Id                     BIGINT IDENTITY(1,1) CONSTRAINT PK_Fact_LifeMatterStatisticalComparisons PRIMARY KEY,
        AnalyticsRunId         BIGINT NOT NULL CONSTRAINT FK_Fact_LifeMatterStatisticalComparisons_Run
                                   REFERENCES dbo.tbl_Fact_AnalyticsRuns (Id),
        SubjectKey             UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_Fact_LifeMatterStatisticalComparisons_Subject
                                   REFERENCES dbo.tbl_Dim_AnalyticsSubjects (SubjectKey),
        LifeMatterFocusId      INT NOT NULL CONSTRAINT FK_Fact_LifeMatterStatisticalComparisons_Focus
                                   REFERENCES dbo.tbl_Rule_LifeMatterFocus (Id),
        EvidenceLensCode       VARCHAR(24) NOT NULL,
        ChartResultId          INT NULL CONSTRAINT FK_Fact_LifeMatterStatisticalComparisons_Chart
                                   REFERENCES dbo.tbl_ChartResults (Id),
        ChartType              VARCHAR(10) NOT NULL,
        HouseFromLagna         TINYINT NULL,
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
        ComputedAtUtc          DATETIME2(0) NOT NULL CONSTRAINT DF_Fact_LifeMatterStatisticalComparisons_Computed DEFAULT SYSUTCDATETIME(),
        CONSTRAINT UQ_Fact_LifeMatterStatisticalComparisons UNIQUE
            (AnalyticsRunId, SubjectKey, LifeMatterFocusId, EvidenceLensCode, FeatureCode),
        CONSTRAINT CK_Fact_LifeMatterStatisticalComparisons_Lens CHECK
            (EvidenceLensCode IN ('D1_PROMISE','VARGA_CONFIRMATION')),
        CONSTRAINT CK_Fact_LifeMatterStatisticalComparisons_House CHECK
            (HouseFromLagna IS NULL OR HouseFromLagna BETWEEN 1 AND 12),
        CONSTRAINT CK_Fact_LifeMatterStatisticalComparisons_Feature CHECK (FeatureCode IN
            ('KI_LM_CAPACITY_V1','KI_LM_CONSISTENCY_V1','KI_LM_CONTEXT_V1','KI_LM_SUPPORT_V1')),
        CONSTRAINT CK_Fact_LifeMatterStatisticalComparisons_Sufficiency CHECK
            (SufficiencyCode IN ('INSUFFICIENT','EXPLORATORY','SUFFICIENT','INCOMPLETE')),
        CONSTRAINT CK_Fact_LifeMatterStatisticalComparisons_PercentileGate CHECK
            ((MeasuredCount < 30 AND Percentile IS NULL) OR MeasuredCount >= 30),
        CONSTRAINT CK_Fact_LifeMatterStatisticalComparisons_Counts CHECK
            (EligibleCount >= 0 AND MeasuredCount >= 0 AND MeasuredCount <= EligibleCount),
        CONSTRAINT CK_Fact_LifeMatterStatisticalComparisons_MissingRate CHECK
            (MissingRate BETWEEN 0 AND 1)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '173_create_life_matter_statistical_comparisons.sql')
    INSERT dbo.SchemaMigrations (ScriptName, AppliedAtUtc, Note)
    VALUES ('173_create_life_matter_statistical_comparisons.sql', SYSUTCDATETIME(),
            'Adds versioned persistence for descriptive Life Matter D1/Varga comparisons.');
GO
PRINT '173 applied: Life Matter statistical-comparison persistence.';
GO
