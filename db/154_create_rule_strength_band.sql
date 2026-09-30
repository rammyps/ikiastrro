-- =====================================================================
-- 154 - tbl_Rule_StrengthBand: the app's one set of Strong / Moderate /
-- Weak cut-offs for three strength statistics, each boundary with its own
-- SourceRefCode.
--
-- Before this, the cut-offs were constants scattered across the UI, and
-- two pages disagreed about the same planet: Key Inference 3.1 called
-- Shadbala Strong at >=100% of the required minimum (Moderate from 80%),
-- Life Matters at >=110% (Weak below 90%). rammyps chose 2026-10-01 to
-- standardise on >=100% / 80% everywhere, because 100% is the one boundary
-- with a classical source: Parasara's required Shadbala (BPHS 27.32-33,
-- the minimums already in tbl_Rule_ShadbalaMinimumRupas) - a planet at or
-- above its requirement is strong. The 80% Moderate floor is the
-- project's own heuristic and is registered as SRC_IKIASTRRO_SYNTHESIS.
--
-- Bhava Bala 7 / 5 Rupas: no classical cut-off found in the corpus, so
-- both boundaries are SRC_IKIASTRRO_SYNTHESIS (unchanged values).
-- Sarvashtakavarga >30 favourable / <25 unfavourable: PVR, as recorded in
-- docs/research/domain/transit-events.md (unchanged values), stored as
-- whole-bindu inclusive lower bounds 31 / 25.
--
-- Every bound is an inclusive LOWER bound: >= StrongFrom is Strong,
-- >= ModerateFrom is Moderate, below is Weak.
--
-- Mirrored in code by Ikiastrro.Core.Engines.Strength.StrengthBands (the
-- same verified-mirror pattern as AstroMath.DeepExaltationPoints); CLI
-- `verify-strength` checks the two agree.
--
-- Apply:  sqlcmd -S "localhost\SQLSERVER2025" -E -d ikiastrro -b -i db/154_create_rule_strength_band.sql
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID('dbo.tbl_Rule_StrengthBand', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Rule_StrengthBand (
        Id                   INT IDENTITY(1,1) CONSTRAINT PK_Rule_StrengthBand PRIMARY KEY,
        RuleSetId            TINYINT NOT NULL CONSTRAINT FK_Rule_StrengthBand_RuleSet REFERENCES dbo.tbl_Rule_Sets (Id),
        ScaleCode            VARCHAR(40) NOT NULL,
        BandCode             VARCHAR(20) NOT NULL,
        LowerBound           DECIMAL(8,3) NOT NULL,
        SourceRefCode        VARCHAR(40) NOT NULL,
        CalculationNarrative NVARCHAR(MAX) NULL,
        IsActive             BIT NOT NULL CONSTRAINT DF_Rule_StrengthBand_IsActive DEFAULT (1),
        CONSTRAINT CK_Rule_StrengthBand_Band CHECK (BandCode IN ('STRONG', 'MODERATE')),
        CONSTRAINT CK_Rule_StrengthBand_Src  CHECK (SourceRefCode LIKE 'SRC[_]%'),
        CONSTRAINT UQ_Rule_StrengthBand UNIQUE (RuleSetId, ScaleCode, BandCode)
    );
END
GO

;WITH seed (ScaleCode, BandCode, LowerBound, SourceRefCode, Narrative) AS (
    SELECT * FROM (VALUES
        ('SHADBALA_PCT_OF_MIN', 'STRONG',   100.000, 'SRC_BPHS_27',
            N'Shadbala at or above the planet''s required minimum (tbl_Rule_ShadbalaMinimumRupas, BPHS 27.32-33) - Parasara''s own "strong enough" line.'),
        ('SHADBALA_PCT_OF_MIN', 'MODERATE',  80.000, 'SRC_IKIASTRRO_SYNTHESIS',
            N'Project heuristic: within 20% of the required minimum reads Moderate rather than Weak. No classical source.'),
        ('BHAVA_BALA_RUPAS',    'STRONG',     7.000, 'SRC_IKIASTRRO_SYNTHESIS',
            N'Project heuristic: 7 Rupas and above reads Strong. No classical cut-off for Bhava Bala found in the corpus.'),
        ('BHAVA_BALA_RUPAS',    'MODERATE',   5.000, 'SRC_IKIASTRRO_SYNTHESIS',
            N'Project heuristic: 5 to under 7 Rupas reads Moderate; below 5 Weak.'),
        ('SAV_BINDUS',          'STRONG',    31.000, 'SRC_PVR_INTEGRATED',
            N'Sarvashtakavarga above 30 bindus in a sign is favourable (docs/research/domain/transit-events.md); 31 as a whole-bindu inclusive bound.'),
        ('SAV_BINDUS',          'MODERATE',  25.000, 'SRC_PVR_INTEGRATED',
            N'Sarvashtakavarga below 25 bindus is unfavourable (same source); 25 to 30 is the middle band.')
    ) s (ScaleCode, BandCode, LowerBound, SourceRefCode, Narrative)
)
INSERT dbo.tbl_Rule_StrengthBand (RuleSetId, ScaleCode, BandCode, LowerBound, SourceRefCode, CalculationNarrative)
SELECT 1, s.ScaleCode, s.BandCode, s.LowerBound, s.SourceRefCode, s.Narrative
FROM seed s
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.tbl_Rule_StrengthBand r
    WHERE r.RuleSetId = 1 AND r.ScaleCode = s.ScaleCode AND r.BandCode = s.BandCode
);
GO

IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '154_create_rule_strength_band.sql')
    INSERT dbo.SchemaMigrations (ScriptName, AppliedAtUtc, Note)
    VALUES ('154_create_rule_strength_band.sql', SYSUTCDATETIME(),
            'Strong/Moderate/Weak cut-offs for Shadbala %, Bhava Bala and SAV, one SourceRefCode per boundary.');
GO

DECLARE @rows INT = (SELECT COUNT(*) FROM dbo.tbl_Rule_StrengthBand WHERE RuleSetId = 1 AND IsActive = 1);
PRINT '154 applied: ' + CAST(@rows AS VARCHAR(10)) + ' tbl_Rule_StrengthBand rows (expect 6).';
GO
