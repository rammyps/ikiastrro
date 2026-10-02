-- =====================================================================
-- 164 - tbl_Rule_VargaScheme: D2, D3 and D7 switch to Jagannatha Hora's
-- default schemes, app-wide (rammyps, 2026-10-02).
--
--   D2  ClassicalTwoSign (Cn/Le only)  -> UmaShambu Hora   (JHora "D-2 (US)")
--   D3  Parasara 1/5/9                 -> UmaShambu Drekkana (JHora "D-3 (US)")
--   D7  Parasara even-from-7th forward -> even-from-7th reversed (JHora "D-7 (7-1)")
--
-- Why: Saptavargaja Bala (Shadbala's Sthana Bala) is summed over D1, D2,
-- D3, D7, D9, D12 and D30. With the Rasi-chart relationship and D1-only
-- moolatrikona rules fixed, our Saptavargaja for 1_RamakrishnanP matches
-- JHora's exactly once these three vargas use JHora's schemes. rammyps chose
-- to switch the charts everywhere rather than only inside Shadbala, so the
-- D2/D3/D7 the app shows are the ones its strengths are computed from.
--
-- All three rules were fitted to, and verified against, all 67 bodies of
-- the RamakrishnanP "Rasis occupied in all vargas" export (2026-09-23):
-- HoraD2UmaShambuSignRule (existing), DrekkanaD3UmaShambuSignRule and
-- SaptamsaD7EvenReverseSignRule (new). RuleParametersJson is sampled from
-- those classes; CLI verify-rules proves the round-trip.
--
-- The D2-US chart type (scheme Id 2) now computes the same chart as D2; it
-- is left in place.
--
-- Stored charts keep their old signs until regenerated:
--   dotnet run --project src/Ikiastrro.Cli -- rebuild-all
--
-- Apply:  sqlcmd -S "localhost\SQLSERVER2025" -E -d ikiastrro -b -i db/164_switch_d2_d3_d7_to_jhora_defaults.sql
-- =====================================================================
USE [ikiastrro];
GO

UPDATE dbo.tbl_Rule_VargaScheme SET
    MethodCode = 'UmaShambu',
    MethodSource = N'Parasara Uma Shambu (JHora default D-2 (US)); PyJHora hora_chart method 1 = __parivritti_even_reverse(dvf=2)',
    SignRuleKind = 'Special',
    SignRuleKey = 'HoraD2UmaShambu',
    RuleParametersJson = N'{"method":"GRID_VARGA","parts":2,"map":[[0,3,4,7,8,11,0,3,4,7,8,11],[1,2,5,6,9,10,1,2,5,6,9,10]]}',
    CalculationNarrative = N'GRID_VARGA parts=2: each rasi sign splits into 2 equal 15 deg parts; map[part][rasiSign] is the 0-based varga sign. Sampled from HoraD2UmaShambuSignRule (SignRuleKey=HoraD2UmaShambu).'
WHERE Id = 1 AND RuleSetId = 1;

UPDATE dbo.tbl_Rule_VargaScheme SET
    MethodCode = 'UmaShambu',
    MethodSource = N'Re-interpreted Parasara Drekkana (Uma-Shambhu), JHora default D-3 (US): first drekkana r + 4*ceil(r/2), odd signs forward, even backward',
    SignRuleKind = 'Special',
    SignRuleKey = 'DrekkanaD3UmaShambu',
    RuleParametersJson = N'{"method":"GRID_VARGA","parts":3,"map":[[0,5,6,11,0,5,6,11,0,5,6,11],[1,4,7,10,1,4,7,10,1,4,7,10],[2,3,8,9,2,3,8,9,2,3,8,9]]}',
    CalculationNarrative = N'GRID_VARGA parts=3: each rasi sign splits into 3 equal 10 deg parts; map[part][rasiSign] is the 0-based varga sign. Sampled from DrekkanaD3UmaShambuSignRule (SignRuleKey=DrekkanaD3UmaShambu).'
WHERE Id = 3 AND RuleSetId = 1;

UPDATE dbo.tbl_Rule_VargaScheme SET
    MethodCode = 'EvenSeventhReverse',
    MethodSource = N'Saptamsa, JHora default D-7 (7-1): odd signs from the sign forward, even signs from the 7th backward',
    SignRuleKind = 'Special',
    SignRuleKey = 'SaptamsaD7EvenReverse',
    RuleParametersJson = N'{"method":"GRID_VARGA","parts":7,"map":[[0,7,2,9,4,11,6,1,8,3,10,5],[1,6,3,8,5,10,7,0,9,2,11,4],[2,5,4,7,6,9,8,11,10,1,0,3],[3,4,5,6,7,8,9,10,11,0,1,2],[4,3,6,5,8,7,10,9,0,11,2,1],[5,2,7,4,9,6,11,8,1,10,3,0],[6,1,8,3,10,5,0,7,2,9,4,11]]}',
    CalculationNarrative = N'GRID_VARGA parts=7: each rasi sign splits into 7 equal 4.2857 deg parts; map[part][rasiSign] is the 0-based varga sign. Sampled from SaptamsaD7EvenReverseSignRule (SignRuleKey=SaptamsaD7EvenReverse).'
WHERE Id = 7 AND RuleSetId = 1;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '164_switch_d2_d3_d7_to_jhora_defaults.sql')
    INSERT dbo.SchemaMigrations (ScriptName, AppliedAtUtc, Note)
    VALUES ('164_switch_d2_d3_d7_to_jhora_defaults.sql', SYSUTCDATETIME(),
            'D2/D3/D7 to JHora defaults: Uma Shambu hora, Uma Shambu drekkana, saptamsa even-reverse (7-1).');
GO

DECLARE @rows INT = (SELECT COUNT(*) FROM dbo.tbl_Rule_VargaScheme
                     WHERE RuleSetId = 1 AND SignRuleKey IN ('HoraD2UmaShambu', 'DrekkanaD3UmaShambu', 'SaptamsaD7EvenReverse'));
PRINT '164 applied: ' + CAST(@rows AS VARCHAR(10)) + ' scheme rows on the JHora rules (expect 4: D2, D2-US, D3, D7).';
GO
