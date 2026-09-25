-- =====================================================================
-- 137 - LifeMatter Subject/Varga and House/SpecialPoint focus bridges.
--
-- Planet/Chara-karaka focus remains owned by tbl_Rule_KarakaMatter.
-- Candidate population is intentionally deferred: HouseFromLagnaText is
-- parsed offline and human-audited before seed rows are committed.
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID('dbo.tbl_Rule_LifeMatterSubject', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Rule_LifeMatterSubject (
        Id                    INT IDENTITY(1,1) NOT NULL
                                  CONSTRAINT PK_Rule_LifeMatterSubject PRIMARY KEY,
        RuleSetId             TINYINT     NOT NULL
                                  CONSTRAINT FK_Rule_LifeMatterSubject_RuleSet
                                  FOREIGN KEY REFERENCES dbo.tbl_Rule_Sets (Id),
        LifeMatterId          INT         NOT NULL
                                  CONSTRAINT FK_Rule_LifeMatterSubject_LifeMatter
                                  FOREIGN KEY REFERENCES dbo.tbl_Dim_LifeMatter (Id),
        DivisionalSubjectCode VARCHAR(40) NOT NULL
                                  CONSTRAINT FK_Rule_LifeMatterSubject_DivisionalSubject
                                  FOREIGN KEY REFERENCES dbo.tbl_Dim_DivisionalSubject (SubjectCode),
        SourceRefCode         VARCHAR(40) NULL,
        IsActive              BIT         NOT NULL
                                  CONSTRAINT DF_Rule_LifeMatterSubject_IsActive DEFAULT 1,
        CONSTRAINT CK_Rule_LifeMatterSubject_Source
            CHECK (SourceRefCode IS NULL OR SourceRefCode LIKE 'SRC[_]%')
    );

    CREATE UNIQUE NONCLUSTERED INDEX UX_Rule_LifeMatterSubject_Active
        ON dbo.tbl_Rule_LifeMatterSubject (RuleSetId, LifeMatterId)
        WHERE IsActive = 1;
END
GO

IF OBJECT_ID('dbo.tbl_Rule_LifeMatterFocus', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Rule_LifeMatterFocus (
        Id               INT IDENTITY(1,1) NOT NULL
                             CONSTRAINT PK_Rule_LifeMatterFocus PRIMARY KEY,
        RuleSetId        TINYINT     NOT NULL
                             CONSTRAINT FK_Rule_LifeMatterFocus_RuleSet
                             FOREIGN KEY REFERENCES dbo.tbl_Rule_Sets (Id),
        LifeMatterId     INT         NOT NULL
                             CONSTRAINT FK_Rule_LifeMatterFocus_LifeMatter
                             FOREIGN KEY REFERENCES dbo.tbl_Dim_LifeMatter (Id),
        FocusKind        VARCHAR(20) NOT NULL,
        ReferenceCode    VARCHAR(24) NULL
                             CONSTRAINT FK_Rule_LifeMatterFocus_HouseReference
                             FOREIGN KEY REFERENCES dbo.tbl_Dim_HouseReference (ReferenceCode),
        HouseNumber      TINYINT     NULL,
        SpecialPointCode VARCHAR(24) NULL,
        Priority         TINYINT     NOT NULL CONSTRAINT DF_Rule_LifeMatterFocus_Priority DEFAULT 1,
        SourceRefCode    VARCHAR(40) NULL,
        IsActive         BIT         NOT NULL CONSTRAINT DF_Rule_LifeMatterFocus_IsActive DEFAULT 1,
        CONSTRAINT CK_Rule_LifeMatterFocus_Kind
            CHECK (FocusKind IN ('House', 'SpecialPoint')),
        CONSTRAINT CK_Rule_LifeMatterFocus_Payload CHECK (
            (FocusKind = 'House'
             AND ReferenceCode IS NOT NULL
             AND HouseNumber BETWEEN 1 AND 12
             AND SpecialPointCode IS NULL)
            OR
            (FocusKind = 'SpecialPoint'
             AND ReferenceCode IS NULL
             AND HouseNumber IS NULL
             AND SpecialPointCode IS NOT NULL
             AND LEN(LTRIM(RTRIM(SpecialPointCode))) > 0
             AND UPPER(SpecialPointCode) <> 'UL')
        ),
        CONSTRAINT CK_Rule_LifeMatterFocus_Source
            CHECK (SourceRefCode IS NULL OR SourceRefCode LIKE 'SRC[_]%')
    );

    CREATE UNIQUE NONCLUSTERED INDEX UX_Rule_LifeMatterFocus_ActiveHouse
        ON dbo.tbl_Rule_LifeMatterFocus (RuleSetId, LifeMatterId, ReferenceCode, HouseNumber)
        WHERE IsActive = 1 AND FocusKind = 'House';

    CREATE UNIQUE NONCLUSTERED INDEX UX_Rule_LifeMatterFocus_ActiveSpecialPoint
        ON dbo.tbl_Rule_LifeMatterFocus (RuleSetId, LifeMatterId, SpecialPointCode)
        WHERE IsActive = 1 AND FocusKind = 'SpecialPoint';
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_Catalog WHERE RuleTableName = 'tbl_Rule_LifeMatterSubject')
    INSERT dbo.tbl_Rule_Catalog (RuleTableName, EngineCode, MethodCodes, Purpose, IntroducedIn)
    VALUES ('tbl_Rule_LifeMatterSubject', 'LIFE_MATTER', 'MAP_LOOKUP',
            'At most one active divisional subject per LifeMatter and RuleSet; resolves the Auto confirmation Varga.',
            '137_create_life_matter_subject_focus.sql');
GO

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_Catalog WHERE RuleTableName = 'tbl_Rule_LifeMatterFocus')
    INSERT dbo.tbl_Rule_Catalog (RuleTableName, EngineCode, MethodCodes, Purpose, IntroducedIn)
    VALUES ('tbl_Rule_LifeMatterFocus', 'LIFE_MATTER', 'MAP_LOOKUP',
            'House and canonical special-point focus per LifeMatter; planet focus remains in tbl_Rule_KarakaMatter.',
            '137_create_life_matter_subject_focus.sql');
GO

INSERT dbo.SchemaMigrations (ScriptName, Note)
SELECT '137_create_life_matter_subject_focus.sql',
       'Creates empty, constrained LifeMatter Subject/Varga and House/SpecialPoint focus bridges; audited seed population deferred.'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '137_create_life_matter_subject_focus.sql');
GO

DECLARE @subjectTable INT = CASE WHEN OBJECT_ID('dbo.tbl_Rule_LifeMatterSubject', 'U') IS NULL THEN 0 ELSE 1 END;
DECLARE @focusTable INT = CASE WHEN OBJECT_ID('dbo.tbl_Rule_LifeMatterFocus', 'U') IS NULL THEN 0 ELSE 1 END;
DECLARE @subjects INT = CASE WHEN @subjectTable = 0 THEN -1 ELSE (SELECT COUNT(*) FROM dbo.tbl_Rule_LifeMatterSubject) END;
DECLARE @foci INT = CASE WHEN @focusTable = 0 THEN -1 ELSE (SELECT COUNT(*) FROM dbo.tbl_Rule_LifeMatterFocus) END;
PRINT '137 applied: subject table=' + CAST(@subjectTable AS VARCHAR(1))
    + ', focus table=' + CAST(@focusTable AS VARCHAR(1))
    + ', subject rows=' + CAST(@subjects AS VARCHAR(10))
    + ', focus rows=' + CAST(@foci AS VARCHAR(10)) + ' (both expected 0 before audited population).';
GO
