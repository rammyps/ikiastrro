-- 157 — Seed Parāśara's four Vimśopaka Bala weighting schemes.
-- Source: SRC_BPHS, Varga Viveka chapter, verses 17-26 (Santhanam translation).
-- Each scheme totals exactly 20 points. Dignity multipliers are implemented in Core;
-- this table owns only the chart weights.

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

DELETE FROM dbo.tbl_Rule_VimsopakaWeight
WHERE RuleSetId = 1 AND SchemeCode IN ('SHADVARGA','SAPTAVARGA','DASAVARGA','SHODASAVARGA');

INSERT dbo.tbl_Rule_VimsopakaWeight
    (RuleSetId, SchemeCode, VargaChartType, Weight, MaxTotal, MethodCode,
     RuleParametersJson, CalculationNarrative, SourceRefCode, IsActive)
SELECT 1, v.SchemeCode, v.ChartType, v.Weight, 20.00, 'BPHS_VARGA_VISWA',
       N'{"dignityScale":{"own":20,"greatFriend":18,"friend":15,"neutral":10,"enemy":7,"greatEnemy":5}}',
       N'Weighted varga dignity: chart weight × BPHS dignity value ÷ 20; scheme maximum = 20.',
       'SRC_BPHS', 1
FROM (VALUES
    ('SHADVARGA','D1',6.00),('SHADVARGA','D2',2.00),('SHADVARGA','D3',4.00),
    ('SHADVARGA','D9',5.00),('SHADVARGA','D12',2.00),('SHADVARGA','D30',1.00),

    ('SAPTAVARGA','D1',5.00),('SAPTAVARGA','D2',2.00),('SAPTAVARGA','D3',3.00),
    ('SAPTAVARGA','D7',2.50),('SAPTAVARGA','D9',4.50),('SAPTAVARGA','D12',2.00),
    ('SAPTAVARGA','D30',1.00),

    ('DASAVARGA','D1',3.00),('DASAVARGA','D2',1.50),('DASAVARGA','D3',1.50),
    ('DASAVARGA','D7',1.50),('DASAVARGA','D9',1.50),('DASAVARGA','D10',1.50),
    ('DASAVARGA','D12',1.50),('DASAVARGA','D16',1.50),('DASAVARGA','D30',1.50),
    ('DASAVARGA','D60',5.00),

    ('SHODASAVARGA','D1',3.50),('SHODASAVARGA','D2',1.00),('SHODASAVARGA','D3',1.00),
    ('SHODASAVARGA','D4',0.50),('SHODASAVARGA','D7',0.50),('SHODASAVARGA','D9',3.00),
    ('SHODASAVARGA','D10',0.50),('SHODASAVARGA','D12',0.50),('SHODASAVARGA','D16',2.00),
    ('SHODASAVARGA','D20',0.50),('SHODASAVARGA','D24',0.50),('SHODASAVARGA','D27',0.50),
    ('SHODASAVARGA','D30',1.00),('SHODASAVARGA','D40',0.50),('SHODASAVARGA','D45',0.50),
    ('SHODASAVARGA','D60',4.00)
) v(SchemeCode, ChartType, Weight);

IF EXISTS (
    SELECT 1 FROM dbo.tbl_Rule_VimsopakaWeight
    WHERE RuleSetId = 1 AND IsActive = 1
    GROUP BY SchemeCode
    HAVING SUM(Weight) <> 20.00
)
    THROW 51000, 'Vimsopaka scheme weights must total 20.', 1;

IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '157_seed_vimsopaka_weights.sql')
    INSERT dbo.SchemaMigrations (ScriptName, Note)
    VALUES ('157_seed_vimsopaka_weights.sql',
            'Seed BPHS Vimshopaka weights: Shadvarga, Saptavarga, Dasavarga, Shodasavarga');

COMMIT TRANSACTION;

PRINT '157 applied: 39 Vimshopaka weight rows seeded; every scheme totals 20.';
