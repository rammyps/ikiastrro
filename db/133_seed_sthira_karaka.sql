-- =====================================================================
-- 133 - Sthira Karaka (fixed house-role significators), Raman-sourced.
--
-- Populates the STHIRA slot in tbl_Dim_KarakaRole reserved-but-unseeded by
-- migration 103 (docs/database/karakafix.md) pending "a verified source."
-- Source: B.V. Raman, How to Judge a Horoscope, Vol. I (SRC_RAMAN_HTJH) --
-- 6 Sthira Karaka roles confirmed via scattered usage across 20+ case
-- studies (docs/research/domain/house-lagna-significations.md, point #3):
--   Thanukaraka (1st, Sun), Dhanakaraka (2nd, Jupiter), Bhratrukaraka
--   (3rd, Mars), Matrukaraka (4th, Moon), Putrakaraka (5th, Jupiter),
--   Ayushkaraka (8th, Saturn).
-- No source found for the other 6 houses (6th/7th/9th/10th/11th/12th) --
-- left unassigned rather than guessed. FEAT-HOUSE-03 / FEAT-KARAKA-03.
--
-- These 6 (planet, house) pairs happen to coincide with the existing
-- Naisargika primary-karaka table (tbl_Rule_KarakaMatter, KarakaTypeCode=
-- NAISARGIKA, migration 103) for the same houses -- expected, since PVR's
-- Naisargika and Raman's Sthira Karaka are independently-sourced accounts
-- of the same classical convention, not two different theories. Modeled
-- as separate KarakaTypeCode rows anyway (not merged), matching this
-- project's "don't merge distinct traditions" norm (see vedic-books.md's
-- Choudhry/PVR correction) -- each keeps its own source and role identity.
--
-- Idempotent throughout.
-- Apply:  sqlcmd -S "localhost\SQLSERVER2025" -E -d ikiastrro -b -f 65001 -i db/133_seed_sthira_karaka.sql
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

-- --- Batch 1: allow a Raman-sourced BasisCode alongside the existing PVR ones ---
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Rule_KarakaMatter_Basis')
    ALTER TABLE dbo.tbl_Rule_KarakaMatter DROP CONSTRAINT CK_Rule_KarakaMatter_Basis;
GO
ALTER TABLE dbo.tbl_Rule_KarakaMatter
    ADD CONSTRAINT CK_Rule_KarakaMatter_Basis CHECK (BasisCode IS NULL OR BasisCode IN
        ('PVR_DIRECT','PVR_CROSSVALIDATED','PROJECT_SYNTHESIS','RAMAN_DIRECT'));
GO

-- --- Batch 2: 6 STHIRA roles ---
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Dim_KarakaRole WHERE KarakaTypeCode = 'STHIRA')
    INSERT dbo.tbl_Dim_KarakaRole (KarakaTypeCode, RoleCode, FixedGrahaId)
    SELECT 'STHIRA', v.RoleCode, p.Id
    FROM (VALUES
        ('STHIRA_THANU',   'Sun'),
        ('STHIRA_DHANA',   'Jupiter'),
        ('STHIRA_BHRATRU', 'Mars'),
        ('STHIRA_MATRU',   'Moon'),
        ('STHIRA_PUTRA',   'Jupiter'),
        ('STHIRA_AYUSH',   'Saturn')
    ) v(RoleCode, PlanetName)
    JOIN dbo.tbl_Planets p ON p.PlanetName = v.PlanetName;
GO

-- --- Batch 3: life-matter vocabulary for the 6 Sthira roles ---
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Dim_LifeMatter_SourceGroup')
    ALTER TABLE dbo.tbl_Dim_LifeMatter DROP CONSTRAINT CK_Dim_LifeMatter_SourceGroup;
GO
ALTER TABLE dbo.tbl_Dim_LifeMatter
    ADD CONSTRAINT CK_Dim_LifeMatter_SourceGroup CHECK (SourceGroup IN
        ('PVR_KARAKATWA_GRID','PVR_LIFE_MATTER','RAMAN_STHIRA_KARAKA'));
GO
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Dim_LifeMatter WHERE SourceGroup = 'RAMAN_STHIRA_KARAKA')
    INSERT dbo.tbl_Dim_LifeMatter (Code, EnglishName, SourceGroup, DisplayOrder)
    VALUES
        ('STHIRA_01', 'Self, body, constitution', 'RAMAN_STHIRA_KARAKA', 1),
        ('STHIRA_02', 'Wealth and family', 'RAMAN_STHIRA_KARAKA', 2),
        ('STHIRA_03', 'Siblings', 'RAMAN_STHIRA_KARAKA', 3),
        ('STHIRA_04', 'Mother', 'RAMAN_STHIRA_KARAKA', 4),
        ('STHIRA_05', 'Children', 'RAMAN_STHIRA_KARAKA', 5),
        ('STHIRA_06', 'Longevity', 'RAMAN_STHIRA_KARAKA', 6);
GO

