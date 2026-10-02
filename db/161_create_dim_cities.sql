-- =====================================================================
-- 161 - Reusable city/country reference catalogue.
--
-- The application still stores the selected labels on tbl_BirthDetails;
-- this dimension supplies the requested reusable list without creating
-- incomplete birth-detail rows.
--
-- Apply: sqlcmd -S "localhost\SQLSERVER2025" -E -d ikiastrro -b -i db/161_create_dim_cities.sql
-- =====================================================================
USE [ikiastrro];
GO

IF OBJECT_ID(N'dbo.tbl_Dim_Cities', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Dim_Cities
    (
        Id          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Dim_Cities PRIMARY KEY,
        CityName    NVARCHAR(200) NOT NULL,
        CountryName NVARCHAR(200) NOT NULL,
        IsActive    BIT NOT NULL CONSTRAINT DF_Dim_Cities_IsActive DEFAULT (1),
        CONSTRAINT UQ_Dim_Cities_CityCountry UNIQUE (CityName, CountryName)
    );
END;
GO

;WITH seed (CityName, CountryName) AS
(
    SELECT * FROM (VALUES
        (N'Bangalore (Karnataka)',                    N'India'),
        (N'Chennai (Tamilnadu)',                      N'India'),
        (N'Hyderabad (Andra Pradesh)',                N'India'),
        (N'Tiruchchirappalli (Tamilnadu)',            N'India'),
        (N'Salem (Tamilnadu)',                        N'India'),
        (N'Delhi',                                    N'India'),
        (N'Pudukkottai (Tamilnadu)',                  N'India'),
        (N'Tirunelvelli (Tamilnadu)',                 N'India'),
        (N'Palayamkottai (Tamilnadu)',                N'India'),
        (N'Kumbakonam (Tamilnadu)',                   N'India'),
        (N'Sankaranayinarkovil (Tamilnadu)',          N'India'),
        (N'Coimbatore (Tamilnadu)',                   N'India'),
        (N'Tirupati (Andra Pradesh)',                 N'India'),
        (N'Cuddapah (Andra Pradesh)',                 N'India'),
        (N'Kanpur (U P)',                             N'India'),
        (N'Ariyalur (Tamilnadu)',                     N'India'),
        (N'Mumbai (Maharashtra)',                     N'India'),
        (N'Trivandrum (Kerala)',                      N'India'),
        (N'Boston',                                   N'U.S.A.'),
        (N'Johannesburg',                             N'South Africa'),
        (N'Kanchipuram (Tamilnadu)',                  N'India'),
        (N'Jullundur (Punjab)',                       N'India'),
        (N'Agra (U P)',                               N'India'),
        (N'Kuala Lumpur',                             N'Malaysia'),
        (N'Seattle',                                  N'U.S.A.'),
        (N'Ghaziabad (U P)',                          N'India'),
        (N'Rampur (U P)',                             N'India'),
        (N'Mosul',                                    N'Iraq')
    ) v (CityName, CountryName)
)
INSERT dbo.tbl_Dim_Cities (CityName, CountryName)
SELECT s.CityName, s.CountryName
FROM seed s
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.tbl_Dim_Cities c
    WHERE c.CityName = s.CityName
      AND c.CountryName = s.CountryName
);
GO

IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = N'161_create_dim_cities.sql')
    INSERT dbo.SchemaMigrations (ScriptName, AppliedAtUtc, Note)
    VALUES (N'161_create_dim_cities.sql', SYSUTCDATETIME(),
            N'Create the reusable city/country dimension and seed the 28 requested locations.');
GO

DECLARE @rows INT = (SELECT COUNT(*) FROM dbo.tbl_Dim_Cities);
PRINT '161 applied: tbl_Dim_Cities rows=' + CAST(@rows AS VARCHAR(10)) + ' (expect at least 28).';
GO
