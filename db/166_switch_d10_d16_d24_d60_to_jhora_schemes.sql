-- =====================================================================
-- 166 - tbl_Rule_VargaScheme: D10, D16, D24 and D60 switch to the schemes
-- Jagannatha Hora uses for 1_RamakrishnanP, app-wide (rammyps, 2026-10-02).
--
--   D10  Parasara even-from-9th forward -> even from the 9th backward (JHora "D-10 (5-8)")
--   D16  Parasara Ar/Le/Sg forward      -> even signs reversed        (JHora "D-16 (Rev)")
--   D24  odd Le / even Cn forward       -> even signs from Cn backward (JHora "D-24 (Rev)")
--   D60  from the sign forward          -> odd from Aries, even from Pisces backward (JHora "D-60 (RvAr)")
--
-- Why: after migration 164 these were the only four of the 20 charts whose
-- placements differed from JHora's "Rasis occupied in all vargas" export,
-- always for bodies in even signs; it also left the Dasavarga / Shodasavarga
-- Vaiseshikamsa counts off JHora's. Each new rule reproduces all 68 bodies of
-- that export (decision 007). RuleParametersJson is sampled from the new
-- classes; CLI verify-rules proves the round-trip.
--
-- Stored charts keep their old signs until regenerated:
--   dotnet run --project src/Ikiastrro.Cli -- rebuild-all
--
-- Apply:  sqlcmd -S "localhost\SQLSERVER2025" -E -d ikiastrro -b -i db/166_switch_d10_d16_d24_d60_to_jhora_schemes.sql
-- =====================================================================
USE [ikiastrro];
GO

UPDATE dbo.tbl_Rule_VargaScheme SET
    MethodCode = 'EvenFifthReverse',
    MethodSource = N'Dasamsa, JHora D-10 (5-8): odd signs from the sign forward, even signs from the 9th counted backward (= 5th) and backward; PyJHora dasamsa_chart method 3',
    SignRuleKind = 'Special',
    SignRuleKey = 'DasamsaD10EvenReverse',
    RuleParametersJson = N'{"method":"GRID_VARGA","parts":10,"map":[[0,5,2,7,4,9,6,11,8,1,10,3],[1,4,3,6,5,8,7,10,9,0,11,2],[2,3,4,5,6,7,8,9,10,11,0,1],[3,2,5,4,7,6,9,8,11,10,1,0],[4,1,6,3,8,5,10,7,0,9,2,11],[5,0,7,2,9,4,11,6,1,8,3,10],[6,11,8,1,10,3,0,5,2,7,4,9],[7,10,9,0,11,2,1,4,3,6,5,8],[8,9,10,11,0,1,2,3,4,5,6,7],[9,8,11,10,1,0,3,2,5,4,7,6]]}',
    CalculationNarrative = N'GRID_VARGA parts=10: each rasi sign splits into 10 equal 3 deg parts; map[part][rasiSign] is the 0-based varga sign. Sampled from DasamsaD10EvenReverseSignRule (SignRuleKey=DasamsaD10EvenReverse).'
WHERE Id = 10 AND RuleSetId = 1;  -- D10

UPDATE dbo.tbl_Rule_VargaScheme SET
    MethodCode = 'EvenReverse',
    MethodSource = N'Shodasamsa, JHora D-16 (Rev): base Ar/Le/Sg by modality, odd signs forward from the base, even signs the same 16 parts reversed (base + 15 - l)',
    SignRuleKind = 'Special',
    SignRuleKey = 'ShodasamsaD16EvenReverse',
    RuleParametersJson = N'{"method":"GRID_VARGA","parts":16,"map":[[0,7,8,3,4,11,0,7,8,3,4,11],[1,6,9,2,5,10,1,6,9,2,5,10],[2,5,10,1,6,9,2,5,10,1,6,9],[3,4,11,0,7,8,3,4,11,0,7,8],[4,3,0,11,8,7,4,3,0,11,8,7],[5,2,1,10,9,6,5,2,1,10,9,6],[6,1,2,9,10,5,6,1,2,9,10,5],[7,0,3,8,11,4,7,0,3,8,11,4],[8,11,4,7,0,3,8,11,4,7,0,3],[9,10,5,6,1,2,9,10,5,6,1,2],[10,9,6,5,2,1,10,9,6,5,2,1],[11,8,7,4,3,0,11,8,7,4,3,0],[0,7,8,3,4,11,0,7,8,3,4,11],[1,6,9,2,5,10,1,6,9,2,5,10],[2,5,10,1,6,9,2,5,10,1,6,9],[3,4,11,0,7,8,3,4,11,0,7,8]]}',
    CalculationNarrative = N'GRID_VARGA parts=16: each rasi sign splits into 16 equal 1.875 deg parts; map[part][rasiSign] is the 0-based varga sign. Sampled from ShodasamsaD16EvenReverseSignRule (SignRuleKey=ShodasamsaD16EvenReverse).'
