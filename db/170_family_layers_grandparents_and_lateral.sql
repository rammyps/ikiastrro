-- 170 - Family layers: grandparents are Extended, siblings become their own Lateral layer.
--
-- The layers are relative to the person being viewed (vw_PersonFamily is one row per person and relative):
--   Core      spouse, parents, children
--   Extended  grandparents (paternal or maternal, by the side of the parent they come through) and grandchildren
--   Lateral   siblings (later: uncles and aunts, cousins, in-laws)
-- Only direct edges (SPOUSE, PARENT_OF) are stored; grandparents, grandchildren and siblings are derived from
-- them, so a layer can never go stale (docs/architecture/compatibility_similarity.md, L4).
-- Siblings were 'Extended' in 168; they are 'Lateral' from here. A grandparent's side is read from the sex of
-- the parent the grandparent is related through; with no recorded sex the side is left off ("Grandfather").
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Dim_RelationType_Tier')
    ALTER TABLE dbo.tbl_Dim_RelationType DROP CONSTRAINT CK_Dim_RelationType_Tier;
ALTER TABLE dbo.tbl_Dim_RelationType
    ADD CONSTRAINT CK_Dim_RelationType_Tier CHECK (Tier IN ('Core', 'Extended', 'Lateral'));
GO

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
grandparents AS (
    -- g: G is a parent of P. p: P is a parent of X. X sees G as a grandparent, on the side of P.
    SELECT p.PersonBId AS PersonId, g.PersonAId AS RelativeId,
           CASE par.Sex WHEN 'Male' THEN 'Paternal ' WHEN 'Female' THEN 'Maternal ' ELSE '' END
             + CASE gp.Sex WHEN 'Male' THEN 'Grandfather' WHEN 'Female' THEN 'Grandmother' ELSE 'Grandparent' END AS Role,
           'Extended' AS Tier,
           CAST(CASE WHEN p.IsAdopted = 1 OR g.IsAdopted = 1 THEN 1 ELSE 0 END AS BIT) AS IsAdopted,
           CAST(CASE WHEN p.IsStep = 1 OR g.IsStep = 1 THEN 1 ELSE 0 END AS BIT) AS IsStep,
           'Current' AS Status
    FROM edges p
    JOIN edges g ON g.PersonBId = p.PersonAId AND g.RelationTypeCode = 'PARENT_OF'
    JOIN dbo.tbl_BirthDetails par ON par.Id = p.PersonAId
    JOIN dbo.tbl_BirthDetails gp ON gp.Id = g.PersonAId
    WHERE p.RelationTypeCode = 'PARENT_OF'
),
grandchildren AS (
    SELECT g.PersonAId AS PersonId, p.PersonBId AS RelativeId,
           CASE c.Sex WHEN 'Male' THEN 'Grandson' WHEN 'Female' THEN 'Granddaughter' ELSE 'Grandchild' END AS Role,
           'Extended' AS Tier,
           CAST(CASE WHEN p.IsAdopted = 1 OR g.IsAdopted = 1 THEN 1 ELSE 0 END AS BIT) AS IsAdopted,
           CAST(CASE WHEN p.IsStep = 1 OR g.IsStep = 1 THEN 1 ELSE 0 END AS BIT) AS IsStep,
           'Current' AS Status
    FROM edges p
    JOIN edges g ON g.PersonBId = p.PersonAId AND g.RelationTypeCode = 'PARENT_OF'
    JOIN dbo.tbl_BirthDetails c ON c.Id = p.PersonBId
    WHERE p.RelationTypeCode = 'PARENT_OF'
),
siblings AS (
    SELECT DISTINCT p1.PersonBId AS PersonId, p2.PersonBId AS RelativeId,
           CASE s.Sex WHEN 'Male' THEN 'Brother' WHEN 'Female' THEN 'Sister' ELSE 'Sibling' END AS Role,
           'Lateral' AS Tier, CAST(0 AS BIT) AS IsAdopted, CAST(0 AS BIT) AS IsStep, 'Current' AS Status
    FROM edges p1
    JOIN edges p2 ON p2.PersonAId = p1.PersonAId AND p2.RelationTypeCode = 'PARENT_OF' AND p2.PersonBId <> p1.PersonBId
    JOIN dbo.tbl_BirthDetails s ON s.Id = p2.PersonBId
    WHERE p1.RelationTypeCode = 'PARENT_OF'
)
SELECT d.PersonId, d.RelativeId, rel.Name AS RelativeName, rel.Sex AS RelativeSex,
       d.Role, d.Tier, d.IsAdopted, d.IsStep, d.Status
FROM (SELECT * FROM directed
      UNION ALL SELECT * FROM grandparents
      UNION ALL SELECT * FROM grandchildren
      UNION ALL SELECT * FROM siblings) d
JOIN dbo.tbl_BirthDetails rel ON rel.Id = d.RelativeId;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '170_family_layers_grandparents_and_lateral.sql')
    INSERT dbo.SchemaMigrations (ScriptName, AppliedAtUtc, Note)
    VALUES ('170_family_layers_grandparents_and_lateral.sql', SYSUTCDATETIME(),
            'vw_PersonFamily: grandparents (paternal/maternal) and grandchildren as Extended, siblings as Lateral; Tier check allows Lateral.');

COMMIT TRANSACTION;
GO
