-- =====================================================================
-- 113 - Production dbo.tbl_Rule_PlanetInHouse. Mirrors db/093's
-- tbl_Rule_HouseLordPlacement shape (RuleSetId + portability tail),
-- simplified: one ResultText per (Planet, House) combo, no
-- BranchCode/ReferencePointCode split - the source itself (B.V. Raman,
-- "How to Judge a Horoscope") states dignity-conditional nuance
-- ("if exalted...if afflicted...") inline within one passage per
-- combination, not as separate baseline/well-disposed/afflicted
-- statements the way the house-lord-placement source did.
--
-- Schema only in this migration - content comes from a dedicated
-- research.*PlanetInHouseClaim transcription pass (citing SRC_RAMAN_HTJH,
-- NOT the existing wrongly-cited SRC_BVRAMAN_PLANET_IN_HOUSE placeholder
-- rows from migrations 064/065 - see docs/database/karakafix.md-adjacent
-- planning notes), then a promotion migration mirroring db/094.
--
-- Apply:  sqlcmd -S localhost\SQLSERVER2025 -E -d ikiastrro -b -i db/113_create_rule_planet_in_house.sql
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.tbl_Rule_PlanetInHouse', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Rule_PlanetInHouse
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Rule_PlanetInHouse PRIMARY KEY,
        RuleSetId TINYINT NOT NULL,
        PlanetId TINYINT NOT NULL,
        HouseNumber TINYINT NOT NULL,
        HouseSystemCode VARCHAR(20) NOT NULL CONSTRAINT DF_Rule_PlanetInHouse_System DEFAULT ('WHOLE_SIGN'),
        ResultText NVARCHAR(2000) NOT NULL,
        InterpretationStatusCode VARCHAR(20) NOT NULL CONSTRAINT DF_Rule_PlanetInHouse_InterpStatus DEFAULT ('Proposed'),
        MethodCode VARCHAR(30) NULL,
        RuleParametersJson NVARCHAR(MAX) NULL,
        CalculationNarrative NVARCHAR(MAX) NULL,
        SourceRefCode VARCHAR(40) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Rule_PlanetInHouse_IsActive DEFAULT (1),
        CONSTRAINT FK_Rule_PlanetInHouse_RuleSet FOREIGN KEY (RuleSetId) REFERENCES dbo.tbl_Rule_Sets(Id),
        CONSTRAINT FK_Rule_PlanetInHouse_Planet FOREIGN KEY (PlanetId) REFERENCES dbo.tbl_Planets(Id),
        CONSTRAINT FK_Rule_PlanetInHouse_Source FOREIGN KEY (SourceRefCode) REFERENCES dbo.tbl_Dim_Source(Code),
        CONSTRAINT UQ_Rule_PlanetInHouse UNIQUE (RuleSetId, PlanetId, HouseNumber, HouseSystemCode),
        CONSTRAINT CK_Rule_PlanetInHouse_House CHECK (HouseNumber BETWEEN 1 AND 12),
        CONSTRAINT CK_Rule_PlanetInHouse_System CHECK (HouseSystemCode IN ('WHOLE_SIGN')),
        CONSTRAINT CK_Rule_PlanetInHouse_InterpStatus CHECK
            (InterpretationStatusCode IN ('Proposed','Researched','Reviewed','Implemented','Verified','Deprecated','Disputed')),
        CONSTRAINT CK_Rule_PlanetInHouse_Json CHECK (RuleParametersJson IS NULL OR ISJSON(RuleParametersJson) = 1),
        CONSTRAINT CK_Rule_PlanetInHouse_Src CHECK (SourceRefCode IS NULL OR SourceRefCode LIKE 'SRC[_]%')
    );
END;
GO

IF OBJECT_ID(N'dbo.tbl_Rule_Catalog', N'U') IS NOT NULL
BEGIN
    INSERT dbo.tbl_Rule_Catalog (RuleTableName, EngineCode, MethodCodes, Purpose, IntroducedIn)
    SELECT 'tbl_Rule_PlanetInHouse', 'HOUSE', 'PLANET_IN_HOUSE_PLACEMENT',
           N'Source-attributed planet-in-house interpretations (9 grahas x 12 houses, whole-sign), promoted from the research.*PlanetInHouse pipeline (B. V. Raman, How to Judge a Horoscope, SRC_RAMAN_HTJH). Distinct from house-lord placement (tbl_Rule_HouseLordPlacement).',
           N'migration 113'
    WHERE NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_Catalog WHERE RuleTableName = 'tbl_Rule_PlanetInHouse');
END;
GO

IF OBJECT_ID(N'dbo.SchemaMigrations', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = N'113_create_rule_planet_in_house.sql')
    INSERT dbo.SchemaMigrations (ScriptName, Note)
    VALUES (N'113_create_rule_planet_in_house.sql', N'Create production dbo.tbl_Rule_PlanetInHouse and register it in tbl_Rule_Catalog. Schema only - content follows via research pipeline + promotion.');
GO

PRINT '113 applied: tbl_Rule_PlanetInHouse created.';
GO
