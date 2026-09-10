/* Repair and complete planet-in-house reference seed after SourceUrl creation. */
IF OBJECT_ID(N'research.tbl_Dim_SourceReferencePlanetInHouseText',N'U') IS NOT NULL
   AND COL_LENGTH(N'research.tbl_Dim_SourceReferencePlanetInHouseText',N'SourceUrl') IS NULL
    ALTER TABLE research.tbl_Dim_SourceReferencePlanetInHouseText ADD SourceUrl NVARCHAR(1000) NULL;
GO
INSERT research.tbl_Dim_SourceReferencePlanetInHouse (PlanetId,HouseId)
SELECT p.Id,h.Id FROM research.tbl_Dim_SourceReferencePlanet p CROSS JOIN research.tbl_Dim_SourceReferenceHouse h
WHERE NOT EXISTS (SELECT 1 FROM research.tbl_Dim_SourceReferencePlanetInHouse x WHERE x.PlanetId=p.Id AND x.HouseId=h.Id);
INSERT research.tbl_Dim_SourceReferencePlanetInHouseText
 (PlanetInHouseId,SourceRefCode,WorkTitle,Author,Chapter,VerseOrPage,TextTypeCode,CopyrightStatus,SourceLocator,SourceUrl,Notes)
SELECT x.Id,s.SourceRefCode,s.WorkTitle,s.Author,s.Chapter,s.VerseOrPage,'Reference',s.CopyrightStatus,s.SourceLocator,s.SourceUrl,N'Reference pointer only; verify the cited edition and record conditions separately.'
FROM research.tbl_Dim_SourceReferencePlanetInHouse x
CROSS JOIN (VALUES ('SRC_PHALADEEPIKA_PLANET_IN_HOUSE',N'Phaladeepika',N'Mantresvara',N'Planetary results in bhavas',N'House-effects chapter; verify planet-specific verse/page','PublicDomainReference',N'Planet-in-house results for the nine grahas.',N'https://www.panchanga.lv/wp-content/uploads/2020/06/Phaladeepika_Shri-Mantreswara.pdf'),('SRC_BVRAMAN_PLANET_IN_HOUSE',N'Hindu Predictive Astrology',N'B. V. Raman',N'Judgment of Bhavas',N'Chapter 19; verify edition page','SummaryOnly',N'Use licensed edition or page reference; do not copy full text.',N'https://vedastro.org/blog/Hindu-Predictive-Astrology-Chapter-19-Judgment-of-Bhavas.html')) s(SourceRefCode,WorkTitle,Author,Chapter,VerseOrPage,CopyrightStatus,SourceLocator,SourceUrl)
WHERE NOT EXISTS (SELECT 1 FROM research.tbl_Dim_SourceReferencePlanetInHouseText t WHERE t.PlanetInHouseId=x.Id AND t.SourceRefCode=s.SourceRefCode);
IF OBJECT_ID(N'dbo.SchemaMigrations',N'U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName=N'065_seed_planet_in_house_research_references.sql')
INSERT dbo.SchemaMigrations(ScriptName,Note) VALUES(N'065_seed_planet_in_house_research_references.sql',N'Add SourceUrl and complete 9x12 planet-in-house research references.');
