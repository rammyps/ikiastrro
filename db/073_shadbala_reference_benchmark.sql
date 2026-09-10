-- =====================================================================
-- 073 — Shadbala reference benchmark: the seven-planet golden totals
--       transcribed from the Jagannatha Hora natal export for
--       1_Ramakrishnan (docs/artifacts/reference-charts/Rammy_Jagannatha.txt).
--
-- Hangs off the migration-46 benchmark case (tbl_Dim_AyanamsaBenchmarkCases
-- / BENCH_RAMAKRISHNAN_P_JHORA_1981), which docs/database/MASTER.md reports
-- orphaned in the live DB — this script re-asserts it idempotently first.
-- A future verify-shadbala asserts computed Shadbala against these rows.
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- 1. Re-assert the benchmark case row (migration 46's record).
DECLARE @BirthDetailId INT =
    (SELECT TOP (1) Id FROM dbo.tbl_BirthDetails
      WHERE Name IN (N'Ramakrishnan P', N'Ramakrishnan')
      ORDER BY CASE WHEN Name = N'Ramakrishnan P' THEN 0 ELSE 1 END, Id);

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Dim_AyanamsaBenchmarkCases WHERE Code = 'BENCH_RAMAKRISHNAN_P_JHORA_1981')
    INSERT dbo.tbl_Dim_AyanamsaBenchmarkCases
      (Code, BirthDetailId, PersonName, BirthDate, BirthTime, UtcOffset, LatitudeDegrees, LongitudeDegrees,
       ReferenceAyanamsaDegrees, ReferenceSourceRefCode, VimshottariDaysPerYear, Notes)
    VALUES
      ('BENCH_RAMAKRISHNAN_P_JHORA_1981', @BirthDetailId, N'Ramakrishnan P', '1981-04-22', '05:30:01', '05:30',
       13.08333333, 80.28333333, 23.59495278, 'SRC_JHORA_EXPORT_RAMAKRISHNAN', 365.242500,
       N'Golden record transcribed from the supplied Jagannatha Hora natal export; latitude 13N05, longitude 80E17.');
GO

-- 2. Benchmark table for the Shadbala totals.
IF OBJECT_ID('dbo.tbl_Dim_ShadbalaBenchmarkValues', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Dim_ShadbalaBenchmarkValues (
        Id                      INT IDENTITY(1,1) CONSTRAINT PK_Dim_ShadbalaBenchmarkValues PRIMARY KEY,
        AyanamsaBenchmarkCaseId INT NOT NULL CONSTRAINT FK_Dim_ShadbalaBenchmarkValues_Case
                                REFERENCES dbo.tbl_Dim_AyanamsaBenchmarkCases (Id),
        PlanetCode              VARCHAR(20) NOT NULL,
        ShadbalaVirupas         DECIMAL(9,3) NOT NULL,
        ShadbalaRupas           DECIMAL(8,3) NOT NULL,
        PercentStrength         DECIMAL(8,3) NOT NULL,
        IshtaPhala              DECIMAL(8,3) NULL,
        KashtaPhala             DECIMAL(8,3) NULL,
        SourceRefCode           VARCHAR(40) NOT NULL CONSTRAINT FK_Dim_ShadbalaBenchmarkValues_Source
                                REFERENCES dbo.tbl_Dim_Source (Code),
        Notes                   NVARCHAR(300) NULL,
        CONSTRAINT UQ_Dim_ShadbalaBenchmarkValues_CasePlanet UNIQUE (AyanamsaBenchmarkCaseId, PlanetCode),
        CONSTRAINT CK_Dim_ShadbalaBenchmarkValues_Positive CHECK (ShadbalaRupas > 0 AND ShadbalaVirupas > 0)
    );
END
GO

DECLARE @CaseId INT = (SELECT Id FROM dbo.tbl_Dim_AyanamsaBenchmarkCases WHERE Code = 'BENCH_RAMAKRISHNAN_P_JHORA_1981');

MERGE dbo.tbl_Dim_ShadbalaBenchmarkValues AS target
USING (VALUES
    ('Sun',     417.03, 6.95, 139.01, 48.99,  3.42),
    ('Moon',    360.77, 6.01, 100.21,  8.48, 23.83),
    ('Mars',    609.75, 10.16, 203.25, 46.64,  7.82),
    ('Mercury', 324.45, 5.41,  77.25,  4.92, 55.04),
    ('Jupiter', 421.20, 7.02, 108.00, 44.77, 13.27),
    ('Venus',   422.41, 7.04, 128.00, 14.86, 16.73),
    ('Saturn',  387.74, 6.46, 129.25, 48.76, 11.07)
) AS source (PlanetCode, ShadbalaVirupas, ShadbalaRupas, PercentStrength, IshtaPhala, KashtaPhala)
ON target.AyanamsaBenchmarkCaseId = @CaseId AND target.PlanetCode = source.PlanetCode
WHEN NOT MATCHED THEN
    INSERT (AyanamsaBenchmarkCaseId, PlanetCode, ShadbalaVirupas, ShadbalaRupas, PercentStrength,
            IshtaPhala, KashtaPhala, SourceRefCode, Notes)
    VALUES (@CaseId, source.PlanetCode, source.ShadbalaVirupas, source.ShadbalaRupas, source.PercentStrength,
            source.IshtaPhala, source.KashtaPhala, 'SRC_JHORA_EXPORT_RAMAKRISHNAN',
            N'JHora "Shadbala In rupas / % Strength / IshtaPhala / KashtaPhala" table.');
GO

INSERT dbo.SchemaMigrations (ScriptName, Note)
SELECT '073_shadbala_reference_benchmark.sql',
       'Add tbl_Dim_ShadbalaBenchmarkValues and seed the seven-planet JHora golden totals for BENCH_RAMAKRISHNAN_P_JHORA_1981; re-assert the benchmark case row.'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '073_shadbala_reference_benchmark.sql');
GO

PRINT '073 applied: Shadbala reference benchmark seeded.';
GO
