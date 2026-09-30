-- 158 — PVR ch.26.3 Table 63: auspicious transit houses and their Vedha houses.
-- The raw OCR places the Sun label on the header line and the Saturn values on the
-- following line; the seven value rows below are aligned by sequence and confirmed by
-- PVR's worked Mercury-in-4th example immediately after the table.
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID('dbo.tbl_Rule_GocharaVedha','U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Rule_GocharaVedha (
        Id INT IDENTITY(1,1) CONSTRAINT PK_Rule_GocharaVedha PRIMARY KEY,
        RuleSetId TINYINT NOT NULL,
        TransitPlanetId TINYINT NOT NULL,
        AuspiciousHouse TINYINT NOT NULL,
        VedhaHouse TINYINT NOT NULL,
        ExcludedObstructorPlanetId TINYINT NULL,
        SourceRefCode VARCHAR(40) NOT NULL,
        CONSTRAINT FK_Rule_GocharaVedha_RuleSet FOREIGN KEY (RuleSetId) REFERENCES dbo.tbl_Rule_Sets(Id),
        CONSTRAINT FK_Rule_GocharaVedha_TransitPlanet FOREIGN KEY (TransitPlanetId) REFERENCES dbo.tbl_Planets(Id),
        CONSTRAINT FK_Rule_GocharaVedha_ExcludedPlanet FOREIGN KEY (ExcludedObstructorPlanetId) REFERENCES dbo.tbl_Planets(Id),
        CONSTRAINT CK_Rule_GocharaVedha_AuspiciousHouse CHECK (AuspiciousHouse BETWEEN 1 AND 12),
        CONSTRAINT CK_Rule_GocharaVedha_VedhaHouse CHECK (VedhaHouse BETWEEN 1 AND 12),
        CONSTRAINT UQ_Rule_GocharaVedha UNIQUE (RuleSetId, TransitPlanetId, AuspiciousHouse)
    );
END;

DELETE FROM dbo.tbl_Rule_GocharaVedha WHERE RuleSetId = 1;

;WITH rules(Planet, GoodHouse, VedhaHouse, ExcludedPlanet) AS (
    SELECT * FROM (VALUES
      ('Sun',3,9,'Saturn'),('Sun',6,12,'Saturn'),('Sun',10,4,'Saturn'),('Sun',11,5,'Saturn'),
      ('Moon',1,5,'Mercury'),('Moon',3,9,'Mercury'),('Moon',6,12,'Mercury'),('Moon',7,2,'Mercury'),('Moon',10,4,'Mercury'),('Moon',11,8,'Mercury'),
      ('Mars',3,12,NULL),('Mars',6,9,NULL),('Mars',11,5,NULL),
      ('Mercury',2,5,'Moon'),('Mercury',4,3,'Moon'),('Mercury',6,9,'Moon'),('Mercury',8,1,'Moon'),('Mercury',10,8,'Moon'),('Mercury',11,12,'Moon'),
      ('Jupiter',2,12,NULL),('Jupiter',5,4,NULL),('Jupiter',7,3,NULL),('Jupiter',9,10,NULL),('Jupiter',11,8,NULL),
      ('Venus',1,8,NULL),('Venus',2,7,NULL),('Venus',3,1,NULL),('Venus',4,10,NULL),('Venus',5,9,NULL),('Venus',8,5,NULL),('Venus',9,11,NULL),('Venus',11,6,NULL),('Venus',12,3,NULL),
      ('Saturn',3,12,'Sun'),('Saturn',6,9,'Sun'),('Saturn',11,5,'Sun')
    ) v(Planet,GoodHouse,VedhaHouse,ExcludedPlanet)
)
INSERT dbo.tbl_Rule_GocharaVedha
    (RuleSetId, TransitPlanetId, AuspiciousHouse, VedhaHouse, ExcludedObstructorPlanetId, SourceRefCode)
SELECT 1, p.Id, r.GoodHouse, r.VedhaHouse, excluded.Id, 'SRC_PVR_INTEGRATED'
FROM rules r
JOIN dbo.tbl_Planets p ON p.PlanetName = r.Planet
LEFT JOIN dbo.tbl_Planets excluded ON excluded.PlanetName = r.ExcludedPlanet;

IF (SELECT COUNT(*) FROM dbo.tbl_Rule_GocharaVedha WHERE RuleSetId=1) <> 36
    THROW 51000, 'Expected 36 aligned PVR Gochara Vedha rows.', 1;

IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName='158_create_rule_gochara_vedha.sql')
    INSERT dbo.SchemaMigrations (ScriptName,Note)
    VALUES ('158_create_rule_gochara_vedha.sql','PVR ch.26.3 Table 63: 36 Gochara Vedha pairs with Sun-Saturn and Moon-Mercury exceptions');

COMMIT TRANSACTION;
PRINT '158 applied: 36 correctly aligned Gochara Vedha rules.';
