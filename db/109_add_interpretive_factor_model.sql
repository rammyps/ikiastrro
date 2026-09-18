-- =====================================================================
-- 109 — Interpretive factor model: normalizes the House/Planet/Sign/Varga
--       facts that tbl_Dim_LifeArea (30), tbl_Dim_DivisionalSubject (38)
--       and tbl_Dim_KarakaRole's Chara rows (103) each touch but have
--       never been cross-referenced against each other. Concretely: the
--       PVR-cited prose in tbl_Dim_DivisionalSubject.D1Foundation (e.g.
--       "7th house, 7th lord, Venus and partnership combinations" for
--       Marriage) already encodes these facts, but only as unparseable
--       text — nothing can query "which subjects cite Venus" today.
--
--   tbl_Dim_InterpretiveFactor     - 4-row catalogue: HOUSE / PLANET /
--                                     SIGN_LAGNA / VARGA. Pure reference
--                                     dim, no RuleSetId (matches 30/38).
--   tbl_Rule_InterpretiveFactorDetail - bridge/fact rows, one per
--                                     (LifeArea | DivisionalSubject |
--                                     KarakaRole, factor, value). Carries
--                                     RuleSetId (it's a tbl_Rule_ table
--                                     with real content, matching 103's
--                                     tbl_Rule_KarakaMatter). Three
--                                     nullable typed FK "which thing"
--                                     columns instead of one polymorphic
--                                     ReferenceId column, because
--                                     tbl_Dim_DivisionalSubject's PK is
--                                     SubjectCode VARCHAR(40), not a
--                                     surrogate int — a single ReferenceId
--                                     column cannot carry real FKs to
--                                     TINYINT/VARCHAR(40)/INT parents at
--                                     once. Same "exactly one populated"
--                                     discriminated-by-null shape 103's
--                                     tbl_Dim_KarakaRole already uses for
--                                     FixedGrahaId/CharaKarakaCode, widened
--                                     3-way; four nullable value columns
--                                     tied to FactorId the same way.
--
-- Seeded this pass: the 4 factor rows, plus all 11 tbl_Dim_DivisionalSubject
-- rows' House/Planet/Varga facts, hand-parsed from each row's own
-- D1Foundation text (source: SRC_PVR_INTEGRATED, same as 38 itself — no
-- new citation, just normalizing what 38 already cites). LifeArea (20
-- rows, PVR Table 11) and Chara-karaka-role details are deliberately left
-- for a follow-up migration — see docs/database/karakafix.md and
-- docs/research/domain/res_charakarakas.md for the still-open background
-- reading those need first. No LifeArea/DivisionalSubject reconciliation
-- view: they answer different PVR questions (broad chart-coverage vs.
-- narrow subject-confirmation), not duplicate encodings of the same fact,
-- so both stay independent — see
-- docs/research/domain/lifearea-varga-charakaraka-synthesis.md.
--
-- Apply:  sqlcmd -S localhost\SQLSERVER2025 -E -d ikiastrro -b -i db/109_add_interpretive_factor_model.sql
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- --- Batch 1: tbl_Dim_InterpretiveFactor ---
IF OBJECT_ID('dbo.tbl_Dim_InterpretiveFactor', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Dim_InterpretiveFactor (
        Id             TINYINT       NOT NULL CONSTRAINT PK_Dim_InterpretiveFactor PRIMARY KEY,
        FactorCode     VARCHAR(20)   NOT NULL CONSTRAINT UQ_Dim_InterpretiveFactor_Code UNIQUE,
        FactorName     NVARCHAR(60)  NOT NULL,
        Description    NVARCHAR(200) NOT NULL,
        SortOrder      TINYINT       NOT NULL,
        SourceRefCode  VARCHAR(40)   NULL,
        IsActive       BIT           NOT NULL CONSTRAINT DF_Dim_InterpretiveFactor_IsActive DEFAULT 1,
        CONSTRAINT CK_Dim_InterpretiveFactor_Source CHECK (SourceRefCode IS NULL OR SourceRefCode LIKE 'SRC[_]%')
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Dim_InterpretiveFactor)
INSERT dbo.tbl_Dim_InterpretiveFactor (Id, FactorCode, FactorName, Description, SortOrder)
VALUES
    (1, 'HOUSE',      N'House',       N'A house (bhava) number, 1-12.', 1),
    (2, 'PLANET',     N'Planet',      N'A graha, including the nodes.', 2),
    (3, 'SIGN_LAGNA', N'Sign / Lagna', N'A rasi, or the Lagna itself as the zero-house reference point.', 3),
    (4, 'VARGA',      N'Varga',       N'A divisional chart (D1-D60).', 4);
GO

-- --- Batch 2: tbl_Rule_InterpretiveFactorDetail ---
IF OBJECT_ID('dbo.tbl_Rule_InterpretiveFactorDetail', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Rule_InterpretiveFactorDetail (
        Id                     INT      IDENTITY(1,1) NOT NULL CONSTRAINT PK_Rule_InterpretiveFactorDetail PRIMARY KEY,
        RuleSetId              TINYINT  NOT NULL
                                   CONSTRAINT FK_Rule_InterpretiveFactorDetail_RuleSet FOREIGN KEY REFERENCES dbo.tbl_Rule_Sets (Id),
        LifeAreaId             TINYINT  NULL
                                   CONSTRAINT FK_Rule_InterpretiveFactorDetail_LifeArea FOREIGN KEY REFERENCES dbo.tbl_Dim_LifeArea (Id),
        DivisionalSubjectCode  VARCHAR(40) NULL
                                   CONSTRAINT FK_Rule_InterpretiveFactorDetail_DivisionalSubject FOREIGN KEY REFERENCES dbo.tbl_Dim_DivisionalSubject (SubjectCode),
        KarakaRoleId           INT      NULL
                                   CONSTRAINT FK_Rule_InterpretiveFactorDetail_KarakaRole FOREIGN KEY REFERENCES dbo.tbl_Dim_KarakaRole (Id),
        FactorId               TINYINT  NOT NULL
                                   CONSTRAINT FK_Rule_InterpretiveFactorDetail_Factor FOREIGN KEY REFERENCES dbo.tbl_Dim_InterpretiveFactor (Id),
        HouseNumber            TINYINT  NULL,
        GrahaId                TINYINT  NULL
                                   CONSTRAINT FK_Rule_InterpretiveFactorDetail_Graha FOREIGN KEY REFERENCES dbo.tbl_Planets (Id),
        SignId                 TINYINT  NULL
                                   CONSTRAINT FK_Rule_InterpretiveFactorDetail_Sign FOREIGN KEY REFERENCES dbo.tbl_SignAttributes (Id),
        ChartTypeId            TINYINT  NULL
                                   CONSTRAINT FK_Rule_InterpretiveFactorDetail_ChartType FOREIGN KEY REFERENCES dbo.tbl_Dim_ChartType (Id),
        IsPrimary              BIT      NOT NULL CONSTRAINT DF_Rule_InterpretiveFactorDetail_IsPrimary DEFAULT 0,
        DisplayOrder           TINYINT  NOT NULL CONSTRAINT DF_Rule_InterpretiveFactorDetail_DisplayOrder DEFAULT 1,
        SourceRefCode          VARCHAR(40) NULL,
        Notes                  NVARCHAR(300) NULL,
        IsActive               BIT      NOT NULL CONSTRAINT DF_Rule_InterpretiveFactorDetail_IsActive DEFAULT 1,
        -- Exactly one of the three "which thing" columns — mirrors 103's CK_Dim_KarakaRole_Target.
        CONSTRAINT CK_Rule_InterpretiveFactorDetail_Reference CHECK (
               (LifeAreaId IS NOT NULL AND DivisionalSubjectCode IS NULL     AND KarakaRoleId IS NULL)
            OR (LifeAreaId IS NULL     AND DivisionalSubjectCode IS NOT NULL AND KarakaRoleId IS NULL)
            OR (LifeAreaId IS NULL     AND DivisionalSubjectCode IS NULL     AND KarakaRoleId IS NOT NULL)
        ),
        -- Exactly one of the four value columns, and it must be the one FactorId names
        -- (1=HOUSE, 2=PLANET, 3=SIGN_LAGNA, 4=VARGA — fixed Ids seeded above, so this is a
        -- same-row literal comparison, not a cross-table lookup a CHECK can't express).
        CONSTRAINT CK_Rule_InterpretiveFactorDetail_FactorValue CHECK (
               (FactorId = 1 AND HouseNumber IS NOT NULL AND GrahaId IS NULL     AND SignId IS NULL     AND ChartTypeId IS NULL)
            OR (FactorId = 2 AND GrahaId IS NOT NULL     AND HouseNumber IS NULL AND SignId IS NULL     AND ChartTypeId IS NULL)
            OR (FactorId = 3 AND SignId IS NOT NULL      AND HouseNumber IS NULL AND GrahaId IS NULL    AND ChartTypeId IS NULL)
            OR (FactorId = 4 AND ChartTypeId IS NOT NULL AND HouseNumber IS NULL AND GrahaId IS NULL    AND SignId IS NULL)
        ),
        CONSTRAINT CK_Rule_InterpretiveFactorDetail_House  CHECK (HouseNumber IS NULL OR HouseNumber BETWEEN 1 AND 12),
        CONSTRAINT CK_Rule_InterpretiveFactorDetail_Source CHECK (SourceRefCode IS NULL OR SourceRefCode LIKE 'SRC[_]%'),
        CONSTRAINT UQ_Rule_InterpretiveFactorDetail UNIQUE
            (RuleSetId, LifeAreaId, DivisionalSubjectCode, KarakaRoleId, FactorId, HouseNumber, GrahaId, SignId, ChartTypeId)
    );
    CREATE INDEX IX_Rule_InterpretiveFactorDetail_LifeArea    ON dbo.tbl_Rule_InterpretiveFactorDetail (LifeAreaId)            WHERE LifeAreaId IS NOT NULL;
    CREATE INDEX IX_Rule_InterpretiveFactorDetail_DivSubject  ON dbo.tbl_Rule_InterpretiveFactorDetail (DivisionalSubjectCode) WHERE DivisionalSubjectCode IS NOT NULL;
    CREATE INDEX IX_Rule_InterpretiveFactorDetail_KarakaRole  ON dbo.tbl_Rule_InterpretiveFactorDetail (KarakaRoleId)          WHERE KarakaRoleId IS NOT NULL;
END
GO

-- --- Seed: VARGA rows, one per DivisionalSubject, straight from its own PrimaryConfirmationChartId ---
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_InterpretiveFactorDetail WHERE FactorId = 4 AND DivisionalSubjectCode IS NOT NULL)
INSERT dbo.tbl_Rule_InterpretiveFactorDetail
    (RuleSetId, DivisionalSubjectCode, FactorId, ChartTypeId, IsPrimary, DisplayOrder, SourceRefCode)
SELECT (SELECT Id FROM dbo.tbl_Rule_Sets WHERE IsActive = 1), ds.SubjectCode, 4, ds.PrimaryConfirmationChartId, 1, 1, 'SRC_PVR_INTEGRATED'
FROM dbo.tbl_Dim_DivisionalSubject ds;
GO

-- --- Seed: HOUSE rows, hand-parsed from each subject's own D1Foundation text (38) ---
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_InterpretiveFactorDetail WHERE FactorId = 1 AND DivisionalSubjectCode IS NOT NULL)
INSERT dbo.tbl_Rule_InterpretiveFactorDetail
    (RuleSetId, DivisionalSubjectCode, FactorId, HouseNumber, IsPrimary, DisplayOrder, SourceRefCode, Notes)
SELECT (SELECT Id FROM dbo.tbl_Rule_Sets WHERE IsActive = 1), v.SubjectCode, 1, v.HouseNumber, v.IsPrimary, v.DisplayOrder, 'SRC_PVR_INTEGRATED', v.Notes
FROM (VALUES
    ('OVERALL_STRENGTH_DHARMA', 1,  1, 1, NULL),
    ('OVERALL_STRENGTH_DHARMA', 9,  0, 2, NULL),
    ('WEALTH',                  2,  1, 1, NULL),
    ('WEALTH',                  11, 0, 2, NULL),
    ('SIBLINGS_COURAGE',        3,  1, 1, NULL),
    ('PROPERTY_RESIDENCE',      4,  1, 1, NULL),
    ('CHILDREN_PROGENY',        5,  1, 1, NULL),
    ('MOTHER_PARENTS',          4,  1, 1, N'Mother specifically'),
    ('MOTHER_PARENTS',          9,  0, 2, N'Parents generally'),
    ('MOTHER_PARENTS',          10, 0, 3, N'Parents generally'),
    ('MARRIAGE_RELATIONSHIPS',  7,  1, 1, NULL),
    ('CAREER_STATUS',           10, 1, 1, NULL),
    ('VEHICLES_COMFORTS',       4,  1, 1, NULL),
    ('EDUCATION_LEARNING',      4,  1, 1, NULL),
    ('EDUCATION_LEARNING',      5,  0, 2, NULL),
    ('EDUCATION_LEARNING',      9,  0, 3, NULL)
) v (SubjectCode, HouseNumber, IsPrimary, DisplayOrder, Notes);
GO

-- --- Seed: PLANET rows, hand-parsed from each subject's own D1Foundation text (38) ---
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_InterpretiveFactorDetail WHERE FactorId = 2 AND DivisionalSubjectCode IS NOT NULL)
INSERT dbo.tbl_Rule_InterpretiveFactorDetail
    (RuleSetId, DivisionalSubjectCode, FactorId, GrahaId, IsPrimary, DisplayOrder, SourceRefCode, Notes)
