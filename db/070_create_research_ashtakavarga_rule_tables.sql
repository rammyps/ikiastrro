/* Research rule definitions for PVR-first Ashtakavarga. Numeric matrices are intentionally deferred. */
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name=N'research') EXEC(N'CREATE SCHEMA research AUTHORIZATION dbo');

IF OBJECT_ID(N'research.tbl_Rule_SourceReferenceAshtakavargaMethod','U') IS NULL
CREATE TABLE research.tbl_Rule_SourceReferenceAshtakavargaMethod
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    MethodCode VARCHAR(50) NOT NULL UNIQUE,
    SourceRefCode VARCHAR(40) NOT NULL,
    MethodName NVARCHAR(200) NOT NULL,
    RecipientScopeCode VARCHAR(40) NOT NULL,
    ContributorScopeCode VARCHAR(40) NOT NULL,
    Notes NVARCHAR(MAX) NULL,
    IsCanonical BIT NOT NULL CONSTRAINT DF_AshtakavargaMethod_Canonical DEFAULT 0
);

IF OBJECT_ID(N'research.tbl_Rule_SourceReferenceAshtakavargaContributor','U') IS NULL
CREATE TABLE research.tbl_Rule_SourceReferenceAshtakavargaContributor
(
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    MethodId INT NOT NULL REFERENCES research.tbl_Rule_SourceReferenceAshtakavargaMethod(Id),
    RecipientCode VARCHAR(30) NOT NULL,
    ContributorCode VARCHAR(30) NOT NULL,
    RuleValueJson NVARCHAR(MAX) NULL,
    SourceLocator NVARCHAR(1000) NULL,
    VerificationStatusCode VARCHAR(20) NOT NULL CONSTRAINT DF_AshtakavargaContributor_Status DEFAULT 'Unverified',
    Notes NVARCHAR(MAX) NULL,
    CONSTRAINT CK_AshtakavargaContributor_Json CHECK (RuleValueJson IS NULL OR ISJSON(RuleValueJson)=1),
    CONSTRAINT UQ_AshtakavargaContributor UNIQUE(MethodId,RecipientCode,ContributorCode)
);

IF OBJECT_ID(N'research.tbl_Rule_SourceReferenceAshtakavargaReduction','U') IS NULL
CREATE TABLE research.tbl_Rule_SourceReferenceAshtakavargaReduction
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    MethodId INT NOT NULL REFERENCES research.tbl_Rule_SourceReferenceAshtakavargaMethod(Id),
    StepCode VARCHAR(40) NOT NULL,
    StepOrder TINYINT NOT NULL,
    FormulaJson NVARCHAR(MAX) NULL,
    SourceLocator NVARCHAR(1000) NULL,
    VerificationStatusCode VARCHAR(20) NOT NULL CONSTRAINT DF_AshtakavargaReduction_Status DEFAULT 'Unverified',
    Notes NVARCHAR(MAX) NULL,
    CONSTRAINT CK_AshtakavargaReduction_Json CHECK (FormulaJson IS NULL OR ISJSON(FormulaJson)=1),
    CONSTRAINT UQ_AshtakavargaReduction UNIQUE(MethodId,StepCode)
);

INSERT research.tbl_Rule_SourceReferenceAshtakavargaMethod
    (MethodCode,SourceRefCode,MethodName,RecipientScopeCode,ContributorScopeCode,Notes,IsCanonical)
SELECT 'PVR_INTEGRATED_ASHTAKAVARGA','SRC_PVR_INTEGRATED',N'PVR integrated Ashtakavarga',
       'SUN_TO_SATURN','SUN_TO_SATURN_PLUS_LAGNA',
       N'Canonical research method. Numeric matrices require source verification before promotion.',1
WHERE NOT EXISTS (SELECT 1 FROM research.tbl_Rule_SourceReferenceAshtakavargaMethod WHERE MethodCode='PVR_INTEGRATED_ASHTAKAVARGA');

IF OBJECT_ID(N'dbo.SchemaMigrations','U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName=N'070_create_research_ashtakavarga_rule_tables.sql')
INSERT dbo.SchemaMigrations(ScriptName,Note) VALUES(N'070_create_research_ashtakavarga_rule_tables.sql',N'Create research-only PVR Ashtakavarga method, contributor-rule and reduction-rule definitions.');
