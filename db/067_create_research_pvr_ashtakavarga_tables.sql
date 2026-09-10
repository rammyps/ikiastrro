/* Isolated PVR-first Ashtakavarga research model.
   Research only: no production strength/fact tables or calculation code are changed. */
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name=N'research')
    EXEC(N'CREATE SCHEMA research AUTHORIZATION dbo');

IF OBJECT_ID(N'research.tbl_Dim_SourceReferenceAshtakavarga','U') IS NULL
BEGIN
    CREATE TABLE research.tbl_Dim_SourceReferenceAshtakavarga
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SourceReferenceAshtakavarga PRIMARY KEY,
        MethodVariantCode VARCHAR(50) NOT NULL,
        SystemCode VARCHAR(40) NOT NULL,
        DisplayName NVARCHAR(200) NOT NULL,
        Description NVARCHAR(1000) NULL,
        SourceRefCode VARCHAR(40) NOT NULL,
        SourceLocator NVARCHAR(1000) NULL,
        SourceUrl NVARCHAR(1000) NULL,
        ResearchStatus VARCHAR(20) NOT NULL CONSTRAINT DF_SourceReferenceAshtakavarga_Status DEFAULT('Proposed'),
        CONSTRAINT UQ_SourceReferenceAshtakavarga_Method UNIQUE(MethodVariantCode,SystemCode)
    );
END;

IF OBJECT_ID(N'research.tbl_Dim_SourceReferenceAshtakavargaText','U') IS NULL
BEGIN
    CREATE TABLE research.tbl_Dim_SourceReferenceAshtakavargaText
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SourceReferenceAshtakavargaText PRIMARY KEY,
        AshtakavargaReferenceId INT NOT NULL,
        WorkTitle NVARCHAR(300) NOT NULL,
        Author NVARCHAR(200) NULL,
        Edition NVARCHAR(300) NULL,
        Chapter NVARCHAR(100) NULL,
        VerseOrPage NVARCHAR(100) NULL,
        TextTypeCode VARCHAR(30) NOT NULL,
        FullText NVARCHAR(MAX) NULL,
        ResearchSummary NVARCHAR(MAX) NULL,
        CopyrightStatus VARCHAR(30) NOT NULL,
        SourceLocator NVARCHAR(1000) NULL,
        SourceUrl NVARCHAR(1000) NULL,
        CONSTRAINT FK_SourceReferenceAshtakavargaText_Reference FOREIGN KEY(AshtakavargaReferenceId) REFERENCES research.tbl_Dim_SourceReferenceAshtakavarga(Id)
    );
END;

IF OBJECT_ID(N'research.tbl_Dim_SourceReferenceAshtakavargaContribution','U') IS NULL
BEGIN
    CREATE TABLE research.tbl_Dim_SourceReferenceAshtakavargaContribution
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SourceReferenceAshtakavargaContribution PRIMARY KEY,
        AshtakavargaReferenceId INT NOT NULL,
        RecipientCode VARCHAR(30) NOT NULL,
        ContributorCode VARCHAR(30) NOT NULL,
        PositionBasisCode VARCHAR(30) NOT NULL CONSTRAINT DF_SourceReferenceAshtakavargaContribution_Basis DEFAULT('FROM_CONTRIBUTOR_SIGN'),
        BeneficPositionsJson NVARCHAR(MAX) NOT NULL,
        SourceRefCode VARCHAR(40) NOT NULL,
        SourceLocator NVARCHAR(1000) NULL,
        EvidenceLevelCode VARCHAR(30) NOT NULL CONSTRAINT DF_SourceReferenceAshtakavargaContribution_Evidence DEFAULT('DirectClassical'),
        ReviewerNotes NVARCHAR(MAX) NULL,
        CONSTRAINT FK_SourceReferenceAshtakavargaContribution_Reference FOREIGN KEY(AshtakavargaReferenceId) REFERENCES research.tbl_Dim_SourceReferenceAshtakavarga(Id),
        CONSTRAINT CK_SourceReferenceAshtakavargaContribution_Json CHECK(ISJSON(BeneficPositionsJson)=1),
        CONSTRAINT UQ_SourceReferenceAshtakavargaContribution UNIQUE(AshtakavargaReferenceId,RecipientCode,ContributorCode)
    );
