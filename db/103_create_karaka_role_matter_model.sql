-- =====================================================================
-- 103 - Karaka role/matter model: tbl_Dim_KarakaRole, tbl_Dim_LifeMatter,
-- tbl_Rule_KarakaMatter. Implements docs/database/karakafix.md.
--
-- Consolidates tbl_Rule_Naisargika_Karakas (12 rows, primary-per-house
-- reduction) + tbl_Rule_Naisargika_Karakatwas (34 rows, full grid) into
-- one normalized bridge table joined to a shared karaka-role catalogue
-- and a shared life-matter vocabulary; also links the 96-row
-- tbl_Rule_LifeMatterReference into the same model via a new
-- LifeMatterId FK, replacing free-text KarakaText parsing with real FKs
-- for single fixed grahas, the 2 chara-role rows (Darakaraka/
-- Putrakaraka), and 20 structured compound rows (parsed generically
-- from "Mars and Rahu" / "Mercury/Jupiter"-style text - no hardcoded
-- planet list). The one genuine non-karaka row (KarakaText='Lagna
-- lord') gets no bridge row, per karakafix.md's own allowance.
--
-- Drops the two now-superseded tables after two compatibility views
-- (vw_Rule_PrimaryNaisargikaKaraka, vw_Rule_NaisargikaKarakatwa)
-- reproduce their shapes. tbl_Rule_LifeMatterReference itself is kept -
-- its PrimaryChartsText/HouseFromLagnaText/HouseFromKarakaText/
-- CalculationNarrative/BasisCode citations have no home in the new
-- model yet (deferred - karakafix.md migration-sequence step 9).
--
-- Idempotent throughout; safe to re-run after the DROP TABLEs succeed
-- (every seed block is guarded on its target data, not just source
-- existence).
-- Apply:  sqlcmd -S "localhost\SQLSERVER2025" -E -d ikiastrro -b -f 65001 -i db/103_create_karaka_role_matter_model.sql
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

-- --- Batch 1: tbl_Dim_KarakaRole ---
IF OBJECT_ID('dbo.tbl_Dim_KarakaRole', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Dim_KarakaRole (
        Id               INT IDENTITY(1,1) NOT NULL
                             CONSTRAINT PK_Dim_KarakaRole PRIMARY KEY,
        KarakaTypeCode   VARCHAR(12)  NOT NULL,
        RoleCode         VARCHAR(20)  NOT NULL,
        FixedGrahaId     TINYINT      NULL
                             CONSTRAINT FK_Dim_KarakaRole_Graha FOREIGN KEY REFERENCES dbo.tbl_Planets (Id),
        CharaKarakaCode  VARCHAR(4)   NULL,
        IsActive         BIT          NOT NULL CONSTRAINT DF_Dim_KarakaRole_IsActive DEFAULT 1,
        CONSTRAINT UQ_Dim_KarakaRole_Code UNIQUE (RoleCode),
        CONSTRAINT CK_Dim_KarakaRole_Type CHECK (KarakaTypeCode IN ('NAISARGIKA','STHIRA','CHARA')),
        CONSTRAINT CK_Dim_KarakaRole_Chara CHECK (CharaKarakaCode IS NULL OR CharaKarakaCode IN ('AK','AmK','BK','MK','PiK','PK','GK','DK')),
        CONSTRAINT CK_Dim_KarakaRole_Target CHECK (
            (KarakaTypeCode IN ('NAISARGIKA','STHIRA') AND FixedGrahaId IS NOT NULL AND CharaKarakaCode IS NULL)
            OR (KarakaTypeCode = 'CHARA' AND CharaKarakaCode IS NOT NULL AND FixedGrahaId IS NULL)
        )
    );
END
GO
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Dim_KarakaRole WHERE KarakaTypeCode = 'NAISARGIKA')
    INSERT dbo.tbl_Dim_KarakaRole (KarakaTypeCode, RoleCode, FixedGrahaId)
    SELECT 'NAISARGIKA', 'GRAHA_' + UPPER(p.PlanetName), p.Id
    FROM dbo.tbl_Planets p;
GO
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Dim_KarakaRole WHERE KarakaTypeCode = 'CHARA')
    INSERT dbo.tbl_Dim_KarakaRole (KarakaTypeCode, RoleCode, CharaKarakaCode)
    SELECT 'CHARA', 'CHARA_' + rk.TargetValue, rk.TargetValue
    FROM dbo.tbl_Rule_Karaka rk
    WHERE rk.KarakaScheme = 'Chara';
