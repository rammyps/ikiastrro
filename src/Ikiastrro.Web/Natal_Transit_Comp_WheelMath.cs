using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Houses;

namespace Ikiastrro.Web;

/// <summary>
/// Pure display helpers for the v2 Transit comparison page
/// (<c>Components/Pages/Natal_Transit_Comp_Wheel.razor</c>).
/// No astrology is computed here — this only orders and formats rows that are already persisted
/// (<c>docs/ui/components/spec_Natal_Transit_Comp_Wheel.md</c>, <c>docs/architecture/domain-contracts.md</c>).
/// </summary>
public static class Natal_Transit_Comp_WheelMath
{
    // "how long a graha holds a house": Saturn slowest … Sun fastest. Both tabs sort by this;
    // the D1 Birth tab puts Lagna first (rank -1).
    private static readonly string[] SortOrder =
        { "Saturn", "Jupiter", "Rahu", "Ketu", "Mars", "Venus", "Mercury", "Moon", "Sun" };

    /// <summary>Sort key for a planet name; Lagna/Ascendant first, unknown names last.</summary>
    public static int PlanetRank(string? planet)
    {
        if (planet is null) return 99;
        if (planet.Equals("Lagna", StringComparison.OrdinalIgnoreCase) ||
            planet.Equals("Ascendant", StringComparison.OrdinalIgnoreCase)) return -1;
        var i = Array.FindIndex(SortOrder, s => s.Equals(planet, StringComparison.OrdinalIgnoreCase));
        return i < 0 ? 98 : i;
    }

    /// <summary>D1 Birth "Motion" cell: retrograde/direct plus a combust note.</summary>
    public static string FormatMotion(bool isRetrograde, bool isCombust)
    {
        var motion = isRetrograde ? "Retro" : "Direct";
        return isCombust ? motion + " · combust" : motion;
    }

    /// <summary>
    /// Current Transit "House from D1": houses between the graha's natal sign (D1) and its
    /// transit sign, same-sign = 1. Sign arithmetic only — <see cref="AstroMath.CountFromSignToSign"/>,
    /// the same helper the Gochara panel uses. Null when either sign name is unrecognised.
    /// </summary>
    public static int? HouseFromD1(string? natalSign, string? transitSign)
    {
        if (!TryParseSign(natalSign, out var from) || !TryParseSign(transitSign, out var to)) return null;
        return AstroMath.CountFromSignToSign(from, to);
    }

    private static bool TryParseSign(string? name, out ZodiacName sign)
    {
        sign = default;
        if (string.IsNullOrWhiteSpace(name)) return false;
        var n = name.Trim();
        if (n.Equals("Capricorn", StringComparison.OrdinalIgnoreCase)) n = "Capricornus";
        return Enum.TryParse(n, ignoreCase: true, out sign);
    }
}
