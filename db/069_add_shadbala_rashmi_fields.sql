/* Add Parasari Rashmi and Ishta/Kashta source fields to planetary strength facts. */
IF OBJECT_ID(N'dbo.tbl_Fact_PlanetaryStrength',N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.tbl_Fact_PlanetaryStrength',N'UchchaRashmi') IS NULL
        ALTER TABLE dbo.tbl_Fact_PlanetaryStrength ADD UchchaRashmi DECIMAL(12,3) NULL;
    IF COL_LENGTH(N'dbo.tbl_Fact_PlanetaryStrength',N'CheshtaRashmi') IS NULL
        ALTER TABLE dbo.tbl_Fact_PlanetaryStrength ADD CheshtaRashmi DECIMAL(12,3) NULL;
    IF COL_LENGTH(N'dbo.tbl_Fact_PlanetaryStrength',N'SubhaRashmi') IS NULL
        ALTER TABLE dbo.tbl_Fact_PlanetaryStrength ADD SubhaRashmi DECIMAL(12,3) NULL;
    IF COL_LENGTH(N'dbo.tbl_Fact_PlanetaryStrength',N'AsubhaRashmi') IS NULL
        ALTER TABLE dbo.tbl_Fact_PlanetaryStrength ADD AsubhaRashmi DECIMAL(12,3) NULL;
    IF COL_LENGTH(N'dbo.tbl_Fact_PlanetaryStrength',N'IshtaPhalaParasara') IS NULL
        ALTER TABLE dbo.tbl_Fact_PlanetaryStrength ADD IshtaPhalaParasara DECIMAL(12,3) NULL;
    IF COL_LENGTH(N'dbo.tbl_Fact_PlanetaryStrength',N'KashtaPhalaParasara') IS NULL
        ALTER TABLE dbo.tbl_Fact_PlanetaryStrength ADD KashtaPhalaParasara DECIMAL(12,3) NULL;
END;

IF OBJECT_ID(N'dbo.vw_ChartShadbala',N'V') IS NOT NULL
    EXEC(N'CREATE OR ALTER VIEW dbo.vw_ChartShadbala AS
SELECT s.Id, s.ChartResultId, s.PlanetId, s.RuleSetId, s.StrengthProfileCode,
       s.FormulaSourceRefCode, s.SthanaBalaVirupas, s.DigBalaVirupas,
       s.KalaBalaVirupas, s.CheshtaBalaVirupas, s.NaisargikaBalaVirupas,
       s.DrikBalaVirupas, s.YuddhaBalaVirupas, s.ShadbalaVirupas,
       s.ShadbalaRupas, s.IshtaBala, s.KashtaBala,
       s.UchchaRashmi, s.CheshtaRashmi, s.SubhaRashmi, s.AsubhaRashmi,
       s.IshtaPhalaParasara, s.KashtaPhalaParasara,
       s.MinimumRequiredRupas, s.CalculationNarrative, s.ComputedAtUtc
FROM dbo.tbl_Fact_PlanetaryStrength s;');

IF OBJECT_ID(N'dbo.SchemaMigrations','U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName=N'069_add_shadbala_rashmi_fields.sql')
    INSERT dbo.SchemaMigrations(ScriptName,Note)
    VALUES(N'069_add_shadbala_rashmi_fields.sql',N'Add Parasari Rashmi and explicit Ishta/Kashta fields to planetary strength facts and Shadbala view.');
