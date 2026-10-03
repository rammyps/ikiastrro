using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Houses;

namespace Ikiastrro.Core.Engines.Matching;

/// <summary>A planet's stored place in a divisional chart: sign, house from that chart's Lagna, and the
/// dignity the chart pipeline recorded (null where none applies).</summary>
public sealed record VargaPoint(ZodiacName Sign, int House, string? Dignity);

/// <summary>What the Navamsa comparison reads for one person: the D1 Lagna and signs, the D9 Lagna and
/// every graha's D9 place, and the Darakaraka (the chara karaka the chart pipeline stored for D1).</summary>
public sealed record NavamsaChart(
    ZodiacName D1Lagna,
    IReadOnlyDictionary<PlanetName, ZodiacName> D1Signs,
    ZodiacName D9Lagna,
    IReadOnlyDictionary<PlanetName, VargaPoint> D9,
    PlanetName? Darakaraka);

/// <summary>One person's Navamsa facts. <see cref="Vargottama"/> lists which of the D1 7th lord, Venus,
/// Jupiter and the Darakaraka sit in the same sign in D1 and D9.</summary>
public sealed record NavamsaReading(
    ZodiacName D9Lagna,
    PlanetName D9LagnaLord,
    ZodiacName SeventhSign,
    PlanetName SeventhLord,
    VargaPoint SeventhLordPlacement,
    IReadOnlyList<PlanetName> SeventhOccupants,
    VargaPoint Venus,
    VargaPoint Jupiter,
    PlanetName? Darakaraka,
    ZodiacName? DarakarakaD1Sign,
    VargaPoint? DarakarakaD9,
    PlanetName D1SeventhLord,
    bool SameSeventhLord,
    IReadOnlyList<(string Point, PlanetName Planet)> Vargottama);

/// <summary>
/// Navamsa (D9) facts for marriage matching, facts only: the D9 Lagna and 7th house, the D9 7th lord's
/// condition, Venus and Jupiter in D9, the Darakaraka, and which of these repeat their D1 sign. No verdict
/// is given on what any of it means for a marriage: no source is cited for that yet
/// (docs/architecture/compatibility_similarity.md, section 10). Every value is read from stored
/// positions; nothing is recomputed. Pure; no I/O.
/// </summary>
public static class NavamsaCompatibility
{
    public static NavamsaReading Read(NavamsaChart chart)
    {
        var seventhSign = HouseEngine.GetHouseSign(chart.D9Lagna, 7);
        var seventhLord = Lord(seventhSign);
        var d1SeventhLord = Lord(HouseEngine.GetHouseSign(chart.D1Lagna, 7));

        var occupants = chart.D9.Where(kv => kv.Value.Sign == seventhSign).Select(kv => kv.Key).OrderBy(p => p).ToList();

        VargaPoint? dkD9 = null;
        ZodiacName? dkD1 = null;
        if (chart.Darakaraka is { } dk)
        {
            dkD9 = chart.D9[dk];
            dkD1 = chart.D1Signs[dk];
        }

        var vargottama = new List<(string, PlanetName)>();
        void Check(string label, PlanetName planet)
        {
            if (chart.D1Signs[planet] == chart.D9[planet].Sign) vargottama.Add((label, planet));
        }
        Check("D1 7th lord", d1SeventhLord);
        Check("Venus", PlanetName.Venus);
        Check("Jupiter", PlanetName.Jupiter);
        if (chart.Darakaraka is { } karaka) Check("Darakaraka", karaka);

        return new NavamsaReading(
            chart.D9Lagna, Lord(chart.D9Lagna),
            seventhSign, seventhLord, chart.D9[seventhLord], occupants,
            chart.D9[PlanetName.Venus], chart.D9[PlanetName.Jupiter],
            chart.Darakaraka, dkD1, dkD9,
            d1SeventhLord, d1SeventhLord == seventhLord, vargottama);
    }

    private static PlanetName Lord(ZodiacName sign) => Enum.Parse<PlanetName>(HouseEngine.GetSignLord(sign));
}
