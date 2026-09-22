-- =====================================================================
-- 132 - Persist longitude-based (sphuta) Graha-drishti strengths for the
-- three relationship charts currently in scope: D1, D9 and D10.
-- Discrete whole-sign aspects remain in tbl_Chart_Aspects; this table
-- stores strength and its calculation evidence without duplicating them.
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID('dbo.tbl_Fact_GrahaDrishtiStrengths', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Fact_GrahaDrishtiStrengths (
        Id                         INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Fact_GrahaDrishtiStrengths PRIMARY KEY,
        ChartResultId              INT          NOT NULL CONSTRAINT FK_Fact_GrahaDrishtiStrengths_ChartResult FOREIGN KEY REFERENCES dbo.tbl_ChartResults (Id),
        RuleSetId                  TINYINT      NOT NULL CONSTRAINT FK_Fact_GrahaDrishtiStrengths_RuleSet FOREIGN KEY REFERENCES dbo.tbl_Rule_Sets (Id),
        ChartTypeId                TINYINT      NOT NULL CONSTRAINT FK_Fact_GrahaDrishtiStrengths_ChartType FOREIGN KEY REFERENCES dbo.tbl_Dim_ChartType (Id),
        AspectingPlanetId          TINYINT      NOT NULL CONSTRAINT FK_Fact_GrahaDrishtiStrengths_AspectingPlanet FOREIGN KEY REFERENCES dbo.tbl_Planets (Id),
        AspectedPointKind          VARCHAR(20)  NOT NULL,
        AspectedPointKey           VARCHAR(40)  NOT NULL,
        AspectedPlanetId           TINYINT      NULL CONSTRAINT FK_Fact_GrahaDrishtiStrengths_AspectedPlanet FOREIGN KEY REFERENCES dbo.tbl_Planets (Id),
        AspectingLongitudeDegrees  DECIMAL(9,6) NOT NULL,
        AspectedLongitudeDegrees   DECIMAL(9,6) NOT NULL,
        DirectedSeparationDegrees  DECIMAL(9,6) NOT NULL,
        OrdinaryVirupas            DECIMAL(9,6) NOT NULL,
        SpecialVirupas             DECIMAL(9,6) NOT NULL,
        TotalVirupas               DECIMAL(9,6) NOT NULL,
        StrengthPercentage         DECIMAL(5,2) NOT NULL,
        DiscreteAspectHouse        TINYINT      NULL,
        SourceRefCode              VARCHAR(40)  NOT NULL CONSTRAINT DF_Fact_GrahaDrishtiStrengths_Source DEFAULT 'SRC_PVR_INTEGRATED',
        CONSTRAINT CK_Fact_GrahaDrishtiStrengths_AspectingLongitude CHECK (AspectingLongitudeDegrees >= 0 AND AspectingLongitudeDegrees < 360),
        CONSTRAINT CK_Fact_GrahaDrishtiStrengths_AspectedLongitude CHECK (AspectedLongitudeDegrees >= 0 AND AspectedLongitudeDegrees < 360),
        CONSTRAINT CK_Fact_GrahaDrishtiStrengths_Separation CHECK (DirectedSeparationDegrees >= 0 AND DirectedSeparationDegrees < 360),
        CONSTRAINT CK_Fact_GrahaDrishtiStrengths_Virupas CHECK (OrdinaryVirupas BETWEEN 0 AND 60 AND SpecialVirupas BETWEEN 0 AND 60 AND TotalVirupas BETWEEN 0 AND 60),
        CONSTRAINT CK_Fact_GrahaDrishtiStrengths_Percentage CHECK (StrengthPercentage BETWEEN 0 AND 100),
        CONSTRAINT CK_Fact_GrahaDrishtiStrengths_DiscreteHouse CHECK (DiscreteAspectHouse IS NULL OR DiscreteAspectHouse IN (3,4,5,7,8,9,10)),
        CONSTRAINT CK_Fact_GrahaDrishtiStrengths_TargetPlanet CHECK ((AspectedPointKind = 'Graha' AND AspectedPlanetId IS NOT NULL) OR (AspectedPointKind <> 'Graha' AND AspectedPlanetId IS NULL)),
        CONSTRAINT CK_Fact_GrahaDrishtiStrengths_Source CHECK (SourceRefCode LIKE 'SRC[_]%'),
        CONSTRAINT UQ_Fact_GrahaDrishtiStrengths UNIQUE (ChartResultId, AspectingPlanetId, AspectedPointKind, AspectedPointKey)
    );

    CREATE INDEX IX_Fact_GrahaDrishtiStrengths_ChartResult
        ON dbo.tbl_Fact_GrahaDrishtiStrengths (ChartResultId, AspectingPlanetId);
END
GO

CREATE OR ALTER VIEW dbo.vw_ChartGrahaDrishtiStrengths
AS
SELECT f.Id, f.ChartResultId, cr.BirthDetailId, ct.Code AS ChartType,
       ap.PlanetName AS AspectingPlanet, f.AspectedPointKind, f.AspectedPointKey,
       tp.PlanetName AS AspectedPlanet, f.AspectingLongitudeDegrees,
       f.AspectedLongitudeDegrees, f.DirectedSeparationDegrees,
       f.OrdinaryVirupas, f.SpecialVirupas, f.TotalVirupas,
       f.StrengthPercentage, f.DiscreteAspectHouse,
       CAST(CASE WHEN f.DiscreteAspectHouse IS NULL THEN 0 ELSE 1 END AS BIT) AS IsDiscreteAspect,
       f.RuleSetId, f.SourceRefCode
FROM dbo.tbl_Fact_GrahaDrishtiStrengths f
JOIN dbo.tbl_ChartResults cr ON cr.Id = f.ChartResultId
JOIN dbo.tbl_Dim_ChartType ct ON ct.Id = f.ChartTypeId
JOIN dbo.tbl_Planets ap ON ap.Id = f.AspectingPlanetId
LEFT JOIN dbo.tbl_Planets tp ON tp.Id = f.AspectedPlanetId;
GO

INSERT dbo.SchemaMigrations (ScriptName, Note)
SELECT '132_create_fact_graha_drishti_strengths.sql',
       'Graha-drishti strength fact and evidence view created for D1/D9/D10'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '132_create_fact_graha_drishti_strengths.sql');
GO

PRINT '132 applied: Graha-drishti strength fact and view ready.';
GO
