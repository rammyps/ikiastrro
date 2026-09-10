-- =====================================================================
-- 071 — Restore the vw_ChartShadbala consumer contract (regressed by 069)
--       and add the per-planet minimum-rupa reference so PercentOfMinimum
--       is populated.
--
-- Migration 069 rewrote vw_ChartShadbala as a flat `SELECT s.*` off
-- tbl_Fact_PlanetaryStrength, dropping BirthDetailId, the Planet name and
-- PercentOfMinimum. AstrologerEvidenceRepository queries
--   SELECT ... FROM dbo.vw_ChartShadbala WHERE BirthDetailId=@id ORDER BY Planet
-- so /charts/{id}/evidence 500s. This migration re-creates the view with the
-- migration-053 column set PLUS the 069 Rashmi / Parasara columns.
--
-- tbl_Rule_ShadbalaMinimumRupas holds the classical Parasari required
-- Shadbala. Confirmed against the JHora export for 1_Ramakrishnan:
-- ShadbalaRupas / MinimumRupas * 100 reproduces JHora %Strength for all
-- seven grahas (Sun 6.95/5 = 139.0, Mercury 5.41/7 = 77.3, ...).
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID('dbo.tbl_Rule_ShadbalaMinimumRupas', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Rule_ShadbalaMinimumRupas (
        Id                   INT IDENTITY(1,1) CONSTRAINT PK_Rule_ShadbalaMinimumRupas PRIMARY KEY,
        RuleSetId            TINYINT NOT NULL CONSTRAINT FK_Rule_ShadbalaMinimumRupas_RuleSet REFERENCES dbo.tbl_Rule_Sets (Id),
        StrengthProfileCode  VARCHAR(40) NOT NULL,
        PlanetId             TINYINT NOT NULL CONSTRAINT FK_Rule_ShadbalaMinimumRupas_Planet REFERENCES dbo.tbl_Planets (Id),
        MinimumRupas         DECIMAL(6,3) NOT NULL,
        SourceRefCode        VARCHAR(40) NULL,
        FormulaSourceRefCode VARCHAR(40) NULL,
        CalculationNarrative NVARCHAR(MAX) NULL,
        IsActive             BIT NOT NULL CONSTRAINT DF_Rule_ShadbalaMinimumRupas_IsActive DEFAULT (1),
        CONSTRAINT CK_Rule_ShadbalaMinimumRupas_Src        CHECK (SourceRefCode IS NULL OR SourceRefCode LIKE 'SRC[_]%'),
        CONSTRAINT CK_Rule_ShadbalaMinimumRupas_FormulaSrc CHECK (FormulaSourceRefCode IS NULL OR FormulaSourceRefCode LIKE 'SRC[_]%'),
        CONSTRAINT CK_Rule_ShadbalaMinimumRupas_Positive   CHECK (MinimumRupas > 0),
        CONSTRAINT UQ_Rule_ShadbalaMinimumRupas UNIQUE (RuleSetId, StrengthProfileCode, PlanetId)
    );
END
GO

-- Classical Parasari required Shadbala in rupas (1 rupa = 60 virupas).
-- Raman, Graha and Bhava Balas — minimum required strength table.
;WITH seed (PlanetName, MinimumRupas, Narrative) AS (
    SELECT * FROM (VALUES
        ('Sun',     5.000, 'Required Shadbala for the Sun: 5 rupas (390 virupas).'),
        ('Moon',    6.000, 'Required Shadbala for the Moon: 6 rupas (360 virupas).'),
        ('Mars',    5.000, 'Required Shadbala for Mars: 5 rupas (300 virupas).'),
        ('Mercury', 7.000, 'Required Shadbala for Mercury: 7 rupas (420 virupas).'),
        ('Jupiter', 6.500, 'Required Shadbala for Jupiter: 6.5 rupas (390 virupas).'),
        ('Venus',   5.500, 'Required Shadbala for Venus: 5.5 rupas (330 virupas).'),
        ('Saturn',  5.000, 'Required Shadbala for Saturn: 5 rupas (300 virupas).')
    ) s (PlanetName, MinimumRupas, Narrative)
)
INSERT dbo.tbl_Rule_ShadbalaMinimumRupas
    (RuleSetId, StrengthProfileCode, PlanetId, MinimumRupas, SourceRefCode, FormulaSourceRefCode, CalculationNarrative)
SELECT 1, 'PVR_INTEGRATED_STRENGTH', p.Id, s.MinimumRupas,
       'SRC_PVR_INTEGRATED', 'SRC_RAMAN_GRAHA_BHAVA_BALAS', s.Narrative
FROM seed s
JOIN dbo.tbl_Planets p ON p.PlanetName = s.PlanetName
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.tbl_Rule_ShadbalaMinimumRupas r
    WHERE r.RuleSetId = 1 AND r.StrengthProfileCode = 'PVR_INTEGRATED_STRENGTH' AND r.PlanetId = p.Id
);
GO

