namespace Ikiastrro.Core.Presentation;

/// <summary>
/// The app-wide strength-band standard: every graha has nine stops, -4..+4, as shades of its own
/// identity colour (wwwroot/css/planet-bands.css). 0 is the planet colour itself; +n lightens and
/// -n darkens, per theme. The eight bands are the gaps between adjacent stops. Dignity, Shadbala,
/// Ashtakavarga and bhava bala are all normalised onto this one axis here, so a "+3" reads the
/// same colour wherever it appears.
/// </summary>
public static class PlanetBand
{
    public const int Min = -4;
    public const int Max = 4;

    private static readonly HashSet<string> Planets = new(StringComparer.OrdinalIgnoreCase)
    {
        "sun", "moon", "mars", "mercury", "jupiter", "venus", "saturn", "rahu", "ketu"
    };

    public static int Clamp(int stop) => Math.Clamp(stop, Min, Max);

    /// <summary>CSS stop suffix: n4 n3 n2 n1 z p1 p2 p3 p4.</summary>
    public static string Suffix(int stop)
    {
        var s = Clamp(stop);
        return s == 0 ? "z" : s < 0 ? $"n{-s}" : $"p{s}";
    }

    /// <summary>"pb pb-sun-p3" — graha colour for a known planet, otherwise the golden chart ramp.</summary>
    public static string Class(string? planet, int stop) =>
        $"pb pb-{(planet is not null && Planets.Contains(planet.Trim()) ? planet.Trim().ToLowerInvariant() : "golden")}-{Suffix(stop)}";

    /// <summary>The CSS custom property for a stop, for borders, bars and SVG fills: var(--pb-moon-n2).</summary>
    public static string Var(string? planet, int stop) =>
        $"var(--pb-{(planet is not null && Planets.Contains(planet.Trim()) ? planet.Trim().ToLowerInvariant() : "golden")}-{Suffix(stop)})";

    /// <summary>Dignity label → stop (<see cref="ChartViewModel.DignityScore"/>: Exalted +4 … Debilitated -4).</summary>
    public static int FromDignity(string? dignityStatus) => ChartViewModel.DignityScore(dignityStatus);

    /// <summary>Ṣaḍbala as % of the required minimum: 100% is par (0), 200%+ is +4, 0% is -4.</summary>
    public static int FromShadbalaPercent(decimal percentOfRequired) =>
        Clamp((int)Math.Round((percentOfRequired / 100m - 1m) * 4m, MidpointRounding.AwayFromZero));

    /// <summary>Bhinnāṣṭavarga bindus in one sign (0–8): 4 is par, so stop = bindus - 4.</summary>
    public static int FromBindus(int bindus) => Clamp(bindus - 4);

    /// <summary>Sarvāṣṭavarga sign total: 28 is par (the average sign); one stop per 3 bindus.</summary>
    public static int FromSarvaTotal(int total) =>
        Clamp((int)Math.Round((total - 28) / 3.0, MidpointRounding.AwayFromZero));

    /// <summary>Plain-language band name for the dignity scale, e.g. +3 → "Moolatrikona".</summary>
    public static string DignityName(int stop) => Clamp(stop) switch
    {
        4 => "Exalted", 3 => "Moolatrikona", 2 => "Own Sign", 1 => "Great Friend", 0 => "Friend",
        -1 => "Neutral", -2 => "Enemy", -3 => "Great Enemy", _ => "Debilitated"
    };
}
