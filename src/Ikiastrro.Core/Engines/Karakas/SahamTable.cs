using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Houses;

namespace Ikiastrro.Core.Engines.Karakas;

/// <summary>
/// The 36 sahams JHora lists, in its order. Each is A − B + C with the +30° arc correction
/// (<see cref="SahamCalculator"/>); at night most swap A and B. Formulas follow PVR ch.28.8 Table 74 and
/// PyJHora's <c>saham.py</c> (AGPL — formulas only), checked against JHora's own "Sahamas" view for a night
/// birth (1_Ramakrishnan, all 36 within an arcsecond of input rounding). Day-birth branches are the mirror of
/// the night ones and follow the same sources; no golden day chart yet.
/// <list type="bullet">
/// <item>Same by day and night: Bhratṛ, Roga, Mṛtyu, Paradeśa, Artha, Vyāpāra, Lābha.</item>
/// <item>Third term other than the Lagna: Mitra (Venus), Gaurava (Sun), Śāstra (Mercury),
/// Karyasiddhi (lord of the luminary's sign), Santāpa (6th cusp), Jāḍya (Mercury).</item>
/// <item>Śāstra, Karyasiddhi, Santāpa and Prīti test the +30° arc against the Lagna, not their third term.</item>
/// <item>Samartha uses the Lagna lord, or Jupiter in Mars's place when Mars rules the Lagna.</item>
/// </list>
/// </summary>
public static class SahamTable
{
    private static readonly (string Code, string Name, string Meaning)[] Catalogue =
    {
        ("PUNYA", "Punya Saham", "Fortune, good deeds"), ("VIDYA", "Vidya Saham", "Learning"),
        ("YASAS", "Yasas Saham", "Fame"), ("MITRA", "Mitra Saham", "Friend"),
        ("MAHATMYA", "Mahatmya Saham", "Greatness"), ("ASHA", "Asha Saham", "Hopes, desires"),
        ("SAMARTHA", "Samartha Saham", "Enterprise"), ("BHRATRU", "Bhratru Saham", "Brother"),
        ("GAURAVA", "Gaurava Saham", "Respect"), ("PITRU", "Pitru Saham", "Father"),
        ("RAJYA", "Rajya Saham", "Kingdom"), ("MATRU", "Matru Saham", "Mother"),
        ("PUTRA", "Putra Saham", "Children"), ("JEEVA", "Jeeva Saham", "Life"),
        ("KARMA", "Karma Saham", "Activities (profession)"), ("ROGA", "Roga Saham", "Diseases"),
        ("KALI", "Kali Saham", "Misfortune"), ("SASTRA", "Sastra Saham", "Sciences"),
        ("BANDHU", "Bandhu Saham", "Relatives"), ("MRITYU", "Mrityu Saham", "Death"),
        ("PARADESA", "Paradesa Saham", "Foreign countries"), ("ARTHA", "Artha Saham", "Wealth"),
        ("PARADARA", "Paradara Saham", "Adultery"), ("VANIK", "Vanik Saham", "Trade"),
        ("KARYASIDDHI", "Karyasiddhi Saham", "Fructification of projects"), ("VIVAHA", "Vivaha Saham", "Marriage"),
        ("SANTAPA", "Santapa Saham", "Sadness"), ("SRADDHA", "Sraddha Saham", "Dedication"),
        ("PREETI", "Preeti Saham", "Love, affection"), ("JADYA", "Jadya Saham", "Sluggishness"),
        ("VYAPARA", "Vyapara Saham", "Business"), ("SATRU", "Satru Saham", "Enemies"),
        ("JALAPATANA", "Jalapatana Saham", "Crossing ocean"), ("BANDHANA", "Bandhana Saham", "Imprisonment"),
        ("APAMRITYU", "Apamrityu Saham", "Untimely death (bad death)"), ("LABHA", "Labha Saham", "Gains"),
    };

