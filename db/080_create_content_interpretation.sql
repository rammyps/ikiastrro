-- =====================================================================
-- 080 — Standard/short reader-facing interpretation content.
--
-- Decision: decisions/003-rules-audit-content-model-ephemeris-interpreter.md
-- Part B. Nothing like this existed before: tbl_Rule_Yoga.ShortFormationRule
-- (db/079) is a *trigger* rule ("what fires this yoga"), not *interpretation*
-- ("what it means for the person") — different content, and no interpretation
-- copy (long or short) existed anywhere in the schema. CalculationNarrative
-- elsewhere in the rule tables is developer-facing provenance prose, not
-- reader copy.
--
-- (SubjectType, SubjectCode) is a generic key so one table serves every
-- domain (yoga, dignity, house, graha-in-sign, ...) without a new table per
-- domain. StandardText is the full reader-facing paragraph (desktop);
-- ShortText is the compact form for tablet/mobile, badges and list rows —
-- see src/Ikiastrro.Web/Components/Shared/InterpretationText.razor.
--
-- No seed rows in this migration: actual copy authoring is a separate
-- content-writing pass (first target: SubjectType='YOGA' rows for the 146
-- already-grounded YogaCodes from db/079). Rows without real copy stay
-- unseeded — the project's "deliberately NULL pending a cited source"
-- discipline, same as every other rule/content table.
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID('dbo.tbl_Content_Interpretation', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Content_Interpretation (
        Id            INT IDENTITY(1,1) CONSTRAINT PK_Content_Interpretation PRIMARY KEY,
        RuleSetId     TINYINT NOT NULL CONSTRAINT FK_Content_Interpretation_RuleSet REFERENCES dbo.tbl_Rule_Sets (Id),
        SubjectType   VARCHAR(30)   NOT NULL, -- 'YOGA' | 'DIGNITY' | 'HOUSE' | 'GRAHA_IN_SIGN' | ...
        SubjectCode   VARCHAR(60)   NOT NULL, -- e.g. 'YOGA_GAJAKESARI', 'EXALTED', 'HOUSE_1'
        StandardText  NVARCHAR(500) NOT NULL, -- full reader-facing paragraph (desktop)
        ShortText     NVARCHAR(160) NOT NULL, -- compact version (tablet/mobile, badges, list rows)
        SourceRefCode VARCHAR(40)   NULL,
        IsActive      BIT NOT NULL CONSTRAINT DF_Content_Interpretation_IsActive DEFAULT (1),
        CONSTRAINT UQ_Content_Interpretation UNIQUE (RuleSetId, SubjectType, SubjectCode),
        CONSTRAINT CK_Content_Interpretation_Src CHECK (SourceRefCode IS NULL OR SourceRefCode LIKE 'SRC[_]%')
    );
END
GO

INSERT dbo.SchemaMigrations (ScriptName, Note)
SELECT '080_create_content_interpretation.sql',
       'tbl_Content_Interpretation created: generic (SubjectType, SubjectCode) key for standard/short reader-facing interpretation copy. No rows seeded — copy authoring is a separate pass.'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '080_create_content_interpretation.sql');
GO

PRINT '080 applied: tbl_Content_Interpretation created (unseeded).';
GO
