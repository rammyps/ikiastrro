USE [ikiastrro];
GO
SET NOCOUNT ON;
IF OBJECT_ID('dbo.tbl_Fact_GrahaDrishtiStrengths', 'U') IS NULL THROW 50132, 'tbl_Fact_GrahaDrishtiStrengths is missing.', 1;
IF OBJECT_ID('dbo.vw_ChartGrahaDrishtiStrengths', 'V') IS NULL THROW 50132, 'vw_ChartGrahaDrishtiStrengths is missing.', 1;
IF EXISTS (SELECT 1 FROM dbo.vw_ChartGrahaDrishtiStrengths WHERE ChartType NOT IN ('D1','D9','D10')) THROW 50132, 'Graha-drishti strengths exist outside D1/D9/D10.', 1;
IF EXISTS (SELECT ChartResultId FROM dbo.vw_ChartGrahaDrishtiStrengths GROUP BY ChartResultId HAVING COUNT(*) <> 81) THROW 50132, 'A populated chart does not contain the expected 9 x 9 directed matrix.', 1;
IF EXISTS (SELECT 1 FROM dbo.tbl_Fact_GrahaDrishtiStrengths WHERE DirectedSeparationDegrees < 0 OR DirectedSeparationDegrees >= 360 OR AspectingLongitudeDegrees < 0 OR AspectingLongitudeDegrees >= 360 OR AspectedLongitudeDegrees < 0 OR AspectedLongitudeDegrees >= 360 OR StrengthPercentage < 0 OR StrengthPercentage > 100) THROW 50132, 'Graha-drishti strength contains an out-of-range value.', 1;
SELECT ChartType, COUNT(*) AS Rows, COUNT(DISTINCT ChartResultId) AS Charts FROM dbo.vw_ChartGrahaDrishtiStrengths GROUP BY ChartType ORDER BY ChartType;
PRINT 'PASS: Graha-drishti strength facts verified.';
GO