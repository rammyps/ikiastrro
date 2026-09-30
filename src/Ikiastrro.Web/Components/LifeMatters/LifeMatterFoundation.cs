using System.Globalization;
using Ikiastrro.Data;
using Ikiastrro.Web.Components.Workspace;

namespace Ikiastrro.Web.Components.LifeMatters;

/// <summary>One foundation fact. <c>Tone</c> is "good", "caution" or "weak" (null = neutral);
/// <c>Step</c> is the Key Inference step that shows it in full.</summary>
public sealed record FoundationItem(string Label, string Value, string Detail, string? Tone, string Step);

/// <summary>
/// The foundation every life-matter reading stands on (PVR's first stages): the rising sign, the
/// Lagna lord and its Ṣaḍbala, the Moon, and how safe the rising sign is from a birth-time error.
/// Read from D1 only; this puts stored facts into words and computes nothing astrological.
/// </summary>
public static class LifeMatterFoundation
{
    /// <summary>Degrees from a sign edge under which the rising sign itself is in doubt.</summary>
    public const double EdgeRiskDegrees = 1.0;

    /// <summary>Degrees from a sign edge under which the rising sign needs a reliable time.</summary>
    public const double EdgeCautionDegrees = 3.0;

    public static IReadOnlyList<FoundationItem> Build(LoadedChart d1, IEnumerable<ShadbalaSummaryRow> shadbala)
    {
        var items = new List<FoundationItem>();
        var strength = shadbala.GroupBy(s => s.Planet, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var asc = d1.KeyDetails.FirstOrDefault(k => k.Planet == "Ascendant");
        double? ascDegree = asc is null ? null
            : asc.DegreesInSignDecimal is { } d ? (double)d : asc.NirayanaLongitudeDegrees % 30.0;
        items.Add(new FoundationItem("Rising sign",
            Disp(d1.AscendantSign) + (ascDegree is { } deg ? $" {deg.ToString("0.#", CultureInfo.InvariantCulture)}°" : ""),
            "The Lagna: the self every life matter is counted from.", null, KeyInferenceLink.Overview));

        var lord = d1.HouseLords.FirstOrDefault(h => h.HouseNumber == 1);
        if (lord is not null)
        {
            var pct = strength.GetValueOrDefault(lord.LordPlanet)?.PercentOfMinimum;
            var dignity = string.IsNullOrWhiteSpace(lord.LordDignityStatus) ? "" : $" · {lord.LordDignityStatus}";
            items.Add(new FoundationItem("Lagna lord",
                $"{lord.LordPlanet} in the {Ordinal(lord.LordPlacedInHouseFromLagna)}{dignity}",
                pct is null ? "Ṣaḍbala not computed yet."
                    : $"Ṣaḍbala {pct.Value.ToString("0", CultureInfo.InvariantCulture)}% of its required minimum: "
                      + (pct >= 100 ? "able to deliver what the chart promises." : "below the minimum, so promises take more effort."),
                pct is null ? null : pct >= 100 ? "good" : "weak", KeyInferenceLink.Strength));
        }

        var moon = d1.KeyDetails.FirstOrDefault(k => k.Planet == "Moon");
        if (moon is not null)
        {
            var nakshatra = string.IsNullOrWhiteSpace(moon.Nakshatra) ? "" : $" · {moon.Nakshatra}";
            var dignity = string.IsNullOrWhiteSpace(moon.DignityStatus) ? "" : $", {moon.DignityStatus}";
            items.Add(new FoundationItem("Moon", Disp(moon.Sign) + nakshatra,
                $"In the {Ordinal(moon.HouseNumberFromLagna)} house{dignity}: the mind, and the second lagna.",
                null, KeyInferenceLink.Planets));
        }

        if (ascDegree is { } a)
        {
            var edge = Math.Min(a, 30.0 - a);
            var e = edge.ToString("0.0", CultureInfo.InvariantCulture);
            items.Add(edge < EdgeRiskDegrees
                ? new FoundationItem("Birth-time check", $"Lagna {e}° from a sign edge",
                    "A birth time a few minutes out would change the rising sign. Confirm the time before trusting any reading.",
                    "weak", KeyInferenceLink.Overview)
                : edge < EdgeCautionDegrees
                    ? new FoundationItem("Birth-time check", $"Lagna {e}° from a sign edge",
                        "The rising sign holds for small errors, but the finer charts (D9 and above) need an accurate time.",
                        "caution", KeyInferenceLink.Overview)
                    : new FoundationItem("Birth-time check", $"Lagna {e}° inside its sign",
                        "Safe from small birth-time errors; the finest charts (D30 and above) still need an accurate time.",
                        "good", KeyInferenceLink.Overview));
        }

        return items;
    }

    private static string Disp(string s) => s == "Capricornus" ? "Capricorn" : s;

    private static string Ordinal(int n) =>
        n + (n % 100 is 11 or 12 or 13 ? "th" : (n % 10) switch { 1 => "st", 2 => "nd", 3 => "rd", _ => "th" });
}
