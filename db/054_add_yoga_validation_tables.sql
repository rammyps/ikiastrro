USE ikiastrro;
GO

SET XACT_ABORT ON;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '054_add_yoga_validation_tables.sql')
BEGIN
    BEGIN TRANSACTION;

    IF OBJECT_ID('dbo.tbl_Dim_ValidationSystems', 'U') IS NULL
    CREATE TABLE dbo.tbl_Dim_ValidationSystems
    (
        Id               INT IDENTITY(1,1) CONSTRAINT PK_Dim_ValidationSystems PRIMARY KEY,
        Code             VARCHAR(50) NOT NULL,
        DisplayName      NVARCHAR(120) NOT NULL,
        ProductVersion   VARCHAR(40) NOT NULL,
        EngineFamily     VARCHAR(60) NULL,
        LicenseCode      VARCHAR(40) NULL,
        ProvenanceUri    NVARCHAR(500) NULL,
        IsActive         BIT NOT NULL CONSTRAINT DF_Dim_ValidationSystems_IsActive DEFAULT (1),
        CONSTRAINT UQ_Dim_ValidationSystems_CodeVersion UNIQUE (Code, ProductVersion)
    );

    IF OBJECT_ID('dbo.tbl_Rule_YogaValidationDefinition', 'U') IS NULL
    CREATE TABLE dbo.tbl_Rule_YogaValidationDefinition
    (
        Id                       INT IDENTITY(1,1) CONSTRAINT PK_Rule_YogaValidationDefinition PRIMARY KEY,
        RuleSetId                TINYINT NOT NULL,
        ValidationSystemId       INT NOT NULL,
        ExternalDefinitionCode   VARCHAR(100) NOT NULL,
        ExternalName             NVARCHAR(300) NOT NULL,
        SourceRefCode            VARCHAR(40) NULL,
        SourceLocator            NVARCHAR(500) NULL,
        GroupName                NVARCHAR(120) NULL,
        ExpressionLanguage       VARCHAR(40) NULL,
        ExpressionText           NVARCHAR(MAX) NULL,
        DefinitionHash           CHAR(64) NOT NULL,
        HigherVargasSupported    BIT NULL,
        ImportedAtUtc            DATETIME2(0) NOT NULL CONSTRAINT DF_Rule_YogaValidationDefinition_Imported DEFAULT (SYSUTCDATETIME()),
        IsActive                 BIT NOT NULL CONSTRAINT DF_Rule_YogaValidationDefinition_IsActive DEFAULT (1),
        CONSTRAINT FK_Rule_YogaValidationDefinition_RuleSet FOREIGN KEY (RuleSetId)
            REFERENCES dbo.tbl_Rule_Sets (Id),
        CONSTRAINT FK_Rule_YogaValidationDefinition_System FOREIGN KEY (ValidationSystemId)
            REFERENCES dbo.tbl_Dim_ValidationSystems (Id),
        CONSTRAINT FK_Rule_YogaValidationDefinition_Source FOREIGN KEY (SourceRefCode)
            REFERENCES dbo.tbl_Dim_Source (Code),
        CONSTRAINT UQ_Rule_YogaValidationDefinition UNIQUE
            (RuleSetId, ValidationSystemId, ExternalDefinitionCode),
        CONSTRAINT CK_Rule_YogaValidationDefinition_Hash CHECK
            (DefinitionHash NOT LIKE '%[^0-9A-Fa-f]%' AND LEN(DefinitionHash) = 64)
    );

    IF OBJECT_ID('dbo.tbl_Dim_YogaValidationMappings', 'U') IS NULL
    CREATE TABLE dbo.tbl_Dim_YogaValidationMappings
    (
        Id                       INT IDENTITY(1,1) CONSTRAINT PK_Dim_YogaValidationMappings PRIMARY KEY,
        YogaValidationDefinitionId INT NOT NULL,
        YogaCode                 VARCHAR(40) NULL,
        SourceVariantCode        VARCHAR(60) NULL,
        MappingStatus            VARCHAR(20) NOT NULL,
        ReviewStatus             VARCHAR(20) NOT NULL CONSTRAINT DF_Dim_YogaValidationMappings_Review DEFAULT ('PENDING'),
        Rationale                NVARCHAR(1000) NULL,
        EvidenceLocator          NVARCHAR(500) NULL,
        ReviewedBy               NVARCHAR(120) NULL,
        ReviewedAtUtc            DATETIME2(0) NULL,
        IsActive                 BIT NOT NULL CONSTRAINT DF_Dim_YogaValidationMappings_IsActive DEFAULT (1),
        CONSTRAINT FK_Dim_YogaValidationMappings_Definition FOREIGN KEY (YogaValidationDefinitionId)
            REFERENCES dbo.tbl_Rule_YogaValidationDefinition (Id),
        CONSTRAINT UQ_Dim_YogaValidationMappings_Definition UNIQUE (YogaValidationDefinitionId),
        CONSTRAINT CK_Dim_YogaValidationMappings_Status CHECK
            (MappingStatus IN ('EXACT','PARTIAL','BROADER','NARROWER','ALIAS_ONLY','UNMAPPED','NOT_COMPARABLE')),
        CONSTRAINT CK_Dim_YogaValidationMappings_Review CHECK
            (ReviewStatus IN ('PENDING','REVIEWED','REJECTED')),
        CONSTRAINT CK_Dim_YogaValidationMappings_Reviewed CHECK
            ((ReviewStatus = 'PENDING' AND ReviewedAtUtc IS NULL)
             OR (ReviewStatus IN ('REVIEWED','REJECTED') AND ReviewedAtUtc IS NOT NULL)),
        CONSTRAINT CK_Dim_YogaValidationMappings_Target CHECK
            ((MappingStatus IN ('UNMAPPED','NOT_COMPARABLE'))
             OR (YogaCode IS NOT NULL AND SourceVariantCode IS NOT NULL))
    );

    IF OBJECT_ID('dbo.tbl_Fact_YogaValidationRuns', 'U') IS NULL
    CREATE TABLE dbo.tbl_Fact_YogaValidationRuns
    (
        Id                    BIGINT IDENTITY(1,1) CONSTRAINT PK_Fact_YogaValidationRuns PRIMARY KEY,
        BirthDetailId         INT NOT NULL,
        ValidationSystemId    INT NOT NULL,
        RuleSetId             TINYINT NOT NULL,
        SettingsProfileCode   VARCHAR(60) NOT NULL,
        SettingsJson          NVARCHAR(MAX) NOT NULL,
        InputSnapshotHash     CHAR(64) NOT NULL,
        RunStatus             VARCHAR(20) NOT NULL,
        StartedAtUtc          DATETIME2(0) NOT NULL,
        CompletedAtUtc        DATETIME2(0) NULL,
        DiagnosticText        NVARCHAR(2000) NULL,
        CONSTRAINT FK_Fact_YogaValidationRuns_BirthDetail FOREIGN KEY (BirthDetailId)
            REFERENCES dbo.tbl_BirthDetails (Id),
        CONSTRAINT FK_Fact_YogaValidationRuns_System FOREIGN KEY (ValidationSystemId)
            REFERENCES dbo.tbl_Dim_ValidationSystems (Id),
        CONSTRAINT FK_Fact_YogaValidationRuns_RuleSet FOREIGN KEY (RuleSetId)
            REFERENCES dbo.tbl_Rule_Sets (Id),
        CONSTRAINT CK_Fact_YogaValidationRuns_Settings CHECK (ISJSON(SettingsJson) = 1),
        CONSTRAINT CK_Fact_YogaValidationRuns_Hash CHECK
            (InputSnapshotHash NOT LIKE '%[^0-9A-Fa-f]%' AND LEN(InputSnapshotHash) = 64),
        CONSTRAINT CK_Fact_YogaValidationRuns_Status CHECK
            (RunStatus IN ('STARTED','COMPLETED','PARTIAL','FAILED')),
        CONSTRAINT CK_Fact_YogaValidationRuns_Completed CHECK
            ((RunStatus = 'STARTED' AND CompletedAtUtc IS NULL)
             OR (RunStatus <> 'STARTED' AND CompletedAtUtc IS NOT NULL))
    );

    IF OBJECT_ID('dbo.tbl_Fact_YogaValidationResults', 'U') IS NULL
    CREATE TABLE dbo.tbl_Fact_YogaValidationResults
    (
        Id                         BIGINT IDENTITY(1,1) CONSTRAINT PK_Fact_YogaValidationResults PRIMARY KEY,
        YogaValidationRunId        BIGINT NOT NULL,
        YogaValidationDefinitionId INT NOT NULL,
        YogaCode                   VARCHAR(40) NULL,
        SourceVariantCode          VARCHAR(60) NULL,
        EvaluationStatus           VARCHAR(20) NOT NULL,
        IsPresent                  BIT NULL,
        RawResult                  NVARCHAR(MAX) NULL,
        EvidenceJson               NVARCHAR(MAX) NULL,
        DurationMilliseconds       INT NULL,
        EvaluatorVersion           VARCHAR(60) NULL,
        CreatedAtUtc               DATETIME2(0) NOT NULL CONSTRAINT DF_Fact_YogaValidationResults_Created DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_Fact_YogaValidationResults_Run FOREIGN KEY (YogaValidationRunId)
            REFERENCES dbo.tbl_Fact_YogaValidationRuns (Id),
        CONSTRAINT FK_Fact_YogaValidationResults_Definition FOREIGN KEY (YogaValidationDefinitionId)
            REFERENCES dbo.tbl_Rule_YogaValidationDefinition (Id),
        CONSTRAINT UQ_Fact_YogaValidationResults UNIQUE
            (YogaValidationRunId, YogaValidationDefinitionId),
        CONSTRAINT CK_Fact_YogaValidationResults_Status CHECK
            (EvaluationStatus IN ('PRESENT','ABSENT','NOT_EVALUATED','ERROR')),
        CONSTRAINT CK_Fact_YogaValidationResults_Presence CHECK
            ((EvaluationStatus = 'PRESENT' AND IsPresent = 1)
             OR (EvaluationStatus = 'ABSENT' AND IsPresent = 0)
             OR (EvaluationStatus IN ('NOT_EVALUATED','ERROR') AND IsPresent IS NULL)),
        CONSTRAINT CK_Fact_YogaValidationResults_Evidence CHECK
            (EvidenceJson IS NULL OR ISJSON(EvidenceJson) = 1),
        CONSTRAINT CK_Fact_YogaValidationResults_Duration CHECK
            (DurationMilliseconds IS NULL OR DurationMilliseconds >= 0)
    );

    CREATE INDEX IX_Fact_YogaValidationRuns_ChartProfile
        ON dbo.tbl_Fact_YogaValidationRuns (BirthDetailId, SettingsProfileCode, InputSnapshotHash);

    CREATE INDEX IX_Fact_YogaValidationResults_Yoga
        ON dbo.tbl_Fact_YogaValidationResults (YogaCode, SourceVariantCode, EvaluationStatus);

    INSERT dbo.tbl_Dim_ValidationSystems
        (Code, DisplayName, ProductVersion, EngineFamily, LicenseCode, ProvenanceUri)
    SELECT v.Code, v.DisplayName, v.ProductVersion, v.EngineFamily, v.LicenseCode, v.ProvenanceUri
    FROM (VALUES
        ('IKIASTRRO', N'ikiastrro', 'CURRENT', 'DOTNET_TYPED_PREDICATES', NULL,
         N'local://ikiastrro'),
        ('MAITREYA', N'Maitreya', '8.2', 'YOGAEXPERT_EXPRESSION', 'GPL-2.0-OR-LATER',
         N'https://github.com/martin-pe/maitreya8/tree/v8.2')
    ) v(Code, DisplayName, ProductVersion, EngineFamily, LicenseCode, ProvenanceUri)
    WHERE NOT EXISTS
    (
        SELECT 1 FROM dbo.tbl_Dim_ValidationSystems x
        WHERE x.Code = v.Code AND x.ProductVersion = v.ProductVersion
    );

    INSERT dbo.tbl_Rule_Catalog
        (RuleTableName, EngineCode, MethodCodes, Purpose, IntroducedIn)
    SELECT v.RuleTableName, 'YOGA', v.RuleTypeCode, v.Purpose, 'migration 054'
    FROM (VALUES
        ('tbl_Rule_YogaValidationDefinition', 'EXTERNAL_DEFINITION',
         'Versioned external yoga calculation definitions used only for validation')
    ) v(RuleTableName, RuleTypeCode, Purpose)
    WHERE NOT EXISTS
        (SELECT 1 FROM dbo.tbl_Rule_Catalog c WHERE c.RuleTableName = v.RuleTableName);

    INSERT dbo.SchemaMigrations (ScriptName, Note)
    VALUES ('054_add_yoga_validation_tables.sql',
            'Append-only external yoga definition, mapping, validation-run and result tables');

    COMMIT TRANSACTION;
