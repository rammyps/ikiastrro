-- =====================================================================
-- 075 — Seed the Parasari Ashtakavarga rule matrix + reduction rules
--       (dbo), and the Jagannatha Hora benchmark for 1_Ramakrishnan
--       (research.* tables from migration 68).
--
-- The benefic-places matrix is the classical BPHS table as vendored in the
-- MIT jyotishganit library (_research/jyotishganit/.../ashtakavarga.py).
-- Row totals: Sun 48, Moon 49, Mars 39, Mercury 54, Jupiter 56, Venus 52,
-- Saturn 39 -> Sarvashtakavarga grand total 337. Hand-verified against the
-- JHora export: the computed Saturn BAV row reproduces JHora's
-- "Sa 3 1 2 4 2 5 1 3 1 7 7 3" exactly.
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- 1. Source row for the classical Ashtakavarga table.
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Dim_Source WHERE Code = 'SRC_BPHS_ASHTAKAVARGA')
    INSERT dbo.tbl_Dim_Source (Code, Title, Author, Edition, Tradition, Notes)
    VALUES ('SRC_BPHS_ASHTAKAVARGA', N'Brihat Parashara Hora Shastra — Ashtakavarga Adhyaya',
            N'Parashara', NULL, 'Parashara',
            N'Classical benefic-places (bindu) matrix; public domain. Cross-checked against the vendored MIT jyotishganit implementation.');
GO

-- 2. Benefic-places matrix: 7 recipients x 8 contributors (Sun..Saturn + Lagna).
;WITH m (RecipientCode, ContributorCode, BeneficPlacesJson) AS (
    SELECT * FROM (VALUES
        -- Sun (48)
        ('SUN','SUN',       '[1,2,4,7,8,9,10,11]'),
        ('SUN','MOON',      '[3,6,10,11]'),
        ('SUN','MARS',      '[1,2,4,7,8,9,10,11]'),
        ('SUN','MERCURY',   '[3,5,6,9,10,11,12]'),
        ('SUN','JUPITER',   '[5,6,9,11]'),
        ('SUN','VENUS',     '[6,7,12]'),
        ('SUN','SATURN',    '[1,2,4,7,8,9,10,11]'),
        ('SUN','LAGNA',     '[3,4,6,10,11,12]'),
        -- Moon (49)
        ('MOON','SUN',      '[3,6,7,8,10,11]'),
        ('MOON','MOON',     '[1,3,6,7,10,11]'),
        ('MOON','MARS',     '[2,3,5,6,9,10,11]'),
        ('MOON','MERCURY',  '[1,3,4,5,7,8,10,11]'),
        ('MOON','JUPITER',  '[1,4,7,8,10,11,12]'),
        ('MOON','VENUS',    '[3,4,5,7,9,10,11]'),
        ('MOON','SATURN',   '[3,5,6,11]'),
        ('MOON','LAGNA',    '[3,6,10,11]'),
        -- Mars (39)
        ('MARS','SUN',      '[3,5,6,10,11]'),
        ('MARS','MOON',     '[3,6,11]'),
        ('MARS','MARS',     '[1,2,4,7,8,10,11]'),
        ('MARS','MERCURY',  '[3,5,6,11]'),
        ('MARS','JUPITER',  '[6,10,11,12]'),
        ('MARS','VENUS',    '[6,8,11,12]'),
        ('MARS','SATURN',   '[1,4,7,8,9,10,11]'),
        ('MARS','LAGNA',    '[1,3,6,10,11]'),
        -- Mercury (54)
        ('MERCURY','SUN',      '[5,6,9,11,12]'),
        ('MERCURY','MOON',     '[2,4,6,8,10,11]'),
        ('MERCURY','MARS',     '[1,2,4,7,8,9,10,11]'),
        ('MERCURY','MERCURY',  '[1,3,5,6,9,10,11,12]'),
        ('MERCURY','JUPITER',  '[6,8,11,12]'),
        ('MERCURY','VENUS',    '[1,2,3,4,5,8,9,11]'),
        ('MERCURY','SATURN',   '[1,2,4,7,8,9,10,11]'),
        ('MERCURY','LAGNA',    '[1,2,4,6,8,10,11]'),
        -- Jupiter (56)
        ('JUPITER','SUN',      '[1,2,3,4,7,8,9,10,11]'),
        ('JUPITER','MOON',     '[2,5,7,9,11]'),
        ('JUPITER','MARS',     '[1,2,4,7,8,10,11]'),
        ('JUPITER','MERCURY',  '[1,2,4,5,6,9,10,11]'),
        ('JUPITER','JUPITER',  '[1,2,3,4,7,8,10,11]'),
        ('JUPITER','VENUS',    '[2,5,6,9,10,11]'),
        ('JUPITER','SATURN',   '[3,5,6,12]'),
        ('JUPITER','LAGNA',    '[1,2,4,5,6,7,9,10,11]'),
        -- Venus (52)
        ('VENUS','SUN',      '[8,11,12]'),
        ('VENUS','MOON',     '[1,2,3,4,5,8,9,11,12]'),
        ('VENUS','MARS',     '[3,5,6,9,11,12]'),
        ('VENUS','MERCURY',  '[3,5,6,9,11]'),
        ('VENUS','JUPITER',  '[5,8,9,10,11]'),
        ('VENUS','VENUS',    '[1,2,3,4,5,8,9,10,11]'),
        ('VENUS','SATURN',   '[3,4,5,8,9,10,11]'),
        ('VENUS','LAGNA',    '[1,2,3,4,5,8,9,11]'),
        -- Saturn (39)
        ('SATURN','SUN',      '[1,2,4,7,8,10,11]'),
        ('SATURN','MOON',     '[3,6,11]'),
        ('SATURN','MARS',     '[3,5,6,10,11,12]'),
        ('SATURN','MERCURY',  '[6,8,9,10,11,12]'),
        ('SATURN','JUPITER',  '[5,6,11,12]'),
        ('SATURN','VENUS',    '[6,11,12]'),
        ('SATURN','SATURN',   '[3,5,6,11]'),
        ('SATURN','LAGNA',    '[1,3,4,6,10,11]')
    ) v (RecipientCode, ContributorCode, BeneficPlacesJson)
)
INSERT dbo.tbl_Rule_AshtakavargaContribution
    (RuleSetId, MethodCode, RecipientCode, ContributorCode, BeneficPlacesJson,
     SourceRefCode, EvidenceLevelCode, SourceLocator)
