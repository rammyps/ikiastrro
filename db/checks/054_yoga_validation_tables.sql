USE ikiastrro;
GO

SELECT Code, DisplayName, ProductVersion, EngineFamily, LicenseCode, IsActive
FROM dbo.tbl_Dim_ValidationSystems
ORDER BY Code, ProductVersion;

SELECT vs.Code, vs.ProductVersion, COUNT(*) AS Definitions,
       SUM(CASE WHEN m.ReviewStatus = 'REVIEWED' THEN 1 ELSE 0 END) AS ReviewedMappings,
       SUM(CASE WHEN m.MappingStatus = 'UNMAPPED' THEN 1 ELSE 0 END) AS UnmappedDefinitions
FROM dbo.tbl_Rule_YogaValidationDefinition d
JOIN dbo.tbl_Dim_ValidationSystems vs ON vs.Id = d.ValidationSystemId
LEFT JOIN dbo.tbl_Dim_YogaValidationMappings m ON m.YogaValidationDefinitionId = d.Id
GROUP BY vs.Code, vs.ProductVersion
ORDER BY vs.Code, vs.ProductVersion;

SELECT ComparisonStatus, COUNT(*) AS Results
FROM dbo.vw_YogaValidationComparison
GROUP BY ComparisonStatus
ORDER BY ComparisonStatus;
GO

