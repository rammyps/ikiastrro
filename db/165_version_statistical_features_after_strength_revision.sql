-- =====================================================================
-- 165 - v10 feature-contract v2 after strength-engine revision f5ac02e.
--
-- Saptavargaja, Oja-Yugma and D2/D3/D7 scheme corrections change the
-- canonical inputs behind Capacity / Consistency. Preserve v1 history and
-- admit v2 codes; never pool the two contracts in one dataset.
-- =====================================================================
USE [ikiastrro];
GO

IF OBJECT_ID('dbo.tbl_Fact_StatisticalComparisons', 'U') IS NOT NULL
BEGIN
    IF EXISTS (
        SELECT 1 FROM sys.check_constraints
        WHERE parent_object_id = OBJECT_ID('dbo.tbl_Fact_StatisticalComparisons')
          AND name = 'CK_Fact_StatisticalComparisons_Feature')
        ALTER TABLE dbo.tbl_Fact_StatisticalComparisons
            DROP CONSTRAINT CK_Fact_StatisticalComparisons_Feature;

    ALTER TABLE dbo.tbl_Fact_StatisticalComparisons WITH CHECK
        ADD CONSTRAINT CK_Fact_StatisticalComparisons_Feature CHECK (FeatureCode IN
            ('KI_D1_HOUSE_CAPACITY_V1','KI_D1_HOUSE_CONSISTENCY_V1',
             'KI_D1_HOUSE_CONTEXT_V1','KI_D1_HOUSE_SUPPORT_V1',
             'KI_D1_HOUSE_CAPACITY_V2','KI_D1_HOUSE_CONSISTENCY_V2',
             'KI_D1_HOUSE_CONTEXT_V2','KI_D1_HOUSE_SUPPORT_V2'));
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '165_version_statistical_features_after_strength_revision.sql')
    INSERT dbo.SchemaMigrations (ScriptName, AppliedAtUtc, Note)
    VALUES ('165_version_statistical_features_after_strength_revision.sql', SYSUTCDATETIME(),
            'Admit v2 D1 house-strength feature codes after f5ac02e; retain v1 history without pooling.');
GO

PRINT '165 applied: statistical feature contract v2 admitted.';
GO