-- --- Batch 4: bridge rows -- every Sthira role is the classically-named
-- primary for its one house, so IsPrimary = 1 throughout ---
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_KarakaMatter km JOIN dbo.tbl_Dim_KarakaRole kr ON kr.Id = km.KarakaRoleId WHERE kr.KarakaTypeCode = 'STHIRA')
    INSERT dbo.tbl_Rule_KarakaMatter (RuleSetId, KarakaRoleId, LifeMatterId, HouseNumber, IsPrimary, DisplayOrder, BasisCode, SourceRefCode)
    SELECT rs.Id, kr.Id, dm.Id, v.HouseNumber, 1, 1, 'RAMAN_DIRECT', 'SRC_RAMAN_HTJH'
    FROM (VALUES
        ('STHIRA_THANU',   1, 'STHIRA_01'),
        ('STHIRA_DHANA',   2, 'STHIRA_02'),
        ('STHIRA_BHRATRU', 3, 'STHIRA_03'),
        ('STHIRA_MATRU',   4, 'STHIRA_04'),
        ('STHIRA_PUTRA',   5, 'STHIRA_05'),
        ('STHIRA_AYUSH',   8, 'STHIRA_06')
    ) v(RoleCode, HouseNumber, MatterCode)
    JOIN dbo.tbl_Dim_KarakaRole kr ON kr.RoleCode = v.RoleCode
    JOIN dbo.tbl_Dim_LifeMatter dm ON dm.Code = v.MatterCode
    CROSS JOIN dbo.tbl_Rule_Sets rs
    WHERE rs.IsActive = 1;
GO

-- --- Batch 5: repository-facing view, same shape as vw_Rule_PrimaryNaisargikaKaraka ---
CREATE OR ALTER VIEW dbo.vw_Rule_SthiraKaraka
AS
SELECT km.RuleSetId, km.HouseNumber, kr.FixedGrahaId AS GrahaId, kr.RoleCode,
       dm.EnglishName AS MattersSignified, km.SourceRefCode, km.IsActive
FROM dbo.tbl_Rule_KarakaMatter km
JOIN dbo.tbl_Dim_KarakaRole kr ON kr.Id = km.KarakaRoleId AND kr.KarakaTypeCode = 'STHIRA'
JOIN dbo.tbl_Dim_LifeMatter dm ON dm.Id = km.LifeMatterId
WHERE km.IsPrimary = 1;
GO

-- --- Batch 6: tbl_Rule_Catalog ---
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_Catalog WHERE RuleTableName = 'vw_Rule_SthiraKaraka')
    INSERT dbo.tbl_Rule_Catalog (RuleTableName, EngineCode, MethodCodes, Purpose, IntroducedIn)
    VALUES ('vw_Rule_SthiraKaraka', 'KARAKA', 'MAP_LOOKUP',
            'Sthira (fixed) Karaka: 6 house-role significators confirmed via B.V. Raman, How to Judge a Horoscope (SRC_RAMAN_HTJH), cross-validated across 20+ case studies. Populates the STHIRA slot in tbl_Dim_KarakaRole reserved by migration 103. No source found for the remaining 6 houses -- left unassigned, not guessed.',
            '133_seed_sthira_karaka.sql');
GO

-- --- Batch 7: ledger + verification summary ---
INSERT dbo.SchemaMigrations (ScriptName, Note)
SELECT '133_seed_sthira_karaka.sql',
       'tbl_Dim_KarakaRole += 6 STHIRA roles; tbl_Dim_LifeMatter += 6 (RAMAN_STHIRA_KARAKA); tbl_Rule_KarakaMatter += 6 (all IsPrimary); vw_Rule_SthiraKaraka added. FEAT-HOUSE-03/FEAT-KARAKA-03.'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '133_seed_sthira_karaka.sql');
GO

DECLARE @roles INT = (SELECT COUNT(*) FROM dbo.tbl_Dim_KarakaRole WHERE KarakaTypeCode = 'STHIRA');
DECLARE @matters INT = (SELECT COUNT(*) FROM dbo.tbl_Dim_LifeMatter WHERE SourceGroup = 'RAMAN_STHIRA_KARAKA');
DECLARE @bridge INT = (SELECT COUNT(*) FROM dbo.tbl_Rule_KarakaMatter km JOIN dbo.tbl_Dim_KarakaRole kr ON kr.Id = km.KarakaRoleId WHERE kr.KarakaTypeCode = 'STHIRA');
DECLARE @viewRows INT = (SELECT COUNT(*) FROM dbo.vw_Rule_SthiraKaraka);
PRINT '133 applied: ' + CAST(@roles AS VARCHAR(10)) + ' STHIRA roles (expect 6), '
    + CAST(@matters AS VARCHAR(10)) + ' life matters (expect 6), '
    + CAST(@bridge AS VARCHAR(10)) + ' bridge rows (expect 6), '
    + CAST(@viewRows AS VARCHAR(10)) + ' vw_Rule_SthiraKaraka rows (expect 6)';
GO
