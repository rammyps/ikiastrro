-- Read-only verification for migration 167.
SET NOCOUNT ON;

SELECT c.Code, c.ClaimTypeCode, c.StatementFormCode, c.StatementText,
       c.SourceRefCode, c.SourceLocator, c.VerificationStatus,
       p.PlanetName AS KarakaPlanet
FROM dbo.tbl_Rule_LifeMatterClaim c
LEFT JOIN dbo.tbl_Dim_KarakaRole kr ON kr.Id = c.KarakaRoleId
LEFT JOIN dbo.tbl_Planets p ON p.Id = kr.FixedGrahaId
ORDER BY c.DisplayOrder, c.Code;

SELECT c.Code, dm.Code AS LifeMatterCode, dm.EnglishName,
       s.DivisionalSubjectCode, s.ScopeNote
FROM dbo.tbl_Rule_LifeMatterClaimScope s
JOIN dbo.tbl_Rule_LifeMatterClaim c ON c.Id = s.LifeMatterClaimId
LEFT JOIN dbo.tbl_Dim_LifeMatter dm ON dm.Id = s.LifeMatterId
ORDER BY c.Code, s.DisplayOrder;

SELECT
    CASE WHEN EXISTS (
        SELECT 1 FROM dbo.tbl_Rule_LifeMatterClaim c
        WHERE c.Code = 'PVR_SATURN_LIVELIHOOD_KARMA'
          AND c.StatementText = N'Saturn is the significator of livelihood and karma.'
          AND c.VerificationStatus = 'LOCATOR_PENDING'
    ) THEN 'PASS' ELSE 'FAIL' END AS SaturnStatementCaptured,
    CASE WHEN NOT EXISTS (
        SELECT 1
        FROM dbo.tbl_Rule_LifeMatterClaim c
        LEFT JOIN dbo.tbl_Dim_Source src ON src.Code = c.SourceRefCode
        WHERE src.Code IS NULL
    ) THEN 'PASS' ELSE 'FAIL' END AS EverySourceResolves,
    CASE WHEN NOT EXISTS (
        SELECT 1 FROM dbo.tbl_Rule_LifeMatterClaimScope s
        WHERE (s.LifeMatterId IS NULL AND s.DivisionalSubjectCode IS NULL)
           OR (s.LifeMatterId IS NOT NULL AND s.DivisionalSubjectCode IS NOT NULL)
    ) THEN 'PASS' ELSE 'FAIL' END AS EveryScopeHasOneTarget;
