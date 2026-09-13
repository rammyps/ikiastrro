/* Taittiriya Brahmana 1.5.1, printed pages 34-35: Sanskrit nakshatra
   deity and paired "above/below" formulations with literal project English.
   Research corpus only. The source uses several Vedic nakshatra names.
   Apply as UTF-8: sqlcmd -S localhost\SQLSERVER2025 -E -C -b -f 65001 -i db\072_seed_taittiriya_brahmana_nakshatra_text.sql */
USE [ikiastrro];
GO

IF COL_LENGTH(N'research.tbl_Dim_SourceReferenceNakshatraText', N'SourceUrl') IS NULL
    ALTER TABLE research.tbl_Dim_SourceReferenceNakshatraText ADD SourceUrl NVARCHAR(1000) NULL;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Dim_Source WHERE Code = 'SRC_TB_1_5_1_NAKSHATRAS')
    INSERT dbo.tbl_Dim_Source (Code,Title,Author,Edition,Tradition,Notes)
    VALUES ('SRC_TB_1_5_1_NAKSHATRAS',N'Taittiriya Brahmana 1.5.1 — Nakshatra powers',
            N'Taittiriya recension (traditional)',N'Sanskrit Documents taittirIyabrAhmaNamniHsvaraH.pdf, 2026 typeset',
            'Vedic',N'Accentless Devanagari text; cite printed pages 34-35 and section unit.');
GO

;WITH passages (NakshatraCode,SectionUnit,SanskritText,EnglishText) AS
(
    SELECT * FROM (VALUES
      ('NAKSHATRA_ASHWINI',5,N'अश्विनोरश्वयुजौ । ग्रामः परस्तात्सेनाऽवस्तात् ।',N'The Ashvayuj pair belongs to the Ashvins. The community is above; the host or army is below.'),
      ('NAKSHATRA_BHARANI',5,N'यमस्यापभरणीः । अपकर्षन्तः परस्तादपवहन्तोऽवस्तात् ।',N'The Apabharanis belong to Yama. Those drawing away are above; those carrying away are below.'),
      ('NAKSHATRA_KRITTIKA',1,N'अग्नेः कृत्तिकाः । शुक्रं परस्ताज्ज्योतिरवस्तात् ।',N'The Krittikas belong to Agni. Brightness is above; light is below.'),
      ('NAKSHATRA_ROHINI',1,N'प्रजापते रोहिणी । आपः परस्तादोषधयोऽवस्तात् ।',N'Rohini belongs to Prajapati. Waters are above; plants are below.'),
      ('NAKSHATRA_MRIGASHIRA',1,N'सोमस्येन्वका । विततानि परस्ताद्वयन्तोऽवस्तात् ।',N'Invaka belongs to Soma. The spread-out threads are above; the weavers are below.'),
      ('NAKSHATRA_ARDRA',1,N'रुद्रस्य बाहू । मृगयवः परस्ताद्विक्षारोऽवस्तात् ।',N'The arms belong to Rudra. The hunters of game are above; scattering is below.'),
      ('NAKSHATRA_PUNARVASU',1,N'अदित्यै पुनर्वसू । वातः परस्तादार्द्रमवस्तात् ।',N'The Punarvasus belong to Aditi. Wind is above; moisture is below.'),
      ('NAKSHATRA_PUSHYA',2,N'बृहस्पतेस्तिष्यः । जुह्वतः परस्ताद्यजमाना अवस्तात् ।',N'Tishya belongs to Brihaspati. Those making offerings are above; the sacrificers are below.'),
      ('NAKSHATRA_ASHLESHA',2,N'सर्पाणामाश्रेषाः । अभ्यागच्छन्तः परस्तादभ्यानृत्यन्तोऽवस्तात् ।',N'The Ashleshas belong to the serpents. Those approaching are above; those dancing near are below.'),
      ('NAKSHATRA_MAGHA',2,N'पितृणां मघाः । रुदन्तः परस्तादपभ्रꣳशोऽवस्तात् ।',N'The Maghas belong to the ancestors. Those weeping are above; falling away is below.'),
      ('NAKSHATRA_PURVA_PHALGUNI',2,N'अर्यम्णः पूर्वे फल्गुनी । जाया परस्तादृषभोऽवस्तात् ।',N'The former Phalgunis belong to Aryaman. The wife is above; the bull is below.'),
      ('NAKSHATRA_UTTARA_PHALGUNI',2,N'भगस्योत्तरे । वहतवः परस्ताद्वहमाना अवस्तात् ।',N'The latter Phalgunis belong to Bhaga. Those who convey are above; those being conveyed are below.'),
      ('NAKSHATRA_HASTA',3,N'देवस्य सवितुर्हस्तः । प्रसवः परस्तात्सनिरवस्तात् ।',N'Hasta belongs to the god Savitar. Impulsion is above; acquisition is below.'),
      ('NAKSHATRA_CHITRA',3,N'इन्द्रस्य चित्रा । ऋतं परस्तात्सत्यमवस्तात् ।',N'Chitra belongs to Indra. Cosmic order is above; truth is below.'),
      ('NAKSHATRA_SWATI',3,N'वायोर्निष्ट्या । व्रततिः परस्तादसिद्धिरवस्तात् ।',N'Nishtya belongs to Vayu. The spreading creeper is above; non-attainment is below.'),
      ('NAKSHATRA_VISHAKHA',3,N'इन्द्राग्नियोर्विशाखे । युगानि परस्तात्कृषमाणा अवस्तात् ।',N'The Vishakhas belong to Indra and Agni. Yokes are above; those ploughing are below.'),
      ('NAKSHATRA_ANURADHA',3,N'मित्रस्यानूराधाः । अभ्यारोहत्परस्तादभ्यारूढमवस्तात् ।',N'The Anuradhas belong to Mitra. Ascending is above; what has been ascended is below.'),
      ('NAKSHATRA_JYESHTHA',4,N'इन्द्रस्य रोहिणी । शृणत्परस्तात्प्रतिशृणदवस्तात् ।',N'Rohini here belongs to Indra. Hearing is above; answering what is heard is below.'),
      ('NAKSHATRA_MULA',4,N'निरृत्यै मूलवर्हणी । प्रतिभञ्जन्तः परस्तात्प्रतिशृणन्तोऽवस्तात् ।',N'Mulabarhani belongs to Nirriti. Those breaking apart are above; those responding are below.'),
      ('NAKSHATRA_PURVA_ASHADHA',4,N'अपां पूर्वा अषाढाः । वर्चः परस्तात्समितिरवस्तात् ।',N'The former Ashadhas belong to the Waters. Splendour is above; gathering together is below.'),
      ('NAKSHATRA_UTTARA_ASHADHA',4,N'विश्वेषां देवानामुत्तराः । अभिजयत्परस्तादभिजितमवस्तात् ।',N'The latter Ashadhas belong to all the gods. Conquering is above; what is conquered is below.'),
      ('NAKSHATRA_SHRAVANA',4,N'विष्णोः श्रोणा । पृच्छमानाः परस्तात्पन्था अवस्तात् ।',N'Shrona belongs to Vishnu. Those asking are above; the path is below.'),
      ('NAKSHATRA_DHANISHTHA',5,N'वसूनां श्रविष्ठाः । भूतं परस्ताद्भूतिरवस्तात् ।',N'The Shravishthas belong to the Vasus. What has come to be is above; prosperity is below.'),
      ('NAKSHATRA_SHATABHISHA',5,N'इन्द्रस्य शतभिषक् । विश्वव्यचाः परस्ताद्विश्वक्षितिरवस्तात् ।',N'Shatabhishaj belongs to Indra. All-pervading extension is above; all-sustaining dwelling is below.'),
      ('NAKSHATRA_PURVA_BHADRAPADA',5,N'अजस्यैकपदः पूर्वे प्रोष्ठपदाः । वैश्वानरं परस्ताद्वैश्वावसवमवस्तात् ।',N'The former Proshthapadas belong to Aja Ekapad. The universal fire is above; universal wealth is below.'),
      ('NAKSHATRA_UTTARA_BHADRAPADA',5,N'अहेर्बुध्नियस्योत्तरे । अभिषिञ्चन्तः परस्तादभिषुण्वन्तोऽवस्तात् ।',N'The latter Proshthapadas belong to Ahi Budhnya. Those sprinkling are above; those pressing out are below.'),
      ('NAKSHATRA_REVATI',5,N'पूष्णो रेवती । गावः परस्ताद्वत्सा अवस्तात् ।',N'Revati belongs to Pushan. Cows are above; calves are below.')
    ) v(NakshatraCode,SectionUnit,SanskritText,EnglishText)
)
INSERT research.tbl_Dim_SourceReferenceNakshatraText
    (NakshatraId,PadaNumber,SourceRefCode,WorkTitle,Author,Edition,Chapter,VerseOrPage,
     LanguageCode,TextTypeCode,FullText,TranslationText,Notes,CopyrightStatus,SourceLocator,SourceUrl)
