-- =====================================================================
-- 099 -- Amsabala reconciliation: tbl_Rule_AmsabalaGroup + tbl_Rule_AmsabalaName.
--
-- PVR Integrated Approach Sec. 6.6 "Varga Grouping and Amsabala" (SRC_PVR_INTEGRATED,
-- pp.63-65): a planet occupying its moolatrikona, own rasi, or exaltation rasi in a
-- divisional chart is a "good" placement there. Across each of 4 named varga groups, the
-- COUNT of good placements (not a points sum) names the "amsa" the planet is said to
-- occupy. This is a distinct concept from:
--   * Saptavargaja Bala (tbl_Rule_ShadbalaComponent, migration 39/71-73) -- a Shadbala
--     sub-component that assigns dignity POINTS (45/30/22.5/...) across the 7-chart
--     Saptavarga and sums them; PvrDignityEvaluator.SaptavargajaPoints already does that.
--   * Vimsopaka Bala (tbl_Rule_VimsopakaWeight, still empty/reserved) -- a weighted
--     0-20 total strength score. PVR names it (pp.188-189) but never gives its numeric
--     per-varga weight table, so it stays a separate, still-open sourcing gap; this
--     migration does not touch it. Per docs/research/domain/pvr-coverage.md item 15,
--     amsabala (this migration) was the blocking prerequisite reconciliation before
--     Vimsopaka could even be scoped.
--
-- Group membership (PVR p.63-64) -- D1 ("Rasi chart") is included in every scheme:
--   SHADVARGA     (6):  D1 D2 D3 D9 D12 D30
--   SAPTAVARGA    (7):  D1 D2 D3 D7 D9 D12 D30
--   DASAVARGA     (10): D1 D2 D3 D7 D9 D10 D12 D16 D30 D60
--   SHODASAVARGA  (16): D1 D2 D3 D4 D7 D9 D10 D12 D16 D20 D24 D27 D30 D40 D45 D60
--
-- Amsa names by good-count (PVR p.64) -- a count of 0 or 1 has no named amsa in the
-- book (its lists start at 2); AmsabalaCalculator returns AmsaName = NULL in that case,
-- not a data gap.
--
-- Verified against PVR's own worked Example 27 (Bill Cosby, Jupiter): Rasi Sg, D2 Cn,
-- D3 Li, D4 Vi, D7 Ge, D9 Sg, D10 Vi, D12 Sc, D16 Pi, D20 Pi, D24 Cn, D27 Ge, D30 Li,
-- D40 Cn, D45 Le, D60 Sc -- Jupiter owns Sg/Pi, exalted in Cn, so a placement in
-- Sg/Pi/Cn counts good. Shadvarga good={Rasi,D2,D9}=3->Vyanjanamsa; Saptavarga
-- good={Rasi,D2,D9}=3->Vyanjanamsa; Dasavarga good={Rasi,D2,D9,D16}=4->Gopuramsa;
-- Shodasavarga good={Rasi,D2,D9,D16,D20,D24,D40}=7->Kalpavrikshamsa. Matches the book
-- exactly (Ikiastrro.Web.Tests.AmsabalaCalculatorTests reproduces this as a golden
-- record).
--
-- Deliberate deviation from the tbl_Rule_* portability contract (same rationale as
-- migration 096 for tbl_Rule_SignNakshatra): this is fixed classical vocabulary from
-- one book passage, not a versioned rule with a provenance dispute -- one SourceRefCode
-- column, no RuleSetId-scoped MethodCode/RuleParametersJson/IsActive tail.
--
-- Idempotent. Apply: sqlcmd -S "localhost\SQLSERVER2025" -E -d ikiastrro -i db/099_create_rule_amsabala_scheme.sql
-- =====================================================================
USE [ikiastrro];
GO

