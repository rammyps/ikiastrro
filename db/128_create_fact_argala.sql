-- =====================================================================
-- 128 - tbl_Fact_Argala: per-chart Argala/Virodhargala results — one row
-- per (target, relation, position, occupant planet). Computed by
-- ArgalaCalculator (src/Ikiastrro.Core/Engines/Houses/ArgalaCalculator.cs)
-- applying tbl_Rule_Argala (migration 127) to a chart's actual planet
-- positions. P.V.R. Narasimha Rao, Vedic Astrology: An Integrated
-- Approach, sec.10.5-10.6 (SRC_PVR_INTEGRATED).
--
-- Grain: one row per occupant of an argala/virodhargala position for a
-- given target (a house 1-12, or a planet's own occupied sign — PVR runs
-- both in the same worked example, sec.10.7). Only occupied positions get
-- rows (an empty position simply has none); tbl_Rule_Argala documents
-- which 8 positions always apply per target. Narrow star-schema, same
-- shape/rationale as tbl_Fact_HouseFromReference (migration 32).
--
-- Populated by: `dotnet run --project src/Ikiastrro.Cli -- backfill-argala`
-- (reads tbl_Chart_KeyDetails Graha rows per D1 chart, not yet wired into
-- the live chart-generation pipeline — see ArgalaFactRepository).
--
-- Apply:  sqlcmd -S "localhost\SQLSERVER2025" -E -d ikiastrro -b -i db/128_create_fact_argala.sql
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID('dbo.tbl_Fact_Argala', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Fact_Argala (
        Id                    INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Fact_Argala PRIMARY KEY,
        ChartResultId         INT          NOT NULL
                                  CONSTRAINT FK_Fact_Argala_ChartResult FOREIGN KEY REFERENCES dbo.tbl_ChartResults (Id),
        RuleSetId             TINYINT      NOT NULL
                                  CONSTRAINT FK_Fact_Argala_RuleSet FOREIGN KEY REFERENCES dbo.tbl_Rule_Sets (Id),
        ChartTypeId           TINYINT      NULL
                                  CONSTRAINT FK_Fact_Argala_ChartType FOREIGN KEY REFERENCES dbo.tbl_Dim_ChartType (Id),
        TargetKind            VARCHAR(12)  NOT NULL,                 -- 'House' or 'Graha'
        TargetKey             VARCHAR(12)  NOT NULL,                 -- '1'..'12' or 'Sun'..'Ketu'
        TargetHouseNumber     TINYINT      NOT NULL,                 -- resolved house-from-lagna of the target
        TargetSignId          TINYINT      NOT NULL
                                  CONSTRAINT FK_Fact_Argala_TargetSign FOREIGN KEY REFERENCES dbo.tbl_SignAttributes (Id),
        RelationTypeCode      VARCHAR(20)  NOT NULL,                 -- 'ARGALA' or 'VIRODHARGALA'
        HouseOffset           TINYINT      NOT NULL,                 -- 2/4/5/11 (argala) or 12/10/3/9 (virodhargala)
        IsPrimary             BIT          NOT NULL,
        OccupantPlanetId      TINYINT      NOT NULL
                                  CONSTRAINT FK_Fact_Argala_Occupant FOREIGN KEY REFERENCES dbo.tbl_Planets (Id),
        ExceptionApplied      BIT          NOT NULL CONSTRAINT DF_Fact_Argala_Exception DEFAULT 0,
        CountedAntiZodiacally BIT          NOT NULL CONSTRAINT DF_Fact_Argala_AntiZodiacal DEFAULT 0,
        SourceRefCode         VARCHAR(40)  NULL,
        CONSTRAINT CK_Fact_Argala_TargetKind CHECK (TargetKind IN ('House','Graha')),
        CONSTRAINT CK_Fact_Argala_RelType    CHECK (RelationTypeCode IN ('ARGALA','VIRODHARGALA')),
        CONSTRAINT CK_Fact_Argala_House      CHECK (TargetHouseNumber BETWEEN 1 AND 12),
        CONSTRAINT CK_Fact_Argala_Offset     CHECK (HouseOffset BETWEEN 1 AND 12),
        CONSTRAINT CK_Fact_Argala_Src        CHECK (SourceRefCode IS NULL OR SourceRefCode LIKE 'SRC[_]%'),
        CONSTRAINT UQ_Fact_Argala UNIQUE (ChartResultId, ChartTypeId, TargetKind, TargetKey, RelationTypeCode, HouseOffset, OccupantPlanetId)
    );
    CREATE NONCLUSTERED INDEX IX_Fact_Argala_ChartResultId ON dbo.tbl_Fact_Argala (ChartResultId);
END
GO

-- Not cataloged in tbl_Rule_Catalog — that catalog indexes tbl_Rule_* tables only (see
-- verify-rules' `LIKE 'tbl_Rule[_]%'` gate); tbl_Fact_HouseFromReference (migration 32) sets
-- the precedent of leaving Fact tables out of it.

INSERT dbo.SchemaMigrations (ScriptName, Note)
SELECT '128_create_fact_argala.sql',
       'tbl_Fact_Argala created (empty)'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '128_create_fact_argala.sql');
GO

PRINT '128 applied: tbl_Fact_Argala created.';
GO
