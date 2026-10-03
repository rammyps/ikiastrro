-- 167 - Preserve source-attributed life-matter statements as first-class rules.
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID('dbo.tbl_Rule_LifeMatterClaim', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Rule_LifeMatterClaim (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Rule_LifeMatterClaim PRIMARY KEY,
        RuleSetId TINYINT NOT NULL CONSTRAINT FK_Rule_LifeMatterClaim_RuleSet FOREIGN KEY REFERENCES dbo.tbl_Rule_Sets (Id),
        Code VARCHAR(80) NOT NULL CONSTRAINT UQ_Rule_LifeMatterClaim_Code UNIQUE,
        KarakaRoleId INT NULL CONSTRAINT FK_Rule_LifeMatterClaim_KarakaRole FOREIGN KEY REFERENCES dbo.tbl_Dim_KarakaRole (Id),
        ClaimTypeCode VARCHAR(24) NOT NULL,
        StatementFormCode VARCHAR(16) NOT NULL,
        StatementText NVARCHAR(1000) NOT NULL,
        SourceRefCode VARCHAR(40) NOT NULL CONSTRAINT FK_Rule_LifeMatterClaim_Source FOREIGN KEY REFERENCES dbo.tbl_Dim_Source (Code),
        SourceLocator NVARCHAR(160) NULL,
        VerificationStatus VARCHAR(20) NOT NULL,
        Notes NVARCHAR(500) NULL,
        DisplayOrder SMALLINT NOT NULL CONSTRAINT DF_Rule_LifeMatterClaim_Order DEFAULT 1,
        IsActive BIT NOT NULL CONSTRAINT DF_Rule_LifeMatterClaim_IsActive DEFAULT 1,
        CONSTRAINT CK_Rule_LifeMatterClaim_Type CHECK (ClaimTypeCode IN ('SIGNIFICATION','READING_METHOD','QUALIFICATION','CONDITION','OUTCOME')),
        CONSTRAINT CK_Rule_LifeMatterClaim_Form CHECK (StatementFormCode IN ('DIRECT_QUOTE','PARAPHRASE','STRUCTURED_RULE')),
        CONSTRAINT CK_Rule_LifeMatterClaim_Verification CHECK (VerificationStatus IN ('VERIFIED','LOCATOR_PENDING','CONFLICTING','RETIRED'))
    );
    CREATE INDEX IX_Rule_LifeMatterClaim_Karaka ON dbo.tbl_Rule_LifeMatterClaim (RuleSetId, KarakaRoleId, IsActive);
    CREATE INDEX IX_Rule_LifeMatterClaim_Source ON dbo.tbl_Rule_LifeMatterClaim (SourceRefCode, VerificationStatus);
END
GO

IF OBJECT_ID('dbo.tbl_Rule_LifeMatterClaimScope', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Rule_LifeMatterClaimScope (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Rule_LifeMatterClaimScope PRIMARY KEY,
        LifeMatterClaimId INT NOT NULL CONSTRAINT FK_Rule_LifeMatterClaimScope_Claim FOREIGN KEY REFERENCES dbo.tbl_Rule_LifeMatterClaim (Id),
        LifeMatterId INT NULL CONSTRAINT FK_Rule_LifeMatterClaimScope_Matter FOREIGN KEY REFERENCES dbo.tbl_Dim_LifeMatter (Id),
        DivisionalSubjectCode VARCHAR(40) NULL CONSTRAINT FK_Rule_LifeMatterClaimScope_Subject FOREIGN KEY REFERENCES dbo.tbl_Dim_DivisionalSubject (SubjectCode),
        ScopeNote NVARCHAR(300) NULL,
        DisplayOrder TINYINT NOT NULL CONSTRAINT DF_Rule_LifeMatterClaimScope_Order DEFAULT 1,
        IsActive BIT NOT NULL CONSTRAINT DF_Rule_LifeMatterClaimScope_IsActive DEFAULT 1,
        CONSTRAINT CK_Rule_LifeMatterClaimScope_OneTarget CHECK (
            (LifeMatterId IS NOT NULL AND DivisionalSubjectCode IS NULL)
            OR (LifeMatterId IS NULL AND DivisionalSubjectCode IS NOT NULL))
    );
    CREATE UNIQUE INDEX UX_Rule_LifeMatterClaimScope_Matter
        ON dbo.tbl_Rule_LifeMatterClaimScope (LifeMatterClaimId, LifeMatterId) WHERE LifeMatterId IS NOT NULL;
    CREATE UNIQUE INDEX UX_Rule_LifeMatterClaimScope_Subject
        ON dbo.tbl_Rule_LifeMatterClaimScope (LifeMatterClaimId, DivisionalSubjectCode) WHERE DivisionalSubjectCode IS NOT NULL;
END
GO

-- Convert the 34 existing PVR ch. 8 karakatwa mappings into readable claims.
;WITH claim_rows AS (
    SELECT km.RuleSetId, km.KarakaRoleId, dm.Id AS LifeMatterId,
           'PVR_NK_' + UPPER(p.PlanetName) + '_' + dm.Code AS Code,
           N'The natural significator ' + p.PlanetName + N' signifies ' + dm.EnglishName
             + CASE WHEN km.HouseNumber IS NULL THEN N'.'
                    ELSE N', read from house ' + CONVERT(NVARCHAR(2), km.HouseNumber) + N'.' END AS StatementText,
           km.SourceRefCode, km.DisplayOrder
    FROM dbo.tbl_Rule_KarakaMatter km
    JOIN dbo.tbl_Dim_LifeMatter dm ON dm.Id = km.LifeMatterId AND dm.SourceGroup = 'PVR_KARAKATWA_GRID'
    JOIN dbo.tbl_Dim_KarakaRole kr ON kr.Id = km.KarakaRoleId AND kr.KarakaTypeCode = 'NAISARGIKA'
    JOIN dbo.tbl_Planets p ON p.Id = kr.FixedGrahaId
)
INSERT dbo.tbl_Rule_LifeMatterClaim
    (RuleSetId, Code, KarakaRoleId, ClaimTypeCode, StatementFormCode, StatementText,
     SourceRefCode, SourceLocator, VerificationStatus, Notes, DisplayOrder)
SELECT RuleSetId, Code, KarakaRoleId, 'SIGNIFICATION', 'STRUCTURED_RULE', StatementText,
       SourceRefCode, N'Chapter 8, p. 79', 'VERIFIED',
       N'Structured paraphrase generated from the normalized PVR naisargika-karakatwa grid.', DisplayOrder
FROM claim_rows r
WHERE NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_LifeMatterClaim c WHERE c.Code = r.Code);
GO

;WITH claim_scopes AS (
    SELECT 'PVR_NK_' + UPPER(p.PlanetName) + '_' + dm.Code AS ClaimCode,
           dm.Id AS LifeMatterId, km.DisplayOrder
    FROM dbo.tbl_Rule_KarakaMatter km
    JOIN dbo.tbl_Dim_LifeMatter dm ON dm.Id = km.LifeMatterId AND dm.SourceGroup = 'PVR_KARAKATWA_GRID'
    JOIN dbo.tbl_Dim_KarakaRole kr ON kr.Id = km.KarakaRoleId AND kr.KarakaTypeCode = 'NAISARGIKA'
    JOIN dbo.tbl_Planets p ON p.Id = kr.FixedGrahaId
)
INSERT dbo.tbl_Rule_LifeMatterClaimScope (LifeMatterClaimId, LifeMatterId, DisplayOrder)
SELECT c.Id, s.LifeMatterId, s.DisplayOrder
FROM claim_scopes s
JOIN dbo.tbl_Rule_LifeMatterClaim c ON c.Code = s.ClaimCode
WHERE NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_LifeMatterClaimScope x
                  WHERE x.LifeMatterClaimId = c.Id AND x.LifeMatterId = s.LifeMatterId);
