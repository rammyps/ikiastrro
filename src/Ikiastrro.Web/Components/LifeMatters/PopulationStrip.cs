using System.Globalization;
using Ikiastrro.Data.Statistics;

namespace Ikiastrro.Web.Components.LifeMatters;

/// <summary>Layout and wording for one population-comparison distribution strip (0–100 scale).</summary>
public static class PopulationStrip
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    /// <summary>Position on the 0–100 axis, clamped; null stays null so nothing is drawn for a missing value.</summary>
    public static decimal? Pos(decimal? value) =>
        value is null ? null : Math.Clamp(value.Value, 0m, 100m);

    public static string Css(decimal? value) =>
        Pos(value) is { } p ? p.ToString("0.##", Inv) : "";

    /// <summary>Left offset and width (both in %) of a low–high band, or null when either end is missing.</summary>
    public static (string Left, string Width)? Band(decimal? low, decimal? high)
    {
        if (Pos(low) is not { } l || Pos(high) is not { } h) return null;
        if (h < l) (l, h) = (h, l);
        return (l.ToString("0.##", Inv), Math.Max(h - l, 0.6m).ToString("0.##", Inv));
    }

    /// <summary>Plain-language reading of the robust z-score. A descriptive position, never a verdict.</summary>
    public static string Position(PopulationComparison c) => c.RobustZ switch
    {
        null => "",
        >= 2m => "Unusually high for this population",
        <= -2m => "Unusually low for this population",
        >= 1m => "Somewhat above typical",
        <= -1m => "Somewhat below typical",
        _ => "Within the typical range"
    };

    /// <summary>A percentile restated as a count out of 100 comparable charts.</summary>
    public static string Rank(PopulationComparison c) =>
        c.Percentile is { } p
            ? $"Higher than {Math.Round(p, MidpointRounding.AwayFromZero):0} of every 100 comparable charts"
            : "";

    public static string Ordinal(decimal? value)
    {
        if (value is null) return "Not published";
        var n = (int)Math.Round(value.Value, MidpointRounding.AwayFromZero);
        var suffix = (n % 100) is >= 11 and <= 13
            ? "th"
            : (n % 10) switch { 1 => "st", 2 => "nd", 3 => "rd", _ => "th" };
        return $"{n}{suffix}";
    }

    public static string Tone(PopulationComparison c) => c.SufficiencyCode switch
    {
        "INSUFFICIENT" or "INCOMPLETE" => "is-thin",
        "EXPLORATORY" => "is-exploratory",
        _ => "is-solid"
    };

    public static string Missing(PopulationComparison c) =>
        c.MissingRate <= 0 ? "no missing values" : $"{c.MissingRate * 100m:0.#}% missing";
}
