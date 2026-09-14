-- =====================================================================
-- 100 — Fix a wrong Sun minimum-Shadbala seed value (db/071), add an
--       engine-reachable MAXIMUM Shadbala reference table, and make
--       vw_ChartShadbala compute both live via a JOIN instead of reading
--       tbl_Fact_PlanetaryStrength.MinimumRequiredRupas.
--
-- --- Bug found (2026-09-15, researching min/max for Key Inference 3.1) ---
-- tbl_Rule_ShadbalaMinimumRupas seeded Sun as 5.000 rupas, but its own
-- CalculationNarrative says "5 rupas (390 virupas)" — 390 virupas is 6.500
-- rupas, not 5. Cross-checked against BPHS 27.32-33 (the classical source)
-- via two independent references (a direct BPHS-verse citation and the
-- Saravali open-source Jyotish engine's own Shadbala requirement table):
-- required Shadbala is 390/360/300/420/390/330/300 virupas for
-- Sun/Moon/Mars/Mercury/Jupiter/Venus/Saturn = 6.5/6.0/5.0/7.0/6.5/5.5/5.0
-- rupas. Every planet except Sun already matches; only Sun is wrong.
--
-- --- Why the view is rewritten, not just backfilled again ---
-- PlanetaryStrengthRepository.InsertAll never sets MinimumRequiredRupas —
-- migration 071's UPDATE only backfilled rows that existed on 2026-09-13;
-- every chart (re)computed since has NULL there, so vw_ChartShadbala's
-- PercentOfMinimum/Status silently go blank in the UI (confirmed live on
-- person 3/Ramakrishnan, 2026-09-14, building Key Inference 3.1). Reading
-- the reference tables live via JOIN instead of a stored snapshot column
-- means this can never regress again, for Minimum or the new Maximum,
-- without touching the insert path. The stored MinimumRequiredRupas
-- column on tbl_Fact_PlanetaryStrength is left in place (still written by
-- InsertAll's VALUES list — unchanged) but the view no longer reads it.
--
-- --- "Maximum" — engine-reachable, not full-classical (rammyps's call,
--     2026-09-15) ---
-- BPHS never defines a maximum (only Parasara's minimum "strong enough"
-- threshold above). A theoretical full-classical ceiling exists per
-- component (Sthana 480 virupas, Dig 60, Chesta 60 virupas — Saravali
-- open-source Jyotish engine's own reference pages), but Kala Bala's full
-- classical form (Tribhaga/Varsha/Masa/Hora/Ayana) is far larger than
-- what ShadbalaCalculator.cs actually computes (Nathonnata + Paksha only,
-- 120-virupas ceiling) — a full-classical max would be a ceiling no chart
-- could ever reach. So this seeds an ENGINE-REACHABLE max instead: the
-- ceiling of ShadbalaCalculator.cs's own formulas as coded today —
-- Sthana 480 (Uchcha 60 + Saptavargaja 315, PvrDignityEvaluator's own
-- Moolatrikona=45 * 7 vargas + Ojayugma 30 + Kendradi 60 + Drekkana 15,
-- matches Saravali's classical per-sub-component maxima) + Dig 60 (same
-- for every planet — AddDig's fraction=0 case) + Kala 120 (Nathonnata
-- max 60 + Paksha — AddKala's non-Moon branch is a constant 60, Moon's
-- ranges 0-60 and maxes at full/new moon) + Cheshta (AddCheshta hardcodes
-- 0 for Sun/Moon, 60 for the other five — a deliberate simplification,
-- not full BPHS where Sun's Cheshta = its Ayana Bala and Moon's = its
-- Paksha Bala) + Naisargika (AddNaisargika's fixed per-planet constant —
-- this alone is both min and max since it never varies) + Drik (AddDrik:
-- signed sum of up to 8 other bodies' aspects /4, so the true ceiling is
-- reached only if every naturally-benefic source — Moon/Mercury/
-- Jupiter/Venus, minus the target itself if it's one of the four — sits
-- at its own max +60-virupas aspect angle simultaneously and every
-- malefic source contributes exactly 0; not astronomically realistic in
-- one real chart, same caveat the Sthana-Bala "all 7 vargas Moolatrikona"
-- ceiling already carries, so treated consistently). Registered under
-- SRC_IKIASTRRO_SYNTHESIS (migration 087) — a project-derived ceiling
-- from the engine's own code, not a direct book citation — with
-- FormulaSourceRefCode SRC_RAMAN_GRAHA_BHAVA_BALAS since the individual
-- component ceilings it sums trace back to that reference.
--
--   Planet    Sthana  Dig  Kala  Chesta  Naisargika  Drik   Total virupas  Rupas
--   Sun          480   60   120       0       60.00    60         780.00  13.000
--   Moon         480   60   120       0       51.43    45         756.43  12.607
--   Mars         480   60   120      60       17.14    60         797.14  13.286
--   Mercury      480   60   120      60       25.71    45         790.71  13.179
--   Jupiter      480   60   120      60       34.29    45         799.29  13.322
--   Venus        480   60   120      60       42.86    45         807.86  13.464
--   Saturn       480   60   120      60        8.57    60         788.57  13.143
--
-- These numbers sit far above the 5.0-7.0 rupas minimum band — expected,
-- since the ceiling requires simultaneous perfect exaltation + Moolatrikona
-- in all 7 vargas + a kendra house + (where applicable) retrograde motion
-- + the birth's day/night arc favoring the planet + every benefic aspect
-- at once, essentially unreachable in any single real chart. A %-of-max
-- reading will normally sit low; that is correct, not a bug.
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- --- Fix the Sun minimum (5.000 -> 6.500 rupas / 390 virupas) ---
UPDATE r
   SET r.MinimumRupas = 6.500,
       r.CalculationNarrative = 'Required Shadbala for the Sun: 6.5 rupas (390 virupas). Corrected 2026-09-15 — was seeded 5.000, contradicting this same row''s own virupas figure and BPHS 27.32-33.'
FROM dbo.tbl_Rule_ShadbalaMinimumRupas r
JOIN dbo.tbl_Planets p ON p.Id = r.PlanetId
WHERE p.PlanetName = 'Sun'
  AND r.RuleSetId = 1
  AND r.StrengthProfileCode = 'PVR_INTEGRATED_STRENGTH'
  AND r.MinimumRupas <> 6.500;
GO

-- --- New sibling table: engine-reachable maximum Shadbala per planet ---
IF OBJECT_ID('dbo.tbl_Rule_ShadbalaMaximumRupas', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Rule_ShadbalaMaximumRupas (
        Id                   INT IDENTITY(1,1) CONSTRAINT PK_Rule_ShadbalaMaximumRupas PRIMARY KEY,
        RuleSetId            TINYINT NOT NULL CONSTRAINT FK_Rule_ShadbalaMaximumRupas_RuleSet REFERENCES dbo.tbl_Rule_Sets (Id),
        StrengthProfileCode  VARCHAR(40) NOT NULL,
        PlanetId             TINYINT NOT NULL CONSTRAINT FK_Rule_ShadbalaMaximumRupas_Planet REFERENCES dbo.tbl_Planets (Id),
        MaximumRupas         DECIMAL(6,3) NOT NULL,
        SourceRefCode        VARCHAR(40) NULL,
        FormulaSourceRefCode VARCHAR(40) NULL,
        CalculationNarrative NVARCHAR(MAX) NULL,
        IsActive             BIT NOT NULL CONSTRAINT DF_Rule_ShadbalaMaximumRupas_IsActive DEFAULT (1),
        CONSTRAINT CK_Rule_ShadbalaMaximumRupas_Src        CHECK (SourceRefCode IS NULL OR SourceRefCode LIKE 'SRC[_]%'),
        CONSTRAINT CK_Rule_ShadbalaMaximumRupas_FormulaSrc CHECK (FormulaSourceRefCode IS NULL OR FormulaSourceRefCode LIKE 'SRC[_]%'),
        CONSTRAINT CK_Rule_ShadbalaMaximumRupas_Positive   CHECK (MaximumRupas > 0),
        CONSTRAINT UQ_Rule_ShadbalaMaximumRupas UNIQUE (RuleSetId, StrengthProfileCode, PlanetId)
    );
END
GO

;WITH seed (PlanetName, MaximumRupas, Narrative) AS (
    SELECT * FROM (VALUES
        ('Sun',     13.000, 'Engine-reachable max Shadbala for the Sun: Sthana 480 + Dig 60 + Kala 120 + Cheshta 0 (ShadbalaCalculator hardcodes luminaries to 0) + Naisargika 60.00 (fixed) + Drik 60 (non-benefic-natured target, 4 benefic sources at max) = 780.00 virupas.'),
        ('Moon',    12.607, 'Engine-reachable max Shadbala for the Moon: Sthana 480 + Dig 60 + Kala 120 (Paksha maxes at full/new moon) + Cheshta 0 (hardcoded) + Naisargika 51.43 (fixed) + Drik 45 (benefic-natured target, 3 remaining benefic sources at max) = 756.43 virupas.'),
        ('Mars',    13.286, 'Engine-reachable max Shadbala for Mars: Sthana 480 + Dig 60 + Kala 120 + Cheshta 60 (retrograde) + Naisargika 17.14 (fixed) + Drik 60 (non-benefic-natured target) = 797.14 virupas.'),
        ('Mercury', 13.179, 'Engine-reachable max Shadbala for Mercury: Sthana 480 + Dig 60 + Kala 120 + Cheshta 60 (retrograde) + Naisargika 25.71 (fixed) + Drik 45 (benefic-natured target) = 790.71 virupas.'),
        ('Jupiter', 13.322, 'Engine-reachable max Shadbala for Jupiter: Sthana 480 + Dig 60 + Kala 120 + Cheshta 60 (retrograde) + Naisargika 34.29 (fixed) + Drik 45 (benefic-natured target) = 799.29 virupas.'),
        ('Venus',   13.464, 'Engine-reachable max Shadbala for Venus: Sthana 480 + Dig 60 + Kala 120 + Cheshta 60 (retrograde) + Naisargika 42.86 (fixed) + Drik 45 (benefic-natured target) = 807.86 virupas.'),
        ('Saturn',  13.143, 'Engine-reachable max Shadbala for Saturn: Sthana 480 + Dig 60 + Kala 120 + Cheshta 60 (retrograde) + Naisargika 8.57 (fixed) + Drik 60 (non-benefic-natured target) = 788.57 virupas.')
    ) s (PlanetName, MaximumRupas, Narrative)
)
INSERT dbo.tbl_Rule_ShadbalaMaximumRupas
    (RuleSetId, StrengthProfileCode, PlanetId, MaximumRupas, SourceRefCode, FormulaSourceRefCode, CalculationNarrative)
SELECT 1, 'PVR_INTEGRATED_STRENGTH', p.Id, s.MaximumRupas,
       'SRC_IKIASTRRO_SYNTHESIS', 'SRC_RAMAN_GRAHA_BHAVA_BALAS', s.Narrative
FROM seed s
JOIN dbo.tbl_Planets p ON p.PlanetName = s.PlanetName
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.tbl_Rule_ShadbalaMaximumRupas r
    WHERE r.RuleSetId = 1 AND r.StrengthProfileCode = 'PVR_INTEGRATED_STRENGTH' AND r.PlanetId = p.Id
);
GO

-- --- vw_ChartShadbala: Minimum/Maximum now computed live via JOIN, not
--     from the (unreliably populated) stored MinimumRequiredRupas column.
--     Column names/positions for the pre-existing Minimum/PercentOfMinimum
--     pair are unchanged — AstrologerEvidenceRepository and
--     PlanetaryStrengthRepository read them by name, and
--     db/checks/073_shadbala_benchmark.sql does too. ---
CREATE OR ALTER VIEW dbo.vw_ChartShadbala
AS
SELECT c.BirthDetailId, s.ChartResultId, c.RuleSetId, p.PlanetName AS Planet,
       s.SthanaBalaVirupas, s.DigBalaVirupas, s.KalaBalaVirupas,
       s.CheshtaBalaVirupas, s.NaisargikaBalaVirupas, s.DrikBalaVirupas,
       s.YuddhaBalaVirupas, s.ShadbalaVirupas, s.ShadbalaRupas,
       minR.MinimumRupas AS MinimumRequiredRupas,
       CASE WHEN minR.MinimumRupas IS NULL OR minR.MinimumRupas = 0 THEN NULL
            ELSE CONVERT(DECIMAL(9,2), s.ShadbalaRupas * 100.0 / minR.MinimumRupas) END AS PercentOfMinimum,
       maxR.MaximumRupas AS MaximumPossibleRupas,
       CASE WHEN maxR.MaximumRupas IS NULL OR maxR.MaximumRupas = 0 THEN NULL
            ELSE CONVERT(DECIMAL(9,2), s.ShadbalaRupas * 100.0 / maxR.MaximumRupas) END AS PercentOfMaximum,
       s.IshtaBala, s.KashtaBala,
       s.UchchaRashmi, s.CheshtaRashmi, s.SubhaRashmi, s.AsubhaRashmi,
       s.IshtaPhalaParasara, s.KashtaPhalaParasara,
       s.StrengthProfileCode, s.FormulaSourceRefCode, s.CalculationNarrative, s.ComputedAtUtc
FROM dbo.tbl_Fact_PlanetaryStrength s
JOIN dbo.tbl_ChartResults c ON c.Id = s.ChartResultId
JOIN dbo.tbl_Planets p ON p.Id = s.PlanetId
LEFT JOIN dbo.tbl_Rule_ShadbalaMinimumRupas minR
       ON minR.RuleSetId = s.RuleSetId AND minR.StrengthProfileCode = s.StrengthProfileCode
      AND minR.PlanetId = s.PlanetId AND minR.IsActive = 1
LEFT JOIN dbo.tbl_Rule_ShadbalaMaximumRupas maxR
       ON maxR.RuleSetId = s.RuleSetId AND maxR.StrengthProfileCode = s.StrengthProfileCode
      AND maxR.PlanetId = s.PlanetId AND maxR.IsActive = 1;
GO

INSERT dbo.SchemaMigrations (ScriptName, Note)
SELECT '100_fix_shadbala_minimum_add_maximum_rupas.sql',
       'Fixed Sun minimum Shadbala 5.000->6.500 rupas; added tbl_Rule_ShadbalaMaximumRupas (engine-reachable ceiling, 7 rows); vw_ChartShadbala now computes Min/Max/%Min/%Max via live JOIN, not stored col.'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '100_fix_shadbala_minimum_add_maximum_rupas.sql');
GO

PRINT '100 applied: Sun minimum Shadbala corrected, Maximum Shadbala reference added, vw_ChartShadbala self-sufficient.';
GO