GO

DECLARE @saturnRoleId INT = (
    SELECT kr.Id FROM dbo.tbl_Dim_KarakaRole kr
    JOIN dbo.tbl_Planets p ON p.Id = kr.FixedGrahaId
    WHERE kr.KarakaTypeCode = 'NAISARGIKA' AND p.PlanetName = 'Saturn');

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_LifeMatterClaim WHERE Code = 'PVR_SATURN_LIVELIHOOD_KARMA')
    INSERT dbo.tbl_Rule_LifeMatterClaim
        (RuleSetId, Code, KarakaRoleId, ClaimTypeCode, StatementFormCode, StatementText,
         SourceRefCode, SourceLocator, VerificationStatus, Notes, DisplayOrder)
    VALUES (1, 'PVR_SATURN_LIVELIHOOD_KARMA', @saturnRoleId, 'SIGNIFICATION', 'DIRECT_QUOTE',
            N'Saturn is the significator of livelihood and karma.', 'SRC_PVR_INTEGRATED', NULL,
            'LOCATOR_PENDING',
            N'PVR attribution supplied by the project owner on 2026-10-03; publication, edition and page remain to be verified.', 1);
GO

DECLARE @claimId INT = (SELECT Id FROM dbo.tbl_Rule_LifeMatterClaim WHERE Code = 'PVR_SATURN_LIVELIHOOD_KARMA');
;WITH matter_scope AS (
    SELECT Id, DisplayOrder FROM dbo.tbl_Dim_LifeMatter
    WHERE Code IN ('CAREER_STATUS_01', 'CAREER_STATUS_03', 'CAREER_STATUS_07'))