SELECT 1, 'PVR_PARASARA_BAV', m.RecipientCode, m.ContributorCode, m.BeneficPlacesJson,
       'SRC_BPHS_ASHTAKAVARGA', 'DirectClassical',
       N'BPHS Ashtakavarga Adhyaya benefic-places table; jyotishganit BENEFIC_HOUSES.'
FROM m
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.tbl_Rule_AshtakavargaContribution r
    WHERE r.RuleSetId = 1 AND r.MethodCode = 'PVR_PARASARA_BAV'
      AND r.RecipientCode = m.RecipientCode AND r.ContributorCode = m.ContributorCode
);
GO

-- 3. Reduction (Sodhana) rules for the Sodhya Pinda pipeline.
;WITH steps (StepCode, StepOrder, AlgorithmJson) AS (
    SELECT * FROM (VALUES
        ('TRIKONA_SODHANA', 1,
         N'{"appliesTo":"each Bhinnashtakavarga","trines":[[1,5,9],[2,6,10],[3,7,11],[4,8,12]],"rule":"Within each trine of signs: if the lowest of the three bindu values is 0, set all three to 0; otherwise subtract that lowest value from all three.","sourceLocator":"BPHS Ashtakavarga - Trikona Sodhana; verify against Raman, Graha & Bhava Balas"}'),
        ('EKADHIPATYA_SODHANA', 2,
         N'{"appliesTo":"each Bhinnashtakavarga, after Trikona Sodhana","sameLordPairs":[[1,8],[2,7],[3,6],[9,12],[10,11]],"singleLordSigns":[4,5],"rule":"For the two signs of one lord, using post-Trikona values and D1 occupancy: (a) both occupied -> unchanged; (b) both unoccupied -> if values differ both take the smaller value, if equal both become 0; (c) exactly one occupied -> the unoccupied sign takes the smaller of the two values (0 if its own value is the larger).","note":"Ekadhipatya has documented variant rule-sets (see PyJHora 3c note); verify the occupancy sub-cases against the cited edition.","sourceLocator":"BPHS Ashtakavarga - Ekadhipatya Sodhana"}')
    ) v (StepCode, StepOrder, AlgorithmJson)
)
INSERT dbo.tbl_Rule_AshtakavargaReduction
    (RuleSetId, MethodCode, StepCode, StepOrder, AlgorithmJson, SourceRefCode, SourceLocator)
