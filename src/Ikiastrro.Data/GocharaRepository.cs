using Dapper;
using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Models;

namespace Ikiastrro.Data;

/// <summary>Sidereal positions used by the natal/transit comparison wheel. Exact longitudes and
/// speeds are calculated for the selected instant; stored boundary metadata is added where the
/// slow-planet transit history supplies it.</summary>
public sealed class GocharaRepository
{
    private static readonly PlanetName[] WheelPlanets = Enum.GetValues<PlanetName>();
    private readonly PlanetSignTransitEventsRepository _transits;
    private readonly SqlConnectionFactory _connectionFactory;

    public GocharaRepository(PlanetSignTransitEventsRepository transits, SqlConnectionFactory connectionFactory) { _transits = transits; _connectionFactory = connectionFactory; }

    public IReadOnlyList<PlanetTransitSnapshot> GetSnapshots(DateTime asOfUtc)
    {
        var utc = DateTime.SpecifyKind(asOfUtc, DateTimeKind.Utc);
        var positions = SwissEphemerisProvider.GetSiderealPositions(
            new DateTimeOffset(utc, TimeSpan.Zero), 0, 0);
        var result = new List<PlanetTransitSnapshot>();
        foreach (var planet in WheelPlanets)
        {
            var stored = _transits.GetSnapshot(planet, utc);
            if (!positions.PlanetLongitudes.TryGetValue(planet, out var longitude)) continue;
            var degree = longitude % 30.0;
            if (degree < 0) degree += 30.0;
            var nakshatra = (byte)(Math.Floor(longitude / (360.0 / 27.0)) + 1);
            var pada = (byte)(Math.Floor((longitude % (360.0 / 27.0)) / (360.0 / 108.0)) + 1);
            var speed = positions.PlanetSpeeds[planet];
            result.Add(new PlanetTransitSnapshot(planet,
                (byte)(Math.Floor(longitude / 30.0) + 1),
                stored?.InSignSinceUtc ?? utc,
                speed < 0 ? "Retrograde" : "Direct",
                stored?.NextChangeUtc,
                longitude, degree, nakshatra, pada, speed, positions.AyanamsaCode,
                stored?.InSignMotion, stored?.NextChangeMotion));
        }
        SaveSnapshots(utc, result.Where(s =>
            s.Planet is PlanetName.Saturn or PlanetName.Jupiter or PlanetName.Rahu or PlanetName.Ketu));
        return result;
    }
    public void SaveSnapshots(DateTime asOfUtc, IEnumerable<PlanetTransitSnapshot> snapshots)
    {
        const string sql = "MERGE dbo.tbl_TransitPositionReference AS t USING (SELECT @PlanetId PlanetId, @AsOfUtc AsOfUtc) s ON t.PlanetId=s.PlanetId AND t.AsOfUtc=s.AsOfUtc WHEN MATCHED THEN UPDATE SET SignId=@SignId, LongitudeDegrees=@LongitudeDegrees, DegreeInSign=@DegreeInSign, NakshatraId=@NakshatraId, Pada=@Pada, SpeedDegreesPerDay=@SpeedDegreesPerDay, MotionDirection=@MotionDirection, AyanamsaRuleId=@AyanamsaRuleId, InSignSinceUtc=@InSignSinceUtc, NextChangeUtc=@NextChangeUtc, InSignMotion=@InSignMotion, NextChangeMotion=@NextChangeMotion WHEN NOT MATCHED THEN INSERT (PlanetId,AsOfUtc,SignId,LongitudeDegrees,DegreeInSign,NakshatraId,Pada,SpeedDegreesPerDay,MotionDirection,AyanamsaRuleId,InSignSinceUtc,NextChangeUtc,InSignMotion,NextChangeMotion) VALUES (@PlanetId,@AsOfUtc,@SignId,@LongitudeDegrees,@DegreeInSign,@NakshatraId,@Pada,@SpeedDegreesPerDay,@MotionDirection,@AyanamsaRuleId,@InSignSinceUtc,@NextChangeUtc,@InSignMotion,@NextChangeMotion);";
        using var connection = _connectionFactory.CreateOpenConnection();
        connection.Execute(sql, snapshots.Select(s => new { PlanetId=(int)s.Planet+1, AsOfUtc=DateTime.SpecifyKind(asOfUtc, DateTimeKind.Utc), SignId=(int)s.SignId, s.LongitudeDegrees, s.DegreeInSign, NakshatraId=(int)s.NakshatraId, Pada=(int)s.Pada, s.SpeedDegreesPerDay, s.MotionDirection, AyanamsaRuleId=7, s.InSignSinceUtc, s.NextChangeUtc, s.InSignMotion, s.NextChangeMotion }));
    }
}
