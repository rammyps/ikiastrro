-- =====================================================================
-- 156 - tbl_Fact_HouseStrengthStatistics + vw_ChartHouseStrengthStatistics:
-- the Life Matters strength statistics saved as data, one row per chart x
-- sign, so they can be queried and exported across many people instead of
-- existing only while the page renders.
--
-- Written by HouseStrengthStatisticsService (Ikiastrro.Data) from the same
-- LifeMatterStatistics class the Life Matters page uses, so saved rows and
-- the page cannot disagree. Filled at the end of
-- ChartGenerationService.GenerateAll / RecomputeAnalytics, and for people
-- already in the database by CLI `backfill-strength-statistics`.
--
-- Per row: the sign's raw signals (SAV, the lord's BAV, raw and independent
-- Bhava Bala with its chart-relative z-score, the lord's Shadbala % and
-- Shodasavarga Amsabala %, Argala pair counts) and the three axes of
-- docs/research/domain/stat_strength.md §4 read for the LORD ONLY:
-- Capacity / Consistency / Context / StrengthPercent (0-100). A matter's
-- karakas join Capacity/Consistency on the page; they are matter-specific,
-- so they are not stored here - join vw_ChartShadbala / vw_ChartAmsabala
-- for a karaka's own figures.
--
-- Bhava Bala is a D1 computation: vargas carry NULL Bhava columns. Argala
-- for a varga is computed live from its placements (ArgalaFacts.ForChart).
--
-- FK to tbl_ChartResults (no cascade), like every other tbl_Fact_*: both
-- ChartGenerationService.GenerateAll and BirthDetailDeletionService delete
-- these rows first (the migration-128 Argala lesson).
--
-- Apply:  sqlcmd -S "localhost\SQLSERVER2025" -E -d ikiastrro -b -i db/156_create_fact_house_strength_statistics.sql
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID('dbo.tbl_Fact_HouseStrengthStatistics', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Fact_HouseStrengthStatistics (
        Id                    INT IDENTITY(1,1) CONSTRAINT PK_Fact_HouseStrengthStatistics PRIMARY KEY,
        ChartResultId         INT NOT NULL CONSTRAINT FK_Fact_HouseStrengthStatistics_ChartResult REFERENCES dbo.tbl_ChartResults (Id),
        RuleSetId             TINYINT NOT NULL CONSTRAINT FK_Fact_HouseStrengthStatistics_RuleSet REFERENCES dbo.tbl_Rule_Sets (Id),
        SignId                TINYINT NOT NULL CONSTRAINT FK_Fact_HouseStrengthStatistics_Sign REFERENCES dbo.tbl_SignAttributes (Id),
        HouseFromLagna        TINYINT NOT NULL,
        LordPlanetId          TINYINT NOT NULL CONSTRAINT FK_Fact_HouseStrengthStatistics_Lord REFERENCES dbo.tbl_Planets (Id),
        SavBindus             SMALLINT NULL,
        LordBavBindus         TINYINT NULL,
        BhavaBalaRupas        DECIMAL(8,3) NULL,
        IndependentBhavaRupas DECIMAL(8,3) NULL,
        IndependentBhavaZ     DECIMAL(8,4) NULL,
        LordShadbalaPercent   DECIMAL(9,2) NULL,
        LordAmsabalaPercent   TINYINT NULL,
        ArgalaHolds           TINYINT NOT NULL,
        ArgalaContested       TINYINT NOT NULL,
        ArgalaObstructed      TINYINT NOT NULL,
        Capacity              TINYINT NULL,
        Consistency           TINYINT NULL,
        Context               TINYINT NULL,
        StrengthPercent       TINYINT NULL,
        ComputedAtUtc         DATETIME2(0) NOT NULL CONSTRAINT DF_Fact_HouseStrengthStatistics_Computed DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT UQ_Fact_HouseStrengthStatistics UNIQUE (ChartResultId, SignId),
        CONSTRAINT CK_Fact_HouseStrengthStatistics_House CHECK (HouseFromLagna BETWEEN 1 AND 12)
    );
END
GO

CREATE OR ALTER VIEW dbo.vw_ChartHouseStrengthStatistics
AS
SELECT cr.BirthDetailId,
       bd.Name,
       cr.ChartType,
       s.ChartResultId,
       s.HouseFromLagna,
       sa.SignName        AS Sign,
       lord.PlanetName    AS LordPlanet,
       s.SavBindus,
       s.LordBavBindus,
       s.BhavaBalaRupas,
       s.IndependentBhavaRupas,
       s.IndependentBhavaZ,
       s.LordShadbalaPercent,
       s.LordAmsabalaPercent,
       s.ArgalaHolds,
       s.ArgalaContested,
       s.ArgalaObstructed,
       s.ArgalaHolds - s.ArgalaObstructed AS ArgalaNet,
       s.Capacity,
       s.Consistency,
       s.Context,
       s.StrengthPercent,
       s.RuleSetId,
       s.ComputedAtUtc
FROM dbo.tbl_Fact_HouseStrengthStatistics s
JOIN dbo.tbl_ChartResults cr   ON cr.Id = s.ChartResultId
JOIN dbo.tbl_BirthDetails bd   ON bd.Id = cr.BirthDetailId
JOIN dbo.tbl_SignAttributes sa ON sa.Id = s.SignId
JOIN dbo.tbl_Planets lord      ON lord.Id = s.LordPlanetId;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '156_create_fact_house_strength_statistics.sql')
    INSERT dbo.SchemaMigrations (ScriptName, AppliedAtUtc, Note)
    VALUES ('156_create_fact_house_strength_statistics.sql', SYSUTCDATETIME(),
            'Saved Life Matters strength statistics per chart x sign, plus vw_ChartHouseStrengthStatistics.');
GO

PRINT '156 applied: tbl_Fact_HouseStrengthStatistics + vw_ChartHouseStrengthStatistics.';
GO
