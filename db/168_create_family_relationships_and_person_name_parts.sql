-- 168 - Family relationships, first/last name parts, and stored name numbers on tbl_BirthDetails.
--
-- Family: direct edges only (SPOUSE, PARENT_OF). Siblings, grandparents and in-laws are derived by
-- vw_PersonFamily from those edges, so tiers never go stale (docs/architecture/compatibility_similarity.md,
-- L4). No relationship rows are seeded here: who is related to whom is the user's own data.
--
-- Name parts: tbl_BirthDetails.Name stays the one unique, displayed name. FirstName/LastName are added
-- beside it. Existing rows whose Name is one run of letters ending in a single capital initial
-- (RamakrishnanP, AnanyaR ...) are split on that initial; any other row is left NULL.
--
-- Name numbers: Cheiro's compound and root name number are stored with the person, written by
-- BirthDetailsRepository on every insert and update (the calculation itself stays in
-- Ikiastrro.Core.Numerology.CheiroNumerology). Existing rows are filled by `backfill-numerology`.
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF COL_LENGTH('dbo.tbl_BirthDetails', 'FirstName') IS NULL
    ALTER TABLE dbo.tbl_BirthDetails ADD FirstName NVARCHAR(100) NULL;
IF COL_LENGTH('dbo.tbl_BirthDetails', 'LastName') IS NULL
    ALTER TABLE dbo.tbl_BirthDetails ADD LastName NVARCHAR(100) NULL;
IF COL_LENGTH('dbo.tbl_BirthDetails', 'NameNumberCompound') IS NULL
    ALTER TABLE dbo.tbl_BirthDetails ADD NameNumberCompound TINYINT NULL;
IF COL_LENGTH('dbo.tbl_BirthDetails', 'NameNumberRoot') IS NULL
    ALTER TABLE dbo.tbl_BirthDetails ADD NameNumberRoot TINYINT NULL;
GO

UPDATE dbo.tbl_BirthDetails
SET FirstName = LEFT(Name, LEN(Name) - 1),
    LastName  = RIGHT(Name, 1)
WHERE FirstName IS NULL AND LastName IS NULL
  AND LEN(Name) >= 3
  AND Name NOT LIKE '% %'
  AND RIGHT(Name, 1) COLLATE Latin1_General_BIN LIKE '[A-Z]'
  AND SUBSTRING(Name, LEN(Name) - 1, 1) COLLATE Latin1_General_BIN LIKE '[a-z]';
GO

