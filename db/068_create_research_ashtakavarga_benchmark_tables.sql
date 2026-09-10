/* Benchmark storage for comparing PVR, Jagannatha Hora and Horoscope Explorer
   Ashtakavarga outputs. Research-only; no production chart facts are changed. */
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name=N'research')
    EXEC(N'CREATE SCHEMA research AUTHORIZATION dbo');

IF OBJECT_ID(N'research.tbl_Dim_SourceReferenceAshtakavargaBenchmarkCase','U') IS NULL
BEGIN
    CREATE TABLE research.tbl_Dim_SourceReferenceAshtakavargaBenchmarkCase
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SourceReferenceAshtakavargaBenchmarkCase PRIMARY KEY,
        CaseCode VARCHAR(80) NOT NULL,
        SubjectLabel NVARCHAR(200) NULL,
        BirthDateTimeUtc DATETIME2(0) NULL,
        Latitude DECIMAL(10,7) NULL,
        Longitude DECIMAL(10,7) NULL,
        TimeZoneOffsetMinutes SMALLINT NULL,
        AyanamsaCode VARCHAR(40) NULL,
        ChartConventionCode VARCHAR(40) NULL,
        Notes NVARCHAR(MAX) NULL,
        CONSTRAINT UQ_SourceReferenceAshtakavargaBenchmarkCase UNIQUE(CaseCode)
    );
END;

IF OBJECT_ID(N'research.tbl_Dim_SourceReferenceAshtakavargaBenchmarkRun','U') IS NULL
BEGIN
    CREATE TABLE research.tbl_Dim_SourceReferenceAshtakavargaBenchmarkRun
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SourceReferenceAshtakavargaBenchmarkRun PRIMARY KEY,
        CaseId INT NOT NULL,
        SourceSystemCode VARCHAR(40) NOT NULL,
        SourceVersion NVARCHAR(100) NULL,
        MethodVariantCode VARCHAR(50) NULL,
        SourceRefCode VARCHAR(40) NULL,
        ExportFileName NVARCHAR(300) NULL,
        CapturedAtUtc DATETIME2(0) NULL,
        RawArtifactPath NVARCHAR(1000) NULL,
        Notes NVARCHAR(MAX) NULL,
        CONSTRAINT FK_SourceReferenceAshtakavargaBenchmarkRun_Case FOREIGN KEY(CaseId) REFERENCES research.tbl_Dim_SourceReferenceAshtakavargaBenchmarkCase(Id)
    );
END;

IF OBJECT_ID(N'research.tbl_Dim_SourceReferenceAshtakavargaBenchmarkCell','U') IS NULL
BEGIN
    CREATE TABLE research.tbl_Dim_SourceReferenceAshtakavargaBenchmarkCell
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SourceReferenceAshtakavargaBenchmarkCell PRIMARY KEY,
        RunId BIGINT NOT NULL,
        MetricCode VARCHAR(40) NOT NULL,
        RecipientCode VARCHAR(30) NULL,
        ContributorCode VARCHAR(30) NULL,
        SignNumber TINYINT NOT NULL,
        HouseNumber TINYINT NULL,
        BinduValue TINYINT NULL,
        RawValue NVARCHAR(100) NULL,
        SourceLocator NVARCHAR(1000) NULL,
        CONSTRAINT FK_SourceReferenceAshtakavargaBenchmarkCell_Run FOREIGN KEY(RunId) REFERENCES research.tbl_Dim_SourceReferenceAshtakavargaBenchmarkRun(Id),
        CONSTRAINT CK_SourceReferenceAshtakavargaBenchmarkCell_Sign CHECK(SignNumber BETWEEN 1 AND 12),
        CONSTRAINT CK_SourceReferenceAshtakavargaBenchmarkCell_House CHECK(HouseNumber IS NULL OR HouseNumber BETWEEN 1 AND 12),
        CONSTRAINT UQ_SourceReferenceAshtakavargaBenchmarkCell UNIQUE(RunId,MetricCode,RecipientCode,ContributorCode,SignNumber)
    );
END;

IF OBJECT_ID(N'research.tbl_Dim_SourceReferenceAshtakavargaBenchmarkDifference','U') IS NULL
BEGIN
    CREATE TABLE research.tbl_Dim_SourceReferenceAshtakavargaBenchmarkDifference
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SourceReferenceAshtakavargaBenchmarkDifference PRIMARY KEY,
        CaseId INT NOT NULL,
        LeftRunId BIGINT NOT NULL,
        RightRunId BIGINT NOT NULL,
        MetricCode VARCHAR(40) NOT NULL,
        RecipientCode VARCHAR(30) NULL,
        ContributorCode VARCHAR(30) NULL,
        SignNumber TINYINT NULL,
        LeftValue NVARCHAR(100) NULL,
        RightValue NVARCHAR(100) NULL,
        DifferenceValue INT NULL,
        DifferenceClassCode VARCHAR(30) NOT NULL,
        AnalysisNotes NVARCHAR(MAX) NULL,
        CONSTRAINT FK_SourceReferenceAshtakavargaBenchmarkDifference_Case FOREIGN KEY(CaseId) REFERENCES research.tbl_Dim_SourceReferenceAshtakavargaBenchmarkCase(Id),
        CONSTRAINT FK_SourceReferenceAshtakavargaBenchmarkDifference_Left FOREIGN KEY(LeftRunId) REFERENCES research.tbl_Dim_SourceReferenceAshtakavargaBenchmarkRun(Id),
        CONSTRAINT FK_SourceReferenceAshtakavargaBenchmarkDifference_Right FOREIGN KEY(RightRunId) REFERENCES research.tbl_Dim_SourceReferenceAshtakavargaBenchmarkRun(Id)
    );
END;

IF OBJECT_ID(N'dbo.SchemaMigrations','U') IS NOT NULL AND NOT EXISTS(SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName=N'068_create_research_ashtakavarga_benchmark_tables.sql')
INSERT dbo.SchemaMigrations(ScriptName,Note) VALUES(N'068_create_research_ashtakavarga_benchmark_tables.sql',N'Create research-only benchmark case, source-run, cell and difference tables for Ashtakavarga comparisons.');
