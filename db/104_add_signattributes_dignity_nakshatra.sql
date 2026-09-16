-- =====================================================================
-- 104 - tbl_SignAttributes: connect ExaltedDegree/DebilitatedDegree to the
-- Nakshatra each falls in.
--
-- tbl_SignAttributes.ExaltedDegree/DebilitatedDegree are WITHIN-SIGN
-- (0-30). tbl_Nakshatras.StartDegree/EndDegree are ABSOLUTE (0-360).
-- Convert: AbsoluteDegree = (SignId-1)*30 + WithinSignDegree, then match
-- the Nakshatra span it falls in ([StartDegree, EndDegree)).
--
-- Values already cross-checked against tbl_Rule_GrahaDignity,
-- AstroMath.DeepExaltationPoints, and PVR Table 6
-- (docs/research/domain/dignity-pvr.md) - no mismatch found; this
-- migration only adds the Nakshatra connection, no degree changes.
--
-- Idempotent.
-- Apply:  sqlcmd -S "localhost\SQLSERVER2025" -E -d ikiastrro -b -i db/104_add_signattributes_dignity_nakshatra.sql
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

IF COL_LENGTH('dbo.tbl_SignAttributes', 'ExaltedNakshatraId') IS NULL
    ALTER TABLE dbo.tbl_SignAttributes
        ADD ExaltedNakshatraId TINYINT NULL
            CONSTRAINT FK_SignAttributes_ExaltedNakshatra FOREIGN KEY REFERENCES dbo.tbl_Nakshatras (Id);
GO
IF COL_LENGTH('dbo.tbl_SignAttributes', 'DebilitatedNakshatraId') IS NULL
    ALTER TABLE dbo.tbl_SignAttributes
        ADD DebilitatedNakshatraId TINYINT NULL
            CONSTRAINT FK_SignAttributes_DebilitatedNakshatra FOREIGN KEY REFERENCES dbo.tbl_Nakshatras (Id);
GO

UPDATE sa
SET ExaltedNakshatraId = nk.Id
FROM dbo.tbl_SignAttributes sa
JOIN dbo.tbl_Nakshatras nk
    ON ((sa.Id - 1) * 30 + sa.ExaltedDegree) >= nk.StartDegree
   AND ((sa.Id - 1) * 30 + sa.ExaltedDegree) < nk.EndDegree
WHERE sa.ExaltedNakshatraId IS NULL AND sa.ExaltedDegree IS NOT NULL;
GO
UPDATE sa
SET DebilitatedNakshatraId = nk.Id
FROM dbo.tbl_SignAttributes sa
JOIN dbo.tbl_Nakshatras nk
    ON ((sa.Id - 1) * 30 + sa.DebilitatedDegree) >= nk.StartDegree
   AND ((sa.Id - 1) * 30 + sa.DebilitatedDegree) < nk.EndDegree
WHERE sa.DebilitatedNakshatraId IS NULL AND sa.DebilitatedDegree IS NOT NULL;
GO

INSERT dbo.SchemaMigrations (ScriptName, Note)
SELECT '104_add_signattributes_dignity_nakshatra.sql',
       'tbl_SignAttributes += ExaltedNakshatraId/DebilitatedNakshatraId FK tbl_Nakshatras, backfilled from degree spans'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '104_add_signattributes_dignity_nakshatra.sql');
GO

DECLARE @exalted INT = (SELECT COUNT(*) FROM dbo.tbl_SignAttributes WHERE ExaltedNakshatraId IS NOT NULL);
DECLARE @debilitated INT = (SELECT COUNT(*) FROM dbo.tbl_SignAttributes WHERE DebilitatedNakshatraId IS NOT NULL);
DECLARE @exaltedMissing INT = (SELECT COUNT(*) FROM dbo.tbl_SignAttributes WHERE ExaltedDegree IS NOT NULL AND ExaltedNakshatraId IS NULL);
DECLARE @debilitatedMissing INT = (SELECT COUNT(*) FROM dbo.tbl_SignAttributes WHERE DebilitatedDegree IS NOT NULL AND DebilitatedNakshatraId IS NULL);
PRINT '104 applied: ' + CAST(@exalted AS VARCHAR(10)) + ' signs resolved ExaltedNakshatraId (expect 7), '
    + CAST(@debilitated AS VARCHAR(10)) + ' resolved DebilitatedNakshatraId (expect 7); unresolved despite a degree present: '
    + CAST(@exaltedMissing AS VARCHAR(10)) + ' exalted, ' + CAST(@debilitatedMissing AS VARCHAR(10)) + ' debilitated (expect 0, 0).';
GO