-- Backfill MinimumRequiredRupas on any strength rows already stored, so the
-- view's PercentOfMinimum is populated without a recompute.
UPDATE s
   SET s.MinimumRequiredRupas = r.MinimumRupas
FROM dbo.tbl_Fact_PlanetaryStrength s
JOIN dbo.tbl_Rule_ShadbalaMinimumRupas r
     ON r.RuleSetId = s.RuleSetId
    AND r.StrengthProfileCode = s.StrengthProfileCode
    AND r.PlanetId = s.PlanetId
WHERE s.MinimumRequiredRupas IS NULL;
GO

-- Re-create the view with the full consumer contract: the migration-053
-- columns (BirthDetailId, Planet name, PercentOfMinimum) plus the 069
-- Rashmi / Parasara columns.
CREATE OR ALTER VIEW dbo.vw_ChartShadbala
AS
SELECT c.BirthDetailId, s.ChartResultId, c.RuleSetId, p.PlanetName AS Planet,
       s.SthanaBalaVirupas, s.DigBalaVirupas, s.KalaBalaVirupas,
       s.CheshtaBalaVirupas, s.NaisargikaBalaVirupas, s.DrikBalaVirupas,
       s.YuddhaBalaVirupas, s.ShadbalaVirupas, s.ShadbalaRupas,
       s.MinimumRequiredRupas,
       CASE WHEN s.MinimumRequiredRupas IS NULL OR s.MinimumRequiredRupas = 0 THEN NULL
            ELSE CONVERT(DECIMAL(9,2), s.ShadbalaRupas * 100.0 / s.MinimumRequiredRupas) END AS PercentOfMinimum,
       s.IshtaBala, s.KashtaBala,
       s.UchchaRashmi, s.CheshtaRashmi, s.SubhaRashmi, s.AsubhaRashmi,
       s.IshtaPhalaParasara, s.KashtaPhalaParasara,
       s.StrengthProfileCode, s.FormulaSourceRefCode, s.CalculationNarrative, s.ComputedAtUtc
FROM dbo.tbl_Fact_PlanetaryStrength s
JOIN dbo.tbl_ChartResults c ON c.Id = s.ChartResultId
JOIN dbo.tbl_Planets p ON p.Id = s.PlanetId;
GO

INSERT dbo.SchemaMigrations (ScriptName, Note)
SELECT '071_fix_shadbala_view_and_minimum_rupas.sql',
       'Restore vw_ChartShadbala consumer columns (BirthDetailId/Planet/PercentOfMinimum) regressed by 069; add tbl_Rule_ShadbalaMinimumRupas + classical seed; backfill MinimumRequiredRupas.'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '071_fix_shadbala_view_and_minimum_rupas.sql');
GO

PRINT '071 applied: vw_ChartShadbala restored, minimum-rupa reference seeded.';
GO