WHERE Id = 13 AND RuleSetId = 1;  -- D16

UPDATE dbo.tbl_Rule_VargaScheme SET
    MethodCode = 'EvenReverse',
    MethodSource = N'Siddhamsa, JHora D-24 (Rev): odd signs from Leo forward, even signs from Cancer backward; PyJHora chaturvimsamsa_chart method 2',
    SignRuleKind = 'Special',
    SignRuleKey = 'SiddhamsaD24EvenReverse',
    RuleParametersJson = N'{"method":"GRID_VARGA","parts":24,"map":[[4,3,4,3,4,3,4,3,4,3,4,3],[5,2,5,2,5,2,5,2,5,2,5,2],[6,1,6,1,6,1,6,1,6,1,6,1],[7,0,7,0,7,0,7,0,7,0,7,0],[8,11,8,11,8,11,8,11,8,11,8,11],[9,10,9,10,9,10,9,10,9,10,9,10],[10,9,10,9,10,9,10,9,10,9,10,9],[11,8,11,8,11,8,11,8,11,8,11,8],[0,7,0,7,0,7,0,7,0,7,0,7],[1,6,1,6,1,6,1,6,1,6,1,6],[2,5,2,5,2,5,2,5,2,5,2,5],[3,4,3,4,3,4,3,4,3,4,3,4],[4,3,4,3,4,3,4,3,4,3,4,3],[5,2,5,2,5,2,5,2,5,2,5,2],[6,1,6,1,6,1,6,1,6,1,6,1],[7,0,7,0,7,0,7,0,7,0,7,0],[8,11,8,11,8,11,8,11,8,11,8,11],[9,10,9,10,9,10,9,10,9,10,9,10],[10,9,10,9,10,9,10,9,10,9,10,9],[11,8,11,8,11,8,11,8,11,8,11,8],[0,7,0,7,0,7,0,7,0,7,0,7],[1,6,1,6,1,6,1,6,1,6,1,6],[2,5,2,5,2,5,2,5,2,5,2,5],[3,4,3,4,3,4,3,4,3,4,3,4]]}',
    CalculationNarrative = N'GRID_VARGA parts=24: each rasi sign splits into 24 equal 1.25 deg parts; map[part][rasiSign] is the 0-based varga sign. Sampled from SiddhamsaD24EvenReverseSignRule (SignRuleKey=SiddhamsaD24EvenReverse).'
WHERE Id = 15 AND RuleSetId = 1;  -- D24