IF OBJECT_ID('dbo.tbl_Dim_RelationType', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Dim_RelationType (
        Code VARCHAR(20) NOT NULL CONSTRAINT PK_Dim_RelationType PRIMARY KEY,
        Name NVARCHAR(40) NOT NULL,
        Tier VARCHAR(10) NOT NULL,
        IsSymmetric BIT NOT NULL,
        Notes NVARCHAR(300) NULL,
        CONSTRAINT CK_Dim_RelationType_Tier CHECK (Tier IN ('Core', 'Extended'))
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Dim_RelationType WHERE Code = 'SPOUSE')
    INSERT dbo.tbl_Dim_RelationType (Code, Name, Tier, IsSymmetric, Notes) VALUES
    ('SPOUSE',    N'Spouse',        'Core', 1, N'Husband and wife. Stored once, lower person id first.'),
    ('PARENT_OF', N'Parent of',     'Core', 0, N'PersonA is a parent of PersonB. Use IsAdopted / IsStep for adoptive and step parents.');
GO

IF OBJECT_ID('dbo.tbl_Person_Relationship', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Person_Relationship (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Person_Relationship PRIMARY KEY,
        PersonAId INT NOT NULL CONSTRAINT FK_Person_Relationship_A FOREIGN KEY REFERENCES dbo.tbl_BirthDetails (Id),
        PersonBId INT NOT NULL CONSTRAINT FK_Person_Relationship_B FOREIGN KEY REFERENCES dbo.tbl_BirthDetails (Id),
        RelationTypeCode VARCHAR(20) NOT NULL CONSTRAINT FK_Person_Relationship_Type FOREIGN KEY REFERENCES dbo.tbl_Dim_RelationType (Code),
        IsAdopted BIT NOT NULL CONSTRAINT DF_Person_Relationship_Adopted DEFAULT 0,
        IsStep BIT NOT NULL CONSTRAINT DF_Person_Relationship_Step DEFAULT 0,
        StartDate DATE NULL,
        EndDate DATE NULL,
        Status VARCHAR(10) NOT NULL CONSTRAINT DF_Person_Relationship_Status DEFAULT 'Current',
        Notes NVARCHAR(300) NULL,
        CreatedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_Person_Relationship_Created DEFAULT SYSUTCDATETIME(),
        CONSTRAINT CK_Person_Relationship_Distinct CHECK (PersonAId <> PersonBId),
        CONSTRAINT CK_Person_Relationship_Status CHECK (Status IN ('Current', 'Ended')),
        CONSTRAINT CK_Person_Relationship_SpouseOrder CHECK (RelationTypeCode <> 'SPOUSE' OR PersonAId < PersonBId),
        CONSTRAINT UQ_Person_Relationship UNIQUE (PersonAId, PersonBId, RelationTypeCode)
    );
    CREATE INDEX IX_Person_Relationship_B ON dbo.tbl_Person_Relationship (PersonBId);
END
GO

-- One row per (person, relative): the relative's role as seen from the person. Direct edges give the
-- core family (husband, wife, father, mother, son, daughter); siblings are derived from a shared parent.
CREATE OR ALTER VIEW dbo.vw_PersonFamily
AS
WITH edges AS (
    SELECT r.PersonAId, r.PersonBId, r.RelationTypeCode, r.IsAdopted, r.IsStep, r.Status
    FROM dbo.tbl_Person_Relationship r
),
directed AS (
    -- SPOUSE: each partner sees the other as husband / wife / spouse.
    SELECT e.PersonAId AS PersonId, e.PersonBId AS RelativeId,
           CASE b.Sex WHEN 'Male' THEN 'Husband' WHEN 'Female' THEN 'Wife' ELSE 'Spouse' END AS Role,
           'Core' AS Tier, e.IsAdopted, e.IsStep, e.Status
    FROM edges e JOIN dbo.tbl_BirthDetails b ON b.Id = e.PersonBId WHERE e.RelationTypeCode = 'SPOUSE'
    UNION ALL
    SELECT e.PersonBId, e.PersonAId,
           CASE a.Sex WHEN 'Male' THEN 'Husband' WHEN 'Female' THEN 'Wife' ELSE 'Spouse' END,
           'Core', e.IsAdopted, e.IsStep, e.Status
    FROM edges e JOIN dbo.tbl_BirthDetails a ON a.Id = e.PersonAId WHERE e.RelationTypeCode = 'SPOUSE'
    UNION ALL
    -- PARENT_OF: the parent sees a son / daughter / child; the child sees a father / mother / parent.
    SELECT e.PersonAId, e.PersonBId,
           CASE b.Sex WHEN 'Male' THEN 'Son' WHEN 'Female' THEN 'Daughter' ELSE 'Child' END,
           'Core', e.IsAdopted, e.IsStep, e.Status
    FROM edges e JOIN dbo.tbl_BirthDetails b ON b.Id = e.PersonBId WHERE e.RelationTypeCode = 'PARENT_OF'
    UNION ALL
    SELECT e.PersonBId, e.PersonAId,
           CASE a.Sex WHEN 'Male' THEN 'Father' WHEN 'Female' THEN 'Mother' ELSE 'Parent' END,
           'Core', e.IsAdopted, e.IsStep, e.Status
    FROM edges e JOIN dbo.tbl_BirthDetails a ON a.Id = e.PersonAId WHERE e.RelationTypeCode = 'PARENT_OF'
),
siblings AS (
    SELECT DISTINCT p1.PersonBId AS PersonId, p2.PersonBId AS RelativeId,
           CASE s.Sex WHEN 'Male' THEN 'Brother' WHEN 'Female' THEN 'Sister' ELSE 'Sibling' END AS Role,
           'Extended' AS Tier, CAST(0 AS BIT) AS IsAdopted, CAST(0 AS BIT) AS IsStep, 'Current' AS Status
    FROM edges p1
    JOIN edges p2 ON p2.PersonAId = p1.PersonAId AND p2.RelationTypeCode = 'PARENT_OF' AND p2.PersonBId <> p1.PersonBId
    JOIN dbo.tbl_BirthDetails s ON s.Id = p2.PersonBId
    WHERE p1.RelationTypeCode = 'PARENT_OF'
)
SELECT d.PersonId, d.RelativeId, rel.Name AS RelativeName, rel.Sex AS RelativeSex,
       d.Role, d.Tier, d.IsAdopted, d.IsStep, d.Status
FROM (SELECT * FROM directed UNION ALL SELECT * FROM siblings) d
JOIN dbo.tbl_BirthDetails rel ON rel.Id = d.RelativeId;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '168_create_family_relationships_and_person_name_parts.sql')
    INSERT dbo.SchemaMigrations (ScriptName, AppliedAtUtc, Note)
    VALUES ('168_create_family_relationships_and_person_name_parts.sql', SYSUTCDATETIME(),
            'Adds tbl_Dim_RelationType, tbl_Person_Relationship, vw_PersonFamily; FirstName/LastName and NameNumberCompound/Root on tbl_BirthDetails.');

COMMIT TRANSACTION;
GO

DECLARE @split INT = (SELECT COUNT(*) FROM dbo.tbl_BirthDetails WHERE FirstName IS NOT NULL);
DECLARE @total INT = (SELECT COUNT(*) FROM dbo.tbl_BirthDetails);
PRINT '168 complete: names split=' + CAST(@split AS VARCHAR(10)) + ' of ' + CAST(@total AS VARCHAR(10));
