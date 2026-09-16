-- =====================================================================
-- 106 — Persist whole-sign HouseNumber on the Ashtakavarga fact tables.
--
-- 074 stored SignNumber (1=Aries..12=Pisces, absolute zodiacal) only. Key
-- Inference's "3.3 ASTAVARGA" tab (AshtakavargaChart.razor) was deriving
-- the H1..H12 label shown next to each sign client-side, by string-matching
-- the chart's AscendantSign against a hardcoded English name array. That
-- match silently fails for any Capricorn-ascendant chart: tbl_Chart_KeyDetails.Sign
-- (and LoadedChart.AscendantSign, read straight off it) stores the C#
-- enum spelling 'Capricornus', not 'Capricorn' — the exact gotcha
-- 105's header already documents for a different join. The failed match
-- silently fell back to Lagna=Aries, so every house number shown for a
-- Capricorn-ascendant person's Ashtakavarga was wrong by a fixed offset.
--
-- Fix: compute HouseNumber once, server-side, off the integer SignId
-- (tbl_Chart_KeyDetails.SignId, populated for the 'Ascendant' row by
-- 05_backfill_chartfact_ids.sql via the ZodiacEnumValue join — immune to
-- the Capricorn/Capricornus spelling split because it never touches the
-- string column) and persist it, instead of re-deriving it from a display
-- string on every page render. Backfills every already-generated D1 chart
-- in this same script — no CLI regenerate pass required.
--
-- Apply:  sqlcmd -S "localhost\SQLSERVER2025" -E -d ikiastrro -b -i db/106_add_ashtakavarga_house_number.sql
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.tbl_Fact_BhinnaAshtakavarga') AND name = 'HouseNumber')
BEGIN
    ALTER TABLE dbo.tbl_Fact_BhinnaAshtakavarga ADD HouseNumber TINYINT NULL
        CONSTRAINT CK_Fact_BhinnaAshtakavarga_House CHECK (HouseNumber BETWEEN 1 AND 12);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.tbl_Fact_SarvaAshtakavarga') AND name = 'HouseNumber')
BEGIN
    ALTER TABLE dbo.tbl_Fact_SarvaAshtakavarga ADD HouseNumber TINYINT NULL
        CONSTRAINT CK_Fact_SarvaAshtakavarga_House CHECK (HouseNumber BETWEEN 1 AND 12);
END
GO

-- --- Backfill every already-generated D1 chart -----------------------------
-- Whole-sign house counting: HouseNumber = ((SignNumber - LagnaSignId + 12) % 12) + 1.

-- SignNumber/SignId are both TINYINT (unsigned in SQL Server) — the subtraction must be
-- widened to INT before it goes negative, or SQL Server overflows narrowing the
-- intermediate result back to TINYINT instead of the final CAST below.
UPDATE f
    SET f.HouseNumber = CAST(((CAST(f.SignNumber AS INT) - CAST(lagna.SignId AS INT) + 12) % 12) + 1 AS TINYINT)
    FROM dbo.tbl_Fact_BhinnaAshtakavarga f
    JOIN dbo.tbl_Chart_KeyDetails lagna
      ON lagna.ChartResultId = f.ChartResultId AND lagna.Planet = 'Ascendant'
    WHERE f.HouseNumber IS NULL AND lagna.SignId IS NOT NULL;
GO

UPDATE f
    SET f.HouseNumber = CAST(((CAST(f.SignNumber AS INT) - CAST(lagna.SignId AS INT) + 12) % 12) + 1 AS TINYINT)
    FROM dbo.tbl_Fact_SarvaAshtakavarga f
    JOIN dbo.tbl_Chart_KeyDetails lagna
      ON lagna.ChartResultId = f.ChartResultId AND lagna.Planet = 'Ascendant'
    WHERE f.HouseNumber IS NULL AND lagna.SignId IS NOT NULL;
GO

-- --- UI read view — expose the persisted house number, not a derived one --

CREATE OR ALTER VIEW dbo.vw_ChartAshtakavarga
AS
SELECT c.BirthDetailId, f.ChartResultId, c.ChartType, f.RuleSetId, f.MethodCode,
       f.RecipientCode, f.SignNumber, f.HouseNumber, f.BinduCount,
       sav.TotalBindus AS SarvaBindus, sav.IncludesLagna AS SarvaIncludesLagna,
       f.ComputedAtUtc
FROM dbo.tbl_Fact_BhinnaAshtakavarga f
JOIN dbo.tbl_ChartResults c ON c.Id = f.ChartResultId
LEFT JOIN dbo.tbl_Fact_SarvaAshtakavarga sav
       ON sav.ChartResultId = f.ChartResultId
      AND sav.MethodCode = f.MethodCode
      AND sav.SignNumber = f.SignNumber;
GO

INSERT dbo.SchemaMigrations (ScriptName, Note)
SELECT '106_add_ashtakavarga_house_number.sql',
       'HouseNumber TINYINT on Bhinna/SarvaAshtakavarga, backfilled from Chart_KeyDetails.SignId; vw_ChartAshtakavarga surfaces it instead of UI deriving from AscendantSign text.'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '106_add_ashtakavarga_house_number.sql');
GO

PRINT '106 applied: Ashtakavarga HouseNumber persisted + backfilled.';
GO