IF OBJECT_ID(N'dbo.tbl_Rule_AmsabalaGroup', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Rule_AmsabalaGroup
    (
        Id            INT      IDENTITY(1,1) NOT NULL CONSTRAINT PK_Rule_AmsabalaGroup PRIMARY KEY,
        RuleSetId     TINYINT  NOT NULL CONSTRAINT FK_Rule_AmsabalaGroup_RuleSet   FOREIGN KEY REFERENCES dbo.tbl_Rule_Sets (Id),
        SchemeCode    VARCHAR(20) NOT NULL,
        ChartTypeId   TINYINT  NOT NULL CONSTRAINT FK_Rule_AmsabalaGroup_ChartType FOREIGN KEY REFERENCES dbo.tbl_Dim_ChartType (Id),
        SourceRefCode VARCHAR(40) NULL,
        CONSTRAINT UQ_Rule_AmsabalaGroup UNIQUE (RuleSetId, SchemeCode, ChartTypeId),
        CONSTRAINT CK_Rule_AmsabalaGroup_Scheme CHECK (SchemeCode IN ('SHADVARGA', 'SAPTAVARGA', 'DASAVARGA', 'SHODASAVARGA')),
        CONSTRAINT CK_Rule_AmsabalaGroup_Src    CHECK (SourceRefCode IS NULL OR SourceRefCode LIKE 'SRC[_]%')
    );
END
GO

IF OBJECT_ID(N'dbo.tbl_Rule_AmsabalaName', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Rule_AmsabalaName
    (
        Id            INT      IDENTITY(1,1) NOT NULL CONSTRAINT PK_Rule_AmsabalaName PRIMARY KEY,
        RuleSetId     TINYINT  NOT NULL CONSTRAINT FK_Rule_AmsabalaName_RuleSet FOREIGN KEY REFERENCES dbo.tbl_Rule_Sets (Id),
        SchemeCode    VARCHAR(20) NOT NULL,
        GoodCount     TINYINT  NOT NULL,
        AmsaName      VARCHAR(30) NOT NULL,
        SourceRefCode VARCHAR(40) NULL,
        CONSTRAINT UQ_Rule_AmsabalaName UNIQUE (RuleSetId, SchemeCode, GoodCount),
        CONSTRAINT CK_Rule_AmsabalaName_Scheme CHECK (SchemeCode IN ('SHADVARGA', 'SAPTAVARGA', 'DASAVARGA', 'SHODASAVARGA')),
        CONSTRAINT CK_Rule_AmsabalaName_Count  CHECK (GoodCount BETWEEN 2 AND 16),
        CONSTRAINT CK_Rule_AmsabalaName_Src    CHECK (SourceRefCode IS NULL OR SourceRefCode LIKE 'SRC[_]%')
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_AmsabalaGroup)
BEGIN
    ;WITH src (SchemeCode, ChartType) AS
    (
        SELECT v.SchemeCode, v.ChartType
        FROM (VALUES
            ('SHADVARGA', 'D1'), ('SHADVARGA', 'D2'), ('SHADVARGA', 'D3'), ('SHADVARGA', 'D9'), ('SHADVARGA', 'D12'), ('SHADVARGA', 'D30'),
            ('SAPTAVARGA', 'D1'), ('SAPTAVARGA', 'D2'), ('SAPTAVARGA', 'D3'), ('SAPTAVARGA', 'D7'), ('SAPTAVARGA', 'D9'), ('SAPTAVARGA', 'D12'), ('SAPTAVARGA', 'D30'),
            ('DASAVARGA', 'D1'), ('DASAVARGA', 'D2'), ('DASAVARGA', 'D3'), ('DASAVARGA', 'D7'), ('DASAVARGA', 'D9'), ('DASAVARGA', 'D10'), ('DASAVARGA', 'D12'), ('DASAVARGA', 'D16'), ('DASAVARGA', 'D30'), ('DASAVARGA', 'D60'),
            ('SHODASAVARGA', 'D1'), ('SHODASAVARGA', 'D2'), ('SHODASAVARGA', 'D3'), ('SHODASAVARGA', 'D4'), ('SHODASAVARGA', 'D7'), ('SHODASAVARGA', 'D9'), ('SHODASAVARGA', 'D10'), ('SHODASAVARGA', 'D12'), ('SHODASAVARGA', 'D16'), ('SHODASAVARGA', 'D20'), ('SHODASAVARGA', 'D24'), ('SHODASAVARGA', 'D27'), ('SHODASAVARGA', 'D30'), ('SHODASAVARGA', 'D40'), ('SHODASAVARGA', 'D45'), ('SHODASAVARGA', 'D60')
        ) AS v (SchemeCode, ChartType)
    )
    INSERT INTO dbo.tbl_Rule_AmsabalaGroup (RuleSetId, SchemeCode, ChartTypeId, SourceRefCode)
    SELECT 1, src.SchemeCode, ct.Id, 'SRC_PVR_INTEGRATED'
    FROM src
    JOIN dbo.tbl_Dim_ChartType ct ON ct.Code = src.ChartType;

    IF (SELECT COUNT(*) FROM dbo.tbl_Rule_AmsabalaGroup) <> 39
        RAISERROR('099: expected 39 tbl_Rule_AmsabalaGroup rows (6+7+10+16) -- a ChartType code failed to resolve against tbl_Dim_ChartType.', 16, 1);
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_AmsabalaName)
BEGIN
    INSERT INTO dbo.tbl_Rule_AmsabalaName (RuleSetId, SchemeCode, GoodCount, AmsaName, SourceRefCode)
    VALUES
        (1, 'SHADVARGA', 2, 'Kimsukamsa', 'SRC_PVR_INTEGRATED'),
        (1, 'SHADVARGA', 3, 'Vyanjanamsa', 'SRC_PVR_INTEGRATED'),
        (1, 'SHADVARGA', 4, 'Chaamaramsa', 'SRC_PVR_INTEGRATED'),
        (1, 'SHADVARGA', 5, 'Chatramsa', 'SRC_PVR_INTEGRATED'),
        (1, 'SHADVARGA', 6, 'Kundalamsa', 'SRC_PVR_INTEGRATED'),

        (1, 'SAPTAVARGA', 2, 'Kimsukamsa', 'SRC_PVR_INTEGRATED'),
        (1, 'SAPTAVARGA', 3, 'Vyanjanamsa', 'SRC_PVR_INTEGRATED'),
        (1, 'SAPTAVARGA', 4, 'Chaamaramsa', 'SRC_PVR_INTEGRATED'),
        (1, 'SAPTAVARGA', 5, 'Chatramsa', 'SRC_PVR_INTEGRATED'),
        (1, 'SAPTAVARGA', 6, 'Kundalamsa', 'SRC_PVR_INTEGRATED'),
        (1, 'SAPTAVARGA', 7, 'Mukutamsa', 'SRC_PVR_INTEGRATED'),

        (1, 'DASAVARGA', 2, 'Paarijaatamsa', 'SRC_PVR_INTEGRATED'),
        (1, 'DASAVARGA', 3, 'Uttamamsa', 'SRC_PVR_INTEGRATED'),
        (1, 'DASAVARGA', 4, 'Gopuramsa', 'SRC_PVR_INTEGRATED'),
        (1, 'DASAVARGA', 5, 'Simhaasanamsa', 'SRC_PVR_INTEGRATED'),
        (1, 'DASAVARGA', 6, 'Paaraavatamsa', 'SRC_PVR_INTEGRATED'),
        (1, 'DASAVARGA', 7, 'Devalokamsa', 'SRC_PVR_INTEGRATED'),
        (1, 'DASAVARGA', 8, 'Brahmalokamsa', 'SRC_PVR_INTEGRATED'),
        (1, 'DASAVARGA', 9, 'Airaavatamsa', 'SRC_PVR_INTEGRATED'),
        (1, 'DASAVARGA', 10, 'Sreedhaamamsa', 'SRC_PVR_INTEGRATED'),

        (1, 'SHODASAVARGA', 2, 'Bhedakamsa', 'SRC_PVR_INTEGRATED'),
        (1, 'SHODASAVARGA', 3, 'Kusumamsa', 'SRC_PVR_INTEGRATED'),
        (1, 'SHODASAVARGA', 4, 'Nagapurushamsa', 'SRC_PVR_INTEGRATED'),
        (1, 'SHODASAVARGA', 5, 'Kandukamsa', 'SRC_PVR_INTEGRATED'),
        (1, 'SHODASAVARGA', 6, 'Keralamsa', 'SRC_PVR_INTEGRATED'),
        (1, 'SHODASAVARGA', 7, 'Kalpavrikshamsa', 'SRC_PVR_INTEGRATED'),
        (1, 'SHODASAVARGA', 8, 'Chandanavanamsa', 'SRC_PVR_INTEGRATED'),
        (1, 'SHODASAVARGA', 9, 'Poornachandramsa', 'SRC_PVR_INTEGRATED'),
        (1, 'SHODASAVARGA', 10, 'Uchchaisravamsa', 'SRC_PVR_INTEGRATED'),
        (1, 'SHODASAVARGA', 11, 'Dhanvantaryamsa', 'SRC_PVR_INTEGRATED'),
        (1, 'SHODASAVARGA', 12, 'Sooryakaantamsa', 'SRC_PVR_INTEGRATED'),
        (1, 'SHODASAVARGA', 13, 'Vidrumamsa', 'SRC_PVR_INTEGRATED'),
        (1, 'SHODASAVARGA', 14, 'Indraasanamsa', 'SRC_PVR_INTEGRATED'),
        (1, 'SHODASAVARGA', 15, 'Golokamsa', 'SRC_PVR_INTEGRATED'),
        (1, 'SHODASAVARGA', 16, 'SreeVallabhamsa', 'SRC_PVR_INTEGRATED');

    IF (SELECT COUNT(*) FROM dbo.tbl_Rule_AmsabalaName) <> 35
        RAISERROR('099: expected 35 tbl_Rule_AmsabalaName rows (5+6+9+15).', 16, 1);
END
GO

IF OBJECT_ID(N'dbo.tbl_Rule_Catalog', N'U') IS NOT NULL
BEGIN
    MERGE dbo.tbl_Rule_Catalog AS tgt
    USING (VALUES
        ('tbl_Rule_AmsabalaGroup', 'STRENGTH', 'GROUP_LOOKUP', 'PVR Sec. 6.6: which varga chart types belong to each amsabala scheme (shadvarga/saptavarga/dasavarga/shodasavarga).', 'migration 99'),
        ('tbl_Rule_AmsabalaName',  'STRENGTH', 'COUNT_LOOKUP', 'PVR Sec. 6.6: the named amsa (Kimsukamsa..Sree Vallabhamsa) for a given count of good (own/moolatrikona/exalted) placements within a scheme.', 'migration 99')
    ) AS src (RuleTableName, EngineCode, MethodCodes, Purpose, IntroducedIn)
    ON tgt.RuleTableName = src.RuleTableName
    WHEN NOT MATCHED BY TARGET THEN
        INSERT (RuleTableName, EngineCode, MethodCodes, Purpose, IntroducedIn)
        VALUES (src.RuleTableName, src.EngineCode, src.MethodCodes, src.Purpose, src.IntroducedIn);
END
GO

IF OBJECT_ID(N'dbo.SchemaMigrations', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = N'099_create_rule_amsabala_scheme.sql')
    INSERT dbo.SchemaMigrations (ScriptName, Note)
    VALUES (N'099_create_rule_amsabala_scheme.sql', N'Amsabala reconciliation (PVR Sec. 6.6): tbl_Rule_AmsabalaGroup (39 rows) + tbl_Rule_AmsabalaName (35 rows). Prerequisite for Vimsopaka Bala, which stays unseeded (weight table not given by PVR).');
GO

PRINT '099 applied: tbl_Rule_AmsabalaGroup (39 rows) + tbl_Rule_AmsabalaName (35 rows) ready.';
GO
