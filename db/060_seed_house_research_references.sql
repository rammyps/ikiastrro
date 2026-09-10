/* Seed reference-first house research metadata. No copied translation text is stored. */
IF OBJECT_ID(N'research.tbl_Dim_SourceReferencePlanetText', N'U') IS NOT NULL
   AND COL_LENGTH(N'research.tbl_Dim_SourceReferencePlanetText', N'SourceUrl') IS NULL
    ALTER TABLE research.tbl_Dim_SourceReferencePlanetText ADD SourceUrl NVARCHAR(1000) NULL;
IF OBJECT_ID(N'research.tbl_Dim_SourceReferenceNakshatraText', N'U') IS NOT NULL
   AND COL_LENGTH(N'research.tbl_Dim_SourceReferenceNakshatraText', N'SourceUrl') IS NULL
    ALTER TABLE research.tbl_Dim_SourceReferenceNakshatraText ADD SourceUrl NVARCHAR(1000) NULL;
IF OBJECT_ID(N'research.tbl_Dim_SourceReferenceHouseText', N'U') IS NOT NULL
   AND COL_LENGTH(N'research.tbl_Dim_SourceReferenceHouseText', N'SourceUrl') IS NULL
    ALTER TABLE research.tbl_Dim_SourceReferenceHouseText ADD SourceUrl NVARCHAR(1000) NULL;

IF OBJECT_ID(N'research.tbl_Dim_SourceReferenceHouse', N'U') IS NOT NULL
BEGIN
    INSERT research.tbl_Dim_SourceReferenceHouse (HouseNumber, HouseCode, SanskritName, EnglishName)
    SELECT v.HouseNumber, v.HouseCode, v.SanskritName, v.EnglishName
    FROM (VALUES
        (1,'HOUSE_TANU',N'Tanu',N'Self / body'),
        (2,'HOUSE_DHANA',N'Dhana',N'Wealth / speech / family'),
        (3,'HOUSE_SAHAJA',N'Sahaja',N'Siblings / courage / effort'),
        (4,'HOUSE_BANDHU',N'Bandhu',N'Mother / home / vehicles / inner comfort'),
        (5,'HOUSE_PUTRA',N'Putra',N'Children / intelligence / merit'),
        (6,'HOUSE_ARI',N'Ari',N'Disease / debt / enemies / service'),
        (7,'HOUSE_YUVATI',N'Yuvati',N'Partner / marriage / contracts'),
        (8,'HOUSE_RANDHRA',N'Randhra',N'Longevity / hidden matters / transformation'),
        (9,'HOUSE_DHARMA',N'Dharma',N'Fortune / teachers / dharma / higher learning'),
        (10,'HOUSE_KARMA',N'Karma',N'Career / action / authority / reputation'),
        (11,'HOUSE_LABHA',N'Labha',N'Gains / fulfilment / elder siblings'),
        (12,'HOUSE_VYAYA',N'Vyaya',N'Expenditure / loss / release / foreign places')
    ) v(HouseNumber, HouseCode, SanskritName, EnglishName)
    WHERE NOT EXISTS (SELECT 1 FROM research.tbl_Dim_SourceReferenceHouse h WHERE h.HouseNumber=v.HouseNumber);
END;

IF OBJECT_ID(N'research.tbl_Dim_SourceReferenceHouseText', N'U') IS NOT NULL
BEGIN
    INSERT research.tbl_Dim_SourceReferenceHouseText
        (HouseId, SourceRefCode, WorkTitle, Author, Chapter, VerseOrPage, TextTypeCode,
         CopyrightStatus, SourceLocator, SourceUrl, Notes)
    SELECT h.Id, s.SourceRefCode, s.WorkTitle, s.Author, s.Chapter, s.VerseOrPage,
           'Reference', s.CopyrightStatus, s.SourceLocator, s.SourceUrl,
           N'Reference pointer only; use the cited edition for verification and record the normalized interpretation separately.'
    FROM research.tbl_Dim_SourceReferenceHouse h
    CROSS JOIN (VALUES
        ('SRC_BPHS_BHAVAS',N'Brihat Parashara Hora Shastra',N'Parashara',N'Bhava Phala Adhyaya',N'Bhavas 1–12', 'PublicDomainReference',N'Bhava results chapters; verify verse numbering against selected edition.',N'https://vedicspace.com/bphs'),
        ('SRC_PHALADEEPIKA_BHAVAS',N'Phaladeepika',N'Mantresvara',N'Definitions and house results',N'Chapter 1 and house-results chapters', 'PublicDomainReference',N'House definitions and results; verify page/verse against selected edition.',N'https://www.panchanga.lv/wp-content/uploads/2020/06/Phaladeepika_Shri-Mantreswara.pdf')
    ) s(SourceRefCode,WorkTitle,Author,Chapter,VerseOrPage,CopyrightStatus,SourceLocator,SourceUrl)
    WHERE NOT EXISTS
    (SELECT 1 FROM research.tbl_Dim_SourceReferenceHouseText x
     WHERE x.HouseId=h.Id AND x.SourceRefCode=s.SourceRefCode);
END;

IF OBJECT_ID(N'dbo.SchemaMigrations', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = N'060_seed_house_research_references.sql')
    INSERT dbo.SchemaMigrations (ScriptName, Note)
    VALUES (N'060_seed_house_research_references.sql', N'Add source URLs and seed twelve-house research reference metadata without copied translation text.');
