/* Two aggregated statistical rollups, refreshed on demand (not per-chart facts like
   tbl_Fact_AshtakavargaPinda -- these summarize across every currently-generated D1 chart):

   1. dbo.tbl_Fact_NakshatraLordDistribution -- how many D1 graha placements (across every
      saved person) currently resolve to each planet as Nakshatra (star) Lord.
   2. dbo.tbl_Fact_KpSubLordChainDistribution -- the same idea for the KP sub-lord chain,
      broken out per level: L1 is the classical Sub Lord already on
      tbl_Chart_KeyDetails.NakshatraSubLordPlanetId; L2-L7 come from tbl_Fact_KpSubLordChain
      (migration 095), which was schema-only until this change -- see
      src/Ikiastrro.Cli/Program.cs's ChartGenerationService composition root, which never
      passed a KpSubLordChainRepository instance, so PersistAnalytics's "if
      (_kpSubLordChainRepo is not null)" guard silently skipped it for every chart ever
      generated. Fixed alongside this migration; `backfill-analytics` now populates it.

   Both tables are a current SNAPSHOT (one row per key, TRUNCATE + reinsert), not a growing
   history -- re-run dbo.usp_RefreshNakshatraKpStatDistributions after generating/backfilling
   more charts to bring the snapshot current. Excludes the Ascendant/special-lagna KeyDetails
   rows (PointKind = 'Graha' but PlanetId IS NULL) and non-D1 chart types -- Nakshatra Lord
   and the KP sub-lord chain are Rasi-chart concepts here, not varga-specific. Zero-count
   planet/level combinations are included (LEFT JOIN + ISNULL), not omitted, so a stacked bar
   chart never silently drops a category. */
USE [ikiastrro];
GO

IF OBJECT_ID(N'dbo.tbl_Fact_NakshatraLordDistribution', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Fact_NakshatraLordDistribution
    (
        PlanetId TINYINT NOT NULL CONSTRAINT PK_Fact_NakshatraLordDistribution PRIMARY KEY,
        PlacementCount INT NOT NULL,
        SourceChartType VARCHAR(10) NOT NULL CONSTRAINT DF_Fact_NakshatraLordDistribution_ChartType DEFAULT ('D1'),
        ComputedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_Fact_NakshatraLordDistribution_Computed DEFAULT (sysutcdatetime()),
        CONSTRAINT FK_Fact_NakshatraLordDistribution_Planet FOREIGN KEY (PlanetId) REFERENCES dbo.tbl_Planets(Id)
    );
END;
GO

IF OBJECT_ID(N'dbo.tbl_Fact_KpSubLordChainDistribution', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Fact_KpSubLordChainDistribution
    (
        Level TINYINT NOT NULL,
        PlanetId TINYINT NOT NULL,
        PlacementCount INT NOT NULL,
        ComputedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_Fact_KpSubLordChainDistribution_Computed DEFAULT (sysutcdatetime()),
        CONSTRAINT PK_Fact_KpSubLordChainDistribution PRIMARY KEY (Level, PlanetId),
        CONSTRAINT FK_Fact_KpSubLordChainDistribution_Planet FOREIGN KEY (PlanetId) REFERENCES dbo.tbl_Planets(Id),
        CONSTRAINT CK_Fact_KpSubLordChainDistribution_Level CHECK (Level BETWEEN 1 AND 7)
    );
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_RefreshNakshatraKpStatDistributions
AS
BEGIN
    SET NOCOUNT ON;

    TRUNCATE TABLE dbo.tbl_Fact_NakshatraLordDistribution;
    INSERT dbo.tbl_Fact_NakshatraLordDistribution (PlanetId, PlacementCount, SourceChartType)
    SELECT pl.Id, ISNULL(c.Cnt, 0), 'D1'
    FROM dbo.tbl_Planets pl
    OUTER APPLY (
        SELECT COUNT(*) AS Cnt
        FROM dbo.tbl_Chart_KeyDetails kd
        JOIN dbo.tbl_ChartResults cr ON cr.Id = kd.ChartResultId
        WHERE kd.PointKind = 'Graha' AND kd.PlanetId IS NOT NULL AND cr.ChartType = 'D1'
          AND kd.NakshatraLordPlanetId = pl.Id
    ) c;

    TRUNCATE TABLE dbo.tbl_Fact_KpSubLordChainDistribution;
    INSERT dbo.tbl_Fact_KpSubLordChainDistribution (Level, PlanetId, PlacementCount)
    SELECT lvl.Level, pl.Id, c.Cnt + c2.Cnt
    FROM dbo.tbl_Planets pl
    CROSS JOIN (SELECT TOP (7) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS Level FROM sys.all_objects) lvl
    -- Exactly one of these two OUTER APPLYs ever matches rows for a given (Level, PlanetId) --
    -- L1 is the classical Sub Lord (tbl_Chart_KeyDetails), L2-L7 is the recursive chain fact
    -- table -- and COUNT(*) always returns 0 (never NULL) over zero matching rows, so a plain
    -- sum of the two branches is exact without any ISNULL/COALESCE needed.
    OUTER APPLY (
        SELECT COUNT(*) AS Cnt
        FROM dbo.tbl_Chart_KeyDetails kd
        JOIN dbo.tbl_ChartResults cr ON cr.Id = kd.ChartResultId
        WHERE lvl.Level = 1 AND kd.PointKind = 'Graha' AND kd.PlanetId IS NOT NULL AND cr.ChartType = 'D1'
          AND kd.NakshatraSubLordPlanetId = pl.Id
    ) c
    OUTER APPLY (
        SELECT COUNT(*) AS Cnt
        FROM dbo.tbl_Fact_KpSubLordChain fc
        WHERE lvl.Level BETWEEN 2 AND 7 AND fc.Level = lvl.Level AND fc.LordPlanetId = pl.Id
    ) c2;
END;
GO

EXEC dbo.usp_RefreshNakshatraKpStatDistributions;
GO

IF OBJECT_ID(N'dbo.SchemaMigrations', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = N'117_create_nakshatra_kp_stat_distributions.sql')
    INSERT dbo.SchemaMigrations (ScriptName, Note)
    VALUES (N'117_create_nakshatra_kp_stat_distributions.sql', N'Add NakshatraLordDistribution + KpSubLordChainDistribution snapshot rollups and usp_RefreshNakshatraKpStatDistributions; fixes the never-wired KpSubLordChainRepository DI gap; runs initial refresh.');
GO

PRINT '117 applied: nakshatra/KP sub-lord chain statistical distributions refreshed.';
GO