SELECT (SELECT Id FROM dbo.tbl_Rule_Sets WHERE IsActive = 1), v.SubjectCode, 2, p.Id, v.IsPrimary, v.DisplayOrder, 'SRC_PVR_INTEGRATED', v.Notes
FROM (VALUES
    ('OVERALL_STRENGTH_DHARMA', 'Sun',     1, 1, NULL),
    ('WEALTH',                  'Jupiter', 1, 1, NULL),
    ('WEALTH',                  'Venus',   0, 2, NULL),
    ('SIBLINGS_COURAGE',        'Mars',    1, 1, NULL),
    ('PROPERTY_RESIDENCE',      'Moon',    1, 1, NULL),
    ('CHILDREN_PROGENY',        'Jupiter', 1, 1, NULL),
    ('MOTHER_PARENTS',          'Moon',    1, 1, N'Mother specifically'),
    ('MARRIAGE_RELATIONSHIPS',  'Venus',   1, 1, NULL),
    ('CAREER_STATUS',           'Sun',     1, 1, NULL),
    ('CAREER_STATUS',           'Saturn',  0, 2, NULL),
    ('VEHICLES_COMFORTS',       'Venus',   1, 1, NULL),
    ('VEHICLES_COMFORTS',       'Moon',    0, 2, NULL),
    ('EDUCATION_LEARNING',      'Mercury', 1, 1, NULL),
    ('EDUCATION_LEARNING',      'Jupiter', 0, 2, NULL)
) v (SubjectCode, PlanetName, IsPrimary, DisplayOrder, Notes)
JOIN dbo.tbl_Planets p ON p.PlanetName = v.PlanetName;
GO
-- KARMIC_ROOTS (D1Foundation: "D1 promise, major life indicators...") names no specific house
-- or planet — VARGA-only row above stands alone, per the plan's own "don't force rows that
-- aren't there" rule.

