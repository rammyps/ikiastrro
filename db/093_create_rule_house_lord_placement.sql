/* Production rule table for the "house lord in house" interpretation layer -- the sibling
   of tbl_Rule_Yoga, following the same versioned-rule conventions (STANDARDS.md SS D.1 /
   docs/database/rules-engine.md): RuleSetId versioning, the MethodCode/RuleParametersJson/
   CalculationNarrative/SourceRefCode/IsActive portability tail, ISJSON + SRC[_]% checks.

   Keyed by (OwnedHouseNumber, OccupiedHouseNumber, BranchCode) rather than a single free-text
   result, because the source itself states separate BASELINE / WELL_DISPOSED / AFFLICTED
   branches per combination (docs/research/domain/house-placement.md "Rule capture
   requirements": "do not fire both from placement alone"). Whole-sign houses, Lagna
   reference point, per the project's 2026-09-07 house-system decision. */
IF OBJECT_ID(N'dbo.tbl_Rule_HouseLordPlacement', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Rule_HouseLordPlacement
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Rule_HouseLordPlacement PRIMARY KEY,
        RuleSetId TINYINT NOT NULL,
        OwnedHouseNumber TINYINT NOT NULL,
        OccupiedHouseNumber TINYINT NOT NULL,
        BranchCode VARCHAR(30) NOT NULL,
        HouseSystemCode VARCHAR(20) NOT NULL CONSTRAINT DF_Rule_HouseLordPlacement_System DEFAULT ('WHOLE_SIGN'),
        ReferencePointCode VARCHAR(20) NOT NULL CONSTRAINT DF_Rule_HouseLordPlacement_RefPoint DEFAULT ('LAGNA'),
        ResultText NVARCHAR(2000) NOT NULL,
        EvaluableTodayCode VARCHAR(10) NULL,
        InterpretationStatusCode VARCHAR(20) NOT NULL CONSTRAINT DF_Rule_HouseLordPlacement_InterpStatus DEFAULT ('Proposed'),
        MethodCode VARCHAR(30) NULL,
        RuleParametersJson NVARCHAR(MAX) NULL,
        CalculationNarrative NVARCHAR(MAX) NULL,
        SourceRefCode VARCHAR(40) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Rule_HouseLordPlacement_IsActive DEFAULT (1),
        CONSTRAINT FK_Rule_HouseLordPlacement_RuleSet FOREIGN KEY (RuleSetId) REFERENCES dbo.tbl_Rule_Sets(Id),
        CONSTRAINT FK_Rule_HouseLordPlacement_Source FOREIGN KEY (SourceRefCode) REFERENCES dbo.tbl_Dim_Source(Code),
        CONSTRAINT UQ_Rule_HouseLordPlacement UNIQUE
            (RuleSetId, OwnedHouseNumber, OccupiedHouseNumber, BranchCode, HouseSystemCode, ReferencePointCode),
        CONSTRAINT CK_Rule_HouseLordPlacement_OwnedHouse CHECK (OwnedHouseNumber BETWEEN 1 AND 12),
        CONSTRAINT CK_Rule_HouseLordPlacement_OccupiedHouse CHECK (OccupiedHouseNumber BETWEEN 1 AND 12),
        CONSTRAINT CK_Rule_HouseLordPlacement_Branch CHECK (BranchCode IN ('BASELINE','WELL_DISPOSED','AFFLICTED')),
        CONSTRAINT CK_Rule_HouseLordPlacement_System CHECK (HouseSystemCode IN ('WHOLE_SIGN')),
        CONSTRAINT CK_Rule_HouseLordPlacement_RefPoint CHECK (ReferencePointCode IN ('LAGNA','MOON','SUN')),
        CONSTRAINT CK_Rule_HouseLordPlacement_Evaluable CHECK (EvaluableTodayCode IS NULL OR EvaluableTodayCode IN ('YES','PARTIAL','NO')),
        CONSTRAINT CK_Rule_HouseLordPlacement_InterpStatus CHECK
            (InterpretationStatusCode IN ('Proposed','Researched','Reviewed','Implemented','Verified','Deprecated','Disputed')),
        CONSTRAINT CK_Rule_HouseLordPlacement_Json CHECK (RuleParametersJson IS NULL OR ISJSON(RuleParametersJson) = 1),
        CONSTRAINT CK_Rule_HouseLordPlacement_Src CHECK (SourceRefCode IS NULL OR SourceRefCode LIKE 'SRC[_]%')
    );
END;
GO

IF OBJECT_ID(N'dbo.tbl_Rule_Catalog', N'U') IS NOT NULL
BEGIN
    INSERT dbo.tbl_Rule_Catalog (RuleTableName, EngineCode, MethodCodes, Purpose, IntroducedIn)
    SELECT 'tbl_Rule_HouseLordPlacement', 'HOUSE', 'HOUSE_LORD_PLACEMENT',
           N'Source-attributed "house lord in house" interpretations (12x12, whole-sign/Lagna): BASELINE plus its own WELL_DISPOSED/AFFLICTED branches where the source states them, promoted from the research.*HouseLordInHouse pipeline. Distinct from planet-in-house (tbl_Rule_HouseSignification family).',
           N'migration 093'
    WHERE NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_Catalog WHERE RuleTableName = 'tbl_Rule_HouseLordPlacement');
END;
GO

IF OBJECT_ID(N'dbo.SchemaMigrations', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = N'093_create_rule_house_lord_placement.sql')
    INSERT dbo.SchemaMigrations (ScriptName, Note)
    VALUES (N'093_create_rule_house_lord_placement.sql', N'Create production dbo.tbl_Rule_HouseLordPlacement and register it in tbl_Rule_Catalog.');
GO

PRINT '093 applied: tbl_Rule_HouseLordPlacement created.';
GO