SELECT n.Id,NULL,'SRC_TB_1_5_1_NAKSHATRAS',N'Taittirīya Brāhmaṇa',
       N'Taittirīya recension (traditional)',N'Sanskrit Documents taittirIyabrAhmaNamniHsvaraH.pdf; accentless 2026 typeset',
       N'First Ashtaka, fifth Prapathaka',
       N'1.5.1.' + CONVERT(NVARCHAR(1),v.SectionUnit) + CASE WHEN v.SectionUnit=5 THEN N'; printed p. 35' ELSE N'; printed p. 34' END,
       'sa-Deva','SanskritPassage',v.SanskritText,v.EnglishText,
       N'Literal project translation from the Sanskrit, not copied from a published translation. The Sanskrit preserves the source’s Vedic nakshatra names; the row mapping supplies the standard later name.',
       'PublicDomainOriginal',N'taittirIyabrAhmaNamniHsvaraH.pdf, printed pages 34-35, TB 1.5.1',
       N'https://sanskritdocuments.org/doc_veda/taittirIyabrAhmaNamniHsvaraH.pdf'
FROM passages v
JOIN research.tbl_Dim_SourceReferenceNakshatra n ON n.NakshatraCode=v.NakshatraCode
WHERE NOT EXISTS
(
    SELECT 1 FROM research.tbl_Dim_SourceReferenceNakshatraText existing
    WHERE existing.NakshatraId=n.Id
      AND existing.SourceRefCode='SRC_TB_1_5_1_NAKSHATRAS'
);
GO

INSERT dbo.SchemaMigrations (ScriptName,Note)
SELECT '072_seed_taittiriya_brahmana_nakshatra_text.sql',
       'Seed TB 1.5.1 printed-page-34-35 Sanskrit passages and literal English for all 27 nakshatras.'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName='072_seed_taittiriya_brahmana_nakshatra_text.sql');
GO
