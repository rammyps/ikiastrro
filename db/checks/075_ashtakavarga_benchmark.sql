-- Read-only inspection of migration 074-075: the Parasari Ashtakavarga
-- rule matrix, the reduction rules, and the JHora benchmark.

-- Per-recipient bindu totals from the seeded matrix.
-- Expect Sun 48, Moon 49, Mars 39, Mercury 54, Jupiter 56, Venus 52, Saturn 39.
SELECT r.RecipientCode, SUM(x.PlaceCount) AS RecipientBinduTotal
FROM dbo.tbl_Rule_AshtakavargaContribution r
CROSS APPLY (SELECT COUNT(*) AS PlaceCount FROM OPENJSON(r.BeneficPlacesJson)) x
WHERE r.RuleSetId = 1 AND r.MethodCode = 'PVR_PARASARA_BAV'
GROUP BY r.RecipientCode
ORDER BY CASE r.RecipientCode
           WHEN 'SUN' THEN 1 WHEN 'MOON' THEN 2 WHEN 'MARS' THEN 3 WHEN 'MERCURY' THEN 4
           WHEN 'JUPITER' THEN 5 WHEN 'VENUS' THEN 6 WHEN 'SATURN' THEN 7 END;

-- Grand total across the whole matrix. Expect 337.
SELECT SUM(x.PlaceCount) AS SarvashtakavargaGrandTotal
FROM dbo.tbl_Rule_AshtakavargaContribution r
CROSS APPLY (SELECT COUNT(*) AS PlaceCount FROM OPENJSON(r.BeneficPlacesJson)) x
WHERE r.RuleSetId = 1 AND r.MethodCode = 'PVR_PARASARA_BAV';

-- Row count check: 7 recipients x 8 contributors = 56.
SELECT COUNT(*) AS ContributionRows FROM dbo.tbl_Rule_AshtakavargaContribution
WHERE RuleSetId = 1 AND MethodCode = 'PVR_PARASARA_BAV';

-- Reduction rules (expect TRIKONA_SODHANA order 1, EKADHIPATYA_SODHANA order 2).
SELECT StepCode, StepOrder, SourceRefCode, LEFT(AlgorithmJson, 80) AS AlgorithmHead
FROM dbo.tbl_Rule_AshtakavargaReduction
WHERE RuleSetId = 1 AND MethodCode = 'PVR_PARASARA_BAV'
ORDER BY StepOrder;

-- JHora benchmark BAV grid (expect 8 recipients x 12 signs = 96 rows).
SELECT cell.RecipientCode, cell.SignNumber, cell.BinduValue
FROM research.tbl_Dim_SourceReferenceAshtakavargaBenchmarkCell cell
JOIN research.tbl_Dim_SourceReferenceAshtakavargaBenchmarkRun run ON run.Id = cell.RunId
JOIN research.tbl_Dim_SourceReferenceAshtakavargaBenchmarkCase c ON c.Id = run.CaseId
WHERE c.CaseCode = 'BENCH_RAMAKRISHNAN_P_JHORA_1981' AND cell.MetricCode = 'BAV'
ORDER BY cell.RecipientCode, cell.SignNumber;

-- Derived SAV from the benchmark BAV grid (7 planets, Ascendant excluded).
-- Expect: Ar 30, Ta 23, Ge 29, Cn 34, Le 23, Vi 34, Li 17, Sc 23, Sg 26, Cp 37, Aq 43, Pi 18; total 337.
SELECT cell.SignNumber, SUM(cell.BinduValue) AS SarvaBindus
FROM research.tbl_Dim_SourceReferenceAshtakavargaBenchmarkCell cell
JOIN research.tbl_Dim_SourceReferenceAshtakavargaBenchmarkRun run ON run.Id = cell.RunId
JOIN research.tbl_Dim_SourceReferenceAshtakavargaBenchmarkCase c ON c.Id = run.CaseId
WHERE c.CaseCode = 'BENCH_RAMAKRISHNAN_P_JHORA_1981' AND cell.MetricCode = 'BAV'
  AND cell.RecipientCode <> 'AS'
GROUP BY cell.SignNumber
ORDER BY cell.SignNumber;