END;
GO

CREATE OR ALTER VIEW dbo.vw_YogaValidationComparison
AS
WITH Comparable AS
(
    SELECT r.Id AS ResultId,
           vr.BirthDetailId,
           vr.SettingsProfileCode,
           vr.InputSnapshotHash,
           vs.Code AS ValidationSystemCode,
           vs.ProductVersion,
           r.YogaCode,
           r.SourceVariantCode,
           r.EvaluationStatus,
           r.IsPresent
    FROM dbo.tbl_Fact_YogaValidationResults r
    JOIN dbo.tbl_Fact_YogaValidationRuns vr ON vr.Id = r.YogaValidationRunId
    JOIN dbo.tbl_Dim_ValidationSystems vs ON vs.Id = vr.ValidationSystemId
    WHERE r.YogaCode IS NOT NULL
)
SELECT i.BirthDetailId,
       i.SettingsProfileCode,
       i.InputSnapshotHash,
       i.YogaCode,
       i.SourceVariantCode,
       i.ResultId AS IkiastrroResultId,
       e.ResultId AS ExternalResultId,
       e.ValidationSystemCode AS ExternalSystemCode,
       e.ProductVersion AS ExternalProductVersion,
       i.EvaluationStatus AS IkiastrroStatus,
       e.EvaluationStatus AS ExternalStatus,
       CASE
           WHEN i.EvaluationStatus = 'NOT_EVALUATED' OR e.EvaluationStatus = 'NOT_EVALUATED' THEN 'MISSING_INPUT'
           WHEN i.EvaluationStatus = 'ERROR' OR e.EvaluationStatus = 'ERROR' THEN 'NOT_COMPARABLE'
           WHEN i.IsPresent = 1 AND e.IsPresent = 1 THEN 'AGREE_PRESENT'
           WHEN i.IsPresent = 0 AND e.IsPresent = 0 THEN 'AGREE_ABSENT'
           ELSE 'DISAGREE'
       END AS ComparisonStatus
FROM Comparable i
JOIN Comparable e
  ON e.BirthDetailId = i.BirthDetailId
 AND e.SettingsProfileCode = i.SettingsProfileCode
 AND e.InputSnapshotHash = i.InputSnapshotHash
 AND e.YogaCode = i.YogaCode
 AND ISNULL(e.SourceVariantCode, '') = ISNULL(i.SourceVariantCode, '')
WHERE i.ValidationSystemCode = 'IKIASTRRO'
  AND e.ValidationSystemCode <> 'IKIASTRRO';
GO
