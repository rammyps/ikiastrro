/* BPHS chapter 3, printed page 6: original Sanskrit graha descriptions and
   conservative English working translations. Research corpus only.
   Apply as UTF-8: sqlcmd -S localhost\SQLSERVER2025 -E -C -b -f 65001 -i db\071_seed_bphs_planet_sanskrit_text.sql */
USE [ikiastrro];
GO

IF COL_LENGTH(N'research.tbl_Dim_SourceReferencePlanetText', N'SourceUrl') IS NULL
    ALTER TABLE research.tbl_Dim_SourceReferencePlanetText ADD SourceUrl NVARCHAR(1000) NULL;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Dim_Source WHERE Code = 'SRC_BPHS_GRAHA_SVARUPA')
    INSERT dbo.tbl_Dim_Source (Code,Title,Author,Edition,Tradition,Notes)
    VALUES ('SRC_BPHS_GRAHA_SVARUPA',N'BPHS chapter 3 — Graha traits and forms',
            N'Parāśara (attributed)',N'Sanskrit Documents par0110.pdf, 2025 typeset',
            'Parasari',N'Original Devanagari text; cite printed page and verse.');
GO

;WITH passages (PlanetCode,VerseNumber,SanskritText,EnglishText) AS
(
    SELECT * FROM (VALUES
      ('PLANET_SUN',23,
       N'मधुपिङ्गलदृक्सूर्यश्चतुरस्रः शुचिर्द्विज । पित्तप्रकृतिको धीमान् पुमानल्पकचो द्विज ॥ २३॥',
       N'The Sun has honey-brown eyes and a square build. He is pure, intelligent, masculine, of a pitta constitution, and has little hair.'),
      ('PLANET_MOON',24,
       N'बहुवातकफः प्राज्ञश्चन्द्रो वृत्ततनुर्द्विज । शुभदृङ्मधुवाक्यश्च चञ्चलो मदनातुरः ॥ २४॥',
       N'The Moon has much vata and kapha, is perceptive and round-bodied, has attractive eyes and sweet speech, and is changeable and moved by desire.'),
      ('PLANET_MARS',25,
       N'क्रूरो रक्तेक्षणो भौमश्चपलोदारमूर्तिकः । पित्तप्रकृतिकः क्रोधी कृशमध्यतनुर्द्विज ॥ २५॥',
       N'Mars is fierce, red-eyed, quick and imposing in form. He has a pitta constitution, is prone to anger, and is slender at the waist.'),
      ('PLANET_MERCURY',26,
       N'वपुःश्रेष्ठः श्लिष्टवाक्च ह्यतिहास्यरुचिर्बुधः । पित्तवान् कफवान् विप्र मारुतप्रकृतिस्तथा ॥ २६॥',
       N'Mercury has an excellent physique, speaks with wit and layered meaning, and delights in laughter. His constitution combines pitta, kapha, and vata.'),
      ('PLANET_JUPITER',27,
       N'बृहद्गात्रो गुरुश्चैव पिङ्गलो मूर्द्धजेक्षणे । कफप्रकृतिको धीमान् सर्वशास्त्रविशारदः ॥ २७॥',
       N'Jupiter has a large body and tawny hair and eyes. He has a kapha constitution, is intelligent, and is learned in all the shastras.'),
      ('PLANET_VENUS',28,
       N'सुखि कान्तवपु श्रेष्ठः सुलोचनो भृगोः सुतः । काव्यकर्ता कफाधिक्योऽनिलात्मा वक्रमूर्धजः ॥ २८॥',
       N'Venus is happy, attractive, well formed, and beautiful-eyed. He is a poet, has abundant kapha with a vata nature, and has curly hair.'),
      ('PLANET_SATURN',29,
       N'कृश्दीर्घतनुः शौरिः पिङ्गदृष्ट्यनिलात्मकः । स्थूलदन्तोऽलसः पङ्गुः खररोमकचो द्विज ॥ २९॥',
       N'Saturn is lean and tall, with tawny eyes and a vata constitution. He has large teeth, is slow, may be lame, and has coarse body and head hair.'),
      ('PLANET_RAHU',30,
       N'धूम्राकारो नीलतनुर्वनस्थोऽपि भयङ्करः । वातप्रकृतिको धीमान् स्वर्भानुस्तत्समः शिखी ॥ ३०॥',
       N'Rahu has a smoky appearance and dark-blue body, dwells in wild places, and is fearsome. He has a vata constitution and is intelligent; Ketu is similar.'),
      ('PLANET_KETU',30,
       N'धूम्राकारो नीलतनुर्वनस्थोऽपि भयङ्करः । वातप्रकृतिको धीमान् स्वर्भानुस्तत्समः शिखी ॥ ३०॥',
       N'Ketu is stated to be similar to Rahu, who has a smoky appearance, dark-blue body, a fearsome quality, a vata constitution, and intelligence.')
    ) v(PlanetCode,VerseNumber,SanskritText,EnglishText)
)
INSERT research.tbl_Dim_SourceReferencePlanetText
    (PlanetId,SourceRefCode,WorkTitle,Author,Edition,Chapter,VerseOrPage,LanguageCode,
     TextTypeCode,FullText,TranslationText,Notes,CopyrightStatus,SourceLocator,SourceUrl)
SELECT p.Id,'SRC_BPHS_GRAHA_SVARUPA',N'Bṛhat Parāśara Horā Śāstra',
       N'Parāśara (attributed)',N'Sanskrit Documents par0110.pdf; typeset 19 June 2025',
       N'3 — Grahaguṇasvarūpa',N'Printed p. 6; verse ' + CONVERT(NVARCHAR(3),v.VerseNumber),
       'sa-Deva','SanskritVerse',v.SanskritText,v.EnglishText,
       N'English is a conservative project translation from the Sanskrit; it is not copied from a published translation.',
       'PublicDomainOriginal',N'par0110.pdf, printed page 6, chapter 3',
       N'https://sanskritdocuments.org/doc_z_misc_sociology_astrology/par0110.pdf'
FROM passages v
JOIN research.tbl_Dim_SourceReferencePlanet p ON p.PlanetCode=v.PlanetCode
WHERE NOT EXISTS
(
    SELECT 1 FROM research.tbl_Dim_SourceReferencePlanetText existing
    WHERE existing.PlanetId=p.Id
      AND existing.SourceRefCode='SRC_BPHS_GRAHA_SVARUPA'
      AND existing.VerseOrPage=N'Printed p. 6; verse ' + CONVERT(NVARCHAR(3),v.VerseNumber)
);
GO

INSERT dbo.SchemaMigrations (ScriptName,Note)
SELECT '071_seed_bphs_planet_sanskrit_text.sql',
       'Seed BPHS chapter 3 printed-page-6 Sanskrit graha verses 23-30 with project English translations.'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName='071_seed_bphs_planet_sanskrit_text.sql');
GO