END;

IF OBJECT_ID(N'research.tbl_Dim_SourceReferenceAshtakavargaReduction','U') IS NULL
BEGIN
    CREATE TABLE research.tbl_Dim_SourceReferenceAshtakavargaReduction
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SourceReferenceAshtakavargaReduction PRIMARY KEY,
        AshtakavargaReferenceId INT NOT NULL,
        ReductionCode VARCHAR(40) NOT NULL,
        InputMetricCode VARCHAR(40) NOT NULL,
        OutputMetricCode VARCHAR(40) NOT NULL,
        AlgorithmJson NVARCHAR(MAX) NOT NULL,
        SourceRefCode VARCHAR(40) NOT NULL,
        SourceLocator NVARCHAR(1000) NULL,
        EvidenceLevelCode VARCHAR(30) NOT NULL CONSTRAINT DF_SourceReferenceAshtakavargaReduction_Evidence DEFAULT('DirectClassical'),
        ReviewerNotes NVARCHAR(MAX) NULL,
        CONSTRAINT FK_SourceReferenceAshtakavargaReduction_Reference FOREIGN KEY(AshtakavargaReferenceId) REFERENCES research.tbl_Dim_SourceReferenceAshtakavarga(Id),
        CONSTRAINT CK_SourceReferenceAshtakavargaReduction_Json CHECK(ISJSON(AlgorithmJson)=1),
        CONSTRAINT UQ_SourceReferenceAshtakavargaReduction UNIQUE(AshtakavargaReferenceId,ReductionCode)
    );
END;

IF OBJECT_ID(N'research.tbl_Dim_SourceReferenceAshtakavargaInterpretation','U') IS NULL
BEGIN
    CREATE TABLE research.tbl_Dim_SourceReferenceAshtakavargaInterpretation
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SourceReferenceAshtakavargaInterpretation PRIMARY KEY,
        AshtakavargaReferenceId INT NOT NULL,
        MetricCode VARCHAR(40) NOT NULL,
        ComparisonOperator VARCHAR(10) NOT NULL,
        ThresholdValue DECIMAL(10,3) NOT NULL,
        InterpretationCode VARCHAR(80) NOT NULL,
        ContextCode VARCHAR(40) NULL,
        SourceRefCode VARCHAR(40) NOT NULL,
        SourceLocator NVARCHAR(1000) NULL,
        EvidenceLevelCode VARCHAR(30) NOT NULL CONSTRAINT DF_SourceReferenceAshtakavargaInterpretation_Evidence DEFAULT('Interpretive'),
        ReviewerNotes NVARCHAR(MAX) NULL,
        CONSTRAINT FK_SourceReferenceAshtakavargaInterpretation_Reference FOREIGN KEY(AshtakavargaReferenceId) REFERENCES research.tbl_Dim_SourceReferenceAshtakavarga(Id),
        CONSTRAINT UQ_SourceReferenceAshtakavargaInterpretation UNIQUE(AshtakavargaReferenceId,MetricCode,ComparisonOperator,ThresholdValue,ContextCode)
    );
END;

IF OBJECT_ID(N'dbo.SchemaMigrations','U') IS NOT NULL AND NOT EXISTS(SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName=N'067_create_research_pvr_ashtakavarga_tables.sql')
INSERT dbo.SchemaMigrations(ScriptName,Note) VALUES(N'067_create_research_pvr_ashtakavarga_tables.sql',N'Create isolated PVR-first Ashtakavarga research tables for contribution matrices, reductions and interpretations.');
