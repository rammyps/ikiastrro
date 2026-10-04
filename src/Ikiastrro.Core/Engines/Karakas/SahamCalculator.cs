using Ikiastrro.Core.Engines.Astronomy;

namespace Ikiastrro.Core.Engines.Karakas;

/// <summary>A natal Saham: its code, the matter it stands for, and its D1 sidereal longitude.</summary>
public sealed record NatalSaham(string Code, string Name, string Meaning, double LongitudeDegrees)
{
    public ZodiacName Sign => AstroMath.GetSignAtLongitude(LongitudeDegrees);
}

/// <summary>
/// PVR ch.28.8.1 Saham arithmetic: (A − B + C) is how far A is from B, taken again from C; when C
/// does not lie on the zodiacal arc going forward from B to A, 30° is added. For most sahams the
/// formula is for day births and night births use (B − A + C) — except those the table marks "same for
/// day and night". Pure.
/// </summary>
public static class SahamCalculator
{
    /// <summary>A − B + C with PVR's +30° arc correction, as a normalised longitude.</summary>
    public static double Evaluate(double a, double b, double c)
    {
        var arc = AstroMath.Normalize(a - b);
        var distanceToC = AstroMath.Normalize(c - b);
        return AstroMath.Normalize(a - b + c + (distanceToC <= arc ? 0.0 : 30.0));
    }

    /// <summary>The day formula's (A, B) swapped for a night birth.</summary>
    public static double EvaluateDayNight(double a, double b, double c, bool isNightBirth) =>
        isNightBirth ? Evaluate(b, a, c) : Evaluate(a, b, c);

    /// <summary>Vivaha (marriage), Table 74: Venus − Saturn + Lagna by day, reversed by night.</summary>
    public static NatalSaham Vivaha(double venus, double saturn, double lagna, bool isNightBirth) =>
        new("VIVAHA", "Vivaha Saham", "Marriage", EvaluateDayNight(venus, saturn, lagna, isNightBirth));

    /// <summary>Kali (great misfortune), Table 74: Jupiter − Mars + Lagna by day, reversed by night.</summary>
    public static NatalSaham Kali(double jupiter, double mars, double lagna, bool isNightBirth) =>
        new("KALI", "Kali Saham", "Great misfortune", EvaluateDayNight(jupiter, mars, lagna, isNightBirth));

    /// <summary>The Sahams PVR §25.3 and §28.8.2 use for transit triggers: Vivaha for marriage (the 7th
    /// lord or Venus transiting near it, Jupiter occupying or aspecting it) and Kali for accidents (the
    /// 6th lord, 8th lord, Mars or Rāhu transiting near it). Longitudes are D1, sidereal.</summary>
    public static IReadOnlyList<NatalSaham> TransitTriggerSahams(
        IReadOnlyDictionary<PlanetName, double> longitudes, double lagna, bool isNightBirth) =>
    [
        Vivaha(longitudes[PlanetName.Venus], longitudes[PlanetName.Saturn], lagna, isNightBirth),
        Kali(longitudes[PlanetName.Jupiter], longitudes[PlanetName.Mars], lagna, isNightBirth),
    ];
}
