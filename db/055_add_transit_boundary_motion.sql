USE [ikiastrro];
GO

IF COL_LENGTH('dbo.tbl_TransitPositionReference', 'InSignMotion') IS NULL
    ALTER TABLE dbo.tbl_TransitPositionReference ADD InSignMotion VARCHAR(10) NULL;
IF COL_LENGTH('dbo.tbl_TransitPositionReference', 'NextChangeMotion') IS NULL
    ALTER TABLE dbo.tbl_TransitPositionReference ADD NextChangeMotion VARCHAR(10) NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_TransitPositionReference_InSignMotion')
    ALTER TABLE dbo.tbl_TransitPositionReference ADD CONSTRAINT CK_TransitPositionReference_InSignMotion
        CHECK (InSignMotion IS NULL OR InSignMotion IN ('Direct', 'Retrograde', 'Stationary'));
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_TransitPositionReference_NextChangeMotion')
    ALTER TABLE dbo.tbl_TransitPositionReference ADD CONSTRAINT CK_TransitPositionReference_NextChangeMotion
        CHECK (NextChangeMotion IS NULL OR NextChangeMotion IN ('Direct', 'Retrograde', 'Stationary'));
GO

-- Refresh existing rows from the sign-boundary event history. Ketu shares Rahu's
-- boundary timestamps and motion direction, while its sign is derived opposite Rahu.
UPDATE target
SET InSignMotion = ingress.MotionDirection,
    NextChangeMotion = nextEvent.MotionDirection
FROM dbo.tbl_TransitPositionReference target
CROSS APPLY
(
    SELECT TOP (1) eventRow.MotionDirection
    FROM dbo.tbl_PlanetSignTransitEvents eventRow
    WHERE eventRow.PlanetId = CASE WHEN target.PlanetId = 9 THEN 8 ELSE target.PlanetId END
      AND eventRow.EventDateTimeUtc <= target.AsOfUtc
    ORDER BY eventRow.EventDateTimeUtc DESC
) ingress
OUTER APPLY
(
    SELECT TOP (1) eventRow.MotionDirection
    FROM dbo.tbl_PlanetSignTransitEvents eventRow
    WHERE eventRow.PlanetId = CASE WHEN target.PlanetId = 9 THEN 8 ELSE target.PlanetId END
      AND eventRow.EventDateTimeUtc > target.AsOfUtc
    ORDER BY eventRow.EventDateTimeUtc
) nextEvent
WHERE target.InSignMotion IS NULL OR target.NextChangeMotion IS NULL;
GO

INSERT dbo.SchemaMigrations (ScriptName, Note)
SELECT '055_add_transit_boundary_motion.sql',
       'Add ingress and next-sign-change motion directions to tbl_TransitPositionReference for the V2 Transit page.'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations
                  WHERE ScriptName = '055_add_transit_boundary_motion.sql');
GO
