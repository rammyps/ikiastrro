-- Read-only inspection of migration 071-073: Shadbala minimum-rupa rule,
-- the restored vw_ChartShadbala contract, and the JHora Shadbala benchmark.

-- Minimum-rupa reference (expect 7 rows; matches the classical Parasari table).
SELECT p.PlanetName, r.MinimumRupas, r.SourceRefCode, r.FormulaSourceRefCode
FROM dbo.tbl_Rule_ShadbalaMinimumRupas r
JOIN dbo.tbl_Planets p ON p.Id = r.PlanetId
WHERE r.RuleSetId = 1 AND r.StrengthProfileCode = 'PVR_INTEGRATED_STRENGTH'
ORDER BY p.Id;

-- vw_ChartShadbala must expose BirthDetailId, Planet and PercentOfMinimum
-- (the columns AstrologerEvidenceRepository selects). Empty result is fine on
-- a DB with no computed strength rows; a "column does not exist" error is not.
SELECT TOP 20 BirthDetailId, Planet, ShadbalaRupas, MinimumRequiredRupas,
       PercentOfMinimum, UchchaRashmi, IshtaPhalaParasara
FROM dbo.vw_ChartShadbala
ORDER BY BirthDetailId, Planet;

-- JHora Shadbala benchmark for 1_Ramakrishnan (expect 7 rows).
SELECT b.PlanetCode, b.ShadbalaVirupas, b.ShadbalaRupas, b.PercentStrength,
       b.IshtaPhala, b.KashtaPhala,
       r.MinimumRupas AS SeededMinimumRupas,
       CONVERT(DECIMAL(8,2), b.ShadbalaRupas * 100.0 / r.MinimumRupas) AS DerivedPercent,
       CONVERT(DECIMAL(8,2), b.ShadbalaRupas * 100.0 / r.MinimumRupas) - b.PercentStrength AS PercentDelta
FROM dbo.tbl_Dim_ShadbalaBenchmarkValues b
JOIN dbo.tbl_Dim_AyanamsaBenchmarkCases c ON c.Id = b.AyanamsaBenchmarkCaseId
JOIN dbo.tbl_Planets p ON p.PlanetName = b.PlanetCode
JOIN dbo.tbl_Rule_ShadbalaMinimumRupas r ON r.PlanetId = p.Id AND r.RuleSetId = 1
WHERE c.Code = 'BENCH_RAMAKRISHNAN_P_JHORA_1981'
ORDER BY p.Id;

-- Kala Bala rule completeness: the six deferred sub-components must now carry
-- params (Nathonnata / Paksha are already computed and are not in scope here).
SELECT SubComponentCode, MaxRupas,
       CASE WHEN RuleParametersJson IS NULL THEN 'MISSING' ELSE 'set' END AS ParamsState
FROM dbo.tbl_Rule_ShadbalaComponent
WHERE RuleSetId = 1 AND StrengthProfileCode = 'PVR_INTEGRATED_STRENGTH' AND BalaCode = 'KALA_BALA'
  AND SubComponentCode IN ('TRIBHAGA_BALA','VARSHA_BALA','MASA_BALA','DINA_BALA','HORA_BALA','AYANA_BALA')
ORDER BY SubComponentCode;

-- Planetary-war rule (expect 1 row, orb 1.00).
SELECT StrengthProfileCode, OrbDegrees, ParticipatingPlanetsCsv, WinnerCriterionCode, AdjustmentMethodCode
FROM dbo.tbl_Rule_PlanetaryWar
WHERE RuleSetId = 1;