UPDATE dbo.tbl_Rule_VargaScheme SET
    MethodCode = 'EvenReverseFromAries',
    MethodSource = N'Shashtyamsa, JHora D-60 (RvAr): odd signs from Aries forward, even signs from Pisces backward; PyJHora shashtyamsa_chart method 3 (parivritti alternate)',
    SignRuleKind = 'Special',
    SignRuleKey = 'ShashtyamsaD60EvenReverseFromAries',
    RuleParametersJson = N'{"method":"GRID_VARGA","parts":60,"map":[[0,11,0,11,0,11,0,11,0,11,0,11],[1,10,1,10,1,10,1,10,1,10,1,10],[2,9,2,9,2,9,2,9,2,9,2,9],[3,8,3,8,3,8,3,8,3,8,3,8],[4,7,4,7,4,7,4,7,4,7,4,7],[5,6,5,6,5,6,5,6,5,6,5,6],[6,5,6,5,6,5,6,5,6,5,6,5],[7,4,7,4,7,4,7,4,7,4,7,4],[8,3,8,3,8,3,8,3,8,3,8,3],[9,2,9,2,9,2,9,2,9,2,9,2],[10,1,10,1,10,1,10,1,10,1,10,1],[11,0,11,0,11,0,11,0,11,0,11,0],[0,11,0,11,0,11,0,11,0,11,0,11],[1,10,1,10,1,10,1,10,1,10,1,10],[2,9,2,9,2,9,2,9,2,9,2,9],[3,8,3,8,3,8,3,8,3,8,3,8],[4,7,4,7,4,7,4,7,4,7,4,7],[5,6,5,6,5,6,5,6,5,6,5,6],[6,5,6,5,6,5,6,5,6,5,6,5],[7,4,7,4,7,4,7,4,7,4,7,4],[8,3,8,3,8,3,8,3,8,3,8,3],[9,2,9,2,9,2,9,2,9,2,9,2],[10,1,10,1,10,1,10,1,10,1,10,1],[11,0,11,0,11,0,11,0,11,0,11,0],[0,11,0,11,0,11,0,11,0,11,0,11],[1,10,1,10,1,10,1,10,1,10,1,10],[2,9,2,9,2,9,2,9,2,9,2,9],[3,8,3,8,3,8,3,8,3,8,3,8],[4,7,4,7,4,7,4,7,4,7,4,7],[5,6,5,6,5,6,5,6,5,6,5,6],[6,5,6,5,6,5,6,5,6,5,6,5],[7,4,7,4,7,4,7,4,7,4,7,4],[8,3,8,3,8,3,8,3,8,3,8,3],[9,2,9,2,9,2,9,2,9,2,9,2],[10,1,10,1,10,1,10,1,10,1,10,1],[11,0,11,0,11,0,11,0,11,0,11,0],[0,11,0,11,0,11,0,11,0,11,0,11],[1,10,1,10,1,10,1,10,1,10,1,10],[2,9,2,9,2,9,2,9,2,9,2,9],[3,8,3,8,3,8,3,8,3,8,3,8],[4,7,4,7,4,7,4,7,4,7,4,7],[5,6,5,6,5,6,5,6,5,6,5,6],[6,5,6,5,6,5,6,5,6,5,6,5],[7,4,7,4,7,4,7,4,7,4,7,4],[8,3,8,3,8,3,8,3,8,3,8,3],[9,2,9,2,9,2,9,2,9,2,9,2],[10,1,10,1,10,1,10,1,10,1,10,1],[11,0,11,0,11,0,11,0,11,0,11,0],[0,11,0,11,0,11,0,11,0,11,0,11],[1,10,1,10,1,10,1,10,1,10,1,10],[2,9,2,9,2,9,2,9,2,9,2,9],[3,8,3,8,3,8,3,8,3,8,3,8],[4,7,4,7,4,7,4,7,4,7,4,7],[5,6,5,6,5,6,5,6,5,6,5,6],[6,5,6,5,6,5,6,5,6,5,6,5],[7,4,7,4,7,4,7,4,7,4,7,4],[8,3,8,3,8,3,8,3,8,3,8,3],[9,2,9,2,9,2,9,2,9,2,9,2],[10,1,10,1,10,1,10,1,10,1,10,1],[11,0,11,0,11,0,11,0,11,0,11,0]]}',
    CalculationNarrative = N'GRID_VARGA parts=60: each rasi sign splits into 60 equal 0.5 deg parts; map[part][rasiSign] is the 0-based varga sign. Sampled from ShashtyamsaD60EvenReverseFromAriesSignRule (SignRuleKey=ShashtyamsaD60EvenReverseFromAries).'
WHERE Id = 20 AND RuleSetId = 1;  -- D60
GO

IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '166_switch_d10_d16_d24_d60_to_jhora_schemes.sql')
    INSERT dbo.SchemaMigrations (ScriptName, AppliedAtUtc, Note)
    VALUES ('166_switch_d10_d16_d24_d60_to_jhora_schemes.sql', SYSUTCDATETIME(),
            'D10/D16/D24/D60 to JHora schemes: D-10 (5-8), D-16 (Rev), D-24 (Rev), D-60 (RvAr).');
GO

DECLARE @rows INT = (SELECT COUNT(*) FROM dbo.tbl_Rule_VargaScheme
                     WHERE RuleSetId = 1 AND SignRuleKey IN ('DasamsaD10EvenReverse', 'ShodasamsaD16EvenReverse',
                                                             'SiddhamsaD24EvenReverse', 'ShashtyamsaD60EvenReverseFromAries'));
PRINT '166 applied: ' + CAST(@rows AS VARCHAR(10)) + ' scheme rows on the JHora rules (expect 4: D10, D16, D24, D60).';
GO
