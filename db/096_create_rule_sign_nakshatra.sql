-- =====================================================================
-- tbl_Rule_SignNakshatra: rasi + nakshatra + KP sub-lord chain levels 1-2,
-- per rammyps's explicit name/shape call on 2026-09-13.
--
-- Deliberate deviation from the tbl_Rule_* contract (docs/database/rules-engine.md
-- STANDARDS.md SS D.1): no RuleSetId, no portability tail (MethodCode/
-- RuleParametersJson/CalculationNarrative/SourceRefCode/IsActive), not registered in
-- tbl_Rule_Catalog. This content is fixed astronomical vocabulary (nakshatra/sub-lord
-- degree boundaries and lord assignments already seeded in tbl_Nakshatras /
-- tbl_NakshatraSubLords, migration db/_archive/021), not a versioned classical rule with
-- a source-provenance dispute to track -- rammyps chose to keep the tbl_Rule_ name anyway
-- rather than tbl_Dim_SignNakshatra, so this header records the departure for anyone
-- auditing against rules-engine.md later.
--
-- Levels 3-7 are deliberately NOT here -- per tbl_Fact_KpSubLordChain (migration 095),
-- those bands are only meaningful at an exact planet degree (narrowest level-7 band is
-- ~1e-8 degrees) and are computed on demand by AstroMath.GetKpSubLordChain, not stored as
-- a boundary table.
--
-- Populated by SELECT off tbl_Nakshatras join tbl_NakshatraSubLords, never hand-retyped,
-- so it can't drift from that seed data. Grain: one row per (NakshatraId,
-- SubSequenceNumber) = 243 rows, same grain as tbl_NakshatraSubLords. RasiId is each
-- nakshatra's PrimaryRasiId (its midpoint sign, migration 033) -- note 9 of the 27
-- nakshatras straddle a sign boundary (tbl_Nakshatras.StraddlesSignBoundary = 1), so this
-- RasiId is the nakshatra's primary sign, not a per-sub-lord-segment sign; a sub-lord
-- segment near either end of a straddling nakshatra can fall in the neighboring sign.
-- =====================================================================
USE [ikiastrro];
GO

IF OBJECT_ID(N'dbo.tbl_Rule_SignNakshatra', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Rule_SignNakshatra
    (
        Id                    INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Rule_SignNakshatra PRIMARY KEY,
        RasiId                TINYINT      NOT NULL CONSTRAINT FK_Rule_SignNakshatra_Rasi      FOREIGN KEY REFERENCES dbo.tbl_SignAttributes (Id),
        NakshatraId           TINYINT      NOT NULL CONSTRAINT FK_Rule_SignNakshatra_Nakshatra  FOREIGN KEY REFERENCES dbo.tbl_Nakshatras (Id),
        NakshatraLordPlanetId TINYINT      NOT NULL CONSTRAINT FK_Rule_SignNakshatra_L1Lord      FOREIGN KEY REFERENCES dbo.tbl_Planets (Id),  -- KP level 1
        SubSequenceNumber     TINYINT      NOT NULL,                                                                                            -- 1-9, order within the nakshatra
        SubLordPlanetId       TINYINT      NOT NULL CONSTRAINT FK_Rule_SignNakshatra_L2Lord      FOREIGN KEY REFERENCES dbo.tbl_Planets (Id),  -- KP level 2
        SubLordStartDegree    DECIMAL(9,6) NOT NULL,
        SubLordEndDegree      DECIMAL(9,6) NOT NULL,
        CONSTRAINT UQ_Rule_SignNakshatra UNIQUE (NakshatraId, SubSequenceNumber),
        CONSTRAINT CK_Rule_SignNakshatra_SubSeq CHECK (SubSequenceNumber BETWEEN 1 AND 9)
    );

    CREATE INDEX IX_Rule_SignNakshatra_Rasi ON dbo.tbl_Rule_SignNakshatra (RasiId);
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_SignNakshatra)
BEGIN
    INSERT INTO dbo.tbl_Rule_SignNakshatra
        (RasiId, NakshatraId, NakshatraLordPlanetId, SubSequenceNumber, SubLordPlanetId, SubLordStartDegree, SubLordEndDegree)
    SELECT
        nak.PrimaryRasiId,
        nak.Id,
        nak.RulingPlanetId,
        sub.SubSequenceNumber,
        sub.SubLordId,
        sub.StartDegree,
        sub.EndDegree
    FROM dbo.tbl_Nakshatras nak
    JOIN dbo.tbl_NakshatraSubLords sub ON sub.NakshatraId = nak.Id;
END
GO

IF OBJECT_ID(N'dbo.SchemaMigrations', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = N'096_create_rule_sign_nakshatra.sql')
    INSERT dbo.SchemaMigrations (ScriptName, Note)
    VALUES (N'096_create_rule_sign_nakshatra.sql', N'Add tbl_Rule_SignNakshatra (rasi+nakshatra+KP sub-lord L1-2, 243 rows). Outside the tbl_Rule_* portability/RuleSetId/Catalog contract -- see header.');
GO

PRINT '096 applied: tbl_Rule_SignNakshatra ready (243 rows: rasi + nakshatra + KP sub-lord levels 1-2).';
GO
