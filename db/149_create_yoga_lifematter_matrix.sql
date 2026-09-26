-- 149 — Seven ranked LifeMatter paths (Area + SA1..SA6) per yoga source variant.
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS(SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName='149_create_yoga_lifematter_matrix.sql')
BEGIN
    CREATE TABLE dbo.tbl_Rule_YogaLifeMatterPath
    (
        Id BIGINT IDENTITY(1,1) CONSTRAINT PK_Rule_YogaLifeMatterPath PRIMARY KEY,
        RuleSetId TINYINT NOT NULL CONSTRAINT FK_YogaLifeMatterPath_RuleSet REFERENCES dbo.tbl_Rule_Sets(Id),
        YogaVariantId INT NOT NULL CONSTRAINT FK_YogaLifeMatterPath_Variant REFERENCES dbo.tbl_Rule_YogaVariant(Id),
        PathRank TINYINT NOT NULL,
        LifeMatterFocusId INT NOT NULL CONSTRAINT FK_YogaLifeMatterPath_Focus REFERENCES dbo.tbl_Rule_LifeMatterFocus(Id),
        RelevanceScore DECIMAL(7,6) NOT NULL,
        SemanticScore DECIMAL(7,6) NOT NULL,
        PriorityScore DECIMAL(7,6) NOT NULL,
        MappingMethodCode VARCHAR(30) NOT NULL,
        ReviewStatusCode VARCHAR(20) NOT NULL CONSTRAINT DF_YogaLifeMatterPath_Review DEFAULT('PROPOSED'),
        Notes NVARCHAR(500) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_YogaLifeMatterPath_Active DEFAULT(1),
        UpdatedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_YogaLifeMatterPath_Updated DEFAULT(SYSUTCDATETIME()),
        CONSTRAINT UQ_YogaLifeMatterPath_Rank UNIQUE(RuleSetId,YogaVariantId,PathRank),
        CONSTRAINT UQ_YogaLifeMatterPath_Focus UNIQUE(RuleSetId,YogaVariantId,LifeMatterFocusId),
        CONSTRAINT CK_YogaLifeMatterPath_Rank CHECK(PathRank BETWEEN 1 AND 7),
        CONSTRAINT CK_YogaLifeMatterPath_Scores CHECK(RelevanceScore BETWEEN 0 AND 1 AND SemanticScore BETWEEN 0 AND 1 AND PriorityScore BETWEEN 0 AND 1),
        CONSTRAINT CK_YogaLifeMatterPath_Method CHECK(MappingMethodCode IN('TFIDF_V1','MANUAL','SOURCE_DIRECT')),
        CONSTRAINT CK_YogaLifeMatterPath_Review CHECK(ReviewStatusCode IN('PROPOSED','REVIEWED','VERIFIED','REJECTED'))
    );

    CREATE INDEX IX_YogaLifeMatterPath_Focus ON dbo.tbl_Rule_YogaLifeMatterPath(LifeMatterFocusId,ReviewStatusCode);
    INSERT dbo.SchemaMigrations(ScriptName,Note) VALUES
    ('149_create_yoga_lifematter_matrix.sql','Yoga-specific 7x7 matrix: seven ranked paths per source variant; each path references tbl_Rule_LifeMatterFocus and exposes Area through SA6.');
END
GO

CREATE OR ALTER VIEW dbo.vw_YogaLifeMatter7x7
AS
SELECT p.RuleSetId,v.SourceRefCode,v.SourceVariantCode,v.YogaCode,p.PathRank,
       COALESCE(lmr.CategoryName,lm.CategoryName,N'General') AS Area,
       COALESCE(lmr.MatterText,lm.EnglishName) AS SubArea1,
       COALESCE(ds.SubjectName,N'Natal promise') AS SubArea2,
       f.FocusKind AS SubArea3,
       f.ReferenceCode AS SubArea4,
       CASE WHEN f.FocusKind='House' THEN CONCAT(N'House ',f.HouseNumber)
            ELSE COALESCE(f.SpecialPointCode,f.ReferenceCode) END AS SubArea5,
       COALESCE(k.Karakas,N'No specific karaka') AS SubArea6,
       p.LifeMatterFocusId,p.RelevanceScore,p.SemanticScore,p.PriorityScore,
       p.MappingMethodCode,p.ReviewStatusCode,p.Notes
FROM dbo.tbl_Rule_YogaLifeMatterPath p
JOIN dbo.tbl_Rule_YogaVariant v ON v.Id=p.YogaVariantId
JOIN dbo.tbl_Rule_LifeMatterFocus f ON f.Id=p.LifeMatterFocusId
JOIN dbo.tbl_Dim_LifeMatter lm ON lm.Id=f.LifeMatterId
LEFT JOIN dbo.tbl_Rule_LifeMatterReference lmr ON lmr.RuleSetId=p.RuleSetId AND lmr.LifeMatterId=lm.Id AND lmr.IsActive=1
LEFT JOIN dbo.tbl_Rule_LifeMatterSubject lms ON lms.RuleSetId=p.RuleSetId AND lms.LifeMatterId=lm.Id AND lms.IsActive=1
LEFT JOIN dbo.tbl_Dim_DivisionalSubject ds ON ds.SubjectCode=lms.DivisionalSubjectCode
OUTER APPLY
(
    SELECT STRING_AGG(CONVERT(NVARCHAR(MAX),q.KarakaName),N', ') WITHIN GROUP(ORDER BY q.DisplayOrder,q.KarakaName) AS Karakas
    FROM
    (
        SELECT DISTINCT km.DisplayOrder,
               CASE WHEN kr.KarakaTypeCode='CHARA' THEN kr.CharaKarakaCode
                    ELSE pl.PlanetName END AS KarakaName
        FROM dbo.tbl_Rule_KarakaMatter km
        JOIN dbo.tbl_Dim_KarakaRole kr ON kr.Id=km.KarakaRoleId
        LEFT JOIN dbo.tbl_Planets pl ON pl.Id=kr.FixedGrahaId
        WHERE km.RuleSetId=p.RuleSetId AND km.LifeMatterId=lm.Id AND km.IsActive=1
    ) q
) k
WHERE p.IsActive=1 AND p.ReviewStatusCode<>'REJECTED';
GO

CREATE OR ALTER VIEW dbo.vw_YogaLifeMatterMatrixCoverage
AS
SELECT v.RuleSetId,v.SourceRefCode,v.SourceVariantCode,v.YogaCode,
       COUNT(p.Id) AS PathCount,
       SUM(CASE WHEN p.ReviewStatusCode='VERIFIED' THEN 1 ELSE 0 END) AS VerifiedPathCount,
       CASE WHEN COUNT(p.Id)=7 THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS HasComplete7x7
FROM dbo.tbl_Rule_YogaVariant v
LEFT JOIN dbo.tbl_Rule_YogaLifeMatterPath p ON p.YogaVariantId=v.Id AND p.RuleSetId=v.RuleSetId AND p.IsActive=1
WHERE v.IsActive=1
GROUP BY v.RuleSetId,v.SourceRefCode,v.SourceVariantCode,v.YogaCode;
GO
