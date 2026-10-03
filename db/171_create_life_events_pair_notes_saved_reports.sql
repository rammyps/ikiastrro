-- 171 - Life events, pair notes and saved pair reports (compatibility phase 6).
--
-- tbl_Person_LifeEvent: dated events in one person's life (marriage, divorce, child birth, death). It
-- enables the dasha running at a marriage or a birth, and is the ground-truth table for later validation.
-- A marriage is stored once per spouse, each row naming the other as RelatedPersonId.
--
-- tbl_Pair_Note: free-text notes the user keeps about a pair. Stored once per pair (lower person id first).
-- It is the user's own writing, not a derived value, so it cannot go stale.
--
-- tbl_Pair_SavedReport: a snapshot the user chose to save, for a pair with a recorded relationship only
-- (the repository checks vw_PersonFamily). It keeps the text as it was shown, the Kuta total and the
-- rule-set version, so it stays a dated record rather than a value that silently changes when the rules do
-- (docs/architecture/compatibility_similarity.md, section 4.2). Arbitrary pairs are never stored.
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID('dbo.tbl_Person_LifeEvent', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Person_LifeEvent (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Person_LifeEvent PRIMARY KEY,
        PersonId INT NOT NULL CONSTRAINT FK_Person_LifeEvent_Person FOREIGN KEY REFERENCES dbo.tbl_BirthDetails (Id),
        EventType VARCHAR(20) NOT NULL,
        EventDate DATE NOT NULL,
        Place NVARCHAR(200) NULL,
        RelatedPersonId INT NULL CONSTRAINT FK_Person_LifeEvent_Related FOREIGN KEY REFERENCES dbo.tbl_BirthDetails (Id),
        Notes NVARCHAR(300) NULL,
        CreatedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_Person_LifeEvent_Created DEFAULT SYSUTCDATETIME(),
        CONSTRAINT CK_Person_LifeEvent_Type CHECK (EventType IN ('MARRIAGE', 'DIVORCE', 'CHILD_BIRTH', 'DEATH')),
        CONSTRAINT CK_Person_LifeEvent_Related CHECK (RelatedPersonId IS NULL OR RelatedPersonId <> PersonId)
    );
    CREATE INDEX IX_Person_LifeEvent_Person ON dbo.tbl_Person_LifeEvent (PersonId, EventType);
END
GO

IF OBJECT_ID('dbo.tbl_Pair_Note', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Pair_Note (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Pair_Note PRIMARY KEY,
        PersonAId INT NOT NULL CONSTRAINT FK_Pair_Note_A FOREIGN KEY REFERENCES dbo.tbl_BirthDetails (Id),
        PersonBId INT NOT NULL CONSTRAINT FK_Pair_Note_B FOREIGN KEY REFERENCES dbo.tbl_BirthDetails (Id),
        NoteText NVARCHAR(1000) NOT NULL,
        CreatedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_Pair_Note_Created DEFAULT SYSUTCDATETIME(),
        CONSTRAINT CK_Pair_Note_Order CHECK (PersonAId < PersonBId),
        CONSTRAINT CK_Pair_Note_Text CHECK (LEN(LTRIM(RTRIM(NoteText))) > 0)
    );
    CREATE INDEX IX_Pair_Note_Pair ON dbo.tbl_Pair_Note (PersonAId, PersonBId);
END
GO

IF OBJECT_ID('dbo.tbl_Pair_SavedReport', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Pair_SavedReport (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Pair_SavedReport PRIMARY KEY,
        GroomId INT NOT NULL CONSTRAINT FK_Pair_SavedReport_Groom FOREIGN KEY REFERENCES dbo.tbl_BirthDetails (Id),
        BrideId INT NOT NULL CONSTRAINT FK_Pair_SavedReport_Bride FOREIGN KEY REFERENCES dbo.tbl_BirthDetails (Id),
        RuleSetVersion VARCHAR(20) NOT NULL,
        KutaScore DECIMAL(5,1) NULL,
        KutaMax DECIMAL(5,1) NULL,
        SummaryText NVARCHAR(MAX) NOT NULL,
        SavedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_Pair_SavedReport_Saved DEFAULT SYSUTCDATETIME(),
        CONSTRAINT CK_Pair_SavedReport_Distinct CHECK (GroomId <> BrideId)
    );
    CREATE INDEX IX_Pair_SavedReport_Pair ON dbo.tbl_Pair_SavedReport (GroomId, BrideId);
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '171_create_life_events_pair_notes_saved_reports.sql')
    INSERT dbo.SchemaMigrations (ScriptName, AppliedAtUtc, Note)
    VALUES ('171_create_life_events_pair_notes_saved_reports.sql', SYSUTCDATETIME(),
            'Adds tbl_Person_LifeEvent, tbl_Pair_Note and tbl_Pair_SavedReport (compatibility phase 6).');

COMMIT TRANSACTION;
GO

PRINT '171 complete.';
