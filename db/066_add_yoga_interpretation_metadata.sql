/* Extend executable yoga rules with explicit interpretation and context axes. */
IF OBJECT_ID(N'dbo.tbl_Rule_Yoga',N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.tbl_Rule_Yoga',N'ExclusionConditionsJson') IS NULL ALTER TABLE dbo.tbl_Rule_Yoga ADD ExclusionConditionsJson NVARCHAR(MAX) NULL;
    IF COL_LENGTH(N'dbo.tbl_Rule_Yoga',N'StrengthConditionsJson') IS NULL ALTER TABLE dbo.tbl_Rule_Yoga ADD StrengthConditionsJson NVARCHAR(MAX) NULL;
    IF COL_LENGTH(N'dbo.tbl_Rule_Yoga',N'TimingConditionsJson') IS NULL ALTER TABLE dbo.tbl_Rule_Yoga ADD TimingConditionsJson NVARCHAR(MAX) NULL;
    IF COL_LENGTH(N'dbo.tbl_Rule_Yoga',N'PrimaryOutcomeCode') IS NULL ALTER TABLE dbo.tbl_Rule_Yoga ADD PrimaryOutcomeCode VARCHAR(80) NULL;
    IF COL_LENGTH(N'dbo.tbl_Rule_Yoga',N'SecondaryOutcomeCode') IS NULL ALTER TABLE dbo.tbl_Rule_Yoga ADD SecondaryOutcomeCode VARCHAR(80) NULL;
    IF COL_LENGTH(N'dbo.tbl_Rule_Yoga',N'AdverseOutcomeCode') IS NULL ALTER TABLE dbo.tbl_Rule_Yoga ADD AdverseOutcomeCode VARCHAR(80) NULL;
    IF COL_LENGTH(N'dbo.tbl_Rule_Yoga',N'EvidencePolicyCode') IS NULL ALTER TABLE dbo.tbl_Rule_Yoga ADD EvidencePolicyCode VARCHAR(30) NULL;
    IF COL_LENGTH(N'dbo.tbl_Rule_Yoga',N'ConfidenceCode') IS NULL ALTER TABLE dbo.tbl_Rule_Yoga ADD ConfidenceCode VARCHAR(30) NULL;
    IF COL_LENGTH(N'dbo.tbl_Rule_Yoga',N'ApplicabilityScopeCode') IS NULL ALTER TABLE dbo.tbl_Rule_Yoga ADD ApplicabilityScopeCode VARCHAR(30) NULL;
    IF COL_LENGTH(N'dbo.tbl_Rule_Yoga',N'InterpretationStatusCode') IS NULL ALTER TABLE dbo.tbl_Rule_Yoga ADD InterpretationStatusCode VARCHAR(20) NULL;
END;
GO
IF OBJECT_ID(N'dbo.tbl_Rule_Yoga',N'U') IS NOT NULL
BEGIN
    IF OBJECT_ID(N'dbo.CK_Rule_Yoga_Json_Interpretation',N'C') IS NULL
        ALTER TABLE dbo.tbl_Rule_Yoga ADD CONSTRAINT CK_Rule_Yoga_Json_Interpretation CHECK
        ((ExclusionConditionsJson IS NULL OR ISJSON(ExclusionConditionsJson)=1) AND
         (StrengthConditionsJson IS NULL OR ISJSON(StrengthConditionsJson)=1) AND
         (TimingConditionsJson IS NULL OR ISJSON(TimingConditionsJson)=1));
    IF OBJECT_ID(N'dbo.CK_Rule_Yoga_InterpretationStatus',N'C') IS NULL
        ALTER TABLE dbo.tbl_Rule_Yoga ADD CONSTRAINT CK_Rule_Yoga_InterpretationStatus CHECK
        (InterpretationStatusCode IS NULL OR InterpretationStatusCode IN ('Proposed','Researched','Reviewed','Implemented','Verified','Deprecated','Disputed'));
    IF OBJECT_ID(N'dbo.CK_Rule_Yoga_Confidence',N'C') IS NULL
        ALTER TABLE dbo.tbl_Rule_Yoga ADD CONSTRAINT CK_Rule_Yoga_Confidence CHECK
        (ConfidenceCode IS NULL OR ConfidenceCode IN ('DirectClassical','CrossSourceAgreement','ModernSynthesis','Interpretive','Disputed'));
END;
GO
IF OBJECT_ID(N'dbo.SchemaMigrations',N'U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName=N'066_add_yoga_interpretation_metadata.sql')
INSERT dbo.SchemaMigrations(ScriptName,Note) VALUES(N'066_add_yoga_interpretation_metadata.sql',N'Add explicit yoga exclusion, strength, timing, outcome, evidence and interpretation metadata.');