-- --- tbl_Rule_Catalog registration ---
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_Catalog WHERE RuleTableName = 'tbl_Dim_InterpretiveFactor')
    INSERT dbo.tbl_Rule_Catalog (RuleTableName, EngineCode, MethodCodes, Purpose, IntroducedIn)
    VALUES ('tbl_Dim_InterpretiveFactor', 'INTERPRETIVE_FACTOR', 'CATALOG_LOOKUP',
            'Catalogue of the 4 interpretive-factor types (House, Planet, Sign/Lagna, Varga) that tbl_Rule_InterpretiveFactorDetail rows are typed against.',
            '109_add_interpretive_factor_model.sql');
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_Catalog WHERE RuleTableName = 'tbl_Rule_InterpretiveFactorDetail')
    INSERT dbo.tbl_Rule_Catalog (RuleTableName, EngineCode, MethodCodes, Purpose, IntroducedIn)
    VALUES ('tbl_Rule_InterpretiveFactorDetail', 'INTERPRETIVE_FACTOR', 'MAP_LOOKUP',
            'Normalizes House/Planet/Sign/Varga facts for tbl_Dim_LifeArea, tbl_Dim_DivisionalSubject and CHARA tbl_Dim_KarakaRole rows into queryable rows instead of free text.',
            '109_add_interpretive_factor_model.sql');
GO

INSERT dbo.SchemaMigrations (ScriptName, Note)
SELECT '109_add_interpretive_factor_model.sql',
       'Adds tbl_Dim_InterpretiveFactor (4 rows) + tbl_Rule_InterpretiveFactorDetail (41 seed rows: DivisionalSubject House/Planet/Varga). LifeArea + CharaKarakaRole details follow up.'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '109_add_interpretive_factor_model.sql');
GO

PRINT '109 applied: tbl_Dim_InterpretiveFactor + tbl_Rule_InterpretiveFactorDetail ready, DivisionalSubject details seeded.';
GO
