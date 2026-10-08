-- 177 - NUMEROFACTS: candidate names (business names, brands, suppliers, products) kept for Cheiro name-number comparison.
--
-- The name is the only input; compound and root are stored beside it, written by NameOptionRepository on every
-- insert (the calculation stays in Ikiastrro.Core.Numerology.CheiroNumerology). Names are never removed by the app
-- except by an explicit Remove from the user. Unique on the trimmed name, case-insensitive.
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID('dbo.tbl_NameOption', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_NameOption
    (
        Id               INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_tbl_NameOption PRIMARY KEY,
        Name             NVARCHAR(200) NOT NULL,
        Purpose          NVARCHAR(60)  NOT NULL CONSTRAINT DF_tbl_NameOption_Purpose DEFAULT ('Business'),
        Note             NVARCHAR(400) NULL,
        CompoundNumber   SMALLINT      NOT NULL,
        RootNumber       TINYINT       NOT NULL,
        CreatedAt        DATETIME2(0)  NOT NULL CONSTRAINT DF_tbl_NameOption_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT UQ_tbl_NameOption_Name UNIQUE (Name)
    );
END;

IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '177_create_name_options.sql')
    INSERT dbo.SchemaMigrations (ScriptName, Note)
    VALUES ('177_create_name_options.sql', 'Adds tbl_NameOption for the NUMEROFACTS name-comparison page.');

COMMIT TRANSACTION;
GO
PRINT '177 applied: tbl_NameOption.';
GO
