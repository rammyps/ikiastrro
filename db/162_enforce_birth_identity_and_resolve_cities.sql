-- =====================================================================
-- 162 - Enforce person/birth-identity uniqueness and keep the city
-- catalogue synchronized for every tbl_BirthDetails write path.
--
-- Apply: sqlcmd -S "localhost\SQLSERVER2025" -E -d ikiastrro -b -i db/162_enforce_birth_identity_and_resolve_cities.sql
-- =====================================================================
USE [ikiastrro];
GO

IF COL_LENGTH(N'dbo.tbl_Dim_Cities', N'Latitude') IS NULL
    ALTER TABLE dbo.tbl_Dim_Cities ADD Latitude DECIMAL(9,6) NULL;
IF COL_LENGTH(N'dbo.tbl_Dim_Cities', N'Longitude') IS NULL
    ALTER TABLE dbo.tbl_Dim_Cities ADD Longitude DECIMAL(9,6) NULL;
IF COL_LENGTH(N'dbo.tbl_Dim_Cities', N'IanaTimeZoneId') IS NULL
    ALTER TABLE dbo.tbl_Dim_Cities ADD IanaTimeZoneId VARCHAR(100) NULL;
IF COL_LENGTH(N'dbo.tbl_Dim_Cities', N'LastResolvedAtUtc') IS NULL
    ALTER TABLE dbo.tbl_Dim_Cities ADD LastResolvedAtUtc DATETIME2(0) NULL;
GO

-- Backfill or refresh catalogue coordinates from already-resolved birth records.
;WITH source AS
(
    SELECT PlaceCity AS CityName, PlaceCountry AS CountryName,
           Latitude, Longitude, IanaTimeZoneId,
           ROW_NUMBER() OVER (PARTITION BY PlaceCity, PlaceCountry ORDER BY Id) AS rn
    FROM dbo.tbl_BirthDetails
)
UPDATE c
SET c.Latitude = s.Latitude,
    c.Longitude = s.Longitude,
    c.IanaTimeZoneId = s.IanaTimeZoneId,
    c.LastResolvedAtUtc = SYSUTCDATETIME()
FROM dbo.tbl_Dim_Cities c
JOIN source s ON s.CityName = c.CityName
             AND s.CountryName = c.CountryName
             AND s.rn = 1;
GO

;WITH source AS
(
    SELECT PlaceCity AS CityName, PlaceCountry AS CountryName,
           Latitude, Longitude, IanaTimeZoneId,
           ROW_NUMBER() OVER (PARTITION BY PlaceCity, PlaceCountry ORDER BY Id) AS rn
    FROM dbo.tbl_BirthDetails
)
INSERT dbo.tbl_Dim_Cities
    (CityName, CountryName, Latitude, Longitude, IanaTimeZoneId, LastResolvedAtUtc)
SELECT s.CityName, s.CountryName, s.Latitude, s.Longitude, s.IanaTimeZoneId, SYSUTCDATETIME()
FROM source s
WHERE s.rn = 1
  AND NOT EXISTS
  (
      SELECT 1 FROM dbo.tbl_Dim_Cities c
      WHERE c.CityName = s.CityName AND c.CountryName = s.CountryName
  );
GO

IF EXISTS (SELECT 1 FROM dbo.tbl_BirthDetails GROUP BY Name HAVING COUNT(*) > 1)
    THROW 51000, 'Cannot enforce unique birth-detail names: duplicate names already exist.', 1;

IF EXISTS
(
    SELECT 1 FROM dbo.tbl_BirthDetails
    GROUP BY DateOfBirth, TimeOfBirth, PlaceCity, PlaceCountry
    HAVING COUNT(*) > 1
)
    THROW 51001, 'Cannot enforce unique birth identity: duplicate date/time/place rows already exist.', 1;
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.tbl_BirthDetails') AND name = N'IX_BirthDetails_Name')
    DROP INDEX IX_BirthDetails_Name ON dbo.tbl_BirthDetails;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.tbl_BirthDetails') AND name = N'UX_BirthDetails_Name')
    CREATE UNIQUE INDEX UX_BirthDetails_Name ON dbo.tbl_BirthDetails (Name);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.tbl_BirthDetails') AND name = N'UX_BirthDetails_BirthIdentity')
    CREATE UNIQUE INDEX UX_BirthDetails_BirthIdentity
        ON dbo.tbl_BirthDetails (DateOfBirth, TimeOfBirth, PlaceCity, PlaceCountry);
GO

CREATE OR ALTER TRIGGER dbo.trg_BirthDetails_SyncCity
ON dbo.tbl_BirthDetails
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH source AS
    (
        SELECT PlaceCity AS CityName, PlaceCountry AS CountryName,
           Latitude, Longitude, IanaTimeZoneId,
               ROW_NUMBER() OVER (PARTITION BY PlaceCity, PlaceCountry ORDER BY Id) AS rn
        FROM inserted
    )
    UPDATE c
    SET c.Latitude = s.Latitude,
        c.Longitude = s.Longitude,
        c.IanaTimeZoneId = s.IanaTimeZoneId,
        c.IsActive = 1,
        c.LastResolvedAtUtc = SYSUTCDATETIME()
    FROM dbo.tbl_Dim_Cities c
    JOIN source s ON s.CityName = c.CityName
                 AND s.CountryName = c.CountryName
                 AND s.rn = 1;

    ;WITH source AS
    (
        SELECT PlaceCity AS CityName, PlaceCountry AS CountryName,
           Latitude, Longitude, IanaTimeZoneId,
               ROW_NUMBER() OVER (PARTITION BY PlaceCity, PlaceCountry ORDER BY Id) AS rn
        FROM inserted
    )
    INSERT dbo.tbl_Dim_Cities
        (CityName, CountryName, Latitude, Longitude, IanaTimeZoneId, LastResolvedAtUtc)
    SELECT s.CityName, s.CountryName, s.Latitude, s.Longitude, s.IanaTimeZoneId, SYSUTCDATETIME()
    FROM source s
    WHERE s.rn = 1
      AND NOT EXISTS
      (
          SELECT 1 FROM dbo.tbl_Dim_Cities c
          WHERE c.CityName = s.CityName AND c.CountryName = s.CountryName
      );
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = N'162_enforce_birth_identity_and_resolve_cities.sql')
    INSERT dbo.SchemaMigrations (ScriptName, AppliedAtUtc, Note)
    VALUES (N'162_enforce_birth_identity_and_resolve_cities.sql', SYSUTCDATETIME(),
            N'Unique names and date/time/place identities; city catalogue coordinates synchronized by trigger for every write path.');
GO