INSERT dbo.tbl_Rule_LifeMatterClaimScope (LifeMatterClaimId, LifeMatterId, ScopeNote, DisplayOrder)
SELECT @claimId, m.Id,
       N'Applies to overall career, employment/service and career/action judgment.', m.DisplayOrder
FROM matter_scope m
WHERE NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_LifeMatterClaimScope s
                  WHERE s.LifeMatterClaimId = @claimId AND s.LifeMatterId = m.Id);

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_LifeMatterClaimScope
               WHERE LifeMatterClaimId = @claimId AND DivisionalSubjectCode = 'KARMIC_ROOTS')
    INSERT dbo.tbl_Rule_LifeMatterClaimScope
        (LifeMatterClaimId, DivisionalSubjectCode, ScopeNote, DisplayOrder)
    VALUES (@claimId, 'KARMIC_ROOTS',
            N'D60 is a karmic confirmation layer; it does not replace D1/D10 livelihood judgment.', 4);
GO

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_Catalog WHERE RuleTableName = 'tbl_Rule_LifeMatterClaim')
    INSERT dbo.tbl_Rule_Catalog (RuleTableName, EngineCode, MethodCodes, Purpose, IntroducedIn)
    VALUES ('tbl_Rule_LifeMatterClaim', 'KARAKA', 'MAP_LOOKUP',
            'Source-attributed statements beside life-matter mappings, with quote/paraphrase form, locator and verification status.',
            '167_create_life_matter_claims.sql');
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_Catalog WHERE RuleTableName = 'tbl_Rule_LifeMatterClaimScope')
    INSERT dbo.tbl_Rule_Catalog (RuleTableName, EngineCode, MethodCodes, Purpose, IntroducedIn)
    VALUES ('tbl_Rule_LifeMatterClaimScope', 'KARAKA', 'MAP_LOOKUP',
            'Many-to-many scope from a claim to life matters or a divisional subject.',
            '167_create_life_matter_claims.sql');
GO

IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '167_create_life_matter_claims.sql')
    INSERT dbo.SchemaMigrations (ScriptName, AppliedAtUtc, Note)
    VALUES ('167_create_life_matter_claims.sql', SYSUTCDATETIME(),
            'Adds life-matter claims/scopes; seeds 34 PVR ch. 8 claims and Saturn livelihood/karma.');

COMMIT TRANSACTION;
GO

DECLARE @claimCount INT = (SELECT COUNT(*) FROM dbo.tbl_Rule_LifeMatterClaim);
DECLARE @scopeCount INT = (SELECT COUNT(*) FROM dbo.tbl_Rule_LifeMatterClaimScope);
PRINT '167 complete: claims=' + CAST(@claimCount AS VARCHAR(10))
    + ', scopes=' + CAST(@scopeCount AS VARCHAR(10));