    /// <summary>All 36 sahams for one chart. <paramref name="longitudes"/> needs the seven grahas; D1 sidereal.</summary>
    public static IReadOnlyList<NatalSaham> Compute(
        IReadOnlyDictionary<PlanetName, double> longitudes, double lagna, bool isNightBirth)
    {
        double P(PlanetName p) => longitudes[p];
        double Cusp(int house) => AstroMath.Normalize(lagna + (house - 1) * 30);
        var lagnaSign = (int)(AstroMath.Normalize(lagna) / 30);
        PlanetName LordOfHouse(int house) =>
            Enum.Parse<PlanetName>(HouseEngine.GetSignLord((ZodiacName)((lagnaSign + house - 1) % 12)));

        // A − B + C by day, A and B swapped by night; the arc test uses `check` (default C).
        double Sw(double a, double b, double c, double? check = null) =>
            isNightBirth
                ? SahamCalculator.Evaluate(b, a, c, check ?? c)
                : SahamCalculator.Evaluate(a, b, c, check ?? c);
        double Fixed(double a, double b, double c) => SahamCalculator.Evaluate(a, b, c);

        var sun = P(PlanetName.Sun); var moon = P(PlanetName.Moon); var mars = P(PlanetName.Mars);
        var mercury = P(PlanetName.Mercury); var jupiter = P(PlanetName.Jupiter);
        var venus = P(PlanetName.Venus); var saturn = P(PlanetName.Saturn);

        var punya = Sw(moon, sun, lagna);
        var sastra = Sw(jupiter, saturn, mercury, lagna);
        var pitru = Sw(saturn, sun, lagna);

        var lagnaLord = LordOfHouse(1);
        var samartha = lagnaLord == PlanetName.Mars
            ? Sw(jupiter, mars, lagna)
            : Sw(mars, P(lagnaLord), lagna);

        var luminary = isNightBirth ? PlanetName.Moon : PlanetName.Sun;
        var luminarySign = (int)(AstroMath.Normalize(P(luminary)) / 30);
        var luminaryLord = Enum.Parse<PlanetName>(HouseEngine.GetSignLord((ZodiacName)luminarySign));
        var karyasiddhi = SahamCalculator.Evaluate(saturn, P(luminary), P(luminaryLord), lagna);

        var values = new Dictionary<string, double>
        {
            ["PUNYA"] = punya,
            ["VIDYA"] = Sw(sun, moon, lagna),
            ["YASAS"] = Sw(jupiter, punya, lagna),
            ["MITRA"] = Sw(jupiter, punya, venus),
            ["MAHATMYA"] = Sw(punya, mars, lagna),
            ["ASHA"] = Sw(saturn, mars, lagna),
            ["SAMARTHA"] = samartha,
            ["BHRATRU"] = Fixed(jupiter, saturn, lagna),
            ["GAURAVA"] = Sw(jupiter, moon, sun),
            ["PITRU"] = pitru,
            ["RAJYA"] = pitru,
            ["MATRU"] = Sw(moon, venus, lagna),
            ["PUTRA"] = Sw(jupiter, moon, lagna),
            ["JEEVA"] = Sw(saturn, jupiter, lagna),
            ["KARMA"] = Sw(mars, mercury, lagna),
            ["ROGA"] = Fixed(lagna, moon, lagna),
            ["KALI"] = Sw(jupiter, mars, lagna),
            ["SASTRA"] = sastra,
            ["BANDHU"] = Sw(mercury, moon, lagna),
            ["MRITYU"] = Fixed(Cusp(8), moon, lagna),
            ["PARADESA"] = Fixed(Cusp(9), P(LordOfHouse(9)), lagna),
            ["ARTHA"] = Fixed(Cusp(2), P(LordOfHouse(2)), lagna),
            ["PARADARA"] = Sw(venus, sun, lagna),
            ["VANIK"] = Sw(moon, mercury, lagna),
            ["KARYASIDDHI"] = karyasiddhi,
            ["VIVAHA"] = Sw(venus, saturn, lagna),
            ["SANTAPA"] = Sw(saturn, moon, Cusp(6), lagna),
            ["SRADDHA"] = Sw(venus, mars, lagna),
            ["PREETI"] = Sw(sastra, punya, lagna),
            ["JADYA"] = Sw(mars, saturn, mercury),
            ["VYAPARA"] = Fixed(mars, saturn, lagna),
            ["SATRU"] = Sw(mars, saturn, lagna),
            ["JALAPATANA"] = Sw(105.0, saturn, lagna),
            ["BANDHANA"] = Sw(punya, saturn, lagna),
            ["APAMRITYU"] = Sw(Cusp(8), mars, lagna),
            ["LABHA"] = Fixed(Cusp(11), P(LordOfHouse(11)), lagna),
        };

        return Catalogue.Select(c => new NatalSaham(c.Code, c.Name, c.Meaning, values[c.Code])).ToList();
    }
}
