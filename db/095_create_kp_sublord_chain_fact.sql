-- =====================================================================
-- KP sub-lord chain, levels 2-7. Level 1 (the classical KP "Sub Lord") already lives on
-- tbl_Chart_KeyDetails.NakshatraSubLordPlanetId; this table is deliberately just the six
-- deeper levels so that single fact is never duplicated/able to drift.
--
-- Reverses the explicit stop-at-level-2 call recorded in
-- db/_archive/021_create_nakshatra_reference_tables.sql ("KP sub-lord hierarchy levels 1-2
-- only, per rammyps's explicit call to stop there") -- rammyps asked for L1-L7 on 2026-09-13.
--
-- Deliberately NOT a materialized boundary/lookup table like tbl_NakshatraSubLords (243 rows
-- for levels 1-2). A full flat table down to level 7 is 27 x 9^6 ~= 14.3M leaf rows, and the
-- narrowest level-7 band (~13.3333 deg x (6/120)^6, driven by the Sun's short Vimshottari
-- span) is ~1e-8 degrees wide -- far past what a hand-seeded DECIMAL(9,6) boundary table (the
-- existing convention) can represent. Levels 2-7 are pure recursive arithmetic instead
-- (AstroMath.GetKpSubLordChain): each level re-applies the same Vimshottari-proportioned
-- 9-way split inside the previous level's own span, cycling Ketu..Mercury from that span's
-- own lord. This table stores only the resolved lord per actual planet placement, one row per
-- (chart, planet, level) -- same shape as tbl_Fact_PlanetaryStrengthComponent /
-- tbl_Fact_BhavaStrengthComponent.
--
-- Schema only in this migration: nothing populates it yet. Wiring into chart generation
-- (ChartAnalyzer / ChartKeyDetailsRepository) is a follow-up, not part of this change.
-- =====================================================================
USE [ikiastrro];
GO

IF OBJECT_ID('dbo.tbl_Fact_KpSubLordChain', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Fact_KpSubLordChain (
        Id            INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Fact_KpSubLordChain PRIMARY KEY,
        ChartResultId INT     NOT NULL CONSTRAINT FK_Fact_KpSubLordChain_ChartResult FOREIGN KEY REFERENCES dbo.tbl_ChartResults (Id),
        PlanetId      TINYINT NOT NULL CONSTRAINT FK_Fact_KpSubLordChain_Planet      FOREIGN KEY REFERENCES dbo.tbl_Planets (Id),
        Level         TINYINT NOT NULL,   -- 2-7; level 1 is tbl_Chart_KeyDetails.NakshatraSubLordPlanetId
        LordPlanetId  TINYINT NOT NULL CONSTRAINT FK_Fact_KpSubLordChain_LordPlanet  FOREIGN KEY REFERENCES dbo.tbl_Planets (Id),
        ComputedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_Fact_KpSubLordChain_Computed DEFAULT (sysutcdatetime()),
        CONSTRAINT CK_Fact_KpSubLordChain_Level CHECK (Level BETWEEN 2 AND 7),
        CONSTRAINT UQ_Fact_KpSubLordChain UNIQUE (ChartResultId, PlanetId, Level)
    );
    CREATE INDEX IX_Fact_KpSubLordChain_Chart ON dbo.tbl_Fact_KpSubLordChain (ChartResultId, PlanetId, Level);
END
GO

IF OBJECT_ID(N'dbo.SchemaMigrations', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = N'095_create_kp_sublord_chain_fact.sql')
    INSERT dbo.SchemaMigrations (ScriptName, Note)
    VALUES (N'095_create_kp_sublord_chain_fact.sql', N'Add tbl_Fact_KpSubLordChain (KP sub-lord levels 2-7, one row per chart/planet/level); level 1 stays on tbl_Chart_KeyDetails.NakshatraSubLordPlanetId. Schema only -- nothing populates it yet.');
GO

PRINT '095 applied: tbl_Fact_KpSubLordChain ready (schema only; see AstroMath.GetKpSubLordChain for the levels 2-7 algorithm).';
GO