SELECT 1, 'PVR_PARASARA_BAV', s.StepCode, s.StepOrder, s.AlgorithmJson,
       'SRC_BPHS_ASHTAKAVARGA', N'BPHS Ashtakavarga Adhyaya - Sodhana pipeline.'
FROM steps s
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.tbl_Rule_AshtakavargaReduction r
    WHERE r.RuleSetId = 1 AND r.MethodCode = 'PVR_PARASARA_BAV' AND r.StepCode = s.StepCode
);
GO

-- 4. Jagannatha Hora benchmark (research.* tables from migration 68).
IF OBJECT_ID('research.tbl_Dim_SourceReferenceAshtakavargaBenchmarkCase', 'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM research.tbl_Dim_SourceReferenceAshtakavargaBenchmarkCase WHERE CaseCode = 'BENCH_RAMAKRISHNAN_P_JHORA_1981')
        INSERT research.tbl_Dim_SourceReferenceAshtakavargaBenchmarkCase
            (CaseCode, SubjectLabel, BirthDateTimeUtc, Latitude, Longitude, TimeZoneOffsetMinutes,
             AyanamsaCode, ChartConventionCode, Notes)
        VALUES
            ('BENCH_RAMAKRISHNAN_P_JHORA_1981', N'Ramakrishnan P (1_Ramakrishnan)', '1981-04-22T00:00:01',
             13.0833333, 80.2833333, 330, 'AYANAMSA_LAHIRI', 'WHOLE_SIGN',
             N'Ashtakavarga of the Rasi chart from docs/artifacts/reference-charts/Rammy_Jagannatha.txt.');

    DECLARE @AvCaseId INT = (SELECT Id FROM research.tbl_Dim_SourceReferenceAshtakavargaBenchmarkCase WHERE CaseCode = 'BENCH_RAMAKRISHNAN_P_JHORA_1981');

    IF NOT EXISTS (SELECT 1 FROM research.tbl_Dim_SourceReferenceAshtakavargaBenchmarkRun WHERE CaseId = @AvCaseId AND SourceSystemCode = 'JAGANNATHA_HORA')
        INSERT research.tbl_Dim_SourceReferenceAshtakavargaBenchmarkRun
            (CaseId, SourceSystemCode, SourceVersion, MethodVariantCode, SourceRefCode, ExportFileName, CapturedAtUtc, RawArtifactPath, Notes)
        VALUES
            (@AvCaseId, 'JAGANNATHA_HORA', N'JHora 7.x', 'PVR_PARASARA_BAV', 'SRC_JHORA_EXPORT_RAMAKRISHNAN',
             N'Rammy_Jagannatha.txt', SYSUTCDATETIME(), N'docs/artifacts/reference-charts/Rammy_Jagannatha.txt',
             N'{"pinda":{"As":{"sodhya":226,"rasi":111,"graha":115},"Su":{"sodhya":103,"rasi":38,"graha":65},"Mo":{"sodhya":199,"rasi":114,"graha":85},"Ma":{"sodhya":232,"rasi":97,"graha":135},"Me":{"sodhya":186,"rasi":106,"graha":80},"Ju":{"sodhya":125,"rasi":90,"graha":35},"Ve":{"sodhya":171,"rasi":136,"graha":35},"Sa":{"sodhya":158,"rasi":48,"graha":110}}}');

    DECLARE @AvRunId BIGINT = (SELECT Id FROM research.tbl_Dim_SourceReferenceAshtakavargaBenchmarkRun WHERE CaseId = @AvCaseId AND SourceSystemCode = 'JAGANNATHA_HORA');

    -- BAV grid: 8 rows (Ascendant + 7 planets) x 12 signs, sign 1 = Aries.
    ;WITH grid (RecipientCode, SignNumber, BinduValue) AS (
        SELECT * FROM (VALUES
            ('AS',1,4),('AS',2,3),('AS',3,6),('AS',4,5),('AS',5,2),('AS',6,7),('AS',7,2),('AS',8,3),('AS',9,3),('AS',10,6),('AS',11,6),('AS',12,2),
            ('SUN',1,4),('SUN',2,4),('SUN',3,3),('SUN',4,5),('SUN',5,2),('SUN',6,5),('SUN',7,4),('SUN',8,2),('SUN',9,4),('SUN',10,6),('SUN',11,5),('SUN',12,4),
            ('MOON',1,3),('MOON',2,2),('MOON',3,6),('MOON',4,5),('MOON',5,4),('MOON',6,5),('MOON',7,4),('MOON',8,4),('MOON',9,2),('MOON',10,7),('MOON',11,6),('MOON',12,1),
            ('MARS',1,4),('MARS',2,2),('MARS',3,5),('MARS',4,3),('MARS',5,3),('MARS',6,6),('MARS',7,1),('MARS',8,2),('MARS',9,1),('MARS',10,4),('MARS',11,6),('MARS',12,2),
            ('MERCURY',1,7),('MERCURY',2,4),('MERCURY',3,4),('MERCURY',4,5),('MERCURY',5,5),('MERCURY',6,5),('MERCURY',7,2),('MERCURY',8,3),('MERCURY',9,6),('MERCURY',10,3),('MERCURY',11,7),('MERCURY',12,3),
            ('JUPITER',1,5),('JUPITER',2,6),('JUPITER',3,2),('JUPITER',4,6),('JUPITER',5,4),('JUPITER',6,5),('JUPITER',7,4),('JUPITER',8,4),('JUPITER',9,6),('JUPITER',10,6),('JUPITER',11,6),('JUPITER',12,2),
            ('VENUS',1,4),('VENUS',2,4),('VENUS',3,7),('VENUS',4,6),('VENUS',5,3),('VENUS',6,3),('VENUS',7,1),('VENUS',8,5),('VENUS',9,6),('VENUS',10,4),('VENUS',11,6),('VENUS',12,3),
            ('SATURN',1,3),('SATURN',2,1),('SATURN',3,2),('SATURN',4,4),('SATURN',5,2),('SATURN',6,5),('SATURN',7,1),('SATURN',8,3),('SATURN',9,1),('SATURN',10,7),('SATURN',11,7),('SATURN',12,3)
        ) v (RecipientCode, SignNumber, BinduValue)
    )
    INSERT research.tbl_Dim_SourceReferenceAshtakavargaBenchmarkCell
        (RunId, MetricCode, RecipientCode, ContributorCode, SignNumber, HouseNumber, BinduValue, RawValue, SourceLocator)
    SELECT @AvRunId, 'BAV', g.RecipientCode, NULL, g.SignNumber, NULL, g.BinduValue, NULL,
           N'JHora "Ashtakavarga of Rasi Chart" grid.'
    FROM grid g
    WHERE NOT EXISTS (
        SELECT 1 FROM research.tbl_Dim_SourceReferenceAshtakavargaBenchmarkCell c
        WHERE c.RunId = @AvRunId AND c.MetricCode = 'BAV' AND c.RecipientCode = g.RecipientCode
          AND c.ContributorCode IS NULL AND c.SignNumber = g.SignNumber
    );
END
GO

INSERT dbo.SchemaMigrations (ScriptName, Note)
SELECT '075_seed_ashtakavarga_rules_and_benchmark.sql',
       'Seed the Parasari BAV benefic-places matrix (56 rows, SAV total 337) + Trikona/Ekadhipatya reduction rules; seed the JHora Ashtakavarga benchmark (BAV grid + Pinda) for 1_Ramakrishnan.'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '075_seed_ashtakavarga_rules_and_benchmark.sql');
GO

PRINT '075 applied: Ashtakavarga rule matrix + JHora benchmark seeded.';
GO
