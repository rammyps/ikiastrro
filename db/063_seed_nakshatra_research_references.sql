/* Seed reference-first nakshatra research metadata. No copied translation text is stored. */
IF OBJECT_ID(N'research.tbl_Dim_SourceReferenceNakshatra',N'U') IS NOT NULL
BEGIN
    INSERT research.tbl_Dim_SourceReferenceNakshatra (NakshatraCode,SanskritName,EnglishName,SequenceNumber)
    SELECT v.NakshatraCode,v.SanskritName,v.EnglishName,v.SequenceNumber
    FROM (VALUES
      ('NAKSHATRA_ASHWINI',N'Aśvinī',N'Ashwini',1),('NAKSHATRA_BHARANI',N'Bharaṇī',N'Bharani',2),('NAKSHATRA_KRITTIKA',N'Kṛttikā',N'Krittika',3),('NAKSHATRA_ROHINI',N'Rohiṇī',N'Rohini',4),('NAKSHATRA_MRIGASHIRA',N'Mṛgaśīrṣa',N'Mrigashira',5),('NAKSHATRA_ARDRA',N'Ārdrā',N'Ardra',6),('NAKSHATRA_PUNARVASU',N'Punarvasu',N'Punarvasu',7),('NAKSHATRA_PUSHYA',N'Puṣya',N'Pushya',8),('NAKSHATRA_ASHLESHA',N'Āśleṣā',N'Ashlesha',9),('NAKSHATRA_MAGHA',N'Maghā',N'Magha',10),('NAKSHATRA_PURVA_PHALGUNI',N'Pūrvaphalgunī',N'Purva Phalguni',11),('NAKSHATRA_UTTARA_PHALGUNI',N'Uttaraphalgunī',N'Uttara Phalguni',12),('NAKSHATRA_HASTA',N'Hastā',N'Hasta',13),('NAKSHATRA_CHITRA',N'Citrā',N'Chitra',14),('NAKSHATRA_SWATI',N'Svātī',N'Swati',15),('NAKSHATRA_VISHAKHA',N'Viśākhā',N'Vishakha',16),('NAKSHATRA_ANURADHA',N'Anurādhā',N'Anuradha',17),('NAKSHATRA_JYESHTHA',N'Jyeṣṭhā',N'Jyeshtha',18),('NAKSHATRA_MULA',N'Mūla',N'Mula',19),('NAKSHATRA_PURVA_ASHADHA',N'Pūrvāṣāḍhā',N'Purva Ashadha',20),('NAKSHATRA_UTTARA_ASHADHA',N'Uttarāṣāḍhā',N'Uttara Ashadha',21),('NAKSHATRA_SHRAVANA',N'Śravaṇa',N'Shravana',22),('NAKSHATRA_DHANISHTHA',N'Dhaniṣṭhā',N'Dhanishtha',23),('NAKSHATRA_SHATABHISHA',N'Śatabhiṣā',N'Shatabhisha',24),('NAKSHATRA_PURVA_BHADRAPADA',N'Pūrvabhādrapadā',N'Purva Bhadrapada',25),('NAKSHATRA_UTTARA_BHADRAPADA',N'Uttarabhādrapadā',N'Uttara Bhadrapada',26),('NAKSHATRA_REVATI',N'Revatī',N'Revati',27)
    ) v(NakshatraCode,SanskritName,EnglishName,SequenceNumber)
    WHERE NOT EXISTS (SELECT 1 FROM research.tbl_Dim_SourceReferenceNakshatra n WHERE n.NakshatraCode=v.NakshatraCode);
END;
IF OBJECT_ID(N'research.tbl_Dim_SourceReferenceNakshatraText',N'U') IS NOT NULL
BEGIN
    INSERT research.tbl_Dim_SourceReferenceNakshatraText
      (NakshatraId,SourceRefCode,WorkTitle,Author,Chapter,VerseOrPage,TextTypeCode,CopyrightStatus,SourceLocator,SourceUrl,Notes)
    SELECT n.Id,s.SourceRefCode,s.WorkTitle,s.Author,s.Chapter,s.VerseOrPage,'Reference',s.CopyrightStatus,s.SourceLocator,s.SourceUrl,N'Reference pointer only; verify nakshatra and pada attributes against the cited edition.'
    FROM research.tbl_Dim_SourceReferenceNakshatra n
    CROSS JOIN (VALUES ('SRC_BPHS_NAKSHATRAS',N'Brihat Parashara Hora Shastra',N'Parashara',N'Nakshatra and Vimshottari material',N'Nakshatra sections; verify edition locator','PublicDomainReference',N'Nakshatra names, lords and applications.',N'https://vedicspace.com/bphs'),('SRC_PHALADEEPIKA_NAKSHATRAS',N'Phaladeepika',N'Mantresvara',N'Nakshatra/rashi definitions',N'Verify chapter and verse in selected edition','PublicDomainReference',N'Nakshatra and lunar mansion references.',N'https://www.panchanga.lv/wp-content/uploads/2020/06/Phaladeepika_Shri-Mantreswara.pdf')) s(SourceRefCode,WorkTitle,Author,Chapter,VerseOrPage,CopyrightStatus,SourceLocator,SourceUrl)
    WHERE NOT EXISTS (SELECT 1 FROM research.tbl_Dim_SourceReferenceNakshatraText x WHERE x.NakshatraId=n.Id AND x.SourceRefCode=s.SourceRefCode);
END;
IF OBJECT_ID(N'dbo.SchemaMigrations',N'U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName=N'063_seed_nakshatra_research_references.sql')
INSERT dbo.SchemaMigrations(ScriptName,Note) VALUES(N'063_seed_nakshatra_research_references.sql',N'Seed 27 nakshatra research records and source-reference metadata without copied translation text.');
