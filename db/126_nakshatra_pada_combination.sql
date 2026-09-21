-- =====================================================================
-- 126 -- tbl_Rule_NakshatraPadaCombination: pada-specific "further
-- analysis" (rammyps, 2026-09-19), the 108-row pada-grain sibling of
-- tbl_Rule_RasiNakshatraCombination (migration 124, 36-row Rasi x
-- Nakshatra grain).
--
-- Why templated, not freehand: migration 124's synthesis text was
-- transcribed from a doc rammyps had already drafted and reviewed
-- (rasi-nakshatra-36-combination-matrix.md). No equivalent doc exists at
-- this pada grain, and 108 rows is 3x that table's size -- hand-writing
-- 108 independent narratives in one pass risks drifting in quality and
-- consistency partway through. Instead this migration generates
-- PadaCharacterModifier / PadaJudgmentNote via a documented SQL CASE
-- template driven entirely by already-computed, already-verified inputs
-- (vw_Rule_NakshatraPadaLordConnection's RasiLord_PadaLord_Relation /
-- NakshatraLord_PadaLord_Relation / same-lord flags, migration 125) --
-- mechanical and reproducible, not ad hoc. Still tagged
-- SRC_IKIASTRRO_SYNTHESIS (migration 087's project-synthesis code) since
-- the template's wording choices are project-authored, not a book
-- citation.
--
-- CombinedCharacter / MainSignifications / PotentialBenefits /
-- PotentialDisadvantages are NOT duplicated onto this table -- they
-- already exist per (RasiId, NakshatraId) on tbl_Rule_RasiNakshatraCombination
-- (migration 124); every pada within one Rasi-Nakshatra combination
-- inherits its parent row's reading, refined by this table's two new
-- pada-specific columns. vw_Rule_NakshatraPadaAnalysis (below) joins
-- both tables plus vw_Rule_NakshatraPadaLordConnection into one queryable
-- row per pada.
--
-- Apply:  sqlcmd -S "localhost\SQLSERVER2025" -E -d ikiastrro -b -i db/126_nakshatra_pada_combination.sql
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID('dbo.tbl_Rule_NakshatraPadaCombination', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Rule_NakshatraPadaCombination (
        Id                     INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Rule_NakshatraPadaCombination PRIMARY KEY,
        RuleSetId              TINYINT       NOT NULL CONSTRAINT FK_Rule_NakshatraPadaCombination_RuleSet FOREIGN KEY REFERENCES dbo.tbl_Rule_Sets (Id),
        NakshatraPadaId        INT           NOT NULL CONSTRAINT FK_Rule_NakshatraPadaCombination_Pada    FOREIGN KEY REFERENCES dbo.tbl_NakshatraPadas (Id),
        PadaCharacterModifier  NVARCHAR(300) NOT NULL,
        PadaJudgmentNote       NVARCHAR(400) NOT NULL,
        MethodCode             VARCHAR(30)   NULL,
        SourceRefCode          VARCHAR(40)   NULL CONSTRAINT CK_Rule_NakshatraPadaCombination_Src CHECK (SourceRefCode IS NULL OR SourceRefCode LIKE 'SRC[_]%'),
        IsActive               BIT           NOT NULL CONSTRAINT DF_Rule_NakshatraPadaCombination_IsActive DEFAULT 1,
        CONSTRAINT UQ_Rule_NakshatraPadaCombination UNIQUE (RuleSetId, NakshatraPadaId)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_NakshatraPadaCombination)
BEGIN
    INSERT dbo.tbl_Rule_NakshatraPadaCombination
        (RuleSetId, NakshatraPadaId, PadaCharacterModifier, PadaJudgmentNote, MethodCode, SourceRefCode)
    SELECT
        1,
        c.NakshatraPadaId,
        N'Pada lord ' + c.PadaLordName + N': ' +
            CASE c.RasiLord_PadaLord_Relation
                WHEN 'Same'    THEN N'echoes the sign lord itself, concentrating the rasi''s own agenda through this pada'
                WHEN 'Friend'  THEN N'supports the sign lord''s agenda'
                WHEN 'Neutral' THEN N'neither reinforces nor resists the sign lord''s agenda'
                WHEN 'Enemy'   THEN N'works against the sign lord''s natural agenda, adding internal friction'
            END +
            CASE c.NakshatraLord_PadaLord_Relation
                WHEN 'Node'    THEN N'; the nakshatra''s own lord is a node with no classical friendship rating here, so judge it on its own placement'
                WHEN 'Same'    THEN N', and matches the nakshatra lord exactly, doubling that signature in this pada'
                WHEN 'Friend'  THEN N', and cooperates with the nakshatra lord'
                WHEN 'Neutral' THEN N', and sits neutrally toward the nakshatra lord'
                WHEN 'Enemy'   THEN N', but clashes with the nakshatra lord, splitting this pada''s expression'
            END + N'.'
            AS PadaCharacterModifier,
        N'Chain ' + c.NaturalConnectionCode + N' (Rasi ' + c.RasiLordName + N' - Nakshatra ' + c.NakshatraLordName + N' - Pada ' + c.PadaLordName + N'): ' +
            CASE
                WHEN c.PadaLordPlanetId = c.RasiLordPlanetId AND c.PadaLordPlanetId = c.NakshatraLordPlanetId
                    THEN N'all three layers share one dispositor -- read that single planet''s house, sign and dignity as the entire operative signal for this pada.'
                WHEN c.PadaLordPlanetId = c.RasiLordPlanetId
                    THEN N'pada lord repeats the sign lord -- this pada intensifies the rasi''s baseline rather than adding a new layer; the nakshatra lord still supplies its own separate signature.'
                WHEN c.PadaLordPlanetId = c.NakshatraLordPlanetId
                    THEN N'pada lord repeats the nakshatra lord -- the D9 layer reinforces the nakshatra''s own theme rather than diverging from it; the sign lord remains the separate structural layer.'
                ELSE N'three distinct dispositors are in play -- weigh each by house, sign and dignity separately before combining; do not assume one subsumes another.'
            END
            AS PadaJudgmentNote,
        'NAKSHATRA_PADA_COMBINATION',
        'SRC_IKIASTRRO_SYNTHESIS'
    FROM dbo.vw_Rule_NakshatraPadaLordConnection c;

    IF (SELECT COUNT(*) FROM dbo.tbl_Rule_NakshatraPadaCombination) <> 108
        RAISERROR('126: expected exactly 108 tbl_Rule_NakshatraPadaCombination rows.', 16, 1);
END
GO

CREATE OR ALTER VIEW dbo.vw_Rule_NakshatraPadaAnalysis
AS
SELECT
    conn.NakshatraPadaId,
    conn.RasiId, conn.RasiName, conn.RasiLordPlanetId, conn.RasiLordName,
    conn.NakshatraId, conn.NakshatraName, conn.NakshatraLordPlanetId, conn.NakshatraLordName,
    conn.PadaNumber, conn.NavamsaSignId, conn.PadaLordPlanetId, conn.PadaLordName, conn.PadaSubLordPlanetId,
    conn.NaturalConnectionCode,
    conn.RasiLord_NakshatraLord_Relation,
    conn.RasiLord_PadaLord_Relation,
    conn.NakshatraLord_PadaLord_Relation,
    parent.SpanStartDegree, parent.SpanEndDegree,
    parent.CombinedCharacter, parent.MainSignifications, parent.PotentialBenefits, parent.PotentialDisadvantages, parent.JudgmentNote AS CombinationJudgmentNote,
    parent.AspectingRasis,
    pada.PadaCharacterModifier,
    pada.PadaJudgmentNote
FROM dbo.vw_Rule_NakshatraPadaLordConnection conn
JOIN dbo.tbl_Rule_RasiNakshatraCombination parent
     ON parent.RuleSetId = 1 AND parent.RasiId = conn.RasiId AND parent.NakshatraId = conn.NakshatraId
JOIN dbo.tbl_Rule_NakshatraPadaCombination pada
     ON pada.RuleSetId = 1 AND pada.NakshatraPadaId = conn.NakshatraPadaId;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_Catalog WHERE RuleTableName = 'tbl_Rule_NakshatraPadaCombination')
    INSERT dbo.tbl_Rule_Catalog (RuleTableName, EngineCode, MethodCodes, Purpose, IntroducedIn)
    VALUES ('tbl_Rule_NakshatraPadaCombination', 'RELATIONSHIP', 'NAKSHATRA_PADA_COMBINATION',
            'Pada-grain (108 rows) further analysis: PadaCharacterModifier/PadaJudgmentNote generated by a documented CASE template off the Rasi/Nakshatra/Pada-lord 3-way relations (migration 125). SRC_IKIASTRRO_SYNTHESIS. vw_Rule_NakshatraPadaAnalysis joins this to the parent 36-row combination table.',
            '126_nakshatra_pada_combination.sql');
GO

INSERT dbo.SchemaMigrations (ScriptName, Note)
SELECT '126_nakshatra_pada_combination.sql',
       'tbl_Rule_NakshatraPadaCombination (108 rows, templated synthesis); vw_Rule_NakshatraPadaAnalysis (full joined pada-detail view); 1 tbl_Rule_Catalog row'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '126_nakshatra_pada_combination.sql');
GO

PRINT '126 applied: tbl_Rule_NakshatraPadaCombination seeded (108 rows); vw_Rule_NakshatraPadaAnalysis ready.';
GO
