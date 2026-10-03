using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Models;

namespace Ikiastrro.Core.Engines.Dasha;

/// <summary>PVR ch.17 Aṣṭottarī: eight lords, 108-year cycle, Mahādaśā and Antardaśā.</summary>
public static class AshtottariDashaCalculator
{
    public const string ChartType = "AshtottariDasha";
    private const double DaysPerYear = 365.2425;
    private const int CycleYears = 108;

    private static readonly PlanetName[] Lords =
        [PlanetName.Sun, PlanetName.Moon, PlanetName.Mars, PlanetName.Mercury,
         PlanetName.Saturn, PlanetName.Jupiter, PlanetName.Rahu, PlanetName.Venus];

    private static readonly int[] Years = [6, 15, 8, 17, 10, 19, 12, 21];
    private static readonly double[] ArcStarts =
        [66d + 40d / 60d, 120d, 160d, 213d + 20d / 60d, 253d + 20d / 60d,
         293d + 20d / 60d, 333d + 20d / 60d, 26d + 40d / 60d];
    private static readonly double[] ArcLengths =
        [53d + 20d / 60d, 40d, 53d + 20d / 60d, 40d, 40d, 40d, 53d + 20d / 60d, 40d];

    public static List<DashaPeriod> Compute(BirthDetails birth, double minimumCoverageYears = 108,
        AyanamsaDefinition? ayanamsa = null)
    {
        var moment = BirthMomentFactory.Create(birth);
        var positions = SwissEphemerisProvider.GetSiderealPositions(moment, birth.Latitude, birth.Longitude, ayanamsa);
        return ComputeFromMoonLongitude(moment, positions.PlanetLongitudes[PlanetName.Moon], minimumCoverageYears);
    }

    public static List<DashaPeriod> ComputeFromMoonLongitude(
        DateTimeOffset birth,
        double moonLongitude,
        double minimumCoverageYears = 108)
    {
        var longitude = ((moonLongitude % 360d) + 360d) % 360d;
        var firstLordIndex = FindArc(longitude);
        var distanceIntoArc = ForwardDistance(ArcStarts[firstLordIndex], longitude);
        var elapsedMahaYears = distanceIntoArc / ArcLengths[firstLordIndex] * Years[firstLordIndex];
        var (firstAntarSlot, elapsedAntarYears) = FindAntarSlot(firstLordIndex, elapsedMahaYears);

        var roots = new List<DashaPeriod>();
        var cursorDays = 0d;
        for (var mahaOffset = 0; cursorDays < minimumCoverageYears * DaysPerYear; mahaOffset++)
        {
            var lordIndex = (firstLordIndex + mahaOffset) % Lords.Length;
            var firstMaha = mahaOffset == 0;
            var fullYears = (double)Years[lordIndex];
            var remainingYears = firstMaha ? fullYears - elapsedMahaYears : fullYears;
            var startDays = cursorDays;
            var endDays = startDays + remainingYears * DaysPerYear;
            var maha = Period(1, lordIndex + 1, Lords[lordIndex], birth, startDays, endDays);
            roots.Add(maha);

            var antarCursor = startDays;
            var firstSlot = firstMaha ? firstAntarSlot : 0;
            for (var slot = firstSlot; slot < Lords.Length; slot++)
            {
                // PVR: the first antardaśā is the lord after the Mahādaśā lord; its own lord is last.
                var antarLordIndex = (lordIndex + slot + 1) % Lords.Length;
                var antarFullYears = fullYears * Years[antarLordIndex] / CycleYears;
                var antarRemaining = firstMaha && slot == firstSlot
                    ? antarFullYears - elapsedAntarYears
                    : antarFullYears;
                var antarEnd = antarCursor + antarRemaining * DaysPerYear;
                maha.Children.Add(Period(2, slot + 1, Lords[antarLordIndex], birth, antarCursor, antarEnd));
                antarCursor = antarEnd;
            }

            cursorDays = endDays;
        }
        return roots;
    }

    private static int FindArc(double longitude)
    {
        for (var i = 0; i < ArcStarts.Length; i++)
            if (ForwardDistance(ArcStarts[i], longitude) < ArcLengths[i] - 1e-9)
                return i;
        throw new InvalidOperationException("The eight Ashtottari arcs must cover the zodiac.");
    }

    private static (int Slot, double ElapsedWithinSlotYears) FindAntarSlot(int mahaLordIndex, double elapsedYears)
    {
        var cumulative = 0d;
        for (var slot = 0; slot < Lords.Length; slot++)
        {
            var antarLordIndex = (mahaLordIndex + slot + 1) % Lords.Length;
            var duration = Years[mahaLordIndex] * Years[antarLordIndex] / (double)CycleYears;
            if (elapsedYears < cumulative + duration || slot == Lords.Length - 1)
                return (slot, elapsedYears - cumulative);
            cumulative += duration;
        }
        throw new InvalidOperationException("Unreachable Aṣṭottarī Antardaśā slot.");
    }

    private static double ForwardDistance(double start, double end)
    {
        var distance = ((end - start) % 360d + 360d) % 360d;
        // Decimal degree boundaries such as 293°20′ can differ by one floating-point ulp.
        // Treat the resulting almost-360° distance as the intended exact boundary (zero).
        return distance > 360d - 1e-9 ? 0d : distance;
    }

    private static DashaPeriod Period(int level, int sequence, PlanetName lord, DateTimeOffset birth, double startDays, double endDays) => new()
    {
        LevelNumber = level,
        SequenceInParent = sequence,
        Lord = lord,
        StartDate = birth.AddDays(startDays),
        EndDate = birth.AddDays(endDays),
        StartDayOffset = (int)Math.Round(startDays),
        EndDayOffset = Math.Max((int)Math.Round(startDays), (int)Math.Round(endDays) - 1)
    };
}
