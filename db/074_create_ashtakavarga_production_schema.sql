-- =====================================================================
-- 074 — Production (dbo) Ashtakavarga schema: rule matrix + reduction
--       rules + Bhinna / Sarva / Pinda fact tables + the UI read view.
--       Numeric matrix seed and the Ramakrishnan benchmark are in 075.
--       The calculators that populate the fact tables are a later CLI change.
--
-- Model (Parasari, matching the vendored MIT jyotishganit and the JHora
-- export): 7 recipients (Sun..Saturn), 8 contributors (Sun..Saturn + Lagna),
-- classical Sarvashtakavarga grand total 337. Lagna-as-recipient (JHora's
-- "As" row) is a JHora extra and is kept only as benchmark data in 075.
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- --- Rule layer ------------------------------------------------------------

IF OBJECT_ID('dbo.tbl_Rule_AshtakavargaContribution', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Rule_AshtakavargaContribution (
        Id                INT IDENTITY(1,1) CONSTRAINT PK_Rule_AshtakavargaContribution PRIMARY KEY,
        RuleSetId         TINYINT NOT NULL CONSTRAINT FK_Rule_AshtakavargaContribution_RuleSet REFERENCES dbo.tbl_Rule_Sets (Id),
        MethodCode        VARCHAR(50) NOT NULL CONSTRAINT DF_Rule_AshtakavargaContribution_Method DEFAULT ('PVR_PARASARA_BAV'),
        RecipientCode     VARCHAR(20) NOT NULL,
        ContributorCode   VARCHAR(20) NOT NULL,
        BeneficPlacesJson NVARCHAR(400) NOT NULL,
        SourceRefCode     VARCHAR(40) NOT NULL CONSTRAINT FK_Rule_AshtakavargaContribution_Source REFERENCES dbo.tbl_Dim_Source (Code),
        EvidenceLevelCode VARCHAR(30) NOT NULL CONSTRAINT DF_Rule_AshtakavargaContribution_Evidence DEFAULT ('DirectClassical'),
        SourceLocator     NVARCHAR(400) NULL,
        IsActive          BIT NOT NULL CONSTRAINT DF_Rule_AshtakavargaContribution_IsActive DEFAULT (1),
        CONSTRAINT CK_Rule_AshtakavargaContribution_Json CHECK (ISJSON(BeneficPlacesJson) = 1),
        CONSTRAINT CK_Rule_AshtakavargaContribution_Recipient CHECK
            (RecipientCode IN ('SUN','MOON','MARS','MERCURY','JUPITER','VENUS','SATURN')),
        CONSTRAINT CK_Rule_AshtakavargaContribution_Contributor CHECK
            (ContributorCode IN ('SUN','MOON','MARS','MERCURY','JUPITER','VENUS','SATURN','LAGNA')),
        CONSTRAINT UQ_Rule_AshtakavargaContribution UNIQUE (RuleSetId, MethodCode, RecipientCode, ContributorCode)
    );
END
GO

IF OBJECT_ID('dbo.tbl_Rule_AshtakavargaReduction', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Rule_AshtakavargaReduction (
        Id            INT IDENTITY(1,1) CONSTRAINT PK_Rule_AshtakavargaReduction PRIMARY KEY,
        RuleSetId     TINYINT NOT NULL CONSTRAINT FK_Rule_AshtakavargaReduction_RuleSet REFERENCES dbo.tbl_Rule_Sets (Id),
        MethodCode    VARCHAR(50) NOT NULL CONSTRAINT DF_Rule_AshtakavargaReduction_Method DEFAULT ('PVR_PARASARA_BAV'),
        StepCode      VARCHAR(40) NOT NULL,
        StepOrder     TINYINT NOT NULL,
        AlgorithmJson NVARCHAR(MAX) NOT NULL,
        SourceRefCode VARCHAR(40) NOT NULL CONSTRAINT FK_Rule_AshtakavargaReduction_Source REFERENCES dbo.tbl_Dim_Source (Code),
        SourceLocator NVARCHAR(400) NULL,
        IsActive      BIT NOT NULL CONSTRAINT DF_Rule_AshtakavargaReduction_IsActive DEFAULT (1),
        CONSTRAINT CK_Rule_AshtakavargaReduction_Json CHECK (ISJSON(AlgorithmJson) = 1),
        CONSTRAINT CK_Rule_AshtakavargaReduction_Step CHECK (StepCode IN ('TRIKONA_SODHANA','EKADHIPATYA_SODHANA')),
        CONSTRAINT UQ_Rule_AshtakavargaReduction UNIQUE (RuleSetId, MethodCode, StepCode)
    );
END
GO

-- --- Fact layer ----------------------------------------------------------

IF OBJECT_ID('dbo.tbl_Fact_BhinnaAshtakavarga', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Fact_BhinnaAshtakavarga (
        Id            INT IDENTITY(1,1) CONSTRAINT PK_Fact_BhinnaAshtakavarga PRIMARY KEY,
        ChartResultId INT NOT NULL CONSTRAINT FK_Fact_BhinnaAshtakavarga_ChartResult REFERENCES dbo.tbl_ChartResults (Id),
        RuleSetId     TINYINT NOT NULL CONSTRAINT FK_Fact_BhinnaAshtakavarga_RuleSet REFERENCES dbo.tbl_Rule_Sets (Id),
        MethodCode    VARCHAR(50) NOT NULL,
        RecipientCode VARCHAR(20) NOT NULL,
        SignNumber    TINYINT NOT NULL,
        BinduCount    TINYINT NOT NULL,
        SourceRefCode VARCHAR(40) NULL,
        ComputedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_Fact_BhinnaAshtakavarga_Computed DEFAULT (sysutcdatetime()),
        CONSTRAINT CK_Fact_BhinnaAshtakavarga_Sign  CHECK (SignNumber BETWEEN 1 AND 12),
        CONSTRAINT CK_Fact_BhinnaAshtakavarga_Bindu CHECK (BinduCount BETWEEN 0 AND 8),
        CONSTRAINT CK_Fact_BhinnaAshtakavarga_Src   CHECK (SourceRefCode IS NULL OR SourceRefCode LIKE 'SRC[_]%'),
        CONSTRAINT UQ_Fact_BhinnaAshtakavarga UNIQUE (ChartResultId, MethodCode, RecipientCode, SignNumber)
    );
    CREATE INDEX IX_Fact_BhinnaAshtakavarga_Chart ON dbo.tbl_Fact_BhinnaAshtakavarga (ChartResultId, RecipientCode);
END
GO

-- The 1/0 explain layer: which contributor earned each bindu (preserve-every-input).
IF OBJECT_ID('dbo.tbl_Fact_BhinnaAshtakavargaContribution', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Fact_BhinnaAshtakavargaContribution (
        Id                    INT IDENTITY(1,1) CONSTRAINT PK_Fact_BhinnaAshtakavargaContribution PRIMARY KEY,
        ChartResultId         INT NOT NULL CONSTRAINT FK_Fact_BhinnaAvContribution_ChartResult REFERENCES dbo.tbl_ChartResults (Id),
        RuleSetId             TINYINT NOT NULL CONSTRAINT FK_Fact_BhinnaAvContribution_RuleSet REFERENCES dbo.tbl_Rule_Sets (Id),
        MethodCode            VARCHAR(50) NOT NULL,
        RecipientCode         VARCHAR(20) NOT NULL,
        ContributorCode       VARCHAR(20) NOT NULL,
        SignNumber            TINYINT NOT NULL,
        IsBindu               BIT NOT NULL,
        ContributorSignNumber TINYINT NULL,
        HouseOffsetFromContributor TINYINT NULL,
        ComputedAtUtc         DATETIME2(0) NOT NULL CONSTRAINT DF_Fact_BhinnaAvContribution_Computed DEFAULT (sysutcdatetime()),
        CONSTRAINT CK_Fact_BhinnaAvContribution_Sign  CHECK (SignNumber BETWEEN 1 AND 12),
        CONSTRAINT CK_Fact_BhinnaAvContribution_CSign CHECK (ContributorSignNumber IS NULL OR ContributorSignNumber BETWEEN 1 AND 12),
        CONSTRAINT CK_Fact_BhinnaAvContribution_Off   CHECK (HouseOffsetFromContributor IS NULL OR HouseOffsetFromContributor BETWEEN 1 AND 12),
        CONSTRAINT UQ_Fact_BhinnaAvContribution UNIQUE (ChartResultId, MethodCode, RecipientCode, ContributorCode, SignNumber)
    );
    CREATE INDEX IX_Fact_BhinnaAvContribution_Chart ON dbo.tbl_Fact_BhinnaAshtakavargaContribution (ChartResultId, RecipientCode, SignNumber);
END
GO

IF OBJECT_ID('dbo.tbl_Fact_SarvaAshtakavarga', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Fact_SarvaAshtakavarga (
        Id            INT IDENTITY(1,1) CONSTRAINT PK_Fact_SarvaAshtakavarga PRIMARY KEY,
        ChartResultId INT NOT NULL CONSTRAINT FK_Fact_SarvaAshtakavarga_ChartResult REFERENCES dbo.tbl_ChartResults (Id),
        RuleSetId     TINYINT NOT NULL CONSTRAINT FK_Fact_SarvaAshtakavarga_RuleSet REFERENCES dbo.tbl_Rule_Sets (Id),
        MethodCode    VARCHAR(50) NOT NULL,
        SignNumber    TINYINT NOT NULL,
        TotalBindus   TINYINT NOT NULL,
        IncludesLagna BIT NOT NULL CONSTRAINT DF_Fact_SarvaAshtakavarga_IncludesLagna DEFAULT (0),
        ComputedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_Fact_SarvaAshtakavarga_Computed DEFAULT (sysutcdatetime()),
        CONSTRAINT CK_Fact_SarvaAshtakavarga_Sign CHECK (SignNumber BETWEEN 1 AND 12),
        CONSTRAINT CK_Fact_SarvaAshtakavarga_Total CHECK (TotalBindus BETWEEN 0 AND 56),
        CONSTRAINT UQ_Fact_SarvaAshtakavarga UNIQUE (ChartResultId, MethodCode, SignNumber)
    );
    CREATE INDEX IX_Fact_SarvaAshtakavarga_Chart ON dbo.tbl_Fact_SarvaAshtakavarga (ChartResultId);
END
GO

IF OBJECT_ID('dbo.tbl_Fact_AshtakavargaPinda', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Fact_AshtakavargaPinda (
        Id                    INT IDENTITY(1,1) CONSTRAINT PK_Fact_AshtakavargaPinda PRIMARY KEY,
        ChartResultId         INT NOT NULL CONSTRAINT FK_Fact_AshtakavargaPinda_ChartResult REFERENCES dbo.tbl_ChartResults (Id),
        RuleSetId             TINYINT NOT NULL CONSTRAINT FK_Fact_AshtakavargaPinda_RuleSet REFERENCES dbo.tbl_Rule_Sets (Id),
        MethodCode            VARCHAR(50) NOT NULL,
        RecipientCode         VARCHAR(20) NOT NULL,
        RasiPinda             SMALLINT NULL,
        GrahaPinda            SMALLINT NULL,
        SodhyaPinda           SMALLINT NULL,
        PostReductionSarvaJson NVARCHAR(MAX) NULL,
        ComputedAtUtc         DATETIME2(0) NOT NULL CONSTRAINT DF_Fact_AshtakavargaPinda_Computed DEFAULT (sysutcdatetime()),
        CONSTRAINT CK_Fact_AshtakavargaPinda_Json CHECK (PostReductionSarvaJson IS NULL OR ISJSON(PostReductionSarvaJson) = 1),
        CONSTRAINT UQ_Fact_AshtakavargaPinda UNIQUE (ChartResultId, MethodCode, RecipientCode)
    );
    CREATE INDEX IX_Fact_AshtakavargaPinda_Chart ON dbo.tbl_Fact_AshtakavargaPinda (ChartResultId);
END
GO

-- --- UI read view (project_standards.md section 4: table <-> view) -------

CREATE OR ALTER VIEW dbo.vw_ChartAshtakavarga
AS
SELECT c.BirthDetailId, f.ChartResultId, c.ChartType, f.RuleSetId, f.MethodCode,
       f.RecipientCode, f.SignNumber, f.BinduCount,
       sav.TotalBindus AS SarvaBindus, sav.IncludesLagna AS SarvaIncludesLagna,
       f.ComputedAtUtc
FROM dbo.tbl_Fact_BhinnaAshtakavarga f
JOIN dbo.tbl_ChartResults c ON c.Id = f.ChartResultId
LEFT JOIN dbo.tbl_Fact_SarvaAshtakavarga sav
       ON sav.ChartResultId = f.ChartResultId
      AND sav.MethodCode = f.MethodCode
      AND sav.SignNumber = f.SignNumber;
GO

INSERT dbo.SchemaMigrations (ScriptName, Note)
SELECT '074_create_ashtakavarga_production_schema.sql',
       'Production Ashtakavarga schema: Rule contribution/reduction tables, Bhinna/Sarva/Pinda fact tables, vw_ChartAshtakavarga.'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '074_create_ashtakavarga_production_schema.sql');
GO

PRINT '074 applied: Ashtakavarga production schema created.';
GO
