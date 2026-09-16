-- =====================================================================
-- 101 -- Amsabala fact table + UI read view.
--
-- Persists AmsabalaCalculator's per-planet, per-scheme result (migration 099's
-- tbl_Rule_AmsabalaGroup/tbl_Rule_AmsabalaName reconciliation, verified against PVR's
-- worked Example 27 in AmsabalaCalculatorTests) onto the D1 tbl_ChartResults row, mirroring
-- the Ashtakavarga fact layer (migration 074). One row per (ChartResultId, PlanetCode,
-- SchemeCode) -- 4 schemes x 7 classical planets = 28 rows per chart.
--
-- Distinct from Vimsopaka Bala (tbl_Rule_VimsopakaWeight, still empty/reserved -- PVR names
-- it but never gives its numeric per-varga weight table, a separate open sourcing gap this
-- migration does not touch). This is the amsa-naming layer only.
--
-- Idempotent. Apply: sqlcmd -S "localhost\SQLSERVER2025" -E -d ikiastrro -i db/101_create_fact_amsabala.sql
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID('dbo.tbl_Fact_Amsabala', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Fact_Amsabala (
        Id            INT IDENTITY(1,1) CONSTRAINT PK_Fact_Amsabala PRIMARY KEY,
        ChartResultId INT NOT NULL CONSTRAINT FK_Fact_Amsabala_ChartResult REFERENCES dbo.tbl_ChartResults (Id),
        RuleSetId     TINYINT NOT NULL CONSTRAINT FK_Fact_Amsabala_RuleSet REFERENCES dbo.tbl_Rule_Sets (Id),
        PlanetCode    VARCHAR(20) NOT NULL,
        SchemeCode    VARCHAR(20) NOT NULL,
        GroupSize     TINYINT NOT NULL,
        GoodCount     TINYINT NOT NULL,
        AmsaName      VARCHAR(30) NULL,
        Detail        NVARCHAR(400) NOT NULL,
        ComputedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_Fact_Amsabala_Computed DEFAULT (sysutcdatetime()),
        CONSTRAINT CK_Fact_Amsabala_Planet CHECK (PlanetCode IN ('SUN','MOON','MARS','MERCURY','JUPITER','VENUS','SATURN')),
        CONSTRAINT CK_Fact_Amsabala_Scheme CHECK (SchemeCode IN ('SHADVARGA','SAPTAVARGA','DASAVARGA','SHODASAVARGA')),
        CONSTRAINT CK_Fact_Amsabala_Good   CHECK (GoodCount BETWEEN 0 AND GroupSize),
        CONSTRAINT UQ_Fact_Amsabala UNIQUE (ChartResultId, PlanetCode, SchemeCode)
    );
    CREATE INDEX IX_Fact_Amsabala_Chart ON dbo.tbl_Fact_Amsabala (ChartResultId, PlanetCode);
END
GO

-- --- UI read view (project_standards.md section 4: table <-> view) -------

CREATE OR ALTER VIEW dbo.vw_ChartAmsabala
AS
SELECT c.BirthDetailId, f.ChartResultId, c.ChartType, f.RuleSetId,
       f.PlanetCode, f.SchemeCode, f.GroupSize, f.GoodCount, f.AmsaName, f.Detail,
       f.ComputedAtUtc
FROM dbo.tbl_Fact_Amsabala f
JOIN dbo.tbl_ChartResults c ON c.Id = f.ChartResultId;
GO

INSERT dbo.SchemaMigrations (ScriptName, Note)
SELECT '101_create_fact_amsabala.sql',
       'tbl_Fact_Amsabala (per-planet/scheme amsa) + vw_ChartAmsabala -- persists AmsabalaCalculator, unblocking Key Inference 3.4.'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '101_create_fact_amsabala.sql');
GO

PRINT '101 applied: tbl_Fact_Amsabala + vw_ChartAmsabala created.';
GO