GO

-- --- Batch 2: tbl_Dim_LifeMatter ---
IF OBJECT_ID('dbo.tbl_Dim_LifeMatter', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Dim_LifeMatter (
        Id            INT IDENTITY(1,1) NOT NULL
                          CONSTRAINT PK_Dim_LifeMatter PRIMARY KEY,
        Code          VARCHAR(40)   NOT NULL,
        EnglishName   NVARCHAR(120) NOT NULL,
        CategoryCode  VARCHAR(30)   NULL,
        CategoryName  NVARCHAR(60)  NULL,
        SourceGroup   VARCHAR(20)   NOT NULL,
        DisplayOrder  TINYINT       NOT NULL,
        IsActive      BIT           NOT NULL CONSTRAINT DF_Dim_LifeMatter_IsActive DEFAULT 1,
        CONSTRAINT UQ_Dim_LifeMatter_Code UNIQUE (Code),
        CONSTRAINT CK_Dim_LifeMatter_SourceGroup CHECK (SourceGroup IN ('PVR_KARAKATWA_GRID','PVR_LIFE_MATTER'))
    );
END
GO
-- 32 rows: one per distinct (Matter, HouseNumber) pair in the Karakatwa grid.
-- Two pairs collapse (Rahu/Ketu share "Occult knowledge"/H8 and "Pilgrimage
-- and foreign travel"/H9) - intentional: same life matter, two karaka roles.
IF OBJECT_ID('dbo.tbl_Rule_Naisargika_Karakatwas', 'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.tbl_Dim_LifeMatter WHERE SourceGroup = 'PVR_KARAKATWA_GRID')
BEGIN
    ;WITH distinct_matters AS (
        SELECT DISTINCT Matter, HouseNumber FROM dbo.tbl_Rule_Naisargika_Karakatwas
    ),
    numbered AS (
        SELECT Matter, ROW_NUMBER() OVER (ORDER BY HouseNumber, Matter) AS rn
        FROM distinct_matters
    )
    INSERT dbo.tbl_Dim_LifeMatter (Code, EnglishName, SourceGroup, DisplayOrder)
    SELECT 'KARAKATWA_' + RIGHT('0' + CAST(rn AS VARCHAR(2)), 2), Matter, 'PVR_KARAKATWA_GRID', CAST(rn AS TINYINT)
    FROM numbered;
END
GO
-- 96 rows: one per tbl_Rule_LifeMatterReference row (kept 1:1 - PVR
-- deliberately repeats some matters, e.g. "Accidents", across categories).
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Dim_LifeMatter WHERE SourceGroup = 'PVR_LIFE_MATTER')
    INSERT dbo.tbl_Dim_LifeMatter (Code, EnglishName, CategoryCode, CategoryName, SourceGroup, DisplayOrder)
    SELECT lm.CategoryCode + '_' + RIGHT('0' + CAST(lm.DisplayOrder AS VARCHAR(2)), 2),
           lm.MatterText, lm.CategoryCode, lm.CategoryName, 'PVR_LIFE_MATTER', lm.DisplayOrder
    FROM dbo.tbl_Rule_LifeMatterReference lm;
GO

-- --- Batch 3: tbl_Rule_KarakaMatter ---
IF OBJECT_ID('dbo.tbl_Rule_KarakaMatter', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Rule_KarakaMatter (
        Id            INT IDENTITY(1,1) NOT NULL
                          CONSTRAINT PK_Rule_KarakaMatter PRIMARY KEY,
        RuleSetId     TINYINT      NOT NULL
                          CONSTRAINT FK_Rule_KarakaMatter_RuleSet FOREIGN KEY REFERENCES dbo.tbl_Rule_Sets (Id),
        KarakaRoleId  INT          NOT NULL
                          CONSTRAINT FK_Rule_KarakaMatter_Role FOREIGN KEY REFERENCES dbo.tbl_Dim_KarakaRole (Id),
        LifeMatterId  INT          NOT NULL
                          CONSTRAINT FK_Rule_KarakaMatter_Matter FOREIGN KEY REFERENCES dbo.tbl_Dim_LifeMatter (Id),
        HouseNumber   TINYINT      NULL,
        IsPrimary     BIT          NOT NULL CONSTRAINT DF_Rule_KarakaMatter_IsPrimary DEFAULT 0,
        DisplayOrder  TINYINT      NOT NULL CONSTRAINT DF_Rule_KarakaMatter_Order DEFAULT 1,
        BasisCode     VARCHAR(20)  NULL,
        SourceRefCode VARCHAR(40)  NULL,
        IsActive      BIT          NOT NULL CONSTRAINT DF_Rule_KarakaMatter_IsActive DEFAULT 1,
        CONSTRAINT CK_Rule_KarakaMatter_House CHECK (HouseNumber IS NULL OR HouseNumber BETWEEN 1 AND 12),
        CONSTRAINT CK_Rule_KarakaMatter_Basis CHECK (BasisCode IS NULL OR BasisCode IN ('PVR_DIRECT','PVR_CROSSVALIDATED','PROJECT_SYNTHESIS')),
        CONSTRAINT CK_Rule_KarakaMatter_Src   CHECK (SourceRefCode IS NULL OR SourceRefCode LIKE 'SRC[_]%'),
        CONSTRAINT UQ_Rule_KarakaMatter UNIQUE (RuleSetId, KarakaRoleId, LifeMatterId, HouseNumber)
    );
    CREATE NONCLUSTERED INDEX IX_Rule_KarakaMatter_Matter ON dbo.tbl_Rule_KarakaMatter (RuleSetId, LifeMatterId);
    CREATE NONCLUSTERED INDEX IX_Rule_KarakaMatter_Role   ON dbo.tbl_Rule_KarakaMatter (RuleSetId, KarakaRoleId);
END
GO

-- 34 rows: Karakatwa grid -> KarakaMatter, joined back onto the LifeMatter
-- rows by Matter text (unique within the grid, including the 2 shared
-- Rahu/Ketu pairs - both graha rows land on the same LifeMatterId).
IF OBJECT_ID('dbo.tbl_Rule_Naisargika_Karakatwas', 'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_KarakaMatter km JOIN dbo.tbl_Dim_LifeMatter dm ON dm.Id = km.LifeMatterId WHERE dm.SourceGroup = 'PVR_KARAKATWA_GRID')
    INSERT dbo.tbl_Rule_KarakaMatter (RuleSetId, KarakaRoleId, LifeMatterId, HouseNumber, DisplayOrder, BasisCode, SourceRefCode)
    SELECT k.RuleSetId, kr.Id, dm.Id, k.HouseNumber, k.DisplayOrder, 'PVR_DIRECT', k.SourceRefCode
    FROM dbo.tbl_Rule_Naisargika_Karakatwas k
    JOIN dbo.tbl_Dim_KarakaRole kr ON kr.KarakaTypeCode = 'NAISARGIKA' AND kr.FixedGrahaId = k.GrahaId
    JOIN dbo.tbl_Dim_LifeMatter dm ON dm.SourceGroup = 'PVR_KARAKATWA_GRID' AND dm.EnglishName = k.Matter;
GO

-- 12 of the 34 rows flip IsPrimary=1: the classically-cited per-house
-- reduction, backed structurally now instead of only by a migration check.
IF OBJECT_ID('dbo.tbl_Rule_Naisargika_Karakas', 'U') IS NOT NULL
    UPDATE km
    SET IsPrimary = 1
    FROM dbo.tbl_Rule_KarakaMatter km
    JOIN dbo.tbl_Dim_KarakaRole kr ON kr.Id = km.KarakaRoleId AND kr.KarakaTypeCode = 'NAISARGIKA'
    JOIN dbo.tbl_Rule_Naisargika_Karakas pk ON pk.RuleSetId = km.RuleSetId AND pk.GrahaId = kr.FixedGrahaId AND pk.HouseNumber = km.HouseNumber
    WHERE km.IsPrimary = 0;
GO

-- 73 + 2 + 40 rows: the 96-set's single-graha, chara-role, and parsed
-- compound-text rows. "Lagna lord" (SELF_HEALTH) gets no bridge row - it
-- names no planet, so it never joins tbl_Planets; stays a LifeMatter with
-- zero karaka roles, per karakafix.md's own "non-karaka reference" allowance.
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_KarakaMatter km JOIN dbo.tbl_Dim_LifeMatter dm ON dm.Id = km.LifeMatterId WHERE dm.SourceGroup = 'PVR_LIFE_MATTER')
BEGIN
    INSERT dbo.tbl_Rule_KarakaMatter (RuleSetId, KarakaRoleId, LifeMatterId, DisplayOrder, BasisCode, SourceRefCode)
    SELECT lm.RuleSetId, kr.Id, dm.Id, lm.DisplayOrder, lm.BasisCode, lm.SourceRefCode
    FROM dbo.tbl_Rule_LifeMatterReference lm
    JOIN dbo.tbl_Dim_LifeMatter dm ON dm.SourceGroup = 'PVR_LIFE_MATTER' AND dm.Code = lm.CategoryCode + '_' + RIGHT('0' + CAST(lm.DisplayOrder AS VARCHAR(2)), 2)
    JOIN dbo.tbl_Dim_KarakaRole kr ON kr.KarakaTypeCode = 'NAISARGIKA' AND kr.FixedGrahaId = lm.NaisargikaGrahaId
    WHERE lm.NaisargikaGrahaId IS NOT NULL;

    INSERT dbo.tbl_Rule_KarakaMatter (RuleSetId, KarakaRoleId, LifeMatterId, DisplayOrder, BasisCode, SourceRefCode)
    SELECT lm.RuleSetId, kr.Id, dm.Id, lm.DisplayOrder, lm.BasisCode, lm.SourceRefCode
    FROM dbo.tbl_Rule_LifeMatterReference lm
    JOIN dbo.tbl_Dim_LifeMatter dm ON dm.SourceGroup = 'PVR_LIFE_MATTER' AND dm.Code = lm.CategoryCode + '_' + RIGHT('0' + CAST(lm.DisplayOrder AS VARCHAR(2)), 2)
    JOIN dbo.tbl_Dim_KarakaRole kr ON kr.KarakaTypeCode = 'CHARA' AND kr.CharaKarakaCode = lm.CharaKarakaCode
    WHERE lm.CharaKarakaCode IS NOT NULL;

    ;WITH compound AS (
        SELECT lm.RuleSetId, lm.DisplayOrder, lm.BasisCode, lm.SourceRefCode, dm.Id AS LifeMatterId,
               LTRIM(RTRIM(s.value)) AS PlanetName
        FROM dbo.tbl_Rule_LifeMatterReference lm
        JOIN dbo.tbl_Dim_LifeMatter dm ON dm.SourceGroup = 'PVR_LIFE_MATTER' AND dm.Code = lm.CategoryCode + '_' + RIGHT('0' + CAST(lm.DisplayOrder AS VARCHAR(2)), 2)
        CROSS APPLY STRING_SPLIT(REPLACE(lm.KarakaText, ' and ', '/'), '/') s
        WHERE lm.NaisargikaGrahaId IS NULL AND lm.CharaKarakaCode IS NULL
    )
    INSERT dbo.tbl_Rule_KarakaMatter (RuleSetId, KarakaRoleId, LifeMatterId, DisplayOrder, BasisCode, SourceRefCode)
    SELECT c.RuleSetId, kr.Id, c.LifeMatterId, c.DisplayOrder, c.BasisCode, c.SourceRefCode
    FROM compound c
    JOIN dbo.tbl_Planets p ON p.PlanetName = c.PlanetName
    JOIN dbo.tbl_Dim_KarakaRole kr ON kr.KarakaTypeCode = 'NAISARGIKA' AND kr.FixedGrahaId = p.Id;
END
GO

-- --- Batch 4: link tbl_Rule_LifeMatterReference into the new model ---
IF COL_LENGTH('dbo.tbl_Rule_LifeMatterReference', 'LifeMatterId') IS NULL
    ALTER TABLE dbo.tbl_Rule_LifeMatterReference
        ADD LifeMatterId INT NULL
            CONSTRAINT FK_Rule_LifeMatterReference_LifeMatter FOREIGN KEY REFERENCES dbo.tbl_Dim_LifeMatter (Id);
GO
UPDATE lm
SET LifeMatterId = dm.Id
FROM dbo.tbl_Rule_LifeMatterReference lm
JOIN dbo.tbl_Dim_LifeMatter dm ON dm.SourceGroup = 'PVR_LIFE_MATTER'
    AND dm.Code = lm.CategoryCode + '_' + RIGHT('0' + CAST(lm.DisplayOrder AS VARCHAR(2)), 2)
WHERE lm.LifeMatterId IS NULL;
GO

-- --- Batch 5: compatibility views, then drop the superseded tables ---
CREATE OR ALTER VIEW dbo.vw_Rule_PrimaryNaisargikaKaraka
AS
SELECT km.RuleSetId, km.HouseNumber, kr.FixedGrahaId AS GrahaId,
       STRING_AGG(dm.EnglishName, ', ') WITHIN GROUP (ORDER BY dm.DisplayOrder) AS MattersSignified,
       km.SourceRefCode, km.IsActive
FROM dbo.tbl_Rule_KarakaMatter km
JOIN dbo.tbl_Dim_KarakaRole kr ON kr.Id = km.KarakaRoleId AND kr.KarakaTypeCode = 'NAISARGIKA'
JOIN dbo.tbl_Dim_LifeMatter dm ON dm.Id = km.LifeMatterId
WHERE km.IsPrimary = 1
GROUP BY km.RuleSetId, km.HouseNumber, kr.FixedGrahaId, km.SourceRefCode, km.IsActive;
GO
CREATE OR ALTER VIEW dbo.vw_Rule_NaisargikaKarakatwa
AS
SELECT km.RuleSetId, kr.FixedGrahaId AS GrahaId, km.HouseNumber, dm.EnglishName AS Matter,
       km.DisplayOrder, km.SourceRefCode, km.IsActive
FROM dbo.tbl_Rule_KarakaMatter km
JOIN dbo.tbl_Dim_KarakaRole kr ON kr.Id = km.KarakaRoleId AND kr.KarakaTypeCode = 'NAISARGIKA'
JOIN dbo.tbl_Dim_LifeMatter dm ON dm.Id = km.LifeMatterId AND dm.SourceGroup = 'PVR_KARAKATWA_GRID';
GO
IF OBJECT_ID('dbo.tbl_Rule_Naisargika_Karakas', 'U') IS NOT NULL
    DROP TABLE dbo.tbl_Rule_Naisargika_Karakas;
GO
IF OBJECT_ID('dbo.tbl_Rule_Naisargika_Karakatwas', 'U') IS NOT NULL
    DROP TABLE dbo.tbl_Rule_Naisargika_Karakatwas;
GO

-- --- Batch 6: tbl_Rule_Catalog ---
DELETE FROM dbo.tbl_Rule_Catalog WHERE RuleTableName IN ('tbl_Rule_Naisargika_Karakatwas','tbl_Rule_Naisargika_Karakas');
GO
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_Catalog WHERE RuleTableName = 'tbl_Dim_KarakaRole')
    INSERT dbo.tbl_Rule_Catalog (RuleTableName, EngineCode, MethodCodes, Purpose, IntroducedIn)
    VALUES ('tbl_Dim_KarakaRole', 'KARAKA', 'CATALOG_LOOKUP',
            'Karaka role catalogue: one row per Naisargika graha (9) and per Chara role (8, driven from tbl_Rule_Karaka); Sthira reserved/unseeded. Replaces the unchecked CharaKarakaCode string with an FK target for tbl_Rule_KarakaMatter.',
            '103_create_karaka_role_matter_model.sql');
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_Catalog WHERE RuleTableName = 'tbl_Dim_LifeMatter')
    INSERT dbo.tbl_Rule_Catalog (RuleTableName, EngineCode, MethodCodes, Purpose, IntroducedIn)
    VALUES ('tbl_Dim_LifeMatter', 'KARAKA', 'CATALOG_LOOKUP',
            'Life-matter vocabulary: 32 rows from the Naisargika karakatwa grid (PVR ch.8) + 96 rows from tbl_Rule_LifeMatterReference''s specific-matter set (PVR Table 11/sec 7.2/Table 12), each a stable Code join target for tbl_Rule_KarakaMatter.',
            '103_create_karaka_role_matter_model.sql');
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_Catalog WHERE RuleTableName = 'tbl_Rule_KarakaMatter')
    INSERT dbo.tbl_Rule_Catalog (RuleTableName, EngineCode, MethodCodes, Purpose, IntroducedIn)
    VALUES ('tbl_Rule_KarakaMatter', 'KARAKA', 'MAP_LOOKUP',
            'Canonical karaka-role <-> life-matter bridge (docs/database/karakafix.md): replaces tbl_Rule_Naisargika_Karakas/Karakatwas and the free-text KarakaText parsing in tbl_Rule_LifeMatterReference. Single and compound karakas (e.g. Mars+Rahu) are separate rows for one LifeMatterId. IsPrimary flags the 12 classically-cited per-house Naisargika reductions.',
            '103_create_karaka_role_matter_model.sql');
