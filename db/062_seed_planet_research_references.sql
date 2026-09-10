/* Seed reference-first planetary research metadata. No copied translation text is stored. */
IF OBJECT_ID(N'research.tbl_Dim_SourceReferencePlanet', N'U') IS NOT NULL
BEGIN
    INSERT research.tbl_Dim_SourceReferencePlanet (PlanetCode, SanskritName, EnglishName)
    SELECT v.PlanetCode, v.SanskritName, v.EnglishName
    FROM (VALUES
        ('PLANET_SUN',N'Sūrya',N'Sun'),('PLANET_MOON',N'Candra',N'Moon'),('PLANET_MARS',N'Maṅgala',N'Mars'),
        ('PLANET_MERCURY',N'Budha',N'Mercury'),('PLANET_JUPITER',N'Guru',N'Jupiter'),('PLANET_VENUS',N'Śukra',N'Venus'),
        ('PLANET_SATURN',N'Śani',N'Saturn'),('PLANET_RAHU',N'Rāhu',N'Rahu'),('PLANET_KETU',N'Ketu',N'Ketu')
    ) v(PlanetCode,SanskritName,EnglishName)
    WHERE NOT EXISTS (SELECT 1 FROM research.tbl_Dim_SourceReferencePlanet p WHERE p.PlanetCode=v.PlanetCode);
END;

IF OBJECT_ID(N'research.tbl_Dim_SourceReferencePlanetText', N'U') IS NOT NULL
BEGIN
    INSERT research.tbl_Dim_SourceReferencePlanetText
        (PlanetId,SourceRefCode,WorkTitle,Author,Chapter,VerseOrPage,TextTypeCode,
         CopyrightStatus,SourceLocator,SourceUrl,Notes)
    SELECT p.Id,s.SourceRefCode,s.WorkTitle,s.Author,s.Chapter,s.VerseOrPage,'Reference',
           s.CopyrightStatus,s.SourceLocator,s.SourceUrl,
           N'Reference pointer only; verify the planetary attribute against the cited edition and record normalized claims separately.'
    FROM research.tbl_Dim_SourceReferencePlanet p
    CROSS JOIN (VALUES
        ('SRC_BPHS_GRAHA_SVARUPA',N'Brihat Parashara Hora Shastra',N'Parashara',N'Graha Guna Svarūpa Adhyāya',N'Chapter 3, verses 1–64','PublicDomainReference',N'Planetary nature, form, cabinet, bodily substances and significations.',N'https://vedicspace.com/bphs'),
        ('SRC_PHALADEEPIKA_PLANETS',N'Phaladeepika',N'Mantresvara',N'Planetary characteristics',N'Chapter 2, verses 1–7','PublicDomainReference',N'Planetary qualities and natural indications.',N'https://www.panchanga.lv/wp-content/uploads/2020/06/Phaladeepika_Shri-Mantreswara.pdf')
    ) s(SourceRefCode,WorkTitle,Author,Chapter,VerseOrPage,CopyrightStatus,SourceLocator,SourceUrl)
    WHERE NOT EXISTS (SELECT 1 FROM research.tbl_Dim_SourceReferencePlanetText x WHERE x.PlanetId=p.Id AND x.SourceRefCode=s.SourceRefCode);
END;

IF OBJECT_ID(N'dbo.SchemaMigrations',N'U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName=N'062_seed_planet_research_references.sql')
INSERT dbo.SchemaMigrations(ScriptName,Note) VALUES(N'062_seed_planet_research_references.sql',N'Seed nine planet research records and source-reference metadata without copied translation text.');