GO
UPDATE dbo.tbl_Rule_Catalog
SET Purpose = 'Bridges PVR Table 11 + sec 7.2 + Table 12 + naisargika/chara karaka: for each of 96 specific interpretive matters (10 categories), which divisional chart, house, karaka, and house-from-karaka to read. BasisCode marks provenance. LifeMatterId (mig. 103) links each row to tbl_Dim_LifeMatter/tbl_Rule_KarakaMatter; legacy karaka columns stay pending migration.'
WHERE RuleTableName = 'tbl_Rule_LifeMatterReference';
GO

-- --- Batch 7: ledger + verification summary ---
INSERT dbo.SchemaMigrations (ScriptName, Note)
SELECT '103_create_karaka_role_matter_model.sql',
       'tbl_Dim_KarakaRole (17) + tbl_Dim_LifeMatter (128) + tbl_Rule_KarakaMatter (149, 12 primary); LifeMatterReference += LifeMatterId; dropped Naisargika_Karakas/Karakatwas (see 2 compat views)'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '103_create_karaka_role_matter_model.sql');
GO

DECLARE @roles INT = (SELECT COUNT(*) FROM dbo.tbl_Dim_KarakaRole);
DECLARE @matters INT = (SELECT COUNT(*) FROM dbo.tbl_Dim_LifeMatter);
DECLARE @bridge INT = (SELECT COUNT(*) FROM dbo.tbl_Rule_KarakaMatter);
DECLARE @primaryCount INT = (SELECT COUNT(*) FROM dbo.tbl_Rule_KarakaMatter WHERE IsPrimary = 1);
DECLARE @oldKarakas INT = (SELECT OBJECT_ID('dbo.tbl_Rule_Naisargika_Karakas', 'U'));
DECLARE @oldKarakatwas INT = (SELECT OBJECT_ID('dbo.tbl_Rule_Naisargika_Karakatwas', 'U'));
DECLARE @unlinked INT = (SELECT COUNT(*) FROM dbo.tbl_Rule_LifeMatterReference WHERE LifeMatterId IS NULL);
PRINT '103 applied: ' + CAST(@roles AS VARCHAR(10)) + ' karaka roles (expect 17), '
    + CAST(@matters AS VARCHAR(10)) + ' life matters (expect 128), '
    + CAST(@bridge AS VARCHAR(10)) + ' karaka-matter bridge rows (expect 149), '
    + CAST(@primaryCount AS VARCHAR(10)) + ' primary rows (expect 12); old tables dropped: '
    + CASE WHEN @oldKarakas IS NULL AND @oldKarakatwas IS NULL THEN 'yes' ELSE 'NO - check' END
    + '; tbl_Rule_LifeMatterReference rows with no LifeMatterId link (expect 0): ' + CAST(@unlinked AS VARCHAR(10));
GO
